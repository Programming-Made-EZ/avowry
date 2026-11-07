# Card 324 red 1 record

## Test added

`tests/Avowry.Compiler.Tests/Diagnostics/LeadingMessagesSayAvowTests.cs`
adds the theory `The_eight_leading_messages_say_Avow(string id, string expected)`.
Its eight rows are:

- `M01-PH010`
- `M02-PH321`
- `M03-PH320`
- `M04-PH327`
- `M05-PH331-input`
- `M06-PH331-hole`
- `M07-PH334`
- `M08-PH203`

Each row compiles `TriggerSources.Source(id)`, selects the diagnostic matching
the code, line and column of the first `TriggerSources.Baseline(id)` row, checks
those three fields exactly, and checks the complete expected message.

## Watched failing run

Command:

```text
dotnet test tests/Avowry.Compiler.Tests --nologo --filter "FullyQualifiedName~LeadingMessagesSayAvow"
```

Exact count and summary:

```text
Failed!  - Failed:     8, Passed:     0, Skipped:     0, Total:     8, Duration: 151 ms - Avowry.Compiler.Tests.dll (net8.0)
```

Two example assertion messages:

```text
Assert.Equal() Failure: Strings differ
        ↓ (pos 0)
Expected: "Avow 'do nothing' returns no value and ca"···
Actual:   "Phrase 'do nothing' returns no value and "···
        ↑ (pos 0)
```

```text
Assert.Equal() Failure: Strings differ
        ↓ (pos 0)
Expected: "Avow specs cannot bind input; use a hole "···
Actual:   "Phrase specs cannot bind input; use a hol"···
        ↑ (pos 0)
```

All eight cases failed for the intended old leading unit word: expected `Avow`
and actual `Phrase`. No case failed from a missing source, compilation error, or
incorrect diagnostic selection.

## Characterisation run

Command:

```text
dotnet test tests/Avowry.Compiler.Tests --nologo --filter "FullyQualifiedName~DiagnosticCharacterisation"
```

Result:

```text
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 89 ms - Avowry.Compiler.Tests.dll (net8.0)
```
