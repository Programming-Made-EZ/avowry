"""Card 234, criterion 1 (D157): the process wiki is split into indexed pages with
one-line summaries, every rule still resolves to its decision anchor, and the card
states its metrics as numbers with a measurement method (adopted condition of
window 234-taskforce, D145; slice 0441cfd amended 5674a66).

The seam these tests pin, for the green to implement
(scripts/check-process-wiki.py; stdlib only; reads files, runs nothing):

  python scripts/check-process-wiki.py [--root DIR]     DIR defaults to the cwd

  exit 0  clean: prints "process-wiki: OK (<n> pages)" on stdout
  exit 1  findings: one stderr line per finding, "<path relative to root>: <keyword> <detail>"
  exit 2  DIR is not a directory: stderr says "not a directory"
  never a traceback.

Wiki shape (slice, "Wiki location, page shape, and index"):
  docs/process-wiki/INDEX.md      first line is its own one-line summary; then a table
                                  | id | path | status | summary |   (header row starts with "id",
                                  a separator row of dashes, then one row per page)
  id                              core/index, rules/<slug>, role/<role>-<step>, howto/<topic>
                                  (<slug>, <role>, <step>, <topic>: lowercase letters, digits, hyphens)
  status                          enabled or disabled
  summary                         equals the first line of the page; 1 to 200 characters
  page                            first line is the summary; every line starting "- Rule:" cites
                                  its decision as [Dnnn](<relative path>/<area>.md#dnnn), where
                                  the text is the D-number, the fragment is the lowercase number,
                                  the file exists under docs/decisions and holds <a id="dnnn">
  card record                     board/234-*.md holds the baselines 50K, 125K and 965K (whole
                                  tokens, so 150K is not 50K); .mob/records/234/record-in-progress.json
                                  holds "measurement_methods" with non-empty strings under
                                  tokens_per_seat, facilitator_context_size and rule_misses

Finding keywords: no index, no enabled page, malformed row, bad id, unknown status,
duplicate id, duplicate path, missing page, unlisted page, missing summary, summary too long,
summary mismatch, not valid UTF-8, missing decision token, missing anchor link, broken anchor,
mismatched anchor, no card, no record, record not valid JSON, missing number <N>,
missing method <key>.

Fixture trees are made with tempfile.mkdtemp and left in place (no cleanup, by the safety rule).
"""

import json
import os
import re
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
SCRIPT = Path(os.environ.get("PROCESS_WIKI_SCRIPT", str(REPO / "scripts" / "check-process-wiki.py")))
# Real-repo tests read this tree; a scratch copy can be named to show a guard red.
REAL = Path(os.environ.get("PROCESS_WIKI_REPO_ROOT", str(REPO)))

CARD = "board/234-an-indexed-process-wiki-with-just-in-time-reading.md"
RECORD = ".mob/records/234/record-in-progress.json"
INDEX = "docs/process-wiki/INDEX.md"
DECISIONS = "docs/decisions/process-and-staffing.md"
METHOD_KEYS = ("tokens_per_seat", "facilitator_context_size", "rule_misses")
TEAM_WORKING_AGREEMENT = "TEAM_WORKING_AGREEMENT.md"
PROPOSED_FLOOR_SHARE = 0.10  # to confirm in red body if slice/architect did not set one
TASK_FORCE_ROLE_IDS = (
    "role/product-manager-ready",
    "role/architect-slice",
    "role/technical-writer",
    "role/qa-red",
    "role/backend-developer-green",
    "role/independent-reviewer-audit",
    "role/delivery-lead-ship",
    "role/product-manager-signoff",
)
CARD_TEXT = (
    "# card 234\n"
    "tokens per seat against about 50K at setup and about 125K median at the end;\n"
    "facilitator context size against about 965K; rule misses from an unread page.\n"
)
INDEX_SUMMARY = "Index of the process wiki: one line per page, each page read just in time."


def run(*args, cwd=None):
    proc = subprocess.run(
        [sys.executable, str(SCRIPT), *args],
        cwd=str(cwd) if cwd else None,
        capture_output=True, text=True, encoding="utf-8", errors="replace",
    )
    return proc.returncode, proc.stdout, proc.stderr


def findings(stderr):
    return [line for line in stderr.splitlines() if line.strip()]


