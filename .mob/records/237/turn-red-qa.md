# Card 237 pre-trial red (step 0a): QA engineer (ctx-3)

Commits: 744e9e3 (checker, fixtures, tests); 74b1c39 (MOB.md regenerated after the pre-push check refused with "4 non-MOB commits stale"; fixed with a new commit, no amend).

Intent (D165): the Decider and reviewer asked that no trial number be believed unless thresholds, cap, freeze and a non-runner miss key were committed in order before the first run and the ledger matches the reviewer recompute (window 237-slice, condition 2).

Assumes (D166): git ancestry, not file dates or contents, is an honest order of commits on trunk; probe: tests/test_wiki_trial.py builds throwaway git repos with commits in forged orders and the checker rejects each.

Hostile inputs (D167):
- forged commit order: key before freeze; key committed with the ledger; thresholds after the run; key rewritten after the run; freeze rewritten after the run; run report before the key.
- runner-authored key: ctx-5; a seat the ledger names as runner; the freeze author; no author; a runner-authored recompute.
- recompute mismatch: counted_tokens, misses, aggregates (both files), a missing recompute row, counted_tokens rule broken.
- missing files: each of the five.
- malformed JSON: each of the five, and a JSON array.
- rows: duplicate, unknown, disabled, reordered (still passes), bad shape, source not committed or escaping the root, cap exceeded, cap_stage_ab below cap_stage_a, missing threshold.

## What the red asserts

scripts/check-wiki-trial.py exits 1 unless: all five files under .mob/records/237/trial/ exist, parse and are committed; thresholds and cap committed before the first run (earliest commit adding the ledger or trial/runs/) and before the ledger; freeze and key not changed at or after the first run; key first committed after the freeze's last commit and before the first run (git merge-base --is-ancestor, strict); key author not a runner seat (ctx-5 plus ledger runner_seats) and not the freeze author; every ledger row has a committed source, a known unique enabled key, counted_tokens = input_total + output_total and equals the reviewer recompute (counted_tokens, misses, per-arm medians, delta); total counted tokens within the cap.

Red shape chosen: "committed failing before any run" and "fails when a file is missing" (slice wording). The checker fails when the trial folder is absent, so test_real_trial_folder_is_ready is the red; the 34 fixture tests pass and pin each failing case. The checker is NOT wired into scripts/mob_process.py check, so the pre-push check on main stays green (it was, the push succeeded). The green seat (step 0d) wires it once the folder exists.

## Failing output

`python scripts/check-wiki-trial.py` exit 1:
```
.mob/records/237/trial/thresholds.json: missing file
.mob/records/237/trial/freeze.json: missing file
.mob/records/237/trial/miss-key.json: missing file
.mob/records/237/trial/ledger.json: missing file
.mob/records/237/trial/recompute.json: missing file
```
`python -m unittest tests.test_wiki_trial`: Ran 35 tests, FAILED (failures=1): test_real_trial_folder_is_ready (AssertionError: 1 != 0 : real trial not ready: ... missing file).

Formats the checker expects (for the 0b to 0d seats): thresholds keys token_delta_adopt_pct, token_delta_reject_pct, blocked_seconds, upkeep_tokens_per_card, upkeep_minutes, cap_stage_a, cap_stage_ab (positive numbers); freeze {author_seat, ...}; miss-key {author_seat, keys:[{run_key, rule}]}; ledger {runner_seats, rows:[{run_key, arm (baseline|wiki), role, stage, input_total, cached_input, output_total, counted_tokens, misses, source}], aggregates:{median_counted_tokens:{arm:n}, delta_pct}}; recompute {author_seat, rows:[{run_key, counted_tokens, misses}], aggregates}.

Not done: the fixture suite takes about 112 s (35 git repos on Windows); wiring into the pre-push check is left to the green seat.

Size: 23629 bytes
