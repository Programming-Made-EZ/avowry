# Card 202 red (D157 Criterion 1), QA (red) seat

Pending new-state red for criterion 1, one test, committed with `Status=Pending` so the gate excludes it. Watched failing by assertion on the current tree (no production change in `src/`). The 7 failing ids fail; the 8 keep-building ids pass inside the same assertion.

## Pre-change commit hash

`git rev-parse HEAD` before this commit: `e95ee6548977a514a3b91a42ea716a0bbbcebba7` (the characterisation commit; production tree still matches `6fbc32366051f4ff883b31591b43cd2f7a75e93e`).

## Command

```
dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Undeclared_native_fail_reports_a_diagnostic"
```

## Failing test

`Avowry.Acceptance.Tests.UndeclaredNativeFail.Undeclared_native_fail_reports_a_diagnostic`

Attributes in order: `[Fact]`, `[Trait("Slice", "202")]`, `[Trait("Criterion", "202-1")]`, `[Trait("Status", "Pending")]`. Calls `PhraseCli.RunAsync`. No `Process.Start(` in the file.

## Expected failure

Assertion failure from `Assert.True(failures.Count == 0, ...)`, not a missing file and not a compile error. Exactly the 7 failing ids, none of the 8 keep ids.

## Aggregated failure text (full)

```
  Failed Avowry.Acceptance.Tests.UndeclaredNativeFail.Undeclared_native_fail_reports_a_diagnostic [2 s]
  Error Message:
   omitted-from-clause: exit 0; no PHnnn (line:col)
no-clause: exit 0; no PHnnn (line:col)
fail-not-first-line: exit 0; no PHnnn (line:col)
one-of-two-undeclared: exit 0; no PHnnn (line:col)
two-undeclared: exit 0; no PHnnn (line:col)
crlf: exit 0; no PHnnn (line:col)
multi-line-call: exit 0; no PHnnn (line:col)
  Stack Trace:
     at Avowry.Acceptance.Tests.UndeclaredNativeFail.Undeclared_native_fail_reports_a_diagnostic() in C:\Users\ringe\projects\wt-202-red\tests\Avowry.Acceptance.Tests\UndeclaredNativeFail.cs:line 35
--- End of stack trace from previous location ---

Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 2 s - Avowry.Acceptance.Tests.dll (net8.0)
```

## Per-id observed pre-change CLI results

Ran `dotnet exec src/Avowry.Cli/bin/Debug/net8.0/Avowry.Cli.dll build FILE -o OUT` from scratch `qa202/` after `dotnet build src/Avowry.Cli`. Sources are `.mob/records/202/repros/`. No surprises: every failing id is exit 0 today; every keep id builds today.

| id | source | kind | exit | observed |
|---|---|---|---|---|
| omitted-from-clause | omitted | fail | 0 | no PHnnn, no Unhandled exception |
| no-clause | noclause (corpus fixture content) | fail | 0 | no PHnnn, no Unhandled exception |
| fail-not-first-line | notfirst | fail | 0 | no PHnnn, no Unhandled exception |
| one-of-two-undeclared | oneof | fail | 0 | no PHnnn, no Unhandled exception |
| two-undeclared | twoundecl | fail | 0 | no PHnnn, no Unhandled exception |
| crlf | crlf (CRLF bytes of omitted) | fail | 0 | no PHnnn, no Unhandled exception |
| multi-line-call | multiline | fail | 0 | no PHnnn, no Unhandled exception |
| declared-twice | twice | keep | 0 | no PHnnn |
| computed | computed | keep | 0 | no PHnnn |
| line-comment | commented | keep | 0 | no PHnnn |
| block-comment | blockc | keep | 0 | no PHnnn |
| trailing-comment | trailing | keep | 0 | no PHnnn |
| raw-string | rawstr | keep | 0 | no PHnnn |
| qualified-call | method | keep | 0 | no PHnnn |
| other-target | pyblock | keep | 0 | no PHnnn |

## D222 list

MULTI Ready row (criterion 1):

| id | construct | Ready row |
|---|---|---|
| omitted-from-clause | native csharp `Fail("X")` while `fails with` omits X | criterion 1 MULTI |
| no-clause | native csharp `Fail("NativeOnly")` with no `fails with` (corpus fixture) | criterion 1 MULTI |

Hostile ids (slice hostile, not from the Ready row):

| id | kind | tag |
|---|---|---|
| fail-not-first-line | fail | slice hostile (not from the Ready row) |
| one-of-two-undeclared | fail | slice hostile (not from the Ready row) |
| two-undeclared | fail | Ready scenario [M] (two undeclared literals) |
| crlf | fail | slice hostile (not from the Ready row) |
| multi-line-call | fail | slice hostile (not from the Ready row) |
| declared-twice | keep | slice hostile (not from the Ready row) |
| computed | keep | Ready scenario [E] (a computed code stays unchecked) |
| line-comment | keep | slice hostile (not from the Ready row) |
| block-comment | keep | slice hostile (not from the Ready row) |
| trailing-comment | keep | slice hostile (not from the Ready row) |
| raw-string | keep | slice hostile (not from the Ready row) |
| qualified-call | keep | slice hostile (not from the Ready row) |
| other-target | keep | slice hostile (not from the Ready row) |

## Green's one data change

The green of criterion 1 changes `tests/Avowry.Acceptance.Tests/Fixtures/corpus/undeclared-native-failure.expected.json` from `{"outcome":"builds",...}` to `{"outcome":"diagnostic","code":"PH337","line":5}`, and rewords only the header line of `tests/Avowry.Acceptance.Tests/Fixtures/corpus/undeclared-native-failure.avowry` (line count stays the same so `Fail` stays on line 5; it must still start `# undeclared-native-failure:`). The theory file is not edited. In this red commit the fixture still says `builds`.

## Gate-filter result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --no-build --filter "Status!=Pending&FullyQualifiedName~UndeclaredNativeFail"`

```
No test matches the given testcase filter `Status!=Pending&FullyQualifiedName~UndeclaredNativeFail` in C:\Users\ringe\projects\wt-202-red\tests\Avowry.Acceptance.Tests\bin\Debug\net8.0\Avowry.Acceptance.Tests.dll
```

0 tests run, exit 0.

## Callers-test result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~GeneratedApiSourceScan"`

```
Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 136 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

`ProcessStartCallers` unchanged.

## Corpus-theory result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Every_corpus_fixture_matches_its_expected_outcome"`

```
Passed!  - Failed:     0, Passed:    23, Skipped:     0, Total:    23, Duration: 19 s - Avowry.Acceptance.Tests.dll (net8.0)
```

`undeclared-native-failure.expected.json` still `{"outcome":"builds",...}`.

## Line count

`tests/Avowry.Acceptance.Tests/UndeclaredNativeFail.cs`: 244 lines, LF only. No corpus fixtures added (those land in the green).

## Correction (facilitator, after the gate review)

The tags of `two-undeclared` and `computed` above were corrected: they come from the Ready draft's scenarios [M] and [E], not from the slice's hostile inputs (the gate reviewer's nit). No id was missing.
