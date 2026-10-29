import importlib.util
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location(
    "check_extension_map",
    ROOT / "scripts" / "check-extension-map.py",
)
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)


def passing_text():
    rows = "\n".join(
        f"| {token} | `docs/GRAMMAR.md` line 1 | web extension |"
        for token in check.AUDIT_TOKENS
    )
    domains = "\n".join(
        f"| {name} | NOT BUILT | waits for a real use |"
        for name in check.CANDIDATE_DOMAINS
    )
    return f"""# ADR: Extension map

## Core

The core is domain-neutral. It holds avows, specs with Given/When/Then, types, control and error patterns, effects as a mechanism, and failure codes.

## Extension

An extension is a library with its own cheat sheet or index, loaded only when a project uses it, with its own keywords, diagnostics and adapter-side emitters. A target-language adapter is orthogonal to a domain extension. A C# adapter serves every domain extension.

## Testing split

Phrase's own specs and Given/When/Then are core. Test-framework specifics (xUnit and NUnit wiring, mocking, fixtures) and the adapter side of spec-to-test generation are extension territory.

## Audit

| Item | Where it lives today | Proposed home |
|---|---|---|
{rows}

## Candidate domains

Another domain extension is built only when a real use needs it, and only web is built now (card 142).

| Domain | Status | Note |
|---|---|---|
{domains}

## How a project uses an extension

The web extension loads implicitly when a file uses an endpoint, a verb or a route, or from an explicit uses web line. Loading is deterministic.
"""


class ExtensionMapTests(unittest.TestCase):
    def test_minimal_document_passes(self):
        self.assertEqual(check.check_text(passing_text(), ROOT), [])

    def test_missing_section_fails(self):
        text = passing_text().replace("## Testing split\n", "", 1)
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("## Testing split" in problem for problem in problems))

    def test_audit_row_missing_file_fails(self):
        text = passing_text().replace("`docs/GRAMMAR.md`", "`docs/no-such-audit-target.md`", 1)
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("docs/no-such-audit-target.md" in problem for problem in problems))

    def test_candidate_domain_not_marked_not_built_fails(self):
        text = passing_text().replace("| web/HTTP | NOT BUILT |", "| web/HTTP | BUILT |", 1)
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("web/HTTP" in problem and "NOT BUILT" in problem for problem in problems))

    def test_real_adr_passes(self):
        problems = check.check_path(check.ADR, ROOT)
        self.assertEqual(problems, [])


if __name__ == "__main__":
    unittest.main()
