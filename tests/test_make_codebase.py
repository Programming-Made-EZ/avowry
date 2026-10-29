import ast
import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / "scripts" / "make-codebase.py"

STDLIB = {
    "__future__",
    "argparse",
    "ast",
    "json",
    "random",
    "shutil",
    "subprocess",
    "sys",
    "pathlib",
    "dataclasses",
    "typing",
}

ALLOWED_THEN = {"returns", "fails_with", "succeeds", "responds", "status"}


def size_class(text: str) -> str:
    # One changed file stays in the small class. The joined module at size 50 does not.
    return "one-file" if len(text.splitlines()) <= 150 else "whole-codebase"


def line_ratio(left: str, right: str) -> float:
    counts = (len(left.splitlines()), len(right.splitlines()))
    return max(counts) / min(counts)


def run(size, out, seed=1, cwd=None, with_seed=True):
    cmd = [sys.executable, str(SCRIPT), str(size), str(out)]
    if with_seed:
        cmd.extend(["--seed", str(seed)])
    return subprocess.run(
        cmd,
        cwd=str(cwd or ROOT),
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )


def tree(root: Path) -> dict[str, bytes]:
    files = {}
    for path in root.rglob("*"):
        if path.is_file():
            files[path.relative_to(root).as_posix()] = path.read_bytes()
    return files


