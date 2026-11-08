# Card 176 red (D157 Criteria 1 and 2), QA (red) seat

Pending new-state red for criteria 1 and 2, one test, committed with `Status=Pending` so the gate excludes it. Watched failing by assertion on the pre-change tree (no packaging change in `src/` or `Directory.Build.props`).

## Pre-change commit hash

`git rev-parse HEAD` before this commit: `2c05ffe6b59937ecef7d51af04ebf5a3b28a522e` (the characterisation commit; production tree still matches `dc2db36afd9d259e238a98811afdf565500c4c8a`).

## Command

```
dotnet test tests/Avowry.Acceptance.Tests --nologo --filter FullyQualifiedName~Packed_tool_is_versioned_installs_and_builds_the_demo
```

## Failing test

`Avowry.Acceptance.Tests.PackedToolIsVersionedInstallsAndBuildsTheDemo.Packed_tool_is_versioned_installs_and_builds_the_demo`

Marked `[Trait("Status", "Pending")]` and `[Trait("Criterion", "176-1")]`. D137 (`TEAM_WORKING_AGREEMENT.md` Practices, and `docs/decisions/process-and-staffing.md` D137 mechanism (1)) requires both traits, with `Criterion=NNN-K`. Criteria 1 and 2 share this one pending test (D137: at most one pending at a time). Live tree only had `[Trait("Status", "Pending")]` on `UsableV1.Bar6` (no Criterion). Card 324 used `[Trait("Criterion", "1")]` without Pending.

## Expected failure

Assertion failure from `Checks.AssertAll`, not a compile error and not a helper exception. Six named checks; four failed on their own assertion; two were blocked by the missing installed tool (pack still produced `Avowry.Cli.1.0.0.nupkg`, which is not an Avowry tool package).

Failed on their own assertion (evidence they catch their violation):

- `pack`: `nupkg is not named for package ID Avowry: Avowry.Cli.1.0.0.nupkg` (`dotnet pack` exited 0).
- `metadata`: `id is Avowry.Cli; Version is 1.0.0; tool command is <missing>` (nuspec inspected; no `DotnetToolSettings.xml`).
- `explicit-version-source`: `CLI project and Directory.Build.props set no Version or PackageVersion` (reads those files; independent of pack).
- `install`: `dotnet tool install exited 1; installed command missing` plus `avowry is not found in NuGet feeds <local feed>` (public sources cleared; did not reach nuget.org). Global tool list was unchanged.

Blocked by the missing package / installed tool (not counted as evidence they catch their violation):

- `version`: `blocked: tool not installed`
- `demo-build`: `blocked: tool not installed`

## Quoted output

```
[xUnit.net 00:00:04.20]     Avowry.Acceptance.Tests.PackedToolIsVersionedInstallsAndBuildsTheDemo.Packed_tool_is_versioned_installs_and_builds_the_demo [FAIL]
  Failed Avowry.Acceptance.Tests.PackedToolIsVersionedInstallsAndBuildsTheDemo.Packed_tool_is_versioned_installs_and_builds_the_demo [3 s]
  Error Message:
   pack (failed): nupkg is not named for package ID Avowry: Avowry.Cli.1.0.0.nupkg
metadata (failed): id is Avowry.Cli; Version is 1.0.0; tool command is <missing>
explicit-version-source (failed): CLI project and Directory.Build.props set no Version or PackageVersion
install (failed): dotnet tool install exited 1; installed command missing
avowry is not found in NuGet feeds C:\Users\ringe\AppData\Local\Temp\phrase-acceptance\a968d03e9f94408eaf057619fdb861b8.

version (blocked): blocked: tool not installed
demo-build (blocked): blocked: tool not installed
  Stack Trace:
     at Avowry.Acceptance.Tests.PackedToolIsVersionedInstallsAndBuildsTheDemo.Checks.AssertAll() in C:\Users\ringe\projects\wt-176-red\tests\Avowry.Acceptance.Tests\PackedToolIsVersionedInstallsAndBuildsTheDemo.cs:line 263
   at Avowry.Acceptance.Tests.PackedToolIsVersionedInstallsAndBuildsTheDemo.Packed_tool_is_versioned_installs_and_builds_the_demo() in C:\Users\ringe\projects\wt-176-red\tests\Avowry.Acceptance.Tests\PackedToolIsVersionedInstallsAndBuildsTheDemo.cs:line 74

Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 3 s - Avowry.Acceptance.Tests.dll (net8.0)
```

The local-feed directory in the install line changes per run.

## Line count of the new C#

`tests/Avowry.Acceptance.Tests/PackedToolIsVersionedInstallsAndBuildsTheDemo.cs`: 265 lines (235 nonempty). Under the 400-line cap.

## Gate-filter result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter Status!=Pending&FullyQualifiedName~Packed_tool_is_versioned`

```
No test matches the given testcase filter `Status!=Pending&FullyQualifiedName~Packed_tool_is_versioned` in C:\Users\ringe\projects\wt-176-red\tests\Avowry.Acceptance.Tests\bin\Debug\net8.0\Avowry.Acceptance.Tests.dll
```

0 tests run, exit 0.
