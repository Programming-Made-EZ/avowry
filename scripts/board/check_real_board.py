#!/usr/bin/env python3
"""Read-only verification for the private Phrase Board project and its card issues."""

import argparse
import json
import re
import subprocess
import sys
from pathlib import Path

OWNER = "Programming-Made-EZ"
REPO = "phrase-lang"
PROJECT_NAME = "Phrase Board"
BOARD_DIR = Path(__file__).resolve().parents[2] / "board"
CARD_FILE_RE = re.compile(r"^(\d{3})-.+\.md$")
MARKER_RE = re.compile(r"<!-- phrase-card:(\d{3}) -->")
EXPECTED_FIELDS = [
    "Rank",
    "Card number",
    "Depends on",
    "Kind",
    "Phase",
    "Card author",
    "Shape",
    "Enables",
]
EXPECTED_STATUS_OPTIONS = ["Ready", "In Progress", "Done", "Superseded/Withdrawn"]


def gh_runner(args, *, check=True, input_text=None):
    proc = subprocess.run(["gh", *[str(part) for part in args]], capture_output=True, text=True)
    if check and proc.returncode != 0:
        raise RuntimeError(proc.stderr.strip() or proc.stdout.strip() or "gh command failed")
    return proc.stdout.strip()


def parse_json(text):
    if not text or not text.strip():
        return {}
    try:
        return json.loads(text)
    except json.JSONDecodeError:
        return {}


def find_project(gh):
    payload = gh(["project", "list", "--owner", OWNER, "--limit", "200", "--format", "json"])
    data = parse_json(payload)
    projects = data.get("projects", []) if isinstance(data, dict) else ([] if not isinstance(data, list) else data)
    if not isinstance(projects, list):
        return None
    for project in projects:
        if not isinstance(project, dict):
            continue
        name = str(project.get("name") or project.get("title") or "").strip()
        if name == PROJECT_NAME:
            return project
    return None


def fetch_fields(gh, project_number):
    payload = gh(["project", "field-list", str(project_number), "--owner", OWNER, "--format", "json"])
    data = parse_json(payload)
    fields = data.get("fields", []) if isinstance(data, dict) else ([] if not isinstance(data, list) else data)
    result = []
    for field in fields:
        if not isinstance(field, dict):
            continue
        result.append({
            "name": field.get("name"),
            "type": field.get("type"),
            "options": [option.get("name") for option in field.get("options", []) if option.get("name")],
        })
    return result


def card_file_numbers(board_dir=None):
    board = Path(board_dir) if board_dir is not None else BOARD_DIR
    numbers = []
    for path in sorted(board.iterdir(), key=lambda item: item.name):
        match = CARD_FILE_RE.match(path.name)
        if match and path.is_file():
            numbers.append(match.group(1))
    return numbers


def verify_issues(gh, board_dir=None):
    """Count issues that carry a phrase-card marker. Read-only.

    Fails unless each card file number has exactly one issue and no issue
    carries a marker for any other number.
    """
    numbers = card_file_numbers(board_dir)
    payload = gh([
        "issue", "list", "--repo", f"{OWNER}/{REPO}", "--state", "all", "--limit", "1000",
        "--json", "number,title,body,state",
    ])
    if payload is None or not str(payload).strip():
        issues = []
    else:
        issues = json.loads(payload)
    if not isinstance(issues, list):
        raise RuntimeError("Issue list did not return a JSON array.")

    found = {}
    for issue in issues:
        if not isinstance(issue, dict):
            continue
        body = issue.get("body") or ""
        for marker in set(MARKER_RE.findall(body)):
            found.setdefault(marker, []).append(issue.get("number"))

    wanted = set(numbers)
    missing = [number for number in numbers if len(found.get(number, [])) == 0]
    duplicates = [number for number, issue_ids in found.items() if len(issue_ids) > 1]
    extras = sorted(number for number in found if number not in wanted)
    used = []
    for number in numbers:
        issue_ids = found.get(number, [])
        if len(issue_ids) == 1:
            used.append(issue_ids[0])
    shared = len(used) != len(set(used))
    if missing or duplicates or extras or shared:
        raise RuntimeError(
            "Marked issues do not match card files. "
            f"missing={missing} duplicates={duplicates} extras={extras} shared={shared}"
        )
    return len(numbers)


