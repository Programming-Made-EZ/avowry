# Card 366 fresh sign-off (fresh Claude context, separate from the gate reviewer; own full gate in its own clone)

**SIGNED OFF WITH CONDITIONS.** Block: none. The conditions are for the facilitator and the retro; none stops the card from closing.

## Gate evidence
Tested commit 83930ddf55df0599f9b95021940243f48f1904ce (the product tip b5f381a is an ancestor; the commits after it are record-only). Copy: a fresh `git clone --no-hardlinks --single-branch -b main` into `signoff366-clone`. Dotnet processes at start: 1 (an idle one already running, not the seat's). Nothing else ran beside the gate. Load line: the first full `dotnet test` took 10m59s and the second 10m58s; the first run hit the known load flake.

## Commands and results
1. `dotnet build src/Avowry.Cli`: 0 warnings, 0 errors.
2. `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`, first run: Compiler 344 passed; Acceptance 379 passed and 1 failed: `BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms` ("Avowry.BenchCs did not finish within 20 seconds").
3. That test rerun alone with `--no-build`: passed in 9 s. The whole gate rerun once: 344 passed, then 380 passed, exit 0. The failure was a load flake; the clean gate is green.
4. The Python run alone (`tests.test_license_files tests.test_pre_publication_checks tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_getting_started`): 74 tests in 106 s, 8 failures, exactly the expected set (rename_196: `test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`; rename_197: `test_lists_equal_baseline` x2; T11); `test_license_files`, `test_pre_publication_checks` and `test_getting_started` had no failure.

## The seat's own checks
1. NOTICE line 2 reads "Copyright 2026 Programming Made EZ"; TRADEMARKS.md line 5 reads "Trademark holder: Programming Made EZ"; docs/LICENSING.md line 27 names the holder and says 2026 is an assumption the product owner can correct. No square-bracket placeholder is left in NOTICE, TRADEMARKS.md, docs/LICENSING.md or README.md (only link syntax). `git diff --quiet 4476596 HEAD -- LICENSE` exits 0.
2. The year label is in docs/LICENSING.md line 27 (the ruling); NOTICE carries a bare "2026", acceptable for a legal notice file. The card's criterion 2 said "the page", meaning the pre-publication page (card 371's); the ruling moved the assumption to the licensing page.
3. Contradictions: a grep outside .mob, docs/decisions, docs/retros and board finds no live "COPYRIGHT HOLDER" or "to be confirmed" text except in the guard test (intended). Stale lines in docs/release/pre-publication-checks.md: line 103 (the holder OPEN and "NOTICE:2 (which also holds a year placeholder)"), line 115 and its recommendation text in line 116 ("holder needed (year and name)"); line 56 mentions "placeholder" for another thing. Card 371 exists on the board as a Draft story; its text does not list these line numbers.
4. Value rule: Dana now sees a real holder in the three files and the year flagged as correctable; no consumer-value gap against the story. The page that still contradicts it is the pre-publication page (card 371).
5. The cold reader's UNCLEAR verdict (about external proof of a trademark registration or a verified legal entity) is out of scope: the card promises who is named, not registration.
6. Criteria: 1 holds (holder and year filled, no placeholder); 2 holds (LICENSE unchanged; the assumption recorded in LICENSING.md per the ruling); 3 holds (the guard tests pass and include planted-placeholder rows). The gate review's SHIP is consistent.

## Conditions
- Card 371 must also correct the stale lines 103, 115 and 116 of docs/release/pre-publication-checks.md (its criteria do not cite them): until then the repository holds a contradictory page.
- The decision record for the 2026 assumption should be appended by the facilitator (not verified by the seat).

## Retro risks
The timed-out-model-tree acceptance test is load-sensitive (it failed on an otherwise idle machine and passed on rerun); the card's criterion 2 named the wrong page and was fixed through a ruling (cards should name the file or page); the "8 expected failures" figure is easy to mistake for a regression.
