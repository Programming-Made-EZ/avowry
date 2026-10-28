#!/usr/bin/env python3
"""Run the Usable v1 LLM-customer check on the existing Phrase bench arm.

A cold model sees only GRAMMAR.md and CHEATSHEET.md plus the current Tasks.phrase
(copied at run time). The result is totals-only JSON: no task text, no reply text.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import shutil
import signal
import subprocess
import sys
import tempfile
from datetime import datetime, timezone
from pathlib import Path


FORBIDDEN_JSON_KEYS = {
    "reply",
    "reply_head",
    "prompt",
    "task",
    "diagnostics",
    "stdout",
    "stderr",
    "source",
    "text",
}
SPEC_LINE = re.compile(r'^\s+spec "', re.M)
WARNING_COUNT = re.compile(r"(\d+) Warning\(s\)")
TOTAL_TESTS = re.compile(r"Total:\s+(\d+)")
FAILED_TESTS = re.compile(r"Failed:\s+(\d+)")
PHRASE_FENCE = re.compile(
    r"```+[ \t]*(?:phrase[^\r\n]*)?\r?\n(?P<body>[\s\S]*?)```+",
    re.I,
)
PLAIN_PHRASE = re.compile(
    r"(?ms)(?:^|\n)(?P<body>(?:module|phrase|type|endpoint)\b[^\n]*(?:\n\s+.*)*)",
    re.I,
)
COMPLETE_PHRASE = re.compile(
    r"(?m)(?:#.*\n)*\s*(?:module|phrase|type|endpoint)\b",
    re.I,
)
REASON_COMPILE = "compile"
REASON_BUILD = "build"
REASON_SPEC = "spec"
REASON_OUTCOME = "outcome"
REASON_TIMEOUT = "timeout"
REASON_ERROR = "error"
QUIET_SUMMARY = re.compile(
    r"Failed:\s+(\d+),\s+Passed:\s+(\d+),\s+Skipped:\s+(\d+),\s+Total:\s+(\d+)",
    re.I,
)
TOTAL_TESTS_LINE = re.compile(r"Total tests:\s+(\d+)", re.I)
FAILED_INDENTED = re.compile(r"^\s*Failed:\s+(\d+)\s*$", re.M | re.I)


def repo_root(explicit: str | None) -> Path:
    if explicit:
        return Path(explicit).resolve()
    return Path(__file__).resolve().parent.parent


def sha256_file(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def copy_file(source: Path, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source, destination)


def prepare_dotnet_env(env: dict[str, str]) -> dict[str, str]:
    prepared = dict(env)
    prepared["MSBUILDDISABLENODEREUSE"] = "1"
    prepared["DOTNET_ROLL_FORWARD"] = "Major"
    for name in (
        "MSBUILD_EXE_PATH",
        "MSBuildExtensionsPath",
        "MSBuildSDKsPath",
        "MSBuildLoadMicrosoftTargetsReadOnly",
    ):
        prepared.pop(name, None)
    dotnet_root = prepared.get("DOTNET_ROOT") or str(Path.home() / ".dotnet")
    if Path(dotnet_root).is_dir():
        prepared["DOTNET_ROOT"] = dotnet_root
        path = prepared.get("PATH", "")
        if not path.lower().startswith(dotnet_root.lower()):
            prepared["PATH"] = dotnet_root + os.pathsep + path
    return prepared


def stop_pid(pid: int) -> None:
    if pid <= 0:
        return
    if os.name == "nt":
        subprocess.run(
            ["taskkill", "/PID", str(pid), "/T", "/F"],
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            check=False,
        )
        return
    try:
        os.killpg(pid, signal.SIGKILL)
    except OSError:
        try:
            os.kill(pid, signal.SIGKILL)
        except OSError:
            return


def run_process(argv: list[str], cwd: Path, env: dict[str, str], timeout: int) -> subprocess.CompletedProcess[str]:
    popen_kwargs: dict = {
        "cwd": str(cwd),
        "env": env,
        "stdout": subprocess.PIPE,
        "stderr": subprocess.PIPE,
        "text": True,
        "encoding": "utf-8",
        "errors": "replace",
    }
    if os.name != "nt":
        popen_kwargs["start_new_session"] = True
    proc = subprocess.Popen(argv, **popen_kwargs)
    pid = proc.pid
    try:
        stdout, stderr = proc.communicate(timeout=timeout)
    except subprocess.TimeoutExpired:
        stop_pid(pid)
        try:
            stdout, stderr = proc.communicate(timeout=10)
        except subprocess.TimeoutExpired:
            stdout, stderr = "", ""
        raise TimeoutError(f"process {pid} exceeded {timeout}s: {' '.join(argv[:6])}")
    return subprocess.CompletedProcess(argv, proc.returncode, stdout or "", stderr or "")


CAPTURE_HELPER = """
import pathlib, shlex, subprocess, sys

