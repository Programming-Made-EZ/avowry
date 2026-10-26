# Proposal: parameterizable failure codes

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1 (base library needs it)
- **North star:** `VISION.md`
- **Source:** Retro 016 model feedback: agy (failure-code holes), Copilot (canonical failure contract). Decider D6.
- **Story:** As an author of reusable guards, I want the caller to supply the failure code, so that one `require ... or fail with {code}` keyword serves every domain.
- **Hypothesis:** We believe failure-code holes will remove per-domain guard clones for LLM authors without hiding failures. We'll know when the guard keywords in the base library drop from one per domain code to one per rule, and PH334 (undeclared failure) coverage stays complete in the dedup count and the spec pass rate.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given the Decider, When the task force votes on this grammar change, Then the card proceeds only if it passes; otherwise it closes as rejected with the reason
  - One: Given `fails with {code: Failure}`, When called with `EmptyOrder`, Then the caller's declared failures include EmptyOrder and the resolver still checks PH326/PH334
  - Many: Given two callers with different codes, When resolved, Then each unit's effective failures are exactly its own
  - Boundaries: Given a code that is not declared by the caller, When resolved, Then one diagnostic names it and lists the declared codes
  - Exceptions: Given a malformed failure hole, When parsed, Then a single diagnostic is reported and the resolver never throws

## Context

- Opens with a Decider because it changes the failure model; needs generics (033).

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled, including one negative check that the criteria cannot be satisfied by fixtures or fake data.
