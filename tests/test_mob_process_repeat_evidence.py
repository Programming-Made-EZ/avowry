"""Card 264 cycle 1b (D157 Criterion 1): mob_process check must call scripts/check-repeat-evidence.py."""
import contextlib, importlib.util, io, shutil, tempfile, unittest
from pathlib import Path
from unittest import mock

REPO = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location("mob_process", REPO / "scripts/mob_process.py")
mob = importlib.util.module_from_spec(SPEC); SPEC.loader.exec_module(mob)
OTHERS = ("check_records", "check_retros", "check_legacy", "check_mob", "check_events", "check_workflows",
          "check_issue_labels", "check_windows", "check_process_wiki", "check_restart_record")


class RepeatEvidenceWiringTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(); self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        (self.root / "scripts").mkdir()
        (self.root / ".mob/records").mkdir(parents=True)
        (self.root / ".mob/records/LEGACY.json").write_text('{"cards": []}', encoding="utf-8")
        for name in OTHERS:
            patcher = mock.patch.object(mob, name, return_value=[]); patcher.start(); self.addCleanup(patcher.stop)

    def install_checker(self):
        shutil.copy(REPO / "scripts/check-repeat-evidence.py", self.root / "scripts/check-repeat-evidence.py")

    def run_check(self):
        out = io.StringIO()
        with contextlib.redirect_stdout(out):
            code = mob.run_check(self.root, [], git=lambda *a: "")
        return code, out.getvalue()

    def record(self, text=None):
        target = self.root / ".mob/records/264"; target.mkdir(parents=True, exist_ok=True)
        if text is None:
            shutil.copy(REPO / ".mob/records/264/repeat-evidence.json", target / "repeat-evidence.json")
        else:
            (target / "repeat-evidence.json").write_text(text, encoding="utf-8")

    def test_absent_record_fails_naming_the_repeat_evidence_check(self):
        self.install_checker()
        code, out = self.run_check()
        self.assertEqual(1, code)
        self.assertIn("repeat-evidence", out)

    def test_invalid_record_fails_naming_the_repeat_evidence_check(self):
        self.install_checker(); self.record("{}")
        code, out = self.run_check()
        self.assertEqual(1, code)
        self.assertIn("repeat-evidence", out)

    def test_missing_checker_script_fails_closed(self):
        self.record()
        code, out = self.run_check()
        self.assertEqual(1, code)
        self.assertIn("repeat-evidence", out)

    def test_valid_record_reports_no_repeat_evidence_error(self):
        self.install_checker(); self.record()
        code, out = self.run_check()
        self.assertEqual(0, code, out)
        self.assertNotIn("repeat-evidence", out)


if __name__ == "__main__":
    unittest.main()
