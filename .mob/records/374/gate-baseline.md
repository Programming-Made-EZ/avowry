# Card 374 facilitator baseline gate (isolated)

Tested commit 396e6434b345c256903f1e260611be9b21bc9cc5 (mob-defaults main, the green rebased onto the Grok launcher commit). A first run on 1b8e694 was stopped after 3 minutes and discarded: the green had not been merged yet (the fast-forward failed unnoticed because main had moved); it was rebased and the gate restarted. Copy: a single-branch --no-hardlinks clone of the mob-defaults checkout, full suite alone: `python -m unittest discover -s tests`, started 09:32:02Z, finished 10:12:29Z.

```text
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=False)
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple.test_claude_agy_and_codex_argv_unchanged) (backend='agy', readonly=True)
FAIL: test_muse_argv_unchanged (test_launch_long_prompt.Simple.test_muse_argv_unchanged)
FAIL: test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config (test_muse_cycle3_scope.MuseCycle3ScopeRed.test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config)
FAIL: test_m_live_install_reads_real_machine_facts_read_only (test_muse_install.MuseInstallRed1.test_m_live_install_reads_real_machine_facts_read_only)
Ran 1976 tests in 2425.292s
FAILED (failures=5, skipped=1)
```

Result: 1976 tests (1966 at card 365 plus the 10 of this card: 3 in test_decider_prompt_files, 3 added in test_decider_runner, the rest the Grok launcher tests), 5 failures, 1 skipped. The five failures are exactly the pre-existing ones (bug 406: test_launch_long_prompt.Simple agy x2 and muse, test_muse_cycle3_scope, test_muse_install). No failure belongs to card 374.

Live check the slice asked for (recorded here): the real start probe of every backend was run from a scratch directory that is not a git work tree (probe_all.py with the scratch directory as the repository argument, the same call open_run now makes): agy, claude, cursor, grok and qwen answered OK; codex, copilot and muse reported their quota stops (not working-directory problems); no prompt file was left in that directory.
