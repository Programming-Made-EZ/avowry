"""Tests for scripts/board/apply_value_verdicts.py: validate, plan, apply and the CLI, all with injected runners (no network, no real gh)."""

import copy
import io
import json
import tempfile
import unittest
from contextlib import redirect_stderr, redirect_stdout
from pathlib import Path

from scripts.board import apply_value_verdicts as avv

REPO = Path(__file__).resolve().parents[1]
RECORDS = REPO / ".mob" / "records" / "reranks"
PERSONA = "Ste" + "ve"
ORDER = [17, 16, 12, 13, 14, 15, 20, 21]


def snap_issue(title, type_=""):
    return {"title": title, "state": "open", "status": "Ready", "shape": "", "type": type_, "actor_or_story": "", "body_sha256": "0" * 64}


def make_snapshot():
    snap = {str(n): snap_issue(f"issue {n}") for n in range(10, 22)}
    snap["17"]["type"] = "bug"
    snap["18"]["type"] = "bug"
    return snap


def make_verdicts():
    return {
        "schema": 1,
        "window": "rerank-test",
        "valid_consumers": ["product owner", "Morgan", "Dana", "Ari"],
        "closed_ok_survivors": [99],
        "close": [
            {"issue": 10, "kind": "waste", "reason": "no consumer outcome is stated"},
            {"issue": 11, "kind": "duplicate", "reason": "duplicate of 12", "survivor": 12},
            {"issue": 19, "kind": "duplicate", "reason": "duplicate of 99, already shipped", "survivor": 99},
        ],
        "edit_contract": [
            {"issue": 13, "consumer": "product owner", "value": "so that the laptop is never disrupted."},
            {"issue": 20, "consumer": "Morgan", "value": "so that reviews are faster."},
        ],
        "edit_story": [{"issue": 14, "story_line": "As Dana, I want a clear card, so that I can rank it."}],
        "edit_unshaped": [{"issue": 15, "story_line": "As Ari, I want a quick spec, so that I spend fewer tokens."}],
        "order": {"after": 298, "first_product_card": 16, "max_bugs_above": 1, "cards": list(ORDER)},
    }


def errors_for(mutate):
    v = make_verdicts()
    mutate(v)
    return avv.validate(v, make_snapshot())


def text_of(errors):
    return " ".join(errors).lower()


