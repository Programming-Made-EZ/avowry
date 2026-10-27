#!/usr/bin/env python3
"""Read-only verification for the private Phrase Board project."""

import argparse
import json
import subprocess

OWNER = "Programming-Made-EZ"
PROJECT_NAME = "Phrase Board"
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


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", action="store_true", help="verify the project exists and has the correct fields")
    args = parser.parse_args(argv)

    if not args.project:
        parser.print_help()
        return 1

    try:
        verify_project(gh_runner)
    except Exception as exc:
        print(f"FAIL: {exc}")
        return 1
    print(f"OK: {PROJECT_NAME} exists privately under {OWNER} with the expected fields and status options.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