class Page:
    def __init__(self, pid, path, summary, rules=(), status="enabled", body=None, write=True):
        self.pid, self.path, self.summary = pid, path, summary
        self.rules, self.status, self.body, self.write = list(rules), status, body, write


class Tree:
    """A fixture repository root with a wiki, decision anchors, the card and its record."""

    def __init__(self):
        self.anchors = [157, 165, 166, 167, 170]
        self.card = CARD_TEXT
        self.methods = {k: "measured from the committed provider usage fields" for k in METHOD_KEYS}
        self.record_text = None
        self.team_working_agreement = "\n".join(f"- **Rule {n}** line {n}" for n in range(1, 11)) + "\n"
        self.extra_files = {}
        self.write_index = True
        self.index_override = None
        self.extra_rows = []
        self.reverse_rows = False
        self.pages = [
            Page("rules/one-criterion", "docs/process-wiki/rules/one-criterion.md",
                 "One acceptance criterion per red-green cycle, checked from git.",
                 [self.rule("Each red commit names exactly one criterion.", 157)]),
            Page("role/product-manager-ready", "docs/process-wiki/roles/product-manager-ready.md",
                 "What the product-manager-ready seat reads and writes before launch.",
                 [self.rule("The ready seat states owner priority and constraints.", 170)]),
            Page("role/architect-slice", "docs/process-wiki/roles/architect-slice.md",
                 "What the architect-slice seat reads and writes for the slice.",
                 [self.rule("The slice names hostile inputs for list-like fields.", 167)]),
            Page("role/technical-writer", "docs/process-wiki/roles/technical-writer.md",
                 "What the technical-writer seat reads and writes for wording updates.",
                 [self.rule("Writing updates keep decision anchors and exact ids.", 157)]),
            Page("role/qa-red", "docs/process-wiki/roles/qa-red.md",
                 "What the QA seat reads and writes at the red step.",
                 [self.rule("The red seat writes the intent line first.", 165),
                  self.rule("The red body answers the hostile-input list.", 167)]),
            Page("role/backend-developer-green", "docs/process-wiki/roles/backend-developer-green.md",
                 "What the backend-developer-green seat reads and writes for green.",
                 [self.rule("Green changes must satisfy each failing red test.", 157)]),
            Page("role/independent-reviewer-audit", "docs/process-wiki/roles/independent-reviewer-audit.md",
                 "What the independent-reviewer-audit seat reads and writes in review.",
                 [self.rule("Review confirms the change matches the cited criterion.", 166)]),
            Page("role/delivery-lead-ship", "docs/process-wiki/roles/delivery-lead-ship.md",
                 "What the delivery-lead-ship seat reads and writes before shipping.",
                 [self.rule("Shipping seat confirms push and record updates.", 170)]),
            Page("role/product-manager-signoff", "docs/process-wiki/roles/product-manager-signoff.md",
                 "What the product-manager-signoff seat reads and writes for signoff.",
                 [self.rule("Signoff confirms the done check and remaining risk.", 170)]),
            Page("howto/hostile-inputs", "docs/process-wiki/howto/hostile-inputs.md",
                 "How to answer the disabled, unknown, repeated and reordered entries.", []),
        ]
        self.briefs = {
            role_id.split("/", 1)[1]: f"Pages: core/index, {role_id}\n"
            for role_id in TASK_FORCE_ROLE_IDS
        }

    @staticmethod
    def rule(text, *numbers, depth=2):
        links = " ".join(
            f"[D{n}]({'../' * depth}decisions/process-and-staffing.md#d{n})" for n in numbers)
        return f"- Rule: {text} See {links}."

    def page(self, pid):
        return next(p for p in self.pages if p.pid == pid)

    def index_text(self):
        rows = [Page("core/index", INDEX, INDEX_SUMMARY)] + self.pages
        lines = [f"| {p.pid} | {p.path} | {p.status} | {p.summary} |" for p in rows]
        lines += self.extra_rows
        if self.reverse_rows:
            lines.reverse()
        return (INDEX_SUMMARY + "\n\n| id | path | status | summary |\n|---|---|---|---|\n"
                + "\n".join(lines) + "\n")

    def build(self):
        root = Path(tempfile.mkdtemp(prefix="pw234-"))

        def put(rel, text, raw=None):
            target = root / rel
            target.parent.mkdir(parents=True, exist_ok=True)
            if raw is not None:
                target.write_bytes(raw)
            else:
                target.write_bytes(text.encode("utf-8"))

        put(DECISIONS, "# Process\n\n" + "".join(f'<a id="d{n}"></a>\n#### Decider D{n}\n\n' for n in self.anchors))
        if self.write_index:
            put(INDEX, self.index_override if self.index_override is not None else self.index_text())
        for p in self.pages:
            if not p.write:
                continue
            if isinstance(p.body, bytes):
                put(p.path, "", raw=p.body)
            else:
                body = p.body if p.body is not None else p.summary + "\n\n" + "\n".join(p.rules) + "\n"
                put(p.path, body)
        if self.card is not None:
            put(CARD, self.card)
        if self.team_working_agreement is not None:
            put(TEAM_WORKING_AGREEMENT, self.team_working_agreement)
        if self.record_text is not None:
            put(RECORD, self.record_text)
        elif self.methods is not None:
            put(RECORD, json.dumps({"card": 234, "measurement_methods": self.methods}, indent=1) + "\n")
        for stem, pages in self.briefs.items():
            put(f"docs/process-wiki/briefs/{stem}.md", f"Brief for {stem}.\n\n{pages}")
        for rel, text in self.extra_files.items():
            put(rel, text)
        return root


