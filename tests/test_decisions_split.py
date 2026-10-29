import hashlib
import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SPLIT = ROOT / "scripts" / "split-decisions.py"
BUILD = ROOT / "scripts" / "build-decisions-index.py"
CHECK = ROOT / "scripts" / "check-decisions-index.py"
CHECK_RECORDED = ROOT / "scripts" / "check-decider-recorded.py"


FIXTURE_LOG = """# Decider log\n\nPreamble is not a record.\n\n#### Decider D1: choose grammar\n\n- Result: passed.\n\n#### Decider D2: choose compiler\n\n- Result: passed.\n"""


FIXTURE_AREAS = {
    "D1": {"area": "language-and-grammar", "status": "in force", "title": "Choose grammar"},
    "D2": {
        "area": "compiler-and-adapters",
        "status": "superseded by D1",
        "title": "Choose compiler",
        "also": ["language-and-grammar"],
    },
}


class DecisionSplitTests(unittest.TestCase):
    def make_repo(self):
        temporary = tempfile.TemporaryDirectory()
        root = Path(temporary.name)
        (root / "docs" / "decisions").mkdir(parents=True)
        (root / "docs" / "decisions" / "DECIDERS.md").write_text(FIXTURE_LOG, encoding="utf-8", newline="")
        (root / "docs" / "decisions" / "areas.json").write_text(
            json.dumps(FIXTURE_AREAS, indent=2) + "\n", encoding="utf-8", newline=""
        )
        return temporary, root

    def run_script(self, script, root, *arguments):
        return subprocess.run(
            [sys.executable, str(script), "--root", str(root), *arguments],
            text=True,
            capture_output=True,
            check=False,
        )

    def test_split_keeps_every_fixture_record_byte_identical_and_exactly_once(self):
        # Given a small append-only log with records assigned to two areas
        temporary, root = self.make_repo()
        self.addCleanup(temporary.cleanup)

        # When the one-shot migration splits the log
        result = self.run_script(SPLIT, root)

        # Then every original record is present byte-for-byte in exactly one area file
        self.assertEqual(0, result.returncode, result.stderr)
        originals = [
            "#### Decider D1: choose grammar\n\n- Result: passed.\n\n",
            "#### Decider D2: choose compiler\n\n- Result: passed.\n",
        ]
        area_text = "".join(path.read_text(encoding="utf-8") for path in (root / "docs" / "decisions").glob("*-and-*.md"))
        for record in originals:
            self.assertEqual(1, area_text.count(record), record)

    def test_split_writes_in_force_and_related_links_without_copying_records(self):
        # Given an in-force decision with a related decision owned by another area
        temporary, root = self.make_repo()
        self.addCleanup(temporary.cleanup)

        # When the log is split
        result = self.run_script(SPLIT, root)

        # Then the owner contains the record and the related area contains only a link
        self.assertEqual(0, result.returncode, result.stderr)
        language = (root / "docs" / "decisions" / "language-and-grammar.md").read_text(encoding="utf-8")
        compiler = (root / "docs" / "decisions" / "compiler-and-adapters.md").read_text(encoding="utf-8")
        self.assertIn("## In force", language)
        self.assertIn("[D1](#d1)", language)
        self.assertIn("[D2](compiler-and-adapters.md#d2)", language)
        self.assertNotIn("#### Decider D2:", language)
        self.assertIn("#### Decider D2:", compiler)

    def test_index_contains_every_id_status_area_and_record_link(self):
        # Given split area files and their metadata
        temporary, root = self.make_repo()
        self.addCleanup(temporary.cleanup)
        self.assertEqual(0, self.run_script(SPLIT, root).returncode)

        # When the deterministic index generator runs
        result = self.run_script(BUILD, root)

        # Then its table and grouped in-force section link to every record
        self.assertEqual(0, result.returncode, result.stderr)
        index = (root / "docs" / "decisions" / "INDEX.md").read_text(encoding="utf-8")
        self.assertIn("| [D1](language-and-grammar.md#d1) | Choose grammar | language-and-grammar | in force |", index)
        self.assertIn("language-and-grammar.md#d1", index)
        self.assertIn("| [D2](compiler-and-adapters.md#d2) | Choose compiler | compiler-and-adapters | superseded by D1 |", index)

    def test_check_rejects_missing_duplicate_unknown_and_bad_supersession(self):
        # Given a valid generated split and index
        temporary, root = self.make_repo()
        self.addCleanup(temporary.cleanup)
        self.assertEqual(0, self.run_script(SPLIT, root).returncode)
        self.assertEqual(0, self.run_script(BUILD, root).returncode)
        decisions = root / "docs" / "decisions"
        language = decisions / "language-and-grammar.md"
        language.write_text(
            language.read_text(encoding="utf-8") + "\n#### Decider D2: duplicate\n\n#### Decider D9: unknown\n",
            encoding="utf-8",
        )
        metadata = json.loads((decisions / "areas.json").read_text(encoding="utf-8"))
        metadata["D2"]["status"] = "superseded by D404"
        (decisions / "areas.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")

        # When the permanent consistency check runs
        result = self.run_script(CHECK, root)

        # Then all structural failures are reported together
        self.assertNotEqual(0, result.returncode)
        self.assertIn("D2 appears in more than one area file", result.stderr)
        self.assertIn("D9 has no areas.json entry", result.stderr)
        self.assertIn("D404 does not exist", result.stderr)

    def test_check_rejects_stale_index_and_missing_in_force_section(self):
        # Given generated files whose index and area summary are damaged
        temporary, root = self.make_repo()
        self.addCleanup(temporary.cleanup)
        self.assertEqual(0, self.run_script(SPLIT, root).returncode)
        self.assertEqual(0, self.run_script(BUILD, root).returncode)
        decisions = root / "docs" / "decisions"
        (decisions / "INDEX.md").write_text("stale\n", encoding="utf-8")
        language = decisions / "language-and-grammar.md"
        language.write_text(language.read_text(encoding="utf-8").replace("## In force", "## Current"), encoding="utf-8")

        # When consistency is checked
        result = self.run_script(CHECK, root)

        # Then both omissions fail the check
        self.assertNotEqual(0, result.returncode)
        self.assertIn("has no In force section", result.stderr)
        self.assertIn("INDEX.md is out of date", result.stderr)

    def test_check_verifies_migration_count_and_hash(self):
        # Given a split whose migration proof describes its original records
        temporary, root = self.make_repo()
        self.addCleanup(temporary.cleanup)
        self.assertEqual(0, self.run_script(SPLIT, root).returncode)
        self.assertEqual(0, self.run_script(BUILD, root).returncode)
        records = (
            "#### Decider D1: choose grammar\n\n- Result: passed.\n\n"
            "#### Decider D2: choose compiler\n\n- Result: passed.\n\n"
        ).encode()
        (root / "docs" / "decisions" / "MIGRATION.txt").write_text(
            f"count 2\nsha256 {hashlib.sha256(records).hexdigest()}\n", encoding="utf-8"
        )

        # When the generated history still matches the proof
        result = self.run_script(CHECK, root)

        # Then the count and byte hash pass
        self.assertEqual(0, result.returncode, result.stderr)

        # And appending a later record to an area file (with its mapping entry) keeps the proof valid
        decisions = root / "docs" / "decisions"
        compiler = decisions / "compiler-and-adapters.md"
        compiler.write_text(
            compiler.read_text(encoding="utf-8").rstrip("\n")
            + '\n\n<a id="d3"></a>\n#### Decider D3: a later decision\n\n- Result: passed.\n',
            encoding="utf-8",
            newline="",
        )
        mapping = json.loads((decisions / "areas.json").read_text(encoding="utf-8"))
        mapping["D3"] = {"area": "compiler-and-adapters", "status": "applied", "title": "A later decision", "order": 2}
        (decisions / "areas.json").write_text(json.dumps(mapping, indent=2) + "\n", encoding="utf-8")
        self.assertEqual(0, self.run_script(BUILD, root).returncode)
        later = self.run_script(CHECK, root)
        self.assertEqual(0, later.returncode, later.stderr)

    def test_recorded_check_reads_area_histories_instead_of_the_legacy_stub(self):
        # Given a migrated decision present only in an area history
        temporary, root = self.make_repo()
        self.addCleanup(temporary.cleanup)
        self.assertEqual(0, self.run_script(SPLIT, root).returncode)

        # When a delivery gate searches for its passed Decider
        result = subprocess.run(
            [sys.executable, str(CHECK_RECORDED), "choose grammar"],
            cwd=root,
            text=True,
            capture_output=True,
            check=False,
        )

        # Then it finds the record without reading DECIDERS.md
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertIn("found", result.stdout)


