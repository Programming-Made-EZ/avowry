#!/usr/bin/env python3
"""Migrate board/NNN-*.md cards into one phrase-lang issue each.

The default is a dry run. --apply creates issues and closes Done, SUPERSEDED
and WITHDRAWN cards. A later apply closes an existing open issue for a finished
card instead of leaving it open. --sync-bodies compares each marked issue with
the card file and, with --apply, rewrites only that body. Owner and repo are
hard-wired; anything else is refused. The script never deletes, and it never
changes an issue that lacks the phrase-card marker.
"""

import argparse
import json
import os
import re
import subprocess
import tempfile
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Sequence

OWNER = "Programming-Made-EZ"
REPO = "phrase-lang"
CREATE_INTERVAL_SECONDS = 1
BOARD_DIR = Path(__file__).resolve().parents[2] / "board"
CARD_FILE_RE = re.compile(r"^(\d{3})-.+\.md$")
MARKER_RE = re.compile(r"<!-- phrase-card:(\d{3}) -->")
HEADING_RE = re.compile(r"^#\s*(.*)$")
STATUS_RE = re.compile(r"(?m)^- \*\*Status:\*\*\s*(.*)$")
TYPE_RE = re.compile(r"(?m)^- \*\*Type:\*\*\s*(.*)$")
REPLACEMENTS = (
    ("C:\\Users\\ringe\\projects\\phrase-lang", "<phrase-lang>"),
    ("C:/Users/ringe/projects/phrase-lang", "<phrase-lang>"),
    ("C:\\Users\\ringe\\mob-crosscli", "<mob-defaults>"),
    ("C:/Users/ringe/mob-crosscli", "<mob-defaults>"),
)
LABEL_SPECS = (
    ("card", "1D76DB", "Phrase board card"),
    ("tooling", "5319E7", "Tooling card"),
)


@dataclass(frozen=True)
class Card:
    number: str
    title: str
    body: str
    close_reason: str | None
    tooling: bool


@dataclass(frozen=True)
class Action:
    kind: str
    card: Card
    line: str
    issue_number: int | None = None
    issue_body: str | None = None


def gh_runner(args: Sequence[str], *, check: bool = True, input_text: str | None = None):
    proc = subprocess.run(
        ["gh", *[str(part) for part in args]],
        input=input_text,
        capture_output=True,
        text=True,
    )
    if check and proc.returncode != 0:
        detail = proc.stderr.strip() or proc.stdout.strip()
        if detail:
            raise RuntimeError(detail)
        raise RuntimeError(f"gh command failed: {' '.join(str(part) for part in args)}")
    return proc.stdout.strip()


def ensure_target(owner: str, repo: str):
    if owner != OWNER or repo != REPO:
        raise ValueError(f"Owner and repo must be {OWNER}/{REPO}; refusing {owner}/{repo}.")


def _field(text: str, pattern: re.Pattern) -> str:
    match = pattern.search(text)
    if not match:
        return ""
    return match.group(1).strip()


def close_reason_for(text: str) -> str | None:
    status = _field(text, STATUS_RE)
    if status.startswith("Done"):
        return "completed"
    if status.startswith("SUPERSEDED") or status.startswith("WITHDRAWN"):
        return "not planned"
    return None


def is_tooling(text: str) -> bool:
    return _field(text, TYPE_RE).startswith("tooling")


def replace_local_paths(text: str) -> str:
    for old, new in REPLACEMENTS:
        text = text.replace(old, new)
    return text


def normalise_newlines(text: str) -> str:
    """Fold CRLF into LF. GitHub stores issue bodies with CRLF; card files may use LF."""
    return (text or "").replace("\r\n", "\n")


def load_card(path: Path) -> Card:
    text = path.read_bytes().decode("utf-8")
    number = CARD_FILE_RE.match(path.name).group(1)
    first, _separator, rest = text.partition("\n")
    newline = "\r\n" if first.endswith("\r") else "\n"
    heading_line = first[:-1] if first.endswith("\r") else first
    heading = HEADING_RE.match(heading_line)
    title = heading.group(1).strip() if heading else heading_line.strip()
    body = f"<!-- phrase-card:{number} -->" + newline + replace_local_paths(rest)
    return Card(number, title, body, close_reason_for(text), is_tooling(text))