def check(tree, *extra):
    root = tree.build()
    code, out, err = run("--root", str(root), *extra)
    return root, code, out, err


class ZeroCases(unittest.TestCase):
    def test_a_tree_with_no_wiki_index_fails_with_no_index(self):
        tree = Tree()
        tree.write_index = False
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/INDEX\.md: no index")

    def test_an_index_with_no_rows_has_no_enabled_page(self):
        tree = Tree()
        tree.pages = []
        tree.index_override = INDEX_SUMMARY + "\n\n| id | path | status | summary |\n|---|---|---|---|\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("no enabled page", err)

    def test_an_index_whose_pages_are_all_disabled_has_no_enabled_page(self):
        tree = Tree()
        for p in tree.pages:
            p.status = "disabled"
        tree.index_override = INDEX_SUMMARY + "\n\n| id | path | status | summary |\n|---|---|---|---|\n" + "\n".join(
            f"| {p.pid} | {p.path} | disabled | {p.summary} |" for p in tree.pages) + "\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("no enabled page", err)

    def test_an_empty_page_file_has_no_summary(self):
        tree = Tree()
        tree.page("howto/hostile-inputs").body = ""
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/howto/hostile-inputs\.md: missing summary")

    def test_a_page_with_no_rule_lines_is_clean(self):
        tree = Tree()
        for p in tree.pages:
            p.rules = []
        tree.team_working_agreement = ""
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("process-wiki: OK", out)

    def test_a_root_that_is_not_a_directory_exits_2(self):
        missing = Path(tempfile.mkdtemp(prefix="pw234-")) / "no-such-root"
        code, _, err = run("--root", str(missing))
        self.assertEqual(code, 2, err)
        self.assertIn("not a directory", err)


class OneCases(unittest.TestCase):
    def test_one_enabled_page_the_index_itself_is_clean(self):
        tree = Tree()
        tree.pages = []
        tree.briefs = {}
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/INDEX\.md: missing role page")

    def test_one_rule_with_its_token_and_a_resolving_link_is_clean(self):
        tree = Tree()
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("process-wiki: OK", out)

    def test_one_rule_naming_two_decisions_needs_both_to_resolve(self):
        tree = Tree()
        tree.page("role/qa-red").rules = [Tree.rule("Intent and hostile inputs.", 165, 999)]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertEqual(len([l for l in findings(err) if "broken anchor" in l]), 1, err)
        self.assertIn("D999", err)


