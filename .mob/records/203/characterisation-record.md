# Card 203 characterisation (D204), QA (red) seat

Declared CHARACTERISATION, not a red. These tests must pass on the pre-change tree (no production change exists in this worktree). Base: `7cdc42e` of branch `wt-203-red`; the full hash is in the red record's pre-change line.

## Declared up front

| Test | Why it passes today |
|---|---|
| corpus fixture `type-lookalikes` (N1-N5 merged: types `Avow`, `AvowryRuntime`, `AvowryBuild`, `Program`, `AVOWS`; `runSpecs: true`, one spec, one endpoint) | None of the five names equals a reserved name by exact ordinal match, so the generated project builds and its spec passes. |
| corpus fixture `avow-fail-no-holes` (N6; `runSpecs: true`, a spec on each of two avows, one with `fails with`) | A bare `fail` method is `Fail()`, a legal overload of the emitted `Fail(string)`. |
| `TasksDemoGeneratedCsharp.Tasks_demo_generated_csharp_matches_the_golden_file` | Pins the emitted bytes (review item 10): the green must keep it green. |
| the 23 existing corpus fixtures and `The_corpus_has_a_header_comment_and_an_expected_file_per_fixture` | Unchanged behaviour. |
| `tests.test_rename_196` `test_corpus_text_unset` with the count raised 23 to 25 | Counts tracked corpus fixtures; passes once the two fixtures are committed. |

## Near misses build for real

Both fixtures were built with the CLI and then the generated `*.Specs.csproj` was built and run by the corpus theory (`runSpecs: true` makes it assert Total > 0 and Failed == 0). N1 to N5 (merged) and N6 therefore genuinely compile today; none had to move to a reject id.

## Commands and pass lines

`dotnet build src/Avowry.Cli`: `0 Error(s)`.

`dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~EdgeCaseFixtureCorpus|FullyQualifiedName~TasksDemoGeneratedCsharp"` (with the two fixtures present):

```
Passed!  - Failed:     0, Passed:    27, Skipped:     0, Total:    27, Duration: 38 s - Avowry.Acceptance.Tests.dll (net8.0)
```

(25 corpus theory rows, the header test, the golden test.)

`dotnet test tests/Avowry.Acceptance.Tests --nologo --no-build --filter "FullyQualifiedName~TasksDemoGeneratedCsharp"`:

```
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 374 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

## Mutation (uncommitted, then restored)

Changed `"line":19` to `"line":20` in `type-lookalikes.expected.json`, ran the corpus filter, then changed it back (the file is untracked at that point and was verified by reading it back).

```
Assert.Equal() Failure: Collections differ
Expected: [MapRecord { Kind = avow, Name = ok {n: Int}, Endpoint = Ping, Line = 20 }, ...]
Actual:   [MapRecord { Kind = avow, Name = ok {n: Int}, Endpoint = Ping, Line = 19 }, ...]
Failed!  - Failed:     1, Passed:    25, Skipped:     0, Total:    26, Duration: 29 s - Avowry.Acceptance.Tests.dll (net8.0)
```

After the restore: `Passed!  - Failed:     0, Passed:    26, Skipped:     0, Total:    26` for `FullyQualifiedName~EdgeCaseFixtureCorpus`.

## Python baseline on the pre-change tree

`python -m unittest tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_extension_map` (with the work stashed, tree at the base): `Ran 56 tests`, `FAILED (failures=8)`: five in rename_196 (`test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`), two in rename_197 (`test_lists_equal_baseline` x2), one in live_docs_266 (T11). `test_corpus_text_unset` passes at 23.
