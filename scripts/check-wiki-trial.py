#!/usr/bin/env python3
"""Card 237 pre-trial check (window 237-slice, condition 2): the wiki trial may only run, and its
ledger may only be believed, when thresholds, freeze and miss-test key were committed in order by
the right seats and every ledger row matches the independent reviewer recompute.

Reads .mob/records/237/trial/{thresholds,freeze,miss-key,ledger,recompute}.json; uses git history
(ancestry, never dates or file contents) for before/after. Writes nothing. Stdlib only.

  python scripts/check-wiki-trial.py [--root DIR]
  exit 0 clean, 1 findings (stderr "<path>: <keyword> <detail>"), 2 not a directory. Never a traceback.
A missing trial file is a finding: the check is red until step 0d of the slice.
"""

from __future__ import annotations

import argparse
import json
import statistics
import subprocess
import sys
from pathlib import Path

TRIAL = ".mob/records/237/trial"
FILES = ("thresholds", "freeze", "miss-key", "ledger", "recompute")
RUNNER_SEATS = {"ctx-5"}  # delivery lead; the ledger may name more
THRESHOLD_KEYS = ("token_delta_adopt_pct", "token_delta_reject_pct", "blocked_seconds",
                  "upkeep_tokens_per_card", "upkeep_minutes", "cap_stage_a", "cap_stage_ab")
ROW_KEYS = ("run_key", "arm", "role", "stage", "input_total", "cached_input", "output_total",
            "counted_tokens", "misses", "source")
INT_KEYS = ("input_total", "cached_input", "output_total", "counted_tokens", "misses")


def git(root, *args):
    proc = subprocess.run(["git", *args], cwd=str(root), capture_output=True, text=True,
                          encoding="utf-8", errors="replace", timeout=60)
    return proc.returncode, proc.stdout.strip()


def commits_of(root, rel):
    """Commits touching rel, oldest first; [] when never committed."""
    code, out = git(root, "log", "--format=%H", "--reverse", "--", rel)
    return out.split() if code == 0 and out else []


def is_ancestor(root, older, newer):
    return older != newer and git(root, "merge-base", "--is-ancestor", older, newer)[0] == 0


def is_int(v):
    return isinstance(v, int) and not isinstance(v, bool)


def load(root, name, findings):
    rel = f"{TRIAL}/{name}.json"
    path = root / rel
    if not path.is_file():
        findings.append(f"{rel}: missing file")
        return None
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, UnicodeDecodeError, ValueError):
        findings.append(f"{rel}: not valid JSON")
        return None
    if not isinstance(data, dict):
        findings.append(f"{rel}: not a JSON object")
        return None
    return data


def aggregates(rows):
    medians = {}
    for arm in sorted({r["arm"] for r in rows}):
        medians[arm] = statistics.median(r["counted_tokens"] for r in rows if r["arm"] == arm)
    out = {"median_counted_tokens": medians}
    if medians.get("baseline") and "wiki" in medians:
        out["delta_pct"] = round((medians["baseline"] - medians["wiki"]) / medians["baseline"] * 100, 2)
    return out


def good_rows(ledger, findings):
    rel = f"{TRIAL}/ledger.json"
    rows = ledger.get("rows")
    if not isinstance(rows, list) or not rows:
        findings.append(f"{rel}: no rows")
        return []
    out = []
    for i, row in enumerate(rows, 1):
        if (not isinstance(row, dict) or any(k not in row for k in ROW_KEYS)
                or not all(is_int(row[k]) for k in INT_KEYS)
                or not isinstance(row["run_key"], str) or not isinstance(row["arm"], str)):
            findings.append(f"{rel}: bad row {i}")
            continue
        out.append(row)
    return out


