import io
import json
import tempfile
import unittest
from contextlib import redirect_stdout
from pathlib import Path

from scripts.board import check_real_board, migrate_cards


SLUG = "Programming-Made-EZ/phrase-lang"
BOARD = Path(__file__).resolve().parents[2] / "board"

OLD_STORY = """# Sync the story

- **Status:** Ready
- **Type:** tooling

- **Story:** As Steve, I want the old story line, so that the board stays stale.
"""

NEW_STORY = """# Sync the story now

- **Status:** Ready
- **Type:** tooling

- **Story:** As Steve, I want the new persona story, so that I can trust every card.
"""

SAME_STORY = """# Leave this body

- **Status:** Ready
- **Type:** bug

- **Story:** As Steve, I want this sentence to stay, so that unchanged bodies are left alone.
"""

MISSING_STORY = """# Not migrated

- **Status:** Ready
- **Type:** docs

- **Story:** As Steve, I want this card to stay off the board, so that sync does not create issues.
"""

DONE_OLD = """# Finished card

- **Status:** Done 2026-10-26
- **Type:** tooling

- **Story:** As Steve, I want the old done line, so that closing is a separate step.
"""

DONE_NEW = """# Finished card

- **Status:** Done 2026-10-26
- **Type:** tooling

- **Story:** As Steve, I want the new done line, so that the body can change without a close.
"""


def as_github(text):
    """Store a body the way GitHub does: CRLF, even when the card file is LF."""
    return text.replace("\r\n", "\n").replace("\n", "\r\n")


def opposite_endings(text):
    """Return the same characters with the other line ending, so a raw compare fails."""
    lf = text.replace("\r\n", "\n")
    if "\r\n" in text:
        return lf
    return lf.replace("\n", "\r\n")


def write_card(board, name, text):
    path = Path(board) / name
    path.write_bytes(text.encode("utf-8"))
    return path


def tracked_issue(number, card, body, **extra):
    issue = {
        "number": number,
        "title": f"({card.number}) {card.title}",
        "body": body,
        "state": "OPEN",
        "labels": ["custom-label"],
        "assignees": ["steve"],
        "comments": [{"id": number, "body": "leave this comment"}],
        "close_reason": None,
    }
    issue.update(extra)
    return issue


def snapshot(issue):
    return {
        "number": issue["number"],
        "title": issue["title"],
        "body": issue["body"],
        "state": issue["state"],
        "labels": list(issue["labels"]),
        "assignees": list(issue["assignees"]),
        "comments": list(issue["comments"]),
        "close_reason": issue["close_reason"],
    }


class FakeGH:
    """Recorded-call gh that returns the real issue list shape.

    An edit stores the body with CRLF. GitHub does that, and a second apply
    has to treat those bytes as the body just written from an LF card file.
    """

    def __init__(self):
        self.calls = []
        self.issues = []
        self.labels = []
        self.body_files = []
        self.edited_bodies = []

    def _next_number(self):
        if not self.issues:
            return 1
        return max(issue["number"] for issue in self.issues) + 1

    def _labels_from(self, cmd):
        labels = []
        for index, part in enumerate(cmd):
            if part == "--label" and index + 1 < len(cmd):
                labels.append(cmd[index + 1])
        return labels

    def __call__(self, args, check=True, input_text=None):
        cmd = [str(part) for part in args]
        self.calls.append(cmd)
        if any(part.lower() == "delete" for part in cmd):
            raise RuntimeError("Refusing a gh command that includes delete.")

        if cmd[:2] == ["issue", "list"]:
            payload = [
                {"number": issue["number"], "title": issue["title"], "body": issue["body"], "state": issue["state"]}
                for issue in self.issues
            ]
            return json.dumps(payload)

        if cmd[:2] == ["label", "list"]:
            return json.dumps([{"name": name} for name in self.labels])

        if cmd[:2] == ["label", "create"]:
            name = cmd[2]
            if name not in self.labels:
                self.labels.append(name)
            return name

        if cmd[:2] == ["issue", "create"]:
            title = cmd[cmd.index("--title") + 1]
            body_path = cmd[cmd.index("--body-file") + 1]
            self.body_files.append(body_path)
            with open(body_path, encoding="utf-8", newline="") as handle:
                body = handle.read()
            number = self._next_number()
            issue = {
                "number": number,
                "title": title,
                "body": body,
                "state": "OPEN",
                "labels": self._labels_from(cmd),
                "assignees": [],
                "comments": [],
                "close_reason": None,
            }
            self.issues.append(issue)
            return f"https://github.com/{SLUG}/issues/{number}"

        if cmd[:2] == ["issue", "edit"]:
            if "--body-file" not in cmd:
                raise AssertionError(f"issue edit is missing --body-file: {cmd}")
            number = int(cmd[2])
            body_path = cmd[cmd.index("--body-file") + 1]
            self.body_files.append(body_path)
            with open(body_path, encoding="utf-8", newline="") as handle:
                body = handle.read()
            self.edited_bodies.append(body)
            stored = as_github(body)
            for issue in self.issues:
                if issue["number"] == number:
                    issue["body"] = stored
                    return f"https://github.com/{SLUG}/issues/{number}"
            raise RuntimeError(f"issue {number} not found")

        if cmd[:2] == ["issue", "close"]:
            number = int(cmd[2])
            reason = cmd[cmd.index("--reason") + 1]
            for issue in self.issues:
                if issue["number"] == number:
                    issue["state"] = "CLOSED"
                    issue["close_reason"] = reason
                    return ""
            raise RuntimeError(f"issue {number} not found")

        if cmd[:2] == ["issue", "reopen"]:
            number = int(cmd[2])
            for issue in self.issues:
                if issue["number"] == number:
                    issue["state"] = "OPEN"
                    issue["close_reason"] = None
                    return ""
            raise RuntimeError(f"issue {number} not found")

        raise AssertionError(f"Unhandled fake gh call: {cmd}")


