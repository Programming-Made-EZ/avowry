# Card 234 independent review (ctx-6, audit lens, Claude seat)

Verdict: CHANGES REQUESTED

Reviewed at b37a8ad: criterion 1 (c12f7f2, c941885), criterion 2 (570dc5c, 707434b, e724cdf, f9a3236, 042bf79), criterion 3 (eb68d95, 9daab9f, 8c4372d). I wrote none of it.

## What holds (commands I ran)

- `python -m unittest tests.test_process_wiki tests.test_restart_record tests.test_mob_process`: Ran 242, OK (119 s). Full suite not run, as asked.
- `python scripts/check-process-wiki.py` prints "process-wiki: OK (17 pages)". `python scripts/check-restart-record.py` prints "restart-record: OK (1 restarts)".
- Callers outside tests: scripts/mob_process.py:473 and :489 define check_process_wiki and check_restart_record; :514 calls both from run_check (the pre-push check). Built means called holds for both checkers.
- `git diff --stat c12f7f2~1 HEAD -- TEAM_WORKING_AGREEMENT.md docs/decisions` prints nothing: no rule or decision was changed. `git diff --diff-filter=D --name-only c12f7f2~1 HEAD` prints nothing.
- The ten commits: CR bytes 0 in each (`git show <c> --format= | tr -cd '\r' | wc -c`); no Co-Authored-By or Signed-off line. D157 order holds. The criterion 2 red fix 707434b repaired a self-contradicting red as its own commit.
- My mutations on fresh copies under the scratchpad (ctx6-mut-*): a repeated page id in a brief gives exit 1; a decision link changed to #d999 gives exit 1 "mismatched anchor". The record's own proof mutation copies (scratch/proto-c3-proof-1 and -2) exist and differ from HEAD in the record, as the green 3 body says.
- Token arithmetic: 937.9k + 11.3k = 949.2k and 519.0k + 9.4k = 528.4k; facilitator delta 782199 - 965000 = -182801. All consistent.

## Findings

