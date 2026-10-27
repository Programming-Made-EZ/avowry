import json
import tempfile
import unittest
from pathlib import Path

from scripts.board import new_card, rerank

SLUG = "Programming-Made-EZ/phrase-lang"
STATUS_OPTIONS = ["Ready", "In Progress", "Done", "Superseded/Withdrawn"]
FIELDS = [
    {"id": "F_rank", "name": "Rank", "type": "ProjectV2Field"},
    {"id": "F_num", "name": "Card number", "type": "ProjectV2Field"},
    {"id": "F_dep", "name": "Depends on", "type": "ProjectV2Field"},
    {"id": "F_kind", "name": "Kind", "type": "ProjectV2SingleSelectField", "options": [{"id": "K_f", "name": "feature"}, {"id": "K_b", "name": "bug"}, {"id": "K_t", "name": "tooling"}]},
    {"id": "F_phase", "name": "Phase", "type": "ProjectV2Field"},
    {"id": "F_author", "name": "Card author", "type": "ProjectV2Field"},
    {"id": "F_shape", "name": "Shape", "type": "ProjectV2SingleSelectField", "options": [{"id": "S_s", "name": "Story"}, {"id": "S_c", "name": "Contract"}]},
    {"id": "F_en", "name": "Enables", "type": "ProjectV2Field"},
    {"id": "F_status", "name": "Status", "type": "ProjectV2SingleSelectField", "options": [{"id": f"St_{i}", "name": n} for i, n in enumerate(STATUS_OPTIONS)]},
]

DRAFT = """# Plain C# baseline on the same tasks

- **Status:** Ready
- **Shape:** Story
- **Type:** feature
- **Milestone:** Measure first
- **Source:** Steve
- **Depends on:** none
- **Story:** As Riley, I want a plain C# baseline, so that I can see what Phrase must beat.
- **Done checks:**
  1. Given x, When y, Then z
"""


class FakeGH:
    def __init__(self, ranks=None):
        self.calls = []
        self.issues = []
        self.items = [
            {"id": f"PVTI_{n}", "title": f"({n:03d}) c", "status": "Ready", "card number": n, "rank": r, "content": {"number": 100 + n, "type": "Issue", "url": f"https://github.com/{SLUG}/issues/{100 + n}"}}
            for n, r in (ranks or {}).items()
        ]

    def __call__(self, args, **kwargs):
        cmd = list(args)
        self.calls.append(cmd)
        if cmd[:2] == ["issue", "list"]:
            return json.dumps(self.issues)
        if cmd[:2] == ["label", "list"]:
            return json.dumps([{"name": "card"}, {"name": "tooling"}])
        if cmd[:2] == ["issue", "create"]:
            number = 200 + len(self.issues)
            body = Path(cmd[cmd.index("--body-file") + 1]).read_text(encoding="utf-8")
            self.issues.append({"number": number, "title": cmd[cmd.index("--title") + 1], "body": body, "state": "OPEN", "url": f"https://github.com/{SLUG}/issues/{number}"})
            return f"https://github.com/{SLUG}/issues/{number}"
        if cmd[:2] == ["project", "view"]:
            return json.dumps({"id": "PVT_1"})
        if cmd[:2] == ["project", "field-list"]:
            return json.dumps({"fields": FIELDS})
        if cmd[:2] == ["project", "item-list"]:
            return json.dumps({"items": self.items, "totalCount": len(self.items)})
        if cmd[:2] == ["project", "item-add"]:
            url = cmd[cmd.index("--url") + 1]
            number = int(url.rsplit("/", 1)[1])
            item = {"id": f"PVTI_new{number}", "title": "new", "content": {"number": number, "type": "Issue", "url": url}}
            self.items.append(item)
            return json.dumps(item)
        if cmd[:2] == ["project", "item-edit"]:
            item = next(x for x in self.items if x["id"] == cmd[cmd.index("--id") + 1])
            field = next(f for f in FIELDS if f["id"] == cmd[cmd.index("--field-id") + 1])
            key = field["name"].lower()
            if "--number" in cmd:
                item[key] = float(cmd[cmd.index("--number") + 1])
            elif "--text" in cmd:
                if field["name"] in {"Rank", "Card number"}:
                    raise RuntimeError("Did not receive a number value")
                item[key] = cmd[cmd.index("--text") + 1]
            else:
                option_id = cmd[cmd.index("--single-select-option-id") + 1]
                item[key] = next(o["name"] for o in field["options"] if o["id"] == option_id)
            return ""
        raise AssertionError(f"Unhandled fake gh call: {cmd}")

    def mutating(self):
        return [c for c in self.calls if c[:2] in (["issue", "create"], ["project", "item-add"], ["project", "item-edit"])]


