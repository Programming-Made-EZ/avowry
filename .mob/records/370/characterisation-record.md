# Card 370 characterisation (D204), QA (red) seat

Declared CHARACTERISATION, not a red. These rows must pass on the pre-change tree: this worktree changes no tracked file (no rewrite, no generator, guard or hook edit, nothing under `src/` or `docs/`). Base: `88ce4470a9308cbd1f7c191cdb86c042cf2ba3e4` (main, the architect's slice commit; the red commit's parent), recorded as the `base` line of `.mob/records/370/scrub-commits.txt`. Branch `wt-370-red`.

Reading note: no file this seat wrote holds a home path or an account name. Paths are shown as `<user>`; every test sample is built from parts at run time (a fictional account, the drive, the users folder and the separators joined in code); the module's patterns cannot match their own text. The row "this card's own files" below checks that.

## Declared up front

All rows are in `tests/test_home_paths_370.py`, class `ScrubChangesOnlyAccountSegments`, run by the non-Pending Fact `NoHomePathsInTrackedFiles.The_rewrite_changes_only_account_segments_and_history_stays` (`Slice=370`, no Criterion, no Status). The target is the `after <sha>` line of `scrub-commits.txt` once the facilitator adds it after green 2b; without it, HEAD. Rows that need the base skip when the base commit is absent (a depth-1 clone, as in CI); a skip is not a pass, so the gate reviewer runs them in a full clone.

| Id | Test | Kind | Why it passes on the pre-change tree and after green 2 |
|---|---|---|---|
| N1 | `test_n1_every_change_is_the_rule_applied_to_the_base_blob` | CHARACTERISATION | For every base file the rule changes (`scrub_bytes(base, path) != base`; 325 files), a target blob that differs from its base blob must equal the rule applied to the base blob. Exempt: the seven green 1 hand-edit files listed in the test (`generate_archive.py`, `check_real_board.py`, `migrate_cards.py`, `check-no-secrets.py`, `test_board_archive.py`, `test_board_migrate.py`, `test_board_sync.py`), the two file-pinned fixtures, and `board/`, where only "no home path left" is checked. Today every target blob equals its base blob, so it holds; after green 2 each equals the rule applied. A non-rule edit to a path-bearing file fails it (mutation M2). |
| N2 | `test_n2_no_base_file_is_missing_at_the_target` | CHARACTERISATION | Every path of the base tree exists at the target (no file deleted or moved). |
| N3 | `test_n3_the_rewrite_leaves_untracked_files_and_line_ends_alone` | CHARACTERISATION (needs no base) | `home_paths.scrub_tracked` in a temporary git repo (made with `tempfile.mkdtemp`, left in place) rewrites a tracked CRLF file and a tracked file with no final newline exactly by the rule (CRLF count kept, no newline added), and leaves an untracked file, an excluded `board/` file and a file with a NUL byte byte-identical. |
| N4 | `test_n4_history_keeps_the_base_blob_and_the_base_is_an_ancestor` | CHARACTERISATION | The base blob of `HANDOFF.md` still holds its home path, and `git merge-base --is-ancestor <base> <target>` exits 0 (history is not rewritten). |
| R1 | `test_the_reach_is_not_empty` | CHARACTERISATION | The rule changes at least one base file, and at least one outside `board/` (measured: 325 files with the file pins applied; 327 under the generic rule, as the slice says). |
| R2 | `test_this_cards_own_files_hold_no_home_path` | CHARACTERISATION (needs no base) | The seven files of this commit (`scripts/home_paths.py`, `tests/test_home_paths_370.py`, the two C# wrappers, this record, `red-record.md`, `scrub-commits.txt`) hold no line the rule would change. |
| R3 | `test_the_migration_pin_is_the_proof_over_the_rule_applied_records` | CHARACTERISATION | On the base: `MIGRATION.txt` pins `count 106`; the 106 records end at D114; the old hash equals `decision_files.proof` over the base records; the rule changes exactly one pinned record, D51 (D191 is outside the 106); the proof over the rule-applied records differs from the old hash. On the target: when `MIGRATION.txt` differs from the base, it must hold `count 106` and the proof over the rule-applied base records. Today the target equals the base, so the second half is not reached; mutation M4 reaches it. |

## Commands and pass lines (pre-change tree)

`python -m unittest -v tests.test_home_paths_370.ScrubChangesOnlyAccountSegments`:

```
test_n1_every_change_is_the_rule_applied_to_the_base_blob (...) ... ok
test_n2_no_base_file_is_missing_at_the_target (...) ... ok
test_n3_the_rewrite_leaves_untracked_files_and_line_ends_alone (...) ... ok
test_n4_history_keeps_the_base_blob_and_the_base_is_an_ancestor (...) ... ok
test_the_migration_pin_is_the_proof_over_the_rule_applied_records (...) ... ok
test_the_reach_is_not_empty (...) ... ok
test_this_cards_own_files_hold_no_home_path (...) ... ok
Ran 7 tests
OK
```

(The full per-id output of the whole module, and the C# Fact's result, are in `red-record.md`.)

## Mutations (in memory, no file edited)

Each was run from one `python -` session that patched a name in the loaded module and ran the named rows with `unittest.TextTestRunner`; nothing was written to disk.

1. M1, the F6/F7 branch of `PATH` made case-insensitive and without its left boundary: the must-not-hit rows protected by the rule (not by the allow-list) turn red. Before this mutation was run, the `api.github.com/users/x` row passed under it too: `x` is an allow-listed segment, so the row could not fail. The row now also holds the same URL with a non-allow-listed segment.

```
test_no_hit_api_users_url ... FAIL
test_no_hit_url_home_path ... FAIL
AssertionError: ... : api_users_url: the rewrite rule changed it
AssertionError: ... : url_home_path: the rewrite rule changed it
FAILED (failures=2)
```

2. M2, N1 with the base set to the parent of `2d58c6c` (two `HANDOFF.md` edits back): a path-bearing file with a non-rule difference fails N1.

```
test_n1_every_change_is_the_rule_applied_to_the_base_blob ... FAIL
AssertionError: Lists differ: [] != ['HANDOFF.md: differs from the rule applie[260 chars]lob']
FAILED (failures=1)
```

3. M3, `scrub_tracked` without its `board/` exclusion:

```
test_n3_the_rewrite_leaves_untracked_files_and_line_ends_alone ... FAIL
AssertionError: Lists differ: ['crlf.txt', 'tail.txt'] != ['board/card.md', 'crlf.txt', 'tail.txt']
FAILED (failures=1)
```

4. M4, a re-pinned `MIGRATION.txt` at the target: a wrong hash (64 zeros) fails, the proof over the rule-applied base records passes (`1c6ee8d99d6b3be9280eda3b682383f5465a5c283ac3f23bc8fa11419e007147`, the value green 2 should pin if the rule is unchanged).

```
M4a wrong re-pin: test_the_migration_pin_is_the_proof_over_the_rule_applied_records ... FAIL
AssertionError: Regex didn't match: '(?m)^sha256 1c6ee8d9...\r?$' not found in 'count 106\nsha256 0000...\n'
M4b right re-pin: test_the_migration_pin_is_the_proof_over_the_rule_applied_records ... ok
```
