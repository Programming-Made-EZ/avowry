# md63 green record

Seat: green (dev, green), composer-2.5, ctx-63g. Branch card-63 at fa83b4d, worktree ~/projects/wt-md63. Not committed.

## git diff --stat

```
 decider.py     |  4 +++-
 facilitator.py | 24 ++++++++++++++++++++----
 2 files changed, 23 insertions(+), 5 deletions(-)
```

## What changed

- `facilitator.reply_first_line_is_vote`: one helper; first non-empty line must start with `Yes`, `Support-it`, `No`, or `ABSOLUTELY NO` (any case) then optional whitespace and punctuation (`.`, `,`, `!`, `:`, `;`, `-`) or end of line.
- `facilitator.run_backend` / `_launch_backend`: new `expect_vote=False`; short successful rate-limit-shaped replies skip the discard when `expect_vote` and the helper both say vote line.
- `decider.vote_seat`: passes `expect_vote=True` into `run_backend`.
- `decider.attempt_reason`: `ok` false and `rc == 0` → `read as a usage-limit notice: <excerpt>`; `rc` nonzero launch failures unchanged; `rc` None still uses facilitator excerpt text.

Unchanged per slice: `RATE_LIMIT_SIGNS`, `RATE_LIMIT_REPLY_MAX_CHARS`, `looks_rate_limited`, probe path.

## Runs

- `python -m pytest tests/test_vote_reply_not_limit.py -q --capture=sys`: **1 failed, 37 passed, 14 subtests passed** in 3.02s  
  - Failure: `test_63_i_vote_seat_counts_a_vote_that_mentions_a_quota_end_to_end` — end-to-end path now returns `out["vote"] == "Yes"` (canonical `parse_vote` / `VOTES`); test asserts `"yes"`. Production behaviour matches existing decider tests (`test_fake_voter_cites`, etc.); assertion looks like a red typo. Green did not edit tests.
- `python -m pytest tests/test_decider_record_units.py tests/test_decider_record.py tests/test_decider_launched.py tests/test_decider_gate.py -q --capture=sys`: **94 passed, 17 subtests passed** in 119.38s

## Fix round 1 green (R63-2)

### diff

```diff
diff --git a/facilitator.py b/facilitator.py
index aa6cb4a..109cb06 100644
--- a/facilitator.py
+++ b/facilitator.py
@@ -224,7 +224,7 @@ def looks_rate_limited(text):
 
 
 _VOTE_FIRST_LINE = re.compile(
-    r"^(?:ABSOLUTELY\s+NO|Support-it|Yes|No)\s*([.,!:;\-]|$)",
+    r"^(?:ABSOLUTELY\s+NO|Support-it|Yes|No)\s*([.,!:;\-\u2013\u2014]|$)",
     re.IGNORECASE,
 )
```

### run

`python -m pytest tests/test_vote_reply_not_limit.py tests/test_decider_record_units.py -q --capture=sys`: **60 passed, 14 subtests passed** in 6.05s