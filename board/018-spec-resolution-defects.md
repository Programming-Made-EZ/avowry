# Spec resolution defects from the independent review (code only)

- **Status:** Done 2026-10-26 (fix 8962dc5; two runs; the first stopped at the cycle cap on an environment fault, the .NET 8 runtime missing from the mob shell, not on size)
- **Type:** bug (exempt from the hypothesis rule); code only. The docs criterion (one diagnostics table) moved to card 053 under the thin-slice rule (D18).
- **Source:** Grok independent review of a47ed6a (Copilot) and 8ba3699 (agy), 2026-10-25, verdict CHANGES.
- **Depends on:** none
- **Story:** As a Phrase author (an engineer or an LLM), I want spec errors reported accurately and specs held to literal data, so that the specs I write are the truth the emitter turns into tests.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given an endpoint spec that expects a failure the endpoint cannot produce, When resolved, Then PH334 names the real endpoint (for example `CreateOrder`), not the placeholder `Probe`, and lists the failures it can produce
  - One: Given `then returns { sku: item.sku, quantity: 1 }` or an input record whose field is `input.customerId`, When resolved, Then PH332 is reported (a path nested inside a spec record or list is still a path, and specs use literal data only)
  - Many: Given a spec that binds the same hole twice, When resolved, Then exactly one diagnostic is reported for the duplicate (not the same PH331 twice)
  - Boundaries: Given `given c = 2` on `add {a: Int} to {b: Int}`, When resolved, Then PH331 says `c` is not a hole of this phrase and lists the holes, instead of the missing-hole wording
  - Exceptions: Given any malformed spec, When resolved, Then the resolver never throws (the fuzz stays green)

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `HANDOFF.md`, `docs/GRAMMAR.md`, `src/Phrase.Compiler/Semantics/Resolver.cs`, `ResolverSpecTests.cs`.
- The endpoint name must come from the model, not a special case in a fixture. Reviewers check that no fix is a hack that only helps tests.
- Tests carry the trait `Slice=018`. Tests first in a new `tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs`, with Given/When/Then comments and one test per criterion. Stage explicit paths only, never `.mob/`.

## Task force

- Backend author [author]: Codex | why: fixes the resolver test-first
- Reviewer [reviewer]: Copilot | why: independent review; checks no fix is a fixture hack
- Language designer: Grok | why: raised the findings; checks the messages and the PH numbers against GRAMMAR.md

## Done when

All must exit 0, run from the repo root (red first: today the test file and its tests do not exist and `Probe` is still in the resolver):

```
python scripts/check-slice-tests.py 018 4
python -c "import sys;t=open('src/Phrase.Compiler/Semantics/Resolver.cs',encoding='utf-8').read();sys.exit(1 if 'Probe' in t else 0)"
dotnet test Phrase.sln --filter "Status!=Pending"
```

One test per criterion above (PH334 real endpoint name, PH332 nested path, one duplicate diagnostic, PH331 wording) is the minimum of four Slice=018 tests; the existing fuzz test stays green inside the gate.
