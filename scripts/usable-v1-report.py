#!/usr/bin/env python3
"""Usable v1 milestone report for CI.

Runs the six per-bar tests (Milestone=UsableV1) and summarizes them. All six reached bars are ratcheted, so the job goes
RED when any criterion regresses or when the report itself cannot be produced. A green run prints USABLE v1 REACHED.

  python3 scripts/usable-v1-report.py              run the tests and report
  python3 scripts/usable-v1-report.py --from-trx F  report from an existing TRX file (no test run)
  python3 scripts/usable-v1-report.py --self-test   check the classification logic
"""

import json
import os
import re
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
RATCHET = ROOT / "board" / "usable-v1-ratchet.json"
BARS = {
    1: "A real app written entirely in .phrase (demo/tasks/Tasks.phrase)",
    2: "One CLI command compiles it to C#/FastEndpoints",
    3: "dotnet build of the generated projects has zero warnings",
    4: "The generated xUnit specs all pass, one test per spec",
    5: "The app runs and live HTTP matches the specs",
    6: "A model that never saw the app adds an endpoint from the docs alone",
}


def parse_trx(path):
    """{bar number: (met, message)} from a TRX file."""
    ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    results = {}
    for r in ET.parse(path).getroot().iter("{%s}UnitTestResult" % ns["t"]):
        m = re.search(r"(?:^|\.)Bar(\d)_", r.get("testName", ""))
        if not m:
            continue
        message = r.findtext("t:Output/t:ErrorInfo/t:Message", default="", namespaces=ns).strip()
        results[int(m.group(1))] = (r.get("outcome") == "Passed", message)
    return results


def classify(results, ratchet):
    """(met bars, regressed ratcheted bars, missing bars)."""
    met = sorted(b for b, (ok, _) in results.items() if ok)
    regressed = sorted(b for b in ratchet if not results.get(b, (False, ""))[0])
    missing = sorted(set(BARS) - set(results))
    return met, regressed, missing


def load_ratchet():
    """The ratcheted bars; a missing or unreadable ratchet file is an error, never an empty ratchet."""
    return [int(b) for b in json.loads(RATCHET.read_text(encoding="utf-8")).get("must_pass", [])]


def reached_line(root=None):
    """Milestone line. Point at demo/tasks/README.md only when that file exists."""
    base = Path(root) if root is not None else ROOT
    if (base / "demo" / "tasks" / "README.md").is_file():
        return "USABLE v1 REACHED: demo app at demo/tasks (see demo/tasks/README.md for how to run it)"
    return "USABLE v1 REACHED: demo app at demo/tasks"


def report(results):
    try:
        ratchet = load_ratchet()
    except (OSError, ValueError) as exc:
        print(f"::error::Usable v1 ratchet unreadable ({RATCHET.name}): {exc}")
        return 1
    if sorted(ratchet) != sorted(BARS):
        print(f"::error::Usable v1 ratchet must list every bar {sorted(BARS)}, found {sorted(ratchet)}")
        return 1
    met, regressed, missing = classify(results, ratchet)
    lines = [f"## Usable v1: {len(met)} of {len(BARS)} criteria met", ""]
    for bar, title in BARS.items():
        ok, message = results.get(bar, (False, "no result"))
        lines.append(f"- {'MET' if ok else 'not met'}: bar {bar}, {title}" + ("" if ok else f" ({message.splitlines()[0][:160] if message else 'no detail'})"))
    summary = "\n".join(lines)
    print(summary)
    target = os.environ.get("GITHUB_STEP_SUMMARY")
    if target:
        Path(target).open("a", encoding="utf-8").write(summary + "\n")
    if missing:
        print(f"::error::Usable v1 report incomplete: no result for bar(s) {missing}")
        return 1
    if regressed:
        print(f"::error::Usable v1 REGRESSION: bar(s) {regressed} previously passed (board/usable-v1-ratchet.json) and now fail")
        return 1
    if len(met) == len(BARS):
        print(reached_line())
        print("::notice::USABLE v1 REACHED")
        return 0
    print(f"::error::Usable v1 not reached: {len(met)} of {len(BARS)} criteria")
    return 1


def self_test():
    ok = {b: (True, "") for b in BARS}
    none = {b: (False, "x") for b in BARS}
    assert classify(none, []) == ([], [], [])
    assert classify(ok, [1, 2])[0] == [1, 2, 3, 4, 5, 6]
    partial = dict(none); partial[1] = (True, "")
    assert classify(partial, [1]) == ([1], [], [])
    assert classify(none, [1])[1] == [1], "a ratcheted bar that fails is a regression"
    assert classify({1: (True, "")}, [])[2] == [2, 3, 4, 5, 6], "missing bars are reported"
    assert report(ok) == 0, "six passing bars reach Usable v1"
    assert report(none) == 1, "a failing bar fails the job now that every bar is ratcheted"
    with tempfile.TemporaryDirectory() as tmp:
        missing = Path(tmp)
        assert reached_line(missing) == "USABLE v1 REACHED: demo app at demo/tasks"
        (missing / "demo" / "tasks").mkdir(parents=True)
        (missing / "demo" / "tasks" / "README.md").write_text("# demo\n", encoding="utf-8")
        assert reached_line(missing) == (
            "USABLE v1 REACHED: demo app at demo/tasks (see demo/tasks/README.md for how to run it)"
        )
    print("self-test ok")
    return 0


def main(argv):
    if "--self-test" in argv:
        return self_test()
    if "--from-trx" in argv:
        return report(parse_trx(argv[argv.index("--from-trx") + 1]))
    out = tempfile.mkdtemp(prefix="usable-v1-")
    cmd = ["dotnet", "test", str(ROOT / "tests" / "Phrase.Acceptance.Tests"), "-c", "Release", "--nologo",
           "--filter", "Milestone=UsableV1", "--logger", "trx;LogFileName=usable-v1.trx", "--results-directory", out]
    proc = subprocess.run(cmd, cwd=str(ROOT), capture_output=True, text=True)
    trx = Path(out) / "usable-v1.trx"
    if not trx.exists():
        print(proc.stdout[-2000:])
        print(proc.stderr[-1000:])
        print("::error::Usable v1 report could not be produced: the tests did not run (build failure?)")
        return 1
    return report(parse_trx(trx))


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
