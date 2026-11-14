# Card 435: the red reproduced in a clean clone (2026-11-14, at the resume)

Run by the facilitator alone in a fresh clone (`gate435red-clone`, from main at d1febe0, which holds the red commit 359114e:
ancestry exit 0), 18:22:58 to 18:25:28 UTC, with the D251 toolchain stamp (`gate-red/stamp.txt`, before and after equal).
Raw outputs: `gate-red/` (summary.txt, build.txt, filter-*.txt).

- Build: exit 0, 0 warnings, 0 errors.
- `Slice=435`: 34 failed, 43 passed, 77 total. The 34 are the Pending tests (435-1: 7, 435-2: 21, 435-3: 6, the five rows
  and the generator fact); the 43 are the characterisation tests, all passing.
- Every failure is by assertion: 33 `Assert.Equal() Failure: Strings differ` (PathScrubForms.AssertNonAsciiFolder 21,
  AssertMissedRoot 12) and the generator fact, whose message is its measurement: `inputs=100000 leaks_base=31534
  leaks_new=31534 regressions=0 second_pass=0 exclusions=0` (the scrub is unchanged, so the new leaks equal the base).
- `Criterion=435-1`: 7 failed, 12 passed. `Criterion=435-2`: 21 failed, 28 passed. `Criterion=435-3`: 6 failed, 3 passed.

LIGHT variant: the full-suite gate step was SKIPPED, because a play-tracker ship-train was alive on the shared laptop (the
owner's rule at the resume: affected-module runs only while one is). The full gate runs at the green when no train is alive.
The red matches the seat's own run (`red-record.md`), so the green may start: the Cursor writing seat (`green-prompt.md`).
