"""Card 217 red 3 (D157 Criterion 3): same-head run-pair comparison and thread-pool floor decision.

scripts/check-run-pairs-217.py (does not exist yet) reads .mob/records/217/run-pairs.json and FAILS unless, for
each environment ('local' and 'ci'), two consecutive full runs at one head give IDENTICAL per-test outcome lists,
the only failures are allowed ones with an open owner card, and the floor decision is recorded and consistent.

  python scripts/check-run-pairs-217.py [--root DIR]
  exit 0 clean ("run-pairs: OK"), 1 findings ("<path>: <keyword> <detail>" on stderr), 2 not a directory.
  Finding keywords: missing, malformed, pair, differ, allowed, owner, head, empty, temp, ci, floor.
  Never a traceback. Writes nothing. Paths in the record are relative to --root.

Record shape (.mob/records/217/run-pairs.json):
  {"card": 217, "head": "<40-hex sha the pairs ran at>", "min_tests": <int >= 1, fewer tests in a list is a finding>,
   "environments": {
     "local": {"runs": [<run>, <run>],
               "allowed_failures": [{"test": "<fully qualified name>", "owner_card": <int of an open card>}, ...]},
     "ci":    {"runs": [<run>, <run>], "allowed_failures": [...]}},   # ci: the three or none
   "floor_decision": {"decision": "removed" | "kept", "reason": "<non-empty text>"}}
  local run: {"outcomes": "<path of committed JSON {fully qualified test name: outcome}, derived from the TRX>",
              "head": "<sha>", "id": {"trx": "<file name>", "timestamp": "<ISO time>"},
              "temp": "<TEMP used>", "tmp": "<TMP used>"}
  ci run:    {"outcomes": "<path>", "head": "<sha>", "id": {"github_run_id": "<id>", "head_sha": "<sha>"}}
Rules: exactly two runs per environment; outcome lists non-empty and at least min_tests entries; both lists of an
environment have the same test names and the same outcome for every test (a different failure set in run 2 is
'differ'); every non-'Passed' test must be in that environment's allowed_failures with an integer owner_card;
every run head, the CI id head_sha and the record head are one sha; local runs name temp and tmp (non-empty);
floor_decision present; 'kept' needs a non-empty reason; 'removed' is refused while
tests/Avowry.Acceptance.Tests/Support/ThreadPoolFloor.cs exists and contains ModuleInitializer.

Red shape (as card 264): the checker is not written, so every test fails on the missing script, and the real-record
test fails because .mob/records/217/run-pairs.json does not exist. Not wired into mob_process.py check.

ZOMBIES: Z test_zero_*, O test_one_*, M test_many_*, B test_boundary_*, I test_interface_*, E test_exception_*,
S test_simple_*. Fixture trees use tempfile.mkdtemp and are left in place (no cleanup, by the safety rule).
"""

import copy
import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
SCRIPT = REPO / "scripts" / "check-run-pairs-217.py"
REL = ".mob/records/217/run-pairs.json"
FLOOR = "tests/Avowry.Acceptance.Tests/Support/ThreadPoolFloor.cs"
HEAD = "a" * 40
OTHER = "b" * 40
T162 = "Avowry.Acceptance.Tests.UsableV1.Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone"
T303A = "Avowry.Acceptance.Tests.CSharpBaselineAcceptance.endpoint_reply_fails_when_response_record_or_status_are_wrong"
T303B = "Avowry.Acceptance.Tests.CSharpBaselineAcceptance.endpoint_reply_passes_when_response_record_and_status_match"
F065 = [T162, T303A, T303B]
OWNERS = {T162: 162, T303A: 303, T303B: 303}
PASSING = [f"Avowry.Acceptance.Tests.Fake.Test{i}" for i in range(6)]


