# Card 435: the green gate in a clean clone (2026-11-14)

Run by the facilitator alone in a fresh clone (`gate435green-clone`, branch card-435-green at 547538f, the green rebased on the resume commit bca4d2d: ancestry exit 0), 11:32:48 to 11:49:53 local, with the D251 toolchain stamp (`gate-green/stamp.txt`, before and after equal: not void). Raw outputs: `gate-green/` (home paths scrubbed).

- Build: exit 0, 0 warnings, 0 errors.
- `Slice=435`: 77 passed of 77 (the 34 that failed at the red now pass; the 43 characterisation tests still pass).
- `Criterion=435-1`: 19 of 19. `Criterion=435-2`: 49 of 49. `Criterion=435-3`: 9 of 9 (detailed logger).
- The generator fact's measurement: `inputs=100000 leaks_base=31534 leaks_new=0 regressions=0 second_pass=0 exclusions=0`.
- Affected module (`Avowry.Acceptance.Tests`, `Status!=Pending`): 586 of 586, 14 m 45 s.

LIGHT variant: the full-suite step (`Avowry.sln`, `Status!=Pending`) was SKIPPED, because a play-tracker ship-train was alive on the shared laptop (the owner's rule at the resume). The affected module ran instead: PathScrub.cs is linked only by Avowry.Bench and Avowry.BenchCs, and its tests are in Avowry.Acceptance.Tests. The writing seat ran the full suite itself during the train (344 of 344 compiler, 586 of 586 acceptance; `green-record.md`), against the rule; that is a retro finding, and the full gate runs before the close if a train-free window opens, else the close records the light gate.

Git audit (`audit-green.txt`, base 2a5be77, HEAD ca84f2d): the scrub file's diff is the only change among PathScrub.cs and the two bench programs; the account name appears 0 times over the tracked tree and 0 over the staged diff.
