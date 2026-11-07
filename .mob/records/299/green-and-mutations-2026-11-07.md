# Card 299: red, green and mutation record (facilitator; seats' reports summarised)

Code repository: mob-crosscli. The green seat worked in its own worktree C:/Users/ringe/wt-299-green (branch card-299-green) per Decider D195's rule; the main checkout's HEAD and status before and after the turns are equal (main-checkout-before.txt, main-checkout-after.txt, taken before the fast-forward merge); the merge, the narrow suites and the worktree and branch removal followed. A first launch of the green 1 seat started in the wrong directory (the facilitator's command had no `cd`); it was stopped after seconds, the main checkout and the phrase-lang repository were checked clean, and it was relaunched from inside the worktree.

## Commits (mob-crosscli, in order)
- 3f2b4a6 red 1 (QA, Claude): CrossProcessWriters, eight cycle 1 tests; six failed at the slice's assertions (WriterRefused not raised; no lock file existed during the git write), two guards passed.
- 7afce8e green 1 (Codex, full access, `--json`, 19 commands, none a delete or push, none on the main checkout; stream green1-codex-events.jsonl): `Writers` takes `mob-writer.lock` by an atomic `os.link`, refuses a second process naming the holder; facilitator.py diff 120 insertions, 11 deletions, line endings kept. CrossProcessWriters 8 OK, test_writer_isolation 39 OK, test_writer_staging 7 OK (seat's own result lines).
- f8ee94d red 2 (QA): six cycle 2 tests; the takeover and late-release tests failed (a dead-pid holder was refused), four passed.
- 4b6ab57 green 2 (Codex, 14 commands, none a delete or push; green2-codex-events.jsonl): bounded stale takeover where the single successful unlink of the renamed stale file decides (the architect's probe: on this volume all six racers' renames succeed); 46 insertions, 5 deletions. CrossProcessWriters 14 OK, test_writer_isolation 45 OK, test_writer_staging 7 OK (seat's own result lines).
- 4ae67db cycle 3 tests (QA): eight tests born green after green 2.
Facilitator reruns on main after the fast-forward merge: CrossProcessWriters 22 OK, test_writer_isolation 53 OK in 102 s, test_writer_staging 7 OK, test_decider_runner 43 OK.

## D193 G2: mutation patches for the born-green cycle (QA seat, scratch git copy; patches git-apply clean, in .mob/records/299/mutations/)
- M2a (the live-pid refusal always true): fails test_a_lock_whose_pid_is_gone_is_taken_over and test_a_killed_holder_leaves_a_lock_the_next_writer_takes_over (WriterRefused).
- M2b (the release's token check always true): fails test_a_late_release_removes_only_its_own_lock ("the late release of w1 removed a lock that w2 owns").
- M3a (no release when the git operation raised): fails test_a_failing_git_operation_releases_for_the_next_process (the lock is still there).
- M3b (the moved-token check always false): fails test_a_takeover_that_moved_a_live_lock_puts_it_back_and_refuses (WriterRefused not raised).
The QA seat's first M3a attempt matched nothing (its search text was wrong), so the test ran unmutated and passed; it was redone with the exact one-line pattern. The scratch directory was removed with a single literal-path delete. Cycle 1 and cycle 2 had reds, so only cycle 3 needed the pair.

Record note: in the two Codex event streams the persona name appearing in command output (the test file's text) was replaced by "the product owner" so the repository's personal-name check passes; no command or result was altered.
