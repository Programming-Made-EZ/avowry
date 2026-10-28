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

## Audit of FastEndpoints in the C# emitter

| Place | Where it lives today | Seam it needs |
|---|---|---|
{places}

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
