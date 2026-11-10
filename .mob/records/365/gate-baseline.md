# Card 365 facilitator baseline gate (isolated)

Tested commit 5d5da21e484fe0bb08b4d1878b649f62fa58b373 (mob-defaults main). Copy: a single-branch --no-hardlinks clone of the mob-defaults checkout (not an archive), full suite alone: `python -m unittest discover -s tests`, started 06:03:43Z, finished 06:42:54Z.

```text
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=False)
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=True)
FAIL: test_muse_argv_unchanged (test_launch_long_prompt.Simple.test_muse_argv_unchanged)
FAIL: test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config (test_muse_cycle3_scope.MuseCycle3ScopeRed.test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config)
FAIL: test_m_live_install_reads_real_machine_facts_read_only (test_muse_install.MuseInstallRed1.test_m_live_install_reads_real_machine_facts_read_only)
Ran 1966 tests in 2349.472s
FAILED (failures=5, skipped=1)
```

Result: 1966 tests (1946 at the base plus the 20 of this card), 5 failures, 1 skipped. The five failures are exactly the pre-existing ones named in slice-architect.md section 4 (they fail on the base 77cebb7 too): test_launch_long_prompt.Simple (agy twice, muse), test_muse_cycle3_scope, test_muse_install. No failure belongs to card 365; the intermittent test_loop_state row passed. The causes of the five are filed as a bug.