class MakeCodebaseTests(unittest.TestCase):
    def test_plain_csharp_uses_no_phrase_vocabulary(self):
        # Given Riley generates each study size as an ordinary C# codebase
        with tempfile.TemporaryDirectory() as tmp:
            for size in (1, 10, 50):
                out = Path(tmp) / str(size)
                self.assertEqual(0, run(size, out, seed=130).returncode, size)

                # When every plain C# file name and its contents are inspected
                occurrences = []
                for path in out.rglob("*"):
                    if not path.is_file() or not (
                        path.is_relative_to(out / "csharp") or path.suffix == ".cs"
                    ):
                        continue
                    relative = path.relative_to(out).as_posix()
                    if "phrase" in relative.lower():
                        occurrences.append(relative)
                    if "phrase" in path.read_text(encoding="utf-8").lower():
                        occurrences.append(f"{relative} (contents)")

                # Then no generated C# name, type, attribute, namespace or comment uses it
                self.assertEqual([], occurrences, f"size {size}: {occurrences}")

    def test_imports_are_stdlib_only(self):
        # Given the generator source
        tree_ast = ast.parse(SCRIPT.read_text(encoding="utf-8"))
        modules = set()
        for node in ast.walk(tree_ast):
            if isinstance(node, ast.Import):
                modules.update(alias.name.split(".")[0] for alias in node.names)
            elif isinstance(node, ast.ImportFrom) and node.module:
                modules.add(node.module.split(".")[0])

        # When its imports are collected
        # Then it depends only on the Python 3 standard library
        self.assertLessEqual(modules, STDLIB)

    def test_same_size_and_seed_are_byte_identical(self):
        # Given two empty output directories and one seed
        with tempfile.TemporaryDirectory() as tmp:
            # When the generator runs twice for each study size
            for size in (1, 10, 50):
                first = Path(tmp) / f"a{size}"
                second = Path(tmp) / f"b{size}"
                self.assertEqual(0, run(size, first, seed=4).returncode)
                self.assertEqual(0, run(size, second, seed=4).returncode)

                # Then every output byte matches
                self.assertEqual(tree(first), tree(second))

    def test_different_seeds_change_the_module(self):
        # Given two seeds
        with tempfile.TemporaryDirectory() as tmp:
            first = Path(tmp) / "a"
            second = Path(tmp) / "b"
            self.assertEqual(0, run(1, first, seed=1).returncode)
            self.assertEqual(0, run(1, second, seed=2).returncode)

            # When the Phrase modules are read
            # Then the seed changed the generated source
            left = (first / "phrase" / "shelf.avowry").read_bytes()
            right = (second / "phrase" / "shelf.avowry").read_bytes()
            self.assertNotEqual(left, right)

    def test_default_seed_matches_seed_one(self):
        with tempfile.TemporaryDirectory() as tmp:
            implied = Path(tmp) / "implied"
            explicit = Path(tmp) / "explicit"
            self.assertEqual(0, run(1, implied, with_seed=False).returncode)
            self.assertEqual(0, run(1, explicit, seed=1).returncode)
            self.assertEqual(tree(implied), tree(explicit))

    def test_sizes_write_shared_types_leaves_and_endpoint_counts(self):
        # Given the three study sizes
        with tempfile.TemporaryDirectory() as tmp:
            for size in (1, 10, 50):
                out = Path(tmp) / str(size)
                result = run(size, out, seed=3)
                self.assertEqual(0, result.returncode, result.stderr)

                # When the Phrase module and the plain C# project are read
                shelf = (out / "phrase" / "shelf.avowry").read_text(encoding="utf-8")
                leaves = (out / "phrase" / "leaves.avowry").read_text(encoding="utf-8")
                self.assertTrue(shelf.startswith("module Shelf\n"))
                self.assertNotIn("module Orders", shelf)
                self.assertIn("type Copy\n", shelf)
                self.assertIn("type Loan\n", shelf)
                self.assertIn("type Shelf\n", shelf)
                self.assertIn("copies: List<Copy>", shelf)
                self.assertIn(leaves.strip("\n"), shelf)
                endpoints = [line for line in shelf.splitlines() if line.startswith("endpoint ")]
                self.assertEqual(size, len(endpoints))
                self.assertEqual(
                    size,
                    shelf.count("ensure input.loan.days is a loan of at most three weeks"),
                )
                if size >= 10:
                    self.assertGreaterEqual(
                        shelf.count("ensure input.loan.days is a loan of at most three weeks"),
                        2,
                    )

                api = out / "csharp" / "Shelf.Api"
                specs = out / "csharp" / "Shelf.Specs"
                self.assertTrue((api / "Shelf.Api.csproj").is_file())
                self.assertTrue((specs / "Shelf.Specs.csproj").is_file())
                self.assertTrue((api / "Program.cs").is_file())
                self.assertTrue((api / "LoanRuleException.cs").is_file())
                self.assertTrue((api / "LoanRules.cs").is_file())
                endpoint_files = [
                    path for path in (out / "phrase" / "endpoints").glob("*.avowry")
                ]
                self.assertEqual(size, len(endpoint_files))
                csharp_endpoints = [
                    path.name
                    for path in api.glob("*.cs")
                    if path.name not in {"Program.cs", "LoanRuleException.cs", "DomainModels.cs", "LoanRules.cs"}
                ]
                self.assertEqual(size, len(csharp_endpoints))
                facts = sum(
                    path.read_text(encoding="utf-8").count("[Fact]")
                    for path in specs.rglob("*.cs")
                )
                self.assertEqual(shelf.count('  spec "'), facts)

    def test_refuses_unignored_path_inside_the_repository(self):
        # Given an output directory inside this repository that is not git-ignored
        out = ROOT / "not-ignored-codebase-out"
        self.assertFalse(out.exists())
        result = run(1, out, seed=1)

        # When the generator is asked to write there
        # Then it refuses and leaves the repository untouched
        self.assertNotEqual(0, result.returncode)
        self.assertIn("not git-ignored", result.stderr)
        self.assertFalse(out.exists())

    def test_gitignored_directory_inside_a_repo_is_writable(self):
        # Given a throwaway repository whose gitignore covers the output path
        with tempfile.TemporaryDirectory() as tmp:
            repo = Path(tmp)
            init = subprocess.run(["git", "init"], cwd=repo, capture_output=True, text=True)
            self.assertEqual(0, init.returncode, init.stderr)
            (repo / ".gitignore").write_text("generated/\n", encoding="utf-8")
            ignored = repo / "generated" / "desk"
            visible = repo / "visible"

            # When the generator writes to the ignored path and to a visible path
            allowed = run(1, ignored, seed=1, cwd=repo)
            refused = run(1, visible, seed=1, cwd=repo)

            # Then only the git-ignored path is written
            self.assertEqual(0, allowed.returncode, allowed.stderr)
            self.assertTrue((ignored / "phrase" / "shelf.avowry").is_file())
            self.assertNotEqual(0, refused.returncode)
            self.assertIn("not git-ignored", refused.stderr)
            self.assertFalse((visible / "phrase").exists())

    def test_rejects_size_outside_the_study(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp) / "nope"
            result = run(2, out, seed=1)
            self.assertNotEqual(0, result.returncode)
            self.assertIn("size must be 1, 10 or 50", result.stderr)
            self.assertFalse((out / "phrase").exists())

    def test_tasks_need_another_file_and_share_a_size_class(self):
        # Given a generated codebase
        with tempfile.TemporaryDirectory() as tmp:
            for size in (1, 10, 50):
                out = Path(tmp) / str(size)
                self.assertEqual(0, run(size, out, seed=8).returncode, size)
                self.assert_modify_needs_the_endpoint(out, size)
                self.assert_fix_needs_the_leaf(out)

    def assert_modify_needs_the_endpoint(self, out: Path, size: int):
        # The loan-limit task changes the shared leaf. The posted limit lives on the endpoint.
        task = out / "tasks" / "modify-loan-limit"
        prompt = (task / "prompt.md").read_text(encoding="utf-8")
        phrase_start = (task / "phrase" / "start.avowry").read_text(encoding="utf-8")
        csharp_start = (task / "csharp" / "start.cs").read_text(encoding="utf-8")
        leaf = (out / "phrase" / "leaves.avowry").read_text(encoding="utf-8")
        rules = (out / "csharp" / "Shelf.Api" / "LoanRules.cs").read_text(encoding="utf-8")
        endpoint = (out / "phrase" / "endpoints" / "PlaceHoldNorth.avowry").read_text(encoding="utf-8")
        endpoint_cs = (out / "csharp" / "Shelf.Api" / "PlaceHoldNorth.cs").read_text(encoding="utf-8")

        self.assertIn("Changed phrase file: phrase/leaves.avowry", prompt)
        self.assertIn("Needs phrase file: phrase/endpoints/PlaceHoldNorth.avowry", prompt)
        self.assertIn("Needs csharp file: csharp/Shelf.Api/PlaceHoldNorth.cs", prompt)
        self.assertNotIn("posts its loan limit", prompt)
        self.assertNotIn("posts its loan limit", phrase_start)
        self.assertIn("posts its loan limit as 14 days", endpoint)
        self.assertIn("posts its loan limit as 14 days", endpoint_cs)
        self.assertEqual(phrase_start, leaf)
        self.assertEqual(csharp_start, rules)
        # A 3x line-count ratio keeps both changed files in the same practical size class.
        self.assertLessEqual(line_ratio(phrase_start, csharp_start), 3.0)
        self.assertEqual(size_class(phrase_start), size_class(csharp_start))
        self.assertEqual("one-file", size_class(phrase_start))
        if size == 50:
            shelf = (out / "phrase" / "shelf.avowry").read_text(encoding="utf-8")
            self.assertEqual("whole-codebase", size_class(shelf))
        self.assert_shared_outcomes(task, prompt)

    def assert_fix_needs_the_leaf(self, out: Path):
        # The fix task changes PlaceHoldNorth. The phrase wording lives only in the shared leaf.
        task = out / "tasks" / "fix-place-hold"
        prompt = (task / "prompt.md").read_text(encoding="utf-8")
        phrase_start = (task / "phrase" / "start.avowry").read_text(encoding="utf-8")
        csharp_start = (task / "csharp" / "start.cs").read_text(encoding="utf-8")
        leaf = (out / "phrase" / "leaves.avowry").read_text(encoding="utf-8")
        rules = (out / "csharp" / "Shelf.Api" / "LoanRules.cs").read_text(encoding="utf-8")
        endpoint = (out / "phrase" / "endpoints" / "PlaceHoldNorth.avowry").read_text(encoding="utf-8")
        endpoint_cs = (out / "csharp" / "Shelf.Api" / "PlaceHoldNorth.cs").read_text(encoding="utf-8")
        call = "is a loan of at most three weeks"

        self.assertIn("Changed phrase file: phrase/endpoints/PlaceHoldNorth.avowry", prompt)
        self.assertIn("Needs phrase file: phrase/leaves.avowry", prompt)
        self.assertIn("Needs csharp file: csharp/Shelf.Api/LoanRules.cs", prompt)
        self.assertNotIn(call, prompt)
        self.assertNotIn(call, phrase_start)
        self.assertNotIn("EnsureDaysIsALoanOfAtMostThreeWeeks", csharp_start)
        self.assertIn(call, leaf)
        self.assertIn("EnsureDaysIsALoanOfAtMostThreeWeeks", rules)
        self.assertIn(call, endpoint)
        self.assertIn("EnsureDaysIsALoanOfAtMostThreeWeeks", endpoint_cs)
        self.assertNotEqual(phrase_start, endpoint)
        self.assertNotEqual(csharp_start, endpoint_cs)
        self.assertEqual(size_class(phrase_start), size_class(csharp_start))
        self.assertEqual(size_class(phrase_start), size_class(endpoint))
        self.assertEqual(size_class(csharp_start), size_class(endpoint_cs))
        self.assertEqual("one-file", size_class(phrase_start))
        # Endpoint implementations may be more ceremonial in C#, but no more than 3x.
        self.assertLessEqual(line_ratio(phrase_start, csharp_start), 3.0)
        self.assert_shared_outcomes(task, prompt)

    def assert_shared_outcomes(self, task: Path, prompt: str):
        outcomes_path = task / "outcomes.json"
        self.assertEqual([outcomes_path], list(task.rglob("outcomes.json")))
        body = outcomes_path.read_text(encoding="utf-8")
        self.assertIn(body.strip("\n"), prompt)
        cases = json.loads(body)
        self.assertGreaterEqual(len(cases), 2)
        for case in cases:
            self.assertEqual({"name", "given", "when", "then"}, set(case))
            self.assertIn(case["when"], {"called", "handled"})
            then = case["then"]
            self.assertLessEqual(set(then), ALLOWED_THEN)
            kinds = set(then) - {"status"}
            self.assertEqual(1, len(kinds))
            if "responds" in then:
                self.assertIsInstance(then["status"], int)
            else:
                self.assertNotIn("status", then)

    def test_outcome_cases_use_only_kinds_both_arms_score(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp) / "out"
            self.assertEqual(0, run(1, out, seed=1).returncode)
            seen = set()
            for path in (out / "tasks").rglob("outcomes.json"):
                for case in json.loads(path.read_text(encoding="utf-8")):
                    seen.update(set(case["then"]) - {"status"})
            self.assertLessEqual(seen, {"returns", "fails_with", "succeeds", "responds"})
            self.assertIn("fails_with", seen)
            self.assertIn("succeeds", seen)
            self.assertIn("responds", seen)


if __name__ == "__main__":
    unittest.main()
