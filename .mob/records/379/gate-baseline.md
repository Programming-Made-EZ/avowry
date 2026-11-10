# Card 379 facilitator baseline gate (isolated)

Tested commit be8fff6e4b1434a36efd7de7a3c0a59d580042ae (mob-defaults main). Merge verification (D243, first card under it): the clone's `git rev-parse HEAD` is be8fff6e4b1434a36efd7de7a3c0a59d580042ae and `git merge-base --is-ancestor <green> HEAD` exited 0. Copy: a single-branch --no-hardlinks clone of the mob-defaults checkout, full suite alone: `python -m unittest discover -s tests`, started 13:07:12Z, finished 13:47:00Z.

```text
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=False)
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=True)
FAIL: test_muse_argv_unchanged (test_launch_long_prompt.Simple.test_muse_argv_unchanged)
FAIL: test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config (test_muse_cycle3_scope.MuseCycle3ScopeRed.test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config)
FAIL: test_m_live_install_reads_real_machine_facts_read_only (test_muse_install.MuseInstallRed1.test_m_live_install_reads_real_machine_facts_read_only)
Ran 1994 tests in 2384.982s
FAILED (failures=5, skipped=2)
```

Result: 1994 tests (1976 at card 374 plus the 18 of this card), 5 failures, 2 skipped (the live Claude deny row and one older skip). The five failures are exactly the pre-existing ones (bug 406). No failure belongs to card 379. Before the gate the facilitator ran the live checks: the real probe of every backend from a non-git scratch directory with the deny wiring (agy, claude, cursor, grok, qwen OK; codex, copilot, muse report their quota stops), and the opt-in live Claude deny test (MOB_LIVE_DENY=1, five tests OK).
