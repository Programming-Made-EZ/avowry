# Card 435 red record (RED writing seat, Grok, linked worktree)

Changes left uncommitted for the facilitator. Worktree HEAD `bb950a0`. Production files were not left changed: the seven hand-applied mutants of `tools/Avowry.Bench/PathScrub.cs` were restored; `git diff --stat -- tools` was empty at hand-off. Line endings of the three test files stayed LF. No bench tool was run against the tracked `bench/` suites or results. Plants use only the `<user>` placeholder and stand-ins built from parts (`"aa" + "bb"`, `"aa" + "\u00F6" + "bb"`, `"aa" + @"\u00f6" + "bb"`, `"aa(" + "bb)"`, `"aa (" + "bb)"`, `"aa " + "bb.cc"`, `"aa " + "\u00F6" + "cc"`). Observed folders, if any, are written here as `<home>` or `<temp>`. Tracebacks are not quoted (they print an absolute worktree path).

Characterisation is declared up front (ruling 7 and D222): plain accented account folders, the criterion-1 rows that already pass, the three escape-lookbehind mutant rows, the two-quote text, the empty string, and the base-copy fact. They carry no Pending trait. The 21 non-ASCII rows that fail today, the seven glued/unlisted/UNC rows that fail today, the five escape rows, and the generator fact are Pending.

## What the red adds

`tests/Avowry.Acceptance.Tests/PathScrubForms.cs` (183 insertions, 0 deletions; no existing test changed):

- data set `GluedAndUncRoots` (7 failing criterion-1 rows, then 12 characterisation rows)
- data set `NonAsciiAccountFolders` (49 rows with a declared fails-today flag: 21 fail, 28 pass)
- data set `EscapesAfterAPath` (5 failing rows, then the two-quote text and the empty string)
- `Id_435_1_glued_unlisted_and_unc_roots` — Pending until the green that changes `tools/Avowry.Bench/PathScrub.cs`; Take(7)
- `Id_435_1_rows_that_pass_today` — characterisation, no Pending; Skip(7)
- `Id_435_2_non_ascii_account_folders` — Pending until the same green; the 21 fails-today rows
- `Id_435_2_rows_that_pass_today` — characterisation, no Pending; the 28 rows that pass today
- `Id_435_3_escapes_after_a_path` — Pending until the same green; Take(5)
- `Id_435_3_rows_that_pass_today` — characterisation, no Pending; Skip(5)

`tests/Avowry.Acceptance.Tests/PathScrubGenerated.cs` (new):

- `Id_435_3_base_copy_still_reproduces_three_known_leaks` — characterisation, no Pending
- `Id_435_3_generated_inputs_leak_nothing_and_settle_in_one_pass` — Pending until the same green; SplitMix64 seed 435, 100,000 inputs, `PinnedExclusionCount = 0`

`tests/Avowry.Acceptance.Tests/Support/PathScrubBefore435.cs` (new): `git show 7f8680c:tools/Avowry.Bench/PathScrub.cs` with only `class PathScrub` renamed `PathScrubBefore435` (global namespace). Diff against that blob is one line:

```
-public static class PathScrub
+public static class PathScrubBefore435
```

Every new test has Given/When/Then comments. No Stopwatch, no time-limited cancellation token, no new kind of tracked file.

## D222 list: the Ready draft's rows first

### Pending ids

- `Id_435_1_glued_unlisted_and_unc_roots` (criterion 1 rows 1 to 7, both account-folder forms)
- `Id_435_2_non_ascii_account_folders` (criterion 2, 21 fails-today rows)
- `Id_435_3_escapes_after_a_path` (criterion 3 rows 1 to 5, both forms where `<user>` appears)
- `Id_435_3_generated_inputs_leak_nothing_and_settle_in_one_pass`

### Characterisation ids

- `Id_435_1_rows_that_pass_today` (criterion 1 rows 8 to 19: nine passing card rows plus three mutant rows)
- `Id_435_2_rows_that_pass_today` (plain accented folders, every folder-plus-file row except the quoted escaped one, and `see C:\Users\<user> bb.`)
- `Id_435_3_rows_that_pass_today` (the two-quote text and the empty string)
- `Id_435_3_base_copy_still_reproduces_three_known_leaks`

Criterion 1 (one theory row per table row; both the `<user>` form and the plain-letter form are asserted inside `AssertMissedRoot`):