1. HIGH. .mob/records/234/record-in-progress.json:104-123 and HANDOFF.md:5. Done check 3 is not met: no facilitator restart happened. usage/facilitator-2026-11-01-0624.json says in its own note "one reading ... before any restart from the wiki". The 782199 (delta -182801 against 965K) is the old long session's size, yet restarts[0] presents it as a restart from the wiki with `wiki_pages_read`. The same Claude Opus session continues. HANDOFF.md is still the 2026-10-29 handoff with one added line "Boundary B1: 3b70513..." (8c4372d). The check proves that a file exists, not that a restart took place. Fix: run a real restart (fresh facilitator reads the index and handoff, then commit its own context reading), or record criterion 3 as not met and say the delta is not a restart measurement.
2. HIGH. scripts/check-restart-record.py:122-183 and :296-304. The guards cannot detect invented values. Copy mutations, all exit 0: seat ctx-1 given setup_tokens=1 and end_tokens=1 pointing at the seats file; facilitator_context_tokens set to 5 with the delta recomputed; rule_miss_cases_zero_source pointed at HANDOFF.md; every seat set to not_measured with reason "x". The checker needs a whole number and an existing committed file and never compares a value with its source. Done check 2 "fails on partial work" is false: the real record passes with all eight seats not_measured. Fix: the source must contain the value (parse the usage JSON by context), and not_measured must be rejected where the seats file holds a number.
3. HIGH. record-in-progress.json:70-103 against usage/seats-2026-11-01-0626.json. Done check 2 (tokens per seat against 50K and 125K) has no measurement. All eight seat_metrics are not_measured with one generic reason, including ctx-3, ctx-6 and ctx-8, which had not run when it was written, while the seats file holds numbers for ctx-4, ctx-5 and ctx-7. No value is compared with 50K or 125K except in prose. The Claude totals are cumulative subagent_tokens, not setup and end checkpoints (the file says so). The adopted condition asked for numbers and a method; the method exists, the numbers do not. Fix: record per-context totals with the cached share, state "setup checkpoint not measured", and give the ratio to 125K.
4. MEDIUM. record-in-progress.json:361-385 (token_usage). The sources are named as scratch files (red-234-2fix.err, sd2-234.err, green-234-3.err) that are not committed (git ls-files .mob/records/234 lists none; scratch/ is untracked and holds no .err). The green 3 figure 6.7m in, 6.5m cached, 31.9k out appears only as prose (lines 350 and 380) and is not in usage/seats-*.json. The Claude values in the seats file (123407, 114056, 151676, 77112, 142065) are copied completion-report numbers with no committed transcript. I can neither confirm nor refute them: unverified, not shown to be invented. Fix: commit the raw usage lines or transcript fields the figures came from.
5. MEDIUM. record-in-progress.json:119-123. rule_miss_cases is empty and its zero-case source is the facilitator context-size file, which says nothing about rule misses. The zero is asserted, not evidenced, and no seat is recorded as asked what it missed. Fix: cite a source that shows the observation (each seat's reading list against the pages its brief names), or record "not measured".
6. MEDIUM. docs/process-wiki/briefs/*.md. Nothing launches a seat from a brief: git grep outside tests and docs/process-wiki finds no consumer of briefs/. TEAM_WORKING_AGREEMENT.md (67072 bytes) is unchanged and still the reading; the wiki is 9574 bytes beside it. Briefs are validated only against themselves, and are not listed in INDEX.md (the technical writer's finding 1). This review's own prompt was hand-written. The token saving the card exists for is not yet in force. Fix: a caller that builds each seat's start prompt from its brief, or state in the record that routing is not wired.
7. MEDIUM. scripts/check-process-wiki.py:241-256 and the page text. The wiki can state false rules and pass: rewriting the seat-rotation rule to "whichever model the facilitator likes" keeps the D163 token and link and exits 0. The floor counts anchored wiki lines against agreement bullets, so padding counts (my padding run was inconclusive because the real share is already 0.34). "Every rule still resolves to its decision" holds for the link, not for the content. Fix: record this limit on the card, or compare page rule text with the decision text.
8. MEDIUM. D136 part 2 (400 changed lines per commit; tests count, only board/, docs/decisions/, MOB.md, .mob/records/, docs/retros/ and generated fixtures are excluded). eb68d95 has 969 added lines and c12f7f2 has 603. There is no Slice-exception line and no deviations entry (deviations holds only the late intent confirmation). 9daab9f at 398 lines is 2 under. Fix: record the deviation for the retro; the reds were not split.
9. LOW. Commit bodies. c941885 and 9daab9f have no Proof block (stand-down 1 and 2 say the facilitator returns a green without one); 9daab9f ships a checker with no caller until 8c4372d wires it, so built means called holds only across the pair. 8c4372d names "a python scratch-copy runner" without showing it, and its CR check is "an equivalent", not the stated command. Red 3 eb68d95 still says "confirmation: pending (facilitator)" though the intent was confirmed at 04:51:05Z. The 042bf79 mutation count (6 run, 6 caught) is consistent with its body.
10. LOW. record-in-progress.json:210: criterion 1 mutations "not measured". The green c941885 (4 pages, about 3 percent of rules) was shown fail-capable only by the red's born-green probes.

## Copilot seats: what stays unverified

Unscanned (command_scans): ctx-1 ready, ctx-2 slice and amendment, ctx-5 green 1 (c941885), ctx-4 red 2 and red 2 fix (570dc5c, 707434b), ctx-7 stand-down 2, ctx-5 green 3 (9daab9f, 8c4372d). Nobody has verified for these: that no delete command ran (the git tree shows no deleted file, but not scratch, temp or out-of-tree), that the mutation claims in 570dc5c ("fresh scratch COPY") and 8c4372d ran as written (I confirmed only that the two proto-c3-proof copies exist), the counts in the Copilot-written stand-down 2, and any token or time claim that came from a Copilot seat. Committed results, files and test runs are independently checkable and passed; the commands that produced them are not.

## Requested before approval

Findings 1, 2 and 3 (or an honest downgrade of Done checks 2 and 3 to "mechanism built, measurement not made"), then 4 and 5. Findings 6 to 10 may go to the retro if the product owner accepts them.

## Re-check (ctx-6)

Verdict: APPROVE

Re-check scope: first review 3c6181c (R1..R10), triage 060edcb, fixes through 68108b7 and the restart evidence commits, record-in-progress.json, and usage files. I wrote none of the code.

Validation run:
- `python -m unittest tests.test_process_wiki tests.test_restart_record tests.test_mob_process` -> Ran 267, OK.
- `python scripts/check-process-wiki.py` -> `process-wiki: OK (18 pages)`.
- `python scripts/check-restart-record.py` -> `restart-record: OK (2 restarts)` with computed seat_ab and median notes.

Fresh-copy mutations (including the four from the first review) were re-run via `.mob/records/234/mutate-green-2c.py`: clean copy passed; all 10/10 mutations failed as expected, including:
1. `ctx-4 setup=end=1` -> source mismatch.
2. `facilitator_context_tokens=5` with recomputed delta -> source mismatch.
3. `rule_miss_cases_zero_source=HANDOFF.md` -> rejected (not a usage file).
4. all seats `not_measured` -> rejected when source has numbers.

Restart reality checks:
- Key/report commit order is correct: key `68108b7` is older than report commit `1bafca7`.
- Report file is the seat text plus the marked token line (`subagent_tokens: 83666`) appended by facilitator, as stated in the file footer note.
- Misses are computed, not asserted: mutating the report in a fresh copy to drop `rules/thin-slices` produced `rule_miss_cases mismatch restart 2: record 0, computed 1`.

Measured pair check:
- `measure-qa-red-full.json` total = `2 + 69003 + 0 + 696 = 69701`.
- `measure-qa-red-brief.json` total = `2 + 18311 + 30341 + 378 = 49032`.
- These match `seat_ab.full_tokens`/`brief_tokens` and checker output ratio `0.70`.

Deviations weighing:
- D136 line/file-limit and push-lag deviations are now explicitly recorded in `deviations` and no longer hidden.
- The previously open commit-body-proof concern is explicitly split to follow-up card `#235`; this is appropriate because prior commit bodies cannot be amended under this flow.

R1..R10 status:
1. R1 (no real restart): **closed** (real restart entry present; key-before-report; computed miss check passes).
2. R2 (checker accepted invented values): **closed** (all first-review and added mutations fail).
3. R3 (Done check 2 unmeasured): **closed** (measured pair recorded and source-checked; remaining unavailable contexts explicitly marked with source-backed reasons).
4. R4 (raw token-source unverifiable): **closed** for Done-check gating (source-checked `seat_metrics`/`seat_ab` enforced; unverified rows are explicitly flagged by checker).
5. R5 (rule_miss_cases zero asserted): **closed** (restart miss count is computed from key/report; zero-source misuse now fails).
6. R6 (briefs not consumed): **closed** (`seat-prompt` consumer added; routes/brief pages are checked).
7. R7 (false rule text could pass): **closed** (verbatim quote checks added and enforced).
8. R8 (oversize reds not recorded): **closed** (deviation recorded in card record).
9. R9 (green body proof gaps): **moved to card 235** (`board/235-green-commit-bodies-carry-a-checked-proof-block.md`).
10. R10 (criterion-1 mutations not measured): **closed** (counted mutation evidence now present and re-checkable).
