# Card 202 characterisation (D157 Criteria 2 and 3), QA (red) seat

Declared CHARACTERISATION, not a red. Slice section 3 says nothing new is needed: the existing tests pass on the pre-change tree. Criterion 2: a leaf that declares the code it raises still builds; a declared native failure still has a source-map failure record at the `fails with` line. Criterion 3: existing corpus fixtures keep their pinned outcomes (the undeclared-native-failure fixture still says `builds`); generated C# for `orders.avowry` keeps hash `19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765`.

No production change. No new test file.

## HEAD

`git rev-parse HEAD` before this commit: `6fbc32366051f4ff883b31591b43cd2f7a75e93e`

Worktree `C:\Users\<user>\projects\wt-202-red`, branch `card-202-red`. CLI rebuilt first: `dotnet build src/Avowry.Cli` succeeded (0 Warning(s), 0 Error(s)).

## Neighbouring tests (pre-change tree)

Command:

```
dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~EdgeCaseFixtureCorpus|FullyQualifiedName~CliKeepsDiagnosticShape|FullyQualifiedName~WebExtensionBoundary|FullyQualifiedName~LeafPhraseFailureCompiles|FullyQualifiedName~SourceMapTraceability|FullyQualifiedName~UnsupportedCsharpConstructs|FullyQualifiedName~GeneratedApiSourceScan"
```

```
Passed!  - Failed:     0, Passed:    47, Skipped:     0, Total:    47, Duration: 23 s - Avowry.Acceptance.Tests.dll (net8.0)
```

Named tests in that run (all passed):

- `LeafPhraseFailureCompiles.Failing_leaf_phrase_surfaces_its_declared_code_and_its_spec_passes`
- `SourceMapTraceability.Failures_inherited_through_a_composite_phrase_still_get_a_failure_record`
- `SourceMapTraceability.A_failure_record_is_found_when_an_earlier_step_can_raise_the_same_name`
- `EdgeCaseFixtureCorpus.Every_corpus_fixture_matches_its_expected_outcome` (23 fixtures, including `undeclared-native-failure` whose `.expected.json` is still `{"outcome":"builds",...}` with one avow record at line 3)
- `WebExtensionBoundary.Orders_phrase_loads_web_and_keeps_the_output_recorded_before_the_slice`
- `CliKeepsDiagnosticShape.The_CLI_exits_1_with_the_diagnostic_line_and_0_when_clean` (ids M13-PH001, M08-PH203, clean)

Also in the same filter, and also passed: `EdgeCaseFixtureCorpus.The_corpus_has_a_header_comment_and_an_expected_file_per_fixture`; the rest of `SourceMapTraceability`; `UnsupportedCsharpConstructs.Unsupported_csharp_constructs_report_a_diagnostic`; six `GeneratedApiSourceScan` tests including `Guard_checked_set_of_process_start_callers`.

## Mutation (shown failing, then restored)

Temporary uncommitted edit of `tests/Avowry.Acceptance.Tests/WebExtensionBoundary.cs`: last character of `OrdersOutputHash` changed from `5` to `6`. Then restored with `git checkout -- tests/Avowry.Acceptance.Tests/WebExtensionBoundary.cs`.

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Orders_phrase_loads_web_and_keeps_the_output_recorded_before_the_slice"`

After the one-character edit:

```
Failed Avowry.Acceptance.Tests.WebExtensionBoundary.Orders_phrase_loads_web_and_keeps_the_output_recorded_before_the_slice [87 ms]
  Error Message:
   19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 87 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

After restore (`git status --short` empty):

```
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 98 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

## Line count

`.mob/records/202/characterisation-record.md` only. No production files, no corpus fixtures, no `docs/GRAMMAR.md`.