class ManyCases(unittest.TestCase):
    def test_many_pages_and_many_rules_are_clean(self):
        tree = Tree()
        for n in range(6):
            slug = f"rules/extra-{n}"
            tree.pages.append(Page(slug, f"docs/process-wiki/rules/extra-{n}.md",
                                   f"Extra rule page number {n}.",
                                   [Tree.rule("Rule A.", 157, 170), Tree.rule("Rule B.", 165, 166, 167)]))
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("process-wiki: OK", out)

    def test_one_broken_anchor_among_many_is_named_by_page_and_decision(self):
        tree = Tree()
        tree.page("role/qa-red").rules.append(Tree.rule("Cites a decision that is gone.", 404))
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        bad = [l for l in findings(err) if "broken anchor" in l]
        self.assertEqual(len(bad), 1, err)
        self.assertTrue(bad[0].startswith("docs/process-wiki/roles/qa-red.md: broken anchor"), bad[0])
        self.assertIn("D404", bad[0])

    def test_two_bad_pages_give_two_findings(self):
        tree = Tree()
        tree.page("role/qa-red").rules.append(Tree.rule("Gone.", 404))
        tree.page("rules/one-criterion").rules.append(Tree.rule("Also gone.", 405))
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertEqual(len([l for l in findings(err) if "broken anchor" in l]), 2, err)


class BoundaryCases(unittest.TestCase):
    def _with_summary(self, length):
        tree = Tree()
        summary = "s" * length
        tree.page("howto/hostile-inputs").summary = summary
        return check(tree)

    def test_a_summary_of_exactly_200_characters_is_clean(self):
        _, code, out, err = self._with_summary(200)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("process-wiki: OK", out)

    def test_a_summary_of_201_characters_is_too_long(self):
        _, code, _, err = self._with_summary(201)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/howto/hostile-inputs\.md: summary too long")

    def test_a_page_whose_first_line_is_blank_has_no_summary(self):
        tree = Tree()
        tree.page("howto/hostile-inputs").body = "\nA summary on the second line.\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("hostile-inputs.md: missing summary", err)

    def test_d17_does_not_resolve_against_the_d170_anchor(self):
        tree = Tree()
        tree.anchors = [170]
        tree.page("rules/one-criterion").rules = [Tree.rule("Prefix trap.", 17)]
        tree.page("role/qa-red").rules = []
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/rules/one-criterion\.md: broken anchor.*D17\b")

    def test_a_rule_line_with_no_decision_token_is_flagged(self):
        tree = Tree()
        tree.page("rules/one-criterion").rules = ["- Rule: A rule that cites nothing."]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/rules/one-criterion\.md: missing decision token")

    def test_a_decision_token_without_a_link_is_flagged(self):
        tree = Tree()
        tree.page("rules/one-criterion").rules = ["- Rule: One criterion per cycle (D157)."]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/rules/one-criterion\.md: missing anchor link.*D157")

    def test_a_link_whose_fragment_names_another_decision_is_mismatched(self):
        tree = Tree()
        tree.page("rules/one-criterion").rules = [
            "- Rule: One criterion per cycle [D157](../../decisions/process-and-staffing.md#d165)."]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/rules/one-criterion\.md: mismatched anchor.*D157")

    def test_a_line_that_is_not_a_rule_line_needs_no_token(self):
        tree = Tree()
        page = tree.page("howto/hostile-inputs")
        page.body = page.summary + "\n\nPlain prose with no decision token.\n"
        _, code, _, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)


class InterfaceCases(unittest.TestCase):
    def test_the_root_defaults_to_the_current_directory(self):
        root = Tree().build()
        code, out, err = run(cwd=root)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("process-wiki: OK", out)

    def test_every_finding_line_starts_with_a_path_relative_to_the_root(self):
        tree = Tree()
        tree.page("role/qa-red").rules.append(Tree.rule("Gone.", 404))
        tree.page("howto/hostile-inputs").body = ""
        root, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        lines = findings(err)
        self.assertEqual(len(lines), 2, err)
        for line in lines:
            self.assertRegex(line, r"^docs/process-wiki/[a-z/\-]+\.md: [a-z ]+", line)
            self.assertNotIn(str(root), line)

    def test_a_clean_run_writes_nothing_to_stderr(self):
        _, code, out, err = check(Tree())
        self.assertEqual((code, err), (0, ""), err)
        self.assertRegex(out, r"process-wiki: OK \(\d+ pages\)")