class BoardSyncTests(unittest.TestCase):
    def run_main(self, argv, fake, board_dir):
        buffer = io.StringIO()
        with redirect_stdout(buffer):
            code = migrate_cards.main(argv, gh_runner=fake, sleep=lambda _seconds: None, board_dir=board_dir)
        return code, buffer.getvalue()

    def populate(self, board):
        old_path = write_card(board, "001-sync.md", OLD_STORY)
        old_card = migrate_cards.load_card(old_path)
        write_card(board, "001-sync.md", NEW_STORY)
        new_card = migrate_cards.load_card(old_path)
        same_card = migrate_cards.load_card(write_card(board, "002-same.md", SAME_STORY))
        missing_card = migrate_cards.load_card(write_card(board, "003-missing.md", MISSING_STORY))
        fake = FakeGH()
        fake.issues.append(tracked_issue(42, old_card, as_github(old_card.body), title="(001) Sync the story"))
        fake.issues.append(tracked_issue(43, same_card, as_github(same_card.body)))
        return fake, new_card, same_card, missing_card

    def test_dry_run_lists_each_changed_body_and_changes_nothing(self):
        # Given one changed story, one body that already matches aside from CRLF, and one card with no issue
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            fake, _new_card, _same_card, _missing_card = self.populate(board)
            before = [snapshot(issue) for issue in fake.issues]

            # When sync-bodies runs as a dry run, including with --apply --dry-run together
            code, output = self.run_main(["--sync-bodies"], fake, board)
            again, again_output = self.run_main(["--sync-bodies", "--dry-run"], fake, board)
            held, held_output = self.run_main(["--sync-bodies", "--apply", "--dry-run"], fake, board)

        # Then each differing marked issue is one sync-body line, the counts gain sync-body, and gh is not asked to edit
        self.assertEqual(code, 0)
        self.assertEqual(again, 0)
        self.assertEqual(held, 0)
        expected = [
            "sync-body (001) Sync the story now",
            "skip (002) Leave this body",
            "skip (003) Not migrated",
            "0 create, 0 close, 0 close-existing, 2 skip, 1 sync-body",
        ]
        self.assertEqual(output.strip().splitlines(), expected)
        self.assertEqual(again_output.strip().splitlines(), expected)
        self.assertEqual(held_output.strip().splitlines(), expected)
        self.assertEqual([snapshot(issue) for issue in fake.issues], before)
        self.assertTrue(all(call[:2] == ["issue", "list"] for call in fake.calls))
        self.assertFalse(any(call[:2] == ["issue", "edit"] for call in fake.calls))
        self.assertFalse(any(call[:2] == ["issue", "create"] for call in fake.calls))
        self.assertFalse(any(part.lower() == "delete" for call in fake.calls for part in call))

    def test_apply_edits_only_the_changed_body_and_a_second_apply_does_nothing(self):
        # Given the changed story is the only body that differs once CRLF and LF are folded together
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            fake, new_card, same_card, _missing_card = self.populate(board)
            unchanged = snapshot(fake.issues[1])
            sleeps = []

            # When apply runs, and then apply runs again after GitHub has stored CRLF
            with redirect_stdout(io.StringIO()):
                code = migrate_cards.main(
                    ["--sync-bodies", "--apply"],
                    gh_runner=fake,
                    sleep=lambda seconds: sleeps.append(seconds),
                    board_dir=board,
                )
            edits = [call for call in fake.calls if call[:2] == ["issue", "edit"]]
            code_again, again_output = self.run_main(["--sync-bodies", "--apply"], fake, board)

        # Then exactly one edit rewrites that body from a temp file, and the second apply edits nothing
        self.assertEqual(code, 0)
        self.assertEqual(code_again, 0)
        self.assertEqual(len(edits), 1)
        call = edits[0]
        self.assertEqual(call[:5], ["issue", "edit", "42", "--repo", SLUG])
        self.assertEqual(call[5], "--body-file")
        self.assertEqual(len(call), 7)
        self.assertNotIn("--title", call)
        self.assertNotIn("--body", call)
        self.assertNotIn("--add-label", call)
        self.assertNotIn("--remove-label", call)
        self.assertNotIn("--label", call)
        self.assertNotIn("close", call)
        self.assertNotIn("reopen", call)
        for forbidden in ("--title", "--add-label", "--remove-label", "close", "reopen"):
            self.assertNotIn(forbidden, call)
        body_path = Path(call[6])
        self.assertFalse(body_path.exists())
        self.assertTrue(body_path.name.startswith("phrase-card-001-"))
        self.assertTrue(str(body_path.resolve()).startswith(str(Path(tempfile.gettempdir()).resolve())))
        self.assertEqual(fake.edited_bodies, [new_card.body])
        self.assertEqual(fake.edited_bodies[0].splitlines()[0], "<!-- phrase-card:001 -->")
        self.assertNotIn("\r", new_card.body)
        changed = fake.issues[0]
        self.assertIn("\r\n", changed["body"])
        self.assertEqual(migrate_cards.normalise_newlines(changed["body"]), migrate_cards.normalise_newlines(new_card.body))
        self.assertEqual(changed["title"], "(001) Sync the story")
        self.assertEqual(changed["state"], "OPEN")
        self.assertEqual(changed["labels"], ["custom-label"])
        self.assertEqual(changed["assignees"], ["steve"])
        self.assertEqual(changed["comments"], [{"id": 42, "body": "leave this comment"}])
        self.assertIsNone(changed["close_reason"])
        self.assertEqual(snapshot(fake.issues[1]), unchanged)
        self.assertEqual(
            migrate_cards.normalise_newlines(fake.issues[1]["body"]),
            migrate_cards.normalise_newlines(same_card.body),
        )
        self.assertEqual(len(fake.issues), 2)
        self.assertEqual(edits, [call for call in fake.calls if call[:2] == ["issue", "edit"]])
        self.assertFalse(any(call[:2] == ["issue", "create"] for call in fake.calls))
        self.assertFalse(any(call[:2] == ["issue", "close"] for call in fake.calls))
        self.assertFalse(any(call[:2] == ["issue", "reopen"] for call in fake.calls))
        self.assertFalse(any(call[:2] == ["label", "create"] for call in fake.calls))
        self.assertFalse(any(part.lower() == "delete" for call in fake.calls for part in call))
        self.assertEqual(sleeps, [])
        self.assertIn("Nothing to do", again_output)

    def test_crlf_versus_lf_is_not_a_body_change(self):
        # Given the issue body is the card body with CRLF where the card file has LF
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            card = migrate_cards.load_card(write_card(board, "001-same.md", SAME_STORY))
            self.assertNotIn("\r", card.body)
            fake = FakeGH()
            stored = as_github(card.body)
            self.assertIn("\r\n", stored)
            self.assertNotEqual(stored, card.body)
            fake.issues.append(tracked_issue(7, card, stored, title="(001) Leave this body"))
            before = snapshot(fake.issues[0])

            # When a dry run and an apply both compare the two bodies
            dry_code, dry_out = self.run_main(["--sync-bodies"], fake, board)
            code, output = self.run_main(["--sync-bodies", "--apply"], fake, board)

        # Then neither run edits the issue
        self.assertEqual(dry_code, 0)
        self.assertEqual(code, 0)
        self.assertEqual(dry_out.strip(), "Nothing to do.")
        self.assertIn("Nothing to do", output)
        self.assertNotIn("sync-body", dry_out)
        self.assertFalse(any(call[:2] == ["issue", "edit"] for call in fake.calls))
        self.assertEqual(snapshot(fake.issues[0]), before)

    def test_wrong_owner_or_repo_is_refused(self):
        fake = FakeGH()
        with self.assertRaises(ValueError):
            migrate_cards.main(["--sync-bodies", "--repo", "other-repo"], gh_runner=fake, sleep=lambda _seconds: None)
        with self.assertRaises(ValueError):
            migrate_cards.main(
                ["--sync-bodies", "--apply", "--owner", "some-other-owner"],
                gh_runner=fake,
                sleep=lambda _seconds: None,
            )
        with self.assertRaises(ValueError):
            migrate_cards.migrate(fake, owner="Programming-Made-EZ", repo="other-repo", sync_bodies=True)
        with self.assertRaises(ValueError):
            migrate_cards.migrate(fake, owner="some-other-owner", repo="phrase-lang", sync_bodies=True, dry_run=False)
        self.assertEqual(fake.calls, [])
        self.assertEqual(fake.issues, [])

    def test_no_delete_unmarked_issue_and_missing_card(self):
        # Given a marked issue, a human issue with no marker, and a card that was never migrated
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            card = migrate_cards.load_card(write_card(board, "001-sync.md", NEW_STORY))
            fake = FakeGH()
            human = tracked_issue(
                9,
                card,
                "Please delete nothing. This issue has no phrase-card marker.",
                title="A human issue",
                labels=["question"],
                state="OPEN",
            )
            human["body"] = "Please delete nothing. This issue has no phrase-card marker."
            fake.issues.append(human)
            write_card(board, "002-missing.md", MISSING_STORY)

            # When sync apply runs, the marked card has no issue yet so there is nothing to sync
            code, output = self.run_main(["--sync-bodies", "--apply"], fake, board)

        # Then nothing is created, nothing is deleted, and the unmarked issue is left byte for byte
        self.assertEqual(code, 0)
        self.assertIn("Nothing to do", output)
        self.assertEqual(len(fake.issues), 1)
        self.assertEqual(fake.issues[0]["body"], "Please delete nothing. This issue has no phrase-card marker.")
        self.assertEqual(fake.issues[0]["title"], "A human issue")
        self.assertEqual(fake.issues[0]["state"], "OPEN")
        self.assertEqual(fake.issues[0]["labels"], ["question"])
        self.assertFalse(any(call[:2] == ["issue", "create"] for call in fake.calls))
        self.assertFalse(any(call[:2] == ["issue", "edit"] for call in fake.calls))
        self.assertFalse(any(part.lower() == "delete" for call in fake.calls for part in call))

        def refuse(args, check=True):
            raise AssertionError(args)

        for args in (["delete"], ["DELETE"], ["issue", "delete", "4"], ["repo", "delete"]):
            with self.assertRaises(RuntimeError):
                migrate_cards.invoke(refuse, args, lambda _seconds: None)

    def test_a_story_that_mentions_delete_is_still_synced(self):
        # Given the new story merely contains the word delete, and the issue already has the marker
        text = """# Delete guard

- **Status:** Ready
- **Type:** tooling

- **Story:** As Steve, I want delete_guard.py mentioned here, so that the word delete is not a command.
"""
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            path = write_card(board, "001-delete-guard.md", text.replace("mentioned here", "missing here"))
            old = migrate_cards.load_card(path)
            write_card(board, "001-delete-guard.md", text)
            card = migrate_cards.load_card(path)
            fake = FakeGH()
            fake.issues.append(tracked_issue(4, old, old.body, title="(001) Delete guard"))

            # When apply syncs that one body
            code, _output = self.run_main(["--sync-bodies", "--apply"], fake, board)

        # Then the edit carries no delete argument, and the body text is allowed to say delete
        self.assertEqual(code, 0)
        edits = [call for call in fake.calls if call[:2] == ["issue", "edit"]]
        self.assertEqual(len(edits), 1)
        for part in edits[0]:
            self.assertNotEqual(part.lower(), "delete")
        self.assertIn("delete_guard.py", fake.edited_bodies[0])
        self.assertIn("<!-- phrase-card:001 -->", fake.edited_bodies[0])

    def test_path_scrubbing_is_part_of_the_compared_body(self):
        # Given the issue still has a local path and the card file would scrub it
        text = (
            "# Paths\n\n- **Status:** Ready\n- **Type:** docs\n\n"
            "See C:\\Users\\ringe\\projects\\phrase-lang\\board and C:/Users/ringe/mob-crosscli/x.\n"
        )
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            path = write_card(board, "001-paths.md", text)
            card = migrate_cards.load_card(path)
            raw_rest = text.partition("\n")[2]
            raw_body = "<!-- phrase-card:001 -->\n" + raw_rest
            fake = FakeGH()
            fake.issues.append(tracked_issue(8, card, raw_body, title="(001) Paths"))

            # When the dry run compares and apply writes the scrubbed body
            dry_code, dry_out = self.run_main(["--sync-bodies"], fake, board)
            self.assertEqual(fake.issues[0]["body"], raw_body)
            code, _output = self.run_main(["--sync-bodies", "--apply"], fake, board)

        # Then the issue is listed and the written body uses the scrubbed paths
        self.assertEqual(dry_code, 0)
        self.assertEqual(code, 0)
        self.assertIn("sync-body (001) Paths", dry_out)
        self.assertIn("1 sync-body", dry_out.splitlines()[-1])
        self.assertEqual(fake.edited_bodies, [card.body])
        self.assertIn("<phrase-lang>", card.body)
        self.assertIn("<mob-defaults>", card.body)
        self.assertNotIn("C:\\Users", card.body)
        self.assertNotIn("C:/Users", card.body)

    def test_sync_does_not_close_and_plain_migrate_does_not_edit_the_body(self):
        # Given a Done card whose issue is still OPEN and whose story line changed
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            path = write_card(board, "001-done.md", DONE_OLD)
            old = migrate_cards.load_card(path)
            write_card(board, "001-done.md", DONE_NEW)
            card = migrate_cards.load_card(path)
            syncing = FakeGH()
            syncing.issues.append(tracked_issue(11, old, old.body, title="(001) Finished card", state="OPEN"))
            migrating = FakeGH()
            migrating.issues.append(tracked_issue(11, old, old.body, title="(001) Finished card", state="OPEN"))

            # When sync apply runs, and separately the original migrator runs
            sync_code, _sync_out = self.run_main(["--sync-bodies", "--apply"], syncing, board)
            migrate_code, _migrate_out = self.run_main(["--apply"], migrating, board)

        # Then sync edits the body and leaves the issue open, and migrate closes without editing the body
        self.assertEqual(sync_code, 0)
        self.assertEqual(migrate_code, 0)
        self.assertEqual([call[:2] for call in syncing.calls if call[1] != "list"], [["issue", "edit"]])
        self.assertEqual(syncing.issues[0]["state"], "OPEN")
        self.assertEqual(syncing.edited_bodies, [card.body])
        self.assertFalse(any(call[:2] == ["issue", "edit"] for call in migrating.calls))
        self.assertEqual(migrating.issues[0]["body"], old.body)
        self.assertEqual(migrating.issues[0]["state"], "CLOSED")
        self.assertEqual(migrating.issues[0]["title"], "(001) Finished card")

    def test_sync_refuses_an_issue_without_the_marker(self):
        card = migrate_cards.Card("001", "Sync the story", "<!-- phrase-card:001 -->\nnew body", None, True)
        action = migrate_cards.Action(
            "sync-body",
            card,
            "sync-body (001) Sync the story",
            issue_number=42,
            issue_body="no marker in the issue",
        )
        with self.assertRaises(RuntimeError) as caught:
            migrate_cards.validate_actions(SLUG, [action])
        self.assertIn("phrase-card:001", str(caught.exception))
        fake = FakeGH()
        with self.assertRaises(RuntimeError):
            migrate_cards.sync_issue_body(fake, SLUG, action, lambda _seconds: None)
        self.assertEqual(fake.calls, [])

    def test_check_bodies_matches_every_card_after_line_ending_normalisation(self):
        # Given every real card has one issue and the stored body uses the other line ending
        fake = FakeGH()
        cards = migrate_cards.load_cards(BOARD)
        for index, card in enumerate(cards, start=1):
            fake.issues.append({
                "number": 1000 + index,
                "title": f"({card.number}) kept title {index}",
                "body": opposite_endings(card.body),
                "state": "OPEN",
                "labels": ["custom-label"],
                "assignees": ["steve"],
                "comments": [{"id": index, "body": "leave this comment"}],
            })
        self.assertNotEqual(fake.issues[0]["body"], cards[0].body)
        fake.issues.append({
            "number": 5,
            "title": "A human issue",
            "body": "no marker here",
            "state": "OPEN",
        })

        # When the read-only body check runs
        buffer = io.StringIO()
        with redirect_stdout(buffer):
            code = check_real_board.main(["--bodies"], gh_runner=fake)
        counted = check_real_board.verify_bodies(fake)

        # Then every marked body matches its card and gh was only asked to list issues
        self.assertEqual(code, 0)
        self.assertEqual(counted, len(cards))
        self.assertIn(f"OK: {len(cards)} issue bodies match their card files, exactly one per card.", buffer.getvalue())
        self.assertTrue(all(call[:2] == ["issue", "list"] for call in fake.calls))
        self.assertIn(
            ["issue", "list", "--repo", SLUG, "--state", "all", "--limit", "1000", "--json", "number,title,body,state"],
            fake.calls,
        )
        self.assertFalse(any(part.lower() == "delete" for call in fake.calls for part in call))
        self.assertFalse(any(call[1] in {"create", "edit", "close", "reopen"} for call in fake.calls))
        self.assertEqual(fake.issues[0]["title"], f"(001) kept title 1")
        self.assertEqual(fake.issues[0]["labels"], ["custom-label"])

    def test_check_bodies_fails_when_the_text_or_the_set_of_issues_is_wrong(self):
        cards = migrate_cards.load_cards(BOARD)

        def matching():
            fake = FakeGH()
            for index, card in enumerate(cards, start=1):
                fake.issues.append({
                    "number": 1000 + index,
                    "title": f"({card.number}) {card.title}",
                    "body": opposite_endings(card.body),
                    "state": "OPEN",
                })
            return fake

        # A changed story line fails the read-only check and does not edit.
        changed = matching()
        changed.issues[0]["body"] = opposite_endings(cards[0].body) + "\nchanged story line"
        buffer = io.StringIO()
        with redirect_stdout(buffer):
            code = check_real_board.main(["--bodies"], gh_runner=changed)
        self.assertEqual(code, 1)
        self.assertIn("FAIL", buffer.getvalue())
        self.assertIn("001", buffer.getvalue())
        self.assertTrue(all(call[:2] == ["issue", "list"] for call in changed.calls))
        self.assertFalse(any(part.lower() == "delete" for call in changed.calls for part in call))

        # A missing card, a duplicate marker, an extra marker, and one issue wearing two markers fail too.
        missing = matching()
        missing.issues.pop()
        with self.assertRaises(RuntimeError) as missing_error:
            check_real_board.verify_bodies(missing)
        self.assertIn("missing", str(missing_error.exception))

        duplicate = matching()
        clone = dict(duplicate.issues[0])
        clone["number"] = 9000
        duplicate.issues.append(clone)
        with self.assertRaises(RuntimeError) as duplicate_error:
            check_real_board.verify_bodies(duplicate)
        self.assertIn("duplicates", str(duplicate_error.exception))

        extra = matching()
        extra.issues.append({
            "number": 9001,
            "title": "extra",
            "body": "<!-- phrase-card:999 -->\nnot a card file",
            "state": "OPEN",
        })
        with self.assertRaises(RuntimeError) as extra_error:
            check_real_board.verify_bodies(extra)
        self.assertIn("extras", str(extra_error.exception))

        shared = matching()
        second = cards[1].number
        shared.issues[0]["body"] = opposite_endings(cards[0].body) + f"\n<!-- phrase-card:{second} -->"
        shared.issues.pop(1)
        with self.assertRaises(RuntimeError) as shared_error:
            check_real_board.verify_bodies(shared)
        self.assertIn("shared", str(shared_error.exception))

        empty = FakeGH()
        fail_buffer = io.StringIO()
        with redirect_stdout(fail_buffer):
            fail_code = check_real_board.main(["--bodies"], gh_runner=empty)
        self.assertEqual(fail_code, 1)
        self.assertIn("FAIL", fail_buffer.getvalue())
        self.assertTrue(all(call[:2] == ["issue", "list"] for call in empty.calls))


if __name__ == "__main__":
    unittest.main()
