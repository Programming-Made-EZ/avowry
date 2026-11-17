# Card 172 red record (red seat: claude sonnet, claude-sonnet-5-5, ctx-172r)

Worktree `~/projects/wt-172`, branch card-172. Nothing committed. No dotnet was run (not built, not tested): the tests below are unverified by a compiler, so the facilitator's first run may show a compile error to fix before any red/green reading.

All new files are under `tests/Avowry.Acceptance.Tests/`; every test carries `[Trait("Slice","172")]` and `[Trait("Criterion","172-N")]`.

## Tests by criterion

### 172-1 trace guards: `TraceGuards172.cs` (class TraceGuards172; runs the real CLI through PhraseCli, map planted by hand in a temp folder)
- `Trace_of_a_deleted_generated_file_warns_that_it_is_missing`: exit 0; output has WARNING, "missing" and the csFile text.
- `Strict_trace_of_a_deleted_generated_file_exits_one`: `--strict` exits 1 (as a hash mismatch does) and names the file.
- `Trace_refuses_a_csFile_that_leaves_the_output_folder` (Theory, 8 forms: dotdot, nested-dotdot, backslash-dotdot, absolute, leading-slash, leading-backslash, drive-letter, other-record). For the forms that can resolve, the outside file really exists with a matching hash, so only a guard before any read can refuse it. Each is run plain and with `--strict`: exit 2, stderr names the record and "csFile", stdout empty (no record printed), no unhandled exception. `other-record` plants a good traced record plus a second record `OtherStep` with a bad csFile; whole-map validation must refuse and name `OtherStep`.
- `Trace_still_accepts_a_present_unchanged_file_and_a_name_that_only_contains_two_dots` (guard against over-refusing): `Gen/Planted.cs` and `Gen..v2/Planted..cs`, strict, exit 0, no warning. Segment test, not substring.

### 172-2 shared helper
`NestedDotnetHelper172.cs` (settings and source scans, offline text reads; comments are removed, string literals kept):
- `The_shared_helper_applies_node_reuse_roll_forward_and_scrubs_every_listed_variable`
- `The_shared_helper_leaves_unrelated_variables_and_arguments_alone`
- `The_listed_helpers_call_the_shared_helper` (DotnetCli, GeneratedApi, PhraseCli, SourceMapDeployment, SourceMapTraceCommand each mention `NestedDotnet`; Support/NestedDotnet.cs exists)
- `The_scrub_list_and_the_node_reuse_settings_are_defined_once_in_the_shared_helper` (Theory over the quoted literals MSBUILD_EXE_PATH, MSBuildSDKsPath, IntermediateOutputPath, UseAppHost, MSBUILDDISABLENODEREUSE, DOTNET_ROLL_FORWARD: across DotnetCli, GeneratedApi, PhraseCli, ProcessRunner, NoMsbuildNodeReuse, SourceMapDeployment, SourceMapTraceCommand and NestedDotnet, each literal appears exactly once, in Support/NestedDotnet.cs)
- `No_listed_helper_kills_a_process_itself_and_the_shared_helper_kills_the_whole_tree` (no `.Kill(` in the seven listed files; NestedDotnet.cs contains `Kill(entireProcessTree: true)`)

`NestedDotnetTimeout172.cs` (class in `Collection("Isolated timing")`, because it waits on a real timeout; passes the WallClock isolation scan):
- `A_timed_out_child_and_its_grandchild_are_both_gone`: a child (powershell.exe on Windows, sh elsewhere) starts a grandchild (ping / sleep, 600 s), writes `<childpid> <grandchildpid>` to a file named by env TREE_PID_FILE, then sleeps. The helper runs it with a 20 s timeout (outer bound 120 s). Asserts the helper reports the timeout, then polls both pids for up to 20 s until `GetProcessById` fails or `HasExited`. A finally block kills any survivor with its own tree, so a red run leaves nothing behind.
- `A_child_that_finishes_in_time_is_not_reported_as_timed_out` (`cmd /c echo done` / `sh -c`).

The tests reach the helper by reflection (`Support/NestedDotnetSeam172.cs`), so its absence fails only these tests and does not stop the project compiling.

