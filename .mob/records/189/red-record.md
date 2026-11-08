# Card 189 red (D157 Criteria 1 and 2), QA (red) seat

Pending new-state red for criteria 1 and 2, one test, committed with `Status=Pending` so the gate excludes it. Watched failing by assertion on the current tree (no production change in `src/`).

## Pre-change commit hash

`git rev-parse HEAD` before this commit: `a641d7fa768aa4bfcf9109f0781e33203585e0a2` (the characterisation commit; production tree still matches `b64e72a24587d240509a690e5c953d4d5bd24e40`).

## Command

```
dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Unsupported_csharp_constructs_report_a_diagnostic"
```

## Failing test

`Avowry.Acceptance.Tests.UnsupportedCsharpConstructs.Unsupported_csharp_constructs_report_a_diagnostic`

Attributes in order: `[Fact]`, `[Trait("Slice", "189")]`, `[Trait("Criterion", "189-1")]`, `[Trait("Status", "Pending")]`. Calls `PhraseCli.RunAsync`. No `Process.Start(` in the file.

## Expected failure

Assertion failure from `Assert.True(failures.Count == 0, ...)`, not a missing file and not a compile error. Every kept row still crashes (exit not 1, Unhandled exception, NotImplementedException, a stack line `   at `, no `PHnnn (line:col)`).

## Aggregated failure text (full)

```
  Failed Avowry.Acceptance.Tests.UnsupportedCsharpConstructs.Unsupported_csharp_constructs_report_a_diagnostic [32 s]
  Error Message:
   plain-call: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
composite-fail: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
native-python-only: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
two-effects: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
let-literal-arg: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
spec-none: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
let-effectful: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
seed-int: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
unused-composite: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
crlf-copy: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
supported-avow-first: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
two-problems: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
  Stack Trace:
     at Avowry.Acceptance.Tests.UnsupportedCsharpConstructs.Unsupported_csharp_constructs_report_a_diagnostic() in C:\Users\ringe\projects\wt-189-red\tests\Avowry.Acceptance.Tests\UnsupportedCsharpConstructs.cs:line 31

Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 32 s - Avowry.Acceptance.Tests.dll (net8.0)
```

Windows exit `-532462766` is HRESULT `0xE0434352` (CLR unhandled exception); the Python CLI probe recorded the same bits as unsigned `3762504530`.

## Per-row observed pre-change CLI results

Ran `dotnet exec src/Avowry.Cli/bin/Debug/net8.0/Avowry.Cli.dll build FILE -o OUT` from scratch `qa189/` after `dotnet build src/Avowry.Cli`.

| id | source | exit | observed |
|---|---|---|---|
| plain-call | r1 | 3762504530 | Unhandled exception, NIE `step 'inner n'` at CSharpProvider.cs:88 |
| composite-fail | r2 | 3762504530 | Unhandled exception, NIE `step 'fail with Bad'` at :88 |
| native-python-only | r3 | 3762504530 | Unhandled exception, NIE `needs a native csharp block on 'double {n: Int}'` at :71 |
| two-effects | r4 | 3762504530 | Unhandled exception, NIE `more than one effect on 'ping {n: Int}'` at :62 |
| let-literal-arg | r5 | 3762504530 | Unhandled exception, NIE `call argument in 'inner {n: Int}'` at :112 |
| spec-none | r6 | 3762504530 | Unhandled exception, NIE `given value of spec 'none'` at :126 |
| let-effectful | r7 | 3762504530 | Unhandled exception, NIE `cannot supply effect 'ItemStore'` at :472 |
| seed-int | r8 | 3762504530 | Unhandled exception, NIE `cannot seed a stored field of type 'Int'` at :741 |
| unused-composite | r13 | 3762504530 | Unhandled exception, NIE `step 'fail with Bad'` at :88 (composite never called) |
| crlf-copy | r1 CRLF | 3762504530 | same as plain-call |
| supported-avow-first | MultiLevel with `ok` first | 3762504530 | same as plain-call, `step 'inner n'` |
| two-problems | native python + fail with | 3762504530 | Unhandled exception, NIE `needs a native csharp block` at :71 |

Kept: all eight in-card rows and the four hostile rows (each crashed). Dropped from the test: none of those. Also probed and not added (already diagnostics, slice dropped): r9 PH322 exit 1; r11 PH321 exit 1. Variants of kept rows, not extra ids: r3b (typescript, same :71 crash as r3), r10 (field argument, same :112 crash as r5), r12 (leaf callee, same :88 crash as r1).

