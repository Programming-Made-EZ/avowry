# Synthesize Phrase from existing code (spike: C# to Phrase)

- **Status:** Ready, FUTURE (rank: see `board/MILESTONE-usable-v1.md`); do not start before Usable v1
- **Milestone:** M5 Synthesis (future)
- **North star:** `VISION.md`
- **Source:** Steve's direction ruling 1 (VISION.md); Decider D4.
- **Story:** As Dana, I want an existing C# codebase translated into Phrase through keyword mapping, so that I can adopt Phrase without paying for a manual rewrite of existing behavior.
- **Hypothesis:** We believe mapping code patterns to catalogue keywords will produce Phrase that is smaller and passes the same behavior for LLM authors/readers. We'll know when, on a sample of real C# files, the synthesized Phrase compiles back through the C# adapter and its specs pass for at least a target share of the files (set from a first measured pass), with the share of code left as native blocks reported as the counter-metric.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given code containing no recognizable pattern, When synthesized, Then the output is native blocks only and the report says no keywords matched
  - One: Given one function matching a base keyword, When synthesized, Then the Phrase uses the keyword and the round trip (Phrase to C# and its specs) passes
  - Many: Given a small real project, When synthesized, Then a report lists keyword matches, native leftovers and the failed files
  - Boundaries: Given code that looks like a keyword but behaves differently, When synthesized, Then the specs catch it and the match is rejected
  - Exceptions: Given unparseable code, When synthesized, Then it is skipped with a logged reason

## Context

- Needs the base library (025), the C# adapter and pattern discovery (024). Spike only: findings are the deliverable.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled.
