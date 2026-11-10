# Card 368 green record (GREEN writing seat, Cursor, linked worktree)

Changes left uncommitted for the facilitator, on top of the red commit 30a4364. Two writers, one at a time. No other production file, no history file and no per-model file was touched. Each edited file kept its existing line endings (all six are CRLF in this worktree, before and after; checked with a CR count against the line count). Test changes are only the removal of `[Trait("Status", "Pending")]` and its "Pending until green N" comment.

## Green 1, the Phrase writer (`tools/Avowry.Bench/Program.cs`)

Section 2 edits only: `string.Empty` at the three former `CreateReplyHead` call sites (empty reply, completed row, exception path); `CreateReplyHead` deleted. `Regex` is still used elsewhere in the file, so the using stays. Diff: 3 insertions, 9 deletions.

Pending removed (7 tests):

- `BenchmarkHarnessAcceptance`: the four 368-1 masking tests and the 368-2 cold-run test `run_records_cold_profile_metadata_and_uses_an_empty_workdir_at_launch`.
- `NewBenchRowsCarryNoReply`: 368-1 `A_completed_run_saves_its_scores_and_no_reply_text` and 368-2a `Every_ending_of_the_Phrase_bench_saves_no_reply_text`.

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Criterion=368-1"
Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 5 s
```

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build --filter "FullyQualifiedName~Every_ending_of_the_Phrase_bench_saves_no_reply_text|FullyQualifiedName~run_records_cold_profile_metadata_and_uses_an_empty_workdir_at_launch"
Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2, Duration: 4 s
```

To check that green 1 fixes only the Phrase writer, I ran the whole 368-2 filter after green 1. The four C# tests still failed on C# reply text, the same reasons as in the red record:

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build --filter "Criterion=368-2"
  Failed ...PrivateHoldoutResults.private_run_leaves_committed_history_and_publishes_numbers_only   Found:  "REPLYMARKER-113-ZEBRA"
  Failed ...CSharpBaselineAcceptance.reply_head_is_scrubbed_before_it_is_cut   Actual:   "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"···
  Failed ...CSharpBaselineAcceptance.reply_head_scrubs_tilde_paths_with_either_separator_before_cutting   Actual:   "[path] [path] [path] FILE: Greeter.cs\nnam"···
  Failed ...NewBenchRowsCarryNoReply.Every_ending_of_the_C_sharp_baseline_saves_no_reply_text
      Actual:   "[path] MARKER-368-2B-PASS this is prose\n["···
      Actual:   "[path] MARKER-368-2B-FAIL this is prose\nn"···
      Actual:   "MARKER-368-2B-ERR\nnot csharp source"
Failed!  - Failed:     4, Passed:     2, Skipped:     0, Total:     6, Duration: 30 s
```

## Green 2, the C# baseline writer (`tools/Avowry.BenchCs/Program.cs`)

Section 2 edits only: `CreateResult` passes `string.Empty`; `ReplyHead` deleted; the `reply` parameter dropped from `CreateResult` and from its four callers (empty reply, passed, failed, exception). The local `reply` in `RunTask` stays because `ExtractCSharp` uses it. `Regex` is still used, so the using stays. Diff: 6 insertions, 12 deletions.

Pending removed (5 tests):

- `NewBenchRowsCarryNoReply`: 368-2b `Every_ending_of_the_C_sharp_baseline_saves_no_reply_text` and 368-3 `Pushed_rows_stay_byte_identical_and_still_read_when_a_new_row_is_appended`.
- `CSharpBaselineAcceptance`: 368-2 `reply_head_is_scrubbed_before_it_is_cut` and `reply_head_scrubs_tilde_paths_with_either_separator_before_cutting`.
- `PrivateHoldoutResults`: 368-2 `private_run_leaves_committed_history_and_publishes_numbers_only`.

No Pending trait remains on any test with `[Trait("Slice", "368")]`. This was checked with a search of the three lines after each Slice 368 trait, which found no match.

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Criterion=368-1"
Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 4 s

dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Criterion=368-2"
  Failed Avowry.Acceptance.Tests.CSharpBaselineAcceptance.reply_head_scrubs_tilde_paths_with_either_separator_before_cutting [8 s]
Failed!  - Failed:     1, Passed:     5, Skipped:     0, Total:     6, Duration: 30 s

dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Criterion=368-3"
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 7 s
```

## The one remaining failure: an environment-dependent assertion in the red test, not reply text

`reply_head_scrubs_tilde_paths_with_either_separator_before_cutting` now gets past `Assert.Equal("", HeadOf(...))`, so reply_head is empty. It fails on the next line, `Assert.DoesNotContain("AppData", stored, StringComparison.OrdinalIgnoreCase)` (CSharpBaselineAcceptance.cs line 477):

```
Assert.DoesNotContain() Failure: Sub-string found
                                   ↓ (pos 181)
String: ···"["C:\\\\Users\\\\<user>\\\\AppData\\\\Local\\\\Temp\\"···
Found:  "AppData"
```

(The account segment is masked here as `<user>`.) The match is not the reply's `~\AppData\...` text. It is the start of the `diagnostics` array: a compiler message that holds the bench's scratch path under the Windows temp directory, which is under AppData. The cause is that the reply puts `// FILE: Greeter.cs` in the middle of its first line, so `ExtractCSharp` sees only the `GreeterTests.cs` marker and the build fails. BenchCs then saves the compiler line, and that line holds the scratch path. The red run never reached line 477 because line 476 failed first.

Confirmation run: I changed no code and pointed TEMP and TMP at a git-ignored folder under `tests/Avowry.Acceptance.Tests/bin/`, whose path has no AppData in it. The same test then passes:

```
TEMP=<git-ignored bin folder> TMP=<same> dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build --filter "FullyQualifiedName~reply_head_scrubs_tilde_paths_with_either_separator_before_cutting"
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 7 s
```

I did not change the test, because the brief forbids test logic beyond the trait. Following the brief, its Pending trait is removed, so the gate fails on any Windows machine with the default temp directory. Possible fix for the facilitator or red seat: assert on the reply's own text, for example `@"~\AppData"`, `"private-answer"` and `"~/private"`, instead of the bare word `AppData`. The other choice is to put Pending back on this test until that is done.

Side finding for the retro (out of scope, not changed): a C# baseline row whose build fails saves the scratch path in `diagnostics`. On Windows that path includes the local account folder. This is the same family as architect risks 4 and 5 (diagnostics and error fields).

## Gate

```
dotnet test Avowry.sln --filter "Status!=Pending"
Avowry.Compiler.Tests:   Passed!  - Failed:     0, Passed:   344, Skipped:     0, Total:   344, Duration: 3 s
Avowry.Acceptance.Tests: Failed!  - Failed:     1, Passed:   401, Skipped:     0, Total:   402, Duration: 9 m 42 s
  Failed Avowry.Acceptance.Tests.CSharpBaselineAcceptance.reply_head_scrubs_tilde_paths_with_either_separator_before_cutting
```

402 is the red gate's 390 plus the twelve 368 tests that are no longer Pending. The only failure is the AppData assertion above. The two unrelated failures listed in the red record (PrePublicationOwnerItems and NoPhCodesInTheLiveTree) did not appear.

## Housekeeping

The confirmation run left scratch folders under `tests/Avowry.Acceptance.Tests/bin/t368`. That folder is ignored by git (`.gitignore:1:bin/`). It is not empty, so it was left in place; no recursive delete was used.
