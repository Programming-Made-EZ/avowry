import io
import json
import re
import tempfile
import unittest
from contextlib import redirect_stdout
from pathlib import Path

from scripts.board import check_real_board, migrate_cards


SLUG = "Programming-Made-EZ/phrase-lang"
BOARD = Path(__file__).resolve().parents[2] / "board"
CARD_FILE_RE = re.compile(r"^(\d{3})-.+\.md$")
MARKER_RE = re.compile(r"<!-- phrase-card:(\d{3}) -->")
STATUS_RE = re.compile(r"(?m)^- \*\*Status:\*\*\s*(.*)$")
TYPE_RE = re.compile(r"(?m)^- \*\*Type:\*\*\s*(.*)$")
REPLACEMENTS = (
    ("C:\\Users\\ringe\\projects\\phrase-lang", "<phrase-lang>"),
    ("C:/Users/ringe/projects/phrase-lang", "<phrase-lang>"),
    ("C:\\Users\\ringe\\mob-crosscli", "<mob-defaults>"),
    ("C:/Users/ringe/mob-crosscli", "<mob-defaults>"),
)


def card_paths(board):
    return sorted((path for path in Path(board).iterdir() if path.is_file() and CARD_FILE_RE.match(path.name)), key=lambda path: path.name)


def read_card(path):
    return Path(path).read_bytes().decode("utf-8")


def heading_of(text):
    first = text.partition("\n")[0].rstrip("\r")
    match = re.match(r"^#\s*(.*)$", first)
    return match.group(1).strip() if match else first.strip()


def status_of(text):
    match = STATUS_RE.search(text)
    return match.group(1).strip() if match else ""


def close_reason_of(text):
    status = status_of(text)
    if status.startswith("Done"):
        return "completed"
    if status.startswith("SUPERSEDED") or status.startswith("WITHDRAWN"):
        return "not planned"
    return None


def tooling_of(text):
    match = TYPE_RE.search(text)
    if not match:
        return False
    return match.group(1).strip().startswith("tooling")


def scrub(text):
    for old, new in REPLACEMENTS:
        text = text.replace(old, new)
    return text


def expected_body(path, number):
    text = read_card(path)
    first, _sep, rest = text.partition("\n")
    newline = "\r\n" if first.endswith("\r") else "\n"
    return f"<!-- phrase-card:{number} -->" + newline + scrub(rest)


def expected_verb(text):
    return "close" if close_reason_of(text) else "create"


class FakeGH:
    """Recorded-call gh that returns the real issue and label JSON shapes."""

    def __init__(self):
        self.calls = []
        self.issues = []
        self.labels = []
        self.body_files = []
        self.rate_limit_creates = 0
        self.fail_creates = 0

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

        if self.rate_limit_creates > 0 and cmd[:2] == ["issue", "create"]:
            self.rate_limit_creates -= 1
            raise RuntimeError("HTTP 403: API rate limit exceeded for user")
        if self.fail_creates > 0 and cmd[:2] == ["issue", "create"]:
            self.fail_creates -= 1
            raise RuntimeError("validation failed")

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
                "close_reason": None,
            }
            self.issues.append(issue)
            return f"https://github.com/{SLUG}/issues/{number}"

        if cmd[:2] == ["issue", "close"]:
            number = int(cmd[2])
            reason = cmd[cmd.index("--reason") + 1]
            for issue in self.issues:
                if issue["number"] == number:
                    issue["state"] = "CLOSED"
                    issue["close_reason"] = reason
                    return ""
            raise RuntimeError(f"issue {number} not found")

        raise AssertionError(f"Unhandled fake gh call: {cmd}")


