# Card 264 sign-off (sign-off, ctx-6, fresh context)

Seat: Claude (a fresh subagent, read-only; reply recorded by the facilitator, command scan clean). Environment: Windows 11 10.0.26300, Python 3.15.1, dotnet 10.0.403, HEAD 7aff183 (read-only). It ran the five Python test modules (41 tests OK), `python scripts/mob_process.py --skip-regeneration check` (OK, 109 closed cards, 18 records, 91 legacy), `python scripts/check-repeat-evidence.py` (OK) and the dotnet filter run (28 passed, 0 failed). It did not run the batch, the class or the whole acceptance project.

## Verdict: SIGNED OFF with conditions

The three Done checks are met as written. The root cause of the 2026-11-01 CI failure is fixed in its mechanism, not proven for the CI trigger. The card and audit FAIL 123 must stay open until the closure condition below is recorded.

| Check | Evidence | Result |
|---|---|---|
| 1. 20 loaded passes | .mob/records/264/repeat-evidence.json: 20 runs numbered 1 to 20, all passed, 17 rows each, load exit 0, none killed; slowest row 40.65 s per the seat's read (the facilitator's own read of the final record: 46.32 s, run 20) against a 600 s boundary; class PhraseExecutedOutcomesAcceptance, load class BenchmarkHarnessAcceptance, concurrency 2, commands, summary 20/20/0. scripts/check-repeat-evidence.py accepts it; scripts/mob_process.py `check_repeat_evidence` (55cb037) is in run_check and fails closed. Batch code HEAD b7c1958, after cycle 2b. | Met |
| 2. Root cause, reproduction, fix | findings.md maps five suspects: shared build and missing results file ruled in; starvation, external dependency and timing/ordering ruled out. Reds c0cb398 and 18327f3 came before greens a7ad8c5 and dc1c39c. The diagnostic gave anomalies=2 (legacy, exit 1, DLL in use) before the fix and 0 after. BenchChild.RunAsync checks the exit code before reading history and reports stdout, stderr and the history path; BenchLauncher runs `run --no-build -c <config>` from the test assembly's directory and throws on missing or stale output with no build fallback. | Met, with the root-cause caveat |
| 3. The 11:27 run | findings.md and ci-evidence.md agree: run 41341801479 (sha f682823) had a green `test` job and a failed `mob-process` job: a different job, the card does not widen. A second run 41341813972 on the same sha is noted as unowned. The seat did not re-fetch the log. | Met |

## Root cause

The CI failure was never reproduced locally ("not reproduced locally" in findings.md). Proven: the harness read results before checking the child's exit (the cause of the DirectoryNotFoundException in CI), and concurrent `dotnet run` builds of the shared Phrase.Bench obj directory collide locally (exit 1, no history). The collision is the most likely trigger, not a proven one: the 11-01 CI log shows no rebuild collision and the child's own output is unrecoverable. The fix removes both the masking and the demonstrated trigger; if CI fails again from another cause, the harness reports the child's real error. Conclusion: fixed mechanism, trigger inferred.

Launcher call sites: six outside the contract tests (CreateStartInfo serves five, CreateStartInfoFor one); the window 264-scope-2 text says five sites in four classes.

## Conditions for the closing record

1. Do not close card 264 or audit FAIL 123 until the closing commit is on main with a green `ci` run that includes the process check and the test job (the sign-off run 42087877796 was in progress when the seat looked).
2. State that the CI failure was never reproduced and the trigger is inferred from a local reproduction of the same defect class; if the flake recurs, reopen with the child's own output, now preserved.
3. State the pre-ship gate's errors: it said no other seat-written CRLF files matter, while the facilitator's note lists eight (two restored to LF in ca7a8f9; the six new files and the evidence JSON stay CRLF, ruled acceptable by this seat as a recorded deviation); its HEAD was 82026bf, not the committed 7aff183 (code equal to ca7a8f9).
4. State that green 2 (a7ad8c5) was pushed with CI red from about 14:44 to 15:49 UTC (the GeneratedApiSourceScan guard test) after one class was run; fixed by 2f331cd. Failed runs 42054294794 and 42058204432. Probe ages at seat start were all under the 50-minute D188 line (no stale-probe breach found).
5. State the stray commit c380122 that appended an out-of-order duplicate Copilot probe line (13:28:12Z); deeaa0b notes it; no other effect.
6. State the scope and cap history: criterion 1 two cycles (1a, 1b), criterion 2 two (2 and 2b, plus the 2c guard fix), criterion 3 one; windows 264-scope and 264-scope-2 adopted; the sum 5 is above the D175 limit of 3; cycle 1b's batch ran as a facilitator machine job of about 100 minutes.
7. Follow-ups: card 302 (the Phrase.BenchCs children still start build-capable) and card 303 (two C# baseline tests fail only on the Windows development machine, identical on a7ad8c5); UsableV1.Bar6 stale LLM evidence (card 162's area, unrelated); the failed mob-process run 41341813972 of 2026-11-01 needs an owner or a note that it is already owned.
8. The card text needs no change; the window text's "five start sites" is six in the implementation.
