#!/usr/bin/env python3
"""D134's independent, read-only mob accountability gate and MOB.md generator."""

import argparse
import datetime as dt
import hashlib
import json
import re
import subprocess
import sys
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

REQUIRED_ROLES = ("ready", "slice", "red", "green", "ship", "signoff")
STATE_RE = re.compile(r"<!-- mob-state: (\{[^\n]*\}) -->")
CARD_RE = re.compile(r"^\((\d+)\)\s")


def command(args, cwd=None):
    result = subprocess.run(args, cwd=cwd, capture_output=True, text=True, encoding="utf-8")
    if result.returncode:
        raise RuntimeError(result.stderr.strip() or f"command failed: {args[0]}")
    return result.stdout.strip()


def git_command(root, *args):
    return command(["git", *args], root)


def gh_json(args):
    return json.loads(command(["gh", *args]))


def cards_hash(cards):
    value = ",".join(str(card) for card in sorted(cards))
    return hashlib.sha256(value.encode()).hexdigest()


def labels(issue):
    return {item["name"] if isinstance(item, dict) else item for item in issue.get("labels", [])}


def card_number(issue):
    match = CARD_RE.match(issue.get("title", ""))
    return int(match.group(1)) if match else None


def read_issues(runner=gh_json):
    issues = runner(["issue", "list", "--state", "closed", "--label", "card", "--limit", "1000",
                     "--json", "number,title,labels,closedAt"])
    other = runner(["issue", "list", "--state", "closed", "--limit", "1000",
                    "--json", "number,title,labels,closedAt"])
    by_number = {item["number"]: item for item in other}
    by_number.update({item["number"]: item for item in issues})
    repo = runner(["repo", "view", "--json", "nameWithOwner"])["nameWithOwner"]
    # Read every closed issue's timeline: a removed `card` label must not make the
    # very unlabeled event we are looking for disappear from the audit population.
    def timeline(item):
        pages = runner(["api", f"repos/{repo}/issues/{item['number']}/timeline", "--paginate", "--slurp",
                        "-H", "Accept: application/vnd.github+json"])
        # --slurp returns a list of pages; a fake runner may return the flat event list directly.
        item["events"] = [event for page in pages for event in (page if isinstance(page, list) else [page])]
        return item

    with ThreadPoolExecutor(max_workers=8) as pool:
        return list(pool.map(timeline, by_number.values()))


def load_records(root):
    records = {}
    for path in sorted((root / ".mob/records").glob("[0-9][0-9][0-9].json")):
        try:
            records[int(path.stem)] = json.loads(path.read_text(encoding="utf-8"))
        except (ValueError, OSError) as exc:
            records[int(path.stem)] = {"_error": str(exc)}
    return records


def driver_names(root):
    names = set()
    for line in (root / "DRIVERS.md").read_text(encoding="utf-8").splitlines():
        if line.startswith("|"):
            first = line.split("|")[1].strip().split()
            if first and first[0].lower() not in {"driver", "---"}:
                names.add(first[0].lower())
    return names


def git_call(git, *args):
    return git(*args)


