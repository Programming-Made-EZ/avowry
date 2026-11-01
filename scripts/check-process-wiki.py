#!/usr/bin/env python3
"""Check card 234 process wiki shape and anchor integrity."""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Iterable

INDEX_REL = Path("docs/process-wiki/INDEX.md")
CARD_GLOB = "board/234-*.md"
RECORD_REL = Path(".mob/records/234/record-in-progress.json")
METHOD_KEYS = ("tokens_per_seat", "facilitator_context_size", "rule_misses")
BASELINES = ("50K", "125K", "965K")

AGREEMENT_REL = Path("TEAM_WORKING_AGREEMENT.md")
BRIEFS_REL = Path("docs/process-wiki/briefs")
FLOOR_SHARE = 0.10  # anchored wiki rule lines / agreement bullet-rule lines (red 570dc5c proposal)
REQUIRED_ROLES = (
    "role/product-manager-ready", "role/architect-slice", "role/technical-writer", "role/qa-red",
    "role/backend-developer-green", "role/independent-reviewer-audit", "role/delivery-lead-ship",
    "role/product-manager-signoff",
)
RULE_LINE_RE = re.compile(r"^- \*\*")

ID_RE = re.compile(
    r"^(?:core/index|rules/[a-z0-9-]+|role/[a-z0-9-]+-[a-z0-9-]+|howto/[a-z0-9-]+)$"
)
DECISION_RE = re.compile(r"\bD(\d+)\b")
LINK_RE = re.compile(r"\[(D\d+)\]\(([^)]+)\)")
SEPARATOR_RE = re.compile(r"^-{3,}$")
QUOTE_RE = re.compile(r'Quote: "([^"]+)"')
ANCHOR_RE = re.compile(r'<a id="d\d+">')


def posix_rel(path: Path, root: Path) -> str:
    return path.relative_to(root).as_posix()


def read_utf8(path: Path) -> tuple[str | None, bool]:
    try:
        return path.read_text(encoding="utf-8"), True
    except UnicodeDecodeError:
        return None, False


def parse_row(line: str) -> list[str] | None:
    stripped = line.strip()
    if not stripped.startswith("|"):
        return None
    cells = [cell.strip() for cell in stripped.split("|")]
    if len(cells) < 3 or cells[0] != "" or cells[-1] != "":
        return ["__malformed__"]
    values = cells[1:-1]
    if len(values) != 4:
        return ["__malformed__"]
    return values


def check_index(root: Path, findings: list[tuple[str, str]]) -> tuple[list[dict[str, str]], set[str]]:
    rows: list[dict[str, str]] = []
    listed_paths: set[str] = set()
    index_path = root / INDEX_REL
    if not index_path.is_file():
        findings.append((INDEX_REL.as_posix(), "no index"))
        return rows, listed_paths

    index_text, ok = read_utf8(index_path)
    if not ok:
        findings.append((INDEX_REL.as_posix(), "not valid UTF-8"))
        return rows, listed_paths

    assert index_text is not None
    seen_id: set[str] = set()
    seen_path: set[str] = set()

    for line in index_text.splitlines():
        parsed = parse_row(line)
        if parsed is None:
            continue
        if parsed == ["__malformed__"]:
            findings.append((INDEX_REL.as_posix(), "malformed row"))
            continue
        pid, page_path, status, summary = parsed
        if pid.lower() == "id" and page_path.lower() == "path":
            continue
        if all(SEPARATOR_RE.fullmatch(part.replace(":", "-")) for part in parsed):
            continue
        if not ID_RE.fullmatch(pid):
            findings.append((INDEX_REL.as_posix(), f"bad id {pid}"))
        if status not in {"enabled", "disabled"}:
            findings.append((INDEX_REL.as_posix(), f"unknown status {status}"))
        if pid in seen_id:
            findings.append((INDEX_REL.as_posix(), f"duplicate id {pid}"))
        else:
            seen_id.add(pid)
        if page_path in seen_path:
            findings.append((INDEX_REL.as_posix(), f"duplicate path {page_path}"))
        else:
            seen_path.add(page_path)
        listed_paths.add(page_path)
        rows.append({"id": pid, "path": page_path, "status": status, "summary": summary})

    if not any(row["status"] == "enabled" for row in rows):
        findings.append((INDEX_REL.as_posix(), "no enabled page"))
    return rows, listed_paths


def extract_tokens_and_links(rule_line: str) -> tuple[list[str], dict[str, list[str]]]:
    tokens = [match.group(1) for match in DECISION_RE.finditer(QUOTE_RE.sub("", rule_line))]
    links: dict[str, list[str]] = {}
    for text, target in LINK_RE.findall(rule_line):
        links.setdefault(text[1:], []).append(target)
    return tokens, links


