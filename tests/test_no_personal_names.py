import importlib.util
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location(
    "check_no_personal_names", ROOT / "scripts" / "check-no-personal-names.py"
)
check = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(check)


class NoPersonalNamesTests(unittest.TestCase):
    def test_rejects_attribution_and_escalation_but_allows_persona_contexts(self):
        name = "Ste" + "ve"
        forbidden = [
            f"{name}'s ruling keeps the core small.",
            f'{name}: "keep the core small"',
            f"approved by {name}",
            f"needs {name}",
            f"direction from {name}",
        ]
        for line in forbidden:
            with self.subTest(line=line):
                self.assertTrue(check.forbidden_reasons("notes.md", line))

        self.assertFalse(check.forbidden_reasons("PERSONAS.md", f"## {name}: sponsor, on a phone"))
        self.assertFalse(check.forbidden_reasons("board/001.md", f"- **Story:** As {name}, I want clarity"))
        self.assertFalse(check.forbidden_reasons("scripts/personas.py", f'PERSONAS = ["{name}", "Morgan"]'))

    def test_archive_findings_are_separate(self):
        name = "Ste" + "ve"
        regular, archive = check.classify_findings(
            [("VISION.md", 3, f"{name} ruled this"), ("board/001-x.md", 4, f"{name} ruled this")]
        )
        self.assertEqual(["VISION.md"], [item[0] for item in regular])
        self.assertEqual(["board/001-x.md"], [item[0] for item in archive])


if __name__ == "__main__":
    unittest.main()
