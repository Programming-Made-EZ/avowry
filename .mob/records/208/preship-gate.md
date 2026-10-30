# Card 208 pre-ship gate (D154, D157), delivery lead ship, ctx-6

Seat: Claude (probes of 2026-10-30T23:03Z show Codex, Copilot, Grok and agy without usable quota, .mob/records/probes/2026-10-30.jsonl). In-flight line committed first (5d3a0ad, on origin). Checked 2026-10-30 23:04 to 23:15 UTC.
Bases on origin: avowry (phrase-lang) origin/main ffacf64 at fetch; mob-defaults origin/main 9cda13d.

| # | Item | Verdict |
|---|------|---------|
| 1 | Hashes and record paths resolve on origin | PASS |
| 2 | Full-suite evidence | PASS (earlier run accepted plus module runs, no re-run) |
| 3 | CI | PASS (no workflow exists; nothing to be green) |
| 4 | D157 order of "Criterion: n" | PASS, with two explained departures from a monotone sequence |
| 5 | D136 deviation d3125ea (463 lines) | PASS (acknowledged) |
| 6 | Domain-expert question at ship (T2) | PASS on content; the answer lands with this file's commit |

## 1. Hashes and paths on origin

Method: a fresh `git worktree add --detach` of each origin/main (wt-208-gate-avowry at ffacf64, wt-208-gate-mobdefaults at 9cda13d), then `git cat-file -e` per object and `git merge-base --is-ancestor <hash> origin/main`; both worktrees removed afterwards with `git worktree remove`.

- 35 distinct hashes cited in record-in-progress.json, review-ctx5.md and .mob/records/decisions/208-*.json. All 35 resolve and are ancestors of origin/main of the repository that holds them.
  - mob-defaults (21): 83714f1 38075b7 26aaa0c e81ed21 6af1cca 56b3686 832ca6a 36f09a0 40cc57c 6effcdb f24885e be9ec1b d3125ea 249eaa2 fcc396d 10d9db5 fa88edb 2a01531 9cda13d 44112b4 e239b12.
  - avowry (14): 66d833f b0d1310 6bd8db2 f1b9b9a b3c246b 1be07da 54d6dc4 023e582 2625dd3 30cf8a2 3a4f777 4b51ec6 575163a b138c0e.
- Record paths: 17 of the 18 cited paths exist as blobs on avowry origin/main: the six 208 checklists and turn (checklist-208-cardtext, -rerank-9, -rerank-10, checklist-slice, checklist-taskforce, turn-slice-architect), record-in-progress.json, review-ctx5.md, blockers/in-flight.md, probes/2026-10-30.jsonl, reranks/2026-10-30-8b.md, the five decisions/208-*.json windows (cardtext, rerank-9, rerank-10, slice, taskforce) and docs/decisions/process-and-staffing.md.
- Cited but absent on origin: `.mob/records/seat-use.jsonl`. It is not a record the card claims exists; it is the runtime history the new rotation reads and appends to (facilitator.py, the seat-use path), created at the first seating the runner makes. No such seating has happened yet, so no file exists. Noted, not a failure: until then the history is empty and every available model counts as never seated.
- Code paths cited (mob-defaults 9cda13d): tests/test_probe_model.py, tests/test_quota_first.py, tests/test_card_mode.py, core/backends.json, mob_process.py, facilitator.py all exist.
- Observation: C:\Users\ringe\mob-crosscli has an untracked `.mob/` directory that this seat did not create and did not touch.

## 2. Full-suite evidence: accept the 2a01531 run plus module runs; no re-run

- Last full run: 444 tests OK, 1 skip, started 22:24:25 UTC, at the tree of 2a01531 (record cycles, full_suite; seat lines in .mob/records/blockers/in-flight.md).
- `git diff --stat 2a01531 origin/main` in mob-defaults: one file, tests/test_probe_model.py, 35 insertions, which is 9cda13d alone. No production file, fixture or shared test helper changed, so every other test sees the same code and helpers as in the 444 run.
- The new tests build their own backend dicts through the local `backend()` helper, patch the environment with `mock.patch.dict` in setUp (undone by addCleanup) and use a temp dir; they touch no module-level dict. This matters because the earlier full run on this card failed 13 tests from BACKENDS pollution (fixed in fa88edb), a defect targeted runs had hidden. So I checked that class of risk directly instead of trusting the module run alone:
  - `python -m unittest tests.test_probe_model` at 9cda13d: Ran 16, OK (13 before plus the 3 new).
  - `python -m unittest tests.test_quota_first tests.test_seat_gate tests.test_seat_rotation tests.test_probe_model` at 9cda13d in one process (the modules that read or write BACKENDS, the new tests last): Ran 73, OK, about 2 s.
  - Adding tests.test_card_mode to that process ran past my 7 minute bound and my own `timeout` stopped it; it gave no verdict and is not counted as evidence. No process of mine remains.
