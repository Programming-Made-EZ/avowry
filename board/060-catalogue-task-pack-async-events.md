# Catalogue tasks for concurrent work and event publication (measure the demand)

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** data (thin; exempt from the hypothesis rule except 060, which creates the evidence)
- **Milestone:** Vocabulary v1 (base-library keywords; NOT on the Usable v1 path)
- **Source:** Steve, 2026-10-26: "I'm thinking there are certain concepts like asynchronous (parallel/concurrent) and events which our language may need to have." Team review: Decider D21 (Codex Support-it, Copilot Support-it, Grok Yes; agy no response). Bootstrap phase applies: a hypothesis to test, not an idea to reject on metrics.
- **Depends on:** 022, 033, 034 (language foundations)
- **Story:** As Riley, I want benchmark tasks that require parallel collection, bounded concurrency, publication and timeout or cancellation, so that I can measure recurring demand and whether named keywords reduce agent work.
- **Done checks (at most 3, each failing on partial work):**
  1. Development tasks are added (holdouts untouched, byte-identical) with the suite version bumped and a Decider recording the change (Goodhart rules)
  2. Baseline rows are recorded with native lines, tokens, attempts-to-green and repeated-run determinism, alongside the old rows unchanged and labeled
  3. The benchmark harness tests pass and the repository gate passes with zero warnings

## Context

- LLMs-as-customers check (D21): the repository shows retry as a repeated candidate but NO evidence yet for parallelism or events, and the benchmark has zero concurrency or event tasks. This card is how the evidence gets made; the keyword cards wait for it. A benchmark-suite change: its own Decider, never bundled with a feature it measures.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `VISION.md`, `docs/GRAMMAR.md`, `docs/decisions/bootstrap-phase.md`, cards 022, 033, 034, 047.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Task force

- Benchmark owner [author]: Codex | why: authors the tasks
- Reviewer [reviewer]: Grok | why: Goodhart rules, holdouts
- QA: Copilot | why: history rows unchanged

## Done when

All must exit 0, run from the repo root:

```
python -c "import json,sys;v=json.load(open('bench/suite.json',encoding='utf-8'))['version'];sys.exit(0 if v>1 else 1)"
python scripts/check-decider-recorded.py catalogue-task-pack-async-events
dotnet test Phrase.sln --filter "Status!=Pending"
```