class BoardMigrateTests(unittest.TestCase):
    def run_main(self, argv, fake, **kwargs):
        sleep = kwargs.pop("sleep", lambda _seconds: None)
        board_dir = kwargs.pop("board_dir", None)
        buffer = io.StringIO()
        with redirect_stdout(buffer):
            code = migrate_cards.main(argv, gh_runner=fake, sleep=sleep, board_dir=board_dir)
        return code, buffer.getvalue()

    def test_default_dry_run_lists_one_line_per_card_and_changes_nothing(self):
        # Given the real board and a fake gh with no issues
        fake = FakeGH()
        sleeps = []
        # When the migrator runs with no flags (the default is dry-run) and again with --dry-run
        code, first = self.run_main([], fake, sleep=lambda seconds: sleeps.append(seconds))
        code_again, second = self.run_main(["--dry-run"], fake, sleep=lambda seconds: sleeps.append(seconds))

        # Then each card is one create or close line, both runs match, and gh is not asked to mutate
        cards = card_paths(BOARD)
        lines = first.strip().splitlines()
        self.assertEqual(code, 0)
        self.assertEqual(code_again, 0)
        self.assertEqual(first, second)
        self.assertEqual(len(lines), len(cards) + 1)
        self.assertNotIn("Nothing to do", first)
        self.assertEqual(sleeps, [])
        self.assertEqual(fake.issues, [])
        self.assertEqual(fake.labels, [])
        self.assertTrue(all(call[:2] == ["issue", "list"] for call in fake.calls))
        self.assertFalse(any("delete" in str(arg).lower() for call in fake.calls for arg in call))
        expected = []
        for path in cards:
            text = read_card(path)
            number = CARD_FILE_RE.match(path.name).group(1)
            expected.append(f"{expected_verb(text)} ({number}) {heading_of(text)}")
        creates = sum(line.startswith("create ") for line in expected)
        closes = sum(line.startswith("close ") for line in expected)
        expected.append(f"{creates} create, {closes} close, 0 close-existing, 0 skip")
        self.assertEqual(lines, expected)

    def test_apply_and_dry_run_flags_together_still_change_nothing(self):
        fake = FakeGH()
        code, _output = self.run_main(["--apply", "--dry-run"], fake)
        self.assertEqual(code, 0)
        self.assertEqual(fake.issues, [])
        self.assertFalse(any(call[1] in {"create", "close"} for call in fake.calls))

    def test_apply_creates_one_marked_issue_per_card(self):
        # Given every card file on the board
        fake = FakeGH()
        sleeps = []
        cards = card_paths(BOARD)
        # When apply runs once
        code, _output = self.run_main(["--apply"], fake, sleep=lambda seconds: sleeps.append(seconds))

        # Then there is one issue per card, the body is the card after its H1, and the pause sits between creates
        self.assertEqual(code, 0)
        self.assertEqual(len(fake.issues), len(cards))
        self.assertEqual(sleeps, [1] * (len(cards) - 1))
        self.assertEqual(len([call for call in fake.calls if call[:2] == ["issue", "list"]]), 1)
        self.assertEqual(
            next(call for call in fake.calls if call[:2] == ["issue", "list"]),
            ["issue", "list", "--repo", SLUG, "--state", "all", "--limit", "1000", "--json", "number,title,body,state"],
        )
        by_marker = {}
        for issue in fake.issues:
            markers = MARKER_RE.findall(issue["body"])
            self.assertEqual(len(markers), 1)
            self.assertEqual(issue["body"].splitlines()[0], f"<!-- phrase-card:{markers[0]} -->")
            by_marker[markers[0]] = issue

        self.assertEqual(set(by_marker), {CARD_FILE_RE.match(path.name).group(1) for path in cards})
        for path in cards:
            number = CARD_FILE_RE.match(path.name).group(1)
            text = read_card(path)
            issue = by_marker[number]
            self.assertEqual(issue["title"], f"({number}) {heading_of(text)}")
            self.assertEqual(issue["body"], expected_body(path, number))
            self.assertNotIn(text.partition("\n")[0].rstrip("\r"), issue["body"].splitlines())
            self.assertNotIn("C:\\Users", issue["body"])
            self.assertNotIn("C:/Users", issue["body"])
            if ".mob\\" not in text:
                self.assertNotIn(".mob\\", issue["body"])
            if ".mob/" in text.partition("\n")[2]:
                self.assertIn(".mob/", issue["body"])
            reason = close_reason_of(text)
            labels = ["card", "tooling"] if tooling_of(text) else ["card"]
            self.assertEqual(issue["labels"], labels)
            if reason is None:
                self.assertEqual(issue["state"], "OPEN")
                self.assertIsNone(issue["close_reason"])
            else:
                self.assertEqual(issue["state"], "CLOSED")
                self.assertEqual(issue["close_reason"], reason)

        create_calls = [call for call in fake.calls if call[:2] == ["issue", "create"]]
        self.assertEqual(len(create_calls), len(cards))
        for call in create_calls:
            self.assertEqual(call[2:4], ["--repo", SLUG])
            self.assertIn("--body-file", call)
            self.assertNotIn("--body", call)
            self.assertEqual(call[call.index("--label") + 1], "card")
            body_path = Path(call[call.index("--body-file") + 1])
            self.assertFalse(body_path.exists())
            self.assertTrue(str(body_path.resolve()).startswith(str(Path(tempfile.gettempdir()).resolve())))
        label_creates = [call for call in fake.calls if call[:2] == ["label", "create"]]
        self.assertEqual([call[2] for call in label_creates], ["card", "tooling"])
        for call in label_creates:
            self.assertEqual(call[3:5], ["--repo", SLUG])
            self.assertEqual(call[5], "--color")
            self.assertRegex(call[6], r"^[0-9A-Fa-f]{6}$")
            self.assertEqual(call[7], "--description")
            self.assertTrue(call[8].strip())
        self.assertIn("card", fake.labels)
        self.assertIn("tooling", fake.labels)

    def test_known_cards_get_the_right_labels_and_close_reasons(self):
        fake = FakeGH()
        self.run_main(["--apply"], fake)
        by_marker = {MARKER_RE.search(issue["body"]).group(1): issue for issue in fake.issues}

        done = by_marker["001"]
        self.assertEqual(done["title"], "(001) Build the resolver")
        self.assertEqual(done["labels"], ["card"])
        self.assertEqual(done["state"], "CLOSED")
        self.assertEqual(done["close_reason"], "completed")

        ready = by_marker["002"]
        self.assertEqual(ready["state"], "OPEN")
        self.assertEqual(ready["labels"], ["card"])

        superseded = by_marker["063"]
        self.assertEqual(superseded["state"], "CLOSED")
        self.assertEqual(superseded["close_reason"], "not planned")
        self.assertEqual(superseded["labels"], ["card", "tooling"])

        withdrawn = by_marker["071"]
        self.assertEqual(withdrawn["state"], "CLOSED")
        self.assertEqual(withdrawn["close_reason"], "not planned")
        self.assertEqual(withdrawn["labels"], ["card", "tooling"])

        tooling_done = by_marker["073"]
        self.assertEqual(tooling_done["labels"], ["card", "tooling"])
        self.assertEqual(tooling_done["close_reason"], "completed")

        dashboard = by_marker["036"]
        self.assertIn("<phrase-lang>", dashboard["body"])
        self.assertIn("<mob-defaults>", dashboard["body"])
        self.assertNotIn("mob-crosscli", dashboard["body"])
        self.assertNotIn("C:\\Users", dashboard["body"])

    def test_second_apply_creates_nothing_and_following_dry_run_says_nothing_to_do(self):
        fake = FakeGH()
        self.run_main(["--apply"], fake)
        creates = [call for call in fake.calls if call[:2] == ["issue", "create"]]
        closes = [call for call in fake.calls if call[:2] == ["issue", "close"]]
        label_creates = [call for call in fake.calls if call[:2] == ["label", "create"]]
        issue_count = len(fake.issues)

        code, applied = self.run_main(["--apply"], fake)
        dry_code, dry = self.run_main(["--dry-run"], fake)

        self.assertEqual(code, 0)
        self.assertEqual(dry_code, 0)
        self.assertEqual(len(fake.issues), issue_count)
        self.assertEqual([call for call in fake.calls if call[:2] == ["issue", "create"]], creates)
        self.assertEqual([call for call in fake.calls if call[:2] == ["issue", "close"]], closes)
        self.assertEqual([call for call in fake.calls if call[:2] == ["label", "create"]], label_creates)
        self.assertIn("Nothing to do", applied)
        self.assertEqual(dry.strip(), "Nothing to do.")

    def test_no_delete_and_issues_without_the_marker_are_not_edited(self):
        fake = FakeGH()
        unmarked = {
            "number": 5000,
            "title": "A human issue",
            "body": "no phrase-card marker here",
            "state": "OPEN",
            "labels": [],
            "close_reason": None,
        }
        marked = {
            "number": 7,
            "title": "(001) Build the resolver",
            "body": "<!-- phrase-card:001 -->\npre-existing body",
            "state": "OPEN",
            "labels": ["card"],
            "close_reason": None,
        }
        fake.issues.extend([unmarked, marked])

        self.run_main(["--apply"], fake)

        self.assertFalse(any("delete" in str(arg).lower() for call in fake.calls for arg in call))
        self.assertFalse(any(call[:2] == ["issue", "edit"] for call in fake.calls))
        self.assertEqual(unmarked["body"], "no phrase-card marker here")
        self.assertEqual(unmarked["state"], "OPEN")
        self.assertEqual(unmarked["title"], "A human issue")
        self.assertEqual(marked["body"], "<!-- phrase-card:001 -->\npre-existing body")
        self.assertEqual(marked["state"], "CLOSED")
        self.assertEqual(marked["close_reason"], "completed")
        markers = [MARKER_RE.findall(issue["body"]) for issue in fake.issues]
        flat = [number for group in markers for number in group]
        self.assertEqual(flat.count("001"), 1)
        for call in fake.calls:
            if call[:2] != ["issue", "close"]:
                continue
            number = int(call[2])
            issue = next(item for item in fake.issues if item["number"] == number)
            self.assertIn("<!-- phrase-card:", issue["body"])

    def test_wrong_owner_or_repo_is_refused(self):
        fake = FakeGH()
        with self.assertRaises(ValueError):
            migrate_cards.main(["--dry-run", "--owner", "some-other-owner"], gh_runner=fake, sleep=lambda _seconds: None)
        with self.assertRaises(ValueError):
            migrate_cards.main(["--apply", "--repo", "other-repo"], gh_runner=fake, sleep=lambda _seconds: None)
        with self.assertRaises(ValueError):
            migrate_cards.main(["--repo", "Programming-Made-EZ/phrase-lang"], gh_runner=fake, sleep=lambda _seconds: None)
        with self.assertRaises(ValueError):
            migrate_cards.migrate(fake, owner="some-other-owner", repo="phrase-lang")
        with self.assertRaises(ValueError):
            migrate_cards.migrate(fake, owner="Programming-Made-EZ", repo="other-repo")
        self.assertEqual(fake.calls, [])
        self.assertEqual(fake.issues, [])

    def test_milestone_and_non_card_files_are_ignored(self):
        self.assertTrue(any(path.name.startswith("MILESTONE") for path in BOARD.iterdir()))
        fake = FakeGH()
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            (board / "001-done.md").write_text(
                "\n".join([
                    "# Done fixture",
                    "",
                    "- **Status:** Done 2026-10-26",
                    "- **Type:** docs",
                    "",
                    "Windows `C:\\Users\\ringe\\projects\\phrase-lang\\board` and `C:/Users/ringe/projects/phrase-lang/docs`.",
                    "Defaults `C:\\Users\\ringe\\mob-crosscli` and `C:/Users/ringe/mob-crosscli/x`.",
                    "The card itself wrote `.mob\\loop\\state.json` and `.mob/loop/state.json`.",
                    "",
                ]),
                encoding="utf-8",
            )
            (board / "002-ready.md").write_text(
                "# Ready fixture\n\n- **Status:** Ready\n- **Type:** bug\n",
                encoding="utf-8",
            )
            (board / "003-superseded.md").write_text(
                "# Superseded fixture\n\n- **Status:** SUPERSEDED 2026-10-26 by 076\n- **Type:** tooling (thin slice)\n",
                encoding="utf-8",
            )
            (board / "004-withdrawn.md").write_text(
                "# Withdrawn fixture\n\n- **Status:** WITHDRAWN 2026-10-26\n- **Type:** tooling, still tooling\n",
                encoding="utf-8",
            )
            (board / "005-future.md").write_text(
                "# Future fixture\n\n- **Status:** Ready, FUTURE\n- **Type:** feature\n",
                encoding="utf-8",
            )
            (board / "006-lf.md").write_bytes(b"# LF fixture\n\n- **Status:** Ready\n- **Type:** bug\nSee C:/Users/ringe/mob-crosscli/y.\n")
            (board / "MILESTONE-usable-v1.md").write_text("# Milestone: Usable v1\n\n- **Status:** Done\n", encoding="utf-8")
            (board / "MILESTONES.md").write_text("# Milestones\n", encoding="utf-8")
            (board / "12-short.md").write_text("# Short\n\n- **Status:** Done\n", encoding="utf-8")
            (board / "1234-long.md").write_text("# Long\n\n- **Status:** Done\n", encoding="utf-8")
            (board / "usable-v1-ratchet.json").write_text("{}", encoding="utf-8")

            code, output = self.run_main(["--dry-run"], fake, board_dir=board)
            self.assertEqual(code, 0)
            self.assertEqual(
                output.strip().splitlines(),
                [
                    "close (001) Done fixture",
                    "create (002) Ready fixture",
                    "close (003) Superseded fixture",
                    "close (004) Withdrawn fixture",
                    "create (005) Future fixture",
                    "create (006) LF fixture",
                    "3 create, 3 close, 0 close-existing, 0 skip",
                ],
            )
            self.assertNotIn("Milestone", output)
            self.assertNotIn("Short", output)
            self.assertNotIn("Long", output)

            apply_code, _applied = self.run_main(["--apply"], fake, board_dir=board)

        self.assertEqual(apply_code, 0)
        self.assertEqual(len(fake.issues), 6)
        titles = [issue["title"] for issue in fake.issues]
        self.assertNotIn("( ) Milestone: Usable v1", titles)
        self.assertFalse(any("Milestone" in title or "Short" in title or "Long" in title for title in titles))
        by_marker = {MARKER_RE.search(issue["body"]).group(1): issue for issue in fake.issues}
        done = by_marker["001"]
        self.assertEqual(done["close_reason"], "completed")
        self.assertEqual(done["labels"], ["card"])
        self.assertIn("<phrase-lang>", done["body"])
        self.assertIn("<mob-defaults>", done["body"])
        self.assertNotIn("C:\\Users", done["body"])
        self.assertNotIn("C:/Users", done["body"])
        self.assertIn(".mob\\loop\\state.json", done["body"])
        self.assertIn(".mob/loop/state.json", done["body"])
        self.assertNotIn("# Done fixture", done["body"].splitlines())
        self.assertEqual(by_marker["002"]["state"], "OPEN")
        self.assertEqual(by_marker["002"]["labels"], ["card"])
        self.assertEqual(by_marker["003"]["close_reason"], "not planned")
        self.assertEqual(by_marker["003"]["labels"], ["card", "tooling"])
        self.assertEqual(by_marker["004"]["close_reason"], "not planned")
        self.assertEqual(by_marker["005"]["state"], "OPEN")
        self.assertIn("<mob-defaults>", by_marker["006"]["body"])
        self.assertNotIn("C:/Users", by_marker["006"]["body"])

    def test_rate_limit_is_retried_once(self):
        fake = FakeGH()
        fake.rate_limit_creates = 1
        sleeps = []
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            (board / "001-one.md").write_text("# One\n\n- **Status:** Ready\n", encoding="utf-8")
            (board / "002-two.md").write_text("# Two\n\n- **Status:** Ready\n", encoding="utf-8")
            code, _output = self.run_main(["--apply"], fake, sleep=lambda seconds: sleeps.append(seconds), board_dir=board)
        self.assertEqual(code, 0)
        self.assertEqual(len(fake.issues), 2)
        create_calls = [call for call in fake.calls if call[:2] == ["issue", "create"]]
        self.assertEqual(len(create_calls), 3)
        self.assertEqual(sleeps, [1, 1])

        refused = FakeGH()
        refused.rate_limit_creates = 2
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            (board / "001-one.md").write_text("# One\n\n- **Status:** Ready\n", encoding="utf-8")
            code, output = self.run_main(["--apply"], refused, board_dir=board)
        self.assertEqual(code, 1)
        self.assertIn("ERROR", output)
        self.assertEqual(len(refused.issues), 0)
        self.assertEqual(len([call for call in refused.calls if call[:2] == ["issue", "create"]]), 2)

        other = FakeGH()
        other.fail_creates = 1
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            (board / "001-one.md").write_text("# One\n\n- **Status:** Ready\n", encoding="utf-8")
            code, _output = self.run_main(["--apply"], other, board_dir=board)
        self.assertEqual(code, 1)
        self.assertEqual(len([call for call in other.calls if call[:2] == ["issue", "create"]]), 1)

    def test_check_real_board_issues_counts_one_marker_per_card(self):
        fake = FakeGH()
        self.run_main(["--apply"], fake)

        buffer = io.StringIO()
        with redirect_stdout(buffer):
            code = check_real_board.main(["--issues"], gh_runner=fake)
        self.assertEqual(code, 0)
        self.assertIn("OK", buffer.getvalue())
        listed = [call for call in fake.calls if call[:2] == ["issue", "list"]]
        self.assertEqual(
            listed[-1],
            ["issue", "list", "--repo", SLUG, "--state", "all", "--limit", "1000", "--json", "number,title,body,state"],
        )

        before = len(fake.calls)
        self.assertEqual(check_real_board.verify_issues(fake), len(card_paths(BOARD)))
        fresh = fake.calls[before:]
        self.assertTrue(all(call[:2] == ["issue", "list"] for call in fresh))
        self.assertFalse(any("delete" in str(arg).lower() for call in fresh for arg in call))
        self.assertFalse(any(call[1] in {"create", "close", "edit"} for call in fresh))

        removed = fake.issues.pop()
        with self.assertRaises(RuntimeError):
            check_real_board.verify_issues(fake)
        fake.issues.append(removed)

        clone = dict(fake.issues[0])
        clone["number"] = 9000
        fake.issues.append(clone)
        with self.assertRaises(RuntimeError):
            check_real_board.verify_issues(fake)
        fake.issues.pop()

        empty = FakeGH()
        fail_buffer = io.StringIO()
        with redirect_stdout(fail_buffer):
            fail_code = check_real_board.main(["--issues"], gh_runner=empty)
        self.assertEqual(fail_code, 1)
        self.assertIn("FAIL", fail_buffer.getvalue())
        self.assertTrue(all(call[:2] == ["issue", "list"] for call in empty.calls))
        self.assertFalse(any("delete" in str(arg).lower() for call in empty.calls for arg in call))

    def test_open_finished_issue_is_closed_once_and_other_existing_issues_stay(self):
        # Given a Done card whose issue was created and then left OPEN, a Ready card
        # whose issue is OPEN, a Done card whose issue is already CLOSED, and a
        # SUPERSEDED card whose issue is still OPEN
        fake = FakeGH()
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            (board / "001-done.md").write_text(
                "# Done fixture\n\n- **Status:** Done 2026-10-26\n- **Type:** docs\n",
                encoding="utf-8",
            )
            (board / "002-ready.md").write_text(
                "# Ready fixture\n\n- **Status:** Ready\n- **Type:** bug\n",
                encoding="utf-8",
            )
            (board / "003-closed.md").write_text(
                "# Closed fixture\n\n- **Status:** Done 2026-10-26\n- **Type:** docs\n",
                encoding="utf-8",
            )
            (board / "004-superseded.md").write_text(
                "# Superseded fixture\n\n- **Status:** SUPERSEDED 2026-10-26 by 076\n- **Type:** tooling\n",
                encoding="utf-8",
            )
            fake.issues.extend([
                {
                    "number": 11,
                    "title": "(001) Done fixture",
                    "body": "<!-- phrase-card:001 -->\nalready created",
                    "state": "OPEN",
                    "labels": ["card"],
                    "close_reason": None,
                },
                {
                    "number": 12,
                    "title": "(002) Ready fixture",
                    "body": "<!-- phrase-card:002 -->\nready body",
                    "state": "OPEN",
                    "labels": ["card"],
                    "close_reason": None,
                },
                {
                    "number": 13,
                    "title": "(003) Closed fixture",
                    "body": "<!-- phrase-card:003 -->\nclosed body",
                    "state": "CLOSED",
                    "labels": ["card"],
                    "close_reason": "not planned",
                },
                {
                    "number": 14,
                    "title": "(004) Superseded fixture",
                    "body": "<!-- phrase-card:004 -->\nopen superseded",
                    "state": "open",
                    "labels": ["card", "tooling"],
                    "close_reason": None,
                },
            ])

            # When a dry run plans the board and apply runs twice
            dry_code, dry_out = self.run_main(["--dry-run"], fake, board_dir=board)
            code, _output = self.run_main(["--apply"], fake, board_dir=board)
            close_calls = [call for call in fake.calls if call[:2] == ["issue", "close"]]
            again, again_out = self.run_main(["--apply"], fake, board_dir=board)

        # Then the open Done and SUPERSEDED issues are closed once, by number, and a second apply does nothing
        self.assertEqual(dry_code, 0)
        self.assertEqual(
            dry_out.strip().splitlines(),
            [
                "close-existing (001) Done fixture",
                "skip (002) Ready fixture",
                "skip (003) Closed fixture",
                "close-existing (004) Superseded fixture",
                "0 create, 0 close, 2 close-existing, 2 skip",
            ],
        )
        self.assertEqual(code, 0)
        self.assertEqual(again, 0)
        by_number = {issue["number"]: issue for issue in fake.issues}
        self.assertEqual(len(fake.issues), 4)
        self.assertEqual(by_number[11]["state"], "CLOSED")
        self.assertEqual(by_number[11]["close_reason"], "completed")
        self.assertEqual(by_number[11]["body"], "<!-- phrase-card:001 -->\nalready created")
        self.assertEqual(by_number[12]["state"], "OPEN")
        self.assertEqual(by_number[12]["body"], "<!-- phrase-card:002 -->\nready body")
        self.assertIsNone(by_number[12]["close_reason"])
        self.assertEqual(by_number[13]["state"], "CLOSED")
        self.assertEqual(by_number[13]["close_reason"], "not planned")
        self.assertEqual(by_number[13]["body"], "<!-- phrase-card:003 -->\nclosed body")
        self.assertEqual(by_number[14]["state"], "CLOSED")
        self.assertEqual(by_number[14]["close_reason"], "not planned")
        self.assertEqual(by_number[14]["body"], "<!-- phrase-card:004 -->\nopen superseded")
        self.assertEqual(
            close_calls,
            [
                ["issue", "close", "11", "--repo", SLUG, "--reason", "completed"],
                ["issue", "close", "14", "--repo", SLUG, "--reason", "not planned"],
            ],
        )
        self.assertFalse(any(call[:2] == ["issue", "create"] for call in fake.calls))
        self.assertFalse(any(call[:2] == ["issue", "edit"] for call in fake.calls))
        self.assertIn("Nothing to do", again_out)
        self.assertEqual([call for call in fake.calls if call[:2] == ["issue", "close"]], close_calls)

    def test_title_containing_delete_is_created_and_delete_commands_are_refused(self):
        # Given a card whose title and body merely contain the word delete
        fake = FakeGH()
        with tempfile.TemporaryDirectory() as tmp:
            board = Path(tmp)
            (board / "001-delete-guard.md").write_text(
                "\n".join([
                    "# Delete guard: refuse unsafe deletes",
                    "",
                    "- **Status:** Ready",
                    "- **Type:** tooling",
                    "",
                    "The body may mention delete and delete_guard.py.",
                    "",
                ]),
                encoding="utf-8",
            )
            # When the dry run and the apply both build the create command
            dry_code, dry_out = self.run_main(["--dry-run"], fake, board_dir=board)
            code, output = self.run_main(["--apply"], fake, board_dir=board)

        # Then the issue is created, and the word inside the title is not treated as a delete command
        self.assertEqual(dry_code, 0)
        self.assertEqual(
            dry_out.strip().splitlines(),
            [
                "create (001) Delete guard: refuse unsafe deletes",
                "1 create, 0 close, 0 close-existing, 0 skip",
            ],
        )
        self.assertEqual(code, 0)
        self.assertNotIn("ERROR", output)
        self.assertEqual(len(fake.issues), 1)
        issue = fake.issues[0]
        self.assertEqual(issue["title"], "(001) Delete guard: refuse unsafe deletes")
        self.assertEqual(issue["state"], "OPEN")
        self.assertEqual(issue["labels"], ["card", "tooling"])
        self.assertIn("<!-- phrase-card:001 -->", issue["body"])
        self.assertIn("delete_guard.py", issue["body"])
        for call in fake.calls:
            for arg in call:
                self.assertNotEqual(str(arg).lower(), "delete")

        def refuse(args, check=True):
            raise AssertionError(args)

        for args in (
            ["delete"],
            ["DELETE"],
            ["issue", "delete", "4"],
            ["Issue", "Delete", "4"],
            ["repo", "delete"],
        ):
            with self.assertRaises(RuntimeError):
                migrate_cards.invoke(refuse, args, lambda _seconds: None)

        seen = []

        def allow(args, check=True):
            seen.append([str(part) for part in args])
            return ""

        migrate_cards.invoke(
            allow,
            ["issue", "create", "--title", "Delete guard: refuse unsafe deletes"],
            lambda _seconds: None,
        )
        migrate_cards.invoke(allow, ["issue", "create", "--title", "delete"], lambda _seconds: None)
        migrate_cards.invoke(allow, ["label", "create", "deleted"], lambda _seconds: None)
        self.assertEqual(len(seen), 3)

    def test_dry_run_rejects_a_bad_card_before_apply_starts(self):
        # Given a card whose body never received the phrase-card marker
        fake = FakeGH()
        bad = migrate_cards.Card("008", "Bad card", "body without the marker", "completed", False)
        original = migrate_cards.load_cards
        migrate_cards.load_cards = lambda _board: [bad]
        try:
            # When the dry run builds the create and close commands
            dry_code, dry_out = self.run_main(["--dry-run"], fake)
            apply_code, apply_out = self.run_main(["--apply"], fake)
        finally:
            migrate_cards.load_cards = original

        # Then both runs report the card and neither creates or closes an issue
        self.assertEqual(dry_code, 1)
        self.assertIn("ERROR", dry_out)
        self.assertIn("phrase-card:008", dry_out)
        self.assertEqual(apply_code, 1)
        self.assertIn("ERROR", apply_out)
        self.assertEqual(fake.issues, [])
        self.assertFalse(any(call[:2] == ["issue", "create"] for call in fake.calls))
        self.assertFalse(any(call[:2] == ["issue", "close"] for call in fake.calls))
        self.assertFalse(any(call[:2] == ["label", "create"] for call in fake.calls))

    def test_close_existing_requires_the_marker_on_the_issue_body(self):
        card = migrate_cards.Card("001", "Done fixture", "<!-- phrase-card:001 -->\ncard", "completed", False)
        action = migrate_cards.Action(
            "close-existing",
            card,
            "close-existing (001) Done fixture",
            issue_number=11,
            issue_body="no marker in the issue",
        )
        with self.assertRaises(RuntimeError) as caught:
            migrate_cards.validate_actions(SLUG, [action])
        self.assertIn("phrase-card:001", str(caught.exception))
        fake = FakeGH()
        with self.assertRaises(RuntimeError):
            migrate_cards.close_existing_issue(fake, SLUG, action, lambda _seconds: None)
        self.assertEqual(fake.calls, [])


if __name__ == "__main__":
    unittest.main()
