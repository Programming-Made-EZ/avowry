#!/usr/bin/env python3
"""Apply the value verdicts of a re-rank window: close issues as not planned, add the consumer value or story line, then re-rank.

Usage: apply_value_verdicts.py --verdicts VERDICTS.json --snapshot SNAPSHOT.json [--apply] [--log LOG.json]

validate() checks the verdict file against the snapshot of open issues and plan() turns it into a fixed list of actions. The default is a dry
run that prints one line per action and calls nothing. With --apply the closures and edits run first, each is read back, and the first failure
stops the run before the rank order is touched; the rank edit (rerank.py, apply) runs last. Nothing is deleted. The apply log records every action."""

import argparse
import json
import os
import re
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[2]))  # so the file also runs as a script

from scripts.board import migrate_cards, rerank as rerank_module, set_fields

OWNER = migrate_cards.OWNER
REPO = migrate_cards.REPO
SLUG = f"{OWNER}/{REPO}"
PERSONA = "Ste" + "ve"
ROLES = ["facilitator", "driver", "reviewer seat", "agile coach", "audit lens", "decider", "script", "ci", "board"]


class InvalidVerdicts(Exception):
    def __init__(self, errors):
        super().__init__("; ".join(errors))
        self.errors = errors


def _is_role(text):
    low = text.lower()
    return any(re.search(rf"\b{re.escape(role)}\b", low) for role in ROLES)


def _check_story_line(issue, line, errors):
    if "so that" not in line.lower():
        errors.append(f"issue {issue}: the story line has no 'so that' value")
    actor = re.match(r"\s*As\s+(?:the\s+|a\s+|an\s+)?([^,]+),", line)
    if actor and _is_role(actor.group(1)):
        errors.append(f"issue {issue}: the story actor '{actor.group(1)}' is a mob role, not a consumer")


def validate(verdicts, snapshot):
    errors = []
    seen = {}
    closed = {c["issue"] for c in verdicts.get("close", [])}
    entries = [("close", e) for e in verdicts.get("close", [])]
    for key in ("edit_contract", "edit_story", "edit_unshaped"):
        entries += [(key, e) for e in verdicts.get(key, [])]
    for key, entry in entries:
        n = entry["issue"]
        seen[n] = seen.get(n, 0) + 1
        if seen[n] == 2:
            errors.append(f"issue {n}: appears twice in the verdicts (closed twice or edited twice)")
        item = snapshot.get(str(n))
        if item is None:
            errors.append(f"issue {n}: not in the snapshot")
        elif str(item.get("state", "")).lower() != "open":
            errors.append(f"issue {n}: not open in the snapshot (state {item.get('state')})")
        if key == "close":
            if not str(entry.get("reason", "")).strip():
                errors.append(f"issue {n}: the closure has no reason")
            if entry.get("kind") == "duplicate":
                survivor = entry.get("survivor")
                if survivor in closed:
                    errors.append(f"issue {n}: the survivor {survivor} is itself closed in this file")
                elif str(survivor) not in snapshot and survivor not in verdicts.get("closed_ok_survivors", []):
                    errors.append(f"issue {n}: the survivor {survivor} is not on the board (not in the snapshot or the known closed survivors)")
        elif key == "edit_contract":
            consumer = str(entry.get("consumer", ""))
            value = str(entry.get("value", ""))
            if _is_role(consumer):
                errors.append(f"issue {n}: the consumer '{consumer}' is a mob role, not a consumer")
            elif consumer not in verdicts.get("valid_consumers", []):
                errors.append(f"issue {n}: the consumer '{consumer}' is not in the valid consumers")
            if not value.strip():
                errors.append(f"issue {n}: the value is empty")
            elif "so that" not in value.lower():
                errors.append(f"issue {n}: the value has no 'so that' text")
        else:
            _check_story_line(n, str(entry.get("story_line", "")), errors)
    order = verdicts.get("order", {})
    cards = order.get("cards", [])
    for n in cards:
        if n in closed:
            errors.append(f"issue {n}: closed in this file and also in the order")
        elif str(n) not in snapshot:
            errors.append(f"issue {n}: in the order but not in the snapshot")
    first = order.get("first_product_card")
    if first not in cards:
        errors.append(f"the first product card {first} is not in the order")
    else:
        bugs = [n for n in cards[: cards.index(first)] if snapshot.get(str(n), {}).get("type") == "bug"]
        if len(bugs) > order.get("max_bugs_above", 0):
            errors.append(f"the order puts {len(bugs)} bug cards above the first product card {first}; at most {order.get('max_bugs_above', 0)} allowed")
    return errors


def plan(verdicts, snapshot):
    errors = validate(verdicts, snapshot)
    if errors:
        raise InvalidVerdicts(errors)
    actions = []
    for c in sorted(verdicts.get("close", []), key=lambda x: x["issue"]):
        comment = f"Closed as not planned: {c['reason']}. This reverses with a reopen."
        actions.append({"op": "close", "issue": c["issue"], "reason": c["reason"], "comment": comment})
    edits = []
    for e in verdicts.get("edit_contract", []):
        who = PERSONA if e["consumer"] == "product owner" else e["consumer"]
        edits.append({"op": "edit", "issue": e["issue"], "kind": "contract", "add_line": f"- **Consumer value:** {who}: {e['value']}"})
    for key, kind in (("edit_story", "story"), ("edit_unshaped", "unshaped")):
        edits += [{"op": "edit", "issue": e["issue"], "kind": kind, "story_line": e["story_line"]} for e in verdicts.get(key, [])]
    actions += sorted(edits, key=lambda x: x["issue"])
    order = verdicts["order"]
    actions.append({"op": "rerank", "after": order["after"], "cards": list(order["cards"])})
    return actions


