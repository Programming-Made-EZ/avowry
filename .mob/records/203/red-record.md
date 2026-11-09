# Card 203 red (D157 Criterion 1), QA (red) seat

One Pending new-state red, `ReservedGeneratedNames.Names_reserved_by_generated_csharp_report_a_diagnostic` (`Slice=203`, `Criterion=203-1`, `Status=Pending`), 9 rows after the facilitator's revision (first committed with 14; see "Revision"). Committed after the characterisation commit (see `characterisation-record.md`). No production code changed. Revision commit: see `git log`. Pre-change tree: `7cdc42e8b264ea14cc76855f5080f427592076c7`, characterisation commit `9c483026bfa728e764b8319e2217e6d9af632409`.

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

First committed: R1 to R8, R10 to R15 (14 rows) as briefed; revised below to R1 to R8 and R10 (9 rows). Test row ids equal the fixture names (`type-avowattribute`, `type-avowryfailure`, `type-avows`, `type-avowspecs`, `endpoint-avow`, `module-program`, `avow-method-avows`, `avow-method-avowryfailure`, `field-named-like-its-type`; the first commit also had `endpoint-types`, `endpoint-avows`, `endpoint-program`, `endpoint-avowryruntime`, `endpoint-avowrybuild`, removed in the revision).

## Findings against the slice (evidence, built on the pre-change tree)

The CLI was run on each row's source and the emitted `M.Specs` project was built with `dotnet build` (it references the Api project).

| id | `dotnet build` of the emitted project | reading |
|---|---|---|
| R1 `type-avowattribute` | CS0101 (x2) | as predicted |
| R2 `type-avowryfailure` | CS0101 (x2) | as predicted |
| R3 `type-avows` | CS0101 (x2) | as predicted |
| R4 `type-avowspecs` | CS1503 and CS1739 (x2 each); the slice predicted CS1729, which is wrong | a C# error, so the rejection is real |
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

Why R11 to R15 do not collide: the endpoint file is written as `{E}.cs.cs` (the double extension that the golden file pins), so `Types.cs.cs` sits beside `Types.cs` and nothing is overwritten (the listing of `M.Api` shows both `Avows.cs` and `Avows.cs.cs`, the avow methods stay in `Avows.cs`). The five rows would reject sources that build and run today. They would only become real collisions if the `.cs.cs` follow-up card changes the extension. They were removed from the test in the revision below and kept as a near-miss fixture.

## Revision (third commit): R11 to R15 moved from reject to near miss

Facilitator decision: R11 to R15 build with 0 errors today (table above), so rejecting them would break working builds with no collision; card 203 is about names that collide. R5 stays a reject (CS0101/CS0111, the Ready draft was right, the slice was wrong). The Pending test is now 9 rows: R1 to R8 and R10. R11 to R15 are now CHARACTERISATION: one merged near-miss corpus fixture `endpoint-file-stems` (endpoints `Types`, `Avows`, `Program`, `AvowryRuntime`, `AvowryBuild`, each with an avow call, `output`, `respond with`, and a spec `then responds { r: 1 }`; `runSpecs: true`). All five really build and their five specs pass on the pre-change tree (corpus theory plus golden: `Passed: 28, Failed: 0`). The corpus count in `tests/test_rename_196.py` is raised from 25 to 26. (A first version without `output` and `respond with` failed with CS0815 in the generated endpoint specs, an error of the fixture, not of the names; it was fixed before commit.)

Final ids: reject R1 `type-avowattribute`, R2 `type-avowryfailure`, R3 `type-avows`, R4 `type-avowspecs`, R5 `endpoint-avow`, R6 `module-program`, R7 `avow-method-avows`, R8 `avow-method-avowryfailure`, R10 `field-named-like-its-type`; near misses N1 to N5 (`type-lookalikes`), N6 (`avow-fail-no-holes`), R11 to R15 as near misses (`endpoint-file-stems`). The green adds five reject corpus fixtures (R3, R5, R6, R7, R10) and raises the count from 26 to 31 (the slice said 30 from 25).

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
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 2 s - Avowry.Acceptance.Tests.dll (net8.0)
```

(Run after the revision, 9 rows; the 14-row run showed the same line for the five removed rows.) Every row fails by assertion on the CLI's observable result; none on a missing file or a compile error (every source parses: exit 0 on all 9). The line, message-name, kind-word and `line N` assertions run only once a PH341 line exists, so on this tree they are not yet exercised (see the limits).

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

## Results after the revision (9 rows, 26 corpus fixtures)

- Red (Pending trait removed, uncommitted, restored): 9 rows fail by assertion with `exit 0; no PH341 (line:col); 7 or 9 files present in out`; `Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1`.
- Corpus and golden: `Passed!  - Failed:     0, Passed:    28, Skipped:     0, Total:    28` (26 theory rows, header test, golden test).
- `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`:

```
Passed!  - Failed:     0, Passed:   324, Skipped:     0, Total:   324, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:   344, Skipped:     0, Total:   344, Duration: 9 m 31 s - Avowry.Acceptance.Tests.dll (net8.0)
```

- `python -m unittest tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_extension_map`: `Ran 56 tests`, `FAILED (failures=8)`, the same eight as before (five rename_196, two rename_197, T11 of live_docs_266); `test_corpus_text_unset` passes at 26.
- Limit added: the green's reject-fixture count arithmetic (26 plus five reject fixtures = 31) differs from the slice's 25 plus five = 30.
