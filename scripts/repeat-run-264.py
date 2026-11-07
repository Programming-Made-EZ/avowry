#!/usr/bin/env python3
"""Card 264 criterion 1: run PhraseExecutedOutcomesAcceptance N times under load and write the evidence record.

Builds the acceptance project once, then for each run starts two `dotnet test --no-build` processes at the same
time (the class under test, and a load class that also starts `dotnet run` children so Avowry.Bench builds can
overlap). Each process has an outer timeout boundary; past it only that child's own process tree is killed (by
PID). Nothing is retried: a failure is recorded as it happened and the loop continues to the last run.

  python scripts/repeat-run-264.py --out .mob/records/264/repeat-evidence.json --scratch DIR [--runs 20]
Writes the record (checked by scripts/check-repeat-evidence.py) and one TRX file per process under --scratch.
Stdlib only.
"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
import threading
import time
import xml.etree.ElementTree as ET
from pathlib import Path

PROJECT = "tests/Avowry.Acceptance.Tests"
CLASS = "PhraseExecutedOutcomesAcceptance"
LOAD_CLASS = "BenchmarkHarnessAcceptance"
BOUNDARY = 600
TEAM = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"


def test_command(cls, results_dir, trx):
    return ["dotnet", "test", PROJECT, "--no-build", "--filter", f"FullyQualifiedName~{cls}",
            "--results-directory", str(results_dir), "--logger", f"trx;LogFileName={trx}"]


def display(cmd):
    """The command as a person would type it (without the per-run result-logging flags)."""
    return " ".join(cmd[:cmd.index("--results-directory")])


def start(cmd, cwd):
    return subprocess.Popen(cmd, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True,
                            encoding="utf-8", errors="replace")


def finish(proc, boundary, box):
    """Wait for proc; past the boundary kill only proc's own tree by PID. Fills box with the result."""
    began = time.time()
    killed = False
    try:
        out, err = proc.communicate(timeout=boundary)
    except subprocess.TimeoutExpired:
        killed = True
        subprocess.run(["taskkill", "/PID", str(proc.pid), "/T", "/F"], capture_output=True)
        out, err = proc.communicate()
    box.update(code=proc.returncode, stdout=out, stderr=err, killed=killed, wall=time.time() - began)


def seconds(duration):
    h, m, s = duration.split(":")
    return int(h) * 3600 + int(m) * 60 + float(s)


def text_of(node, path):
    found = node.find(path)
    return found.text if found is not None and found.text else ""


def read_trx(path):
    """Return (rows, failures) from a TRX file, or (None, None) when it is missing or unreadable."""
    try:
        root = ET.parse(path).getroot()
    except (OSError, ET.ParseError):
        return None, None
    classes = {}
    for unit in root.iter(TEAM + "UnitTest"):
        method = unit.find(TEAM + "TestMethod")
        classes[unit.get("id")] = method.get("className") if method is not None else ""
    rows, failures = [], []
    for r in root.iter(TEAM + "UnitTestResult"):
        name = r.get("testName", "")
        if "." not in name.split("(")[0]:
            name = f"{classes.get(r.get('testId'), '')}.{name}"
        rows.append((name, seconds(r.get("duration", "0:0:0"))))
        if r.get("outcome") != "Passed":
            failures.append({"row": name, "outcome": r.get("outcome"),
                             "message": text_of(r, f"{TEAM}Output/{TEAM}ErrorInfo/{TEAM}Message"),
                             "trace": text_of(r, f"{TEAM}Output/{TEAM}ErrorInfo/{TEAM}StackTrace"),
                             "stdout": text_of(r, f"{TEAM}Output/{TEAM}StdOut")})
    return rows, failures


