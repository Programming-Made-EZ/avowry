# Card 313: red, green and mutation record (facilitator; seats' own reports summarised, commands and results as the seats gave them)

Code repository: mob-crosscli. The green seat worked in its own worktree C:/Users/<user>/wt-313-green (branch card-313-green) per Decider D195; the main checkout's HEAD and status before and after the turns are equal (main-checkout-before.txt, main-checkout-after.txt; after the turns the facilitator fast-forwarded main to the branch).

## Commits (mob-crosscli, in order)
- d34b873 red 1 (QA, Claude): VoteLockRegistration, four cycle 1 tests: [Z] passes; [O], [B], [E] fail at the slice's assertions (holder count 0 != 1; open count 1 != 5).
- 1fb3cb5 green 1 (Codex, full access, `--json`, 11 commands, no delete command, one explicit-path git add, no push; stream green1-codex-events.jsonl): hold_vote_lock retries up to 5 times and warns once. VoteLockRegistration 4 OK, LiveVoteOverlap 7 OK, LiveVote 8 OK (seat's own result lines).
- 3dacde5 green 1 follow-up (facilitator): the seat's commit had normalised the whole of decider.py to LF (904 changed lines against the file's original mixed endings, 414 CR and 473 LF). The follow-up restores the original endings of untouched lines: same content ignoring line endings (checked), a 44-line diff. For the next green briefs: keep the file's existing line endings and rewrite no untouched line.
- 3b3e486 cycles 2 and 3 tests (QA, Claude): five tests (nine with the cycle 1 ones: `python -m unittest tests.test_decider_runner.VoteLockRegistration` Ran 9, OK, facilitator rerun) born green after green 1.

## D193 G2: mutation pairs for the born-green cycles (QA seat, scratch git copy of HEAD 3dacde5 plus the new tests; patches committed in .mob/records/313/mutations/)
- M2b (temporary file outside the holders directory) fails test_the_replace_point_cannot_lose_the_directory ("the holders directory was removed at the replace point").
- M2d (decider_holders deletes a dead holder) fails test_the_readers_see_the_holder_and_delete_nothing (the first read already deletes the file: "Lists differ").
- M2a (mkdir moved above the loop) fails the after-the-mkdir and before-the-open subTests and cycle 1 [O] ("0 != 1 : expected exactly one holder file, found []"); the before-the-replace subTest passes, as the slice said.
- M3a (release skips rmdir) fails the either-order test ("the holders directory was left behind").
- M3b (release deletes every *.json) fails the either-order test at the single-own-holder assertion after the first end.
- M3c (the sweep does not unlink a dead holder) fails the dead-holder subtest ("the dead holder was not swept"); its second subtest also errors with FileExistsError because the first left the directory behind (collateral).
The scratch directory was removed with a single literal-path delete. Cycle 2's first two subTests and cycle 3 are born green because green 1's retry already covers them; cycle 1 was the only cycle with a red.
