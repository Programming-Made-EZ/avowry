import importlib.util
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location(
    "check_library_binding_layering",
    ROOT / "scripts" / "check-library-binding-layering.py",
)
check = importlib.util.module_from_spec(spec)
spec.loader.exec_module(check)


def passing_text():
    places = "\n".join(
        f"| {place} | `src/Phrase.Adapters.CSharp/CSharpProvider.cs` line 1; "
        f"`tools/Phrase.BenchCs/Program.cs` line 1; "
        f"`tools/Phrase.Bench/Program.cs` line 1 | the HTTP binding seam |"
        for place in check.AUDIT_PLACES
    )
    return f"""# ADR: Library bindings and project profile

## Layers

A domain extension defines abstract concepts: endpoint, route and store.
A library binding under each adapter maps them to a concrete library:
web plus C# to FastEndpoints by default, Minimal APIs or controllers as possible;
data to EF Core or Dapper as possible.
A project profile file selects the bindings, the architecture style
(vertical slices, clean or onion) and the coding standards
(naming, nullable, analyzers, file layout, test framework),
with smart defaults and an override.
The mapping is deterministic: emitters and an LLM-as-compiler both honour it.
The same spec and generated tests verify every binding.

## Logic sources, in order

1. Library pattern.
2. Adapter idiom.
3. LLM-generated body.
4. Native escape hatch.

The share of logic still in native blocks is tracked as a trade-off signal, not a target.

## Audit of FastEndpoints in the C# emitter

| Place | Where it lives today | Seam it needs |
|---|---|---|
{places}

Other mentions, no seam needed: `scripts/check-extension-map.py`.

## Hard-wire rule

The prototype path may keep today's FastEndpoints default.
Later cards may not hard-wire a second library into specs.

## Second binding

A second binding is built only on a real need.
The default stays FastEndpoints.
The specs never name a library.

## Profile sketch

A sketch, NOT implemented:

```toml
[bindings]
web = "fastendpoints"
```
"""


class LibraryBindingLayeringTests(unittest.TestCase):
    def test_minimal_document_passes(self):
        self.assertEqual(check.check_text(passing_text(), ROOT), [])

    def test_missing_section_fails(self):
        text = passing_text().replace("## Second binding\n", "", 1)
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("## Second binding" in problem for problem in problems))

    def test_audit_row_missing_file_fails(self):
        text = passing_text().replace(
            "`src/Phrase.Adapters.CSharp/CSharpProvider.cs`",
            "`src/Phrase.Adapters.CSharp/NoSuchBinding.cs`",
            1,
        )
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("NoSuchBinding.cs" in problem for problem in problems))

    def test_audit_citation_beyond_end_of_file_fails(self):
        text = passing_text().replace(
            "`src/Phrase.Adapters.CSharp/CSharpProvider.cs` line 1",
            "`src/Phrase.Adapters.CSharp/CSharpProvider.cs` line 999999",
            1,
        )
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("line 999999" in problem for problem in problems))

    def test_audit_citation_of_line_zero_fails(self):
        text = passing_text().replace(
            "`src/Phrase.Adapters.CSharp/CSharpProvider.cs` line 1",
            "`src/Phrase.Adapters.CSharp/CSharpProvider.cs` line 0",
            1,
        )
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("line 0" in problem for problem in problems))

    def test_audit_file_outside_table_must_exist(self):
        text = passing_text().replace(
            "`scripts/check-extension-map.py`",
            "`scripts/no-such-audit-mention.py`",
        )
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("no-such-audit-mention.py" in problem for problem in problems))

    def test_missing_logic_sources_section_fails(self):
        text = passing_text().replace("## Logic sources, in order", "## Logic")
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("## Logic sources, in order" in problem for problem in problems))

    def test_logic_source_order_fails_when_reordered(self):
        text = passing_text().replace(
            "2. Adapter idiom.\n3. LLM-generated body.",
            "2. LLM-generated body.\n3. Adapter idiom.",
        )
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("four-step order" in problem for problem in problems))

    def test_missing_native_share_signal_fails(self):
        text = passing_text().replace(
            "The share of logic still in native blocks is tracked as a trade-off signal, not a target.",
            "Native blocks can be counted.",
        )
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("native-share signal" in problem for problem in problems))

    def test_second_binding_rule_missing_fails(self):
        text = passing_text().replace(
            "The specs never name a library.",
            "Specs may name FastEndpoints.",
            1,
        )
        problems = check.check_text(text, ROOT)
        self.assertTrue(
            any("specs never name a library" in problem for problem in problems)
        )

    def test_sketch_not_marked_unimplemented_fails(self):
        text = passing_text().replace("NOT implemented", "implemented", 1)
        problems = check.check_text(text, ROOT)
        self.assertTrue(any("NOT implemented" in problem for problem in problems))

    def test_real_adr_passes(self):
        problems = check.check_path(check.ADR, ROOT)
        self.assertEqual(problems, [])


if __name__ == "__main__":
    unittest.main()
