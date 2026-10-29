#!/usr/bin/env python3
"""Edit one card on the Phrase Board after the cutover: its issue body, its project Status and Rank.

Usage:
  edit_card.py NNN --status Done --status-line "Done 2026-10-26 (what shipped)"   (default dry run; --apply mutates)
  edit_card.py NNN --rank 7

The issue is the card text (board/NNN-*.md is a generated archive). Only the issue body, the project Status and Rank of ONE marked
card change; nothing is ever deleted; the owner and repo are hard-wired. A Done card's issue is closed as completed.
Afterwards run scripts/board/generate_archive.py --apply to refresh the archive."""

import argparse
import json
import re
import subprocess
import sys
import tempfile
from pathlib import Path

OWNER = "Programming-Made-EZ"
REPO = "avowry"
SLUG = f"{OWNER}/{REPO}"
STATUSES = ("Ready", "In Progress", "Done", "Superseded/Withdrawn")


def gh(args, input_text=None):
    proc = subprocess.run(["gh", *map(str, args)], input=input_text, capture_output=True, text=True, encoding="utf-8")
    if proc.returncode:
        raise RuntimeError(proc.stderr.strip() or proc.stdout.strip() or "gh failed")
    return proc.stdout.strip()


def find_issue(number):
    issues = json.loads(gh(["issue", "list", "--repo", SLUG, "--state", "all", "--limit", "1000", "--json", "number,title,body,state"]))
    marker = f"<!-- phrase-card:{number} -->"
    for issue in issues:
        if marker in (issue.get("body") or ""):
            return issue
    raise RuntimeError(f"no marked issue for card {number}")


def replace_status_line(body, text):
    new, count = re.subn(r"(?m)^- \*\*Status:\*\*[^\n]*", "- **Status:** " + text, body.replace("\r\n", "\n"), count=1)
    if not count:
        raise RuntimeError("the card body has no Status line")
    return new


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("number")
    ap.add_argument("--status", choices=STATUSES)
    ap.add_argument("--status-line", help="new text of the card's Status line (the card body)")
    ap.add_argument("--rank", type=int)
    ap.add_argument("--apply", action="store_true")
    args = ap.parse_args(argv)
    if not re.fullmatch(r"\d{3}", args.number):
        print("card number must be three digits")
        return 2
    issue = find_issue(args.number)
    plan = []
    if args.status_line:
        plan.append("edit issue body Status line")
    if args.status:
        plan.append(f"set project Status {args.status}")
        if args.status == "Done" and issue["state"].upper() == "OPEN":
            plan.append("close issue as completed")
    if args.rank is not None:
        plan.append(f"set project Rank {args.rank}")
    for line in plan:
        print(("apply: " if args.apply else "would: ") + line)
    if not args.apply or not plan:
        return 0
    if args.status_line:
        body = replace_status_line(issue["body"], args.status_line)
        with tempfile.NamedTemporaryFile("w", suffix=".md", delete=False, encoding="utf-8", newline="\n") as handle:
            handle.write(body)
            path = handle.name
        try:
            gh(["issue", "edit", issue["number"], "--repo", SLUG, "--body-file", path])
        finally:
            Path(path).unlink(missing_ok=True)
    if args.status or args.rank is not None:
        project = json.loads(gh(["project", "view", "1", "--owner", OWNER, "--format", "json"]))
        fields = {f["name"]: f for f in json.loads(gh(["project", "field-list", "1", "--owner", OWNER, "--format", "json"]))["fields"]}
        items = json.loads(gh(["project", "item-list", "1", "--owner", OWNER, "--format", "json", "--limit", "1000"]))["items"]
        item = next(x for x in items if x.get("content", {}).get("number") == issue["number"])
        if args.status:
            option = next(o for o in fields["Status"]["options"] if o["name"] == args.status)
            gh(["project", "item-edit", "--id", item["id"], "--project-id", project["id"], "--field-id", fields["Status"]["id"], "--single-select-option-id", option["id"]])
            if args.status == "Done" and issue["state"].upper() == "OPEN":
                gh(["issue", "close", issue["number"], "--repo", SLUG, "--reason", "completed"])
        if args.rank is not None:
            gh(["project", "item-edit", "--id", item["id"], "--project-id", project["id"], "--field-id", fields["Rank"]["id"], "--number", args.rank])
    return 0


if __name__ == "__main__":
    sys.exit(main())
