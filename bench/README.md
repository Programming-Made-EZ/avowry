# Phrase benchmark

This suite measures how reliably an unmodified model can write, read, modify, and fix Phrase source. It is a language benchmark, not a prompt-tuning exercise: keep model prompts and Phrase documentation representative of what a new user sees.

## Tasks

`suite.json` lists each task by a stable `id`, a `kind`, a prompt path relative to this directory, and a `holdout` boolean. Task files live under `tasks/<id>/`. Supported kinds describe the requested operation: `write`, `read`, `modify`, or `fix`. A prompt should state the user goal and expected output without teaching syntax solely to improve a benchmark score.

Tasks marked `holdout: false` form the development suite. Holdout tasks are reserved for the final keep-or-revert evaluation and must not be inspected or tuned against while implementing a language change. Changes to tasks, prompts, or scoring require a separate recorded decision so results remain comparable.

## Running and scoring

Run the deterministic harness check with:

```text
dotnet run --project tools/Phrase.Bench -- --self-check
```

A normal run invokes one model profile headlessly, extracts Phrase source from a plain reply or a fenced block, and compiles that source. Compiler diagnostics are returned for bounded retries. A reply containing no Phrase is recorded with `no_phrase_found`; an authentication, quota, launch, or timeout failure is recorded as `unavailable`, not as a language failure.

Each result row records the model and task, prompt hash, pass status, attempts, input and output token counts, whether tokens are estimated or measured, diagnostics, and the misread-intent marker. Baseline rows are appended to `results/history.jsonl`. Deterministic self-check rows belong only under `results/self-check/` and must never be counted as model evidence.

## Reading results

Compare like-for-like rows using the prompt hash and suite version. Pass rate should be read together with attempts, token use, diagnostics, misread intent, and unavailable models; one number alone is not evidence that the language improved. Report missing models and their recorded errors explicitly rather than silently excluding them.

The self-check's repeated deterministic runs establish the harness noise floor. It should report zero change in pass rate, attempts, and tokens. If the self-check varies, treat model comparisons as untrustworthy until the harness is fixed. Real-model variation above that floor may still require repeated runs before attributing a change to Phrase.