def run(root):
    assert SCRIPT.exists(), "scripts/check-run-pairs-217.py does not exist yet (card 217 criterion 3 red)"
    p = subprocess.run([sys.executable, str(SCRIPT), "--root", str(root)], capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    return p.returncode, p.stdout, p.stderr


def outcomes(failing=()):
    d = {t: "Passed" for t in PASSING}
    for t in failing:
        d[t] = "Failed"
    return d


def good():
    """A passing fixture: (record, {relative path: outcome dict})."""
    files, runs = {}, {}
    for env, failing in (("local", F065), ("ci", ())):
        runs[env] = []
        for n in (1, 2):
            rel = f"evidence/{env}-{n}.json"
            files[rel] = outcomes(failing)
            run_rec = {"outcomes": rel, "head": HEAD}
            if env == "local":
                run_rec["id"] = {"trx": f"local-{n}.trx", "timestamp": f"2026-11-05T1{n}:00:00Z"}
                run_rec["temp"] = "C:/t/TEMP"
                run_rec["tmp"] = "C:/t/TEMP"
            else:
                run_rec["id"] = {"github_run_id": str(1000 + n), "head_sha": HEAD}
            runs[env].append(run_rec)
    allowed = [{"test": t, "owner_card": OWNERS[t]} for t in F065]
    rec = {
        "card": 217, "head": HEAD, "min_tests": 6,
        "environments": {"local": {"runs": runs["local"], "allowed_failures": copy.deepcopy(allowed)},
                         "ci": {"runs": runs["ci"], "allowed_failures": copy.deepcopy(allowed)}},
        "floor_decision": {"decision": "removed", "reason": "both no-floor run pairs are green"},
    }
    return rec, files


def tree(rec=None, files=None, floor=None, raw=None):
    root = Path(tempfile.mkdtemp(prefix="run-pairs-"))
    p = root / REL
    p.parent.mkdir(parents=True)
    if raw is not None:
        p.write_text(raw, encoding="utf-8")
    elif rec is not None:
        p.write_text(json.dumps(rec), encoding="utf-8")
    for rel, data in (files or {}).items():
        f = root / rel
        f.parent.mkdir(parents=True, exist_ok=True)
        f.write_text(data if isinstance(data, str) else json.dumps(data), encoding="utf-8")
    if floor is not None:
        f = root / FLOOR
        f.parent.mkdir(parents=True, exist_ok=True)
        f.write_text(floor, encoding="utf-8")
    return root


class RunPairs(unittest.TestCase):
    def reject(self, edit=None, keyword="", floor=None, raw=None, missing=False):
        rec, files = good()
        if edit:
            edit(rec, files)
        code, out, err = run(tree(None if missing else rec, files, floor, raw))
        self.assertEqual(code, 1, f"expected a finding, got exit {code}: {out}{err}")
        self.assertIn(keyword, err)

    def accept(self, edit=None, floor=None):
        rec, files = good()
        if edit:
            edit(rec, files)
        code, out, err = run(tree(rec, files, floor))
        self.assertEqual(code, 0, f"expected OK, got exit {code}: {err}")
        self.assertIn("run-pairs: OK", out)

    # --- Z: zero
    def test_zero_missing_record_is_a_finding(self):
        self.reject(keyword="missing", missing=True)

    def test_zero_runs_in_an_environment_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["ci"].update(runs=[]), "pair")

    def test_zero_empty_outcome_list_is_rejected(self):
        self.reject(lambda r, f: f.update({"evidence/ci-1.json": {}, "evidence/ci-2.json": {}}), "empty")

    def test_zero_failures_in_ci_is_accepted(self):
        self.accept()  # ci passes everything: "the three or none"

    def test_zero_missing_floor_decision_is_rejected(self):
        self.reject(lambda r, f: r.pop("floor_decision"), "floor")

    # --- O: one
    def test_one_run_is_not_a_pair(self):
        self.reject(lambda r, f: r["environments"]["local"]["runs"].pop(), "pair")

    def test_one_environment_only_is_rejected(self):
        self.reject(lambda r, f: r["environments"].pop("ci"), "ci")

    def test_one_unlisted_failure_is_rejected(self):
        def edit(r, f):
            for n in (1, 2):
                f[f"evidence/ci-{n}.json"]["Avowry.Acceptance.Tests.Fake.Test0"] = "Failed"
        self.reject(edit, "allowed")

    # --- M: many
    def test_many_all_four_runs_matching_is_accepted(self):
        self.accept()

    def test_many_three_runs_in_an_environment_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["ci"]["runs"].append(r["environments"]["ci"]["runs"][0]), "pair")

    def test_many_local_pair_matches_but_ci_pair_differs_is_rejected(self):
        self.reject(lambda r, f: f["evidence/ci-2.json"].update({PASSING[0]: "Failed"}), "differ")

    # --- B: boundary
    def test_boundary_second_run_fails_a_different_test_is_rejected(self):
        def edit(r, f):
            f["evidence/local-2.json"][T303B] = "Passed"
            f["evidence/local-2.json"]["Avowry.Acceptance.Tests.Fake.Test1"] = "Failed"
        self.reject(edit, "differ")

    def test_boundary_second_run_drops_an_allowed_failure_is_rejected(self):
        self.reject(lambda r, f: f["evidence/local-2.json"].update({T162: "Passed"}), "differ")

    def test_boundary_second_run_lists_a_different_test_set_is_rejected(self):
        self.reject(lambda r, f: f["evidence/local-2.json"].update({"Avowry.Acceptance.Tests.Fake.Extra": "Passed"}),
                    "differ")

    def test_boundary_fourth_failure_beyond_the_three_is_rejected(self):
        def edit(r, f):
            for n in (1, 2):
                f[f"evidence/local-{n}.json"]["Avowry.Acceptance.Tests.Fake.Test2"] = "Failed"
        self.reject(edit, "allowed")

    def test_boundary_exactly_min_tests_is_accepted_and_one_fewer_rejected(self):
        self.accept(lambda r, f: r.update(min_tests=6))

        def fewer(r, f):
            for env in ("local", "ci"):
                for n in (1, 2):
                    f[f"evidence/{env}-{n}.json"].pop(PASSING[5])
        self.reject(fewer, "empty")

    def test_boundary_ci_head_sha_one_char_off_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["ci"]["runs"][1]["id"].update(head_sha=HEAD[:-1] + "b"), "head")

    # --- I: interface
    def test_interface_not_a_directory_exits_2(self):
        assert SCRIPT.exists(), "scripts/check-run-pairs-217.py does not exist yet (card 217 criterion 3 red)"
        p = subprocess.run([sys.executable, str(SCRIPT), "--root", str(REPO / "no-such-dir-217")],
                           capture_output=True, text=True)
        self.assertEqual(p.returncode, 2)

    def test_interface_malformed_json_is_a_finding_not_a_traceback(self):
        rec, files = good()
        code, out, err = run(tree(None, files, None, raw="{not json"))
        self.assertEqual(code, 1)
        self.assertIn("malformed", err)
        self.assertNotIn("Traceback", err)

    def test_interface_outcome_file_missing_is_a_finding(self):
        self.reject(lambda r, f: f.pop("evidence/local-1.json"), "missing")

    def test_interface_outcome_file_malformed_is_a_finding(self):
        self.reject(lambda r, f: f.update({"evidence/local-1.json": "[1, 2]"}), "malformed")

    def test_interface_local_run_without_tmp_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["local"]["runs"][0].pop("tmp"), "temp")

    def test_interface_local_run_with_empty_temp_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["local"]["runs"][1].update(temp=""), "temp")

    def test_interface_ci_run_without_github_run_id_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["ci"]["runs"][0]["id"].pop("github_run_id"), "ci")

    def test_interface_local_run_without_trx_name_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["local"]["runs"][0]["id"].pop("trx"), "malformed")

    # --- E: exception
    def test_exception_failure_without_owner_card_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["local"]["allowed_failures"][0].pop("owner_card"), "owner")

    def test_exception_failure_outside_allowed_list_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["local"]["allowed_failures"].pop(), "allowed")

    def test_exception_runs_at_different_heads_are_rejected(self):
        self.reject(lambda r, f: r["environments"]["local"]["runs"][1].update(head=OTHER), "head")

    def test_exception_pair_head_differs_from_record_head_is_rejected(self):
        def edit(r, f):
            for env in r["environments"].values():
                for x in env["runs"]:
                    x["head"] = OTHER
                    if "head_sha" in x["id"]:
                        x["id"]["head_sha"] = OTHER
        self.reject(edit, "head")

    def test_exception_ci_run_not_tied_to_the_head_is_rejected(self):
        self.reject(lambda r, f: r["environments"]["ci"]["runs"][0]["id"].pop("head_sha"), "ci")

    def test_exception_floor_removed_while_module_initializer_exists_is_rejected(self):
        self.reject(floor="[ModuleInitializer]\ninternal static void Raise() {}\n", keyword="floor")

    def test_exception_floor_kept_without_reason_is_rejected(self):
        def edit(r, f):
            r["floor_decision"] = {"decision": "kept", "reason": "  "}
        self.reject(edit, "floor", floor="[ModuleInitializer]")

    def test_exception_floor_decision_unknown_value_is_rejected(self):
        self.reject(lambda r, f: r["floor_decision"].update(decision="maybe"), "floor")

    # --- S: simple
    def test_simple_floor_removed_and_file_gone_is_accepted(self):
        self.accept()

    def test_simple_floor_kept_with_reason_is_accepted(self):
        def edit(r, f):
            r["floor_decision"] = {"decision": "kept", "reason": "second local run starved without the floor"}
        self.accept(edit, floor="[ModuleInitializer]")

    # --- the REAL record
    def test_real_record_run_pairs_passes_the_checker(self):
        self.assertTrue((REPO / REL).exists(), f"{REL} does not exist yet (card 217 criterion 3 red)")
        code, out, err = run(REPO)
        self.assertEqual(code, 0, f"real record not clean: {err}")


if __name__ == "__main__":
    unittest.main()
