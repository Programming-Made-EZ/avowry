# Card 435 rework green record (GREEN writing seat, Cursor default model, linked worktree, branch `card-435-rework`, HEAD `e49e4a9`)

Changes left uncommitted.

`git diff --stat`:

    tests/Avowry.Acceptance.Tests/PathScrubForms.cs |  6 ------
    tools/Avowry.Bench/PathScrub.cs                 | 21 ++++++++++++---------
    2 files changed, 12 insertions(+), 15 deletions(-)

What changed:
- Applied section 1 text in `tools/Avowry.Bench/PathScrub.cs` exactly:
  - `EscBefore`, then `Glued` (in that order), then `FinalSegment` updated to the provided constants text.
  - Pattern part D (line 28 UNC branch) replaced with the provided text.
  - Pattern part G (line 35 tail) replaced with the provided text.
  - `Apply` condition changed to include `file == "~"`.
  - Header comments updated per section 1 wording changes.
- Removed only the three pending markers from:
  - `Id_435_1_rework_rows`
  - `Id_435_2_rework_rows`
  - `Id_435_3_rework_rows`
  in `tests/Avowry.Acceptance.Tests/PathScrubForms.cs`:
  - `[Trait("Status", "Pending")]`
  - `// Pending until the rework green ...`

Commands run and summary lines:
1. `dotnet build Avowry.sln -c Debug`
   - Build succeeded.
   - 0 Warning(s)
   - 0 Error(s)

2. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435"`
   - Passed! - Failed: 0, Passed: 112, Skipped: 0, Total: 112

3. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-1"`
   - Passed! - Failed: 0, Passed: 30, Skipped: 0, Total: 30

4. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-2"`
   - Passed! - Failed: 0, Passed: 57, Skipped: 0, Total: 57

5. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-3" --logger "console;verbosity=detailed"`
   - Test Run Successful.
   - Total tests: 25
   - Passed: 25
   - Generator line: `inputs=100000 leaks_base=31534 leaks_new=0 regressions=0 second_pass=0 exclusions=0`

6. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=426"`
   - Passed! - Failed: 0, Passed: 30, Skipped: 0, Total: 30

7. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=418"`
   - Passed! - Failed: 0, Passed: 45, Skipped: 0, Total: 45

Model shown by CLI in this run:
- No explicit model identifier appeared in the command outputs.
