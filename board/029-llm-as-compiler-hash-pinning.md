# LLM-as-compiler with hash pinning for keywords without an adapter implementation

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Vocabulary v1
- **North star:** `VISION.md`
- **Source:** Steve's earlier confirmed design: LLM-as-compiler with hash pinning. Decider D3.
- **Story:** As an engineer targeting a new language, I want a keyword without a hand-written implementation to be produced by an LLM and pinned by hash, so that the output is reproducible and still held to the keyword's specs.
- **Hypothesis:** We believe pinned LLM-produced implementations will let a new target support most of the catalogue without hand-written adapter implementations. We'll know when, for one target, the share of seed keywords produced by the LLM that pass their specs on the first pinned run reaches a target set from the card-028 baseline, with re-runs reproducing the pinned output exactly.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a keyword with a hand-written implementation, When compiled, Then the LLM is not called
  - One: Given a keyword without one, When compiled, Then an LLM produces the implementation, its specs run, and the output is stored with its hash
  - Many: Given a rerun, When the inputs are unchanged, Then the pinned output is reused byte for byte and the LLM is not called
  - Boundaries: Given a changed keyword definition, When compiled, Then the pin is invalidated and regenerated
  - Exceptions: Given an LLM output that fails its specs, When it happens, Then compilation fails naming the keyword and keeps no pin

## Context

- After Usable v1 and the second-adapter spike; needs a decision on which models may be used as compilers (Decider).

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled (mechanical checks that each criterion's evidence exists, plus the gate `dotnet test Phrase.sln --filter "Status!=Pending"`).
