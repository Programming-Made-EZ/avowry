#!/usr/bin/env python3
"""Fail unless docs/decisions/DECIDERS.md has a Decider record that names <keyword> and says 'Result: passed'.

Usage: check-decider-recorded.py <keyword>

For cards whose Done-when needs a recorded team decision (an ADR, a benchmark-suite change)."""

import pathlib
import re
import sys


def recorded(text, keyword):
    blocks = re.split(r"\n#### ", text)
    return any(keyword.lower() in b.lower() and "Result: passed" in b for b in blocks)


def main(argv):
    if len(argv) != 2:
        print("usage: check-decider-recorded.py <keyword>", file=sys.stderr)
        return 2
    text = pathlib.Path("docs/decisions/DECIDERS.md").read_text(encoding="utf-8")
    ok = recorded(text, argv[1])
    print(f"Decider record naming '{argv[1]}' with 'Result: passed': {'found' if ok else 'MISSING'}")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv))
