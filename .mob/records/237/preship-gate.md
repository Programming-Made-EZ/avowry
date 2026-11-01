# Card 237 pre-ship gate (D173, D171), independent reviewer ship, ctx-4

Checked at `2026-11-01T18:05Z` to `18:25Z` from `main...origin/main` (0 ahead, HEAD `50087da` = origin/main) after `git fetch --all --prune --quiet`. Probe given for the card: 2026-11-01 17:35 UTC.
Sources: board/237 card, `.mob/records/237/record-in-progress.json`, `.mob/records/decisions/237-*.json`, `.mob/records/deciders/D173-votes.json`, `.mob/records/237/trial/*`, `.mob/records/237/assessment-pm.md`, git history.
Disclosure: ctx-4 also wrote the stage A recompute (`96658ea`, `85ea412`) and voted in the windows. It is independent of the runner (ctx-5) and the key writer (ctx-3), not of its own recompute.

**Verdict: SHIP.** No blocking finding. Two non-blocking findings (a, b) must be recorded as deviations in the closing record.

| # | Check | Verdict | Evidence |
|---|-------|---------|----------|
| 1 | Done checks met by committed evidence | **PARTIAL, honestly recorded** | See the done-check table below. |
| 2 | Decision trail complete | **PASS** | See the decision table below. |
| 3a | Thresholds before run 1; cap raise followed the amendment | **PASS** | thresholds `139f607` (05:30 -08:00); first run `1281b05` (06:20); amendment window file `4435877` (06:55), ruling `327d635` (06:58, ancestor of the raise), raise `c30b932` (07:03). `git diff 139f607 c30b932` touches only `cap_stage_a`, `cap_stage_ab` and adds `cap_amendment` (last key). Run 2 is `62ab086` (08:28). Exactly 2 commits ever touched thresholds.json. |
| 3b | Key from a non-runner, after the freeze, before run 1 | **PASS** | miss-key.json:3 `author_seat` ctx-3, `written_after_freeze_commit` `01b3058` (05:41); key `b0014b1` (05:47); run 1 `1281b05` (06:20). ledger.json `runner_seats` = `["ctx-5"]` (delivery lead), so the key writer is not the runner. |
| 3c | Independent recompute | **PASS** | recompute.json `author_seat` ctx-4 (not ctx-5), `96658ea`. I re-added the 12 `runs/*.json` usage blocks by the slice rule: total `8868059` (matches ledger and recompute), 5 of 6 roles at or over 20%, median of baseline role totals `911895.5`, wiki `531242.5`, delta `41.74%` (matches D173). Note: the median of the six per-role deltas is `43.9`; D173 and the thresholds use the delta of medians. The recompute found the ledger wrong in 2 places (wiki-slice-1 misses 0 vs 1; stale aggregates) and `ca56511` corrected them. |
| 3d | `python scripts/check-wiki-trial.py` | **PASS** | `wiki-trial: OK (12 rows)` |
| 3e | `python -m unittest tests.test_wiki_trial` | **PASS** | `Ran 72 tests in 420.648s`, `OK` |
| 4 | Main is green | **PASS** | `python scripts/mob_process.py check`: `mob-process: OK (100 closed cards, 9 records, 91 legacy)` |
| 5 | Every commit pushed, none out of order | **PASS** | `git rev-list --count origin/main..HEAD` = 0. 80 commits since 05:00 -08:00: 0 merges, 0 revert/fixup/wip subjects, 0 committer-date regressions, 0 Co-Authored-By trailers. Deviations already in record-in-progress.json: `deviations[1]` subject of `713a8e5` cut off; `deviations[2]` `ca9f617` refused by pre-push and left unpushed a few minutes; `deviations[7]` probe commit 17:37 named after the 17:35 probe. All three are recorded; none is out of order in git. |
| 6a | No seat wrote under .mob/audits/ | **PASS** | `git log --since 2026-11-01T05:00-08:00 --name-only` lists 0 paths under `.mob/audits/`; `git log -- .mob/audits` is empty. Only `last.json` exists. |
| 6b | No delete or stash in committed tool logs | **FAIL on the letter, non-blocking (finding a)** | 12 `*.tools.jsonl`, 167 calls (Bash 60, Read 53, Glob 14, Write 12, Edit 12, PowerShell 9, Grep 7). Scanned for rm, rmdir, del, Remove-Item, git stash, git clean, git rm, reset --hard, rmtree, os.remove. 0 `git stash`, 0 recursive deletes. 1 real delete: `runs/wiki-red-1.tools.jsonl` seq 14, `rm t.tmp` (a literal temp file in that run's own `pilot-output` folder, not recursive, no variable path; `scripts/delete_guard.find_unsafe` returns `[]`). The other hits are task text about card 227 (`rm -rf`, `Remove-Item` quoted in reads and reports). |

## Done checks on board/237 against committed evidence

| Done check | Verdict | Evidence and honest gap |
|---|---|---|
| 1. Measures with committed sources: tokens per seat (more than one run per role), facilitator context growth, messages blocked | **PARTIAL (recorded)** | Tokens: 12 runs, ledger `0e6cb7d`/`ca56511`, recompute `96658ea`. The card says more than one run per role; the pilot ran N=1 per role per arm (2 per role across arms) by the adopted slice condition (window 237-slice, `8708cc5`) and was not widened. Facilitator context growth: not measured. Blocked messages: 0 and 0 but vacuous (headless one-shots). assessment-pm.md:15-17 says all three plainly, and D173 motion A repeats it. |
| 2. Alternatives compared, including doing nothing, each costed | **MET** | assessment-pm.md section 2: (i) adopt as is, (ii) reshape, (iii) stage B, (v) do nothing, each with a cost column. The upkeep cost is an estimate only (unmeasured). |
| 3. Decider decides; also decides the retro-end review, as an experiment | **MET** | D173 (docs/decisions/process-and-staffing.md:690-695), commit `0b48026`; motion A RESHAPE, motion B three-retro experiment; follow-up card 238 filed in `50087da`. |

## Decision trail (D153, D171)

Every window polled all five non-proposer declared roles and counted five NO OBJECTION replies in its final round.

| Window | Proposer | Counted round | Polled and counted | Cites |
|---|---|---|---|---|
| 237-taskforce | ctx-1 | 2 | ctx-2, 3, 4, 5, 6 (5/5) | `cite_ok` is null (older gate); by script 5/5 cites found verbatim in the proposal, 9 to 13 words |
| 237-slice | ctx-2 | 2 | ctx-1, 3, 4, 5, 6 (5/5) | `cite_ok` true 5/5 |
| 237-key-secrecy | ctx-3 | 1 | ctx-1, 2, 4, 5, 6 (5/5) | `cite_ok` true 5/5 |
| 237-cap-fired | ctx-2 | 2 | ctx-1, 3, 4, 5, 6 (5/5) | `cite_ok` true 5/5; the round 1 QA cite was unread and re-polled, as the decision records |
| 237-amendment-ordering | ctx-3 | 1 | ctx-1, 2, 4, 5, 6 (5/5) | `cite_ok` true 5/5 |
| D173 (votes JSON) | ctx-1 | round 1 and independent re-run | 6 of 6 seats each round, all Claude | by script 12/12 cites found verbatim in assessment-pm.md at `d2ad79f`, 6 to 13 words |

D173 has both rounds: round 1 motion A 6 Yes, motion B 2 Yes + 4 Support-it; re-run motion A 5 Yes + 1 Support-it, motion B 1 Yes + 5 Support-it; no No in either. Notes: the proposer's seat (pmready) voted in round 1, and the re-run replaced it with a coach seat. Both rounds ran on Claude because Copilot seats returned no reply (card 229, in the record's observations).