def check_rows(root, key, rows, findings):
    rel = f"{TRIAL}/ledger.json"
    known = None if key is None else {k.get("run_key") for k in (key.get("keys") or []) if isinstance(k, dict)}
    seen = set()
    for row in rows:
        rk = row["run_key"]
        if rk in seen:
            findings.append(f"{rel}: duplicate row key {rk}")
        seen.add(rk)
        if known is not None and rk not in known:
            findings.append(f"{rel}: unknown row key {rk}")
        if row.get("disabled"):
            findings.append(f"{rel}: disabled row key {rk}")
        if row["counted_tokens"] != row["input_total"] + row["output_total"]:
            findings.append(f"{rel}: counted_tokens rule broken {rk}")
        src = row["source"]
        if (not isinstance(src, str) or not src or Path(src).is_absolute() or ".." in Path(src).parts
                or not (root / src).is_file() or not commits_of(root, src)):
            findings.append(f"{rel}: source not committed {rk}")


def check_recompute(ledger, recompute, rows, findings):
    rel = f"{TRIAL}/recompute.json"
    rrows = recompute.get("rows")
    if not isinstance(rrows, list) or not rrows:
        findings.append(f"{rel}: no rows")
        return
    by_key = {r.get("run_key"): r for r in rrows if isinstance(r, dict)}
    for row in rows:
        r = by_key.get(row["run_key"])
        if r is None:
            findings.append(f"{rel}: recompute missing {row['run_key']}")
            continue
        for field in ("counted_tokens", "misses"):
            if not is_int(r.get(field)) or r[field] != row[field]:
                findings.append(f"{rel}: recompute mismatch {field} {row['run_key']}")
    ledger_keys = {r["run_key"] for r in rows}
    for k in by_key:
        if k not in ledger_keys:
            findings.append(f"{rel}: recompute row without ledger row {k}")
    expected = aggregates(rows)
    for name, doc in (("recompute", recompute), ("ledger", ledger)):
        if doc.get("aggregates") != expected:
            findings.append(f"{TRIAL}/{name}.json: recompute mismatch aggregates")


def show_json(root, rev, rel):
    """The JSON object stored at rev:rel, or None."""
    code, out = git(root, "show", f"{rev}:{rel}")
    try:
        data = json.loads(out) if code == 0 else None
    except ValueError:
        return None
    return data if isinstance(data, dict) else None


CAP_KEYS = ("cap_stage_a", "cap_stage_ab")


def check_amendment(root, first_run, log, t1, findings):
    """Window 237-amendment-ordering: thresholds are judged by content against the file at the first run."""
    rel = f"{TRIAL}/thresholds.json"
    t0 = show_json(root, first_run, rel)
    if t0 is None:
        findings.append(f"{rel}: committed or changed at or after the first run")
        return
    if t1 is None:
        return  # missing or invalid: already a finding
    missing = object()
    for k in sorted(set(t0) | set(t1)):
        if k in CAP_KEYS or (k == "cap_amendment" and k not in t0):
            continue
        a, b = t0.get(k, missing), t1.get(k, missing)
        if a is missing or b is missing or json.dumps(a, sort_keys=True) != json.dumps(b, sort_keys=True):
            findings.append(f"{rel}: committed or changed at or after the first run: amendment changes {k}")
    for k in CAP_KEYS:  # caps only rise, and stay whole numbers
        a, b = t0.get(k), t1.get(k)
        if is_int(a) and not is_int(b):
            findings.append(f"{rel}: committed or changed at or after the first run: amendment {k} is not an integer")
        elif is_int(a) and b < a:
            findings.append(f"{rel}: committed or changed at or after the first run: amendment lowers {k}")


