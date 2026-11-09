# Card 370 fresh sign-off (fresh Claude context, separate from the architect, the writers and the gate reviewer; own full gate in its own clone)

**SIGNED OFF, with two conditions for the facilitator at closure.** No block fails on the evidence. The red was written by a Cursor writing seat and the greens by a Muse writing seat (Probation rung 1).

## Gate evidence
Tested commit 79fdf9d (the product tip is e19198d plus fa1eb43; the commits after it are record-only: ca2257d MOB.md regeneration, 79fdf9d gate review). Copy: the seat's own full clone (`--no-hardlinks --single-branch -b main`) with the origin reset to the GitHub URL; no uncommitted changes. Dotnet processes at start: 1 (a dotnet.exe not the seat's; still there at the end). Load: the dotnet runs had nothing of the seat's beside them except sleep timers.

## Commands and results
1. `dotnet build src/Avowry.Cli`: 0 warnings, 0 errors.
2. `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`, first run: Compiler 344 passed; Acceptance 386 of 387 (10 m); the one failure was `BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms` (29 s), the known load flake.
3. That test alone with `--no-build`: passed (1 of 1, 9 s).
4. The whole gate rerun once: Compiler 344 of 344, Acceptance 387 of 387 (9 m 31 s); zero failures.
5. `python -m unittest -v tests.test_home_paths_370 tests.test_verdict tests.test_license_files tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266`, alone: 139 tests, 8 failures, 0 errors, 0 skipped: the expected eight (rename_196: `test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`; rename_197: `test_lists_equal_baseline` x2; T11 in live_docs_266); the seven `ScrubChangesOnlyAccountSegments` rows ran against the full clone with the base commit present, all ok, none skipped.

## The seat's own checks
- Scan: `python scripts/home_paths.py --root .` printed `0 files`; `python scripts/check-no-secrets.py --tree` gave 0 hits in each of A1 to A8 and exit 0; plain `git grep -I -i -E` for the Windows form (single, double or any number of backslashes, and `/`), `/c/Users/<acct>`, `/mnt/c/Users/<acct>`, the slug form, `/Users/<acct>`, `/home/<acct>` and the URL-encoded form: 0 files, 0 lines for every form. `git grep -a` with `Users.{1,4}<acct>` finds 1 hit, in `.mob/records/229/nohome-check/nohome229_check.py:20`: a list built from parts in a record's own check script (the segments are not adjacent in the file; not a path in the file, but it carries the name). `<user>` now stands in 304 files and 1,009 lines.
- Hook: in a throwaway clone with `core.hooksPath=.githooks`, a staged file with a planted Windows path built from parts was refused with exit 1 (`pre-commit: ... docs/plant.md:2: home path (F1)`, no account name); the same file with `<user>` committed with exit 0.
- Board: `gh` authenticated; `generate_archive.py --check` exit 0 (every card and the table "unchanged"); `python scripts/check-board-archive.py` exit 0.
- Pins: `mob_process.py check` OK (129 closed cards, 38 records, 91 legacy); `check-decisions-index.py` passed (227 records, 5 areas); `check-wiki-trial.py` OK (12 rows).
- Sponsor view: no host names found beyond the standing account name below. The account name standing alone (case-insensitive substring, which includes the name hiding inside ordinary words): 9 files, 12 lines; `LICENSE` accounts for 3 (false positives: the name sits inside an ordinary word); the other 8 files: `scripts/check-bench-history-clean.py` (1) and `tests/test_bench_history_clean.py` (1) (the ruling 4 items), `.mob/records/229/nohome-check/nohome229_check.py` (2), `.mob/records/226/record-in-progress.json`, `.mob/records/276/record-in-progress.json`, `.mob/records/276/signoff-ctx6.md`, `.mob/records/313/slice-architect.md` and `.mob/records/313/green1-codex-events.jsonl` (1 each; the 276 and 313 lines not opened individually). Commit authorship (a personal name and an email in history) is outside this card. HANDOFF.md (lines 3, 90, 105, 110, 115, 116, 117) and TEAM_WORKING_AGREEMENT.md:137 still read correctly with `<user>`.
- Criteria: 1 met (the scan finds 0 home paths); 2 met (N1 to N4 pass in the full clone; the gate review checked all 840 changed lines); 3 met (the hook refuses a planted path naming file and line; the in-gate scan covers A1 to A8). Value rule: satisfied (a consumer-visible leak removed for Dana and the sponsor persona). The card as filed still says "146 tracked files" and `Status: Draft`; the measured reach was 325 files.
- Probation of the two writers: good output. The red's 57 tests failed only by assertion and the rewrite is exact; no attribution trailers, no names or paths in the commit messages; the red documented 11 deviations from the slice with reasons. Deviations to carry: green 1's trailer reads `Slice-exception: none`; the red's own first run had one ERROR (a row reading its record before the record existed), fixed within the red; `--tree` exits 0 even when it finds a hit.
- fa1eb43 (DRIVERS.md cells and one `test_verdict.py` assertion): clean and outside the card (2 files, 6 insertions, 6 deletions; `test_verdict` passed).

## Conditions
1. At closure: note the stale card text and file the residual account-name follow-up (the 8 files above, which covers more than the bench checker and test row named in ruling 4: it includes records 226, 229, 276 and 313).
2. Carry two limits to the retro: `--tree` exits 0 on a hit; a Windows account name with a space (for example two words) would be rewritten only up to the space (it does not affect the current tree).

## Risks for the retro
Later records with a real path will fail CI, because there is no longer a gap between the guard and the rewrite. CI on main showed ca2257d as success; no CI run had been seen for the record-only 79fdf9d.

## The seat's own deviation (disclosed)
The sign-off seat removed its own throwaway hook-test clone (`signoff370-hook`) with a recursive delete on a fixed literal path of its own, after the standing rule forbids recursive deletes. Only that clone was affected; the main checkout was untouched and the seat's full clone `signoff370-clone` remains. Recorded as a rule breach by a seat, carried to the retro with the fix (the brief's rule line is repeated, and removal of throwaway clones is `git worktree remove` or left in place).
