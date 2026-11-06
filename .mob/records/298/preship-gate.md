# Pre-ship gate: card 298 (Copilot ctx-5, read-only; report committed unchanged below the line)

Facilitator's notes (not the reviewer's text): the reviewer reported no LiveVote or writer-isolation counts; the facilitator's own run of LiveVoteOverlap plus LiveVote gave 'Ran 15 tests ... OK' (39.5 s) and the Codex green 3 seat reported writer-isolation 31 OK (record-only). The reviewer's race review (f) is labelled record-only. Both Codex event streams were also read by the facilitator: green3 has 18 commands, none a delete, one explicit-path git add, no push.

---
Done check 1: PASS [rerun] `python -m unittest tests.test_decider_runner.LiveVoteOverlap -v` ran 7 tests, OK. The proving test is `test_a_commit_and_push_after_the_first_run_ends_are_refused_while_the_second_votes`; it asserts both records have full equal `head_start` and `head_end` in `tests/test_decider_runner.py:515-593` [rerun].
Done check 2: PASS [rerun] cycle-2 tests `test_the_checkout_stays_refused_until_the_last_run_ends_a_first` and `_b_first` pass, and the mutation failures in `.mob/records/298/cycle2-mutation-evidence.md` are adequate evidence of the failure mode [record-only].
Done check 3: PASS [rerun] `test_a_dead_only_lock_is_replaced_and_the_new_run_refuses_writes`, `test_one_dead_and_one_live_entry_refuse_writes_until_the_live_run_ends`, `test_a_legacy_lock_file_with_a_dead_pid_is_replaced_and_removed`, and `test_a_live_legacy_lock_file_keeps_the_checkout_refused` all pass in `tests/test_decider_runner.py:634-724` [rerun].
Extra check a: PASS [rerun] `git diff e40962c HEAD -- decider.py facilitator.py` shows only the lock code (`hold_vote_lock`, `decider_holders_dir`, `decider_holders`, `decider_lock_live`) changed; no weakened existing assertion in `tests` beyond added coverage [rerun].
Extra check b: PASS [rerun] `git diff e40962c HEAD | grep -c $'\r'` returned `0` [rerun].
Extra check c: PASS [rerun] both event streams list `command_execution` commands; no delete command (`rm`, `Remove-Item`, `del`, `rmdir`, `git clean`) appears in either stream. `git add` is explicit path-only in green3: `git add decider.py`; green1 had no `git add` because it stopped before commit. No push command appears in either stream [rerun].
Extra check d: PASS [rerun] `git log --format='%B' e40962c..HEAD` shows no `Co-Authored-By` trailer on the card commits [rerun].
Extra check e: PASS [rerun] commit order is red before green for cycles 1 and 3, and cycle 2 has no red; the mutation evidence is adequate for cycle 2 [record-only].
Extra check f: PASS [record-only] race review: `.tmp` is not in `*.json`, so another run’s sweep cannot read or delete it; sweeps only delete entries whose pid is not alive, and `decider_lock_live` remains read-only (it only inspects, never mutates) [record-only].
Blocking finding: none [rerun].
Verdict: SHIP; the card’s three Done checks are satisfied, the mutation evidence is adequate, and the lock implementation stays read-only with no weakening of the existing assertions.



exit=0
