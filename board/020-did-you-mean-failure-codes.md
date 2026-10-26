# Did-you-mean suggestions for failure codes and other unknown names

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1
- **Source:** Retro 002 model feedback: Codex (typo-aware failure suggestions).
- **Story:** As an LLM fixing a typo, I want the compiler to name the closest valid failure code or name, so that I fix it in one attempt.
- **Hypothesis:** We believe a closest-match suggestion on PH334 (and other unknown-name diagnostics) will reduce attempts-to-green on fix tasks for LLM authors. We'll know when attempts-to-green on the benchmark's fix-a-failing-spec tasks moves from the card-016/017 baseline to at least one attempt fewer on average, with the counter-metric (message tokens) up by no more than 10%.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a spec expecting `InvalidQty` where the unit can fail with `InvalidQuantity`, When resolved, Then PH334 adds `Did you mean 'InvalidQuantity'?`
  - One: Given an unknown type, variable or field, When resolved, Then the existing diagnostic adds the closest valid name (PH001 already does this for phrases)
  - Many: Given several close candidates, When resolved, Then at most the two closest are suggested, ordered by edit distance
  - Boundaries: Given nothing close (edit distance above a threshold), When resolved, Then no suggestion is made and the full list of valid options is shown instead
  - Exceptions: Given empty candidates, When resolved, Then the resolver never throws and the message is unchanged

## Context

- Needs the benchmark (016/017) for its baseline; ranked after 017.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled (mechanical checks that each criterion's evidence exists, plus the gate `dotnet test Phrase.sln --filter "Status!=Pending"`).
