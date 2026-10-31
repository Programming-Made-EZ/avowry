# Card 226 pre-ship gate (D154, D157), delivery lead ship, ctx-6

Seat: Claude (probes of 2026-10-31T06:49Z show Codex, Copilot, Grok and agy without usable quota, .mob/records/probes/2026-10-30.jsonl). In-flight line committed first (6ac91e3, on origin; 07:10:20 UTC). Checked 2026-10-31 07:11 to 07:25 UTC. No test was run by this seat; no gh call was made.
Bases on origin after `git fetch` in both repositories: avowry (phrase-lang) origin/main 6ac91e3; mob-defaults origin/main 56b39bc. Range under gate: mob-defaults 9cda13d..56b39bc, 24 commits (`git rev-list --count 9cda13d..56b39bc`), linear.

| # | Item | Verdict |
|---|------|---------|
| 1 | Hashes and record paths resolve on origin | PASS (61 of 61 hashes; every cited path but one runtime file) |
| 2 | Full-suite evidence | PASS, no re-run needed (583 OK at 43912b7, then a one-file test-only guard, 13 modules 245 OK) |
| 3 | CI | PASS for mob-defaults (no workflow); avowry has two workflows whose result is not read here (see below) |
| 4 | D157 order of "Criterion: n" | PASS, with four explained departures |
| 5 | D136 per commit | PASS (24 of 24 within 15 files and 400 lines; largest 333) |
| 6 | No trailers; line endings | PASS (0 trailers in 146 commits); two CRLF record files noted, no rewrite |
| 7 | Domain-expert question at ship (T2) | PASS on content; the answer lands with this file's commit |
| 8 | Open review condition: name the "probe" field in Done check 3 | CARRIED to the closer (not failed) |

## 1. Hashes and paths on origin

Method: `git fetch` in both repositories; the hashes were pulled with a regex (7 to 8 hex characters containing a digit and a letter) from record-in-progress.json, review-ctx5.md, board/226-*.md and .mob/records/decisions/226-*.json (two files); each was checked with `git cat-file -e <hash>^{commit}` and `git merge-base --is-ancestor <hash> origin/main` in each repository, and then `git branch -r --contains <hash>` on the repository that holds it.

- 61 distinct hashes. 61 resolve as commits and are ancestors of origin/main of the repository that holds them; none resolves in neither.
  - avowry (36): `git branch -r --contains` lists origin/main for all 36.
  - mob-defaults (25): `git branch -r --contains` lists origin/main for all 25. This includes the cycle commits 8357f14 to 56b39bc (all 24 of the range, as the record cites them) and 9cda13d.
- Record and code paths (regex over the same files gave 18 distinct strings, 17 real paths; I added record-in-progress.json, the two decisions files and the doc, so 20 checked (11 avowry plus 8 mob-defaults present, 1 absent)), checked with `git ls-tree -r origin/main --name-only` per repository (MSYS_NO_PATHCONV=1, because the Git Bash path conversion made a first `cat-file` pass report false absences; the ls-tree pass is the one counted):
  - avowry, present: .mob/records/226/ checklist-slice.md, checklist-taskforce.md, review-ctx5.md, turn-slice-architect.md, record-in-progress.json; .mob/records/blockers/in-flight.md; .mob/records/probes/2026-10-30.jsonl; .mob/records/decisions/226-slice.json and 226-taskforce.json; board/226-decision-seats-start-on-a-fresh-probe-and-rotate-like-card-seats.md; docs/decisions/process-and-staffing.md (11 of 11).
  - mob-defaults, present: decider.py, facilitator.py, mob_process.py, core/backends.json, tests/test_decider_gate.py, test_decider_launched.py, test_decider_pin_scrub.py, test_decider_record.py (8 of 8).
  - Cited but absent on both: `.mob/records/seat-use.jsonl` (also written as `seat-use.jsonl`). Same finding as card 208: it is the runtime history that card 208's rotation reads and appends to, not a record the card claims to exist; `ls .mob/records/seat-use.jsonl` in avowry says no such file, and `git ls-tree -r origin/main --name-only | grep -F seat-use` is empty in both repositories. Noted, not a failure. Consequence for ship: the card's delivery-lead brief says this seat stages seat-use.jsonl; there is no such file to stage, so none is staged (the rotation tests use temporary repos).
