#!/usr/bin/env python3
"""Generate the frozen Phrase-versus-C# verdict page from numeric result rows."""

from __future__ import annotations

import argparse
import json
import re
from collections import defaultdict
from pathlib import Path


# Decider D32 is the sole source for every frozen numeric threshold.
BAR = {
    "scheduled_runs": 5,
    "attempt_cap": 3,
    "win_gap_points": 5,
    "tie_gap_points": 10,
    "read_block_points": 10,
    "token_ratio": 0.75,
    "patch_rate": 0,
    "minimum_models": 3,
    "minimum_pairs": 3,
    "maximum_harness_error_share": 0.10,
    "holdout_tasks": 3,
}

LABELS = {
    "csharp": re.compile(r"^baseline-094-r([1-5])$"),
    "phrase": re.compile(r"^head2head-dev-r([1-5])$"),
}
KINDS = ("write", "read", "modify", "fix")
CHANGE_KINDS = ("read", "modify", "fix")


def load_rows(path: Path, arm: str) -> list[dict]:
    rows = []
    for line in path.read_text(encoding="utf-8").splitlines():
        if not line.strip():
            continue
        row = json.loads(line)
        match = LABELS[arm].match(str(row.get("label", "")))
        if match and row.get("kind") in KINDS:
            copy = dict(row)
            copy["_run"] = int(match.group(1))
            rows.append(copy)
    return rows


def total_tokens(row: dict) -> float:
    # Documentation is already included in Phrase tokens_in.
    return number(row.get("tokens_in")) + number(row.get("tokens_out"))


def number(value) -> float:
    return float(value) if isinstance(value, (int, float)) and not isinstance(value, bool) else 0.0


def is_harness_error(row: dict) -> bool:
    status = str(row.get("status", "")).lower()
    error = str(row.get("error", "")).lower()
    return status in {"harness-error", "unavailable", "timed-out", "timeout"} or any(
        word in error for word in ("timeout", "timed out", "unavailable", "quota")
    )


def mean(values: list[float]) -> float:
    return sum(values) / len(values) if values else 0.0


def task_macro(rows: list[dict], value) -> float:
    tasks = defaultdict(list)
    for row in rows:
        tasks[(str(row.get("model", "")), str(row.get("task", "")))].append(value(row))
    return mean([mean(values) for values in tasks.values()])


def summaries(rows: list[dict]) -> list[dict]:
    groups = defaultdict(list)
    for row in rows:
        groups[(str(row.get("model", "")), row["kind"])].append(row)
    result = []
    for (model, kind), items in sorted(groups.items()):
        result.append({
            "model": model,
            "kind": kind,
            "pass_rate": task_macro(items, lambda row: 1.0 if row.get("passed") is True else 0.0),
            "attempts": task_macro(items, lambda row: min(number(row.get("attempts")) or BAR["attempt_cap"], BAR["attempt_cap"])),
            "tokens": task_macro(items, total_tokens),
            "context": task_macro(items, lambda row: number(row.get("existing_code_tokens"))),
        })
    by_model = defaultdict(list)
    for row in rows:
        if row["kind"] in CHANGE_KINDS:
            by_model[str(row.get("model", ""))].append(row)
    for model, items in sorted(by_model.items()):
        result.append({
            "model": model,
            "kind": "change",
            "pass_rate": task_macro(items, lambda row: 1.0 if row.get("passed") is True else 0.0),
            "attempts": task_macro(items, lambda row: min(number(row.get("attempts")) or BAR["attempt_cap"], BAR["attempt_cap"])),
            "tokens": task_macro(items, total_tokens),
            "context": task_macro(items, lambda row: number(row.get("existing_code_tokens"))),
        })
    return sorted(result, key=lambda item: (item["model"], item["kind"]))


def arm_pass_rate(rows: list[dict]) -> float:
    by_model = defaultdict(list)
    for row in rows:
        by_model[str(row.get("model", ""))].append(row)
    return mean([task_macro(items, lambda row: 1.0 if row.get("passed") is True else 0.0)
                 for items in by_model.values()])