class ExceptionCases(unittest.TestCase):
    def test_a_row_with_the_wrong_number_of_cells_is_malformed(self):
        tree = Tree()
        tree.extra_rows = ["| howto/other | docs/process-wiki/howto/other.md | enabled |"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/INDEX\.md: malformed row")

    def test_a_page_that_is_not_valid_utf8_is_a_finding_not_a_traceback(self):
        tree = Tree()
        tree.page("howto/hostile-inputs").body = b"\xff\xfe\x00bad"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertNotIn("Traceback", err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/howto/hostile-inputs\.md: not valid UTF-8")

    def test_an_enabled_row_whose_file_is_missing_is_a_missing_page(self):
        tree = Tree()
        tree.page("howto/hostile-inputs").write = False
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/INDEX\.md: missing page.*howto/hostile-inputs")

    def test_a_status_other_than_enabled_or_disabled_is_unknown(self):
        tree = Tree()
        tree.page("howto/hostile-inputs").status = "maybe"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/INDEX\.md: unknown status.*maybe")

    def test_an_id_outside_the_four_kinds_is_a_bad_id(self):
        tree = Tree()
        tree.page("howto/hostile-inputs").pid = "misc/Hostile Inputs"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/INDEX\.md: bad id.*misc/Hostile Inputs")

    def test_a_link_to_a_decision_file_that_does_not_exist_is_broken(self):
        tree = Tree()
        tree.page("rules/one-criterion").rules = [
            "- Rule: One criterion [D157](../../decisions/no-such-area.md#d157)."]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/rules/one-criterion\.md: broken anchor.*D157")

    def test_an_index_summary_that_differs_from_the_page_first_line_is_a_mismatch(self):
        tree = Tree()
        page = tree.page("howto/hostile-inputs")
        page.body = "A different first line than the index row says.\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/howto/hostile-inputs\.md: summary mismatch")


class HostileInputs(unittest.TestCase):
    """D167, criterion 1 Inputs, entry by entry (slice amendment)."""

    # disabled
    def test_a_disabled_row_whose_file_is_missing_is_not_required(self):
        tree = Tree()
        page = tree.page("howto/hostile-inputs")
        page.status, page.write = "disabled", False
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("process-wiki: OK", out)  # disabled pages are not counted

    # unknown
    def test_a_page_file_missing_from_the_index_is_an_unlisted_page(self):
        tree = Tree()
        root = tree.build()
        stray = root / "docs/process-wiki/rules/stray.md"
        stray.write_bytes(b"A page nobody listed.\n")
        code, _, err = run("--root", str(root))
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/rules/stray\.md: unlisted page")

    # repeated
    def test_a_duplicate_page_id_is_rejected_and_named(self):
        tree = Tree()
        tree.extra_rows = ["| rules/one-criterion | docs/process-wiki/rules/one-criterion.md | enabled | "
                           "One acceptance criterion per red-green cycle, checked from git. |"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/INDEX\.md: duplicate id.*rules/one-criterion")

    def test_the_same_path_under_two_ids_is_rejected(self):
        tree = Tree()
        tree.extra_rows = ["| rules/second-name | docs/process-wiki/rules/one-criterion.md | enabled | "
                           "One acceptance criterion per red-green cycle, checked from git. |"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/INDEX\.md: duplicate path.*one-criterion\.md")

    def test_the_same_decision_cited_twice_on_one_page_is_clean(self):
        tree = Tree()
        tree.page("rules/one-criterion").rules = [
            Tree.rule("First.", 157), Tree.rule("Second.", 157), Tree.rule("Third.", 157, 157)]
        _, code, _, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)

    # reordered
    def test_reversing_the_index_rows_keeps_a_clean_wiki_clean(self):
        tree = Tree()
        tree.reverse_rows = True
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("process-wiki: OK", out)

    def test_reversing_the_index_rows_does_not_change_the_findings(self):
        def run_with(reverse):
            tree = Tree()
            tree.reverse_rows = reverse
            tree.page("role/qa-red").rules.append(Tree.rule("Gone.", 404))
            tree.page("howto/hostile-inputs").body = ""
            _, code, _, err = check(tree)
            return code, sorted(findings(err))

        forward, backward = run_with(False), run_with(True)
        self.assertEqual(forward[0], 1)
        self.assertGreaterEqual(len(forward[1]), 2, forward)
        self.assertEqual(forward, backward)


class CardNumbersAndMethods(unittest.TestCase):
    """Adopted condition of window 234-taskforce: numbers and methods, git-checkable."""

    def test_each_baseline_number_missing_from_the_card_is_named(self):
        for number in ("50K", "125K", "965K"):
            with self.subTest(number=number):
                tree = Tree()
                tree.card = CARD_TEXT.replace(number, "NN")
                _, code, _, err = check(tree)
                self.assertEqual(code, 1, err)
                self.assertRegex(err, rf"(?m)^{re.escape(CARD)}: missing number {number}\b")

    def test_150k_is_not_the_50k_baseline(self):
        tree = Tree()
        tree.card = CARD_TEXT.replace("about 50K", "about 150K")
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing number 50K", err)

    def test_each_measurement_method_missing_from_the_record_is_named(self):
        for key in METHOD_KEYS:
            with self.subTest(key=key):
                tree = Tree()
                del tree.methods[key]
                _, code, _, err = check(tree)
                self.assertEqual(code, 1, err)
                self.assertRegex(err, rf"(?m)^{re.escape(RECORD)}: missing method {key}\b")

    def test_a_blank_method_counts_as_missing(self):
        tree = Tree()
        tree.methods["rule_misses"] = "   "
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing method rule_misses", err)

    def test_a_record_with_no_measurement_methods_at_all_names_all_three(self):
        tree = Tree()
        tree.methods = None
        tree.record_text = json.dumps({"card": 234}) + "\n"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        for key in METHOD_KEYS:
            self.assertIn(f"missing method {key}", err)

    def test_a_missing_card_file_is_no_card(self):
        tree = Tree()
        tree.card = None
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^board/234-\*\.md: no card")

    def test_a_missing_record_file_is_no_record(self):
        tree = Tree()
        tree.methods = None
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, rf"(?m)^{re.escape(RECORD)}: no record")

    def test_a_record_that_is_not_json_is_a_finding_not_a_traceback(self):
        tree = Tree()
        tree.record_text = "{not json"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertNotIn("Traceback", err)
        self.assertRegex(err, rf"(?m)^{re.escape(RECORD)}: record not valid JSON")


class CriterionTwoSeatBriefsAndCoverage(unittest.TestCase):
    """Card 234, criterion 2 red: seat briefs, role pages, and coverage floor."""

    # ZOMBIES for criterion 2 (seat briefs + floor): Z (no briefs), O (one brief),
    # M (all task-force briefs), B (disabled/unknown/repeated/reordered Pages ids),
    # I (finding names file + detail), E (bad brief format), S (line-order invariance).
    def test_every_task_force_role_needs_a_role_page_and_a_brief(self):
        tree = Tree()
        tree.page("role/technical-writer").write = False
        del tree.briefs["technical-writer"]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertIn("missing role page role/technical-writer", err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/briefs/technical-writer\.md: missing brief")

    def test_a_brief_lists_index_plus_its_role_pages_only(self):
        tree = Tree()
        tree.extra_files["docs/process-wiki/briefs/architect-slice.md"] = (
            "Brief for the architect slice seat.\n\n"
            "Pages: core/index, role/architect-slice, rules/one-criterion\n"
        )
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(
            err,
            r"(?m)^docs/process-wiki/briefs/architect-slice\.md: brief pages must include only core/index and role/")

    def test_the_floor_fails_when_anchored_rule_coverage_is_below_the_share(self):
        tree = Tree()
        tree.team_working_agreement = "\n".join(f"- **Rule {n}** baseline." for n in range(1, 21)) + "\n"
        for p in tree.pages:
            p.rules = []
        tree.page("rules/one-criterion").rules = [Tree.rule("Only one anchored rule.", 157)]
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, rf"(?m)^docs/process-wiki/INDEX\.md: coverage floor.*{PROPOSED_FLOOR_SHARE:.2f}")

    def test_the_floor_passes_exactly_at_the_boundary_share(self):
        tree = Tree()
        tree.team_working_agreement = "\n".join(f"- **Rule {n}** baseline." for n in range(1, 11)) + "\n"
        tree.page("rules/one-criterion").rules = [Tree.rule("First.", 157)]
        tree.page("role/qa-red").rules = [Tree.rule("Second.", 165)]
        _, code, out, err = check(tree)
        self.assertEqual((code, err), (0, ""), err)
        self.assertIn("process-wiki: OK", out)


class HostileInputsCriterionTwo(unittest.TestCase):
    """D167, criterion 2 Inputs: disabled, unknown, repeated, reordered."""

    def _brief_tree(self, pages_line):
        tree = Tree()
        tree.extra_files["docs/process-wiki/briefs/architect-slice.md"] = (
            "Brief for the architect slice seat.\n\nPages: " + pages_line + "\n"
        )
        return tree

    # disabled
    def test_a_disabled_page_id_in_a_brief_fails_the_brief(self):
        tree = self._brief_tree("core/index, role/architect-slice, rules/one-criterion")
        tree.page("rules/one-criterion").status = "disabled"
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(
            err, r"(?m)^docs/process-wiki/briefs/architect-slice\.md: disabled page id rules/one-criterion")

    # unknown
    def test_an_unknown_page_id_in_pages_fails_the_brief(self):
        tree = self._brief_tree("core/index, role/architect-slice, role/no-such-seat")
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/briefs/architect-slice\.md: unknown page id.*role/no-such-seat")

    # repeated
    def test_a_repeated_page_id_in_pages_is_flagged(self):
        tree = self._brief_tree("core/index, role/architect-slice, role/architect-slice")
        _, code, _, err = check(tree)
        self.assertEqual(code, 1, err)
        self.assertRegex(err, r"(?m)^docs/process-wiki/briefs/architect-slice\.md: repeated page id.*role/architect-slice")

    # reordered
    def test_reordered_pages_remain_valid_when_the_set_is_the_same(self):
        good = self._brief_tree("core/index, role/architect-slice")
        reordered = self._brief_tree("role/architect-slice, core/index")
        g_code, _, g_err = check(good)[1:]
        r_code, _, r_err = check(reordered)[1:]
        self.assertEqual((g_code, g_err), (0, ""), g_err)
        self.assertEqual((r_code, r_err), (0, ""), r_err)


class ThisRepository(unittest.TestCase):
    """The real tree. The first group are assumption probes (D166) that hold today."""

    def read(self, rel):
        path = REAL / rel
        self.assertTrue(path.is_file(), f"{rel} is missing from {REAL}")
        return path.read_text(encoding="utf-8")

    def test_assumption_decision_anchors_d157_to_d170_exist(self):
        text = self.read(DECISIONS)
        for n in range(157, 171):
            self.assertIn(f'<a id="d{n}">', text, f"anchor d{n}")

    def test_assumption_decisions_index_links_the_process_decisions(self):
        text = self.read("docs/decisions/INDEX.md")
        for n in (157, 165, 166, 167, 168, 170):
            self.assertIn(f"process-and-staffing.md#d{n})", text, f"index link d{n}")

    def test_assumption_the_card_carries_the_baselines_and_the_adopted_condition(self):
        text = self.read(CARD)
        for number in ("50K", "125K", "965K"):
            self.assertRegex(text, rf"\b{number}\b")
        self.assertIn("a git-checkable test fails if any number or method is missing", text)

    def test_assumption_the_handoff_document_exists_for_restart(self):
        self.assertIn("handoff", self.read("HANDOFF.md").lower())

    def test_the_record_holds_the_three_measurement_methods(self):
        record = json.loads(self.read(RECORD))
        methods = record.get("measurement_methods")
        self.assertIsInstance(methods, dict, "measurement_methods is missing from the card record")
        for key in METHOD_KEYS:
            self.assertTrue(str(methods.get(key, "")).strip(), key)

    def test_the_wiki_index_exists_and_lists_every_kind_of_page(self):
        text = self.read(INDEX)
        ids = re.findall(r"(?m)^\|\s*((?:core|rules|role|howto)/[a-z0-9\-/]+)\s*\|", text)
        self.assertIn("core/index", ids)
        for kind in ("rules/", "role/", "howto/"):
            self.assertTrue([i for i in ids if i.startswith(kind)], f"no {kind} page in the index")

    def test_every_indexed_page_exists_with_unix_line_endings(self):
        text = self.read(INDEX)
        paths = re.findall(r"(?m)^\|\s*[a-z]+/[a-z0-9\-/]+\s*\|\s*(docs/process-wiki/[^|\s]+)\s*\|", text)
        self.assertTrue(paths, "the index lists no pages")
        for rel in paths:
            self.assertNotIn(b"\r", (REAL / rel).read_bytes(), rel)

    def test_the_checker_passes_on_this_repository(self):
        code, out, err = run("--root", str(REAL))
        self.assertEqual((code, err), (0, ""), err)
        self.assertRegex(out, r"process-wiki: OK \(\d+ pages\)")


if __name__ == "__main__":
    unittest.main()
