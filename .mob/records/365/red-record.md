# Card 365 red record (Cursor writing seat; characterisation and red in mob-defaults commit 224738b9416b57f65bed1476e0056b5717213bbc)

Facilitator reproduction (2026-11-10): group A (PrefixRule 2a-2d, LoneRun 3b: 9 rows) passes on the unchanged tree; with the four test files copied outside the repository and every expectedFailure decorator stripped, the four modules ran 123 tests with 12 failures (11 red ids, the 4b subtests count twice) and 0 errors, every one an AssertionError; with the decorators present the same modules end OK (expected failures=11). Two facilitator edits to the seat's output: the runner test file was restored to its CRLF line endings (the seat had converted it to LF, which made the diff show every line), and the characterisation and the red went into ONE commit (224738b) because PrefixRule and the red rows share test_decider_rotation.py and hunks could not be split without interactive staging. Baseline: five pre-existing failures named in slice-architect.md section 4 and the intermittent test_loop_state row. The seat's own record follows verbatim.

```text
Card 365 red record (RED writing seat, QA). mob-defaults base commit 77cebb7eb4640585151264a88d70117207429ad9, branch
card-365-red, all changes uncommitted. Design: the architect's slice, section 3 (card 365 test list). Section 5 is card 374
and is not part of this seat's list; nothing for card 374 was written.

Files
  tests/test_decider_rotation.py   characterisation PrefixRule (365-2a..2d); red VerifyHistory 1c, StartRace helper,
                                   ConcurrentStarts (1a, 3a), SingleWriterFallback (5a)
  tests/test_decider_record.py     characterisation LoneRun (365-3b)
  tests/test_decider_gate.py       red SeatUseLock (2e, 2f, 2g, 4a, 4b) and the helper seat_use_lock_path
  tests/test_decider_runner.py     red LiveVoteOverlap new method (1d), SixSeatsAtOnce (1b); existing methods untouched
  tests/fakes/fake_barrier_voter.py  new: file barrier in the BARRIER_BACKEND's calls, then fake_voter.main()
  Line endings: tests/test_decider_runner.py was committed with CRLF; it was converted to LF as a whole (the LF rule), so
  `git diff` shows every line; `git diff --ignore-cr-at-eol` shows the 115 added lines only. The other files are LF.

Commit split for the facilitator
  1. characterisation: PrefixRule (test_decider_rotation.py) and LoneRun (test_decider_record.py)
  2. red: everything else above, including tests/fakes/fake_barrier_voter.py

======================================================================================================================
A. CHARACTERISATION (no decorator), on the unchanged tree
======================================================================================================================
Ids and tests:
  365-2a  PrefixRule.test_a_seen_line_removed_is_refused_at_the_start / _at_the_end
  365-2b  PrefixRule.test_a_seen_line_changed_with_a_foreign_line_appended_is_refused_at_the_start / _at_the_end
  365-2c  PrefixRule.test_a_history_emptied_is_refused_at_the_start / _at_the_end
  365-2d  PrefixRule.test_a_seen_line_removed_and_foreign_lines_appended_or_a_reorder_are_refused_at_the_start / _at_the_end
          (subtests: removed_and_appended (equal length), removed_and_two_appended (longer), reordered)
  365-3b  LoneRun.test_a_lone_runs_vote_holds_the_record_keys_and_its_line_follows_the_seeded_lines (subtests codex, claude)

python -m unittest tests.test_decider_rotation.PrefixRule tests.test_decider_record.LoneRun -v
  test_a_history_emptied_is_refused_at_the_end ... ok
  test_a_history_emptied_is_refused_at_the_start ... ok
  test_a_seen_line_changed_with_a_foreign_line_appended_is_refused_at_the_end ... ok
  test_a_seen_line_changed_with_a_foreign_line_appended_is_refused_at_the_start ... ok
  test_a_seen_line_removed_and_foreign_lines_appended_or_a_reorder_are_refused_at_the_end ... ok
  test_a_seen_line_removed_and_foreign_lines_appended_or_a_reorder_are_refused_at_the_start ... ok
  test_a_seen_line_removed_is_refused_at_the_end ... ok
  test_a_seen_line_removed_is_refused_at_the_start ... ok
  test_a_lone_runs_vote_holds_the_record_keys_and_its_line_follows_the_seeded_lines ... ok
  ----------------------------------------------------------------------
  Ran 9 tests in 1.922s

  OK

======================================================================================================================
B. RED (each with @unittest.expectedFailure and a `# 365-xx` comment)
======================================================================================================================
  365-1a  test_decider_rotation.ConcurrentStarts.test_a_run_started_while_another_checks_its_voters_does_not_refuse_either
  365-1b  test_decider_runner.SixSeatsAtOnce.test_six_runs_started_at_once_each_write_one_line_and_pass
  365-1c  test_decider_rotation.VerifyHistory.test_a_line_another_run_appended_while_the_voters_ran_is_accepted
  365-1d  test_decider_runner.LiveVoteOverlap.test_two_overlapping_runs_both_pass_with_a_clean_record_and_one_seat_use_line_each
  365-2e  test_decider_gate.SeatUseLock.test_a_live_holder_refuses_the_run_after_the_wait_and_nothing_is_written
  365-2f  test_decider_gate.SeatUseLock.test_a_dead_holders_lock_is_swept_and_the_run_goes_ahead
  365-2g  test_decider_gate.SeatUseLock.test_the_history_is_not_verified_past_a_live_holder
  365-3a  test_decider_rotation.ConcurrentStarts.test_each_runs_record_points_at_its_own_lines_and_claudes_reason_is_recomputed_from_the_file
  365-4a  test_decider_gate.SeatUseLock.test_the_lock_is_held_for_each_check_and_append_and_never_for_a_probe_or_a_vote
  365-4b  test_decider_gate.SeatUseLock.test_a_refused_run_held_the_lock_for_its_checks_and_released_it
          (subtests: a Claude rotation refusal; an unwritable history)
  365-5a  test_decider_rotation.SingleWriterFallback.test_outside_git_two_runs_in_one_process_are_serialised_with_one_warning

