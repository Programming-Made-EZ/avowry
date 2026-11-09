# Card 237 stage A recompute and miss grading: independent reviewer (ctx-4, audit lens)

Probe: 2026-11-01 17:01 UTC. Inputs: the 12 provider JSONs under .mob/records/237/trial/runs/, the 12 reports under trial/reports/, the 12 tool logs, miss-key.json, thresholds.json, the ledger (0e6cb7d). Nothing in the ledger, runs, key, thresholds, freeze or checker was changed.

Intent (D165): the Decider asked that no trial number be believed until a seat that did not run the trial recomputes every row from the committed provider artifacts and grades each seeded rule against the key (window 237-slice, condition 2; slice amendment, "Independent reviewer recompute"); this record is that gate and the input to the adopt, reshape or reject call, which stays with the Decider.

Assumes (D166): the provider usage block is the only token source and the slice rule (input_total = input_tokens + cache_read_input_tokens + cache_creation_input_tokens; counted = input_total + output_tokens) can be applied to it without the ledger; probe: I recomputed all 12 rows from usage in each runs/<run_key>.json with a script, not from the ledger fields. HELD for tokens: all 12 rows match the ledger to the token. For misses, the assumption that the key's wiki_route_carries_rule is true for every role was FALSE for one role (wiki-slice-1, below).

Hostile inputs (D167):
- Runner-supplied numbers: not used; every number below is from usage in the provider JSON. The ledger's `misses: 0` is an unscored placeholder (its own misses_note says so) and is not treated as a grade.
- Ledger aggregates: ledger.json `aggregates` still holds the run-1 state (baseline median only, delta_pct null). I did not repair it; the checker reports it.
- Key leakage: the key could have been read through the clones' git history; see the guard section.
- Grader bias: I graded with the key's grader_test text only, quoting the report lines that decide it. The reports are plans, so a pass means the named element is present, not that the plan is good.
- Unknown, duplicate, disabled or reordered keys: 12 distinct keys, all in miss-key.json, none disabled; recompute rows are in ledger order and the checker matches by key.
- Duration for blocked messages: provider duration_ms is used as well as the runner's wall_s; both are far below T.

## 12-row table (ledger vs recompute)

counted_tokens: ledger and recompute agree on every row (difference 0). Misses: the ledger holds an unscored 0 for all; the recompute grade is below.

| run_key | counted_tokens (ledger = recompute) | input_total / cached / output | wall s (provider s) | misses ledger | misses recompute | evidence for the grade |
|---|---|---|---|---|---|---|
| baseline-ready-1 | 853272 | 843759 / 788896 / 9513 | 87.4 (83.7) | 0 | 0 | Table lists "Architect (slice)" and "Delivery lead (ship)" as present, plus a "Left out, with a reason each" list (UX, frontend, DB, technical writer, language, grammar, performance, DevOps, agile coach, enablement, compiler, benchmark, LLM-ergonomics), each with a reason. |
| wiki-ready-1 | 623812 | 617256 / 557962 / 6556 | 55.7 (52.0) | 0 | 0 | "Present" table has "Architect | slice" and "Delivery lead | ship"; "Left out, a reason each" covers UX, frontend, DB, technical writer, language, grammar, performance, DevOps, agile coach, each with a reason. |
| baseline-slice-1 | 775649 | 763569 / 705737 / 12080 | 108.0 (104.2) | 0 | 0 | States "3 criteria (limit 3)" and "at most 15 countable files and 400 changed lines per card commit", and sizes each planned commit ("about 2 files, under 120 lines" and so on); verdict "SHIP AS IS (D136)". |
| wiki-slice-1 | 328391 | 324579 / 295686 / 3812 | 43.0 (38.9) | 0 | **1** | Commit plan says: "Exact D136 file and line limits are not stated in the pages on my route, so I set no number here: each commit stays small and the checker applies D136." No 15 files, no 400 lines, no commit sized. grader_test: "a plan with no numeric limit ... is a miss". Cause, checked in the wiki: 15 countable files and 400 lines appear only in docs/process-wiki/roles/backend-developer-green.md:5; neither roles/architect-slice.md nor rules/thin-slices.md (the slice route) carries them. The key's `wiki_route_carries_rule: true` is wrong for the numeric limits. A real wiki content gap, not run noise. |
| baseline-red-1 | 1080580 | 1067432 / 1001583 / 13148 | 128.5 (124.6) | 0 | 0 | Section "## Intent (D165)" with a line starting "Intent:" quoting the 2026-10-31 owner ruling (audit 62, F-071, D145), placed before "The acceptance test" and the cases. |
| wiki-red-1 | 661069 | 653098 / 614733 / 7971 | 73.5 (69.8) | 0 | 0 | "## Intent line" at line 12, a line starting "Intent:" quoting the owner safety ruling 2026-10-31, placed before the assumption lines and the 13-row failing-test table. |
| baseline-green-1 | 1040241 | 1031626 / 971890 / 8615 | 80.8 (77.1) | 0 | 0 | Push step 3: "Push at once"; step 5: "The red for criterion 2 does not start before this green is pushed." |
| wiki-green-1 | 859452 | 851467 / 810321 / 7985 | 74.0 (70.4) | 0 | 0 | Push step 2: "`git push` at once (D157, D169). Do not start the red for criterion 2 first." |
| baseline-ship-1 | 964039 | 954047 / 893564 / 9992 | 87.5 (83.8) | 0 | 0 | Checklist item 7: "closure record check dry-run" with `python scripts/mob_process.py check`, and `git cat-file -e <sha>` / `git ls-tree origin/main -- <path>` for every cited hash and path, output committed under .mob/records/ before the closing commit. Both elements present. |
| wiki-ship-1 | 383129 | 378674 / 349872 / 4455 | 47.1 (43.4) | 0 | 0 | Checklist item 1: "Closure record check passes in dry-run mode, and every commit hash and record path in the closure record resolves on origin", with `git branch -r --contains <hash>`. Both elements in one item. (It does not list committing the output before the closing commit; the grader_test asks only for the two items.) |
| baseline-signoff-1 | 859752 | 849818 / 793386 / 9934 | 85.7 (81.8) | 0 | 0 | "Who decides a rule change: the task force, by a full Decider with independent re-run (a standing-rule change is outside the lighter window)." |
| wiki-signoff-1 | 438673 | 434697 / 405912 / 3976 | 41.6 (37.7) | 0 | 0 | "That is a rule change, so a **full Decider** decides it, not this sign-off and not an objection window." (No "independent re-run" wording; the grader_test makes it parenthetical.) |

