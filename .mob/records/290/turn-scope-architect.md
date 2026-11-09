# SCOPE PROPOSAL: card 290, D175 C4 window (criterion 1, before green 1)

Intent (D165): decider.py counts a vote only when it has a vote line and a CITE that meets D171 as written (6+ words verbatim, or "line N: <first 4 words>"); any other reply is uncounted.

Assumes (D184, run on HEAD 38b8eb5 in C:\Users\<user>\mob-crosscli, read-only):

```
python -c "import decider; p='alpha beta gamma delta epsilon zeta eta\ntheta iota kappa lambda mu'
for c in [5 words, 6 words, 'line 2: theta iota kappa lambda', 'line 1: alpha beta gamma delta', 'a']: print(c, decider.cite_ok('Yes\nCITE: '+c, p))"
```
Output: 5 words True; 6 words True; line 2 form False; line 1 form False; 'a' True.
Also: a 6-word phrase in double quotes gives False (quotes kept in the match).

- A1. cite_ok (decider.py:132) is a substring test only: no word minimum, so one letter passes.
- A2. The "line N:" form is always False today, even when correct.
- A3. A quoted phrase, which D171 C1 names, is always False today.
- A4. P5 ("two lines in result_of") is false: red 1's B 5-word cases fail on cite_ok, not result_of (red 1 commit body says so).

## Gap

Red 1 (38b8eb5) needs cite_ok to enforce the 6-word minimum. The "line N:" form is untested and wrong today, so a correct line cite would be uncounted after result_of changes.

## Smallest scope (criterion 1 only)

Red 1b (tests only, cycle 1): cite_ok unit cases below, in tests/test_decider_counted_result.py.
Green 1: result_of counts only counted votes (two lines), and cite_ok accepts a CITE when either
- the phrase, outer double quotes stripped, has 6+ words and is found verbatim in the proposal, or
- it is "line N: w1 w2 w3 w4", N is a line of the proposal (1-based), and its first 4 words equal w1..w4.

Acceptance: red 1 and red 1b pass; the 5 guards stay green.

ZOMBIES (cite_ok):
- Z: no CITE line, or empty CITE: False.
- O: 6 verbatim words: True; same in double quotes: True.
- M: two CITE lines: the first is read (as today).
- B: 5 words: False; 6: True; one character off: False; line N past the end, or N = 0: False; line form with 3 words: False.
- I: correct line form: True; wrong 4th word: False.
- E: CRLF reply; extra spaces inside "line N:"; non-ASCII words.
- S: cites already valid today stay True.

Cap: criterion 1, at most 1 cycles (unchanged; no green has run, red 1b is the same cycle). Total 1+1+1 = 3. Size: S to M (cite_ok about 10 lines plus 2 in result_of).

## Stop rule

If green 1 needs decider.py changes outside result_of and cite_ok, or any test outside red 1, red 1b and the C3 file list fails, the developer stops, commits nothing further, records the names, and the facilitator opens a new C4 window. No test is skipped or deleted.

## Cut

- D171's "title line and VOTING RULE line do not count" (needs the proposal's structure; a later card).
- Changes to how D171 is checked by hand (C2).
- Doubles and fake_voter (criterion 3).
- Card 286 (NO OBJECTION stored as No) and card 291 (Grok parsing).
