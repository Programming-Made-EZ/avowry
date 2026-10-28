#!/usr/bin/env python3
"""Reject personal attribution of product direction in tracked public files."""

import re
import subprocess
import sys
from pathlib import Path


NAME = "Ste" + "ve"
NAME_RE = re.escape(NAME)
VERBS = r"said|ruled|asked|wants|wanted|approved|decided|noted|confirmed|suggested|insists|prefers"
FORBIDDEN = (
    ("possessive attribution", re.compile(rf"\b{NAME_RE}[’']s\b", re.I)),
    ("direction attributed to a person", re.compile(rf"\b{NAME_RE}\s+(?:{VERBS})\b", re.I)),
    ("colon attribution", re.compile(rf"\b{NAME_RE}\s*:\s*\S", re.I)),
    ("dated attribution", re.compile(rf"(?:\({NAME_RE}|,\s*{NAME_RE})\s*,?\s*20\d\d", re.I)),
    ("by/from attribution", re.compile(rf"\b(?:by|from)\s+{NAME_RE}\b", re.I)),
    ("personal escalation", re.compile(rf"\bneeds[ -]+{NAME_RE}\b|\bNeeds-{NAME_RE}\b", re.I)),
    ("quoted product-owner attribution", re.compile(r'\bproduct owner\s*:\s*"', re.I)),
)
GENDERED_PRONOUN = re.compile(r"\b(?:his|him|he|himself|hers)\b", re.I)
PRODUCT_ROLE = re.compile(rf"\b(?:product owner|sponsor|{NAME_RE})\b", re.I)


def _allowed(path, line):
    if path.replace("\\", "/") == "PERSONAS.md" and re.match(
        rf"^##\s+{NAME_RE}\s*:", line, re.I
    ):
        return True
    if re.search(rf"\bAs\s+{NAME_RE}\s*,", line, re.I):
        line = re.sub(rf"\bAs\s+{NAME_RE}\s*,", "As PERSONA,", line, flags=re.I)
    normalized = path.replace("\\", "/")
    if normalized.startswith(("scripts/", "tests/")) and re.search(
        rf"(?:PERSONAS?|persona.names|valid.persona).*{NAME_RE}|{NAME_RE}.*(?:PERSONAS?|valid.persona)",
        line,
        re.I,
    ):
        return True
    return line


def forbidden_reasons(path, line):
    masked = _allowed(path, line)
    if masked is True:
        return []
    reasons = [label for label, pattern in FORBIDDEN if pattern.search(masked)]
    if PRODUCT_ROLE.search(masked) and GENDERED_PRONOUN.search(masked):
        reasons.append("gendered pronoun beside product role or persona")
    if re.search(NAME_RE, masked, re.I) and '"' in masked:
        reasons.append("quoted passage beside a personal name")
    return reasons


def classify_findings(findings):
    archive, regular = [], []
    for finding in findings:
        (archive if finding[0].replace("\\", "/").startswith("board/") else regular).append(finding)
    return regular, archive


def tracked_files(root=Path(".")):
    result = subprocess.run(
        ["git", "ls-files", "-z"], cwd=root, capture_output=True, check=True
    )
    return [Path(part.decode("utf-8")) for part in result.stdout.split(b"\0") if part]


def scan(root=Path(".")):
    findings = []
    for relative in tracked_files(root):
        posix = relative.as_posix()
        if posix.startswith("bench/results/") and posix.endswith(".jsonl"):
            continue
        if posix.startswith("bench/holdouts-unsealed/"):
            continue
        try:
            lines = (root / relative).read_text(encoding="utf-8").splitlines()
        except (UnicodeDecodeError, OSError):
            continue
        for number, line in enumerate(lines, 1):
            if forbidden_reasons(posix, line):
                findings.append((posix, number, line))
    return classify_findings(findings)


def _print_group(title, findings):
    if not findings:
        return
    print(title, file=sys.stderr)
    for path, number, line in findings:
        print(f"  {path}:{number}: {line.strip()}", file=sys.stderr)


def main():
    regular, archive = scan()
    _print_group("personal attribution:", regular)
    _print_group("archive (regenerated from issues):", archive)
    return 1 if regular else 0


if __name__ == "__main__":
    sys.exit(main())
