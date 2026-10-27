# ADR: deterministic asynchronous and event semantics

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** decision (thin; exempt from the hypothesis rule except 060, which creates the evidence)
- **Milestone:** Vocabulary v1 (base-library keywords; NOT on the Usable v1 path)
- **Source:** Steve, 2026-10-26: "I'm thinking there are certain concepts like asynchronous (parallel/concurrent) and events which our language may need to have." Team review: Decider D21 (Codex Support-it, Copilot Support-it, Grok Yes; agy no response). Bootstrap phase applies: a hypothesis to test, not an idea to reject on metrics.
- **Depends on:** 022, 033, 034 (language foundations)
- **Story:** As Ari, I want documented portable semantics for asynchronous work, parallel collection, bounded concurrency, timeouts, cancellation and events, so that I can express and deterministically specify those behaviors for every target.
- **Done checks (at most 3, each failing on partial work):**
  1. docs/decisions/ADR-async-events.md records the grammar, library and effect boundaries and names the effects (Scheduler, Clock, Cancellation, Events)
  2. It defines the portable observables (input-ordered results, failure aggregation, timeout and cancel outcomes, delivery) and the spec model (virtual clock, scripted scheduler, event capture; specs never depend on wall time or OS interleaving)
  3. It lists the adapter obligations (what a second adapter must prove) and the follow-up cards; a Decider approves it

## Context

- Language designer's view (D21): keep the core grammar unchanged; async is an execution property of an adapter, not user-facing syntax; a handler sentence must not start with the line-start word `when` (reserved for spec triggers). Main risk: accidentally standardizing C#'s scheduling and event semantics, producing flaky specs that cannot port (card 028 is the check). Usable v1 needs NONE of this: the Tasks demo (009) and the status API (038, first slice) do not (D21, all three seats). Depends on 022 (keyword model), 033 (generics) and 034 (behavioral holes and test doubles).
- Read first: `TEAM_WORKING_AGREEMENT.md`, `VISION.md`, `docs/GRAMMAR.md`, `docs/decisions/bootstrap-phase.md`, cards 022, 033, 034, 047.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Task force

- Language designer [author]: Grok | why: owns the semantics wording
- Architect [reviewer]: Copilot | why: checks the adapter mappings and portability
- Domain expert: Codex | why: checks the C# mapping (Task.WhenAll, SemaphoreSlim, CancellationToken, TimeProvider)

## Done when

All must exit 0, run from the repo root:

```
python -c "import sys;t=open('docs/decisions/ADR-async-events.md',encoding='utf-8').read();sys.exit(0 if all(w in t for w in ('Scheduler','Clock','Cancellation','Events','adapter')) else 1)"
python scripts/check-decider-recorded.py ADR-async-events
```
