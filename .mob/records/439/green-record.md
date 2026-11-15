## Card 439 green record

### git diff --stat
```text
 tests/Avowry.Acceptance.Tests/PathScan439.cs       |   6 -
 .../PathScan439Generated.cs                        |   4 -
 tools/Avowry.Bench/PathScrub.cs                    | 946 ++++++++++++++++++++-
 3 files changed, 915 insertions(+), 41 deletions(-)
```

### What changed
- Replaced the regex-based implementation in `tools/Avowry.Bench/PathScrub.cs` with a linear scanner per slice 439 (candidate-root scan, escape handling, extent/tail/account rules, and replacement behavior).
- Removed pending markers from every pending 439 test in:
  - `tests/Avowry.Acceptance.Tests/PathScan439.cs`
  - `tests/Avowry.Acceptance.Tests/PathScan439Generated.cs`
  - removed both `[Trait("Status", "Pending")]` and the matching pending comment lines.

### Required commands and summary lines
1. `dotnet build Avowry.sln -c Debug`  
   Build succeeded. 0 Warning(s), 0 Error(s).

2. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439"`  
   Passed! Failed: 0, Passed: 163, Total: 163.

3. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435|Slice=426|Slice=418"`  
   Passed! Failed: 0, Passed: 186, Total: 186.

4. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439" --logger "console;verbosity=detailed"`  
   Test Run Successful. Total tests: 163, Passed: 163.

### Sweep lines (from detailed Slice=439 run)
- `sweep=B extended lists seed 439 inputs=100000 leaks_base=45965 leaks_new=4310 regressions=0 second_pass=0 exclusions=0`
- `sweep=C review shapes seed 4391 inputs=100000 leaks_base=50216 leaks_new=2563 regressions=0 second_pass=0 exclusions=0`

### CLI model line
- No explicit CLI-reported model line appeared in the command outputs.

### Fix round green

#### git diff --stat
```text
 tests/Avowry.Acceptance.Tests/PathScan439.cs |    4 -
 tools/Avowry.Bench/PathScrub.cs              | 1144 +++++++-------------------
 2 files changed, 309 insertions(+), 839 deletions(-)
```

#### Port details (reference -> PathScrub)
- Replaced `tools/Avowry.Bench/PathScrub.cs` with the architect's fixed scanner structure and logic from `reference-PathScan.cs.txt`, adapted to class `PathScrub` and method `Apply(string? text)`.
- Removed probe-only mutant machinery: deleted `Mutant`, deleted `M(string)`, and removed every `M("...")` switch by keeping non-mutant behavior:
  - `M("x") ? A : B` -> `B`
  - `if (M("x")) ...` -> removed
  - terms such as `&& !M("x")` / `|| M("x")` -> removed
- Kept the scanner rules/constants/comments/flow from the reference and updated the file header to call out the linear scanner and limits `2.5n`, `3.4d`, `3.8a-d`, `9b`.
- Removed pending markers from pending fix-round tests in `tests/Avowry.Acceptance.Tests/PathScan439.cs` (removed `[Trait("Status", "Pending")]` and the matching pending comment line in each pending method).

#### Required commands and summary lines
1. `dotnet build Avowry.sln -c Debug`  
   `Build succeeded.`

2. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439"`  
   `Passed!  - Failed:     0, Passed:   180, Skipped:     0, Total:   180`

3. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435|Slice=426|Slice=418"`  
   `Passed!  - Failed:     0, Passed:   186, Skipped:     0, Total:   186`

4. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439" --logger "console;verbosity=detailed"`  
   `Test Run Successful.`
   `Total tests: 180`
   `Passed: 180`

#### Sweep lines
- `sweep=B extended lists seed 439 inputs=100000 leaks_base=45965 leaks_new=4559 regressions=0 second_pass=0 exclusions=0`
- `sweep=C review shapes seed 4391 inputs=100000 leaks_base=50216 leaks_new=2679 regressions=0 second_pass=0 exclusions=0`

#### PathScrub.cs line count
- `412`

#### CLI model line
- No explicit CLI-reported model line appeared in the command outputs.

### Fix round 2 green

#### git diff --stat
```text
 tests/Avowry.Acceptance.Tests/PathScan439.cs |  4 ----
 tools/Avowry.Bench/PathScrub.cs              | 25 ++++++++++++++++---------
 2 files changed, 16 insertions(+), 13 deletions(-)
```

#### Port details (reference -> PathScrub)
- Ported the fixed reference scanner shape into `tools/Avowry.Bench/PathScrub.cs` while keeping `PathScrub.Apply(string? text)` and the linear-scanner header limits (`2.5n`, `3.4d`, `3.8a-d`, `9b`).
- Removed probe-only mutant machinery by keeping non-mutant behavior only:
  - no `Mutant` field;
  - no `M(string)` helper;
  - each mutant branch reduced to its non-mutant path (`M(...) ? A : B` -> `B`, `if (M(...))` removed, `&& !M(...)` / `|| M(...)` terms removed).
- Kept the reference scanner constants/comments/flow and restored reference behaviors including drive adjacency gating via prior root form, one-letter colon-list handling in slash chains, and the reference account-marker walk rule.
- Removed the two pending lines from each pending fix-2 test in `tests/Avowry.Acceptance.Tests/PathScan439.cs` (`[Trait("Status", "Pending")]` and the matching pending comment).

#### Required commands and summary lines
1. `dotnet build Avowry.sln -c Debug`  
   `Build succeeded.`

2. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439"`  
   `Passed!  - Failed:     0, Passed:   209, Skipped:     0, Total:   209, Duration: 6 s`

3. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435|Slice=426|Slice=418"`  
   `Passed!  - Failed:     0, Passed:   186, Skipped:     0, Total:   186, Duration: 2 m 17 s`

4. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439" --logger "console;verbosity=detailed"`  
   `Test Run Successful.`
   `Total tests: 209`
   `Passed: 209`
   `Total time: 8.4593 Seconds`

#### Sweep lines
- `sweep=B extended lists seed 439 inputs=100000 leaks_base=45965 leaks_new=4559 regressions=0 second_pass=0 exclusions=0`
- `sweep=C review shapes seed 4391 inputs=100000 leaks_base=50216 leaks_new=2679 regressions=0 second_pass=0 exclusions=0`

#### PathScrub.cs line count
- `419`

#### CLI model line
- No explicit CLI-reported model line appeared in the command outputs.