| row | construct | kind | test |
|---|---|---|---|
| 1 | `gcc /tmp/a.c -I /usr/include -I/home/<user>/include` | NEW-STATE (435-1) | `Id_435_1_glued_unlisted_and_unc_roots` |
| 2 | `in /tmp/x.cs at /opt/data/home/<user>/x.cs` | NEW-STATE (435-1) | `Id_435_1_glued_unlisted_and_unc_roots` |
| 3 | `Restoring /tmp/x.csproj for /cygdrive/c/Users/<user>/x.csproj` | NEW-STATE (435-1) | `Id_435_1_glued_unlisted_and_unc_roots` |
| 4 | `/cygdrive/d/Users/<user>` | NEW-STATE (435-1) | `Id_435_1_glued_unlisted_and_unc_roots` |
| 5 | `\\host\c$\Users\<user>` | NEW-STATE (435-1) | `Id_435_1_glued_unlisted_and_unc_roots` |
| 6 | `\\wsl$\Ubuntu\home\<user>` | NEW-STATE (435-1) | `Id_435_1_glued_unlisted_and_unc_roots` |
| 7 | `\\\\host\\c$\\Users\\<user>` | NEW-STATE (435-1) | `Id_435_1_glued_unlisted_and_unc_roots` |
| 8 | `/Users/<user>/x.cs` | CHARACTERISATION (435-1) | `Id_435_1_rows_that_pass_today` |
| 9 | `/Users/<user>` | CHARACTERISATION (435-1) | `Id_435_1_rows_that_pass_today` |
| 10 | `/mnt/d/Users/<user>/x.cs` | CHARACTERISATION (435-1) | `Id_435_1_rows_that_pass_today` |
| 11 | `/d/Users/<user>` | CHARACTERISATION (435-1) | `Id_435_1_rows_that_pass_today` |
| 12 | `c:\documents and settings\<user>` | CHARACTERISATION (435-1) | `Id_435_1_rows_that_pass_today` |
| 13 | `\\host\share\x.cs` | CHARACTERISATION (435-1) | `Id_435_1_rows_that_pass_today` |
| 14 | `https://example.com/` + `Users/x` | CHARACTERISATION (435-1) | `Id_435_1_rows_that_pass_today` |
| 15 | `https://example.com/` + `home/x` | CHARACTERISATION (435-1) | `Id_435_1_rows_that_pass_today` |
| 16 | `/opt/data/x.cs` | CHARACTERISATION (435-1) | `Id_435_1_rows_that_pass_today` |
| 17 | `id u0041C:\a\x.cs` | CHARACTERISATION (435-1, mutant 4) | `Id_435_1_rows_that_pass_today` |
| 18 | `id u0041\\host\share\x.cs` | CHARACTERISATION (435-1, mutant 5) | `Id_435_1_rows_that_pass_today` |
| 19 | `id u0041/tmp/a/x.cs` | CHARACTERISATION (435-1, mutant 6) | `Id_435_1_rows_that_pass_today` |

Criterion 2 (folders built from parts; each of four prefixes, alone and followed by `\x.cs`; helper asserts exact output, no `aa`/`bb`/`cc`, stable second pass):

| construct | kind | test |
|---|---|---|
| `"aa" + @"\u00f6" + "bb"` alone, all four prefixes | NEW-STATE (435-2) | `Id_435_2_non_ascii_account_folders` |
| `"aa(" + "bb)"` alone, all four prefixes | NEW-STATE (435-2) | `Id_435_2_non_ascii_account_folders` |
| `"aa (" + "bb)"` alone, all four prefixes | NEW-STATE (435-2) | `Id_435_2_non_ascii_account_folders` |
| `"aa " + "bb.cc"` alone, all four prefixes | NEW-STATE (435-2) | `Id_435_2_non_ascii_account_folders` |
| `"aa " + "\u00F6" + "cc"` alone, all four prefixes | NEW-STATE (435-2) | `Id_435_2_non_ascii_account_folders` |
| quoted escaped folder plus `\x.cs` | NEW-STATE (435-2) | `Id_435_2_non_ascii_account_folders` |
| `"aa" + "\u00F6" + "bb"` alone and plus `\x.cs`, all four prefixes (8 rows) | CHARACTERISATION (435-2, ruling 7) | `Id_435_2_rows_that_pass_today` |
| every other folder plus `\x.cs` except the quoted escaped one (19 rows) | CHARACTERISATION (435-2) | `Id_435_2_rows_that_pass_today` |
| `see C:\Users\<user> bb.` | CHARACTERISATION (435-2) | `Id_435_2_rows_that_pass_today` |

That is 21 Pending rows and 28 characterisation rows.

Criterion 3:

