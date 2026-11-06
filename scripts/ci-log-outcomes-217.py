#!/usr/bin/env python3
"""Card 217: derive the Phrase.Acceptance.Tests per-test outcome list from the console log of the CI Test step.

  python scripts/ci-log-outcomes-217.py LOG [--job NAME] [--meta key=value]... [--out OUT.json]

LOG is the saved output of `dotnet test Phrase.sln ... --logger "console;verbosity=normal"` (for example
`gh run view <id> --repo <repo> --log`, which prefixes each line with "job<TAB>step<TAB>timestamp"; the log is
never saved in the repo). Real shape (run 42079881523): the Compiler and Acceptance projects run concurrently and
their result lines interleave, each project ends with the legacy summary block
  Test Run Successful.   (or: Test Run Failed.)
  Total tests: N
       Passed: N         (Failed: N and Skipped: N when not zero)
(the older "Passed!/Failed! - Failed: N, Passed: N, Skipped: N, Total: N" line is understood too).

A result line is exactly two spaces, Passed|Failed|Skipped, the test name (theory arguments stay in the name, in
parentheses) and an optional "[time]". Only names starting with Phrase.Acceptance.Tests. are kept. The count of
every project's result lines must match the summaries (the Acceptance count and its passed/failed/skipped split
must equal one summary block exactly, the other projects' counts the remaining blocks), so a log that was cut,
double-printed or mixed is refused. With several jobs in the log, --job is required.

The xUnit adapter truncates a long theory argument (name ends with the three characters U+00B7 then ")"), so
several lines can share one such name: they are merged into one entry (the common outcome, or "Mixed(...)") and
source.collapsed_truncated_names says how many lines each merged name stands for; "count" is the number of lines.
Any other duplicate name is refused.

Emits {"source": {"kind": "ci-console", "file", "sha256", "count", "job", "summary", ...--meta}, "outcomes": {...}}.
Exit 0 ok, 1 refused ("<path>: <keyword> <detail>": missing, malformed, duplicate, count, empty, job), 2 usage.
"""

import hashlib
import json
import re
import sys
from pathlib import Path

PROJECT = "Phrase.Acceptance.Tests."
ANSI = re.compile(r"\x1b\[[0-9;]*[A-Za-z]")
GH = re.compile(r"^([^\t]*)\t([^\t]*)\t(.*)$")
STAMP = re.compile(r"^\d{4}-\d\d-\d\dT[\d:.]+Z ?")
RESULT = re.compile(r"^  (Passed|Failed|Skipped) (.*)$")
TIME = re.compile(r" \[(?:< )?\d+(?:[.,]\d+)? ?(?:ms|s|m|h)(?: \d+ ?(?:ms|s|m))*\]$")
NAME = re.compile(r"^[A-Za-z_][\w.]*(?:\(.*\))?$")
BANG = re.compile(r"^(?:Passed|Failed)!\s+- Failed:\s*(\d+), Passed:\s*(\d+), Skipped:\s*(\d+), Total:\s*(\d+)")
KEYS = {"Total tests": "total", "Passed": "passed", "Failed": "failed", "Skipped": "skipped"}
SUMMARY_LINE = re.compile(r"^\s+(Total tests|Passed|Failed|Skipped): (\d+)\s*$|^(Total tests): (\d+)\s*$")


TRUNCATED = "···)"  # the xUnit adapter cuts a long theory argument and ends the name with "···)"


class Rejected(Exception):
    pass


def merge(outcomes):
    if len(set(outcomes)) == 1:
        return outcomes[0]
    tally = {}
    for o in outcomes:
        tally[o] = tally.get(o, 0) + 1
    return "Mixed(" + ",".join(f"{k}={tally[k]}" for k in sorted(tally)) + ")"


def project_of(name):
    m = re.match(r"^(Phrase\.[A-Za-z0-9_]+\.Tests)\.", name)
    return m.group(1) if m else name.split(".")[0]


