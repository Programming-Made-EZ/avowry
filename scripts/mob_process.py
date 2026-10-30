#!/usr/bin/env python3
"""D134's independent, read-only mob accountability gate and MOB.md generator."""

import argparse
import datetime as dt
import hashlib
import json
import re
import subprocess
import sys
import time
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

REQUIRED_ROLES = ("ready", "slice", "red", "green", "ship", "signoff")
OPTIONAL_ROLES = ("refactor",)
# D134/D135: the frozen list of legacy cards. The pin lives in the checker so that extending LEGACY.json also needs a
# change to this file, which the PREVIOUS checker version (run by CI on every push) judges and rejects.
FROZEN_LEGACY_SHA256 = "2618ef6c69243938dfad116f9f99e772d8a48dd3256b264a1a098ec1d42788d1"
STATE_RE = re.compile(r"<!-- mob-state: (\{[^\n]*\}) -->")
CARD_RE = re.compile(r"^\((\d+)\)\s")
GH_LIMIT = 5000


def command(args, cwd=None):
    result = subprocess.run(args, cwd=cwd, capture_output=True, text=True, encoding="utf-8")
    if result.returncode:
        raise RuntimeError(result.stderr.strip() or f"command failed: {args[0]}")
    return result.stdout.strip()


def git_command(root, *args):
    return command(["git", *args], root)


def gh_json(args):
    last = None
    for attempt in range(3):
        try:
            return json.loads(command(["gh", *args]))
        except (RuntimeError, ValueError) as exc:
            last = exc
            if attempt < 2:
                time.sleep(0.1)
    raise RuntimeError(f"gh failed after 3 attempts: {last}")


def cards_hash(cards):
    value = ",".join(str(card) for card in sorted(cards))
    return hashlib.sha256(value.encode()).hexdigest()


def labels(issue):
    return {item["name"] if isinstance(item, dict) else item for item in issue.get("labels", [])}


def card_number(issue):
    match = CARD_RE.match(issue.get("title", ""))
    return int(match.group(1)) if match else None


def read_issues(runner=gh_json):
    issues = runner(["issue", "list", "--state", "closed", "--label", "card", "--limit", str(GH_LIMIT),
                     "--json", "number,title,labels,closedAt"])
    other = runner(["issue", "list", "--state", "closed", "--limit", str(GH_LIMIT),
                    "--json", "number,title,labels,closedAt"])
    if len(issues) == GH_LIMIT or len(other) == GH_LIMIT:
        raise RuntimeError(f"gh issue list returned the {GH_LIMIT} item limit; audit is incomplete")
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

    with ThreadPoolExecutor(max_workers=4) as pool:
        return list(pool.map(timeline, (by_number[number] for number in sorted(by_number))))


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


CONTEXT_RE = re.compile(r"^\W*ctx-(\d+)\W*$", re.IGNORECASE)


def seat_identity(seat):
    """D145 C4: a seat is its driver (first word, lower-cased) plus an optional integer context id (ctx-<digits>); more than one ctx id makes the context ambiguous (None, True)."""
    words = str(seat).split()
    contexts = [int(m.group(1)) for m in map(CONTEXT_RE.match, words[1:]) if m]
    return (words[0].lower() if words else "", contexts[0] if len(contexts) == 1 else None, len(contexts) > 1)


def seats_distinct(first, second):
    (driver_a, ctx_a, many_a), (driver_b, ctx_b, many_b) = seat_identity(first), seat_identity(second)
    if many_a or many_b:
        return False
    return driver_a != driver_b or (ctx_a is not None and ctx_b is not None and ctx_a != ctx_b)


