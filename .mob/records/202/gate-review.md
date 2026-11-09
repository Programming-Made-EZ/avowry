# Card 202 pre-ship gate review (an independent fresh Claude context that wrote none of the card; read-only; scratch single-branch clone gate202-review)

Verdict: SHIP. Tip under review: b11505b (the green a067ef9 on the red d9aa096; the commit before the first production change is d9aa096). Reported by the reviewer, committed in substance by the facilitator.

## Findings
1. D222 (the new rule, first exercise): pass. The red record lists the MULTI Ready row with both ids (`omitted-from-clause`, `no-clause`); the Ready scenarios [M] (two undeclared literals) and [E] (a computed code) are present as `two-undeclared` and `computed`. Nit: the red record tagged those two "slice hostile (not from the Ready row)" although they come from Ready scenarios [M] and [E]: a labelling error, not a missing id; corrected by the facilitator in the red record (a Correction section).
2. Red replay (D157/D154): pass. At d9aa096, with the CLI built first, the test fails by assertion with exactly the 7 failing ids (omitted-from-clause, no-clause, fail-not-first-line, one-of-two-undeclared, two-undeclared, crlf, multi-line-call), each "exit 0; no PHnnn", and none of the 8 keep-building ids; at the tip it passes. Criterion order from `git log`: characterisation `Criterion: 2-3`, red `Criterion: 1`, green `Criterion: 1`. The corpus theory passed 23 of 23 at the red.
3. D137/D204: pass. The test-file diff d9aa096..a067ef9 is exactly the one removed Pending trait line; `git diff --stat` is 7 files (Ast.cs, Parser.cs, Resolver.cs, GRAMMAR.md, the fixture header, the fixture expected.json, the test file), matching slice section 3 with nothing extra; the fixture expects PH337 at line 5 and the corpus theory passes at the tip.
4. Mutations (rebuilt each time, restored): (a) delete the `ReportUndeclaredNativeFails` call: UndeclaredNativeFail and the corpus theory fail; (b) PH338: both fail; (c) remove the lookbehind `(?<![\w.])`: UndeclaredNativeFail fails; (d) remove the `//` comment alternative: UndeclaredNativeFail fails; (e) `seen.Add` replaced by a plain `Contains`: SURVIVES (an undeclared code called twice would give two diagnostics instead of one; declared codes still dedupe through the seed; harmless and arguably not worth a test); (f) line without the body offset: UndeclaredNativeFail fails; (g) column 0: SURVIVES (the known unpinned column, bug card 352).
5. False positives: none. Tasks.avowry, orders.avowry and orders-mutation.avowry exit 0 with identical output file lists and per-file SHAs between the red CLI and the tip CLI. Own sources: `Fail("Neg")` inside an `if` with braces and a ternary: exit 0; a declared Fail inside a lambda: exit 0; `Fail($"X{n}")` and `Fail(c)`: exit 0, not flagged. Positive controls: an undeclared Fail inside a lambda and one inside braces are flagged (5:24 and 7:18); a Fail inside a string or a `//` comment is not.
6. Diagnostic quality: good. Example: `PH337 (5:16) Avow 'reject {n: Int}' raises Fail("X") in its native csharp block on line 5, but X is not in its 'fails with' (declared: Declared). Add X to 'fails with' or remove the call.` no-clause says `(declared: none)` at 5:5; two-undeclared gives two diagnostics (lines 4 and 5); CRLF gives the right line; a body that follows blank lines is right (PH337 at 8:16 for the Fail on line 8). Only the first call site of a repeated undeclared code is reported.
7. Neighbours: no new red. 8 failures at the tip (5 test_rename_196: eol_unchanged x2, lists_equal x2, new_dirs_tracked_old_gone; 2 test_rename_197: lists_equal_baseline x2; T11 in test_live_docs_266): the identical 8 fail at d9aa096.
8. Full gate at the tip: Compiler 324 of 324; Acceptance 340 of 340 in 9 m 29 s; build 0 warnings, 0 errors; the benchmark flake did not occur.
9. Scope: pass. The diff delivers only the PH337 diagnostic; TRACEABILITY.md untouched; the GRAMMAR.md edit is exactly a table row, the code-list clause and one clause with the scan limits (D72 reason in the green commit body). Gaps of the same family that still build (repros in the reviewer's scratch, gate202-scratch): `Fail ("X")` with a space before the parenthesis (the likeliest real shape; filed as bug card 357); `this.Fail("X")` and other qualified calls (excluded by design); `throw new AvowryFailure("X")` (a differently named helper, named in the GRAMMAR.md clause); `Fail("X" + "Y")` and `Fail(@"Z")` (concatenated and verbatim, named in the clause).

## Facilitator's isolated gate run (for the record)
At the green a067ef9: Compiler 324 of 324, Acceptance 340 of 340, 0 warnings (gate202.out), one dotnet process at start and at end.

## Process notes
- First exercise of D222: no HOLD from a dropped half; the gate reviewer's comparison of the red record's list with the Ready draft found every named construct.
- The reviewer's full run was free of the benchmark flake (issue 304).

## Gate evidence
Tested commit b11505b2e2133ca19147ae63cfb246f21baf1747. Copy: `git clone --no-hardlinks --single-branch -b main` from C:/Users/<user>/projects/phrase-lang into gate202-review, not an archive. `dotnet build src/Avowry.Cli`: 0 warnings, 0 errors.

## Load
One dotnet.exe running at the start (PID 41076, a leftover, not the reviewer's); nothing else of the reviewer's was running; dotnet commands one at a time.
