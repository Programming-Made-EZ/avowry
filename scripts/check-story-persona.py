#!/usr/bin/env python3
"""Fail unless each open card's story names a persona from PERSONAS.md and has a 'so that'.

Usage: check-story-persona.py [card.md ...]    (default: every board/NNN-*.md)

Steve's standard (2026-10-26): a Connextra story names a persona, not a role, and the so-that is value that
persona gets. This checks the mechanical half: 'As <PersonaName>' and a 'so that' clause. Whether the so-that is
real value rather than an internal design goal is the Ready check's judgement (the product manager reads it).
Done, SUPERSEDED and WITHDRAWN cards are skipped (history stays as written)."""

import pathlib
import re
import sys

SKIP = ("done", "superseded", "withdrawn")


def persona_names(personas_path):
    names = []
    for line in pathlib.Path(personas_path).read_text(encoding="utf-8").splitlines():
        m = re.match(r"##\s+(.+?)\s*$", line)
        if not m:
            continue
        heading = m.group(1)
        paren = re.search(r"\(([^)]+)\)", heading)
        name = paren.group(1) if paren else heading.split(":")[0]
        name = name.strip()
        if name and name.lower() not in ("how to write a story with these",):
            names.append(name)
    return names


def check_card(path, names):
    text = pathlib.Path(path).read_text(encoding="utf-8")
    status = re.search(r"(?m)^- \*\*Status:\*\*\s*(.*)$", text)
    if status and status.group(1).strip().lower().startswith(SKIP):
        return None
    story = re.search(r"(?m)^- \*\*Story:\*\*\s*(.*)$", text)
    if not story:
        return "no Story line"
    line = story.group(1).strip()
    if not any(re.match(rf"As\s+{re.escape(n)}\b", line, re.IGNORECASE) for n in names):
        return f"the story does not start with 'As <persona>' for a persona in PERSONAS.md ({', '.join(names)}): {line[:70]}"
    if not re.search(r"\bso that\b", line, re.IGNORECASE):
        return "the story has no 'so that' (value to the persona)"
    return None


def main(argv):
    root = pathlib.Path(__file__).resolve().parents[1]
    names = persona_names(root / "PERSONAS.md")
    files = [pathlib.Path(a) for a in argv[1:]] or sorted(p for p in (root / "board").glob("[0-9][0-9][0-9]-*.md"))
    bad = 0
    for path in files:
        problem = check_card(path, names)
        if problem:
            bad += 1
            print(f"{path.name}: {problem}")
    print(f"{bad} card(s) with a role-only or value-less story")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
