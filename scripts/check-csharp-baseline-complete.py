#!/usr/bin/env python3
"""Validate that the plain-C# baseline mirrors the Phrase task suite."""

import json
import sys
from pathlib import Path


def _load_json(path: Path):
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def check_repo(repo_root: Path):
    phrase_suite = repo_root / "bench" / "suite.json"
    csharp_suite = repo_root / "bench" / "csharp-baseline" / "suite.json"
    issues = {"missing": set(), "extra": set(), "holdout": set()}

    if not phrase_suite.exists() or not csharp_suite.exists():
        raise FileNotFoundError(f"Missing suite file(s): {phrase_suite} or {csharp_suite}")

    phrase_tasks = {}
    for item in _load_json(phrase_suite).get("tasks", []):
        task_id = item.get("id")
        if task_id:
            phrase_tasks[task_id] = item

    csharp_tasks = {}
    for item in _load_json(csharp_suite).get("tasks", []):
        task_id = item.get("id")
        if task_id:
            csharp_tasks[task_id] = item

    for task_id, task in sorted(phrase_tasks.items()):
        if task.get("holdout"):
            if task_id in csharp_tasks:
                issues["holdout"].add(task_id)
            continue

        csharp_task = csharp_tasks.get(task_id)
        if csharp_task is None:
            issues["missing"].add(task_id)
            continue

        if csharp_task.get("kind") != task.get("kind"):
            issues["missing"].add(task_id)

        prompt_rel = csharp_task.get("prompt")
        if not prompt_rel:
            issues["missing"].add(task_id)
        else:
            prompt_path = repo_root / "bench" / "csharp-baseline" / prompt_rel
            if not prompt_path.exists():
                issues["missing"].add(task_id)

        outcomes_rel = csharp_task.get("outcomes")
        if not outcomes_rel:
            issues["missing"].add(task_id)
        else:
            outcomes_path = repo_root / "bench" / "csharp-baseline" / outcomes_rel
            if not outcomes_path.exists():
                issues["missing"].add(task_id)
            else:
                try:
                    cases = _load_json(outcomes_path)
                except json.JSONDecodeError:
                    issues["missing"].add(task_id)
                else:
                    if not isinstance(cases, list) or len(cases) == 0:
                        issues["missing"].add(task_id)

        if task.get("kind") in {"read", "modify", "fix"}:
            start_path = repo_root / "bench" / "csharp-baseline" / "tasks" / task_id / "start.cs"
            if not start_path.exists():
                issues["missing"].add(task_id)

    for task_id in sorted(csharp_tasks):
        if task_id not in phrase_tasks:
            issues["extra"].add(task_id)
        elif phrase_tasks[task_id].get("holdout") and task_id in csharp_tasks:
            issues["holdout"].add(task_id)

    return issues


def main(argv):
    repo_root = Path(argv[1]).resolve() if len(argv) > 1 else Path.cwd()
    issues = check_repo(repo_root)

    missing = sorted(issues["missing"])
    extra = sorted(issues["extra"])
    holdout = sorted(issues["holdout"])

    if missing or extra or holdout:
        if missing:
            print(f"Missing C# twin(s): {', '.join(missing)}")
        if extra:
            print(f"Extra C# twin(s): {', '.join(extra)}")
        if holdout:
            print(f"Holdout twin(s) present in C# suite: {', '.join(holdout)}")
        return 1

    print("C# baseline suite is complete for the eight development tasks.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