## Findings

a. **Non-blocking, record as a deviation.** `rm t.tmp` in `wiki-red-1.tools.jsonl` seq 14: a trial subject removed its own literal scratch file. No effect on any measure or on the key. The recompute guard (recompute.json `guard`) checked reads of `.mob/records/237/` only, and record-in-progress.json `deviations` has no entry for this.
b. **Non-blocking.** The 12 `*.session.jsonl` files in `.mob/records/237/trial/runs/` are untracked (`git ls-files` shows 24 tracked files there: 12 json + 12 tools.jsonl; 0 session). recompute.json `guard` says it checked "12 session logs" for key reads, and that part cannot be reproduced from git. The committed tool logs were enough for me to see no read of the key. Either commit the session logs or record that they stayed local.
c. Untracked `.mob/records/234/scratch/` belongs to card 234 and is left out of this commit.
d. Facilitator context growth and upkeep are unmeasured; D173 carries both into card 238.

## Role mapping for the record checker (open_points)

Mapping: ready = ctx-1, slice = ctx-2, red = ctx-3, green = ctx-5 (delivery lead running the trial), ship = ctx-4, signoff = ctx-6.

- **Disclosed:** yes, in record-in-progress.json `open_points[0]` and the taskforce decision note (no green seat declared).
- **Evidenced for ctx-5:** yes as work: `turn-pilot-delivery.md` (`80f6b37`), ledger.json `runner_seats` ctx-5, and the 12 run-artifact commits from `1281b05` to `6e35665`.
- **Honest as "green":** only as a label. ctx-5 wrote no failing-to-passing change. The red/green code cycles of this card (`08aff53` to `3536229`, 16 commits) were QA ctx-3's, per `turn-ordering-qa.md`. The mapping is acceptable for the record checker because it is stated up front; the closing record should say "green = trial run, no product code" and not claim a TDD green for ctx-5.
