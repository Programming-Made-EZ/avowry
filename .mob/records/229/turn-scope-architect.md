# SCOPE PROPOSAL: window 229-scope (criterion 2, cycle 2)

Intent: the vote record that criterion 1's live step commits as evidence never carries a secret or home path, and a vote without a valid D171 cite never moves the result. Source: the facilitator's review of green 2 a2bb1d4; slice C2 "an attempt with no parsed vote is never counted" and the QA condition "every counted vote carries a valid cite".

Assumes: mob_summary.scrub masks secrets, tokens and home paths and never raises (mob_summary.py:41-48); every other stored text in vote_seat goes through it (decider.py:148 clone stderr, :164 reply; parse_vote's reason). Probe: red 2's inputs (TOO_LONG, "x" * 296 + " END") contain no secret or path, so scrub leaves them unchanged.

## Gaps (evidence)

1. decider.py:165 `excerpt = "" if ok else failure_excerpt(text)` uses raw text. Line 168 writes it, and the reason that embeds it (`attempt_reason`, also out["reason"]), into attempt_log, and so into decider-result.json (:352), without scrub.
2. decider.py:339 sets `counted`, but :340 `result_of({s: v["vote"] ...})` still takes every parsed vote. A Yes with no valid CITE counts toward "passed" (result_of, :76-78).

## Smallest scope: C2 cycle 2 of 2

Acceptance: the excerpt is scrubbed first, then trimmed: `failure_excerpt(scrub(text))`. The reason is built from that excerpt, so no stored attempt text skips scrub. result_of takes only counted votes: a parsed vote with cite_ok false is passed in as "none". That is the existing "did not answer" rule, so the result fails and the vote is not counted. Why not drop it instead: dropping it would let a window pass with fewer answering seats. counted_seats is unchanged.
ZOMBIES:
- Z: empty stderr with rc 1: excerpt "", reason "launch failed: rc 1: ".
- O: a token in stderr is REDACTED in both failure_excerpt and reason.
- M: two failed attempts, each scrubbed, kept in order.
- B: a secret that straddles the 300-character cut is masked, not half-kept (scrub runs before trim); the length test applies to the scrubbed text (at most 300 plus the ellipsis).
- I: no stored field in decider-result.json matches contains_secret, and none holds C:\Users\<user>.
- E: scrub on None or non-ASCII text gives no exception.
- S: Yes with a valid cite gives passed. Yes without a valid cite gives failed and the seat is left out of counted_seats. No still fails.

Red 2's 300-character test does not have to change: scrub leaves its inputs unchanged, so it stays green. The new cases go in the cycle-2 red, in the same module (tests/test_launch_failure_honesty.py): a home path, a token, a straddling secret, and uncited Yes means failed. If the seat edits any existing test, that edit is part of this red, and its commit body names it.

Cap: criterion 2, from 1 to 2 cycles. Total 1+2+1 = 4. This red's body: "Cycle 2 of 2 for criterion 2 (D175 C5)" (D181 C2: green a2bb1d4 passed red 858649b, so this is not a lettered red).

Cycles used: C1 1 of 1 (ab8f4a5 / 4674d19). C2 1 of 2 (858649b / a2bb1d4). C3 0 of 1.

## Stop rule

The live step (criterion 1 (b)) does not start until green C2-2 is committed and the facilitator has confirmed that a decider-result.json from a fake failing seat holds no home path. If red C2-2 already passes on a2bb1d4, the cycle is void: no green, and the cap returns to 1. If green C2-2 does not pass its red, there is no cycle 3. The facilitator stops, commits the failing evidence and opens a new window. Nothing ships meanwhile.

## Cut

Out of scope:
- scrubbing inside facilitator.run_backend
- a stdout excerpt
- changes to cite_ok's matching rule
- re-asking an uncited seat (the D171 re-ask stays with the facilitator)
- the live step itself
- criterion 3

Size: about 3.4 KB. One red module edit and about 4 lines of decider.py.