def anchor_exists(decision_text: str, number: str) -> bool:
    return f'<a id="d{number}">' in decision_text


def decision_section(decision_text: str, number: str) -> str:
    """The text of one decision: from its anchor to the next decision anchor."""
    start = decision_text.find(f'<a id="d{number}">')
    nxt = ANCHOR_RE.search(decision_text, start + 1)
    return decision_text[start:nxt.start() if nxt else len(decision_text)]


def check_quotes(
    rel: str, line: str, sections: dict[str, str], resolved_all: bool, findings: list[tuple[str, str]]
) -> None:
    """Each cited decision needs a verbatim Quote from its own text; each Quote must come from a cited one."""
    quotes = QUOTE_RE.findall(line)
    numbers = sorted(sections, key=int)
    if not numbers:
        return
    if not quotes:
        for number in numbers:
            findings.append((rel, f"missing quote D{number}"))
        return
    covered = {n for n in numbers if any(q in sections[n] for q in quotes)}
    for number in numbers:
        if number not in covered:
            findings.append((rel, f"quote not found in D{number}"))
    if resolved_all and len(covered) == len(numbers):
        for quote in quotes:
            if not any(quote in sections[n] for n in numbers):
                findings.append((rel, "quote not found in " + " or ".join(f"D{n}" for n in numbers)))


def check_pages(
    root: Path, rows: Iterable[dict[str, str]], listed_paths: set[str], findings: list[tuple[str, str]]
) -> int:
    enabled_count = 0
    decision_cache: dict[Path, str | None] = {}
    docs_root = root / "docs/process-wiki"
    for row in rows:
        if row["status"] != "enabled":
            continue
        enabled_count += 1
        page_rel = Path(row["path"])
        page_path = root / page_rel
        if not page_path.is_file():
            findings.append((INDEX_REL.as_posix(), f"missing page {row['id']}"))
            continue
        page_text, ok = read_utf8(page_path)
        rel = posix_rel(page_path, root)
        if not ok:
            findings.append((rel, "not valid UTF-8"))
            continue
        assert page_text is not None
        lines = page_text.splitlines()
        if not lines or lines[0].strip() == "":
            findings.append((rel, "missing summary"))
            continue
        summary = lines[0]
        if len(summary) > 200:
            findings.append((rel, "summary too long"))
        if summary != row["summary"]:
            findings.append((rel, "summary mismatch"))

        for line in lines:
            if not line.startswith("- Rule:"):
                continue
            tokens, links = extract_tokens_and_links(line)
            if not tokens:
                findings.append((rel, "missing decision token"))
                continue
            sections: dict[str, str] = {}
            for number in tokens:
                link_targets = links.get(number, [])
                if not link_targets:
                    findings.append((rel, f"missing anchor link D{number}"))
                    continue
                for link_target in link_targets:
                    target_path, sep, fragment = link_target.partition("#")
                    expected_fragment = f"d{number}"
                    if sep == "" or fragment != expected_fragment:
                        findings.append((rel, f"mismatched anchor D{number}"))
                        continue
                    decision_path = (page_path.parent / target_path).resolve()
                    try:
                        decision_rel = decision_path.relative_to(root)
                    except ValueError:
                        findings.append((rel, f"broken anchor D{number}"))
                        continue
                    if decision_rel.parts[:2] != ("docs", "decisions"):
                        findings.append((rel, f"broken anchor D{number}"))
                        continue
                    if not decision_path.is_file():
                        findings.append((rel, f"broken anchor D{number}"))
                        continue
                    if decision_path not in decision_cache:
                        text, decoded = read_utf8(decision_path)
                        decision_cache[decision_path] = text if decoded else None
                    decision_text = decision_cache[decision_path]
                    if decision_text is None or not anchor_exists(decision_text, number):
                        findings.append((rel, f"broken anchor D{number}"))
                    else:
                        sections.setdefault(number, decision_section(decision_text, number))
            check_quotes(rel, line, sections, len(sections) == len(set(tokens)), findings)

    if docs_root.is_dir():
        for page in sorted(docs_root.rglob("*.md")):
            rel = posix_rel(page, root)
            if page.parent == root / BRIEFS_REL:
                continue
            if rel not in listed_paths:
                findings.append((rel, "unlisted page"))
    return enabled_count


