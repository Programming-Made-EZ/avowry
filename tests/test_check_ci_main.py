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
        self.assertEqual(1, ci.main([], runner=lambda a: [run("completed", "failure")], head=lambda b: "abcdef1234"))

    def test_in_progress_run_does_not_hide_an_earlier_red_one(self):
        self.assertEqual("red", ci.verdict([run("in_progress", ""), run("completed", "failure")])[0])

    def test_green_and_pending_pass(self):
        self.assertEqual("green", ci.verdict([run("completed", "success")])[0])
        self.assertEqual("pending", ci.verdict([run("in_progress", ""), run("completed", "success")])[0])
        self.assertEqual(0, ci.main([], runner=lambda a: [run("completed", "success")], head=lambda b: "abcdef1234"))

    def test_newest_cancelled_run_is_red_but_older_cancelled_runs_are_skipped(self):
        self.assertEqual("red", ci.verdict([run("completed", "cancelled"), run("completed", "success")])[0])
        self.assertEqual("green", ci.verdict([run("completed", "success"), run("completed", "cancelled")])[0])

    def test_unreadable_status_is_not_green(self):
        def broken(args):
            raise RuntimeError("no network")
        self.assertEqual(2, ci.main([], runner=broken, head=lambda b: "abcdef1234"))

    def test_non_dict_head_answer_is_unreadable_exit_2_not_a_headless_fallback(self):
        def runner(args):
            return [run("completed", "success")]  # a list for the head lookup too
        self.assertEqual(2, ci.main([], runner=runner))

    def test_empty_or_non_string_head_is_unreadable_exit_2_and_never_judges_headless(self):
        calls = []
        def runner(args):
            calls.append(args)
            return [run("completed", "success", sha="")]
        for bad in ("", None, 0, ["abcdef1234"]):
            calls.clear()
            self.assertEqual(2, ci.main([], runner=runner, head=lambda b, bad=bad: bad), repr(bad))
            self.assertEqual([], calls, repr(bad))

    def test_newer_successful_ci_rerun_beats_older_failed_attempt_for_same_sha(self):
        old = dict(run("completed", "failure"), databaseId=1, createdAt="2026-01-01T00:00:00Z")
        new = dict(run("completed", "success"), databaseId=2, createdAt="2026-01-01T00:10:00Z")
        for runs in ([old, new], [new, old]):
            self.assertEqual("green", ci.head_verdict(runs, "abcdef1234")[0])

    def test_older_success_with_newer_failed_ci_attempt_is_red(self):
        old = dict(run("completed", "success"), databaseId=1, createdAt="2026-01-01T00:00:00Z")
        new = dict(run("completed", "failure"), databaseId=2, createdAt="2026-01-01T00:10:00Z")
        for runs in ([old, new], [new, old]):
            self.assertEqual("red", ci.head_verdict(runs, "abcdef1234")[0])

    def test_completed_ci_with_skipped_conclusion_exits_2(self):
        self.assertEqual("unknown", ci.verdict([run("completed", "skipped")], head="abcdef1234")[0])
        self.assertEqual(2, ci.main([], runner=lambda a: [run("completed", "skipped")], head=lambda b: "abcdef1234"))


# --- ci-head-1: verdict is judged for the CURRENT HEAD commit, not the newest run of any workflow ---
# ASSUMED INTERFACE (not yet implemented in scripts/check-ci-main.py):
#   verdict(runs, head=SHA)                -> head is the branch's current head SHA
#   main(argv, runner=..., sleep=..., head=callable(branch) -> SHA)   -> injectable head resolver
# Rules: ci run for head must be completed+success; every run for head must be completed;
# any queued/in_progress run for head -> pending; any failed run for head -> red; no ci run for head -> pending.
# Helpers fall back to the current signatures on TypeError so the tests fail by ASSERTION, not exception.
import contextlib
import io

H = "c20bc15aaaa"
OLD = "0ld0ld0ld0"


def wf(name, status, conclusion, sha=H):
    return {"status": status, "conclusion": conclusion, "headSha": sha, "workflowName": name, "url": "u"}