## Gate-filter result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --no-build --filter "Status!=Pending&FullyQualifiedName~UnsupportedCsharpConstructs"`

```
No test matches the given testcase filter `Status!=Pending&FullyQualifiedName~UnsupportedCsharpConstructs` in C:\Users\ringe\projects\wt-189-red\tests\Avowry.Acceptance.Tests\bin\Debug\net8.0\Avowry.Acceptance.Tests.dll
```

0 tests run, exit 0.

## Callers-test result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~GeneratedApiSourceScan"`

```
Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 116 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

`ProcessStartCallers` unchanged.

## Neighbouring-tests result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~EdgeCaseFixtureCorpus|FullyQualifiedName~CliKeepsDiagnosticShape|FullyQualifiedName~PhraseBuildOneFile"`

```
Passed!  - Failed:     0, Passed:    23, Skipped:     0, Total:    23, Duration: 18 s - Avowry.Acceptance.Tests.dll (net8.0)
```

Pre-change (before any new test file): the wider neighbour filter including WebExtensionBoundary and GeneratedApiSourceScan passed 30. `python -m unittest tests.test_rename_196` was already red on that tree (6 failures); not this seat's.

## Line count

`tests/Avowry.Acceptance.Tests/UnsupportedCsharpConstructs.cs`: 388 lines, LF only. No corpus fixtures added (those land in the green).

## Revision 1

Adds the missing `then returns none` row. The independent gate reviewer found one reachable crash the original red did not cover: the card's table row reads "avow spec given/then returns of none", and the red (`068c378`) and the green (`164b928`) cover only the `given` half. A leaf `maybe {n: Int} -> Option<Int>` with a spec `given n = 0`, `when called`, `then returns none` still exits with an unhandled NotImplementedException at `src/Avowry.Adapters.CSharp/CSharpProvider.cs:150` ("does not support the outcome of spec ... yet"). This revision is built on main `bbbc6e7`. The test has no Pending trait (it passes on main); after this commit it is expected to FAIL by assertion until the green revision lands, so this revision is pushed only together with its green.

### Pre-change commit hash

`git rev-parse HEAD` before this commit: `bbbc6e7b1f3bf463f3fcab8463e9a51831fbc018` (main).

### Command

```
dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Unsupported_csharp_constructs_report_a_diagnostic"
```

### Failing test

`Avowry.Acceptance.Tests.UnsupportedCsharpConstructs.Unsupported_csharp_constructs_report_a_diagnostic`

No Pending trait. Calls `PhraseCli.RunAsync`. One new row `spec-then-none`; no other row, Check() logic, or attribute changed.

### Expected failure

Assertion failure from `Assert.True(failures.Count == 0, ...)`, not a missing file and not a compile error. Exactly one row id: `spec-then-none`. Every other row already reports a diagnostic on this tree.

```
  Failed Avowry.Acceptance.Tests.UnsupportedCsharpConstructs.Unsupported_csharp_constructs_report_a_diagnostic [5 s]
  Error Message:
   spec-then-none: exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)
  Stack Trace:
     at Avowry.Acceptance.Tests.UnsupportedCsharpConstructs.Unsupported_csharp_constructs_report_a_diagnostic() in C:\Users\ringe\projects\wt-189-red2\tests\Avowry.Acceptance.Tests\UnsupportedCsharpConstructs.cs:line 30
--- End of stack trace from previous location ---

Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 5 s - Avowry.Acceptance.Tests.dll (net8.0)
```

Windows exit `-532462766` is HRESULT `0xE0434352` (CLR unhandled exception).

### Observed pre-change CLI result of the new row

Ran `dotnet exec src/Avowry.Cli/bin/Debug/net8.0/Avowry.Cli.dll build FILE -o OUT` from scratch `qa189/` after `dotnet build src/Avowry.Cli`.

| id | source | exit | observed |
|---|---|---|---|
| spec-then-none | leaf `maybe {n: Int} -> Option<Int>`, spec `given n = 0` / `when called` / `then returns none`, native csharp `return null` | -532462766 | Unhandled exception, NIE `The C# emitter does not support the outcome of spec 'none' yet.` at CSharpProvider.cs:150 |

A control copy that changes only the outcome to `then returns 0` (native csharp `return n`) exits 0, so the only problem is the spec outcome.
