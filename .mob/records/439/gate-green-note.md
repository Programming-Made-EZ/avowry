# Card 439: the green gate in a clean clone (2026-11-15)

Run by the facilitator alone in a fresh clone (`gate439green-clone`, branch card-439 at 8ea2668), 18:14:45 to 18:33:31 local, with the D251 stamp (`gate-green/stamp.txt`, before and after equal). Raw outputs: `gate-green/` (home paths scrubbed).

- Build: exit 0, 0 warnings, 0 errors.
- `Slice=439`: 163 of 163 (the 81 rows that failed at the red now pass; the 82 characterisation rows still pass); the two sweep lines are in `filter-Slice-439-.txt` and `green-record.md`.
- `Slice=435|Slice=426|Slice=418`: 186 of 186.
- The affected module (`Avowry.Acceptance.Tests`, `Status!=Pending`): 784 of 784.

LIGHT: the full suite was skipped while a play-tracker train was alive (4 ship-train processes at launch); PathScrub.cs is linked only by the two bench tools, whose tests are all in the module that ran.
