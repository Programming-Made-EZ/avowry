# Publish exact diagnostic message templates in GRAMMAR.md

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1
- **Source:** Retro 002 model feedback: Codex, Copilot, Grok and agy (4 of 4); agy also flagged PH326 vs endpoint failures.
- **Story:** As Ari, I want the exact message template for each diagnostic in GRAMMAR.md, so that I can understand and fix a failing program without opening the compiler.
- **Hypothesis:** We believe publishing the exact message template next to each diagnostic code will cut wasted reads and wrong fixes for LLM authors. We'll know when attempts-to-green on the benchmark's fix-a-failing-spec tasks moves from the card-016 baseline to at most 80% of it, with pass rate unchanged, on bench suite v1.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given the diagnostics table in GRAMMAR.md, When read, Then every code lists its exact message template with placeholders (for example `PH334 Spec expects failure '{code}', which '{unit}' cannot produce. It can fail with: {codes}`)
  - One: Given PH334, When its template is compared with the compiler's output, Then they match, and a test fails if a message drifts from the table (a docs-vs-code test)
  - Many: Given all of PH001-PH335, When the table is checked, Then every code the resolver can emit has a template, and no template names a code the resolver cannot emit
  - Boundaries: Given an endpoint, When its failures are read in the docs, Then one paragraph says endpoints propagate step failures implicitly (no `fails with` clause) while a composite phrase must declare them (PH326), with an example of each
  - Interface: Given a model with only GRAMMAR.md, When it predicts the diagnostic for a broken snippet, Then the predicted code and message match what the compiler prints (the benchmark's fix tasks measure this)
  - Exceptions: Given a message that changes later, When the table is not updated, Then the docs-vs-code test fails

## Context

- Depends on bug card 018 (PH334 must name the real endpoint, not the placeholder `Probe`) so the published template is the true one.
- Record the baseline from `bench/results/` first and re-run the benchmark after; keep, revise or revert by the result.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled (mechanical checks that each criterion's evidence exists, plus the gate `dotnet test Phrase.sln --filter "Status!=Pending"`).
