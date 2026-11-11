# Card 417 green record (GREEN writing seat, linked worktree)

STATUS: STOPPED AND REPORTED. The three briefed changes are made, built and run. Two tests the green was not asked to change now fail. Both fail for the same reason: they name the file or class the green moves. The green did not edit either test. The facilitator rules before anything is committed (see "Stop and report").

Changes left uncommitted. No commit, no trailer. No production code, history file, bench/ or other .mob/ file touched. All three files are LF (`git ls-files --eol`: BenchmarkHarnessAcceptance.cs and WallClockTestsRunIsolated.cs `i/lf w/lf`; BenchTimeoutIsolation.cs `w/lf`).

## What the green changed

1. New `tests/Avowry.Acceptance.Tests/BenchTimeoutIsolation.cs`:
   - class `BenchTimeoutIsolation` with exactly `[Collection("Isolated timing")]` and the D247/card 417 comment;
   - in the same file, the one `[CollectionDefinition("Isolated timing", DisableParallelization = true)]` on `public sealed class IsolatedTimingCollection { }`;
   - usings: `Avowry.Acceptance.Tests.Support`, `System.Diagnostics`, `System.Text.Json` (the three the test and its helpers need); no fields.
2. `timed_out_model_tree_is_stopped_and_recorded_in_both_arms` moved by a script that sliced the exact source lines (HEAD lines 12 to 72: the Feature comment, `[Fact]`, `[Trait("Slice", "114")]` and the body). `diff` against `git show HEAD:` lines 12-72 shows no difference, and both 20 second limits are unchanged.
3. `BenchmarkHarnessAcceptance.cs`: deletions only (128 lines: the test and the four helpers below). It keeps `[Collection("Bench processes")]` and its other 44 tests (45 `[Fact]`/`[Theory]` at HEAD, 44 now).
4. `WallClockTestsRunIsolated.cs`: only the line `[Trait("Status", "Pending")] // Pending until the green (card 417)` removed from L1.

## Helpers

The usages were found by searching `tests/` for each identifier the test calls.

| helper | users | where it is now |
|---|---|---|
| `WriteCSharpTimeoutSuite` | the timing test only | moved (byte-identical, `diff` against HEAD 1400-1464) |
| `WriteIgnoringModelTree` | the timing test only | moved |
| `IsRunning` | the timing test only | moved |
| `RunBenchmarkArm` | the timing test only | moved |
| `CopySuiteTo` | the timing test and about 35 other tests in BenchmarkHarnessAcceptance | stays private there; the new class has a private copy |

Why a copy and not `internal static`: a copy leaves the old file's diff as deletions only and keeps the moved body character for character. `internal static` would need either a `using static` or a qualified call. There is also a precedent: `PhraseCheatSheet.cs` (line 210) already holds its own private copy, and `diff` shows the new copy matches it exactly.

## Commands and output

`dotnet --version`: `10.0.404` before and after.

```
dotnet build Avowry.sln -c Debug --nologo
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=417"
  Failed Avowry.Acceptance.Tests.WallClockTestsRunIsolated.L2_live_tree_detected_set_contains_BenchmarkHarnessAcceptance [170 ms]
  Error Message:
   Assert.Contains() Failure: Item not found in collection
Collection: ["BenchTimeoutIsolation"]
Failed!  - Failed:     1, Passed:    23, Skipped:     0, Total:    24, Duration: 220 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

L1 (no Pending trait now) passes. It is among the 23.

```
dotnet test ... --no-build -c Debug --filter "FullyQualifiedName~BenchTimeoutIsolation"
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 10 s - Avowry.Acceptance.Tests.dll (net8.0)
```

```
dotnet test ... --no-build -c Debug --filter "FullyQualifiedName~BenchmarkHarnessAcceptance"
  Failed Avowry.Acceptance.Tests.WallClockTestsRunIsolated.L2_live_tree_detected_set_contains_BenchmarkHarnessAcceptance [62 ms]
Failed!  - Failed:     1, Passed:    44, Skipped:     0, Total:    45, Duration: 1 m 1 s - Avowry.Acceptance.Tests.dll (net8.0)
```

This filter also matches L2, whose method name contains the class name. The 44 passes are the whole remaining BenchmarkHarnessAcceptance class.

```
dotnet test ... --no-build -c Debug --filter "FullyQualifiedName~GeneratedApiSourceScan|FullyQualifiedName~NoBlockingProcessWaits"
  Failed Avowry.Acceptance.Tests.GeneratedApiSourceScan.Guard_checked_set_of_process_start_callers [29 ms]
  Error Message:
   Assert.Equal() Failure: Collections differ
Expected: OrderedEnumerable<string, string> ["BenchmarkHarnessAcceptance.cs", "CSharpBaselineAcceptance.cs", "EnsureStepCompiles.cs", "GettingStartedDocs.cs", "LlmCustomerCheck.cs", ···]
Actual:   List<string>                      ["BenchTimeoutIsolation.cs", "BenchmarkHarnessAcceptance.cs", "CSharpBaselineAcceptance.cs", "EnsureStepCompiles.cs", "GettingStartedDocs.cs", ···]
Failed!  - Failed:     1, Passed:    33, Skipped:     0, Total:    34, Duration: 142 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

```
python -m unittest tests.test_bench_launcher_scan_264
Ran 6 tests in 0.056s

OK
```

The new file contains `Project: "Avowry.Bench"`, so the Python scan reads it. It contains no quoted `"--project"`, so nothing is flagged.

The full solution suite was not run (it is the facilitator's gate).

## Stop and report: two tests the green was not asked to change

1. **`WallClockTestsRunIsolated.L2_live_tree_detected_set_contains_BenchmarkHarnessAcceptance` (the red's born-green test) is a flaw in the red.**
   - It hard-codes the class name `BenchmarkHarnessAcceptance`. Card 417 criterion 2 moves the timing test out of that class.
   - After the move, the detector finds exactly `["BenchTimeoutIsolation"]`, which is the correct result. So L2 cannot stay green after any correct green.
   - Its purpose (a blind scanner must not pass L1 forever) still holds if it asserts `BenchTimeoutIsolation`, or that the detected set is not empty. Both its name and its Then comment would change.
2. **`GeneratedApiSourceScan.Guard_checked_set_of_process_start_callers` needs the new file in its exact allow-list.**
   - `RunBenchmarkArm` starts the process, and criterion 2 requires it to live in BenchTimeoutIsolation.
   - BenchmarkHarnessAcceptance.cs still has another process start (the helper near the end of the file), so its own entry stays.
   - The fix is one added entry `"BenchTimeoutIsolation.cs"` in `ProcessStartCallers`. The card's scenario [I] requires this guard to still pass.
   - The green did not get around the guard with a respelling of the call.

Neither edit is made. Both wait for the facilitator's ruling.

## Other notes

- The comment on BenchmarkHarnessAcceptance's `[Collection("Bench processes")]` still says the class starts the bench tools "under a 20 second limit". After the move it no longer does. It is left unchanged, as briefed.
- Scratch file left in place: `/tmp/old-test-417.txt` (the HEAD slice used for the identity diff).
