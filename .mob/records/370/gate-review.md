# Card 370 independent gate review (fresh Claude context, read-only in the main checkout; own full clone)

The red was written by a Cursor WRITING seat and the greens by a Muse WRITING seat (both Probation rung 1): the reviewer checked their claims independently.

**Step 0** (clone of main, tested commit `ca2257dcf7e732514aac3bc127db9a18f142a3e5`):
- `git merge-base --is-ancestor 0a3ec382725f5e753bfae8d306cd6d083e834937 ca2257dcf7e732514aac3bc127db9a18f142a3e5`: exit status 0.
- `git merge-base --is-ancestor e19198dd2d2901b808f14bca60db1aa367595c22 ca2257dcf7e732514aac3bc127db9a18f142a3e5`: exit status 0.

**Verdict: SHIP.** No blocking finding; two limits and one `--tree` behaviour that differs from the brief.

**Gate evidence.** Tested commit ca2257dcf7e7; a full clone made with `--no-hardlinks` (the origin URL reset to GitHub so `gh` works), no uncommitted changes while the gate ran; dotnet processes at the start: 0 (one build-server process remained after the runs). Two scratch worktrees removed with `git worktree remove`; no recursive delete; the main checkout untouched. **Load:** nothing else ran during the dotnet gate (about 10 minutes).

## 1. Order and red-first
Order: slice 88ce447, red 0a3ec38, green 1 626172d, green 2a a886f63, green 2b e19198d. Between 0a3ec38 and the tip the two C# red test files differ only by one deleted `Pending` trait line each (green 1 removed it from `PreCommitRefusesAHomePath.cs`, green 2b from `NoHomePathsInTrackedFiles.cs`); `tests/test_home_paths_370.py` has an empty diff after the red. Red replay (detached worktree at 0a3ec38): `python -m unittest -v tests.test_home_paths_370`: 57 tests, 31 failures, all by assertion and none by error: A1 to A7 (813, 77, 11, 6, 3, 9 and 13 hits), the generator rows, the F1 to F8 hit rows and G1. CLI built; Pending traits removed uncommitted: `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Slice=370"`: 2 failed, 1 passed (both failures by assertion: the 370-1 fact (A1 to A7) and the G1 fact; the characterisation fact passed).

## 2. The rewrite is exact
At 626172d, `python scripts/home_paths.py --root . --apply` rewrote 258 files; its tree differs from a886f63 only in `docs/decisions/MIGRATION.txt` (the re-pinned hash line). `git diff --numstat 626172d a886f63` covers 259 files and added equals deleted in every one. All 840 changed lines were checked (not a sample): each new line equals the old line with only an account segment swapped for `<user>` (no bad lines). The changes cover `.mob` (251 files), HANDOFF.md, TEAM_WORKING_AGREEMENT.md, bench history, docs (4) and tests (1); exactly one real account segment was replaced. CR count, LF count and the final-newline state are unchanged in all 258 files (37 contain CRLF; 73 contain the escaped double-backslash form). Untracked files: the rewrite reads only tracked paths; nothing untracked or ignored after the apply; the red record's N3 passes. e19198d touched 61 `board/` files plus the one Pending trait line and nothing else (numstat 138 added, 139 deleted; the added lines use `<phrase-lang>`, `<mob-defaults>` and similar placeholders). `python scripts/board/generate_archive.py --check` exits 0 with `gh` logged in.

## 3. MIGRATION.txt re-pin
Recomputed `proof(records, metadata, 106)` gives `(106, 1c6ee8d99d6b3be9280eda3b682383f5465a5c283ac3f23bc8fa11419e007147)`, matching the file. `check-decisions-index.py` passes (227 records, 5 areas); `check-wiki-trial.py` passes (12 rows); `mob_process.py check` passes (129 closed cards, 38 records, 91 legacy).

