# Card 418 green record (GREEN writing seat, Cursor, linked worktree)

Greens 1 and 2 of section 4 of the slice, production code only. Every change is left UNCOMMITTED for the facilitator. No test body, other production file, file under bench/, pushed history file, scripts/check-bench-history-clean.py or stored-result reader was changed. No bench tool was run against the tracked bench/ suites or results (`git status` after the full suite shows only the six files below). Every edited file keeps CRLF, and the new PathScrub.cs is CRLF like the tools' other sources.

## Changes

- New `tools/Avowry.Bench/PathScrub.cs`: `public static class PathScrub { public static string Apply(string? text) }`, global namespace, one regular expression, no Path.GetFileName, with `using System.Text.RegularExpressions;`. Roots (each behind a word-boundary lookbehind): drive `X:\` or `X:/` with an optional `file:///` prefix and an optional `Users` segment recorded as the home marker; UNC `\\`; tilde `~\` and `~/`; `/home/` and `/Users/` (home markers), `/root/`, `/tmp/`, `/var/folders/`, `/private/var/folders/`, with an optional `file://` prefix. Separators are `[\\/]+`, so doubled backslashes are covered. A non-final segment allows spaces, parentheses and `~`; the final segment stops at whitespace, quotes, `(`, `:`, `,`, `;` and brackets. The `<word>` placeholder may appear in any segment. Replacement: an empty final segment gives `<path>`; a home marker with no directory segment (an account folder alone) gives `<path>`; otherwise the final segment (the short name). The slice's known limits are noted in a header comment. Git Bash `/c/Users/` and `/mnt/c/Users/` are left unscrubbed, as the slice says.
- `tools/Avowry.Bench/Program.cs` (green 1): the model-exit message added to diagnostics (line 310); the `error` of the unavailable row (line 356); `PersistResult` scrubs every Diagnostics entry, Error and Label before serialising; `FailedExecuted` scrubs before `Cap(..., FeedbackCap)`; `RunDotnet` scrubs stdout plus stderr (so `CompactFailureSummary` gets scrubbed text before its cap); `ReadFailures` scrubs each message before `FirstLine` (so before the cap at 895). The retry prompt (`AddDiagnostics(taskPrompt, score.Diagnostics)`) is built from the scrubbed score.
- `tools/Avowry.BenchCs/Avowry.BenchCs.csproj` (green 2): `<ItemGroup><Compile Include="..\Avowry.Bench\PathScrub.cs" Link="PathScrub.cs" /></ItemGroup>`.
- `tools/Avowry.BenchCs/Program.cs` (green 2): line 242 `var output = PathScrub.Apply((await stdout) + (await stderr));` (before `CompactFailureSummary`'s cap and the `tests_did_not_run:` text); `ReadTestResult` scrubs each message before `FirstLine` and the 600 cap; `CreateResult` scrubs Diagnostics and Error; `Persist` scrubs every Diagnostics entry, Error and Label before serialising. `diagnostics[^1]` at line 178 holds the scrubbed score text, so the retry prompt and the row match.
- Pending removed (the trait line and its comment only): green 1 from 418-U1, both 418-U2 tests, 418-2a, 418-2b, 418-2c and 418-3b; green 2 from 418-1a, 418-1b, 418-1d, 418-2d, 418-2e, 418-3a and 418-U4 (U4 checks the C# project link, so it belongs to green 2, as its comment said). `rg -n "Pending"` over both 418 test files finds nothing, and no other test file carries `"Slice", "418"`.

## Commands and output

### Green 1

```
dotnet build Avowry.sln -c Debug
```
Build succeeded. 0 Warning(s), 0 Error(s).

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~PathScrubForms"
```
`Failed: 1, Passed: 28, Skipped: 0, Total: 29`. The one failure was 418-U4 (still Pending at that point; green 2's project link was not yet added): `Assert.Contains() Failure: Sub-string not found` / Not found: `Include="..\\Avowry.Bench\\PathScrub.cs"`.

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~Id_418_2a|FullyQualifiedName~Id_418_2b|FullyQualifiedName~Id_418_2c|FullyQualifiedName~Id_418_3b"
```
`Passed!  - Failed: 0, Passed: 4, Skipped: 0, Total: 4, Duration: 30 s`.

### Green 2

```
dotnet build Avowry.sln -c Debug
```
0 Warning(s), 0 Error(s).

Each Criterion filter of section 3, with `--no-build -c Debug` on tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj:

- `--filter "Criterion=418-1"`: `Passed!  - Failed: 0, Passed: 4, Skipped: 0, Total: 4, Duration: 15 s` (418-1a, 1b, 1c, 1d)
- `--filter "Criterion=418-2"`: `Passed!  - Failed: 0, Passed: 5, Skipped: 0, Total: 5, Duration: 19 s` (418-2a to 2e)
- `--filter "Criterion=418-3"`: `Passed!  - Failed: 0, Passed: 2, Skipped: 0, Total: 2, Duration: 16 s` (418-3a, 3b)
- `--filter "Criterion=418-U"`: `Passed!  - Failed: 0, Passed: 29, Skipped: 0, Total: 29, Duration: 37 ms` (U1 rows, U2 rows, idempotency, U4)
- `--filter "Slice=418"`: `Passed!  - Failed: 0, Passed: 40, Skipped: 0, Total: 40, Duration: 50 s`

### Full suite (alone, nothing else running)

```
dotnet test Avowry.sln --filter "Status!=Pending"
```
```
Passed!  - Failed:     0, Passed:   344, Skipped:     0, Total:   344, Duration: 2 s - Avowry.Compiler.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:   450, Skipped:     0, Total:   450, Duration: 9 m 30 s - Avowry.Acceptance.Tests.dll (net8.0)
```
Exit code 0. Acceptance went from 411 (the red record's gate) to 450: the 39 un-pended 418 tests. The pushed-row hash pins of NewBenchRowsCarryNoReply are part of that run and passed.

### After the full suite

The comment in PathScrub.cs that names an account-folder example was changed to the `<user>` placeholder (comment only). Then:

```
python -m unittest tests.test_home_paths_370 tests.test_account_name_376 tests.test_no_personal_names
```
`Ran 86 tests in 18.842s` / `OK`. For this run PathScrub.cs was marked intent-to-add (`git add -N`) so git-based guards would see it; that mark was removed afterwards (`git reset -q -- tools/Avowry.Bench/PathScrub.cs`), so the file is untracked again.

An account scan of the four changed production files by a python one-liner that reads the name through getpass printed `account name hits: 0`.

```
dotnet build Avowry.sln -c Debug
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~PathScrubForms|FullyQualifiedName~Id_418_2b"
```
0 Error(s); `Passed!  - Failed: 0, Passed: 30, Skipped: 0, Total: 30`. The full suite was not re-run after that comment-only edit.

## Notes for the facilitator

- No test assertion looked wrong, and the scrub satisfies every test as written.
- Scratch left in place: `$TEMP/418-green-full.txt` (the full-suite output). Folders under the temp folder made by the tests themselves were not touched.
- Not run: the Linux and macOS paths of the tests (Windows only here), and a re-run of the full suite after the comment-only edit.
