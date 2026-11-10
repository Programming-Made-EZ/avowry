"""Fail if the bench history holds a drive path, a tilde-relative path or a local account name.

A hit names the file, the line and the kind only; the matched text (and so the account) is never
printed. The account names come from home_paths.local_accounts (card 376), so this file names none.
"""

import re
import sys
from collections import namedtuple
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import home_paths  # noqa: E402

PATTERNS = (
    ("drive path", re.compile(r"(?<![A-Za-z0-9_])[A-Za-z]:[\\/][^\s\"'<>]*")),
    ("tilde-relative path", re.compile(r"(?<![A-Za-z0-9_])~[\\/][^\s\"'<>]*")),
)
NAME_KIND = "user name"

Leak = namedtuple("Leak", "line kind")


def scan_history(path, accounts=None):
    """Each leak in ``path`` as Leak(line, kind). ``accounts=None`` means home_paths.local_accounts().

    A name inside a path already reported on that line is not reported again as a user name.
    """
    if accounts is None:
        accounts = home_paths.local_accounts()
    hits = []
    for line_number, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
        rest = line
        for kind, pattern in PATTERNS:
            hits.extend(Leak(line_number, kind) for _match in pattern.finditer(line))
            rest = pattern.sub(" ", rest)
        if home_paths.find_accounts(rest, accounts):
            hits.append(Leak(line_number, NAME_KIND))
    return hits


def main(argv=None):
    argv = sys.argv[1:] if argv is None else argv
    path = Path(argv[0]) if argv else Path("bench/results/history.jsonl")
    hits = scan_history(path)
    for line_number, kind in hits:
        print(f"{path}:{line_number}:{kind}")
    if hits:
        print(f"Found {len(hits)} potential leak(s).")
        return 1
    print(f"{path}: clean")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
