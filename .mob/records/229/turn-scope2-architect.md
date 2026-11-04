# SCOPE PROPOSAL: window 229-scope-2 (criterion 2, cycle 3)

Intent: decider-result.json, which criterion 1's live step commits, never stores a secret or home path in failure text. This carries forward 229-scope's first gap only.

Assumes (read in code, not inferred):
- mob_summary.py:33 masks any run of 48 or more letters, digits, `+` or `/` as a token.
- In tests/test_launch_failure_honesty.py, Boundaries uses "e" * 400 (:172) and "x" * 296 (:180). Both are such runs, so scrub-then-trim breaks those tests by their input, not by the rule.
- tests/fakes/fake_voter.py:43 replies a bare "Yes" with no CITE. Every double built on it is uncited, which is why 27 tests fail once the result counts only cited votes.

## Evidence

- .mob/records/229/green2-cycle2-attempt.diff: two decider.py lines.
- record-in-progress.json, decisions.green_2_cycle_2: both premises failed; no commit; tree equals red 7c26629.

## Options

- (a) Keep scrub-then-trim and change the inputs of the two Boundaries tests to spaced words, for example `"word " * 80`. The assertions are unchanged: the length limit, the TAIL-MARK end, and an exact-300 text kept whole. Cost: one test module, input lines only. Risk: none to the rule. The tests stop exercising a single long run, and the I case still covers tokens.
- (b) Move "the result counts only cited votes" out of this card. Today cite_ok and counted_seats are already recorded (a2bb1d4), and the facilitator's window recorders enforce D171 (cite_ok in window JSON, for example 229-taskforce.json). The result field is the only gap. File it as a bug card (D183 allows bugs), "decider.py's result counts a parsed vote with no valid D171 cite". Its first red reuses this card's uncited-Yes cases, and its scope includes the fake_voter default reply. Cost here: none.
- (c) Keep both and give the 27 doubles valid cites across six modules (decider_launched, gate, pin_scrub, record, record_units, runner). Cost: about 30 test edits in six files, past one cycle's size, and churn in tests that prove other things. Rejected.

Choice: (a) plus (b).

## Scope: C2 cycle 3 of 3

Red (new; its commit body names every edit):
- Boundaries :172 and :180: change the inputs to spaced words; assertions unchanged.
- Remove from CycleTwoSimple :435 (uncited Yes fails) and :445 (uncited beside cited fails). The body says "moved to bug card <n>; result rule out of card 229 per window 229-scope-2".
- Keep :430 (cited Yes passes) and :450 (No fails) as guards.
- Keep every scrub case (Z, O, M, B, I, E) as committed in 7c26629.

Green: `failure_excerpt` returns `one_line(scrub(text), 300)`, and the reason is built from it. result_of is unchanged.

Acceptance: a token or home path in stderr never reaches attempt_log or reason; the length rule applies to the scrubbed text; the full suite modules named in a2bb1d4 stay green.

ZOMBIES:
- Z: empty stderr gives an empty excerpt.
- O: a token and a home path are masked in both the excerpt and the reason.
- M: two attempts, each masked, in order.
- B: a token straddling the 300 cut is masked; the spaced 400-word text is cut to 300 plus the ellipsis; exactly 300 is kept whole.
- I: no stored string matches contains_secret or holds a home path.
- E: None and non-ASCII text give no exception.
- S: cited Yes passes and No fails, as today. An uncited Yes is still recorded with counted false.

Cap: criterion 2, from 2 to 3 cycles. Total 1+3+1 = 5.

Cycles used: C1 1 of 1. C2 2 of 3: red 858649b / green a2bb1d4, then red 7c26629 with no green.

## Stop rule

If green C2-3 does not pass its red, there is no cycle 4. The facilitator commits the failing evidence and the card holds. The live step (criterion 1 (b)) does not start until C2-3's green is committed and a fake failing seat's decider-result.json is shown free of home paths. The bug card is filed before the red, so the moved cases have a home.

## Cut

Out of card 229:
- the cited-only result rule (to the bug card)
- changes to the 27 doubles or to fake_voter
- changes to scrub's patterns
- scrubbing inside facilitator.run_backend
- criterion 3

Size: about 3.6 KB. One test module edited and one decider.py line.
