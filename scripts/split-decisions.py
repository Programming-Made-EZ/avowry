#!/usr/bin/env python3
"""One-shot migration from DECIDERS.md to cohesive area histories."""

import argparse
import json
from pathlib import Path

from decision_files import AREAS, load_metadata, parse_records, render_area


def draft_statements(path):
    if not path.exists():
        return {}
    statements = {}
    for line in path.read_text(encoding="utf-8").splitlines():
        if not line.startswith("- ") or " (in force) " not in line:
            continue
        identifier, statement = line[2:].split(" (in force) ", 1)
        statement = statement.rsplit(" [also:", 1)[0].rstrip(".")
        # The draft is authoritative, but long clauses are compacted so generated
        # Markdown remains readable without changing the full record below it.
        if len(statement) > 165:
            clauses = [part.strip() for part in statement.split(";")]
            statement = clauses[0]
        if len(statement) > 165:
            statement = statement[:162].rsplit(" ", 1)[0] + "…"
        statements[identifier] = statement
    return statements


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path.cwd())
    args = parser.parse_args()
    decisions = args.root / "docs" / "decisions"
    metadata = load_metadata(decisions)
    source = (decisions / "DECIDERS.md").read_text(encoding="utf-8", newline="")
    ordered_records = parse_records(source)
    records = dict(ordered_records)
    statements = draft_statements(decisions / "index-draft.md")
    missing = set(metadata) - set(records)
    extra = set(records) - set(metadata)
    if missing or extra:
        parser.error(f"mapping/log mismatch; missing={sorted(missing)}, extra={sorted(extra)}")

    for order, (identifier, _) in enumerate(ordered_records):
        metadata[identifier]["order"] = order
        if metadata[identifier]["status"] == "in force" and identifier in statements:
            metadata[identifier]["statement"] = statements[identifier]
    ordered_metadata = {identifier: metadata[identifier] for identifier, _ in ordered_records}
    (decisions / "areas.json").write_text(json.dumps(ordered_metadata, indent=2) + "\n", encoding="utf-8", newline="")
    for area in AREAS:
        (decisions / f"{area}.md").write_text(
            render_area(area, ordered_metadata, records), encoding="utf-8", newline=""
        )
    (decisions / "DECIDERS.md").write_text(
        "# Decider log moved\n\n"
        "The append-only decision history is now split by area so readers can load one concern at a time.\n\n"
        "Start with [the decision index](INDEX.md), which links every decision to its unchanged record. "
        "This path remains as a stable landing page for older references to `docs/decisions/DECIDERS.md`.\n\n"
        "## How records are written\n\n"
        "Durable copy of Decider runs. Per-card MOB.md files are gitignored, so results that outlive a card are kept here.\n"
        "Format: `Decider D<n>: <proposal>`, votes, `Result: passed|failed`. All-Yes results may be a single line.\n"
        "A new record is appended to its area file and gets one entry in `areas.json`.\n\n"
        "To find a decision by number (for example D86 or D21), open INDEX.md: its table lists every number with its area file and link.\n",
        encoding="utf-8",
        newline="",
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
