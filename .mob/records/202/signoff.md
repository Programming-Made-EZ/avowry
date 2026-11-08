# Card 202 sign-off (a fresh Claude context, separate from the gate reviewer; read-only; own scratch single-branch clone signoff202-clone; it read the gate review only after its own reruns)

Verdict: SIGNED OFF WITH CONDITIONS. Criteria 1 to 3 all hold on its own reruns. The conditions are residual gaps and the unpinned column, each tracked or documented.

## Findings
1. The three criteria, built with the CLI from its clone's tip into a scratch directory: a leaf with `fails with Declared` and `Fail("X")` exits 1 with `PH337 (5:16)`, the line of the call, X in the message ("declared: Declared"); the corpus fixture `undeclared-native-failure` exits 1 with `PH337 (5:5)` and "declared: none" (its Fail is on line 5, matching the card); a leaf that declares the code builds with exit 0 (a standalone leaf's map has only the avow record, since failure records come from endpoints; the map-record promise rests on `SourceMapTraceability`, which passed in the full run; no endpoint case was built by the sign-off itself); Tasks.avowry and the orders fixture build with exit 0 (the orders output hash check is in `WebExtensionBoundary`, passed in the full run).
   Promise mapping: "reported at build time" is criterion 1; "cannot reach an endpoint undeclared" follows from the build failing; "source-map record" is criteria 2 (declared codes still record) and 3 (the hash); computed codes are the card's explicit not-in-card item. Nothing the card promises goes unchecked.
   Own shapes of an undeclared Fail in a leaf's native csharp body. Caught: `Fail("X")` inside a nested interpolated string followed by a call (PH337 at 5:47); `Fail("X")` with `fails with` omitted. Still builds, documented in GRAMMAR.md: `Fail($"X")`, `Fail(@"X")`, `Fail("X" + "Y")` and `const c; Fail(c)`; `AvowryFailure.Raise("X")`. Still builds, not documented: `Fail ("X")` with a space before the parenthesis (bug card 357); `Action<string> f = Fail; f("X")`, an indirect call (added to bug 357 as a comment).
2. Red replay: at d9aa096, with the CLI rebuilt, `Undeclared_native_fail_reports_a_diagnostic` fails by assertion with exactly the 7 ids (omitted-from-clause, no-clause, fail-not-first-line, one-of-two-undeclared, two-undeclared, crlf, multi-line-call); the other 37 tests passed in the same run, including the characterisations and the corpus theory. At the tip the test passes inside the full gate. The test file differs d9aa096..a067ef9 only by the removed Pending trait line. `git log` Criterion order is 2-3, then 1 (red), then 1 (green).
3. Full gate at the tip: Compiler 324/324 and Acceptance 340/340 (9 m 7 s), 0 warnings and 0 errors; the benchmark flake did not occur.
4. Neighbours: eight failures, identical at d9aa096 and at the tip, all on the allowed list: test_rename_196 (eol_unchanged x2, lists_equal x2, new_dirs_tracked_old_gone), test_rename_197 (lists_equal_baseline x2), T11 in test_live_docs_266. No new failure.
5. Scope and value: the diff d9aa096..a067ef9 is 7 files, +37/-9, and only adds the PH337 diagnostic (the resolver scan is 30 lines; `NativeBlock` gains `BodyAt`; the fixture header and expected.json move to PH337 at line 5; the GRAMMAR.md edit is a PH337 row, the code-list wording and one clause with the scan limits, the card 189 precedent, hashes left for card 162). After a067ef9 the tip adds only records and MOB.md.

## Gate evidence
Tested commit dc2966ca8e4de4f1090d156fd537513a99ecc51c (main tip). Copy: `git clone --no-hardlinks --single-branch -b main` into C:/Users/ringe/projects/signoff202-clone, not an archive. `dotnet build src/Avowry.Cli`: 0 warnings, 0 errors.

## Load
One dotnet.exe at the start (PID 41076, a leftover, not the sign-off's). One dotnet command at a time. One dotnet.exe at the end.

## Independence
The sign-off read gate-review.md only after its reruns; they agree on the 7 ids, the 324/340 totals, the identical 8 neighbour failures and the scope. It additionally found the undocumented method-group gap and the standalone-leaf map note.

## Conditions (non-blocking)
- Bug 357 (`Fail ("X")` with a space) stays open; it is the likeliest real shape and GRAMMAR.md does not list it. The method-group gap is a comment on it.
- Bug 352 (the diagnostic column) stays open; a wrong column would pass.
- Benchmark risk, not run: an LLM-written leaf with a stray undeclared Fail now gets PH337, so benchmark scores may shift (new baselines are card 201's job).
- Only the first csharp native block of an avow is scanned, and only the first call site per undeclared code is reported (not exercised by the sign-off).