def extract_text(raw, file_name, job=None, meta=None):
    fail = lambda kw, msg: Rejected(f"{file_name}: {kw} {msg}")
    try:
        text = raw.decode("utf-8-sig").replace("﻿", "")
    except UnicodeDecodeError as e:
        raise fail("malformed", f"not UTF-8 ({e})")
    jobs, kept = [], []
    for line in text.splitlines():
        m = GH.match(line)
        if m:
            line_job, line = m.group(1), m.group(3)
            if job is not None and line_job != job:
                continue
        else:
            line_job = None
        kept.append((line_job, ANSI.sub("", line)))
    results, summaries, cur = [], [], None  # results: (job, outcome, name, line number)
    for n, (line_job, line) in enumerate(kept, 1):
        line = STAMP.sub("", line, count=1)
        m = RESULT.match(line)
        if m:
            outcome, rest = m.group(1), TIME.sub("", m.group(2).rstrip())
            if not NAME.match(rest):
                raise fail("malformed", f"line {n}: not a test name: {rest!r}")
            results.append((line_job, outcome, rest, n))
            continue
        b = BANG.match(line)
        if b:
            summaries.append((line_job, {"failed": int(b.group(1)), "passed": int(b.group(2)),
                                         "skipped": int(b.group(3)), "total": int(b.group(4))}))
            continue
        if line.startswith("Test Run "):
            cur = {"total": None, "passed": 0, "failed": 0, "skipped": 0}
            summaries.append((line_job, cur))
            continue
        s = SUMMARY_LINE.match(line)
        if s and cur is not None:
            key = s.group(1) or s.group(3)
            cur[KEYS[key]] = int(s.group(2) or s.group(4))
    if job is not None and not results and not any(j == job for j, _ in kept):
        raise fail("job", f"no lines of job {job!r} in the log")
    if job is None:
        names = sorted({j for j, *_ in results if j is not None})
        if len(names) > 1:
            raise fail("job", f"several jobs have result lines ({', '.join(names)}); pass --job")
        job = names[0] if names else None
    if any(s["total"] is None for _, s in summaries):
        raise fail("malformed", "a 'Test Run' summary block has no 'Total tests' line")
    groups, per_project, acc_lines, collapsed = {}, {}, [], {}
    for _, outcome, name, n in results:
        proj = project_of(name)
        seen = groups.setdefault(name, []) if proj == PROJECT[:-1] else per_project.setdefault(proj, {}).setdefault(name, [])
        if seen and not name.endswith(TRUNCATED):
            raise fail("duplicate", f"test name {name} (line {n})")
        seen.append(outcome)
        if proj == PROJECT[:-1]:
            acc_lines.append(outcome)
    outcomes = {}
    for name, group in groups.items():
        outcomes[name] = merge(group)
        if len(group) > 1:
            collapsed[name] = len(group)
    if not outcomes:
        raise fail("empty", f"no {PROJECT[:-1]} result lines")
    acc = {k: sum(1 for o in acc_lines if o == v) for k, v in
           (("passed", "Passed"), ("failed", "Failed"), ("skipped", "Skipped"))}
    acc["total"] = len(acc_lines)
    sums = [s for _, s in summaries]
    match = next((s for s in sums if all(s[k] == acc[k] for k in acc)), None)
    if match is None:
        raise fail("count", f"{acc['total']} Acceptance result lines ({acc['passed']} passed, {acc['failed']} failed, "
                            f"{acc['skipped']} skipped) match no summary block (totals found: "
                            f"{[s['total'] for s in sums]})")
    rest = list(sums)
    rest.remove(match)
    if sorted(s["total"] for s in rest) != sorted(sum(len(g) for g in v.values()) for v in per_project.values()):
        raise fail("count", f"other projects have {sorted(sum(len(g) for g in v.values()) for v in per_project.values())} result lines but the "
                            f"remaining summary blocks say {sorted(s['total'] for s in rest)}")
    source = {"kind": "ci-console", "file": file_name, "sha256": hashlib.sha256(raw).hexdigest(),
              "count": len(acc_lines), "job": job, "summary": {k: match[k] for k in ("total", "passed", "failed", "skipped")}}
    if collapsed:
        source["collapsed_truncated_names"] = collapsed
    source.update(meta or {})
    return {"source": source, "outcomes": dict(sorted(outcomes.items()))}


def extract(path, job=None, meta=None):
    path = Path(path)
    if not path.is_file():
        raise Rejected(f"{path}: missing no such file")
    return extract_text(path.read_bytes(), path.name, job, meta)


def main(argv):
    usage = "usage: ci-log-outcomes-217.py LOG [--job NAME] [--meta key=value]... [--out OUT.json]"
    args, job, out, meta = argv[1:], None, None, {}
    pos = []
    i = 0
    while i < len(args):
        a = args[i]
        if a in ("--job", "--out", "--meta") and i + 1 < len(args):
            v = args[i + 1]
            if a == "--job":
                job = v
            elif a == "--out":
                out = v
            else:
                if "=" not in v:
                    print(usage, file=sys.stderr)
                    return 2
                meta[v.split("=", 1)[0]] = v.split("=", 1)[1]
            i += 2
        elif a.startswith("--"):
            print(usage, file=sys.stderr)
            return 2
        else:
            pos.append(a)
            i += 1
    if len(pos) != 1:
        print(usage, file=sys.stderr)
        return 2
    try:
        data = extract(pos[0], job, meta)
    except Rejected as e:
        print(e, file=sys.stderr)
        return 1
    text = json.dumps(data, indent=2, ensure_ascii=False) + "\n"
    if out:
        try:
            with open(out, "w", encoding="utf-8", newline="\n") as f:
                f.write(text)
        except OSError as e:
            print(f"{out}: cannot write ({e})", file=sys.stderr)
            return 2
    else:
        sys.stdout.buffer.write(text.encode("utf-8"))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
