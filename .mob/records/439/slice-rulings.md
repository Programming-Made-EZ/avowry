# Card 439: the facilitator's rulings on the architect's open points (2026-11-15)

The slice: slice-architect.md (architect: claude opus = claude-opus-5-5, ctx-1, read-only; measured in its own probe outside the repositories; 15,342 bytes). Its measured summary: all 239 data-set rows (418, 426, 435, both forms) equal today's output with stable second passes; the 200 review inputs leak 2 (one input, 2.5n) against 83 today and 102 before card 435, with 0 second-pass changes; 45 of 45 mutants killed by named rows; generator sweeps A, B and C with 0 new leaks and 0 second-pass changes; every blow-up shape under 1.3 ms at 20,000 characters and under 70 ms at 1,000,000 (today: 2,005 ms on 20,000 characters of the doubled-backslash drive shape).

1. Account folders at any depth in UNC and rooted Unix paths, the first folder only on a drive path: ACCEPTED (today's behaviour, criterion-consistent).
2. Sweeps B and C: the red asserts 0 new leaks against PathScrubBefore435 and 0 second-pass changes, with the absolute leak counts pinned and printed (D256: a count is pinned and never hidden); no exclusion is committed, because the new-leak count is 0.
3. 3.5e (`<path>-x.cs`): KEPT; criterion 2 treats a glued `(1)` as part of the account folder and the third reviewer's own mutant M-R8 expects that output.
4. 3.4d (`foo-~
x.cs` now left as is): ACCEPTED; it leaks nothing and is stable on a second pass.
5. 3.8a to 3.8d (a bridged first path's short name becomes `<path>`): ACCEPTED under D259 clause 4 (an over-scrub that never leaks, on contrived inputs); named as a limit with 2.5n.

The red: a claude sonnet seat (D261, ctx-2) in a linked worktree writes the rows the slice names (the review inputs with their ids, the mutant-killing rows, sweeps B and C), Pending until the green, with the Pending trait convention of 2a5be77 in its brief. The mutants were run once by the architect against its reference scanner (45 of 45 killed); the red proves each killing row fails today where the slice says so.

## Rulings on the fix slice (round 1 of the review, 2026-11-15)

The fix slice: slice-fix.md (the architect, ctx-1, in its own probe). Measured: 239 of 239 data rows equal today; 48 of 48 mutants killed (45 plus 3 new); sweeps A, B and C 0 new leaks and 0 second-pass changes (A: 0 differences from the replaced regex; C: leaks 2,904 to 2,679); every blow-up shape linear, `/=-I` and `/(-I` 1.7 and 1.1 ms at 20,000 characters (1,435 and 1,328 ms before the fix); the mixed-slash drive leak fixed; review findings 2 and 3 do not occur in the reference (they come from the green).

1. The walk fix (finding 1) changes no output, so no mutant of it can be killed by a row. Its evidence is the linear-time rows D261 clause 3 names, measured at the slice and re-measured at the review (D254: no wall-clock assertion); no step counter is added to production code for a test.
2. `/home/<user>\notes\run-426` becomes `<path>\notes\run-426`: KEPT (the replaced regex's output; neither leaks).
3. Finding 9b (a contrived chain after an account tail): KEPT as a named limit, MINOR, with 2.5n, 3.4d and 3.8a-d.
4. The green PORTS the fixed reference (probe439/PathScan.cs without its probe-only mutant switches) instead of reimplementing it: findings 2, 3, 5, 7, 8 and 10 came from the green departing from the design.
