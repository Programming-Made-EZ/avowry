#!/usr/bin/env python3
"""Stop-the-line check: is the CI of the current head commit on main green?

Usage: python scripts/check-ci-main.py [--wait SECONDS] [--branch main]
Green only when the newest ci run (rerun attempt) for the head SHA succeeded and every run for that SHA completed; queued, in-progress
or missing ci runs are pending (exit 0, --wait keeps polling). Exit 0 for green/pending,
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


FIELDS = "databaseId,conclusion,status,headSha,workflowName,url,createdAt"


def latest_runs(branch, runner, head):
    """All runs for one commit. A head is mandatory: there is no headless query."""
    return runner(["run", "list", "--branch", branch, "--commit", head, "--limit", "50", "--json", FIELDS])


def resolve_head(branch, runner=gh_json):
    """Current head SHA of the branch via gh. RuntimeError when the head commit cannot be read."""
    commit = runner(["api", f"repos/{{owner}}/{{repo}}/commits/{branch}"])
    if not isinstance(commit, dict):
        raise RuntimeError("head commit unreadable (gh api did not return a commit object)")
    sha = commit.get("sha")
    if not sha:
        raise RuntimeError("head commit has no sha")
    return sha


def head_verdict(runs, head):
    """Verdict for one commit: ci must be completed+success and every run for the commit completed."""
    mine = [r for r in runs if r.get("headSha") == head]
    # Several ci attempts for one SHA (a rerun): only the NEWEST attempt counts (createdAt, then run id, highest first;
    # a stable sort keeps gh's newest-first order when neither is present). Older ci attempts are superseded.
    ci = sorted((r for r in mine if r.get("workflowName") == "ci"),
                key=lambda r: (r.get("createdAt") or "", r.get("databaseId") or 0), reverse=True)
    mine = [r for r in mine if r.get("workflowName") != "ci"] + ci[:1]
    for r in mine:
        if r.get("status") == "completed" and (r.get("conclusion") in RED or r.get("conclusion") == "cancelled"):
            return "red", r
    for r in mine:
        if r.get("status") != "completed":
            return "pending", r
    if not ci:
        return "pending", None
    if ci[0].get("conclusion") == "success":
        return "green", ci[0]
    return "unknown", ci[0]


def verdict(runs, head=None):
    """('red'|'green'|'pending'|'unknown', run). With head: judged for that commit only. Without: runs newest first.

    The headless form is kept only for direct calls by the old tests; main never uses it."""
    if head:
        return head_verdict(runs, head)
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


def main(argv=None, runner=gh_json, sleep=time.sleep, head=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--branch", default="main")
    parser.add_argument("--wait", type=int, default=0, help="seconds to wait for an in-progress run (bounded)")
    args = parser.parse_args(argv)
    deadline = time.time() + args.wait
    while True:
        try:
            sha_head = (head or (lambda b: resolve_head(b, runner)))(args.branch)
            if not isinstance(sha_head, str) or not sha_head:
                raise RuntimeError("head commit unreadable (empty or non-string sha)")
            runs = latest_runs(args.branch, runner, sha_head)
            state, run = verdict(runs, sha_head)
        except (RuntimeError, ValueError) as exc:
            print(f"CI status unreadable: {exc}")
            return 2
        waiting = state == "pending"
        if not waiting or time.time() >= deadline:
            break
        sleep(30)
    sha = sha_head[:7]
    if state == "unknown":
        print(f"CI status unreadable: no usable verdict for {args.branch} at {sha} (ci run concluded {(run or {}).get('conclusion') or 'n/a'}, neither success nor failure)")
        return 2
    if state == "red":
        print(f"STOP THE LINE: CI on {args.branch} is red at {sha} ({run.get('workflowName')}): {run.get('url')}")
        return 1
    print(f"CI on {args.branch}: {state}" + (f" at {sha}" if sha else ""))
    return 0


if __name__ == "__main__":
    sys.exit(main())
