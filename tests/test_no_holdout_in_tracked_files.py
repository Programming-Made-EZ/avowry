import importlib.util
import subprocess
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location(
    "check_no_holdout_in_tracked_files",
    ROOT / "scripts" / "check-no-holdout-in-tracked-files.py",
)
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)


class NoHoldoutInTrackedFilesTests(unittest.TestCase):
    def init_repo(self, repo: Path) -> None:
        subprocess.run(["git", "init", "-q"], cwd=repo, check=True, capture_output=True)

    def track(self, repo: Path) -> None:
        subprocess.run(["git", "-C", str(repo), "add", "-A"], check=True, capture_output=True)

    def test_this_repository_has_no_tracked_holdout(self):
        # Given this public repository
        # When the holdout check reads its tracked files
        # Then no holdout path or holdout-labelled results row is tracked
        self.assertEqual([], check.find_violations(ROOT))
        self.assertEqual(0, check.main([str(ROOT)]))

    def test_fails_when_a_tracked_file_is_under_bench_holdouts_unsealed(self):
        # Given a temp git repo with a tracked file under bench/holdouts-unsealed
        with tempfile.TemporaryDirectory() as tmp:
            repo = Path(tmp)
            self.init_repo(repo)
            holdout = repo / "bench" / "holdouts-unsealed" / "H1-write.md"
            holdout.parent.mkdir(parents=True)
            holdout.write_text("invented holdout text\n", encoding="utf-8")
            self.track(repo)

            # When the check runs
            violations = check.find_violations(repo)

            # Then it fails and names that path
            self.assertTrue(any("bench/holdouts-unsealed/" in item for item in violations))
            self.assertEqual(1, check.main([str(repo)]))

    def test_fails_when_a_tracked_results_row_label_starts_with_holdout(self):
        # Given a tracked results row whose label starts with holdout
        with tempfile.TemporaryDirectory() as tmp:
            repo = Path(tmp)
            self.init_repo(repo)
            history = repo / "bench" / "results" / "history.jsonl"
            history.parent.mkdir(parents=True)
            history.write_text(
                '{"model":"fixture","task":"H1","label":"baseline-r1","passed":true}\n'
                '{"model":"fixture","task":"H1","label":"Holdout-r1","reply_head":"secret reply"}\n',
                encoding="utf-8",
            )
            self.track(repo)

            # When the check runs
            violations = check.find_violations(repo)

            # Then it fails on that row
            self.assertTrue(any("Holdout-r1" in item or "holdout" in item.lower() for item in violations))
            self.assertEqual(1, check.main([str(repo)]))

    def test_fails_when_a_tracked_results_row_has_a_holdout_task_id(self):
        # Given a tracked results row with an H-number task id and an ordinary label
        with tempfile.TemporaryDirectory() as tmp:
            repo = Path(tmp)
            self.init_repo(repo)
            history = repo / "bench" / "results" / "history.jsonl"
            history.parent.mkdir(parents=True)
            history.write_text(
                '{"model":"fixture","task":"H9","label":"mistaken-run","reply_head":"secret reply"}\n',
                encoding="utf-8",
            )
            self.track(repo)

            # When the check runs
            violations = check.find_violations(repo)

            # Then it fails on the H-number task even though the label is not holdout-like
            self.assertTrue(any("H9" in item for item in violations))
            self.assertEqual(1, check.main([str(repo)]))

    def test_accepts_an_untracked_holdout_path_and_a_label_that_does_not_start_with_holdout(self):
        # Given a tracked baseline row and an untracked file under bench/holdouts-unsealed
        with tempfile.TemporaryDirectory() as tmp:
            repo = Path(tmp)
            self.init_repo(repo)
            history = repo / "bench" / "csharp-baseline" / "results" / "history.jsonl"
            history.parent.mkdir(parents=True)
            history.write_text('{"label":"my-holdout-note","task":"001-write-greeting"}\n', encoding="utf-8")
            self.track(repo)
            leaked = repo / "bench" / "holdouts-unsealed" / "H2-modify.md"
            leaked.parent.mkdir(parents=True)
            leaked.write_text("not tracked\n", encoding="utf-8")

            # When the check runs
            # Then the untracked path and the non-prefix label are allowed
            self.assertEqual([], check.find_violations(repo))
            self.assertEqual(0, check.main([str(repo)]))


if __name__ == "__main__":
    unittest.main()
