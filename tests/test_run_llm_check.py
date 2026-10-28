import hashlib
import importlib.util
import json
import os
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / "scripts" / "run-llm-check.py"


def load_runner():
    spec = importlib.util.spec_from_file_location("run_llm_check", SCRIPT)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def print_file_command(path: Path) -> str:
    if os.name == "nt":
        return f'more > nul && type "{path}"'
    return f'cat > /dev/null && cat "{path}"'


class RunLlmCheckTests(unittest.TestCase):
    def test_extract_phrase_keeps_blank_lines_in_a_module(self):
        runner = load_runner()
        source = (
            "# header comment\n"
            "module Tasks\n"
            "\n"
            "type Task\n"
            "  id: Id\n"
            "\n"
            'phrase "list open tasks {available: Bool}" -> List<Task>\n'
            "  native csharp\n"
            "    return taskstore.List();\n"
        )
        extracted = runner.extract_phrase("ignored prefix\n" + source)
        self.assertIn("module Tasks", extracted)
        self.assertIn("type Task", extracted)
        self.assertIn("list open tasks", extracted)
        self.assertEqual(source.strip(), runner.extract_phrase(source))

    def test_capture_wrapper_prints_file_when_stdin_holds_a_prompt(self):
        runner = load_runner()
        with tempfile.TemporaryDirectory(prefix="llm-check-capture-") as raw:
            work = Path(raw)
            reply = work / "correct.phrase"
            reply.write_text(
                "module Demo\n\nphrase \"ok\"\n  native csharp\n    return;\n",
                encoding="utf-8",
            )
            runner.write_capture_wrapper(work, print_file_command(reply))
            completed = subprocess.run(
                [sys.executable, str(work / "capture_model.py")],
                input="PROMPT " * 4000,
                capture_output=True,
                text=True,
                cwd=str(work),
            )
            captured = (work / "reply.txt").read_text(encoding="utf-8")
            self.assertEqual(0, completed.returncode, completed.stdout + completed.stderr)
            self.assertIn("module Demo", completed.stdout)
            self.assertIn("module Demo", captured)
            self.assertIn('phrase "ok"', captured)

    def test_committed_task_folder_has_no_start_copy(self):
        task = ROOT / "bench" / "usable-v1-llm-check" / "tasks" / "U1-add-endpoint"
        self.assertTrue((task / "prompt.md").is_file())
        self.assertTrue((task / "outcomes.json").is_file())
        self.assertFalse((task / "start.phrase").exists())
        self.assertFalse((task / "Tasks.phrase").exists())
        prompt = (task / "prompt.md").read_text(encoding="utf-8")
        self.assertNotIn("```", prompt)
        self.assertNotIn("native csharp", prompt)
        outcomes = json.loads((task / "outcomes.json").read_text(encoding="utf-8"))
        self.assertTrue(outcomes)
        self.assertIn("responds", outcomes[0]["then"])
        self.assertEqual("handled", outcomes[0]["when"])
        self.assertTrue(any("fails_with" in case["then"] for case in outcomes))

    def test_compile_failure_writes_reason_and_omits_reply(self):
        runner = load_runner()
        with tempfile.TemporaryDirectory(prefix="llm-check-py-") as raw:
            work = Path(raw)
            reply = work / "broken.phrase"
            reply.write_text("this is not a Phrase module\nSECRET_REPLY_TEXT\n", encoding="utf-8")
            output = work / "llm-check.json"
            completed = subprocess.run(
                [
                    sys.executable,
                    str(SCRIPT),
                    "--model",
                    "codex",
                    "--model-command",
                    print_file_command(reply),
                    "--out",
                    str(output),
                    "--repo",
                    str(ROOT),
                    "--timeout",
                    "180",
                ],
                cwd=str(ROOT),
                capture_output=True,
                text=True,
                env={**os.environ, "MSBUILDDISABLENODEREUSE": "1"},
            )
            self.assertNotEqual(0, completed.returncode, completed.stdout + completed.stderr)
            self.assertTrue(output.is_file(), completed.stdout + completed.stderr)
            text = output.read_text(encoding="utf-8")
            document = json.loads(text)
            self.assertFalse(document["passed"])
            self.assertEqual("compile", document["reason"])
            self.assertEqual("codex", document["model"])
            self.assertNotIn("SECRET_REPLY_TEXT", text)
            self.assertNotIn("reply", document)
            self.assertNotIn("reply_head", document)
            self.assertNotIn("diagnostics", document)
            grammar = hashlib.sha256((ROOT / "docs" / "GRAMMAR.md").read_bytes()).hexdigest()
            sheet = hashlib.sha256((ROOT / "docs" / "CHEATSHEET.md").read_bytes()).hexdigest()
            self.assertEqual(grammar, document["grammar_sha256"])
            self.assertEqual(sheet, document["cheatsheet_sha256"])
            self.assertEqual(64, len(document["grammar_sha256"]))
            copied = runner.prepare_docs(ROOT, work)
            docs_root = copied[0]
            names = sorted(p.name for p in (docs_root / "docs").iterdir())
            self.assertEqual(["CHEATSHEET.md", "GRAMMAR.md"], names)
            self.assertFalse((docs_root / "tests").exists())


if __name__ == "__main__":
    unittest.main()
