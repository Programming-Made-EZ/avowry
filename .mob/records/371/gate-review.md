# Card 371 independent gate review (fresh Claude context, read-only in the main checkout; own clone)

**Step 0:** `git merge-base --is-ancestor f61e99c 729933c2064bef29a92bea6bd0594f39e7856443`; f61e99c is `f61e99c24acd5821a1e40dee9341eb34fb3fae3e` (the last page revision, after the cold-reader and second-review reports); tested commit `729933c2064bef29a92bea6bd0594f39e7856443`; exit status 0.

**Verdict: SHIP.** No blocking issue; two minor findings.

## Files and criteria checked (the D231 amendment: this review is a committed record)
Files: `docs/release/pre-publication-checks.md`, `tests/test_pre_publication_owner_items.py`, `tests/test_pre_publication_checks.py`, `tests/Avowry.Acceptance.Tests/PrePublicationOwnerItems.cs`, `.mob/records/371/red-record.md`, `taskforce-proposal-pm.md`, `cold-reader.md`, `second-review.md`, NOTICE, TRADEMARKS.md, docs/LICENSING.md, LICENSE. Criteria: 371-1 (A and E rows), 371-2 (Z, S and F rows), 371-3 (B rows), card 186's module as a characterisation row.

## 1. Order and red-first
Order: 1a82c2f, 9355735, 54a6875, 479f2db, f61e99c, 729933c; 9355735 and 54a6875 precede the green. The green `479f2db` touched only the page and `PrePublicationOwnerItems.cs` (exactly three `Trait("Status","Pending")` lines removed); `f61e99c` touched only the page; `729933c` only `MOB.md` and `.mob/records/seat-use.jsonl`. NOTICE, TRADEMARKS.md, docs/LICENSING.md and LICENSE: empty diff from 54a6875 to 729933c; the `tests` diff over that range is those three trait lines and nothing else.
Red replay (detached worktree at 54a6875): `python -m unittest -v tests.test_pre_publication_owner_items`: Ran 7, FAILED (failures=32), all by assertion: A1 to A9, A4q, A11; Z1 and S1 to S9 (S6a, S6b, S6c); B0 to B8; A10, the E rows, the F rows and X1 pass. CLI built (0 errors), Pending traits removed uncommitted: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Slice=371"`: `Failed! Failed: 3, Passed: 1, Total: 4` (`Each_of_the_nine_owner_items_shows_its_answer`, `No_page_line_still_calls_an_answered_item_open`, `The_page_carries_the_trademark_search_brief` fail by assertion; the A10/X1 characterisation passes). Restored; worktree removed.

## 2. Card 186 revision (9355735)
7 deletions in `tests/test_pre_publication_checks.py`: the six `- ...: OPEN, asked 2026-11-08` fixture rows and the whole `elif cid == "owner-items":` block; `## Owner items` and `Recommendation: ask.` stay; no dead code. `python -m unittest tests.test_pre_publication_checks` passes at 1a82c2f (Ran 6, OK) and at 9355735 (Ran 6, OK).

## 3. Page truth
Citations checked and correct: `docs/release/v0.1-bar.md:34` and `:51`; `CSharpProvider.cs:237`, `:251`, `:252`, `:253`; `docs/decisions/process-and-staffing.md:129`; `NOTICE:2`; `TRADEMARKS.md:5`; `TEAM_WORKING_AGREEMENT.md:108`; `docs/LICENSING.md:27`; tip `c4692e0` and commit `dde36dc` exist and the commit count at the tip is 3124; the PackageReference versions; `Avowry.Cli.csproj` has package id, version, tool command and description and lacks licence, authors and NOTICE; cards 366 to 370 exist in `board/` and match items 6 to 9. Scan figures: 65 hits and 146/277/381/137 match `.mob/records/186/signoff.md:9-11`; the 336, 120, 331, 139 and 45/5/4/8/3 figures appear in the card 186 records. The cold reader's blocking point is addressed by the "What blocks what" paragraph (line 100) and the consistent registry line; the second review's six should-fix items are fixed in f61e99c (citations at lines 13-16 and 84; the intro's exception for the Owner items section, line 3; the page saying it is the record of the owner's answers, line 102; the Phrase clause, line 134; capitalised plain-sentence answers; brief completeness). Carried by the records: jargon in untouched sections (lines 41, 47, 52), the stale "(card 186)" heading and date (lines 1 and 3), the "it passes" claim verified only at the tip commit. No email, personal name, secret or real home path on the page (the only `C:\Users` text is the counting regex on line 60); `scripts/check-no-personal-names.py` and `scripts/check-no-secrets.py` exit 0. The brief's anchor `#name-collisions-not-a-professional-search` matches the heading at line 82.

## 4. Ready coverage (D222)
Every id of the Ready draft is in the red record's table and in the test file: A1 to A11, A4q, E1 to E5, Z1, S1 to S8 plus S9 (S6 split into three), F1 to F6, X1, B1 to B8, and the extra B0 and X1 siblings. The three criteria are covered, one Pending wrapper each.

## 5. Mutation table (clone only, restored with `git checkout`)
| Mutation | Killed by |
|---|---|
| (a) one item reverted to OPEN | A1, A11, Z1 |
| (b) "commissioned by the owner; pending result" removed from the search item | A3 |
| (c) "stay open" re-added near line 20 | S1 |
| (d) brief heading deleted | B0 to B8, A11, the nine-items count |
| (e) tenth top-level item | the nine-items count |
| (f) class 42 changed to 41 | B3, B4 |
| (g) crates.io registry line dropped from the brief | B6 |
| (h) email address added | card 186's findings-match test |
| (i) stale checker returns `[]` | F1 to F6 |
| (j) Jurisdictions label reverted | B5 |
All ten killed. Test limit: mutation (b) removes the phrase from the item (line 106) and not from line 93; the Name collisions copy at line 93 is not guarded by the new tests (the red record's test limits already say matching is by fixed phrases and the stale list is closed).

## 6. Gate
`dotnet build src/Avowry.Cli`: 0 errors. `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`, alone: `Passed! Failed: 0, Passed: 344` (Compiler.Tests); `Passed! Failed: 0, Passed: 384` (Acceptance.Tests, 9 m 40 s); no load flake. `python -m unittest tests.test_pre_publication_checks tests.test_pre_publication_owner_items tests.test_license_files tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266`: Ran 74, FAILED (failures=8): the expected eight (five rename_196, two rename_197, T11); every pre-publication, licence-file and card 371 test passes. Sponsor reading: what is settled is clear (nine items "answered 2026-11-08" with a plain decision and a card number); what waits is clear (the trademark search result gates the release); "What blocks what" says preparation (cards 366 to 370) can proceed meanwhile; the page says it is itself the record of the owner's answers.

## Findings (minor, none blocking)
1. `pre-publication-checks.md:14` says "three tracked test projects, `reference/csharp/Orders.Specs`...": `Orders.Specs` is double-counted (only three tracked projects reference Microsoft.NET.Test.Sdk); the wording is from card 186's original page, not a card 371 regression.
2. The jargon, stale heading and tip-only "it passes" items above are carried by the records.

**Gate evidence:** tested commit `729933c2064bef29a92bea6bd0594f39e7856443`; copy: fresh `--no-hardlinks --single-branch` clone, with a detached worktree at 54a6875 for the red replay; dotnet processes at start: 1 (the build server, not a test run). Clone: `C:\Users\ringe\projects\gate371-review`; stray files: a throwaway mutation script and the gate log in the session scratchpad directory.
**Load:** the full dotnet gate ran alone; the Python suites started only after it finished; no load flake seen.