def check_records(root, issues, git):
    errors, records = [], load_records(root)
    closed = {card_number(i): i for i in issues if "card" in labels(i) and "not-card" not in labels(i) and card_number(i)}
    legacy = set(json.loads((root / ".mob/records/LEGACY.json").read_text(encoding="utf-8"))["cards"])
    drivers = driver_names(root)
    decision_text = "\n".join(p.read_text(encoding="utf-8") for p in (root / "docs/decisions").glob("*.md"))
    for card in sorted(set(closed) - legacy):
        if card not in records:
            errors.append(f"Card {card:03} has no trail record")
    for card, record in sorted(records.items()):
        path = root / f".mob/records/{card:03}.json"
        required = {"schema", "card", "title", "process", "seats", "retro", "notes"}
        if record.get("_error") or set(record) != required or record.get("schema") != 1 or record.get("card") != card:
            errors.append(f"Card {card:03} record does not match schema 1 or its filename")
            continue
        seats = record.get("seats", [])
        roles = [seat.get("role") for seat in seats]
        if sorted(roles) != sorted(REQUIRED_ROLES) or any(set(s) != {"role", "seat", "model", "evidence"} for s in seats):
            errors.append(f"Card {card:03} must have exactly the six required role seats")
            continue
        if any(str(s["seat"]).split()[0].lower() not in drivers for s in seats):
            errors.append(f"Card {card:03} has a seat not listed in DRIVERS.md")
        by_role = {s["role"]: s for s in seats}
        if by_role["signoff"]["seat"].split()[0].lower() == by_role["green"]["seat"].split()[0].lower():
            errors.append(f"Card {card:03} signoff seat must differ from green seat")
        evidence = [str(s["evidence"]) for s in seats]
        if len(set(evidence)) != len(evidence):
            errors.append(f"Card {card:03} seats require distinct evidence")
        for value in evidence:
            if re.fullmatch(r"D\d+", value, re.I):
                if not re.search(rf'<a id=["\']{re.escape(value.lower())}["\']', decision_text, re.I):
                    errors.append(f"Card {card:03} evidence {value} has no decision anchor")
            else:
                try:
                    git_call(git, "merge-base", "--is-ancestor", value, "HEAD")
                    message = git_call(git, "show", "-s", "--format=%B", value)
                    if f"Card {card}" not in message and f"Card {card:03}" not in message:
                        raise RuntimeError("wrong message")
                except (RuntimeError, subprocess.CalledProcessError):
                    errors.append(f"Card {card:03} evidence {value} is not a reachable matching commit")
        if card in closed:
            try:
                added = git_call(git, "log", "--diff-filter=A", "--follow", "--format=%cI", "--", str(path.relative_to(root))).splitlines()[-1]
                if dt.datetime.fromisoformat(added.replace("Z", "+00:00")) > dt.datetime.fromisoformat(closed[card]["closedAt"].replace("Z", "+00:00")):
                    errors.append(f"Card {card:03} record was first committed after issue closure")
            except (RuntimeError, IndexError, ValueError, subprocess.CalledProcessError):
                errors.append(f"Card {card:03} record has no first-commit date")
    return errors


def retro_cards(root):
    found, errors = set(), []
    for path in (root / "docs/retros").glob("*.md"):
        for line in path.read_text(encoding="utf-8").splitlines():
            if not line.startswith("Cards:"):
                continue
            value = line[6:].strip()
            if not re.fullmatch(r"\d+(?:-\d+)?(?:\s*,\s*\d+(?:-\d+)?)*", value):
                errors.append(f"{path.as_posix()} has an invalid Cards line")
                continue
            for part in re.split(r"\s*,\s*", value):
                bounds = [int(x) for x in part.split("-")]
                found.update(range(bounds[0], bounds[-1] + 1))
    return found, errors


def check_retros(root, expected):
    found, errors = retro_cards(root)
    for card in sorted(expected - found):
        errors.append(f"Card {card:03} has no retro Cards entry")
    return errors


def check_legacy(root, issues):
    path = root / ".mob/records/LEGACY.json"
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, ValueError) as exc:
        return [f"LEGACY.json is unreadable: {exc}"]
    errors = []
    digest = cards_hash(data.get("cards", []))
    if data.get("schema") != 1 or data.get("decision") != "D134" or data.get("cards_sha256") != digest:
        errors.append("LEGACY.json schema, decision, or cards_sha256 is not frozen")
    decision = (root / "docs/decisions/process-and-staffing.md").read_text(encoding="utf-8")
    match = re.search(r"sha256 of the list:\s*([0-9a-f]{64})", decision, re.I)
    if not match or match.group(1).lower() != digest:
        errors.append("LEGACY.json card hash does not equal the D134 pinned hash")
    cutoff = dt.datetime.fromisoformat(data.get("cutoff", "").replace("Z", "+00:00"))
    legacy = set(data.get("cards", []))
    for item in issues:
        if card_number(item) in legacy and dt.datetime.fromisoformat(item["closedAt"].replace("Z", "+00:00")) > cutoff:
            errors.append(f"Legacy Card {card_number(item):03} closed after the cutoff")
    return errors


