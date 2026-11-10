# Card 379 facilitator baseline gate on the final tree (isolated)

Tested commit 577461aad01b2dff49589c0f78e92848a5a234cf (mob-defaults main after cycle 3). Merge verification (D243): HEAD of the clone is 577461aad01b2dff49589c0f78e92848a5a234cf and the ancestry check exited 0. Full suite alone, started 15:28:12Z, finished 16:08:33Z.

```text
FAIL: test_no_tracked_file_but_the_hook_its_tests_and_markdown_names_the_hook (test_delete_guard_hook_3b.OffByDefault.test_no_tracked_file_but_the_hook_its_tests_and_markdown_names_the_hook)
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=False)
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=True)
FAIL: test_muse_argv_unchanged (test_launch_long_prompt.Simple.test_muse_argv_unchanged)
FAIL: test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config (test_muse_cycle3_scope.MuseCycle3ScopeRed.test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config)
FAIL: test_m_live_install_reads_real_machine_facts_read_only (test_muse_install.MuseInstallRed1.test_m_live_install_reads_real_machine_facts_read_only)
Ran 2012 tests in 2418.834s
FAILED (failures=6, skipped=2)
```

Result: 2012 tests, 6 failures, 2 skipped: the five pre-existing ones (bug 406) and ONE NEW: test_delete_guard_hook_3b.OffByDefault.test_no_tracked_file_but_the_hook_its_tests_and_markdown_names_the_hook: the new tracked file core/claude-seat-settings.json names the hook and was not in the card 228 test's list of card 379 files (my local run missed it because the file was still untracked in the worktree where the module ran). Fixed by adding the file to the list (mob-defaults commit after 577461a); the module and the other card 379 modules pass on the fixed tree; the sign-off seat's full-suite run is on the fixed tree and is the full check. Lesson for the retro: a test that reads git ls-files cannot see an untracked new file, so run it after git add.
