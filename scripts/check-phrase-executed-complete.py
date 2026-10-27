#!/usr/bin/env python3
"""Validate that the Phrase arm copied each C# twin's executed outcomes."""

import filecmp
import sys
from pathlib import Path

TASK_IDS = [
    "001-write-greeting",
    "002-write-total",
    "004-read-phrase",
    "005-read-failure",
    "007-modify-return",
    "008-modify-type",
    "010-fix-trigger",
    "011-fix-type",
]


def check_repo(repo_root: Path):
    issues = {"missing": [], "drifted": []}
    for task_id in TASK_IDS:
        phrase_path = repo_root / "bench" / "tasks" / task_id / "outcomes.json"
        csharp_path = repo_root / "bench" / "csharp-baseline" / "tasks" / task_id / "outcomes.json"
        if not phrase_path.exists() or not csharp_path.exists():
            issues["missing"].append(task_id)
            continue
        if not filecmp.cmp(phrase_path, csharp_path, shallow=False):
            issues["drifted"].append(task_id)
    return issues


def main(argv):
    repo_root = Path(argv[1]).resolve() if len(argv) > 1 else Path.cwd()
    issues = check_repo(repo_root)
    problems = []
    if issues["missing"]:
        problems.append(f"missing: {', '.join(sorted(issues['missing']))}")
    if issues["drifted"]:
        problems.append(f"drifted: {', '.join(sorted(issues['drifted']))}")
    if problems:
        print("\n".join(problems))
        return 1
    print("Phrase executed-outcomes suite is complete for the eight head-to-head tasks.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
