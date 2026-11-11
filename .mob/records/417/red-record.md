# Card 417 red record (RED writing seat, linked worktree)

Changes left uncommitted for the facilitator. Existing files were not touched. The new guard file uses LF. The new class starts nothing and carries no `[Collection]`.

## What the red adds

New file `tests/Avowry.Acceptance.Tests/WallClockTestsRunIsolated.cs` (class `WallClockTestsRunIsolated`): an offline text scanner of `tests/Avowry.Acceptance.Tests/**/*.cs` (no build, no process). Code variant blanks comments, string contents and char literals; Text variant blanks comments only. CRLF is normalised to LF first. Top-level test classes are found on the Code variant; partials merge by name; a class inside a raw string is not counted. A class asserts a wall-clock limit when blanked code compares `Elapsed` / `TotalSeconds` / `TotalMilliseconds` with `<` or `<=`, or creates a `CancellationTokenSource` with `TimeSpan.FromSeconds(` / `TimeSpan.FromMilliseconds(` and fails with `did not finish within`. The attribute reader accepts only exactly `Collection("Isolated timing")` or `Xunit.Collection("Isolated timing")` with one literal string. Report shape: a header line without class names, then one offender line sorted by file and class.

## Test ids

Born green (`Kind=BornGreen`, `Criterion=417-1`, no Pending):

| id | method |
|---|---|
| Z1 | `Z1_no_class_asserts_a_wall_clock_limit` |
| O1 | `O1_elapsed_comparison_without_the_collection_is_named` |
| O2 | `O2_cancellation_token_limit_without_the_collection_is_named` |
| O3 above | `O3_accepted_collection_spellings_and_placements_are_not_named` (`above`) |
| O3 same-line | same (`same-line`) |
| O3 comma-list | same (`comma-list`) |
| O3 after-a-comment | same (`after-a-comment`) |
| O3 xunit-qualified | same (`xunit-qualified`) |
| B comment | `B_near_misses_are_named_only_when_the_collection_is_wrong` (`comment`) |
| B string | same (`string`) |
| B timeout-ms-argument | same (`timeout-ms-argument`) |
| B wait-for-exit-limit | same (`wait-for-exit-limit`) |
| B wrong-case-collection | same (`wrong-case-collection`) — named |
| B bench-processes | same (`bench-processes`) — named |
| B collection-attribute | same (`collection-attribute`) — named |
| B constant | same (`constant`) — named |
| B commented-out | same (`commented-out`) — named |
| I strings/raw | `I_string_literal_forms_and_a_class_inside_a_raw_string_are_not_false_hits` |
| I several classes | `I_several_classes_in_one_file_are_judged_separately` |
| I partial | `I_a_partial_class_is_merged_across_files` |
| I bin/obj | `I_bin_and_obj_copies_are_skipped` |
| I own file | `I_the_guard_file_is_not_named` |
| L2 | `L2_live_tree_detected_set_contains_BenchmarkHarnessAcceptance` |

Pending until the green (`Criterion=417-2`):

| id | method |
|---|---|
| L1 | `L1_live_tree_has_no_wall_clock_class_outside_isolated_timing` — `[Trait("Status","Pending")]` with comment `Pending until the green (card 417)` |

Every test carries `[Trait("Slice","417")]` and Given/When/Then comments. 23 born-green tests (Z1, O1, O2, five O3 rows, nine B rows, five I tests, L2). L1 is the one Pending live-tree test.

## Commands and output

Build first:

```
dotnet build tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --nologo
```

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Fixtures (`Slice=417&Status!=Pending`)

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build --nologo --filter "Slice=417&Status!=Pending"
```

```
Passed!  - Failed:     0, Passed:    23, Skipped:     0, Total:    23, Duration: 192 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

### L1 Pending (`Slice=417&Status=Pending`)

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build --nologo --filter "Slice=417&Status=Pending"
```

Exactly 1 failed, an `Assert.True` failure whose message names `BenchmarkHarnessAcceptance` and no other class:

```
Failed Avowry.Acceptance.Tests.WallClockTestsRunIsolated.L1_live_tree_has_no_wall_clock_class_outside_isolated_timing [55 ms]
  Error Message:
   a test class that asserts a wall-clock limit must carry Collection("Isolated timing"):
  tests/Avowry.Acceptance.Tests/BenchmarkHarnessAcceptance.cs: BenchmarkHarnessAcceptance

Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 55 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

One offender line. Header holds no class name.

### Other guards

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build --nologo --filter "FullyQualifiedName~GeneratedApiSourceScan|FullyQualifiedName~NoBlockingProcessWaits"
```

```
Passed!  - Failed:     0, Passed:    34, Skipped:     0, Total:    34, Duration: 821 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

```
python -m unittest tests.test_bench_launcher_scan_264
```

```
......
----------------------------------------------------------------------
Ran 6 tests in 0.061s

OK
```

## Mutation checks (slice 3.2; uncommitted, restored after each)

Each mutation was applied in the worktree, `dotnet test ... --filter "Slice=417&Status!=Pending"` was run, then the original file was restored. The restored file was re-run: 23 passed.

### 1. A start-matcher that never matches

`ElapsedCompare` and `CtsFromSpan` replaced with patterns that never match.

Failed 12, passed 11. Rows that fail: O1, O2, B (wrong-case-collection, bench-processes, collection-attribute, constant, commented-out), I (strings/raw, several classes, partial, bin/obj), L2. Rows that still pass: Z1, O3 (all five), B (comment, string, timeout-ms-argument, wait-for-exit-limit), I own file.

### 2. A blanker that does not blank strings

String contents left in the Code variant; comments still blanked.

Failed 2, passed 21. Rows that fail: B string; I strings/raw (Phantom and Strings named beside Clocked). B comment still passes.

### 3. A case-insensitive collection name

`RegexOptions.IgnoreCase` added to the Isolated timing attribute reader.

Failed 1, passed 22. Row that fails: B wrong-case-collection (`Isolated Timing` was accepted).

### 4. The Text variant used where the Code variant belongs

Class finder, `[Fact]`/`[Theory]` detection and the limit matchers read the Text variant (comments blanked, strings kept).

Failed 2, passed 21. Rows that fail: B string; I strings/raw (Phantom and Strings named). Same shape as mutation 2.

## Full run alone

`dotnet --version` before: `10.0.404`

```
dotnet test Avowry.sln --filter "Status!=Pending"
```

```
Passed!  - Failed:     0, Passed:   344, Skipped:     0, Total:   344, Duration: 2 s - Avowry.Compiler.Tests.dll (net8.0)

Passed!  - Failed:     0, Passed:   478, Skipped:     0, Total:   478, Duration: 9 m 28 s - Avowry.Acceptance.Tests.dll (net8.0)
```

Exit 0. `dotnet --version` after: `10.0.404` (unchanged).

478 = the previous 455 acceptance tests plus the 23 born-green Slice 417 fixtures. L1 is Pending and was filtered out.

## Could not run

Nothing asked for in the brief failed to run.

## Working tree

Uncommitted, not staged: `tests/Avowry.Acceptance.Tests/WallClockTestsRunIsolated.cs` and this record. Facilitator commits.
