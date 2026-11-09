# Card 370 red (D157 Criteria 370-1 and 370-3), QA (red) seat

Two Pending new-state criterion tests in one red commit (Ruling 1 of the Ready draft: D204 over D137's one-pending rule, as on cards 361, 366 and 371), with a non-Pending characterisation Fact and the stub module. No tracked file changed: nothing under `src/` or `docs/`, no rewrite, no edit to the board generator, the hook or `check-no-secrets.py`. Pre-change tree and base: `88ce4470a9308cbd1f7c191cdb86c042cf2ba3e4` (recorded in `scrub-commits.txt`). The characterisation rows are declared in `characterisation-record.md`.

- `NoHomePathsInTrackedFiles.Tracked_files_carry_no_home_path_of_a_local_account` (`Slice=370`, `Criterion=370-1`, `Status=Pending`): runs `tests.test_home_paths_370.TreeScan` (A1-A8) and `GeneratorRow` (two A2 generator rows).
- `PreCommitRefusesAHomePath.The_hook_names_the_file_that_adds_a_home_path` (`Slice=370`, `Criterion=370-3`, `Status=Pending`): runs `PreCommitRefusesAHomePath` (G1, G2, the hook row) and `ScannerRows` (F1-F9 planted rows).
- `NoHomePathsInTrackedFiles.The_rewrite_changes_only_account_segments_and_history_stays` (`Slice=370` only): runs `ScrubChangesOnlyAccountSegments` (N1-N4, the reach, own files, the `MIGRATION.txt` pin).

Each wrapper starts `python -m unittest` through `ProcessRunner.RunAsync` (the `LicenseFiles.cs` shape), one `ArgumentList` entry per test id.

Reading note: this record shows paths as `<user>` and quotes no matched text; the failure lines below name a file, a line and a form id only. Tracebacks are not quoted, because Python prints the absolute path of the test file.

## D222 list: the Ready draft's rows first

| id | construct | kind | test |
|---|---|---|---|
| A1 | `.mob/` (records, decisions) | NEW-STATE (370-1) | `TreeScan.test_a1_mob_records` |
| A2 | `board/` | NEW-STATE (370-1) | `TreeScan.test_a2_board`, and `GeneratorRow.test_a2g_placeholders_are_kept`, `GeneratorRow.test_a2g_a_literal_account_is_neutralised` ("a regenerated board archive also scans clean") |
| A3 | tests | NEW-STATE (370-1) | `TreeScan.test_a3_tests` |
| A4 | scripts | NEW-STATE (370-1) | `TreeScan.test_a4_scripts` |
| A5 | docs | NEW-STATE (370-1) | `TreeScan.test_a5_docs` |
| A6 | `HANDOFF.md` and `TEAM_WORKING_AGREEMENT.md` | NEW-STATE (370-1) | `TreeScan.test_a6_handoff_and_working_agreement` |
| A7 | bench | NEW-STATE (370-1) | `TreeScan.test_a7_bench` |
| A8 | other | NEW-STATE (370-1), a catch-all: 0 hits today, so it passes on the pre-change tree | `TreeScan.test_a8_other` |
| F1 | backslash form, one, two or four backslashes (the draft's F1 and F2) | NEW-STATE unit (370-3) | `ScannerRows.test_hit_F1_single`, `test_hit_F1_double`, `test_hit_F1_quad` |
| F2 | drive, colon, forward slashes (the draft's F3, first half) | NEW-STATE unit | `test_hit_F2_forward` |
| F3 | lowercase users folder (the draft's F3, second half) | NEW-STATE unit | `test_hit_F3_lowercase` |
| F4 | `/c/` and `/mnt/c/` roots | NEW-STATE unit | `test_hit_F4_drive_root`, `test_hit_F4_mnt` |
| F5 | project-folder slug | NEW-STATE unit | `test_hit_F5_slug` |
| F6 | macOS users root, case-sensitive | NEW-STATE unit | `test_hit_F6_macos` |
| F7 | Unix home root | NEW-STATE unit | `test_hit_F7_unix_home` |
| F8 | file URL and URL-encoded | NEW-STATE unit | `test_hit_F8_file_url`, `test_hit_F8_url_encoded` |
| F9 | the segment ends a line, the file (no final newline), or is followed by a double quote, a single quote, a backtick, a space or a dot | NEW-STATE unit | `test_hit_F9_end_of_line`, `_double_quote`, `_single_quote`, `_backtick`, `_space`, `_file_end_no_newline`, `_trailing_dot` |
| G1 | a staged non-`MOB.md` file adds a home path: exit 1, file and line named, account not printed | NEW-STATE (370-3) | `PreCommitRefusesAHomePath.test_g1_a_staged_home_path_is_refused` |
| G2 | the same file with the placeholder: exit 0 | CONTROL (370-3), passes on the pre-change tree | `PreCommitRefusesAHomePath.test_g2_the_neutral_form_commits_cleanly` |
| N1 to N4 | rewrite equals the rule, no file missing, untracked files and line ends kept, history kept | CHARACTERISATION | `ScrubChangesOnlyAccountSegments` (see `characterisation-record.md`) |

Further rows (no draft id): the must-not-hit rows `ScannerRows.test_no_hit_*` (16: the placeholder in every form F1-F8, `%USERPROFILE%` and `SpecialFolder.UserProfile`, `~/`, the CI runner home, the `api.github.com` users URL, a URL `/home/` path, the `x` placeholder, the two file-pinned fixtures); `ScannerRows.test_hit_pinned_segment_in_another_file` (a pinned segment is a hit outside its file); `ScannerRows.test_hits_carry_no_text`; `PreCommitRefusesAHomePath.test_the_hook_still_calls_the_guard`; and the characterisation rows R1 (reach), R2 (own files), R3 (`MIGRATION.txt`).

## The slice's changes, with the reason

| change | reason (architect's slice) |
|---|---|
| forms numbered as the slice's section 1 and the comments of its rule (F1 any backslash count, F2 forward slash, F3 lowercase), not as the draft's list (F2 doubled, F3 forward slash and lowercase) | sections 2 to 5 of the slice are binding and its rule's comments use this numbering; the table above maps each draft id |
| the rule is the slice's section 2, verbatim, in `scripts/home_paths.py` (`SEGX`, `SEG`, `SEP`, `NB`, `PATH`, `SLUG`) with the allow-list by segment (`runner`, `runneradmin`, `x`, `...`, the ellipsis) and by file (`someone` in `tests/test_bench_history_clean.py`, the `CSharpBaselineAcceptance.cs` fixture account) | the slice fixes the rule, the allow-list and the reasons |
| no allow-list entry for the bundle under `.mob/records/196` or for `api.github.com/users/` | the slice measured: the bundle has a NUL byte and is skipped, and the rule does not match the URL |
| the reach is measured with the rule, not `git grep`: 325 files and 932 lines (A1 251 / 813, A2 61 / 77, A3 4 / 11, A4 3 / 6, A5 3 / 3, A6 2 / 9, A7 1 / 13, A8 0); 327 files under the generic rule (the two pinned fixtures) | equals the slice's section 1 figures exactly |
| D191 is outside the 106 pinned records; the rule changes one pinned record, D51 | measured by R3 (the draft asked the red to measure it) |

## This seat's own choices (deviations from the slice, with the reason)

| change | reason |
|---|---|
| `TreeScan` detects a hit with `home_paths.scrub_bytes` (a line the rule would change, file pins applied), joined with the `find` hits | with the stub's `find` returning nothing, a `find`-only scan would pass vacuously; the facilitator's brief asked for this. After green 1 the union also counts any extra `find` hit |
| the form id in a `TreeScan` message comes from a small classifier in the test (`form_of`), not from `find` | the stub has no classification; the id is only used in the message |
| `ScannerRows` are run by the 370-3 wrapper, beside G1 and G2 | CI runs only `dotnet test`; with no wrapper the scanner rows would never be gated. They turn green with the guard in green 1, which also removes Pending from 370-3 |
| the stub also carries a real `scrub_text` and `scrub_tracked` (with the CLI), and no `account_root_re` | N3 calls `scrub_tracked` and must pass on the pre-change tree; both are thin wrappers of the real `scrub_bytes`. `account_root_re` is green 1's (it serves `migrate_cards` only) |
| `scrub_bytes(data, path=None)`: with a path the file pins apply, without one the generic rule | the slice counts 327 files for the generic rule and 325 with `FILE_ALLOW`; one function gives both |
| N1 compares a target blob with the rule applied only when it differs from its base blob | the slice's wording (target equals the rule applied) cannot pass before green 2, when the target equals the base; a declared characterisation row must pass on the pre-change tree |
| R3 always checks that the proof over the rule-applied base records differs from the old pin, and checks the target's pin only once `MIGRATION.txt` differs from the base | same reason; the target half is reached by mutation M4 in the characterisation record |
| `api.github.com/users/x` row also holds the same URL with a non-allow-listed segment | mutation M1 showed the `x`-only sample could not fail (`x` is allow-listed) |
| two A2 generator rows (placeholders kept; a literal account in an issue body becomes the placeholder) | the slice's green 1 says the generator keeps the placeholders and passes bodies through `scrub_text`; one row per behaviour |
| G1 plants three lines (single backslash, doubled backslash, slug) and expects each line named | the draft says the scenario fails if the guard misses the escaped forms |
| the fixture account of `CSharpBaselineAcceptance.cs` is built from parts in the module | no first name is written as one token in any file of this commit |

## Pre-change failure line per id

`python -m unittest -v tests.test_home_paths_370` against the stub (test file path withheld; one line per id, then each assertion line):

```
GeneratorRow.test_a2g_a_literal_account_is_neutralised ... FAIL
GeneratorRow.test_a2g_placeholders_are_kept ... FAIL
PreCommitRefusesAHomePath.test_g1_a_staged_home_path_is_refused ... FAIL
PreCommitRefusesAHomePath.test_g2_the_neutral_form_commits_cleanly ... ok
PreCommitRefusesAHomePath.test_the_hook_still_calls_the_guard ... ok
ScannerRows.test_hit_F1_double ... FAIL
ScannerRows.test_hit_F1_quad ... FAIL
ScannerRows.test_hit_F1_single ... FAIL
ScannerRows.test_hit_F2_forward ... FAIL
ScannerRows.test_hit_F3_lowercase ... FAIL
ScannerRows.test_hit_F4_drive_root ... FAIL
ScannerRows.test_hit_F4_mnt ... FAIL
ScannerRows.test_hit_F5_slug ... FAIL
ScannerRows.test_hit_F6_macos ... FAIL
ScannerRows.test_hit_F7_unix_home ... FAIL
ScannerRows.test_hit_F8_file_url ... FAIL
ScannerRows.test_hit_F8_url_encoded ... FAIL
ScannerRows.test_hit_F9_backtick ... FAIL
ScannerRows.test_hit_F9_double_quote ... FAIL
ScannerRows.test_hit_F9_end_of_line ... FAIL
ScannerRows.test_hit_F9_file_end_no_newline ... FAIL
ScannerRows.test_hit_F9_single_quote ... FAIL
ScannerRows.test_hit_F9_space ... FAIL
ScannerRows.test_hit_F9_trailing_dot ... FAIL
ScannerRows.test_hit_pinned_segment_in_another_file ... FAIL
ScannerRows.test_hits_carry_no_text ... FAIL
ScannerRows.test_no_hit_* (16 rows) ... ok
ScrubChangesOnlyAccountSegments.* (7 rows) ... ok
TreeScan.test_a1_mob_records ... FAIL
TreeScan.test_a2_board ... FAIL
TreeScan.test_a3_tests ... FAIL
TreeScan.test_a4_scripts ... FAIL
TreeScan.test_a5_docs ... FAIL
TreeScan.test_a6_handoff_and_working_agreement ... FAIL
TreeScan.test_a7_bench ... FAIL
TreeScan.test_a8_other ... ok

AssertionError: 0 != 2 : A2g literal: 2 generated lines hold a home path
AssertionError: 2 != 0 : A2g placeholders: 0 of 2 kept in the generated card
AssertionError: 1 != 0 : G1: expected exit 1, got 0
AssertionError: Lists differ: [(1, 'F1')] != []      (F1_double; then F1_quad [(1, 'F1')], F1_single [(2, 'F1')],
                                                       F2_forward [(3, 'F2')], F3 [(1, 'F3')], F4 x2 [(1, 'F4')],
                                                       F5 [(1, 'F5')], F6 [(1, 'F6')], F7 [(1, 'F7')], F8 x2 [(1, 'F8')],
                                                       F9 backtick, double quote, end of line [(1, 'F1')],
                                                       F9 file end [(2, 'F2')], F9 single quote [(1, 'F2')],
                                                       F9 space, trailing dot, pinned elsewhere [(1, 'F1')]; each "!= []")
AssertionError: 2 != 0 : expected 2 hits, got 0
AssertionError: 0 != 813 : A1 .mob/: 813 hits in 251 files (first: .mob/records/176/agreement-candidate.md:137:F2, .mob/records/176/red-record.md:47:F1, .mob/records/176/red-record.md:52:F1)
AssertionError: 0 != 77 : A2 board/: 77 hits in 61 files (first: board/036-live-mob-dashboard.md:24:F1, board/036-live-mob-dashboard.md:25:F1, board/036-live-mob-dashboard.md:26:F1)
AssertionError: 0 != 11 : A3 tests/: 11 hits in 4 files (first: tests/board/test_board_archive.py:235:F1, tests/board/test_board_migrate.py:19:F1, tests/board/test_board_migrate.py:20:F2)
AssertionError: 0 != 6 : A4 scripts/: 6 hits in 3 files (first: scripts/board/check_real_board.py:291:F1, scripts/board/generate_archive.py:32:F1, scripts/board/migrate_cards.py:33:F1)
AssertionError: 0 != 3 : A5 docs/: 3 hits in 3 files (first: docs/decisions/compiler-and-adapters.md:29:F1, docs/decisions/process-and-staffing.md:1028:F2, docs/retros/experiments.md:405:F2)
AssertionError: 0 != 9 : A6 HANDOFF.md and TEAM_WORKING_AGREEMENT.md: 9 hits in 2 files (first: HANDOFF.md:3:F1, HANDOFF.md:35:F1, HANDOFF.md:90:F2)
AssertionError: 0 != 13 : A7 bench/: 13 hits in 1 files (first: bench/csharp-baseline/results/history.jsonl:18:F1, bench/csharp-baseline/results/history.jsonl:34:F1, bench/csharp-baseline/results/history.jsonl:45:F1)
Ran 57 tests
FAILED (failures=31)
```

31 failures by assertion: 7 `TreeScan` rows, 2 `GeneratorRow` rows, G1, 20 must-hit scanner rows and `test_hits_carry_no_text`. Every one is an assertion (no missing file, import or compile error). The 26 passes are G2, the hook row, A8, the 16 must-not-hit rows and the 7 characterisation rows. (The first run also had one ERROR, the own-files row reading this record before it existed; it passes now.)

The C# run, with the two Pending traits removed (uncommitted, restored afterwards and checked with `rg -n Trait` on both files), after `dotnet build src/Avowry.Cli` (`Build succeeded.`, `0 Error(s)`):

`dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Slice=370"`

```
Failed Avowry.Acceptance.Tests.PreCommitRefusesAHomePath.The_hook_names_the_file_that_adds_a_home_path [1 s]
  AssertionError: 1 != 0 : G1: expected exit 1, got 0
  (20 must-hit rows "Lists differ: [(line, 'Fn')] != []", and "expected 2 hits, got 0")
  Ran 40 tests, FAILED (failures=22)
Failed Avowry.Acceptance.Tests.NoHomePathsInTrackedFiles.Tracked_files_carry_no_home_path_of_a_local_account [3 s]
  (the seven TreeScan lines above, A1 to A7, and the two A2g lines)
  Ran 10 tests, FAILED (failures=9)
Failed!  - Failed:     2, Passed:     1, Skipped:     0, Total:     3, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)
```

The one pass is the characterisation Fact.

## Checks with the Pending tests committed

`dotnet test Avowry.sln --nologo --filter "Status!=Pending"` (the new files untracked at the time, the Pending traits in place):

```
Passed!  - Failed:     0, Passed:   344, Skipped:     0, Total:   344, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:   385, Skipped:     0, Total:   385, Duration: 9 m 41 s - Avowry.Acceptance.Tests.dll (net8.0)
```

The 385 include the characterisation Fact. A first run of the same command, made while the Python suite below ran at the same time, had one failure, `BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms` (`Failed:     1, Passed:   384`, the test took 30 s); run alone it passed (`Passed:     1`, 9 s), and the run above was made with nothing else running. It is a timing test outside this card.

`python -m unittest tests.test_home_paths_370 tests.test_license_files tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266`: `Ran 118 tests`, `FAILED (failures=39)`: the 31 new-state ids above, and the eight pre-existing failures (five in rename_196: `test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`; two in rename_197: `test_lists_equal_baseline` x2; T11 in live_docs_266). Without the new module, `python -m unittest tests.test_license_files tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266`: `Ran 61 tests`, `FAILED (failures=8)`, the same eight.

## Test limits noticed

- A8 and G2 cannot fail on the pre-change tree: A8 is a catch-all with 0 hits today, G2 is the control. The must-not-hit rows pass against the stub's `find`; they are made able to fail by their `scrub_bytes` half (mutation M1 in the characterisation record turns the two URL rows red), but their `find` half is only exercised after green 1.
- `TreeScan` reads the working tree: in a dirty tree an uncommitted edit counts. Files with a NUL byte are skipped (one today, the bundle under `.mob/records/196`); UTF-16 text is not decoded (the slice found none).
- The form id in a `TreeScan` message comes from the test's own classifier; a wrong form id would not fail anything.
- The characterisation Fact passes in CI on a skip: the depth-1 checkout lacks the base commit, so N1, N2, N4, R1 and R3 skip and the wrapper sees exit 0. Only N3 and R2 run there. The gate reviewer must run the class in a full clone.
- N1 compares only base files that the rule changes, and fails on any other edit to them between the base and the target: a later edit to `HANDOFF.md` (a living file) before the `after` line is recorded would fail N1 although it is not the scrub's.
- N1 does not compare the seven hand-edited green 1 files; their review is the gate reviewer's.
- R3's target half is not reached until `MIGRATION.txt` changes; it was reached only by mutation M4.
- G1 runs `check-no-secrets.py` directly, not the hook; the hook row checks only that the hook still names the script.
- The account name standing alone (Ruling 4) is out of scope and no row looks for it.
