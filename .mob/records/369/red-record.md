# Card 369 red (D157 Criteria 369-1, 369-2, 369-3), RED writing seat

One red commit's worth of tests, left uncommitted for the facilitator. No production file changed: `docs/release/pre-publication-checks.md`, `reference/Directory.Build.props`, lockfiles, and provider/tool projects are untouched. Base: `8b66340ba5a20f14594b141829d45c750d21e02e`. No `dotnet restore` of the reference projects was run.

## D222 list

| id | construct | kind |
|---|---|---|
| L1 | both lockfiles exist, parse, and have `dependencies["net8.0"]` | NEW-STATE (369-1) |
| L2 | union of Direct and Transitive (excluding Project) matches page rows; version conflict | NEW-STATE (369-1) |
| L3 | Direct in either lock => kind Direct; where-used from which lock | NEW-STATE (369-1) |
| L4 | every row has licence, one of the three licence-source labels, and where-used | NEW-STATE (369-1) |
| L5 | section names both lockfile paths, `dotnet restore`, `--locked-mode`, and a 40-hex commit whose blobs match | NEW-STATE (369-1) |
| L6 | `reference/Directory.Build.props` sets RestorePackagesWithLockFile and RestoreLockedMode true | NEW-STATE (369-1 / 369-3) |
| Z1 | three tool projects have no PackageReference; page has "The Avowry tool restores no packages" | NEW-STATE (369-2) |
| B1 | non-expression rows, and System.Reflection.Metadata 1.6.0 and xunit.abstractions 2.0.3, named with a recommendation | NEW-STATE (369-2) |
| B2 | acceptance bullet names Microsoft.CodeCoverage 17.11.1 with a recommendation | NEW-STATE (369-2) |
| B3 | rows outside the allowed list carry `needs review`, otherwise the none line | NEW-STATE (369-2) |
| B4 | exactly one `Recommendation:` in the new section; Package licences no longer says "not reviewed" and points here | NEW-STATE (369-2) |
| E1 | planted: lockfile package missing from the page | NEW-STATE unit (born green) |
| E2 | planted: lockfile package listed at another version | NEW-STATE unit (born green) |
| E3 | planted: page row in neither lockfile | NEW-STATE unit (born green) |
| E4 | Orders.Api / Orders.Specs Direct sets equal the provider's ApiProject / SpecsProject packages | NEW-STATE (369-3); planted sibling born green |
| E5 | planted PackageReference in a tool project is reported (clean/dirty pair) | NEW-STATE unit (born green) |
| F1 | C# wrapper, Slice 369 Criterion 369-1: L1 to L6 | NEW-STATE Pending until green 2 |
| F2 | C# wrapper, Slice 369 Criterion 369-2: Z1, B1 to B4 | NEW-STATE Pending until green 2 |
| F3 | C# wrapper, Slice 369 Criterion 369-3: E4, E5, L1, L6 | NEW-STATE Pending until green 1 |
| F4 | C# wrapper, Slice 369 Criterion 369-3: E1 to E3 planted plus live L2 | NEW-STATE Pending until green 2 |
| F5 | C# networked locked restore equals lockfiles; planted mismatch is NU1004 | NEW-STATE Pending until green 1 |
| F6 | C# networked restore set equals page rows; nuspec, licence files, acceptance set | NEW-STATE Pending until green 2 |
| 186 | `Pre_publication_findings_match_the_repo` | CHANGED: `[Trait("Status","Pending")]` until green 2 |
| 186-live | `PrePublicationChecksTests.test_pre_publication_findings_match_the_repo` | CHANGED: heading missing + still says not reviewed |
| 186-self | `PrePublicationSelfTests` (COMPLIANT, BLOCKED, HEADINGS, "not reviewed" inverted) | CHANGED fixtures; stay green |

Born green by construction (planted fixtures; declared as such, as card 371): E1, E2, E3, E4 planted, E5 pair, B3 planted inside/outside/flagged, and `test_passing_fixture_gives_no_failures`. The tool-project half of Z1 is already true on the tree (no PackageReference); the live Z1 id still fails on the missing page phrase.

## Files

- new `tests/test_restored_package_licences.py` (class `RestoredLicencesOffline`, `collect_failures(page, locks, provider_text, tool_texts)`)
- edited `tests/test_pre_publication_checks.py` (`HEADINGS` and `BLOCKED` gain `restored-package-licences`; package-licences body must not contain "not reviewed" and must contain "Restored package licences"; `COMPLIANT` updated)
- new `tests/Avowry.Acceptance.Tests/RestoredPackageLicences.cs` (F1 to F4; `[Collection("Bench processes")]`)
- new `tests/Avowry.Acceptance.Tests/RestoredPackageLicencesLockedRestore.cs` (F5, F6; same collection)
- edited `tests/Avowry.Acceptance.Tests/PrePublicationChecks.cs` (Pending trait and Given/When/Then comments; still uses `Process.Start` so the Slice 214 allow-list guard stays green)

