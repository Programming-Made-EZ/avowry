# Card 374 red record (mob-defaults commits 0e93c52 and a6f4385)

Seats: part 1 by the Qwen WRITING seat (its first writing job, Probation, through local_driver.py with aider: tests/test_decider_prompt_files.py, 374-3a and 374-3b with expectedFailure, 374-3c a control that passes now); part 2 by a Cursor WRITING seat (374-1 two runs at once with expectedFailure, 374-2a and 374-2b still-refused rows that pass now, and tests/fakes/fake_reply.py gains the optional --hold flag). The slice is .mob/records/365/slice-architect.md section 5; the declaration is Decider D241 (cards 365 and 374).

## What the facilitator had to fix on the Qwen seat (Probation: nothing is accepted on the driver's word)

1. The seat wrote a record file with an INVENTED test run (placeholder timings and paths): an aider-driven model cannot run commands. The file was empty in git by the time it was committed and was removed in a follow-up commit; the facilitator ran every command.
2. The two red tests failed for the wrong reason (the probe never ran): the facilitator's own goal text told the seat to wrap the backends mapping in {"backends": ...}, but open_run wraps it itself. The facilitator corrected the helper.
3. The seat's spy recorded the working directory even when no prompt file was written, so the control test 374-3c failed today; the facilitator changed the spy to record the directory of the file actually written.
4. aider added .aider* to .gitignore and left chat history files; the facilitator reverted the .gitignore change and removed the files.

Assessment: the Qwen seat can write a bounded test file from a precise goal, but it needs the facilitator to run and check every claim; widen it only to small, fully specified test additions.

## Facilitator reproduction (2026-11-10)

With the decorators present: `python -m unittest tests.test_decider_prompt_files tests.test_decider_runner` ran 51 tests, OK (expected failures=3). With the @unittest.expectedFailure lines stripped in a copy outside the repository the same two modules ran 51 tests with FAILED (failures=3): 374-3a (a prompt file written to the repository root, expected under the run directory), 374-3b (the left-over probe file is in the repository root), 374-1 (run A exits 5 with a change '+ ?? prompt-....txt'); 374-2a, 374-2b, 374-3c and every existing test pass. The Cursor seat's own record follows verbatim.

