# Card 219 pre-ship gate (D154, D157, ctx-6, delivery lead, Claude seat)

Checked against origin/main e60a511ab270748c824893c4379db020dcd4ddc1 (fetched 2026-10-31 00:47 UTC; the local in-flight line c1fd36e is not on origin and is not part of the checked tree). Seat start 2026-10-31 00:47:51 UTC (last probe age 13 min, probe of 2026-10-31T00:34:50Z; cap 01:12:47 UTC). Seat is Claude because the 00:34 UTC probes show Codex, Copilot, Grok and agy without usable quota.

Overall: PENDING on item 3 only (CI run still in progress). Items 1, 2, 4, 5, 6 PASS.

## 1. Cited hashes and record paths resolve on origin: PASS

Fresh `git worktree add --detach` of origin/main (scratchpad wt-219-gate). Scanned .mob/records/219/record-in-progress.json, .mob/records/219/review-ctx5.md, .mob/records/decisions/219-slice.json and 219-taskforce.json: 24 distinct commit hashes cited (1a08bd6 1c850b0 21b9cb5 2bc05f6 37e456c 38ff854 3b04e20 451f4ea 55b5794 58e4b81 5a2d281 5a3d4af 66904ab 704bb02 79e0892 8f262a8 ab94601 ae7f982 b374ef7 bca5510 d02114d e4cda78 f4627fc fcffe85). Each: `git cat-file -e <h>^{commit}` succeeds and `git merge-base --is-ancestor <h> origin/main` holds (zero MISSING, zero NOT-ANCESTOR). Cited paths present in the origin tree: .mob/records/219/{checklist-slice.md, checklist-taskforce.md, record-in-progress.json, turn-slice-architect.md, review-ctx5.md}, .mob/records/decisions/219-{slice,taskforce}.json, .mob/records/blockers/2026-10-30-209-record-order.md, .mob/records/blockers/in-flight.md, .mob/records/probes/2026-10-30.jsonl, board/219-the-record-checker-accepts-cards-that-close-out-of-number-order.md, docs/decisions/process-and-staffing.md, scripts/mob_process.py, tests/test_mob_process.py. (209.json and 208.json appear in the record as the held records, not yet on origin by design; they land after ship, with the MOB.md regeneration, review finding 3.)

## 2. Whole Python suite at origin/main: PASS

`python -m unittest discover -s tests` in the clean worktree at e60a511, 00:48:18 to 00:50:18 UTC: Ran 313 tests in 118.7 s, OK (skipped=7), exit 0. The "REGRESSION" and "CI status unreadable" lines in the output are printed by the self-tests of those scripts, not failures.

## 3. CI on origin/main: PENDING

`gh run list --workflow ci --branch main`: run 41152086289, push, head e60a511ab270748c824893c4379db020dcd4ddc1, status in_progress, no conclusion at 00:50:38 UTC (created 00:47:44Z). The runs for 8f262a8 (41151684932) and ae7f982 (41151606156) were also in_progress. Success is required: record stays PENDING with run id 41152086289 until it concludes success.

## 4. D157 Criterion order across the card from git: PASS

History order on origin (git log --reverse 79e0892..origin/main), each with "Criterion: n" in the body:
- Criterion 1: red 451f4ea, unit ab94601, green b374ef7
- Criterion 2: red 55b5794, unit 66904ab, green 1a08bd6
Green 1 (b374ef7) precedes red 2 (55b5794). Done check 3 is folded into criterion 2's cycle by the slice amendment 37e456c (window 219-slice delivery lead condition, adopted 2e126db), declared in 55b5794's body (born-green held-record test, mutation-proved M1 and M5). Matches the independent review (review-ctx5.md, APPROVE). Greens b374ef7 and 1a08bd6 are on origin.

## 5. Product owner waiver (D162): PASS

Cited in the record: .mob/records/219/record-in-progress.json `gate_waiver` (by product owner, word "waive", decision D162, scope this card only, ends when the card closes, no gate code change), `decisions.waiver` = D162, and the Intent line quoting "waive"; waiver record commit c50fb7a is an ancestor of origin/main. Cited in the decision note: docs/decisions/process-and-staffing.md "Decider D162: product owner ruling (not a team vote)" (introduced by e2b1607, on origin), same scope; and .mob/records/decisions/219-taskforce.json (rule and gate fields "starts under the product owner's one-card waiver D162"). The waiver ends when card 219 closes; the facilitator should close the card and re-rank per D162 Placement after ship.

## 6. Domain-expert question at ship (T2): PASS

Should a member with different domain knowledge or context have joined, or be consulted before ship? No. The change is one ordering helper (previous_record, scripts/mob_process.py) plus tests, with no security boundary, runner, hook, workflow or language change. The independent review answered the same (review-ctx5.md T2) and found no Blocker or High; the three Done checks each have a named test and a mutation. No expert was added mid-card, and nothing at ship needs one.

## Carry-forward for the facilitator (not gate failures)

- Landing the held records 209 and 208 needs the MOB.md writer run in the same step, then `check` again (review finding 3, D159 C6).
- Seat note: all seven seats are Claude on probe evidence; D163 (balanced rotation) is not violated while the other models have no quota.