## Commands and real output

Working directory: repository root of this worktree. Failure excerpts are the assertion text; paths in stack traces are omitted here.

### 1. `python -m unittest tests.test_restored_package_licences.RestoredLicencesOffline`

`Ran 17 tests in 0.020s` `FAILED (failures=12)`

Live ids (fail):

| id | excerpt |
|---|---|
| L1 | `restored-package-licences: Orders.Api -: lockfile missing` / `Orders.Specs -: lockfile missing` |
| L2 | same two lockfile-missing lines (union is not compared against an empty page) |
| L3 | same two lockfile-missing lines |
| L4 | `restored-package-licences: section -: missing` |
| L5 | `restored-package-licences: section -: missing` |
| L6 | `RestorePackagesWithLockFile -: not true` / `RestoreLockedMode -: not true` |
| Z1 | `Avowry tool -: page missing "The Avowry tool restores no packages"` |
| B1 | `section -: missing` |
| B2 | `section -: missing` |
| B3 live | `section -: missing` |
| B4 | `section -: missing`; `expected exactly one Recommendation line, found 0`; `Package licences -: still says not reviewed`; `does not point to Restored package licences` |
| E4 live | same two lockfile-missing lines |

Planted ids (pass, born green): E1, E2, E3, E4 planted, E5, B3 planted, passing fixture. Dot pattern mixed with the 12 failures; isolated rerun of the five planted methods: `Ran 5 tests in 0.002s` `OK`.

### 2. `python -m unittest tests.test_pre_publication_checks.PrePublicationChecksTests`

`Ran 1 test in 28.342s` `FAILED (failures=1)`

```
package-licences: still says not reviewed
restored-package-licences: heading missing
```

Not an import error. `python -m unittest tests.test_pre_publication_checks.PrePublicationSelfTests`: `Ran 5 tests in 1.268s` `OK`.

### 3. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Slice=369"`

Built. `Failed!  - Failed: 6, Passed: 0, Skipped: 0, Total: 6, Duration: 815 ms`

| fact | excerpt |
|---|---|
| F1 | L1 lockfile missing; L4/L5 `section -: missing`; L6 props not true (all six L ids fail) |
| F2 | Z1 page missing the tool phrase; B1–B3 `section -: missing`; B4 still says not reviewed |
| F3 | E5 planted `.` then E4/L1 lockfile missing and L6 props not true (`F.FF`, 3 of 4 python methods fail) |
| F4 | planted E1–E3 `.` then live L2 lockfile missing (`.F`) |
| F5 | `lockfile missing: reference/csharp/Orders.Api/packages.lock.json` at the first assertion, duration `< 1 ms` |
| F6 | same first-assertion message, duration `2 ms` |

F5 and F6 did not start a restore.

### 4. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Slice=186"`

`Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 18 s`

Python inside the wrapper: `F.....` `Ran 6 tests` `FAILED (failures=1)` — the five self-tests pass; the live test fails with `package-licences: still says not reviewed` and `restored-package-licences: heading missing`.

### 5. `dotnet test Avowry.sln --filter "Status!=Pending"`

After restoring `Process.Start` on the 186 wrapper (see below): `Passed!  - Failed: 0, Passed: 344` (`Avowry.Compiler.Tests`) and `Passed!  - Failed: 0, Passed: 401, Duration: 9 m 20 s` (`Avowry.Acceptance.Tests`).

A first run of this command failed on `GeneratedApiSourceScan.Guard_checked_set_of_process_start_callers` because the 186 wrapper had been switched to `ProcessRunner` and dropped off the allow-list. That guard was not in this card's edit list. The wrapper was put back on `Process.Start` (Pending trait kept). The new 369 classes use `ProcessRunner.RunAsync`. Re-run of the guard alone: `Passed! Total: 1`. Then the full `Status!=Pending` filter passed as above.

## Notes for green

- Green 1 un-pends F3 and F5 (lockfiles and props). L1, L6, E4 live go green; page ids stay red.
- Green 2 un-pends F1, F2, F4, F6 and the 186 wrapper (the page section and the Package licences wording).
- Networked facts fail today before restore; once lockfiles exist they run `dotnet restore --locked-mode` in a temp copy of `reference/csharp` (5 minute limit, `NETWORK MISSING` / `restore exceeded 5 minutes`).
