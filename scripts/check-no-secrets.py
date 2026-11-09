#!/usr/bin/env python3
"""Fail a commit if a staged file adds a secret or a home path.

Used by .githooks/pre-commit. The published MOB.md is a generated summary and is already scrubbed by the facilitator
(mob-defaults/mob_summary.py); this is the second, independent check at the commit boundary.

For every staged file other than MOB.md only the added lines (git diff --cached -U0) are scanned
for home paths, so existing record files do not block a commit. MOB.md keeps a whole-file scan:
the secret patterns below plus home_paths.find. A hit names the file, the line and the form only;
the matched text (and so the account) is never printed.

With --tree, scan every tracked file in the working tree instead and print, per area, the hit
count and the first three file:line:form.
"""

import re
import subprocess
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import home_paths  # noqa: E402

PATTERNS = [
    (r"-----BEGIN [A-Z ]*PRIVATE KEY-----", "private key"),
    (r"\bgh[pousr]_[A-Za-z0-9]{20,}\b", "GitHub token"),
    (r"\bgithub_pat_[A-Za-z0-9_]{20,}\b", "GitHub token"),
    (r"\bsk-[A-Za-z0-9_\-]{16,}\b", "API key"),
    (r"\bAKIA[0-9A-Z]{16}\b", "AWS key"),
    (r"\bxox[abprs]-[A-Za-z0-9\-]{10,}\b", "Slack token"),
    (r"\bAIza[0-9A-Za-z_\-]{30,}\b", "Google key"),
    (r"(?i)\b(?:bearer|basic)\s+[A-Za-z0-9._~+/\-]{16,}=*", "auth header"),
    (r"(?i)\b(?:api[_\-]?key|secret|token|passwd|password|authorization|access[_\-]?key)\b\s*[:=]\s*\S+", "credential assignment"),
    (r"\b[A-Fa-f0-9]{40,}\b", "long hex string"),
]

AREA_LABELS = {
    "A1": ".mob/",
    "A2": "board/",
    "A3": "tests/",
    "A4": "scripts/",
    "A5": "docs/",
    "A6": "HANDOFF.md and TEAM_WORKING_AGREEMENT.md",
    "A7": "bench/",
    "A8": "other",
}


def area_of(path):
    if path.startswith(".mob/"):
        return "A1"
    if path.startswith("board/"):
        return "A2"
    if path.startswith("tests/"):
        return "A3"
    if path.startswith("scripts/"):
        return "A4"
    if path.startswith("docs/"):
        return "A5"
    if path in ("HANDOFF.md", "TEAM_WORKING_AGREEMENT.md"):
        return "A6"
    if path.startswith("bench/"):
        return "A7"
    return "A8"


def git_text(*args):
    return subprocess.run(["git", *args], capture_output=True, text=True, encoding="utf-8", errors="replace").stdout


def staged_names():
    out = subprocess.run(["git", "diff", "--cached", "--name-only", "-z"], capture_output=True).stdout
    return [part.decode("utf-8", "replace") for part in out.split(b"\0") if part]


def added_lines():
    """{path: [(new line number, added text)]} from git diff --cached -U0; binary files are skipped."""
    diff = git_text("diff", "--cached", "-U0", "--no-color")
    added = {}
    current = None
    binary = False
    new_line = 0
    for raw in diff.splitlines():
        if raw.startswith("diff --git "):
            current = None
            binary = False
            continue
        if raw.startswith("Binary files ") or raw == "GIT binary patch":
            binary = True
            current = None
            continue
        if raw.startswith("+++ "):
            if binary or raw[4:].strip() == "/dev/null":
                current = None
            else:
                name = raw[4:].strip()
                current = name[2:] if name.startswith("b/") else name
                added.setdefault(current, [])
            continue
        if raw.startswith("@@ "):
            match = re.search(r"\+(\d+)", raw)
            new_line = int(match.group(1)) if match else 0
            continue
        if current is None:
            continue
        if raw.startswith("+") and not raw.startswith("+++"):
            added[current].append((new_line, raw[1:]))
            new_line += 1
        elif raw.startswith("-") and not raw.startswith("---"):
            continue
        elif raw.startswith("\\"):
            continue
        elif raw.startswith(" "):
            new_line += 1
    return added


def scan_staged():
    bad = []
    names = staged_names()
    added = added_lines()
    for name in names:
        if name.split("/")[-1] == "MOB.md":
            text = git_text("show", f":{name}")
            for pattern, label in PATTERNS:
                for match in re.finditer(pattern, text):
                    line = text.count("\n", 0, match.start()) + 1
                    bad.append(f"{name}:{line}: looks like a {label}")
            for hit in home_paths.find(name, text):
                bad.append(f"{name}:{hit.line}: home path ({hit.form})")
            continue
        for line, text in added.get(name, []):
            for hit in home_paths.find(name, text):
                bad.append(f"{name}:{line}: home path ({hit.form})")
    return bad


def scan_tree():
    root = Path(git_text("rev-parse", "--show-toplevel").strip() or ".")
    hits = {area: [] for area in AREA_LABELS}
    for relative in home_paths.tracked_paths(root):
        target = root / relative
        if not target.is_file():
            continue
        data = target.read_bytes()
        if b"\0" in data:
            continue
        seen = set()
        for hit in home_paths.find(relative, data.decode("utf-8", "replace")):
            if (relative, hit.line) in seen:
                continue
            seen.add((relative, hit.line))
            hits[area_of(relative)].append((relative, hit.line, hit.form))
    for area, label in AREA_LABELS.items():
        found = hits[area]
        files = len({path for path, _line, _form in found})
        print(f"{area} {label}: {len(found)} hits in {files} files")
        for path, line, form in found[:3]:
            print(f"{path}:{line}:{form}")
    return 0


def main(argv=None):
    args = list(sys.argv[1:] if argv is None else argv)
    if "--tree" in args:
        return scan_tree()
    bad = scan_staged()
    if bad:
        print("pre-commit: staged files must not add secrets or home paths (the repo is PUBLIC):", file=sys.stderr)
        for entry in bad[:50]:
            print("  " + entry, file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
