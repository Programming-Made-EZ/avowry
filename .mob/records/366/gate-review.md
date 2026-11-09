# Card 366 independent gate review (fresh Claude context, read-only in the main checkout; own clone)

**Step 0:** `git merge-base --is-ancestor 1e83647051f0316073154a0ec8a913d48ba27d76 fab5dc9b5f7861353241b571ce33ef6e19ddce09`; tested commit fab5dc9b5f7861353241b571ce33ef6e19ddce09 (it descends from the cold-reader report commit; no page was revised after the report); exit status 0.

**Verdict: SHIP.** No defects; two notes and three test limits.

## Files and criteria checked (the D231 amendment: this review is a committed record)
Files: `.mob/records/366/taskforce-proposal-pm.md` (the Ready draft, id rows), `slice-architect.md`, `red-record.md`, `characterisation-record.md`, `cold-reader.md`, `tests/test_license_files.py`, `tests/test_pre_publication_checks.py`, `tests/Avowry.Acceptance.Tests/LicenseFiles.cs`, NOTICE, TRADEMARKS.md, docs/LICENSING.md, LICENSE. Criteria: criterion 1 (N1, N2, N3, T1), criterion 2 (G1, G2, P1 to P4, X1, X2), criterion 3 (L1, C1, C2).

## 1. Order and red-first
Order in the clone: 4476596 (slice), fc32c36 (loosening), b044e96 (red), b5f381a (green), 1e83647 (report), fab5dc9 (MOB.md regeneration): fc32c36 and b044e96 precede the green. The green changed exactly four files: NOTICE, TRADEMARKS.md, docs/LICENSING.md and `tests/Avowry.Acceptance.Tests/LicenseFiles.cs`; its only test change is the removal of the three `Status=Pending` traits (the b044e96..b5f381a diff also lists MOB.md, from the ceb725a regeneration in that range). `git diff --quiet 4476596 fab5dc9 -- LICENSE` exited 0. Between 4476596 and fab5dc9 the changes outside `.mob` are MOB.md, the three text files, LicenseFiles.cs, tests/test_license_files.py and tests/test_pre_publication_checks.py; no other test file changed.
Red replay in a detached worktree at b044e96: `python -m unittest -v tests.test_license_files`: Ran 10 tests, FAILED (failures=6): N1, N2, T1, G1, G2, L1 by assertion (e.g. N1: `'Programming Made EZ' != '[COPYRIGHT HOLDER: ...]'`); N3, P1 to P4, X1, X2, C1 and the four C2 tests passed. `dotnet build src/Avowry.Cli`: 0 errors. With the Pending traits deleted uncommitted there: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Slice=366"`: `Failed: 3, Passed: 1, Total: 4`; the three failures (Notice_and_trademarks_hold_no_placeholder_and_the_scanner_catches_one, Notice_and_trademarks_name_the_holder_and_the_year, The_licensing_page_names_the_holder_and_the_year_assumption) are assertion failures from the Python tests; the passing one is the non-Pending characterisation wrapper. The worktree was removed afterwards.

## 2. Loosening (fc32c36)
It removed only the `PLACEHOLDER` constant, the `placeholder`, `notice` and `trademarks` context keys, the placeholder `elif` clause and the local variables `ph`, `notice`, `marks` in `tests/test_pre_publication_checks.py`; the 6-OPEN check is kept; no unused variable remains. `python -m unittest tests.test_pre_publication_checks` passes (6 tests, OK) on the final tree. (Not re-run on the pre-loosening tree; the red record says it passed after the loosening and the diff shows nothing else changed.)

## 3. Ready coverage (D222)
All 15 ids (N1 N2 N3 T1 G1 G2 P1 to P4 X1 X2 L1 C1 C2) appear in the red record's D222 table; the replay matched its stated pass/fail split exactly; the three criteria are covered.

## 4. Mutation table (clone only, each restored with `git checkout`; `git status` clean)
| Mutant | Result |
|---|---|
| (a) NOTICE year 2025 | N2 fails |
| (b) `[YEAR]` back in NOTICE | G1 and N2 fail |
| (c) "(to be confirmed)" added to TRADEMARKS.md line 5 | G2 and T1 fail |
| (d) holder changed in TRADEMARKS.md only | T1 fails |
| (e) 'assumption' removed from docs/LICENSING.md | L1 fails |
| (f) LICENSE touched | C1 fails (hash test) |
| (g) scanner pattern matches markdown links | X1 fails |
| (h) scanner returns `[]` | P1 to P4 fail |
| (i) "marked placeholder" restored in LICENSING.md | L1 fails |
| extra: second `Copyright` line in NOTICE | N3 fails |
| extra: `[Holder Name]` appended to TRADEMARKS.md line 5 | T1 fails |
No mutant survived. Test limits: the scanner is line-based (a placeholder split across lines is missed); the bracket-token pattern needs an all-caps start (a mixed-case `[Holder Name]` is not flagged by the scanner; T1's exact-line check caught it); nothing checks that 2026 is the right year.

## 5. Gate
`dotnet build src/Avowry.Cli`: 0 warnings, 0 errors. `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`: `Passed! - Failed: 0, Passed: 344` (Compiler.Tests); `Passed! - Failed: 0, Passed: 380` (Acceptance.Tests, 10 m 34 s); no load flake. `python -m unittest tests.test_license_files tests.test_pre_publication_checks tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_getting_started`: Ran 74, FAILED (failures=8): the expected eight (five rename_196, two rename_197, T11); nothing from test_license_files, test_pre_publication_checks or test_getting_started failed.
Reading the three texts as Dana: NOTICE ("Copyright 2026 Programming Made EZ") is clear; TRADEMARKS.md is consistent; docs/LICENSING.md line 27 names the holder, calls 2026 "the year the project began", says the owner can correct it, no longer calls the holder a placeholder and keeps the release-gate wording: clear and consistent. Notes (not findings): `docs/release/pre-publication-checks.md` is stale against these changes (line 100 "holds a year placeholder", line 103 holder OPEN, line 116 "holder needed"): card 371's; docs/LICENSING.md still carries the card 185 contributing-note reference, outside this card.

**Gate evidence:** tested commit fab5dc9b5f7861353241b571ce33ef6e19ddce09; copy: `git clone --no-hardlinks --single-branch -b main` of the main checkout; dotnet processes at start: 1 (a pre-existing dotnet.exe, not the reviewer's).
**Load:** the gate ran alone (10 m 34 s for the Acceptance project, slow for a quiet machine); the Python suites ran only after it.
Clone left at `C:\Users\ringe\projects\gate366-review` (clean, on main at fab5dc9, no extra worktrees).