### 172-3: `ExistingTestsKept172.cs` + `ExistingTestNames172.txt`
- Names before the card (657 Fact/Theory methods of both test projects, as `relative/path.cs::Method`, sorted) are recorded in `tests/Avowry.Acceptance.Tests/ExistingTestNames172.txt`, written before any new test file existed (generated with a small python scan, same line rule as the test).
- `Every_test_that_existed_before_the_card_is_still_there_under_the_same_name_and_file`: rescans both projects; every recorded name must still be found.
- `The_generated_csharp_pins_are_unchanged`: WebExtensionBoundary.cs still pins `OrdersOutputHash = "19fe6d71`, and TasksDemoGeneratedCsharp.golden exists. The real output check stays with Slice 142 and 176.

## Expected today (before green)

Fail today (22 tests, counting each Theory row: 10 + 2 + 8 + 2):
- 172-1, all 10 rows/tests except the guard: deleted-file warns (CheckHash returns false for a missing file, so exit 0 and no warning); strict deleted (exit 0, not 1); the 8 refusal rows (today exit 0; absolute and `..` files that exist and match are read and trusted; the missing ones are simply "not mismatching"). That is 2 + 8 = 10 reds.
- 172-2 settings: both Apply tests (NestedDotnet does not exist).
- 172-2 scans: listed helpers call the shared helper (none does); defined-once x 6 rows (each literal sits in 4 to 7 files today); no-Kill scan (DotnetCli, GeneratedApi, PhraseCli, ProcessRunner, SourceMapDeployment, SourceMapTraceCommand call `Kill(`). 8 reds.
- 172-2 timeout: both tests in NestedDotnetTimeout172 (the helper type is missing, so they fail at once, before any process starts).

Pass today (guards): `Trace_still_accepts_a_present_unchanged_file...`, both 172-3 tests.
The existing Slice 167, 168, 142 and 176 tests should be unaffected and pass today.

