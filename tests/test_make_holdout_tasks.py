import json
import importlib.util
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / "scripts" / "make-holdout-tasks.py"


def load_converter():
    spec = importlib.util.spec_from_file_location("make_holdout_tasks", SCRIPT)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class MakeHoldoutTasksTests(unittest.TestCase):
    def make_holdout(self, kind: str = "modify") -> str:
        if kind == "modify":
            return """<!-- nonce: TEST-NONCE -->
# Holdout task H1: endpoint greeting

## Header (exactly these keys, one per line)

- **id:** H1
- **kind:** modify
- **fits_no_pattern:** false
- **why_it_fits_no_pattern:** n/a
- **libraries:** ASP.NET Core minimal API

## The task, in plain words (neutral)
Update the greeting endpoint so it accepts a `name` and returns a friendly greeting; this deliberately long prose line verifies that neutral task text is preserved without depending on signatures, class names, or language-specific syntax hints.

Blank names are invalid, and callers must receive the failure code described below.

## Observable outcomes (the cases both arms are judged by)

Every case is observed by the caller, including the failure path.

```json
[
  { "name": "greets Ada", "given": { "name": "Ada" }, "when": "called", "then": { "returns": "hello Ada" } },
  { "name": "rejects blank name", "given": { "name": "" }, "when": "called", "then": { "fails_with": "MissingName" } }
]
```

## Starting code (modify tasks only; delete this section for write tasks)

### start.phrase
```phrase
module Greeter

endpoint Greet : GET /greet
  input
    name: Text
  output
    message: Text
  steps
    respond with { message: "hello " + input.name }
```

### start.cs
```csharp
namespace Greeter;

public static class GreeterEndpoint
{
    public static string Greet(string name) => "hello " + name;
}
```

### The change requested
Change the endpoint to return an enthusiastic friendly greeting and reject blank values with `MissingName`.

The outcomes above are the complete behavior expected after the change.

## Extra material that is not part of starting code

This trailing section must not leak into the requested change.
"""
        return """<!-- nonce: TEST-NONCE -->
# Holdout task H1: endpoint greeting

## Header (exactly these keys, one per line)

- **id:** H1
- **kind:** write
- **fits_no_pattern:** false
- **why_it_fits_no_pattern:** n/a
- **libraries:** ASP.NET Core minimal API

## The task, in plain words (neutral)
Write a tiny greeting endpoint that accepts a `name` and returns a friendly greeting; this deliberately long prose line verifies that neutral task text is preserved without depending on signatures, class names, or language-specific syntax hints.

## Observable outcomes (the cases both arms are judged by)

Every case is observed by the caller, including the failure path.

```json
[
  { "name": "greets Ada", "given": { "name": "Ada" }, "when": "called", "then": { "returns": "hello Ada" } },
  { "name": "rejects blank name", "given": { "name": "" }, "when": "called", "then": { "fails_with": "MissingName" } }
]
```
"""

    def test_generates_matching_task_pairs(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            holdout = root / "H1-modify.md"
            holdout.write_text(self.make_holdout("modify"), encoding="utf-8")
            out_dir = root / "generated"

            result = subprocess.run(
                [sys.executable, str(SCRIPT), str(holdout), str(out_dir)],
                cwd=str(ROOT),
                capture_output=True,
                text=True,
            )

            self.assertEqual(0, result.returncode, result.stderr or result.stdout)
            phrase_dir = out_dir / "phrase" / "tasks" / "H1"
            csharp_dir = out_dir / "csharp" / "tasks" / "H1"
            self.assertTrue((phrase_dir / "prompt.md").exists())
            self.assertTrue((csharp_dir / "prompt.md").exists())
            self.assertTrue((phrase_dir / "outcomes.json").exists())
            self.assertTrue((csharp_dir / "outcomes.json").exists())
            self.assertTrue((phrase_dir / "start.phrase").exists())
            self.assertTrue((csharp_dir / "start.cs").exists())

            phrase_prompt = (phrase_dir / "prompt.md").read_text(encoding="utf-8")
            csharp_prompt = (csharp_dir / "prompt.md").read_text(encoding="utf-8")
            self.assertIn("Libraries: ASP.NET Core minimal API", phrase_prompt)
            self.assertIn("Libraries: ASP.NET Core minimal API", csharp_prompt)
            self.assertIn('"greets Ada"', phrase_prompt)
            self.assertIn('"greets Ada"', csharp_prompt)
            self.assertIn("Use the provided Phrase documentation. Return only valid Phrase source.", phrase_prompt)
            self.assertNotIn("provided Phrase documentation", csharp_prompt)
            self.assertIn("Return only valid C# source.", csharp_prompt)
            self.assertNotEqual(phrase_prompt, csharp_prompt)
            self.assertIn("module Greeter", phrase_prompt)
            self.assertIn("public static class GreeterEndpoint", csharp_prompt)
            self.assertIn("enthusiastic friendly greeting", phrase_prompt)
            self.assertIn("enthusiastic friendly greeting", csharp_prompt)
            self.assertNotIn("must not leak", phrase_prompt)
            self.assertNotIn("must not leak", csharp_prompt)

            phrase_suite = json.loads((out_dir / "phrase" / "suite.json").read_text(encoding="utf-8"))
            csharp_suite = json.loads((out_dir / "csharp" / "suite.json").read_text(encoding="utf-8"))
            self.assertEqual(False, phrase_suite["tasks"][0]["holdout"])
            self.assertEqual(False, csharp_suite["tasks"][0]["holdout"])
            committed_phrase = json.loads((ROOT / "bench" / "suite.json").read_text(encoding="utf-8"))
            committed_csharp = json.loads((ROOT / "bench" / "csharp-baseline" / "suite.json").read_text(encoding="utf-8"))
            self.assertEqual(set(committed_phrase["tasks"][0]), set(phrase_suite["tasks"][0]))
            self.assertEqual(set(committed_csharp["tasks"][0]), set(csharp_suite["tasks"][0]))

            phrase_outcome_bytes = (phrase_dir / "outcomes.json").read_bytes()
            csharp_outcome_bytes = (csharp_dir / "outcomes.json").read_bytes()
            self.assertEqual(phrase_outcome_bytes, csharp_outcome_bytes)
            phrase_outcomes = json.loads(phrase_outcome_bytes)
            csharp_outcomes = json.loads(csharp_outcome_bytes)
            self.assertEqual(phrase_outcomes, csharp_outcomes)
            self.assertNotIn("TEST-NONCE", result.stdout + result.stderr)
            self.assertNotIn("enthusiastic friendly greeting", result.stdout + result.stderr)

    def test_write_and_modify_prompts_differ_only_by_declared_language_wrapper(self):
        # Given Riley's invented faithful-layout write and modify holdouts
        converter = load_converter()
        for kind in ("write", "modify"):
            with self.subTest(kind=kind), tempfile.TemporaryDirectory() as tmp:
                root = Path(tmp)
                holdout = root / f"H1-{kind}.md"
                holdout.write_text(self.make_holdout(kind), encoding="utf-8")
                out_dir = root / "generated"
                result = subprocess.run(
                    [sys.executable, str(SCRIPT), str(holdout), str(out_dir)],
                    cwd=str(ROOT),
                    capture_output=True,
                    text=True,
                )
                self.assertEqual(0, result.returncode, result.stderr or result.stdout)

                # When only the explicitly declared wrapper lines and matching start fence are stripped
                phrase_prompt = (out_dir / "phrase" / "tasks" / "H1" / "prompt.md").read_text(encoding="utf-8")
                csharp_prompt = (out_dir / "csharp" / "tasks" / "H1" / "prompt.md").read_text(encoding="utf-8")
                phrase_remainder = converter.strip_language_wrapper(phrase_prompt, converter.PHRASE_WRAPPER_LINES)
                csharp_remainder = converter.strip_language_wrapper(csharp_prompt, converter.CSHARP_WRAPPER_LINES)

                # Then no unlisted arm-specific wording, ordering, or whitespace remains
                self.assertEqual(phrase_remainder, csharp_remainder)

    def test_refuses_repo_internal_output_path(self):
        repo_root = ROOT
        probe = repo_root / "bench" / "holdout-refusal-test"
        self.assertFalse(probe.exists(), "refusal probe must start absent")
        holdout = repo_root / "bench" / "holdout-refusal.md"
        try:
            holdout.write_text(self.make_holdout("write"), encoding="utf-8")
            out_dir = probe
            result = subprocess.run(
                [sys.executable, str(SCRIPT), str(holdout), str(out_dir)],
                cwd=str(ROOT),
                capture_output=True,
                text=True,
            )
            self.assertNotEqual(0, result.returncode)
            self.assertFalse((out_dir / "phrase").exists())
            self.assertFalse((out_dir / "csharp").exists())
        finally:
            if holdout.exists():
                holdout.unlink()
            self.assertFalse(probe.exists())


if __name__ == "__main__":
    unittest.main()
