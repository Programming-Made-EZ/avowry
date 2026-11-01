"""Card 237 pre-trial red (window 237-slice condition 2; slice step 0a).

scripts/check-wiki-trial.py reads .mob/records/237/trial/{thresholds,freeze,miss-key,ledger,recompute}.json
and uses git ancestry for before/after. It must FAIL unless: thresholds and cap were committed before the
first run; the miss-test key was written by a non-runner and committed after the freeze and before the
first run; every ledger row equals the reviewer recompute.

  python scripts/check-wiki-trial.py [--root DIR]
  exit 0 clean ("wiki-trial: OK"), 1 findings ("<path>: <keyword> <detail>" on stderr), 2 not a directory.

Red shape (slice wording "committed failing before any run"; "a file is missing" is a finding): the
checker FAILS when the trial folder is absent, so the real-repo test below is red now and goes green
only at step 0d. The checker is NOT wired into scripts/mob_process.py check, so the pre-push check on main
stays green; the green seat wires it after 0d. Fixture tests build throwaway git repos with commits in
chosen orders and pass today: they pin each failing case.

Inputs (D167): disabled: test_disabled_row_key; unknown: test_unknown_row_key; repeated: test_duplicate_row_key;
reordered: test_reordered_rows_still_pass.

Fixture trees are made with tempfile.mkdtemp and left in place (no cleanup, by the safety rule).
"""

import copy
import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
SCRIPT = REPO / "scripts" / "check-wiki-trial.py"
T = ".mob/records/237/trial"


