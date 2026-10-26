# Usable v1: acceptance test passes on main and gates CI

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1 (plumbing for bars 1-6; exempt from the hypothesis rule) (`board/MILESTONE-usable-v1.md`)
- **Story:** As Steve, I want one automated test that says whether Phrase is usable to build an application, so that usable is detected rather than guessed.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a clean checkout, When the milestone test runs, Then it fails with a message naming the first unmet bar
  - One: Given the demo app and the CLI, When the test runs, Then it compiles, builds with zero warnings, runs the generated specs and checks live HTTP
  - Many: Given every bar in `board/MILESTONE-usable-v1.md` is met, When the test runs, Then it passes and CI prints USABLE v1 REACHED
  - Boundaries: Given GRAMMAR.md changed since the LLM check, When the test runs, Then it fails until the check is re-run
  - Exceptions: Given a failure at any step, When reported, Then the message names the step and the log path

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check that the card's new tests exist.
