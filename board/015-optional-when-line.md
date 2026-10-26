# Proposal: make the spec `when` line optional

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1
- **Source:** Retro 001 model feedback: agy (the `when called`/`when handled` line carries no information).
- **Story:** As an LLM writing specs, I want to omit the `when` line because the unit kind already determines it, so that I spend fewer tokens and cannot get it wrong.
- **Hypothesis:** We believe making the `when` line optional will reduce tokens out per spec-writing task for LLM authors without lowering pass rate. We'll know when mean tokens out per spec task drop at least 5% from the baseline on bench suite v1 with pass rate unchanged (within the noise floor).
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a phrase spec with no `when` line, When parsed, Then it is accepted and treated as `when called`
  - One: Given an endpoint spec with no `when` line, When parsed, Then it is treated as `when handled`
  - Many: Given the existing fixture with `when` lines, When parsed, Then it still parses (backward compatible)
  - Boundaries: Given `when handled` in a phrase spec (or `when called` in an endpoint), When resolved, Then PH330 still applies
  - Exceptions: Given the Decider protocol, When the four seats vote on this exact proposal, Then it proceeds only if it passes; the votes are recorded. If it fails, this card is closed as rejected with the reason

## Context

- Record the baseline from `bench/results/` before starting, and re-run the benchmark after; keep, revise or revert by the result and note why in MOB.md.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `tests/Phrase.Acceptance.Tests/Fixtures/orders.phrase`.
- Stage explicit paths only, never `.mob/`. Grammar changes need a recorded reason (Decider protocol if models disagree).
- **This card starts with a Decider run** (Codex, Copilot, Grok, agy vote simultaneously: Yes, Support-it or No). The `when` line currently makes specs read as Given/When/Then; the designer's concern is losing that BDD shape.

## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check of the doc change.
