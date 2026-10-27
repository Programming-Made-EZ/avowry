# Proposal: consistent given/responds syntax

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1
- **Source:** Retro 002 model feedback: agy (`given input { ... }` vs `given items = []`; `then responds { ... }` vs `respond with { ... }`).
- **Depends on:** 017
- **Story:** As Ari, I want bindings and responses to use one consistent syntax shape, so that I can write specs without losing attempts to a dropped `=` or `with`.
- **Hypothesis:** We believe making `given input = { ... }` and `then responds with { ... }` consistent with the other forms will cut first-attempt syntax errors for LLM authors. We'll know when first-attempt PH2xx syntax diagnostics per spec-writing task move from the baseline to at most half of it on bench suite v1, with no rise in tokens out.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given the Decider protocol, When the task force votes on this exact grammar change, Then the card proceeds only if it passes; if it fails the card closes as rejected with the reason (a grammar change is a language-direction question only if the Decider cannot settle it)
  - One: Given `given input = { ... }`, When parsed, Then it is accepted, and the old form still parses (backward compatible) or is migrated with a formatter rule
  - Many: Given `then responds with { ... }`, When parsed, Then it is accepted, alongside the existing form during the transition
  - Boundaries: Given the fixture and all existing tests, When the grammar changes, Then they still pass
  - Exceptions: Given a malformed binding, When parsed, Then the diagnostic names both accepted forms

## Context

- Opens with a Decider; ranked low because it changes the language and needs the benchmark to prove it helps.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled (mechanical checks that each criterion's evidence exists, plus the gate `dotnet test Phrase.sln --filter "Status!=Pending"`).
