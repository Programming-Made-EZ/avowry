# Card 227 pre-ship gate (D154, D157), delivery lead ship, ctx-7

Seat: Claude (the 22:23 UTC probes in .mob/records/probes/2026-10-30.jsonl show Codex, Copilot, Grok and agy without usable quota). In-flight line committed first (90e45b2, on origin; 23:02:17 UTC). Checked 2026-10-31 23:03 to 23:15 UTC. No test was run by this seat; no gh call and no network call other than `git fetch` was made.
Bases on origin after `git fetch` in both repositories: avowry (phrase-lang) origin/main 90e45b2 (one other seat's commit, 865a408, is local and unpushed at the time of writing; not mine); mob-defaults origin/main f8a45da, `git status -sb` prints `main...origin/main` with nothing ahead. Range under gate: mob-defaults 56b39bc..f8a45da, 23 commits (`git rev-list --count 56b39bc..f8a45da` prints 23; `git log --merges --oneline 56b39bc..f8a45da | wc -l` prints 0, so linear). `git diff --stat 56b39bc f8a45da`: 34 files, 4272 insertions, 15 deletions.

| # | Item | Verdict |
|---|------|---------|
| 1 | Hashes and record paths resolve on origin | PASS (71 of 71 hashes; every cited record path present; one non-hash token excluded) |
| 2 | Full-suite evidence | PASS, nothing unrun after it (1253 OK at f8a45da; no mob-defaults commit after it) |
| 3 | CI | mob-defaults: no workflow (PASS by absence). avowry: workflows exist, runs NOT MEASURED (cannot be read without gh) |
| 4 | D157 order of "Criterion: n" | PASS, with three explained departures |
| 5 | D136 per commit | PASS (23 of 23 within 15 files and 400 lines; largest 399 lines, 10 files) |
| 6 | No trailers; line endings | PASS (0 trailer lines in 211 commits; 0 CR in 211 bodies; no CR added to any file) |
| 7 | Push on every commit | PASS (unpushed: ok; ladder: ok; outputs pasted below) |
| 8 | command_scans coverage ctx-1 to ctx-8 | PASS for ctx-1 to ctx-6; ctx-7 and ctx-8 CARRIED |
| 9 | Domain-expert question at ship (T2) | PASS: no further member; the answer lands with this file's commit |
| 10 | Open direction question to the product owner ("block" = prevention before execution?) | CARRIED to the product owner (not failed) |

## 1. Hashes and paths on origin

Method: `git fetch` in both repositories. Hashes were pulled by regex (7 to 40 hex characters containing a digit and a letter) from board/227-*.md, .mob/records/227/record-in-progress.json, review-ctx6.md, review-security-ctx3.md and .mob/records/trust-ladder.jsonl, plus the task's own list (73cdf20, 810e818, c803a43, 5deedcc, 56b39bc, f8a45da). Each was checked with `git cat-file -e <hash>^{commit}` and `git merge-base --is-ancestor <hash> origin/main` in both repositories, then `git branch -r --contains <hash>` in the repository that holds it.

- 72 distinct tokens. 71 are commits and ancestors of origin/main, and `git branch -r --contains` lists origin/main for all 71: 48 in avowry, 23 in mob-defaults, 0 in both. The 72nd, `73298da3`, is a session identifier inside a scratchpad path in review-security-ctx3.md line 7 (a Windows temp path), not a commit; it resolves in neither repository and is excluded. 0 of the 71 are off origin.
- The 23 mob-defaults hashes are the 23 commits of the range. Review fbe02c3 and c165f52 (the two first reviews), the re-check commits 4a1cc50 and e5105b1 (avowry), the slice 4037cfe, 2e2e0f3 and the intent 254e1d9 are all among the avowry 48.
- Record paths: a regex over the same five files gave 85 path-like strings; 27 resolve in `git ls-tree -r origin/main --name-only` (MSYS_NO_PATHCONV=1) of one repository (13 avowry, 16 mob-defaults, some in both by file name). The other 58 are not repository paths and are listed by kind, not failures: 38 are `subagents/agent-*.jsonl` seat transcripts (they live under the Claude session folder, not in either repository; the record names them as the scan source), 3 are globs (`board/227-*.md`, `board/228-*`, `board/228-*.md`; the real files `ls board | grep 22[78]` prints for 227 and 228), 17 are fixture or example names inside quoted test strings (`x.py`, `f.py`, `docs/old`, `.mob/scratch` as a relative scratch root, and the like). The files that matter are present: .mob/records/227/ checklist-227-slice.md, checklist-227-taskforce.md, record-in-progress.json, review-ctx6.md, review-security-ctx3.md, turn-slice-architect.md, .mob/records/trust-ladder.jsonl, .mob/records/probes/2026-10-30.jsonl and the board cards 227 and 228 (all on origin/main of avowry); delete_guard.py, command_source.py, mob_process.py, decider.py, facilitator.py, core/backends.json and the 16 test files of the range (on origin/main of mob-defaults).
- Caveat: .mob/records/227/turn-green-dev.md exists in the avowry worktree and is the developer seat's, committed by another seat (865a408, local); it is not cited by the sources and not mine to push.
- Local-only in mob-defaults: `git status -sb` shows `?? .mob/` (untracked scratch), nothing tracked changed.

PASS.

## 2. Full-suite evidence

- The last green's single full run: `python -m unittest discover -s tests` from C:/Users/ringe/mob-crosscli, 22:10:21 to about 22:38 UTC: Ran 1253 tests in 1684.397 s, OK (skipped=1) (record cycles, criterion 3 increment 3d, and `git log -1 --format=%B f8a45da` line 15). The earlier run of the same code without the test_card_mode isolation: Ran 1253, failures=1 (a timing test) - the record says so and the body of f8a45da says so; it is the red run, not the one counted. The commit f8a45da is stamped 15:38:51 -07:00 = 22:38:51 UTC, which fits a run that ended about 22:38 on the tree being committed.
- Anything unrun after it: `git log --oneline origin/main` in mob-defaults shows f8a45da as the head, and `git rev-list --count f8a45da..origin/main` is 0. No mob-defaults commit exists after the full run. The avowry commits after the run (5deedcc 15:39:06 -07:00, one line count 16 in TEAM_WORKING_AGREEMENT.md, then record, in-flight and turn-record commits) change text and records only, no code; the rule text they carry is read by `push_rule_problems`, which the 3d body and the re-check ran on both repositories (`[]` on both, review-ctx6.md "What I ran").
- Re-checks ran subsets, not the full suite, and say so: review-ctx6.md re-check at f8a45da: command-scan group Ran 684 OK (skipped=1), decider and seat group Ran 205 OK, "Not run: the full 1253-test suite". These are the seats' counts; this seat ran nothing.
- Limit: the 1253 run is one run by the developer seat, not independently repeated.

PASS; no re-run needed.

## 3. CI

- mob-defaults (f8a45da): `git ls-tree -r origin/main --name-only | grep -ci -E '^\.github|workflow'` prints 0 of 98 files. No workflow, so no CI to be green or red. The code for this card lives only there.
- avowry (90e45b2): `git ls-tree -r origin/main --name-only | grep -E '^\.github'` lists .github/CODEOWNERS, .github/workflows/ci.yml and .github/workflows/mob-process.yml (3 files). ci.yml runs `python3 scripts/mob_process.py check`, a previous-checker step, and a dotnet restore, build and test of Phrase.sln. Whether the runs can be read without gh: no. A run's result is held by GitHub Actions and is not in git (`git log` has no run record; no script in scripts/ reads workflow runs: a grep for workflow_runs, actions/runs and check-runs over scripts/ lists nothing). Reading it needs `gh` or an authenticated API call, both outside this brief.
- Verdict for avowry CI: NOT MEASURED. This seat makes no claim that the avowry workflows are green on the pushed head. The facilitator reads them after the push or the product owner does.

mob-defaults: PASS by absence. avowry: NOT MEASURED.

## 4. D157 line: order of "Criterion: n" (mob-defaults, 56b39bc..f8a45da, 23 commits)

Method: for each of the 23 commits, `git log -1 --format=%B <hash>` and a regex `^Criterion: *[0-9]+`. Count: 23 of 23 commits carry a "Criterion:" line (no commit lacks one). The Intent line (D165) comes first: 254e1d9 (avowry, 03:17:57 -07:00) before the first red 524171f (03:24:03 -07:00).

| Cycle | Commits in log order | Criterion line |
|---|---|---|
| 1 | 524171f red, 2fc4f98 green | 1, 1 |
| 1b | 3beff31 red, 8440697 green | 1, 1 |
| 2 | 5cc58d2 red, 79c572e green | 2, 2 |
| 3 | 2dbcb29 red, 00f9e93 green | 3, 3 |
| 1c (review fix) | 54c95da red, c84342a green | 1, 1 |
| 1d (review fix) | 47c9f67 red, d047643 green, d5ccc7f guard | 1, 1, 1 |
| 1e (review fix) | ce20e89 red, 191b8fb green | 1, 1 |
| 1f (review fix) | d4641c1 red, b8bbfaf green | 1, 1 |
| 2b (review fix) | a02f956 red, 1d65a30 green | 2, 2 |
| 3c (review fix) | 89e1b00 red, 799af53 green | 3, 3 |
| 3d (review fix) | 9cd8006 red, f8a45da green | 3, 3 |

- Main sequence 1, 1b, 2, 3: each green (2fc4f98, 8440697, 79c572e) precedes the next red (3beff31, 5cc58d2, 2dbcb29). No red for n+1 begins before the green for n in the first eight commits.
- Departure 1, 1b: criterion 1 got a second cycle before criterion 2. Explained: the facilitator's confirmation at 11:31:40Z (stand-down 1 adjustment) held that criterion 1 was not closed while its plan read "not done" (command_source.py, the run_backend wiring, the record check); it is still "Criterion: 1" and its red follows the criterion 1 green.
- Departure 2, the return to 1, 2 and 3 after 3 (the seven review-fix cycles 1c to 3d): the two first reviews (fbe02c3 and c165f52, CHANGES REQUESTED) were triaged by the architect into fix-in-227 per criterion, outside-in, one criterion per cycle (record reviews[3]). Each red follows the previous cycle's green and precedes its own green; none overlaps. Explained.
- Departure 3, d5ccc7f: a born-green guard for the two survivors of the 1d mutation run (the script-argument rule; $() inside an unquoted data heredoc), no red of its own; the body and the record say both survivors are red under their mutations. Recorded, no rewrite.
- Not a departure: several reds carry born-green guards shown red under mutations (524171f has 86, "shown red under six mutations", record). That is the red seat's practice on this card, not an order break.
- Greens pushed at once: `git rev-list --count origin/main..main` prints 0 in mob-defaults (all 23 on origin, item 7). Record deviations are listed in record-in-progress.json deviations (10 entries: 5a5659e title, one-increment overshoot, intent line before confirmation, late re-probe, "+20" vs +19, helper swept the ladder entry into 3c82909, Python-computed stamp, seat launched 6 minutes after its in-flight line, the red 1c rm -rf slip, and the unread findings result); none is an order break.

PASS.

## 5. D136 per commit (at most 15 countable files and 400 changed lines)

Method: `git log --reverse --format='@@%h' --numstat 56b39bc..f8a45da`, files and insertions plus deletions summed per commit. Files are all .py, .md, .json or .jsonl in mob-defaults; none is a board, decisions or MOB.md file.

- Files per commit, in log order: 10, 2, 4, 6, 1, 2, 1, 4, 1, 2, 1, 1, 1, 1, 2, 1, 5, 1, 1, 1, 2, 1, 4. Maximum 10 (524171f, a test-only red with fixtures).
- Lines per commit, in the same order: 233, 363, 397, 284, 387, 108, 399, 161, 277, 147, 345, 175, 81, 176, 82, 205, 82, 153, 12, 172, 14, 205, 69. Maximum 399 (2dbcb29), then 397 (3beff31), 387 (5cc58d2), 363 (2fc4f98). 23 of 23 within 15 files and 400 lines; three reds at 387 to 399 are close to the line and under it.
- avowry commits named in the task: 73cdf20 1 file 10 lines, 810e818 1 file 11 lines, c803a43 1 file 1 line, 5deedcc 1 file 16 lines (all TEAM_WORKING_AGREEMENT.md); the ladder lines: 3c82909 2 files 3 lines (.mob/records/trust-ladder.jsonl and MOB.md; the title is MOB.md regenerated, see the record's deviation) and 55bfa8d (the second ladder line). All within D136.
- The card exceeds the per-card guide by one increment and one concern (recorded at the slice, 2e2e0f3). The review-fix increments are separate cycles, each within D136.

PASS.

## 6. Trailers and line endings

- Trailers: over the 23 mob-defaults commits, `git log -1 --format=%B <hash>` with `grep -ciE '^(co-authored-by|signed-off-by|reviewed-by|generated with)'` prints 0 per commit, total 0. Over avowry `git rev-list cdcc04c^..HEAD` (cdcc04c is the card's first commit, the board archive): 188 commits, 0 trailer lines. Total 211 commits, 0 trailer lines.
- CR in commit bodies: 0 over all 211.
- Line endings, mob-defaults (`git show f8a45da:<file> | tr -cd '\r' | wc -c` per changed file, against 56b39bc): decider.py 341 CR in 341 lines (327 in 327 before) and facilitator.py 1723 in 1723 (1650 before) are CRLF files that stay fully CRLF, no mixed endings; the other 32 changed files have 0 CR. `git diff --stat 56b39bc f8a45da -- decider.py facilitator.py` and the same with `--ignore-cr-at-eol` both print 97 insertions, 10 deletions, so no line differs only by its ending. Nothing was converted. (review-ctx6.md notes the 3d body says 1727 lines where the file has 1723; a body miscount, not a break.)
- Line endings, avowry: the card 227 files .mob/records/227/ (checklist-227-slice.md, checklist-227-taskforce.md, record-in-progress.json, review-ctx6.md, review-security-ctx3.md, turn-slice-architect.md, turn-green-dev.md), .mob/records/trust-ladder.jsonl and board/227-*.md: `tr -cd '\r' | wc -c` prints 0 for each (10 files). One older CRLF file, .mob/records/probes/2026-10-30.jsonl (92 CRs), is the probe log from before this card; not rewritten.
- This file: written with LF; `git diff --cached | tr -cd '\r' | wc -c` is run before the commit (result in the commit step).

PASS.

## 7. Push on every commit (outputs pasted)

Run at 23:05 UTC:

```
$ python C:/Users/ringe/mob-crosscli/mob_process.py unpushed C:/Users/ringe/projects/phrase-lang C:/Users/ringe/mob-crosscli
ok
exit 0

$ python C:/Users/ringe/mob-crosscli/mob_process.py ladder C:/Users/ringe/projects/phrase-lang
ok
exit 0
```

- `git rev-list --count origin/main..main`: mob-defaults 0; avowry 1 (865a408, the developer seat's turn record, under 5 minutes old at the check; `unpushed` printed ok for that reason; not mine to push).
- The ladder file has 2 lines: card 226 criterion 1 (13:07:00Z, backend developer ctx-4) and card 227 criterion 1 (18:27:41Z, QA engineer ctx-4, the red 1c seat). Both: step "none" because Claude as the last-resort driver is already at Rung 1; coaching is this card's scan, the ladder check and the seat brief.
- Note, not a failure: the scan flagged the red 1c delete about 15:52 UTC and the facilitator recorded it unread for about 2 h 40 min (record deviation); the push detector and ladder cover push and the ladder, not that step, and the record names the fix (read every `findings` result before the record commit).

PASS.

## 8. command_scans coverage of every task-force context

Method: `python` over record-in-progress.json `command_scans` (32 entries, 1033 commands in the entries that give a count; 25 clean, 7 findings).

| Context | Role | Entries | Verdict |
|---|---|---|---|
| ctx-1 | product manager (ready) | 1 clean (pm-ready, 10 commands) | covered |
| ctx-2 | architect (slice) | 2 clean (architect-slice 60, architect-amend 15) | covered |
| ctx-3 | security reviewer | 3 (security-slice clean 17; security-review findings, false positive; recheck-security clean 59) | covered |
| ctx-4 | QA engineer (red) | 11 (10 clean; red-1c findings, REAL; red-1d findings, false positive, rescanned clean) | covered |
| ctx-5 | backend developer (green) | 11 (10 clean; green-1d findings, false positive) | covered |
| ctx-6 | independent reviewer | 2 findings (review-ctx6, recheck-ctx6; both false positives: test strings passed to scan_command in a Python heredoc, read by hand) | covered |
| ctx-7 | delivery lead (ship) | 2 entries (standdown-1 findings, a false positive; standdown-2 clean 42) exist under the label ctx-7; this ship seat's own transcript is not scanned yet | CARRIED: the facilitator scans this seat on return, per the stand-down 2 step |
| ctx-8 | product manager (signoff) | none; seat not yet seated | CARRIED |

- 7 findings entries: 6 false positives (rmtree named inside Python string literals or test data; the class is card 228 item 3) and 1 real (red 1c, `rm -rf "$M"` on a variable path; trust-ladder line 2 at 18:27:41Z). Every findings entry has a non-empty reason; 0 `not run`.
- The ctx-1 to ctx-6 coverage rests on the facilitator's scan results as recorded; this seat did not re-run `delete_guard.py scan-transcript`.
- This seat ran no delete command of any kind (the brief forbids it): its commands were git reads, Python reads and `mob_process.py` checks.

ctx-1 to ctx-6 PASS; ctx-7 and ctx-8 CARRIED.

## 9. Domain-expert question at ship (T2)

Should a member with different domain knowledge or context join at ship? No.
- Earlier answers hold (record domain_expert: refinement ctx-1 yes, a security reviewer joined; slice ctx-2 no further member). The security reviewer sat at the slice and at the review (ctx-3, APPROVE at e5105b1).
- The ship boundary adds no code and no trust boundary: hash resolution, test evidence, commit order and the push checks are covered by the seats already on the task force.
- Open points left are product choices, not domain knowledge: the direction question (item 10) and card 228's scan coverage.

PASS. The answer is new at ship, so its hash is the commit that adds this file.

## 10. Open direction question to the product owner

Carried, not failed. The record (open_direction_question) and the intent confirmation say: does "block" in the safety ruling require prevention before a command runs (an opt-in hook, now in card 228), or is detect-and-fail-the-record after the fact enough for card 227? Card 227 is sliced as detection (the scan of executed commands, the record check, the push detector); prevention before execution is not built by it. Both re-checks approved on that slicing. The product owner answers; until then card 227 ships with this limit named.

CARRIED (owner: the product owner).

## For the product manager (sign-off, ctx-8)

Reviews: independent reviewer (ctx-6) APPROVE (commit 4a1cc50, re-checked at mob-defaults f8a45da); security reviewer (ctx-3) APPROVE (commit e5105b1, strings re-run on f8a45da). Nothing here blocks the push or the sign-off. Not verified by this seat: avowry CI (item 3, not measured); a repeat of the 1253 run (item 2); the ctx-7 and ctx-8 command scans (item 8, carried); the product owner's answer on "block" (item 10, carried). The mob-defaults commits are all on origin (f8a45da).
