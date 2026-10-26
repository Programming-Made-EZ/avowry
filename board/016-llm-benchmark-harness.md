# LLM benchmark harness v0: measure how well models use Phrase

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Source:** Steve, 2026-10-25: data-driven, hypothesis-driven work. This is the instrument for every later feature card.
- **Story:** As the mob, I want a fixed, versioned suite of Phrase tasks run cold by each model in the rotation, so that every feature and doc change is judged by data rather than opinion.
- **Hypothesis:** We believe a pinned, repeatable benchmark will let us rank and keep or revert changes by metric for LLM authors/readers. We'll know when two runs of the same suite on the same model differ by no more than 10 points of pass rate (noise floor) and every later feature card cites a baseline from it.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given an empty results directory, When the harness runs a suite, Then it writes one result file per model and an appended `bench/results/history.jsonl` line
  - One: Given one task (write a phrase from a description) and one model, When run cold, Then the result records pass/fail, attempts to green, tokens in and out (measured, or estimated and flagged `estimated`), diagnostics hit by code, and whether the failure was a misread of intent
  - Many: Given the suite `bench/suite.json` (version 1) with at least 12 tasks across four kinds: write, read-and-explain, modify, fix-a-failing-spec, When run for Codex, Copilot, Grok and agy, Then each model gets the identical prompt and the same pinned model id, recorded in the result
  - Boundaries: Given a model that is unavailable or times out, When run, Then the result records `unavailable` with the error and the other models still run; one model's failure never aborts the suite
  - Interface: Given a task, When scored, Then v0 scores parse + resolve success and diagnostics using the real compiler (spec execution arrives with card 017); an attempt loop gives the model the diagnostics and up to 2 retries
  - Boundaries (Goodhart guard): Given `bench/suite.json`, When tasks are listed, Then a fixed holdout subset (at least a quarter of the tasks) is marked `holdout`, is excluded from development runs, and runs only as the keep-or-revert check; each reported target metric is paired with its counter-metric
  - Exceptions: Given the same suite and model run twice, When compared, Then the harness reports the noise (difference per metric) so later hypotheses can be judged against it

## Context

- Layout: `bench/suite.json` (suite version, task list), `bench/tasks/NNN-name/` (prompt, starter `.phrase` where needed, expected behavior as specs), `bench/results/*.json` and `history.jsonl`, runner as a .NET console project `tools/Phrase.Bench` that calls `PhraseCompiler` and shells out to the model CLIs the way `mob-crosscli` does.
- Pinning: prompt text is fixed in the task files and hashed into the result; model ids are recorded; the CLIs expose no seed, so the noise-floor check (Exceptions) is how nondeterminism is controlled.
- Read-only against the repo: model runs happen in a scratch copy, never in the working tree.
- Metrics recorded per model and per task: pass rate, attempts to green, tokens in/out, diagnostics hit, misread-intent errors.
- Baseline first: the first stored run is the baseline cards 013, 014 and 015 cite.

## Task force

Assembled 2026-10-25 (revised the same day to the roles-that-apply form).

- Backend/tool engineer [author]: Codex | why: writes `tools/Phrase.Bench` and ships it
- Benchmark/eval designer: Grok | why: designs tasks, holdout split, scoring and the Goodhart guard
- QA engineer [reviewer]: Copilot | why: independent verification, including a rerun for the noise floor
- Security reviewer: agy | why: model CLIs run in scratch copies and results must hold no secrets
- Technical writer: Copilot | why: bench README and task authoring guide
- Agile coach: Claude (session) | why: flow metrics, and a role-fit check at retro

## Done when

The PM writes the concrete commands at the Ready check (the gate plus mechanical checks that `bench/suite.json`, the runner and a stored results file exist).
