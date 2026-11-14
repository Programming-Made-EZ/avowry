The green works: all 77 Slice 435 tests pass with no exclusions, and the full `Status!=Pending` run passed on its own. The changes are left uncommitted for you.

**What changed**
- `tools/Avowry.Bench/PathScrub.cs`: design parts A to H applied exactly as section 2 of the slice reads, including the header comment. The extra `\\d+` token was not added (ruling 6).
- In `PathScrubForms.cs` and `PathScrubGenerated.cs`, I removed `[Trait("Status", "Pending")]` and its "Pending until…" comment from the four tests that were Pending. Nothing else in the tests changed.
- The base copy and both `Program.cs` files have an empty diff. `Exclusions` is still empty and `PinnedExclusionCount` is still 0. All files kept LF line endings.

**Results**
- **Build:** 0 warnings, 0 errors.
- **Slice=435:** 77 of 77 passed.
- **Criteria:** 435-1 passed 19 of 19, 435-2 passed 49 of 49, and 435-3 passed 9 of 9.
- **Generator counts line:** `inputs=100000 leaks_base=31534 leaks_new=0 regressions=0 second_pass=0 exclusions=0`.
- **`PathScrubForms` class:** 136 of 136 passed.
- **`Status!=Pending`, run alone:** compiler tests passed 344 of 344. Acceptance tests passed 586 of 586 in 18 minutes 3 seconds, against 11 minutes 47 seconds at the red. 586 is the red's 552 plus the 34 tests that were Pending.
- **`python scripts/home_paths.py --root .`:** `0 files`. A scan that reads the account name from the environment found 0 hits in the four touched files.

Every row the red marked Pending is now green.

**Findings for you (no test changed)**
1. **Mutant 2 now survives.** This is narrowing `/[a-z]/` to `/c/` in the Unix home root. With it applied to the green code, all 107 Slice 435 and 426 tests still pass. The cause is part E: `/d/Users/<user>` now also matches as the prefix folder `/d` plus the root `/Users/`. The mutant still changes behaviour. Going by the pattern (I didn't run this), a lowercase `/d/users/<user>` would leak under it, and no row or generator root has that shape. A row built as `"/d/" + "users/" + user` that expects `<path>` would catch it again. I restored the file, confirmed it matches the green byte for byte, rebuilt, and re-ran the slice: 77 of 77 passed.
2. **Mutant 7's base text is gone.** Part D widens it to `(?<home>(?i:users|home))`. Going by the pattern, the equivalent "any folder" mutant would still be caught by `\\host\share\x.cs`. I didn't re-run mutants 1, 3, 4, 5 or 6; their text is unchanged from the base.

**Slip:** while checking that the full run had started, one `tail` of the raw test output printed a line holding the worktree's absolute path (and so the home folder) into my tool output. It was not written to any repository file. After that I printed only summary lines.

Everything above, with the output excerpts, is in `.mob/records/435/green-record.md`.That notification is for the full `Status!=Pending` run, which I already reported. It exited 0: 344 of 344 compiler tests and 586 of 586 acceptance tests passed. That result is in `.mob/records/435/green-record.md`, and nothing else needs doing.

--- worktree after the seat ---
M tests/Avowry.Acceptance.Tests/PathScrubForms.cs
 M tests/Avowry.Acceptance.Tests/PathScrubGenerated.cs
 M tools/Avowry.Bench/PathScrub.cs
?? .mob/records/435/green-record.md
tests/Avowry.Acceptance.Tests/PathScrubForms.cs    |  6 ----
 .../Avowry.Acceptance.Tests/PathScrubGenerated.cs  |  2 --
 tools/Avowry.Bench/PathScrub.cs                    | 34 +++++++++++++---------
 3 files changed, 21 insertions(+), 21 deletions(-)
