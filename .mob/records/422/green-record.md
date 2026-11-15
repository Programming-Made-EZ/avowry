# Card 422 green record

## git diff --stat

```text
 .../NoPhCodesInTheLiveTree.cs                      |  20 +-
 .../RestoredLicenceChecks.cs                       | 118 +++++-
 .../RestoredPackageLicences.cs                     |   6 -
 .../RestoredPackageLicencesLockedRestore.cs        |  10 +-
 tests/test_restored_package_licences.py            | 423 +++++++++++++++++++--
 5 files changed, 524 insertions(+), 53 deletions(-)
```

## Port notes

- Replaced the check-function block in `tests/test_restored_package_licences.py` with the card 422 reference behavior (switch-off behavior only), including:
  - `figure_failures` and figure parsing/counting rules.
  - `tool_closure` XML project traversal, directory props/targets/packages props traversal, and import handling.
  - XML tool-item reading (`PackageReference`, `GlobalPackageReference`, `PackageDownload`) with Include/Update, list split, and version extraction from attributes or child elements.
  - `props_failures` document-order + condition-aware evaluation with unknown/import handling.
  - `lockfile_commit_skip` and updated `lockfile_commit_failures`.
  - 0-to-3-leading-space row handling for restored-package rows.
  - `classify_licence_text`, `licence_value_failures`, and `restore_outcome` logic aligned to reference behavior.
- Kept every existing test class and every `Test422*` class unchanged.
- Renamed `tests/Avowry.Acceptance.Tests/RestoredPackageLicences422Stubs.cs` to `tests/Avowry.Acceptance.Tests/RestoredLicenceChecks.cs` via `git mv`, and replaced stub throws with real implementations of `RestoredLicenceChecks.LicenceValueFailures` and `RestoredLicenceChecks.RestoreOutcome`, including the reviewed URL table entry for `xunit.abstractions 2.0.3`.
- Updated `ReadRestoredRows` in `tests/Avowry.Acceptance.Tests/RestoredPackageLicencesLockedRestore.cs` to accept 0 to 3 leading spaces.
- Applied guard changes in `tests/Avowry.Acceptance.Tests/NoPhCodesInTheLiveTree.cs`:
  - Removed H6/H7 entries from `History` and from `HistoryPrefixes`.
  - Updated `Every_history_allow_list_entry_still_holds_a_PH_code` and its comment to H1..H5.
  - For files named exactly `packages.lock.json`, blank only the base64 `contentHash` value before scanning.
- Removed Slice 422 pending traits only:
  - `tests/Avowry.Acceptance.Tests/RestoredPackageLicences.cs`
  - `tests/Avowry.Acceptance.Tests/RestoredPackageLicencesLockedRestore.cs`
  - `tests/Avowry.Acceptance.Tests/NoPhCodesInTheLiveTree.cs`
- Reference/probe-only mutant switches were not carried into committed code.
- Deliberate implementation difference from literal reference text:
  - Python `condition_value` treats expressions containing ` and ` or ` or ` as undecidable, per slice rule S1 and row expectations (`S-unknown-and`), instead of attempting a partial parse.

## Command summaries

- `python -m unittest tests.test_restored_package_licences` -> `Ran 141 tests` / `OK`.
- `dotnet build Avowry.sln -c Debug` -> `Build succeeded` / `0 Warning(s)` / `0 Error(s)`.
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=422"` -> `Passed` / `62 passed, 0 failed`.
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "FullyQualifiedName~NoPhCodesInTheLiveTree"` -> `Passed` / `47 passed, 0 failed`.
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=369&Network!=yes"` -> `Passed` / `6 passed, 0 failed`.
  - No `Slice=369` test in this run required network; none were skipped for `Network=yes`.

## CLI model line

- No explicit model line was printed by the invoked command outputs in this run.