def check_records(root, issues, git):
    errors, records = [], load_records(root)
    closed = {card_number(i): i for i in issues if "card" in labels(i) and "not-card" not in labels(i) and card_number(i)}
    legacy = set(json.loads((root / ".mob/records/LEGACY.json").read_text(encoding="utf-8"))["cards"])
    drivers = driver_names(root)
    decision_text = "\n".join(p.read_text(encoding="utf-8") for p in sorted((root / "docs/decisions").glob("*.md")))
    for card in sorted(set(closed) - legacy):
        if card not in records:
            errors.append(f"Card {card:03} has no trail record")
    commit_evidence = {}
    record_dates = {}
    record_commits = {}
    for card in sorted(records):
        path = root / f".mob/records/{card:03}.json"
        try:
            record_dates[card] = git_call(git, "log", "--diff-filter=A", "--follow", "--format=%cI", "--", str(path.relative_to(root))).splitlines()[-1]
            record_commits[card] = git_call(git, "log", "--diff-filter=A", "--follow", "--format=%H", "--", str(path.relative_to(root))).splitlines()[-1]
        except (RuntimeError, IndexError, subprocess.CalledProcessError):
            pass
    for card, record in sorted(records.items()):
        path = root / f".mob/records/{card:03}.json"
        required = {"schema", "card", "title", "process", "seats", "retro", "notes"}
        if record.get("_error") or set(record) != required or record.get("schema") != 1 or record.get("card") != card:
            errors.append(f"Card {card:03} record does not match schema 1 or its filename")
            continue
        seats = record.get("seats", [])
        if (not isinstance(seats, list) or any(not isinstance(s, dict) for s in seats)
                or any(not isinstance(s.get(key), str) or not s[key].strip()
                       for s in seats for key in ("role", "seat", "model", "evidence"))):
            errors.append(f"Card {card:03} has a seat with an empty seat, model or evidence field")
            continue
        roles = [seat.get("role") for seat in seats]
        required_roles = [role for role in roles if role in REQUIRED_ROLES]
        if (sorted(required_roles) != sorted(REQUIRED_ROLES) or len(roles) != len(set(roles))
                or any(role not in REQUIRED_ROLES + OPTIONAL_ROLES for role in roles)
                or any(set(s) != {"role", "seat", "model", "evidence"} for s in seats)):
            errors.append(f"Card {card:03} must have exactly the six required role seats")
            continue
        if any(not str(s[key]).strip() for s in seats for key in ("seat", "model", "evidence")):
            errors.append(f"Card {card:03} has a seat with an empty seat, model or evidence field")
            continue
        if any(str(s["seat"]).split()[0].lower() not in drivers for s in seats):
            errors.append(f"Card {card:03} has a seat not listed in DRIVERS.md")
        by_role = {s["role"]: s for s in seats}
        if not seats_distinct(by_role["signoff"]["seat"], by_role["green"]["seat"]):
            errors.append(f"Card {card:03} signoff seat must differ from green seat")
        evidence = [str(s["evidence"]) for s in seats]
        if len(set(evidence)) != len(evidence):
            errors.append(f"Card {card:03} seats require distinct evidence")
        for value in evidence:
            if re.fullmatch(r"D\d+", value, re.I):
                anchor = re.search(rf'<a id=["\']{re.escape(value.lower())}["\'].*?(?=<a id=|\Z)', decision_text, re.I | re.S)
                if not anchor:
                    errors.append(f"Card {card:03} evidence {value} has no decision anchor")
                elif not re.search(rf"\bCard 0*{card}\b", anchor.group(0)):
                    errors.append(f"Card {card:03} evidence {value} decision does not mention Card {card}")
            else:
                try:
                    git_call(git, "merge-base", "--is-ancestor", value, "HEAD")
                    message = git_call(git, "show", "-s", "--format=%B", value)
                    if not re.search(rf"\bCard 0*{card}\b", message):
                        raise RuntimeError("wrong message")
                    resolved = git_call(git, "rev-parse", value)
                    if resolved == record_commits.get(card):
                        errors.append(f"Card {card:03} evidence {value} is the record's own commit")
                    when = git_call(git, "show", "-s", "--format=%cI", value)
                    if card in closed and dt.datetime.fromisoformat(when.replace("Z", "+00:00")) > dt.datetime.fromisoformat(closed[card]["closedAt"].replace("Z", "+00:00")):
                        errors.append(f"Card {card:03} evidence {value} is dated after issue closure")
                    previous = max((n for n in record_dates if n < card), default=None)
                    if previous and dt.datetime.fromisoformat(when.replace("Z", "+00:00")) < dt.datetime.fromisoformat(record_dates[previous].replace("Z", "+00:00")):
                        errors.append(f"Card {card:03} evidence {value} predates Card {previous:03}'s record")
                    if resolved in commit_evidence and commit_evidence[resolved] != card:
                        errors.append(f"Card {card:03} evidence {value} is reused by Card {commit_evidence[resolved]:03}")
                    commit_evidence[resolved] = card
                except (RuntimeError, subprocess.CalledProcessError):
                    errors.append(f"Card {card:03} evidence {value} is not a reachable matching commit")
        if card in closed:
            try:
                added = record_dates[card]
                if dt.datetime.fromisoformat(added.replace("Z", "+00:00")) > dt.datetime.fromisoformat(closed[card]["closedAt"].replace("Z", "+00:00")):
                    errors.append(f"Card {card:03} record was first committed after issue closure")
            except (RuntimeError, IndexError, ValueError, subprocess.CalledProcessError):
                errors.append(f"Card {card:03} record has no first-commit date")
        retro = root / str(record["retro"])
        try:
            retro.resolve().relative_to((root / "docs/retros").resolve())
            under_retros = True
        except ValueError:
            under_retros = False
        if not retro.is_file() or not under_retros:
            errors.append(f"Card {card:03} record retro does not name an existing file under docs/retros")
        else:
            covered, retro_errors = retro_cards(root, [retro], max((item["number"] for item in issues), default=0) + 50)
            if retro_errors or card not in covered:
                errors.append(f"Card {card:03} record retro does not cover the card")
            text = retro.read_text(encoding="utf-8")
            headings = ("Individual", "Team", r"Organi[sz]ation", "Product")
            if any(not re.search(rf"(?mi)^#+\s+{heading}\s*$", text) for heading in headings) or "Process improvement pick" not in text:
                errors.append(f"Card {card:03} record retro is missing required sections or Process improvement pick")
    return errors


