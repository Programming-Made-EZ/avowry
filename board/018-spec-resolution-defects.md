# Spec resolution defects from the independent review

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** bug (exempt from the hypothesis rule)
- **Source:** Grok independent review of a47ed6a (Copilot) and 8ba3699 (agy), 2026-10-25, verdict CHANGES.
- **Story:** As a Phrase author (an engineer or an LLM), I want spec errors reported accurately and specs held to literal data, so that the specs I write are the truth the emitter turns into tests.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given an endpoint spec that expects a failure the endpoint cannot produce, When resolved, Then PH334 names the real endpoint (for example `CreateOrder`), not the placeholder `Probe`, and lists the failures it can produce
  - One: Given `then returns { sku: item.sku, quantity: 1 }` or an input record whose field is `input.customerId`, When resolved, Then PH332 is reported (a path nested inside a spec record or list is still a path, and specs use literal data only)
  - Many: Given a spec that binds the same hole twice, When resolved, Then exactly one diagnostic is reported for the duplicate (not the same PH331 twice)
  - Boundaries: Given `given c = 2` on `add {a: Int} to {b: Int}`, When resolved, Then PH331 says `c` is not a hole of this phrase and lists the holes, instead of the missing-hole wording
  - Interface: Given the HANDOFF.md diagnostics table and `docs/GRAMMAR.md`, When read, Then only one of them holds the full table and the other points to it (today both do, and GRAMMAR.md wrongly says the table moved)
  - Exceptions: Given any malformed spec, When resolved, Then the resolver never throws (the fuzz stays green)

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `HANDOFF.md`, `docs/GRAMMAR.md`, `src/Phrase.Compiler/Semantics/Resolver.cs`, `ResolverSpecTests.cs`.
- The endpoint name must come from the model, not a special case in a fixture. Reviewers check that no fix is a hack that only helps tests.
- Tests first in a new `tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs`, with Given/When/Then comments and one test per criterion. Stage explicit paths only, never `.mob/`.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes the concrete, red-first commands when the card is pulled: a mechanical check that `ResolverSpecDefectsTests.cs` exists with a test per criterion, a check that `Probe` no longer appears in the PH334 message code, the gate `dotnet test Phrase.sln --filter "Status!=Pending"`, and a docs check that only one file holds the full diagnostics table.