No test names a symbol the green adds: the lock is located with `git rev-parse --git-path mob-seat-use.lock`
(seat_use_lock_path in test_decider_gate.py), and SeatUseLock.setUp patches the wait with
mock.patch.object(fac, "SEAT_USE_LOCK_WAIT", 0.5, create=True). ConcurrentStarts runs `git init -q` after RotationRun.setUp.

----------------------------------------------------------------------------------------------------------------------
Replay with the decorators stripped (a copy outside the repository; the tests in the worktree were not edited)
----------------------------------------------------------------------------------------------------------------------
  copy:  tar --exclude=./.git --exclude=__pycache__ -cf - . | (cd <scratch>/red365-copy && tar -xf -)
  strip: sed -i '/^\s*@unittest.expectedFailure/d' tests/test_decider_rotation.py tests/test_decider_runner.py \
             tests/test_decider_gate.py tests/test_decider_record.py          (grep -c expectedFailure: 0 in each)
  run:   python -m unittest tests.test_decider_rotation tests.test_decider_runner tests.test_decider_gate tests.test_decider_record

Result: Ran 123 tests in 170.715s, FAILED (failures=12), errors 0. Twelve failures = the 11 red ids, with 4b reported once
per subtest. Every one is an AssertionError; no AttributeError, TypeError or other error.

