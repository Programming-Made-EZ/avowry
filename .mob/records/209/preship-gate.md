# Card 209 pre-ship gate (D154, D157), delivery lead ctx-7

Run 2026-10-30 about 16:20 to 16:30 UTC, Claude seat (probes at 15:33 UTC, .mob/records/probes/2026-10-30.jsonl, show Codex, Copilot, Grok and agy without usable quota).
Origins at the time: phrase-lang origin/main = 2ca4071 (= local HEAD); mob-defaults origin/main = e239b12 (= local HEAD, git fetch just before).
Method: fresh `git worktree add --detach` of each origin/main (scratchpad wt-209-gate-pl, wt-209-gate-md), `git cat-file -e` plus `git merge-base --is-ancestor <hash> origin/main` (cat-file alone also finds local-only objects, so reachability from origin/main is the test), then `git worktree remove`.

## 1. Hashes and record paths resolve on origin: PASS

Sources read: .mob/records/209/record-in-progress.json, review-ctx4.md, turn-slice-architect.md, .mob/records/decisions/209-slice.json and 209-taskforce.json.

- mob-defaults (all ancestors of origin/main e239b12): 6836d76 5a2c930 b527e1d 8e387e3 7d371fa 44112b4 37b1273 145947b 147a90a 18f3962 794a4a7 1bb0a83 73026aa 7fdb168 e239b12, and 46864d0 (the card's base).
- phrase-lang (all ancestors of origin/main 2ca4071): dd49100 b69366f 9052942 386cf3e fac4808 a4475fc fcfdb7b d70fb97 a3f6e49 a3a9719 7c80c75 406354e 18cb108 38762d2 255e753 0f7225f, plus aadb7b6 (record of the clean run), 61a4741 (review re-check), 1a3dd48 (in-flight for the rerun) and 2ca4071.
- Record paths, present in the origin/main tree of phrase-lang: .mob/records/209/{turn-slice-architect.md,review-ctx4.md,record-in-progress.json}, .mob/records/probes/2026-10-30.jsonl, .mob/records/decisions/{209-slice,209-taskforce,214-rerank-7}.json, docs/decisions/process-and-staffing.md, scripts/mob_process.py. Code paths present in the mob-defaults origin/main tree: facilitator.py, mob_process.py, decider.py, tests/{test_card_mode,test_mob_process,test_template_removal}.py, tests/fakes/fake_cli.py, templates/{TASK_FORCE,TEAM_WORKING_AGREEMENT.default}.md.
- Two cited ids are not commits on origin and are not done or PASS claims: b21d94e (the developer's unpushed layer 3 commit, amended away 29 s later; cited in observation (d) as reflog evidence; correctly absent from origin) and d202d953d123a60c41b918a068364bee (a file content hash of facilitator.py quoted in review-ctx4.md, not a commit).

## 2. Full suite evidence: PASS (accepted, not re-run)

- origin/main of mob-defaults is e239b12 and git diff e239b12 origin/main is empty, so the run's code is the code that ships.
- The record (aadb7b6, on origin) cites mob-defaults at e239b12: Ran 338 tests in 1030.8 s, OK (skipped=1), facilitator verification run 15:59 to 16:16 UTC, with an in-flight line on origin (1a3dd48, started 15:59:28, cap 16:24). The count is consistent with the earlier cycles (332 after criterion 2, 336 after criterion 3, plus the review's added tests). The invalid first run (it executed avowry's suite) is disclosed and not relied on.
- Caveats, accepted: the run was local in the working copy at e239b12 (head checked), not a clean checkout, and its log is in a facilitator scratchpad, not in git; the 338 figure rests on the record line. The reviewer's re-check (61a4741) separately ran tests.test_template_removal (6 OK) and did not re-run the full suite. A re-run (about 17 min plus an in-flight line) was possible inside the 16:50 cap but not needed: nothing has changed since e239b12.

## 3. CI on mob-defaults main: PASS (no CI workflow)

`gh run list -R Programming-Made-EZ/mob-defaults --branch main --limit 5` returned no runs, and the origin/main tree (e239b12) holds no .github directory or workflow file. Result: no CI workflow, so there is no CI result to wait for.

## 4. D157 line: git log order of Criterion lines in 6836d76..e239b12: PASS (no order breach; two gaps explained)

`git log --reverse` over 46864d0..e239b12, 15 commits:

| order | commit | Criterion line |
|---|---|---|
| 1 | 6836d76 red | Criterion: 1 |
| 2 | 5a2c930 unit layer 3 | Criterion: 1 |
| 3 | b527e1d unit layer 4 | Criterion: 1 |
| 4 | 8e387e3 unit layer 5 | Criterion: 1 |
| 5 | 7d371fa green | Criterion: 1 |
| 6 | 44112b4 red | Criterion: 2 |
| 7 | 37b1273 CRLF restore | none: body says "Not a Criterion line", no test or behaviour changed |
| 8 | 145947b unit | Criterion: 2 |
| 9 | 147a90a unit (correction of 145947b) | Criterion: 2 |
| 10 | 18f3962 green | Criterion: 2 |
| 11 | 794a4a7 red | Criterion: 3 |
| 12 | 1bb0a83 green | Criterion: 3 |
| 13 | 73026aa review fix F1 (test only) | none |
| 14 | 7fdb168 review fix F2 red | none |
| 15 | e239b12 review fix F2 green | none |

Green 1 precedes red 2, green 2 precedes red 3, and no red for criterion n+1 appears before green n. Pushes (per the record): criterion 1 together at about 14:27 UTC, criterion 2 at about 14:56, criterion 3 at about 15:21; the review fixes (73026aa to e239b12) were held until the 16:16 verification run and carry in-flight lines (15:56:34, 15:59:28), consistent with D157 C3.
Gaps, not order breaches: (a) 37b1273 has no Criterion line and says why; (b) 7fdb168 is a red-shaped commit without a Criterion line, but it is a review finding fix after all three criteria were green, not a red for a slice criterion, and F1/F2 are named in the review (a4475fc), so C1 (a line "naming one slice criterion") has nothing to name. The auditor may weigh (b). Process notes already in the record: the criterion 1 amend of an unpushed commit (b21d94e, stand-down 1 obs. d) and the D136 line-ending churn in 44112b4 (record deviations entry).

## 5. The card's grep: PASS

In the origin/main worktree of mob-defaults (e239b12), `grep -rn "removed: cycle" tests templates *.py` finds only tests/test_card_mode.py, lines 1349, 1463, 1509, 1511, 1599; a whole-tree search finds no other file. All five are refusal cases: 1349 (a mid-card joiner removed at cycle 2 is a problem, at retro is not), 1463 (added and removed in one edit is refused), 1509 and 1511 (several removals: only the cycle ones are named), 1599 (joined mid-card is refused its own way). templates/ has no "removed: cycle": TEAM_WORKING_AGREEMENT.default.md and TASK_FORCE.md (fixed by F2, e239b12) are clean; the only "cycle" hits in templates are unrelated prose (TDD cycle, cycle boundary) and templates/TASK_FORCE.md line 50 `joined: cycle 2`, which is a join, not a removal.

## 6. Domain-expert question at ship (T2): answer no

Question: should an agent with different domain knowledge or context join before ship? Answer: no. Reason: what ships is a refusal rule in facilitator.py and mob_process.py plus two template paragraphs and their scans, all covered by the backend, QA and independent-reviewer seats; the reviewer's last answer was "no, conditional on F1 and F2" and both are fixed and approved (61a4741); this gate found no language, UX, security or workflow question those seats could not answer.

## Open items (not part of this gate's six lines)

- PENDING: the D154 rule 2 dry run of the closure record check (scripts/mob_process.py) and the origin-resolution list for the closing record. There is no closure record yet (the record is still record-in-progress.json); the facilitator runs it at closure. This file is the origin-resolution list for what exists today. It was not run here to stay within the gh call budget (the checker uses about 92 REST calls).
- PENDING: PM sign-off (ctx-5) and the push of this file; this seat did not push.
