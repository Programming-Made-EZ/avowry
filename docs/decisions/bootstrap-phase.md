# Bootstrap phase (Steve's standing rule, 2026-10-26)

The rule and its exit criteria are in `TEAM_WORKING_AGREEMENT.md` ("Bootstrap phase"). This file tracks status.

## Status of the exit criteria (2026-10-26)

| Criterion | Status |
|---|---|
| Benchmark trustworthy: four models valid and non-empty, full task coverage | Met for the zero-shot baseline (cfb1952): 32 rows, 8 development tasks per model, 2 of 32 unavailable, every completed row has a non-empty `reply_head`; invalid rows moved to `bench/results/invalid/` |
| Prompts include the docs a new user sees (docs-in-context, zero-shot rows labeled) | Not met: card 037 |
| Runs are cold (fresh session, no memory) | Not met: agy passes 7 of 8 zero-shot tasks, which suggests warm sessions; card 037 |
| Holdout exists and is untouched | Met (4 of 12 tasks held out; not run in development) |
| Steady cadence: a run after every Done card, at least 5 in a row | Not met: 1 run of a trustworthy zero-shot suite so far |
| Variance: noise floor at most 10 points of pass rate, self-check zero drift | Self-check zero drift: met; real-model noise floor: not measured (needs a repeat run) |
| Counter-metrics tracked per run | Partly: tokens (estimated), attempts, diagnostics, misread and `reply_head` recorded; wall-clock per task not yet |

Declaring the baseline "established" is a Decider, with this table updated and attached.

## Re-check of earlier decisions (2026-10-26)

- **Rejected on metrics?** None of the rejected ideas was rejected for "no metric impact". Grok's snippet mode (retro 002) and
  comma-separated `given` bindings (retro 001) were rejected on principle (language working as designed; a second way to do
  one thing against the small-core and deterministic-resolution rules), so they stay rejected unless the mob reconsiders. The
  comma-bindings note already said "revisit if measured token waste says otherwise", which fits this phase.
- **E1 (true mobbing):** its verdict is reframed as **early data, setup-heavy, not final**. It is not dropped. It stays as the
  opt-in `--true-mobbing` flag in mob-defaults and is re-evaluated after the baseline is established. See
  `docs/experiments/E1-true-mobbing.md`.
- **Hypothesis cards** (013, 014, 015, 019, 020, 021, 023, 024, 025, 033, 034, 035): unchanged. They keep their hypotheses and
  record measurements; none is gated on a metric until the baseline is established.

## Baseline snapshot (2026-10-26, zero-shot, not cold; for the record, not for decisions)

Passed of 8 development tasks: agy 7 (1 unavailable), codex 3, grok 1 (1 unavailable), copilot 0. The earlier 'two of 29' figure was the
harness's capture faults and is kept under `bench/results/invalid/`.
