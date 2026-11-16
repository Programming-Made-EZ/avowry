SIGN-OFF

# md63 review round 2 (agy, gemini-3.8-flash-medium, fresh context)

Range `git diff 220b674..7658bf2` (card-63 fix round 1 for ruling R63-2). Reviewed in read-only clone. No repo file edited.

## Checks against R63-2
1. **The change does exactly R63-2 and nothing else:**
   - In `facilitator.py:227`, `_VOTE_FIRST_LINE` character class for trailing punctuation was updated from `[.,!:;\-]` to `[.,!:;\-\u2013\u2014]`, accepting en dash (`–`, U+2013) and em dash (`—`, U+2014).
   - No other logic in `facilitator.py` or elsewhere was modified.
2. **The em and en dash are accepted after every vote word:**
   - Verified that `_VOTE_FIRST_LINE` and `facilitator.reply_first_line_is_vote` accept both `\u2014` and `\u2013` following every vote word (`Yes`, `No`, `Support-it`, `ABSOLUTELY NO`, case-insensitive), both when followed immediately by the dash or separated by whitespace (e.g. `Yes — ...`, `Yes—...`, `No – ...`, `No–...`).
3. **Negative cases stay not ok:**
   - `Yes (` (parenthesis), `Vote: Yes` (preamble on first line), and `No quota left` (vote word followed by space and another word) do not match `_VOTE_FIRST_LINE` and continue to fail `reply_first_line_is_vote`. When `looks_rate_limited` is true, these are preserved as not ok.
4. **No test was edited except the added Test63Fix1:**
   - In `tests/test_vote_reply_not_limit.py`, 4 new tests were added under `Test63Fix1` covering em dash, en dash, parenthesis, and preamble. Zero existing test assertions or test methods were modified.

## Findings
- Round 1 MINOR 1 (`Yes — ...` and `Yes – ...` rejected by `_VOTE_FIRST_LINE`) is resolved cleanly by this change.
- Round 1 MINOR 2 (notice starting with a vote word and punctuation) remains an acknowledged named limit on the card per R63-2; uncounted replies are omitted from tallies via CITE validation.
- No blocking or minor defects found in the round 1 fix.

## Run
`python -m pytest tests/test_vote_reply_not_limit.py tests/test_decider_record_units.py -q --capture=sys`: 60 passed, 14 subtests passed in 4.28s
