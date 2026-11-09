# Card 189 sign-off (a fresh Claude context, separate from the gate reviewer; read-only; own scratch single-branch clone signoff189-clone; it read the gate review only after its own reruns)

Verdict: SIGNED OFF WITH CONDITIONS. The conditions are minor and none blocks closure.

## Findings
1. Criteria on the card's own repros. At the tip, `MultiLevel.avowry` and repros r1 to r13 (including r3b, r10, r12 and the unused-composite case r13) each exit 1 with `PHnnn (line:col) message` and no stack trace. r1, r2, r4 to r8, r10, r12 and r13 give PH340; r3 and r3b give PH020; r9 and r11 give PH322 and PH321 as before the card. MultiLevel reports `PH340 (15:5)` and line 15 is `inner n`. Story promise ("a clear message", "a limitation, not a crash"): Criterion 1 covers the plain call and Criterion 2 the other rows, both through `Unsupported_csharp_constructs_report_a_diagnostic`; the wording "does not support ... yet" is checked; PH325 for a composite return type is covered by the characterisation. Not checked by any test: the diagnostic column; "names the construct" only loosely. The sign-off's own shapes: a Decimal stored field with `then succeeds` gives PH340; a `List<Option<Int>>` given containing `none` gives PH340; a leaf spec returning a record with a `none` field gives PH340 (message "the outcome 'returns of type P'"); an endpoint with two effects builds with exit 0. No remaining NotImplementedException from user source found; the hole-type and return-type throws at lines 105 and 109 could not be reached because the resolver rejects those types first (an observation, not a proof).
2. Red replay: pass. At 068c378 the test fails by assertion with 12 id lines and the PH325 characterisation passes; at 3f2c974 it fails with exactly one line `spec-then-none`; at the tip both pass. The test file is byte-identical between 3f2c974 and the tip; between 068c378 and 164b928 the only change is the removed Pending trait line.
3. Own full gate at the tip: Compiler 324 of 324, Acceptance 339 of 339, 0 warnings; the benchmark flake did not occur.
4. Neighbours: the seven python modules ran 76 tests with 8 failures, exactly the allowed set (5 in test_rename_196, 2 in test_rename_197, T11 in test_live_docs_266); the same 8 fail at b64e72a.
5. Value and scope: the diff from b64e72a delivers Dana's value and nothing else (the throw conversions, the RequireSupportedSeeds pre-check, the Column properties used only for the diagnostic). Supported output unchanged: Tasks.avowry builds with exit 0 and the orders hash test is in the passing gate. The GRAMMAR.md edit is minimal (the PH020 row, the PH340 row and two clauses) and consistent with the D72/D127 decision. tests/test_rename_196.py was edited beyond the count change (14 to 23 after the revision; the R100 check restricted to fixtures present at the rename commit): defensible, judged non-weakening by the gate review.
6. Gate review (read last): agrees, including the minor points (column unpinned, the clumsy non-none return message); the first HOLD on `then returns none` is fixed by the revisions replayed. The difference: its Acceptance run hit the 20-second benchmark flake and the sign-off's did not.

## Gate evidence
Tested commit 5fe2628 (main's tip). Copy: `git clone --no-hardlinks --single-branch -b main C:/Users/<user>/projects/phrase-lang C:/Users/<user>/projects/signoff189-clone`, not an archive. `dotnet build src/Avowry.Cli`: 0 warnings, 0 errors.

## Load
One dotnet process at the start (tasklist count of lines matching dotnet). dotnet commands one at a time; the Python neighbour run took place after the gate finished.

## Conditions (non-blocking; filed by the facilitator the same day)
- C1: pin the diagnostic column in a test (a column of 0 survives): filed as bug card 352.
- C2: reword the message for an unsupported non-none return and make the effects, argument and seed messages name the callee, argument or field; row 4 points at the avow, not the `effects` line: filed as bug card 353.
- C3: card 162's fold must include the PH020 and PH340 rows (the facilitator's decision on the slice); noted in .mob/records/162/ at its pull.
- Residual risk named: `RequireSupportedSeeds` duplicates the `KnownIdSeed` logic and could drift from it; no test ties the two together (to be taken by the next card that touches seeding).
