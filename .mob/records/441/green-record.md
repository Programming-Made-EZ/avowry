# Card 441 green record (GREEN seat, dev)

## git diff --stat
```
 tests/Avowry.Acceptance.Tests/PathScan441.cs |   6 --
 tools/Avowry.Bench/PathScrub.cs              | 149 +++++++++++++++++++++------
 2 files changed, 118 insertions(+), 37 deletions(-)
```

## How the port was done
- Replaced `tools/Avowry.Bench/PathScrub.cs` with the architect reference scanner structure and rules.
- Removed probe-only mutant machinery: no `Mutant` field, no `M(string)` helper, and no `M("...")` terms.
- Folded each mutant branch to non-mutant behavior while keeping the scanner constants, comments, and linear-scan structure.
- Kept and updated the file header comment for card 441 and pinned limits.
- Kept the literal-name example as a split string form in tests (`"-v /home/" + "u1a:/data"` remains split where used).
- Removed the two Pending marker lines from each Pending test method in `tests/Avowry.Acceptance.Tests/PathScan441.cs`.

## Command summary lines
- `dotnet build Avowry.sln -c Debug`
  - `Build succeeded.`
  - `0 Warning(s)`
  - `0 Error(s)`
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=441"`
  - `Passed!  - Failed:     0, Passed:   111, Skipped:     0, Total:   111`
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439|Slice=435"`
  - `Passed!  - Failed:     0, Passed:   341, Skipped:     0, Total:   341`
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439" --logger "console;verbosity=detailed"`
  - `Total tests: 229`
  - `Passed: 229`

## Sweep lines (from Slice=439 detailed run)
- `sweep=B extended lists seed 439 inputs=100000 leaks_base=45965 leaks_new=4559 regressions=0 second_pass=0 exclusions=0`
- `sweep=C review shapes seed 4391 inputs=100000 leaks_base=50216 leaks_new=2679 regressions=0 second_pass=0 exclusions=0`

## PathScrub.cs line count
- `tools/Avowry.Bench/PathScrub.cs`: 508 lines

## CLI model line
- No CLI-reported model line appeared in the command outputs run for this slice.

## Fix round 1 green

### git diff --stat
```
 tests/Avowry.Acceptance.Tests/PathScan441.cs |  2 --
 tools/Avowry.Bench/PathScrub.cs              | 25 ++++++++++++++++++-------
 2 files changed, 18 insertions(+), 9 deletions(-)
```

### How the port was done
- Ported the fixed reference scanner from `reference-PathScan.cs.txt` into `tools/Avowry.Bench/PathScrub.cs` by keeping the scanner rules, constants, comments, and structure.
- Removed all probe-only mutant machinery (`Mutant`, `M(string)`, and all `M("...")` terms) and kept non-mutant behavior each time.
- Updated the file header comment for this card's fix round and pinned limits.
- Removed the two `// Pending until the card 441 fix green ...` lines from pending tests in `tests/Avowry.Acceptance.Tests/PathScan441.cs`.

### Command summary lines
- `dotnet build Avowry.sln -c Debug`
  - `Build succeeded.`
  - `0 Warning(s)`
  - `0 Error(s)`
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=441"`
  - `Passed!  - Failed:     0, Passed:   140, Skipped:     0, Total:   140`
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439|Slice=435"`
  - `Passed!  - Failed:     0, Passed:   341, Skipped:     0, Total:   341`
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439" --logger "console;verbosity=detailed"`
  - `Test Run Successful.`
  - `Total tests: 229`
  - `Passed: 229`

### Sweep lines (Slice=439 detailed run)
- `sweep=B extended lists seed 439 inputs=100000 leaks_base=45965 leaks_new=4559 regressions=0 second_pass=0 exclusions=0`
- `sweep=C review shapes seed 4391 inputs=100000 leaks_base=50216 leaks_new=2679 regressions=0 second_pass=0 exclusions=0`

### PathScrub.cs line count
- `tools/Avowry.Bench/PathScrub.cs`: `519`

### CLI model line
- No CLI-reported model line appeared in the command outputs for this run.
