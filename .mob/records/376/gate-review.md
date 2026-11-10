# Card 376 independent gate review (fresh context, main at d6a12bc74849344be48c1a3b0437f341b3bc959d)

Verdict: PASS

No account name, personal name, email or real home path appears below; counts were taken by paths-only or count-only commands.

## Step 0: order and seats

- Red commits: e915aa99bf5553601943abbfddc183135c2624d3 (tests and stubs), 88ac67e0f246ea6fe19d4dd195cc3215c69b4b19 (red record).
- Green 1: 586c2cdd9f12e37b2d6892770376f8ebadae33b6. Green 2: 69d764ef002bcc32914346b32f8f741fb2885d9c. Red parent: 381b37fa672eaf5f81a2fbbe8e026d215e97e800.
- `git merge-base --is-ancestor` is true for all four pairs (each red before each green), and green 2 is on main. The red record, green 1 and green 2 all precede the later commits 9bd6224b272fc87c5ab3df4b88bd96e58e3f038b (after = green 2 in scrub-commits.txt) and 935305898914d5570e806ff5a4a4cfce2dfe7017 (bug 397 fix).
- Trailers: no commit carries a Co-Authored-By or Signed-off-by line. Green 2 alone carries `Slice-exception: rewrite s/ACCOUNT/<user>/`, as the slice allows. The git author is the same machine identity on all commits (the facilitator commits), so the seat split (Muse red, Cursor green) cannot be seen from git; it rests on the commit and record messages. Noted, not a finding against the change.
- Red and green are separate commits by different working seats per the records; I could not verify the seat from git metadata.

## Step 1: red replay

- Throwaway worktrees at 381b37f (the red parent has no 376 test file at all, so a replay there is meaningless) and at the red e915aa9 (new tests plus stubs, old production behaviour). `python -m unittest tests.test_account_name_376 tests.test_bench_history_clean` at e915aa9: 33 tests, FAILED (failures=24).
- Failing rows exactly as red-record.md lists: U1; S1, S1eol, S2, S3, S4 x3, S5 x2, S7; G3, T1, T2, T4; W-A1, W-A3, W-A4; B1, B2, B3, B5 x3 (USERNAME, USER, LOGNAME). Passing as controls: W-A2, W-A5 to W-A8, B4, B6, T3, G4, S6, C1, R1 and the two old bench rows. All failures are assertion failures.
- At main: 33 tests, OK. W-A1 to W-A8 and R1 report `ok`, not skipped (a local account exists on this machine, and the full clone has the base commit). Worktrees were removed with `git worktree remove`; none of mine remains in `git worktree list`.
- 370 suite at main: 57 tests OK (the baseline N1 failure, bug 397, is fixed by 935305898914d5570e806ff5a4a4cfce2dfe7017).

## Step 2: scope of the greens

- Green 1 (`git diff --name-only 586c2cd^ 586c2cd`): scripts/check-bench-history-clean.py, scripts/check-no-secrets.py, scripts/home_paths.py, tests/test_bench_history_clean.py and the Pending removal in the 376-2 and 376-3 wrappers (two lines). That is exactly slice section 5.
- Green 2 (`git diff --name-only 586c2cd 69d764e`): the six records (226 record-in-progress.json, 229 nohome229_check.py, 276 record-in-progress.json, 276 signoff-ctx6.md, 313 green1-codex-events.jsonl, 313 slice-architect.md), .mob/records/376/green2_rewrite.py, and one Pending line removed from NoAccountNameInTrackedFiles.cs. Nothing else.
- Rule reproduction: applying `home_paths.scrub_account_bytes(base_blob, local_accounts())` to the six base blobs at `base` cf5be43f28dfee04ca59e7c85772f17ef3b8ee58 gives bytes identical to the committed blobs at 69d764e: six MATCH, none DIFF. All six stay LF in index and working tree.
- Slice section 5 says base is the red's parent; the file names cf5be43 (the commit before it), which differs only by scrub-commits.txt. Harmless, the six files are identical in both.

## Step 3: acceptance coverage, with my own mutations

Each mutation was made in a scratch worktree at main and discarded.