def load_cards(board: Path) -> list[Card]:
    cards = []
    for path in sorted(Path(board).iterdir(), key=lambda item: item.name):
        if path.is_file() and CARD_FILE_RE.match(path.name):
            cards.append(load_card(path))
    return cards


def _rate_limited(exc: Exception) -> bool:
    text = str(exc).lower()
    return "rate limit" in text or "429" in text


# Values that follow these flags are card text. They are never scanned for "delete".
USER_TEXT_FLAGS = {"--title", "--body"}


def assert_allowed(args):
    """Refuse a gh delete subcommand or a bare delete argument. Titles are not inspected."""
    command = [str(part) for part in args]
    skip_value = False
    for part in command:
        if skip_value:
            skip_value = False
            continue
        if part in USER_TEXT_FLAGS:
            skip_value = True
            continue
        if part.lower() == "delete":
            raise RuntimeError("Refusing a gh command that includes delete.")


def assert_marker(body: str, card: Card, number: int | None = None):
    marker = f"<!-- phrase-card:{card.number} -->"
    if marker not in (body or ""):
        target = f"issue {number}" if number is not None else "an issue"
        raise RuntimeError(f"Refusing to change {target} without {marker}.")


def invoke(gh, args, sleep):
    command = [str(part) for part in args]
    assert_allowed(command)
    try:
        return gh(command, check=True)
    except Exception as exc:
        if not _rate_limited(exc):
            raise
        sleep(CREATE_INTERVAL_SECONDS)
        return gh(command, check=True)


def fetch_issues(gh, slug: str, sleep):
    payload = invoke(
        gh,
        ["issue", "list", "--repo", slug, "--state", "all", "--limit", "1000", "--json", "number,title,body,state"],
        sleep,
    )
    if payload is None or not str(payload).strip():
        return []
    data = json.loads(payload)
    if not isinstance(data, list):
        raise RuntimeError("Issue list did not return a JSON array.")
    return data


def index_markers(issues) -> dict:
    found = {}
    for issue in issues:
        if not isinstance(issue, dict):
            continue
        body = issue.get("body") or ""
        for number in set(MARKER_RE.findall(body)):
            found.setdefault(number, issue)
    return found


def issue_state(issue) -> str:
    if not isinstance(issue, dict):
        return ""
    return str(issue.get("state") or "").strip().upper()


def plan_actions(cards, existing) -> list[Action]:
    actions = []
    for card in cards:
        issue = existing.get(card.number)
        if issue is None:
            kind = "close" if card.close_reason else "create"
            actions.append(Action(kind, card, f"{kind} ({card.number}) {card.title}"))
            continue
        if card.close_reason and issue_state(issue) == "OPEN":
            actions.append(Action(
                "close-existing",
                card,
                f"close-existing ({card.number}) {card.title}",
                issue_number=int(issue["number"]),
                issue_body=issue.get("body") or "",
            ))
            continue
        actions.append(Action("skip", card, f"skip ({card.number}) {card.title}"))
    return actions


def plan_sync_actions(cards, existing) -> list[Action]:
    """Plan body updates for issues that already carry the card marker.

    A card with no issue is skipped. This mode does not create or close issues.
    """
    actions = []
    for card in cards:
        issue = existing.get(card.number)
        current = "" if issue is None else (issue.get("body") or "")
        if issue is None or normalise_newlines(current) == normalise_newlines(card.body):
            actions.append(Action("skip", card, f"skip ({card.number}) {card.title}"))
            continue
        actions.append(Action(
            "sync-body",
            card,
            f"sync-body ({card.number}) {card.title}",
            issue_number=int(issue["number"]),
            issue_body=current,
        ))
    return actions


def ensure_labels(gh, slug: str, sleep):
    payload = invoke(gh, ["label", "list", "--repo", slug, "--json", "name", "--limit", "1000"], sleep)
    names = set()
    if payload and str(payload).strip():
        data = json.loads(payload)
        if isinstance(data, list):
            for item in data:
                if isinstance(item, dict) and item.get("name"):
                    names.add(item["name"])
    for name, color, description in LABEL_SPECS:
        if name in names:
            continue
        invoke(
            gh,
            ["label", "create", name, "--repo", slug, "--color", color, "--description", description],
            sleep,
        )


