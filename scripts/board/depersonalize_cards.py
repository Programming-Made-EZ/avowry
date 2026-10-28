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

# Reviewed phrase rewrites for older cards: direction becomes a product principle, quotes become paraphrase.
PHRASES = [
    (f"{NAME}'s purpose-level ruling relayed by Dispatch,", "Product direction (purpose level),"),
    (f"{NAME}'s direction rulings relayed by Dispatch,", "Product direction,"),
    (f"{NAME}'s direction ruling relayed by Dispatch,", "Product direction,"),
    (f"{NAME}'s direction relayed by Dispatch, 2026-10-27 (let's get to a real prototype first)", "Product direction, 2026-10-27 (reach a real prototype first)"),
    (f"{NAME}'s answers relayed by Dispatch, 2026-10-27 (use a publicly available repository instead of his private projects", "Product direction, 2026-10-27 (use a publicly available repository instead of private projects"),
    (f"{NAME}'s idea relayed by Dispatch, 2026-10-27 (test against different code base sizes; use his real projects as the medium and large codebases)", "Product direction, 2026-10-27 (test against different code base sizes; use real projects as the medium and large codebases)"),
    (f"{NAME}'s hypothesis relayed by Dispatch,", "Product hypothesis,"),
    (f"{NAME}'s standing rule relayed by Dispatch, 2026-10-28 (as part of the mob loop it should always test for models to be available again)", "Product direction, 2026-10-28 (the mob loop always re-tests whether unavailable models are available again)"),
    (f"{NAME}'s priority ruling, 2026-10-26", "Product priority, 2026-10-26"),
    (f"{NAME} and Dispatch, 2026-10-26 (proposal B)", "Product direction, 2026-10-26 (proposal B)"),
    (f"{NAME}, 2026-10-26 (proposal A)", "Product direction, 2026-10-26 (proposal A)"),
    (f"{NAME}, 2026-10-26, on card 003's story: \"this is a role, not a persona.\"", "Product direction, 2026-10-26, on card 003's story: a role is not a persona."),
    (f'{NAME}, 2026-10-26 (mob-defaults is his product): "it should be a loop."', "Product direction, 2026-10-26: the work should run as a loop."),
    (f'{NAME}: "I do have a concern the slices are too big."', "Concern raised: the slices are too big."),
    ("\"don't forget the team can find their own features to add. so theoretically the loop is continuous and never ends.\"", "the team finds its own features to add, so the loop is continuous and never ends."),
    ('"this seems like a good starter app for the phrase lang"', "a Tasks app is a good starter app for the language"),
    (f"{NAME}, 2026-10-26 (VISION ruling 5): \"I feel like we aren't adhering to consumer related output. 'succeeds' isn't really a result the customer gets value from.\"", "Product direction, 2026-10-26 (VISION ruling 5): output should be something the customer gets value from; a bare 'succeeds' is not such a result."),
    (f"{NAME}'s direction ruling 4", "Product direction ruling 4"),
    (f"{NAME}'s direction ruling 1", "Product direction ruling 1"),
    (f"{NAME}'s purpose ruling", "Product purpose ruling"),
    (f"{NAME}'s earlier confirmed design", "Earlier confirmed design"),
    (f"{NAME}'s rulings", "Product rulings"),
    
    ("on his phone", "on their phone"),
    ("his choice", "the product " + "owner's choice"),
    ("(mob-defaults is his product)", "(mob-defaults is the product " + "owner's product)"),
]


def depersonalize(text):
    text = text.replace("\r\n", "\n")
    text = re.sub(
        rf"(?im)^(- \*\*Source:\*\*)\s*{N}(?:'s)?\s*,\s*(20\d\d-\d\d-\d\d)\s*:",
        r"\1 product direction, \2:",
        text,
    )
    text = re.sub(rf"(?i)\bneeds\s+{N}\b", "needs the product owner", text)
    text = re.sub(rf"(?i)\bNeeds-{N}\b", "Needs-product-owner", text)
    for old, new in PHRASES:
        text = text.replace(old, new)
    return text


def generic_names(text):
    """Last-resort rewrite of the remaining mentions (the persona opener 'As NAME,' is kept)."""
    text = re.sub(rf"\b{N}'s\b", "the product owner's", text)
    text = re.sub(rf"(?<!As )\b{N}\b", "the product owner", text)
    text = re.sub(r"(?m)(^[-0-9. *]*|[.:] )the product owner", lambda m: m.group(1) + "The product owner", text)
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


def plan(issue_rows, generic=False):
    changes, unknown = [], []
    for issue in issue_rows:
        body = issue.get("body") or ""
        rewritten = depersonalize(body)
        if generic:
            rewritten = generic_names(rewritten)
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
    parser.add_argument("--generic", action="store_true", help="also rewrite every remaining mention (after review)")
    args = parser.parse_args(argv)
    changes, unknown = plan(issues(), args.generic)
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
