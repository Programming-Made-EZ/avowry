# Card 422: the red record

Seat: red (task-force role dev, red; claude sonnet = claude-sonnet-5-5, Decider D268, context ctx-2). Worktree `wt-422`, branch card-422, HEAD cbd6e0f, nothing committed. The check code is untouched: the diff is additive (4 files changed, 1 new file).

## What was written

- `tests/test_restored_package_licences.py`: a test-support block and two classes after the existing check code, `Test422Characterisation` (29 row tests and the completeness test) and `Test422Pending` (94 row tests), named `test_422_<row id>`.
- `tests/Avowry.Acceptance.Tests/RestoredPackageLicences.cs`: a constructor taking `ITestOutputHelper`, the helper `AssertPython422` (writes the Python output to the test log, ruling 4), and 9 Facts that run the Python rows per criterion and per class (F7 to F15; Pending where they run `Test422Pending`).
- `tests/Avowry.Acceptance.Tests/RestoredPackageLicencesLockedRestore.cs`: the offline Theory `Licence_value_rows_422` (12 rows, 422-2) and `Restore_outcome_rows_422` (18 rows, 422-6), with the nuspec facts of the 30 packages as a fixture.
- `tests/Avowry.Acceptance.Tests/NoPhCodesInTheLiveTree.cs`: `PlantedRows422Holding` and `PlantedRows422Pending` (23 guard rows, text built from parts with `Old`, `@real` stands for the real lockfile) and the Theories `Lockfile_planted_rows_that_hold_today` and `Lockfile_planted_rows_pending` (422-7).
- `tests/Avowry.Acceptance.Tests/RestoredPackageLicences422Stubs.cs` (NEW): a STUB. `RestoredLicenceChecks.LicenceValueFailures(IReadOnlyList<string[]> rows, IReadOnlyDictionary<string, PackageEvidence> evidence)` and `RestoredLicenceChecks.RestoreOutcome(int exitCode, bool timedOut, string text, int restoredCount)` throw `NotImplementedException`, and `PackageEvidence(Expression, LicenceUrl, LicenceFiles)` is the evidence record. The green replaces the bodies (or moves the two helpers into the LockedRestore class and deletes this file; then the signatures in the two Theories change with it). The helper names are mine: the slice names the Python functions (`licence_value_failures`, `restore_outcome`) and says the C# helpers are ports.
- Python completeness check `test_422_every_corpus_row_is_embedded` (in `Test422Characterisation`): reads `.mob/records/422/corpus.txt` and asserts every row id appears, quoted, in the Python test file or in the three C# files read as text. It passes today because all 176 rows are embedded.

All 176 corpus rows are embedded: Python 123 (all of page-figure, tools, props; page-licence offline 18; shallow 3), C# 53 (licence 12, restore 18, guard 23).

## How a row is classified

A row asserts the reference's outcome for the check it exercises. A Python page-licence row calls `collect_failures` only, and also `figure_failures` when its expected outcome is a figure outcome (every page-figure row, every row whose expected text says `page says`, Z-page and E-swap-off). Tool rows go through the new `tool_closure` (the overlay needs it), except Z-tools, which uses today's `load_tool_texts()` on the real tree. A row that fails on today's code is in `Test422Pending` (or is a C# row with `[Trait("Status", "Pending")]`); a row that passes is in `Test422Characterisation` (or a C# row without the trait). Python Pending rows fail with an error, not an assertion, only where the slice says the function is new: `figure_failures` (B1), `tool_closure` (I1), `lockfile_commit_skip` (O1).

## Rows by family and criterion

| family | criterion | rows | Python Pending | Python characterisation | C# Pending | C# characterisation |
|---|---|---|---|---|---|---|
| guard | 422-7 | 23 | 0 | 0 | 10 | 13 |
| page-figure | 422-3 | 33 | 33 | 0 | 0 | 0 |
| page-licence | 422-1 | 1 | 1 | 0 | 0 | 0 |
| page-licence | 422-2 | 14 | 2 | 0 | 12 | 0 |
| page-licence | 422-3 | 15 | 3 | 12 | 0 | 0 |
| props | 422-1 | 1 | 0 | 1 | 0 | 0 |
| props | 422-5 | 30 | 15 | 15 | 0 | 0 |
| restore | 422-6 | 21 | 3 | 0 | 18 | 0 |
| tools | 422-1 | 1 | 0 | 1 | 0 | 0 |
| tools | 422-4 | 37 | 37 | 0 | 0 | 0 |
| total | | 176 | 94 | 29 | 40 | 13 |

Criterion map: 422-1 [Z] (Z-page, Z-tools, Z-props, completeness), 422-2 [E] (E-*, Z-licence), 422-3 [M][B] (page-figure, L-*), 422-4 [I] (tools), 422-5 [S] (props), 422-6 [O] (restore, Z-restore, shallow), 422-7 [B guard] (G-*, Z-guard-*).

