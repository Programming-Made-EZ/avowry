# Card 229 retro input (facilitator; facts only)

Card: a Copilot (and, folded in, Grok) decision seat receives its whole prompt and casts a vote; code in mob-defaults ab8f4a5..2d4d24d (9 commits); signed off with conditions (.mob/records/229/signoff-ctx6.md).

## Flow metrics (UTC, from commits)
- Ready 21:00 on 2026-11-03 (probe), task-force window 21:08 to about 21:58 (two rounds and a re-ask), slice window 22:11, first red 22:39, last green 02:00 on 2026-11-04, live step 02:10 to 02:14, gate SHIP 02:45, sign-off about 03:00: about 6 hours.
- Caps: slice 1+1+1; criterion 2 raised to 2 by window 229-scope and to 3 by window 229-scope-2, each committed before its red (D181). Green 2 cycle 2 could not pass its red (both scope premises wrong in code); its stop rule held: no commit, evidence committed, a new window.
- Windows: 229-taskforce (2 rounds + re-ask), 229-slice, 229-scope (2 rounds), 229-scope-2; plus the D183 merit review. Full suites: 1686 (red 2's 20 expected failures), 1699 OK, 1713 OK.

## Adjustments to review first (D155)
1. D181 (a lettered red ends at the first passing green), first card of its 3-card trial: reds past the last adopted cap with no window before them: 0. Lettered reds: 0. Scope windows: 2, both adopted. Minutes from a failing real-evidence check to the next red: 22:20 green-2 review to the scope window, then 00:50 failed green to the scope-2 window (about 10 minutes each to a proposal).
2. D182's QA condition: red 2 (the empty-vote guard) was committed before green 1: held.
3. D175 caps and scope windows (criterion 2 raised twice).
4. D183 (process improvements serve the story; the board holds stories and bugs), new during the card.

## Observations
- The live check (Dispatch's condition, adopted as part (b) of Done check 1) caught what unit tests could not: Copilot now votes through the fixed launch; Grok receives the prompt but in 8 of 10 window attempts stops after narration, and the first-line window runner dropped the 2 votes it did cast after narration (card 291).
- A diff review of green 2 found unscrubbed failure text and an uncited-vote gap no red caught; the scope that followed rested on a probe that was wrong in code (scrub masks long runs), found only by the green.
- The decision-seat gate placed Copilot in counted seats although the declaration left it out; the gate seat also ran on Copilot, the card's subject.
- Cards filed: 290 (the result counts only cited votes), 291 (Grok ends with a vote; votes after narration read). Card 270 folded in.
- Probe ages crossed 60 minutes twice before an in-flight line (re-probed and rewritten each time).

## Queued inputs to weigh
- D183's sort of the existing process cards at the next re-rank (re-rank 21), with the merit-review procedure.

## Standing section: the feedback-loop lens (D165)
Each voice names, for the 7 loops, who or what gives the feedback, its latency, what the team does with it, and MISSING where none exists.