def retro_cards(root, paths=None, maximum=None):
    found, errors = set(), []
    for path in sorted(paths or (root / "docs/retros").glob("*.md")):
        for line in path.read_text(encoding="utf-8").splitlines():
            if not line.startswith("Cards:"):
                continue
            value = line[6:].strip()
            if not re.fullmatch(r"\d+(?:-\d+)?(?:\s*,\s*\d+(?:-\d+)?)*", value):
                errors.append(f"{path.as_posix()} has an invalid Cards line")
                continue
            for part in re.split(r"\s*,\s*", value):
                bounds = [int(x) for x in part.split("-")]
                if len(bounds) == 2 and bounds[0] > bounds[1]:
                    errors.append(f"{path.as_posix()} Cards range starts after it ends")
                    continue
                if bounds[-1] - bounds[0] > 250:
                    errors.append(f"{path.as_posix()} Cards range is wider than 250 numbers")
                    continue
                if maximum is not None and bounds[-1] > maximum:
                    errors.append(f"{path.as_posix()} Cards number {bounds[-1]} exceeds the audit maximum {maximum}")
                    continue
                found.update(range(bounds[0], bounds[-1] + 1))
    return found, errors


def check_retros(root, expected, maximum=None):
    found, errors = retro_cards(root, maximum=maximum)
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
    if digest != FROZEN_LEGACY_SHA256:
        errors.append("LEGACY.json differs from the frozen list pinned in the checker (D134/D135)")
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
        events = item.get("events", [])
        if any(event.get("event") == "unlabeled" and event.get("label", {}).get("name") == "card" for event in events):
            errors.append(f"Card {card:03} has an unlabeled event")
        if any(event.get("event") == "reopened" for event in events) and card not in legacy and not str(records.get(card, {}).get("notes", "")).strip():
            errors.append(f"Card {card:03} was reopened but its record notes are empty")
    return errors


WINDOW_CATEGORIES = ("task-force declaration", "slice decision", "loop-boundary re-rank", "card-text or declaration fix", "mid-card addition")


def seat_errors(path, index, reply):
    return []


def window_errors(path, data):
    errors = []
    if data.get("category") not in WINDOW_CATEGORIES:
        errors.append(f"{path.name}: category must be one of the closed list ({', '.join(WINDOW_CATEGORIES)})")
    ended = data.get("ended_at")
    if not isinstance(ended, str) or not ended.strip():
        errors.append(f"{path.name}: ended_at is missing or empty")
    replies = data.get("replies")
    for index, reply in enumerate(replies if isinstance(replies, list) else []):
        if isinstance(reply, dict):
            errors.extend(seat_errors(path, index, reply))
        else:
            errors.append(f"{path.name}: seat {index + 1} is not an object")
    return errors


def check_windows(root):
    errors = []
    for path in sorted((root / ".mob/records/decisions").glob("[0-9][0-9][0-9]-*.json")):
        errors.extend(window_errors(path, json.loads(path.read_text(encoding="utf-8"))))
    return errors


def check_workflows(root):
    errors = []
    ci = root / ".github/workflows/ci.yml"
    text = ci.read_text(encoding="utf-8") if ci.exists() else ""
    ci_block = job_block(text)
    if not valid_job(ci_block, previous=True):
        errors.append("ci.yml must contain the mob-process job running the checker")
    event = root / ".github/workflows/mob-process.yml"
    event_text = event.read_text(encoding="utf-8") if event.exists() else ""
    event_block = job_block(event_text)
    if (not event.exists()
            or not all(word in event_text for word in ("issues:", "closed", "reopened", "unlabeled", "schedule:"))
            or not valid_job(event_block, previous=False)
            or "mob_process_prev.py" in event_block):
        errors.append("mob-process.yml must run for issue events and on a schedule")
    owners = root / ".github/CODEOWNERS"
    owner_text = owners.read_text(encoding="utf-8") if owners.exists() else ""
    required = ("/scripts/mob_process.py", "/.github/workflows/", "/.mob/records/LEGACY.json",
                "/docs/decisions/process-and-staffing.md", "/.github/CODEOWNERS", "/.githooks/",
                "/tests/test_mob_process.py", "/docs/PROCESS-CHECKS.md", "/.mob/records/process.json",
                "/.mob/records/held.json")
    for path in required:
        if not re.search(rf"(?m)^{re.escape(path)}\s+\S+", owner_text):
            errors.append(f"CODEOWNERS must protect {path}")
    return errors


