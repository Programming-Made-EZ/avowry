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


def contract(consumer="the C# emitter", observes="an API project whose handler returns the declared status", enables="039", shape="Contract"):
    return (f"# C\n\n- **Status:** Ready\n- **Shape:** {shape}\n- **Consumer:** {consumer}\n- **Given:** a resolved model\n"
            f"- **Observes:** {observes}\n- **Fails with:** PH333\n- **Invariants:** deterministic output\n- **Enables:** {enables}\n")


class ContractCardTests(unittest.TestCase):
    def verdict(self, text, target_shape="Story", target_exists=True):
        with tempfile.TemporaryDirectory() as tmp:
            tmp = Path(tmp)
            personas = tmp / "PERSONAS.md"
            personas.write_text(PERSONAS, encoding="utf-8")
            if target_exists:
                (tmp / "039-parent.md").write_text(card("As Ari, I want x, so that y.").replace("- **Status:** Ready", f"- **Status:** Ready\n- **Shape:** {target_shape}"), encoding="utf-8")
            path = tmp / "040-contract.md"
            path.write_text(text, encoding="utf-8")
            return check.check_card(path, check.persona_names(personas))

    def test_a_contract_naming_a_component_an_observation_and_a_story_passes(self):
        self.assertIsNone(self.verdict(contract()))

    def test_a_contract_that_enables_nothing_is_an_orphan(self):
        self.assertIn("orphan", self.verdict(contract(enables="")))

    def test_a_contract_whose_enabled_card_is_missing_is_an_orphan(self):
        self.assertIn("orphan", self.verdict(contract(), target_exists=False))

    def test_a_contract_that_enables_only_another_contract_is_an_orphan(self):
        self.assertIn("orphan", self.verdict(contract(), target_shape="Contract"))

    def test_a_bare_success_observation_is_rejected(self):
        for bare in ("it succeeds", "no exception is thrown", "it compiles", "it does not throw"):
            with self.subTest(bare=bare):
                self.assertIn("observable", self.verdict(contract(observes=bare)))

    def test_a_person_or_role_as_the_consumer_is_rejected(self):
        for who in ("an engineer", "Ari", "a user", "the developer"):
            with self.subTest(who=who):
                self.assertIn("component", self.verdict(contract(consumer=who)))

    def test_a_contract_missing_observes_is_rejected(self):
        text = contract().replace("- **Observes:** an API project whose handler returns the declared status\n", "")
        self.assertIn("Observes", self.verdict(text))


if __name__ == "__main__":
    unittest.main()