class ValidateTests(unittest.TestCase):
    def test_valid_fixture_has_no_errors(self):
        self.assertEqual(avv.validate(make_verdicts(), make_snapshot()), [])

    def test_consumer_not_in_valid_consumers(self):
        errs = errors_for(lambda v: v["edit_contract"][0].update(consumer="Zed"))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("13", t)
        self.assertIn("consumer", t)

    def test_mob_role_as_consumer(self):
        for role in ["facilitator", "driver", "reviewer seat", "agile coach", "audit lens", "Decider", "script", "CI", "board", "FACILITATOR"]:
            with self.subTest(role=role):
                def mutate(v, role=role):
                    v["valid_consumers"].append(role)
                    v["edit_contract"][0]["consumer"] = role
                errs = errors_for(mutate)
                self.assertTrue(errs)
                t = text_of(errs)
                self.assertIn("13", t)
                self.assertIn("role", t)

    def test_mob_role_as_story_actor(self):
        for role in ["facilitator", "Driver", "reviewer seat", "agile coach", "audit lens", "decider", "script", "CI", "board"]:
            with self.subTest(role=role):
                line = f"As the {role}, I want a clear card, so that I can rank it."
                errs = errors_for(lambda v, line=line: v["edit_story"][0].update(story_line=line))
                self.assertTrue(errs)
                t = text_of(errs)
                self.assertIn("14", t)
                self.assertIn("role", t)

    def test_mob_role_as_unshaped_actor(self):
        errs = errors_for(lambda v: v["edit_unshaped"][0].update(story_line="As the facilitator, I want x, so that y."))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("15", t)
        self.assertIn("role", t)

    def test_contract_value_empty(self):
        errs = errors_for(lambda v: v["edit_contract"][0].update(value=""))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("13", t)
        self.assertIn("value", t)

    def test_contract_value_without_so_that(self):
        errs = errors_for(lambda v: v["edit_contract"][0].update(value="the laptop is never disrupted"))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("13", t)
        self.assertIn("so that", t)

    def test_story_line_without_so_that(self):
        errs = errors_for(lambda v: v["edit_story"][0].update(story_line="As Dana, I want a clear card."))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("14", t)
        self.assertIn("so that", t)

    def test_unshaped_line_without_so_that(self):
        errs = errors_for(lambda v: v["edit_unshaped"][0].update(story_line="As Ari, I want a quick spec."))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("15", t)
        self.assertIn("so that", t)

    def test_closure_with_empty_reason(self):
        errs = errors_for(lambda v: v["close"][0].update(reason="  "))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("10", t)
        self.assertIn("reason", t)

    def test_duplicate_survivor_closed_as_waste_in_same_file(self):
        errs = errors_for(lambda v: v["close"][1].update(survivor=10))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("11", t)
        self.assertIn("survivor", t)

    def test_duplicate_survivor_not_in_snapshot_or_known_closed(self):
        errs = errors_for(lambda v: v["close"][1].update(survivor=555))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("11", t)
        self.assertIn("survivor", t)

    def test_duplicate_survivor_known_closed_is_accepted_only_when_listed(self):
        # issue 19 has survivor 99, which is only in closed_ok_survivors
        self.assertEqual(avv.validate(make_verdicts(), make_snapshot()), [])
        v = make_verdicts()
        v["closed_ok_survivors"] = []
        errs = avv.validate(v, make_snapshot())
        self.assertTrue(errs)
        self.assertIn("19", text_of(errs))

    def test_issue_both_closed_and_ordered(self):
        errs = errors_for(lambda v: v["order"]["cards"].append(10))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("10", t)
        self.assertIn("closed", t)
        self.assertIn("order", t)

    def test_too_many_bugs_above_first_product_card(self):
        errs = errors_for(lambda v: v["order"].update(max_bugs_above=0))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("bug", t)
        self.assertIn("first product card", t)

    def test_bugs_after_first_product_card_do_not_count(self):
        def mutate(v):
            v["order"]["cards"] = [17, 16, 18, 12]
            v["order"]["max_bugs_above"] = 1
        self.assertEqual(errors_for(mutate), [])

    def test_issue_absent_from_snapshot(self):
        errs = errors_for(lambda v: v["edit_story"][0].update(issue=777))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("777", t)
        self.assertIn("snapshot", t)

    def test_issue_not_open_in_snapshot(self):
        snap = make_snapshot()
        snap["13"]["state"] = "closed"
        errs = avv.validate(make_verdicts(), snap)
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("13", t)
        self.assertIn("open", t)

    def test_issue_closed_twice(self):
        errs = errors_for(lambda v: v["close"].append({"issue": 10, "kind": "waste", "reason": "again"}))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("10", t)
        self.assertIn("twice", t)

    def test_issue_edited_twice(self):
        errs = errors_for(lambda v: v["edit_story"].append({"issue": 13, "story_line": "As Dana, I want z, so that w."}))
        self.assertTrue(errs)
        t = text_of(errs)
        self.assertIn("13", t)
        self.assertIn("twice", t)


class PlanTests(unittest.TestCase):
    def test_plan_is_deterministic_and_ordered(self):
        a = avv.plan(make_verdicts(), make_snapshot())
        b = avv.plan(copy.deepcopy(make_verdicts()), make_snapshot())
        self.assertEqual(a, b)
        self.assertEqual(len(a), 3 + 2 + 1 + 1 + 1)
        self.assertEqual([x["op"] for x in a[:3]], ["close"] * 3)
        self.assertEqual([x["issue"] for x in a[:3]], [10, 11, 19])
        edits = a[3:-1]
        self.assertTrue(all(x["op"] == "edit" for x in edits))
        self.assertEqual([x["issue"] for x in edits], [13, 14, 15, 20])
        self.assertEqual(sum(1 for x in a if x["op"] == "rerank"), 1)
        self.assertEqual(a[-1], {"op": "rerank", "after": 298, "cards": ORDER})

    def test_closure_comment_names_reason_and_not_planned(self):
        first = avv.plan(make_verdicts(), make_snapshot())[0]
        self.assertEqual(first["reason"], "no consumer outcome is stated")
        self.assertIn("no consumer outcome is stated", first["comment"])
        self.assertIn("not planned", first["comment"])

    def test_contract_edit_line_writes_persona_for_product_owner(self):
        by_issue = {x["issue"]: x for x in avv.plan(make_verdicts(), make_snapshot()) if x["op"] == "edit"}
        self.assertEqual(by_issue[13]["add_line"], f"- **Consumer value:** {PERSONA}: so that the laptop is never disrupted.")
        self.assertEqual(by_issue[20]["add_line"], "- **Consumer value:** Morgan: so that reviews are faster.")

    def test_story_and_unshaped_edits_carry_story_line(self):
        by_issue = {x["issue"]: x for x in avv.plan(make_verdicts(), make_snapshot()) if x["op"] == "edit"}
        self.assertEqual(by_issue[14]["story_line"], "As Dana, I want a clear card, so that I can rank it.")
        self.assertEqual(by_issue[15]["story_line"], "As Ari, I want a quick spec, so that I spend fewer tokens.")

    def test_invalid_verdicts_raise_with_error_list(self):
        v = make_verdicts()
        v["close"][0]["reason"] = ""
        with self.assertRaises(avv.InvalidVerdicts) as ctx:
            avv.plan(v, make_snapshot())
        errs = getattr(ctx.exception, "errors", None)
        self.assertTrue(errs)
        self.assertIn("10", text_of(errs))


