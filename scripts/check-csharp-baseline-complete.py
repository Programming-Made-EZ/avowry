#!/usr/bin/env python3
"""Validate that the plain-C# baseline mirrors the Phrase task suite."""

import json
import sys
from pathlib import Path


def _load_json(path: Path):
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def _empty_prompt(path: Path):
    if not path.exists():
        return True
    return not path.read_text(encoding="utf-8").strip()


def _valid_outcomes(path: Path):
    try:
        cases = _load_json(path)
    except (json.JSONDecodeError, OSError):
        return False
    if not isinstance(cases, list) or not cases:
        return False
    for case in cases:
        if not isinstance(case, dict):
            return False
        if not {"name", "given", "when", "then"}.issubset(case):
            return False
    return True


def _collect_tasks(suite_path: Path):
    suite = _load_json(suite_path)
    tasks = suite.get("tasks", [])
    if not isinstance(tasks, list):
        return {}, set()
    results = {}
    duplicates = set()
    seen = set()
    for item in tasks:
        if not isinstance(item, dict):
            continue
        task_id = item.get("id")
        if not task_id:
            continue
        if task_id in seen:
            duplicates.add(task_id)
            continue
        seen.add(task_id)
        results[task_id] = item
    return results, duplicates


def check_repo(repo_root: Path):
    phrase_suite = repo_root / "bench" / "suite.json"
    csharp_suite = repo_root / "bench" / "csharp-baseline" / "suite.json"
    issues = {
        "missing": set(),
        "extra": set(),
        "holdout": set(),
        "duplicate": set(),
        "empty_prompt": set(),
        "malformed_outcomes": set(),
        "forbidden_start": set(),
        "kind_mismatch": set(),
    }

    if not phrase_suite.exists() or not csharp_suite.exists():
        raise FileNotFoundError(f"Missing suite file(s): {phrase_suite} or {csharp_suite}")

    phrase_tasks, phrase_duplicates = _collect_tasks(phrase_suite)
    csharp_tasks, csharp_duplicates = _collect_tasks(csharp_suite)
    issues["duplicate"].update(phrase_duplicates)
    issues["duplicate"].update(csharp_duplicates)

    phrase_holdout = {task_id for task_id, task in phrase_tasks.items() if task.get("holdout")}
    csharp_holdout = {task_id for task_id, task in csharp_tasks.items() if task.get("holdout")}
    issues["holdout"].update(phrase_holdout & set(csharp_tasks))
    issues["holdout"].update(csharp_holdout)

    csharp_root = repo_root / "bench" / "csharp-baseline"
    task_root = csharp_root / "tasks"
    if task_root.exists():
        dirs = {entry.name for entry in task_root.iterdir() if entry.is_dir()}
        extra_dirs = dirs - set(csharp_tasks)
        issues["extra"].update(extra_dirs)
        issues["holdout"].update(dirs & phrase_holdout)

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
            issues["kind_mismatch"].add(task_id)

        prompt_rel = csharp_task.get("prompt")
        if not prompt_rel:
            issues["missing"].add(task_id)
        else:
            prompt_path = csharp_root / prompt_rel
            if not prompt_path.exists():
                issues["missing"].add(task_id)
            elif _empty_prompt(prompt_path):
                issues["empty_prompt"].add(task_id)

        outcomes_rel = csharp_task.get("outcomes")
        if not outcomes_rel:
            issues["missing"].add(task_id)
        else:
            outcomes_path = csharp_root / outcomes_rel
            if not outcomes_path.exists():
                issues["missing"].add(task_id)
            elif not _valid_outcomes(outcomes_path):
                issues["malformed_outcomes"].add(task_id)

        start_path = task_root / task_id / "start.cs"
        if task.get("kind") in {"read", "modify", "fix"}:
            if not start_path.exists():
                issues["missing"].add(task_id)
        elif task.get("kind") == "write":
            if start_path.exists():
                issues["forbidden_start"].add(task_id)

    for task_id in sorted(csharp_tasks):
        if task_id not in phrase_tasks:
            issues["extra"].add(task_id)
        if phrase_tasks.get(task_id, {}).get("holdout") and task_id in csharp_tasks:
            issues["holdout"].add(task_id)

    return issues


def main(argv):
    repo_root = Path(argv[1]).resolve() if len(argv) > 1 else Path.cwd()
    issues = check_repo(repo_root)

    categories = [
        ("missing", sorted(issues["missing"])),
        ("extra", sorted(issues["extra"])),
        ("holdout", sorted(issues["holdout"])),
        ("duplicate", sorted(issues["duplicate"])),
        ("empty_prompt", sorted(issues["empty_prompt"])),
        ("malformed_outcomes", sorted(issues["malformed_outcomes"])),
        ("forbidden_start", sorted(issues["forbidden_start"])),
        ("kind_mismatch", sorted(issues["kind_mismatch"])),
    ]

    problems = [
        f"{label}: {', '.join(values)}" for label, values in categories if values
    ]
    if problems:
        print("\n".join(problems))
        return 1

    print("C# baseline suite is complete for the eight development tasks.")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
