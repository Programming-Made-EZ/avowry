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

ID_RE = re.compile(
    r"^(?:core/index|rules/[a-z0-9-]+|role/[a-z0-9-]+-[a-z0-9-]+|howto/[a-z0-9-]+)$"
)
DECISION_RE = re.compile(r"\bD(\d+)\b")
LINK_RE = re.compile(r"\[(D\d+)\]\(([^)]+)\)")
SEPARATOR_RE = re.compile(r"^-{3,}$")


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
    tokens = [match.group(1) for match in DECISION_RE.finditer(rule_line)]
    links: dict[str, list[str]] = {}
    for text, target in LINK_RE.findall(rule_line):
        links.setdefault(text[1:], []).append(target)
    return tokens, links


def anchor_exists(decision_text: str, number: str) -> bool:
    return f'<a id="d{number}">' in decision_text


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

    if docs_root.is_dir():
        for page in sorted(docs_root.rglob("*.md")):
            rel = posix_rel(page, root)
            if rel not in listed_paths:
                findings.append((rel, "unlisted page"))
    return enabled_count


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