class RecordCompletenessTests(unittest.TestCase):
    def test_a_heading_without_a_decision_id_stays_inside_the_record_before_it(self):
        # Given a log whose second section is a sign-off heading with no decision id
        sys.path.insert(0, str(ROOT / "scripts"))
        import decision_files

        text = (
            "#### Decider D1: first\n\n- Result: passed.\n\n"
            "#### Sign-off on card 7\n\n- Result: SIGN-OFF.\n\n"
            "#### Decider D2: second\n\n- Result: passed.\n"
        )

        # When the log is parsed into records
        records = decision_files.parse_records(text)

        # Then the sign-off text is inside D1 and the records add up to the whole log, byte for byte
        self.assertEqual(["D1", "D2"], [identifier for identifier, _ in records])
        self.assertIn("SIGN-OFF", dict(records)["D1"])
        self.assertEqual(text, "".join(record for _, record in records))

    def test_a_link_anchor_above_a_heading_belongs_to_that_record_and_is_ignored_by_the_proof(self):
        # Given two records where the second has a link anchor line above its heading
        sys.path.insert(0, str(ROOT / "scripts"))
        import decision_files

        plain = "#### Decider D1: first\n\n- Result: passed.\n\n#### Decider D2: second\n\n- Result: passed.\n"
        anchored = plain.replace("#### Decider D2", '<a id="d2"></a>\n#### Decider D2')

        # When both are parsed and hashed
        plain_records = dict(decision_files.parse_records(plain))
        anchored_records = dict(decision_files.parse_records(anchored))
        metadata = {"D1": {"order": 0}, "D2": {"order": 1}}

        # Then the anchor starts D2 (not the end of D1), and the proof ignores anchors
        self.assertTrue(anchored_records["D2"].startswith("<a id="))
        self.assertNotIn("<a id=", anchored_records["D1"])
        self.assertEqual(decision_files.proof(plain_records, metadata), decision_files.proof(anchored_records, metadata))


if __name__ == "__main__":
    unittest.main()