- Not cited, local only at the time of the check: 9b5874b (QA turn record) and c4a5387 (developer turn record) are local commits of other seats, ahead of origin by 2; the record does not cite them. Not mine to push.

PASS.

## 2. Full-suite evidence: accept the 43912b7 run plus the 13-module run at 56b39bc; no re-run

- The last green's single full run: `python -m unittest discover -s tests` in C:\Users\ringe\mob-crosscli at 43912b7 (green for criterion 3, the F2 and F3 fix): Ran 583 tests in 1353.595 s, OK (skipped=1), started 05:54:55 UTC (record cycles, last criterion 3 entry; commit body of 43912b7 line 27 to 28 says the same, `git log -1 --format=%B 43912b7`). The commit is dated 06:18:27 UTC, 23 minutes after the start, which fits a run on the tree being committed.
- Then one more commit, 56b39bc, the test-only guard (R1): `git diff --stat 43912b7 56b39bc` prints 1 file changed, 14 insertions(+), 4 deletions(-), tests/test_decider_launched.py; `git rev-list --count 43912b7..56b39bc` is 1. No production file, fixture or shared helper changed, so the code under test at 56b39bc is byte-identical to the code the 583 run saw.
- What 56b39bc's body records for itself (`git log -1 --format=%B 56b39bc`): `python -m unittest tests.test_decider_launched` at 43912b7: Ran 28, OK; the 13 decider and seat modules in one `python -m unittest` call: Ran 245, OK. Those 13 are test_decider_launched, pin_scrub, probe, gate, rotation, record, record_units, runner, test_seat_gate, seat_rotation, seat_rotation_rules, quota_first and probe_model. The independent reviewer re-ran independently (review-ctx5.md, second re-check): the eight decider modules on the good tree, Ran 154, OK; the guard alone, 12 runs, 12 OK on the good tree and 12 FAILED on the M4 mutant; the eight modules on the mutant, 3 runs, each Ran 154, FAILED (failures=1). These are the seats' counts; this seat ran nothing.
- Pollution check by reading (the class of risk that failed 13 tests at card 208): `grep -rln 'test_decider_launched\|LaunchedCase' .` in mob-defaults finds only tests/test_decider_launched.py itself, so no other module imports from it. The diff adds `import threading`, a local `calls` dict and lock, and replaces `decider.vote_seat` through `mock.patch.object` used as a context manager, which restores it on exit. No module-level state is touched.
- Verdict: sufficient; no re-run needs to happen. Reasoning: the only change since the full run is one test file that nothing else imports, with its patch scoped to a `with` block, and the modules that could interact with it (the 13) ran green together at the head. A re-run would cost about 22 minutes (1353.6 s at 43912b7) for no new information. If the facilitator wants a discover run at 56b39bc itself anyway, the command is `python -m unittest discover -s tests` in C:\Users\ringe\mob-crosscli, with a cap of about 30 minutes; this seat has not run it.
- Limit: no single `discover` run has been taken at 56b39bc itself. The 583 run and the 245 run together stand in, on the argument above.

PASS.

## 3. CI

- mob-defaults: `git ls-tree -r origin/main --name-only | grep -c '^\.github'` prints 0 and `... | grep -c -i workflow` prints 0 (56b39bc). There is no workflow, so there is no CI to be green or red. Same finding as card 208 (which confirmed it with two gh calls). The code for this card lives only there.
- avowry (the record repository, 6ac91e3): `git ls-tree -r origin/main --name-only | grep '^\.github'` lists CODEOWNERS and two workflows, .github/workflows/ci.yml (on push to main and on pull requests to main; runs `python3 scripts/mob_process.py check` and a previous-checker step) and .github/workflows/mob-process.yml (on issue close, reopen and unlabel, every 30 minutes, and manually; the same checker). They check the record repository, not the mob-defaults code. This card's commits in avowry are records only (board, .mob/records, MOB.md).
- Not verified here: the result of those two workflows on the pushed head. The task said no gh calls, and the push has not happened yet. The facilitator reads the run after the push.

PASS for the code repository (absence confirmed); the avowry run is open until after the push.

## 4. D157 line: order of "Criterion: n" (mob-defaults, 9cda13d..56b39bc, 24 commits, linear on origin/main)

