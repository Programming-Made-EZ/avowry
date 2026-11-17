CHANGES

# Card 172 QA review round 1 (claude opus, ctx-172q; saved by the facilitator from the seat's reply: the brief's verdict path was missing)

Range `git diff 8b78eb3..e0780f2` (red 92e69a2, green e0780f2), read-only, no dotnet.

BLOCKING
1. `GeneratedApiSourceScan.Guard_checked_set_of_process_start_callers` (GeneratedApiSourceScan.cs:21-29, :80-91; Slice 214, not Pending) pins the exact set of files containing `Process.Start(`. After the green, Support/DotnetCli.cs, Support/PhraseCli.cs, Support/ProcessRunner.cs, SourceMapDeployment.cs and SourceMapTraceCommand.cs no longer contain it and Support/NestedDotnet.cs does. The filtered proof never ran it; the full gate fails. Fix: update `ProcessStartCallers` (test data; nothing removed or renamed).

MINOR
2. NestedDotnet.cs:33-56: after the tree kill, the wait for exit and both stream drains are unbounded; a pipe held outside the tree hangs instead of throwing.
3. ProcessRunner.cs:13-14: a null timeout used to mean no limit and now means 5 minutes (PrivateHoldoutResults.cs:310 RunPython); TimedOut is ignored.
4. NestedDotnetTimeout172.cs: GoneAsync's 20 s polling cannot tell kill-and-wait from kill-without-wait; the finally cleans only pids already read (read the pid file inside the finally); KillIfRunning by pid could hit a reused pid.
5. NestedDotnetHelper172.cs scans are fairly easy to fool (6 of 14 names; the Kill regex misses a method group).
6. docs/TRACEABILITY.md:11 is stale on the new missing-file warning and the exit 2 refusal.
7. Program.cs:189-195: a missing-file warning hides an assembly-check message (a hash mismatch already did).

Checks out: criterion 1 met (whole-map validation before any read, exit codes as R172-1); criterion 2 met apart from 1 to 3 (12-name scrub harmless for DotnetCli and PhraseCli; GeneratedApi unchanged in effect); criterion 3: no test removed or renamed, src only Program.cs, pins untouched; the red's tests mostly strong.
