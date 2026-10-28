#!/usr/bin/env python3
"""Fail unless docs/adr/ADR-library-binding-layering.md records the layers.

The check fails when a required section is missing, when an audit row names a
file that does not exist, when the second-binding rule is absent, or when the
profile sketch is not marked a sketch.

Usage: python scripts/check-library-binding-layering.py
"""

import pathlib
import re
import sys


ROOT = pathlib.Path(__file__).resolve().parents[1]
ADR = ROOT / "docs" / "adr" / "ADR-library-binding-layering.md"

SECTIONS = (
    "## Layers",
    "## Logic sources, in order",
    "## Audit of FastEndpoints in the C# emitter",
    "## Hard-wire rule",
    "## Second binding",
    "## Profile sketch",
)

LAYERS_NEEDLES = (
    "domain extension",
    "abstract",
    "endpoint",
    "route",
    "store",
    "library binding",
    "FastEndpoints",
    "Minimal APIs",
    "controllers",
    "EF Core",
    "Dapper",
    "project profile",
    "vertical slices",
    "clean",
    "onion",
    "naming",
    "nullable",
    "analyzers",
    "file layout",
    "test framework",
    "smart defaults",
    "override",
    "deterministic",
    "LLM-as-compiler",
    "same spec",
    "generated tests",
)

AUDIT_HEADER = "| Place | Where it lives today | Seam it needs |"

AUDIT_PLACES = (
    "Project template",
    "Program.cs",
    "Endpoint classes",
    "Effectful generated specs",
    "Test helpers",
    "Benchmark scorers of cards 110 and 111",
    "{Record}Store provisional convention",
)

AUDIT_FILES = (
    "src/Phrase.Adapters.CSharp/CSharpProvider.cs",
    "tools/Phrase.BenchCs/Program.cs",
    "tools/Phrase.Bench/Program.cs",
)

HARDWIRE_NEEDLES = (
    "prototype path",
    "may not",
    "hard-wire",
)

SECOND_NEEDLES = (
    "only on a real need",
    "default stays FastEndpoints",
    "specs never name a library",
)

SKETCH_NEEDLES = (
    "NOT implemented",
    "sketch",
)

FILE_IN_TICKS = re.compile(r"`([^`]+)`")
FILE_PATH = re.compile(r"^[\w./-]+\.(?:cs|csproj|md|py)$")
LINE_CITATION = re.compile(
    r"`(?P<path>[\w./-]+\.(?:cs|csproj|md|py))`\s+"
    r"(?:line|lines)\s+(?P<lines>[\d,\s-]+)"
)

LOGIC_SOURCE_ORDER = (
    "library pattern",
    "adapter idiom",
    "LLM-generated body",
    "native escape hatch",
)


def section(text, heading):
    lines = text.splitlines()
    start = None
    for index, line in enumerate(lines):
        if line.strip() == heading:
            start = index + 1
            break
    if start is None:
        return None
    body = []
    for line in lines[start:]:
        if line.startswith("## "):
            break
        body.append(line)
    return "\n".join(body)


def has_token(text, token):
    if token.isalpha() and token.islower():
        return re.search(rf"\b{re.escape(token)}\b", text) is not None
    return token in text


def missing_needles(body, needles):
    return [needle for needle in needles if not has_token(body or "", needle)]


def table_rows(body):
    rows = []
    for line in (body or "").splitlines():
        if not line.startswith("|"):
            continue
        cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
        if len(cells) < 3:
            continue
        if cells[0].lower() == "place" or set(cells[0]) <= set("-: "):
            continue
        rows.append(cells)
    return rows


def cited_files(where):
    cited = []
    for token in FILE_IN_TICKS.findall(where):
        if FILE_PATH.fullmatch(token):
            cited.append(token)
    return cited


def cited_lines(where):
    for match in LINE_CITATION.finditer(where):
        numbers = [int(value) for value in re.findall(r"\d+", match.group("lines"))]
        yield match.group("path"), numbers


def check_text(text, root):
    problems = []
    for heading in SECTIONS:
        if section(text, heading) is None:
            problems.append(f"missing section {heading}")

    layers = section(text, "## Layers")
    for needle in missing_needles(layers, LAYERS_NEEDLES):
        problems.append(f"layers section lacks {needle}")

    logic_sources = section(text, "## Logic sources, in order")
    if logic_sources is not None:
        lowered_logic_sources = logic_sources.lower()
        positions = [lowered_logic_sources.find(source.lower()) for source in LOGIC_SOURCE_ORDER]
        if any(position < 0 for position in positions) or positions != sorted(positions):
            problems.append("logic-sources section lacks the four-step order")
        native_share = (
            "share of logic still in native blocks",
            "trade-off signal",
            "not a target",
        )
        if missing_needles(logic_sources, native_share):
            problems.append("logic-sources section lacks the native-share signal statement")

    audit = section(text, "## Audit of FastEndpoints in the C# emitter")
    if audit is not None:
        if AUDIT_HEADER not in audit:
            problems.append("audit section lacks the place table")
        for place in AUDIT_PLACES:
            if place not in audit:
                problems.append(f"audit lacks {place}")
        for rel in AUDIT_FILES:
            if rel not in audit:
                problems.append(f"audit lacks {rel}")
        rows = table_rows(audit)
        if not rows:
            problems.append("audit table has no rows")
        for rel in cited_files(audit):
            if not (pathlib.Path(root) / rel).is_file():
                problems.append(f"audit cites missing file {rel}")
        for rel, numbers in cited_lines(audit):
            cited_path = pathlib.Path(root) / rel
            if not cited_path.is_file():
                continue
            line_count = len(cited_path.read_text(encoding="utf-8").splitlines())
            for number in numbers:
                if number < 1 or number > line_count:
                    problems.append(
                        f"audit cites {rel} line {number} beyond its {line_count} lines"
                    )
        for cells in rows:
            where = cells[1]
            files = cited_files(where)
            if not files:
                problems.append(f"audit row '{cells[0]}' names no file")
            for rel in files:
                cited_path = pathlib.Path(root) / rel
                if not cited_path.is_file():
                    problems.append(f"audit row cites missing file {rel}")
            seam = cells[2] if len(cells) > 2 else ""
            if not seam:
                problems.append(f"audit row '{cells[0]}' has no seam")
            elif "seam" not in seam.lower():
                problems.append(f"audit row '{cells[0]}' does not name a seam")

    hardwire = section(text, "## Hard-wire rule")
    for needle in missing_needles(hardwire, HARDWIRE_NEEDLES):
        problems.append(f"hard-wire section lacks {needle}")

    second = section(text, "## Second binding")
    for needle in missing_needles(second, SECOND_NEEDLES):
        problems.append(f"second-binding section lacks {needle}")

    sketch = section(text, "## Profile sketch")
    for needle in missing_needles(sketch, SKETCH_NEEDLES):
        problems.append(f"profile sketch lacks {needle}")
    if sketch is not None:
        lowered = sketch.lower()
        if "toml" not in lowered and "yaml" not in lowered:
            problems.append("profile sketch is not TOML or YAML")

    return problems


def check_path(path, root):
    document = pathlib.Path(path)
    if not document.is_file():
        return [f"ADR is missing: {document}"]
    return check_text(document.read_text(encoding="utf-8"), root)


def main():
    problems = check_path(ADR, ROOT)
    if problems:
        for problem in problems:
            print(problem)
        return 1
    print(f"{ADR.relative_to(ROOT)} records the library-binding layering")
    return 0


if __name__ == "__main__":
    sys.exit(main())
