# SCOPE PROPOSAL: card 290, D175 C4 window (criterion 1, cycle 2)

Intent (D165): a vote counts only with a vote line and a D171-valid CITE; any other reply is left out of the result (task force 290). A seat that never answered is not a reply, and still fails the result.

Assumes (D184, run on HEAD 3d503c8 in C:\Users\<user>\mob-crosscli, read-only):

```
python -c "import decider as d; print(d.result_of({'a':'Yes','b':'Yes'})); print(d.result_of({'a':'Yes','b':'Yes','c':'none'}))"
```
Output: `passed`, then `failed`.

- A1. result_of already fails on "none" (decider.py:81); no change there.
- A2. The call site is decider.py:353 on HEAD (340 at 93c4f62): `result_of({... if v["counted"]})`, and counted is False for "none" (line 352), so "none" never reaches result_of.
- A3. Unrun: test_a_seat_that_never_answers_counts_as_no_response_and_fails gets "passed" once the doubles cite (green 3 attempt 1, stated in the record).

## Gap

Green 1 (93c4f62) left out "did not answer" with uncited replies. No red covered a missing seat beside cited votes.

## Smallest scope (criterion 1, cycle 2)

Red 1c (tests only): with cited Yes votes from the other seats, a seat with vote "none" makes the result "failed"; an uncited No beside cited Yes votes still gives "passed".
Green 1b: at the same call site, pass a vote to result_of when it is counted or its vote is "none". No other decider.py change.

Acceptance: red 1c, red 1, red 1b, red 2 pass; the five required modules stay green; the other failures change only by the never-answers case.

ZOMBIES (call site):
- Z: every seat "none": failed. No seat counted, none missing: "failed: no counted votes".
- O: one "none" beside cited Yes: failed.
- M: two "none" plus cited Yes: failed.
- B: uncited No plus "none": failed (from "none" only).
- I: uncited Yes/Support-it/No alone beside cited Yes: passed.
- E: a seat that timed out records "none": failed.
- S: all cited Yes: passed (as today).

Cap: criterion 1 from 1 to 2 cycles; total 2+1+1 = 4. Size: S (one line plus tests).

## Stop rule

If green 1b needs any decider.py change beyond line 353, or a test outside red 1c and the fixed never-answers case changes result, the developer stops, commits nothing, records names, and the facilitator opens a new C4 window. No test is skipped or deleted.

## After this: green 3

Green 3 re-applies .mob/records/290/green3-attempt-1.diff (the fake_voter change) and re-runs; the runner must then exit 1 for a never-answering seat, so that test passes. No production change in green 3.

## Cut

- Changing result_of or cite_ok.
- Reworking the "unread" record field.
- Cards 286 and 291.