def run(root):
    p = subprocess.run([sys.executable, str(SCRIPT), "--root", str(root)], capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    return p.returncode, p.stdout, p.stderr


def sh(root, *args):
    subprocess.run(["git", "-c", "user.name=t", "-c", "user.email=t@example.test", "-c", "commit.gpgsign=false",
                    *args], cwd=str(root), check=True, capture_output=True, text=True)


def rows():
    out = []
    for arm, base in (("baseline", 1000), ("wiki", 700)):
        for i, role in enumerate(("ready", "slice", "red", "green", "ship", "signoff")):
            inp, outp = base + i * 10, 50
            out.append({"run_key": f"{arm}-{role}-1", "arm": arm, "role": role, "stage": "A",
                        "input_total": inp, "cached_input": 100, "output_total": outp,
                        "counted_tokens": inp + outp, "misses": 0, "source": f"{T}/runs/{arm}-{role}-1.txt"})
    return out


def aggs(rs):
    import statistics
    m = {a: statistics.median(r["counted_tokens"] for r in rs if r["arm"] == a) for a in ("baseline", "wiki")}
    return {"median_counted_tokens": m, "delta_pct": round((m["baseline"] - m["wiki"]) / m["baseline"] * 100, 2)}


class Trial:
    """Commit steps in order; mutate docs/steps before build()."""

    def __init__(self):
        rs = rows()
        self.docs = {
            "thresholds": {"token_delta_adopt_pct": 20, "token_delta_reject_pct": 5, "blocked_seconds": 600,
                           "upkeep_tokens_per_card": 12400, "upkeep_minutes": 10,
                           "cap_stage_a": 1000000, "cap_stage_ab": 2000000},
            "freeze": {"author_seat": "ctx-2", "tasks_sha256": "ab" * 32, "arms": ["baseline", "wiki"]},
            "miss-key": {"author_seat": "ctx-4", "keys": [{"run_key": r["run_key"], "rule": "r"} for r in rs]},
            "ledger": {"runner_seats": ["ctx-5"], "rows": rs, "aggregates": aggs(rs)},
            "recompute": {"author_seat": "ctx-4",
                          "rows": [{"run_key": r["run_key"], "counted_tokens": r["counted_tokens"],
                                    "misses": r["misses"]} for r in rs], "aggregates": aggs(rs)},
        }
        self.raw = {}  # name -> raw text override
        self.omit = set()
        self.sources = True
        self.steps = [["thresholds"], ["freeze"], ["miss-key"], ["runs", "ledger", "recompute"]]
        self.after = []  # extra steps: list of names re-written and committed after the build

    def build(self):
        root = Path(tempfile.mkdtemp(prefix="wiki-trial-"))
        sh(root, "init", "-q")
        (root / "README.md").write_text("x\n")
        sh(root, "add", "README.md")
        sh(root, "commit", "-q", "-m", "base")
        for n_step, step in enumerate(self.steps + self.after):
            paths = []
            for name in step:
                if name == "runs":
                    if self.sources:
                        for r in self.docs["ledger"]["rows"]:
                            p = root / r["source"] if isinstance(r.get("source"), str) else None
                            if p and ".." not in p.parts:
                                p.parent.mkdir(parents=True, exist_ok=True)
                                p.write_text("usage\n")
                                paths.append(str(p.relative_to(root)))
                    continue
                if name in self.omit:
                    continue
                p = root / T / f"{name}.json"
                p.parent.mkdir(parents=True, exist_ok=True)
                p.write_text(self.raw.get(name) if name in self.raw else json.dumps(self.docs[name], indent=1)
                             + ("\n" if n_step >= len(self.steps) else ""), encoding="utf-8")
                paths.append(f"{T}/{name}.json")
            if not paths:
                continue
            sh(root, "add", "--", *paths)
            sh(root, "commit", "-q", "--allow-empty", "-m", "step " + "+".join(step), "--", *paths)
        return root


def fails(case, test, keyword):
    root = case.build()
    code, out, err = run(root)
    test.assertEqual(code, 1, (out, err))
    test.assertIn(keyword, err)
    test.assertNotIn("Traceback", err)
    return err


class WikiTrial(unittest.TestCase):
    def test_good_tree_passes(self):
        code, out, err = run(Trial().build())
        self.assertEqual((code, err), (0, ""))
        self.assertIn("wiki-trial: OK (12 rows)", out)

    def test_not_a_directory(self):
        code, _, err = run(Path(tempfile.mkdtemp(prefix="wiki-trial-")) / "nope")
        self.assertEqual(code, 2)
        self.assertIn("not a directory", err)

    def test_not_a_git_repo(self):
        code, _, err = run(Path(tempfile.mkdtemp(prefix="wiki-trial-")))
        self.assertEqual(code, 1)
        self.assertIn("not a git repository", err)

    def test_each_missing_file(self):
        for name in ("thresholds", "freeze", "miss-key", "ledger", "recompute"):
            c = Trial()
            c.omit.add(name)
            fails(c, self, f"{name}.json: missing file")

    def test_malformed_json_each_file(self):
        for name in ("thresholds", "freeze", "miss-key", "ledger", "recompute"):
            c = Trial()
            c.raw[name] = "{not json"
            fails(c, self, f"{name}.json: not valid JSON")

    def test_json_not_an_object(self):
        c = Trial()
        c.raw["ledger"] = "[1, 2]"
        fails(c, self, "not a JSON object")

    def test_runner_authored_key(self):
        c = Trial()
        c.docs["miss-key"]["author_seat"] = "ctx-5"
        fails(c, self, "key author is a runner seat ctx-5")

    def test_key_author_named_runner_in_ledger(self):
        c = Trial()
        c.docs["ledger"]["runner_seats"] = ["ctx-5", "ctx-4"]
        fails(c, self, "key author is a runner seat ctx-4")

    def test_key_author_wrote_freeze(self):
        c = Trial()
        c.docs["miss-key"]["author_seat"] = "ctx-2"
        fails(c, self, "key author also wrote the freeze")

    def test_key_without_author(self):
        c = Trial()
        del c.docs["miss-key"]["author_seat"]
        fails(c, self, "missing author_seat")

    def test_forged_order_key_before_freeze(self):
        c = Trial()
        c.steps = [["thresholds"], ["miss-key"], ["freeze"], ["runs", "ledger", "recompute"]]
        fails(c, self, "key not committed after the freeze")

    def test_forged_order_key_with_the_ledger(self):
        c = Trial()
        c.steps = [["thresholds"], ["freeze"], ["runs", "ledger", "recompute", "miss-key"]]
        fails(c, self, "key not committed before the first run")

    def test_forged_order_thresholds_after_run(self):
        c = Trial()
        c.steps = [["freeze"], ["miss-key"], ["runs", "ledger", "recompute"], ["thresholds"]]
        fails(c, self, "thresholds.json: committed or changed at or after the first run")

    def test_forged_order_key_rewritten_after_run(self):
        c = Trial()
        c.after = [["miss-key"]]
        fails(c, self, "miss-key.json: committed or changed at or after the first run")

    def test_forged_order_freeze_rewritten_after_run(self):
        c = Trial()
        c.after = [["freeze"]]
        fails(c, self, "freeze.json: committed or changed at or after the first run")

    def test_forged_order_run_report_before_key(self):
        c = Trial()
        c.steps = [["thresholds"], ["freeze"], ["runs"], ["miss-key"], ["ledger", "recompute"]]
        fails(c, self, "key not committed before the first run")

    def test_recompute_mismatch_tokens(self):
        c = Trial()
        c.docs["recompute"]["rows"][3]["counted_tokens"] += 1
        fails(c, self, "recompute mismatch counted_tokens baseline-green-1")

    def test_recompute_mismatch_misses(self):
        c = Trial()
        c.docs["ledger"]["rows"][7]["misses"] = 1
        fails(c, self, "recompute mismatch misses wiki-slice-1")

    def test_recompute_mismatch_aggregates(self):
        c = Trial()
        c.docs["recompute"]["aggregates"]["delta_pct"] = 99.0
        fails(c, self, "recompute.json: recompute mismatch aggregates")

    def test_ledger_aggregates_wrong(self):
        c = Trial()
        c.docs["ledger"]["aggregates"]["median_counted_tokens"]["wiki"] = 1
        fails(c, self, "ledger.json: recompute mismatch aggregates")

    def test_recompute_missing_row(self):
        c = Trial()
        del c.docs["recompute"]["rows"][0]
        fails(c, self, "recompute missing baseline-ready-1")

    def test_recompute_by_a_runner(self):
        c = Trial()
        c.docs["recompute"]["author_seat"] = "ctx-5"
        fails(c, self, "recompute author is a runner seat")

    def test_counted_tokens_rule(self):
        c = Trial()
        c.docs["ledger"]["rows"][0]["counted_tokens"] += 5
        c.docs["recompute"]["rows"][0]["counted_tokens"] += 5
        fails(c, self, "counted_tokens rule broken baseline-ready-1")

    def test_row_without_committed_source(self):
        c = Trial()
        c.sources = False
        fails(c, self, "source not committed")

    def test_row_source_escapes_root(self):
        c = Trial()
        c.docs["ledger"]["rows"][0]["source"] = "../outside.txt"
        fails(c, self, "source not committed baseline-ready-1")

    def test_bad_row_shape(self):
        c = Trial()
        c.docs["ledger"]["rows"][0]["misses"] = "0"
        fails(c, self, "bad row 1")

    def test_duplicate_row_key(self):
        c = Trial()
        c.docs["ledger"]["rows"].append(copy.deepcopy(c.docs["ledger"]["rows"][0]))
        fails(c, self, "duplicate row key baseline-ready-1")

    def test_unknown_row_key(self):
        c = Trial()
        c.docs["ledger"]["rows"][0]["run_key"] = "baseline-ready-9"
        fails(c, self, "unknown row key baseline-ready-9")

    def test_disabled_row_key(self):
        c = Trial()
        c.docs["ledger"]["rows"][0]["disabled"] = True
        fails(c, self, "disabled row key baseline-ready-1")

    def test_reordered_rows_still_pass(self):
        c = Trial()
        c.docs["ledger"]["rows"].reverse()
        c.docs["recompute"]["rows"].reverse()
        code, out, err = run(c.build())
        self.assertEqual((code, err), (0, ""))

    def test_cap_exceeded_stage_a(self):
        c = Trial()
        c.docs["thresholds"]["cap_stage_a"] = 5000
        fails(c, self, "cap exceeded")

    def test_cap_stage_ab_below_stage_a(self):
        c = Trial()
        c.docs["thresholds"]["cap_stage_ab"] = 10
        fails(c, self, "cap_stage_ab below cap_stage_a")

    def test_missing_threshold(self):
        c = Trial()
        del c.docs["thresholds"]["blocked_seconds"]
        fails(c, self, "bad threshold blocked_seconds")

    def test_uncommitted_file_is_not_committed(self):
        c = Trial()
        c.steps = [["thresholds"], ["freeze"], ["runs", "ledger", "recompute"]]
        c.omit.add("miss-key")
        fails(c, self, "miss-key.json: missing file")

    # Card 237, window 237-amendment-ordering, cycle 1: one missing file must not hide the other findings.
    def test_order_failure_is_reported_beside_a_missing_file(self):
        for name in ("freeze", "recompute"):
            c = Trial()
            c.omit.add(name)
            c.after = [["miss-key"]]
            err = fails(c, self, f"{name}.json: missing file")
            self.assertIn("miss-key.json: committed or changed at or after the first run", err)
            self.assertNotIn("check could not run", err)

    def test_other_checks_still_run_beside_a_missing_file(self):
        for name in ("thresholds", "freeze", "miss-key", "recompute"):
            c = Trial()
            c.omit.add(name)
            c.docs["ledger"]["rows"][0]["disabled"] = True
            err = fails(c, self, f"{name}.json: missing file")
            self.assertIn("disabled row key baseline-ready-1", err)
            self.assertNotIn("check could not run", err)

    def test_order_failure_is_reported_beside_a_malformed_file(self):
        c = Trial()
        c.raw["recompute"] = "{not json"
        c.after = [["freeze"]]
        err = fails(c, self, "recompute.json: not valid JSON")
        self.assertIn("freeze.json: committed or changed at or after the first run", err)
        self.assertNotIn("check could not run", err)

    def test_real_trial_folder_is_ready(self):
        """RED until step 0d: the real repo has no committed trial folder yet, so the check must fail."""
        code, out, err = run(REPO)
        self.assertEqual(code, 0, f"real trial not ready: {err}")


if __name__ == "__main__":
    unittest.main()
