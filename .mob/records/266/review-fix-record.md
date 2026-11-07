# Card 266 review fix (D157 Criterion 1), QA engineer seat

The pre-ship gate reviewer found three defects in the QA tests and patches. This commit fixes them. Only `tests/test_live_docs_266.py`, two files in `.mob/records/266/mutations/` and this record are touched; no doc is edited.

## 1. T8 and T9 read the wrong "committed" bytes
At HEAD fd9246c, `python -m unittest tests.test_live_docs_266` was 17 tests, 1 FAIL (T8): `TEAM_WORKING_AGREEMENT.md: old 90716 bytes, expected 90716, committed 91169, first differing line 431`. At 29efd59 it was 17 OK.
Cause (checked): the helper `newest_green_commit()` matched the subject prefix `Card 266 green`, which also matches 4fcb9ff `Card 266 green record ...` (the facilitator's later ledger edit). So `committed_bytes()` returned that commit's blob, not the card's own green. (The helper already read a git blob; the defect was which commit.) Any later commit with a subject starting `Card 266 green` would break T8 and T9 the same way.
Fix: the helper matches `Card 266 green 1` (the newest such commit); working tree only if none exists. T1 to T3, T5 to T7 and T10 to T14 are unchanged (they stay on the working tree, as guards on the live docs).

## 2. Red first for the fix (watched)
New test `GuardScan266.test_byte_rules_read_the_green_commit_not_the_working_tree`. It builds a `git clone --shared --no-checkout` of the repository in a TemporaryDirectory, commits a later edit to `TEAM_WORKING_AGREEMENT.md` as `Card 266 green record (scratch)`, appends an uncommitted line to the working tree of README.md and VISION.md, points the module's `REPO` at the clone, and asserts that `committed_bytes` returns the `Card 266 green 1` blobs of the seven sed files plus README and VISION, and that those blobs satisfy the T8 rules (`apply_rules`) and the T9 rules (`expected_readme`, `expected_vision`).
Against the CURRENT helper (before the fix):
```
FAIL: test_byte_rules_read_the_green_commit_not_the_working_tree
AssertionError: ['TEAM_WORKING_AGREEMENT.md'] ... : the helper did not return the green 1 blobs of: ['TEAM_WORKING_AGREEMENT.md']
Ran 1 test in 6.8s   FAILED (failures=1)
```
(An earlier draft of the test used a full `git clone`: it failed the same way but took 33 s on this repo's 20k loose objects, so it was switched to `--shared --no-checkout`.)
After the fix (prefix `Card 266 green 1`): `Ran 1 test ... OK` (6.6 s).

## 3. The M3 and M4 patches were mislabelled
Both are rebuilt with `git diff` in a scratch clone at the stated base and verified in a fresh scratch clone with `git apply --index` (forward), running the tests at that base.
- M3 `M3-delete-the-readme-note.patch`: deletes the README note paragraph; base 29efd59 (post-green-1). `GuardScan266` is OK before and FAILED (failures=7) after: T1, T2, T5, T10, T11, T12, T14.
- M4 `M4-restore-one-phrase-tasks.patch`: restores one `phrase-tasks` (the `dotnet test` line) in `demo/tasks/README.md`; base 9675b3c (post-green-2). `tests.test_demo_readme` is OK before and FAILED (failures=1) after: `test_every_tmp_folder_in_the_readme_is_avowry_tasks`.
Each patch is a plain forward edit that changes the tree from green to a mutated state (no reverse apply needed).

## Count at HEAD (with this fix)
`python -m unittest tests.test_live_docs_266.GuardScan266`: Ran 14, OK. `python -m unittest tests.test_live_docs_266`: Ran 18 (17 + the new one), OK.

## Facilitator notes
- Later edits to a scoped file (this retro's TEAM_WORKING_AGREEMENT.md edit, card 326's README.md edit) no longer break T8 or T9, provided the commit that carries the card's green keeps the subject `Card 266 green 1`.
- T8 and T9 now only see the green commit; they say nothing about the working tree. T1 to T3, T5 to T7 and T10 to T14 guard the live docs.
- `UntouchedBytes266` uses its own `comparison_commit()` (green 1 or green 2 prefixes) and was not touched.
