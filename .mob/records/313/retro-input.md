# Card 313 retro input (facilitator; facts only)

Card: a decision run that starts as another ends is never left without a vote lock (bug; the consumer is the product owner). Signed off with conditions (.mob/records/313/signoff-ctx6.md). Code in mob-crosscli (mob-defaults): hold_vote_lock retries the registration up to 5 times and warns once.

## Flow metrics, opened as the product owner asked (UTC; seat start files in the scratchpad, commit times from git)
- Decision windows against implementation. The task-force decision ran from the first window start (00:58) to the last seat start of Decider D195 (04:12): 3 h 14 min wall clock including about an hour waiting for the product owner's answer after the escalation (02:37 to 03:39). Seat-running time across the rounds: window w313 (10 seat runs, 9 min), re-ask w313r2 (2 runs), Decider rounds d313a 13 min, d313b 10, d313c 3, the independent re-run of round B about 13, d313e 11, d313f 10, d313g 3, d313h 12 (the re-run of round D), then D195: d313i 10, d313j 2, d313k 0.5, d313l 10, d313m 4; about 111 minutes of seat time in 106 seat runs for one decision (the declaration), of which D194 (rounds A to D) failed and D195 passed in the first run with no outlier.
- Red, green and tests: red 1 committed 20:20 local (04:20), green 1 04:22, follow-up 04:23, cycles 2 and 3 tests 04:25: about 5 minutes between the first and last of these commits, and about 15 minutes from the architect's slice to the last test commit; gate and sign-off about 40 minutes (the sign-off alone 18).
- So decision windows cost about 111 minutes of seat time (3 h 14 min wall clock) against about 15 minutes of slice, red and green: seven times as much.
- Re-ask causes (cites D171; 10 re-asks in the runs d313c, d313g, d313j, d313k, d313m): a capital letter, a trailing full stop, Codex echoing the prompt's placeholder cite three or four times, one empty reply. Real objections: the audit lens three times (each after its previous condition was adopted), QA once, the architect once in a re-run.

## Product-owner rulings applied this card
- The lighter path (D153) had expired by count and the declaration ran as a full Decider; D194 failed; the product owner ruled it was not an escalation and that the worktree is a new proposal; D195 passed (first run six Yes or Support-it, no outlier; independent re-run the same). Story 320 (align the Decider with the original Core Protocols) is filed. Findings queued: .mob/records/313/retro-queue.md (the audit-lens seat brief and the outlier's promise; D153's missing verdict; the cite failures).

## D193 trial, first of three cards (the gate and the sign-off)
- G1: the gate ran VoteLockRegistration (9), LiveVoteOverlap (7), LiveVote (8) and the whole file (43) and quoted counts: one LiveVote test failed in the reviewer's shell and could not be reproduced by the facilitator (8 runs) or the sign-off (concurrent and alone). Verdict HOLD.
- G2: the six committed mutation patches: the reviewer's reproductions were blocked (the patch header line stops git apply); the facilitator and the sign-off reproduced four of six.
- G3: 11 of the 20 lines were record-only for the reviewer; the verdict was HOLD until the facilitator ran the probes (g3probe.py); all behaved as the slice predicted.
- Sign-off-only findings on this card: one suspicion (a sweep treating an unreadable live holder as malformed), not demonstrated; no new defect that the gate missed. Tag: G3 (the sweep versus release gap). The trial's first card did not cost more than twice the previous gate in minutes? Gate about 25 min against the 298 gate about 10 min (2.5 times).

## Process notes
- The green seat (Codex, in the worktree) rewrote decider.py's line endings for the whole file (904 changed lines); fixed by a follow-up commit; the brief now needs "keep the file's existing line endings".
- Worktree rule worked: the main checkout's HEAD and status were equal before and after.
- GitHub installation rate limit hit by CI earlier tonight (mob-process timeline reads); the 32 issues closed as not planned labelled not-card; a checker fix landed test-first.
