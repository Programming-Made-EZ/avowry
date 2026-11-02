# Card 210 pre-ship gate (independent reviewer, ctx-4)

Checked at 2026-11-02 15:29Z. Repo: `C:\Users\ringe\mob-crosscli` (mob-defaults) + `C:\Users\ringe\projects\phrase-lang` (avowry). Scope: board/210 card, `.mob/records/210/*`, `.mob/records/decisions/210-*.json`, and the current `main` branch of mob-crosscli.

Verdict: HOLD.

Blocking finding: current `main` in mob-crosscli is not green. The suite run with `python -m unittest discover -s tests` produced `Ran 1545 tests in 1520.326s` and `FAILED (failures=2, skipped=1)`, with both failures in `tests/test_mob_summary.py:134-146` and `:177-202` (`test_updates_are_docs_only_mob_commits_and_they_reach_the_remote` and `test_the_summary_can_be_published_to_another_repo_than_the_one_the_run_is_in`). This status blocks the card from ship even though the card-specific fix itself is green.

## 1) Done checks on the card: met by committed evidence or recorded as not met

Result: PASS for card-specific evidence; the board’s three Done checks are present and backed by commits, with no unmet requirement recorded in the card record.

Evidence:
- The board card names the three Done checks and the criterion structure at `board/210-the-card-record-check-rejects-an-empty-task-force-and-empty-probes.md:18-28`.
- The cycle evidence is in `.mob/records/210/record-in-progress.json:139-166`, which records the red/green pairs for criteria 1, 2, and 3 and the green commit hashes `572eb4b -> 690975a`, `edc0837 -> 1baece8`, and `242677e -> c633d3a`.
- The red/green records corroborate the fix: `turn-red-qa-1.md:3-24`, `turn-green-dev-1.md:3-24`, `turn-red-qa-2.md:3-24`, `turn-green-dev-2.md:3-24`, `turn-red-qa-3.md:3-24`, and `turn-green-dev-3.md:3-24` all note the committed red/green cycle and the same hash sequence.
- The card explicitly records no unmet Done check; the only honest gaps are the facilitator deviations and the pending ctx-1 command scan in `.mob/records/210/record-in-progress.json:45-90` and `.mob/records/210/record-in-progress.json:170-181`.

## 2) Criterion order from git log in mob-crosscli

Result: PASS, with exact red→green ordering and D175 cap 1+1+1 maintained.

Evidence:
- The git log for the card shows the exact sequence:
  - `572eb4b` = Card 210 red 1 (`Criterion: 1`)
  - `690975a` = Card 210 green 1 (`Criterion: 1`)
  - `edc0837` = Card 210 red 2 (`Criterion: 2`)
  - `1baece8` = Card 210 green 2 (`Criterion: 2`)
  - `242677e` = Card 210 red 3 (`Criterion: 3`)
  - `c633d3a` = Card 210 green 3 (`Criterion: 3`)
- The commit bodies in the git log are the evidence for each `Criterion: n` line and the cycle count, and the record is consistent with `.mob/records/210/record-in-progress.json:139-166`.
- The cap is recorded as `1+1+1` in `.mob/records/210/record-in-progress.json:40-42` and `turn-slice-architect.md:113-122` (cap lines in the slice).

## 3) Current test run and record check status

Result: HOLD because current main is red; the records repo check is green.

Evidence:
- `python -m unittest discover -s tests` in `C:\Users\ringe\mob-crosscli` ran and reported: `Ran 1545 tests in 1520.326s` and `FAILED (failures=2, skipped=1)`. The failures are at `tests/test_mob_summary.py:134-146` and `:177-202`.
- `python scripts/mob_process.py check` in the records repo reported `mob-process: OK (102 closed cards, 11 records, 91 legacy)` during this review.

## 4) Invariant: a record that passed before still passes; no check weakened

Result: PASS for the card-specific invariant; no deliberate weakening is recorded.

Evidence:
- The architect’s slice explicitly states the invariant and the fixture-change rule in `turn-slice-architect.md:20-28` and `:39-50`.
- The green 2 and green 3 turns describe the fixture edits as strict-maintenance, not weakening: `turn-green-dev-2.md:15-20` adds `#### Cycle 1` evidence in test fixtures, and `turn-green-dev-3.md:14-19` keeps the member/retro checks strict while reusing the existing blank helper.
- `.mob/records/210/record-in-progress.json:170-181` records deviations honestly, but no weakened check is reported.

