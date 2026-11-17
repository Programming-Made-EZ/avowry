# Card 172 green record (green seat: cursor composer-2.5, ctx-172g)

Worktree `~/projects/wt-172`, branch `card-172`. Nothing committed. No dotnet run (facilitator gate).

## git diff --stat

```
 src/Avowry.Cli/Program.cs                          | 39 ++++++++++--
 .../Avowry.Acceptance.Tests/SourceMapDeployment.cs | 33 ++--------
 .../SourceMapTraceCommand.cs                       | 33 ++--------
 tests/Avowry.Acceptance.Tests/Support/DotnetCli.cs | 35 ++---------
 .../Support/GeneratedApi.cs                        | 34 +++--------
 .../Support/NestedDotnet.cs                        | 71 ++++++++++++++++++++++
 .../Support/NoMsbuildNodeReuse.cs                  |  4 +-
 tests/Avowry.Acceptance.Tests/Support/PhraseCli.cs | 61 ++++---------------
 .../Support/ProcessRunner.cs                       | 42 +------------
 9 files changed, 145 insertions(+), 207 deletions(-)
```

## Changes by file and red tests answered

### `src/Avowry.Cli/Program.cs`
- **Validate:** `CsFileLeavesOutputFolder` rejects any map record whose `csFile` is rooted, drive-qualified, leading `/` or `\`, or has a `..` path segment; malformed message names `record {Name}` and `csFile` (whole-map check).
- **CheckHash:** missing generated file yields a WARNING line naming the csFile; still sets mismatch so `--strict` exits 1; existing hash mismatch unchanged.
- **172-1:** `Trace_of_a_deleted_generated_file_warns_that_it_is_missing`, `Strict_trace_of_a_deleted_generated_file_exits_one`, all eight `Trace_refuses_a_csFile_that_leaves_the_output_folder` rows (including `other-record`), `Trace_still_accepts_a_present_unchanged_file_and_a_name_that_only_contains_two_dots` (segment-only `..` guard).

### `tests/Avowry.Acceptance.Tests/Support/NestedDotnet.cs` (new)
- **Apply:** `DOTNET_ROLL_FORWARD=Major`, `MSBUILDDISABLENODEREUSE=1`, scrubs the 12-name union list.
- **RunAsync:** start, drain stdout/stderr from start, wait; on timeout `Kill(entireProcessTree: true)`, await exit and drains; returns `ProcessResult` with `TimedOut`.
- **KillEntireTreeIfRunning** for server dispose; **ApplyProcessWideNodeReuseDisable** for module initializer (single quoted `MSBUILDDISABLENODEREUSE`).
- **172-2:** `The_shared_helper_applies_node_reuse_roll_forward_and_scrubs_every_listed_variable`, `The_shared_helper_leaves_unrelated_variables_and_arguments_alone`, `The_scrub_list_and_the_node_reuse_settings_are_defined_once_in_the_shared_helper` (six Theory rows), `No_listed_helper_kills_a_process_itself_and_the_shared_helper_kills_the_whole_tree`, `A_timed_out_child_and_its_grandchild_are_both_gone`, `A_child_that_finishes_in_time_is_not_reported_as_timed_out`.

### `tests/Avowry.Acceptance.Tests/Support/DotnetCli.cs`
- Delegates scrub and run to `NestedDotnet`; removed local env copies and `.Kill(`.
- **172-2:** listed-helper and defined-once / no-Kill scans; indirect coverage of nested test runs.

### `tests/Avowry.Acceptance.Tests/Support/PhraseCli.cs`
- `NestedDotnet.Apply` / `RunAsync` for CLI and bootstrap build; removed `ApplyHostEnvironment` and kills.
- **172-1:** all TraceGuards172 tests (CLI path); **172-2:** listed-helper scan.

### `tests/Avowry.Acceptance.Tests/Support/GeneratedApi.cs`
- `NestedDotnet.Apply` in `Prepare`, `RunAsync` for build subprocess, `KillEntireTreeIfRunning` on dispose; ASPNETCORE scrub/logging unchanged locally.
- **172-2:** listed-helper, defined-once, no-Kill scans.

### `tests/Avowry.Acceptance.Tests/Support/ProcessRunner.cs`
- Forwards `RunAsync` to `NestedDotnet`; removed duplicate kill/wait logic.
- **172-2:** defined-once and no-Kill scans.

### `tests/Avowry.Acceptance.Tests/Support/NoMsbuildNodeReuse.cs`
- Module initializer calls `NestedDotnet.ApplyProcessWideNodeReuseDisable` (no repeated literal).
- **172-2:** defined-once row for `MSBUILDDISABLENODEREUSE`.

### `tests/Avowry.Acceptance.Tests/SourceMapDeployment.cs`
- Publish path uses `NestedDotnet.Apply` and `RunAsync`.
- **172-2:** `The_listed_helpers_call_the_shared_helper`, defined-once, no-Kill.

### `tests/Avowry.Acceptance.Tests/SourceMapTraceCommand.cs`
- `BuiltTasks.BuildAsync` uses `NestedDotnet.Apply` and `RunAsync`.
- **172-2:** listed-helper, defined-once, no-Kill.

### Untouched (172-3)
- Slice 172 test files and generated C# unchanged; `ExistingTestsKept172` needs no green edits.

## Fix round 1 green (R172-2)

Worktree `~/projects/wt-172`, branch `card-172`. Nothing committed. No dotnet run (facilitator gate).

### git diff --stat (this round only)

```
 docs/TRACEABILITY.md                               |  2 +-
 .../Support/NestedDotnet.cs                        | 45 +++++++++++++++++++---
 .../Support/ProcessRunner.cs                       | 17 ++++++--
 3 files changed, 54 insertions(+), 10 deletions(-)
```

### Changes and review findings

- **NestedDotnet.cs** — **finding 2 (M2):** `PostKillWaitBound` (10 s); after tree kill, exit wait and stdout/stderr drains use that bound via `DrainWithBoundAsync`; still returns `TimedOut` instead of hanging. Nullable `timeout` on `RunAsync` restores unbounded wait when null (supports M3).
- **ProcessRunner.cs** — **finding 3 (M3):** forwards null timeout to `NestedDotnet` (no 5-minute default); when a limit was given and `TimedOut`, throws `TimeoutException` naming command and limit.
- **docs/TRACEABILITY.md** — **finding 6 (M6):** trace paragraph documents missing generated file as warning (exit 1 with `--strict`) and `csFile` outside the output folder refused with exit 2.