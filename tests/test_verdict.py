import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / "scripts" / "verdict.py"


class VerdictPageTests(unittest.TestCase):
    def run_verdict(self, csharp_rows, phrase_rows, holdout=None):
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
        page = self.run_verdict(csharp, phrase)
        # Then the frozen bar calls the result a win
        self.assertTrue(page.startswith("WIN "), page)

    def test_loss_for_pass_gap_over_ten_points(self):
        # Given Phrase is twenty points behind plain C#
        page = self.run_verdict(self.rows("csharp", 5, 200), self.rows("phrase", 4, 100))
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
        page = self.run_verdict(self.rows("csharp", 5, 200), phrase)
        self.assertTrue(page.startswith("TIE "), page)

    def test_inconclusive_for_too_many_harness_errors(self):
        # Given more than ten percent of scheduled rows are harness errors
        phrase = self.rows("phrase", tokens=100, harness_errors=2)
        # When and then completeness is checked before performance
        page = self.run_verdict(self.rows("csharp", tokens=200), phrase)
        self.assertTrue(page.startswith("INCONCLUSIVE "), page)
        self.assertIn("harness-error", page)

    def test_inconclusive_holdout_names_unavailable_drivers_and_resets(self):
        # Given a numbers-only holdout summary with only two usable models
        holdout = {"groups": [
            {"arm": arm, "model": model, "kind": kind, "count": 5,
             "pass_rate": 0.0 if model == "copilot" else 1.0,
             "mean_attempts": 1.0, "mean_total_tokens": 100.0}
            for arm in ("csharp", "phrase") for model in ("codex", "grok", "copilot")
            for kind in ("write", "modify")
        ], "harness_error_share": 0.4}
        # When the holdout is judged on the same frozen bar
        page = self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), holdout)
        # Then it is inconclusive and tells Steve the concrete availability reasons
        self.assertTrue(page.startswith("INCONCLUSIVE "), page)
        self.assertIn("Copilot has no scored holdout rows", page)
        self.assertIn("Grok's holdout runs timed out", page)
        self.assertIn("2026-11-01 00:00 UTC", page)
        self.assertIn("2026-11-01 21:40 UTC", page)

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
        page = self.run_verdict(self.rows("csharp", tokens=300), self.rows("phrase", tokens=1183))
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
        # Then today's development-only data is a TIE and no stored task text leaks
        self.assertTrue(page.startswith("TIE "), page)
        self.assertNotIn("001-write-greeting", page)
        self.assertNotIn("returns hello", page)
        self.assertNotIn("bench/", page)


if __name__ == "__main__":
    unittest.main()
