# Card 376 red record (RED writing seat, worktree card-376-red, uncommitted)

The account name is never printed here. Counts were taken by command; planted names in the
tests are built at run time from parts plus a random suffix.

## What the red adds

- Red stubs only, no production behaviour change: `local_accounts` in
  [home_paths.py](/scripts/home_paths.py) returns the unfiltered machine names;
  `find(path, text, accounts=())` accepts and ignores `accounts`;
  `scan_history(path, accounts=None)` in
  [check-bench-history-clean.py](/scripts/check-bench-history-clean.py) accepts and ignores
  `accounts`. The old literal bench pattern and the old checker behaviour are untouched.
- [test_account_name_376.py](/tests/test_account_name_376.py): WholeWordTreeScan (W-A1 to
  W-A8), LocalAccountRows (U1), TreeScanGatesByExitCode (T1 to T4, G3, G4), SpacedAccountRows
  (S1 to S6, S1eol, S7), SpaceRuleChangesNothingToday (C1), RecordRewriteIsTheRule (R1).
- Class `BenchHistoryNameRows` (B1 to B6) added to
  [test_bench_history_clean.py](/tests/test_bench_history_clean.py); existing classes unchanged.
- Four C# Facts in three files: [NoAccountNameInTrackedFiles.cs](/tests/Avowry.Acceptance.Tests/NoAccountNameInTrackedFiles.cs)
  (criterion 376-1, Pending; plus the characterisation Fact, Slice only),
  [TreeScanGatesByExitCode.cs](/tests/Avowry.Acceptance.Tests/TreeScanGatesByExitCode.cs)
  (376-2, Pending), [AccountNameWithASpace.cs](/tests/Avowry.Acceptance.Tests/AccountNameWithASpace.cs)
  (376-3, Pending). No ProcessStartCallers change.

## Runs

- `python -m unittest tests.test_account_name_376 tests.test_bench_history_clean`:
  33 tests, FAILED (failures=24 counting subtests; 19 failing test methods, listed below).
  Every new-state failure is an AssertionError, none is AttributeError or TypeError.
- `python -m unittest tests.test_home_paths_370`: 57 tests, FAILED (failures=1), the known
  baseline N1 below. Unchanged by this red.
- `dotnet build tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj`: succeeded,
  0 warnings, 0 errors.
- `dotnet test tests/Avowry.Acceptance.Tests --filter "FullyQualifiedName~NoAccountNameInTrackedFiles|FullyQualifiedName~TreeScanGatesByExitCode|FullyQualifiedName~AccountNameWithASpace"`
  (this filter runs the Pending Facts too): Failed 3, Passed 1. The three Pending criterion
  Facts fail; the Slice-only characterisation Fact (C1, R1) passes.

## Per-id status now

Failing, new state (376-1):

- W-A1: `0 != 7 : A1 .mob/: 7 hits in 6 files (first: .mob/records/226/record-in-progress.json:208:F10, .mob/records/229/nohome-check/nohome229_check.py:20:F10, .mob/records/229/nohome-check/nohome229_check.py:48:F10)`
- W-A3: `0 != 1 : A3 tests/: 1 hits in 1 files (first: tests/test_bench_history_clean.py:32:F10)`
- W-A4: `0 != 1 : A4 scripts/: 1 hits in 1 files (first: scripts/check-bench-history-clean.py:9:F10)`
- U1: `('qaacct7',) != ('runner', 'Runner', 'qaacct7', 'QAACCT7', 'ab', 'root') : U1: expected ('qaacct7',), got ('runner', 'Runner', 'qaacct7', 'QAACCT7', 'ab', 'root')` (against the unfiltered stub)
- B1: `[(2, 'user name')] != [] : B1: expected a user-name hit, got none`
- B2: `[(1, 'user name')] != [] : B2: expected a user-name hit, got none`
- B3: `[(2, 'user name'), (4, 'user name'), ...] != [] : B3: expected a user-name hit, got none` (50 expected hits)
- B5 (subtests USERNAME, USER, LOGNAME): expected stdout exactly
  `history.jsonl:1:drive path`, `history.jsonl:2:user name`, `Found 2 potential leak(s).`;
  actual stdout holds the planted drive path as matched text on line 1 and has no line 2
  user-name hit, so the equality assertion fails. The planted value is not quoted here.

Passing, controls (376-1):

- W-A2, W-A5, W-A6, W-A7, W-A8: 0 hits, pass.
- B4: the same row with `<user>` gives [], pass. B6: the planted name glued to a letter or
  digit on either side gives [], pass.
- The two pre-existing BenchHistoryCleanTests rows still pass.

Failing, new state (376-2):

- T1: `1 != 0 : T1: expected exit 1, got 0`
- T2: `1 != 0 : T2: expected exit 1, got 0`
- T4: `'60 hits in 1 files' not found in ['A1 .mob/: 0 hits in 0 files', ...]`; the
  no-planted-text half of T4 passes before this assertion.
- G3: `1 != 0 : G3: expected exit 1, got 0`

Passing, controls (376-2):

- T3: only `<user>` forms, `--tree` exits 0. G4: the same staged file with `<user>` forms
  exits 0.

Failing, new state (376-3), all on the rewrite assertion (the second word is left):

- S1: expected `...<user>\projects`, got `...<user> seven\projects` (`S1: the rewrite rule`)
- S1eol: expected `...<user>` at end of line, got `...<user> seven`
- S2: expected `.../<user>/projects/x`, got `.../<user> seven/projects/x`
- S3: doubled-backslash form, got `...<user> seven\\x`
- S4 (three subtests: double quote F1, single quote F2, backtick F1): got `...<user> seven`
  before the closing quote or backtick in each
- S5 (two subtests): got `...<user>%20seven%5Cdoc` and `...<user>%20seven/x.md`
- S7: `AssertionError: unexpectedly None : S7: account_root_re did not match` (under the old
  segment the pattern cannot match at all, because the tail must follow the first word
  directly; see the slice note below)

Passing, control (376-3):

- S6 (three subtests): only the single-word account is replaced and the prose stays, pass.

Passing, characterisation:

- C1: `scrub_tracked(ROOT, exclude=())` returns [], pass.
- R1: each of the six record paths at the target (HEAD, no `after` line yet) equals its base
  blob, and the set of base files with an F10 hit is the six records plus the two hand-edited
  files, pass. The C# characterisation Fact running C1 and R1 passes.

## Baseline, not this red

- N1 in tests.test_home_paths_370 (`ScrubChangesOnlyAccountSegments.test_n1_...`) fails on
  this full clone, before and after this red: `N1: 2 files` (TEAM_WORKING_AGREEMENT.md and
  docs/decisions/process-and-staffing.md differ from the rule applied to their base blobs).
  This is the known baseline failure of bug 397 noted in the slice; it is not touched here.
  Existing 370 rows are therefore NOT all passing: 56 of 57 pass, N1 fails.

## Slice notes (things found wrong or loose)

- Section 4 says S7 fails on the rewrite assertion. It does not: `account_root_re` returns no
  match at all under the old segment (the tail cannot follow the spaced segment), so S7 fails
  with `unexpectedly None`. The row is still a valid red assertion and turns green only when
  the segment captures both words.
- R1 needs `home_paths.scrub_account_bytes`, which the red stubs deliberately do not provide.
  The row short-circuits on `after == before`, so it passes now as the slice requires; after
  green 1 the function exists and the second branch is live.
