import importlib.util
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location("check_story_persona", ROOT / "scripts" / "check-story-persona.py")
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)

PERSONAS = """# Personas

## Ari: cold-start coding agent

text

## the mob (Riley): dogfood user
"""


def card(story, status="Ready"):
    return f"# T\n\n- **Status:** {status}\n- **Story:** {story}\n"


class StoryPersonaTests(unittest.TestCase):
    def verdict(self, text, status="Ready"):
        with tempfile.TemporaryDirectory() as tmp:
            personas = Path(tmp) / "PERSONAS.md"
            personas.write_text(PERSONAS, encoding="utf-8")
            path = Path(tmp) / "001-x.md"
            path.write_text(card(text, status), encoding="utf-8")
            return check.check_card(path, check.persona_names(personas))

    def test_persona_names_are_read_from_the_headings(self):
        with tempfile.TemporaryDirectory() as tmp:
            personas = Path(tmp) / "PERSONAS.md"
            personas.write_text(PERSONAS, encoding="utf-8")
            self.assertEqual(check.persona_names(personas), ["Ari", "Riley"])

    def test_a_story_naming_a_persona_with_a_so_that_passes(self):
        self.assertIsNone(self.verdict("As Ari, I want a phrase to compile, so that I can see it work in one attempt."))

    def test_a_role_only_story_is_rejected(self):
        self.assertIn("persona", self.verdict("As an engineer (or an LLM) writing Phrase, I want a phrase to compile, so that it works."))

    def test_a_story_without_a_so_that_is_rejected(self):
        self.assertIn("so that", self.verdict("As Ari, I want a phrase to compile."))

    def test_a_persona_named_only_later_in_the_sentence_is_rejected(self):
        self.assertIn("persona", self.verdict("As an engineer, I want what Ari wants, so that it works."))

    def test_done_superseded_and_withdrawn_cards_are_skipped(self):
        for status in ("Done 2026-10-26", "SUPERSEDED by 076", "WITHDRAWN 2026-10-26"):
            self.assertIsNone(self.verdict("As an engineer, I want x.", status=status))


if __name__ == "__main__":
    unittest.main()
