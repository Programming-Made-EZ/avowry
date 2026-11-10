# Card 368 red record (RED writing seat, Grok, linked worktree)

Changes left uncommitted for the facilitator. No production file, history file or per-model file was touched. Line endings of the four test files are LF, matching what they already used. History files were read as bytes only.

## What the red adds

New file `tests/Avowry.Acceptance.Tests/NewBenchRowsCarryNoReply.cs` (class `NewBenchRowsCarryNoReply`):

- 368-1 `A_completed_run_saves_its_scores_and_no_reply_text` — Pending until green 1 (Phrase writer)
- 368-2a `Every_ending_of_the_Phrase_bench_saves_no_reply_text` — Pending until green 1 (Phrase writer)
- 368-2b `Every_ending_of_the_C_sharp_baseline_saves_no_reply_text` — Pending until green 2 (C# baseline writer)
- 368-3 `Pushed_rows_stay_byte_identical_and_still_read_when_a_new_row_is_appended` — Pending until green 2 (C# baseline writer)

A temp suite is outside Git tracking, so 368-1 reaches Persist through `--private-results` aimed at that suite's own `results/history.jsonl`. The assertions still read `results/history.jsonl` and `results/<model>.json`.

Flipped, Pending until the named green:

- 368-1 green 1: `reply_head_scrubs_a_user_path_before_cutting_to_200_characters`
- 368-1 green 1: `reply_head_masks_a_tilde_relative_path_inside_the_first_200_characters`
- 368-1 green 1: `reply_head_masks_a_secret_looking_token`
- 368-1 green 1: `reply_head_without_sensitive_content_is_only_cut_to_200_characters`
- 368-2 green 1: `run_records_cold_profile_metadata_and_uses_an_empty_workdir_at_launch`
- 368-2 green 2: `reply_head_is_scrubbed_before_it_is_cut`
- 368-2 green 2: `reply_head_scrubs_tilde_paths_with_either_separator_before_cutting`
- 368-2 green 2: `private_run_leaves_committed_history_and_publishes_numbers_only`

Flipped, no Pending (already empty both ways):

- `run_retries_an_empty_reply_once_and_records_it_as_unavailable` (HeadOf helper at the old line-716 assert)

Every new or flipped Pending test carries `[Trait("Slice", "368")]`, `[Trait("Criterion", "368-N")]`, `[Trait("Status", "Pending")]` and a comment naming the green. Every test has Given/When/Then comments.

## Commands and output

`dotnet build tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj` succeeded, 0 warnings, 0 errors.

### Criterion=368-1

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Criterion=368-1"
```

Total tests: 5, Failed: 5. Each failure is an assertion on stored reply text, not a build error.

- 368-1 `A_completed_run_saves_its_scores_and_no_reply_text`: `Assert.DoesNotContain() Failure: Sub-string found` at `reply_head":"MARKER-368-1\\nmodule "` / Found: `"MARKER-368-1"`
- 368-1 `reply_head_scrubs_a_user_path_before_cutting_to_200_characters`: `Assert.Equal() Failure: Strings differ` Expected: `""` Actual: `"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"···`
- 368-1 `reply_head_without_sensitive_content_is_only_cut_to_200_characters`: `Assert.Equal() Failure: Strings differ` Expected: `""` Actual: `"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"···`
- 368-1 `reply_head_masks_a_tilde_relative_path_inside_the_first_200_characters`: `Assert.Equal() Failure: Strings differ` Expected: `""` Actual: `"Inspect [path] before answering."`
- 368-1 `reply_head_masks_a_secret_looking_token`: `Assert.Equal() Failure: Strings differ` Expected: `""` Actual: `"Use api_key=[redacted] before answering."`

### Criterion=368-2

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Criterion=368-2"
```

Total tests: 6, Failed: 6. Duration 30.8 s.

- 368-2 `run_records_cold_profile_metadata_and_uses_an_empty_workdir_at_launch`: Expected: `""` Actual: `"[]\r\nmodule Greeting\r\n\r\navow to "say hello"···`
- 368-2 `private_run_leaves_committed_history_and_publishes_numbers_only`: `Assert.DoesNotContain() Failure: Sub-string found` at `reply_head":"REPLYMARKER-113-ZEBRA"` / Found: `"REPLYMARKER-113-ZEBRA"`
- 368-2a `Every_ending_of_the_Phrase_bench_saves_no_reply_text` (two of four endings; empty-reply and exception already write an empty head and did not fail):
  - pass: Expected: `""` Actual: `"MARKER-368-2A-PASS\nmodule Greeting\n\navow "···`
  - fail: Expected: `""` Actual: `"MARKER-368-2A-FAIL this is prose, not a p"···`
- 368-2 `reply_head_is_scrubbed_before_it_is_cut`: Expected: `""` Actual: `"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"···`
- 368-2 `reply_head_scrubs_tilde_paths_with_either_separator_before_cutting`: Expected: `""` Actual: `"[path] [path] [path] FILE: Greeter.cs\nnam"···`
- 368-2b `Every_ending_of_the_C_sharp_baseline_saves_no_reply_text` (three of four endings; empty-reply already writes an empty head and did not fail):
  - pass: Expected: `""` Actual: `"[path] MARKER-368-2B-PASS this is prose\n["···`
  - fail: Expected: `""` Actual: `"[path] MARKER-368-2B-FAIL this is prose\nn"···`
  - exception (earlier attempt's reply): Expected: `""` Actual: `"MARKER-368-2B-ERR\nnot csharp source"`

### Criterion=368-3

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Criterion=368-3"
```

Total tests: 1, Failed: 1. The prefix hashes of the first 336 and 120 newlines matched the recorded SHA-256 values, and the stored-result readers agreed on the pushed lines. The appended rows still carry reply text:

```
phrase appended reply_head: module Greeting

avow to "say hello"
  native csharp
    return;
csharp appended reply_head: [path] FILE: Greeter.cs
namespace Greeting; public static class Greeter { public static string SayHello() => "hello"; }
[path] FILE: GreeterTests.cs
using Greeting;
public sealed class GreeterTests {
```

### Empty-reply flip (no Pending)

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "FullyQualifiedName~run_retries_an_empty_reply_once_and_records_it_as_unavailable"
```

Passed: 1. `HeadOf` reads the existing empty `reply_head`.

### Red gate

```
dotnet test Avowry.sln --filter "Status!=Pending"
```

Compiler.Tests: Passed 344, Failed 0.

Acceptance.Tests: Failed 2, Passed 389, Total 391, Duration 9 m 14 s.

The two failures are not from this red (those files were not edited):

- `PrePublicationOwnerItems.Each_of_the_nine_owner_items_shows_its_answer`: owner-items A1 missing `"Avowry confirmed (D125); registration status pending the owner's trademark search."`; A3 missing `"commissioned by the owner; pending result"` and `"Publishing the name waits on the result."`
- `NoPhCodesInTheLiveTree.Every_history_allow_list_entry_still_holds_a_PH_code`: `H6 MOB.md: no old-prefix code left (generated from the records; it follows them and is regenerated, not edited)`

All twelve Pending 368 tests were excluded by the filter. The empty-reply HeadOf flip is among the 389 that passed.

## Per-id list (D222)

| Id | Test | Status now | Un-pends on |
| --- | --- | --- | --- |
| 368-1 | `A_completed_run_saves_its_scores_and_no_reply_text` | fail, reply_head holds MARKER-368-1 | green 1 |
| 368-1 | `reply_head_scrubs_a_user_path_before_cutting_to_200_characters` | fail, reply_head is the 190 x's | green 1 |
| 368-1 | `reply_head_masks_a_tilde_relative_path_inside_the_first_200_characters` | fail, reply_head is `Inspect [path] before answering.` | green 1 |
| 368-1 | `reply_head_masks_a_secret_looking_token` | fail, reply_head is `Use api_key=[redacted] before answering.` | green 1 |
| 368-1 | `reply_head_without_sensitive_content_is_only_cut_to_200_characters` | fail, reply_head is 200 a's | green 1 |
| 368-2a | `Every_ending_of_the_Phrase_bench_saves_no_reply_text` | fail on pass and fail endings; empty and exception already empty | green 1 |
| 368-2 | `run_records_cold_profile_metadata_and_uses_an_empty_workdir_at_launch` | fail, reply_head starts `[]` then the phrase | green 1 |
| 368-2b | `Every_ending_of_the_C_sharp_baseline_saves_no_reply_text` | fail on pass, fail, and exception (earlier attempt); empty already empty | green 2 |
| 368-2 | `reply_head_is_scrubbed_before_it_is_cut` | fail, reply_head is the 190 x's | green 2 |
| 368-2 | `reply_head_scrubs_tilde_paths_with_either_separator_before_cutting` | fail, reply_head starts `[path] [path] [path] FILE:` | green 2 |
| 368-2 | `private_run_leaves_committed_history_and_publishes_numbers_only` | fail, private file holds REPLYMARKER-113-ZEBRA in reply_head | green 2 |
| 368-3 | `Pushed_rows_stay_byte_identical_and_still_read_when_a_new_row_is_appended` | fail, both appended rows have a non-empty reply_head | green 2 |
| — | `run_retries_an_empty_reply_once_and_records_it_as_unavailable` | pass (HeadOf, no Pending) | — |
