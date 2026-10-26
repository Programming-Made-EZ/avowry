# Benchmark baseline is invalid: fix the harness and store a trustworthy baseline

- **Status:** Ready, rank 3 (see `board/MILESTONE-usable-v1.md`)
- **Type:** bug (exempt from the hypothesis rule); the first card run under experiment E1, true mobbing
- **Milestone:** Usable v1 (plumbing: every hypothesis card needs a trustworthy baseline); gates milestone M2 'Measured'
- **Source:** Facilitator's check of card 016's stored results, 2026-10-25. Decider D5.
- **Story:** As the mob, I want a benchmark baseline we can trust, so that every later feature card is judged by data that measures the language and not the harness.
- **Evidence of the defects (from `bench/results/history.jsonl`):**
  - codex rows are 8 completed and 8 `unavailable`: `Not inside a trusted directory and --skip-git-repo-check was not specified`.
  - grok rows are 8 completed, 0 passed, with PH104 and PH201/PH202 diagnostics on every attempt, and 8 `unavailable` with `Max turns reached`: the harness is probably scoring model chatter or markdown fences as Phrase, and not giving the CLI a one-shot invocation.
  - agy has 3 completed and 5 unavailable; copilot has no rows.
  - the fake `self-check-v1` model's 48 rows share `history.jsonl`, and they satisfy the 'at least 3 models' check.
  - there is no `bench/README.md`.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a model reply that is only narration or a markdown fence around Phrase, When scored, Then the runner extracts the Phrase source (fences and prose stripped) and scores that, and a reply with no Phrase at all is a failed attempt with a clear `no_phrase_found` reason, not PH104
  - One: Given each of Codex, Copilot, Grok and agy, When one task is run, Then the CLI is invoked one-shot and headless with its own working flags (Codex with `--skip-git-repo-check`; Grok within its turn limit), and the result is `completed`
  - Many: Given the whole development suite and all four models, When the baseline is run, Then at most 10% of rows are `unavailable`, every model has completed rows, and every row records the model id, prompt hash, attempts, tokens (flagged estimated or measured), diagnostics and misread-intent
  - Boundaries: Given the self-check, When it runs, Then its rows go to `bench/results/self-check/` and never into `history.jsonl`, and no Done check may count them as a model
  - Interface: Given `bench/README.md`, When a new task author reads it, Then it explains the task format, kinds, holdout rule, scoring, the noise floor and how to read the results
  - Exceptions: Given a model that is genuinely unavailable (quota, auth), When run, Then it is recorded `unavailable` with the error, and the baseline run says which models are missing and why

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/experiments/E1-true-mobbing.md`, `board/016-llm-benchmark-harness.md`, `tools/Phrase.Bench`.
- Goodhart guard: do not tune prompts to make a model pass. The baseline measures the language and docs as they are today; fix only harness faults (invocation, parsing, row hygiene).
- Run with `--true-mobbing` (experiment E1): a driver executes what the navigator directs, observers check at each checkpoint, and the whole task force signs off before the commit. The metrics (turns, flags, fix rounds, estimated tokens, seconds, cycles) are the experiment's data.

## Task force

Assembled via Decider D5 (kickoff proposal by the facilitator; the same vote covered the card). Experiment E1: every member signs off before the commit.

- Benchmark engineer: Codex | why: owns the runner's model invocations and reply parsing
- Eval designer: Grok | why: keeps scoring fair and the baseline honest (Goodhart guard), and navigates
- Skeptical QA: Copilot | why: checks the rows, not the claims, and signs off as a whole-team member
- Security reviewer: agy | why: model CLIs run in scratch copies, and results must hold no secrets or file paths
- Agile coach: Claude (session) | why: records the E1 metrics and checks role fit at retro

## Done when

The first four commands must exit 0; they are red today.

```
python -c "import json,sys; rows=[json.loads(l) for l in open('bench/results/history.jsonl',encoding='utf-8') if l.strip()]; sys.exit(1 if any(str(r.get('model','')).startswith('self-check') for r in rows) else 0)"
python -c "import json,sys; rows=[json.loads(l) for l in open('bench/results/history.jsonl',encoding='utf-8') if l.strip()]; m={r['model'].split('-')[0] for r in rows if r.get('status')=='completed'}; un=sum(1 for r in rows if r.get('status')=='unavailable'); sys.exit(0 if {'codex','copilot','grok','agy'}<=m and un*10<=len(rows) else 1)"
python -c "import pathlib,sys; sys.exit(0 if pathlib.Path('bench/README.md').is_file() and len(pathlib.Path('bench/README.md').read_text(encoding='utf-8'))>600 else 1)"
python -c "import json,sys; rows=[json.loads(l) for l in open('bench/results/history.jsonl',encoding='utf-8') if l.strip()]; bad=[r for r in rows if r.get('status')=='completed' and any(str(d).startswith('PH10') for d in r.get('diagnostics',[])) and r.get('attempts')==1]; sys.exit(0 if len(bad)*10<=max(1,len(rows)) else 1)"
dotnet run --project tools/Phrase.Bench -- --self-check
dotnet test Phrase.sln --filter "Status!=Pending"
```
