"""Card 217 green 3a (D157 Criterion 3): tests of the two outcome extractors.

scripts/trx-outcomes-217.py reads a TRX file and scripts/ci-log-outcomes-217.py reads the console log of the CI
Test step. Both emit {"source": {...identity...}, "outcomes": {<fully qualified test name>: <outcome>}} and refuse
a missing outcome, a duplicate name, an empty list, a malformed file (finding keywords: missing, malformed,
duplicate, outcome, count, empty, job). The fixtures below are small; the CI log fixture copies the real shape of
run 42079881523 (job test): a "job<TAB>step<TAB>timestamp" prefix, two projects interleaved, and the legacy summary
block "Test Run Successful. / Total tests: N / Passed: N" (not the "Passed! - Failed: N" form).
"""

import hashlib
import importlib.util
import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
NS = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"
ACC = "Phrase.Acceptance.Tests"


def load(name):
    spec = importlib.util.spec_from_file_location(name.replace("-", "_"), REPO / "scripts" / f"{name}.py")
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


trx = load("trx-outcomes-217")
ci = load("ci-log-outcomes-217")


def tmp(name, data):
    d = Path(tempfile.mkdtemp(prefix="extract217-"))
    p = d / name
    p.write_bytes(data if isinstance(data, bytes) else data.encode("utf-8"))
    return p


def trx_doc(results, counters=True):
    rows = "".join(
        f'<UnitTestResult executionId="e{i}" testName="{n}"' + (f' outcome="{o}"' if o is not None else "") + " />"
        for i, (n, o) in enumerate(results))
    total = len(results)
    summary = (f'<ResultSummary outcome="Completed"><Counters total="{total}" executed="{total}" /></ResultSummary>'
               if counters else "")
    return f'<?xml version="1.0" encoding="utf-8"?><TestRun xmlns="{NS}"><Results>{rows}</Results>{summary}</TestRun>'


def prefixed(lines, job="test", step="UNKNOWN STEP"):
    return "\n".join(f"{job}\t{step}\t2026-11-05T17:59:45.0000000Z{l}" for l in lines) + "\n"


def log_doc(acc, other=(), summaries=None, job="test"):
    """acc/other: [(outcome, name)]; summaries default to a legacy block per project."""
    lines = []
    for o, n in other[:1]:
        lines.append(f"   {o} {n} [< 1 ms]")
    for o, n in acc:
        lines.append(f"   {o} {n} [3 s]")
    for o, n in other[1:]:
        lines.append(f"   {o} {n} [8 ms]")
    if summaries is None:
        summaries = [(len(other), sum(1 for o, _ in other if o == "Passed"), sum(1 for o, _ in other if o == "Failed")),
                     (len(acc), sum(1 for o, _ in acc if o == "Passed"), sum(1 for o, _ in acc if o == "Failed"))]
    for total, passed, failed in summaries:
        lines += ["", "Test Run " + ("Successful." if not failed else "Failed."), f"Total tests: {total}",
                  f"     Passed: {passed}"] + ([f"     Failed: {failed}"] if failed else []) + [" Total time: 1 Minutes"]
    return prefixed(lines, job)


A = [("Passed", f"{ACC}.Fake.One"), ("Passed", f'{ACC}.Fake.Theory(x: "a (b) [c]")'), ("Failed", f"{ACC}.Fake.Three")]
C = [("Passed", "Phrase.Compiler.Tests.Syntax.Lexer.Arrow"), ("Passed", "Phrase.Compiler.Tests.Syntax.Lexer.Word")]


