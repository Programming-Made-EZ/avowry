# Card 422: rulings on fix slice 1 (facilitator, 2026-11-15)

Fix slice 1 is `slice-fix.md`, with neighbour rows in `corpus-fix.txt` (83 rows: `fix-wire` 75, `fix-log` 8; 39 fail on 232be69). It answers rulings R1-1 and R1-2 of review round 1 (`slice-rulings.md`). The seat was the architect (claude opus = claude-opus-5-5, ctx-1b). It changed no existing outcome and killed 27 of 27 mutants, 7 of them wiring mutants.

FX-1: option (a), with a condition.
- The red first makes a behaviour-preserving refactor, kept separate in its record. It copies today's decisions unchanged into the new pure functions (`RestoredPackageFrom`, `F6Failures`, `F6AcceptanceFailures`, `RestoredCount`, `ClassifyRestore`) and rewires F5, F6 and `RestoreLockedOrFail` to call them.
- Every existing test must pass after that step and before any new row is added; the red runs `Slice=422`, `Slice=369&Network!=yes` and the Python module to prove it.
- The 30 rows that then fail do so because today's decision logic is wrong, not because a call is missing.
- The green changes only the two function bodies the slice names and the log wrapper.
- Reason: a green that cannot leave the wiring out, which was round 1's gap. The 7 source-scan rows keep the wiring from being undone later.

FX-2: accepted. The source-scan wiring rows are tests. They read the two C# files as text and assert each call site the slice names.

FX-3: the rows are committed as `corpus-fix.txt`, and `test_422_every_corpus_row_is_embedded` reads both files, as on card 430.

Named limit, no row: a folder or package named exactly `NU1004` in the restore output reads as a mismatch. Filed on #444.

Recorded: the fix architect used one shell heredoc for a quoted Python edit on its own probe file. No repository file was touched and its outputs were measured afterwards, so no record is affected. The heredoc rule protects backslashes, and future briefs restate it.

The networked F5 and F6 run once at the fix gate under the heavy lock, on a quiet machine.