def quote_for_shell(argument):
    if sys.platform == "win32":
        if argument == "":
            return '""'
        if any(ch in argument for ch in ' \t"&|<>^()%'):
            return '"' + argument.replace('"', '\\\\"') + '"'
        return argument
    return shlex.quote(argument)

cmd = pathlib.Path(COMMAND_PATH).read_text(encoding="utf-8")
if sys.argv[1:]:
    cmd = cmd + " " + " ".join(quote_for_shell(argument) for argument in sys.argv[1:])
prompt = sys.stdin.buffer.read()
if sys.platform == "win32":
    proc = subprocess.run("cmd.exe /d /c " + cmd, input=prompt, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
else:
    proc = subprocess.run(["/bin/sh", "-c", cmd], input=prompt, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
pathlib.Path(OUT_PATH).write_bytes(proc.stdout or b"")
sys.stdout.buffer.write(proc.stdout or b"")
sys.stderr.buffer.write(proc.stderr or b"")
raise SystemExit(proc.returncode)
"""


def write_capture_wrapper(scratch: Path, command: str) -> str:
    command_path = scratch / "model-command.txt"
    capture_path = scratch / "reply.txt"
    helper_path = scratch / "capture_model.py"
    command_path.write_text(command, encoding="utf-8")
    helper_path.write_text(
        f"COMMAND_PATH = {json.dumps(str(command_path))}\n"
        f"OUT_PATH = {json.dumps(str(capture_path))}\n"
        + CAPTURE_HELPER,
        encoding="utf-8",
    )
    python = sys.executable or "python"
    if os.name == "nt" and " " not in python:
        return f'{python} "{helper_path}"'
    return f'"{python}" "{helper_path}"'


def contains_phrase(source: str) -> bool:
    return re.search(r"(?m)^(?:module|phrase|type|endpoint)\b", source, re.I) is not None


def extract_phrase(output: str) -> str:
    stripped = output.strip()
    if contains_phrase(stripped) and COMPLETE_PHRASE.match(stripped):
        return stripped
    for match in PHRASE_FENCE.finditer(output):
        body = match.group("body").strip()
        if contains_phrase(body):
            return body
    match = PLAIN_PHRASE.search(output)
    if match:
        body = match.group("body").strip()
        if contains_phrase(body):
            return body
    return stripped


def count_specs(source: str) -> int:
    return len(SPEC_LINE.findall(source))


def default_model_command(model: str) -> str:
    return {"codex": "codex", "grok": "grok"}[model]


def prepare_suite(repo: Path, work: Path) -> Path:
    suite_src = repo / "bench" / "usable-v1-llm-check"
    suite_dst = work / "bench"
    task_dst = suite_dst / "tasks" / "U1-add-endpoint"
    task_dst.mkdir(parents=True, exist_ok=True)
    copy_file(suite_src / "suite.json", suite_dst / "suite.json")
    copy_file(suite_src / "tasks" / "U1-add-endpoint" / "prompt.md", task_dst / "prompt.md")
    copy_file(suite_src / "tasks" / "U1-add-endpoint" / "outcomes.json", task_dst / "outcomes.json")
    copy_file(repo / "demo" / "tasks" / "Tasks.phrase", task_dst / "start.phrase")
    return suite_dst / "suite.json"


def prepare_docs(repo: Path, work: Path) -> tuple[Path, str, str]:
    docs_root = work / "docs-root"
    grammar_src = repo / "docs" / "GRAMMAR.md"
    sheet_src = repo / "docs" / "CHEATSHEET.md"
    copy_file(grammar_src, docs_root / "docs" / "GRAMMAR.md")
    copy_file(sheet_src, docs_root / "docs" / "CHEATSHEET.md")
    return docs_root, sha256_file(grammar_src), sha256_file(sheet_src)


def phrase_dll_candidates(repo: Path) -> list[Path]:
    root = repo / "src" / "Phrase.Cli" / "bin"
    if not root.is_dir():
        return []
    return [path for path in root.glob("*/net8.0/phrase.dll") if path.is_file()]


def find_phrase_dll(repo: Path, env: dict[str, str]) -> Path:
    existing = phrase_dll_candidates(repo)
    if existing:
        return max(existing, key=lambda path: path.stat().st_mtime)
    project = repo / "src" / "Phrase.Cli" / "Phrase.Cli.csproj"
    built = run_process(
        ["dotnet", "build", str(project), "--nologo", "--disable-build-servers"],
        repo,
        env,
        timeout=180,
    )
    if built.returncode != 0:
        raise RuntimeError("phrase CLI did not build:\n" + built.stdout + built.stderr)
    existing = phrase_dll_candidates(repo)
    if existing:
        return max(existing, key=lambda path: path.stat().st_mtime)
    raise FileNotFoundError("phrase.dll is missing after build")


def find_csproj(root: Path, suffix: str) -> Path | None:
    matches = list(root.rglob(f"*{suffix}"))
    return matches[0] if matches else None


def classify_reply(source: str, repo: Path, work: Path, env: dict[str, str]) -> dict:
    specs = count_specs(source)
    if not contains_phrase(source):
        return {
            "passed": False,
            "reason": REASON_COMPILE,
            "proof_level": "none",
            "specs": specs,
            "specs_passed": 0,
            "build_warnings": 0,
        }

    generated = work / "generated"
    generated.mkdir(parents=True, exist_ok=True)
    source_path = work / "reply.phrase"
    source_path.write_text(source + ("\n" if not source.endswith("\n") else ""), encoding="utf-8")
    dll = find_phrase_dll(repo, env)
    compiled = run_process(
        ["dotnet", str(dll), "build", str(source_path), "-o", str(generated)],
        repo,
        env,
        timeout=120,
    )
    if compiled.returncode != 0:
        return {
            "passed": False,
            "reason": REASON_COMPILE,
            "proof_level": "none",
            "specs": specs,
            "specs_passed": 0,
            "build_warnings": 0,
        }

    api = find_csproj(generated, ".Api.csproj")
    specs_project = find_csproj(generated, ".Specs.csproj")
    if api is None or specs_project is None:
        return {
            "passed": False,
            "reason": REASON_BUILD,
            "proof_level": "none",
            "specs": specs,
            "specs_passed": 0,
            "build_warnings": 0,
        }

    built = run_process(
        ["dotnet", "build", str(api), "--nologo", "--disable-build-servers", "-v", "m"],
        api.parent,
        env,
        timeout=180,
    )
    warning_match = WARNING_COUNT.search(built.stdout + built.stderr)
    warnings = int(warning_match.group(1)) if warning_match else (0 if built.returncode == 0 else 1)
    if built.returncode != 0 or warnings > 0:
        return {
            "passed": False,
            "reason": REASON_BUILD,
            "proof_level": "none",
            "specs": specs,
            "specs_passed": 0,
            "build_warnings": warnings,
        }

    tested = run_process(
        [
            "dotnet",
            "test",
            str(specs_project),
            "--nologo",
            "--disable-build-servers",
            "--verbosity",
            "quiet",
        ],
        specs_project.parent,
        env,
        timeout=300,
    )
    return classify_test_run(specs, warnings, tested.stdout + tested.stderr, tested.returncode)


def parse_dotnet_test_counts(log: str) -> tuple[int | None, int | None]:
    quiet = QUIET_SUMMARY.search(log)
    if quiet:
        return int(quiet.group(4)), int(quiet.group(1))
    total_tests = TOTAL_TESTS_LINE.search(log)
    if total_tests:
        total = int(total_tests.group(1))
        failed_match = FAILED_INDENTED.search(log) or FAILED_TESTS.search(log)
        failed = int(failed_match.group(1)) if failed_match else None
        return total, failed
    total_match = TOTAL_TESTS.search(log)
    failed_match = FAILED_TESTS.search(log)
    total = int(total_match.group(1)) if total_match else None
    failed = int(failed_match.group(1)) if failed_match else None
    return total, failed


def classify_test_run(specs: int, warnings: int, log: str, returncode: int) -> dict:
    total, failed = parse_dotnet_test_counts(log)
    if total is None or total == 0:
        return {
            "passed": False,
            "reason": REASON_SPEC,
            "proof_level": "none",
            "specs": specs,
            "specs_passed": 0,
            "build_warnings": warnings,
        }
    if failed is None:
        failed = 0 if returncode == 0 else max(specs, 1)
    passed_count = max(total - failed, 0)
    if returncode != 0 or failed > 0:
        return {
            "passed": False,
            "reason": REASON_SPEC,
            "proof_level": "none",
            "specs": max(specs, total),
            "specs_passed": passed_count,
            "build_warnings": warnings,
        }
    return {
        "passed": True,
        "proof_level": "specs-passed",
        "specs": max(specs, total),
        "specs_passed": passed_count,
        "build_warnings": warnings,
    }


def read_bench_row(results_dir: Path) -> dict:
    history = results_dir / "history.jsonl"
    if not history.is_file():
        for item in results_dir.glob("*.json"):
            if item.name == "history.jsonl":
                continue
            return json.loads(item.read_text(encoding="utf-8"))
        return {}
    lines = [line for line in history.read_text(encoding="utf-8").splitlines() if line.strip()]
    return json.loads(lines[-1]) if lines else {}


def bench_failure_reason(bench_row: dict) -> str | None:
    if not bench_row:
        return REASON_ERROR
    status = str(bench_row.get("status") or "").lower()
    if status != "unavailable":
        return None
    error = str(bench_row.get("error") or "")
    if "timed out" in error.lower():
        return REASON_TIMEOUT
    return REASON_ERROR


def totals_document(
    model: str,
    grammar_sha: str,
    sheet_sha: str,
    bench_row: dict,
    classified: dict,
    outcome_passed: bool,
) -> dict:
    passed = bool(classified.get("passed")) and outcome_passed
    reason = classified.get("reason")
    bench_reason = bench_failure_reason(bench_row)
    if bench_reason:
        passed = False
        reason = bench_reason
    elif classified.get("passed") and not outcome_passed:
        reason = REASON_OUTCOME
        passed = False
    document = {
        "model": model,
        "date": datetime.now(timezone.utc).date().isoformat(),
        "attempts": int(bench_row.get("attempts") or 1),
        "tokens_in": int(bench_row.get("tokens_in") or 0),
        "tokens_out": int(bench_row.get("tokens_out") or 0),
        "passed": passed,
        "proof_level": "specs-passed" if passed else "none",
        "specs": int(classified.get("specs") or 0),
        "specs_passed": int(classified.get("specs_passed") or 0),
        "build_warnings": int(classified.get("build_warnings") or 0),
        "grammar_sha256": grammar_sha,
        "cheatsheet_sha256": sheet_sha,
    }
    if not passed:
        document["reason"] = reason or REASON_COMPILE
    leaked = FORBIDDEN_JSON_KEYS.intersection(document)
    if leaked:
        raise RuntimeError("totals json contained forbidden keys: " + ", ".join(sorted(leaked)))
    return document


def write_json(path: Path, document: dict) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(document, indent=2, sort_keys=False) + "\n", encoding="utf-8")


def run_check(args: argparse.Namespace) -> int:
    repo = repo_root(args.repo)
    model = args.model
    env = prepare_dotnet_env(os.environ.copy())
    out_path = Path(args.out).resolve() if args.out else repo / "docs" / "usable-v1" / "llm-check.json"
    grammar_sha = sha256_file(repo / "docs" / "GRAMMAR.md")
    sheet_sha = sha256_file(repo / "docs" / "CHEATSHEET.md")

    def write_failure(reason: str, bench_row: dict | None = None, classified: dict | None = None) -> int:
        document = totals_document(
            model,
            grammar_sha,
            sheet_sha,
            bench_row or {},
            classified or {"passed": False, "reason": reason, "specs": 0, "specs_passed": 0, "build_warnings": 0},
            False,
        )
        document["reason"] = reason
        document["passed"] = False
        document["proof_level"] = "none"
        write_json(out_path, document)
        return 1

    try:
        with tempfile.TemporaryDirectory(prefix="phrase-llm-check-") as raw:
            work = Path(raw)
            suite = prepare_suite(repo, work)
            docs_root, grammar_sha, sheet_sha = prepare_docs(repo, work)
            command = args.model_command or default_model_command(model)
            wrapped = write_capture_wrapper(work, command)
            private_results = work / "private" / "history.jsonl"
            private_results.parent.mkdir(parents=True, exist_ok=True)
            bench_project = repo / "tools" / "Phrase.Bench" / "Phrase.Bench.csproj"
            argv = [
                "dotnet",
                "run",
                "--disable-build-servers",
                "--project",
                str(bench_project),
                "--",
                "run",
                "--suite",
                str(suite),
                "--model",
                model,
                "--model-command",
                wrapped,
                "--cold",
                "--scoring",
                "executed",
                "--mode",
                "docs-in-context",
                "--docs-root",
                str(docs_root),
                "--custom-docs",
                "--private-results",
                str(private_results),
                "--max-attempts",
                "1",
            ]
            try:
                bench = run_process(argv, repo, env, timeout=args.timeout)
            except TimeoutError:
                return write_failure(REASON_TIMEOUT)
            capture = work / "reply.txt"
            reply = capture.read_text(encoding="utf-8", errors="replace") if capture.is_file() else ""
            if not contains_phrase(extract_phrase(reply)):
                reply = bench.stdout or reply
            source = extract_phrase(reply)
            classified = classify_reply(source, repo, work, env)
            bench_row = read_bench_row(work / "bench" / "results")
            if not bench_row and private_results.is_file():
                lines = [line for line in private_results.read_text(encoding="utf-8").splitlines() if line.strip()]
                bench_row = json.loads(lines[-1]) if lines else {}
            bench_reason = bench_failure_reason(bench_row)
            if bench_reason:
                return write_failure(bench_reason, bench_row, classified)
            outcome_passed = classified.get("passed") is True and bench.returncode == 0
            if classified.get("reason") in {REASON_COMPILE, REASON_BUILD, REASON_SPEC}:
                outcome_passed = classified.get("passed") is True
            document = totals_document(model, grammar_sha, sheet_sha, bench_row, classified, outcome_passed)
            write_json(out_path, document)
            return 0 if document["passed"] else 1
    except TimeoutError:
        return write_failure(REASON_TIMEOUT)
    except Exception:
        return write_failure(REASON_ERROR)


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Run the Usable v1 LLM-customer check.")
    parser.add_argument("--model", required=True, choices=("codex", "grok"))
    parser.add_argument("--model-command", dest="model_command", default=None)
    parser.add_argument("--out", default=None)
    parser.add_argument("--repo", default=None)
    parser.add_argument("--timeout", type=int, default=600)
    return parser.parse_args(argv)


def main(argv: list[str]) -> int:
    try:
        args = parse_args(argv)
    except SystemExit:
        raise
    except Exception as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 2
    try:
        return run_check(args)
    except TimeoutError as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 1
    except Exception as exc:  # pragma: no cover - CLI failure path
        print(f"error: {exc}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