def head_verdict(runs, head=H):
    try:
        return ci.verdict(runs, head=head)[0]
    except TypeError:
        return ci.verdict(runs)[0]


def head_main(runs, head=H, resolver=None):
    resolver = resolver or (lambda branch: head)
    out = io.StringIO()
    with contextlib.redirect_stdout(out):
        try:
            code = ci.main([], runner=lambda a: runs, head=resolver)
        except TypeError:
            code = ci.main([], runner=lambda a: runs)
    return code, out.getvalue()


class CheckCiHeadTests(unittest.TestCase):
    # Zero
    def test_zero_no_ci_run_for_head_is_pending(self):
        self.assertEqual("pending", head_verdict([wf("mob-process", "completed", "success")]))

    def test_zero_only_previous_head_runs_is_pending(self):
        self.assertEqual("pending", head_verdict([wf("ci", "completed", "success", OLD)]))

    # One
    def test_one_ci_success_for_head_alone_is_green(self):
        self.assertEqual("green", head_verdict([wf("ci", "completed", "success")]))

    # Many
    def test_many_ci_success_but_other_workflow_in_progress_is_pending(self):
        runs = [wf("ci", "completed", "success"), wf("mob-process", "in_progress", "")]
        self.assertEqual("pending", head_verdict(runs))

    def test_many_ci_success_but_other_workflow_queued_is_pending(self):
        runs = [wf("ci", "completed", "success"), wf("pages", "queued", "")]
        self.assertEqual("pending", head_verdict(runs))

    def test_many_ci_success_but_other_workflow_failed_is_red(self):
        runs = [wf("ci", "completed", "success"), wf("mob-process", "completed", "failure")]
        self.assertEqual("red", head_verdict(runs))

    # Interface / the exact c20bc15 case
    def test_c20bc15_newest_mob_process_green_but_ci_in_progress_is_not_green(self):
        runs = [wf("mob-process", "completed", "success"), wf("ci", "in_progress", "")]
        self.assertNotEqual("green", head_verdict(runs))
        self.assertEqual("pending", head_verdict(runs))

    def test_c20bc15_main_does_not_print_green(self):
        runs = [wf("mob-process", "completed", "success"), wf("ci", "in_progress", "")]
        code, out = head_main(runs)
        self.assertNotIn("green", out)
        self.assertIn("pending", out)

    def test_main_green_for_head_prints_green_and_exits_0(self):
        code, out = head_main([wf("ci", "completed", "success")])
        self.assertEqual(0, code)
        self.assertIn("green", out)

    # Boundaries
    def test_boundary_older_green_ci_does_not_make_new_head_green(self):
        runs = [wf("ci", "completed", "success", OLD)]
        self.assertEqual("pending", head_verdict(runs))
        self.assertNotIn("green", head_main(runs)[1])

    def test_boundary_older_green_ci_with_current_ci_in_progress_is_pending(self):
        runs = [wf("ci", "in_progress", ""), wf("ci", "completed", "success", OLD)]
        self.assertEqual("pending", head_verdict(runs))

    def test_boundary_failed_ci_for_head_is_red_despite_older_green(self):
        runs = [wf("ci", "completed", "failure"), wf("ci", "completed", "success", OLD)]
        self.assertEqual("red", head_verdict(runs))
        self.assertEqual(1, head_main(runs)[0])

    # Exceptions
    def test_exception_head_resolver_failure_is_unreadable_exit_2(self):
        def boom(branch):
            raise RuntimeError("cannot resolve head")
        self.assertEqual(2, head_main([wf("ci", "completed", "success")], resolver=boom)[0])

    def test_exception_runner_failure_with_head_is_exit_2(self):
        def broken(args):
            raise RuntimeError("no network")
        self.assertEqual(2, head_main_broken(broken))


def head_main_broken(runner):
    with contextlib.redirect_stdout(io.StringIO()):
        try:
            return ci.main([], runner=runner, head=lambda b: H)
        except TypeError:
            return ci.main([], runner=runner)


if __name__ == "__main__":
    unittest.main()