def paired(csharp: list[dict], phrase: list[dict], kinds: tuple[str, ...]) -> tuple[list[dict], list[dict]]:
    def key(row):
        return (str(row.get("model", "")), str(row.get("task", "")), row["kind"], row["_run"])
    left = {key(row): row for row in csharp if row["kind"] in kinds and row.get("passed") is True}
    right = {key(row): row for row in phrase if row["kind"] in kinds and row.get("passed") is True}
    keys = sorted(left.keys() & right.keys())
    return [left[key] for key in keys], [right[key] for key in keys]


def group_means(csharp: list[dict], phrase: list[dict], kinds: tuple[str, ...]) -> dict:
    left, right = paired(csharp, phrase, kinds)
    return {
        "pairs": len(left),
        "csharp_tokens": task_macro(left, total_tokens),
        "phrase_tokens": task_macro(right, total_tokens),
        "csharp_attempts": task_macro(left, lambda row: number(row.get("attempts"))),
        "phrase_attempts": task_macro(right, lambda row: number(row.get("attempts"))),
    }


def format_percent(value: float) -> str:
    return f"{value * 100:.1f}%"


def format_number(value: float) -> str:
    return f"{value:,.0f}"


def holdout_reasons(summary: dict | None) -> list[str]:
    if summary is None:
        return []
    groups = summary.get("groups", []) if isinstance(summary, dict) else []
    if not groups or any(
            "completed_count" not in group or "unavailable_count" not in group
            for group in groups):
        return ["holdout summary lacks harness-error counts"]
    models = defaultdict(list)
    scheduled = 0
    unavailable = 0
    for group in groups:
        model = str(group.get("model", ""))
        count = int(number(group.get("count")))
        completed_count = int(number(group.get("completed_count")))
        unavailable_count = int(number(group.get("unavailable_count")))
        models[model].append(completed_count)
        scheduled += count
        unavailable += unavailable_count
    usable = sum(1 for completed in models.values() if completed and all(value > 0 for value in completed))
    reasons = []
    if usable < BAR["minimum_models"]:
        noun = "model" if usable == 1 else "models"
        reasons.append(f"the holdout has only {usable} usable {noun}, fewer than {BAR['minimum_models']}")
    error_share = unavailable / scheduled if scheduled else 1.0
    if error_share > BAR["maximum_harness_error_share"]:
        reasons.append(f"holdout harness-error rows are {format_percent(error_share)}, above 10.0%")
    return reasons


def decide(csharp: list[dict], phrase: list[dict], holdout: dict | None) -> tuple[str, list[str], dict, dict]:
    csharp_rate, phrase_rate = arm_pass_rate(csharp), arm_pass_rate(phrase)
    gap = (csharp_rate - phrase_rate) * 100
    write, change = group_means(csharp, phrase, ("write",)), group_means(csharp, phrase, CHANGE_KINDS)
    all_rows = csharp + phrase
    models = set(str(row.get("model", "")) for row in all_rows)
    harness_share = sum(is_harness_error(row) for row in all_rows) / len(all_rows) if all_rows else 1.0
    reasons = []
    if len(models) < BAR["minimum_models"]:
        reasons.append(f"only {len(models)} models are present, fewer than {BAR['minimum_models']}")
    for name, group in (("write", write), ("change", change)):
        if group["pairs"] < BAR["minimum_pairs"]:
            reasons.append(f"the {name} group has only {group['pairs']} paired task-runs, fewer than 3")
    if harness_share > BAR["maximum_harness_error_share"]:
        reasons.append(f"harness-error rows are {format_percent(harness_share)}, above 10.0%")
    reasons.extend(holdout_reasons(holdout))
    if reasons:
        return "INCONCLUSIVE", reasons, write, change
    if gap > BAR["tie_gap_points"]:
        return "LOSS", [f"Phrase pass rate is {gap:.1f} points below plain C#"], write, change
    if gap >= BAR["win_gap_points"]:
        return "TIE", [f"Phrase pass rate is {gap:.1f} points below plain C#"], write, change

    read_c = task_macro([row for row in csharp if row["kind"] == "read"], lambda row: row.get("passed") is True)
    read_p = task_macro([row for row in phrase if row["kind"] == "read"], lambda row: row.get("passed") is True)
    read_blocked = (read_c - read_p) * 100 > BAR["read_block_points"]
    token_ok = []
    attempts_ok = []
    for group in (write, change):
        token_ok.append(group["phrase_tokens"] <= BAR["token_ratio"] * group["csharp_tokens"])
        attempts_ok.append(group["phrase_attempts"] <= group["csharp_attempts"])
    patch_rate = sum(row.get("patched") is True for row in phrase) / len(phrase) if phrase else 0.0
    if all(token_ok) and all(attempts_ok) and patch_rate == BAR["patch_rate"] and not read_blocked:
        return "WIN", ["Phrase meets every frozen pass, token, attempt, read and patch condition"], write, change
    failed = []
    if not all(token_ok):
        failed.append("the token condition fails")
    if not all(attempts_ok):
        failed.append("the attempt condition fails")
    if patch_rate != BAR["patch_rate"]:
        failed.append("the patch rate is above zero")
    if read_blocked:
        failed.append("the read-task gap blocks a win")
    # D32: "A write-only saving is a TIE."
    return "TIE", failed, write, change