def job_block(text):
    match = re.search(r"(?ms)^  mob-process:\s*$.*?(?=^  [A-Za-z0-9_-]+:\s*$|\Z)", text)
    return match.group(0) if match else ""


def valid_job(block, previous=False):
    unsafe = re.search(r"(?mi)^\s*(?:if:\s*.*false|continue-on-error:)|\|\|\s*true", block)
    current = "python3 scripts/mob_process.py check" in block
    checker_steps = re.findall(r"(?ms)^\s*-\s+(.*?)(?=^\s*-\s+|\Z)", block)
    authenticated = all("GH_TOKEN:" in step for step in checker_steps if "mob_process.py" in step)
    old = all(x in block for x in ("github.event.before", "mob_process_prev.py", "check --root . --skip-regeneration"))
    push_main = all(x in block for x in ("github.event_name == 'push'", "refs/heads/main"))
    return bool(block and current and authenticated and not unsafe
                and (old and push_main if previous else True))


def check_issue_labels(issues):
    errors = []
    for item in issues:
        issue_labels = labels(item)
        if "card" not in issue_labels and "not-card" not in issue_labels:
            errors.append(f"Closed issue #{item['number']} has neither card nor not-card label")
        if "card" in issue_labels and card_number(item) is None:
            errors.append(f"Closed card issue #{item['number']} title has no (N) prefix")
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


def check_mob(root, issues, git, skip_regeneration=False):
    path = root / "MOB.md"
    if not path.exists():
        return ["MOB.md is missing"]
    current_bytes = path.read_bytes()
    current = current_bytes.decode("utf-8")
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
    expected_done = sorted(set(load_records(root)) | set(json.loads((root / ".mob/records/LEGACY.json").read_text(encoding="utf-8"))["cards"]))
    if state.get("done") != expected_done:
        errors.append("MOB.md done list does not equal record and legacy cards")
    if not skip_regeneration and current_bytes != render_mob(root, issues, git, state["as_of"]).encode("utf-8"):
        errors.append("MOB.md is not byte-identical to write-mob-md output")
    return errors


def run_check(root, issues, git=None, skip_regeneration=False):
    git = git or (lambda *args: git_command(root, *args))
    legacy = json.loads((root / ".mob/records/LEGACY.json").read_text(encoding="utf-8"))["cards"]
    closed_cards = {card_number(i) for i in issues if "card" in labels(i) and card_number(i)}
    errors = []
    maximum = max((item["number"] for item in issues), default=0) + 50
    for check in (lambda: check_records(root, issues, git), lambda: check_retros(root, closed_cards | set(legacy), maximum),
                  lambda: check_legacy(root, issues), lambda: check_mob(root, issues, git, skip_regeneration),
                  lambda: check_events(root, issues), lambda: check_workflows(root), lambda: check_issue_labels(issues), lambda: check_windows(root)):
        try:
            errors.extend(check())
        except (OSError, ValueError, RuntimeError, TypeError, AttributeError, KeyError) as exc:
            errors.append(f"check could not read its input: {exc!r}")
    if errors:
        for error in errors:
            print(f"::error::mob-process: {error}")
        return 1
    print(f"mob-process: OK ({len(closed_cards)} closed cards, {len(load_records(root))} records, {len(legacy)} legacy)")
    return 0


def main(argv=None, issue_reader=read_issues):
    parser = argparse.ArgumentParser()
    parser.add_argument("command", choices=("check", "write-mob-md"))
    parser.add_argument("--root", type=Path)
    parser.add_argument("--skip-regeneration", action="store_true",
                        help="skip only MOB.md's byte-for-byte regeneration comparison")
    args = parser.parse_args(argv)
    root = args.root.resolve() if args.root else Path(__file__).resolve().parents[1]
    try:
        issues = issue_reader()
        git = lambda *values: git_command(root, *values)
        if args.command == "check":
            return run_check(root, issues, git, args.skip_regeneration)
        (root / "MOB.md").write_text(render_mob(root, issues, git), encoding="utf-8", newline="\n")
        print("mob-process: wrote MOB.md")
        return 0
    except (RuntimeError, ValueError, OSError) as exc:
        print(f"::error::mob-process: {exc}")
        return 1


if __name__ == "__main__":
    sys.exit(main())