## Seam the green must add
`tests/Avowry.Acceptance.Tests/Support/NestedDotnet.cs`, type `Avowry.Acceptance.Tests.Support.NestedDotnet` (static; public or internal):
- `Apply(ProcessStartInfo)`: sets `DOTNET_ROLL_FORWARD=Major`, `MSBUILDDISABLENODEREUSE=1`, and removes these 12 names from the start info: MSBUILD_EXE_PATH, MSBuildExtensionsPath, MSBuildSDKsPath, MSBuildLoadMicrosoftTargetsReadOnly, OutputPath, OutDir, BaseOutputPath, BaseIntermediateOutputPath, IntermediateOutputPath, Configuration, TargetFramework, UseAppHost. Touches nothing else (arguments, working directory, other variables).
- `RunAsync(ProcessStartInfo, TimeSpan)` returning a Task: start, drain both streams from the start, wait for exit, and on timeout kill the entire tree (`Kill(entireProcessTree: true)`), wait for exit and for both drains. It may either throw `TimeoutException` or return a result with a bool property `TimedOut` (ProcessRunner's `ProcessResult` already has it). A finished-in-time child must not report a timeout.
- A tree-kill entry for the server case (GeneratedApi's dispose, lines 112 and 243), and a way for `NoMsbuildNodeReuse` to set the process-wide variable without repeating the literal.
- Each quoted literal in the six-row Theory must appear once in NestedDotnet.cs and in none of the other seven files (use a const or the one list; a comment mention is fine).
- DotnetCli, PhraseCli, GeneratedApi, SourceMapDeployment and SourceMapTraceCommand must mention `NestedDotnet`; none of the seven listed files may contain `.Kill(`.

Product (`src/Avowry.Cli/Program.cs`): in `Validate` refuse a csFile that is rooted, has a drive letter, starts with `/` or `\`, or has a `..` segment split on both `/` and `\` (exit 2, stderr names the record and says csFile); in `CheckHash` report a missing file as a warning naming the csFile, nonzero (1) under `--strict`. Do not touch the generated C#.

## Mutation steps (for the facilitator, after green; each should turn the named tests red, then be reverted)
1. In `CheckHash` restore `File.Exists(path) && ...` (missing file silent): the two deleted-file tests go red.
2. Remove the `..` check from the csFile guard: the dotdot, nested-dotdot and backslash-dotdot rows go red. Remove only the rooted check: absolute, leading-slash, leading-backslash, drive-letter go red. Make the guard cover only the traced record: `other-record` goes red. Make the guard a substring test on `..`: the over-refusal guard test goes red.
3. In NestedDotnet change `Kill(entireProcessTree: true)` to `Kill()`: the timeout test goes red (grandchild survives) and the Kill scan goes red. Drop the kill on timeout: the timeout test goes red.
4. Drop `MSBUILDDISABLENODEREUSE` or one scrub name from the helper: the Apply test goes red. Paste one literal back into DotnetCli (or any listed file): its defined-once row goes red.
5. Rename any test in SourceMapTraceCommand or SourceMapDeployment: the 172-3 names test goes red. Change a character of the `19fe6d71` pin: the pin guard goes red.

## Ambiguities and risks (for the facilitator to rule)
- The scrub list is the 12-name union already used by GeneratedApi, SourceMapDeployment and SourceMapTraceCommand (DotnetCli and PhraseCli scrub only the first four today). I read "one scrub list" as the union; the tests require all 12 gone. The GeneratedApi-only ASPNETCORE_* removals and the logging variable are left to GeneratedApi (the tests do not mention them).
- ProcessRunner is in the Kill and defined-once scan set although the slice names it only among the copies. It may delegate to NestedDotnet or be folded into it; it may not keep its own `.Kill(`.
- `--strict` on a missing file: I assert exit 1 (following the hash mismatch). The slice says only "nonzero"; 1 is the follow-the-mismatch reading.
- Whole-map validation (any record with a bad csFile refuses the trace, even of another name) is the `other-record` row; the slice puts the guard in the validation near line 258, which reads that way. If the facilitator rules per-traced-record, drop that row.
- Out of scope per the slice and untouched by the scans: bench and LLM-check handling, and the other test files that set MSBUILDDISABLENODEREUSE (EnsureStepCompiles, TasksDemoApp, UsableV1 and others); only the seven files above are scanned.
- The timeout test needs powershell.exe on Windows (present on Windows 11) or sh elsewhere, and takes about 20 s plus a short poll. It sits in the "Isolated timing" collection so it runs alone.
- Unverified by a compiler: please watch for a compile error in the list and tuple patterns in NestedDotnetHelper172.cs and the reflection calls in NestedDotnetSeam172.cs.

## Fix round 1 red (R172-2)

Tests only; no dotnet run; no test removed or renamed; nothing committed.

- B1, `GeneratedApiSourceScan.cs` `ProcessStartCallers`: now exactly the 20 files that contain `Process.Start(` in tests/Avowry.Acceptance.Tests (checked by grep): the 17 unchanged entries, plus `Support/NestedDotnet.cs`; removed `Support/DotnetCli.cs`, `Support/PhraseCli.cs`, `Support/ProcessRunner.cs`, `SourceMapDeployment.cs`, `SourceMapTraceCommand.cs`. None of the card 172 test files contains `Process.Start(`. Expected: `Guard_checked_set_of_process_start_callers` (Slice 214) passes.
- M4, `NestedDotnetTimeout172.cs` `A_timed_out_child_and_its_grandchild_are_both_gone`: a watcher task starts before the child and, as soon as the pid file lists both ids, records each live process's start time. The `finally` stops the watcher, reads the pid file itself (so a failure before the assertions still cleans up), and `KillIfRunning(pid, recordedStart)` kills the tree only when the process's start time is within 1 s of the recorded one; a pid with no recorded start time (never seen alive) or a different start time is left alone. The assertions are unchanged (helper reports timeout; both pids gone within the bounded poll).
- Reminder from R172-2: later proofs run the full `Status!=Pending` set, not a slice filter. The watcher uses `Task.Delay` with a token and a plain `CancellationTokenSource` (no WhenAny, no timed source), so the wall-clock isolation scan is unaffected; the class stays in "Isolated timing".