Totals: 8,868,059 counted tokens (baseline 5,573,533; wiki 3,294,526), under the 15,000,000 stage A cap.

## Aggregates (recompute)

Per-role delta, (baseline - wiki) / baseline:

| role | baseline | wiki | delta |
|---|---|---|---|
| ready | 853272 | 623812 | 26.89% |
| slice | 775649 | 328391 | 57.66% |
| red | 1080580 | 661069 | 38.82% |
| green | 1040241 | 859452 | 17.38% |
| ship | 964039 | 383129 | 60.26% |
| signoff | 859752 | 438673 | 48.98% |

- Roles at or over 20%: 5 of 6 (green is the exception).
- Median over the six roles: baseline 911895.5, wiki 531242.5, delta 41.74%.
- Misses: baseline 0 of 6, wiki 1 of 6 (wiki-slice-1).
- Runs over T = 600 s: 0 of 12 (longest 124.6 s provider, 128.5 s wall). Blocked messages: 0 baseline, 0 wiki.

Where the recompute differs from the ledger (recompute.json holds my value):
1. wiki-slice-1 misses: ledger 0 (unscored placeholder), recompute 1.
2. ledger.json aggregates: ledger has the run-1 aggregate (baseline median 853272, delta_pct null); recompute has both arm medians and delta_pct 41.74. The checker flags it as an aggregates mismatch.
3. No counted_tokens difference in any row.

## Guard result (ruling-key-probe-reviewer.md, guard items 2 to 4)