Output (the FAIL blocks; the copy's path is written <scratch>\red365-copy):

FAIL: test_a_run_started_while_another_checks_its_voters_does_not_refuse_either (tests.test_decider_rotation.ConcurrentStarts.test_a_run_started_while_another_checks_its_voters_does_not_refuse_either)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_rotation.py", line 450, in test_a_run_started_while_another_checks_its_voters_does_not_refuse_either
    a, b, _ = self.race(["codex"], ["grok"])
  File "<scratch>\red365-copy\tests\test_decider_rotation.py", line 425, in race
    self.assertIsInstance(outcome.get(name), dict, f"run {name} raised instead of returning its records")
AssertionError: SeatRefused('the decider voter seat in D169 cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is missing or changed). Restore it from git.') is not an instance of <class 'dict'> : run a raised instead of returning its records

FAIL: test_each_runs_record_points_at_its_own_lines_and_claudes_reason_is_recomputed_from_the_file (tests.test_decider_rotation.ConcurrentStarts.test_each_runs_record_points_at_its_own_lines_and_claudes_reason_is_recomputed_from_the_file)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_rotation.py", line 454, in test_each_runs_record_points_at_its_own_lines_and_claudes_reason_is_recomputed_from_the_file
    a, b, run_a = self.race(["codex", "claude"], ["grok"])
  File "<scratch>\red365-copy\tests\test_decider_rotation.py", line 425, in race
    self.assertIsInstance(outcome.get(name), dict, f"run {name} raised instead of returning its records")
AssertionError: SeatRefused('the decider voter seat in D169-claude cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is missing or changed). Restore it from git.') is not an instance of <class 'dict'> : run a raised instead of returning its records

FAIL: test_outside_git_two_runs_in_one_process_are_serialised_with_one_warning (tests.test_decider_rotation.SingleWriterFallback.test_outside_git_two_runs_in_one_process_are_serialised_with_one_warning)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_rotation.py", line 472, in test_outside_git_two_runs_in_one_process_are_serialised_with_one_warning
    a, b, _ = self.race(["codex"], ["grok"])
  File "<scratch>\red365-copy\tests\test_decider_rotation.py", line 425, in race
    self.assertIsInstance(outcome.get(name), dict, f"run {name} raised instead of returning its records")
AssertionError: SeatRefused('the decider voter seat in D169 cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is missing or changed). Restore it from git.') is not an instance of <class 'dict'> : run a raised instead of returning its records

FAIL: test_a_line_another_run_appended_while_the_voters_ran_is_accepted (tests.test_decider_rotation.VerifyHistory.test_a_line_another_run_appended_while_the_voters_ran_is_accepted)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_rotation.py", line 266, in test_a_line_another_run_appended_while_the_voters_ran_is_accepted
    self.assertIsNone(self.verify())
AssertionError: 'the decider voter seat in D169 cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is missing or changed). Restore it from git.' is not None

FAIL: test_two_overlapping_runs_both_pass_with_a_clean_record_and_one_seat_use_line_each (tests.test_decider_runner.LiveVoteOverlap.test_two_overlapping_runs_both_pass_with_a_clean_record_and_one_seat_use_line_each)
Card 365: run B starts while run A votes; B's seat-use line lands after A's, and neither run counts it as a
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_runner.py", line 713, in test_two_overlapping_runs_both_pass_with_a_clean_record_and_one_seat_use_line_each
    self.assertEqual(rc, 0, f"run {name}")
AssertionError: 5 != 0 : run a

FAIL: test_six_runs_started_at_once_each_write_one_line_and_pass (tests.test_decider_runner.SixSeatsAtOnce.test_six_runs_started_at_once_each_write_one_line_and_pass)
----------------------------------------------------------------------
(re-run of this test alone after its message was changed to quote each failing run's last stderr line; the full replay
above had the first line, "INTEGRITY: ...", and exit codes {'codex': 5, 'copilot': 5, 'grok': 5, 'agy': 5, 'cursor': 5,
'qwen': 2}; the codes vary from sample to sample, as the slice found)
AssertionError: {'codex': 2, 'copilot': 2, 'grok': 5, 'agy': 2, 'cursor': 5, 'qwen': 2} != {'codex': 0, 'copilot': 0, 'grok': 0, 'agy': 0, 'cursor': 0, 'qwen': 0}
- {'agy': 2, 'codex': 2, 'copilot': 2, 'cursor': 5, 'grok': 5, 'qwen': 2}
?         ^           ^             ^            ^          ^          ^

+ {'agy': 0, 'codex': 0, 'copilot': 0, 'cursor': 0, 'grok': 0, 'qwen': 0}
?         ^           ^             ^            ^          ^          ^
 : codex: error: the decider voter seat in decider cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is miss; copilot: error: the decider voter seat in decider cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is miss; grok: the decider voter seat in decider cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is missing or ; agy: error: the decider voter seat in decider cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is miss; cursor: the decider voter seat in decider cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is missing or ; qwen: error: the decider voter seat in decider cannot start: the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is miss
Ran 1 test in 5.449s

FAIL: test_a_dead_holders_lock_is_swept_and_the_run_goes_ahead (tests.test_decider_gate.SeatUseLock.test_a_dead_holders_lock_is_swept_and_the_run_goes_ahead)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_gate.py", line 391, in test_a_dead_holders_lock_is_swept_and_the_run_goes_ahead
    self.assertFalse(self.lock.exists(), "the dead holder's seat-use lock was not swept")
AssertionError: True is not false : the dead holder's seat-use lock was not swept

FAIL: test_a_live_holder_refuses_the_run_after_the_wait_and_nothing_is_written (tests.test_decider_gate.SeatUseLock.test_a_live_holder_refuses_the_run_after_the_wait_and_nothing_is_written)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_gate.py", line 378, in test_a_live_holder_refuses_the_run_after_the_wait_and_nothing_is_written
    self.assertEqual(code, 2, out + err)
AssertionError: 0 != 2 : Decider: passed
  Codex: Yes

FAIL: test_a_refused_run_held_the_lock_for_its_checks_and_released_it (tests.test_decider_gate.SeatUseLock.test_a_refused_run_held_the_lock_for_its_checks_and_released_it) (refusal='a Claude rotation refusal')
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_gate.py", line 421, in test_a_refused_run_held_the_lock_for_its_checks_and_released_it
    self.assertTrue(held["check"] and all(held["check"]), f"the seat-use lock was not held while a voter was checked: {held}")
AssertionError: False is not true : the seat-use lock was not held while a voter was checked: {'probe': [False, False, False, False, False], 'check': [False], 'append': [], 'vote': []}

FAIL: test_a_refused_run_held_the_lock_for_its_checks_and_released_it (tests.test_decider_gate.SeatUseLock.test_a_refused_run_held_the_lock_for_its_checks_and_released_it) (refusal='an unwritable history')
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_gate.py", line 436, in test_a_refused_run_held_the_lock_for_its_checks_and_released_it
    self.assertTrue(held["check"] and all(held["check"]), f"the seat-use lock was not held while a voter was checked: {held}")
AssertionError: False is not true : the seat-use lock was not held while a voter was checked: {'probe': [False, False, False, False, False], 'check': [False], 'append': [False], 'vote': []}

FAIL: test_the_history_is_not_verified_past_a_live_holder (tests.test_decider_gate.SeatUseLock.test_the_history_is_not_verified_past_a_live_holder)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_gate.py", line 400, in test_the_history_is_not_verified_past_a_live_holder
    with self.assertRaises(fac.SeatRefused) as refused:
AssertionError: SeatRefused not raised

FAIL: test_the_lock_is_held_for_each_check_and_append_and_never_for_a_probe_or_a_vote (tests.test_decider_gate.SeatUseLock.test_the_lock_is_held_for_each_check_and_append_and_never_for_a_probe_or_a_vote)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "<scratch>\red365-copy\tests\test_decider_gate.py", line 409, in test_the_lock_is_held_for_each_check_and_append_and_never_for_a_probe_or_a_vote
    self.assertTrue(held["append"] and all(held["append"]), f"the seat-use lock was not held while the line was appended: {held}")
AssertionError: False is not true : the seat-use lock was not held while the line was appended: {'probe': [False, False, False, False, False], 'check': [False, False], 'append': [False, False], 'vote': [False, False]}

----------------------------------------------------------------------
Ran 123 tests in 170.715s

FAILED (failures=12)

----------------------------------------------------------------------------------------------------------------------
With the decorators present (the worktree, final files)
----------------------------------------------------------------------------------------------------------------------
python -m unittest tests.test_decider_rotation tests.test_decider_runner tests.test_decider_gate tests.test_decider_record
  ..........xx........x......x...........(stderr of the existing LiveVote / LiveVoteOverlap tests)...
  x............x..........................xxxxx..........................
  ----------------------------------------------------------------------
  Ran 123 tests in 144.295s

  OK (expected failures=11)
(103 tests before this seat, plus 9 characterisation and 11 red. The INTEGRITY lines on stderr come from the existing
guard tests that move HEAD on purpose and from the existing overlap tests that trip the seat-use check, as the comment at
test_decider_runner.py:590 says.)

----------------------------------------------------------------------------------------------------------------------
Red message per id (before the change)
----------------------------------------------------------------------------------------------------------------------
  365-1a  AssertionError: SeatRefused('... in D169 cannot start: ... is not append-only ...') is not an instance of <class 'dict'> : run a raised instead of returning its records
  365-1b  AssertionError: {'codex': 2, 'copilot': 2, 'grok': 5, 'agy': 2, 'cursor': 5, 'qwen': 2} != {all 0} : <seat>: ... is not append-only ...
  365-1c  AssertionError: '... in D169 cannot start: ... is not append-only ...' is not None
  365-1d  AssertionError: 5 != 0 : run a
  365-2e  AssertionError: 0 != 2 : Decider: passed
  365-2f  AssertionError: True is not false : the dead holder's seat-use lock was not swept
  365-2g  AssertionError: SeatRefused not raised
  365-3a  AssertionError: SeatRefused('... in D169-claude cannot start: ... is not append-only ...') is not an instance of <class 'dict'> : run a raised instead of returning its records
  365-4a  AssertionError: False is not true : the seat-use lock was not held while the line was appended: {...}
  365-4b  AssertionError: False is not true : the seat-use lock was not held while a voter was checked: {...} (both subtests)
  365-5a  AssertionError: SeatRefused('... in D169 cannot start: ... is not append-only ...') is not an instance of <class 'dict'> : run a raised instead of returning its records

----------------------------------------------------------------------------------------------------------------------
D222: multi-construct ids
----------------------------------------------------------------------------------------------------------------------
  365-2a, 2b, 2c  two constructs each: the start layer and the end layer (two methods per id)
  365-2d          two layers x three tampers (a seen line removed plus one foreign line, equal length; plus two foreign
                  lines, longer; a reordered file), as subtests
  365-3b          two seats (codex without rotation_reason, claude with it), as subtests
  365-4a          one run observed at four call points (probe, check, append, voter launch) plus the release afterwards
  365-4b          two refusal paths (a Claude rotation refusal; an unwritable history), as subtests; the stripped replay
                  reports one failure per subtest
  Single-construct: 1a, 1b, 1c, 1d, 2e, 2f, 2g, 3a, 5a.

----------------------------------------------------------------------------------------------------------------------
Notes for the green
----------------------------------------------------------------------------------------------------------------------
- 365-5a asserts exactly one fallback warning naming this test's temporary directory. In a module run, earlier tests on
  non-git repositories reach the fallback first, so the warning must be printed once per repository (per resolved history
  path, the key of the fallback lock) per process. A single warning per process would make 5a fail once its decorator is
  removed.
- 365-2e and 365-2g need the wait to be read when the lock is taken (SEAT_USE_LOCK_WAIT patched to 0.5 s); 2e asserts at
  least 0.5 s elapsed and the live lock left in place.
- 365-4b (an unwritable history) makes the seeded history read-only with os.chmod and skips that subtest when the file
  stays writable (running as root).
- 365-1a, 3a, 5a wrap check_seat on run A's instance and append_seat_use on run B's instance; check_voters must keep
  calling run.check_seat and commit_seat must keep calling self.append_seat_use.
```
