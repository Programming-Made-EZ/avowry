#!/usr/bin/env python3
"""Add ONE new card to the Phrase Board after the cutover: a draft card file becomes an issue and a project item with its fields.

Usage: new_card.py DRAFT.md [--apply]

DRAFT.md is named NNN-slug.md (three digits) and written like a board card (Connextra story naming a persona, or a Contract with an Actor
and Enables, Depends on, Done when). It lives OUTSIDE board/ (board/ is a generated archive). Default is a dry run; --apply creates the issue
(labels card, and tooling for tooling cards), adds it to project 1 and sets Status, Card number, Depends on, Kind, Phase, Card author, Shape
and Enables from the draft. It never sets Rank (use rerank.py), never deletes, refuses any owner or repo but Programming-Made-EZ/avowry,
and creates nothing if the card already has a marked issue. Afterwards run scripts/board/generate_archive.py --apply to refresh the archive."""

import argparse
import importlib.util
import re
import sys
import time
from pathlib import Path

from scripts.board import migrate_cards, set_fields

OWNER = migrate_cards.OWNER
REPO = migrate_cards.REPO
ROOT = Path(__file__).resolve().parents[2]


def _lint():
    spec = importlib.util.spec_from_file_location("check_story_persona", ROOT / "scripts" / "check-story-persona.py")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def check_draft(path):
    """The Ready check's mechanical half, before anything is created."""
    lint = _lint()
    text = Path(path).read_text(encoding="utf-8")
    shape = re.search(r"(?m)^- \*\*Shape:\*\*\s*(\w+)", text)
    if shape and shape.group(1).lower() == "contract":
        if not re.search(r"(?m)^- \*\*Enables:\*\*.*\b\d{3}\b", text):
            raise ValueError("a Contract draft needs an Enables line naming a Story card (no orphan contracts)")
        return
    problem = lint.check_card(path, lint.persona_names(ROOT / "PERSONAS.md"))
    if problem:
        raise ValueError(f"the draft fails the Ready check: {problem}")


def new_card(gh, draft_path, owner=OWNER, repo=REPO, dry_run=True, sleep=None):
    migrate_cards.ensure_target(owner, repo)
    sleep = sleep or time.sleep
    path = Path(draft_path)
    if not migrate_cards.CARD_FILE_RE.match(path.name):
        raise ValueError("the draft must be named NNN-slug.md with three digits")
    check_draft(path)
    slug = f"{owner}/{repo}"
    card = migrate_cards.load_card(path)
    existing = migrate_cards.index_markers(migrate_cards.fetch_issues(gh, slug, sleep))
    if card.number in existing:
        return [f"skip ({card.number}): an issue already carries its marker"]
    actions = [f"create issue ({card.number}) {card.title}", "add it to the project", "set Status, Card number, Depends on, Kind, Phase, Card author, Shape, Enables"]
    if dry_run:
        return actions
    migrate_cards.ensure_labels(gh, slug, sleep)
    number = migrate_cards.create_issue(gh, slug, card, sleep)
    url = f"https://github.com/{slug}/issues/{number}"
    project_id = set_fields.parse_json(gh(["project", "view", str(set_fields.PROJECT_NUMBER), "--owner", owner, "--format", "json"])).get("id")
    fields = set_fields.parse_json(gh(["project", "field-list", str(set_fields.PROJECT_NUMBER), "--owner", owner, "--format", "json"])).get("fields", [])
    field_map = {f.get("name"): f for f in fields}
    item = set_fields.parse_json(gh(["project", "item-add", str(set_fields.PROJECT_NUMBER), "--owner", owner, "--url", url, "--format", "json"]))
    values = next(c for c in set_fields.load_cards(path.parent) if c.number == card.number)
    edits = [("edit", values, None, item, (field_map[name], value)) for name, value in values.values.items() if name != "Rank"]
    set_fields.apply_actions(gh, project_id, field_map, edits, owner, repo)
    return actions


def main(argv=None, gh_runner=set_fields.gh_runner):
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("draft")
    ap.add_argument("--apply", action="store_true")
    ap.add_argument("--owner", default=OWNER)
    ap.add_argument("--repo", default=REPO)
    args = ap.parse_args(argv)
    try:
        actions = new_card(gh_runner, args.draft, args.owner, args.repo, dry_run=not args.apply)
    except ValueError as exc:
        print(f"ERROR: {exc}")
        return 2
    except Exception as exc:
        print(f"ERROR: {exc}")
        return 1
    for line in actions:
        print(("applied: " if args.apply else "would: ") + line)
    return 0


if __name__ == "__main__":
    sys.exit(main())