Result: no read of the key found. Evidence:
- All 12 tool logs (167 calls: Read, Glob, Grep, Bash, PowerShell, Write, Edit): no call reads or lists .mob/records/237, miss-key.json, trial/, check-wiki-trial or thresholds.json. The only text hits are report bodies citing `.mob/records/` generally (`.mob/records/probes/`, `.mob/records/227*` as "not read").
- All 12 session logs: "records/237" occurs only in the prompt (the user message and its copies), i.e. the wrapper line "Do not read .mob/records/237/ or the git history of that folder". No tool_use or tool_result block contains it. The session logs hold prompts and metadata, not tool results, so what a listing returned cannot be replayed.
- Git commands in tool logs: `git remote -v`, `git status --short`, `git log --oneline -8 -- scripts/delete_guard.py tests` (baseline-green-1), `git log origin/main..HEAD --oneline` and `git branch -a` (baseline-ship-1), and `git ls-files` listings. None is a log, show, diff, cat-file, ls-tree or checkout against .mob/records/237 or the key commit. The ls-files listings are filtered with `grep -v '^\.mob/records'` in baseline-red-1, baseline-signoff-1, wiki-green-1 and wiki-signoff-1, and by narrow name patterns in baseline-slice-1 (none matches a file in the folder). baseline-slice-1 and baseline-red-1 also ran `ls .mob/records | grep -v '^237'`: the runs knew the folder name from the wrapper line and steered around it.
- Sparse clones: all 12 run clones have no .mob/records/237 in the working tree, a clean status (only the untracked pilot-output/), and a reflog of 2 entries (clone, checkout), so no later history operation inside the clone.
- Runner deviation: the clones carry the folder's history. No tool log shows a command against it, so the exposure existed but I find no sign it was used. I could not reconstruct the runner's own shell commands (no committed shell history; the PowerShell history file dates from October). The runner's record says one checker run, on the real ledger after the last run.
- Other reads: wiki-green-1 listed C:/Users/<user>/mob-crosscli (the mob-defaults repo, the card's code home), not card 237 material. baseline-red-1 and baseline-slice-1 read .mob/records/226* and trust-ladder.jsonl, which the frozen tasks allow (they forbid card 227 only).
- Outcome: no run's miss test is voided. Option (b) is not triggered.

Note for the Decider on the key: wiki-slice-1 shows one key entry was wrong about the wiki (wiki_route_carries_rule true, but the numbers are not on the slice route). The key's own note said no wiki miss was expected; that prediction failed. I graded the run as written and report the cause: the miss reflects a real routing gap in the wiki.

## Checker and tests

recompute.json commit: 96658ea (pushed, pre-push check OK).

`python scripts/check-wiki-trial.py` after that commit, exit 1:
```
.mob/records/237/trial/recompute.json: recompute mismatch misses wiki-slice-1
.mob/records/237/trial/ledger.json: recompute mismatch aggregates
```
Red by design: it compares my recompute with the ledger, and I recorded my own values. It turns green only when the ledger owner corrects wiki-slice-1 misses to 1 and the aggregates for both arms; I fixed nothing. (Before the commit it also printed "recompute.json: not committed".)

`python -m unittest tests.test_wiki_trial`: Ran 72 tests in 409.275 s, FAILED (failures=1). The one failure is test_real_trial_folder_is_ready (tests/test_wiki_trial.py:627): `AssertionError: 1 != 0 : real trial not ready:` followed by the same two findings as above. The other 71 fixture tests pass.

## Rule-by-rule comparison with thresholds.json

Thresholds are read at their committed value, with the cap amendment. The cap is not in question: 8,868,059 of 15,000,000.

| rule | threshold | result | outcome the rule implies |
|---|---|---|---|
| Token delta | adopt: wiki median at least 20% below baseline in at least 4 of 6 roles; reshape 5% to under 20%; reject under 5% | median delta 41.74%; 5 of 6 roles at or over 20% (green 17.38%) | Adopt. Not in the ambiguous band (5% up to 20%). Caveats: N=1 per cell, one card (227) as the task frame, and counted_tokens sums cache reads over every agentic turn, so it moves with turn count (wiki-green took 24 turns against 19 for baseline and shows the smallest saving). The 41.7% saving is above card 234's 29.7%. |
| Misses | adopt: wiki at or under baseline and 0 wiki misses of 6; reject: wiki over baseline by 2 or more | wiki 1, baseline 0 (excess of 1) | Not adopt (the adopt clause needs 0 wiki misses). Not reject (excess under 2). An excess of exactly 1 is the slice amendment's "ambiguous" case, which sends the trial to stage B; otherwise reshape. The miss has a specific, fixable cause: D136's 15 files and 400 lines sit only on the green page, not on the slice route (architect-slice.md, thin-slices.md). Reshape direction: put the numbers on the slice route and re-run the slice cell. |
| Blocked messages | adopt: wiki count at or under baseline; reject: wiki over by 2 or more; T = 600 s | 0 and 0; longest run 124.6 s | Adopt, but vacuous. The runs were headless one-shot invocations with no message queued behind them, so 0 shows only that no run approached T; it is no evidence about the wiki. Wiki runs were shorter (wall 41.6 to 74.0 s against 80.8 to 128.5 s). |
| Upkeep | at most 12400 tokens per card and at most 10 minutes added retro time; above budget forces reshape | not measured by any run | Unmeasured, so no outcome. It cannot be shown within budget, and the adopt rule requires it. How to estimate (not done here): replay the retro-changed rules of the last K closed cards into the wiki (pages touched, INDEX rows, quote lines) and record tokens and minutes per card; or count from git history the rules changed per card in TEAM_WORKING_AGREEMENT.md and the pages each would touch; or run a staleness diff of agreement rules against the wiki role routes, which would also have caught the slice gap. Build cost is in the card 234 record. |
| Facilitator context growth | slice criterion 1 measure, no threshold | not measured in stage A | Unmeasured. |

Combined view, as an input only: tokens point to adopt, blocked messages are vacuous, misses block a clean adopt by one miss with a known cause, and upkeep is unmeasured. Together the rules do not support adopt now. They imply reshape (put the thin-slice numbers on the slice route, then re-run at least the slice cell) or stage B under the ambiguity clause, and they do not support reject. The Decider decides.

## Commits

- recompute.json: 96658ea.
- This record: committed next (hash in the reply).
