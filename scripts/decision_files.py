"""Shared parsing and rendering for the split decision history."""

from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path


AREAS = (
    "language-and-grammar",
    "compiler-and-adapters",
    "process-and-staffing",
    "benchmarks-and-measurement",
    "board-and-delivery",
)

AREA_TITLES = {area: area.replace("-", " ").title() for area in AREAS}

SCOPE = {
    "language-and-grammar": (
        "Language syntax, semantics, vocabulary, and domain-neutral authoring rules.",
        "Compiler mechanics and delivery history belong in their own area files.",
    ),
    "compiler-and-adapters": (
        "Compiler, generated-code, adapter, runtime, and target-integration decisions.",
        "Language design and measurement policy belong in their own area files.",
    ),
    "process-and-staffing": (
        "Team practices, experiments, staffing, review, and decision-making rules.",
        "Product delivery and benchmark rules belong in their own area files.",
    ),
    "benchmarks-and-measurement": (
        "Benchmark design, evidence, scoring, privacy, and measurement decisions.",
        "Language and delivery decisions are linked when related, never copied.",
    ),
    "board-and-delivery": (
        "Board ordering, milestones, continuous delivery, and shipped-card decisions.",
        "Process, compiler, and measurement policy belong in their own area files.",
    ),
}

BOUNDARY = re.compile(r"(?m)^(?:<a id=\"[^\"]+\"></a>|#### .+|Record R[2-4].*)$")
DECISION_HEADING = re.compile(r"^#### Decider (D\d+)(?: \(failed\) and (D\d+) \(passed\))?[:(]")
RECORD_HEADING = re.compile(r"^(?:#### )?Record (R[1-4])[: (]")


def load_metadata(decisions: Path) -> dict[str, dict]:
    return json.loads((decisions / "areas.json").read_text(encoding="utf-8"))


def record_id(first_line: str) -> str | None:
    match = DECISION_HEADING.match(first_line)
    if match:
        return f"{match.group(1)}/{match.group(2)}" if match.group(2) else match.group(1)
    match = RECORD_HEADING.match(first_line)
    return match.group(1) if match else None


ANCHOR = re.compile(r'<a id="[^"]+"></a>\n')


def parse_records(text: str) -> list[tuple[str, str]]:
    """Return decision records in source order without changing any record byte.

    A heading that carries no decision id (for example a sign-off section) stays inside the
    record before it, so no byte of the log can fall between two records. A link anchor line
    directly above a record heading belongs to that record.
    """
    boundaries = list(BOUNDARY.finditer(text))
    starts = []
    for index, boundary in enumerate(boundaries):
        identifier = record_id(boundary.group())
        if not identifier:
            continue
        start = boundary.start()
        if index > 0:
            previous = boundaries[index - 1]
            if ANCHOR.fullmatch(previous.group() + "\n") and previous.end() + 1 == start:
                start = previous.start()
        starts.append((identifier, start))
    records = []
    for index, (identifier, start) in enumerate(starts):
        end = starts[index + 1][1] if index + 1 < len(starts) else len(text)
        records.append((identifier, text[start:end]))
    return records


def area_paths(decisions: Path) -> list[Path]:
    return [decisions / f"{area}.md" for area in AREAS]


def slug(text: str) -> str:
    text = re.sub(r"<[^>]+>", "", text.lower())
    text = re.sub(r"[^a-z0-9 _-]", "", text)
    return re.sub(r"[ _]+", "-", text).strip("-")


def record_anchor(record: str) -> str:
    first_line = next(line for line in record.splitlines() if not ANCHOR.fullmatch(line + "\n"))
    identifier = record_id(first_line)
    if not identifier:
        raise ValueError(f"not a decision record: {first_line}")
    return identifier.lower().replace("/", "-")


def id_sort_key(identifier: str):
    match = re.search(r"\d+", identifier)
    return (0 if identifier.startswith("D") else 1, int(match.group()) if match else 0)


def render_area(area: str, metadata: dict[str, dict], records: dict[str, str]) -> str:
    lines = [f"# {AREA_TITLES[area]} decisions", "", *SCOPE[area], "", "## In force", ""]
    current = [identifier for identifier, item in metadata.items() if item["area"] == area and item["status"] == "in force"]
    if current:
        for identifier in current:
            item = metadata[identifier]
            note = ""
            superseded = [old for old, old_item in metadata.items() if old_item["status"] == f"superseded by {identifier.split('/')[-1]}"]
            if superseded:
                note = f"; supersedes {', '.join(superseded)}"
            lines.append(f"- [{identifier}](#{record_anchor(records[identifier])}) — {item.get('statement', item['title'])}{note}")
    else:
        lines.append("- None.")

    related = [
        identifier
        for identifier, item in metadata.items()
        if area in item.get("also", []) and item["area"] != area
    ]
    lines.extend(["", "Related decisions: " + (
        ", ".join(
            f"[{identifier}]({metadata[identifier]['area']}.md#{record_anchor(records[identifier])})"
            for identifier in related
        ) if related else "None."
    ), "", "## Records", ""])
    prefix = "\n".join(lines) + "\n"
    owned_records = "".join(
        f'<a id="{record_anchor(records[identifier])}"></a>\n' + records[identifier]
        for identifier, item in metadata.items()
        if item["area"] == area
    )
    return prefix + owned_records


def render_index(metadata: dict[str, dict], records: dict[str, str]) -> str:
    lines = [
        "# Decision index",
        "",
        "Start here for the complete decision history. Records are stored once in their owning area; related areas link to them.",
        "",
        "Status vocabulary: `in force`, `applied`, `superseded by Dnnn`, `reverted`.",
        "",
        "## In force",
        "",
    ]
    for area in AREAS:
        lines.extend([f"### {area}", ""])
        current = [identifier for identifier, item in metadata.items() if item["area"] == area and item["status"] == "in force"]
        if current:
            for identifier in current:
                item = metadata[identifier]
                lines.append(f"- [{identifier}]({area}.md#{record_anchor(records[identifier])}) — {item.get('statement', item['title'])}")
        else:
            lines.append("- None.")
        lines.append("")
    lines.extend([
        "## All decisions",
        "",
        "| ID | Title | Area | Status |",
        "| --- | --- | --- | --- |",
    ])
    for identifier, item in metadata.items():
        link = f"{item['area']}.md#{record_anchor(records[identifier])}"
        lines.append(f"| [{identifier}]({link}) | {item['title']} | {item['area']} | {item['status']} |")
    return "\n".join(lines) + "\n"


def records_from_areas(decisions: Path) -> tuple[dict[str, str], dict[str, list[str]]]:
    records: dict[str, str] = {}
    locations: dict[str, list[str]] = {}
    for path in area_paths(decisions):
        if not path.exists():
            continue
        for identifier, record in parse_records(path.read_text(encoding="utf-8", newline="")):
            locations.setdefault(identifier, []).append(path.name)
            records.setdefault(identifier, record)
    return records, locations


def proof(records: dict[str, str], metadata: dict[str, dict]) -> tuple[int, str]:
    ordered = sorted(metadata, key=lambda identifier: metadata[identifier].get("order", list(metadata).index(identifier)))
    content = "".join(ANCHOR.sub("", records[identifier], count=1) for identifier in ordered).encode("utf-8")
    return len(ordered), hashlib.sha256(content).hexdigest()
