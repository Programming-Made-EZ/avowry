#!/usr/bin/env python3
"""Check decision-area ownership, references, index freshness, and migration proof."""

import argparse
import re
import sys
from pathlib import Path

from decision_files import area_paths, load_metadata, proof, records_from_areas, render_index


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path.cwd())
    args = parser.parse_args()
    decisions = args.root / "docs" / "decisions"
    metadata = load_metadata(decisions)
    records, locations = records_from_areas(decisions)
    errors = []

    for identifier, files in locations.items():
        if identifier not in metadata:
            errors.append(f"{identifier} has no areas.json entry")
        if len(files) > 1:
            errors.append(f"{identifier} appears in more than one area file: {', '.join(files)}")
    for identifier, item in metadata.items():
        expected = f"{item['area']}.md"
        if identifier not in locations:
            errors.append(f"{identifier} is missing from {expected}")
        elif expected not in locations[identifier]:
            errors.append(f"{identifier} is in {locations[identifier][0]}, expected {expected}")
        match = re.fullmatch(r"superseded by (D\d+)", item["status"])
        if match and not any(match.group(1) in part.split("/") for part in metadata):
            errors.append(f"{identifier} supersedes to {match.group(1)}, but {match.group(1)} does not exist")

    for path in area_paths(decisions):
        if not path.exists() or "## In force" not in path.read_text(encoding="utf-8"):
            errors.append(f"{path.name} has no In force section")

    if set(metadata) <= set(records):
        expected_index = render_index(metadata, records)
        actual_index = (decisions / "INDEX.md").read_text(encoding="utf-8") if (decisions / "INDEX.md").exists() else ""
        if actual_index != expected_index:
            errors.append("INDEX.md is out of date with areas.json")

    migration = decisions / "MIGRATION.txt"
    if migration.exists() and set(metadata) <= set(records):
        match_count = re.search(r"(?m)^count (\d+)$", migration.read_text(encoding="utf-8"))
        match_hash = re.search(r"(?m)^sha256 ([0-9a-f]{64})$", migration.read_text(encoding="utf-8"))
        recorded = int(match_count.group(1)) if match_count else None
        count, digest = proof(records, metadata, recorded)
        if recorded is None or recorded != count:
            errors.append(f"MIGRATION.txt count does not match {count} area records")
        if not match_hash or match_hash.group(1) != digest:
            errors.append("MIGRATION.txt sha256 does not match the area records")

    if errors:
        for error in errors:
            print(error, file=sys.stderr)
        return 1
    print(f"decision index: {len(metadata)} records across 5 areas, passed")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
