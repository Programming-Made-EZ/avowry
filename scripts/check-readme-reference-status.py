#!/usr/bin/env python3
"""Fail when README's reference-target status disagrees with a fresh test run."""

import pathlib
import re
import subprocess
import sys


TEST_NAME = "ReferenceTarget.Reference_output_builds_and_every_spec_passes"
STATUS_PATTERN = re.compile(
    rf"(?m)^\|\s*`{re.escape(TEST_NAME)}`\s*\|\s*([^|]+?)\s*\|"
)


def read_status(readme_path):
    text = pathlib.Path(readme_path).read_text(encoding="utf-8")
    match = STATUS_PATTERN.search(text)
    return match.group(1).strip() if match else None


def run_reference_test(root):
    result = subprocess.run(
        [
            "dotnet",
            "test",
            str(root / "tests" / "Phrase.Acceptance.Tests"),
            "--filter",
            f"FullyQualifiedName~{TEST_NAME}",
            "--nologo",
            "--disable-build-servers",
        ],
        cwd=root,
        text=True,
    )
    return result.returncode == 0


def check_status(readme_path, runner):
    status = read_status(readme_path)
    if status not in ("Green", "Failing"):
        return f"reference status must be Green or Failing, found {status or 'no row'}"
    actual = "Green" if runner() else "Failing"
    if status != actual:
        return f"README says {status}, but the fresh reference test is {actual}; status disagrees"
    return None


def main():
    root = pathlib.Path(__file__).resolve().parents[1]
    problem = check_status(root / "README.md", lambda: run_reference_test(root))
    if problem:
        print(problem)
        return 1
    print(f"README and fresh {TEST_NAME} run agree")
    return 0


if __name__ == "__main__":
    sys.exit(main())
