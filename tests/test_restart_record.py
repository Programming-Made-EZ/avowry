"""Card 234, criterion 3 (D157): the facilitator restarts from the wiki and the handoff
notes at a boundary; its context size is recorded against about 965K; rule misses from
an unread page are counted; token values for every seat and for the facilitator are
committed with the usage source they came from (slice 0441cfd, amendment 5674a66;
stand-down 2 adjustment 2f9009d).

The seam these tests pin, for the green to implement
(scripts/check-restart-record.py; stdlib only; reads files, writes nothing):

  python scripts/check-restart-record.py [--root DIR]     DIR defaults to the cwd

  exit 0  clean: prints "restart-record: OK (<n> restarts)" on stdout; a warning
          (keyword "warning ...") may go to stderr and does not change the exit code
  exit 1  findings: one stderr line per finding, "<path relative to root>: <keyword> <detail>"
  exit 2  DIR is not a directory: stderr says "not a directory"
  never a traceback.

Files it reads:
  .mob/records/234/record-in-progress.json   the card record (a JSON object) with
      "task_force": [{"role", "context"}, ...]   the contexts that need token values
      "seat_metrics": {"<ctx>": entry}           one entry per task-force context, either
          measured:      {"setup_tokens": int, "end_tokens": int, "provider_usage_source": <ref>}
          not measured:  {"not_measured_reason": non-empty text, "available_evidence_source": <ref>}
          (both kinds in one entry is a finding)
      "restarts": [restart, ...]                 at least one; positions are 1-based in findings
          {"boundary_marker": "B<n>", "boundary_hash": 7 to 40 lowercase hex characters,
           "wiki_pages_read": [page id, ...]     ids of enabled rows of the wiki index, must hold core/index,
           "handoff_refs": [path, ...]           files under the root, must hold HANDOFF.md,
           "facilitator_context_tokens": int, "facilitator_provider_usage_source": <ref>,
           "baseline_tokens": 965000, "delta_tokens": tokens minus baseline,
           "rule_miss_cases": [case, ...], "rule_miss_cases_zero_source": <ref> (needed when the list is empty)}
          case = {"context", "boundary", "missed_page_id", "source"}; missed_page_id is an id of the index
          and is not in the same restart's wiki_pages_read; boundary is a marker of the handoff notes.
  <ref>: a path relative to the root of an existing committed file, not outside the root.
  token int: a whole number of at least 1 (not a string, not a float, not a bool).
  docs/process-wiki/INDEX.md                 only its table | id | path | status | summary | is read.
  HANDOFF.md (and any other handoff_refs)    lines "Boundary B<n>: <hash>" are the boundary markers:
      a marker line not of that shape after the word "Boundary" is an unknown boundary marker; the same
      marker twice: the last line wins and a warning is written; a marker number lower than the one before
      it is out of order; the record's boundary_hash must equal the handoff hash of its marker. When the
      root is a git repository the boundary hash must also resolve to a commit.

Finding keywords: no record, record not valid JSON, no task force, no seat metrics, missing seat <ctx>,
missing <field> <where>, bad token value <field> <where>, source not found <field> <where>,
source outside root <field> <where>, measured and not measured <ctx>, no restart, bad boundary hash,
unknown boundary hash, unknown boundary marker, boundary hash mismatch, boundary out of order,
no index, unknown page id, disabled page id, repeated page id, missing core/index, missing handoff file,
handoff refs without HANDOFF.md, baseline mismatch, delta mismatch, rule_miss_cases not a list,
rule_miss_cases empty without source, bad rule miss case, missed page unknown, missed page was read.
Warning keyword: warning repeated boundary marker.

Fixture trees are made with tempfile.mkdtemp and left in place (no cleanup, by the safety rule).
"""

import copy
import json
import os
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
SCRIPT = Path(os.environ.get("RESTART_RECORD_SCRIPT", str(REPO / "scripts" / "check-restart-record.py")))
# Real-repo tests read this tree; a scratch copy can be named to show a guard red.
REAL = Path(os.environ.get("RESTART_RECORD_REPO_ROOT", str(REPO)))

RECORD = ".mob/records/234/record-in-progress.json"
INDEX = "docs/process-wiki/INDEX.md"
HANDOFF = "HANDOFF.md"
USAGE = ".mob/records/234/usage"
H1, H2, H3 = "a1b2c3d", "b2c3d4e5f", "c3d4e5f6a7"
BASELINE = 965000


