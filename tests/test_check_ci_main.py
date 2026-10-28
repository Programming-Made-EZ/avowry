import importlib.util
import unittest
from pathlib import Path

spec = importlib.util.spec_from_file_location("check_ci_main", Path(__file__).resolve().parents[1] / "scripts" / "check-ci-main.py")
ci = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ci)


def run(status, conclusion, sha="abcdef1234"):
    return {"status": status, "conclusion": conclusion, "headSha": sha, "workflowName": "ci", "url": "u"}


class CheckCiMainTests(unittest.TestCase):
    def test_red_latest_finished_run_stops_the_line(self):
        self.assertEqual("red", ci.verdict([run("completed", "failure")])[0])
        self.assertEqual(1, ci.main([], runner=lambda a: [run("completed", "failure")]))

    def test_in_progress_run_does_not_hide_an_earlier_red_one(self):
        self.assertEqual("red", ci.verdict([run("in_progress", ""), run("completed", "failure")])[0])

    def test_green_and_pending_pass(self):
        self.assertEqual("green", ci.verdict([run("completed", "success")])[0])
        self.assertEqual("pending", ci.verdict([run("in_progress", ""), run("completed", "success")])[0])
        self.assertEqual(0, ci.main([], runner=lambda a: [run("completed", "success")]))

    def test_newest_cancelled_run_is_red_but_older_cancelled_runs_are_skipped(self):
        self.assertEqual("red", ci.verdict([run("completed", "cancelled"), run("completed", "success")])[0])
        self.assertEqual("green", ci.verdict([run("completed", "success"), run("completed", "cancelled")])[0])

    def test_unreadable_status_is_not_green(self):
        def broken(args):
            raise RuntimeError("no network")
        self.assertEqual(2, ci.main([], runner=broken))


if __name__ == "__main__":
    unittest.main()
