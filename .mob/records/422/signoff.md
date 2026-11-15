# Card 422 sign-off, card-422 at 69e4c95175a34055bb191e41ee4eb36d8acbdd7d

VERDICT: SIGN-OFF. Every filter passes on a quiet machine with no failure; the networked F5 and F6 pass and are wired to the new decisions; the guard's History is H1 to H5; the Python module passes in the depth-1 clone (the CI shape); and no tracked file changed after the run. The [O] CI observation on main is pending by design.

| Scenario | Evidence | Met |
|---|---|---|
| [Z] committed page passes, guard names no file | Slice=422 144/144; Slice=369 6/6 incl. networked F5 and F6; NoPhCodesInTheLiveTree 47/47; Status!=Pending 1511/1511; Python 144 OK | met |
| [E] licence value changed, networked test names package and both licences | F6 in clone source calls RestoredLicenceChecks.F6Failures (line 81), whose body calls LicenceValueFailures (line 228); wiring rows and F6 rows pass; networked F6 passed in 3 s | met |
| [M][B] prose figures | Offline figure rows in the Python module and Slice=422 pass (144/144; Python 144 OK) | met |
| [I] tool PackageReference forms | Python tool_closure rows and Slice=422 rows pass; green record and review round 3 | met |
| [S] false-Condition PropertyGroup | Python props_failures rows and Slice=422 rows pass | met |
| [O] locked restore classification, shallow skip | F5 (planted step, line 50) and RestoreLockedOrFail (line 792) call ClassifyRestore, which returns RestoreOutcome; F5 passed in 4 s; depth-1 clone run prints "commit-blob check skipped (shallow checkout)" with Python exit 0 | met, except the CI-on-main observation: pending by design |
| [B] guard skips only contentHash, H6 and H7 removed | History is H1 to H5 (source and Assert.Equal on H1..H5); guard 47/47 | met |

Named limits (not gaps): R-3 (nearer props/targets overriding lock properties), R-5 (MOB.md allow-list entry), review 1 MINOR 4-12, review 3 MINOR 1-3, the NU1004 folder-name case, the two older shallow-unsafe modules; all on #444 or its follow-up.

## Wall times
- Machine quiet (lock free, play-tracker testhost count 0): 2026-11-15T23:28:02Z (waited from about 22:50Z, busy with another seat's lock).
- Gate: from about 23:28Z to 23:47:49Z, about 19.8 min in all. Clone, build and Python are the rest.
- Slice=422: 11 s (test time 8.9 s). Slice=369: 13 s (11.5 s). Guard: 29 s (27.4 s).
- Status!=Pending: 1023 s = 17.05 min (against the old [S] 900 s figure it is over, but card 422 has no time ceiling).
- After the run: lock free, play-tracker testhost count 0.

## Counts (from the filter logs)
- Python, full clone: Ran 144, OK. Depth-1 clone: Ran 144, OK, prints the shallow skip note.
- Build: 0 warnings, 0 errors (dotnet 10.0.404 before and after).
- Slice=422: Total 144, Passed 144, Failed 0.
- Slice=369: Total 6, Passed 6, Failed 0 (F1-F4, F5, F6; none skipped).
- NoPhCodesInTheLiveTree: Total 47, Passed 47, Failed 0.
- Status!=Pending: Total 1511, Passed 1511, Failed 0.
- `git status --short` in the clone after the run: empty.
