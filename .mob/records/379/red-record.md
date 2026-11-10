# Card 379 red record (Grok writing seat, its first real writing seat; mob-defaults commit 63fb99f)

The Grok writing seat (grok_write.py, brief red379-brief) wrote the tests tests/test_safety_policy.py, tests/test_launcher_deny.py, tests/test_remove_throwaway.py and the two stubs safety_policy.py and remove_throwaway.py; it ran its own commands and copied real output (no invented record, unlike the Qwen seat on card 374). Facilitator reproduction (2026-11-10): the four modules run 22 tests OK (skipped=1, expected failures=17); with the @unittest.expectedFailure lines stripped in a copy outside the repository the three new modules run 18 tests with failures=17, skipped=1, errors=0, every failure an AssertionError. The seat's own record follows verbatim.

```text
Card 379 RED record (QA writing seat). Uncommitted. No trailer. Paths redacted: local account -> <user>.

## Files created (bytes, LF)

safety_policy.py	534
remove_throwaway.py	411
tests/test_safety_policy.py	6538
tests/test_launcher_deny.py	7189
tests/test_remove_throwaway.py	7903

git status --short (worktree root, after this record existed):
?? RED-RECORD-379.txt
?? remove_throwaway.py
?? safety_policy.py
?? tests/test_launcher_deny.py
?? tests/test_remove_throwaway.py
?? tests/test_safety_policy.py

## Suite (decorators on; characterisation included)

command:
python -m unittest tests.test_safety_policy tests.test_launcher_deny tests.test_remove_throwaway tests.test_grok_write tests.test_delete_guard_hook_3a tests.test_delete_guard_hook_3b

(tests.test_delete_guard_hook does not exist; 3a and 3b do.)

real stdout/stderr:

xxxxxxxx
--- worktree after the seat ---


xsxxxxxxxx.setup error: the worktree is not clean at the start
.setup error: this is a main checkout (its .git is a directory); a writing seat works in a linked worktree
.setup error: the prompt does not state the standing writer rules (missing: ['recursive', 'trailer', 'personal name'])
..........................................................
----------------------------------------------------------------------
Ran 79 tests in 26.963s

OK (skipped=1, expected failures=17)

## Prove: copies outside the repo, @unittest.expectedFailure lines stripped

Three test files copied to C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8
COPIED test_safety_policy.py bytes 6388
COPIED test_launcher_deny.py bytes 7069
COPIED test_remove_throwaway.py bytes 7663
Run from the repository root. Every red failure is AssertionError (errors=0).

test_S1_refuses_the_recursive_delete_table (test_safety_policy.SafetyPolicy.test_S1_refuses_the_recursive_delete_table) ... FAIL
test_S2_renderers_contain_every_pattern_in_driver_syntax (test_safety_policy.SafetyPolicy.test_S2_renderers_contain_every_pattern_in_driver_syntax) ... FAIL
test_S3_hook_decision_exit_codes_for_each_payload_shape (test_safety_policy.SafetyPolicy.test_S3_hook_decision_exit_codes_for_each_payload_shape) ... FAIL
test_S4_log_refusal_appends_exactly_one_json_line (test_safety_policy.SafetyPolicy.test_S4_log_refusal_appends_exactly_one_json_line) ... FAIL
test_S5_committed_deny_files_equal_the_rendered_text (test_safety_policy.SafetyPolicy.test_S5_committed_deny_files_equal_the_rendered_text) ... FAIL
test_L1_launcher_problems_on_committed_backends_is_empty (test_launcher_deny.LauncherDeny.test_L1_launcher_problems_on_committed_backends_is_empty) ... FAIL
test_L2_planted_backends_missing_deny_flags_are_named (test_launcher_deny.LauncherDeny.test_L2_planted_backends_missing_deny_flags_are_named) ... FAIL
test_L3_writer_commands_carry_the_deny_wiring (test_launcher_deny.LauncherDeny.test_L3_writer_commands_carry_the_deny_wiring) ... FAIL
test_L4_writers_refuse_when_the_deny_file_is_missing (test_launcher_deny.LauncherDeny.test_L4_writers_refuse_when_the_deny_file_is_missing) ...
--- worktree after the seat ---


FAIL
test_L5_live_claude_denies_recursive_delete_on_nonexistent_targets (test_launcher_deny.LauncherDeny.test_L5_live_claude_denies_recursive_delete_on_nonexistent_targets) ... skipped 'opt-in live Claude deny check; set MOB_LIVE_DENY=1'
test_R1_a_main_checkout_is_refused (test_remove_throwaway.RemoveThrowaway.test_R1_a_main_checkout_is_refused) ... FAIL
test_R2_a_dirty_clone_is_refused_and_removed_with_force_clean (test_remove_throwaway.RemoveThrowaway.test_R2_a_dirty_clone_is_refused_and_removed_with_force_clean) ... FAIL
test_R3_a_symlink_or_junction_is_refused (test_remove_throwaway.RemoveThrowaway.test_R3_a_symlink_or_junction_is_refused) ... FAIL
test_R4_the_current_directory_and_its_parents_are_refused (test_remove_throwaway.RemoveThrowaway.test_R4_the_current_directory_and_its_parents_are_refused) ... FAIL
test_R5_a_path_outside_the_allow_list_or_with_a_non_matching_leaf_is_refused (test_remove_throwaway.RemoveThrowaway.test_R5_a_path_outside_the_allow_list_or_with_a_non_matching_leaf_is_refused) ... FAIL
test_R6_a_linked_worktree_goes_through_git_worktree_remove (test_remove_throwaway.RemoveThrowaway.test_R6_a_linked_worktree_goes_through_git_worktree_remove) ... FAIL
test_R7_a_clean_allow_listed_clone_is_removed (test_remove_throwaway.RemoveThrowaway.test_R7_a_clean_allow_listed_clone_is_removed) ... FAIL
test_R8_every_attempt_appends_one_json_line (test_remove_throwaway.RemoveThrowaway.test_R8_every_attempt_appends_one_json_line) ... FAIL

======================================================================
FAIL: test_S1_refuses_the_recursive_delete_table (test_safety_policy.SafetyPolicy.test_S1_refuses_the_recursive_delete_table)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_safety_policy.py", line 77, in test_S1_refuses_the_recursive_delete_table
    self.assertGreaterEqual(len(names), 12)
    ~~~~~~~~~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^
AssertionError: 0 not greater than or equal to 12

======================================================================
FAIL: test_S2_renderers_contain_every_pattern_in_driver_syntax (test_safety_policy.SafetyPolicy.test_S2_renderers_contain_every_pattern_in_driver_syntax)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_safety_policy.py", line 100, in test_S2_renderers_contain_every_pattern_in_driver_syntax
    self.assertIn("deny", claude.lower())
    ~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^^^^^^^
AssertionError: 'deny' not found in '{}'

======================================================================
FAIL: test_S3_hook_decision_exit_codes_for_each_payload_shape (test_safety_policy.SafetyPolicy.test_S3_hook_decision_exit_codes_for_each_payload_shape)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_safety_policy.py", line 151, in test_S3_hook_decision_exit_codes_for_each_payload_shape
    self.assertEqual(code, 2, payload)
    ~~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^
AssertionError: 0 != 2 : {'tool_name': 'Bash', 'tool_input': {'command': 'rm -rf x'}}

======================================================================
FAIL: test_S4_log_refusal_appends_exactly_one_json_line (test_safety_policy.SafetyPolicy.test_S4_log_refusal_appends_exactly_one_json_line)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_safety_policy.py", line 167, in test_S4_log_refusal_appends_exactly_one_json_line
    self.assertTrue(path.is_file())
    ~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^
AssertionError: False is not true

======================================================================
FAIL: test_S5_committed_deny_files_equal_the_rendered_text (test_safety_policy.SafetyPolicy.test_S5_committed_deny_files_equal_the_rendered_text)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_safety_policy.py", line 178, in test_S5_committed_deny_files_equal_the_rendered_text
    self.assertTrue(claude_path.is_file(), "committed .claude/settings.json is missing")
    ~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
AssertionError: False is not true : committed .claude/settings.json is missing

======================================================================
FAIL: test_L1_launcher_problems_on_committed_backends_is_empty (test_launcher_deny.LauncherDeny.test_L1_launcher_problems_on_committed_backends_is_empty)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_launcher_deny.py", line 84, in test_L1_launcher_problems_on_committed_backends_is_empty
    self.assertEqual(safety_policy.launcher_problems(load_backends()), [])
    ~~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
AssertionError: Lists differ: ['stub'] != []

First list contains 1 additional elements.
First extra element 0:
'stub'

- ['stub']
+ []

======================================================================
FAIL: test_L2_planted_backends_missing_deny_flags_are_named (test_launcher_deny.LauncherDeny.test_L2_planted_backends_missing_deny_flags_are_named)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_launcher_deny.py", line 91, in test_L2_planted_backends_missing_deny_flags_are_named
    self.assertTrue(any("claude" in p.lower() and "settings" in p.lower() for p in problems), problems)
    ~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
AssertionError: False is not true : ['stub']

======================================================================
FAIL: test_L3_writer_commands_carry_the_deny_wiring (test_launcher_deny.LauncherDeny.test_L3_writer_commands_carry_the_deny_wiring)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_launcher_deny.py", line 108, in test_L3_writer_commands_carry_the_deny_wiring
    self.assertIn("--deny", grok_cmd)
    ~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^^^
AssertionError: '--deny' not found in ['grok', '--prompt-file', 'p.txt', '-m', 'grok-4.6', '--always-approve', '--cwd', 'wt']

======================================================================
FAIL: test_L4_writers_refuse_when_the_deny_file_is_missing (test_launcher_deny.LauncherDeny.test_L4_writers_refuse_when_the_deny_file_is_missing)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_launcher_deny.py", line 147, in test_L4_writers_refuse_when_the_deny_file_is_missing
    self.assertEqual(code, 2, f"{name}: {msg}")
    ~~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^^^^^^^^^^^^
AssertionError: 0 != 2 : cursor_write:

======================================================================
FAIL: test_R1_a_main_checkout_is_refused (test_remove_throwaway.RemoveThrowaway.test_R1_a_main_checkout_is_refused)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_remove_throwaway.py", line 100, in test_R1_a_main_checkout_is_refused
    self.assertNotEqual(code, 0)
    ~~~~~~~~~~~~~~~~~~~^^^^^^^^^
AssertionError: 0 == 0

======================================================================
FAIL: test_R2_a_dirty_clone_is_refused_and_removed_with_force_clean (test_remove_throwaway.RemoveThrowaway.test_R2_a_dirty_clone_is_refused_and_removed_with_force_clean)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_remove_throwaway.py", line 112, in test_R2_a_dirty_clone_is_refused_and_removed_with_force_clean
    self.assertNotEqual(code, 0)
    ~~~~~~~~~~~~~~~~~~~^^^^^^^^^
AssertionError: 0 == 0

======================================================================
FAIL: test_R3_a_symlink_or_junction_is_refused (test_remove_throwaway.RemoveThrowaway.test_R3_a_symlink_or_junction_is_refused)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_remove_throwaway.py", line 142, in test_R3_a_symlink_or_junction_is_refused
    self.assertNotEqual(code, 0)
    ~~~~~~~~~~~~~~~~~~~^^^^^^^^^
AssertionError: 0 == 0

======================================================================
FAIL: test_R4_the_current_directory_and_its_parents_are_refused (test_remove_throwaway.RemoveThrowaway.test_R4_the_current_directory_and_its_parents_are_refused)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_remove_throwaway.py", line 157, in test_R4_the_current_directory_and_its_parents_are_refused
    self.assertNotEqual(code, 0)
    ~~~~~~~~~~~~~~~~~~~^^^^^^^^^
AssertionError: 0 == 0

======================================================================
FAIL: test_R5_a_path_outside_the_allow_list_or_with_a_non_matching_leaf_is_refused (test_remove_throwaway.RemoveThrowaway.test_R5_a_path_outside_the_allow_list_or_with_a_non_matching_leaf_is_refused)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_remove_throwaway.py", line 175, in test_R5_a_path_outside_the_allow_list_or_with_a_non_matching_leaf_is_refused
    self.assertNotEqual(code, 0)
    ~~~~~~~~~~~~~~~~~~~^^^^^^^^^
AssertionError: 0 == 0

======================================================================
FAIL: test_R6_a_linked_worktree_goes_through_git_worktree_remove (test_remove_throwaway.RemoveThrowaway.test_R6_a_linked_worktree_goes_through_git_worktree_remove)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_remove_throwaway.py", line 190, in test_R6_a_linked_worktree_goes_through_git_worktree_remove
    self.assertFalse(wt.exists())
    ~~~~~~~~~~~~~~~~^^^^^^^^^^^^^
AssertionError: True is not false

======================================================================
FAIL: test_R7_a_clean_allow_listed_clone_is_removed (test_remove_throwaway.RemoveThrowaway.test_R7_a_clean_allow_listed_clone_is_removed)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_remove_throwaway.py", line 201, in test_R7_a_clean_allow_listed_clone_is_removed
    self.assertFalse(clone.exists())
    ~~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^^
AssertionError: True is not false

======================================================================
FAIL: test_R8_every_attempt_appends_one_json_line (test_remove_throwaway.RemoveThrowaway.test_R8_every_attempt_appends_one_json_line)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\<user>\AppData\Local\Temp\red-379-prove-0fqypba8\test_remove_throwaway.py", line 218, in test_R8_every_attempt_appends_one_json_line
    self.assertTrue(log.is_file())
    ~~~~~~~~~~~~~~~^^^^^^^^^^^^^^^
AssertionError: False is not true

----------------------------------------------------------------------
Ran 18 tests in 7.439s

FAILED (failures=17, skipped=1)

---SUMMARY---
tests 18 failures 17 errors 0 skipped 1
all_assertion True

## AssertionError of each red id

S-1  AssertionError: 0 not greater than or equal to 12
S-2  AssertionError: 'deny' not found in '{}'
S-3  AssertionError: 0 != 2 : {'tool_name': 'Bash', 'tool_input': {'command': 'rm -rf x'}}
S-4  AssertionError: False is not true
S-5  AssertionError: False is not true : committed .claude/settings.json is missing
L-1  AssertionError: Lists differ: ['stub'] != []
L-2  AssertionError: False is not true : ['stub']
L-3  AssertionError: '--deny' not found in ['grok', '--prompt-file', 'p.txt', '-m', 'grok-4.6', '--always-approve', '--cwd', 'wt']
L-4  AssertionError: 0 != 2 : cursor_write:
L-5  skipped 'opt-in live Claude deny check; set MOB_LIVE_DENY=1'
R-1  AssertionError: 0 == 0
R-2  AssertionError: 0 == 0
R-3  AssertionError: 0 == 0
R-4  AssertionError: 0 == 0
R-5  AssertionError: 0 == 0
R-6  AssertionError: True is not false
R-7  AssertionError: True is not false
R-8  AssertionError: False is not true
```
