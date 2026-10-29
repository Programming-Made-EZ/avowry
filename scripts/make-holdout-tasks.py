#!/usr/bin/env python3
"""Build fair Phrase and C# task folders from a holdout-format source file."""

from __future__ import annotations

import argparse
import json
import re
import shutil
import subprocess
import sys
from pathlib import Path
from typing import Any


PHRASE_WRAPPER_LINES = (
    "Use the provided Phrase documentation. Return only valid Phrase source.",
    "```avowry",
)
CSHARP_WRAPPER_LINES = (
    "Return only valid C# source.",
    "```csharp",
)
START_FENCE_END = "```"


def strip_language_wrapper(prompt: str, wrapper_lines: tuple[str, str]) -> str:
    wrapper, start_fence = wrapper_lines
    remainder: list[str] = []
    in_start = False
    for line in prompt.splitlines(keepends=True):
        exact = line.rstrip("\r\n")
        if in_start:
            if exact == START_FENCE_END:
                in_start = False
            continue
        if exact == wrapper:
            continue
        if exact == start_fence:
            in_start = True
            continue
        remainder.append(line)
    if in_start:
        raise ValueError(f"unclosed matching-language start fence: {start_fence}")
    return "".join(remainder)


def git_root() -> Path | None:
    git = shutil.which("git")
    if git is None:
        return None
    result = subprocess.run(
        [git, "rev-parse", "--show-toplevel"],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0:
        return None
    root = result.stdout.strip()
    return Path(root).resolve() if root else None


def is_path_in_repo(path: Path, repo_root: Path | None) -> bool:
    if repo_root is None:
        return False
    try:
        path.resolve().relative_to(repo_root)
        return True
    except ValueError:
        return False


def refuse_if_in_repo_and_unignored(out_dir: Path) -> None:
    repo_root = git_root()
    if not is_path_in_repo(out_dir, repo_root):
        return
    git = shutil.which("git")
    if git is None:
        raise RuntimeError("refusing to write: repository path is not git-ignored and git unavailable")
    probe = out_dir / "__holdout_probe__"
    result = subprocess.run(
        [git, "-C", str(repo_root), "check-ignore", "--quiet", "--no-index", str(probe)],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode == 0:
        return
    if result.returncode in (1, 2, 128):
        raise RuntimeError("refusing to write: output path is inside the repository and not git-ignored")
    raise RuntimeError("refusing to write: failed to verify output path safety")


def parse_header(text: str) -> dict[str, str]:
    header: dict[str, str] = {}
    for line in extract_section(text, "Header").splitlines():
        if ":" not in line:
            continue
        left, right = line.split(":", 1)
        left = left.strip().lstrip("-").strip().strip("*").strip()
        if not left:
            continue
        value = right.strip().strip("*").strip()
        header[left.lower()] = value
    return header


def extract_section(text: str, heading_prefix: str) -> str:
    pattern = re.compile(
        rf"^## {re.escape(heading_prefix)}(?:\s*\([^\n]*\))?\s*$\n(.*?)(?=^## |\Z)",
        re.M | re.S,
    )
    match = pattern.search(text)
    if not match:
        raise ValueError(f"missing section: {heading_prefix}")
    return match.group(1).strip()


def extract_task_text(text: str) -> str:
    return extract_section(text, "The task, in plain words")


def extract_outcomes(text: str) -> list[dict[str, Any]]:
    section = extract_section(text, "Observable outcomes")
    match = re.search(r"```json\s*(.*?)\s*```", section, re.S)
    if not match:
        raise ValueError("missing observable outcomes JSON")
    payload = match.group(1)
    return json.loads(payload)


def extract_modify_sections(text: str) -> tuple[str | None, str | None, str | None]:
    try:
        section = extract_section(text, "Starting code")
    except ValueError:
        return None, None, None
    phrase_match = re.search(r"^### start\.avowry\s*$\s*```avowry\s*(.*?)\s*```", section, re.M | re.S)
    csharp_match = re.search(r"^### start\.cs\s*$\s*```csharp\s*(.*?)\s*```", section, re.M | re.S)
    change_match = re.search(r"^### The change requested\s*$\n(.*?)(?=^### |\Z)", section, re.M | re.S)
    start_phrase = phrase_match.group(1).strip() if phrase_match else None
    start_csharp = csharp_match.group(1).strip() if csharp_match else None
    change = change_match.group(1).strip() if change_match else None
    return start_phrase, start_csharp, change


def render_prompt(task_text: str, libraries: str, outcomes: list[dict[str, Any]], kind: str, start_phrase: str | None, start_csharp: str | None, change: str | None, arm: str) -> str:
    base = [
        task_text.strip(),
        "",
        f"Libraries: {libraries}",
        "",
        "Outcomes:",
        json.dumps(outcomes, ensure_ascii=False, indent=2),
        "",
    ]
    if change is not None:
        base.extend(["Change requested:", change.strip(), ""])
    if arm == "phrase":
        wrapper, start_fence = PHRASE_WRAPPER_LINES
        if kind == "modify" and start_phrase is not None:
            base.extend([wrapper, "", start_fence, start_phrase.strip(), START_FENCE_END])
        else:
            base.append(wrapper)
        return "\n".join(base) + "\n"

    wrapper, start_fence = CSHARP_WRAPPER_LINES
    if kind == "modify" and start_csharp is not None:
        base.extend([wrapper, "", start_fence, start_csharp.strip(), START_FENCE_END])
    else:
        base.append(wrapper)
    return "\n".join(base) + "\n"


def write_json(path: Path, value: Any) -> None:
    payload = json.dumps(value, ensure_ascii=False, indent=2)
    path.write_text(payload + "\n", encoding="utf-8")


def write_task(out_dir: Path, task_id: str, task_text: str, libraries: str, outcomes: list[dict[str, Any]], kind: str, start_phrase: str | None, start_csharp: str | None, change: str | None) -> None:
    phrase_dir = out_dir / "phrase" / "tasks" / task_id
    csharp_dir = out_dir / "csharp" / "tasks" / task_id
    phrase_dir.mkdir(parents=True, exist_ok=True)
    csharp_dir.mkdir(parents=True, exist_ok=True)

    phrase_prompt = render_prompt(task_text, libraries, outcomes, kind, start_phrase, start_csharp, change, "phrase")
    csharp_prompt = render_prompt(task_text, libraries, outcomes, kind, start_phrase, start_csharp, change, "csharp")

    (phrase_dir / "prompt.md").write_text(phrase_prompt, encoding="utf-8")
    (csharp_dir / "prompt.md").write_text(csharp_prompt, encoding="utf-8")
    write_json(phrase_dir / "outcomes.json", outcomes)
    write_json(csharp_dir / "outcomes.json", outcomes)

    if kind == "modify":
        if start_phrase is not None:
            (phrase_dir / "start.avowry").write_text(start_phrase + "\n", encoding="utf-8")
        if start_csharp is not None:
            (csharp_dir / "start.cs").write_text(start_csharp + "\n", encoding="utf-8")

    phrase_suite = {"version": 1, "tasks": [{"id": task_id, "kind": kind, "prompt": f"tasks/{task_id}/prompt.md", "holdout": False}]}
    csharp_suite = {"version": 1, "tasks": [{"id": task_id, "kind": kind, "prompt": f"tasks/{task_id}/prompt.md", "outcomes": f"tasks/{task_id}/outcomes.json", "holdout": False}]}
    write_json(out_dir / "phrase" / "suite.json", phrase_suite)
    write_json(out_dir / "csharp" / "suite.json", csharp_suite)


def parse_holdout(path: Path) -> tuple[str, str, str, str, str | None, str | None, list[dict[str, Any]], str | None]:
    text = path.read_text(encoding="utf-8")
    header = parse_header(text)
    missing = [key for key in ("id", "kind", "fits_no_pattern", "libraries") if key not in header]
    if missing:
        raise ValueError(f"missing header entry: {', '.join(missing)}")

    task_id = header["id"]
    kind = header["kind"]
    if kind not in {"write", "modify"}:
        raise ValueError(f"unsupported kind: {kind!r}")

    task_text = extract_task_text(text)
    outcomes = extract_outcomes(text)
    start_phrase, start_csharp, change = extract_modify_sections(text)
    if kind == "modify" and (start_phrase is None or start_csharp is None):
        raise ValueError("modify task requires start.avowry and start.cs blocks")
    return task_id, kind, task_text, header["libraries"], start_phrase, start_csharp, outcomes, change


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description="Build fair Phrase and C# task folders from a holdout-format file.")
    parser.add_argument("holdout_file")
    parser.add_argument("out_dir")
    args = parser.parse_args(argv)

    holdout_file = Path(args.holdout_file).resolve()
    out_dir = Path(args.out_dir).resolve()

    try:
        refuse_if_in_repo_and_unignored(out_dir)
        task_id, kind, task_text, libraries, start_phrase, start_csharp, outcomes, change = parse_holdout(holdout_file)
        out_dir.mkdir(parents=True, exist_ok=True)
        write_task(out_dir, task_id, task_text, libraries, outcomes, kind, start_phrase, start_csharp, change)
        print(f"{out_dir / 'phrase' / 'suite.json'}")
        print(f"{out_dir / 'csharp' / 'suite.json'}")
        print(f"wrote {task_id}: 2 task folders")
        return 0
    except Exception as exc:  # pragma: no cover - CLI exit path
        print(f"error: {exc}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