Method: for each of the 24 commits, `git log -1 --format=%B <hash> | grep -oE '^Criterion: *[0-9]+'`. Rule (board/226 Context): one criterion per red-green cycle; "Criterion: n" in each red commit body; the green for criterion n before any red for n+1; greens pushed at once; commits within D136, citing D163 and D164.

| Commits in log order | Criterion line |
|---|---|
| 8357f14 red, 372d4b0 and ddbf6ff units, 2189ca2 green | 1, 1, 1, 1 |
| d3826ea structural | none (see departure 1) |
| a54c9d9 red, df9d9d7 82d8c2f 134e25a a217647 081281b 10a6f9d units, ed5b0e1 green | 2 x 8 |
| 9c8bb71 red, 2ebedd0 c466110 e6d24bb units, 165fde8 green | 3 x 5 |
| 4810ed8 red, fec8489 green, c65ec21 guard (review F1) | 2, 2, 2 |
| 3a0819f red, 43912b7 green (review F2, F3) | 3, 3 |
| 56b39bc guard (re-check R1) | 2 |

- Main sequence: 1, 2, 3, each green (2189ca2, ed5b0e1, 165fde8) before the next red (a54c9d9, 9c8bb71). No red for n+1 starts before the green for n.
- Departure 1, d3826ea: no "Criterion:" line in the body (`grep -ci Criterion` finds only the subject, "D157 Criterion 2"). It is the architect's structural commit (the slice's split of gate_seat into check_seat and commit_seat, "no behaviour change", between the criterion 1 green and the criterion 2 red). Its body gives the before and after test runs. Explained: it is not a red.
- Departure 2, the return to 2 after 3 (4810ed8, fec8489, c65ec21): review fix F1 on criterion 2, raised by the reviewer's first review (69ff413). It is a new red-green cycle, not an overlap: the red 4810ed8 follows the criterion 3 green 165fde8, and its green fec8489 precedes the next red 3a0819f. Then the criterion 3 cycle for F2 and F3 (3a0819f, 43912b7).
- Departure 3, c65ec21: a born-green guard for mutation M4, with no red commit of its own. The body and the record say it was shown red against M4 in a scratch copy (1 failure). The reviewer found it racy (6 of 8 runs failed on the mutant) and requested the fix.
- Departure 4, 56b39bc: a test-only guard after the criterion 3 green, again born green, replacing the racy guard so M4 fails on every run (reviewer: guard alone 12 of 12 FAILED on the mutant; APPROVE at 6f73e2e). No red for it; the mutation proof in its body stands in. Recorded as a deviation, no rewrite.
- Every subject cites D157 and a criterion; 18 of 24 also cite MD-002 (the six review-fix and guard subjects 4810ed8, fec8489, c65ec21, 3a0819f, 43912b7 and 56b39bc cite the review finding instead; recount by `git log --format=%s 9cda13d..56b39bc | grep -c MD-002`); none carries trailers. D163 and D164 are cited in the card text and the record; this seat does not claim they are repeated in each of the 24 subjects (they are not).

PASS.

## 5. D136 per commit (limits: at most 15 countable files and 400 changed lines per commit, docs/decisions/process-and-staffing.md D136 part 2)

Method: `git log --reverse --format='@@%h' --numstat 9cda13d..56b39bc`, summed per commit (insertions plus deletions; a CRLF file counts the same under `git diff --ignore-cr-at-eol --stat`, 10 files, 1993 insertions, 18 deletions both ways). Countable files: all are .py files in mob-defaults; no board, decisions, MOB.md or record file is in the range.

