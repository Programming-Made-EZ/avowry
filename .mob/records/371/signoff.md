# Card 371 fresh sign-off (fresh Claude context, separate from the gate reviewer; own full gate in its own clone)

**SIGNED OFF WITH CONDITIONS.** None of the conditions blocks the sign-off.

## Gate evidence
Tested commit `720ce3c53cf5b0c609b66d91122063f3ddf5838a` (the page content is the same as at `f61e99c24acd5821a1e40dee9341eb34fb3fae3e`, an ancestor of it). Copy: a fresh `git clone --no-hardlinks --single-branch -b main` into `signoff371-clone\repo`. Dotnet processes at start: 1 (still there after the run: the build server, not a test run). Load line: the dotnet gate ran alone, then the Python suites ran alone; the seat only read files and ran greps while the dotnet gate ran; no load flake.

## Commands and results
1. `dotnet build src/Avowry.Cli`: 0 warnings, 0 errors.
2. `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`: Compiler.Tests 344 of 344; Acceptance.Tests 384 of 384 (9 m 37 s); the benchmark timed-out-model-tree test did not flake.
3. `python -m unittest tests.test_pre_publication_checks tests.test_pre_publication_owner_items tests.test_license_files tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266`, alone after the dotnet gate: 74 tests, 8 failures: the expected eight (five rename_196, two rename_197, T11); every pre-publication, owner-items and licence-file test passes.

## Judgement against the done checks
1. Sponsor reading: settled: all nine owner items marked "answered 2026-11-08", each with a plain decision and a card number where one carries it; waiting: the trademark search ("commissioned by the owner; pending result") and cards 367 to 370; blocking: the "What blocks what" paragraph says nothing is published as a release until the search result is in and the owner approves in writing, and that preparation can proceed meanwhile (this settles the cold reader's UNCLEAR). No tracked live file outside `.mob`, `docs/decisions`, `docs/retros` and `board` calls an answered item open or a placeholder (the remaining "placeholder" hits on the page are about secret-scan fixtures).
2. Brief: a provider could start from it (applicant, the mark as a word mark with the logo excluded, classes 9 and 42 with a goods and services description, jurisdictions US, EU, UK marked as proposed by the mob, seven registry lines with "not checked since", what was not checked, a link). A provider would still ask: the applicant's country and legal form (the page says the owner supplies this privately), whether the search covers common-law marks and phonetic similarity and whether it is a clearance check or a pre-filing opinion, existing use dates or filings, whether to widen the classes: normal provider intake questions, not blockers.
3. Contradictions: none with NOTICE (line 2), TRADEMARKS.md (line 5) or cards 366 to 370 as filed. `docs/LICENSING.md:27` says nothing is published "until the pre-publication checks pass and the owner approves"; the page names the trademark search result as the gate: compatible, the page's wording is narrower.
4. Value rule: the sponsor persona gets what the story promises (what is settled, what waits, the brief to hand a provider); no consumer-value gap.
5. Criteria: 371-1 (nine answers; the search item reads "commissioned by the owner; pending result") holds; 371-2 (no line still calls an answered item open) holds; 371-3 (the brief with mark, classes, jurisdictions and collisions found) holds; the tests follow the page and the gate review's mutations were killed.
6. The owner still has to: get the search result from the provider, confirm the jurisdictions with the provider, supply the applicant's country and legal form privately, correct the year 2026 if wrong, give written approval before any release, and re-run the registry lookups on the publish day: all named on the page.

## Conditions (none blocking)
- `HANDOFF.md:75` said "the NOTICE holder is a marked placeholder": stale (a live handoff file contradicting the answered holder item). Fixed by the facilitator in the card's close-out commit (HANDOFF.md lines 75 and 119).
- The generated board files for cards 366 to 371 still show `Status: Draft`: regenerated at the post-close step.
- The page heading still says "(card 186)" and "recorded 2026-11-08": carried.

## Retro risks
The cold reader returned UNCLEAR for a real reason (the split release gate); page revision 1 fixed it after the report and the cold reader was not re-run on that revision, so the fix is verified by the gate reviewer and the sign-off only. The second review returned REVISE and its six should-fix items were folded into revision 1. Test limits: matching is by fixed phrases; the Name collisions copy of "commissioned by the owner; pending result" (line 93) is not guarded by the new tests (gate review mutation b). Carried: jargon in untouched sections ("red commit", "seat", unlinked D121 and D125); "it passes" for the personal-name check verified only at the scan tip; the page double-counts `Orders.Specs` in the licences table; a generated board file for card 186 names the product owner by first name in a public file (a bug for the board generator); the brief's jurisdictions are the mob's proposal and unconfirmed (if the provider changes them, the page has to be edited).
