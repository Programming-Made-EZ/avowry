# Benchmark task 005 gets a failure-path spec (suite change, versioned)

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** data (thin slice; exempt from the hypothesis rule)
- **Milestone:** Vocabulary v1 and Usable v1 plumbing (consumer-observable outcomes, VISION ruling 5)
- **Source:** Steve, 2026-10-26 (VISION ruling 5): "I feel like we aren't adhering to consumer related output. 'succeeds' isn't really a result the customer gets value from." Team review: Decider D20 (Codex Support-it, Copilot Support-it, Grok Yes; agy no response).
- **Depends on:** none
- **Story:** As Ari, I want benchmark task 005 to declare and demonstrate its `MissingName` failure in a versioned spec, so that the example teaches me to preserve the failure behavior a consumer observes.
- **Done checks (at most 3, each failing on partial work):**
  1. bench/tasks/005-read-failure/prompt.md has a `then fails with MissingName` spec next to the happy-path spec; every other task file (holdouts included) is byte-identical to before
  2. The suite version is bumped, the change is recorded by a Decider (Goodhart rule), and the existing zero-shot rows in bench/results/history.jsonl stay unchanged and labeled with the old suite version
  3. The benchmark harness tests pass and the repository gate passes with zero warnings

## Context

- A benchmark-suite change: needs its own Decider and is never bundled with a feature it measures. Must land BEFORE card 055 (PH336), or 005's own source would fail to resolve. Audit result (visible tree): this is the only benchmark task with an outcome-less spec; holdout tasks 006 and 012 are untouched.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `VISION.md` (ruling 5), `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Task force

- Benchmark owner [author]: Codex | why: edits the task and the suite version
- Reviewer [reviewer]: Grok | why: checks the Goodhart rules and the holdout files
- QA: Copilot | why: confirms the history rows are unchanged

## Done when

All must exit 0, run from the repo root:

```
python -c "import sys;t=open('bench/tasks/005-read-failure/prompt.md',encoding='utf-8').read();sys.exit(0 if 'then fails with MissingName' in t else 1)"
python -c "import json,subprocess,sys;o=subprocess.run(['git','show','cfb1952:bench/results/history.jsonl'],capture_output=True,text=True).stdout;n=open('bench/results/history.jsonl',encoding='utf-8').read();v=json.load(open('bench/suite.json',encoding='utf-8'))['version'];sys.exit(0 if n.startswith(o) and v>1 else 1)"
dotnet test Phrase.sln --filter "Status!=Pending"
```
