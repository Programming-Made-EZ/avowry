import importlib.util
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location(
    "check_readme_reference_status",
    ROOT / "scripts" / "check-readme-reference-status.py",
)
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)


class ReadmeReferenceStatusTests(unittest.TestCase):
    def readme(self, status):
        return (
            "| Test | Status | Why |\n"
            "|---|---|---|\n"
            "| `ReferenceTarget.Reference_output_builds_and_every_spec_passes` "
            f"| {status} | evidence |\n"
        )

    def verdict(self, status, runner):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "README.md"
            path.write_text(self.readme(status), encoding="utf-8")
            return check.check_status(path, runner)

    def test_green_readme_disagrees_with_a_failed_run(self):
        self.assertIn("disagrees", self.verdict("Green", lambda: False))

    def test_matching_green_result_passes(self):
        self.assertIsNone(self.verdict("Green", lambda: True))

    def test_matching_failing_result_passes(self):
        self.assertIsNone(self.verdict("Failing", lambda: False))

    def test_only_green_or_failing_is_accepted(self):
        self.assertIn("Green or Failing", self.verdict("Unknown", lambda: True))


if __name__ == "__main__":
    unittest.main()
