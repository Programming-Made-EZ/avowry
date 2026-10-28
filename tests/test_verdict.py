import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / "scripts" / "verdict.py"
SUMMARIZER = ROOT / "scripts" / "summarize-private-results.py"


class VerdictPageTests(unittest.TestCase):
    def run_verdict(self, csharp_rows, phrase_rows, holdout=None, drivers=None):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            csharp = root / "csharp.jsonl"
            phrase = root / "phrase.jsonl"
            output = root / "verdict.md"
            csharp.write_text("".join(json.dumps(row) + "\n" for row in csharp_rows), encoding="utf-8")
            phrase.write_text("".join(json.dumps(row) + "\n" for row in phrase_rows), encoding="utf-8")
            command = [sys.executable, str(SCRIPT), "--csharp", str(csharp), "--phrase", str(phrase)]
            if holdout is not None:
                summary = root / "holdout.json"
                summary.write_text(json.dumps(holdout), encoding="utf-8")
                command += ["--holdout-summary", str(summary)]
            if drivers is not None:
                drivers_file = root / "DRIVERS.md"
                drivers_file.write_text(drivers, encoding="utf-8")
                command += ["--drivers", str(drivers_file)]
            command += ["--out", str(output)]
            completed = subprocess.run(command, cwd=ROOT, text=True, capture_output=True)
            self.assertEqual(0, completed.returncode, completed.stderr)
            return output.read_text(encoding="utf-8")

    @staticmethod
    def rows(arm, pass_count=5, tokens=100, attempts=1, harness_errors=0, read_pass_count=None):
        rows = []
        for model in ("alpha", "beta", "gamma"):
            for kind in ("write", "read", "modify", "fix"):
                passed_for_kind = read_pass_count if kind == "read" and read_pass_count is not None else pass_count
                for run in range(5):
                    rows.append({
                        "model": model,
                        "task": f"{kind}-task",
                        "kind": kind,
                        "label": ("baseline-094-r" if arm == "csharp" else "head2head-dev-r") + str(run + 1),
                        "passed": run < passed_for_kind,
                        "attempts": attempts if run < passed_for_kind else 3,
                        "tokens_in": tokens,
                        "tokens_out": 0,
                        "existing_code_tokens": 0 if kind == "write" else 40,
                        "patched": False,
                        "status": "harness-error" if run < harness_errors else "completed",
                    })
        return rows

    def test_clear_win(self):
        # Given three models, paired write/change tasks, equal passes and cheaper Phrase rows
        csharp = self.rows("csharp", tokens=200)
        phrase = self.rows("phrase", tokens=100)
        # When Steve generates the verdict page
        page = self.run_verdict(csharp, phrase, self.winning_holdout())
        # Then the frozen bar calls the result a win
        self.assertTrue(page.startswith("WIN "), page)

    def test_loss_for_pass_gap_over_ten_points(self):
        # Given Phrase is twenty points behind plain C#
        page = self.run_verdict(self.rows("csharp", 5, 200), self.rows("phrase", 4, 100), self.winning_holdout())
        # When and then the frozen pass band is applied
        self.assertTrue(page.startswith("LOSS "), page)

    def test_tie_for_gap_between_five_and_ten_points(self):
        # Given a ten-point aggregate pass gap and otherwise winning efficiency
        phrase = self.rows("phrase", 5, 100)
        for row in phrase:
            if (row["model"] == "alpha" and row["kind"] in ("write", "read")
                    and int(row["label"][-1]) in (4, 5)):
                row["passed"] = False
                row["attempts"] = 3
        # When and then the frozen pass band is applied
        page = self.run_verdict(self.rows("csharp", 5, 200), phrase, self.winning_holdout())
        self.assertTrue(page.startswith("TIE "), page)

    def test_inconclusive_for_too_many_harness_errors(self):
        # Given more than ten percent of scheduled rows are harness errors
        phrase = self.rows("phrase", tokens=100, harness_errors=2)
        # When and then completeness is checked before performance
        page = self.run_verdict(self.rows("csharp", tokens=200), phrase, self.winning_holdout())
        self.assertTrue(page.startswith("INCONCLUSIVE "), page)
        self.assertIn("harness-error", page)

    def test_inconclusive_holdout_names_unavailable_drivers_and_resets(self):
        # Given Steve has real model names and an invented driver table with current statuses
        holdout = self.winning_holdout()
        for group in holdout["groups"]:
            group["model"] = {
                "delta": "copilot-default", "epsilon": "codex-gpt-5.6-sol", "zeta": "grok-4.7",
            }[group["model"]]
            if group["model"] == "copilot-default":
                group["completed_count"] = 0
                group["unavailable_count"] = 5
            elif group["model"] == "codex-gpt-5.6-sol" and group["arm"] == "phrase" and group["kind"] == "modify":
                group["completed_count"] = 2
                group["unavailable_count"] = 3
        drivers = """| Driver | Status | Last probe | Known reset | Note |
|---|---|---|---|---|
| Copilot | UNAVAILABLE (invented quota) | today | 2031-02-03 04:05 UTC | invented |
| Codex | available | today | none | invented |
| Grok | available | today | none | invented |
"""
        # When the holdout is judged on the same frozen bar
        page = self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), holdout, drivers)
        # Then Steve sees the driver-table display name, status, and reset for the prefixed model
        self.assertTrue(page.startswith("INCONCLUSIVE "), page)
        self.assertIn("Copilot has no completed holdout rows", page)
        self.assertIn("Copilot status: UNAVAILABLE (invented quota)", page)
        self.assertIn("Codex has 3 unavailable holdout rows", page)
        self.assertIn("holdout harness-error rows are", page)
        self.assertIn("Copilot resets 2031-02-03 04:05 UTC", page)

    def test_committed_driver_table_describes_real_unavailable_models(self):
        # Given Steve has unavailable real model names and the committed driver table
        holdout = self.winning_holdout()
        for group in holdout["groups"]:
            group["model"] = {
                "delta": "copilot-default", "epsilon": "agy-gemini-4-argon-high", "zeta": "grok-4.7",
            }[group["model"]]
            if group["model"] in ("copilot-default", "agy-gemini-4-argon-high"):
                group["completed_count"] = 0
                group["unavailable_count"] = 5
        # When the page uses DRIVERS.md through its default command-line path
        page = self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), holdout)
        # Then Steve sees the committed display names, statuses, and reset times
        self.assertIn("Copilot status: UNAVAILABLE (no quota)", page)
        self.assertIn("Copilot resets 2026-11-01 00:00 UTC (account endpoint)", page)
        self.assertIn("agy status: UNAVAILABLE (quota)", page)
        self.assertIn("agy resets in 108h50m", page)

    def test_fully_completed_three_model_holdout_uses_the_performance_bar(self):
        # Given all holdout groups completed for three models
        holdout = self.winning_holdout()
        # When Steve generates the verdict page
        page = self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), holdout)
        # Then availability does not obscure the frozen performance verdict
        self.assertTrue(page.startswith("WIN "), page)

    def test_old_holdout_summary_is_inconclusive(self):
        # Given a holdout summary from before harness-error counts were published
        holdout = {"groups": [
            {"arm": "phrase", "model": model, "kind": "write", "count": 5,
             "pass_rate": 1.0, "mean_attempts": 1.0, "mean_total_tokens": 100.0}
            for model in ("codex", "grok", "copilot")
        ]}
        # When Steve generates the verdict page
        page = self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), holdout)
        # Then the unknown harness health cannot be mistaken for a conclusive result
        self.assertTrue(page.startswith("INCONCLUSIVE "), page)
        self.assertIn("holdout summary lacks harness-error counts", page)

    def test_summarized_unavailable_rows_make_the_verdict_inconclusive(self):
        # Given private rows with two completed models and one unavailable model in both arms
        rows = [
            {"arm": arm, "model": model, "kind": "write", "status": status,
             "passed": status == "completed", "attempts": 1 if status == "completed" else 3,
             "tokens_in": 80 if status == "completed" else 900, "tokens_out": 20}
            for arm in ("csharp", "phrase") for model, status in
            (("codex", "completed"), ("grok", "completed"), ("copilot", "unavailable"))
        ]
        rows.extend(
            {"arm": arm, "model": "codex", "kind": "write", "status": "timed-out",
             "passed": False, "attempts": 3, "tokens_in": 900, "tokens_out": 100}
            for arm in ("csharp", "phrase"))
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            private_rows = root / "private.jsonl"
            summary = root / "summary.json"
            private_rows.write_text("".join(json.dumps(row) + "\n" for row in rows), encoding="utf-8")
            summarized = subprocess.run(
                [sys.executable, str(SUMMARIZER), str(private_rows), "--out", str(summary)],
                cwd=ROOT, text=True, capture_output=True)
            self.assertEqual(0, summarized.returncode, summarized.stderr)
            published = json.loads(summary.read_text(encoding="utf-8"))
            codex_phrase = next(group for group in published["groups"]
                                if group["arm"] == "phrase" and group["model"] == "codex")
            self.assertEqual(2, codex_phrase["count"])
            self.assertEqual(1, codex_phrase["completed_count"])
            self.assertEqual(1, codex_phrase["unavailable_count"])
            self.assertEqual(1.0, codex_phrase["mean_attempts"])
            self.assertEqual(100.0, codex_phrase["mean_total_tokens"])
            # When that published numbers-only summary drives the verdict
            page = self.run_verdict(
                self.rows("csharp", tokens=200), self.rows("phrase", tokens=100),
                published)
        # Then Steve sees an inconclusive holdout instead of ordinary model failures
        self.assertTrue(page.startswith("INCONCLUSIVE "), page)
        self.assertIn("only 2 usable models", page)
        self.assertIn("holdout harness-error rows are 50.0%", page)

    def test_frozen_constants_match_decider_d32(self):
        # Given the single frozen table used by the verdict implementation
        import scripts.verdict as verdict
        expected = {
            "scheduled_runs": 5, "attempt_cap": 3, "win_gap_points": 5,
            "tie_gap_points": 10, "read_block_points": 10, "token_ratio": 0.75,
            "patch_rate": 0, "minimum_models": 3, "minimum_pairs": 3,
            "maximum_harness_error_share": 0.10, "holdout_tasks": 3,
        }
        # When D32's frozen numbers are checked
        d32 = (ROOT / "docs" / "decisions" / "DECIDERS.md").read_text(encoding="utf-8")
        clause = d32.split("#### Decider D32:", 1)[1].split("#### Decider D31:", 1)[0]
        # Then the implementation table agrees and the source clause still carries every threshold
        self.assertEqual(expected, verdict.BAR)
        for text in ("N = 5", "attempt cap 3", "within 5 points", "5 to 10 points",
                     "worse than 10 points", "75 percent", "Patch rate 0",
                     "fewer than 3 models", "fewer than 3 paired", "more than 10 percent",
                     "Holdout: 3 tasks"):
            self.assertIn(text, clause)

    def test_token_failure_is_tie_and_reports_development_comparison(self):
        # Given Phrase passes equally but uses more than 75 percent of C# tokens
        page = self.run_verdict(self.rows("csharp", tokens=300), self.rows("phrase", tokens=1183), self.winning_holdout())
        # When and then efficiency is evaluated separately from pass rate
        self.assertTrue(page.startswith("TIE "), page)
        self.assertIn("1,183", page)
        self.assertIn("300", page)

    def test_committed_development_rows_have_the_frozen_result_and_no_task_text(self):
        # Given the committed development histories only
        command = [sys.executable, str(SCRIPT), "--csharp", "bench/csharp-baseline/results/history.jsonl",
                   "--phrase", "bench/results/history.jsonl"]
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "verdict.md"
            # When the real development page is generated
            completed = subprocess.run(command + ["--out", str(output)], cwd=ROOT, text=True, capture_output=True)
            self.assertEqual(0, completed.returncode, completed.stderr)
            page = output.read_text(encoding="utf-8")
        # Then today's development-only reading is retained under an inconclusive headline and no task text leaks
        self.assertTrue(page.startswith("INCONCLUSIVE "), page)
        self.assertIn("Development tasks alone: TIE", page)
        self.assertNotIn("001-write-greeting", page)
        self.assertNotIn("returns hello", page)
        self.assertNotIn("bench/", page)

    def test_no_holdout_can_never_be_the_headline_result(self):
        # Given Steve has development rows that would otherwise be a clear WIN
        page = self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100))
        # When he asks for the decision without supplying the sealed-result summary
        # Then the headline is inconclusive while the development-only reading remains visible
        self.assertTrue(page.startswith("INCONCLUSIVE "), page)
        self.assertIn("no holdout result supplied", page.splitlines()[0])
        self.assertIn("Development tasks alone: WIN", page)

    def test_holdout_loss_and_token_failure_change_the_headline(self):
        # Given development is a WIN but one holdout loses on pass rate and another misses the token bar
        loss = self.winning_holdout(phrase_pass_rate=0.8)
        token_tie = self.winning_holdout(phrase_tokens=180)
        # When Steve reads each combined decision
        # Then D32's "judged on the same bar" clause changes the headline
        self.assertTrue(self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), loss).startswith("LOSS "))
        self.assertTrue(self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), token_tie).startswith("TIE "))

    def test_holdout_attempt_ceiling_and_read_gap_change_the_headline(self):
        # Given development is a WIN but one holdout needs more attempts and another has a blocking read gap
        attempts = self.winning_holdout()
        for group in attempts["groups"]:
            if group["arm"] == "phrase":
                group["mean_attempts"] = 2.0
        read_gap = self.winning_holdout()
        read_gap["groups"].extend(
            {"arm": arm, "model": model, "kind": "read", "count": 5,
             "completed_count": 5, "unavailable_count": 0,
             "pass_rate": 0.85 if arm == "phrase" else 1.0,
             "mean_attempts": 1.0,
             "mean_total_tokens": 100.0 if arm == "phrase" else 200.0}
            for arm in ("csharp", "phrase") for model in ("delta", "epsilon", "zeta"))
        # When Steve reads each combined decision
        # Then the same frozen attempt and read conditions prevent a WIN
        self.assertTrue(self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), attempts).startswith("TIE "))
        self.assertTrue(self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), read_gap).startswith("TIE "))

    def test_patch_rate_is_computed_and_construction_wording_is_conditional(self):
        # Given one Phrase development row records a generated-C# patch
        phrase = self.rows("phrase", tokens=100)
        phrase[0]["patched"] = True
        # When the page is generated
        page = self.run_verdict(self.rows("csharp", tokens=200), phrase, self.winning_holdout())
        # Then Steve sees the measured rate, without a false by-construction claim
        self.assertIn("Patch rate: Phrase 1.7%.", page)
        self.assertNotIn("1.7% by construction", page)

    def test_patch_rate_is_not_by_construction_when_a_row_does_not_record_false(self):
        # Given a Phrase development row has no explicit patched measurement
        phrase = self.rows("phrase", tokens=100)
        del phrase[0]["patched"]
        # When Steve reads the computed zero patch rate
        page = self.run_verdict(self.rows("csharp", tokens=200), phrase, self.winning_holdout())
        # Then the page does not claim every row recorded false
        self.assertIn("Patch rate: Phrase 0.0%.", page)
        self.assertNotIn("0.0% by construction", page)

    def test_each_verdict_has_distinct_vocabulary_guidance(self):
        # Given examples of every possible frozen-bar verdict
        pages = {
            "WIN": self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), self.winning_holdout()),
            "LOSS": self.run_verdict(self.rows("csharp", 5, 200), self.rows("phrase", 4, 100), self.winning_holdout()),
            "TIE": self.run_verdict(self.rows("csharp", 5, 200), self.rows("phrase", 5, 180), self.winning_holdout()),
            "INCONCLUSIVE": self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100)),
        }
        # When Steve reads what each result means for paused vocabulary work
        guidance = [page.splitlines()[2] for page in pages.values()]
        # Then all four plain sentences differ
        self.assertEqual(4, len(set(guidance)), guidance)

    def test_exact_pass_rate_boundaries_are_stable(self):
        # Given Phrase is exactly 5.0 or exactly 10.0 percentage points behind
        exact_five = self.rows("phrase", tokens=100)
        exact_ten = self.rows("phrase", tokens=100)
        for index in range(3):
            exact_five[index]["passed"] = False
        for index in range(6):
            exact_ten[index]["passed"] = False
        # When the integer-row pass gaps reach D32's boundaries
        # Then exactly 5 remains WIN-eligible and exactly 10 is a TIE
        self.assertTrue(self.run_verdict(self.rows("csharp", tokens=200), exact_five, self.winning_holdout()).startswith("WIN "))
        self.assertTrue(self.run_verdict(self.rows("csharp", tokens=200), exact_ten, self.winning_holdout()).startswith("TIE "))

    def test_exact_ten_point_development_read_gap_remains_win_eligible(self):
        # Given Steve has exact 80.0% versus 70.0% development read rates and a pass gap under 5.0 points
        csharp = [row for row in self.rows("csharp", tokens=200) if row["kind"] != "read"]
        phrase = [row for row in self.rows("phrase", tokens=100) if row["kind"] != "read"]
        for rows in (csharp, phrase):
            for row in rows:
                row["task"] += "-" + row["label"][-1]
        for model in ("alpha", "beta", "gamma"):
            for run in range(10):
                common = {"model": model, "task": f"read-{run}", "kind": "read",
                          "label": "baseline-094-r" + str(run % 5 + 1), "attempts": 1,
                          "tokens_in": 200, "tokens_out": 0, "existing_code_tokens": 40,
                          "patched": False, "status": "completed"}
                csharp.append({**common, "passed": run < 8})
                phrase.append({**common, "label": "head2head-dev-r" + str(run % 5 + 1),
                               "tokens_in": 100, "passed": run < 7})
        # When the development path applies the frozen read-gap boundary
        page = self.run_verdict(csharp, phrase, self.winning_holdout())
        # Then an exact 10.0-point read gap does not block the otherwise qualifying WIN
        self.assertTrue(page.startswith("WIN "), page)

    def test_page_notes_the_recorded_token_failure_reading(self):
        # Given D32 does not name the token-failure result when pass rate is fine
        page = self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=180), self.winning_holdout())
        # When the script applies all conditions required for WIN
        # Then the page makes its TIE reading explicit for the facilitator
        self.assertIn("NOTE: D32 does not name the outcome of a token condition that fails while pass rate is fine", page)

    @staticmethod
    def winning_holdout(phrase_pass_rate=1.0, phrase_tokens=100):
        return {"groups": [
            {"arm": arm, "model": model, "kind": kind, "count": 5,
             "completed_count": 5, "unavailable_count": 0,
             "pass_rate": phrase_pass_rate if arm == "phrase" else 1.0,
             "mean_attempts": 1.0,
             "mean_total_tokens": phrase_tokens if arm == "phrase" else 200.0}
            for arm in ("csharp", "phrase") for model in ("delta", "epsilon", "zeta")
            for kind in ("write", "modify")
        ]}


if __name__ == "__main__":
    unittest.main()