## 4. The guard (separate throwaway clone and branch, planted path built from parts)
With the planted file staged, `check-no-secrets.py` exits 1 and prints `docs/plant1.md:2: home path (F1)` (the account does not appear). The same line with `<user>` exits 0. A deleted line that held a path exits 0. `.githooks/pre-commit` still calls `check-no-secrets.py` first. 22 planted samples checked against `scripts/home_paths.py` (`find()` and `scrub_text`): F1 with one, two and four backslashes, F2 to F8 and the URL-encoded form, end of line, double quote, backtick, space, no final newline, CRLF and a trailing dot: every one gave the right form, changed on scrub, left no residual hit and no leaked account; `runner` and an already-`<user>` segment gave no hit. `find()` and `scrub_bytes` use the same regex objects, so they agree by construction (not an independent cross-check). `--tree` printed per-area counts plus `file:line:form` for each hit and exits 0 even with a hit (the brief said counts only; the output still shows no account, but a CI use of `--tree` cannot rely on the exit code).

## 5. Coverage (D222) and Probation checks
All of A1 to A8, F1 to F9, G1, G2 and N1 to N4 appear in `red-record.md` (table at lines 17 to 36) and map to tests in `tests/test_home_paths_370.py` (A in TreeScan plus the generator rows; F in ScannerRows; G in PreCommitRefusesAHomePath; N in ScrubChangesOnlyAccountSegments). Files checked: `taskforce-proposal-pm.md`, `slice-architect.md`, `red-record.md`, `characterisation-record.md`, `scrub-commits.txt`, `checklist-d235.md`, `declaration-proposal.md` under `.mob/records/370/`; the commit messages of the red and the greens; `scripts/home_paths.py`, `scripts/check-no-secrets.py`, `.githooks/pre-commit`, `scripts/board/generate_archive.py`. Probation: no attribution trailer on the red or the greens (only `Criterion` and `Slice-exception`; two messages say "Written by a Muse writing seat" in prose); all commit messages LF only; no account name or home path in any commit message. Not tested by the reviewer: whether the seats ran a recursive delete.

## 6. Mutation table (reviewer's clone only, restored, working tree clean afterwards)
| Mutant | Result |
|---|---|
| (a) `find()` returns `[]` | Killed: 22 failures including G1 and the F1 to F9 hit rows |
| (b) drop the trailing-dot rule | Killed: `test_hit_F9_trailing_dot` and `test_n1_every_change_is_the_rule_applied_to_the_base_blob` |
| (c) remove the `runner` allow-list entry | Killed: `test_a1_mob_records`, `test_a3_tests`, `test_no_hit_ci_runner` |
| (d) planted path in `scripts/check-unsafe-deletes.py` | Killed: `test_a4_scripts`, naming area and `file:line:form` |
| (e) generator expands placeholders | Killed: `test_a2g_placeholders_are_kept`, `test_a2g_a_literal_account_is_neutralised` |
| (f) MIGRATION hash changed by one hex | Survived in `test_home_paths_370` (limit 1); `check-decisions-index.py` caught it ("sha256 does not match") |
| (g) path in `docs/retros/experiments.md` | Killed: `test_a5_docs` (history is not exempt) |
| (h) staged scan skips `home_paths` | Killed: `test_g1_a_staged_home_path_is_refused` |
The G1 mutant was also confirmed independently with a real staged file (section 4).

## 7. Gate runs
`dotnet build src/Avowry.Cli`: 0 errors. `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`: exit 0; Compiler tests 344 passed, Acceptance tests 387 passed (9m39s); no `BenchmarkHarnessAcceptance` flake. `python -m unittest tests.test_home_paths_370 tests.test_verdict tests.test_license_files tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266`: 139 tests, 8 failures, no errors, none skipped: the expected eight (five rename_196: `test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`; two rename_197: `test_lists_equal_baseline`; the T11 test in live_docs_266; not re-run at the parent commit). The seven `ScrubChangesOnlyAccountSegments` rows ran against the full clone with the base commit present, all passed, none skipped.

## Test limits (not blocking)
1. Mutant (f): `test_the_migration_pin_...` reads MIGRATION.txt from the committed target blob, so an uncommitted edit is invisible to it; `check-decisions-index.py` (a pre-commit and CI check) covers that case.
2. The segment rule ends at a space: a Windows account like "Jo Bloggs" is rewritten to `<user> Bloggs`, leaving the second word in the file (verified with a planted sample; by design in F9, but a residual leak for such accounts). The 258-file rewrite had exactly one account segment, without a space, so the current tree is unaffected.
3. The `--tree` exit-code behaviour noted in section 4.
