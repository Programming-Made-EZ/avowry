import importlib.util
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SPEC = importlib.util.spec_from_file_location(
    "depersonalize_cards", ROOT / "scripts" / "board" / "depersonalize_cards.py"
)
cards = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(cards)


class DepersonalizeCardsTests(unittest.TestCase):
    def test_reviewed_rules_rewrite_sources_and_escalations_but_keep_persona_story(self):
        name = "Ste" + "ve"
        original = (
            f"- **Source:** {name}, 2026-10-25: web concepts belong in an extension. Decider D51.\n"
            f"- **Story:** As {name}, I want proof, so that I can decide.\n"
            f"Escalation needs {name}; write Needs-{name}.md.\n"
        )
        expected = (
            "- **Source:** product direction, 2026-10-25: web concepts belong in an extension. Decider D51.\n"
            f"- **Story:** As {name}, I want proof, so that I can decide.\n"
            "Escalation needs the product owner; write Needs-product-owner.md.\n"
        )
        self.assertEqual(expected, cards.depersonalize(original))
        self.assertEqual(expected, cards.depersonalize(expected))

    def test_unknown_mentions_are_reported(self):
        name = "Ste" + "ve"
        rewritten = cards.depersonalize(f"A note mentioning {name} casually.\n")
        self.assertEqual([(1, f"A note mentioning {name} casually.")], cards.unknown_mentions(rewritten))


if __name__ == "__main__":
    unittest.main()