## Counts

- Python: Pending 94 rows (`Test422Pending`), characterisation 29 rows plus the completeness test = 30 tests (`Test422Characterisation`).
- C#: Pending 40 row tests (licence 12, restore 18, guard 10) plus 6 Python-wrapper Facts that run `Test422Pending` = 46 Pending tests; characterisation 13 guard rows plus 3 Python-wrapper Facts = 16 tests.
- No Slice=422 test is networked, so there is no `Network` trait and no `&Network!=yes` in the filters.
- 20 of the 94 Python Pending rows are rows today's checks would get right (Z-page, E-swap-off, the 7 positive W-* rows, I-E5, I-remove, I-cond-false, I-pkgversion, I-reference-props, I-projref-plain, I-import-sdk, I-framework, and the 3 shallow rows): they fail only because they call a new function (`figure_failures`, `tool_closure`, `lockfile_commit_skip`). Every C# licence and restore row fails by the stub for the same reason, including the rows the old logic agrees with (Z-licence, Z-restore, O-cold and others).

## Runs (cwd = the worktree)

- `python -m unittest tests.test_restored_package_licences.Test422Characterisation`: `Ran 30 tests in 0.087s` / `OK`.
- `python -m unittest tests.test_restored_package_licences.Test422Pending`: `Ran 94 tests in 0.498s` / `FAILED (failures=17, errors=77)` (94 of 94 not ok).
- `python -m unittest tests.test_restored_package_licences`: `Ran 141 tests in 0.857s` / `FAILED (failures=17, errors=77)`: the 17 existing tests (`RestoredLicencesOffline`, run alone: `Ran 17 tests in 0.226s` / `OK`) and the 30 characterisation tests pass, the 94 Pending tests fail, nothing else fails. No test is networked or skipped in this module.
- `dotnet build Avowry.sln -c Debug`: `Build succeeded.` `0 Warning(s)` `0 Error(s)`.
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=422&Status=Pending"`: `Failed!  - Failed:    46, Passed:     0, Skipped:     0, Total:    46`.
- `... --filter "Slice=422&Status!=Pending"`: `Passed!  - Failed:     0, Passed:    16, Skipped:     0, Total:    16`.

Checked against the reference: the architect's `eval422.py` on the corpus gives 176 of 176 rows as expected on `refcheck.py` and `guard.py`. The Python test classes, run on a module made of `refcheck.py` plus the new section, give `Ran 124 tests` / 124 ok (123 rows and the completeness test). The C# rows carry the corpus expectations as emitted by the generator (round-trip tested); they cannot run against the reference, which is Python, so they are checked only through the corpus.

## Failing Pending ids, with the first failure line

### Python, `Test422Pending` (94)

- `NameError: name 'figure_failures' is not defined` (37 rows): Z-page, E-xa-mit-off, E-swap-off, L-gpl-flagged, M-add, B-total, B-api, B-specs, B-direct, B-trans, B-mit, B-apache, B-notice, B-noexpr, B-withexpr, B-acc, B-acc-word, B-group, B-rec-total, B-rec-mit, B-rec-apache, B-acc-named, W-words, W-words-hyphen, W-words-wrong, W-bold, W-bold-wrong, W-code, W-code-wrong, W-wrapped, W-wrapped-wrong, W-inner, W-leading, W-gone, W-dup-wrong, W-notice-unknown, W-url
- L-indent-dup: `assertion: L-indent-dup: expected fail:FastEndpoints 8.3.0: appears more than once, got []`
- L-indent3-dup: `assertion: L-indent3-dup: expected fail:FastEndpoints 8.3.0: appears more than once, got []`
- `NameError: name 'tool_closure' is not defined` (37 rows): I-E5, I-order, I-child, I-child-cdata, I-entity, I-single, I-update, I-update-child, I-list, I-lower, I-multiline, I-inner-ws, I-value-ws, I-tab, I-comment, I-cdata-text, I-remove, I-noversion, I-override, I-cond-false, I-download, I-root-props, I-root-targets, I-src-props, I-global, I-pkgversion, I-reference-props, I-projref-single, I-projref-backslash, I-projref-order, I-projref-plain, I-import, I-import-thisdir, I-import-unknown, I-import-sdk, I-broken, I-framework
- S-false: `assertion: S-false: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true (set only under a false Condition), got []`
- S-false-compare: `assertion: S-false-compare: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true, got []`
- S-false-ne: `assertion: S-false-ne: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true, got []`
- S-false-single: `assertion: S-false-single: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true, got []`
- S-unknown: `assertion: S-unknown: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true (set under a Condition the check cannot decide), got []`
- S-unknown-and: `assertion: S-unknown-and: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true, got []`
- S-elem-false: `assertion: S-elem-false: expected fail:RestoreLockedMode -: not true, got []`
- S-override: `assertion: S-override: expected fail:RestoreLockedMode -: not true (last set to false), got []`
- S-override-unknown: `assertion: S-override-unknown: expected fail:RestoreLockedMode -: not true, got []`
- S-target: `assertion: S-target: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true, got []`
- S-choose: `assertion: S-choose: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true, got []`
- S-import-after: `assertion: S-import-after: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true (a later Import may change it), got []`
- S-comment: `assertion: S-comment: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true (never set), got ['restored-package-licences: RestorePackagesWithLockFile -: not true', 'restored-package-licences: RestoreLockedMode -: not true']`
- S-cond-entity: `assertion: S-cond-entity: expected fail:RestorePackagesWithLockFile -: not true && RestoreLockedMode -: not true, got []`
- S-missing: `assertion: S-missing: expected fail:RestoreLockedMode -: not true (never set), got ['restored-package-licences: RestoreLockedMode -: not true']`
- `NameError: name 'lockfile_commit_skip' is not defined. Did you mean: 'lockfile_commit_failures'?` (3 rows): O-shallow, O-deep, O-shallow-nohash

