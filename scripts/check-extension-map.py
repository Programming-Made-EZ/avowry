#!/usr/bin/env python3
"""Fail unless docs/adr/ADR-extension-map.md records the core/extension map.

The check fails when a required section is missing, when an audit row names a
file that does not exist, or when a candidate domain is not marked NOT BUILT.

Usage: python scripts/check-extension-map.py
"""

import pathlib
import re
import sys


ROOT = pathlib.Path(__file__).resolve().parents[1]
ADR = ROOT / "docs" / "adr" / "ADR-extension-map.md"

SECTIONS = (
    "## Core",
    "## Extension",
    "## Testing split",
    "## Audit",
    "## Candidate domains",
    "## How a project uses an extension",
)

CORE_NEEDLES = (
    "domain-neutral",
    "phrases",
    "Given/When/Then",
    "types",
    "control",
    "error",
    "effects",
    "failure codes",
)

EXTENSION_NEEDLES = (
    "library",
    "cheat sheet",
    "loaded only when",
    "keywords",
    "diagnostics",
    "orthogonal",
    "C# adapter",
    "every domain extension",
)

TESTING_NEEDLES = (
    "Given/When/Then",
    "xUnit",
    "NUnit",
    "mocking",
    "fixtures",
    "spec-to-test",
)

AUDIT_TOKENS = (
    "PATCH",
    "endpoint",
    "responds",
    "when handled",
    "route parameter",
    "reserved name input",
    "422",
    "PH209",
    "PH304",
    "PH322",
    "PH323",
    "PH330",
    "PH331",
    "PH333",
    "FastEndpoints",
    "Microsoft.NET.Sdk.Web",
    "Program.cs",
    "xUnit",
    "ensure",
    "validate",
    "find",
    "load",
    "compute",
    "save",
    "publish",
    "Scheduler",
    "Clock",
    "Cancellation",
    "Events",
    "native csharp",
)

CANDIDATE_DOMAINS = (
    "web/HTTP",
    "desktop UI (WinForms, WPF)",
    "mobile",
    "data and databases (queries, transactions, migrations)",
    "testing frameworks",
    "messaging and queues",
    "file and IO",
    "auth and security",
    "background jobs",
)

CANDIDATE_NEEDLES = (
    "only when a real use needs it",
    "only web is built now",
    "card 142",
)

HOW_NEEDLES = (
    "endpoint",
    "verb",
    "route",
    "uses web",
    "deterministic",
)

FILE_IN_TICKS = re.compile(r"`([^`]+)`")
FILE_PATH = re.compile(r"^[\w./-]+\.(?:cs|csproj|md|py)$")


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
        if cells[0].lower() == "item" or set(cells[0]) <= set("-: "):
            continue
        rows.append(cells)
    return rows


def cited_files(where):
    cited = []
    for token in FILE_IN_TICKS.findall(where):
        if FILE_PATH.fullmatch(token):
            cited.append(token)
    return cited


def check_text(text, root):
    problems = []
    for heading in SECTIONS:
        if section(text, heading) is None:
            problems.append(f"missing section {heading}")

    core = section(text, "## Core")
    for needle in missing_needles(core, CORE_NEEDLES):
        problems.append(f"core section lacks {needle}")

    extension = section(text, "## Extension")
    for needle in missing_needles(extension, EXTENSION_NEEDLES):
        problems.append(f"extension section lacks {needle}")

    testing = section(text, "## Testing split")
    for needle in missing_needles(testing, TESTING_NEEDLES):
        problems.append(f"testing section lacks {needle}")

    audit = section(text, "## Audit")
    if audit is not None:
        if "| Item | Where it lives today | Proposed home |" not in audit:
            problems.append("audit section lacks the item table")
        for token in missing_needles(audit, AUDIT_TOKENS):
            problems.append(f"audit lacks {token}")
        rows = table_rows(audit)
        if not rows:
            problems.append("audit table has no rows")
        for cells in rows:
            where = cells[1]
            files = cited_files(where)
            if not files:
                problems.append(f"audit row '{cells[0]}' names no file")
            for rel in files:
                if not (pathlib.Path(root) / rel).is_file():
                    problems.append(f"audit row cites missing file {rel}")
            if not cells[2]:
                problems.append(f"audit row '{cells[0]}' has no proposed home")

    domains = section(text, "## Candidate domains")
    if domains is not None:
        for needle in missing_needles(domains, CANDIDATE_NEEDLES):
            problems.append(f"candidate section lacks {needle}")
        for domain in CANDIDATE_DOMAINS:
            marked = False
            for line in domains.splitlines():
                if not line.startswith("|") or domain not in line:
                    continue
                cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
                if any(cell == "NOT BUILT" for cell in cells):
                    marked = True
            if not marked:
                problems.append(f"candidate domain {domain} is not marked NOT BUILT")

    how = section(text, "## How a project uses an extension")
    for needle in missing_needles(how, HOW_NEEDLES):
        problems.append(f"use section lacks {needle}")

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
    print(f"{ADR.relative_to(ROOT)} records the extension map")
    return 0


if __name__ == "__main__":
    sys.exit(main())