def check_briefs(root: Path, rows: list[dict[str, str]], findings: list[tuple[str, str]]) -> None:
    status = {row["id"]: row["status"] for row in rows}
    for role in REQUIRED_ROLES:
        if status.get(role) != "enabled" or not (root / next(r["path"] for r in rows if r["id"] == role)).is_file():
            findings.append((INDEX_REL.as_posix(), f"missing role page {role}"))
        stem = role.split("/", 1)[1]
        if not (root / BRIEFS_REL / f"{stem}.md").is_file():
            findings.append(((BRIEFS_REL / f"{stem}.md").as_posix(), "missing brief"))
    briefs = root / BRIEFS_REL
    for brief in sorted(briefs.glob("*.md")) if briefs.is_dir() else []:
        rel = posix_rel(brief, root)
        text, ok = read_utf8(brief)
        if not ok or text is None:
            findings.append((rel, "not valid UTF-8"))
            continue
        pages_lines = [ln for ln in text.splitlines() if ln.startswith("Pages:")]
        if len(pages_lines) != 1:
            findings.append((rel, "missing Pages line" if not pages_lines else "repeated Pages line"))
            continue
        ids = [part.strip() for part in pages_lines[0][len("Pages:"):].split(",") if part.strip()]
        seen: set[str] = set()
        for pid in ids:
            if pid in seen:
                findings.append((rel, f"repeated page id {pid}"))
            seen.add(pid)
            if pid not in status:
                findings.append((rel, f"unknown page id {pid}"))
            elif status[pid] != "enabled":
                findings.append((rel, f"disabled page id {pid}"))
            elif pid != "core/index" and not pid.startswith("role/"):
                findings.append((rel, f"brief pages must include only core/index and role/ pages, not {pid}"))
        own = f"role/{brief.stem}"
        if "core/index" not in seen:
            findings.append((rel, "brief pages must include core/index"))
        if own not in seen:
            findings.append((rel, f"brief pages must include {own}"))
        for pid in sorted(seen - {"core/index", own}):
            if pid.startswith("role/") and pid in status:
                findings.append((rel, f"brief pages must include only core/index and {own}, not {pid}"))


def check_floor(root: Path, rows: list[dict[str, str]], findings: list[tuple[str, str]]) -> None:
    agreement = root / AGREEMENT_REL
    text, ok = read_utf8(agreement) if agreement.is_file() else (None, False)
    if text is None:
        findings.append((AGREEMENT_REL.as_posix(), "no agreement to measure the coverage floor against"))
        return
    total = sum(1 for line in text.splitlines() if RULE_LINE_RE.match(line))
    anchored = 0
    for row in rows:
        page = root / row["path"]
        if row["status"] == "enabled" and page.is_file():
            body, ok = read_utf8(page)
            anchored += sum(1 for line in (body or "").splitlines() if line.startswith("- Rule:"))
    if total and anchored < FLOOR_SHARE * total:
        findings.append((INDEX_REL.as_posix(),
                         f"coverage floor {FLOOR_SHARE:.2f} not met: {anchored} anchored rule lines of {total} agreement rule lines"))


def check_card_and_record(root: Path, findings: list[tuple[str, str]]) -> None:
    cards = sorted(root.glob(CARD_GLOB))
    if not cards:
        findings.append((CARD_GLOB, "no card"))
    else:
        card_text, ok = read_utf8(cards[0])
        if ok and card_text is not None:
            for baseline in BASELINES:
                if not re.search(rf"\b{re.escape(baseline)}\b", card_text):
                    findings.append((cards[0].relative_to(root).as_posix(), f"missing number {baseline}"))

    record_path = root / RECORD_REL
    if not record_path.is_file():
        findings.append((RECORD_REL.as_posix(), "no record"))
        return
    record_text, ok = read_utf8(record_path)
    if not ok or record_text is None:
        findings.append((RECORD_REL.as_posix(), "record not valid JSON"))
        return
    try:
        record_data = json.loads(record_text)
    except json.JSONDecodeError:
        findings.append((RECORD_REL.as_posix(), "record not valid JSON"))
        return
    methods = record_data.get("measurement_methods")
    if not isinstance(methods, dict):
        methods = {}
    for key in METHOD_KEYS:
        value = methods.get(key, "")
        if not str(value).strip():
            findings.append((RECORD_REL.as_posix(), f"missing method {key}"))


def run(root: Path) -> int:
    findings: list[tuple[str, str]] = []
    rows, listed_paths = check_index(root, findings)
    enabled_count = check_pages(root, rows, listed_paths, findings) if rows else 0
    check_card_and_record(root, findings)
    if rows:
        check_briefs(root, rows, findings)
        check_floor(root, rows, findings)
    if findings:
        for path, detail in findings:
            print(f"{path}: {detail}", file=sys.stderr)
        return 1
    print(f"process-wiki: OK ({enabled_count} pages)")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(add_help=True)
    parser.add_argument("--root", default=".", help="Repository root to check.")
    args = parser.parse_args()
    root = Path(args.root)
    if not root.is_dir():
        print("not a directory", file=sys.stderr)
        return 2
    try:
        return run(root.resolve())
    except Exception:
        print(f"{INDEX_REL.as_posix()}: malformed row", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
