# Card 324 red 3 record

## Files added

- `tests/Avowry.Compiler.Tests/Diagnostics/OtherTenMessagesSayAvowTests.cs` (31 lines)
- `tests/Avowry.Acceptance.Tests/Support/CSharpLiteralScanner.cs` (240 lines)
- `tests/Avowry.Acceptance.Tests/CompilerMessagesNameTheAvow.cs` (129 lines)

C# total: 400. No production edits under `src/`.

## What each test checks

`The_other_ten_messages_say_Avow(id)` (Criterion 3): 11 rows M09-PH325, M10-PH328, M11-PH330, M12-PH333, M13-PH001, M14a-PH208-native-then-steps, M14b-PH208-steps-then-native, M15-PH200-template, M16-PH200-clause, M17-PH200-call, M18-PH200-trigger. Compiles `TriggerSources.Source(id)` and for each baseline row matches code, line and column, then `Assert.Equal(ToNew(baseline text), actual.Message)`.

`A_planted_old_name_is_found_with_its_file_and_line`: planted `src/Foo.cs` with regular, interpolated, verbatim and raw literals containing Phrase/phrase, plus a `{phrase.Template}` hole, a comment, a char literal, and `obj/` `bin/` copies of Phrase. Scan returns exactly `(src/Foo.cs, 3)`, `(4)`, `(6)`, `(7)`.

`The_registry_keys_in_ExtensionKeywords_are_the_only_named_exceptions`: six pinned literals yield 0 violations and `ExceptionsMatched == 6`; a seventh `"phrase"` plus the same literal in Resolver.cs yield one violation each.

`No_message_in_src_names_the_old_unit`: live `src/**/*.cs` scan, skip obj/bin. Expects 0 violations, `ExceptionsMatched == 6`, at least 15 files. Failure message lists `file:line` and the literal body.

`Build_prints_the_avow_wording_for_an_unmatched_step`: CLI on the M13 source; exit 1 and stderr line `PH001 (11:5) No avow matches this step. Closest: "do nothing".`.

## Stub-first

`CSharpLiteralScanner.Scan` first returned empty (0 violations, ExceptionsMatched 0, FilesScanned 0). Command:

```text
dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~A_planted_old_name_is_found|FullyQualifiedName~The_registry_keys_in_ExtensionKeywords"
```

```text
Failed!  - Failed:     2, Passed:     0, Skipped:     0, Total:     2, Duration: 143 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

Pin test: Expected 6, Actual 0. Planted test: expected the four `(src/Foo.cs, line)` tuples, actual empty.

## Real scanner

Same command after filling `Scan`:

```text
Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2, Duration: 33 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

## Reds on the current tree

(a) `FullyQualifiedName~No_message_in_src_names_the_old_unit`

```text
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 23 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

11 sites as printed:

```text
Resolver.cs:119 Composite phrase '' cannot return a value.; Resolver.cs:351 Composite phrase '' must declare effect ''.; Resolver.cs:389 Spec '' on phrase '' must use 'when called'.; Resolver.cs:566 Spec '' expects success for a phrase that returns a value.; Resolver.cs:729 No phrase matches this step.; Parser.cs:291 a quoted phrase template; Parser.cs:341 A phrase is implemented by native blocks or by steps, not both.; Parser.cs:353 A phrase is implemented by native blocks or by steps, not both.; Parser.cs:358 a phrase clause (effects, fails with, spec, native or steps); Parser.cs:699 a phrase call; Parser.cs:873 `called` (phrase spec) or `handled` (endpoint spec)
```

Lines: Resolver 119, 351, 389, 566, 729; Parser 291, 341, 353, 358, 699, 873. Interpolated holes are stripped, so those bodies show empty quotes.

(b) `FullyQualifiedName~OtherTenMessagesSayAvow`

```text
Failed!  - Failed:    11, Passed:     0, Skipped:     0, Total:    11, Duration: 83 ms - Avowry.Compiler.Tests.dll (net8.0)
```

Each failed because actual still says phrase (ToNew expected avow).

(c) `FullyQualifiedName~Build_prints_the_avow_wording_for_an_unmatched_step`

```text
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 165 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

Expected `No avow matches`; actual `No phrase matches`.

## Guards

`FullyQualifiedName~DiagnosticCharacterisation`:

```text
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 99 ms - Avowry.Compiler.Tests.dll (net8.0)
```

`FullyQualifiedName~LeadingMessagesSayAvow`:

```text
Passed!  - Failed:     0, Passed:     8, Skipped:     0, Total:     8, Duration: 56 ms - Avowry.Compiler.Tests.dll (net8.0)
```