- Files per commit: 1 or 2 for all 24 (maximum 2). Lines per commit, in log order: 8357f14 166, 372d4b0 120, ddbf6ff 66, 2189ca2 40, d3826ea 27, a54c9d9 208, df9d9d7 4, 82d8c2f 124, 134e25a 40, a217647 37, 081281b 36, 10a6f9d 39, ed5b0e1 44, 9c8bb71 333, 2ebedd0 100, c466110 56, e6d24bb 67, 165fde8 58, 4810ed8 275, fec8489 5, c65ec21 9, 3a0819f 202, 43912b7 13, 56b39bc 18.
- Largest: 9c8bb71 (red, 333 lines, one file, tests/test_decider_record.py), then 4810ed8 (275). Both are under 400. 24 of 24 within the limits; no D136 deviation arises on this card (card 208's d3125ea at 463 lines had one).
- The range as a whole (10 files, 1993 insertions across 24 commits) is not a single commit; D136's per-card limits (three criteria, three increments) are the slice's, checked at the slice window (226-slice SHIP AS IS).

PASS.

## 6. Trailers and line endings

- Trailers: `git log -1 --format=%B <hash> | git interpret-trailers --parse | wc -l` plus a `grep -ciE '^(co-authored-by|signed-off-by|reviewed-by|generated with)'` per commit: 0 lines over the 24 mob-defaults commits. The same grep over avowry `git rev-list bff6f2a^..HEAD` (122 commits, from the card's first commit to the head): 0 lines. Total 0 over 146 commits.
- Line endings, mob-defaults (`git ls-files --eol`, and CR counts per changed file at 9cda13d and 56b39bc): the eight test files and mob_process.py and core/backends.json are LF (0 CR). decider.py is CRLF index and worktree (i/crlf w/crlf); it had 207 CRs in 207 lines at 9cda13d and has 327 CRs in 327 lines at 56b39bc, so every line stays CRLF with no mixed endings. facilitator.py is CRLF the same way (1634 of 1634 lines, then 1650 of 1650); the d3826ea body notes the stat equals the `--ignore-cr-at-eol` stat. These files were CRLF before the card, so nothing was converted.
- Line endings, avowry: the card-226 files `.mob/records/226/` checklist-slice.md, checklist-taskforce.md, record-in-progress.json, review-ctx5.md and the board card have 0 CR (`tr -cd '\r' | wc -c`). Two CRLF files, both older than this gate and not mine: .mob/records/226/turn-slice-architect.md (99 CRs in 99 lines, i/crlf; card 208's turn-slice-architect.md is CRLF the same way, so it is the architect seat's pattern) and .mob/records/probes/2026-10-30.jsonl (92 CRs). Neither is cited as a line-ending claim anywhere; I recorded and did not rewrite them. Suggest the retro: the architect turn record is written as CRLF on both cards.
- This file: written with LF; the pre-commit check `git diff --cached | tr -cd '\r' | wc -c` is run before the commit (result in the commit step).

PASS.

## 7. Domain-expert question at ship (T2)

Should a member with different domain knowledge or context join at ship? No.
- The record's earlier answers (domain_expert entries: refinement ctx-1 no, slice ctx-2 no, after criterion 1 ctx-6 no) hold; this is the ship answer.
- The ship boundary adds no code and no trust boundary: the checks are hashes on origin, test evidence and commit order, all covered by the seats already on the task force.
- The vendor-knowledge question the card raised, whether the probe on the pinned model reads the quota of the model a voter will launch (review F2), was answered in the code (3a0819f red, 43912b7 green: the voter launches with the backend's probe_model pin, fallback default) and closed by the reviewer in the re-check (5e06fce) and approved at 6f73e2e. No open domain question remains.
- What remains open are product choices, not domain knowledge: which Claude model family a Claude seat uses (product owner, D164) and a ratio cap (not in this card).

PASS. The answer is new at ship, so its hash is the commit that adds this file.

## 8. The open review condition: name the "probe" field in Done check 3

Carried to the closer. It is not failed and it is not this seat's to close.

- The condition: review-ctx5.md, "The item left to me" (line 25 to 27) accepted the top-level probe object in decider-result.json and the "Probe in force" log line (taken_at plus, per backend, ok and evidence), on condition that the card close names the field in the Done check 3 text; the second re-check (line 84) says it "stays open for the closer". record-in-progress.json reviews[0].probe_object and reviews[1].condition_open say the same.
- Why it matters: Done check 3 reads "names each voter's model, probe time and reason", which does not mention the top-level `probe` object the recompute needs (a Claude voter is judged on the other models' ok flags, and those models are usually not voters).
- Check at ship: `board/226-*.md` Done check 3 still reads as above, so the condition is open as expected. The closer edits the issue (the board file is generated, so the issue is the source) at close and cites this item.

CARRIED (open, owner: the closer).

## For the product manager (sign-off, ctx-7)

Nothing in this gate blocks the push. Not verified by this seat: a `discover` run at 56b39bc itself (item 2 limit); the avowry workflow result on the pushed head (item 3); the "probe" field naming (item 8, carried). The mob-defaults commits are on origin (56b39bc); this seat did not push avowry.