```text
Card 374 red, part 2 (second RED writing seat, QA): red record
================================================================

Tests added (no change to decider.py or facilitator.py):
- 374-1 (red, @unittest.expectedFailure): tests/test_decider_runner.py, new class ProbePromptFileOverlap,
  test_374_1_a_run_whose_start_probe_holds_a_prompt_file_does_not_break_a_run_that_is_voting. Two in-process
  decider.run calls on one checkout; both use a {PROMPT_FILE} backend (tests/fakes/fake_reply.py) with probe_model; run B's
  backend adds --hold DIR. Order: A votes (blocked in a mocked vote_seat); B starts and its start probe holds; the test waits
  for DIR/in-*; A is released and takes its end snapshot; A is joined; DIR/go releases B. Asserts both runs exit 0 with
  integrity {"ok": True, "changes": []}.
- 374-2a (characterisation, passes now): tests/test_decider_runner.py, new class IntegrityStillRefuses: a voter that
  writes the tracked README.md in the real tree is refused (exit 5, README.md named in the output and in integrity.changes).
- 374-2b (characterisation, passes now): same class: a voter that writes prompt-planted.txt into the real repository root
  is refused (exit 5, prompt-planted.txt named); guards against a fix that whitelists prompt-*.txt.
- tests/fakes/fake_reply.py: optional --hold DIR (creates DIR/in-<pid>, polls every 0.05 s for DIR/go, at most 60 s,
  then replies). Without --hold the behaviour is unchanged.

Method: both touched modules were copied to a temporary directory outside the repository; in the copy the
@unittest.expectedFailure lines were deleted and HERE was pointed at the repository's tests directory (the copy is run
from the repository root, so decider.py, facilitator.py and the fakes are the repository's own). The copy differs from
the worktree only by these lines. In the output below the local account name in paths is replaced by <user>; nothing
else is edited.

1. Diff of the copy against the worktree (diff --strip-trailing-cr tests/<f> <copy>/<f>)
-------------------------------------------------------------------------------------
== test_decider_prompt_files.py
8c8
< HERE = Path(__file__).resolve().parent
---
> HERE = Path.cwd() / "tests"
33d32
<     @unittest.expectedFailure
78d76
<     @unittest.expectedFailure
== test_decider_runner.py
13c13
< HERE = Path(__file__).resolve().parent
---
> HERE = Path.cwd() / "tests"
1107d1106
<     @unittest.expectedFailure

2. With the decorators (worktree as is): python -m unittest tests.test_decider_prompt_files tests.test_decider_runner
-------------------------------------------------------------------------------------------------------------------
C:\Users\<user>\wt-374-red\facilitator.py:377: ResourceWarning: unclosed file <_io.TextIOWrapper name=5 encoding='utf-8'>
  result = _launch_backend(
ResourceWarning: Enable tracemalloc to get the object allocation traceback
C:\Users\<user>\wt-374-red\facilitator.py:377: ResourceWarning: unclosed file <_io.TextIOWrapper name=4 encoding='utf-8'>
  result = _launch_backend(
ResourceWarning: Enable tracemalloc to get the object allocation traceback
xx............INTEGRITY: the real working tree or HEAD changed while the vote ran (a voter escaped its sandbox, or another process is writing here):
  HEAD moved 9d01e52 -> e75aa77
...INTEGRITY: the real working tree or HEAD changed while the vote ran (a voter escaped its sandbox, or another process is writing here):
  HEAD moved 647fd0c -> db8cd97
...............INTEGRITY: the real working tree or HEAD changed while the vote ran (a voter escaped its sandbox, or another process is writing here):
  + ?? prompt-4w5zm88a.txt
x..................
----------------------------------------------------------------------
Ran 51 tests in 117.393s

OK (expected failures=3)
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
  HEAD moved 9d01e52 -> e75aa77
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
  Copilot: Yes
  Grok: Yes
Decider: passed
  Codex: Yes
  HEAD moved 647fd0c -> db8cd97
Decider: failed
  Codex: Yes
  Copilot: No
  Grok: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
  + ?? prompt-4w5zm88a.txt
Decider: passed
  Codex: Yes
exit 0

3. Without the decorators (the copy, run from the repository root):
   python -m unittest discover -v -s <copy> -t <copy> -p "test_decider_*.py"
-------------------------------------------------------------------------------------------------------------------
test_374_3b_a_leftover_probe_file_from_a_killed_probe_stays_in_the_run_directory (test_decider_prompt_files.TestPromptFileCleanup.test_374_3b_a_leftover_probe_file_from_a_killed_probe_stays_in_the_run_directory) ... C:\Users\<user>\wt-374-red\facilitator.py:377: ResourceWarning: unclosed file <_io.TextIOWrapper name=5 encoding='utf-8'>
  result = _launch_backend(
ResourceWarning: Enable tracemalloc to get the object allocation traceback
C:\Users\<user>\wt-374-red\facilitator.py:377: ResourceWarning: unclosed file <_io.TextIOWrapper name=4 encoding='utf-8'>
  result = _launch_backend(
ResourceWarning: Enable tracemalloc to get the object allocation traceback
FAIL
test_374_3a_a_probe_prompt_file_is_written_under_the_run_directory_never_in_the_repository_root (test_decider_prompt_files.TestPromptFileLocation.test_374_3a_a_probe_prompt_file_is_written_under_the_run_directory_never_in_the_repository_root) ... FAIL
test_374_3c_a_backend_without_a_prompt_file_placeholder_writes_no_file (test_decider_prompt_files.TestPromptFileLocation.test_374_3c_a_backend_without_a_prompt_file_placeholder_writes_no_file) ... ok
test_a_run_probes_every_backend_before_its_vote_and_writes_no_probe_files_into_the_real_tree (test_decider_runner.FreshProbe.test_a_run_probes_every_backend_before_its_vote_and_writes_no_probe_files_into_the_real_tree) ... ok
test_374_2a_a_tracked_file_changed_during_the_vote_is_still_refused (test_decider_runner.IntegrityStillRefuses.test_374_2a_a_tracked_file_changed_during_the_vote_is_still_refused) ... ok
test_374_2b_a_prompt_file_a_voter_plants_in_the_root_is_still_refused (test_decider_runner.IntegrityStillRefuses.test_374_2b_a_prompt_file_a_voter_plants_in_the_root_is_still_refused) ... ok
test_a_meaningless_proposal_is_refused_before_any_seat_is_started (test_decider_runner.Isolation.test_a_meaningless_proposal_is_refused_before_any_seat_is_started) ... ok
test_a_voter_that_escapes_to_the_real_tree_is_detected (test_decider_runner.Isolation.test_a_voter_that_escapes_to_the_real_tree_is_detected) ... ok
test_a_voter_that_writes_files_changes_only_its_clone_and_is_reported (test_decider_runner.Isolation.test_a_voter_that_writes_files_changes_only_its_clone_and_is_reported) ... ok
test_every_seat_gets_the_same_full_prompt (test_decider_runner.Isolation.test_every_seat_gets_the_same_full_prompt) ... ok
test_the_scratch_clones_are_removed_afterwards (test_decider_runner.Isolation.test_the_scratch_clones_are_removed_afterwards) ... ok
test_voters_run_in_separate_scratch_clones_not_in_the_real_tree (test_decider_runner.Isolation.test_voters_run_in_separate_scratch_clones_not_in_the_real_tree) ... ok
test_a_facilitator_commit_during_the_vote_is_refused_before_head_moves (test_decider_runner.LiveVote.test_a_facilitator_commit_during_the_vote_is_refused_before_head_moves) ... ok
test_a_facilitator_push_during_the_vote_is_refused_and_lands_once_after (test_decider_runner.LiveVote.test_a_facilitator_push_during_the_vote_is_refused_and_lands_once_after)
O (red 1d, rewritten from the integration attempt): the push is refused before any git write; origin does not ... ok
test_a_genuine_external_head_move_is_recorded_as_unequal_heads_and_the_lock_is_released (test_decider_runner.LiveVote.test_a_genuine_external_head_move_is_recorded_as_unequal_heads_and_the_lock_is_released) ... INTEGRITY: the real working tree or HEAD changed while the vote ran (a voter escaped its sandbox, or another process is writing here):
  HEAD moved c350b91 -> d1124dd
ok
test_attempts_at_the_start_barrier_and_just_before_the_end_snapshot_cannot_enter (test_decider_runner.LiveVote.test_attempts_at_the_start_barrier_and_just_before_the_end_snapshot_cannot_enter) ... ok
test_concurrent_voters_share_one_protected_interval_until_all_finish (test_decider_runner.LiveVote.test_concurrent_voters_share_one_protected_interval_until_all_finish)
codex returns first; copilot and grok attempt only after codex's voter has finished, so a lock released per ... ok
test_guard_a_genuine_external_head_move_still_exits_5 (test_decider_runner.LiveVote.test_guard_a_genuine_external_head_move_still_exits_5)
Today's behaviour, kept: a process that bypasses Writers really moves HEAD; the runner reports it. ... INTEGRITY: the real working tree or HEAD changed while the vote ran (a voter escaped its sandbox, or another process is writing here):
  HEAD moved 0302f6f -> 29e739f
ok
test_guard_vote_semantics_and_counted_seats_are_unchanged (test_decider_runner.LiveVote.test_guard_vote_semantics_and_counted_seats_are_unchanged) ... ok
test_with_no_integration_attempt_the_record_holds_equal_full_heads (test_decider_runner.LiveVote.test_with_no_integration_attempt_the_record_holds_equal_full_heads) ... ok
test_a_commit_and_push_after_the_first_run_ends_are_refused_while_the_second_votes (test_decider_runner.LiveVoteOverlap.test_a_commit_and_push_after_the_first_run_ends_are_refused_while_the_second_votes) ... ok
test_a_dead_only_lock_is_replaced_and_the_new_run_refuses_writes (test_decider_runner.LiveVoteOverlap.test_a_dead_only_lock_is_replaced_and_the_new_run_refuses_writes) ... ok
test_a_legacy_lock_file_with_a_dead_pid_is_replaced_and_removed (test_decider_runner.LiveVoteOverlap.test_a_legacy_lock_file_with_a_dead_pid_is_replaced_and_removed) ... ok
test_a_live_legacy_lock_file_keeps_the_checkout_refused (test_decider_runner.LiveVoteOverlap.test_a_live_legacy_lock_file_keeps_the_checkout_refused) ... ok
test_one_dead_and_one_live_entry_refuse_writes_until_the_live_run_ends (test_decider_runner.LiveVoteOverlap.test_one_dead_and_one_live_entry_refuse_writes_until_the_live_run_ends) ... ok
test_the_checkout_stays_refused_until_the_last_run_ends_a_first (test_decider_runner.LiveVoteOverlap.test_the_checkout_stays_refused_until_the_last_run_ends_a_first) ... ok
test_the_checkout_stays_refused_until_the_last_run_ends_b_first (test_decider_runner.LiveVoteOverlap.test_the_checkout_stays_refused_until_the_last_run_ends_b_first) ... ok
test_two_overlapping_runs_both_pass_with_a_clean_record_and_one_seat_use_line_each (test_decider_runner.LiveVoteOverlap.test_two_overlapping_runs_both_pass_with_a_clean_record_and_one_seat_use_line_each)
Card 365: run B starts while run A votes; B's seat-use line lands after A's, and neither run counts it as a ... ok
test_the_result_can_be_appended_to_the_decider_log_in_the_standard_form (test_decider_runner.Logging.test_the_result_can_be_appended_to_the_decider_log_in_the_standard_form) ... ok
test_result_rules (test_decider_runner.ParseVote.test_result_rules) ... ok
test_the_outlier_reason_follows_the_vote (test_decider_runner.ParseVote.test_the_outlier_reason_follows_the_vote) ... ok
test_votes_are_found_after_narration_in_conditionals_and_in_echoed_transcripts (test_decider_runner.ParseVote.test_votes_are_found_after_narration_in_conditionals_and_in_echoed_transcripts) ... ok
test_374_1_a_run_whose_start_probe_holds_a_prompt_file_does_not_break_a_run_that_is_voting (test_decider_runner.ProbePromptFileOverlap.test_374_1_a_run_whose_start_probe_holds_a_prompt_file_does_not_break_a_run_that_is_voting) ... INTEGRITY: the real working tree or HEAD changed while the vote ran (a voter escaped its sandbox, or another process is writing here):
  + ?? prompt-ggd270p0.txt
FAIL
test_a_codex_transcript_echo_does_not_confuse_the_vote (test_decider_runner.Running.test_a_codex_transcript_echo_does_not_confuse_the_vote) ... ok
test_a_long_reply_is_kept_up_to_twenty_thousand_characters (test_decider_runner.Running.test_a_long_reply_is_kept_up_to_twenty_thousand_characters) ... ok
test_a_seat_that_never_answers_counts_as_no_response_and_fails (test_decider_runner.Running.test_a_seat_that_never_answers_counts_as_no_response_and_fails) ... ok
test_a_silent_seat_is_asked_once_more (test_decider_runner.Running.test_a_silent_seat_is_asked_once_more) ... ok
test_all_yes_passes_and_every_seat_voted (test_decider_runner.Running.test_all_yes_passes_and_every_seat_voted) ... ok
test_an_absolute_no_fails_even_when_everyone_else_agrees (test_decider_runner.Running.test_an_absolute_no_fails_even_when_everyone_else_agrees) ... ok
test_an_outlier_fails_the_proposal_and_its_reason_is_kept (test_decider_runner.Running.test_an_outlier_fails_the_proposal_and_its_reason_is_kept) ... ok
test_the_full_reply_is_kept_for_feedback_written_before_the_vote (test_decider_runner.Running.test_the_full_reply_is_kept_for_feedback_written_before_the_vote) ... ok
test_six_runs_started_at_once_each_write_one_line_and_pass (test_decider_runner.SixSeatsAtOnce.test_six_runs_started_at_once_each_write_one_line_and_pass) ... ok
test_a_dead_holder_and_an_empty_directory_are_swept_and_the_run_is_registered (test_decider_runner.VoteLockRegistration.test_a_dead_holder_and_an_empty_directory_are_swept_and_the_run_is_registered)
[E] a dead-pid holder file is swept and an empty directory is accepted; either way the run is registered. ... ok
test_a_lone_run_registers_once_and_leaves_nothing (test_decider_runner.VoteLockRegistration.test_a_lone_run_registers_once_and_leaves_nothing)
[Z] no other run: one open, one holder file named for the token, nothing left after release. ... ok
test_a_release_by_the_other_run_at_each_point_leaves_this_run_registered (test_decider_runner.VoteLockRegistration.test_a_release_by_the_other_run_at_each_point_leaves_this_run_registered)
[O][B] run A (real) releases once at each point of run B's registration; B ends with exactly one holder, refused_both holds. ... ok
test_a_removal_before_each_of_the_first_four_attempts_still_registers (test_decider_runner.VoteLockRegistration.test_a_removal_before_each_of_the_first_four_attempts_still_registers)
[B] the removal lands before attempts 1 to 4; the 5th (last allowed) attempt registers. ... ok
test_a_removal_before_every_attempt_stops_after_five_with_one_warning (test_decider_runner.VoteLockRegistration.test_a_removal_before_every_attempt_stops_after_five_with_one_warning)
[E] the removal lands before all 5 attempts: stop after 5, no exception, one stderr warning, a harmless release. ... ok
test_a_removal_before_the_open_still_registers (test_decider_runner.VoteLockRegistration.test_a_removal_before_the_open_still_registers)
[O] the directory is removed once, just before the first open: the run still ends with one holder. ... ok
test_the_first_run_to_end_leaves_the_checkout_refused_in_either_order (test_decider_runner.VoteLockRegistration.test_the_first_run_to_end_leaves_the_checkout_refused_in_either_order)
[B] two runs hold the lock; the first to end leaves the checkout refused, the last to end releases it and removes the directory. ... ok
test_the_readers_see_the_holder_and_delete_nothing (test_decider_runner.VoteLockRegistration.test_the_readers_see_the_holder_and_delete_nothing)
[I] decider_holders and decider_lock_live list and judge a directory with a live and a dead holder, and remove nothing. ... ok
test_the_replace_point_cannot_lose_the_directory (test_decider_runner.VoteLockRegistration.test_the_replace_point_cannot_lose_the_directory)
[B] at the replace the temporary file is in the directory, so the other run's release cannot remove it: no retry. ... ok

======================================================================
FAIL: test_374_3b_a_leftover_probe_file_from_a_killed_probe_stays_in_the_run_directory (test_decider_prompt_files.TestPromptFileCleanup.test_374_3b_a_leftover_probe_file_from_a_killed_probe_stays_in_the_run_directory)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red374b-copy\test_decider_prompt_files.py", line 86, in test_374_3b_a_leftover_probe_file_from_a_killed_probe_stays_in_the_run_directory
    self.assertEqual(list(repo.glob("prompt-*.txt")), [])
    ~~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
AssertionError: Lists differ: [WindowsPath('C:/Users/<user>/AppData/Local[121 chars]xt')] != []

First list contains 2 additional elements.
First extra element 0:
WindowsPath('C:/Users/<user>/AppData/Local/Temp/p374-8p9ai7i3/prompt-2i7hinqs.txt')

+ []
- [WindowsPath('C:/Users/<user>/AppData/Local/Temp/p374-8p9ai7i3/prompt-2i7hinqs.txt'),
-  WindowsPath('C:/Users/<user>/AppData/Local/Temp/p374-8p9ai7i3/prompt-_j5cei_n.txt')]

======================================================================
FAIL: test_374_3a_a_probe_prompt_file_is_written_under_the_run_directory_never_in_the_repository_root (test_decider_prompt_files.TestPromptFileLocation.test_374_3a_a_probe_prompt_file_is_written_under_the_run_directory_never_in_the_repository_root)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red374b-copy\test_decider_prompt_files.py", line 52, in test_374_3a_a_probe_prompt_file_is_written_under_the_run_directory_never_in_the_repository_root
    self.assertTrue(run_dir_resolved in [p, *p.parents], f"prompt file written to {p}, expected under {run_dir_resolved}")
    ~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
AssertionError: False is not true : prompt file written to C:\Users\<user>\AppData\Local\Temp\p374-j6u_pvk0, expected under C:\Users\<user>\AppData\Local\Temp\p374-j6u_pvk0\.scratch-run

======================================================================
FAIL: test_374_1_a_run_whose_start_probe_holds_a_prompt_file_does_not_break_a_run_that_is_voting (test_decider_runner.ProbePromptFileOverlap.test_374_1_a_run_whose_start_probe_holds_a_prompt_file_does_not_break_a_run_that_is_voting)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red374b-copy\test_decider_runner.py", line 1152, in test_374_1_a_run_whose_start_probe_holds_a_prompt_file_does_not_break_a_run_that_is_voting
    self.assertEqual({name: (outcome[name][0], outcome[name][1]["integrity"]) for name in ("a", "b")},
    ~~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                     {"a": clean, "b": clean})
                     ^^^^^^^^^^^^^^^^^^^^^^^^^
AssertionError: {'a': (5, {'ok': False, 'changes': ['+ ?? prompt[52 chars][]})} != {'a': (0, {'ok': True, 'changes': []}), 'b': (0,[25 chars][]})}
+ {'a': (0, {'changes': [], 'ok': True}), 'b': (0, {'changes': [], 'ok': True})}
- {'a': (5, {'changes': ['+ ?? prompt-ggd270p0.txt'], 'ok': False}),
-  'b': (0, {'changes': [], 'ok': True})}

----------------------------------------------------------------------
Ran 51 tests in 104.462s

FAILED (failures=3)
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
  HEAD moved c350b91 -> d1124dd
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
  Copilot: Yes
  Grok: Yes
Decider: passed
  Codex: Yes
  HEAD moved 0302f6f -> 29e739f
Decider: failed
  Codex: Yes
  Copilot: No
  Grok: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
Decider: passed
  Codex: Yes
  + ?? prompt-ggd270p0.txt
Decider: passed
  Codex: Yes
exit 1
```
