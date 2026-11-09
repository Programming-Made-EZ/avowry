# Card 203 independent gate review (fresh Claude context, read-only in the main checkout; own clone)

**Step 0:** `git merge-base --is-ancestor 5a82e7d30e4cc26b1d53cd2498f05b07cff7fee3 HEAD`, tested commit 5a82e7d, exit status 0.

**Verdict: SHIP.** Two non-blocking findings (F1, F2); no blocking ones.

## Findings
- F1 (minor, wrong line in a message): `src/Avowry.Adapters.CSharp/CSharpProvider.cs:145`. The field hit is reported with `type.Line, type.Column`, so the message says "rename the field declared on line N" with N the type's line, not the field's. In the corpus fixture `field-named-like-its-type.avowry`, `type Email` is on line 4 and `email: Text` on line 5; the fixture pins line 4 and the red test's marker is `type Email`, so the fixtures agree with the code. The card's criterion 1 names module, type and avow, not field, so no criterion breaks; the slice had said field hits would use the field's line.
- F2 (minor, dead code): `Model.cs` adds `FieldModel.Line` and `FieldModel.Column` and `Resolver.cs` sets them, but nothing reads them (CODING_GUIDELINES item 7), related to F1.
- Ready-draft R8 was conditional; the green rejects `avowry failure` unconditionally as the slice says (a known cost; no row pins the unconditional case, limit 4).

## Checks
1. Red-first and order: 9c48302 (16:47), 481e263 (17:11), 2b38921 (17:26) precede 5a82e7d (17:43). The green changed the red test file by exactly one line, removing `[Trait("Status","Pending")]`. Red replay at 2b38921 in a detached worktree of the reviewer's clone: `dotnet build src/Avowry.Cli` 0 errors; `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Slice=203"` gave `Failed!  - Failed: 1, Passed: 0, Total: 1`, by assertion at `ReservedGeneratedNames.cs:34`; the nine per-row lines each read `exit 0; no PH341 (line:col); 7 files present in out` (9 for `endpoint-avow`): type-avowattribute, type-avowryfailure, type-avows, type-avowspecs, endpoint-avow, module-program, avow-method-avows, avow-method-avowryfailure, field-named-like-its-type. The only Python test touching the card is the corpus count (25 in the characterisation, 26 in the red revision, 31 in the green).
2. Ready coverage (D222): the red record's table accounts for every id: R1-R8 and R10 kept; R9 dropped with a reason; N1-N5 merged into `type-lookalikes`; N6 `avow-fail-no-holes`; R11-R15 near misses merged into `endpoint-file-stems`; R5 kept as a reject (the slice's silent-overwrite claim was refuted; CS0101 and CS0111). All three criteria are covered; the message names the reserved name and the line, for example `PH341 (8:1) ... reserves the name 'AvowSpecs' ... endpoint ... line 8`.
3. Generated C# unchanged (`TasksDemoGeneratedCsharp` passes); GRAMMAR.md diff is exactly the PH341 row plus the one intro sentence; no rename drift; no reserved literal outside `ReservedNames.cs`; no throw on user input; exact assertions.

## Mutation table (each: CLI rebuild, then `--filter "ReservedGeneratedNames|EdgeCaseFixtureCorpus|TasksDemoGeneratedCsharp"`, 34 tests; restored with `git checkout`)
| Mutation | Result | Caught by |
|---|---|---|
| a1-a4 drop `AvowAttribute`, `AvowryFailure`, `Avows`, `AvowSpecs` from `TypeNames` (one each) | killed | red test (a3 also the type-avows fixture) |
| a5 no module `Program` check | killed | red test and module-program fixture |
| a6 drop `Avows` from `MethodNames` | killed | red test and avow-method-avows fixture |
| a7 drop `AvowryFailure` from `MethodNames` | killed | red test |
| a8 no field rule; a9 no endpoint rule | killed | red test and the fixtures |
| b guard also rejects endpoint `Types` | killed | `endpoint-file-stems` |
| c `TypeNames` case-insensitive | killed | `type-lookalikes` (`AVOWS`) |
| c2 `MethodNames` case-insensitive | **survived** | none (34 passed) |
| d1 change a comment in the emitted runtime text | killed | golden test |
| d2 rename `Code` to `CodeX` in the emitted exception | killed | golden test and 5 corpus rows |
| e1 report only the first hit | **survived** | none |
| e2 hits not sorted by line | **survived** | none |
| f call to the guard removed | killed | red test, 6 rows |

Surviving mutants are test limits: (1) c2: no near miss for a method name differing only in case (an avow `"AVOWS"`); (2) e1, e2: no row has two collisions, so "report all hits, sorted by line" is untested; (3) kind-word assertions are weak substring matches (`avow` matches `AvowryFailure`); (4) R4 and R8 unconditional rejection is not pinned by a source naming the type or method without using it.

## Gate and neighbour commands
`dotnet build src/Avowry.Cli --nologo`: 0 Warning(s), 0 Error(s). `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`: Compiler 324 of 324, Acceptance 350 of 350 in 9 m 28 s; no retry; the timing test did not fail. `python -m unittest tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_extension_map`: Ran 56 tests, FAILED (failures=8): the expected eight (five rename_196, two rename_197, T11 live_docs_266); `test_corpus_text_unset` passes at 31.

**Gate evidence:** tested commit 5a82e7d30e4cc26b1d53cd2498f05b07cff7fee3, a fresh `--no-hardlinks` single-branch clone of local main; working tree clean after the gate; dotnet processes at the start: 0 before cloning and 1 (a residual build server) when the gate started. The main checkout was not written to.
**Load:** the gate ran alone, the Python neighbour set after it; nothing else of the reviewer's ran during the 9 m 28 s Acceptance run.

## Facilitator note
My own baseline gate (isolated clone, CLI first) on the same tip: first run one failure (`BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms`, 29 s, under load; passes alone); the rerun was 324 of 324 and 350 of 350. F1 is filed as a bug after the close; F2 rides with it.
