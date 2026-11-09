# Card 203 red (D157 Criterion 1), QA (red) seat

One Pending new-state red, `ReservedGeneratedNames.Names_reserved_by_generated_csharp_report_a_diagnostic` (`Slice=203`, `Criterion=203-1`, `Status=Pending`), 14 rows. Committed after the characterisation commit (see `characterisation-record.md`). No production code changed. Pre-change tree: `7cdc42e8b264ea14cc76855f5080f427592076c7`, characterisation commit `9c483026bfa728e764b8319e2217e6d9af632409`.

## D222 list: the Ready draft's rows first

| id | construct | kind |
|---|---|---|
| R1 | `type AvowAttribute` | reject |
| R2 | `type AvowryFailure` | reject |
| R3 | `type Avows` | reject |
| R4 | `type AvowSpecs` used in a spec value | reject |
| R5 | `endpoint Avow` | reject |
| R6 | `module Program` | reject |
| R7 | avow `"avows"` (method `Avows`) | reject |
| R8 | avow `"avowry failure"` with a failing avow beside it | reject |
| R9 | avow `"fail {x: Text}"` beside failures | reject |
| R10 | `type Email` with field `email` | reject |
| N1 | `type Avow` | near miss (characterisation) |
| N2 | `type AvowryRuntime` | near miss |
| N3 | `type AvowryBuild` | near miss |
| N4 | `type Program` | near miss |
| N5 | `type AVOWS` | near miss |
| N6 | `avow to "fail {x: Int}"` | near miss |

## The slice's changes, with the reason

