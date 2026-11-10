# Card 374 sign-off (fresh Claude context, not the gate reviewer, green or facilitator)

Tested commit (full): 396e6434b345c256903f1e260611be9b21bc9cc5 (mob-defaults main; main had not moved past it).
Clean clone: `git clone --no-hardlinks --single-branch -b main C:/Users/<user>/mob-crosscli C:/Users/<user>/signoff374-clone`; `git rev-parse HEAD` in the clone printed the hash above. Clone left in place (no recursive delete). No tracked file in either checkout was edited.

## 1. Full suite, run alone

`python -m unittest discover -s tests` (output to a file outside the clone), started about 10:19Z, nothing else launched meanwhile.

```text
Ran 1976 tests in 2475.049s
FAILED (failures=5, skipped=1)
```
Failures, all five pre-existing (bug 406 family), none new:
- FAIL test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple) (backend='agy', readonly=False)
- FAIL test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple) (backend='agy', readonly=True)
- FAIL test_muse_argv_unchanged (test_launch_long_prompt.Simple)
- FAIL test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config (test_muse_cycle3_scope.MuseCycle3ScopeRed)
- FAIL test_m_live_install_reads_real_machine_facts_read_only (test_muse_install.MuseInstallRed1)
Same list and totals as the facilitator baseline (gate-baseline.md: 1976, failures=5, skipped 1). No timing-sensitive failure, so no module re-run was needed. No errors.

## 2. The card's own tests

- `grep -c expectedFailure tests/test_decider_prompt_files.py tests/test_decider_runner.py` -> 0 and 0.
- `python -m unittest tests.test_decider_prompt_files tests.test_decider_runner` -> `Ran 51 tests in 116.126s` / `OK`.
- `python -m unittest tests.test_decider_runner.ProbePromptFileOverlap` five more times -> Ran 1 test OK each time (2.451 s, 2.479 s, 2.436 s, 3.363 s, 2.300 s). No flakiness.

## 3. The three scenarios with my own inputs (not the repo tests)

Method: a throwaway git repo per scenario under C:/Users/<user>/signoff374-work (outside both checkouts); real `decider.py` processes from the clone; a backend whose invocation uses `{PROMPT_FILE}` and delegates to `tests/fakes/fake_reply.py` through my own wrapper (wrap.py, drive.py in that directory). The wrapper holds run A's vote open, run B's probe uses fake_reply `--hold`.

Scenario A, two real runs at once, B's probe held (its prompt file alive) while A takes its after-snapshot:
```text
A voting: True
B probe held with prompt file alive: True
root prompt-*.txt while B held: []
untracked while B held: ?? .mob/records/seat-use.jsonl   (the decider's own history file, which the snapshot ignores)
A exit 0 integrity {'ok': True, 'changes': []}
B exit 0 integrity {'ok': True, 'changes': []}
root prompt-*.txt after both: []
status after: ?? .mob/records/seat-use.jsonl
```
Scenario B1, a tracked file (README.md) changed by the voter during the run:
```text
tracked exit 5
integrity {'ok': False, 'changes': ['+  M README.md']}
named: True
```
Scenario B2, a voter plants prompt-planted.txt in the repository root:
```text
planted exit 5
integrity {'ok': False, 'changes': ['+ ?? prompt-planted.txt']}
named: True
root prompt-*.txt after: ['prompt-planted.txt']   (the voter's own planted file, as expected; the run was refused)
```
Scenario C: after the clean overlap runs the repository root held no `prompt-*.txt` (see the "after both" line above).

## 4. Hygiene

- `python scripts/home_paths.py --root C:/Users/<user>/projects/phrase-lang` -> `0 files` (run again after writing this file: see the last line below).
- Files under .mob/records/374 containing the account name (case-insensitive, count of files): 0.

## Verdict: SIGN-OFF

Reasons: the three scenarios in the card text hold with my own inputs on real processes (both concurrent runs exit 0 with integrity ok; a changed tracked file and a planted root prompt file are still refused with exit 5 naming the file; the root holds no prompt file after a run); the card's tests are green and stable; the full suite equals the baseline (1976 tests, 5 pre-existing failures, 1 skipped, none new); no account name or home path in the records.
Carried notes, not blockers (as in the gate review): probe_all and card-mode probes still use the real repository and remain a filed follow-up (card 374 does not close that class); the decision probe no longer sees the per-repository gate lock (write it in the close note); the first real parallel window should be watched.