### C#, by assertion (10 guard rows, `Lockfile_planted_rows_pending`)

- G-requested: `G-requested: expected a hit for reference/csharp/Orders.Api/packages.lock.json, got 0`
- G-hash-wrapped: `G-hash-wrapped: expected a hit for reference/csharp/Orders.Api/packages.lock.json, got 0`
- G-minified-hit: `G-minified-hit: expected a hit for reference/csharp/Orders.Api/packages.lock.json, got 0`
- G-resolved: `G-resolved: expected a hit for reference/csharp/Orders.Api/packages.lock.json, got 0`
- G-key: `G-key: expected a hit for reference/csharp/Orders.Specs/packages.lock.json, got 0`
- G-hash-case: `G-hash-case: expected a hit for reference/csharp/Orders.Api/packages.lock.json, got 0`
- G-bak: `G-bak: expected a hit for reference/csharp/Orders.Api/packages.lock.json.bak, got 0`
- G-other-lock: `G-other-lock: expected no hit for tools/X/packages.lock.json, got 1`
- G-hash-text: `G-hash-text: expected a hit for reference/csharp/Orders.Api/packages.lock.json, got 0`
- G-hash-and-key: `G-hash-and-key: expected a hit for reference/csharp/Orders.Api/packages.lock.json, got 0`

### C#, by the stub's `NotImplementedException` (30 rows)

- `Licence_value_rows_422` (12 rows: Z-licence, E-xa-mit, E-srm-apache, E-swap, E-expr-swap, E-url-unreviewed, E-url-missing, E-file-apache, E-file-none, E-file-wrapped, E-file-leading, E-file-newtonsoft): `System.NotImplementedException : card 422 green: port licence_value_failures from the reference`
- `Restore_outcome_rows_422` (18 rows: Z-restore, O-warm-warning, O-warm-nopkg, O-cold, O-cold-host, O-partial, O-nu1102, O-other, O-timeout, O-crlf, O-leading, O-planted, O-planted-offline, O-planted-lower, O-planted-wrapped, O-planted-nu1102, O-planted-cold, O-planted-nu10041): `System.NotImplementedException : card 422 green: port restore_outcome from the reference`

### C#, the 6 Python-wrapper Facts that run `Test422Pending`

F8 (422-1), F9 (422-2), F11 (422-3), F12 (422-4), F14 (422-5), F15 (422-6): each fails by assertion because the Python run exits non-zero (the Python line for each row is above). Their test log holds the Python output.

## Notes for the facilitator and the green

- The existing test `Every_history_allow_list_entry_still_holds_a_PH_code` still expects H1 to H7 and passes today. When the green removes H6 and H7 from `History` and `HistoryPrefixes` it must also change that test to expect H1 to H5 (the slice says so); I did not touch it.
- Ruling 4 (the Python test skips the commit-blob check with its reason on a shallow checkout) has no corpus row. The wrapper Facts write the Python output to the log, so the skip shows on a passing run. O-shallow, O-deep and O-shallow-nohash test the pure `lockfile_commit_skip(page, shallow)` and expect `restored-package-licences: section -: commit-blob check skipped (shallow checkout)` or `None`.
- Ruling 3 and the R-5 item have no rows, as ruled. The O3 rule (`ReadRestoredRows` taking 0 to 3 leading spaces) has no corpus row either; L-indent-dup and L-indent3-dup cover the Python side.
- The networked parts of [E] and [O] (F5, F6 calling the new helpers) are not changed by this red.
