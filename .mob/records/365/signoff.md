# Card 365 sign-off (independent sign-off seat, fresh Claude context, 2026-11-10)

Scope: concurrent seat-use appends never fail the append-only check.

## VERDICT: SIGN-OFF

Tested commit (full hash): 5d5da21e484fe0bb08b4d1878b649f62fa58b373 (mob-defaults main; main had no later commit). The records read: declaration-proposal.md, gate-review.md, second-review.md, gate-baseline.md, red-record.md, seat-changes.md.

Reasons: the full suite alone gives 1966 tests, 5 failures, all five pre-existing and identical to the baseline, no new failure, test_loop_state passed; the card's four modules pass (123 tests) with no expectedFailure left; the six-process test passed 5 of 5 more times; my own three scenarios (three real processes at once, a tamper, a dead-pid stale lock) behave as the card says; the home-path scan is 0 files and the card 365 records hold no account name.

## 1. Clean clone and the full suite, alone

Before the start only the Windows desktop helper python processes of this session were running (no test run, no dotnet).

```
$ git clone --no-hardlinks --single-branch -b main <user>\mob-crosscli <user>\signoff365-clone
$ git -C signoff365-clone rev-parse HEAD
5d5da21e484fe0bb08b4d1878b649f62fa58b373
$ git -C signoff365-clone status --short        (empty)
$ python -m unittest discover -s tests > <user>\signoff365-out\full.out 2>&1      (started 07:04:21Z, finished 07:43:39Z, nothing else launched meanwhile)
Ran 1966 tests in 2355.769s
FAILED (failures=5, skipped=1)
```
Every failure, by name:
```
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple...) (backend='agy', readonly=False)
FAIL: test_claude_agy_and_codex_argv_unchanged (test_launch_long_prompt.Simple...) (backend='agy', readonly=True)
FAIL: test_muse_argv_unchanged (test_launch_long_prompt.Simple...)
FAIL: test_I_run_backend_argv_has_no_model_element_for_shipped_muse_config (test_muse_cycle3_scope.MuseCycle3ScopeRed...)
FAIL: test_m_live_install_reads_real_machine_facts_read_only (test_muse_install.MuseInstallRed1...)
```
Errors: 0. New failures: none; the five are exactly the pre-existing set of gate-baseline.md (same totals: 1966, failures=5, skipped=1). test_loop_state and the timing-sensitive concurrency tests did not fail, so no module re-run was needed.

## 2. The card's own tests

```
$ python -m unittest tests.test_decider_rotation tests.test_decider_gate tests.test_decider_record tests.test_decider_runner
Ran 123 tests in 160.906s
OK
$ grep -c expectedFailure tests/test_decider_rotation.py tests/test_decider_gate.py tests/test_decider_record.py tests/test_decider_runner.py
0 0 0 0
$ python -m unittest tests.test_decider_runner.SixSeatsAtOnce      (five more times)
Ran 1 test in 5.943s OK
Ran 1 test in 6.798s OK
Ran 1 test in 6.051s OK
Ran 1 test in 6.204s OK
Ran 1 test in 6.922s OK
```

## 3. Three scenarios with inputs of my own (script outside both checkouts; throwaway git repos under <user>\signoff365-scen; fake backends from tests/fakes of the clone)

A history file of my own (two seeded lines, contexts mine-0 and mine-1) in a throwaway repo; three real `decider.py` processes (seats codex, grok, agy) started together, the last backend holding at a file barrier so all three check and append at once.
```
exit codes: {'codex': 0, 'grok': 0, 'agy': 0}
'not append-only' in any stderr: False
history ends with newline: True  line count: 5 (2 seeded + 3 new)
contexts: ['mine-0', 'mine-1', 'decider-codex', 'decider-grok', 'decider-agy']
seeded lines unchanged as prefix: True
  codex: integrity={'ok': True, 'changes': []} line_index=2 -> context decider-codex
  grok: integrity={'ok': True, 'changes': []} line_index=3 -> context decider-grok
  agy: integrity={'ok': True, 'changes': []} line_index=4 -> context decider-agy
lock file left behind: False
```
Tamper: a run reads three lines, a foreign line is appended (accepted, four lines), then I delete the second line:
```
first read ok, lines seen: 3
foreign append accepted, lines now: 4
start_voters refused: ... the seat-use history .mob/records/seat-use.jsonl is not append-only (a line this run saw or wrote is missing or changed). Restore it from git.
file unchanged by the refused start (no voter line written): True
verify_history refused: ... is not append-only (a line this run saw or wrote is missing or changed). Restore it from git.
```
Stale lock: a lock file in the git dir naming a pid that had exited (alive: False), token planted0:
```
exit code: 0  elapsed 3.4s (the wait for a live holder is about 30 s)
lines: 3 (2 seeded + 1) last context: decider-codex
planted lock still there: False
lock file state after: absent
```
The dead holder's lock was swept at once, the run went ahead and its line was kept.

## 4. Home paths and account name

```
$ python scripts/home_paths.py --root <user>\projects\phrase-lang
0 files
```
(exit 0). Card 365 records: files containing the account name 0, occurrences 0 (checked before this file was written; this file uses <user> for every home path).

## Notes carried (not blocking, already ruled)

The residual lock hazards of second-review.md (findings 1 to 4) and the gate-review untested card-mode branch of gate_seat are filed as follow-ups; they do not touch the three done checks, which I re-proved above. The throwaway clone and scenario directories are left in place (no recursive delete). Not part of this sign-off: card 374, the D228 line removal, the live parallel window.
