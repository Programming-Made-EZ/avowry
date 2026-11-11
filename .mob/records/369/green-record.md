# Card 369 green, Cursor writing seat: STOPPED in green 1 on a flaw in F5

Base: `ebb9b261beb3d183db824c49dbbf081724943545` (`git rev-parse HEAD`, nothing committed by this seat). All changes are left uncommitted. SDK `dotnet --version`: 10.0.403. Paths in output excerpts below are written `<home>` and `<temp>`.

## Outcome

- Green 1 is done except F5. The lockfiles exist, the props are set, the closure is the expected 30 packages (16 + 14), and all four locked restores pass. F3 is green.
- F5 does NOT go green, because of a flaw in a test this seat was not asked to change. The brief says stop and report, so this seat stopped. **Green 2 (the page) was NOT started.** F1, F2, F4, F6 and the Slice 186 wrapper keep their Pending traits.
- F5's Pending trait was removed and then put back unchanged (net diff on `RestoredPackageLicencesLockedRestore.cs` is zero), so that `Status!=Pending` does not go red on a known test flaw.

## The flaw (F5, `PlantLockfileMismatch` and the `NU1004` assertion)

F5 removes `FluentValidation` (a Transitive entry) from the copied Orders.Api lockfile and asserts a non-zero exit **and** `NU1004`. On SDK 10.0.403 the exit is non-zero, but the code is `NU1102`, not `NU1004`. NuGet checks the lockfile's Direct entries and project references for consistency (the `NU1004` check). A missing Transitive entry passes that check, and resolution then fails because the locked set has no FluentValidation.

The F5 run (`dotnet test ... --filter "FullyQualifiedName~RestoredPackageLicencesLockedRestore"` with F5 un-pended):

```
Failed ...F5_locked_restore_matches_the_lockfiles_and_a_planted_mismatch_is_NU1004 [4 s]
  Error Message:
   Assert.Contains() Failure: Sub-string not found
String:    "  Determining projects to restore...\r\n<ho"···
Not found: "NU1004"
  ... RestoredPackageLicencesLockedRestore.cs:line 54
```

Line 54 (one line up from the red version, after the trait was removed) is `Assert.Contains("NU1004", ...)`. So the first half of F5 (the locked restore of both projects, and the restored set equal to the union of the lockfiles) and the non-zero exit assertion passed.

