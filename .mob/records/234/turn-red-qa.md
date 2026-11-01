# Card 234, red seat turn record (QA, ctx-4)

This turn record is written at close from git and the record, not during the turns. Sources: `git log` and `git show --numstat` in avowry (phrase-lang) for the nine commits below, the commit bodies, `.mob/records/234/record-in-progress.json` (cycles, deviations, token_usage, observations_for_next_stand_down), and `.mob/records/234/usage/seats-2026-11-01-0626.json`. Nothing was re-run for this record: every count is copied from a commit body or the record and names its command, or says "not measured". Sizes come from `git show --numstat` run for this record.

## Commits

`git log` confirms the list. The intent line is c588838. The reds are c12f7f2, 570dc5c, 707434b (a red fix), eb68d95, eb47780, ae04522, daf3e6d and ef38d5e. All nine are authored Steve Bishop (the shared git identity), so authorship does not tell the seats apart; the seat comes from the record. The greens and record commits in the range are not listed.

| Commit | Criterion | Subject | Seat | Size (git show --numstat) |
|---|---|---|---|---|
| c588838 | intent | Card 234 intent line (QA ctx-4): the product owner's ruling D170 on the indexed process wiki, pending facilitator confirmation (D165) | Claude | not measured |
| c12f7f2 | Criterion: 1 | Card 234 red (D170; D157 Criterion 1): the process wiki check fails on a missing or malformed index, an unsummarised or unlisted page, a rule that does not resolve to its decision anchor, and a card or record without the baseline numbers and measurement methods | Claude | 1 file, +603 |
| 570dc5c | Criterion: 2 | Card 234 red (D170; D157 Criterion 2): pin seat briefs, floor, and caller | Copilot | 2 files, +130 |
| 707434b | Criterion: 2 (red fix) | Card 234 red fix (D170; D157 Criterion 2): resolve contradictory criterion-2 red fixtures | Copilot | 1 file, +58 |
| eb68d95 | Criterion: 3 | Card 234 red (D170; D157 Criterion 3): the restart-record check fails on missing token values, usage sources, restart fields or rule_miss_cases, and mob_process check calls it | Claude | 2 files, +969 |
| eb47780 | Criterion: 1 (1b, review) | Card 234 red 1b (review 3c6181c R7, R10; D157 Criterion 1): add quote-verification reds and named mutation probes | Copilot | 1 file, +139 |
| ae04522 | Criterion: 2 (2b, review) | Card 234 red 2b (review 3c6181c R6, b37a8ad W1-W4; D157 Criterion 2): route rows and seat-prompt red | Copilot | 2 files, +162 |
| daf3e6d | Criterion: 2 (2c, review) | Card 234 red 2c (review 3c6181c R2-R4; D157 Criterion 2): pin source-key and seat-pair mutation failures | Copilot | 1 file, +112 |
| ef38d5e | Criterion: 3 (3b, review) | Card 234 red 3b (review 3c6181c R1, R2, R5; D157 Criterion 3): add restart-key/report and restart-evidence reds | Copilot | 1 file, +199 |

Which were Copilot seats, and how I know: the record's cycles name "Copilot seat" for 570dc5c, 707434b, eb47780, ae04522, daf3e6d and ef38d5e, and "Claude seat" for eb68d95. token_usage lists Copilot CLI rows for red 2 fix (707434b), red 1b, red 2b, red 2c and red 3b. For c12f7f2 and c588838 the cycle line names no seat; the seats file row "QA (intent and red 1)" says claude-sonnet subagent, so I count them as Claude. 570dc5c has a Copilot seat in the cycle but no token_usage row (its seats-file row "QA (red 2)" is null: run with -s, not measured). So six of the eight reds are Copilot seats (five with a token value, one without) and two are Claude.

## Failing counts by red

Each count is from the commit body, with its command; none was re-run for this record. "Failing" is failure records; a body that also states tests failing by assertion is given.

