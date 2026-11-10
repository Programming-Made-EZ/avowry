# Card 379 facilitator baseline gate on the hardened tree (isolated)

Tested commit ead9a4e52b9dd35f3cdc795aa1d065c3a4c07e11 (mob-defaults main after cycle 2). Merge verification (D243): the clone's `git rev-parse HEAD` is ead9a4e52b9dd35f3cdc795aa1d065c3a4c07e11 and `git merge-base --is-ancestor <green> HEAD` exited 0. Copy: a single-branch --no-hardlinks clone of the mob-defaults checkout, full suite alone, started 14:23:51Z, finished 15:03:58Z. (The first gate of this card, on be8fff6 after cycle 1, 1994 tests, is in gate-baseline.md.)

```text
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=False)
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=True)
FAIL: test_muse_argv_unchanged (test_launch_long_prompt.Simple.test_muse_argv_unchanged)
FAIL: test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config (test_muse_cycle3_scope.MuseCycle3ScopeRed.test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config)
FAIL: test_m_live_install_reads_real_machine_facts_read_only (test_muse_install.MuseInstallRed1.test_m_live_install_reads_real_machine_facts_read_only)
Ran 2006 tests in 2405.075s
FAILED (failures=5, skipped=2)
```

Result: 2006 tests (1976 at card 374 plus the 30 of this card across both cycles), 5 failures, 2 skipped. The five failures are exactly the pre-existing ones (bug 406). No failure belongs to card 379.
