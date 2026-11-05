"""Card 264 red 1 (D157 Criterion 1): the 20-run parallel repeat evidence check.

scripts/check-repeat-evidence.py (does not exist yet) reads .mob/records/264/repeat-evidence.json and
FAILS unless it records >= 20 consecutive passing runs of the class PhraseExecutedOutcomesAcceptance with a
named load class and an integer concurrency, the exact repeat command, a per-process timeout boundary, one
entry per run, and a full explanation for any failure.

  python scripts/check-repeat-evidence.py [--root DIR]
  exit 0 clean ("repeat-evidence: OK"), 1 findings ("<path>: <keyword> <detail>" on stderr), 2 not a directory.
  Finding keywords: missing, malformed, command, class, load, timeout, runs, unexplained, summary.

Record shape (.mob/records/264/repeat-evidence.json):
  {"card": 264, "class": "PhraseExecutedOutcomesAcceptance",
   "repeat_command": "<exact command, names the class>",
   "load": {"class": "<other process-starting acceptance class>", "concurrency": <int >= 1>, "command": "<exact>"},
   "timeout_boundary_seconds": <number > 0, the outer per-process boundary; no product-test timeout>,
   "runs": [{"run": 1, "passed": true, "slowest_seconds": 28.3}, ...,
            {"run": n, "passed": false, "slowest_seconds": s,
             "failure": {"row": "<fully qualified test row>", "exit_code": <int>, "stdout": "...",
                         "stderr": "...", "history_exists": <bool>}}],
   "summary": {"total": N, "passed": P, "failed": F}}
Runs are numbered 1..N with no gap or repeat; at least 20; every run passed (a failure inside the record, explained
or not, means the 20 consecutive passes are not shown); no slowest_seconds above timeout_boundary_seconds.

Red shape (as card 237): the checker is not written, so every test fails on the missing script, and the two
real-record tests fail because the record does not exist. Not wired into mob_process.py check.

ZOMBIES: Z test_zero_*, O test_one_*, M test_many_*, B test_boundary_*, I test_interface_*, E test_exception_*,
S test_simple_*. Fixture trees use tempfile.mkdtemp and are left in place (no cleanup, by the safety rule).
"""

import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
SCRIPT = REPO / "scripts" / "check-repeat-evidence.py"
REL = ".mob/records/264/repeat-evidence.json"
ROW = ('Phrase.Acceptance.Tests.PhraseExecutedOutcomesAcceptance.'
       'Correct_read_and_modify_replies_pass_and_record_the_starting_code(task: "004-read-phrase", kind: "read")')