def run(*args, cwd=None):
    proc = subprocess.run(
        [sys.executable, str(SCRIPT), *args],
        cwd=str(cwd) if cwd else None,
        capture_output=True, text=True, encoding="utf-8", errors="replace",
    )
    return proc.returncode, proc.stdout, proc.stderr


def findings(stderr):
    return [line for line in stderr.splitlines() if line.strip() and ": warning" not in line]


def warnings(stderr):
    return [line for line in stderr.splitlines() if ": warning" in line]


class Tree:
    """A fixture repository root: the card record, usage sources, the handoff notes and the wiki index."""

    def __init__(self):
        self.contexts = ["ctx-1", "ctx-2", "ctx-3"]
        self.files = {}
        self.record = {
            "card": 234,
            "task_force": [{"role": f"seat {c}", "context": c} for c in self.contexts],
            "seat_metrics": {
                "ctx-1": self.measured("ctx-1", 41000, 118000),
                "ctx-2": self.measured("ctx-2", 47000, 122000),
                "ctx-3": self.not_measured("ctx-3"),
            },
            "restarts": [self.restart("B1", H1, 412000)],
        }
        self.handoff = f"# Facilitator handoff\n\nBoundary B1: {H1}\n"
        self.index_rows = [
            ("core/index", "docs/process-wiki/INDEX.md", "enabled"),
            ("role/qa-red", "docs/process-wiki/roles/qa-red.md", "enabled"),
            ("rules/one-criterion", "docs/process-wiki/rules/one-criterion.md", "enabled"),
            ("howto/retired", "docs/process-wiki/howto/retired.md", "disabled"),
        ]
        self.write_index = True
        self.write_record = True
        self.git = False

    def source(self, name, text="usage lines: input 1 output 1\n"):
        rel = f"{USAGE}/{name}.txt"
        self.files[rel] = text
        return rel

    def measured(self, ctx, setup, end):
        return {"setup_tokens": setup, "end_tokens": end, "provider_usage_source": self.source(ctx)}

    def not_measured(self, ctx):
        return {"not_measured_reason": "Copilot seat: this repository records no command or usage source for it",
                "available_evidence_source": self.source(f"{ctx}-probe")}

    def restart(self, marker, boundary_hash, tokens):
        return {
            "boundary_marker": marker, "boundary_hash": boundary_hash,
            "wiki_pages_read": ["core/index", "role/qa-red"],
            "handoff_refs": [HANDOFF],
            "facilitator_context_tokens": tokens,
            "facilitator_provider_usage_source": self.source(f"facilitator-{marker}"),
            "baseline_tokens": BASELINE, "delta_tokens": tokens - BASELINE,
            "rule_miss_cases": [],
            "rule_miss_cases_zero_source": self.source(f"misses-{marker}", "restart checked: 0 misses\n"),
        }

    def miss(self, marker="B1", page="rules/one-criterion"):
        return {"context": "facilitator", "boundary": marker, "missed_page_id": page,
                "source": self.source(f"miss-{marker}-{page.replace('/', '-')}")}

    def r(self, n=0):
        return self.record["restarts"][n]

    def index_text(self):
        lines = [f"| {pid} | {path} | {status} | Summary of {pid}. |" for pid, path, status in self.index_rows]
        return ("Index of the process wiki.\n\n| id | path | status | summary |\n|---|---|---|---|\n"
                + "\n".join(lines) + "\n")

    def build(self):
        root = Path(tempfile.mkdtemp(prefix="rr234-"))

        def put(rel, text=None, raw=None):
            target = root / rel
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(raw if raw is not None else text.encode("utf-8"))

        for rel, text in self.files.items():
            put(rel, text if isinstance(text, str) else None, raw=text if isinstance(text, bytes) else None)
        if self.write_record:
            put(RECORD, json.dumps(self.record, indent=1) + "\n")
        if self.write_index:
            put(INDEX, self.index_text())
        if isinstance(self.handoff, bytes):
            put(HANDOFF, raw=self.handoff)
        elif self.handoff is not None:
            put(HANDOFF, self.handoff)
        if self.git:
            self.commit_hash = git_commit(root)
        return root


def git_commit(root):
    def git(*args):
        return subprocess.run(["git", "-c", "user.name=t", "-c", "user.email=t@example.test", *args],
                              cwd=str(root), capture_output=True, text=True, check=True).stdout.strip()
    git("init", "-q")
    git("add", "-A")
    git("commit", "-q", "-m", "fixture")
    return git("rev-parse", "HEAD")


