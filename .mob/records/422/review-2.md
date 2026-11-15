# Card 422 review round 2 (told to try to break the change), card-422 at c0787a7

VERDICT: FLAG; new BLOCKING: 1; new MINOR: 1

Seat: QA (claude opus = claude-opus-5-5, ctx-4b, D268), read-only in a throwaway clone (`"projects", "review422r2-clone"`, HEAD c0787a7). Probe: `<scratchpad>` + `review422r2probe` (`replica.py`, `attack_f6.py`, `attack_o.py`, outputs in `*.out`; `shallow/` is a depth-1 local `file://` clone of the review clone, made to replay CI's checkout). Runs in the clone: `python -m unittest tests.test_restored_package_licences` Ran 143, OK; `dotnet build Avowry.sln -c Debug` 0 warnings, 0 errors; `dotnet test ... --no-build -c Debug --filter "Slice=422|FullyQualifiedName~NoPhCodesInTheLiveTree"` 168 passed, 0 failed. Nothing networked was run; no background process left. `gate-fix1/summary.txt` is not committed yet.

| # | Finding with a minimal input | Replay | Class with reason |
|---|---|---|---|
| 1 | The new Python row FL-py-L5-deep fails on a shallow checkout, and CI's `test` job checks out shallow. `Test422FixCharacterisation.l5(False)` replaces `is_shallow` with `lambda: False`, so `lockfile_commit_failures` runs `git show 49db6a8...:reference/csharp/...packages.lock.json`. In a depth-1 clone the cited commit is absent, so test_L5 fails with `lockfile blob missing at that commit` (both lockfiles), and `test_422_FL_py_L5_deep` asserts `ok`. The C# Fact `F16_422_6_fix_log_python_rows_that_hold_today` (Slice 422, no Pending trait) runs that row through `AssertPython422`, and `.github/workflows/ci.yml` job `test` uses `actions/checkout@v4` without `fetch-depth` and then `dotnet test Avowry.sln ... --filter "Status!=Pending"`. So the CI test job goes red. The laptop gate runs in a full clone, so it cannot see this. Input: the real tree, checked out at depth 1. | confirmed in the probe: in `shallow/` (`git rev-parse --is-shallow-repository` true), `python -m unittest tests.test_restored_package_licences.Test422FixCharacterisation` gives `FAILED (failures=1)` on `test_422_FL_py_L5_deep`, and the whole module gives Ran 143, `FAILED (failures=1)`. The same module passes in the full clone. | BLOCKING: a regression on a real shape (this repository's CI checkout). A passing CI job turns red with no ruling. New with this fix round (the row was added at 5460482). A possible fix: the deep row also stubs the blob check, for example by passing a page with no 40-hex commit, or by stubbing `git`, so it only proves that nothing is printed. Or it is skipped when the real checkout is shallow. |
| 2 | Stale comments after the green. The `F6Failures` summary still says "Today's decision: existence only." and the `ClassifyRestore` summary still says "Today's decision: the network text first, NU1004 as a substring." Both now do the opposite: they call `LicenceValueFailures` and delegate to `RestoreOutcome`. The red's Theory names `F6_rows_422fix_pending`, `Classify_rows_422fix_pending` and `Wiring_rows_422fix_*_pending` no longer carry a Pending trait. | reviewer (code read, `RestoredLicenceChecks.cs`, `RestoredPackageLicencesLockedRestore.cs`) | MINOR: no behaviour change, but a future reader is misled about what the networked path decides. New with this fix round. |

## What HELD

- Round 1 BLOCKING 1 ([E] on the networked path) is fixed on the code. F6 now builds every restored package through `ReadRestoredPackages` and `RestoredPackageFrom`, then asserts `F6Failures` plus `F6AcceptanceFailures` are empty. `F6Failures` passes each checked row and its evidence to `LicenceValueFailures`. Replay with my Python replica of the C# functions over the real page and the real local package folder (no restore, no network):
  - the real page gives no failure;
  - E-swap fails, and no licence swap between two rows with different licences passes (every such pair was tried);
  - 600 single-row edits (each row with each of 7 wrong or blank licences and each of 3 source labels) give 0 wrong-licence passes;
  - a duplicate row with a wrong licence fails;
  - a page row missing from the restore, and a restored package missing from the page, both fail the set check.
  - The only page change that passes is Newtonsoft.Json relabelled `packaged licence file` with MIT, which is still the right licence (its LICENSE.md says MIT).
  - Only 2 of the 30 packages ship a licence-named file (Newtonsoft.Json LICENSE.md, System.Reflection.Metadata LICENSE.TXT), and each classifies as the page licence. So no nested or third-party file can carry a wrong licence on the real files.
  - A package with both a licence file and a reviewed URL that disagree passes whichever source the page names. This follows the slice's FW-both rows, and no real package has both.
- Round 1 BLOCKING 2 ([O] on the networked path) is fixed on the code. F5's planted step and `RestoreLockedOrFail` both call `ClassifyRestore(exit, timedOut, PublicText(stdout + stderr), RestoredCount(ReadAssets(project)))`. `ClassifyRestore` is only `=> RestoreOutcome(...)`. `NetworkMissing` and `Assert.Contains("NU1004", ...)` are gone (the wiring rows scan for both). Nothing else on the networked path decides the outcome. The F5 set comparison against the lockfiles is card 369's and unchanged.
  - Replica on restore texts:
    - the CRLF, German, French, Japanese and full-width-colon planted texts give mismatch;
    - NU10041, NU1004a, `_NU1004` and xNU1004 give fail;
    - cold NU1301 (count 0) gives network-missing, and partial NU1301 (count 12) gives fail;
    - exit 0 with a NU1801 warning gives pass, and a time-out with NU1004 gives fail.
  - Every non-pass outcome fails the test, so `RestoredCount` changes only which message is shown; it cannot turn a failure into a pass.
- RestoredCount handles null, `{}`, `libraries` as a list, project-only and `"Package"` in another case as 0. Empty, blank or truncated JSON throws, which fails closed. `TempWorkspace.CopyFrom` skips `obj`, and both callers restore into a fresh copy, so a stale assets file cannot be read.
- The log change: `Report` writes stdout plus stderr before the assert, so a passing F1 run now carries the shallow-checkout note (round 1 MINOR 3 is resolved). The green changed only the two function bodies, `Report` and the Pending traits, as FX-1 required.
- Super-linear time: none.
  - `RestoreOutcome` replica: 10,000 / 100,000 / 1,000,000 repeats of `NU100` take 1.9 / 17 / 178 ms, and repeats of a near network marker take 11 / 101 / 1045 ms.
  - `F6Failures` replica: 21,000 page rows in 53 ms.
  - `F6AcceptanceFailures` is lines times 30 packages, and `MethodBody422` is one scan with a brace counter. Both are linear by reading.
- Named limits, not reported again: a `nu1004` path or URL segment in restore text reads as mismatch. This is the FX named limit on #444; real restore output (records 369 and 417) has no help links.
