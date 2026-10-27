#!/usr/bin/env python3
"""Put marked card issues on Phrase Board and set their card fields.

The default is a read-only dry run.  --apply only adds missing items, changes
field values that differ, and closes an open issue when its card is Done.
"""

import argparse
import json
import re
import subprocess
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Sequence

OWNER = "Programming-Made-EZ"
REPO = "phrase-lang"
PROJECT_NUMBER = 1
BOARD_DIR = Path(__file__).resolve().parents[2] / "board"
CARD_FILE_RE = re.compile(r"^(\d{3})-.+\.md$")
MARKER_RE = re.compile(r"<!-- phrase-card:(\d{3}) -->")
KINDS = ("feature", "bug", "docs", "tooling", "spike", "decision", "data")
STATUSES = ("Ready", "In Progress", "Done", "Superseded/Withdrawn")
SHAPES = ("Story", "Contract")
NUMBER_FIELDS = ("Rank", "Card number")
FIELD_NAMES = ("Rank", "Card number", "Depends on", "Kind", "Phase", "Card author", "Shape", "Enables", "Status")


@dataclass(frozen=True)
class CardValues:
    number: str
    path: Path
    values: dict


TRANSIENT = ("something went wrong", "rate limit", "502", "503", "504", "timed out", "timeout")


def gh_runner(args: Sequence[str], *, check=True, input_text=None, attempts=4):
    for attempt in range(1, attempts + 1):
        proc = subprocess.run(["gh", *map(str, args)], input=input_text, capture_output=True, text=True)
        if not proc.returncode:
            break
        detail = (proc.stderr or proc.stdout or "").lower()
        if attempt < attempts and any(word in detail for word in TRANSIENT):
            time.sleep(2 * attempt)  # a transient GitHub error: wait and retry the same call
            continue
        break
    if check and proc.returncode:
        raise RuntimeError(proc.stderr.strip() or proc.stdout.strip() or "gh command failed")
    return proc.stdout.strip()


def parse_json(payload):
    return json.loads(payload) if payload and str(payload).strip() else {}


def validate_target(owner, repo):
    if owner != OWNER:
        raise ValueError(f"Owner must be {OWNER}; refusing {owner!r}.")
    if repo != REPO:
        raise ValueError(f"Repo must be {REPO}; refusing {repo!r}.")


def item_key(field_name):
    return field_name.lower()  # the real gh item-list keeps spaces: 'card number', 'depends on'


def _line(text, label):
    match = re.search(rf"(?m)^- \*\*{re.escape(label)}:\*\*\s*(.*)$", text)
    return match.group(1).strip() if match else None


def _phase(text):
    value = _line(text, "Milestone")
    if value is None:
        return None
    sentence = re.match(r".*?[.!?](?=\s|$)", value)
    return (sentence.group(0) if sentence else value)[:60]


def _author(text):
    match = re.search(r"(?mi)^- .*?\[author\]:\s*([^|\r\n]+?)(?:\s*\||$)", text)
    return match.group(1).strip() if match else None


def load_ranks(board_dir):
    path = Path(board_dir) / "MILESTONE-usable-v1.md"
    ranks = {}
    done = set()
    if not path.exists():
        return ranks, done
    for line in path.read_text(encoding="utf-8").splitlines():
        cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
        if len(cells) < 2 or not cells[0].isdigit():
            continue
        match = re.match(r"(\d{3})(?:\s|$)", cells[1])
        if not match:
            continue
        number = match.group(1)
        rank = int(cells[0])
        if number in ranks or rank in ranks.values():
            raise RuntimeError(f"Duplicate card or Rank in milestone table: {number}, {rank}")
        ranks[number] = rank
        if "**Done" in cells[1]:
            done.add(number)
    return ranks, done


def load_cards(board_dir=BOARD_DIR, running=None):
    board = Path(board_dir)
    ranks, table_done = load_ranks(board)
    result = []
    for path in sorted(board.iterdir(), key=lambda p: p.name):
        match = CARD_FILE_RE.match(path.name)
        if not match or not path.is_file():
            continue
        number = match.group(1)
        text = path.read_text(encoding="utf-8")
        status_line = _line(text, "Status") or ""
        if number in table_done or status_line.lower().startswith("done"):
            status = "Done"
        elif status_line.upper().startswith(("SUPERSEDED", "WITHDRAWN")):
            status = "Superseded/Withdrawn"
        elif running is not None and number == str(running).zfill(3):
            status = "In Progress"
        else:
            status = "Ready"
        shape_line = _line(text, "Shape")
        shape = shape_line if shape_line in SHAPES else "Story"
        values = {"Card number": int(number), "Status": status, "Shape": shape}
        enables = _line(text, "Enables")
        if enables is not None:
            values["Enables"] = ", ".join(re.findall(r"(?<!\d)\d{3}(?!\d)", enables))
        if number in ranks:
            values["Rank"] = ranks[number]
        depends = _line(text, "Depends on")
        if depends is not None:
            values["Depends on"] = depends
        type_line = _line(text, "Type")
        kind = type_line.split()[0].lower().rstrip("(:,;") if type_line else None
        if kind in KINDS:
            values["Kind"] = kind
        phase = _phase(text)
        if phase is not None:
            values["Phase"] = phase
        author = _author(text)
        if author is not None:
            values["Card author"] = author
        result.append(CardValues(number, path, values))
    return result


