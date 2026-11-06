#!/usr/bin/env python3
"""Card 217 (D157 Criterion 3): same-head run-pair comparison and thread-pool floor decision.

  python scripts/check-run-pairs-217.py [--root DIR]

Reads .mob/records/217/run-pairs.json under DIR (default: the repository root) and FAILS unless, for each environment
('local' and 'ci'), two consecutive full runs at one head give IDENTICAL per-test outcome lists, the only failures
are allowed ones with an open owner card, and the floor decision is recorded and consistent with the code.
The contract (record shape and rules) is the docstring of tests/test_run_pairs_217.py.

Exit 0 clean ("run-pairs: OK"), 1 findings ("<path>: <keyword> <detail>" on stderr), 2 DIR is not a directory.
Keywords: missing, malformed, pair, differ, allowed, owner, head, empty, temp, ci, floor. Never a traceback.
Writes nothing. Paths in the record are relative to DIR.

An outcome file is either the flat {test: outcome} object or the extractor form {"source": {...}, "outcomes": {...}}
(scripts/trx-outcomes-217.py, scripts/ci-log-outcomes-217.py); when the extractor form names its source file or
GitHub run id, it must equal the run's id in the record. An owner card is open when board/<n>-*.md exists under DIR
(checked only when DIR has a board folder) and its Status line does not start with Done, Closed or Withdrawn.
"""

import json
import re
import sys
from datetime import datetime
from pathlib import Path

REL = ".mob/records/217/run-pairs.json"
FLOOR = "tests/Phrase.Acceptance.Tests/Support/ThreadPoolFloor.cs"
SHA = re.compile(r"^[0-9a-f]{40}$")
CLOSED = ("done", "closed", "withdrawn")


class Findings(list):
    def add(self, where, keyword, detail):
        self.append(f"{where}: {keyword} {detail}")


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def is_int(v):
    return isinstance(v, int) and not isinstance(v, bool)


def owner_open(root, n):
    board = root / "board"
    if not board.is_dir():
        return True
    files = sorted(board.glob(f"{n}-*.md"))
    if not files:
        return False
    for line in files[0].read_text(encoding="utf-8", errors="replace").splitlines():
        m = re.match(r"^- \*\*Status:\*\*\s*(.*)$", line)
        if m:
            return not m.group(1).strip().lower().startswith(CLOSED)
    return True


def load_outcomes(root, rel, where, f):
    """The {test: outcome} dict of one run, or None after adding a finding."""
    if not isinstance(rel, str) or not rel.strip():
        f.add(where, "malformed", "run has no outcomes path")
        return None, None
    p = (root / rel).resolve()
    try:
        p.relative_to(root.resolve())
    except ValueError:
        f.add(where, "malformed", f"outcomes path {rel} leaves the root")
        return None, None
    if not p.is_file():
        f.add(where, "missing", f"outcome file {rel}")
        return None, None
    try:
        data = read_json(p)
    except (ValueError, OSError) as e:
        f.add(where, "malformed", f"outcome file {rel} is not JSON ({e})")
        return None, None
    source = None
    if isinstance(data, dict) and set(data) == {"source", "outcomes"}:
        source, data = data["source"], data["outcomes"]
    if not isinstance(data, dict) or not all(isinstance(k, str) and isinstance(v, str) and v for k, v in data.items()):
        f.add(where, "malformed", f"outcome file {rel} is not an object of test name to outcome text")
        return None, None
    if source is not None and not isinstance(source, dict):
        f.add(where, "malformed", f"outcome file {rel} has a source that is not an object")
        return None, None
    return data, source


def check_run(root, env, n, run, record_head, f, heads):
    where = f"{REL} {env} run {n}"
    if not isinstance(run, dict):
        f.add(where, "malformed", "run is not an object")
        return None
    head = run.get("head")
    if not isinstance(head, str) or not SHA.match(head):
        f.add(where, "head", f"run head is not a 40-hex sha: {head!r}")
    else:
        heads.add(head)
        if head != record_head:
            f.add(where, "head", f"run head {head} differs from the record head {record_head}")
    ident = run.get("id")
    if not isinstance(ident, dict):
        f.add(where, "ci" if env == "ci" else "malformed", "run has no id object")
        ident = {}
    if env == "local":
        if not isinstance(ident.get("trx"), str) or not ident["trx"].strip():
            f.add(where, "malformed", "local run id has no trx file name")
        ts = ident.get("timestamp")
        try:
            datetime.fromisoformat(ts.replace("Z", "+00:00"))
        except (AttributeError, ValueError):
            f.add(where, "malformed", f"local run id has no ISO timestamp: {ts!r}")
        for key in ("temp", "tmp"):
            if not isinstance(run.get(key), str) or not run[key].strip():
                f.add(where, "temp", f"local run does not name {key.upper()}")
    else:
        rid = ident.get("github_run_id")
        if not isinstance(rid, (str, int)) or isinstance(rid, bool) or not str(rid).strip():
            f.add(where, "ci", "ci run id has no github_run_id")
        sha = ident.get("head_sha")
        if not isinstance(sha, str) or not sha:
            f.add(where, "ci", "ci run is not tied to a head (id.head_sha missing)")
        elif sha != record_head:
            f.add(where, "head", f"ci head_sha {sha} differs from the record head {record_head}")
    data, source = load_outcomes(root, run.get("outcomes"), where, f)
    if source:
        pairs = ((source.get("file"), ident.get("trx")),) if env == "local" else \
            ((source.get("github_run_id"), ident.get("github_run_id")),)
        for have, want in pairs:
            if have is not None and want is not None and str(have) != str(want):
                f.add(where, "malformed", f"outcome file source {have!r} is not the run id {want!r}")
    return data


