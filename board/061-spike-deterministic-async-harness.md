# Spike: a scripted scheduler, fake clock and event capture make an async spec deterministic

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** spike (thin; exempt from the hypothesis rule except 060, which creates the evidence)
- **Milestone:** Vocabulary v1 (base-library keywords; NOT on the Usable v1 path)
- **Source:** Steve, 2026-10-26: "I'm thinking there are certain concepts like asynchronous (parallel/concurrent) and events which our language may need to have." Team review: Decider D21 (Codex Support-it, Copilot Support-it, Grok Yes; agy no response). Bootstrap phase applies: a hypothesis to test, not an idea to reject on metrics.
- **Depends on:** 022, 033, 034 (language foundations); 025, 047 (a base library and the first effect)
- **Story:** As an engineer or an LLM writing Phrase, I want one scripted timeout-and-cancel spec and one event-capture spec run on the C# adapter with a fake clock and a controlled scheduler, no real waiting, with the same trace every run, so that asynchronous and event-driven work is expressible and specified deterministically on every target.
- **Done checks (at most 3, each failing on partial work):**
  1. The spec scripts completion order and the clock as literal data and the generated test follows it with no real wait (trait Slice=061)
  2. Running the generated tests twenty times gives the same result and trace every time
  3. Gaps found are filed as cards; the repository gate passes with zero warnings

## Context

- Time-boxed spike (one run). Extends the spec test doubles of card 034 and the injected effect of card 047. Not a feature: the keyword cards (parallel collect, bounded concurrency, publish/observe) are seeds until the ADR (059) and the demand evidence (060) exist.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `VISION.md`, `docs/GRAMMAR.md`, `docs/decisions/bootstrap-phase.md`, cards 022, 033, 034, 047.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Task force

- Backend author [author]: Codex | why: builds the harness test-first
- Reviewer [reviewer]: Copilot | why: independent review
- Language designer: Grok | why: checks the spec model matches the ADR

## Done when

All must exit 0, run from the repo root:

```
python scripts/check-slice-tests.py 061 2
dotnet test Phrase.sln --filter "Status!=Pending"
```
