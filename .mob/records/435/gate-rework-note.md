# Card 435: the rework gate in a clean clone (2026-11-14, Decider D259 clause 1)

Run by the facilitator alone in a fresh clone (`gate435rework-clone`, branch card-435-rework at 43d79f0: the rework red, its record and the rework green rebased on main 5c33c34; ancestry exit 0), 14:10:38 to 14:26:37 local, with the D251 toolchain stamp (`gate-rework/stamp.txt`, before and after equal: not void). Raw outputs: `gate-rework/` (home paths scrubbed).

- Build: exit 0, 0 warnings, 0 errors.
- `Slice=435`: 112 of 112 (the 77 of the first round and the 35 rework rows, 21 of which failed at the rework red).
- `Criterion=435-1`: 30 of 30. `Criterion=435-2`: 57 of 57. `Criterion=435-3`: 25 of 25 (detailed logger).
- The generator: `inputs=100000 leaks_base=31534 leaks_new=0 regressions=0 second_pass=0 exclusions=0`.
- Affected module (`Avowry.Acceptance.Tests`, `Status!=Pending`): 621 of 621, 14 m 16 s.

LIGHT variant: the full-suite step was SKIPPED because a play-tracker ship-train was alive on the shared laptop (4 ship-train processes at launch). PathScrub.cs is linked only by the two bench programs and its tests are all in the module that ran.
