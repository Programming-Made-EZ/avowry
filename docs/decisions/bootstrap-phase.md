# Bootstrap phase (Steve's standing rule, 2026-10-26)

The rule and its exit criteria are in `TEAM_WORKING_AGREEMENT.md` ("Bootstrap phase"). This file tracks status.

## Status of the exit criteria (2026-10-26)

| Criterion | Status |
|---|---|
| Benchmark trustworthy: four models valid and non-empty, full task coverage | Not met: agy's 8 rows are empty replies and Copilot lacks a task row; card 032 is reopened to fix both |
| Prompts include the docs a new user sees (docs-in-context, zero-shot rows labeled) | Not met: card 037 |
| Holdout exists and is untouched | Met (4 of 12 tasks held out; not run in development) |
| Steady cadence: a run after every Done card, at least 5 in a row | Not met: 0 consecutive runs of a trustworthy suite so far |
| Variance: noise floor at most 10 points of pass rate, self-check zero drift | Self-check zero drift: met; real-model noise floor: not measured |
| Counter-metrics tracked per run | Partly: tokens (estimated), attempts, diagnostics and misread recorded; wall-clock per task not yet |

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
