#!/usr/bin/env python3
"""Card 370: one definition of a home path of a local account, and the rewrite to the neutral form.

A home path is a drive-letter or Unix home root followed by an account segment. The rewrite keeps every byte
except the account segment, which becomes PLACEHOLDER (``<`` cannot occur in a Windows file name, so no real
account is ever named PLACEHOLDER). Escaping (one, two or four backslashes), CRLF and a missing final newline
are kept as they are.

Users: the in-gate scan (tests/test_home_paths_370.py), the commit guard (scripts/check-no-secrets.py) and the
board generator. Import with ``sys.path.insert(0, <scripts dir>)`` then ``import home_paths``.

This file holds no home path: its patterns cannot match their own text, and its tests build every sample from
parts.

Red stub: ``find`` returns no hit yet (the green makes it real); ``scrub_bytes`` is the real rule.
"""

import argparse
import re
import subprocess
import sys
from collections import namedtuple
from pathlib import Path

PLACEHOLDER = "<user>"

SEGX = rb"\x5c/\s\x22\x27`<>|*?:,;()\[\]{}%&=$"
SEG  = rb"(?P<seg>[^"+SEGX+rb"]*[^"+SEGX+rb".])"      # not empty, no trailing dot, never starts "<" or "$"
SEP  = rb"(?:\x5c|/|%5[cC]|%2[fF])+"                   # any run of separators, kept verbatim
NB   = rb"(?<![A-Za-z0-9_.\-])"
PATH = re.compile(rb"(?P<pre>[A-Za-z](?::|%3[aA])"+SEP+rb"(?i:users)"        # F1 F2 F3 F8 (file:/// ends here)
       rb"|"+NB+rb"(?:/mnt)?/[a-z]"+SEP+rb"(?i:users)"                       # F4
       rb"|"+NB+rb"(?:/|\x5c+|%2[fF]|%5[cC])(?:Users|home))"                 # F6 F7, case-sensitive
       rb"(?P<sep>"+SEP+rb")"+SEG)
SLUG = re.compile(rb"(?P<pre>[A-Za-z]--(?i:users)-)(?P<seg>[A-Za-z0-9_]+)")  # F5

ALLOWED_SEGMENTS = {
    "runner": "the CI account on the hosted runners",
    "runneradmin": "the CI administrator account on the hosted runners",
    "x": "a placeholder the card 370 records use",
    "...": "a placeholder the card 370 records use",
    "\u2026": "a placeholder the card 370 records use (one ellipsis character)",
}

FILE_ALLOW = {
    "tests/test_bench_history_clean.py": {
        "someone": "a fictional account in the bench history scrubber fixture",
    },
    "tests/Avowry.Acceptance.Tests/CSharpBaselineAcceptance.cs": {
        "al" + "ice": "a fictional account in the bench scrubber fixture; as the placeholder its DoesNotContain could not fail",
    },
}

FORMS = {
    "F1": "drive letter, colon, backslashes (one, two or more)",
    "F2": "drive letter, colon, forward slashes",
    "F3": "the users folder not spelled with a capital U",
    "F4": "a Unix-style drive root (/c/ or /mnt/c/) then the users folder",
    "F5": "the project-folder slug (drive letter, two hyphens, the users folder, a hyphen)",
    "F6": "the macOS users root, case-sensitive",
    "F7": "the Unix home root",
    "F8": "a file URL or a URL-encoded separator or colon",
    "F9": "the segment ends a line or the file, or is followed by a quote, a backtick or a space",
}

Hit = namedtuple("Hit", "line form")


def _allowed(path):
    allowed = {name.encode("utf-8") for name in ALLOWED_SEGMENTS}
    if path is not None:
        allowed |= {name.encode("utf-8") for name in FILE_ALLOW.get(str(path).replace("\\", "/"), {})}
    return allowed


def scrub_bytes(data, path=None):
    """The rewrite: every account segment not on the allow-list becomes PLACEHOLDER; every other byte stays.

    With ``path`` (repository-relative, forward slashes) the file-pinned segments of FILE_ALLOW stay too.
    """
    allowed = _allowed(path)
    neutral = PLACEHOLDER.encode("ascii")

    def path_sub(match):
        if match.group("seg") in allowed:
            return match.group(0)
        return match.group("pre") + match.group("sep") + neutral

    def slug_sub(match):
        if match.group("seg") in allowed:
            return match.group(0)
        return match.group("pre") + neutral

    return SLUG.sub(slug_sub, PATH.sub(path_sub, data))


def scrub_text(text, path=None):
    return scrub_bytes(text.encode("utf-8", "surrogateescape"), path).decode("utf-8", "surrogateescape")


def find(path, text):
    """Each home path in ``text`` as Hit(line, form); never the matched text. Red stub: finds nothing."""
    return []


def tracked_paths(root):
    out = subprocess.run(["git", "ls-files", "-z"], cwd=root, capture_output=True, check=True).stdout
    return [part.decode("utf-8") for part in out.split(b"\0") if part]


def scrub_tracked(root, apply=False, exclude=("board/",)):
    """Apply scrub_bytes to every tracked file under ``root`` (binary mode; a file with a NUL byte is skipped;
    untracked files are never read). Returns the changed paths; writes them only when ``apply``."""
    root = Path(root)
    changed = []
    for relative in tracked_paths(root):
        if any(relative.startswith(prefix) for prefix in exclude):
            continue
        target = root / relative
        if not target.is_file():
            continue
        data = target.read_bytes()
        if b"\0" in data:
            continue
        new = scrub_bytes(data, relative)
        if new != data:
            changed.append(relative)
            if apply:
                target.write_bytes(new)
    return changed


def main(argv=None):
    parser = argparse.ArgumentParser(description="Rewrite home paths in tracked files to the neutral form.")
    parser.add_argument("--root", type=Path, default=Path.cwd())
    parser.add_argument("--apply", action="store_true", help="write the changed files (default: list them)")
    args = parser.parse_args(argv)
    changed = scrub_tracked(args.root, apply=args.apply)
    for relative in changed:
        print(("rewrote " if args.apply else "would rewrite ") + relative)
    print(f"{len(changed)} files")
    return 0


if __name__ == "__main__":
    sys.exit(main())
