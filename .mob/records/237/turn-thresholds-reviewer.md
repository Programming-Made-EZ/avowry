# Card 237 step 0b: thresholds.json (independent reviewer, ctx-4)

Intent (D165): commit the adopt, reshape and reject numbers and the token caps before any trial data exists, so git history shows they were set first (window 237-slice, condition 3).

Assumes (D166): the slice amendment section 3 states every number the checker reads, so none needs inventing; probe: each of the seven keys in THRESHOLD_KEYS of scripts/check-wiki-trial.py was matched to a slice line below, including the blocked threshold T (600 s).

Hostile inputs (D167): this file takes no list input. Checker-side cases already pinned by the red seat: missing threshold, non-positive or boolean value, cap_stage_ab below cap_stage_a, thresholds committed at or after the first run or the ledger.

## Numbers and their slice line (section 3 of the amendment)

- token_delta_adopt_pct 20, with token_delta_adopt_min_roles 4 of 6: "adopt needs wiki median counted_tokens at least 20% below baseline, in at least 4 of 6 roles".
- token_delta_reject_pct 5 (reshape band 5 to under 20): "Reshape from 5% to under 20%. Reject under 5%".
- Misses: "adopt needs wiki misses at or under baseline, and 0 wiki misses of 6 keys in stage A. Reject if wiki misses exceed baseline by 2 or more".
- blocked_seconds 600 (this is T): "waits over 600 s (10 minutes) behind a long turn"; adopt at or under baseline, reject at 2 or more over.
- upkeep_tokens_per_card 12400 and upkeep_minutes 10: "at most 12400 tokens per card ... at most 10 minutes added retro time"; above budget forces reshape.
- cap_stage_a 1000000 and cap_stage_ab 2000000: "stage A 1,000,000 counted tokens, stages A and B together 2,000,000".

Extra keys in the file are only restatements of these lines; none is a new number.

## Checker output (after the thresholds commit)

```
.mob/records/237/trial/freeze.json: missing file
.mob/records/237/trial/miss-key.json: missing file
.mob/records/237/trial/ledger.json: missing file
.mob/records/237/trial/recompute.json: missing file
```

## Commits

- 139f607 thresholds.json (pushed to main).
- This record: see the next commit on main.

Open points: none. T was stated.