def write_body_file(card: Card) -> Path:
    fd, name = tempfile.mkstemp(prefix=f"phrase-card-{card.number}-", suffix=".md", dir=tempfile.gettempdir())
    os.close(fd)
    path = Path(name)
    path.write_bytes(card.body.encode("utf-8"))
    return path


def create_command(slug: str, card: Card, body_path) -> list[str]:
    command = [
        "issue", "create", "--repo", slug, "--title", f"({card.number}) {card.title}",
        "--body-file", str(body_path), "--label", "card",
    ]
    if card.tooling:
        command.extend(["--label", "tooling"])
    return command


def close_command(slug: str, number, card: Card) -> list[str]:
    return ["issue", "close", str(number), "--repo", slug, "--reason", card.close_reason]


def edit_body_command(slug: str, number, body_path) -> list[str]:
    return ["issue", "edit", str(number), "--repo", slug, "--body-file", str(body_path)]


def format_summary(actions, *, sync_bodies: bool = False) -> str:
    counts = {"create": 0, "close": 0, "close-existing": 0, "skip": 0, "sync-body": 0}
    for action in actions:
        counts[action.kind] = counts.get(action.kind, 0) + 1
    summary = (
        f"{counts['create']} create, {counts['close']} close, "
        f"{counts['close-existing']} close-existing, {counts['skip']} skip"
    )
    if sync_bodies or counts["sync-body"]:
        summary += f", {counts['sync-body']} sync-body"
    return summary


def validate_actions(slug: str, actions):
    """Build every planned mutating command and run the marker and refuse checks."""
    for action in actions:
        if action.kind == "skip":
            continue
        if action.kind == "close-existing":
            command = close_command(slug, action.issue_number, action.card)
            assert_marker(action.issue_body or "", action.card, action.issue_number)
            assert_allowed(command)
            continue
        if action.kind == "sync-body":
            assert_marker(action.issue_body or "", action.card, action.issue_number)
            path = write_body_file(action.card)
            try:
                content = path.read_bytes().decode("utf-8")
                command = edit_body_command(slug, action.issue_number, path)
                assert_marker(content, action.card, action.issue_number)
                assert_allowed(command)
            finally:
                try:
                    path.unlink()
                except OSError:
                    pass
            continue
        path = write_body_file(action.card)
        try:
            content = path.read_bytes().decode("utf-8")
            command = create_command(slug, action.card, path)
            assert_marker(content, action.card)
            assert_allowed(command)
            if f"({action.card.number}) {action.card.title}" not in command:
                raise RuntimeError(f"Create command for card {action.card.number} is missing its title.")
            if "card" not in command:
                raise RuntimeError(f"Create command for card {action.card.number} is missing the card label.")
            if action.card.tooling and "tooling" not in command:
                raise RuntimeError(f"Create command for card {action.card.number} is missing the tooling label.")
            if action.kind == "close":
                # The issue number is assigned when create returns. The preflight
                # command still carries the reason and is checked the same way.
                closing = close_command(slug, 0, action.card)
                assert_marker(content, action.card)
                assert_allowed(closing)
        finally:
            try:
                path.unlink()
            except OSError:
                pass


def issue_number_from_url(output: str) -> int:
    match = None
    for found in re.finditer(r"/issues/(\d+)", output or ""):
        match = found
    if match is None:
        raise RuntimeError(f"Issue create did not return an issue URL: {output!r}")
    return int(match.group(1))


def create_issue(gh, slug: str, card: Card, sleep) -> int:
    path = write_body_file(card)
    try:
        content = path.read_bytes().decode("utf-8")
        command = create_command(slug, card, path)
        assert_marker(content, card)
        output = invoke(gh, command, sleep)
    finally:
        try:
            path.unlink()
        except OSError:
            pass
    return issue_number_from_url(output)


def close_issue(gh, slug: str, number: int, card: Card, sleep):
    assert_marker(card.body, card, number)
    invoke(gh, close_command(slug, number, card), sleep)