class NewCardTests(unittest.TestCase):
    def draft(self, tmp, name="084-plain-csharp-baseline.md", text=DRAFT):
        path = Path(tmp) / name
        path.write_text(text, encoding="utf-8")
        return path

    def test_dry_run_creates_nothing(self):
        fake = FakeGH()
        with tempfile.TemporaryDirectory() as tmp:
            actions = new_card.new_card(fake, self.draft(tmp), dry_run=True)
        self.assertEqual(fake.mutating(), [])
        self.assertTrue(any("create issue" in a for a in actions))

    def test_apply_creates_one_marked_issue_adds_the_item_and_sets_the_fields(self):
        fake = FakeGH()
        with tempfile.TemporaryDirectory() as tmp:
            new_card.new_card(fake, self.draft(tmp), dry_run=False, sleep=lambda s: None)
        self.assertEqual(len(fake.issues), 1)
        self.assertTrue(fake.issues[0]["body"].startswith("<!-- phrase-card:084 -->"))
        self.assertEqual(fake.issues[0]["title"], "(084) Plain C# baseline on the same tasks")
        item = fake.items[-1]
        self.assertEqual((item["card number"], item["status"], item["shape"], item["kind"], item["depends on"]), (84.0, "Ready", "Story", "feature", "none"))

    def test_a_second_apply_for_the_same_card_creates_nothing(self):
        fake = FakeGH()
        with tempfile.TemporaryDirectory() as tmp:
            path = self.draft(tmp)
            new_card.new_card(fake, path, dry_run=False, sleep=lambda s: None)
            before = len(fake.mutating())
            new_card.new_card(fake, path, dry_run=False, sleep=lambda s: None)
        self.assertEqual(len(fake.mutating()), before)

    def test_a_role_only_story_is_refused_before_anything_is_created(self):
        fake = FakeGH()
        bad = DRAFT.replace("As Riley, I want a plain C# baseline, so that I can see what Phrase must beat.", "As an engineer, I want a baseline.")
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaises(ValueError):
                new_card.new_card(fake, self.draft(tmp, text=bad), dry_run=False, sleep=lambda s: None)
        self.assertEqual(fake.mutating(), [])

    def test_no_delete_and_the_wrong_repo_is_refused(self):
        fake = FakeGH()
        with tempfile.TemporaryDirectory() as tmp:
            new_card.new_card(fake, self.draft(tmp), dry_run=False, sleep=lambda s: None)
            with self.assertRaises(ValueError):
                new_card.new_card(fake, self.draft(tmp), repo="other", dry_run=True)
        self.assertFalse(any(arg.lower() == "delete" for call in fake.calls for arg in call))


class RerankTests(unittest.TestCase):
    def ranks(self, fake):
        return {x["card number"]: x.get("rank") for x in fake.items}

    def test_moving_cards_after_another_renumbers_only_what_changes(self):
        fake = FakeGH({1: 1, 2: 2, 3: 3, 4: 4, 5: 5})
        rerank.rerank(fake, after=2, cards=[5, 4], dry_run=False)
        self.assertEqual(self.ranks(fake), {1: 1, 2: 2, 5: 3, 4: 4, 3: 5})
        edits = [c for c in fake.calls if c[:2] == ["project", "item-edit"]]
        self.assertEqual(len(edits), 2)

    def test_dry_run_writes_nothing_and_a_second_apply_writes_nothing(self):
        fake = FakeGH({1: 1, 2: 2, 3: 3})
        rerank.rerank(fake, after=1, cards=[3], dry_run=True)
        self.assertEqual(fake.mutating(), [])
        rerank.rerank(fake, after=1, cards=[3], dry_run=False)
        before = len(fake.mutating())
        rerank.rerank(fake, after=1, cards=[3], dry_run=False)
        self.assertEqual(len(fake.mutating()), before)

    def test_an_unknown_card_or_anchor_is_an_error_and_writes_nothing(self):
        fake = FakeGH({1: 1, 2: 2})
        with self.assertRaises(ValueError):
            rerank.rerank(fake, after=9, cards=[2], dry_run=False)
        with self.assertRaises(ValueError):
            rerank.rerank(fake, after=1, cards=[9], dry_run=False)
        self.assertEqual(fake.mutating(), [])

    def test_unranked_cards_stay_unranked_and_new_cards_with_no_rank_are_placed(self):
        fake = FakeGH({1: 1, 2: 2})
        fake.items.append({"id": "PVTI_x", "title": "(084)", "card number": 84, "content": {"number": 284, "type": "Issue", "url": f"https://github.com/{SLUG}/issues/284"}})
        fake.items.append({"id": "PVTI_u", "title": "(003)", "card number": 3, "content": {"number": 103, "type": "Issue", "url": f"https://github.com/{SLUG}/issues/103"}})
        rerank.rerank(fake, after=1, cards=[84], dry_run=False)
        ranks = self.ranks(fake)
        self.assertEqual((ranks[1], ranks[84], ranks[2], ranks.get(3)), (1, 2, 3, None))

    def test_no_delete_and_only_rank_is_written(self):
        fake = FakeGH({1: 1, 2: 2, 3: 3})
        rerank.rerank(fake, after=1, cards=[3], dry_run=False)
        for call in fake.mutating():
            self.assertEqual(call[call.index("--field-id") + 1], "F_rank")
        self.assertFalse(any(arg.lower() == "delete" for call in fake.calls for arg in call))


if __name__ == "__main__":
    unittest.main()
