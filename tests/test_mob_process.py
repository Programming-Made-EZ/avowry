import contextlib
import importlib.util
import io
import json
import subprocess
import tempfile
import unittest
from pathlib import Path


SPEC = importlib.util.spec_from_file_location(
    "mob_process", Path(__file__).resolve().parents[1] / "scripts" / "mob_process.py"
)
mob = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(mob)


def issue(card, labels=("card",), closed="2026-10-29T20:00:00Z", events=()):
    return {"number": card + 1000, "title": f"({card}) work", "labels": list(labels),
            "closedAt": closed, "events": list(events)}


class MobProcessTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        (self.root / ".mob/records").mkdir(parents=True)
        (self.root / "docs/decisions").mkdir(parents=True)
        (self.root / "docs/retros").mkdir(parents=True)
        (self.root / ".github/workflows").mkdir(parents=True)
        (self.root / "board").mkdir()
        (self.root / ".mob/records/LEGACY.json").write_text(
            json.dumps({"schema": 1, "decision": "D134", "cutoff": "2026-10-29T20:35:37Z",
                        "cards": [], "cards_sha256": mob.cards_hash([])}), encoding="utf-8")
        (self.root / "docs/decisions/process-and-staffing.md").write_text(
            f"D134 sha256 of the list: {mob.cards_hash([])}\n<a id=\"d7\"></a>", encoding="utf-8")
        (self.root / "DRIVERS.md").write_text("| Driver | Status |\n|---|---|\n| Codex | available |\n| Claude | available |", encoding="utf-8")
        (self.root / ".github/workflows/ci.yml").write_text("mob-process:\n  run: python3 scripts/mob_process.py check", encoding="utf-8")
        (self.root / ".github/workflows/mob-process.yml").write_text("issues: [closed, reopened, unlabeled]\nschedule:\n", encoding="utf-8")
        self.git("init", "-b", "main")
        self.git("config", "user.email", "test@example.com")
        self.git("config", "user.name", "Test")
        self.git("add", ".")
        self.git("commit", "-m", "base")

    def tearDown(self):
        self.temp.cleanup()

    def git(self, *args):
        return subprocess.run(["git", *args], cwd=self.root, check=True, capture_output=True, text=True).stdout.strip()

    def valid_record(self, card=201):
        seats = []
        for i, role in enumerate(mob.REQUIRED_ROLES):
            seats.append({"role": role, "seat": "Claude" if role == "signoff" else "Codex",
                          "model": "test", "evidence": "D7" if i == 0 else f"D{10+i}"})
            if i:
                with (self.root / "docs/decisions/process-and-staffing.md").open("a", encoding="utf-8") as f:
                    f.write(f"\n<a id=\"d{10+i}\"></a>")
        record = {"schema": 1, "card": card, "title": "work", "process": "mob", "seats": seats,
                  "retro": "docs/retros/r.md", "notes": ""}
        path = self.root / f".mob/records/{card:03}.json"
        path.write_text(json.dumps(record), encoding="utf-8")
        (self.root / "docs/retros/r.md").write_text(f"Cards: {card}\n", encoding="utf-8")
        self.git("add", ".")
        self.git("commit", "-m", f"Card {card}: trail")
        return record

    def test_persona_story_check_reports_one_github_annotation_per_violation(self):
        # Given Morgan audits a closed card with no process trail.
        # When the read-only detector runs.
        out = io.StringIO()
        with contextlib.redirect_stdout(out):
            code = mob.run_check(self.root, [issue(201)], git=self.git)
        # Then CI is red and every finding is independently visible.
        self.assertEqual(1, code)
        self.assertTrue(all(line.startswith("::error::mob-process: ") for line in out.getvalue().splitlines()))

    def test_record_schema_roles_eligibility_distinct_evidence_and_independent_signoff(self):
        self.valid_record()
        record = json.loads((self.root / ".mob/records/201.json").read_text())
        record["seats"][1]["evidence"] = record["seats"][0]["evidence"]
        record["seats"][-1]["seat"] = "Codex"
        (self.root / ".mob/records/201.json").write_text(json.dumps(record))
        errors = mob.check_records(self.root, [issue(201)], self.git)
        self.assertTrue(any("distinct evidence" in e for e in errors))
        self.assertTrue(any("signoff seat" in e for e in errors))

    def test_record_evidence_must_be_a_decider_anchor_or_reachable_card_commit(self):
        self.valid_record()
        record = json.loads((self.root / ".mob/records/201.json").read_text())
        record["seats"][0]["evidence"] = "deadbeef"
        (self.root / ".mob/records/201.json").write_text(json.dumps(record))
        self.assertTrue(any("evidence" in e for e in mob.check_records(self.root, [issue(201)], self.git)))

    def test_record_first_commit_must_not_postdate_issue_close(self):
        self.valid_record()
        errors = mob.check_records(self.root, [issue(201, closed="2000-01-01T00:00:00Z")], self.git)
        self.assertTrue(any("first committed" in e for e in errors))

    def test_every_closed_card_including_legacy_has_a_strict_cards_line(self):
        (self.root / "docs/retros/r.md").write_text("Cards: 1 to 3\n")
        self.assertTrue(mob.check_retros(self.root, {1, 2, 3}))
        (self.root / "docs/retros/r.md").write_text("Cards: 1-3\n")
        self.assertEqual([], mob.check_retros(self.root, {1, 2, 3}))

    def test_legacy_list_is_hash_pinned_and_cannot_cross_cutoff(self):
        legacy = json.loads((self.root / ".mob/records/LEGACY.json").read_text())
        legacy["cards"] = [1]
        (self.root / ".mob/records/LEGACY.json").write_text(json.dumps(legacy))
        errors = mob.check_legacy(self.root, [issue(1, closed="2026-10-30T00:00:00Z")])
        self.assertGreaterEqual(len(errors), 2)

    def test_reopened_and_unlabeled_timeline_events_are_enforced(self):
        self.valid_record()
        events = ({"event": "unlabeled"}, {"event": "reopened"})
        errors = mob.check_events(self.root, [issue(201, events=events)])
        self.assertTrue(any("unlabeled event" in e for e in errors))
        self.assertTrue(any("reopened" in e for e in errors))

    def test_ci_and_issue_workflow_wiring_are_checked(self):
        (self.root / ".github/workflows/ci.yml").write_text("jobs: {}")
        (self.root / ".github/workflows/mob-process.yml").unlink()
        self.assertEqual(2, len(mob.check_workflows(self.root)))

    def test_closed_unclassified_work_item_cannot_hide(self):
        errors = mob.check_issue_labels([issue(9, labels=())])
        self.assertEqual(1, len(errors))
        self.assertEqual([], mob.check_issue_labels([issue(9, labels=("not-card",))]))

    def test_card_number_comes_from_title_prefix_never_issue_number(self):
        item = issue(201)
        item["number"] = 9999
        self.assertEqual(201, mob.card_number(item))

    def test_mob_state_requires_one_comment_ancestor_freshness_and_exact_regeneration(self):
        (self.root / "MOB.md").write_text("no state\n")
        self.assertTrue(mob.check_mob(self.root, [], self.git))

    def test_generator_is_byte_identical_and_uses_commit_date_not_clock(self):
        data = mob.render_mob(self.root, [], self.git)
        self.assertEqual(data, mob.render_mob(self.root, [], self.git))
        self.assertIn(self.git("show", "-s", "--format=%cI", "HEAD"), data)
        self.assertNotRegex(data, r"[0-9a-f]{40}")


if __name__ == "__main__":
    unittest.main()