def close_existing_issue(gh, slug: str, action: Action, sleep):
    assert_marker(action.issue_body or "", action.card, action.issue_number)
    if not action.card.close_reason:
        raise RuntimeError(f"Refusing to close issue {action.issue_number} without a close reason.")
    invoke(gh, close_command(slug, action.issue_number, action.card), sleep)


def sync_issue_body(gh, slug: str, action: Action, sleep):
    """Edit one issue's body. Title, labels, state, assignees and comments stay."""
    assert_marker(action.issue_body or "", action.card, action.issue_number)
    path = write_body_file(action.card)
    try:
        content = path.read_bytes().decode("utf-8")
        command = edit_body_command(slug, action.issue_number, path)
        assert_marker(content, action.card, action.issue_number)
        assert_allowed(command)
        invoke(gh, command, sleep)
    finally:
        try:
            path.unlink()
        except OSError:
            pass


def apply_actions(gh, slug: str, actions, sleep):
    if not any(action.kind != "skip" for action in actions):
        return
    if any(action.kind in {"create", "close"} for action in actions):
        ensure_labels(gh, slug, sleep)
    mutated = 0
    for action in actions:
        if action.kind == "skip":
            continue
        if mutated:
            sleep(CREATE_INTERVAL_SECONDS)
        if action.kind == "close-existing":
            close_existing_issue(gh, slug, action, sleep)
        elif action.kind == "sync-body":
            sync_issue_body(gh, slug, action, sleep)
        else:
            number = create_issue(gh, slug, action.card, sleep)
            if action.kind == "close":
                close_issue(gh, slug, number, action.card, sleep)
        mutated += 1


def migrate(gh, *, owner: str = OWNER, repo: str = REPO, dry_run: bool = True, sleep=None, board_dir=None, sync_bodies: bool = False):
    ensure_target(owner, repo)
    if sleep is None:
        sleep = time.sleep
    slug = f"{owner}/{repo}"
    board = Path(board_dir) if board_dir is not None else BOARD_DIR
    cards = load_cards(board)
    existing = index_markers(fetch_issues(gh, slug, sleep))
    if sync_bodies:
        actions = plan_sync_actions(cards, existing)
    else:
        actions = plan_actions(cards, existing)
    validate_actions(slug, actions)
    if not dry_run:
        apply_actions(gh, slug, actions, sleep)
    return actions


def main(argv: Sequence[str] | None = None, gh_runner=gh_runner, sleep=time.sleep, board_dir=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apply", action="store_true", help="create issues and close finished cards")
    parser.add_argument("--dry-run", action="store_true", help="list one line per card and change nothing")
    parser.add_argument("--sync-bodies", action="store_true", help="rewrite the body of a marked issue when its card file changed")
    parser.add_argument("--owner", default=OWNER, help="GitHub owner; must be Programming-Made-EZ")
    parser.add_argument("--repo", default=REPO, help="GitHub repo; must be phrase-lang")
    args = parser.parse_args(argv)

    if args.owner != OWNER or args.repo != REPO:
        raise ValueError(f"Owner and repo must be {OWNER}/{REPO}; refusing {args.owner}/{args.repo}.")

    dry_run = not args.apply or args.dry_run
    try:
        actions = migrate(
            gh_runner,
            owner=args.owner,
            repo=args.repo,
            dry_run=dry_run,
            sleep=sleep,
            board_dir=board_dir,
            sync_bodies=args.sync_bodies,
        )
    except ValueError:
        raise
    except Exception as exc:
        print(f"ERROR: {exc}")
        return 1

    if all(action.kind == "skip" for action in actions):
        print("Nothing to do.")
        return 0

    for action in actions:
        print(action.line)
    if dry_run:
        print(format_summary(actions, sync_bodies=args.sync_bodies))
    elif args.sync_bodies:
        synced = sum(action.kind == "sync-body" for action in actions)
        print(f"Applied: {synced} sync-body.")
    else:
        created = sum(action.kind in {"create", "close"} for action in actions)
        closed = sum(action.kind == "close" for action in actions)
        reclosed = sum(action.kind == "close-existing" for action in actions)
        if reclosed:
            print(f"Applied: {created} created, {closed} closed, {reclosed} close-existing.")
        else:
            print(f"Applied: {created} created, {closed} closed.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except ValueError as exc:
        print(f"ERROR: {exc}")
        raise SystemExit(2)
