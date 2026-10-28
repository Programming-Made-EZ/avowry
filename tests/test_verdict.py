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
        # Given a numbers-only holdout summary where Copilot is wholly unavailable and Grok has an unavailable group
        holdout = {"groups": [
            {"arm": arm, "model": model, "kind": kind, "count": 5,
             "completed_count": (0 if model == "copilot" or (model == "grok" and arm == "phrase" and kind == "modify") else 5),
             "unavailable_count": (5 if model == "copilot" or (model == "grok" and arm == "phrase" and kind == "modify") else 0),
             "pass_rate": 0.0 if model == "copilot" else 1.0,
             "mean_attempts": 1.0, "mean_total_tokens": 100.0}
            for arm in ("csharp", "phrase") for model in ("codex", "grok", "copilot")
            for kind in ("write", "modify")
        ]}
        # When the holdout is judged on the same frozen bar
        page = self.run_verdict(self.rows("csharp", tokens=200), self.rows("phrase", tokens=100), holdout)
        # Then Steve sees both frozen-bar completeness reasons
        self.assertTrue(page.startswith("INCONCLUSIVE "), page)
        self.assertIn("only 1 usable model", page)
        self.assertIn("holdout harness-error rows are", page)

    def test_fully_completed_three_model_holdout_uses_the_performance_bar(self):
        # Given all holdout groups completed for three models
        holdout = {"groups": [
            {"arm": arm, "model": model, "kind": kind, "count": 5,
             "completed_count": 5, "unavailable_count": 0, "pass_rate": 1.0,
             "mean_attempts": 1.0, "mean_total_tokens": 100.0}
            for arm in ("csharp", "phrase") for model in ("codex", "grok", "copilot")
            for kind in ("write", "modify")
        ]}
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
