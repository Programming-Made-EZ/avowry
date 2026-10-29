#!/usr/bin/env python3
"""Generate docs/decisions/INDEX.md from areas.json and area records."""

import argparse
from pathlib import Path

from decision_files import load_metadata, records_from_areas, render_index


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path.cwd())
    args = parser.parse_args()
    decisions = args.root / "docs" / "decisions"
    metadata = load_metadata(decisions)
    records, _ = records_from_areas(decisions)
    missing = set(metadata) - set(records)
    if missing:
        parser.error(f"records missing from area files: {sorted(missing)}")
    (decisions / "INDEX.md").write_text(render_index(metadata, records), encoding="utf-8", newline="")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
