#!/usr/bin/env python3
"""Scan every line added in the history reachable from REV (default HEAD) for secret-shaped text.

Prints "kind path commit" per match (never the matched text), "binary files in range: K", then "count N".
Patterns come from check-no-secrets.py minus its two noisy labels. --self-test runs on a temp repo.
"""
import importlib.util, re, subprocess, sys, tempfile
from pathlib import Path

HERE = Path(__file__).resolve().parent
NOISY = {"long hex string", "home path"}


def git(args, cwd):
    return subprocess.run(["git", *args], cwd=cwd, capture_output=True, text=True, encoding="utf-8",
                          errors="replace", check=True).stdout


def scan(rev, cwd):
    spec = importlib.util.spec_from_file_location("check_no_secrets", HERE / "check-no-secrets.py")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    patterns = [(re.compile(p), k) for p, k in module.PATTERNS if k not in NOISY]
    hits, binaries, commit, path, in_hunk = [], 0, "", "", False
    for line in git(["log", rev, "-p", "-U0", "--no-color", "--format=@@C %H"], cwd).splitlines():
        if line.startswith("@@C "):
            commit, in_hunk = line[4:].strip(), False
        elif line.startswith("diff --git"):
            in_hunk = False
        elif line.startswith("@@ "):
            in_hunk = True
        elif not in_hunk and line.startswith("+++ "):
            path = line[4:].removeprefix("b/")
        elif not in_hunk and line.startswith("Binary files "):
            binaries += 1
        elif in_hunk and line.startswith("+"):
            hits += [(k, path, commit) for r, k in patterns for _ in r.finditer(line, 1)]
    return hits, binaries


def self_test():
    who = "t" + "@" + "example.test"
    base = ["-c", "user.name=tester", "-c", "user.email=" + who, "-c", "commit.gpgsign=false"]
    counts = []
    with tempfile.TemporaryDirectory() as tmp:
        git(["init", "-q"], tmp)
        for name, text in (("a.txt", "api" + "_key = " + "abcdef123456\n"), ("b.txt", "nothing to see\n")):
            (Path(tmp) / name).write_text(text, encoding="utf-8")
            git(["add", name], tmp)
            git([*base, "commit", "-q", "-m", name], tmp)
            counts.append(len(scan("HEAD", tmp)[0]))
    ok = counts == [1, 1]
    print("self-test " + ("ok" if ok else "FAILED: counts " + str(counts)))
    return 0 if ok else 1


def main(argv):
    if argv[:1] == ["--self-test"]:
        return self_test()
    hits, binaries = scan(argv[0] if argv else "HEAD", Path.cwd())
    for kind, path, commit in hits:
        print(kind, path, commit)
    print("binary files in range: " + str(binaries))
    print("count " + str(len(hits)))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