def verify_project(gh):
    project = find_project(gh)
    if project is None:
        raise RuntimeError(f"Project {PROJECT_NAME!r} missing under {OWNER!r}.")

    public_flag = bool(project.get("public"))
    visibility = str(project.get("visibility") or "").upper()
    if public_flag or visibility == "PUBLIC":
        raise RuntimeError(f"Project visibility is not private: public={public_flag!r}, visibility={visibility!r}")

    project_number = project.get("number")
    if project_number is None:
        raise RuntimeError("Project list output did not include a project number.")

    fields = fetch_fields(gh, project_number)
    field_names = [field.get("name") for field in fields if field.get("name")]
    missing = [name for name in EXPECTED_FIELDS if name not in field_names]
    if missing:
        raise RuntimeError(f"Missing expected fields: {missing}")

    status_field = None
    for field in fields:
        if field.get("name") == "Status":
            status_field = field
            break
    if status_field is None:
        raise RuntimeError("Status field missing.")

    options = status_field.get("options", [])
    if sorted(options) != sorted(EXPECTED_STATUS_OPTIONS):
        raise RuntimeError(f"Status options mismatch: {options!r} != {EXPECTED_STATUS_OPTIONS!r}")

    return project


def verify_fields(gh, board_dir=None, running=None):
    """Read every project item and compare it with the card-derived values."""
    from scripts.board import set_fields

    payload = parse_json(gh(["project", "item-list", "1", "--owner", OWNER, "--format", "json", "--limit", "1000"]))
    items = payload.get("items", []) if isinstance(payload, dict) else []
    by_number = {}
    for item in items:
        number = item.get("card number")
        if number is not None:
            by_number[f"{int(float(number)):03d}"] = item
    for number, item in by_number.items():
        if item.get("shape") != "Contract":
            continue
        enables = re.findall(r"(?<!\d)\d{3}(?!\d)", str(item.get("enables") or ""))
        if not enables:
            raise RuntimeError(f"Contract {number} has empty Enables")
        for enabled in enables:
            target = by_number.get(enabled)
            if target is None:
                raise RuntimeError(f"Contract {number} enables card {enabled}, which is not on the board")
            if target.get("shape") == "Contract":
                raise RuntimeError(f"Contract {number} enables Contract {enabled}")

    cards = set_fields.load_cards(board_dir or BOARD_DIR, running=running)
    _project, _fields, actions = set_fields.plan(gh, board_dir or BOARD_DIR, running=running)
    if actions:
        pending = [f"{action} {card.number}" for action, card, _issue, _item, _detail in actions]
        raise RuntimeError("Project fields do not match; dry run still has work: " + ", ".join(pending))
    return len(cards)


def verify_bodies(gh, board_dir=None):
    """Read every marked issue and compare its body with the card file. Read-only.

    Line endings are folded from CRLF to LF on both sides. Fails unless each
    card has exactly one marked issue and that body is the one the migrator
    would build.
    """
    from scripts.board import generate_archive, migrate_cards

    board = Path(board_dir) if board_dir is not None else BOARD_DIR
    if (board / "CUTOVER").exists():
        # after the cutover the files are generated from the issues: the check is the archive check
        if generate_archive.main(["--check"], gh_runner=gh, board_dir=board):
            raise RuntimeError("the generated archive differs from the issues: run scripts/board/generate_archive.py --apply")
        return len(list(board.glob("[0-9][0-9][0-9]-*.md")))
    cards = migrate_cards.load_cards(board)
    numbers = [card.number for card in cards]
    issues = migrate_cards.fetch_issues(gh, f"{OWNER}/{REPO}", lambda _seconds: None)

    found = {}
    for issue in issues:
        if not isinstance(issue, dict):
            continue
        body = issue.get("body") or ""
        for marker in set(MARKER_RE.findall(body)):
            found.setdefault(marker, []).append(issue)

    wanted = set(numbers)
    missing = [number for number in numbers if len(found.get(number, [])) == 0]
    duplicates = [number for number, items in found.items() if len(items) > 1]
    extras = sorted(number for number in found if number not in wanted)
    used = []
    for number in numbers:
        items = found.get(number, [])
        if len(items) == 1:
            used.append(items[0].get("number"))
    shared = len(used) != len(set(used))

    by_number = {card.number: card for card in cards}
    mismatches = []
    for number in numbers:
        items = found.get(number, [])
        if len(items) != 1:
            continue
        actual = migrate_cards.normalise_newlines(items[0].get("body") or "")
        expected = migrate_cards.normalise_newlines(by_number[number].body)
        if actual != expected:
            mismatches.append(number)

    if missing or duplicates or extras or shared or mismatches:
        raise RuntimeError(
            "Issue bodies do not match card files. "
            f"missing={missing} duplicates={duplicates} extras={extras} "
            f"shared={shared} mismatches={mismatches}"
        )
    return len(cards)


