# Card 179 red (D157 Criteria 1 to 3), QA (red) seat

Pending new-state red for criteria 1 to 3, one test, committed with `Status=Pending` so the gate excludes it. Watched failing by assertion on the pre-change tree (no `docs/GETTING_STARTED.md`; README still has the resolver Pending / `NotImplementedException` claims).

## Pre-change commit hash

The red is built on the characterisation commit `80868541e231936730e3737ec4dfe8528b55f611`. That commit's parent (BASE, `git rev-parse HEAD` before characterisation) is `b30803420f30f4ff749f77d39be7640461d0506f`.

## Command

```
python -m unittest tests.test_getting_started
```

and, after `dotnet build src/Avowry.Cli`:

```
dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Getting_started_docs_match_danas_story"
```

## Failing test

`tests.test_getting_started.GettingStartedTests.test_readme_and_guide_match_the_story`

C# wrapper: `Avowry.Acceptance.Tests.GettingStartedDocs.Getting_started_docs_match_danas_story` with `[Trait("Slice", "179")]`, `[Trait("Criterion", "179-1")]`, `[Trait("Status", "Pending")]`.

Seven tests in the python module: six self-tests pass on this tree and on the post-change scratch tree; the one Pending live test fails.

## Expected failure

AssertionError from `self.assertEqual([], failures, ...)`, not a FileNotFoundError. The guide is missing. README and PREREQUISITES checks run on today's text. Every guide check is blocked by the missing page.

## Aggregated failure text in full

```
F
======================================================================
FAIL: test_readme_and_guide_match_the_story (tests.test_getting_started.GettingStartedTests.test_readme_and_guide_match_the_story)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\ringe\projects\wt-179-red\tests\test_getting_started.py", line 279, in test_readme_and_guide_match_the_story
    self.assertEqual([], failures, HEADER + "\n".join(failures))
AssertionError: Lists differ: [] != ['clone-url: missing', 'link-guide: missin[1961 chars]ing']

README.md, docs/GETTING_STARTED.md and docs/PREREQUISITES.md must match Dana's story.
clone-url: missing
link-guide: missing
link-prerequisites: missing
own-language: missing
adapter-preview: missing
verifiability: missing
preserved-intent: missing
reviewability: missing
dana-agent: dana-agent
known-limitations: known-limitations
layout: still present
test-status: still present
table-row: still present
pending: still present
resolver: still present
not-implemented: still present
trace: still present
ph-code: still present
dotnet-roll-forward: still present
dotnet-test: still present
reference-target: still present
line-count: README is more than 50 lines
status-script-retired: status script or its test is still present
newer-sdk: no sentence holds newer SDK, untested, does not replace and .NET 8 SDK
page: docs/GETTING_STARTED.md is missing
clone: blocked: page missing
dotnet-pack: blocked: page missing
tool-install: blocked: page missing
no-angle-brackets: blocked: page missing
avowry-build: blocked: page missing
no-dev-cli: blocked: page missing
avowry-listing: blocked: page missing
listing-create: blocked: page missing
listing-read: blocked: page missing
listing-fails: blocked: page missing
dotnet-test-specs: blocked: page missing
passed-n: blocked: page missing
dotnet-run-api: blocked: page missing
create-call: blocked: page missing
read-call: blocked: page missing
failure-422: blocked: page missing
source-avowry: blocked: page missing
generated-output: blocked: page missing
rebuild-leftovers: blocked: page missing
guide-prerequisites-link: blocked: page missing
guide-net8: blocked: page missing
commands: blocked: page missing
pwsh-windows-path: blocked: page missing
pwsh-no-tmp: blocked: page missing
pwsh-no-bare-curl: blocked: page missing
pwsh-http-client: blocked: page missing
guide-no-phrase: blocked: page missing
guide-no-wasm: blocked: page missing
guide-no-resolver: blocked: page missing

Ran 7 tests in 0.015s
FAILED (failures=1)
```

The C# wrapper fails by assertion through that output (`Assert.True(process.ExitCode == 0, text)` at GettingStartedDocs.cs:35):

```
Failed Avowry.Acceptance.Tests.GettingStartedDocs.Getting_started_docs_match_danas_story [337 ms]
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 337 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

Self-tests that passed (python `-v`, and again on the post-change scratch clone at `C:\Users\ringe\AppData\Local\Temp\avowry-179-char-pass` where `docs/GETTING_STARTED.md` exists and README.md is rewritten):

- `test_compliant_fixtures_give_no_failures`
- `test_missing_guide_reports_page_missing_and_blocked_ids_without_exception`
- `test_restyled_readme_table_fails`
- `test_curl_bash_without_pwsh_fails`
- `test_pwsh_block_with_bare_curl_fails`
- `test_old_prerequisites_newer_sdk_fails_and_new_wording_passes`

## Hostile inputs

Self-tests (embedded fixtures only): a missing guide yields `page: docs/GETTING_STARTED.md is missing` plus `blocked: page missing` for every guide id, with no exception; a compliant README plus a `|` row (NotImplementedException removed) fails `table-row`; a curl bash block with no pwsh block fails the aggregated assertion; a pwsh block with a bare `curl` fails `pwsh-no-bare-curl`; old PREREQUISITES lines 7 to 9 fail `newer-sdk` and the new wording passes. The live list above shows every README keep/drop that fails on today's text, the PREREQUISITES `newer-sdk` check, and each guide step as its own id.

## Gate-filter result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Status!=Pending&FullyQualifiedName~GettingStartedDocs"`

```
No test matches the given testcase filter `Status!=Pending&FullyQualifiedName~GettingStartedDocs` in C:\Users\ringe\projects\wt-179-red\tests\Avowry.Acceptance.Tests\bin\Debug\net8.0\Avowry.Acceptance.Tests.dll
```

0 tests run, exit 0.

## Callers-test result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~GeneratedApiSourceScan"`

```
Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 139 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

`"GettingStartedDocs.cs"` was added to `ProcessStartCallers` in sorted position.

## Neighbouring tests

`python -m unittest tests.test_demo_readme tests.test_license_files tests.test_prerequisites tests.test_readme_reference_status tests.test_live_docs_266 tests.test_demo_readme_untouched_179`

Ran 44 tests in 16.018s, FAILED (failures=1): `tests.test_live_docs_266` T11 (`TEAM_WORKING_AGREEMENT.md:156 holds no Avowry to put Phrase back over`). Bug 338, already red, not this card. Characterisation still passes.

`tests.test_rename_196` ran in this worktree: already FAILED on the pre-change tree (list/eol/dir counts drifted since card 196).

`tests.test_rename_197.ListsAndPaths.test_lists_equal_baseline` ran in this worktree: FAILED. Compiler list 284 vs 324 (already drifted). Acceptance list 317 vs 329; one of the 12 extras is `GettingStartedDocs.Getting_started_docs_match_danas_story` (slice risk 7). Already red before this card except that one extra row.

## Line counts of the new files

- `tests/test_getting_started.py`: 283 lines (cap 350).
- `tests/Avowry.Acceptance.Tests/GettingStartedDocs.cs`: 37 lines (cap 45).
