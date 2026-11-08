# Card 178 red (D157 Criteria 1 and 2), QA (red) seat

Pending new-state red for criteria 1 and 2, one test, committed with `Status=Pending` so the gate excludes it. Watched failing by assertion on the pre-change tree (no `docs/PREREQUISITES.md`).

## Pre-change commit hash

`git rev-parse HEAD` before this commit: `290de76a355da9ca682e0af573040d8a4d1b2ddc` (the characterisation commit; BASE in the characterisation test is `b9e642348fdac7f80aac4fc33bdc2f74dcddaff2`).

## Command

```
python -m unittest tests.test_prerequisites
```

and

```
dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Prerequisites_page_matches_what_CI_uses"
```

## Failing test

`tests.test_prerequisites.PrerequisitesPageTests.test_prerequisites_page_matches_what_CI_uses`

C# wrapper: `Avowry.Acceptance.Tests.PrerequisitesPage.Prerequisites_page_matches_what_CI_uses` with `[Trait("Slice", "178")]`, `[Trait("Criterion", "178-1")]`, `[Trait("Status", "Pending")]`.

Four tests in the python module: three self-tests pass on this tree; the one Pending test fails.

## Expected failure

AssertionError from `self.assertEqual([], failures, ...)`, not a FileNotFoundError. The page is missing. One check failed on its own assertion; every page check is blocked by the missing page. Workflow-only checks (parser leftover, non-vacuous pins, architecture-workflows) are not in the list (they passed).

Failed on its own assertion:

- `page`: `docs/PREREQUISITES.md is missing`

Blocked by the missing page:

- `runs-on ubuntu-latest`
- `dotnet-version 8.0.x`
- `reverse-sdk`
- `reverse-os`
- `architecture`
- `untested`
- `dotnet-first`
- `roll-forward`
- `local-feed`
- `commands`

## Quoted output

```
F...
======================================================================
FAIL: test_prerequisites_page_matches_what_CI_uses (tests.test_prerequisites.PrerequisitesPageTests.test_prerequisites_page_matches_what_CI_uses)
----------------------------------------------------------------------
Traceback (most recent call last):
  File "C:\Users\ringe\projects\wt-178-red\tests\test_prerequisites.py", line 527, in test_prerequisites_page_matches_what_CI_uses
    self.assertEqual([], failures, HEADER + "\n".join(failures))
AssertionError: Lists differ: [] != ['page: docs/PREREQUISITES.md is missing', ... 11 items]

page: docs/PREREQUISITES.md is missing
runs-on ubuntu-latest: blocked: page missing
dotnet-version 8.0.x: blocked: page missing
reverse-sdk: blocked: page missing
reverse-os: blocked: page missing
architecture: blocked: page missing
untested: blocked: page missing
dotnet-first: blocked: page missing
roll-forward: blocked: page missing
local-feed: blocked: page missing
commands: blocked: page missing

Ran 4 tests in 0.005s
FAILED (failures=1)
```

The C# wrapper fails by assertion through that output (`Assert.True(exit == 0, text)` at PrerequisitesPage.cs:35):

```
Failed Avowry.Acceptance.Tests.PrerequisitesPage.Prerequisites_page_matches_what_CI_uses [238 ms]
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 238 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

## Self-tests (pass on the pre-change tree)

- `test_parser_reads_a_synthetic_matrix_workflow`: parser reads matrix lists, dash items, include rows and a `|` pin block.
- `test_minimal_compliant_page_passes`: embedded page (ubuntu-latest, 8.0.x, architecture unpinned, untested, .NET-first, local feed, three bash blocks with Run from:/Needs: and Same in PowerShell.) yields an empty failure list against the live workflows. Each check can be satisfied.
- `test_labels_only_page_fails_every_prose_check`: a page that only names `ubuntu-latest` and `8.0.x` passes the forward match and fails architecture, untested, dotnet-first, local-feed and commands.

## Line counts of the new files

- `tests/test_prerequisites.py`: 531 lines (aim was 350; the parser, the embedded fixtures and the self-tests are the bulk).
- `tests/Avowry.Acceptance.Tests/PrerequisitesPage.cs`: 37 lines (35 nonempty). Under the 400-line cap; about the 40-line wrapper size.

## Gate-filter result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Status!=Pending&FullyQualifiedName~Prerequisites_page"`

```
No test matches the given testcase filter `Status!=Pending&FullyQualifiedName~Prerequisites_page` in C:\Users\ringe\projects\wt-178-red\tests\Avowry.Acceptance.Tests\bin\Debug\net8.0\Avowry.Acceptance.Tests.dll
```

0 tests run, exit 0.

## Neighbouring tests

`python -m unittest tests.test_demo_readme tests.test_readme_reference_status tests.test_license_files`: Ran 18, OK.

`tests.test_live_docs_266` was already failing on this tree at characterisation (T11, TEAM_WORKING_AGREEMENT.md:156), unrelated to these files.
