# Card 227 retro input (facilitator; facts only)

Card: recursive deletes on variable paths are blocked and every commit is pushed (stop-the-line corrective for F-071 and audit 62; the product owner's safety ruling of 2026-10-31; code in mob-defaults). Signed off as "detected, not blocked" (d060208): the scan reads every executed command and fails the record; prevention before a command runs is card 228's opt-in hook, and the product owner's "block" question is open.

## Flow metrics (UTC, from commits)
- Filed 08:2x; re-rank 13 window 08:47 to 08:58; ready window 09:12 to 09:28 (one adopted condition: the scan sees executed commands); slice 09:45, security review at slice 09:51, amendment 09:57 (follow-up card 228 filed), slice window 10:13 (55 KB slice, pointer prompt).
- Criterion 1 (scan): red 10:26 (524171f), green 11:13 (2fc4f98); stand-down 1 found 4 of 8 plan items undone; increment 1b red 12:05, green 12:46.
- Criterion 2 (trust ladder): red 13:00, green 13:31. Stand-down 2 13:51: the scan reached 0 of 3 real seats (Agent-tool seats); facilitator decision e7054a7 (hand scans of every seat transcript).
- Criterion 3 (push detector): red 14:21, green 15:12.
- Reviews 15:35 (both CHANGES REQUESTED, 27 findings); triage 15:43 (ten blocking kept, seven lines to 228); seven fix increments 15:51 to 22:38 (1c, 1d, 1e, 1f, 2b, 3c, 3d); re-checks 22:53 and 22:58 (both APPROVE); gate 23:08; sign-off 23:17.
- Ready to sign-off: about 14 h. Full mob-defaults runs: 12 (718 to 1253 tests, 22 to 32 min each), one failed (green 3d, a timing test, isolated in the second run).

## Practice metrics
- D157: held across 3 criteria and 9 increments, with guards for surviving mutations as their own commits.
- D136: 23 of 23 mob-defaults commits within limits (largest 399 lines).
- Push on every commit (first card under the ruling): unpushed detector ok on both repositories at the gate; push lag median about 1 min; four push races between seats and the facilitator in one checkout, each retried.
- Command scans: 34 entries over 8 contexts; 1 real finding (red 1c), 7 false positives (Python string literals and heredoc test data, mostly after green 1d).
- Safety: one real recursive delete on a variable path (red 1c seat, scratch, most likely nothing deleted) and two refused 'rm -f /dev/null'; trust-ladder line added. The facilitator recorded the red 1c finding unread for about 2 h 40 min.
- Probes: on time; all four non-Claude drivers without quota at every probe; every seat Claude.

## Adjustments to review first (D155)
1. Stand-down 1 (d823a7e): no item closed and no name cited as n/a or later without a command and count; plan tables in bodies.
2. Stand-down 2 (d78bd4e): built means called.
3. D167 hostile-input list (first card under it), D166 assumption line, D165 intent line.
4. The product owner's push-on-every-commit ruling (first card under it).

## Observations for this retro
- Dispatch observation (thin slicing): the slice was 55 KB, too big for a prompt; hypothesis: smaller slices cut vote windows and time to first red; counter-metric: follow-up cards (this card spawned one, 228, with 11 owned lines).
- The reviews found 27 findings after three clean greens; the triage added seven increments (about 7 h).
- The scan's own new rules produced most of its false positives on real seat transcripts; each false positive costs a manual read, and an unread real finding sat for 2 h 40 min.
- Facilitator slips: a helper commit swept a seat's staged file; a seat launched 6 minutes after its in-flight line; the red 1c finding unread.
- The watchdog and the product owner asked for bounded turns; turns now end at a capped wait, not a launch.

## Standing section: the feedback-loop lens (D165)
For each of the seven loops (pairing, unit TDD, acceptance TDD, slice, feature, epic, lifecycle): who or what gives the feedback, how fast it arrives, what the team does with it, and MISSING where there is none; then where drift came from.
