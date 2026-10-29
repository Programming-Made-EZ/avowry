import io
import json
import tempfile
import unittest
from contextlib import redirect_stdout
from pathlib import Path

from scripts.board import check_real_board, set_fields


OWNER = "Programming-Made-EZ"
SLUG = f"{OWNER}/avowry"


class FakeGH:
    """Recorded-call fake using the JSON shapes returned by the real gh CLI."""

    def __init__(self):
        self.calls = []
        self.project_id = "PVT_board"
        names = {
            "Rank": "PVTF_rank", "Card number": "PVTF_number", "Depends on": "PVTF_depends",
            "Kind": "PVTF_kind", "Phase": "PVTF_phase", "Card author": "PVTF_author",
            "Status": "PVTF_status", "Shape": "PVTF_shape", "Enables": "PVTF_enables",
        }
        self.fields = []
        for name, field_id in names.items():
            field = {"id": field_id, "name": name, "type": "ProjectV2SingleSelectField" if name in {"Kind", "Status", "Shape"} else "ProjectV2Field"}
            if name == "Kind":
                field["options"] = [{"id": f"kind-{value}", "name": value} for value in set_fields.KINDS]
            elif name == "Status":
                field["options"] = [{"id": f"status-{value.lower().replace(' ', '-')}", "name": value} for value in set_fields.STATUSES]
            elif name == "Shape":
                field["options"] = [{"id": f"shape-{value.lower()}", "name": value} for value in set_fields.SHAPES]
            self.fields.append(field)
        self.issues = []
        self.items = []

    def add_issue(self, number, card, state="OPEN"):
        issue = {"number": number, "title": f"({card}) Fixture", "body": f"<!-- phrase-card:{card} -->\nbody", "state": state, "url": f"https://example.test/issues/{number}"}
        self.issues.append(issue)
        return issue

    def __call__(self, args, *, check=True, input_text=None):
        cmd = [str(x) for x in args]
        self.calls.append(cmd)
        if cmd[:2] == ["project", "view"]:
            return json.dumps({"id": self.project_id, "number": 1, "title": "Phrase Board"})
        if cmd[:2] == ["project", "field-list"]:
            return json.dumps({"fields": self.fields, "totalCount": len(self.fields)})
        if cmd[:2] == ["project", "item-list"]:
            return json.dumps({"items": self.items, "totalCount": len(self.items)})
        if cmd[:2] == ["issue", "list"]:
            return json.dumps(self.issues)
        if cmd[:2] == ["project", "item-add"]:
            url = cmd[cmd.index("--url") + 1]
            issue = next(x for x in self.issues if x["url"] == url)
            item = {"id": f"PVTI_{issue['number']}", "content": {"number": issue["number"], "type": "Issue", "url": url}, "title": issue["title"]}
            self.items.append(item)
            return json.dumps(item)
        if cmd[:2] == ["project", "item-edit"]:
            item = next(x for x in self.items if x["id"] == cmd[cmd.index("--id") + 1])
            field = next(x for x in self.fields if x["id"] == cmd[cmd.index("--field-id") + 1])
            key = field["name"].lower()  # the real item-list key
            if "--number" in cmd:
                item[key] = float(cmd[cmd.index("--number") + 1])
            elif "--text" in cmd:
                if field["name"] in {"Rank", "Card number"}:  # what the real API answered when a number field got --text
                    raise RuntimeError("GraphQL: Did not receive a number value to update a field of type number (updateProjectV2ItemFieldValue)")
                item[key] = cmd[cmd.index("--text") + 1]
            else:
                option_id = cmd[cmd.index("--single-select-option-id") + 1]
                item[key] = next(x["name"] for x in field["options"] if x["id"] == option_id)
            return ""
        if cmd[:2] == ["issue", "close"]:
            issue = next(x for x in self.issues if str(x["number"]) == cmd[2])
            issue["state"] = "CLOSED"
            return ""
        raise AssertionError(f"Unhandled fake gh call: {cmd}")


