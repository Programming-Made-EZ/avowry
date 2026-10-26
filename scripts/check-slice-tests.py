#!/usr/bin/env python3
"""Fail unless at least MIN tests carry the Trait("Slice", "<number>") of a thin card.

Usage: check-slice-tests.py <card-number> [min-count]   (default min-count 1)

A thin card's Done-when runs this first, so the gate cannot pass on a card that added no test of its own."""

import pathlib
import re
import sys


def count_slice_tests(root, number):
    pattern = re.compile(r'\[\s*Trait\(\s*"Slice"\s*,\s*"' + re.escape(number) + r'"\s*\)\s*\]')
    return sum(len(pattern.findall(p.read_text(encoding="utf-8", errors="replace"))) for p in pathlib.Path(root).rglob("*.cs"))


def main(argv):
    if len(argv) < 2:
        print("usage: check-slice-tests.py <card-number> [min-count]", file=sys.stderr)
        return 2
    number, minimum = argv[1], int(argv[2]) if len(argv) > 2 else 1
    found = count_slice_tests("tests", number)
    print(f"card {number}: {found} test(s) with Trait Slice={number} (need {minimum})")
    return 0 if found >= minimum else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv))
