#!/usr/bin/env python3
"""Card 264 criterion 1 check: the 20-run parallel repeat evidence for PhraseExecutedOutcomesAcceptance.

Reads .mob/records/264/repeat-evidence.json and fails unless it records at least 20 consecutive passing runs of
the class with a named load class, an integer concurrency, the exact repeat command, a per-process timeout
boundary, one entry per run, and a full explanation for any failure. Writes nothing. Stdlib only.

  python scripts/check-repeat-evidence.py [--root DIR]
  exit 0 clean ("repeat-evidence: OK"), 1 findings (stderr "<path>: <keyword> <detail>"), 2 not a directory.
Finding keywords: missing, malformed, command, class, load, timeout, runs, unexplained, summary.
Never a traceback. Contract: tests/test_repeat_evidence.py.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

REL = ".mob/records/264/repeat-evidence.json"
CLASS = "PhraseExecutedOutcomesAcceptance"
MIN_RUNS = 20
FAILURE_KEYS = ("row", "exit_code", "stdout", "stderr", "history_exists")


def is_int(v):
    return isinstance(v, int) and not isinstance(v, bool)


def is_number(v):
    return isinstance(v, (int, float)) and not isinstance(v, bool)


def text(v):
    return isinstance(v, str) and v.strip() != ""


def check_load(rec, add):
    load = rec.get("load")
    if not isinstance(load, dict):
        add("load", "no named load class (an unloaded pass is not enough)")
        return
    if not text(load.get("class")):
        add("load", "load class is not named")
    elif CLASS in load["class"]:
        add("load", "the load class must be a different class from the one under test")
    if not is_int(load.get("concurrency")) or load["concurrency"] < 1:
        add("load", "concurrency must be an integer >= 1")
    if not text(load.get("command")):
        add("load", "load command is not recorded")


def failure_problem(f):
    if not isinstance(f, dict):
        return "failed run carries no failure explanation"
    for key in FAILURE_KEYS:
        if key not in f:
            return f"failure lacks {key}"
    if not text(f["row"]) or "." not in f["row"].split("(")[0]:
        return "failure row is not a fully qualified test row"
    if not is_int(f["exit_code"]):
        return "failure exit_code must be an integer"
    if not isinstance(f["stdout"], str) or not isinstance(f["stderr"], str):
        return "failure stdout and stderr must be text"
    if not isinstance(f["history_exists"], bool):
        return "failure history_exists must be true or false"
    return None


def check_runs(rec, boundary, add):
    runs = rec.get("runs")
    if not isinstance(runs, list) or not all(isinstance(r, dict) for r in runs):
        add("runs", "runs must be a list of run entries")
        return None
    if len(runs) < MIN_RUNS:
        add("runs", f"{len(runs)} runs recorded, at least {MIN_RUNS} required")
    if [r.get("run") for r in runs] != list(range(1, len(runs) + 1)):
        add("runs", "run numbers must be 1..N without gap or repeat")
    for r in runs:
        n = r.get("run")
        if not isinstance(r.get("passed"), bool):
            add("runs", f"run {n}: passed must be true or false")
        elif not r["passed"]:
            problem = failure_problem(r.get("failure"))
            if problem:
                add("unexplained", f"run {n}: {problem}")
            add("runs", f"run {n} failed: 20 consecutive passes are not shown")
        slow = r.get("slowest_seconds")
        if not is_number(slow) or slow < 0:
            add("runs", f"run {n}: slowest_seconds must be a non-negative number")
        elif boundary is not None and slow > boundary:
            add("timeout", f"run {n}: slowest {slow}s is above the {boundary}s boundary")
    return runs


def check_summary(rec, runs, add):
    s = rec.get("summary")
    if not isinstance(s, dict):
        add("summary", "summary is absent")
        return
    if runs is None:
        return
    passed = sum(1 for r in runs if r.get("passed") is True)
    want = {"total": len(runs), "passed": passed, "failed": len(runs) - passed}
    for key, value in want.items():
        if not is_int(s.get(key)) or s[key] != value:
            add("summary", f"{key} is {s.get(key)!r}, the runs give {value}")


def findings_for(rec):
    out = []

    def add(keyword, detail):
        out.append(f"{REL}: {keyword} {detail}")

    if rec.get("class") != CLASS:
        add("class", f"class under test must be {CLASS}")
    cmd = rec.get("repeat_command")
    if not text(cmd) or CLASS not in cmd:
        add("command", f"repeat_command must be recorded and name {CLASS}")
    check_load(rec, add)
    boundary = rec.get("timeout_boundary_seconds")
    if not is_number(boundary) or boundary <= 0:
        add("timeout", "timeout_boundary_seconds must be a number > 0")
        boundary = None
    runs = check_runs(rec, boundary, add)
    check_summary(rec, runs, add)
    return out


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--root", default=str(Path(__file__).resolve().parent.parent))
    root = Path(ap.parse_args(argv).root)
    if not root.is_dir():
        print(f"{root}: not a directory", file=sys.stderr)
        return 2
    path = root / REL
    if not path.is_file():
        print(f"{REL}: missing file", file=sys.stderr)
        return 1
    try:
        rec = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, ValueError) as exc:
        print(f"{REL}: malformed {exc}", file=sys.stderr)
        return 1
    if not isinstance(rec, dict):
        print(f"{REL}: malformed top level must be an object", file=sys.stderr)
        return 1
    findings = findings_for(rec)
    if findings:
        print("\n".join(findings), file=sys.stderr)
        return 1
    print("repeat-evidence: OK")
    return 0


if __name__ == "__main__":
    sys.exit(main())