def check_order(root, docs, findings):
    key_rel, led_rel = f"{TRIAL}/miss-key.json", f"{TRIAL}/ledger.json"
    log = {n: commits_of(root, f"{TRIAL}/{n}.json") for n in FILES}
    for n in FILES:
        if not log[n]:
            findings.append(f"{TRIAL}/{n}.json: not committed")
    # first run commit: the earliest commit adding the ledger or any run report under trial/runs/
    code, out = git(root, "log", "--format=%H", "--reverse", "--diff-filter=A", "--", led_rel, f"{TRIAL}/runs")
    first_run = out.split()[0] if code == 0 and out else (log["ledger"][0] if log["ledger"] else None)
    if first_run:
        for n in ("freeze", "miss-key"):  # a later edit would be a forged order
            if log[n] and not is_ancestor(root, log[n][-1], first_run):
                findings.append(f"{TRIAL}/{n}.json: committed or changed at or after the first run")
        if log["thresholds"]:
            check_amendment(root, first_run, log["thresholds"], docs["thresholds"], findings)
    if log["thresholds"] and log["ledger"] and not is_ancestor(root, log["thresholds"][0], log["ledger"][0]):
        findings.append(f"{TRIAL}/thresholds.json: thresholds not committed before the ledger")
    if log["freeze"] and log["miss-key"] and not is_ancestor(root, log["freeze"][-1], log["miss-key"][0]):
        findings.append(f"{key_rel}: key not committed after the freeze")
    if first_run and log["miss-key"] and not is_ancestor(root, log["miss-key"][-1], first_run):
        findings.append(f"{key_rel}: key not committed before the first run")
    key, freeze, ledger = docs["miss-key"], docs["freeze"], docs["ledger"]
    if key is not None:
        author = key.get("author_seat")
        runners = RUNNER_SEATS | {s for s in ((ledger or {}).get("runner_seats") or []) if isinstance(s, str)}
        if not isinstance(author, str) or not author:
            findings.append(f"{key_rel}: missing author_seat")
        elif author in runners:
            findings.append(f"{key_rel}: key author is a runner seat {author}")
        elif freeze is not None and author == freeze.get("author_seat"):
            findings.append(f"{key_rel}: key author also wrote the freeze {author}")
    if docs["recompute"] is not None:
        runners = RUNNER_SEATS | {s for s in ((ledger or {}).get("runner_seats") or []) if isinstance(s, str)}
        if docs["recompute"].get("author_seat") in runners:
            findings.append(f"{TRIAL}/recompute.json: recompute author is a runner seat")


def check_thresholds(th, rows, findings):
    rel = f"{TRIAL}/thresholds.json"
    for k in THRESHOLD_KEYS:
        v = th.get(k)
        if isinstance(v, bool) or not isinstance(v, (int, float)) or v <= 0:
            findings.append(f"{rel}: bad threshold {k}")
    cap_a, cap_ab = th.get("cap_stage_a"), th.get("cap_stage_ab")
    if is_int(cap_a) and is_int(cap_ab):
        if cap_ab < cap_a:
            findings.append(f"{rel}: bad threshold cap_stage_ab below cap_stage_a")
        total = sum(r["counted_tokens"] for r in rows)
        limit = cap_ab if any(r.get("stage") == "B" for r in rows) else cap_a
        if total > limit:
            findings.append(f"{TRIAL}/ledger.json: cap exceeded {total} > {limit}")


def run(root):
    findings = []
    if git(root, "rev-parse", "--git-dir")[0] != 0:
        print(f"{TRIAL}: not a git repository", file=sys.stderr)
        return 1
    docs = {n: load(root, n, findings) for n in FILES}
    ledger, key = docs["ledger"], docs["miss-key"]
    rows = good_rows(ledger, findings) if ledger is not None else []
    if docs["thresholds"] is not None:
        check_thresholds(docs["thresholds"], rows, findings)
    if ledger is not None:
        check_rows(root, key, rows, findings)
    if ledger is not None and docs["recompute"] is not None:
        check_recompute(ledger, docs["recompute"], rows, findings)
    check_order(root, docs, findings)  # ordering is judged on whichever files exist
    if findings:
        for line in findings:
            print(line, file=sys.stderr)
        return 1
    print(f"wiki-trial: OK ({len(docs['ledger']['rows'])} rows)")
    return 0


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", default=".")
    root = Path(parser.parse_args().root)
    if not root.is_dir():
        print("not a directory", file=sys.stderr)
        return 2
    try:
        return run(root.resolve())
    except Exception as exc:  # fail closed, never a traceback
        print(f"{TRIAL}: check could not run {type(exc).__name__}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
