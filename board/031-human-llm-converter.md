# Converter between LLM-optimized Phrase and a human-friendly rendering (spike)

- **Status:** Ready, FUTURE (rank: see `board/MILESTONE-usable-v1.md`); do not start before Usable v1
- **Milestone:** M6 Human view (future)
- **North star:** `VISION.md`
- **Source:** Steve's direction ruling 4 (VISION.md); Decider D4. Reading confirmed by Steve: two-way converter.
- **Depends on:** 030
- **Story:** As Morgan, I want bidirectional conversion between LLM-optimized Phrase and a human-friendly rendering, so that I can read and edit the behavior an agent maintains.
- **Hypothesis:** We believe a lossless two-way converter will let humans review Phrase without raising the cost for LLM authors/readers. We'll know when round trips (LLM form to human form and back) reproduce the original exactly on the fixtures, and reviewers rate the human form faster to read than the LLM form, with LLM token counts for the canonical form unchanged.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given an empty module, When converted both ways, Then it round-trips to an empty module
  - One: Given orders.phrase, When converted to the human form and back, Then the result is byte-identical to the original (lossless)
  - Many: Given the whole fixture set, When converted, Then every file round-trips
  - Boundaries: Given a human-form edit, When converted back, Then it is validated by the parser and reported with the same diagnostics
  - Exceptions: Given a human form that cannot be mapped back, When detected, Then the converter refuses with a message that names the line

## Context

- Reading confirmed by Steve: a converter between the LLM-optimized Phrase form and a human-friendly rendering, in both directions. Generated one-way documentation may be built first as a stepping stone, but the card is not done without the reverse direction.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled.