| row | construct | kind | test |
|---|---|---|---|
| 1 | `C:\\a\\x.cs\nnext` | NEW-STATE (435-3) | `Id_435_3_escapes_after_a_path` |
| 2 | `/tmp/a/x.cs\tnext` | NEW-STATE (435-3) | `Id_435_3_escapes_after_a_path` |
| 3 | `see C:\\a\\x.cs\u0022` | NEW-STATE (435-3) | `Id_435_3_escapes_after_a_path` |
| 4 | `\"C:\\a\\x.cs\"` | NEW-STATE (435-3) | `Id_435_3_escapes_after_a_path` |
| 5 | `C:\\a\\x.cs\n/home/<user>/y.cs` | NEW-STATE (435-3) | `Id_435_3_escapes_after_a_path` |
| 6 | `""` (two-quote text) | CHARACTERISATION (435-3) | `Id_435_3_rows_that_pass_today` |
| 7 | empty string | CHARACTERISATION (435-3) | `Id_435_3_rows_that_pass_today` |
| copy | `-I`, `/opt/data/home`, `\\host\c$` leaks | CHARACTERISATION (435-3) | `Id_435_3_base_copy_still_reproduces_three_known_leaks` |
| gen | 100,000 SplitMix64 inputs, seed 435 | NEW-STATE (435-3) | `Id_435_3_generated_inputs_leak_nothing_and_settle_in_one_pass` |

Ruling 6: no `match \\d+ here` test row. Today that text becomes `match d+ here`.

## Commands and output

### `dotnet build Avowry.sln -c Debug`

First build failed with xUnit2013 on `Assert.Equal(PinnedExclusionCount, Exclusions.Length)` (empty-collection size). The pin check was rewritten as `Assert.True(Exclusions.Length == PinnedExclusionCount, ...)`. Second build: Build succeeded. 0 Warning(s), 0 Error(s). Time Elapsed 00:00:04.72.

### `Slice=435`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435" --logger "console;verbosity=detailed"
```

Total tests: 77. Passed: 43. Failed: 34. The 34 failures are the Pending theory rows (7 of 435-1, 21 of 435-2, 5 of 435-3) and the generator fact. The 43 passes are the 12+28+2 characterisation theory rows and the base-copy fact. Each Pending row failed by `Assert.Equal` on the stored text (or `Assert.True` on the generator counts), not a build error or a typo.

Pending failure excerpts (Error Message only):

- 435-1 `gcc ... -I/home/<user>/include`: Expected `"gcc a.c -I /usr/include -Iinclude"` Actual `"gcc a.c -I /usr/include -I/home/<user>/include"`
- 435-1 `in /tmp/x.cs at /opt/data/home/<user>/x.cs`: Expected `"in x.cs at x.cs"` Actual `"in x.cs at /opt/data/home/<user>/x.cs"`
- 435-1 Restoring cygdrive: Expected `"Restoring x.csproj for x.csproj"` Actual keeps `/cygdrive/c/Users/<user>/x.csproj`
- 435-1 `/cygdrive/d/Users/<user>`: Expected `"<path>"` Actual `"/cygdrive/d/Users/<user>"`
- 435-1 `\\host\c$\Users\<user>`: Expected `"<path>"` Actual `"<user>"`
- 435-1 `\\wsl$\Ubuntu\home\<user>`: Expected `"<path>"` Actual `"<user>"`
- 435-1 doubled UNC: Expected `"<path>"` Actual `"<user>"`
- 435-2 escaped folder alone: Expected `"<path>"` Actual `"u00f6bb"`
- 435-2 quoted escaped folder: Expected `"\u0027<path>\u0027"` Actual `"\u0027<path>\u00f6bb\u0027"`
- 435-2 quoted escaped folder plus file: Expected `"\u0027x.cs\u0027"` Actual `"\u0027<path>\u00f6bb\x.cs\u0027"`
- 435-2 `aa(bb)` alone: Expected `"<path>"` Actual `"<path>(bb)"`
- 435-2 `aa (bb)` alone: Expected `"<path>"` Actual `"<path> (bb)"`
- 435-2 `aa bb.cc` alone: Expected `"<path>"` Actual `"<path>.cc"`
- 435-2 `aa` + space + ö + `cc` alone: Expected `"<path>"` Actual `"<path> "` then the ö-word
- 435-3 `C:\\a\\x.cs\nnext`: Expected `"x.cs\nnext"` Actual `"nnext"`
- 435-3 `/tmp/a/x.cs\tnext`: Expected `"x.cs\tnext"` Actual `"tnext"`
- 435-3 `see C:\\a\\x.cs\u0022`: Expected `"see x.cs\u0022"` Actual `"see u0022"`
- 435-3 `\"C:\\a\\x.cs\"`: Expected `"\"x.cs\""` Actual `"\"<path>"`
- 435-3 `C:\\a\\x.cs\n/home/<user>/y.cs`: Expected `"x.cs\ny.cs"` Actual `"y.cs"`
- 435-3 generator: `inputs=100000 leaks_base=31534 leaks_new=31534 regressions=0 second_pass=0 exclusions=0`

### `Criterion=435-1`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-1"
```