def fixture_board(root):
    board = Path(root)
    cards = {
        "001-one.md": "# One\n\n- **Status:** Ready\n- **Shape:** Contract\n- **Enables:** Stories 004 and 002; not card 99\n- **Type:** feature details\n- **Milestone:** First sentence. More detail.\n- **Depends on:** 007, exact text\n\n## Task force\n\n- Driver [author]: Codex | why\n",
        "002-two.md": "# Two\n\n- **Status:** Done 2026-10-26\n- **Type:** bug\n",
        "003-three.md": "# Three\n\n- **Status:** SUPERSEDED by 004\n- **Type:** mystery\n",
        "004-four.md": "# Four\n\n- **Status:** Ready\n- **Type:** docs only\n",
    }
    for name, text in cards.items():
        (board / name).write_text(text, encoding="utf-8")
    (board / "MILESTONE-usable-v1.md").write_text(
        "| Rank | Card | Why | Origin |\n|---|---|---|---|\n| 1 | 004 Four | x | y |\n| 2 | 001 One (**Done**) | x | y |\n| 3 | 003 Three | x | y |\n",
        encoding="utf-8",
    )
    return board


class BoardFieldTests(unittest.TestCase):
    def run_main(self, args, fake, board):
        out = io.StringIO()
        with redirect_stdout(out):
            code = set_fields.main(args, gh_runner=fake, board_dir=board)
        return code, out.getvalue()

    def test_values_status_rank_metadata_and_running(self):
        with tempfile.TemporaryDirectory() as tmp:
            board = fixture_board(tmp)
            values = {x.number: x.values for x in set_fields.load_cards(board, running="004")}
        self.assertEqual(values["001"], {"Rank": 2, "Card number": 1, "Depends on": "007, exact text", "Kind": "feature", "Phase": "First sentence.", "Card author": "Codex", "Status": "Done", "Shape": "Contract", "Enables": "004, 002"})
        self.assertEqual(values["002"], {"Card number": 2, "Kind": "bug", "Status": "Done", "Shape": "Story"})
        self.assertEqual(values["003"]["Status"], "Superseded/Withdrawn")
        self.assertNotIn("Kind", values["003"])
        self.assertEqual(values["004"]["Rank"], 1)
        self.assertEqual(values["004"]["Status"], "In Progress")
        self.assertNotIn("Rank", values["002"])
        self.assertNotIn("Depends on", values["002"])
        self.assertEqual(len({values[n]["Rank"] for n in values if "Rank" in values[n]}), 3)

    def test_apply_is_idempotent_adds_one_item_and_closes_open_done_once(self):
        fake = FakeGH()
        for i in range(1, 5):
            fake.add_issue(100 + i, f"{i:03}")
        with tempfile.TemporaryDirectory() as tmp:
            board = fixture_board(tmp)
            code, _ = self.run_main(["--apply"], fake, board)
            first_writes = [c for c in fake.calls if c[:2] in (["project", "item-add"], ["project", "item-edit"], ["issue", "close"])]
            before = len(fake.calls)
            again, output = self.run_main(["--apply"], fake, board)
            second_writes = [c for c in fake.calls[before:] if c[:2] in (["project", "item-add"], ["project", "item-edit"], ["issue", "close"])]
        self.assertEqual((code, again), (0, 0))
        self.assertEqual(len(fake.items), 4)
        self.assertTrue(first_writes)
        self.assertEqual(second_writes, [])
        self.assertIn("Nothing to do", output)
        closes = [c for c in fake.calls if c[:2] == ["issue", "close"]]
        self.assertEqual(closes, [["issue", "close", "101", "--repo", SLUG, "--reason", "completed"], ["issue", "close", "102", "--repo", SLUG, "--reason", "completed"]])
        self.assertFalse(any(arg.lower() == "delete" for call in fake.calls for arg in call))

    def test_default_dry_run_and_wrong_owner_or_repo_refused(self):
        fake = FakeGH()
        with tempfile.TemporaryDirectory() as tmp:
            board = fixture_board(tmp)
            self.run_main([], fake, board)
            self.assertFalse(any(c[:2] in (["project", "item-add"], ["project", "item-edit"], ["issue", "close"]) for c in fake.calls))
            with self.assertRaises(ValueError):
                set_fields.main(["--owner", "wrong"], gh_runner=fake, board_dir=board)
            with self.assertRaises(ValueError):
                set_fields.main(["--repo", "wrong"], gh_runner=fake, board_dir=board)

    def test_fields_verifier_is_read_only_and_detects_mismatch(self):
        fake = FakeGH()
        for i in range(1, 5):
            fake.add_issue(100 + i, f"{i:03}")
        with tempfile.TemporaryDirectory() as tmp:
            board = fixture_board(tmp)
            self.run_main(["--apply"], fake, board)
            before = len(fake.calls)
            self.assertEqual(check_real_board.verify_fields(fake, board_dir=board), 4)
            read_calls = fake.calls[before:]
            self.assertFalse(any(c[:2] in (["project", "item-add"], ["project", "item-edit"], ["issue", "close"]) for c in read_calls))
            fake.items[0]["rank"] = 99
            with self.assertRaises(RuntimeError):
                check_real_board.verify_fields(fake, board_dir=board)

    def test_contract_without_enables_fails_the_fields_check(self):
        fake = FakeGH()
        for i in range(1, 5):
            fake.add_issue(100 + i, f"{i:03}")
        with tempfile.TemporaryDirectory() as tmp:
            board = fixture_board(tmp)
            self.run_main(["--apply"], fake, board)
            contract = next(item for item in fake.items if item["card number"] == 1)
            contract.pop("enables")
            with self.assertRaisesRegex(RuntimeError, "Contract 001 has empty Enables"):
                check_real_board.verify_fields(fake, board_dir=board)

    def test_contract_cannot_enable_another_contract(self):
        fake = FakeGH()
        for i in range(1, 5):
            fake.add_issue(100 + i, f"{i:03}")
        with tempfile.TemporaryDirectory() as tmp:
            board = fixture_board(tmp)
            self.run_main(["--apply"], fake, board)
            target = next(item for item in fake.items if item["card number"] == 4)
            target["shape"] = "Contract"
            with self.assertRaisesRegex(RuntimeError, "Contract 001 enables Contract 004"):
                check_real_board.verify_fields(fake, board_dir=board)

    def test_contract_cannot_enable_a_card_missing_from_the_board(self):
        fake = FakeGH()
        for i in range(1, 5):
            fake.add_issue(100 + i, f"{i:03}")
        with tempfile.TemporaryDirectory() as tmp:
            board = fixture_board(tmp)
            self.run_main(["--apply"], fake, board)
            contract = next(item for item in fake.items if item["card number"] == 1)
            contract["enables"] = "999"
            with self.assertRaisesRegex(RuntimeError, "card 999, which is not on the board"):
                check_real_board.verify_fields(fake, board_dir=board)


class TransientErrorTests(unittest.TestCase):
    def test_a_transient_github_error_is_retried_then_succeeds(self):
        # Given gh answers 'Something went wrong' once (the real API did, mid-apply), then succeeds
        import subprocess
        from unittest import mock

        answers = [subprocess.CompletedProcess([], 1, "", "GraphQL: Something went wrong while executing your query"),
                   subprocess.CompletedProcess([], 0, "ok", "")]
        with mock.patch.object(subprocess, "run", side_effect=answers) as run, mock.patch.object(set_fields.time, "sleep"):
            # When the runner is called
            out = set_fields.gh_runner(["project", "item-list", "1"])
        # Then it retried once and returned the output
        self.assertEqual((out, run.call_count), ("ok", 2))

    def test_a_real_error_is_not_retried(self):
        import subprocess
        from unittest import mock

        bad = subprocess.CompletedProcess([], 1, "", "GraphQL: Did not receive a number value")
        with mock.patch.object(subprocess, "run", return_value=bad) as run, mock.patch.object(set_fields.time, "sleep"):
            with self.assertRaises(RuntimeError):
                set_fields.gh_runner(["project", "item-edit"])
        self.assertEqual(run.call_count, 1)


if __name__ == "__main__":
    unittest.main()
