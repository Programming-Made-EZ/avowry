#!/usr/bin/env python3
"""Stop-the-line check: is the latest finished CI run on main green?

Usage: python scripts/check-ci-main.py [--wait SECONDS] [--branch main]
Exit 0 when the latest finished run succeeded (a run still in progress is reported, not failed),
exit 1 when it failed (STOP THE LINE: fix main before any other card), exit 2 when GitHub cannot be read.
The loop runs this after every push; a red result is never ignored.
"""

import argparse
import json
import subprocess
import sys
import time

RED = {"failure", "timed_out", "startup_failure", "action_required"}


def gh_json(args):
    proc = subprocess.run(["gh", *args], capture_output=True, text=True, encoding="utf-8")
    if proc.returncode:
        raise RuntimeError(proc.stderr.strip() or "gh failed")
    return json.loads(proc.stdout)


def latest_runs(branch, runner=gh_json):
    return runner(["run", "list", "--branch", branch, "--limit", "10", "--json", "databaseId,conclusion,status,headSha,workflowName,url"])


def verdict(runs):
    """('red'|'green'|'pending'|'unknown', run) from runs ordered newest first."""
    if not runs:
        return "unknown", None
    # A newest run that ended cancelled has no verdict (a timeout cancels a job): treat it as red, never as green.
    # Older cancelled runs are superseded pushes and are skipped.
    if runs[0].get("status") == "completed" and runs[0].get("conclusion") == "cancelled":
        return "red", runs[0]
    finished = [r for r in runs if r.get("status") == "completed" and r.get("conclusion") != "cancelled"]
    if finished and finished[0].get("conclusion") in RED:
        return "red", finished[0]
    if runs[0].get("status") != "completed":
        return "pending", runs[0]
    if finished and finished[0].get("conclusion") == "success":
        return "green", finished[0]
    return "unknown", runs[0]


def main(argv=None, runner=gh_json, sleep=time.sleep):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--branch", default="main")
    parser.add_argument("--wait", type=int, default=0, help="seconds to wait for an in-progress run (bounded)")
    args = parser.parse_args(argv)
    deadline = time.time() + args.wait
    while True:
        try:
            runs = latest_runs(args.branch, runner)
            state, run = verdict(runs)
        except (RuntimeError, ValueError) as exc:
            print(f"CI status unreadable: {exc}")
            return 2
        newest_running = bool(runs) and runs[0].get("status") != "completed"
        if not newest_running or time.time() >= deadline:
            break
        sleep(30)
    sha = (run or {}).get("headSha", "")[:7]
    if state == "red":
        print(f"STOP THE LINE: CI on {args.branch} is red at {sha} ({run.get('workflowName')}): {run.get('url')}")
        return 1
    print(f"CI on {args.branch}: {state}" + (f" at {sha}" if sha else ""))
    return 0


if __name__ == "__main__":
    sys.exit(main())