Reproduced by hand in a scratch copy (only the `.cs`, `.csproj` and lockfile of each project copied; FluentValidation removed from Orders.Api's lockfile), `dotnet restore <copy>/Orders.Api/Orders.Api.csproj --locked-mode --nologo --disable-build-servers`:

```
exit: 1
<temp>\avowry-369-green-drift-1794356073\Orders.Api\Orders.Api.csproj : error NU1102: Unable to find package FluentValidation with version (>= 12.1.1)
... error NU1102:   - Found 192 version(s) in nuget.org [ Nearest version: 12.1.1 ]
```

For comparison, a Direct-entry mismatch in the same copy (lockfile `FastEndpoints.requested` changed to `[8.2.0, )`) does give NU1004:

```
exit: 1
... error NU1004: The package reference FastEndpoints version has changed from [8.2.0, ) to [8.3.0, ).The packages lock file is inconsistent with the project dependencies so restore can't be run in locked mode. ...
```

For the facilitator to decide (this seat changed no test logic): plant the drift on a Direct entry (its `requested` or `resolved`) and keep `NU1004`, or keep the FluentValidation removal and accept `NU1102`. The slice (section 2, "removes one entry, for example FluentValidation ... asserts ... NU1004") carries the same assumption.

## Green 1: commands and real output

Run from the repository root of this worktree.

1. `reference/Directory.Build.props`: comment kept; added a `PropertyGroup` with `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>` and `<RestoreLockedMode>true</RestoreLockedMode>`. LF kept.

2. Creating restore, with `NUGET_PACKAGES` set to a new empty folder built from `$TEMP` (folder name `avowry-369-green-nuget-1794356004`, 0 entries before):
   `dotnet restore reference/csharp/Orders.Specs/Orders.Specs.csproj -p:RestoreLockedMode=false --nologo`, exit 0. The restore log was lost to a broken output filter in this seat's shell line. The exit code, both lockfiles, and the checks below are the evidence. Both `packages.lock.json` files were created by this one command, so no separate Orders.Api restore was needed. Afterwards: `obj/project.assets.json` of Orders.Specs has 1 `packageFolders` entry, equal to the scratch folder. The scratch folder holds 34 entries: the 30 packages, plus `microsoft.aspnetcore.app.ref`, `microsoft.netcore.app.ref`, `microsoft.netcore.app.host.win-x64` and `microsoft.windowsdesktop.app.ref` (SDK packs, out of scope).

3. Lockfiles as written: `tfms: ['net8.0']`. Orders.Api: `{'Direct': 1, 'Transitive': 15}` (135 CRLF). Orders.Specs: `{'Direct': 3, 'Transitive': 27, 'Project': 1}` (239 CRLF). Union: 30 distinct ids, each at one version, no version conflict. Orders.Api has 16 and Orders.Specs adds 14. **Counts are 30 = 16 + 14, as the slice expected; no figure needs recomputing.** The ids and versions are exactly the slice section 1 list (FastEndpoints and its five 8.3.0 packages, FluentValidation 12.1.1, the eight Microsoft.Extensions.* 10.0.11 and System.Diagnostics.DiagnosticSource 10.0.11; the test closure of 14 as listed). Direct: FastEndpoints (Orders.Api), Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio (Orders.Specs). No local path in either lockfile (`rg` found none).

4. Normalised both lockfiles to LF (bytes rewritten, CRLF to LF): `Orders.Api crlf after: 0 ... bytes: 5732`, `Orders.Specs crlf after: 0 ... bytes: 9745`. NuGet writes no final newline, and none was added. They stayed LF after the locked restores below (136 and 240 lines, 0 CRLF).

5. Locked restores, all exit 0:
   - With the scratch `NUGET_PACKAGES`: `dotnet restore reference/csharp/Orders.Api/Orders.Api.csproj --locked-mode --nologo` gave `Restored ...Orders.Api.csproj (in 286 ms)`. `dotnet restore reference/csharp/Orders.Specs/Orders.Specs.csproj --locked-mode --nologo` gave `Restored ...Orders.Specs.csproj (in 274 ms)` and `1 of 2 projects are up-to-date for restore`.
   - With the default package folder (`NUGET_PACKAGES` unset, `--force` added): Orders.Api `(in 307 ms)`, Orders.Specs `(in 265 ms)`, exit 0 each.

6. `.gitattributes` existed (LF). Appended the line `reference/csharp/*/packages.lock.json text eol=lf`.

7. Pending removed from F3 (`RestoredPackageLicences.cs`, the trait line with its "green 1 ... removes this Pending trait" comment). For F5 it was removed and then restored (see above).

8. `python -m unittest tests.test_restored_package_licences.RestoredLicencesOffline`: `Ran 17 tests in 0.018s`, `FAILED (failures=9)` (red had 12). L1, L6 and E4 live now pass. Still failing, all on the page: L2 (lockfile packages `missing from the page`), L3, L4, L5, B1, B2, B3 (`section -: missing`), B4 (`section -: missing`, `expected exactly one Recommendation line, found 0`, `Package licences -: still says not reviewed`, `does not point to Restored package licences`), Z1 (`page missing "The Avowry tool restores no packages"`).

9. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "FullyQualifiedName~RestoredPackageLicencesLockedRestore"` (F5 un-pended at the time): `Failed!  - Failed: 2, Passed: 0, Skipped: 0, Total: 2, Duration: 7 s`. F6: `section missing: Restored package licences` (expected red). F5: the NU1004 flaw above.

10. `dotnet test ... --filter "FullyQualifiedName~F3_lockfiles_match_the_provider"`: `Passed!  - Failed: 0, Passed: 1, Skipped: 0, Total: 1, Duration: 277 ms`.

11. `python scripts/check-no-personal-names.py`: exit 0. It printed only its usual `archive (regenerated from issues)` findings under `board/`, which do not change the exit code.

## Not run

- Green 2 was not started: no page edit, no Pending removed from F1, F2, F4, F6 or the 186 wrapper, no licence reading for the page, and no fetch of the xunit.abstractions licence URL.
- Not run: `python -m unittest tests.test_restored_package_licences tests.test_pre_publication_checks tests.test_pre_publication_owner_items`, the `Slice=369|Slice=186|Slice=371` filter, and the full suite (left to the facilitator).

## Files changed (uncommitted)

- `reference/Directory.Build.props` (modified, LF)
- `.gitattributes` (one line appended, LF)
- `reference/csharp/Orders.Api/packages.lock.json` (new, LF, 16 packages)
- `reference/csharp/Orders.Specs/packages.lock.json` (new, LF, 30 packages plus the Orders.Api project entry)
- `tests/Avowry.Acceptance.Tests/RestoredPackageLicences.cs` (F3 Pending trait removed)

## Scratch left in place (inside the system temp directory; no recursive delete was run)

- `avowry-369-green-nuget-1794356004` (the fresh package folder, 34 entries)
- `avowry-369-green-drift-1794356073` (the hand-made drift copy, with `restore.log`, `restore2.log` and the projects' `obj` folders)
- `avowry-369-restore.log`, `avowry-369-green1-test.log` (log files)
- The restores also wrote git-ignored `obj` folders under `reference/csharp/Orders.Api` and `Orders.Specs` in this worktree.

# Card 369 green 2 (the page), Cursor writing seat, new worktree

Done; all changes left uncommitted. `git rev-parse HEAD` in this worktree: `48fabf37e90e403916e739692d990f9f360380db`. That is NOT green 1's commit: it is the facilitator's "Card 369 green 2 prompt addition (the page)" commit on top of it (it changes only `.mob/records/369/green-prompt.md`). Green 1's commit is `49db6a8b21f2b4972ad9e48b6785e86649877b50` (`git rev-parse 49db6a8`). `git diff --stat 49db6a8 HEAD -- reference` is empty, so the lockfile blobs are the same at both. **The page cites `49db6a8b21f2b4972ad9e48b6785e86649877b50`**, the commit the lockfiles came from (the card asks for "the commit they came from"); if the facilitator wants HEAD's hash instead, it is a one-hash swap on the Command line of the new section and L5 passes either way.

## Data read (no network for the packages)

- Lockfiles: Orders.Api 16 packages (1 Direct, 15 Transitive); Orders.Specs 30 packages (3 Direct, 27 Transitive) plus the `orders.api` Project entry. Union 30 distinct ids, one version each; 16 in the Orders.Api lockfile, 14 only in Orders.Specs. Direct: FastEndpoints, Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio. No figure differs from the slice.
- Nuspec files in the NuGet package folder (path built from environment variables inside an inline `python -` script; never printed): all 30 folders present, each `.nupkg.metadata` source `https://api.nuget.org/v3/index.json`. 28 have `<license type="expression">`; counting the two below, the 30 are 21 MIT and 9 Apache-2.0. System.Reflection.Metadata 1.6.0 and xunit.abstractions 2.0.3 have no `<license>` element, only `<licenseUrl>`. `requireLicenseAcceptance` true: Microsoft.CodeCoverage, Microsoft.NET.Test.Sdk, Microsoft.TestPlatform.ObjectModel, Microsoft.TestPlatform.TestHost (all 17.11.1); no other.
- System.Reflection.Metadata 1.6.0 packaged `LICENSE.TXT`, first line: `The MIT License (MIT)`.
- xunit.abstractions 2.0.3: no licence file in the package. `curl -sS -L --max-time 30 https://raw.githubusercontent.com/xunit/xunit/master/license.txt` read the file: `Unless otherwise noted, the source code here is covered by the following license:` ... `Licensed under the Apache License, Version 2.0`, then two imported .NET folders `covered by the following license: The MIT License (MIT)`. Row: Apache-2.0, source `licence URL`, review `ok`; the page says the URL points at the master branch (today's text, not the 2.0.3 release) and recommends reading it again on the publish day.
- Bundled third-party notice files (file name containing "third" or "notice"): 12 packages (Microsoft.CodeCoverage, the eight Microsoft.Extensions packages, Microsoft.TestPlatform.TestHost, System.Diagnostics.DiagnosticSource, System.Reflection.Metadata). The page says they are not reviewed.

## Files changed (uncommitted)

- `docs/release/pre-publication-checks.md` (LF kept): intro line 3 (licences read from nuspec and packaged licence files; the new section lists the lockfiles at `49db6a8`); line 24 (transitive packages listed and reviewed in Restored package licences, 30 = 4 direct + 26 transitive, none outside the allowed list; "not reviewed" gone from the section); line 26 Recommendation (follow the new section for the 26 transitive packages); new section `## Restored package licences` after Package licences: Command line (creating command, both lockfile paths, `--locked-mode` check command, the 40-hex commit, where the licences were read), the tool line naming Avowry.Cli, Avowry.Compiler and Avowry.Adapters.CSharp, a 30-row 7-column table, bullets for no licence expression (2), licence acceptance required (4, named), the none-outside line with the notice-file caveat, and one Recommendation line. No STALE phrase, no path, no address.
- Pending trait lines removed (one line each, nothing else): F1, F2, F4 in `RestoredPackageLicences.cs`; F6 in `RestoredPackageLicencesLockedRestore.cs`; the Slice 186 wrapper in `PrePublicationChecks.cs`. All LF kept (`git ls-files --eol`: `i/lf w/lf`).

## Commands and real output (repository root)

1. `python -m unittest tests.test_restored_package_licences tests.test_pre_publication_checks tests.test_pre_publication_owner_items`: `Ran 30 tests in 19.465s` `OK`.
2. `python scripts/check-no-personal-names.py`: exit 0.
3. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Slice=369|Slice=186|Slice=371"`: `Passed!  - Failed:     0, Passed:    11, Skipped:     0, Total:    11, Duration: 20 s`. F5 and F6 ran their locked restores (network present).
4. `git diff -U0` scanned for a users path, `@` in prose, a package-folder path and 40-hex strings: only the existing scan tip on line 3 and the cited lockfile commit.

## Not run

- The full solution suite (`dotnet test Avowry.sln --filter "Status!=Pending"`), left to the facilitator as briefed.

## Scratch

None created by green 2 (the licence reads were inline scripts; the URL fetch went to stdout). The C# test run wrote the usual git-ignored `bin` and `obj` folders in this worktree.
