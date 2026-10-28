#!/usr/bin/env python3
"""Fail if a tracked file or tracked results row contains holdout material.

The repository is public. Sealed holdouts live under bench/holdouts-unsealed and
must stay untracked. A results row whose label starts with "holdout" or whose task
id is H-numbered would publish a derived reply, so it must not be tracked either.

Used by .githooks/pre-commit. Reads git ls-files (the index), not the work tree of
untracked paths.
"""

from __future__ import annotations

import json
import re
import subprocess
import sys
from pathlib import Path


HOLDOUT_PREFIX = "bench/holdouts-unsealed"


def tracked_files(repo: Path) -> list[str]:
    result = subprocess.run(
        ["git", "-C", str(repo), "ls-files", "-z"],
        capture_output=True,
        check=False,
    )
    if result.returncode != 0:
        message = result.stderr.decode("utf-8", "replace").strip() or "git ls-files failed"
        raise RuntimeError(message)
    return [item.replace("\\", "/") for item in result.stdout.decode("utf-8", "replace").split("\0") if item]


def is_holdout_path(path: str) -> bool:
    return path == HOLDOUT_PREFIX or path.startswith(HOLDOUT_PREFIX + "/")


def is_results_file(path: str) -> bool:
    return "/results/" in f"/{path}" and path.endswith((".json", ".jsonl"))


def tracked_text(repo: Path, relative: str) -> str | None:
    result = subprocess.run(
        ["git", "-C", str(repo), "show", f":{relative}"],
        capture_output=True,
        check=False,
    )
    if result.returncode != 0:
        return None
    return result.stdout.decode("utf-8", "replace")


def iter_rows(relative: str, text: str):
    if relative.endswith(".jsonl"):
        for line_number, line in enumerate(text.splitlines(), 1):
            line = line.strip()
            if not line:
                continue
            try:
                value = json.loads(line)
            except json.JSONDecodeError:
                continue
            if isinstance(value, dict):
                yield line_number, value
        return

    try:
        value = json.loads(text)
    except json.JSONDecodeError:
        return
    rows = value if isinstance(value, list) else [value]
    for row in rows:
        if isinstance(row, dict):
            yield 1, row


def label_starts_with_holdout(row: dict) -> bool:
    label = row.get("label")
    return isinstance(label, str) and label.lower().startswith("holdout")


def has_holdout_task_id(row: dict) -> bool:
    task = row.get("task")
    return isinstance(task, str) and re.fullmatch(r"H[0-9]+", task) is not None


def find_violations(repo: Path) -> list[str]:
    violations = []
    for relative in tracked_files(repo):
        if is_holdout_path(relative):
            violations.append(f"{relative}: tracked file under {HOLDOUT_PREFIX}")
            continue
        if not is_results_file(relative):
            continue
        text = tracked_text(repo, relative)
        if text is None:
            continue
        for line_number, row in iter_rows(relative, text):
            if label_starts_with_holdout(row):
                violations.append(f"{relative}:{line_number}: label starts with holdout ({row.get('label')})")
            if has_holdout_task_id(row):
                violations.append(f"{relative}:{line_number}: task id matches ^H[0-9]+$ ({row.get('task')})")
    return violations


def main(argv: list[str] | None = None) -> int:
    argv = list(sys.argv[1:] if argv is None else argv)
    repo = Path(argv[0]).resolve() if argv else Path.cwd()
    try:
        violations = find_violations(repo)
    except RuntimeError as error:
        print(f"check-no-holdout-in-tracked-files: {error}", file=sys.stderr)
        return 1
    if not violations:
        return 0
    print("pre-commit: tracked files must not publish holdouts (the repo is PUBLIC):", file=sys.stderr)
    for violation in violations:
        print(f"  {violation}", file=sys.stderr)
    return 1


if __name__ == "__main__":
    sys.exit(main())