def check(tree, *extra):
    root = tree.build()
    code, out, err = run("--root", str(root), *extra)
    return root, code, out, err


class ZeroCases(unittest.TestCase):
    def test_a_tree_with_no_record_fails_with_no_record(self):
        tree = Tree()
        tree.write_record = False
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^\.mob/records/234/record-in-progress\.json: no record")

    def test_a_record_that_is_not_json_is_a_finding_not_a_traceback(self):
        tree = Tree()
        tree.write_record = False
        tree.files[RECORD] = "{not json"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("record not valid JSON", err)
        self.assertNotIn("Traceback", err)

    def test_a_record_that_is_a_json_list_is_a_finding_not_a_traceback(self):
        tree = Tree()
        tree.write_record = False
        tree.files[RECORD] = "[1, 2]"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("record not valid JSON", err)
        self.assertNotIn("Traceback", err)

    def test_a_record_with_no_restarts_has_no_restart(self):
        tree = Tree()
        del tree.record["restarts"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("no restart", err)

    def test_an_empty_restarts_list_has_no_restart(self):
        tree = Tree()
        tree.record["restarts"] = []
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("no restart", err)

    def test_a_record_with_no_seat_metrics_has_no_seat_metrics(self):
        tree = Tree()
        del tree.record["seat_metrics"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("no seat metrics", err)

    def test_an_empty_seat_metrics_object_names_every_context(self):
        tree = Tree()
        tree.record["seat_metrics"] = {}
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        for ctx in tree.contexts:
            self.assertIn(f"missing seat {ctx}", err)

    def test_a_record_with_no_task_force_has_no_task_force(self):
        tree = Tree()
        del tree.record["task_force"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("no task force", err)

    def test_a_restart_with_no_rule_miss_cases_field_is_named(self):
        tree = Tree()
        del tree.r()["rule_miss_cases"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing rule_miss_cases restart 1", err)

    def test_a_missing_handoff_notes_file_is_named(self):
        tree = Tree()
        tree.handoff = None
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing handoff file HANDOFF.md", err)

    def test_a_root_that_is_not_a_directory_exits_2(self):
        missing = Path(tempfile.mkdtemp(prefix="rr234-")) / "no-such-root"
        code, _, err = run("--root", str(missing))
        self.assertEqual(code, 2, err)
        self.assertIn("not a directory", err)


class OneCases(unittest.TestCase):
    def test_the_clean_tree_with_one_restart_is_clean(self):
        _, code, out, err = check(Tree())
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("restart-record: OK (1 restarts)", out)

    def test_a_task_force_of_one_context_with_one_entry_is_clean(self):
        tree = Tree()
        tree.record["task_force"] = [{"role": "seat", "context": "ctx-1"}]
        tree.record["seat_metrics"] = {"ctx-1": tree.measured("ctx-1", 41000, 118000)}
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)

    def test_one_miss_case_with_its_page_and_source_is_clean(self):
        tree = Tree()
        tree.r()["rule_miss_cases"] = [tree.miss()]
        del tree.r()["rule_miss_cases_zero_source"]
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)

    def test_one_seat_missing_its_end_tokens_is_named_with_its_context(self):
        tree = Tree()
        del tree.record["seat_metrics"]["ctx-1"]["end_tokens"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertEqual(len(findings(err)), 1, err)
        self.assertIn("missing end_tokens ctx-1", err)


class ManyCases(unittest.TestCase):
    def test_many_restarts_with_many_boundaries_are_clean(self):
        tree = Tree()
        tree.record["restarts"] = [tree.restart("B1", H1, 412000), tree.restart("B2", H2, 380000),
                                   tree.restart("B3", H3, 350000)]
        tree.handoff = f"Boundary B1: {H1}\nBoundary B2: {H2}\nBoundary B3: {H3}\n"
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("(3 restarts)", out)

    def test_many_seats_with_one_bad_seat_names_only_that_seat(self):
        tree = Tree()
        tree.record["task_force"] = [{"role": "s", "context": f"ctx-{n}"} for n in range(1, 9)]
        tree.record["seat_metrics"] = {f"ctx-{n}": tree.measured(f"ctx-{n}", 40000 + n, 100000 + n) for n in range(1, 9)}
        tree.record["seat_metrics"]["ctx-6"]["provider_usage_source"] = ""
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        bad = findings(err)
        self.assertEqual(len(bad), 1, err)
        self.assertIn("ctx-6", bad[0])

    def test_two_bad_seats_give_two_findings(self):
        tree = Tree()
        del tree.record["seat_metrics"]["ctx-1"]["setup_tokens"]
        del tree.record["seat_metrics"]["ctx-2"]["provider_usage_source"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertEqual(len(findings(err)), 2, err)

    def test_only_the_bad_restart_among_three_is_named_by_position(self):
        tree = Tree()
        tree.record["restarts"] = [tree.restart("B1", H1, 412000), tree.restart("B2", H2, 380000),
                                   tree.restart("B3", H3, 350000)]
        tree.handoff = f"Boundary B1: {H1}\nBoundary B2: {H2}\nBoundary B3: {H3}\n"
        del tree.r(1)["facilitator_context_tokens"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing facilitator_context_tokens restart 2", err)
        self.assertNotIn("restart 1", err)
        self.assertNotIn("restart 3", err)

    def test_many_miss_cases_are_clean(self):
        tree = Tree()
        tree.index_rows.append(("rules/second", "docs/process-wiki/rules/second.md", "enabled"))
        tree.r()["rule_miss_cases"] = [tree.miss(page="rules/one-criterion"), tree.miss(page="rules/second")]
        del tree.r()["rule_miss_cases_zero_source"]
        _, code, _, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)


class SeatTokenCases(unittest.TestCase):
    def test_a_seat_with_no_setup_tokens_is_named(self):
        tree = Tree()
        del tree.record["seat_metrics"]["ctx-2"]["setup_tokens"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing setup_tokens ctx-2", err)

    def test_a_seat_with_no_usage_source_is_named(self):
        tree = Tree()
        del tree.record["seat_metrics"]["ctx-1"]["provider_usage_source"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing provider_usage_source ctx-1", err)

    def test_an_empty_usage_source_counts_as_missing(self):
        tree = Tree()
        tree.record["seat_metrics"]["ctx-1"]["provider_usage_source"] = "   "
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing provider_usage_source ctx-1", err)

    def test_a_usage_source_naming_a_file_that_is_not_committed_is_not_found(self):
        tree = Tree()
        tree.record["seat_metrics"]["ctx-1"]["provider_usage_source"] = f"{USAGE}/never-written.txt"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("source not found provider_usage_source ctx-1", err)

    def test_a_usage_source_that_is_a_scratch_name_with_no_path_is_not_found(self):
        tree = Tree()
        tree.record["seat_metrics"]["ctx-1"]["provider_usage_source"] = "scratch red-234-2fix.err"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("source not found provider_usage_source ctx-1", err)

    def test_a_usage_source_outside_the_root_is_rejected(self):
        tree = Tree()
        outside = Path(tempfile.mkdtemp(prefix="rr234-out-")) / "usage.txt"
        outside.write_text("usage\n", encoding="utf-8")
        tree.record["seat_metrics"]["ctx-1"]["provider_usage_source"] = "../" + outside.parent.name + "/usage.txt"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("source outside root provider_usage_source ctx-1", err)

    def test_a_usage_source_that_is_a_folder_is_not_found(self):
        tree = Tree()
        tree.record["seat_metrics"]["ctx-1"]["provider_usage_source"] = USAGE
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("source not found provider_usage_source ctx-1", err)

    def test_a_not_measured_seat_with_no_reason_is_named(self):
        tree = Tree()
        del tree.record["seat_metrics"]["ctx-3"]["not_measured_reason"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing not_measured_reason ctx-3", err)

    def test_a_blank_not_measured_reason_counts_as_missing(self):
        tree = Tree()
        tree.record["seat_metrics"]["ctx-3"]["not_measured_reason"] = ""
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing not_measured_reason ctx-3", err)

    def test_a_not_measured_seat_with_no_evidence_source_is_named(self):
        tree = Tree()
        del tree.record["seat_metrics"]["ctx-3"]["available_evidence_source"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing available_evidence_source ctx-3", err)

    def test_an_entry_that_is_both_measured_and_not_measured_is_rejected(self):
        tree = Tree()
        tree.record["seat_metrics"]["ctx-1"].update(tree.not_measured("ctx-1"))
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("measured and not measured ctx-1", err)

    def test_an_entry_with_no_fields_at_all_is_named_not_crashed(self):
        tree = Tree()
        tree.record["seat_metrics"]["ctx-1"] = {}
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("ctx-1", err)
        self.assertNotIn("Traceback", err)

    def test_a_seat_entry_that_is_not_an_object_is_named_not_crashed(self):
        tree = Tree()
        tree.record["seat_metrics"]["ctx-1"] = 41000
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("ctx-1", err)
        self.assertNotIn("Traceback", err)


class TokenBoundaryCases(unittest.TestCase):
    def _seat_value(self, value):
        tree = Tree()
        tree.record["seat_metrics"]["ctx-1"]["setup_tokens"] = value
        return check(tree)

    def test_one_token_is_clean(self):
        _, code, _, err = self._seat_value(1)
        self.assertEqual((code, err), (0, ""), err)

    def test_zero_tokens_is_a_bad_token_value(self):
        _, code, _, err = self._seat_value(0)
        self.assertEqual(code, 1, err)
        self.assertIn("bad token value setup_tokens ctx-1", err)

    def test_negative_tokens_is_a_bad_token_value(self):
        _, code, _, err = self._seat_value(-5)
        self.assertEqual(code, 1, err)
        self.assertIn("bad token value setup_tokens ctx-1", err)

    def test_a_string_of_digits_is_a_bad_token_value(self):
        _, code, _, err = self._seat_value("41000")
        self.assertEqual(code, 1, err)
        self.assertIn("bad token value setup_tokens ctx-1", err)

    def test_a_shortened_figure_like_937_9k_is_a_bad_token_value(self):
        _, code, _, err = self._seat_value("937.9k")
        self.assertEqual(code, 1, err)
        self.assertIn("bad token value setup_tokens ctx-1", err)

    def test_a_float_is_a_bad_token_value(self):
        _, code, _, err = self._seat_value(41000.5)
        self.assertEqual(code, 1, err)
        self.assertIn("bad token value setup_tokens ctx-1", err)

    def test_a_boolean_is_a_bad_token_value(self):
        _, code, _, err = self._seat_value(True)
        self.assertEqual(code, 1, err)
        self.assertIn("bad token value setup_tokens ctx-1", err)

    def test_null_is_a_missing_value_not_a_crash(self):
        _, code, _, err = self._seat_value(None)
        self.assertEqual(code, 1, err)
        self.assertIn("setup_tokens ctx-1", err)
        self.assertNotIn("Traceback", err)

    def _facilitator(self, tokens, delta=None):
        tree = Tree()
        tree.r()["facilitator_context_tokens"] = tokens
        tree.r()["delta_tokens"] = tokens - BASELINE if delta is None else delta
        return check(tree)

    def test_facilitator_tokens_exactly_at_the_baseline_with_delta_zero_are_clean(self):
        _, code, _, err = self._facilitator(BASELINE)
        self.assertEqual((code, err), (0, ""), err)

    def test_facilitator_tokens_above_the_baseline_are_recorded_not_a_finding(self):
        _, code, _, err = self._facilitator(BASELINE + 1)
        self.assertEqual((code, err), (0, ""), err)

    def test_a_delta_that_is_not_tokens_minus_baseline_is_a_mismatch(self):
        _, code, _, err = self._facilitator(412000, delta=-1)
        self.assertEqual(code, 1, err)
        self.assertIn("delta mismatch restart 1", err)

    def test_a_baseline_other_than_965000_is_a_mismatch(self):
        tree = Tree()
        tree.r()["baseline_tokens"] = 96500
        tree.r()["delta_tokens"] = 412000 - 96500
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("baseline mismatch restart 1", err)

    def test_facilitator_tokens_as_text_are_a_bad_token_value(self):
        tree = Tree()
        tree.r()["facilitator_context_tokens"] = "412k"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("bad token value facilitator_context_tokens restart 1", err)

    def test_a_facilitator_with_no_usage_source_is_named(self):
        tree = Tree()
        del tree.r()["facilitator_provider_usage_source"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing facilitator_provider_usage_source restart 1", err)

    def test_a_facilitator_usage_source_that_does_not_exist_is_not_found(self):
        tree = Tree()
        tree.r()["facilitator_provider_usage_source"] = f"{USAGE}/nope.txt"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("source not found facilitator_provider_usage_source restart 1", err)


class HashBoundaryCases(unittest.TestCase):
    def _hash(self, value):
        tree = Tree()
        tree.r()["boundary_hash"] = value
        tree.handoff = f"Boundary B1: {value}\n"
        return check(tree)

    def test_a_hash_of_7_characters_is_clean(self):
        _, code, _, err = self._hash("a1b2c3d")
        self.assertEqual((code, err), (0, ""), err)

    def test_a_hash_of_6_characters_is_a_bad_boundary_hash(self):
        _, code, _, err = self._hash("a1b2c3")
        self.assertEqual(code, 1, err)
        self.assertIn("bad boundary hash restart 1", err)

    def test_a_hash_of_40_characters_is_clean(self):
        _, code, _, err = self._hash("a" * 40)
        self.assertEqual((code, err), (0, ""), err)

    def test_a_hash_of_41_characters_is_a_bad_boundary_hash(self):
        _, code, _, err = self._hash("a" * 41)
        self.assertEqual(code, 1, err)
        self.assertIn("bad boundary hash restart 1", err)

    def test_an_uppercase_hash_is_a_bad_boundary_hash(self):
        _, code, _, err = self._hash("A1B2C3D")
        self.assertEqual(code, 1, err)
        self.assertIn("bad boundary hash restart 1", err)

    def test_a_hash_that_is_not_hex_is_a_bad_boundary_hash(self):
        _, code, _, err = self._hash("zzzzzzz")
        self.assertEqual(code, 1, err)
        self.assertIn("bad boundary hash restart 1", err)

    def test_a_missing_boundary_hash_is_named(self):
        tree = Tree()
        del tree.r()["boundary_hash"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing boundary_hash restart 1", err)

    def test_a_missing_boundary_marker_is_named(self):
        tree = Tree()
        del tree.r()["boundary_marker"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing boundary_marker restart 1", err)

    def test_in_a_git_repository_a_hash_that_is_a_commit_is_clean(self):
        tree = Tree()
        tree.git = True
        tree.r()["boundary_hash"] = "0" * 7  # replaced below once the commit exists
        root = tree.build()
        head = tree.commit_hash
        record = json.loads((root / RECORD).read_text(encoding="utf-8"))
        record["restarts"][0]["boundary_hash"] = head[:12]
        (root / RECORD).write_text(json.dumps(record), encoding="utf-8")
        (root / HANDOFF).write_text(f"Boundary B1: {head[:12]}\n", encoding="utf-8")
        code, out, err = run("--root", str(root))
        self.assertEqual((code, err), (0, ""), err)

    def test_in_a_git_repository_a_well_formed_hash_that_is_no_commit_is_unknown(self):
        tree = Tree()
        tree.git = True
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("unknown boundary hash restart 1", err)


class PagesReadCases(unittest.TestCase):
    def test_a_restart_with_no_pages_read_is_named(self):
        tree = Tree()
        del tree.r()["wiki_pages_read"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing wiki_pages_read restart 1", err)

    def test_an_empty_pages_read_list_is_missing_core_index(self):
        tree = Tree()
        tree.r()["wiki_pages_read"] = []
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing core/index restart 1", err)

    def test_pages_read_without_the_index_page_is_missing_core_index(self):
        tree = Tree()
        tree.r()["wiki_pages_read"] = ["role/qa-red"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing core/index restart 1", err)

    def test_only_the_index_page_read_is_clean(self):
        tree = Tree()
        tree.r()["wiki_pages_read"] = ["core/index"]
        _, code, _, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)

    def test_a_wiki_with_no_index_file_is_a_no_index_finding(self):
        tree = Tree()
        tree.write_index = False
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/INDEX\.md: no index")


class HandoffRefCases(unittest.TestCase):
    def test_a_restart_with_no_handoff_refs_is_named(self):
        tree = Tree()
        del tree.r()["handoff_refs"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing handoff_refs restart 1", err)

    def test_an_empty_handoff_refs_list_is_without_the_handoff_notes(self):
        tree = Tree()
        tree.r()["handoff_refs"] = []
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("handoff refs without HANDOFF.md restart 1", err)

    def test_refs_that_do_not_name_the_handoff_notes_are_rejected(self):
        tree = Tree()
        tree.files["notes/other.md"] = f"Boundary B1: {H1}\n"
        tree.r()["handoff_refs"] = ["notes/other.md"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("handoff refs without HANDOFF.md restart 1", err)

    def test_a_ref_to_a_file_that_is_not_there_is_a_missing_handoff_file(self):
        tree = Tree()
        tree.r()["handoff_refs"] = [HANDOFF, "notes/gone.md"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing handoff file notes/gone.md", err)

    def test_a_handoff_file_that_is_not_utf8_is_a_finding_not_a_traceback(self):
        tree = Tree()
        tree.handoff = b"Boundary B1: a1b2c3d\n\xff\xfe\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("not valid UTF-8", err)
        self.assertNotIn("Traceback", err)


class RuleMissCases(unittest.TestCase):
    def test_an_empty_miss_list_with_no_zero_source_fails(self):
        tree = Tree()
        del tree.r()["rule_miss_cases_zero_source"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("rule_miss_cases empty without source restart 1", err)

    def test_an_empty_miss_list_with_a_blank_zero_source_fails(self):
        tree = Tree()
        tree.r()["rule_miss_cases_zero_source"] = ""
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("rule_miss_cases empty without source restart 1", err)

    def test_an_empty_miss_list_whose_zero_source_file_is_not_there_fails(self):
        tree = Tree()
        tree.r()["rule_miss_cases_zero_source"] = f"{USAGE}/no-such-run.txt"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("source not found rule_miss_cases_zero_source restart 1", err)

    def test_a_miss_list_that_is_not_a_list_is_named(self):
        tree = Tree()
        tree.r()["rule_miss_cases"] = "none"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("rule_miss_cases not a list restart 1", err)

    def test_a_null_miss_list_is_not_a_list(self):
        tree = Tree()
        tree.r()["rule_miss_cases"] = None
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("rule_miss_cases", err)
        self.assertNotIn("Traceback", err)

    def test_a_case_missing_each_field_is_named_by_field(self):
        for field in ("context", "boundary", "missed_page_id", "source"):
            with self.subTest(field=field):
                tree = Tree()
                case = tree.miss()
                del case[field]
                tree.r()["rule_miss_cases"] = [case]
                _, code, _, err = check(tree)
                self.assertEqual(code, 1, err)
                self.assertIn(f"bad rule miss case 1 {field}", err)

    def test_a_case_whose_source_file_is_not_there_fails(self):
        tree = Tree()
        case = tree.miss()
        case["source"] = f"{USAGE}/missing-miss.txt"
        tree.r()["rule_miss_cases"] = [case]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("source not found source restart 1", err)

    def test_a_case_naming_a_page_the_index_does_not_have_is_unknown(self):
        tree = Tree()
        tree.r()["rule_miss_cases"] = [tree.miss(page="rules/no-such-page")]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missed page unknown rules/no-such-page restart 1", err)

    def test_a_case_naming_a_page_the_restart_says_it_read_is_a_contradiction(self):
        tree = Tree()
        tree.r()["rule_miss_cases"] = [tree.miss(page="role/qa-red")]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missed page was read role/qa-red restart 1", err)

    def test_a_case_whose_boundary_is_not_a_marker_of_the_notes_is_unknown(self):
        tree = Tree()
        tree.r()["rule_miss_cases"] = [tree.miss(marker="B9")]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("unknown boundary marker B9", err)


class InterfaceCases(unittest.TestCase):
    def test_the_root_defaults_to_the_current_directory(self):
        root = Tree().build()
        code, out, err = run(cwd=root)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("restart-record: OK", out)

    def test_every_finding_line_starts_with_a_path_relative_to_the_root(self):
        tree = Tree()
        del tree.record["seat_metrics"]["ctx-1"]["end_tokens"]
        tree.index_rows = []
        root, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        lines = findings(err)
        self.assertGreaterEqual(len(lines), 1, err)
        for line in lines:
            path = line.split(": ", 1)[0]
            self.assertFalse(os.path.isabs(path), line)
            self.assertNotIn("\\", path, line)
            self.assertTrue((root / path).exists() or "INDEX" in path, line)

    def test_a_clean_run_writes_nothing_to_stderr(self):
        _, code, _, err = check(Tree())
        self.assertEqual((code, err), (0, ""), err)


class HostileInputsCriterionThree(unittest.TestCase):
    """The slice's Inputs line for criterion 3, entry by entry."""

    # disabled: handoff sections are not disable-able (n/a); the nearest hostile input is a disabled wiki page
    def test_a_disabled_wiki_page_id_in_the_pages_read_fails_the_restart(self):
        tree = Tree()
        tree.r()["wiki_pages_read"] = ["core/index", "howto/retired"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("disabled page id howto/retired restart 1", err)

    # unknown: an unknown boundary marker in the handoff notes fails the restart check
    def test_a_marker_line_in_the_handoff_notes_with_an_unknown_shape_fails(self):
        tree = Tree()
        tree.handoff += "Boundary X9: d4e5f6a\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^HANDOFF\.md: unknown boundary marker X9")

    def test_a_restart_naming_a_marker_the_handoff_notes_lack_fails(self):
        tree = Tree()
        tree.r()["boundary_marker"] = "B7"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("unknown boundary marker B7 restart 1", err)

    def test_an_unknown_wiki_page_id_in_the_pages_read_fails(self):
        tree = Tree()
        tree.r()["wiki_pages_read"] = ["core/index", "rules/not-in-the-index"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("unknown page id rules/not-in-the-index restart 1", err)

    # repeated: a repeated boundary marker uses last-write-wins and records a warning
    def test_a_repeated_marker_uses_the_last_line_and_warns(self):
        tree = Tree()
        tree.handoff = f"Boundary B1: d4e5f6a\nBoundary B1: {H1}\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 0, err)
        self.assertEqual(len(warnings(err)), 1, err)
        self.assertRegex(err, r"(?m)^HANDOFF\.md: warning repeated boundary marker B1")

    def test_a_repeated_marker_makes_the_first_line_hash_a_mismatch(self):
        tree = Tree()
        tree.handoff = f"Boundary B1: {H1}\nBoundary B1: d4e5f6a\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("boundary hash mismatch B1 restart 1", err)
        self.assertEqual(len(warnings(err)), 1, err)

    def test_a_repeated_wiki_page_id_in_the_pages_read_is_flagged(self):
        tree = Tree()
        tree.r()["wiki_pages_read"] = ["core/index", "role/qa-red", "role/qa-red"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("repeated page id role/qa-red restart 1", err)

    # reordered: boundary entries out of order fail when the monotonic sequence is broken
    def test_boundaries_in_the_handoff_notes_out_of_order_fail(self):
        tree = Tree()
        tree.record["restarts"] = [tree.restart("B1", H1, 412000), tree.restart("B2", H2, 380000)]
        tree.handoff = f"Boundary B2: {H2}\nBoundary B1: {H1}\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^HANDOFF\.md: boundary out of order B1")

    def test_boundaries_in_order_with_a_gap_in_the_numbers_are_clean(self):
        tree = Tree()
        tree.record["restarts"] = [tree.restart("B1", H1, 412000), tree.restart("B4", H2, 380000)]
        tree.handoff = f"Boundary B1: {H1}\nBoundary B4: {H2}\n"
        _, code, _, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)

    def test_reordering_the_restart_entries_in_the_record_does_not_change_the_result(self):
        tree = Tree()
        tree.record["restarts"] = [tree.restart("B2", H2, 380000), tree.restart("B1", H1, 412000)]
        tree.handoff = f"Boundary B1: {H1}\nBoundary B2: {H2}\n"
        _, code, _, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)

    def test_reordering_the_pages_read_does_not_change_the_result(self):
        tree = Tree()
        tree.r()["wiki_pages_read"] = ["role/qa-red", "core/index"]
        _, code, _, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)

    def test_reordering_the_seat_metrics_keys_does_not_change_the_result(self):
        tree = Tree()
        tree.record["seat_metrics"] = dict(reversed(list(tree.record["seat_metrics"].items())))
        _, code, _, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)


class SimpleCases(unittest.TestCase):
    def test_a_record_with_every_value_and_source_and_the_notes_is_clean(self):
        tree = Tree()
        tree.record["restarts"] = [tree.restart("B1", H1, 412000), tree.restart("B2", H2, 380000)]
        tree.r(1)["rule_miss_cases"] = [tree.miss(marker="B2")]
        del tree.r(1)["rule_miss_cases_zero_source"]
        tree.handoff = f"# Handoff\n\nBoundary B1: {H1}\n\nNotes.\n\nBoundary B2: {H2}\n"
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("(2 restarts)", out)


class ThisRepository(unittest.TestCase):
    """Guards on the real tree. The first two are born green (they pin the assumptions
    the checker relies on); the last two need the files the green adds."""

    def read(self, rel):
        return (REAL / rel).read_text(encoding="utf-8")

    def test_assumption_the_record_names_the_task_force_contexts_ctx_1_to_ctx_8(self):
        contexts = {item["context"] for item in json.loads(self.read(RECORD))["task_force"]}
        self.assertEqual(contexts, {f"ctx-{n}" for n in range(1, 9)})

    def test_assumption_the_handoff_notes_file_exists_and_is_utf8(self):
        self.assertIn("handoff", self.read(HANDOFF).lower())

    def test_the_handoff_notes_carry_at_least_one_boundary_marker_line(self):
        import re
        self.assertRegex(self.read(HANDOFF), r"(?m)^Boundary B\d+: [0-9a-f]{7,40}\s*$")

    def test_the_restart_checker_passes_on_this_repository(self):
        code, out, err = run("--root", str(REAL))
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("restart-record: OK", out)


if __name__ == "__main__":
    unittest.main()