| Scenario or id | Mutation | Result |
|---|---|---|
| 376-2, T1, T2 | `scan_tree` returns 0 always | T1 and T2 fail (2 failures) |
| 376-2, G3 | hook `scan_staged` uses no accounts | G3 fails |
| 376-2, T4 | no cap on the listed hits | T4 fails |
| 376-3, S1 to S5, S7 | space rule takes one word only | 10 failures: S1, S1eol, S2, S3, S4 x3, S5 x2, S7 |
| 376-1, B1 to B3, B5 | bench checker ignores accounts | 7 failures (B1, B2, B3, B5 x3 and the old name row) |
| 376-1, W-A4 | account name appended to a tracked script | W-A4 fails |
| 376-1, W-A1 | the pre-change blob of a record put back in the working tree | W-A1 fails |
| U1 | `local_accounts` filtering removed | U1 fails |
| R1 | `scrub_account_bytes` writes a different placeholder | R1 fails |

- Ids not mutated separately: W-A2, A3, A5 to A8 (same scanner as A1 and A4, per-area), T3, G4, B4, B6, S6 (controls by design; they pass before and after the red), C1 (characterisation, passes in the red as the slice requires).
- Noted: R1 reads committed blobs at the recorded `after` commit, so a working-tree edit to a record does not fail R1 (it does fail W-A1 if it re-adds the name). That is the D206 design in the slice.
- The three criterion wrappers (376-1, 376-2, 376-3) no longer carry Pending; the characterisation wrapper is Slice only. The facilitator's clean-clone run reports the wrappers passing; I did not run the C# suite.

## Step 4: name and path scan

- `git grep -I -i -w -l -e "$USERNAME"`: 0 files (also 0 for `$USER`).
- `python scripts/check-no-secrets.py --tree`: exit 0; eight area lines all `0 hits in 0 files`; `F10: checked 1 local account name(s)`; `0 hits in 0 files`.
- `python scripts/home_paths.py --root .`: 0 files. Committed 376 records (including pm-draft-raw.md and the probe file): 0 files hold the name.
- Tracked history still holds the name in older commits; green 2 rewrites the working tree only, and the 370 stance is that history stays. See finding 2.

## Step 5: value rule

- Persona: Dana, the sponsor, opening the public repository at HEAD, now finds no account name in any tracked file, with spaced names also handled by the path rule.
- `--tree` gates by exit code (1 on any F1 to F10 hit, 0 otherwise), with output that names only area counts, `path:line:form`, and whether the F10 check ran, so an exit 0 is not read as proof when no local name exists. Delivered consumer value; this is not mob-internal busywork.

## Step 6: CI limit

- In CI the account is `runner`, which is on the generic list, so `local_accounts()` is empty. W-A1 to W-A8 skip ("no local account name outside the allow-list", tests/test_account_name_376.py line 145) and R1 skips (lines 405, 410, 418: no local account; base commit absent in a depth-1 clone; base blobs show no hit). The messages say why. The planted rows (U1, S, T, G, B, C1) still run in CI and carry the regression guard. So CI cannot see a real person's name leaking back; the hook and a local `--tree` run on a developer machine do. That is a real limit and it is stated in the skip text.

## Findings, ranked

1. No blocking finding.
2. Git history still holds the account name in commits before 69d764e. The card scope is the tree, not history, but "public repository with no account name" is only true of the checked-out tree until a history rewrite or a fresh public repository is decided. Not delivered by this card; it needs a decision and a story.
3. CI cannot catch a real name (see step 6); the guarantee is local and via the pre-commit hook, which only checks added lines of staged files.
4. Records 229, 226 and 313 were edited (the 229 script check at line 48 changes meaning); the slice anticipated this, they are never re-run, and 226 and 313 JSON stay valid.
5. Seat split (Muse, Cursor, facilitator) is not verifiable from git author data; only from record text.
6. R1 is blind to working-tree record edits by design.

## Not delivered

- Name removal from git history.
- A CI check that sees a real person's name.
- Spaced names of four or more words, double spaces or tabs, and a spaced name followed by prose at a non-closer (stated in the slice as not fixed).
