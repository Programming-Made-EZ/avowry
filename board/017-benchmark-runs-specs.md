# Benchmark v1: score by running the generated specs

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Source:** Follow-up to card 016; needs the emitter (card 006).
- **Story:** As Riley, I want benchmark tasks scored by whether their generated specs pass, so that I can see whether a model produced the requested behavior rather than merely resolvable code.
- **Hypothesis:** We believe scoring against executed specs will expose misread-intent errors that parse-and-resolve scoring misses for LLM authors. We'll know when the share of tasks that resolve but fail their specs is measured and reported per model, from not measured (v0) to a number on suite v1.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a task whose solution resolves but has no passing specs, When scored, Then it is a failure counted as a misread-intent error
  - One: Given a correct solution, When scored, Then the generated xUnit specs pass and the task passes
  - Many: Given the full suite, When run, Then pass rate is reported per model with attempts to green and tokens
  - Boundaries: Given a task whose solution fails the build, When scored, Then it is a failure with the build diagnostics recorded
  - Exceptions: Given a scoring run that cannot start the build, When it happens, Then the result is `unscored` with the log path, never a silent pass

## Context

- Depends on card 006 (specs emitted as xUnit) and card 016.
- Re-baseline all models when scoring changes; keep v0 results for comparison and mark the suite version.

## Done when

The PM writes the concrete commands at the Ready check (the gate plus mechanical checks that `bench/suite.json`, the runner and a stored results file exist).