class FakeGH:
    """Records every call. State and body views are answered from dicts; a close call flips the state unless told not to."""

    def __init__(self, plan, states=None, raise_on=None, close_has_no_effect=(), missing_line=()):
        self.calls = []
        self.states = {a["issue"]: "OPEN" for a in plan if a["op"] != "rerank"}
        self.states.update(states or {})
        self.lines = {a["issue"]: a.get("add_line") or a.get("story_line") for a in plan if a["op"] == "edit"}
        self.raise_on = raise_on  # (verb, issue) whose call raises
        self.close_has_no_effect = set(close_has_no_effect)
        self.missing_line = set(missing_line)

    def __call__(self, args):
        self.calls.append([str(x) for x in args])
        args = [str(x) for x in args]
        nums = [int(x) for x in args if x.isdigit()]
        n = nums[0] if nums else None
        is_view = "view" in args
        verb = "view" if is_view else ("close" if "close" in args else ("edit" if "edit" in args else "other"))
        if self.raise_on == (verb, n):
            raise RuntimeError(f"gh failed: {verb} {n}")
        if is_view and "state" in args:
            return self.states.get(n, "OPEN") + "\n"
        if is_view and "body" in args:
            if n in self.missing_line or n not in self.lines:
                return "old body without the new line\n"
            return f"old body\n{self.lines[n]}\n"
        if verb == "close" and n not in self.close_has_no_effect:
            self.states[n] = "CLOSED"
        return ""

    def mutating(self):
        return [c for c in self.calls if "view" not in c]

    def touched(self):
        return {int(x) for c in self.mutating() for x in c if x.isdigit()}


class FakeRerank:
    def __init__(self):
        self.calls = []

    def __call__(self, after, cards):
        self.calls.append((after, list(cards)))


def run_apply(plan, gh):
    rerank = FakeRerank()
    with tempfile.TemporaryDirectory() as d:
        log = Path(d) / "log.json"
        result = avv.apply(plan, gh, rerank, str(log))
        data = json.loads(log.read_text(encoding="utf-8"))
    return result, data, rerank


def simple_plan():
    return avv.plan(make_verdicts(), make_snapshot())


class ApplyTests(unittest.TestCase):
    def test_success_runs_everything_once_in_order(self):
        plan = simple_plan()
        gh = FakeGH(plan)
        result, log, rerank = run_apply(plan, gh)
        self.assertTrue(result["ok"])
        self.assertEqual(rerank.calls, [(298, ORDER)])
        closes = [c for c in gh.mutating() if "close" in c]
        edits = [c for c in gh.mutating() if "edit" in c]
        self.assertEqual(len(closes), 3)
        self.assertEqual(len(edits), 4)
        self.assertLess(gh.calls.index(closes[-1]), gh.calls.index(edits[0]))
        logged = [a["issue"] for a in log["actions"] if a.get("op") != "rerank"]
        self.assertEqual(logged, [10, 11, 19, 13, 14, 15, 20])
        for entry in log["actions"]:
            self.assertIn("status", entry)

    def test_failing_close_stops_and_skips_rerank(self):
        plan = simple_plan()
        gh = FakeGH(plan, raise_on=("close", 11))
        result, log, rerank = run_apply(plan, gh)
        self.assertFalse(result["ok"])
        self.assertEqual(result["issue"], 11)
        self.assertEqual(rerank.calls, [])
        self.assertNotIn(19, gh.touched())
        self.assertNotIn(13, gh.touched())
        self.assertTrue(any(a.get("issue") == 11 and a.get("status") == "failed" for a in log["actions"]))
        self.assertFalse(any(a.get("issue") == 19 for a in log["actions"]))

    def test_close_readback_not_closed_stops(self):
        plan = simple_plan()
        gh = FakeGH(plan, close_has_no_effect={10})
        result, log, rerank = run_apply(plan, gh)
        self.assertFalse(result["ok"])
        self.assertEqual(result["issue"], 10)
        self.assertEqual(rerank.calls, [])
        self.assertNotIn(11, gh.touched())

    def test_failing_edit_readback_stops_and_skips_rerank(self):
        plan = simple_plan()
        gh = FakeGH(plan, missing_line={13})
        result, log, rerank = run_apply(plan, gh)
        self.assertFalse(result["ok"])
        self.assertEqual(result["issue"], 13)
        self.assertEqual(rerank.calls, [])
        self.assertNotIn(14, gh.touched())
        self.assertNotIn(15, gh.touched())
        self.assertTrue(any(a.get("issue") == 13 and a.get("status") == "failed" for a in log["actions"]))

    def test_failing_edit_call_stops(self):
        plan = simple_plan()
        gh = FakeGH(plan, raise_on=("edit", 14))
        result, log, rerank = run_apply(plan, gh)
        self.assertFalse(result["ok"])
        self.assertEqual(result["issue"], 14)
        self.assertEqual(rerank.calls, [])
        self.assertNotIn(15, gh.touched())

    def test_already_closed_issue_is_skipped_not_an_error(self):
        plan = simple_plan()
        gh = FakeGH(plan, states={10: "CLOSED"})
        result, log, rerank = run_apply(plan, gh)
        self.assertTrue(result["ok"])
        self.assertNotIn(10, gh.touched())
        entry = [a for a in log["actions"] if a.get("issue") == 10][0]
        self.assertEqual(entry["status"], "skipped")
        self.assertEqual(len(rerank.calls), 1)


class CliTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.dir = Path(self.tmp.name)
        self.snap = self.dir / "snap.json"
        self.snap.write_text(json.dumps({"schema": 1, "taken": "x", "issues": make_snapshot()}), encoding="utf-8")

    def write_verdicts(self, verdicts):
        path = self.dir / "verdicts.json"
        path.write_text(json.dumps(verdicts), encoding="utf-8")
        return str(path)

    @staticmethod
    def boom(*args, **kwargs):
        raise AssertionError("must not be called")

    def run_main(self, argv, **kw):
        out, err = io.StringIO(), io.StringIO()
        with redirect_stdout(out), redirect_stderr(err):
            code = avv.main(argv, **kw)
        return code, out.getvalue(), err.getvalue()

    def test_dry_run_prints_one_line_per_action_and_calls_nothing(self):
        argv = ["--verdicts", self.write_verdicts(make_verdicts()), "--snapshot", str(self.snap)]
        code, out, _ = self.run_main(argv, gh=self.boom, rerank=self.boom)
        self.assertEqual(code, 0)
        lines = [ln for ln in out.splitlines() if ln.strip()]
        self.assertEqual(len(lines), len(simple_plan()))

    def test_invalid_verdicts_return_nonzero_and_call_nothing(self):
        bad = make_verdicts()
        bad["close"][0]["reason"] = ""
        path = self.write_verdicts(bad)
        for extra in ([], ["--apply", "--log", str(self.dir / "log.json")]):
            with self.subTest(extra=extra):
                code, out, err = self.run_main(["--verdicts", path, "--snapshot", str(self.snap)] + extra, gh=self.boom, rerank=self.boom)
                self.assertNotEqual(code, 0)
                shown = out + err
                self.assertIn("10", shown)
                self.assertIn("reason", shown.lower())

    def test_apply_calls_apply_with_injected_runners(self):
        gh = FakeGH(simple_plan())
        rerank = FakeRerank()
        log = self.dir / "log.json"
        argv = ["--verdicts", self.write_verdicts(make_verdicts()), "--snapshot", str(self.snap), "--apply", "--log", str(log)]
        code, _, _ = self.run_main(argv, gh=gh, rerank=rerank)
        self.assertEqual(code, 0)
        self.assertEqual(len(rerank.calls), 1)
        self.assertTrue(gh.mutating())
        self.assertTrue(log.exists())


class RealArtifactTests(unittest.TestCase):
    def test_committed_verdicts_are_valid_and_plan_has_49_actions(self):
        verdicts = json.loads((RECORDS / "rerank-26-verdicts.json").read_text(encoding="utf-8"))
        snapshot = json.loads((RECORDS / "rerank-26-open-issues.json").read_text(encoding="utf-8"))["issues"]
        self.assertEqual(avv.validate(verdicts, snapshot), [])
        plan = avv.plan(verdicts, snapshot)
        self.assertEqual(len(verdicts["close"]), 32)
        self.assertEqual(len(verdicts["edit_contract"]), 12)
        self.assertEqual(len(verdicts["edit_story"]), 2)
        self.assertEqual(len(verdicts["edit_unshaped"]), 2)
        self.assertEqual(len(plan), 32 + 16 + 1)
        self.assertEqual(plan[-1]["op"], "rerank")
        self.assertEqual(sum(1 for a in plan if a["op"] == "close"), 32)


if __name__ == "__main__":
    unittest.main()