def check_events(root, issues):
    records, errors = load_records(root), []
    legacy = set(json.loads((root / ".mob/records/LEGACY.json").read_text(encoding="utf-8"))["cards"])
    for item in issues:
        card = card_number(item)
        was_card = "card" in labels(item) or any(
            event.get("label", {}).get("name") == "card" for event in item.get("events", [])
        )
        if not card or not was_card:
            continue
        events = {event.get("event") for event in item.get("events", [])}
        if "unlabeled" in events:
            errors.append(f"Card {card:03} has an unlabeled event")
        if "reopened" in events and card not in legacy and not str(records.get(card, {}).get("notes", "")).strip():
            errors.append(f"Card {card:03} was reopened but its record notes are empty")
    return errors


def check_workflows(root):
    errors = []
    ci = root / ".github/workflows/ci.yml"
    text = ci.read_text(encoding="utf-8") if ci.exists() else ""
    if not re.search(r"(?m)^\s{2}mob-process:\s*$", text) or "scripts/mob_process.py check" not in text:
        errors.append("ci.yml must contain the mob-process job running the checker")
    event = root / ".github/workflows/mob-process.yml"
    event_text = event.read_text(encoding="utf-8") if event.exists() else ""
    if not event.exists() or not all(word in event_text for word in ("issues:", "closed", "reopened", "unlabeled", "schedule:")):
        errors.append("mob-process.yml must run for issue events and on a schedule")
    return errors


def check_issue_labels(issues):
    errors = []
    for item in issues:
        issue_labels = labels(item)
        if "card" not in issue_labels and "not-card" not in issue_labels:
            errors.append(f"Closed issue #{item['number']} has neither card nor not-card label")
    return errors


def render_mob(root, issues, git, as_of=None):
    records = load_records(root)
    legacy = json.loads((root / ".mob/records/LEGACY.json").read_text(encoding="utf-8"))["cards"]
    as_of = as_of or git_call(git, "rev-parse", "--short", "HEAD")
    generated = git_call(git, "show", "-s", "--format=%cI", as_of)
    ratchet = json.loads((root / "board/usable-v1-ratchet.json").read_text(encoding="utf-8")) if (root / "board/usable-v1-ratchet.json").exists() else {"must_pass": [], "suspended": {}}
    suspended = ", ".join(sorted(ratchet.get("suspended", {}))) or "none"
    held = json.loads((root / ".mob/records/held.json").read_text(encoding="utf-8")) if (root / ".mob/records/held.json").exists() else []
    process = json.loads((root / ".mob/records/process.json").read_text(encoding="utf-8")) if (root / ".mob/records/process.json").exists() else {"state": "self-attested", "note": "D134 owner actions pending"}
    closed = {card_number(i): i for i in issues if card_number(i)}
    recent = sorted(records, key=lambda c: closed.get(c, {}).get("closedAt", ""), reverse=True)[:10]
    retros = sorted((root / "docs/retros").glob("*.md"))
    latest_retro = retros[-1].relative_to(root).as_posix() if retros else "docs/retros/"
    state = {"as_of": as_of, "generated_at": generated, "done": sorted(set(records) | set(legacy)),
             "records": len(records), "legacy": len(legacy), "retros": len(retros)}
    lines = [f"Usable v1: {len(ratchet.get('must_pass', []))} bars ratcheted, suspended: [{suspended}]",
             f"<!-- mob-state: {json.dumps(state, sort_keys=True, separators=(',', ':'))} -->", "", "# Mob status", "",
             "## Held cards", ""]
    lines.extend([f"- {x['card']}: {x['reason']}" for x in held] or ["- None"])
    lines += ["", "## Process state", "", f"- {process.get('state', 'unknown')}: {process.get('note', '')}", "",
              "## Recently Done", ""]
    for card in recent:
        seats = ", ".join(f"{s['role']}={s['seat']} ({s['model']})" for s in records[card].get("seats", []))
        retro = records[card].get("retro", "")
        lines.append(f"- Card {card:03}: {seats}; [retro]({retro})")
    if not recent:
        lines.append("- None")
    lines += ["", "## Counts", "", f"- Records: {len(records)}", f"- Legacy: {len(legacy)}", f"- Retros: {len(retros)}", "",
              "## Links", "", "- [Drivers](DRIVERS.md)", f"- [Latest retro]({latest_retro})", ""]
    return "\n".join(lines)