- c12f7f2, at HEAD before any green: `python -m unittest tests.test_process_wiki`: Ran 53, FAILED (failures=53, errors=0); 49 tests fail by assertion (two tests report three subtests each), 4 born-green probes pass. `python -m unittest tests.test_process_wiki.ThisRepository`: Ran 8, failures=4. A throw-away reference checker run with PROCESS_WIKI_SCRIPT: Ran 53, failures=4 (exactly the four that need the real wiki). Full suite: not run.
- 570dc5c, at HEAD: `python -m unittest -q` on `tests.test_process_wiki.CriterionTwoSeatBriefsAndCoverage`, `tests.test_process_wiki.HostileInputsCriterionTwo` and the two named `tests.test_mob_process.MobProcessTests` caller tests: FAILED (failures=9, ran=10). With PROCESS_WIKI_SCRIPT set to a fresh scratch copy, `tests.test_process_wiki.CriterionTwoSeatBriefsAndCoverage.test_every_task_force_role_needs_a_role_page_and_a_brief`: FAILED (failures=1, ran=1). Broader suites the green touches: not measured.
- 707434b (red fix), HEAD: `python -m unittest tests.test_process_wiki`: Ran 61, FAILED (failures=20). Developer's prototype (PROCESS_WIKI_SCRIPT pointing at the scratch checker), same command: Ran 61, FAILED (failures=1), the real-repository test that needs the 8 seat brief files. This is a fix of fixtures, not a new red; twelve tests changed.
- eb68d95, HEAD: `python -m unittest tests.test_restart_record`: Ran 98, FAILED (failures=99, errors=0); 96 tests fail by assertion, 2 born-green probes pass. `python -m unittest tests.test_mob_process`: Ran 83, FAILED (failures=3, errors=0). Prototype green, same two commands: Ran 98, failures=2 (needs real files) and Ran 83, failures=2 (two existing window tests). Prototype full discover: Ran 478, failures=7, errors=1, skipped 7, of which 4 need a git checkout and were "not measured at HEAD".
- eb47780, HEAD: `python -m unittest tests.test_process_wiki.CriterionOneQuotesAndMutations`: Ran 7, FAILED (failures=11); mutation probes run=7, caught=0. `python -m unittest tests.test_process_wiki`: Ran 68, failures=11. Prototype, the first command: Ran 7, OK. Prototype, `python -m unittest tests.test_process_wiki`: Ran 68, failures=20 (existing tests the green changes, listed in the body).
- ae04522, HEAD: `python -m unittest tests.test_process_wiki.CriterionTwoSeatBriefsAndCoverage tests.test_process_wiki.HostileInputsCriterionTwo tests.test_mob_process.SeatPromptBuilderTests`: Ran 11, failures=11. Prototype copy, same command: Ran 11, OK. Three existing ThisRepository tests listed as changed by the green. Full suite: not measured.
- daf3e6d, HEAD: `python -m unittest tests.test_restart_record.CriterionTwoReviewFixesR2R4`: failures=6 of 6. `python -m unittest tests.test_restart_record`: failures=6 of 104 (98 pass). Prototype, run as a script path (`python <scratch>\tests\test_restart_record.py CriterionTwoReviewFixesR2R4`, not `-m unittest`): failures=0 of 6. Full suite: not measured.
- ef38d5e, HEAD: `python -m unittest tests.test_restart_record.CriterionThreeReviewFixesR1R2R5 tests.test_restart_record.ThisRepository`: Ran 13, FAILED (failures=8). Prototype, same command: Ran 13, failures=4; the new class alone: Ran 6, OK. The four remaining need the green's committed key, report, HANDOFF and index lines.

## Prototype-first practice

570dc5c was written without a prototype run. It contradicted itself: the default `Tree()` had to fail the role and brief check and also pass clean; the zero-rule tests could not pass under any floor; the disabled-page test named an id its brief never lists (record cycle 2, field "blocked", found by the developer ctx-5). The count `failures=9, ran=10` therefore measured tests that could not all go green, not a seam. 707434b fixed it, and its body states HEAD `failures=20` against the developer's prototype `failures=1`.

