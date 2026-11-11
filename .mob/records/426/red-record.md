# Card 426 red record (RED writing seat, Grok, linked worktree)

Changes left uncommitted for the facilitator. Worktree HEAD `dcd96f4` (`Card 426: architect slice and red prompt`). Production files were not left changed: the label-proof deletions of `tools/Avowry.Bench/Program.cs` line 817 and `tools/Avowry.BenchCs/Program.cs` line 850 were restored; `git diff --stat -- tools` was empty at hand-off. Line endings of both test files stayed LF. No bench tool was run against the tracked `bench/` suites or results. Plants use only the `<user>` placeholder and a plain-letter folder built from parts (`"aa" + "bb"`). Observed folders, if any, are written here as `<home>` or `<temp>`. Tracebacks are not quoted (they print an absolute worktree path).

## What the red adds

`tests/Avowry.Acceptance.Tests/PathScrubForms.cs`:

- data set `WordsAroundAPath` (7 criterion 1 rows; the regex row is `@"match \d+ here"`)
- data set `MissedRoots` (12 criterion 2 rows as verbatim strings with `<user>`); each theory also applies `text.Replace("<user>", plain)` at run time
- `Id_426_1_the_words_around_a_path_stay` — Pending until the green that changes `tools/Avowry.Bench/PathScrub.cs`; MemberData rows 1 to 3
- `Id_426_1_rows_that_pass_today` — characterisation, no Pending trait; MemberData rows 4 to 7
- `Id_426_2_no_account_folder_survives_in_the_missed_forms` — Pending until the same green; MemberData rows 1 to 11, both account-folder forms
- `Id_426_2_drive_slash_users_row_passes_today` — characterisation, no Pending trait; the `C:/Users/<user>/x.cs` row, both forms
- `Id_418_U2_apply_is_idempotent_on_every_form` — changed: the loop now covers `Forms`, `WordsAroundAPath`, and both forms of `MissedRoots`

`tests/Avowry.Acceptance.Tests/BenchRowsHoldNoFolderPath.cs`:

- `Id_426_3a_phrase_bench_label_holds_no_folder_path` — characterisation, Slice 426, Criterion 426-3, no Pending; Phrase bench, `PrintFileCommand(PassingPhrase)`, default scoring; two runs (`--label C:\Users\<user>\notes\run-426` and `--label run-426`)
- `Id_426_3b_csharp_baseline_label_holds_no_folder_path` — characterisation, Slice 426, Criterion 426-3, no Pending; C# baseline, `PrintFileCommand(PassingCSharp)` with `DotnetStandIn(WriteStandInDotnet(..., trxToCopy: PassingTrx, exitCode: 0))`; two runs with the same labels
- `Id_418_3b_phrase_retry_prompt_carries_the_scrubbed_text` — changed: asserts `passed`, `attempts == 2` and `AssertNoResidue`; added Slice 426 and Criterion 426-3 traits; no Pending

Every new and changed test has Given/When/Then comments. No Stopwatch, no time-limited cancellation token, no new kind of tracked file.

## D222 list: the Ready draft's rows first

Criterion 1 (one theory row per table row):

| row | construct | kind | test |
|---|---|---|---|
| 1 | `Could not find file /tmp/x.cs in /usr/lib/dotnet` | NEW-STATE (426-1) | `Id_426_1_the_words_around_a_path_stay` |
| 2 | `copy /tmp/a/x.cs to /tmp/b/y.cs` | NEW-STATE (426-1) | `Id_426_1_the_words_around_a_path_stay` |
| 3 | `see C:\a\b.cs (line 3) and /docs/x` | NEW-STATE (426-1) | `Id_426_1_the_words_around_a_path_stay` |
| 4 | `match \d+ here` | CHARACTERISATION (426-1) | `Id_426_1_rows_that_pass_today` |
| 5 | `see C:\a\x.cs.` | CHARACTERISATION (426-1) | `Id_426_1_rows_that_pass_today` |
| 6 | `C:\a\x.cs, then` | CHARACTERISATION (426-1) | `Id_426_1_rows_that_pass_today` |
| 7 | `/x then /tmp/a/y.cs` | CHARACTERISATION (426-1) | `Id_426_1_rows_that_pass_today` |

