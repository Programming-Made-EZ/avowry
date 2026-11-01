# Card 237: cap raise (decider, ctx-6)

Intent (D165): land the one-commit cap raise adopted in window 237-cap-fired, before run 2, so git shows the new cap postdates no outcome data.

Assumes (D166): ruling 327d635 is the key-probe ruling the condition requires first; probe: git log shows 327d635 is an ancestor of this commit, and thresholds.json diff touches only the two caps plus cap_amendment.

## Values

| field | before | after |
|---|---|---|
| cap_stage_a | 1000000 | 15000000 |
| cap_stage_ab | 2000000 | 30000000 |
| cap_amendment | absent | added: old and new values, window 237-cap-fired, ruling 327d635, sized from one baseline run's cost only |

## Commit

Cap raise: c30b932 (thresholds.json alone, pushed).

## Checker output

`python scripts/check-wiki-trial.py` exits 1 with one finding: `.mob/records/237/trial/recompute.json: missing file`. `python -m unittest tests.test_wiki_trial`: 35 tests, 1 failure (test_real_trial_folder_is_ready, same missing recompute.json message; it failed the same way before this commit).

## Finding: the ordering check is masked, and will fail

check_order returns early while recompute.json is uncommitted, so the ordering check did not run. I ran check_order with a stand-in for the missing commit. It reports for thresholds.json: "committed or changed at or after the first run" and "thresholds not committed before the ledger". Once recompute.json is committed, the checker will reject this raise as a forged order. I did not change the checker. The team must decide how the checker treats a recorded amendment (for example, accept a thresholds commit that is named by a cap_amendment and postdates only cost data).
