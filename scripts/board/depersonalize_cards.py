#!/usr/bin/env python3
"""Plan or apply reviewed depersonalization rules to Phrase Board issue bodies."""

import argparse
import json
import re
import subprocess
import sys
import tempfile
from pathlib import Path


OWNER = "Programming-Made-EZ"
REPO = "phrase-lang"
SLUG = f"{OWNER}/{REPO}"
NAME = "Ste" + "ve"
N = re.escape(NAME)


def depersonalize(text):
    text = text.replace("\r\n", "\n")
    text = re.sub(
        rf"(?im)^(- \*\*Source:\*\*)\s*{N}(?:'s)?\s*,\s*(20\d\d-\d\d-\d\d)\s*:",
        r"\1 product direction, \2:",
        text,
    )
    text = re.sub(rf"(?i)\bneeds\s+{N}\b", "needs the product owner", text)
    text = re.sub(rf"(?i)\bNeeds-{N}\b", "Needs-product-owner", text)
    return text


def unknown_mentions(text):
    result = []
    for number, line in enumerate(text.splitlines(), 1):
        masked = re.sub(rf"\bAs\s+{N}\s*,", "As PERSONA,", line, flags=re.I)
        if re.search(rf"\b{N}\b", masked, re.I):
            result.append((number, line))
    return result


def gh(args, input_text=None):
    proc = subprocess.run(
        ["gh", *map(str, args)], input=input_text, capture_output=True, text=True, encoding="utf-8"
    )
    if proc.returncode:
        raise RuntimeError(proc.stderr.strip() or proc.stdout.strip() or "gh failed")
    return proc.stdout.strip()


def issues(runner=gh):
    return json.loads(runner(["issue", "list", "--repo", SLUG, "--state", "all", "--limit", "1000", "--json", "number,title,body"]))


def plan(issue_rows):
    changes, unknown = [], []
    for issue in issue_rows:
        body = issue.get("body") or ""
        rewritten = depersonalize(body)
        leftovers = unknown_mentions(rewritten)
        if leftovers:
            unknown.append((issue["number"], leftovers))
        if rewritten != body.replace("\r\n", "\n"):
            changes.append((issue, rewritten))
    return changes, unknown


def apply_changes(changes, runner=gh):
    for issue, body in changes:
        with tempfile.NamedTemporaryFile("w", suffix=".md", delete=False, encoding="utf-8", newline="\n") as handle:
            handle.write(body)
            path = handle.name
        try:
            runner(["issue", "edit", issue["number"], "--repo", SLUG, "--body-file", path])
        finally:
            Path(path).unlink(missing_ok=True)


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument("--check", action="store_true")
    mode.add_argument("--apply", action="store_true")
    args = parser.parse_args(argv)
    changes, unknown = plan(issues())
    for issue, _ in changes:
        print(("apply: " if args.apply else "would edit: ") + f"card {issue['number']:03}")
    for number, lines in unknown:
        for line_number, line in lines:
            print(f"unknown: card {number:03}, line {line_number}: {line}")
    if args.apply:
        apply_changes(changes)
    return 1 if args.check and (changes or unknown) else 0


if __name__ == "__main__":
    sys.exit(main())
