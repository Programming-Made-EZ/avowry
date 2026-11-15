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
