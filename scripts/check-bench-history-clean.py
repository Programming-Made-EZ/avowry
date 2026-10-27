import re
import sys
from pathlib import Path


PATTERNS = (
    ("drive path", re.compile(r"(?<![A-Za-z0-9_])[A-Za-z]:[\\/][^\s\"'<>]*")),
    ("tilde-relative path", re.compile(r"(?<![A-Za-z0-9_])~[\\/][^\s\"'<>]*")),
    ("user name", re.compile(r"ringe", re.IGNORECASE)),
)


def scan_history(path):
    hits = []
    for line_number, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
        for kind, pattern in PATTERNS:
            hits.extend((line_number, kind, match.group()) for match in pattern.finditer(line))
    return hits


def main(argv=None):
    argv = sys.argv[1:] if argv is None else argv
    path = Path(argv[0]) if argv else Path("bench/results/history.jsonl")
    hits = scan_history(path)
    for line_number, kind, match in hits:
        print(f"{path}:{line_number}: {kind}: {match}")
    if hits:
        print(f"Found {len(hits)} potential leak(s).")
        return 1
    print(f"{path}: clean")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