def render(csharp: list[dict], phrase: list[dict], holdout: dict | None) -> str:
    verdict, reasons, write, change = decide(csharp, phrase, holdout)
    csharp_rate, phrase_rate = arm_pass_rate(csharp), arm_pass_rate(phrase)
    first = (f"{verdict} Phrase passes {format_percent(phrase_rate)} versus plain C# {format_percent(csharp_rate)}; "
             f"{'; '.join(reasons)}.")
    lines = [first, "", "Vocabulary work remains paused because this result does not clear the frozen WIN bar."
             if verdict != "WIN" else "Vocabulary work may continue because Phrase clears the frozen WIN bar."]
    lines += ["", "Development totals", "", "| Arm | Model | Kind | Pass rate | Mean attempts | Mean total tokens | Context tokens |",
              "|---|---|---|---:|---:|---:|---:|"]
    for arm, rows in (("Plain C#", csharp), ("Phrase", phrase)):
        for item in summaries(rows):
            context = format_number(item["context"]) if item["kind"] in CHANGE_KINDS else "0"
            lines.append(f"| {arm} | {item['model']} | {item['kind']} | {format_percent(item['pass_rate'])} | "
                         f"{item['attempts']:.2f} | {format_number(item['tokens'])} | {context} |")
    development_tokens_ok = (write["phrase_tokens"] <= BAR["token_ratio"] * write["csharp_tokens"]
                             and change["phrase_tokens"] <= BAR["token_ratio"] * change["csharp_tokens"])
    lines += ["", f"Unweighted mean pass rate: plain C# {format_percent(csharp_rate)}; Phrase {format_percent(phrase_rate)}.",
              f"Harness-error share: plain C# {format_percent(mean([is_harness_error(r) for r in csharp]))}; "
              f"Phrase {format_percent(mean([is_harness_error(r) for r in phrase]))}.",
              "Patch rate: Phrase 0.0% by construction; the Phrase arm writes only Phrase.",
              f"Write tasks passing in both arms: Phrase {format_number(write['phrase_tokens'])} tokens and {write['phrase_attempts']:.2f} attempts; "
              f"plain C# {format_number(write['csharp_tokens'])} tokens and {write['csharp_attempts']:.2f} attempts.",
              f"Change tasks passing in both arms: Phrase {format_number(change['phrase_tokens'])} tokens and {change['phrase_attempts']:.2f} attempts; "
              f"plain C# {format_number(change['csharp_tokens'])} tokens and {change['csharp_attempts']:.2f} attempts.",
              f"On development tasks alone, the token condition {'passes' if development_tokens_ok else 'fails'}: "
              f"Phrase about {format_number(task_macro(phrase, total_tokens))} tokens per task against plain C# about {format_number(task_macro(csharp, total_tokens))}."]
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--csharp", required=True, type=Path)
    parser.add_argument("--phrase", required=True, type=Path)
    parser.add_argument("--holdout-summary", type=Path)
    parser.add_argument("--out", required=True, type=Path)
    args = parser.parse_args()
    holdout = json.loads(args.holdout_summary.read_text(encoding="utf-8")) if args.holdout_summary else None
    page = render(load_rows(args.csharp, "csharp"), load_rows(args.phrase, "phrase"), holdout)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(page, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