Criterion 2 (one theory row per table row; both the `<user>` form and the plain-letter form are asserted inside that method, so a leak in either form fails the same id):

| row | construct | kind | test |
|---|---|---|---|
| 1 | `/mnt/c/Users/<user>/x.cs` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 2 | `/mnt/c/Users/<user>` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 3 | `/c/Users/<user>/x.cs` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 4 | `/c/Users/<user>` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 5 | `/var/tmp/a/x.cs` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 6 | `/private/tmp/a/x.cs` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 7 | `file://localhost/home/<user>/x.cs` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 8 | `C:\Documents and Settings\<user>` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 9 | `\\host\Users\<user>` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 10 | `\u0027C:\\Users\\<user>\\x.cs\u0027` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 11 | `\\\\host\\Users\\<user>\\x.cs` | NEW-STATE (426-2) | `Id_426_2_no_account_folder_survives_in_the_missed_forms` |
| 12 | `C:/Users/<user>/x.cs` | CHARACTERISATION (426-2) | `Id_426_2_drive_slash_users_row_passes_today` |
| Then | `https://host/path` and `https://example.com/home/x` stay unchanged | CHARACTERISATION | existing `Id_418_U2_must_not_touch_rows_stay_byte_identical` (reason for staying those ids: already on the tree; this red does not duplicate them) |

Criterion 3 (one id per tool covers both label runs, as the slice asked; 418-3b is the retry):

| construct | kind | test |
|---|---|---|
| Phrase bench `--label C:\Users\<user>\notes\run-426` and `--label run-426` | CHARACTERISATION (426-3) | `Id_426_3a_phrase_bench_label_holds_no_folder_path` |
| C# baseline, same two labels | CHARACTERISATION (426-3) | `Id_426_3b_csharp_baseline_label_holds_no_folder_path` |
| 418-3b retry that then passes | CHARACTERISATION (426-3) | `Id_418_3b_phrase_retry_prompt_carries_the_scrubbed_text` |

### Pending ids

- `Id_426_1_the_words_around_a_path_stay` (criterion 1 rows 1 to 3)
- `Id_426_2_no_account_folder_survives_in_the_missed_forms` (criterion 2 rows 1 to 11, both forms)

### Characterisation ids

- `Id_426_1_rows_that_pass_today` (criterion 1 rows 4 to 7)
- `Id_426_2_drive_slash_users_row_passes_today` (criterion 2 `C:/Users` row, both forms)
- `Id_418_U2_apply_is_idempotent_on_every_form` (extended loop)
- `Id_426_3a_phrase_bench_label_holds_no_folder_path`
- `Id_426_3b_csharp_baseline_label_holds_no_folder_path`
- `Id_418_3b_phrase_retry_prompt_carries_the_scrubbed_text` (strengthened)

## Commands and output

### `dotnet build Avowry.sln -c Debug`

Build succeeded. 0 Warning(s), 0 Error(s). Time Elapsed 00:00:12.87.

