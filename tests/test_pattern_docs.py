"""Card 162, criterion 1: the grammar and the cheat sheet teach the ensure step and the record update.

Reads the working tree only (no git history). Whitespace is collapsed before every phrase needle,
and sections are split on the three-hash heading marker.
"""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GRAMMAR_PATH = ROOT / "docs" / "GRAMMAR.md"
SHEET_PATH = ROOT / "docs" / "CHEATSHEET.md"


def collapse(text):
    return re.sub(r"\s+", " ", text).strip()


def read(path):
    return path.read_bytes().decode("utf-8")


def sections(text):
    """Map each level-3 heading line to its body (up to the next level-3 heading)."""
    parts = re.split(r"(?m)^### ", text)
    found = {}
    for part in parts[1:]:
        head, _, body = part.partition("\n")
        found[head.strip()] = body
    return found


def find_section(text, prefix):
    for head, body in sections(text).items():
        if head.lower().startswith(prefix.lower()):
            return collapse(body)
    return None


def ebnf(text):
    match = re.search(r"```ebnf\n(.*?)```", text, re.S)
    return match.group(1) if match else ""


def rule(block, name):
    """The right-hand side of one ebnf rule (up to the terminating semicolon), whitespace collapsed."""
    match = re.search(r"(?m)^\s*" + re.escape(name) + r"\s*=(.*?);", block, re.S)
    return collapse(match.group(1)) if match else None


def fence_lines(text):
    lines = []
    inside = False
    for line in text.split("\n"):
        if line.startswith("```"):
            inside = not inside
            continue
        if inside:
            lines.append(line)
    return lines


class GrammarTeachesTheSteps(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.raw = read(GRAMMAR_PATH)
        cls.text = collapse(cls.raw)
        cls.block = ebnf(cls.raw)

    def need_section(self, prefix):
        body = find_section(self.raw, prefix)
        self.assertIsNotNone(body, "docs/GRAMMAR.md has no '### " + prefix + "' section")
        return body

    def need_in(self, haystack, needles, where):
        missing = [needle for needle in needles if needle not in haystack]
        self.assertEqual([], missing, where + " is missing: " + ", ".join(missing))

    def test_G1_ebnf_step_lists_ensure_step_and_defines_it(self):
        step = rule(self.block, "step")
        self.assertIsNotNone(step, "the ebnf block has no step rule")
        self.assertIn("ensure_step", step, "the ebnf step rule does not list ensure_step: " + step)
        self.assertIsNotNone(rule(self.block, "ensure_step"), "the ebnf block has no ensure_step rule")

    def test_G2_ensure_section_states_the_four_conditions_and_all_six_comparisons(self):
        section = self.need_section("Ensure step")
        self.need_in(section, ["is blank", "is not blank", "length at most"], "the Ensure step section")
        operators = rule(self.block, "compare_op")
        self.assertIsNotNone(operators, "the ebnf block has no compare_op rule")
        missing = [op for op in ["<", "<=", ">", ">=", "==", "!="] if '"' + op + '"' not in operators]
        self.assertEqual([], missing, "compare_op is missing operators: " + ", ".join(missing) + " in: " + operators)

    def test_G3_ensure_section_names_else_fails_with_and_AV326_and_the_table_row_says_ensure(self):
        section = self.need_section("Ensure step")
        self.need_in(section, ["else", "fails with", "AV326"], "the Ensure step section")
        row = re.search(r"(?m)^\| `AV326` \|(.*)$", self.raw)
        self.assertIsNotNone(row, "the diagnostics table has no AV326 row")
        self.assertIn("ensure", row.group(1).lower(), "the AV326 table row does not mention ensure: " + row.group(1).strip())

    def test_G4_ensure_without_else_is_an_ordinary_avow_call(self):
        needle = "An `ensure` line without `else` is an ordinary avow call"
        self.assertTrue(needle in self.text, "docs/GRAMMAR.md never says: " + needle)

    def test_G5_update_step_is_in_the_ebnf_and_in_the_step_rule(self):
        step = rule(self.block, "step")
        self.assertIsNotNone(step, "the ebnf block has no step rule")
        self.assertIn("update_step", step, "the ebnf step rule does not list update_step: " + step)
        self.assertIsNotNone(rule(self.block, "update_step"), "the ebnf block has no update_step rule")

    def test_G6_record_update_section_states_the_rules(self):
        section = self.need_section("Record update step")
        self.need_in(section, ["AV200", "AV303", "AV311", "AV312", "AV313", "trailing comma", "nested"],
                     "the Record update step section")

    def test_G7_the_AV_code_convention_is_stated(self):
        needle = "Every diagnostic code is `AV` followed by three digits"
        self.assertTrue(needle in self.text, "docs/GRAMMAR.md never says: " + needle)

    def test_G8_with_is_no_longer_called_an_ordinary_word_in_steps(self):
        old = "words like `with` or `input` are ordinary words."
        self.assertFalse(old in self.text, "docs/GRAMMAR.md still says: " + old)
        self.assertTrue("starts a record update" in self.text, "docs/GRAMMAR.md never says: starts a record update")


class CheatSheetShowsTheSteps(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.lines = fence_lines(read(SHEET_PATH))

    def test_S1_a_sheet_example_has_an_ensure_step_with_else(self):
        pattern = re.compile(r"^\s+ensure .+ else [A-Z]\w*$")
        self.assertTrue(any(pattern.match(line) for line in self.lines),
                        "no fence line in docs/CHEATSHEET.md matches an ensure ... else Failure step")

    def test_S2_a_sheet_example_has_a_record_update(self):
        pattern = re.compile(r"^\s+let \w+ = [\w.]+ with \{ .+ \}$")
        self.assertTrue(any(pattern.match(line) for line in self.lines),
                        "no fence line in docs/CHEATSHEET.md matches a let x = rec with { f: v } step")


if __name__ == "__main__":
    unittest.main()