def expected_pick(items):
    """The first Ready item by Rank whose dependencies are all Done (an independent re-statement of the picker's rules)."""
    import re

    by_number = {}
    for x in items:
        n = x.get("card number")
        if n is not None:
            by_number["%03d" % int(n)] = x
    ready = [x for x in items if str(x.get("status", "")).lower() == "ready" and x.get("rank") is not None]
    for x in sorted(ready, key=lambda y: y["rank"]):
        raw = x.get("depends on")
        if raw is None:
            continue
        text = str(raw).strip().lower()
        deps = [] if text == "none" or re.fullmatch(r"none\s*\(.*\)", text) else re.findall(r"(?<!\d)(\d{3})(?!\d)", str(raw))
        if all(d in by_number and str(by_number[d].get("status", "")).lower() == "done" for d in deps):
            return "%03d" % int(x["card number"])
    return None


def verify_pick(gh, picker):
    """Read-only: the live board's expected pick must equal what the loop's picker says (picker() returns its output line)."""
    payload = parse_json(gh(["project", "item-list", "1", "--owner", OWNER, "--format", "json", "--limit", "1000"]))
    expected = expected_pick(payload.get("items", []))
    said = (picker() or "").split()[:1]
    got = said[0] if said and said[0].isdigit() else None
    if got != expected:
        raise RuntimeError(f"picker says {got!r} but the first eligible Ready card by Rank is {expected!r}")
    return expected


def default_picker():
    import os

    repo = os.environ.get("MOB_DEFAULTS", r"C:\Users\ringe\mob-crosscli")
    out = subprocess.run([sys.executable, "loop.py", "--pick"], cwd=repo, capture_output=True, text=True)
    return out.stdout.strip()


def main(argv=None, gh_runner=gh_runner):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", action="store_true", help="verify the project exists and has the correct fields")
    parser.add_argument("--issues", action="store_true", help="verify exactly one marked issue exists per card file")
    parser.add_argument("--fields", action="store_true", help="verify every card item's values (read-only)")
    parser.add_argument("--bodies", action="store_true", help="verify each marked issue body matches its card file (read-only)")
    parser.add_argument("--pick", action="store_true", help="verify the loop picker returns the first eligible Ready card by Rank (read-only)")
    args = parser.parse_args(argv)

    if not args.project and not args.issues and not args.fields and not args.bodies and not args.pick:
        parser.print_help()
        return 1

    try:
        count = None
        if args.project:
            verify_project(gh_runner)
        if args.issues:
            count = verify_issues(gh_runner)
        if args.fields:
            count = verify_fields(gh_runner)
        if args.bodies:
            count = verify_bodies(gh_runner)
        picked = verify_pick(gh_runner, default_picker) if args.pick else None
    except Exception as exc:
        print(f"FAIL: {exc}")
        return 1

    if args.project:
        print(f"OK: {PROJECT_NAME} exists privately under {OWNER} with the expected fields and status options.")
    if args.issues:
        print(f"OK: {count} marked issues, exactly one per card file.")
    if args.fields:
        print(f"OK: {count} project items match their card fields; a second dry run has nothing to do.")
    if args.bodies:
        print(f"OK: {count} issue bodies match their card files, exactly one per card.")
    if args.pick:
        print(f"OK: the loop picker and the board agree: next eligible card is {picked}.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