Failed! Failed: 7, Passed: 12, Skipped: 0, Total: 19, Duration: 49 ms. The 7 failures match the Pending excerpts above. The 12 characterisation rows passed.

### `Criterion=435-2`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-2"
```

Failed! Failed: 21, Passed: 28, Skipped: 0, Total: 49, Duration: 57 ms. The 21 failures match the Pending excerpts above. The 28 characterisation rows (plain accented folders, file rows except the quoted escaped one, and the sentence-ending dot) passed.

### `Criterion=435-3`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-3" --logger "console;verbosity=detailed"
```

Total tests: 9. Passed: 3. Failed: 6. The 6 failures are the five escape rows and the generator. The 3 passes are the two-quote text, the empty string, and the base-copy fact. Detailed logger printed:

```
inputs=100000 leaks_base=31534 leaks_new=31534 regressions=0 second_pass=0 exclusions=0
```

(The architect measured 31,704 leaking inputs on a throwaway copy of these shapes; this generator's pick order produced 31,534. Pin stays 0: every leak fails the assertion.)

### `FullyQualifiedName~PathScrubForms`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~PathScrubForms"
```

Failed! Failed: 33, Passed: 103, Skipped: 0, Total: 136, Duration: 94 ms. The 33 failures are the Pending 435-1, 435-2 and 435-3 theory rows (the generator lives in `PathScrubGenerated`). The 418-U rows, the 426 rows, and the 435 characterisation theory rows passed.

### `match \\d+ here` today (ruling 6, not a test row)

A throwaway console in `<temp>` compiled the current `PathScrub.cs` and printed:

```
single=[match \d+ here]
double=[match d+ here]
```

The doubled-backslash row does not fall out free. It is not claimed.

### Seven mutants (each applied to the base file, Slice characterisation 435-1 run, then restored)

Filter: `FullyQualifiedName~Id_435_1_rows_that_pass_today` (12 characterisation rows). Each mutant is named by its base text.

1. `/Users/` -> `/users/` (line 23). Failed: 2, Passed: 10. `/Users/<user>/x.cs` stayed `/Users/<user>/x.cs` (expected `x.cs`); `/Users/<user>` stayed `/Users/<user>` (expected `<path>`).
2. `/[a-z]/` -> `/c/` (line 23). Failed: 2, Passed: 10. `/mnt/d/Users/<user>/x.cs` stayed unchanged (expected `x.cs`); `/d/Users/<user>` stayed unchanged (expected `<path>`).
3. `(?i:users|documents and settings)` -> `(?i:users)|Documents and Settings` (line 19). Failed: 1, Passed: 11. `c:\documents and settings\<user>` became `<user>` (expected `<path>`).
4. line 18 `(?<=(?<esc>\\u[0-9A-Fa-f]{4}))` -> `(?<=(?<esc>u[0-9A-Fa-f]{4}))`. Failed: 1, Passed: 11. `id u0041C:\a\x.cs` became `id u0041x.cs`.
5. the same on line 20. Failed: 1, Passed: 11. `id u0041\\host\share\x.cs` became `id u0041x.cs`.
6. the same on line 22. Failed: 1, Passed: 11. `id u0041/tmp/a/x.cs` became `id u0041x.cs`.
7. line 20 `(?<home>(?i:users))` -> `(?<home>[^\\/\s]+)`. Failed: 1, Passed: 11. `\\host\share\x.cs` became `<path>` (expected `x.cs`).

After the last restore, `tools/Avowry.Bench/PathScrub.cs` matched the 7f8680c blob.

### `git diff --stat -- tools`

No output (empty). `PathScrub.cs` matches HEAD.

### `Status!=Pending` (alone)

```
dotnet test Avowry.sln --filter "Status!=Pending"
```

Passed. Compiler tests: Failed: 0, Passed: 344, Skipped: 0, Total: 344, Duration: 1 s. Acceptance tests: Failed: 0, Passed: 552, Skipped: 0, Total: 552, Duration: 11 m 47 s.

552 = the card-426 sign-off's 509 plus the 43 characterisation rows of this red (12+28+2+1). The 34 Pending tests are excluded.

### Python guards

```
python scripts/home_paths.py --root .
```

```
0 files
```

A scan of the three new/changed test files with `home_paths.find` (accounts from the environment) reported 0 hits.

## What was not run

Nothing required by section 3 was skipped. The python unittest wrappers `tests.test_home_paths_370`, `tests.test_account_name_376` and `tests.test_no_personal_names` were not run; the brief named the `home_paths.py` scan.