def check_mob(root, issues, git):
    path = root / "MOB.md"
    if not path.exists():
        return ["MOB.md is missing"]
    current = path.read_text(encoding="utf-8")
    matches = STATE_RE.findall(current)
    if len(matches) != 1:
        return ["MOB.md must contain exactly one mob-state comment"]
    if re.search(r"\b[0-9a-fA-F]{40}\b", current):
        return ["MOB.md must not contain a 40-character commit hash"]
    try:
        state = json.loads(matches[0])
        git_call(git, "merge-base", "--is-ancestor", state["as_of"], "HEAD")
        count = int(git_call(git, "rev-list", "--count", f"{state['as_of']}..HEAD", "--", ".", ":(exclude)MOB.md"))
    except (KeyError, ValueError, RuntimeError, subprocess.CalledProcessError):
        return ["MOB.md as_of is not an ancestor of HEAD"]
    errors = []
    if count > 3:
        errors.append(f"MOB.md is {count} non-MOB commits stale (maximum 3)")
    expected_done = sorted(set(load_records(root)) | set(json.loads((root / ".mob/records/LEGACY.json").read_text())["cards"]))
    if state.get("done") != expected_done:
        errors.append("MOB.md done list does not equal record and legacy cards")
    if current != render_mob(root, issues, git, state["as_of"]):
        errors.append("MOB.md is not byte-identical to write-mob-md output")
    return errors


def run_check(root, issues, git=None):
    git = git or (lambda *args: git_command(root, *args))
    legacy = json.loads((root / ".mob/records/LEGACY.json").read_text(encoding="utf-8"))["cards"]
    closed_cards = {card_number(i) for i in issues if "card" in labels(i) and card_number(i)}
    errors = []
    for check in (lambda: check_records(root, issues, git), lambda: check_retros(root, closed_cards | set(legacy)),
                  lambda: check_legacy(root, issues), lambda: check_mob(root, issues, git),
                  lambda: check_events(root, issues), lambda: check_workflows(root), lambda: check_issue_labels(issues)):
        try:
            errors.extend(check())
        except (OSError, ValueError, RuntimeError) as exc:
            errors.append(f"check could not read its input: {exc}")
    if errors:
        for error in errors:
            print(f"::error::mob-process: {error}")
        return 1
    print(f"mob-process: OK ({len(closed_cards)} closed cards, {len(load_records(root))} records, {len(legacy)} legacy)")
    return 0


def main(argv=None, issue_reader=read_issues):
    parser = argparse.ArgumentParser()
    parser.add_argument("command", choices=("check", "write-mob-md"))
    args = parser.parse_args(argv)
    root = Path(__file__).resolve().parents[1]
    try:
        issues = issue_reader()
        git = lambda *values: git_command(root, *values)
        if args.command == "check":
            return run_check(root, issues, git)
        (root / "MOB.md").write_text(render_mob(root, issues, git), encoding="utf-8", newline="\n")
        print("mob-process: wrote MOB.md")
        return 0
    except (RuntimeError, ValueError, OSError) as exc:
        print(f"::error::mob-process: {exc}")
        return 1


if __name__ == "__main__":
    sys.exit(main())