def run_entry(n, main, trx_path, boundary):
    rows, failures = read_trx(trx_path)
    mine = [s for name, s in (rows or []) if CLASS in name]
    slowest = round(max(mine) if mine else main["wall"], 2)
    entry = {"run": n, "passed": False, "slowest_seconds": slowest, "wall_seconds": round(main["wall"], 1),
             "rows": len(mine)}
    if rows is not None and failures == [] and len(mine) > 0 and main["code"] == 0 and not main["killed"]:
        entry["passed"] = True
        return entry
    first = (failures or [{}])[0]
    body = first.get("message", "") + ("\n" + first["trace"] if first.get("trace") else "")
    if main["killed"]:
        body = f"killed at the {boundary}s outer boundary\n" + body
    elif not failures:
        body = f"no failed row in the TRX (rows seen: {len(mine)}, expected at least 1)\n" + body
    entry["failure"] = {
        "row": first.get("row") or f"Avowry.Acceptance.Tests.{CLASS}",
        "exit_code": main["code"],
        "stdout": (first.get("stdout", "") + "\n" + main["stdout"][-4000:]).strip(),
        "stderr": (body + "\n" + main["stderr"][-2000:]).strip(),
        # the Bench child creates results/history.jsonl; a DirectoryNotFoundException on it means it was absent
        "history_exists": not ("DirectoryNotFound" in body and "history.jsonl" in body),
    }
    return entry


def record_of(a, runs, main_cmd, load_cmd):
    passed = sum(1 for r in runs if r["passed"])
    return {
        "card": 264, "class": CLASS,
        "repeat_command": f"for i in $(seq {a.runs}); do {display(main_cmd)}; done "
                          f"(after one `dotnet build {PROJECT}`; each repetition runs beside the load)",
        "load": {"class": LOAD_CLASS, "concurrency": 2,
                 "command": f"{display(load_cmd)} (started at the same time as each repetition; "
                            "2 concurrent dotnet test processes)"},
        "timeout_boundary_seconds": a.boundary,
        "timeout_note": "outer per-process boundary enforced by scripts/repeat-run-264.py: a child past it is "
                        "killed by its own PID (taskkill /PID /T); no timeout was added to any product test",
        "slowest_seconds_meaning": "slowest single test row of the class under test in that run (from the TRX)",
        "runs": runs,
        "summary": {"total": len(runs), "passed": passed, "failed": len(runs) - passed},
    }


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("--out", required=True)
    ap.add_argument("--runs", type=int, default=20)
    ap.add_argument("--scratch", required=True)
    ap.add_argument("--boundary", type=int, default=BOUNDARY)
    a = ap.parse_args(argv)
    cwd = str(Path(__file__).resolve().parent.parent)
    scratch = Path(a.scratch)
    scratch.mkdir(parents=True, exist_ok=True)

    def log(msg):
        with (scratch / "progress.log").open("a", encoding="utf-8") as f:
            f.write(f"{time.strftime('%H:%M:%S')} {msg}\n")

    build = subprocess.run(["dotnet", "build", PROJECT], cwd=cwd, capture_output=True, text=True,
                           encoding="utf-8", errors="replace")
    log(f"build exit {build.returncode}")
    if build.returncode != 0:
        print(build.stdout[-3000:], file=sys.stderr)
        return 3

    runs = []
    for n in range(1, a.runs + 1):
        rd = scratch / f"run-{n:02d}"
        main_cmd = test_command(CLASS, rd / "main", f"main-{n:02d}.trx")
        load_cmd = test_command(LOAD_CLASS, rd / "load", f"load-{n:02d}.trx")
        procs = (start(main_cmd, cwd), start(load_cmd, cwd))
        boxes = ({}, {})
        threads = [threading.Thread(target=finish, args=(p, a.boundary, b)) for p, b in zip(procs, boxes)]
        for t in threads:
            t.start()
        for t in threads:
            t.join()
        entry = run_entry(n, boxes[0], rd / "main" / f"main-{n:02d}.trx", a.boundary)
        entry["load_exit_code"] = boxes[1]["code"]
        entry["load_killed"] = boxes[1]["killed"]
        runs.append(entry)
        log(f"run {n}: {'pass' if entry['passed'] else 'FAIL'} slowest {entry['slowest_seconds']}s "
            f"wall {entry['wall_seconds']}s load exit {boxes[1]['code']}")
        Path(a.out).write_text(json.dumps(record_of(a, runs, main_cmd, load_cmd), indent=2) + "\n",
                               encoding="utf-8")
    log("done")
    return 0


if __name__ == "__main__":
    sys.exit(main())
