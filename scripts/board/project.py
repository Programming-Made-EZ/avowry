#!/usr/bin/env python3
"""Create or reuse the private Phrase Board project and its fields."""

import argparse
import json
import subprocess
from typing import Sequence

OWNER = "Programming-Made-EZ"
PROJECT_NAME = "Phrase Board"
EXPECTED_STATUS_OPTIONS = ["Ready", "In Progress", "Done", "Superseded/Withdrawn"]
FIELD_SPECS = [
    {"name": "Rank", "type": "NUMBER"},
    {"name": "Card number", "type": "NUMBER"},
    {"name": "Depends on", "type": "TEXT"},
    {"name": "Kind", "type": "SINGLE_SELECT", "options": ["feature", "bug", "docs", "tooling", "spike", "decision", "data"]},
    {"name": "Phase", "type": "TEXT"},
    {"name": "Card author", "type": "TEXT"},
    {"name": "Shape", "type": "SINGLE_SELECT", "options": ["Story", "Contract"]},
    {"name": "Enables", "type": "TEXT"},
    {"name": "Status", "type": "SINGLE_SELECT", "options": EXPECTED_STATUS_OPTIONS},
]


def parse_json(text):
    if not text or not text.strip():
        return {}
    try:
        return json.loads(text)
    except json.JSONDecodeError:
        return {}


def gh_runner(args: Sequence[str], *, check: bool = True, input_text: str | None = None):
    proc = subprocess.run(["gh", *[str(part) for part in args]], input=input_text, capture_output=True, text=True)
    if check and proc.returncode != 0:
        if proc.stderr.strip():
            raise RuntimeError(proc.stderr.strip())
        raise RuntimeError(f"gh command failed: {' '.join(args)}")
    return proc.stdout.strip()


def ensure_owner(owner: str):
    if owner != OWNER:
        raise ValueError(f"Owner must be {OWNER}; refusing {owner!r}.")


def project_plan(owner: str = OWNER):
    lines = [
        f"Project: {PROJECT_NAME}",
        f"Owner: {owner}",
        "Visibility: private",
        "Fields:",
    ]
    for field in FIELD_SPECS:
        if field["type"] == "SINGLE_SELECT" and field.get("options"):
            option_text = ", ".join(field["options"])
            lines.append(f"  - {field['name']} ({field['type']} options: {option_text})")
        else:
            lines.append(f"  - {field['name']} ({field['type']})")
    status_text = ", ".join(EXPECTED_STATUS_OPTIONS)
    lines.append(f"Status options: {status_text}")
    return lines


def _projects_from_payload(payload):
    projects = parse_json(payload)
    if isinstance(projects, list):
        return projects
    if isinstance(projects, dict):
        nested = projects.get("projects")
        if isinstance(nested, list):
            return nested
        if any(projects.get(key) for key in ("name", "title", "projectTitle")):
            return [projects]
    return []


def find_project(gh, owner: str = OWNER):
    ensure_owner(owner)
    payload = gh(["project", "list", "--owner", owner, "--limit", "200", "--format", "json"], check=True)
    for project in _projects_from_payload(payload):
        if not isinstance(project, dict):
            continue
        name = str(project.get("name") or project.get("title") or project.get("projectTitle") or "").strip()
        if name == PROJECT_NAME:
            return project
    return None


def find_project_id(gh, owner: str = OWNER):
    project = find_project(gh, owner)
    if project is None:
        return None
    project_id = project.get("id") or project.get("node_id")
    if project_id:
        return str(project_id)
    return None


def create_project(gh, owner: str = OWNER):
    ensure_owner(owner)
    payload = gh(["project", "create", "--owner", owner, "--title", PROJECT_NAME, "--format", "json"], check=True)
    project_data = parse_json(payload)
    if not isinstance(project_data, dict):
        raise RuntimeError("Project creation returned no project data.")
    project_id = project_data.get("id") or project_data.get("node_id")
    if not project_id:
        raise RuntimeError("Project creation did not return an id.")
    return project_data


def ensure_private_visibility(gh, project, owner: str = OWNER):
    project_number = project.get("number")
    if project_number is None:
        return
    if project.get("public") is True or str(project.get("visibility") or "").upper() == "PUBLIC":
        gh(["project", "edit", str(project_number), "--owner", owner, "--visibility", "PRIVATE"], check=True)
        project["public"] = False
        project["visibility"] = "PRIVATE"


def fetch_project_fields(gh, project_number: int, owner: str = OWNER):
    payload = gh(["project", "field-list", str(project_number), "--owner", owner, "--format", "json"], check=True)
    data = parse_json(payload)
    fields = data.get("fields", []) if isinstance(data, dict) else data
    if not isinstance(fields, list):
        return []
    result = []
    for field in fields:
        if not isinstance(field, dict):
            continue
        options = []
        for option in field.get("options", []):
            if isinstance(option, dict):
                options.append({
                    "id": option.get("id"),
                    "name": option.get("name"),
                    "color": option.get("color"),
                    "description": option.get("description"),
                })
        result.append({
            "id": field.get("id"),
            "name": field.get("name"),
            "type": field.get("type"),
            "options": options,
        })
    return result


