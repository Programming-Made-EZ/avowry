# Converter between LLM-optimized Phrase and a human-friendly rendering (spike)

- **Status:** Ready, FUTURE (rank: see `board/MILESTONE-usable-v1.md`); do not start before Usable v1
- **Milestone:** M6 Human view (future)
- **North star:** `VISION.md`
- **Source:** Steve's direction ruling 4 (VISION.md); Decider D4. Reading to confirm with Steve.
- **Story:** As a human reviewer, I want a converter between the LLM-optimized Phrase form and a human-friendly rendering, in both directions, so that I can read and edit what the LLM maintains.
- **Hypothesis:** We believe a lossless two-way converter will let humans review Phrase without raising the cost for LLM authors/readers. We'll know when round trips (LLM form to human form and back) reproduce the original exactly on the fixtures, and reviewers rate the human form faster to read than the LLM form, with LLM token counts for the canonical form unchanged.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given an empty module, When converted both ways, Then it round-trips to an empty module
  - One: Given orders.phrase, When converted to the human form and back, Then the result is byte-identical to the original (lossless)
  - Many: Given the whole fixture set, When converted, Then every file round-trips
  - Boundaries: Given a human-form edit, When converted back, Then it is validated by the parser and reported with the same diagnostics
  - Exceptions: Given a human form that cannot be mapped back, When detected, Then the converter refuses with a message that names the line

## Context

- Reading of Steve's ruling used here: a two-way converter between two renderings. The alternative (one-way documentation generated from Phrase) is a subset of this and would be built first if Steve means that. To confirm with Steve before building.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled.
