#!/usr/bin/env python3
"""Card 217: derive a per-test outcome list from a TRX file.

  python scripts/trx-outcomes-217.py FILE.trx [--out OUT.json]

Reads UnitTestResult testName/outcome in the namespace
http://microsoft.com/schemas/VisualStudio/TeamTest/2010 and emits
  {"source": {"kind": "trx", "file": <name>, "sha256": <hex of the file bytes>, "count": <n>,
              "run_start": ..., "run_finish": ...},
   "outcomes": {<fully qualified test name>: <outcome>}}
as JSON (LF line endings). Refuses (exit 1, "<path>: <keyword> <detail>" on stderr, never a traceback): a missing
file, a malformed file, a result without a name, a missing or empty outcome, a duplicate name, an empty list, or a
ResultSummary/Counters total that differs from the number of results. "count" is the number of results. The
xUnit adapter truncates a long theory argument (name ends with the three characters U+00B7 then ")"), so several
results can share one such name: they are merged into one entry (the common outcome, or "Mixed(Failed=1,Passed=13)"),
and source.collapsed_truncated_names says how many results each merged name stands for. Any other duplicate is refused. Exit 2 on a usage error.
"""

import hashlib
import json
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

NS = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"


TRUNCATED = "···)"  # the xUnit adapter cuts a long theory argument and ends the name with "···)"


class Rejected(Exception):
    pass


def merge(name, outcomes):
    """One outcome for a name that several results share (only a truncated theory name may be shared)."""
    if len(set(outcomes)) == 1:
        return outcomes[0]
    tally = {}
    for o in outcomes:
        tally[o] = tally.get(o, 0) + 1
    return "Mixed(" + ",".join(f"{k}={tally[k]}" for k in sorted(tally)) + ")"


def extract(path):
    path = Path(path)
    if not path.is_file():
        raise Rejected(f"{path}: missing no such file")
    raw = path.read_bytes()
    try:
        root = ET.fromstring(raw)
    except ET.ParseError as e:
        raise Rejected(f"{path}: malformed not XML ({e})")
    if root.tag != f"{{{NS}}}TestRun":
        raise Rejected(f"{path}: malformed root is not {{{NS}}}TestRun")
    results = root.findall(f"{{{NS}}}Results/{{{NS}}}UnitTestResult")
    seen, outcomes, collapsed = {}, {}, {}
    for i, r in enumerate(results, 1):
        name = (r.get("testName") or "").strip()
        if not name:
            raise Rejected(f"{path}: malformed result {i} has no testName")
        outcome = (r.get("outcome") or "").strip()
        if not outcome:
            raise Rejected(f"{path}: outcome missing for {name}")
        if name in seen and not name.endswith(TRUNCATED):
            raise Rejected(f"{path}: duplicate test name {name}")
        seen.setdefault(name, []).append(outcome)
    for name, group in seen.items():
        outcomes[name] = merge(name, group)
        if len(group) > 1:
            collapsed[name] = len(group)
    if not outcomes:
        raise Rejected(f"{path}: empty no UnitTestResult in the file")
    counters = root.find(f"{{{NS}}}ResultSummary/{{{NS}}}Counters")
    if counters is not None and counters.get("total") is not None:
        try:
            total = int(counters.get("total"))
        except ValueError:
            raise Rejected(f"{path}: malformed Counters total is not a number")
        if total != len(results):
            raise Rejected(f"{path}: count Counters total {total} differs from {len(results)} results")
    times = root.find(f"{{{NS}}}Times")
    source = {"kind": "trx", "file": path.name, "sha256": hashlib.sha256(raw).hexdigest(), "count": len(results),
              "run_start": times.get("start") if times is not None else None,
              "run_finish": times.get("finish") if times is not None else None}
    if collapsed:
        source["collapsed_truncated_names"] = collapsed
    return {"source": source, "outcomes": dict(sorted(outcomes.items()))}


def main(argv):
    args = argv[1:]
    out = None
    if "--out" in args:
        i = args.index("--out")
        if i + 1 >= len(args):
            print("usage: trx-outcomes-217.py FILE.trx [--out OUT.json]", file=sys.stderr)
            return 2
        out = args[i + 1]
        del args[i:i + 2]
    if len(args) != 1:
        print("usage: trx-outcomes-217.py FILE.trx [--out OUT.json]", file=sys.stderr)
        return 2
    try:
        data = extract(args[0])
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
