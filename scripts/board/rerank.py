#!/usr/bin/env python3
"""Move cards in the Rank order of the Phrase Board: place the named cards right after another card, renumber, write only what changes.

Usage: rerank.py --after NNN --cards 084,085,086 [--apply]

Rank is the only field written (one item-edit per changed item). Default is a dry run. Cards without a Rank that are not named stay unranked.
Every named card and the anchor must be on the board, or nothing is written. Never deletes; the owner is hard-wired. A re-rank is a
board decision: the loop's retro hook calls this only after a passed Decider (or on Steve's direction); a human phone edit of Rank is the
other way to reorder. Run scripts/board/generate_archive.py --apply afterwards to refresh the archive table."""

import argparse
import sys

from scripts.board import set_fields

OWNER = set_fields.OWNER
PROJECT = set_fields.PROJECT_NUMBER


def _card(item):
    value = item.get("card number")
    return int(value) if value is not None else None


def plan(items, after, cards):
    by_card = {_card(x): x for x in items if _card(x) is not None}
    for number in [after, *cards]:
        if number not in by_card:
            raise ValueError(f"card {number:03d} is not on the board")
    if after in cards:
        raise ValueError("the anchor card cannot also be moved")
    ranked = sorted((x for x in items if x.get("rank") is not None and _card(x) not in cards), key=lambda x: (x["rank"], _card(x)))
    order = [_card(x) for x in ranked]
    at = order.index(after) + 1 if after in order else None
    if at is None:
        raise ValueError(f"the anchor card {after:03d} has no Rank")
    order[at:at] = list(cards)
    return [(number, by_card[number], new_rank) for new_rank, number in enumerate(order, 1) if by_card[number].get("rank") != new_rank]


def rerank(gh, after, cards, owner=OWNER, dry_run=True):
    set_fields.validate_target(owner, set_fields.REPO)
    items = set_fields.parse_json(gh(["project", "item-list", str(PROJECT), "--owner", owner, "--format", "json", "--limit", "1000"])).get("items", [])
    changes = plan(items, after, list(cards))
    if dry_run or not changes:
        return changes
    project_id = set_fields.parse_json(gh(["project", "view", str(PROJECT), "--owner", owner, "--format", "json"])).get("id")
    fields = set_fields.parse_json(gh(["project", "field-list", str(PROJECT), "--owner", owner, "--format", "json"])).get("fields", [])
    rank_field = next(f for f in fields if f.get("name") == "Rank")
    for number, item, new_rank in changes:
        gh(["project", "item-edit", "--id", item["id"], "--project-id", project_id, "--field-id", rank_field["id"], "--number", str(new_rank)])
        item["rank"] = new_rank
    return changes


def main(argv=None, gh_runner=set_fields.gh_runner):
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--after", type=int, required=True)
    ap.add_argument("--cards", required=True, help="comma-separated card numbers, in the order they should follow the anchor")
    ap.add_argument("--apply", action="store_true")
    args = ap.parse_args(argv)
    cards = [int(x) for x in args.cards.split(",") if x.strip()]
    try:
        changes = rerank(gh_runner, args.after, cards, dry_run=not args.apply)
    except ValueError as exc:
        print(f"ERROR: {exc}")
        return 2
    except Exception as exc:
        print(f"ERROR: {exc}")
        return 1
    if not changes:
        print("Nothing to do: the order already matches.")
        return 0
    for number, item, new_rank in changes:
        print(("applied: " if args.apply else "would: ") + f"({number:03d}) rank {item.get('rank')} -> {new_rank}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