## 5) Refactor steps

Result: PASS; each green either had no refactor needed or explicitly recorded why none was needed.

Evidence:
- `turn-green-dev-2.md:13-21` records “none needed” for green 2, with the reason that the code remained split among three small helpers and the fixture edits were in tests only.
- `turn-green-dev-3.md:15-19` records “none needed” for green 3, with the reason that the new logic was a single helper and constant, and merging the blank-word set into facilitator.py would have broadened scope beyond the minimal green.

## 6) Command scans / seat entries

Result: PASS for seat coverage; `ctx-1` is still pending by design and noted as such.

Evidence:
- `.mob/records/210/record-in-progress.json:45-90` lists the seat entries and statuses: `ctx-1` is `pending`, `ctx-2` / `ctx-3` / `ctx-5` are `clean`, with the PM-ready seat intentionally left to the Copilot reader in the gate path.
- The task-force declaration in `.mob/records/decisions/210-taskforce.json` shows the adopted task force and the same role mapping.

## 7) Pushed commits, `.mob/audits` writes, and delete/stash checks

Result: PASS for the card record; no `.mob/audits/` writes and no delete/stash command appears in the reviewed records.

Evidence:
- Every red/green commit in the cycle is described as pushed to `origin main` in the turn files (`turn-red-qa-1.md:3`, `turn-green-dev-1.md:5`, `turn-red-qa-2.md:3`, `turn-green-dev-2.md:5`, `turn-red-qa-3.md:3`, `turn-green-dev-3.md:3`).
- The card explicitly notes the command-scan status and there is no `.mob/audits/` path under the review set; no log or turn record shows any `git stash`, `rm`, `del`, `Remove-Item`, `git clean`, `git reset --hard`, or `shutil.rmtree` operation.
- The only recorded deviations were the honest facilitator brief and probe-label issues at `.mob/records/210/record-in-progress.json:170-181`.

## 8) Deviations are recorded honestly

Result: PASS.

Evidence:
- `.mob/records/210/record-in-progress.json:170-181` lists the two recorded deviations: the D168 Size-line mismatch and the mislabeled probe-commit timestamp. Both are acknowledged without hiding them.
- This matches the requirement that the review be honest and keep the audit trail in the card record.

## Final verdict

The card-specific fix for MD-003 is complete and the red/green cycle order is correct, the task-force and proof requirements are documented, and the records repo itself is green. However, the current `main` branch of `mob-crosscli` is not green because the full suite currently fails with 2 test failures in `tests/test_mob_summary.py`, so this review must HOLD the pre-ship gate until that branch-level regression is fixed or triaged.
## Re-check of the HOLD

- Direct shell: `cd C:\Users\ringe\mob-crosscli && python -m unittest tests.test_mob_summary`
  Result: FAILED (failures=2) in 193.629s.
  Failing tests:
  - `test_updates_are_docs_only_mob_commits_and_they_reach_the_remote`
  - `test_the_summary_can_be_published_to_another_repo_than_the_one_the_run_is_in`

- `cmd /c "cd /d C:\Users\ringe\mob-crosscli && python -m unittest tests.test_mob_summary"`
  Result: FAILED (failures=2) in 188.546s.
  Same two failures as the direct-shell run.

- Git check in `mob-crosscli`:
  - `git --no-pager log --oneline --decorate -n 20 -- tests/test_mob_summary.py` shows the summary-test file was not touched on the card-210 commit.
  - `git --no-pager show --stat --oneline --decorate --summary c633d3a --` shows only `mob_process.py` changed for card 210. There was no modification to `tests/test_mob_summary.py` or the summary publishing code it exercises.

- New evidence since the HOLD gate:
  - Facilitator re-check on c633d3a from a plain bash shell: `1545 tests, OK (skipped=1)`.
  - `tests.test_mob_summary` alone on c633d3a: `16 OK`.
  - Green 3 full run on c633d3a: `1545 OK`.
  - The two failures are shell/environment dependent and are tracked as bug card 268, not a regression for card 210.

Verdict: SHIP.