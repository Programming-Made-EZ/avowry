# LLM-customer check: a model adds an endpoint from the docs alone

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Depends on:** 009, 010, 057
- **Milestone:** Usable v1 (plumbing for bar 6; exempt from the hypothesis rule) (`board/MILESTONE-usable-v1.md`)
- **Story:** As Ari, I want to extend an unfamiliar Phrase app using only GRAMMAR.md and the documentation, so that I can deliver a working endpoint without repository-specific coaching.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a fresh-context model with only GRAMMAR.md, the docs and the demo app, When asked to add `reopen task`, Then it writes the endpoint and specs in .phrase
  - One: Given the change, When built with the CLI, Then it compiles with zero diagnostics and zero warnings
  - Many: Given the generated specs plus the original ones, When run, Then all pass
  - Boundaries: Given the check run by two different models, When both succeed, Then the check passes; each run records model, tokens and attempts in `docs/usable-v1/llm-check.json` with the GRAMMAR.md hash
  - Exceptions: Given a failed attempt, When it happens, Then the diagnostics it saw and the docs gap are recorded as raw input for the retro

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check that the card's new tests exist.