- Why not re-run: the full suite costs about 21 minutes (1258 s), the change is test-only and isolated, and the two runs above exercise the only plausible interaction. A re-run would need a second in-flight line and a 31 minute cap for no new information. The command stays available: `python -m unittest discover -s tests` in C:\Users\ringe\mob-crosscli.
- PASS on hash 9cda13d (origin/main of mob-defaults). Limit: the product code at 9cda13d has not itself been through one full discover run; it is identical to 2a01531 outside the one test file.

## 3. CI

`gh api repos/Programming-Made-EZ/mob-defaults/contents/.github/workflows` returned 404 Not Found, and `gh api repos/Programming-Made-EZ/mob-defaults/actions/workflows --jq .total_count` returned 0 (two gh calls, within the limit of four). mob-defaults has no workflow, so there is no CI to be green or red; the local evidence in item 2 stands in. PASS (absence confirmed).

## 4. D157 line: order of "Criterion: n" (mob-defaults, 83714f1 to 9cda13d, 19 commits, linear on origin/main)

Rule (board/208 Context): one criterion per red-green cycle; "Criterion: n" in each red commit body; the green for criterion n before any red for n+1.

| Commits in log order | Criterion line |
|---|---|
| 83714f1 red, 38075b7 and 26aaa0c units, e81ed21 green | 1, 1, 1, 1 |
| 6af1cca red, 56b3686 and 832ca6a units, 36f09a0 green | 2, 2, 2, 2 |
| 40cc57c red, 6effcdb and f24885e units, be9ec1b green | 3, 3, 3, 3 |
| d3125ea red, 249eaa2 fcc396d 10d9db5 fa88edb units, 2a01531 green | 2 x 6 (the D164 correction) |
| 9cda13d review fix F1 | 3 |

- Original cycle: 1, 2, 3, each green (e81ed21, 36f09a0, be9ec1b) before the next red (6af1cca, 40cc57c). No breach.
- Departure 1, the return to 2 after 3: the sequence is not monotone. It is the corrective cycle for D164, which corrected Done check 2 (least-recently-used rotation replaces the per-seat Claude ban) after the original criterion 2 had shipped and criterion 3 was green. It is a new red-green cycle, not an overlap: the red d3125ea follows the green be9ec1b, and the green 2a01531 precedes the next criterion line. No red for n+1 starts before the green for n. Evidence: D164 in docs/decisions/process-and-staffing.md, board/208 Done check 2 (corrected), the corrected cycle in record-in-progress.json, review-ctx5.md line 67.
- Departure 2, 9cda13d: a "Criterion: 3" line after the criterion 2 green is the review fix, not a new cycle. It is a test-only commit that was green when written (the branch it tests already existed) and carries its own mutation proof in the body (branch disabled: 6 failures; restored: Ran 16, OK). It is the one born-green test, so the red-first rule (D137) is shown by the mutation instead of a red commit. The independent reviewer raised F1 and re-checked it closed (review-ctx5.md, 41ca560 on origin). Recorded as a deviation, no rewrite.
- Also in order: every subject cites MD-002 and, for the correction, D164; no trailers (the reviewer recorded the same).

## 5. D136 deviation (d3125ea, 463 lines)

d3125ea changes two test files, tests/test_card_mode.py (+124) and tests/test_seat_rotation.py (+339), 463 lines in one red. The independent reviewer recorded it and accepted it (review-ctx5.md line 67: one criterion, one layer, tests only; the acceptance tests need the unit-level doubles beside them), and the commit body says "Tests only; the facilitator and mob_process are unchanged". I acknowledge it as the recorded D136 deviation; it is not a finding against ship and no rewrite follows. Evidence on origin: d3125ea (mob-defaults) and 41ca560 (avowry). PASS.

## 6. Domain-expert question at ship (T2)

Should a member with different domain knowledge or context join at ship? No.
- The ship boundary adds no code and no trust boundary: the checks are hashes on origin, test evidence and commit order, all covered by the seats already on the task force.
- The one vendor-knowledge question the card raised, whether a probe on the pinned model reads the quota of the model a seat will launch (review F3), was answered in the code (seat and probe launch the pinned model under reordered models plus a default key) and re-checked closed by the reviewer in ctx-5. No open domain question remains.
- What remains open are product choices, not domain knowledge: which Claude model family a Claude seat uses (left to the product owner, D164 C5) and a ratio cap on Claude seats (not in this card).

Verdict PASS. The answer is new at ship, so its hash is the commit that adds this file.

## For the product manager (sign-off, ctx-7)

Nothing blocks. Not verified: a full `discover` run at 9cda13d itself (item 2 limit). The mob-defaults commits are on origin (9cda13d); this seat did not push.
