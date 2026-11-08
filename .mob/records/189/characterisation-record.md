# Card 189 characterisation (D157 Criterion 3), QA (red) seat

Declared CHARACTERISATION, not a red. The tests pass on the pre-change tree (HEAD `b64e72a24587d240509a690e5c953d4d5bd24e40`, no card 189 production change yet). Reason: criterion 3 says a composite with a return type already exits 1 with PH325 and no Unhandled exception, the 174 corpus fixtures keep their pinned outcomes, and generated C# for orders.avowry keeps hash `19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765`.

## HEAD

`git rev-parse HEAD` before this commit: `b64e72a24587d240509a690e5c953d4d5bd24e40`

## What was written

`tests/Avowry.Acceptance.Tests/CompositeReturnTypeStaysPH325.cs`, method `A_composite_with_a_return_type_already_reports_PH325`, using `PhraseCli.RunAsync` in the style of `PhraseBuildOneFile.Compile_error_exits_one_and_prints_the_diagnostic`. Asserts exit 1, `PH325 (` in the output, and no `Unhandled exception`. No Pending trait. No `Process.Start(` in the file.

## New characterisation test

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~A_composite_with_a_return_type_already_reports_PH325"`

```
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 151 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

(Restored re-run after the mutation: Duration 138 ms, same Passed 1.)

CLI probe of the same source (`dotnet exec src/Avowry.Cli/bin/Debug/net8.0/Avowry.Cli.dll build ph325.avowry -o OUT`): exit 1, `PH325 (3:1) Composite avow 'count {n: Int}' cannot return a value.`, no Unhandled exception.

## Existing characterisation tests

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Orders_phrase_loads_web_and_keeps_the_output_recorded_before_the_slice"`

```
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 96 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~EdgeCaseFixtureCorpus.Every_corpus_fixture_matches_its_expected_outcome"`

```
Passed!  - Failed:     0, Passed:    14, Skipped:     0, Total:    14, Duration: 20 s - Avowry.Acceptance.Tests.dll (net8.0)
```

## Mutation (shown failing, then restored)

Changed the expected code in the test from `PH325 (` to `PH999 (`, ran the same filter once, then restored.

```
Failed Avowry.Acceptance.Tests.CompositeReturnTypeStaysPH325.A_composite_with_a_return_type_already_reports_PH325 [151 ms]
  Error Message:
   Assert.Contains() Failure: Sub-string not found
String:    "avowry: Compilation failed:\nPH325 (3:1) C"···
Not found: "PH999 ("
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 151 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

## Neighbouring tests (pre-change tree, before this file was committed)

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~EdgeCaseFixtureCorpus|FullyQualifiedName~CliKeepsDiagnosticShape|FullyQualifiedName~PhraseBuildOneFile|FullyQualifiedName~WebExtensionBoundary|FullyQualifiedName~GeneratedApiSourceScan"`

```
Passed!  - Failed:     0, Passed:    30, Skipped:     0, Total:    30, Duration: 22 s - Avowry.Acceptance.Tests.dll (net8.0)
```

Command: `python -m unittest tests.test_rename_196`

Already red on this pre-change tree (not this seat's): Ran 15, FAILED (failures=6). `test_eol_unchanged` (Compiler.Tests and Acceptance.Tests), `test_lists_equal` (284 vs 324; 317 vs 329), `test_five_modules` (ANSI colour in OK output), `test_new_dirs_tracked_old_gone` (counts 27/100 vs 33/109).

## Line count

`tests/Avowry.Acceptance.Tests/CompositeReturnTypeStaysPH325.cs`: 36 lines, LF only.
