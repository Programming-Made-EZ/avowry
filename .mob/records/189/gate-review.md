# Card 189 pre-ship gate review (an independent fresh Claude context that wrote none of the card; read-only; scratch single-branch clones gate189-review and gate189-review4)

Verdict: SHIP (after one HOLD, fixed by a red revision and a green revision and re-reviewed). Reported by the reviewer, committed in substance by the facilitator.

## First review (tip bbbc6e7): HOLD on one finding, everything else passed
1. HOLD: a reachable crash left in the "avow spec none" row. The Ready table row read "Avowry spec given/then returns of none (:122/:126)"; the green converted only the `given` half. A leaf `maybe {n: Int} -> Option<Int>` with a spec `given n = 0`, `when called`, `then returns none` exited with an unhandled NotImplementedException (CSharpProvider.cs:150, "does not support the outcome of spec ... yet"), as it did before the green; nothing in the red, the fixtures or the green exercised it. The reviewer would have changed this to SHIP had the facilitator ruled the half out of scope with a bug; the facilitator ruled it in scope (the card's criterion 2 includes the row and the value is Dana never meeting a crash).
2. Red replay (D157/D154): pass. At 068c378 the test fails by assertion with 12 id lines (the eight rows and four hostile inputs, each "exit -532462766; Unhandled exception; NotImplementedException; stack at; no PHnnn (line:col)"); the PH325 characterisation passes there; at the tip both pass. Criterion order: characterisation (`Criterion: 3`), red (`Criterion: 1-2`), green (`Criterion: 1-2`).
3. D137/D204: pass. The wrapper diff 068c378..164b928 is exactly the one removed Pending trait line; the diff stat (22 files, 211 insertions, 16 deletions) matches slice section 3. The green's extra edit to tests/test_rename_196.py (the R100 rename check restricted to the fixtures present at the rename commit) does not weaken it: with one original fixture renamed, `test_corpus_renames_r100` fails "14 != 13"; the count assertion 22 still guards additions.
4. Mutations of the production change: caught: effects (:63), plain-call and fail (:89), seed (:138), effectful let (:94), spec given (:154), let argument (:117), PH020 (:72) each reverted to NotImplementedException; PH340 to PH341 at :94 and :138; RequireSupportedSeeds removed. Survived: the Column set to 0 at :89, :138 and :63 (all 24 tests pass): nothing pins the column; the acceptance test and the expected.json check only the code and the line (the older corpus convention). Minor.
5. Supported output unchanged: pass. Two supported sources (a stored Int field without a `then succeeds` spec; a Text id with Text/Bool fields and `then succeeds`) and demo/tasks/Tasks.avowry build with exit 0 and byte-identical output (19 files for Tasks) to the red build; RequireSupportedSeeds mirrors KnownIdSeed and StoredDefault.
6. Diagnostic quality as Dana: good enough. Every row exits 1 with `PHnnn (line:col) message` on stderr and no stack trace; messages name the construct and say "does not support ... yet"; PH020 says what to do. Notes (polish): row 6 read "a given value of that kind in spec ..." (vague; improved in the green revision), row 4 points at the avow rather than the `effects` clause, rows 5, 7 and 8 do not name the argument, the callee or the field.
7. Neighbours: nothing new red: 76 tests, 8 failures (5 in test_rename_196, 2 in test_rename_197, T11 bug 338), all also red at 068c378.
8. Full gate at the tip: Compiler 324 of 324; Acceptance 337 of 338 in 9 m 18 s: the one failure was `BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms` ("did not finish within 20 seconds"), passing 1 of 1 alone in 9 s (issue 304, fourth sighting).
9. Scope: the diff delivers "diagnostic, not a crash" for the eight rows and nothing else; the dead throw sites are untouched; the GRAMMAR.md edit is limited to a PH020 row, a PH340 row and a clause, with the D72 reason in the green commit body.

## Re-review of the delta (tip 8ee6230)
1. Red revision replay: pass. At 3f2c974 (CLI built first) the test fails by assertion with exactly one line `spec-then-none: ...` and nothing else; at 8ee6230 it passes (1 of 1).
2. Test diffs: `git diff 3f2c974 8ee6230 -- tests/Avowry.Acceptance.Tests/UnsupportedCsharpConstructs.cs` is empty; the red revision's change to that file is 20 added lines, no deletions.
3. Mutations at the tip: the outcome throw reverted to NotImplementedException: caught by the test and the corpus theory; code changed to PH341: caught by both; Column set to 0: survives (25 of 25 pass), the same minor finding.
4. Messages as Dana: good enough. `PH340 (5:3) The C# target does not support the outcome 'returns none' in spec 'none' yet.` and `PH340 (5:3) ... the given value none for 'x' in spec 'none' yet.`; a non-none unsupported return reads `the outcome 'returns of type Box' in spec 'rec' yet` (clumsy, still names the construct and the spec).
5. Neighbours: the corpus theory, CliKeepsDiagnosticShape, WebExtensionBoundary (orders hash), PhraseBuildOneFile, GeneratedApiSourceScan and the PH325 characterisation: 40 of 40; test_rename_196 shows the same 5 already-red names.
6. Other crashes in the spec/outcome area: none found (a record containing none gives PH340 at 7:3; a List<Option<Int>> given of [none] gives PH340 at 12:3; a Decimal given and return builds; an endpoint spec responding with a record containing none builds with exit 0; `then returns n` gives the resolver diagnostic PH332; a composite spec with `fails with` is covered by the existing composite-fail row).

## Facilitator's isolated gate runs (for the record)
At the green 164b928: Compiler 324 of 324, Acceptance 338 of 338, 0 warnings (gate189.out). At the green revision 8ee6230: Compiler 324 of 324, Acceptance 339 of 339, 0 warnings (gate189b.out), one dotnet process at start and at end.

## Process notes
- The HOLD cost one red revision and one green revision (about 45 minutes); the cause: the PM draft's row 6 named two halves (`given` and `then returns`), the slice's row list and the red carried only `given`. Retro finding: a row that names two halves becomes two ids in the red.
- Column unpinned: no test asserts the diagnostic column (the corpus convention is code and line only); the CLI shape `PHnnn (line:col)` is asserted by CliKeepsDiagnosticShape for other codes.

## Gate evidence
Tested commit 8ee6230dc3c8b9fd05f3cd3de164aec40564fe3e (equal to main's tip at clone time for the re-review; bbbc6e7b1f3bf463f3fcab8463e9a51831fbc018 for the first review). Copy: `git clone --no-hardlinks --single-branch -b main` from C:/Users/<user>/projects/phrase-lang, not an archive. Build: 0 warnings, 0 errors. The re-review did not rerun the full gate (the sign-off does).

## Load
First review: one dotnet.exe (PID 28072) at the start; during the full run only idle wait loops. Re-review: one dotnet.exe at the start and at the end; dotnet commands one at a time.
