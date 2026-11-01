# Card 237: turn record, checker accepts a recorded cap-only raise (QA, ctx-3)

Intent (D165): make scripts/check-wiki-trial.py judge thresholds.json by content against the file at the first run (window 237-amendment-ordering, option a), so the recorded cap raise passes and a forged or widened edit does not, and make an ordering failure show up even when another trial file is missing. Outside-in TDD, one criterion per cycle, a red commit pushed and then a green commit pushed.

Assumes (D166): the window decision (.mob/records/decisions/237-amendment-ordering.json), the cap raise c30b932, its ruling 327d635 and the first-run commit 1281b05 are what git says; probe 2026-11-01 14:43 UTC, plus `git log` and `git show` on the real repo. I did not read the miss key. thresholds.json, the key, the freeze, the ledger and the runs are unchanged.

## Hostile inputs (D167), each with its own test

| # | Case | Test (tests/test_wiki_trial.py) | Cycle |
|---|------|---------------------------------|-------|
| 1 | Lowered cap, either one | test_amendment_that_lowers_cap_stage_a / _ab | 4 |
| 2 | Caps equal, cap_amendment added | test_pin_amendment_with_equal_caps_passes (pin) | 4 |
| 3 | Delta threshold, blocked_seconds, cap_reached changed | three tests named test_amendment_that_changes_* | 3 |
| 4 | Key removed, renamed, extra key added | test_amendment_that_removes_a_key / renames_a_key / adds_another_key | 3 |
| 5 | cap_amendment edited in a second change | test_cap_amendment_edited_in_a_second_change; also test_amendment_that_edits_a_cap_amendment_present_at_the_first_run | 8, 3 |
| 6 | Caps raised, cap_amendment absent | test_amendment_without_a_cap_amendment | 5 |
| 7 | Decision file does not exist | test_amendment_naming_a_decision_file_that_does_not_exist | 5 |
| 8 | Decision file working-tree only, other branch, added after the change | three tests named test_amendment_with_a_decision_file_* | 5 |
| 9 | Decision file added in the change commit | test_amendment_with_a_decision_file_added_in_the_change_commit | 5 |
| 10 | Decision path with `..`, absolute, or outside the folder | test_amendment_window_that_escapes_the_decisions_folder (four spellings, decoy file in git) | 5 |
| 11 | Ruling missing, too short, tree, blob, branch posing as a hash | four tests named test_amendment_*ruling* (tree and blob share one test) | 6 |
| 12 | Ruling on another branch | test_amendment_whose_ruling_is_not_an_ancestor_of_the_change | 6 |
| 13 | Ruling equal to the change commit | not buildable (the change commit's hash would sit in its own content); the ancestor test is strict (older != newer) | none |
| 14 | Amendment after run 2 | test_amendment_that_lands_after_run_2, plus accept side test_pin_amendment_before_run_2_passes | 7 |
| 15 | Two thresholds commits, each fine alone | test_two_amendment_commits_each_fine_alone | 8 |
| 16 | Non-integer, bool, string, negative cap | test_amendment_with_a_float_cap (red); test_pin_amendment_with_bad_cap_values (pin) | 4 |
| 17 | Whitespace-only rewrite after the first run | test_whitespace_only_rewrite_after_the_first_run_passes | 2 |
| 18 | cap_amendment lacks window text or ruling text | test_amendment_naming_no_window; test_amendment_naming_no_ruling_commit | 5, 6 |
| 19 | Cap exceeded judged on the new value | test_pin_raised_cap_is_what_the_total_is_judged_against (pin) | 4 |
| 20 | One file missing or malformed, ordering still printed | three tests in cycle 1 | 1 |
| 21 | First run falls back to the ledger commit | test_amendment_that_lowers_a_cap_when_the_first_run_is_the_ledger_commit | 4 |

Tests marked pin passed when added (the old rules already held); each other test was shown failing before its green commit. Cycles 3, 4, 5 and 6 each group closely related cases; every case has its own test.

## Cycles

| Cycle | Criterion | Red | Green |
|-------|-----------|-----|-------|
| 1 | Ordering and row findings print even when a file is missing or malformed (early return at :149-150 and the `if not findings:` gate at :198 removed) | 08aff53 | 9765881 |
| 2 | Thresholds judged by content against the file at the first run (recorded raise and whitespace-only rewrite pass) | b74a5f0 | d357a5d |
| 3 | Changed keys limited to cap_stage_a, cap_stage_ab and an added cap_amendment; every other field matches the first run | c7ecad1 | 0ae30c0 |
| 4 | Caps only rise, and stay integers | 2c54dbc | f77c64d |
| 5 | cap_amendment names a decision file under .mob/records/decisions/ that is in git before the change commit | 56f14c5 | 8e27cce |
| 6 | The ruling commit it names is a real commit and a strict ancestor of the change commit | 961171c | 6fbcddb |
| 7 | The change lands before run 2 | 6d82144 | c0bd9bf |
| 8 | One thresholds commit after the first run | f2a74b9 | 3536229 |

The pre-push check refused four times as MOB.md was stale; each time I ran `python scripts/mob_process.py write-mob-md` and committed MOB.md alone: 57d8cc2, b2003c1, e1eba98, fc27cfb.

Note on cycle 2: its green is deliberately the loosest change that passes (any parsable thresholds at the first run is accepted, except identical content). Cycles 3 to 8 then tightened it, each against a test that failed on the looser code. The checker is not wired into the pre-push check, so the intermediate commits gated nothing.

Known gap, not in the list: the checker judges the working-tree thresholds.json while git decides the order, so an uncommitted edit to thresholds.json is not compared with HEAD. Offered as a follow-up, not done.

## Final check on the real repo state

`python scripts/check-wiki-trial.py` (exit 1):

```
.mob/records/237/trial/recompute.json: missing file
.mob/records/237/trial/recompute.json: not committed
```

No ordering finding: thresholds.json at the first run (1281b05) differs from HEAD only in cap_stage_a, cap_stage_ab and the added cap_amendment (c30b932), caps rose, decision 237-cap-fired was added before c30b932, ruling 327d635 is an ancestor of it, and no run row was first committed after the first run. Only recompute.json is missing.

`python -m unittest tests.test_wiki_trial`: Ran 72 tests, FAILED (failures=1). The one failure is test_real_trial_folder_is_ready, which reports "real trial not ready: .mob/records/237/trial/recompute.json: missing file". The other 71 pass (35 tests existed before this turn, 37 are new).

Size: 30135 bytes (git diff a9e02b6..HEAD over scripts/check-wiki-trial.py and tests/test_wiki_trial.py; 437 insertions, 27 deletions)
