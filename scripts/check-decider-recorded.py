#!/usr/bin/env python3
"""Fail unless an area history has a Decider record naming <keyword> with 'Result: passed'.

Usage: check-decider-recorded.py <keyword>

For cards whose Done-when needs a recorded team decision (an ADR, a benchmark-suite change)."""

import pathlib
import re
import sys

from decision_files import area_paths


def recorded(text, keyword):
    blocks = re.split(r"\n#### ", text)
    return any(keyword.lower() in b.lower() and "Result: passed" in b for b in blocks)


def main(argv):
    if len(argv) != 2:
        print("usage: check-decider-recorded.py <keyword>", file=sys.stderr)
        return 2
    decisions = pathlib.Path("docs/decisions")
    text = "\n".join(
        path.read_text(encoding="utf-8")
        for path in area_paths(decisions)
        if path.exists()
    )
    ok = recorded(text, argv[1])
    print(f"Decider record naming '{argv[1]}' with 'Result: passed': {'found' if ok else 'MISSING'}")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv))
