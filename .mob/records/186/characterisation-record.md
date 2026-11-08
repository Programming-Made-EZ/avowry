# Card 186 characterisation (D157), QA (red) seat

Declared CHARACTERISATION, not a red. Slice section 4: the `personal-names` id, the licence files and the NOTICE/TRADEMARKS placeholders already pass on the pre-change tree; nothing new is needed. HEAD of this worktree before this commit: `89592516a4eb6998800888b8c4c92f14cc0eece3` (branch `card-186-red`). The findings page and `scripts/scan-history-for-secrets.py` are absent.

## Commands and pass lines

`python -m unittest tests.test_no_personal_names tests.test_license_files tests.test_prerequisites tests.test_getting_started`

```
Ran 21 tests in 0.026s

OK
```

`python scripts/check-no-personal-names.py`

exit status: 0

`dotnet build src/Avowry.Cli` then `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "FullyQualifiedName~DocumentationPrinciples|FullyQualifiedName~GeneratedApiSourceScan|FullyQualifiedName~PrerequisitesPage|FullyQualifiedName~GettingStartedDocs"`

```
Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 282 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

## Legal files against the base

Base: merge-base of HEAD and `origin/main` is `0412ea222d5a54ab4555e1a4b4c5586e1d994ad0`.

`git diff 0412ea222d5a54ab4555e1a4b4c5586e1d994ad0 HEAD -- NOTICE TRADEMARKS.md LICENSE` is empty.

`git diff HEAD -- NOTICE TRADEMARKS.md LICENSE` (working tree) is empty.

## Mutation (uncommitted, then restored)

Changed one character of the pinned placeholder expectation in `tests/test_license_files.py` (`HOLDER` to `HOLDXR`), ran `python -m unittest tests.test_license_files`, then `git checkout -- tests/test_license_files.py`. Working tree of that file is clean afterwards.

```
FAIL: test_notice_and_trademarks_carry_the_marked_placeholder_until_the_owner_confirms_the_holder
AssertionError: '[COPYRIGHT HOLDXR: to be confirmed by the product owner at the pre-publication checks (card 186)]' not found in NOTICE
Ran 6 tests in 0.003s
FAILED (failures=1)
```

## Neighbouring tests

The four Python modules and the four C# classes above are the neighbouring tests named in the brief. All passed on this pre-change tree. No neighbouring failure was already red.