| change | reason (architect's slice) |
|---|---|
| R9 dropped | Unreachable: the method name joins hole names, so `fail {x: Text}` is `FailX`; a bare `fail` is `Fail()`, a legal overload of `Fail(string)`. |
| N6 becomes `avow-fail-no-holes` | Same reason: the legal case is the bare `fail`. |
| N1 to N5 merged as `type-lookalikes` | One rule (exact ordinal match); one `dotnet test` instead of five. |
| R11 `endpoint-types`, R12 `endpoint-avows`, R13 `endpoint-program`, R14 `endpoint-avowryruntime`, R15 `endpoint-avowrybuild` added | The slice reads the endpoint file name `{E}.cs` against the Api file stems as a silent overwrite. **Not borne out by the build, see "Findings against the slice" below.** |

Implemented: R1 to R8, R10 to R15 (14 rows) as the brief says. Test row ids equal the fixture names (`type-avowattribute`, `type-avowryfailure`, `type-avows`, `type-avowspecs`, `endpoint-avow`, `module-program`, `avow-method-avows`, `avow-method-avowryfailure`, `field-named-like-its-type`, `endpoint-types`, `endpoint-avows`, `endpoint-program`, `endpoint-avowryruntime`, `endpoint-avowrybuild`).

## Findings against the slice (evidence, built on the pre-change tree)

The CLI was run on each row's source and the emitted `M.Specs` project was built with `dotnet build` (it references the Api project).

| id | `dotnet build` of the emitted project | reading |
|---|---|---|
| R1 `type-avowattribute` | CS0101 (x2) | as predicted |
| R2 `type-avowryfailure` | CS0101 (x2) | as predicted |
| R3 `type-avows` | CS0101 (x2) | as predicted |
| R4 `type-avowspecs` | CS1503 and CS1739 (x2 each); not CS1729 | a C# error, so the rejection is real; the predicted code was wrong |
| R5 `endpoint-avow` | CS0101 (x2) and CS0111 (x4) | **a real C# error, contrary to the slice**: `M.Specs/AvowSpecs.cs` and `M.Specs/AvowSpecs.cs.cs` both exist and both declare `AvowSpecs`. The Ready draft was right about R5 and the slice's "silent overwrite" was wrong. |
| R6 `module-program` | CS0101 (x2) | as predicted |
| R7 `avow-method-avows` | CS0542 (x2) | as predicted |
| R8 `avow-method-avowryfailure` | CS0119 (x2) | as predicted |
| R10 `field-named-like-its-type` | CS0542 (x2) | as predicted |
| R11 `endpoint-types` | **Build succeeded, 0 errors** | no collision today |
| R12 `endpoint-avows` | **Build succeeded, 0 errors** | no collision today |
| R13 `endpoint-program` | **Build succeeded, 0 errors** | no collision today |
| R14 `endpoint-avowryruntime` | **Build succeeded, 0 errors** | no collision today |
| R15 `endpoint-avowrybuild` | **Build succeeded, 0 errors** | no collision today |

Why R11 to R15 do not collide: the endpoint file is written as `{E}.cs.cs` (the double extension that the golden file pins), so `Types.cs.cs` sits beside `Types.cs` and nothing is overwritten (the listing of `M.Api` shows both `Avows.cs` and `Avows.cs.cs`, the avow methods stay in `Avows.cs`). The five rows would reject sources that build and run today. They would only become real collisions if the `.cs.cs` follow-up card changes the extension. They are in the test because the brief names them; the decision to keep or drop them (and the "no consumer value, no work" rule) belongs to the facilitator before the green starts. If they are dropped, the test shrinks to 9 rows plus R5, and nothing else changes.

## Pre-change failure line per id

Run with the Pending trait removed (uncommitted, restored afterwards):

`dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~ReservedGeneratedNames"`

```
Failed Avowry.Acceptance.Tests.ReservedGeneratedNames.Names_reserved_by_generated_csharp_report_a_diagnostic [2 s]
  type-avowattribute: exit 0; no PH341 (line:col); 7 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  type-avowryfailure: exit 0; no PH341 (line:col); 7 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  type-avows: exit 0; no PH341 (line:col); 7 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  type-avowspecs: exit 0; no PH341 (line:col); 7 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  endpoint-avow: exit 0; no PH341 (line:col); 9 files present in out (.cs or source map), first M.Api\Avow.cs.cs
  module-program: exit 0; no PH341 (line:col); 7 files present in out (.cs or source map), first Program.Api\AvowryBuild.cs
  avow-method-avows: exit 0; no PH341 (line:col); 7 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  avow-method-avowryfailure: exit 0; no PH341 (line:col); 7 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  field-named-like-its-type: exit 0; no PH341 (line:col); 7 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  endpoint-types: exit 0; no PH341 (line:col); 9 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  endpoint-avows: exit 0; no PH341 (line:col); 9 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  endpoint-program: exit 0; no PH341 (line:col); 9 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  endpoint-avowryruntime: exit 0; no PH341 (line:col); 9 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
  endpoint-avowrybuild: exit 0; no PH341 (line:col); 9 files present in out (.cs or source map), first M.Api\AvowryBuild.cs
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 2 s - Avowry.Acceptance.Tests.dll (net8.0)
```

Every row fails by assertion on the CLI's observable result; none on a missing file or a compile error (every source parses: exit 0 on all 14). The line, message-name, kind-word and `line N` assertions run only once a PH341 line exists, so on this tree they are not yet exercised (see the limits).

## Gate and neighbour commands

- `dotnet test Avowry.sln --nologo --filter "Status!=Pending"` with the test committed Pending: see the results block at the end of this file.
- `python -m unittest tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_extension_map`: before (base tree, work stashed) `Ran 56 tests`, `FAILED (failures=8)`; after (with the new fixtures, the raised count and the new test): `Ran 56 tests`, `FAILED (failures=8)`; the same eight (five rename_196: `test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`; two rename_197: `test_lists_equal_baseline` x2; one live_docs_266: T11). `test_corpus_text_unset` passes at 25.

## Test limits noticed

- The line, name, kind-word and `line N` assertions cannot be watched failing until a PH341 exists; they are asserted by reading, and the green's first run is their first execution. A wrong kind word (for example `field` vs `type` for R10) would only show then.
- R10's expected name is `'Email'` and kind word `field`; the slice message for a field is `field 'email' of type 'Email'`, so `'Email'` matches the type part. If the green words it differently the row must be reread, not weakened.
- R5's expected quoted name is `'AvowSpecs'` (the reserved name), not the endpoint name `Avow`; the slice message shows the user's name in `{shown}` and the reserved name in `'{Name}'`.
- Kind-word checks are substring matches (`type`, `endpoint`, `module`, `avow`, `field`): `avow` also matches `AvowryFailure`, and `type` matches the `of type` in the field message, so they do not discriminate between kinds. The line and name checks carry the weight.
- The test only checks the first PH341 line; the slice says the guard collects every hit, but no row has two collisions, so "all hits sorted by line" is not covered.
- Case-insensitive file systems (endpoint `AVOWS` against `Avows.cs`) and a type named `Specs` against the `M.Specs` namespace are out of scope per the slice and untested.
- R4 and R8 reject unconditionally; the fixtures contain a use only so that the C# error is real (R4: CS1503/CS1739). No row pins that a source which names these but does not use them is still rejected.
- The characterisation fixtures prove only that the five lookalike names and a bare `fail` build and pass one spec each today; they do not prove that a guard using prefix or case-insensitive comparison would fail them for every name (`Avow`, `AVOWS` do; `AvowryRuntime` and `AvowryBuild` would only fail a guard that wrongly reserved file stems for types).

## Results block (gate with the Pending test committed)

`dotnet test Avowry.sln --nologo --filter "Status!=Pending"` (second, quiet run, no other load):

```
Passed!  - Failed:     0, Passed:   324, Skipped:     0, Total:   324, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:   343, Skipped:     0, Total:   343, Duration: 9 m 30 s - Avowry.Acceptance.Tests.dll (net8.0)
```

The first run, made while the python neighbour set ran at the same time, had one failure: `BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms` (a 29 s timing test). It passed alone (`FullyQualifiedName~BenchmarkHarnessAcceptance`: 45 passed) and in the quiet full run; it is timing-sensitive under load and unrelated to this card.