def run(root):
    assert SCRIPT.exists(), "scripts/check-repeat-evidence.py does not exist yet (card 264 criterion 1 red)"
    p = subprocess.run([sys.executable, str(SCRIPT), "--root", str(root)], capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    return p.returncode, p.stdout, p.stderr


def good(n=20):
    return {
        "card": 264,
        "class": "PhraseExecutedOutcomesAcceptance",
        "repeat_command": "for i in $(seq 20); do dotnet test tests/Phrase.Acceptance.Tests "
                          "--filter FullyQualifiedName~PhraseExecutedOutcomesAcceptance; done",
        "load": {"class": "BenchmarkHarnessAcceptance", "concurrency": 2,
                 "command": "dotnet test tests/Phrase.Acceptance.Tests --filter FullyQualifiedName~BenchmarkHarnessAcceptance"},
        "timeout_boundary_seconds": 1800,
        "runs": [{"run": i, "passed": True, "slowest_seconds": 28.3} for i in range(1, n + 1)],
        "summary": {"total": n, "passed": n, "failed": 0},
    }


def failed_run(rec, idx, **failure_over):
    f = {"row": ROW, "exit_code": 1, "stdout": "out", "stderr": "err", "history_exists": False}
    f.update(failure_over)
    rec["runs"][idx] = {"run": idx + 1, "passed": False, "slowest_seconds": 21.0, "failure": f}
    n = len(rec["runs"])
    rec["summary"] = {"total": n, "passed": n - 1, "failed": 1}
    return rec


def tree(rec=None, raw=None):
    root = Path(tempfile.mkdtemp(prefix="repeat-evidence-"))
    p = root / REL
    p.parent.mkdir(parents=True)
    if raw is not None:
        p.write_text(raw, encoding="utf-8")
    elif rec is not None:
        p.write_text(json.dumps(rec), encoding="utf-8")
    return root


class RepeatEvidence(unittest.TestCase):
    def reject(self, rec, keyword, raw=None):
        code, out, err = run(tree(rec, raw))
        self.assertEqual(code, 1, f"expected a finding, got exit {code}: {out}{err}")
        self.assertIn(keyword, err)

    def accept(self, rec):
        code, out, err = run(tree(rec))
        self.assertEqual(code, 0, f"expected OK, got exit {code}: {err}")
        self.assertIn("repeat-evidence: OK", out)

    # --- Z: zero
    def test_zero_missing_record_is_a_finding(self):
        self.reject(None, "missing")

    def test_zero_runs_is_rejected(self):
        rec = good()
        rec["runs"] = []
        rec["summary"] = {"total": 0, "passed": 0, "failed": 0}
        self.reject(rec, "runs")

    def test_zero_unloaded_pass_is_not_enough(self):
        rec = good()
        del rec["load"]
        self.reject(rec, "load")

    # --- O: one
    def test_one_run_is_rejected(self):
        self.reject(good(1), "runs")

    # --- M: many
    def test_many_nineteen_runs_rejected_twenty_and_more_accepted(self):
        self.reject(good(19), "runs")
        self.accept(good(20))
        self.accept(good(25))

    # --- B: boundary
    def test_boundary_run_numbers_must_be_one_to_n_without_gap_or_repeat(self):
        gap = good()
        gap["runs"][10]["run"] = 99
        dup = good()
        dup["runs"][10]["run"] = 10
        for name, rec in (("gap", gap), ("repeat", dup)):
            with self.subTest(name):
                self.reject(rec, "runs")

    def test_boundary_slowest_run_must_stay_inside_the_timeout_boundary(self):
        at = good()
        at["runs"][4]["slowest_seconds"] = 1800
        self.accept(at)
        over = good()
        over["runs"][4]["slowest_seconds"] = 1800.5
        self.reject(over, "timeout")

    def test_boundary_timeout_boundary_must_be_a_positive_number(self):
        for name, v in (("zero", 0), ("negative", -1), ("string", "30m"), ("bool", True), ("absent", None)):
            with self.subTest(name):
                rec = good()
                if v is None:
                    del rec["timeout_boundary_seconds"]
                else:
                    rec["timeout_boundary_seconds"] = v
                self.reject(rec, "timeout")

    # --- I: interface (command, class, load, concurrency)
    def test_interface_repeat_command_is_recorded_and_names_the_class(self):
        for name, v in (("absent", None), ("empty", ""),
                        ("other class", "dotnet test --filter BenchmarkHarnessAcceptance")):
            with self.subTest(name):
                rec = good()
                if v is None:
                    del rec["repeat_command"]
                else:
                    rec["repeat_command"] = v
                self.reject(rec, "command")

    def test_interface_class_under_test_is_the_read_and_modify_class(self):
        rec = good()
        rec["class"] = "BenchmarkHarnessAcceptance"
        self.reject(rec, "class")

    def test_interface_load_class_and_concurrency_are_named(self):
        cases = {
            "unnamed class": lambda L: L.update({"class": ""}),
            "absent class": lambda L: L.pop("class"),
            "load is the class under test": lambda L: L.update({"class": "PhraseExecutedOutcomesAcceptance"}),
            "absent concurrency": lambda L: L.pop("concurrency"),
            "zero concurrency": lambda L: L.update({"concurrency": 0}),
            "string concurrency": lambda L: L.update({"concurrency": "two"}),
            "bool concurrency": lambda L: L.update({"concurrency": True}),
            "absent load command": lambda L: L.pop("command"),
        }
        for name, mutate in cases.items():
            with self.subTest(name):
                rec = good()
                mutate(rec["load"])
                self.reject(rec, "load")

    # --- E: exception (failures must be explained)
    def test_exception_failed_run_without_explanation_is_rejected(self):
        rec = good()
        rec["runs"][6] = {"run": 7, "passed": False, "slowest_seconds": 21.0}
        rec["summary"] = {"total": 20, "passed": 19, "failed": 1}
        self.reject(rec, "unexplained")

    def test_exception_failure_must_carry_every_diagnostic_field(self):
        for key in ("row", "exit_code", "stdout", "stderr", "history_exists"):
            with self.subTest(key):
                rec = failed_run(good(), 6)
                del rec["runs"][6]["failure"][key]
                self.reject(rec, "unexplained")

    def test_exception_failure_fields_must_have_the_right_types(self):
        cases = {
            "row not fully qualified": {"row": "Correct_read_and_modify_replies_pass_and_record_the_starting_code"},
            "empty row": {"row": ""},
            "exit_code string": {"exit_code": "1"},
            "exit_code bool": {"exit_code": False},
            "stdout not text": {"stdout": None},
            "history_exists not bool": {"history_exists": "no"},
        }
        for name, over in cases.items():
            with self.subTest(name):
                self.reject(failed_run(good(), 6, **over), "unexplained")

    def test_exception_explained_failure_still_breaks_the_twenty_consecutive_passes(self):
        self.reject(failed_run(good(), 6), "runs")

    # --- S: simple (machine-countable summary, parseable record)
    def test_simple_summary_must_match_the_runs(self):
        for name, s in (("absent", None), ("total off", {"total": 21, "passed": 20, "failed": 0}),
                        ("passed off", {"total": 20, "passed": 19, "failed": 0}),
                        ("failed hidden", {"total": 20, "passed": 20, "failed": 1})):
            with self.subTest(name):
                rec = good()
                if s is None:
                    del rec["summary"]
                else:
                    rec["summary"] = s
                self.reject(rec, "summary")

    def test_simple_malformed_record_is_a_finding_not_a_traceback(self):
        for name, raw in (("truncated", '{"card": 264, "runs": ['), ("array", "[]"), ("empty", "")):
            with self.subTest(name):
                code, out, err = run(tree(raw=raw))
                self.assertEqual(code, 1, err)
                self.assertIn("malformed", err)
                self.assertNotIn("Traceback", err)

    def test_simple_root_that_is_not_a_directory_exits_two(self):
        code, out, err = run(Path(tempfile.mkdtemp(prefix="repeat-evidence-")) / "nope")
        self.assertEqual(code, 2, err)

    def test_simple_complete_record_is_accepted(self):
        self.accept(good())

    # --- the real record for card 264 is required
    def test_real_record_file_exists_for_card_264(self):
        """RED now: .mob/records/264/repeat-evidence.json is not committed yet."""
        self.assertTrue((REPO / REL).is_file(), f"{REL} is missing: no qualifying 20-run record for card 264")

    def test_real_record_for_card_264_passes_the_check(self):
        """RED now: neither the checker nor the record exists."""
        code, out, err = run(REPO)
        self.assertEqual(code, 0, f"real repeat evidence not qualifying: {err}")


if __name__ == "__main__":
    unittest.main()