def describe(action):
    if action["op"] == "close":
        return f"close {action['issue']} as not planned: {action['reason']}"
    if action["op"] == "rerank":
        return f"rerank {','.join(str(n) for n in action['cards'])} after {action['after']}"
    return f"edit {action['issue']}: add {action.get('add_line') or action['story_line']}"


def new_body(body, action):
    lines = body.split("\n")
    if "add_line" in action:
        actor = [i for i, ln in enumerate(lines) if ln.startswith("- **Actor:**")]
        context = [i for i, ln in enumerate(lines) if ln.startswith("## Context")]
        at = actor[0] + 1 if actor else (context[0] if context else len(lines))
        lines.insert(at, action["add_line"])
    else:
        line = action["story_line"]
        story = [i for i, ln in enumerate(lines) if ln.startswith("- **Story:**")]
        if story:
            lines[story[0]] = f"- **Story:** {line}"
        else:
            meta = [i for i, ln in enumerate(lines) if ln.startswith(("- **Source:**", "- **Status:**"))]
            lines.insert(meta[-1] + 1 if meta else len(lines), f"- **Story:** {line}")
    return "\n".join(lines)


def _state(gh, n):
    return gh(["issue", "view", n, "--repo", SLUG, "--json", "state", "--jq", ".state"]).strip().upper()


def _body(gh, n):
    return gh(["issue", "view", n, "--repo", SLUG, "--json", "body", "--jq", ".body"])


def _do_close(gh, action):
    n = action["issue"]
    if _state(gh, n) == "CLOSED":
        return "skipped"
    gh(["issue", "close", n, "--repo", SLUG, "--reason", "not planned", "--comment", action["comment"]])
    if _state(gh, n) != "CLOSED":
        raise RuntimeError(f"issue {n} is not closed after the close call")
    return "done"


def _do_edit(gh, action):
    n = action["issue"]
    current = _body(gh, n)
    if (action.get("add_line") or action["story_line"]) in current:
        return "skipped"  # already applied: a re-run never adds the line twice
    text = new_body(current.rstrip("\n"), action) + "\n"
    folder = tempfile.mkdtemp()
    path = os.path.join(folder, "body.md")
    try:
        with open(path, "w", encoding="utf-8", newline="\n") as f:
            f.write(text)
        gh(["issue", "edit", n, "--repo", SLUG, "--body-file", path])
    finally:
        if os.path.exists(path):
            os.remove(path)
        os.rmdir(folder)
    wanted = action.get("add_line") or action["story_line"]
    if wanted not in _body(gh, n):
        raise RuntimeError(f"issue {n} does not show the edit when read back")
    return "done"


def apply(actions, gh, rerank, log_path):
    log = []
    result = {"ok": True}
    for action in actions:
        entry = {k: v for k, v in action.items() if k != "comment"}
        try:
            if action["op"] == "close":
                entry["status"] = _do_close(gh, {**action, "issue": str(action["issue"])})
            elif action["op"] == "edit":
                entry["status"] = _do_edit(gh, {**action, "issue": str(action["issue"])})
            else:
                rerank(action["after"], action["cards"])
                entry["status"] = "done"
        except Exception as exc:
            entry["status"] = "failed"
            entry["error"] = str(exc)
            result = {"ok": False, "issue": action.get("issue"), "error": str(exc)}
        log.append(entry)
        if not result["ok"]:
            break
    with open(log_path, "w", encoding="utf-8", newline="\n") as f:
        json.dump({"ok": result["ok"], "actions": log}, f, indent=1)
        f.write("\n")
    return result


def _real_rerank(after, cards):
    rerank_module.rerank(set_fields.gh_runner, after, cards, dry_run=False)


def main(argv=None, gh=None, rerank=None):
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--verdicts", required=True)
    ap.add_argument("--snapshot", required=True)
    ap.add_argument("--apply", action="store_true")
    ap.add_argument("--log", default=".mob/records/reranks/rerank-26-apply-log.json")
    args = ap.parse_args(argv)
    with open(args.verdicts, encoding="utf-8") as f:
        verdicts = json.load(f)
    with open(args.snapshot, encoding="utf-8") as f:
        snapshot = json.load(f)["issues"]
    try:
        actions = plan(verdicts, snapshot)
    except InvalidVerdicts as exc:
        for line in exc.errors:
            print(f"ERROR: {line}")
        return 2
    if not args.apply:
        for action in actions:
            print("would: " + describe(action))
        return 0
    result = apply(actions, gh or set_fields.gh_runner, rerank or _real_rerank, args.log)
    if not result["ok"]:
        print(f"ERROR: stopped at issue {result['issue']}: {result['error']}")
        return 1
    print(f"applied {len(actions)} actions; log {args.log}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
