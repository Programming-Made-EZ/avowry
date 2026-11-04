Intent: decider.py's result counts a vote only when it has a recognised vote line and a D171-valid CITE found verbatim in the proposal; any other reply is unread and left out, whatever its vote word, so an uncited Yes never makes a Decider pass (card 290, adopted window 290-taskforce).

## Premises (D184)

Source for P1 to P5: .mob/records/290/record-in-progress.json decisions.slice_attempt_1 and .mob/records/290/slice-probe-stopped.diff (first slice attempt, stopped). Committed as given; not re-run in this turn.

- P1. The six decider modules (decider_launched, gate, pin_scrub, record, record_units, runner) pass at HEAD: 201 tests, OK.
- P2. result_of counting only counted votes fails 27 of those 201 (doubles in tests/fakes/fake_voter.py vote a bare "Yes").
- P3. With cites added to the doubles in the six modules, the 201 pass again.
- P4. Wider modules then failed: one run of 73 tests had 2 failures, one of 81 tests had 9.
- P5. The real decider.py change is two lines; the rest of that diff is line-ending churn.
- P6 (run this turn, probe worktree wt-290-probe holding P2 and P3's changes):

```
python -m unittest discover -s tests -p "test_*.py" 2>&1 | findstr /B /C:"FAIL:" /C:"ERROR:" /C:"Ran " /C:"OK" /C:"FAILED"
```
Output:
```
unrun: no summary line after about 15 minutes (the architect's turn budget);
the run was left going, not killed. Output file: tasks/bdljq3ham.output.
```
If P6 stays unrun: red 3's first step is this command on HEAD plus the
result_of change, and its FAIL/ERROR names become the C3 file list.

If P5 is false (the change is wider than result_of), the green stops and the stop rule applies.

## Domain-expert (T2) answer

Question: what evidence makes a vote valid enough to change a decision result: a valid proposal cite, the vote line, or both?

Answer: both. A vote changes the result only when (a) its reply carries a recognised vote line (Yes, Support-it, No, or the window's OBJECTION / NO OBJECTION) and (b) its CITE meets D171 and is found verbatim in the proposal (cite_ok true). A vote line without a valid cite is unread: recorded with its vote and cite_ok false, counted false, and left out of result_of. A cite without a vote line is unread too. Symmetric: an uncited No cannot fail a decision.

This card proves: C1 uncited votes are uncounted, C2 cited votes count as today, C3 the doubles carry cites and every decider test passes. Not here: card 286 (NO OBJECTION stored as No); card 291 (Grok vote parsing).

## Criterion 1 (Done check 1): an uncited vote is not counted and cannot change the result

Acceptance: result_of reads only votes with counted true (vote line and cite_ok). The two cases moved from card 229's red 7c26629: an uncited Yes alone does not pass; an uncited Yes beside cited votes cannot tip the result. Per the adopted condition, tests also prove an uncited Support-it and an uncited No are uncounted and do not change the result. Each uncounted seat is named in a finding with its vote and cite_ok.
ZOMBIES: Z no counted votes: not passed, "no counted votes". O one uncited Yes: not passed. M uncited Yes, Support-it and No beside cited votes: result equals the cited-only result. B a CITE one character off the proposal text: cite_ok false, uncounted; a cite under the D171 word minimum: uncounted. I finding names seat, vote and cite_ok; counted_seats excludes it. E vote line present, CITE line missing; CITE present, vote line missing. S cited-only runs unchanged.
Cap: criterion 1, at most 1 cycles
Size: S (two lines in result_of, one red module).

## Criterion 2 (Done check 2): a cited Yes passes and a cited No fails, as today

Acceptance: with every vote cited and found in the proposal, result_of returns what HEAD returns for the same votes: all-cited Yes and Support-it passes; a cited No fails. A pinned test per case, green at HEAD and after the change.
ZOMBIES: Z n/a (covered by C1 Z). O one cited Yes passes. M cited Yes, Support-it, No mixes match HEAD's result. B cite equal to the whole proposal line. I result field and counted_seats agree. E none. S decider-result.json shape unchanged.
Cap: criterion 2, at most 1 cycles
Size: XS.

## Criterion 3 (Done check 3): the doubles carry valid cites, and every decider test module passes

Acceptance: every double that means a counted vote emits a CITE found verbatim in its test's proposal (per file or through one fake_voter helper; the developer picks, the window left it open). Doubles that mean an uncited vote stay uncited. The full discovery run above reports OK with no FAIL or ERROR line. Files to change (from P6 and P3):
- known (P2, P3): tests/fakes/fake_voter.py and the test_* files of the six modules (decider_launched, gate, pin_scrub, record, record_units, runner).
- outside the six: unknown, because P6 is unrun. P4 measured 2 + 9 failures in wider runs, with no names recorded. Red 3 names them from the P6 command before any edit.
ZOMBIES: Z a test with no proposal text: its double still counts only if it cites. O one double cited. M all 27 in the six modules plus the files above. B a double whose cite is the first or last proposal line. I the full discovery count is unchanged from HEAD (no test deleted or skipped). E a test that asserts an uncited vote counted: rewritten to assert the new rule, named in the green. S window recorders untouched.
Cap: criterion 3, at most 1 cycles
Size: S (test doubles only).

Caps: 1+1+1 = 3 (D175). D181: a red after a passed green is a new cycle and needs a window first.

## Out of scope

- Card 286: NO OBJECTION stored as No. Not fixed here even if seen in a double.
- Card 291: Grok narration before the vote line. Grok stays out of counted seats.

## Stop rule

If after green 1 any test outside the files listed in C3 fails, or the decider.py change grows past result_of, the developer stops, commits nothing further, records the failing names, and the facilitator opens a D175 C4 scope window before any further red. No test is skipped or deleted to reach OK.
