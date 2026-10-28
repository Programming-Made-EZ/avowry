#!/usr/bin/env python3
"""Publish numbers from private benchmark rows.

Per arm, model and kind the summary has scheduled, completed and unavailable
row counts, pass rate, and mean attempts and total tokens over completed rows
only. A Phrase group also lists the documentation-token share when a row carries
documentation_tokens or docs_tokens. Task text, replies, reply heads, prompts
and task ids are left out.
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import defaultdict
from pathlib import Path


DOC_TOKEN_FIELDS = ("documentation_tokens", "docs_tokens")


def arm_of(row: dict) -> str:
    arm = row.get("arm")
    if arm in ("phrase", "csharp"):
        return arm
    if row.get("mode") == "plain-csharp":
        return "csharp"
    return "phrase"


def kind_of(row: dict) -> str:
    kind = row.get("kind")
    return kind if isinstance(kind, str) else ""


def documentation_tokens(row: dict) -> float | None:
    for name in DOC_TOKEN_FIELDS:
        value = row.get(name)
        if isinstance(value, bool) or not isinstance(value, (int, float)):
            continue
        return float(value)
    return None


def load_rows(paths: list[Path]) -> list[dict]:
    rows: list[dict] = []
    for path in paths:
        for line in path.read_text(encoding="utf-8").splitlines():
            line = line.strip()
            if not line:
                continue
            value = json.loads(line)
            if isinstance(value, dict):
                rows.append(value)
    return rows


def summarize(rows: list[dict]) -> dict:
    grouped: dict[tuple[str, str, str], list[dict]] = defaultdict(list)
    for row in rows:
        grouped[(arm_of(row), str(row.get("model") or ""), kind_of(row))].append(row)

    groups = []
    for arm, model, kind in sorted(grouped):
        items = grouped[(arm, model, kind)]
        count = len(items)
        completed = [row for row in items if row.get("status") == "completed"]
        completed_count = len(completed)
        unavailable_count = count - completed_count
        passed = sum(1 for row in items if row.get("passed") is True)
        attempts = sum(float(row.get("attempts") or 0) for row in completed)
        completed_tokens = sum(
            float(row.get("tokens_in") or 0) + float(row.get("tokens_out") or 0) for row in completed)
        entry = {
            "arm": arm,
            "model": model,
            "kind": kind,
            "count": count,
            "completed_count": completed_count,
            "unavailable_count": unavailable_count,
            "pass_rate": (passed / count) if count else 0.0,
            "mean_attempts": (attempts / completed_count) if completed_count else 0.0,
            "mean_total_tokens": (completed_tokens / completed_count) if completed_count else 0.0,
        }
        if arm == "phrase":
            present = [documentation_tokens(row) for row in items]
            if any(value is not None for value in present):
                documented = sum(value or 0.0 for value in present)
                total_tokens = sum(
                    float(row.get("tokens_in") or 0) + float(row.get("tokens_out") or 0) for row in items)
                entry["documentation_tokens_share"] = (documented / total_tokens) if total_tokens else 0.0
        groups.append(entry)
    return {"groups": groups}


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Summarize private benchmark rows as numbers only.")
    parser.add_argument("rows", nargs="+", type=Path)
    parser.add_argument("--out", required=True, type=Path)
    args = parser.parse_args(argv)
    summary = summarize(load_rows(args.rows))
    if args.out.parent != Path(""):
        args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(summary, indent=2) + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    sys.exit(main())