### `Slice=426`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=426"
```

Failed: 14, Passed: 8, Skipped: 0, Total: 22, Duration: 24 s. The 14 failures are the Pending theory rows (3 of 426-1, 11 of 426-2). The 8 passes are the four 426-1 characterisation rows, the one 426-2 `C:/Users` row, 426-3a, 426-3b and 418-3b. Each Pending row failed by `Assert.Equal` on the scrubbed text (not a build error or a typo).

Pending failure excerpts (Error Message only):

- 426-1 `Could not find file /tmp/x.cs in /usr/lib/dotnet`: Expected `"Could not find file x.cs in /usr/lib/dotn"···` Actual `"Could not find file dotnet"`
- 426-1 `copy /tmp/a/x.cs to /tmp/b/y.cs`: Expected `"copy x.cs to y.cs"` Actual `"copy y.cs"`
- 426-1 `see C:\a\b.cs (line 3) and /docs/x`: Expected `"see b.cs (line 3) and /docs/x"` Actual `"see x"`
- 426-2 `/mnt/c/Users/<user>/x.cs`: Expected `"x.cs"` Actual `"/mnt/c/Users/<user>/x.cs"`
- 426-2 `/mnt/c/Users/<user>`: Expected `"<path>"` Actual `"/mnt/c/Users/<user>"`
- 426-2 `/c/Users/<user>/x.cs`: Expected `"x.cs"` Actual `"/c/Users/<user>/x.cs"`
- 426-2 `/c/Users/<user>`: Expected `"<path>"` Actual `"/c/Users/<user>"`
- 426-2 `/var/tmp/a/x.cs`: Expected `"x.cs"` Actual `"/var/tmp/a/x.cs"`
- 426-2 `/private/tmp/a/x.cs`: Expected `"x.cs"` Actual `"/private/tmp/a/x.cs"`
- 426-2 `file://localhost/home/<user>/x.cs`: Expected `"x.cs"` Actual `"file://localhost/home/<user>/x.cs"`
- 426-2 `C:\Documents and Settings\<user>`: Expected `"<path>"` Actual `"<user>"`
- 426-2 `\\host\Users\<user>`: Expected `"<path>"` Actual `"<user>"`
- 426-2 JSON-escaped drive: Expected `"\\u0027x.cs\\u0027"` Actual `"\\u0027C:\\\\Users\\\\<user>u0027"`
- 426-2 doubled UNC: Expected `"x.cs"` Actual `"\\\\\\\\host\\\\Users\\\\<user>x.cs"`

### `FullyQualifiedName~PathScrubForms`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~PathScrubForms"
```

Failed: 14, Passed: 39, Skipped: 0, Total: 53, Duration: 49 ms. The 14 failures match the Pending excerpts above. The 418-U rows, the extended run-twice check, and the 426 characterisation rows passed.

### `FullyQualifiedName~BenchRowsHoldNoFolderPath`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~BenchRowsHoldNoFolderPath"
```

Passed! Failed: 0, Passed: 13, Skipped: 0, Total: 13, Duration: 1 m 4 s. That is the eleven 418 ids plus 426-3a and 426-3b.

### Label proof (1): delete Phrase line 817

Deleted `Label = result.Label is null ? null : PathScrub.Apply(result.Label),` from `tools/Avowry.Bench/Program.cs` (the trailing comma on the Error line still compiled). `dotnet build Avowry.sln -c Debug`: Build succeeded. 0 Warning(s), 0 Error(s). Time Elapsed 00:00:03.41.

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~Id_426_3a"
```

Failed: 1, Passed: 0, Total: 1, Duration: 2 s. Error Message: `Assert.Equal() Failure: Strings differ` Expected `"run-426"` Actual `"C:\\Users\\<user>\\notes\\run-426"`. Stack: `AssertLabelIsRun426` then `Id_426_3a_phrase_bench_label_holds_no_folder_path` (the path-label run). Restored the line.

### Label proof (2): delete C# line 850

Deleted the same Label assignment from `tools/Avowry.BenchCs/Program.cs`. `dotnet build Avowry.sln -c Debug`: Build succeeded. 0 Warning(s), 0 Error(s). Time Elapsed 00:00:02.87.

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~Id_426_3b"
```

Failed: 1, Passed: 0, Total: 1, Duration: 5 s. Error Message: `Assert.Equal() Failure: Strings differ` Expected `"run-426"` Actual `"C:\\Users\\<user>\\notes\\run-426"`. Stack: `AssertLabelIsRun426` then `Id_426_3b_csharp_baseline_label_holds_no_folder_path`. Restored the line.

### `git diff --stat -- tools`

No output (empty). Both Program.cs files match HEAD.

### `Status!=Pending` (alone)

```
dotnet test Avowry.sln --filter "Status!=Pending"
```

Passed. Compiler tests: Failed: 0, Passed: 344, Skipped: 0, Total: 344, Duration: 2 s. Acceptance tests: Failed: 0, Passed: 487, Skipped: 0, Total: 487, Duration: 11 m 6 s.

### Python guards

```
python scripts/home_paths.py --root .
```

```
0 files
```

## What was not run

Nothing required by section 3 was skipped. The python unittest wrappers `tests.test_home_paths_370`, `tests.test_account_name_376` and `tests.test_no_personal_names` were not run; the brief named the `home_paths.py` scan.
