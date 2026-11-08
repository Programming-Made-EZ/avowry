# Card 186 red (D157 Criteria 1 to 3), QA (red) seat

Pending new-state red for criteria 1 to 3, one test, committed with `Status=Pending` so the gate excludes it. Watched failing by assertion on the pre-change tree (no `docs/release/pre-publication-checks.md`, no `scripts/scan-history-for-secrets.py`). Self-tests and `personal-names` pass.

## Pre-change commit hash

`git rev-parse HEAD` before this commit: `5049248226cffbe1ed48f19ce5788cd72a777d9c` (the characterisation commit).

## Command

```
python -m unittest tests.test_pre_publication_checks
```

and after `dotnet build src/Avowry.Cli`:

```
dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~Pre_publication_findings_match_the_repo"
```

## Failing test

`tests.test_pre_publication_checks.PrePublicationChecksTests.test_pre_publication_findings_match_the_repo`

C# wrapper: `Avowry.Acceptance.Tests.PrePublicationChecks.Pre_publication_findings_match_the_repo` with `[Fact]`, `[Trait("Slice", "186")]`, `[Trait("Criterion", "186-1")]`, `[Trait("Status", "Pending")]`.

Six tests in the python module: five self-tests pass on this tree; the one Pending test fails.

## Expected failure

AssertionError from `self.assertEqual([], failures, ...)`, not an exception. The page is missing. `personal-names` is not in the list (the checker exits 0). `scan-script` fails because the script is missing. Every other page check is blocked.

## Aggregated failure text (full)

```
page: docs/release/pre-publication-checks.md is missing
package-licences: blocked: page missing
vendor-terms: blocked: page missing
secrets-scan: blocked: page missing
author-metadata: blocked: page missing
name-collisions: blocked: page missing
owner-items: blocked: page missing
recommendations: blocked: page missing
scan-script: scripts/scan-history-for-secrets.py is missing
```

Python run: `F.....` then `Ran 6 tests in 7.080s` / `FAILED (failures=1)`.

The C# wrapper fails by assertion through that output (`Assert.True(process.ExitCode == 0, text)` at PrePublicationChecks.cs:34):

```
Failed Avowry.Acceptance.Tests.PrePublicationChecks.Pre_publication_findings_match_the_repo [5 s]
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 5 s - Avowry.Acceptance.Tests.dll (net8.0)
```

## D222 list

Ready draft MULTI rows first:

| criterion | ids |
|---|---|
| 1 | `package-licences`, `vendor-terms` |
| 2 | `secrets-scan`, `author-metadata`, `personal-names` |
| 3 | `name-collisions`, `owner-items` |

Slice additions:

| id | tag |
|---|---|
| `recommendations` | slice hostile (not from the Ready row) |
| `scan-script` | slice hostile (not from the Ready row) |

## Gate-filter result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --no-build --filter "Status!=Pending&FullyQualifiedName~PrePublicationChecks"`

```
No test matches the given testcase filter `Status!=Pending&FullyQualifiedName~PrePublicationChecks` in C:\Users\ringe\projects\wt-186-red\tests\Avowry.Acceptance.Tests\bin\Debug\net8.0\Avowry.Acceptance.Tests.dll
```

0 tests run, exit 0.

## Callers-test result

Command: `dotnet test tests/Avowry.Acceptance.Tests --nologo --no-build --filter "FullyQualifiedName~GeneratedApiSourceScan"`

```
Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 125 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

`"PrePublicationChecks.cs"` added to `ProcessStartCallers` in sorted position.

## Neighbouring tests

`python -m unittest tests.test_no_personal_names tests.test_license_files tests.test_prerequisites tests.test_getting_started`: Ran 21 tests in 0.028s, OK.

`dotnet test tests/Avowry.Acceptance.Tests --nologo --no-build --filter "FullyQualifiedName~DocumentationPrinciples|FullyQualifiedName~GeneratedApiSourceScan|FullyQualifiedName~PrerequisitesPage|FullyQualifiedName~GettingStartedDocs"`:

```
Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 270 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

## Line counts

- `tests/test_pre_publication_checks.py`: 339 lines (cap 350), LF.
- `tests/Avowry.Acceptance.Tests/PrePublicationChecks.cs`: 36 lines (cap 45), LF.
