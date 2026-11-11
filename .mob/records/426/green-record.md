# Card 426 green record (GREEN writing seat, Cursor, linked worktree)

Changes left uncommitted for the facilitator. Worktree HEAD `aef63b9` (`Card 426: red reproduced in a clean clone, D255 audit, green prompt`). Line endings of both changed files stayed LF (`file` reports `ASCII text`, no CRLF). Neither `tools/Avowry.Bench/Program.cs` nor `tools/Avowry.BenchCs/Program.cs` was touched (`git diff --stat` on both: empty). No new kind of tracked file. Test output was filtered to summary and assertion lines, so no folder path was printed or copied here.

## What changed

`tools/Avowry.Bench/PathScrub.cs` (the slice's design, section 2):

- A. The folder loop: `(?:(?<dir>Segment)(?<!\s)(?(esc)(?!\\u[0-9A-Fa-f]{4}))Separator)*`. A folder name may not end in whitespace, so `x.cs in `, `x.cs to ` and `b.cs (line 3) and ` are no longer folders.
- B. The check before a drive: `(?:(?<![A-Za-z0-9_])|(?<=(?<esc>\\u[0-9A-Fa-f]{4})))`. When `esc` is captured, a single `\uXXXX` after a folder is not read as a separator (the guard in A), so `\\x.cs\u0027` ends at `x.cs`. The `esc` capture inside the lookbehind works as designed (risk 1 of the slice). The fallback was not needed.
- C. Drive home mark: `(?<home>(?i:users|documents and settings))`.
- D. UNC root: `(?<![\w\\:])(?:\\\\)?\\\\(?=[^\\/\s])(?:[^\\/\s]+Separator(?<home>(?i:users))Separator)?`. It now accepts four leading backslashes and an optional host followed by `Users`, which sets the home mark.
- E. Unix roots: `(?:file://(?:localhost)?)?(?:(?<home>/home/|/Users/|(?:/mnt)?/[a-z]/(?i:users)/)|/root/|/tmp/|/var/tmp/|/var/folders/|/private/tmp/|/private/var/folders/)`.
- F. Header comment: dropped the limits this card fixes (prose between two rooted paths, and the missing /mnt/c/Users, /c/Users, /var/tmp, /private/tmp and `Documents and Settings` roots). Kept `\\?\`, the account folder alone swallowing the words after it, and a TMPDIR outside the listed roots. Added the two new limits: prose followed by a relative path (`copy /tmp/x.cs to src/y.cs` becomes `copy y.cs`, checked below), and a path after a `\uXXXX` escape ending before a folder named `u` plus four hex digits.

`tests/Avowry.Acceptance.Tests/PathScrubForms.cs`: removed `[Trait("Status", "Pending")]` and the comment `// Pending until the green that changes tools/Avowry.Bench/PathScrub.cs` from `Id_426_1_the_words_around_a_path_stay` and `Id_426_2_no_account_folder_survives_in_the_missed_forms` (the only two Pending ids in the red record). Nothing else in the tests changed. `BenchRowsHoldNoFolderPath.cs` had no Pending trait and is unchanged.

`git diff --stat`: `PathScrubForms.cs | 4 ----`, `PathScrub.cs | 20 +++++++++++---------`, 2 files changed, 11 insertions(+), 13 deletions(-).

## Commands and output

### `dotnet build Avowry.sln -c Debug`

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.68
```

### `Slice=426`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=426"
```

`Passed!  - Failed:     0, Passed:    22, Skipped:     0, Total:    22, Duration: 24 s - Avowry.Acceptance.Tests.dll (net8.0)`

That is the same 22 tests as the red (14 failed there). All 14 Pending rows now pass, along with the 8 characterisation tests.

### `Criterion=426-1`, `Criterion=426-2`, `Criterion=426-3`, `FullyQualifiedName~PathScrubForms`

Same command form with `--no-build -c Debug`:

```
== Criterion=426-1
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 9 ms - Avowry.Acceptance.Tests.dll (net8.0)
== Criterion=426-2
Passed!  - Failed:     0, Passed:    12, Skipped:     0, Total:    12, Duration: 34 ms - Avowry.Acceptance.Tests.dll (net8.0)
== Criterion=426-3
Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 23 s - Avowry.Acceptance.Tests.dll (net8.0)
== FullyQualifiedName~PathScrubForms
Passed!  - Failed:     0, Passed:    53, Skipped:     0, Total:    53, Duration: 36 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

PathScrubForms covers 418-U1, U2 (including the run-twice check over the new rows), U4 and U5, plus the 426-1 and 426-2 rows. That includes the URL rows `https://host/path` and `https://example.com/home/x`.

### `dotnet test Avowry.sln --filter "Status!=Pending"` (alone)

Run with no test host running (`tasklist` for `testhost.exe`: 0). Six idle `dotnet.exe` processes were present before and after the run. They are consistent with MSBuild or build-server nodes left by the earlier build, but I did not inspect them. Started 14:30:56 UTC and ended 14:42:28 UTC; exit code 0.

```
Passed!  - Failed:     0, Passed:   344, Skipped:     0, Total:   344, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:   501, Skipped:     0, Total:   501, Duration: 11 m 25 s - Avowry.Acceptance.Tests.dll (net8.0)
```

Acceptance: 501 = the red's 487 plus the 14 rows that were Pending.

### `python scripts/home_paths.py --root .`

```
0 files
```

Exit code 0.

### Header-comment check (not required)

I loaded the built `tools/Avowry.Bench/bin/Debug/net8.0/Avowry.Bench.dll` in `pwsh` and called `PathScrub.Apply`:

```
[copy /tmp/x.cs to src/y.cs] -> [copy y.cs]
[https://host/path] -> [https://host/path]
[https://example.com/home/x] -> [https://example.com/home/x]
```

(The same call from Windows PowerShell 5 failed because it cannot load a net8.0 assembly. That failure is not a product finding.)

## Rows not turned green

None. Every Slice 426 row passes with the slice's design, and I found no flaw in the tests.

## What was not run

The label-deletion proofs of criterion 3 were not re-run: the green does not touch either `Program.cs`, and the red record holds them. The python unittest wrappers (`tests.test_home_paths_370`, `tests.test_account_name_376`, `tests.test_no_personal_names`) were not run; the brief named only the `home_paths.py` scan, and the full `dotnet test` run does not run them. I did not run a getpass-based account-name count over the diff; that is the facilitator's D255 audit step.
