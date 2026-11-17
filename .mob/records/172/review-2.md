SIGN-OFF

# Card 172 review round 2 (QA seat: claude opus, claude-opus-5-5, ctx-172q)

Range `git diff e0780f2..76c14e1` (fix round 1: red 36d642e, green 76c14e1), against R172-2 in slice.md. Read-only; no dotnet run. The gate facts are the facilitator's (76c14e1, full `Status!=Pending` set: build 0 errors, compiler 344/344, acceptance 1550/1550).

## Round 1 findings, as ruled

- B1 fixed. `ProcessStartCallers` (tests/Avowry.Acceptance.Tests/GeneratedApiSourceScan.cs:21-30) now drops DotnetCli, PhraseCli, ProcessRunner, SourceMapDeployment and SourceMapTraceCommand and adds Support/NestedDotnet.cs. It matches a text grep of the tree for `Process.Start(` exactly (20 files). The test's name, assertion and scan are unchanged; only its data list moved. The full gate ran it.
- M2 fixed. After the tree kill, the exit wait (Support/NestedDotnet.cs:53-60) and each drain (`DrainWithBoundAsync`, :79-90) are bounded by `PostKillWaitBound` = 10 s (:10). The worst case is about 30 s after the kill, then the helper reports TimedOut. Without a timeout, nothing changes.
- M3 fixed. ProcessRunner passes null through (Support/ProcessRunner.cs:15), and NestedDotnet only treats a cancel as a timeout when a limit was given (NestedDotnet.cs:49). So null means no limit again, and PrivateHoldoutResults.RunPython goes back to the card 217 behaviour. A timeout now throws TimeoutException (ProcessRunner.cs:16-21).
- M4 fixed. A watcher starts before the child, records each listed pid's start time while that process is alive (NestedDotnetTimeout172.cs:150-171), and the finally block re-reads the pid file (:49). `KillIfRunning` kills only when the live start time is within 1 s of the recorded one (:192). A pid that was never recorded is never touched. The test's assertions (timed out, two distinct pids, both gone) are unchanged, so it is not weaker. If the helper hangs past HelperBound, the cleanup now still finds the processes.
- M6 fixed. docs/TRACEABILITY.md:11 states the missing-file warning (exit 1 with `--strict`) and the exit 2 refusal.
- M5 and M7 are recorded as named limits; I did not recheck them.

## New findings (all MINOR)

1. MINOR. The M3 throw drops the output that callers used to report. Many callers handled `TimedOut` themselves and printed stdout and stderr with it, for example PackedToolIsVersionedInstallsAndBuildsTheDemo.cs:79-87 and :112-120 (`Extra(...)`), RestoredPackageLicencesLockedRestore.cs:792 (`ClassifyRestore(..., result.TimedOut, ...)`), and the `!result.TimedOut` asserts in about 15 files. Those branches are now unreachable, and a timeout surfaces as `TimeoutException("<file> exceeded <limit>.")` without the process output. Most callers fill `ArgumentList`, not `Arguments`, so the "command" in the message is often just `dotnet` or `git` (ProcessRunner.cs:18-20). Every timeout still fails, so no check got weaker; only the diagnostics did. A later card could put the arguments and the drained output in the exception text.
2. MINOR. Sometimes the M2 bound ends in a different exception instead of a reported timeout. If the killed process has still not exited after the 10 s exit bound (the tree kill failed, or access was denied), `process.ExitCode` at Support/NestedDotnet.cs:76 throws InvalidOperationException ("Process must exit..."). The helper no longer hangs, but in that rare case it does not "report the timeout as before" (R172-2 M2). A guard such as `process.HasExited ? process.ExitCode : -1` would close it.
3. MINOR (nit). The comment at StarvationProbe.cs:21 still describes ProcessRunner as returning TimedOut true on expiry. The ProcessRunner summary was updated (ProcessRunner.cs:6-9), but that comment was not.
4. MINOR (nit). docs/TRACEABILITY.md:11 says "before any file is read". The map itself is read first; "before any generated file is read" would be exact.

## What I could not check

- I could not compile or run anything (no dotnet), so I rely on the facilitator's gate run for the whole suite, including the B1 guard and the timeout test.
- I did not run the M4 cleanup path (the helper hangs, the test fails early) or the new M2 bound path. Both are read statically.
- I did not check whether `Process.StartTime` is readable for the powershell and ping processes on every runner (it is for processes you own on Windows; on Linux it reads /proc).