def read_board(gh, owner=OWNER, repo=REPO):
    validate_target(owner, repo)
    slug = f"{owner}/{repo}"
    project = parse_json(gh(["project", "view", str(PROJECT_NUMBER), "--owner", owner, "--format", "json"], check=True))
    fields_data = parse_json(gh(["project", "field-list", str(PROJECT_NUMBER), "--owner", owner, "--format", "json"], check=True))
    items_data = parse_json(gh(["project", "item-list", str(PROJECT_NUMBER), "--owner", owner, "--format", "json", "--limit", "1000"], check=True))
    issues = parse_json(gh(["issue", "list", "--repo", slug, "--state", "all", "--limit", "1000", "--json", "number,title,body,state,url"], check=True))
    fields = fields_data.get("fields", [])
    items = items_data.get("items", [])
    return project, fields, items, issues


def indexed_records(items, issues):
    marked = {}
    for issue in issues:
        markers = set(MARKER_RE.findall(issue.get("body") or ""))
        for number in markers:
            if number in marked:
                raise RuntimeError(f"More than one issue carries phrase-card:{number}")
            marked[number] = issue
    by_url = {}
    for item in items:
        url = (item.get("content") or {}).get("url")
        if url in by_url:
            raise RuntimeError(f"Issue appears more than once on project: {url}")
        if url:
            by_url[url] = item
    return marked, by_url


def _same(expected, actual):
    if isinstance(expected, (int, float)):
        try:
            return float(expected) == float(actual)
        except (TypeError, ValueError):
            return False
    return expected == actual


def plan(gh, board_dir=BOARD_DIR, owner=OWNER, repo=REPO, running=None):
    cards = load_cards(board_dir, running=running)
    project, fields, items, issues = read_board(gh, owner, repo)
    field_map = {field.get("name"): field for field in fields}
    missing_fields = [name for name in FIELD_NAMES if name not in field_map]
    if missing_fields:
        raise RuntimeError(f"Missing project fields: {missing_fields}")
    marked, by_url = indexed_records(items, issues)
    actions = []
    for card in cards:
        issue = marked.get(card.number)
        if issue is None:
            raise RuntimeError(f"No marked issue for card {card.number}")
        item = by_url.get(issue.get("url"))
        if item is None:
            actions.append(("add", card, issue, None, None))
        else:
            for name, value in card.values.items():
                if not _same(value, item.get(item_key(name))):
                    actions.append(("edit", card, issue, item, (field_map[name], value)))
        if card.values["Status"] == "Done" and str(issue.get("state", "")).upper() == "OPEN":
            actions.append(("close", card, issue, item, None))
    return project, field_map, actions


def apply_actions(gh, project_id, field_map, actions, owner=OWNER, repo=REPO):
    slug = f"{owner}/{repo}"
    newly_added = {}
    for action, card, issue, item, detail in actions:
        if action == "add":
            payload = gh(["project", "item-add", str(PROJECT_NUMBER), "--owner", owner, "--url", issue["url"], "--format", "json"], check=True)
            newly_added[card.number] = parse_json(payload)
            continue
        if action == "close":
            gh(["issue", "close", str(issue["number"]), "--repo", slug, "--reason", "completed"], check=True)
            continue
        field, value = detail
        command = ["project", "item-edit", "--id", item["id"], "--project-id", project_id, "--field-id", field["id"]]
        # the real gh reports number and text fields alike as ProjectV2Field, so numbers are known by name
        if field["name"] in NUMBER_FIELDS:
            command.extend(["--number", str(value)])
        elif str(field.get("type", "")).endswith("SingleSelectField") or field.get("type") == "SINGLE_SELECT":
            option = next((x for x in field.get("options", []) if x.get("name") == value), None)
            if option is None:
                raise RuntimeError(f"Missing option {value!r} for {field['name']}")
            command.extend(["--single-select-option-id", option["id"]])
        else:
            command.extend(["--text", str(value)])
        gh(command, check=True)

    # Newly added items have no custom fields; set all expected values now.
    for card_number, item in newly_added.items():
        card = next(entry[1] for entry in actions if entry[1].number == card_number)
        synthetic = [("edit", card, None, item, (field_map[name], value)) for name, value in card.values.items()]
        apply_actions(gh, project_id, field_map, synthetic, owner, repo)


def main(argv=None, gh_runner=gh_runner, board_dir=BOARD_DIR):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apply", action="store_true")
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--owner", default=OWNER)
    parser.add_argument("--repo", default=REPO)
    parser.add_argument("--running", help="three-digit card number currently running")
    args = parser.parse_args(argv)
    validate_target(args.owner, args.repo)
    dry_run = not args.apply or args.dry_run
    try:
        project, fields, actions = plan(gh_runner, board_dir, args.owner, args.repo, args.running)
        if not dry_run:
            apply_actions(gh_runner, project["id"], fields, actions, args.owner, args.repo)
    except Exception as exc:
        print(f"ERROR: {exc}")
        return 1
    if not actions:
        print("Nothing to do: every card item and field already matches.")
    else:
        verb = "Would apply" if dry_run else "Applied"
        counts = {name: sum(1 for action in actions if action[0] == name) for name in ("add", "edit", "close")}
        print(f"{verb}: {counts['add']} add, {counts['edit']} field update, {counts['close']} close.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except ValueError as exc:
        print(f"ERROR: {exc}")
        raise SystemExit(2)