def check(root):
    f = Findings()
    rec_path = root / REL
    if not rec_path.is_file():
        f.add(REL, "missing", "run-pairs record does not exist")
        return f
    try:
        rec = read_json(rec_path)
    except (ValueError, OSError) as e:
        f.add(REL, "malformed", f"not JSON ({e})")
        return f
    if not isinstance(rec, dict):
        f.add(REL, "malformed", "record is not an object")
        return f
    if rec.get("card") != 217 or not is_int(rec.get("card")):
        f.add(REL, "malformed", "card is not 217")
    head = rec.get("head")
    if not isinstance(head, str) or not SHA.match(head):
        f.add(REL, "head", f"record head is not a 40-hex sha: {head!r}")
    min_tests = rec.get("min_tests")
    if not is_int(min_tests) or min_tests < 1:
        f.add(REL, "malformed", "min_tests is not an integer of at least 1")
        min_tests = 1
    envs = rec.get("environments")
    if not isinstance(envs, dict):
        f.add(REL, "malformed", "environments is not an object")
        envs = {}
    for env in ("local", "ci"):
        if env not in envs:
            f.add(REL, "ci" if env == "ci" else "malformed", f"environment {env} is missing from the record")
            continue
        e = envs[env]
        where = f"{REL} {env}"
        if not isinstance(e, dict):
            f.add(where, "malformed", "environment is not an object")
            continue
        allowed, ok = {}, True
        al = e.get("allowed_failures")
        if not isinstance(al, list):
            f.add(where, "malformed", "allowed_failures is not a list")
            al = []
        for a in al:
            if not isinstance(a, dict) or not isinstance(a.get("test"), str) or not a["test"]:
                f.add(where, "malformed", f"allowed failure without a test name: {a!r}")
                continue
            if not is_int(a.get("owner_card")):
                f.add(where, "owner", f"allowed failure {a['test']} has no integer owner_card")
                continue
            if not owner_open(root, a["owner_card"]):
                f.add(where, "owner", f"owner card {a['owner_card']} of {a['test']} is not open")
            allowed[a["test"]] = a["owner_card"]
        runs = e.get("runs")
        if not isinstance(runs, list) or len(runs) != 2:
            f.add(where, "pair", f"{env} needs exactly two runs, found "
                                 f"{len(runs) if isinstance(runs, list) else 'none'}")
            continue
        heads = set()
        lists = [check_run(root, env, i, r, head, f, heads) for i, r in enumerate(runs, 1)]
        if len(heads) > 1:
            f.add(where, "head", f"the two runs are at different heads: {sorted(heads)}")
        for i, data in enumerate(lists, 1):
            if data is None:
                continue
            if not data or len(data) < min_tests:
                f.add(where, "empty", f"run {i} lists {len(data)} tests, fewer than min_tests {min_tests}")
            for name, outcome in sorted(data.items()):
                if outcome != "Passed" and name not in allowed:
                    f.add(where, "allowed", f"run {i}: {name} is {outcome} and is not an allowed failure with an owner")
        if lists[0] is not None and lists[1] is not None:
            a, b = lists
            for name in sorted(set(a) | set(b)):
                if name not in a or name not in b:
                    f.add(where, "differ", f"{name} is listed in only run {1 if name in a else 2}")
                elif a[name] != b[name]:
                    f.add(where, "differ", f"{name} is {a[name]} in run 1 and {b[name]} in run 2")
    fd = rec.get("floor_decision")
    floor_file = root / FLOOR
    floor_text = floor_file.read_text(encoding="utf-8", errors="replace") if floor_file.is_file() else None
    if not isinstance(fd, dict) or fd.get("decision") not in ("removed", "kept"):
        f.add(REL, "floor", "floor_decision is missing or its decision is not 'removed' or 'kept'")
    else:
        reason = fd.get("reason")
        if not isinstance(reason, str) or not reason.strip():
            f.add(REL, "floor", f"floor_decision {fd['decision']} has no reason")
        has_floor = floor_text is not None and "ModuleInitializer" in floor_text
        if fd["decision"] == "removed" and has_floor:
            f.add(REL, "floor", f"floor is recorded as removed but {FLOOR} still exists with a ModuleInitializer")
        if fd["decision"] == "kept" and not has_floor:
            f.add(REL, "floor", f"floor is recorded as kept but {FLOOR} has no ModuleInitializer")
    return f


def main(argv):
    args = argv[1:]
    root = Path(__file__).resolve().parent.parent
    if args:
        if len(args) != 2 or args[0] != "--root":
            print("usage: check-run-pairs-217.py [--root DIR]", file=sys.stderr)
            return 2
        root = Path(args[1])
    if not root.is_dir():
        print(f"{root}: not a directory", file=sys.stderr)
        return 2
    try:
        findings = check(root)
    except Exception as e:  # never a traceback
        findings = [f"{REL}: malformed unexpected {type(e).__name__}: {e}"]
    if findings:
        print("\n".join(findings), file=sys.stderr)
        return 1
    print("run-pairs: OK")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
