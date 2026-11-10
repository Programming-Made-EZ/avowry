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

``find`` reports exactly what ``scrub_bytes`` would change; ``scrub_bytes`` is the rewrite rule.

Card 376, the space rule: an account segment may be up to three words joined by one literal space or ``%20``;
the multi-word form is taken only if a separator, a quote, a backtick or the end of the line or file directly
follows the last word, otherwise the segment is the first word alone. Known limits: four or more words, double
spaces or tabs, and a spaced name followed by prose are not taken whole; prose such as ``<account> then foo/bar``
after a path is over-scrubbed to ``<user>/bar`` (the safe direction).

Card 376, F10: the local account name standing alone (whole word, any case). ``local_accounts`` lists the names
of this machine outside the allow-lists; ``find(path, text, accounts)`` reports them and ``scrub_account_bytes``
rewrites them. ``scrub_tracked`` never applies the F10 rewrite.
"""

import argparse
import functools
import getpass
import os
import re
import subprocess
import sys
from collections import namedtuple
from pathlib import Path

PLACEHOLDER = "<user>"

SEGX = rb"\x5c/\s\x22\x27`<>|*?:,;()\[\]{}%&=$"
_WORD  = rb"[^"+SEGX+rb"]*[^"+SEGX+rb".]"             # not empty, no trailing dot, never starts "<" or "$"
_CLOSE = rb"(?=[\x5c/\x22\x27`]|%5[cC]|%2[fF]|\r?\n|\r?\Z)"
SEG  = rb"(?P<seg>(?:"+_WORD+rb"(?: |%20)){1,2}"+_WORD+_CLOSE+rb"|"+_WORD+rb")"  # up to three words, see below
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
FORMS["F10"] = "the local account name standing alone (whole word, any case)"

MIN_ACCOUNT_LENGTH = 3

GENERIC_ACCOUNTS = {
    "root": "the Unix superuser; a common word in paths and prose",
    "administrator": "the built-in Windows administrator",
    "admin": "a common administrator account and a common word",
    "user": "a common default account and a common word (also inside the placeholder)",
    "ubuntu": "the default account of Ubuntu cloud images; also the distribution name",
    "vsts": "the Azure Pipelines hosted agent account",
    "default": "the Windows default profile and a common word",
    "guest": "the Windows guest account and a common word",
    "public": "the Windows public profile and a common word",
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


_DRIVE_ROOT = re.compile(rb"(?:/mnt)?/[a-z](?:\x5c|/|%5[cC]|%2[fF])+(?i:users)")

_SEP_STR = r"(?:\\|/|%5[cC]|%2[fF])+"
_WORD_STR = r"[^\\/\s\"'`<>|*?:,;()\[\]{}%&=$]*[^\\/\s\"'`<>|*?:,;()\[\]{}%&=$.]"
_SEG_STR = (r"(?P<seg>(?:" + _WORD_STR + r"(?: |%20)){1,2}" + _WORD_STR
            + r"(?=[\\/\"'`]|%5[cC]|%2[fF]|\r?\n|\r?\Z)|" + _WORD_STR + r")")
_NB_STR = r"(?<![A-Za-z0-9_.\-])"


def account_root_re(tail):
    """A regex (text) matching a home-path prefix, an account segment and then ``tail``.

    ``tail`` is a regex fragment that follows the account segment directly (it usually starts
    with a separator class). The pattern carries the groups ``pre``, ``sep`` and ``seg`` of the
    bytes rule, so a caller can leave an allowed segment unchanged and rewrite any other.
    """
    return re.compile(
        r"(?P<pre>[A-Za-z](?::|%3[aA])" + _SEP_STR + r"(?i:users)"
        r"|" + _NB_STR + r"(?:/mnt)?/[a-z]" + _SEP_STR + r"(?i:users)"
        r"|" + _NB_STR + r"(?:/|\\+|%2[fF]|%5[cC])(?:Users|home))"
        r"(?P<sep>" + _SEP_STR + r")" + _SEG_STR + tail
    )


def _form_of(match, is_slug, line):
    if is_slug:
        return "F5"
    pre, sep = match.group("pre"), match.group("sep")
    if b"%" in pre or b"%" in sep or line[:match.start()].endswith(b"file:///"):
        return "F8"
    if pre[1:2] == b":":
        if pre[-5:] != b"Users":
            return "F3"
        return "F1" if pre[2:3] == b"\\" else "F2"
    if _DRIVE_ROOT.fullmatch(pre):
        return "F4"
    return "F6" if pre.endswith(b"Users") else "F7"


def local_accounts(candidates=None):
    """The account names of this machine that F10 looks for, as a tuple (empty when none remains).

    Default candidates: getpass.getuser(), the home folder name and the non-empty LOGNAME, USER and
    USERNAME. Each is stripped; a name shorter than MIN_ACCOUNT_LENGTH, on ALLOWED_SEGMENTS or in
    GENERIC_ACCOUNTS is dropped, and so is a repeat (case-folded; the first spelling is kept).
    """
    if candidates is None:
        candidates = []
        try:
            candidates.append(getpass.getuser())
        except Exception:
            pass
        try:
            candidates.append(Path.home().name)
        except Exception:
            pass
        for name in ("LOGNAME", "USER", "USERNAME"):
            value = os.environ.get(name)
            if value:
                candidates.append(value)
    dropped = {name.casefold() for name in ALLOWED_SEGMENTS} | {name.casefold() for name in GENERIC_ACCOUNTS}
    accounts = []
    for candidate in candidates:
        name = (candidate or "").strip()
        folded = name.casefold()
        if len(name) < MIN_ACCOUNT_LENGTH or folded in dropped:
            continue
        dropped.add(folded)
        accounts.append(name)
    return tuple(accounts)


@functools.lru_cache(maxsize=None)
def _account_pattern(accounts):
    names = sorted({name for name in accounts if name}, key=len, reverse=True)
    if not names:
        return None
    alternation = "|".join(re.escape(name) for name in names)
    return re.compile(r"(?<![^\W_])(?:" + alternation + r")(?![^\W_])", re.IGNORECASE)


def _as_text(data):
    return data.decode("utf-8", "surrogateescape") if isinstance(data, (bytes, bytearray)) else data


def find_accounts(text, accounts):
    """Each line of ``text`` holding one of ``accounts`` as a whole word, as Hit(line, "F10").

    A letter or digit next to the name blocks a match; any other neighbour does not. Any case.
    """
    pattern = _account_pattern(tuple(accounts))
    if pattern is None:
        return []
    return [Hit(number, "F10") for number, line in enumerate(_as_text(text).split("\n"), start=1)
            if pattern.search(line)]


def find(path, text, accounts=()):
    """Each home path in ``text`` as Hit(line, form); never the matched text.

    Uses the same PATH and SLUG regexes as scrub_bytes, so a hit is reported exactly where
    scrub_bytes would change a segment (allow-lists honoured).
    With ``accounts``, a line holding one of them as a whole word is reported as F10, unless the
    line already has a home-path hit (one finding per occurrence, never two).
    """
    data = text.encode("utf-8", "surrogateescape") if isinstance(text, str) else bytes(text)
    allowed = _allowed(path)
    pattern = _account_pattern(tuple(accounts))
    hits = []
    for number, line in enumerate(data.split(b"\n"), start=1):
        found = []
        for regex, is_slug in ((PATH, False), (SLUG, True)):
            for match in regex.finditer(line):
                if match.group("seg") in allowed:
                    continue
                found.append((match.start(), is_slug, match))
        for _start, is_slug, match in sorted(found, key=lambda item: item[0]):
            hits.append(Hit(number, _form_of(match, is_slug, line)))
        if not found and pattern is not None and pattern.search(_as_text(line)):
            hits.append(Hit(number, "F10"))
    return hits


def scrub_account_bytes(data, accounts):
    """The F10 rewrite: each whole-word occurrence of ``accounts`` (any case) becomes PLACEHOLDER.

    Every other byte, line endings included, stays. Not part of scrub_bytes or scrub_tracked.
    """
    pattern = _account_pattern(tuple(accounts))
    if pattern is None:
        return bytes(data)
    text = bytes(data).decode("utf-8", "surrogateescape")
    return pattern.sub(PLACEHOLDER, text).encode("utf-8", "surrogateescape")


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