class TrxExtractor(unittest.TestCase):
    def reject(self, data, keyword):
        p = tmp("x.trx", data)
        with self.assertRaises(trx.Rejected) as cm:
            trx.extract(p)
        self.assertIn(keyword, str(cm.exception))

    def test_simple_trx_gives_outcomes_and_identity(self):
        raw = trx_doc([(f"{ACC}.A.One", "Passed"), (f"{ACC}.A.Two", "Failed")]).encode("utf-8")
        p = tmp("run1.trx", raw)
        out = trx.extract(p)
        self.assertEqual(out["outcomes"], {f"{ACC}.A.One": "Passed", f"{ACC}.A.Two": "Failed"})
        self.assertEqual(out["source"]["file"], "run1.trx")
        self.assertEqual(out["source"]["sha256"], hashlib.sha256(raw).hexdigest())
        self.assertEqual(out["source"]["count"], 2)

    def test_table_rejections(self):
        ok = [(f"{ACC}.A.One", "Passed")]
        table = [
            ("missing outcome", trx_doc(ok + [(f"{ACC}.A.Two", None)]), "outcome"),
            ("empty outcome", trx_doc(ok + [(f"{ACC}.A.Two", "")]), "outcome"),
            ("duplicate name", trx_doc(ok + ok), "duplicate"),
            ("no results", trx_doc([]), "empty"),
            ("not xml", "{not xml", "malformed"),
            ("wrong root", f'<Other xmlns="{NS}"/>', "malformed"),
            ("wrong namespace", trx_doc(ok).replace(NS, "urn:other"), "malformed"),
            ("missing name", trx_doc(ok).replace(f'testName="{ACC}.A.One"', ""), "malformed"),
            ("counter mismatch", trx_doc(ok).replace('total="1"', 'total="2"'), "count"),
        ]
        for label, data, keyword in table:
            with self.subTest(label):
                self.reject(data, keyword)

    def test_truncated_theory_names_are_merged_and_counted(self):
        t = f'{ACC}.A.Corpus(source: &quot;C:/x/y&quot;···)'
        out = trx.extract(tmp("t.trx", trx_doc([(t, "Passed"), (t, "Passed"), (f"{ACC}.A.One", "Passed")])))
        self.assertEqual(out["source"]["count"], 3)
        self.assertEqual(len(out["outcomes"]), 2)
        self.assertEqual(list(out["source"]["collapsed_truncated_names"].values()), [2])
        mixed = trx.extract(tmp("m.trx", trx_doc([(t, "Passed"), (t, "Failed")])))
        self.assertEqual(list(mixed["outcomes"].values()), ["Mixed(Failed=1,Passed=1)"])

    def test_missing_file_is_rejected(self):
        with self.assertRaises(trx.Rejected) as cm:
            trx.extract(REPO / "no-such-217.trx")
        self.assertIn("missing", str(cm.exception))

    def test_trx_without_counters_is_accepted(self):
        out = trx.extract(tmp("c.trx", trx_doc([(f"{ACC}.A.One", "Passed")], counters=False)))
        self.assertEqual(out["source"]["count"], 1)

    def test_cli_writes_json_and_rejects_without_traceback(self):
        script = str(REPO / "scripts" / "trx-outcomes-217.py")
        good = tmp("g.trx", trx_doc([(f"{ACC}.A.One", "Passed")]))
        out_file = good.parent / "o.json"
        p = subprocess.run([sys.executable, script, str(good), "--out", str(out_file)], capture_output=True, text=True)
        self.assertEqual(p.returncode, 0, p.stderr)
        self.assertEqual(json.loads(out_file.read_text(encoding="utf-8"))["outcomes"], {f"{ACC}.A.One": "Passed"})
        self.assertTrue(out_file.read_bytes().endswith(b"\n"))
        self.assertNotIn(b"\r", out_file.read_bytes())
        bad = tmp("b.trx", "{not xml")
        p = subprocess.run([sys.executable, script, str(bad)], capture_output=True, text=True)
        self.assertEqual(p.returncode, 1)
        self.assertIn("malformed", p.stderr)
        self.assertNotIn("Traceback", p.stderr)