From then on every red here was run against a throw-away prototype green in a fresh scratch copy before commit, and the body states both numbers: eb68d95 (HEAD 99 then prototype 2), eb47780 (11 then the new class OK), ae04522 (11 then OK), daf3e6d (6 then 0), ef38d5e (8 then 4, the four named as needing real files). The practice: a red is committed only when a prototype makes every test that does not need a real committed file pass. The prototype is never committed (scratch folders under `.mob/records/234/scratch/`, left untracked). I did not find the practice written as a named rule in the record; the evidence is the bodies above. c12f7f2 predates the practice, though it did run a reference checker (Ran 53, failures=4), so it was partly prototyped.

## Reds that broke D136 (c12f7f2, eb68d95)

D136 sets a limit of 400 changed lines. c12f7f2 is +603 in one file and eb68d95 is +969 in two files (`git show --numstat`). Both were found at review (3c6181c R8) and recorded as a deviation by "QA engineer ctx-4" in the record; the handling recorded is that every fix red stays at or under 350 lines (triage 060edcb). The fix reds are eb47780 +139, ae04522 +162, daf3e6d +112 and ef38d5e +199; the largest is 199. The commit bodies of the two oversize reds do not mention the limit; the sizes were not checked before commit.

## Token usage, Copilot seats against Claude seats

Per the seats file, Claude values are `subagent_tokens` from the completion report (cumulative across the seat's turns); Copilot values are the Copilot CLI's own usage lines when run without `-s`, input plus output. The record marks the Copilot red-fix row (949200) as unverified: only a scratch .err line backs it, no raw file is committed. The red 1b, 2b, 2c and 3b rows come from scratch .err files named in token_usage and are not in the seats file.

| Seat | Commit | Model | Tokens |
|---|---|---|---|
| QA (intent and red 1) | c588838, c12f7f2 | Claude | 123407 |
| QA (red 2) | 570dc5c | Copilot, run with -s | not measured |
| QA (red 2 fix) | 707434b | Copilot | 949200 (in 937.9k, 881.8k cached; out 11.3k); 6 Premium requests, 3m 42s |
| QA (red 3) | eb68d95 | Claude | 142065 |
| QA (red 1b) | eb47780 | Copilot | in 1.6m (1.6m cached), out 16.2k; 6 Premium, 4m 37s |
| QA (red 2b) | ae04522 | Copilot | in 3.3m (3.1m cached), out 22.0k; 6 Premium, 16m 55s |
| QA (red 2c) | daf3e6d | Copilot | in 2.2m (2.1m cached), out 19.3k; 6 Premium, 11m 14s |
| QA (red 3b) | ef38d5e | Copilot | in 2.3m (2.1m cached), out 18.5k; 6 Premium, 13m 42s |

Against the card's baseline of 50K at setup and 125K at the end, the two Claude QA steps were 123407 and 142065 (the record's ctx-4 setup_tokens and end_tokens). The measured Copilot QA steps ran from 949200 to about 3.3m, roughly 7 to 27 times a Claude step (my arithmetic from the rounded values, not a recorded figure). Most of the Copilot input is cached. The facilitator's observation in the record: Copilot seats re-read far more than Claude subagents. Caveats: the two Claude rows are whole-seat totals over several turns, the Copilot rows are one run each, and the two providers count tokens differently, so this is a signal and not a like-for-like cost. Time to a commit for the Claude rows: not measured. One Copilot red (570dc5c) has no token value.

## One thing I would do differently

I would run the prototype-first step on the first red of each criterion, not only after a red had failed review. 570dc5c's nine failures included tests no green could pass, so it needed 707434b. And I would read `git diff --cached --numstat` before each commit: c12f7f2 (+603) and eb68d95 (+969) shipped over D136's 400 lines without the size being checked, and splitting them by seam (index and anchors; briefs and floor; key and report) would have avoided the deviation.
