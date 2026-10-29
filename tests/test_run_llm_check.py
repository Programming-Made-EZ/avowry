import hashlib
import importlib.util
import json
import os
import subprocess
import sys
import tempfile
import time
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
    python = sys.executable
    python_q = python if " " not in python else f'"{python}"'
    return (
        f'{python_q} -c "import pathlib,sys; print(pathlib.Path(sys.argv[1]).read_text(encoding=\'utf-8\'))" '
        f"{json.dumps(str(path))}"
    )


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
            'avow to "list open tasks {available: Bool}" -> List<Task>\n'
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
            reply = work / "correct.avowry"
            reply.write_text(
                "module Demo\n\navow to \"ok\"\n  native csharp\n    return;\n",
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
            self.assertIn('avow to "ok"', captured)

    def test_committed_task_folder_has_no_start_copy(self):
        task = ROOT / "bench" / "usable-v1-llm-check" / "tasks" / "U1-add-endpoint"
        self.assertTrue((task / "prompt.md").is_file())
        self.assertTrue((task / "outcomes.json").is_file())
        self.assertFalse((task / "start.avowry").exists())
        self.assertFalse((task / "Tasks.avowry").exists())
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
            reply = work / "broken.avowry"
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

    def test_capture_wrapper_forwards_profile_flags_to_a_codex_like_cli(self):
        runner = load_runner()
        with tempfile.TemporaryDirectory(prefix="llm-check-flags-") as raw:
            work = Path(raw)
            argv_log = work / "codex-probe-argv.json"
            fake_cli = work / "codex-probe.py"
            fake_cli.write_text(
                "import json, pathlib, sys\n"
                f"pathlib.Path({json.dumps(str(argv_log))}).write_text("
                "json.dumps(sys.argv[1:]), encoding='utf-8')\n"
                "print('not a phrase')\n",
                encoding="utf-8",
            )
            python = sys.executable
            python_q = python if " " not in python else f'"{python}"'
            runner.write_capture_wrapper(work, f'{python_q} "{fake_cli}"')
            extra = ["exec", "--skip-git-repo-check", "--sandbox", "read-only"]
            completed = subprocess.run(
                [sys.executable, str(work / "capture_model.py"), *extra],
                input="PROMPT " * 200,
                capture_output=True,
                text=True,
                cwd=str(work),
            )
            self.assertEqual(0, completed.returncode, completed.stdout + completed.stderr)
            recorded = json.loads(argv_log.read_text(encoding="utf-8"))
            for flag in extra:
                self.assertIn(flag, recorded, recorded)

    def test_capture_wrapper_quotes_paths_with_spaces_on_both_platforms(self):
        runner = load_runner()
        with tempfile.TemporaryDirectory(prefix="llm-check-quote-") as raw:
            work = Path(raw)
            argv_log = work / "grok-probe-argv.json"
            fake_cli = work / "grok-probe.py"
            fake_cli.write_text(
                "import json, pathlib, sys\n"
                f"pathlib.Path({json.dumps(str(argv_log))}).write_text("
                "json.dumps(sys.argv[1:]), encoding='utf-8')\n"
                "print('not a phrase')\n",
                encoding="utf-8",
            )
            python = sys.executable
            python_q = python if " " not in python else f'"{python}"'
            runner.write_capture_wrapper(work, f'{python_q} "{fake_cli}"')
            prompt_file = work / "prompt with spaces.txt"
            prompt_file.write_text("hello", encoding="utf-8")
            extra = ["--max-turns", "10", "--prompt-file", str(prompt_file)]
            completed = subprocess.run(
                [sys.executable, str(work / "capture_model.py"), *extra],
                input="PROMPT",
                capture_output=True,
                text=True,
                cwd=str(work),
            )
            self.assertEqual(0, completed.returncode, completed.stdout + completed.stderr)
            recorded = json.loads(argv_log.read_text(encoding="utf-8"))
            self.assertEqual(extra, recorded)

    def test_zero_tests_in_the_dotnet_summary_is_a_spec_failure(self):
        runner = load_runner()
        for log in (
            "",
            "Test Run Successful.\nTotal tests: 0\n     Passed: 0\n     Failed: 0\n",
            "Passed!  - Failed:     0, Passed:     0, Skipped:     0, Total:     0, Duration: 1 ms\n",
        ):
            classified = runner.classify_test_run(specs=13, warnings=0, log=log, returncode=0)
            self.assertFalse(classified["passed"], log)
            self.assertEqual("spec", classified["reason"], log)

    def test_dotnet_test_summary_parses_total_tests_and_quiet_lines(self):
        runner = load_runner()
        quiet = "Passed!  - Failed:     0, Passed:    13, Skipped:     0, Total:    13, Duration: 2 s\n"
        self.assertEqual((13, 0), runner.parse_dotnet_test_counts(quiet))
        vstest = "Test Run Successful.\nTotal tests: 13\n     Passed: 13\n     Failed: 0\n"
        self.assertEqual((13, 0), runner.parse_dotnet_test_counts(vstest))
        failed = "Failed!  - Failed:     2, Passed:    11, Skipped:     0, Total:    13, Duration: 2 s\n"
        self.assertEqual((13, 2), runner.parse_dotnet_test_counts(failed))

    def test_missing_bench_row_is_a_failure_with_reason_class(self):
        runner = load_runner()
        document = runner.totals_document(
            "codex",
            "a" * 64,
            "b" * 64,
            {},
            {"passed": True, "specs": 13, "specs_passed": 13, "build_warnings": 0, "proof_level": "specs-passed"},
            True,
        )
        self.assertFalse(document["passed"])
        self.assertEqual("error", document["reason"])
        self.assertNotIn("reply", document)

    def test_unavailable_model_is_a_failure_with_reason_class(self):
        runner = load_runner()
        bench_row = {
            "status": "unavailable",
            "attempts": 1,
            "tokens_in": 40,
            "tokens_out": 0,
            "error": "empty reply",
        }
        document = runner.totals_document(
            "grok",
            "a" * 64,
            "b" * 64,
            bench_row,
            {"passed": True, "specs": 13, "specs_passed": 13, "build_warnings": 0, "proof_level": "specs-passed"},
            True,
        )
        self.assertFalse(document["passed"])
        self.assertEqual("error", document["reason"])
        self.assertEqual(1, document["attempts"])
        self.assertNotIn("reply", document)
        self.assertNotIn("error", document)

    def test_timeout_still_writes_out_json_without_reply_text(self):
        runner = load_runner()
        original = runner.run_process

        def boom(*_args, **_kwargs):
            raise TimeoutError("process 1 exceeded 1s: dotnet run")

        runner.run_process = boom
        try:
            with tempfile.TemporaryDirectory(prefix="llm-check-timeout-") as raw:
                work = Path(raw)
                output = work / "llm-check.json"
                code = runner.main(
                    [
                        "--model",
                        "codex",
                        "--model-command",
                        print_file_command(work / "missing.avowry"),
                        "--out",
                        str(output),
                        "--repo",
                        str(ROOT),
                    ]
                )
                self.assertNotEqual(0, code)
                self.assertTrue(output.is_file())
                text = output.read_text(encoding="utf-8")
                document = json.loads(text)
                self.assertFalse(document["passed"])
                self.assertEqual("timeout", document["reason"])
                self.assertEqual("codex", document["model"])
                self.assertNotIn("reply", document)
                self.assertNotIn("```", text)
        finally:
            runner.run_process = original

    def test_crash_still_writes_out_json_with_error_reason(self):
        runner = load_runner()
        original = runner.run_process

        def boom(*_args, **_kwargs):
            raise RuntimeError("dotnet crashed SECRET_REPLY_TEXT")

        runner.run_process = boom
        try:
            with tempfile.TemporaryDirectory(prefix="llm-check-crash-") as raw:
                work = Path(raw)
                output = work / "llm-check.json"
                code = runner.main(
                    [
                        "--model",
                        "grok",
                        "--model-command",
                        print_file_command(work / "missing.avowry"),
                        "--out",
                        str(output),
                        "--repo",
                        str(ROOT),
                    ]
                )
                self.assertNotEqual(0, code)
                text = output.read_text(encoding="utf-8")
                document = json.loads(text)
                self.assertFalse(document["passed"])
                self.assertEqual("error", document["reason"])
                self.assertNotIn("SECRET_REPLY_TEXT", text)
                self.assertNotIn("reply", document)
        finally:
            runner.run_process = original

    def test_run_process_uses_utf8_replace_and_posix_process_group(self):
        runner = load_runner()
        source = SCRIPT.read_text(encoding="utf-8")
        self.assertIn("encoding", source)
        self.assertIn("utf-8", source)
        self.assertIn("replace", source)
        self.assertIn("start_new_session", source)
        self.assertIn("killpg", source)
        self.assertNotIn("p" + "kill", source)
        self.assertNotIn("kill" + "all", source)
        self.assertNotIn("/IM", source)

    def test_run_process_replaces_invalid_utf8_and_kills_the_tree_on_timeout(self):
        runner = load_runner()
        with tempfile.TemporaryDirectory(prefix="llm-check-tree-") as raw:
            work = Path(raw)
            child_pid = work / "child.pid"
            script = work / "tree.py"
            script.write_text(
                "import pathlib, subprocess, sys, time\n"
                "child = subprocess.Popen([sys.executable, '-c', 'import time; time.sleep(30)'])\n"
                f"pathlib.Path({json.dumps(str(child_pid))}).write_text(str(child.pid))\n"
                "child.wait()\n",
                encoding="utf-8",
            )
            with self.assertRaises(TimeoutError):
                runner.run_process(
                    [sys.executable, str(script)],
                    work,
                    os.environ.copy(),
                    timeout=1,
                )
            self.assertTrue(child_pid.is_file())
            pid = int(child_pid.read_text(encoding="utf-8").strip())
            self.assertFalse(_pid_is_running(pid), f"child {pid} was still running")

            replaced = runner.run_process(
                [sys.executable, "-c", "import sys; sys.stdout.buffer.write(b'\\xff hello')"],
                work,
                os.environ.copy(),
                timeout=10,
            )
            self.assertEqual(0, replaced.returncode)
            self.assertIn("hello", replaced.stdout)

    def test_find_phrase_dll_prefers_the_freshest_build(self):
        runner = load_runner()
        with tempfile.TemporaryDirectory(prefix="llm-check-dll-") as raw:
            repo = Path(raw)
            debug = repo / "src" / "Phrase.Cli" / "bin" / "Debug" / "net8.0"
            release = repo / "src" / "Phrase.Cli" / "bin" / "Release" / "net8.0"
            debug.mkdir(parents=True)
            release.mkdir(parents=True)
            older = debug / "phrase.dll"
            newer = release / "phrase.dll"
            older.write_bytes(b"old")
            newer.write_bytes(b"new")
            older_time = 1_700_000_000
            newer_time = 1_800_000_000
            os.utime(older, (older_time, older_time))
            os.utime(newer, (newer_time, newer_time))
            chosen = runner.find_phrase_dll(repo, os.environ.copy())
            self.assertEqual(newer.resolve(), chosen.resolve())

            os.utime(older, (newer_time + 10, newer_time + 10))
            chosen_debug = runner.find_phrase_dll(repo, os.environ.copy())
            self.assertEqual(older.resolve(), chosen_debug.resolve())


def _pid_is_running(pid: int) -> bool:
    if pid <= 0:
        return False
    if os.name == "nt":
        completed = subprocess.run(
            ["tasklist", "/FI", f"PID eq {pid}"],
            capture_output=True,
            text=True,
            check=False,
        )
        return str(pid) in completed.stdout
    # A killed child may need a moment to exit and, when its parent is gone, can stay a zombie until init reaps it.
    deadline = time.time() + 5
    while True:
        try:
            os.kill(pid, 0)
        except OSError:
            return False
        try:
            state = Path(f"/proc/{pid}/stat").read_text(encoding="utf-8").rsplit(")", 1)[1].split()[0]
        except (OSError, IndexError):
            state = ""
        if state == "Z":
            return False
        if time.time() >= deadline:
            return True
        time.sleep(0.1)


if __name__ == "__main__":
    unittest.main()
