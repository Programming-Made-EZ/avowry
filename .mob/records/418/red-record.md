# Card 418 red record (RED writing seat, Grok, linked worktree)

Changes left uncommitted for the facilitator. No production file, existing test, history file or per-model file was touched. Line endings of the two new test files are LF, matching `NewBenchRowsCarryNoReply.cs`. No bench tool was run against the tracked `bench/` suites or results. Plants use only the `<user>` placeholder. Observed folders are written here as `<home>` or `<temp>`.

## What the red adds

New file `tests/Avowry.Acceptance.Tests/BenchRowsHoldNoFolderPath.cs` (class `BenchRowsHoldNoFolderPath`, `[Collection("Bench processes")]` with a D247 comment):

- 418-1a `Id_418_1a_csharp_planted_build_failure_stores_short_file_names` — Pending until green 2 (the C# tool and its project link)
- 418-1b `Id_418_1b_csharp_tests_did_not_run_stores_short_file_names` — Pending until green 2
- 418-1c `Id_418_1c_build_failed_without_a_path_is_stored_unchanged` — characterisation, no Pending trait, passes on the current tree
- 418-1d `Id_418_1d_csharp_real_syntax_error_holds_no_folder` — Pending until green 2
- 418-2a `Id_418_2a_phrase_model_exit_then_pass_scrubs_diagnostics` — Pending until green 1 (the Phrase tool and the PathScrub.cs file)
- 418-2b `Id_418_2b_phrase_model_exit_on_every_attempt_scrubs_error` — Pending until green 1
- 418-2c `Id_418_2c_phrase_executed_real_build_holds_no_folder` — Pending until green 1
- 418-2d `Id_418_2d_csharp_start_failure_stores_the_missing_file_short_name` — Pending until green 2
- 418-2e `Id_418_2e_csharp_model_exit_scrubs_error` — Pending until green 2
- 418-3a `Id_418_3a_csharp_retry_prompt_carries_the_scrubbed_text` — Pending until green 2
- 418-3b `Id_418_3b_phrase_retry_prompt_carries_the_scrubbed_text` — Pending until green 1

New file `tests/Avowry.Acceptance.Tests/PathScrubForms.cs` (class `PathScrubForms`, loads `PathScrub` by `typeof(BenchmarkResults).Assembly.GetType("PathScrub")`):

- 418-U1 `Id_418_U1_each_rooted_form_becomes_the_short_name` — theory, Pending until green 1
- 418-U2 `Id_418_U2_must_not_touch_rows_stay_byte_identical` and `Id_418_U2_apply_is_idempotent_on_every_form` — Pending until green 1
- 418-U4 `Id_418_U4_csharp_tool_links_the_shared_file_and_defines_no_scrub_class` — Pending until green 2

Changed tests: none.

418-2d points `PHRASE_BENCHCS_DOTNET` at `<workspace>/Users/<user>/missing-dotnet` without creating that folder: on Windows a directory named `<user>` cannot be created (`CreateDirectory` throws "The filename, directory name, or volume label syntax is incorrect"). The missing-file path still carries the planted `Users\<user>` form.

Every new-state test carries `[Trait("Slice", "418")]`, `[Trait("Criterion", "418-N")]` (unit ids use `418-U`) and `[Trait("Status", "Pending")]` with a comment naming the green. 418-1c has Slice and Criterion 418-1 and no Pending. Every test has Given/When/Then comments.

## Observed start-failure message (net8.0)

A net8.0 `Process.Start` of a missing file with `UseShellExecute = false` (the C# tool's `StartProcess` shape) threw `System.ComponentModel.Win32Exception`. The observer replaced the profile folder first, so temp-under-profile reads as `<home>\AppData\Local\Temp`:

```
An error occurred trying to start process '<home>\AppData\Local\Temp\phrase-418-missing\missing-dotnet' with working directory '<home>\AppData\Local\Temp\'. The system cannot find the file specified.
```

The message names both the missing file and the working directory. 418-2d relies on that prefix: its stored `error` contains `An error occurred trying to start process` and `missing-dotnet`, and the residue check then fails because the real temp and profile folders are stored.

## Real rows on the current tree hold a real folder

- Phrase executed (418-2c): `results/418-2c.json diagnostics[0]` holds a drive path, `Users\`, `AppData`, `phrase-bench`, the profile folder and the temp folder.
- C# real syntax error (418-1d): `results/418-1d.json diagnostics[0]` holds the same forms, including the profile folder and the temp folder.
- C# start failure (418-2d): `results/418-2d.json error` holds the same, including the profile folder and the temp folder.

## Commands and output

First `dotnet build Avowry.sln -c Debug` failed on xUnit2031 (`Assert.Single` after `Where` in `AssertNoResidue`). After switching to the predicate overload of `Assert.Single`, the same command succeeded: 0 Warning(s), 0 Error(s).

### `FullyQualifiedName~PathScrubForms`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~PathScrubForms"
```

Failed: 29, Passed: 0, Total: 29, Duration: 311 ms.

- 418-U1 (every theory row): `PathScrub is missing from the Phrase tool assembly.`
- 418-U2 (every must-not-touch row and idempotency): `PathScrub is missing from the Phrase tool assembly.`
- 418-U4: `Assert.Contains() Failure: Sub-string not found` / Not found: `Include="..\Avowry.Bench\PathScrub.cs"`

### `FullyQualifiedName~Id_418_1c`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~Id_418_1c"
```

Passed: 1, Failed: 0, Duration: 2 s.

### `Criterion=418-1`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=418-1"
```

Failed: 3, Passed: 1, Total: 4, Duration: 15 s. 418-1c passed. The three Pending ids failed by assertion, not by a build error.

- 418-1a: `Assert.Equal() Failure: Strings differ` Expected: `"Candidate.cs(7,1): error CS1529: planted "···` Actual: `"C:\\Users\\<user>\\AppData\\Local\\Temp\\phrase"···`
- 418-1b: the same Equal failure on the plant after `tests_did_not_run:`
- 418-1d: residue on the stored row, including `decoded holds the profile folder`, `decoded holds the temp folder`, `holds a drive path`, `holds Users\`, `holds AppData`, `holds phrase-bench`

### Per-test: 418-2a, 418-2b, 418-2d, 418-2e, 418-3a

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~Id_418_2a|FullyQualifiedName~Id_418_2b|FullyQualifiedName~Id_418_2d|FullyQualifiedName~Id_418_2e|FullyQualifiedName~Id_418_3a"
```

The first 418-2d run failed on `Directory.CreateDirectory` for `Users\<user>` (illegal name on Windows). After dropping the create, 418-2d was re-run on its own (rebuild, then `--filter "FullyQualifiedName~Id_418_2d"`).

- 418-2a: `results/418-2a.json diagnostics[0] holds a drive path; ... holds the <user> placeholder; ... holds Users\; ... holds AppData; ... holds phrase-bench`
- 418-2b: `results/418-2b.json error holds a drive path; ... holds /home/; ... holds the <user> placeholder; ... holds Users\; ... holds AppData; ... holds phrase-bench`
- 418-2d (after the CreateDirectory fix): `results/418-2d.json error holds a drive path; ... holds the <user> placeholder; ... decoded holds the profile folder; ... decoded holds the temp folder`
- 418-2e: `results/418-2e.json error holds a drive path; ... holds the <user> placeholder; ... holds Users\; ... holds AppData; ... holds phrase-bench`
- 418-3a: `Assert.Equal() Failure: Strings differ` Expected: `"Candidate.cs(7,1): error CS1529: planted "···` Actual: `"C:\\Users\\<user>\\AppData\\Local\\Temp\\phrase"···`

### Per-test: 418-2c, 418-3b

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~Id_418_2c|FullyQualifiedName~Id_418_3b"
```

Failed: 2, Passed: 0, Duration: 27 s.

- 418-2c: `results/418-2c.json diagnostics[0] holds a drive path; ... holds Users\; ... decoded holds the profile folder; ... decoded holds the temp folder` (Contains `error CS0103` and `Avows.cs` had already passed)
- 418-3b: `prompt-2 Compiler diagnostics holds a drive path; ... decoded holds the profile folder; ... decoded holds the temp folder` (the `Compiler diagnostics:` header and `Avows.cs` had already passed)

### `Slice=418`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=418"
```

Failed: 39, Passed: 1, Total: 40, Duration: 49 s. The one pass is 418-1c.

### `FullyQualifiedName~BenchRowsHoldNoFolderPath`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~BenchRowsHoldNoFolderPath"
```

Failed: 10, Passed: 1, Total: 11, Duration: 50 s. The one pass is 418-1c. Failure excerpts match the per-id runs above.

### `Status!=Pending` (alone)

```
dotnet test Avowry.sln --filter "Status!=Pending"
```

Passed. Compiler tests: Failed: 0, Passed: 344, Skipped: 0, Total: 344, Duration: 2 s. Acceptance tests: Failed: 0, Passed: 411, Skipped: 0, Total: 411, Duration: 9 m 32 s.

### Python guards

```
python -m unittest tests.test_home_paths_370 tests.test_account_name_376 tests.test_no_personal_names
```

Ran 86 tests in 28.791s. OK.

## What was not run

Nothing required by section 3 was skipped. The first 418-2d attempt (CreateDirectory of `Users\<user>`) is not the red of that id; the residue failure after dropping the create is.