class CiLogExtractor(unittest.TestCase):
    def reject(self, text, keyword, **kw):
        with self.assertRaises(ci.Rejected) as cm:
            ci.extract_text(text.encode("utf-8"), "log.txt", **kw)
        self.assertIn(keyword, str(cm.exception))

    def test_simple_acceptance_block_with_two_interleaved_projects(self):
        raw = log_doc(A, C).encode("utf-8")
        out = ci.extract_text(raw, "log.txt", job="test")
        self.assertEqual(out["outcomes"], {n: o for o, n in A})
        s = out["source"]
        self.assertEqual((s["file"], s["count"], s["job"]), ("log.txt", 3, "test"))
        self.assertEqual(s["sha256"], hashlib.sha256(raw).hexdigest())
        self.assertEqual(s["summary"], {"total": 3, "passed": 2, "failed": 1, "skipped": 0})

    def test_one_project_only_without_job_prefix_and_with_ansi_and_bom(self):
        text = ("﻿2026-11-05T17:59:57.0Z   Passed " + f"{ACC}.A.One [12 s]\n"
                + "2026-11-05T17:59:58.0Z \x1b[0m  Skipped " + f"{ACC}.A.Two\n"
                + "\nTest Run Successful.\nTotal tests: 2\n     Passed: 1\n    Skipped: 1\n")
        out = ci.extract_text(text.encode("utf-8"), "l.txt")
        self.assertEqual(out["outcomes"], {f"{ACC}.A.One": "Passed", f"{ACC}.A.Two": "Skipped"})

    def test_theory_arguments_with_brackets_and_times_are_kept_in_the_name(self):
        out = ci.extract_text(log_doc(A, C).encode(), "l.txt", job="test")
        self.assertIn(f'{ACC}.Fake.Theory(x: "a (b) [c]")', out["outcomes"])

    def test_time_forms(self):
        for t in ("[< 1 ms]", "[8 ms]", "[14 s]", "[1 m 5 s]", "[2 h 3 m]", ""):
            with self.subTest(t):
                text = prefixed([f"   Passed {ACC}.A.One {t}".rstrip(), "", "Test Run Successful.", "Total tests: 1",
                                 "     Passed: 1"])
                self.assertEqual(ci.extract_text(text.encode(), "l.txt", job="test")["outcomes"],
                                 {f"{ACC}.A.One": "Passed"})

    def test_failure_detail_lines_are_not_results(self):
        lines = [f"   Failed {ACC}.A.One [3 s]", "     Error Message:", "      Failed Assert.True() failure",
                 "     Stack Trace:", "", "Test Run Failed.", "Total tests: 1", "     Failed: 1"]
        out = ci.extract_text(prefixed(lines).encode(), "l.txt", job="test")
        self.assertEqual(out["outcomes"], {f"{ACC}.A.One": "Failed"})

    def test_table_rejections(self):
        dup = A + [A[0]]
        table = [
            ("duplicate name", log_doc(dup, C, summaries=[(2, 2, 0), (4, 3, 1)]), "duplicate", {}),
            ("count differs from summary", log_doc(A, C, summaries=[(2, 2, 0), (4, 3, 1)]), "count", {}),
            ("passed differs from summary", log_doc(A, C, summaries=[(2, 2, 0), (3, 3, 0)]), "count", {}),
            ("compiler summary missing", log_doc(A, C, summaries=[(3, 2, 1)]), "count", {}),
            ("no acceptance lines", log_doc([], C, summaries=[(2, 2, 0)]), "empty", {}),
            ("no summary", prefixed([f"   Passed {ACC}.A.One [1 s]"]), "count", {}),
            ("malformed name", prefixed(["   Passed not a name!! [1 s]", "Test Run Successful.", "Total tests: 1",
                                         "     Passed: 1"]), "malformed", {}),
            ("empty text", "", "empty", {}),
            ("two jobs, no --job", log_doc(A, C) + log_doc(A, C, job="usable-v1"), "job", {}),
            ("job named but absent", log_doc(A, C), "job", {"job": "nope"}),
        ]
        for label, text, keyword, kw in table:
            with self.subTest(label):
                self.reject(text, keyword, **kw)

    def test_truncated_theory_lines_are_merged_and_counted_against_the_summary(self):
        t = f'{ACC}.Fake.Corpus(source: "/home/runner/x"···)'
        acc = [("Passed", t), ("Passed", t), ("Passed", f"{ACC}.Fake.One")]
        out = ci.extract_text(log_doc(acc, C).encode(), "l.txt", job="test")
        self.assertEqual(out["source"]["count"], 3)
        self.assertEqual(out["outcomes"][t], "Passed")
        self.assertEqual(out["source"]["collapsed_truncated_names"], {t: 2})

    def test_job_filter_selects_one_of_two_jobs(self):
        text = log_doc(A, C) + log_doc(A[:1], [], summaries=[(1, 1, 0)], job="usable-v1")
        self.assertEqual(len(ci.extract_text(text.encode(), "l.txt", job="test")["outcomes"]), 3)
        self.assertEqual(len(ci.extract_text(text.encode(), "l.txt", job="usable-v1")["outcomes"]), 1)

    def test_old_passed_bang_summary_form_is_checked_too(self):
        lines = [f"   Passed {ACC}.A.One [1 s]", f"   Failed {ACC}.A.Two [1 s]",
                 "Failed!  - Failed:     1, Passed:     1, Skipped:     0, Total:     2, Duration: 1 s - x.dll (net8.0)"]
        out = ci.extract_text(prefixed(lines).encode(), "l.txt", job="test")
        self.assertEqual(len(out["outcomes"]), 2)
        lines[-1] = lines[-1].replace("Total:     2", "Total:     3")
        self.reject(prefixed(lines), "count", job="test")

    def test_meta_is_recorded_in_the_identity(self):
        out = ci.extract_text(log_doc(A, C).encode(), "l.txt", job="test", meta={"github_run_id": "42"})
        self.assertEqual(out["source"]["github_run_id"], "42")

    def test_cli_rejects_without_traceback_and_writes_lf_json(self):
        script = str(REPO / "scripts" / "ci-log-outcomes-217.py")
        good = tmp("ci.txt", log_doc(A, C))
        out_file = good.parent / "o.json"
        p = subprocess.run([sys.executable, script, str(good), "--job", "test", "--meta", "github_run_id=7",
                            "--out", str(out_file)], capture_output=True, text=True)
        self.assertEqual(p.returncode, 0, p.stderr)
        data = json.loads(out_file.read_text(encoding="utf-8"))
        self.assertEqual(data["source"]["github_run_id"], "7")
        self.assertNotIn(b"\r", out_file.read_bytes())
        p = subprocess.run([sys.executable, script, str(good.parent / "none.txt")], capture_output=True, text=True)
        self.assertEqual(p.returncode, 1)
        self.assertIn("missing", p.stderr)
        self.assertNotIn("Traceback", p.stderr)


if __name__ == "__main__":
    unittest.main()
