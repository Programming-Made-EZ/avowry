# C# emitter: specs as xUnit, and the first acceptance test goes green

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1 (plumbing for bar 4; exempt from the hypothesis rule) (`board/MILESTONE-usable-v1.md`)
- **Story:** As an engineer (or an LLM), I want every Given/When/Then spec emitted as a real xUnit test, so that specs run as the truth of my program.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a unit with no specs, When emitted, Then no test is produced for it and the project still builds
  - One: Given one phrase spec, When emitted, Then exactly one xUnit test is produced and passes
  - Many: Given the six specs in `orders.phrase`, When emitted, Then six tests run and pass (`CompileEndpointToCSharp` goes green; remove its Pending trait)
  - Boundaries: Given a spec expecting a failure code, an endpoint-response spec and a composite spec, When emitted, Then each outcome kind asserts correctly
  - Exceptions: Given a spec whose expected value differs from the actual one, When run, Then the generated test fails with a message that names the spec

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check that the card's new tests exist.