def create_field(gh, project_number: int, owner: str, spec: dict):
    command = ["project", "field-create", str(project_number), "--owner", owner, "--name", spec["name"], "--data-type", spec["type"]]
    if spec["type"] == "SINGLE_SELECT" and spec.get("options"):
        command.extend(["--single-select-options", ",".join(spec["options"])])
    gh(command, check=True)


def update_status_field(gh, project_id: str, field: dict):
    existing = {option.get("name"): option for option in field.get("options", []) if option.get("name")}
    status_options = []
    for name in EXPECTED_STATUS_OPTIONS:
        option = existing.get(name)
        if option:
            status_options.append({
                "id": option.get("id"),
                "name": name,
                "color": option.get("color") or "GRAY",
                "description": option.get("description") or "",
            })
        else:
            status_options.append({
                "name": name,
                "color": "GRAY",
                "description": "",
            })

    query = """
mutation UpdateProjectV2Field($fieldId: ID!, $singleSelectOptions: [ProjectV2SingleSelectFieldOptionInput!]!) {
  updateProjectV2Field(input: {fieldId: $fieldId, singleSelectOptions: $singleSelectOptions}) {
    projectV2Field {
      ... on ProjectV2SingleSelectField {
        name
      }
    }
  }
}
"""
    payload = json.dumps({
        "query": query,
        "variables": {
            "fieldId": field.get("id"),
            "singleSelectOptions": status_options,
        },
    })
    gh(["api", "graphql", "--input", "-"], check=True, input_text=payload)


def ensure_project(gh, owner: str = OWNER, dry_run: bool = False):
    ensure_owner(owner)
    project_record = find_project(gh, owner)
    if project_record is None:
        if dry_run:
            return {"project": None, "changes": ["create project"], "dry_run": True}
        project_record = create_project(gh, owner)

    project_id = project_record.get("id") or project_record.get("node_id")
    project_number = project_record.get("number")
    if project_id is None:
        raise RuntimeError("Project creation did not return an id.")

    if project_record.get("public") is True or str(project_record.get("visibility") or "").upper() == "PUBLIC":
        if dry_run:
            return {"project": project_record, "changes": ["make project private"], "dry_run": True}
        ensure_private_visibility(gh, project_record, owner=owner)

    if project_number is None:
        raise RuntimeError("Project list output did not include the project number required for field operations.")

    fields = fetch_project_fields(gh, project_number, owner=owner)
    existing_by_name = {field.get("name"): field for field in fields if field.get("name")}
    needed_changes = []

    for spec in FIELD_SPECS:
        name = spec["name"]
        if name == "Status":
            field = existing_by_name.get(name)
            if field is None:
                needed_changes.append(f"create field {name}")
                continue
            current_options = [option.get("name") for option in field.get("options", []) if option.get("name")]
            if current_options != EXPECTED_STATUS_OPTIONS:
                needed_changes.append(f"update field {name}")
            continue
        if name not in existing_by_name:
            needed_changes.append(f"create field {name}")

    if dry_run:
        return {"project": project_record, "changes": needed_changes, "dry_run": True}

    if project_record.get("public") is True or str(project_record.get("visibility") or "").upper() == "PUBLIC":
        ensure_private_visibility(gh, project_record, owner=owner)

    for spec in FIELD_SPECS:
        name = spec["name"]
        if name == "Status":
            field = existing_by_name.get(name)
            if field is None:
                create_field(gh, project_number, owner, spec)
                fields = fetch_project_fields(gh, project_number, owner=owner)
                existing_by_name = {field_info.get("name"): field_info for field_info in fields if field_info.get("name")}
                field = existing_by_name.get(name)
            if field is not None:
                current_options = [option.get("name") for option in field.get("options", []) if option.get("name")]
                if current_options != EXPECTED_STATUS_OPTIONS:
                    update_status_field(gh, project_id, field)
            continue
        if name not in {field_info.get("name") for field_info in fields if field_info.get("name")}:
            create_field(gh, project_number, owner, spec)

    return {"project": project_record, "changes": needed_changes, "dry_run": False}


def main(argv: Sequence[str] | None = None, gh_runner=gh_runner):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apply", action="store_true", help="create or update the project and its fields")
    parser.add_argument("--dry-run", action="store_true", help="preview the action without mutating anything")
    parser.add_argument("--owner", default=OWNER, help="GitHub owner; must be Programming-Made-EZ")
    args = parser.parse_args(argv)

    if args.owner != OWNER:
        raise ValueError(f"Owner must be {OWNER}; refusing {args.owner!r}.")

    dry_run = not args.apply or args.dry_run
    try:
        summary = ensure_project(gh_runner, owner=args.owner, dry_run=dry_run)
    except Exception as exc:  # pragma: no cover - CLI error path
        print(f"ERROR: {exc}")
        return 1

    if dry_run:
        if not summary["changes"]:
            print("Nothing to do: project and fields already match the expected private board layout.")
        else:
            print(f"Would update: {', '.join(summary['changes'])}")
    else:
        print("Applied: project and fields are now in the expected private board layout.")

    for line in project_plan(args.owner):
        print(line)
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except ValueError as exc:
        print(f"ERROR: {exc}")
        raise SystemExit(2)
