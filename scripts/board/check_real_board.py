#!/usr/bin/env python3
"""Read-only verification for the private Phrase Board project and its card issues."""

import argparse
import json
import re
import subprocess
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
]
EXPECTED_STATUS_OPTIONS = ["Ready", "In Progress", "Done", "Superseded/Withdrawn"]


def gh_runner(args):
    proc = subprocess.run(["gh", *[str(part) for part in args]], capture_output=True, text=True)
    if proc.returncode != 0:
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


def main(argv=None, gh_runner=gh_runner):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", action="store_true", help="verify the project exists and has the correct fields")
    parser.add_argument("--issues", action="store_true", help="verify exactly one marked issue exists per card file")
    args = parser.parse_args(argv)

    if not args.project and not args.issues:
        parser.print_help()
        return 1

    try:
        count = None
        if args.project:
            verify_project(gh_runner)
        if args.issues:
            count = verify_issues(gh_runner)
    except Exception as exc:
        print(f"FAIL: {exc}")
        return 1

    if args.project:
        print(f"OK: {PROJECT_NAME} exists privately under {OWNER} with the expected fields and status options.")
    if args.issues:
        print(f"OK: {count} marked issues, exactly one per card file.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
