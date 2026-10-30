# Card 219 slice: architect (ctx-2, Claude seat)

Sources read: board/219-*.md, D159 (docs/decisions/process-and-staffing.md#d159), D162, scripts/mob_process.py check_records (lines 126 to 226), tests/test_mob_process.py fixtures, the held records 208.json.held and 209.json.held (read only, not committed), git history of .mob/records/*.json. Seat started 2026-10-30 23:45:31 UTC (in-flight line f4627fc; last probe age 42 min; cap 00:10:31 UTC).

## The change in one place

scripts/mob_process.py check_records, today:

    previous = max((n for n in record_dates if n < card), default=None)

D159 C1 replaces "next-lower card number" with "the record added in git history immediately before this card's own record". Everything else (distinct evidence, own-commit, after-closure, reuse, Card NNN in the message, the string "evidence X predates Card NNN's record") stays. The error text already names the card of the record compared with; it keeps that exact text, so D159 C3 is met by a test asserting the named card is the commit-order one, not the number-lower one.

Placement (D136, one concern): one pure helper beside check_records, `previous_record(card, record_commits, commit_order)`, returning the card number of the record added strictly before `card`'s record and nearest to it, or None; plus one `git rev-list --topo-order --reverse HEAD` call (through the injected `git`) to build `commit_order` (commit hash to index). No other function changes. Ordering uses history position, never committer dates, so a forged date cannot reorder records.

Rules the helper must hold (each named by a test below):
- R1 no earlier record gives None (the first record in history is checked against nothing, as today).
- R2 two records added in the same commit do not constrain each other (neither is "before"); each takes the nearest record from a strictly earlier commit.
- R3 a record's position is its FIRST-add commit (record_commits keeps `splitlines()[-1]`, the oldest add), unchanged. This matters for the held records, see criterion 3.

## Numbered Done checks (D157)

1. Out-of-order close accepted: a higher-numbered card's record committed first (214 then 209; 209's evidence after 214's record, 214's evidence before 209's record) passes check.
2. Back-dated evidence still refused: evidence dated before the record committed immediately before this card's record fails with "Card 209 evidence <value> predates Card 214's record" (names the commit-order record, not a number-lower one).
3. Main passes with the held records added: a fixture tree reproducing the real record sequence (205, 214, then 209 and 208 with their add, held-back removal, a 219 record, and re-add history) exits clean.

One red-green cycle each. "Criterion: n" in each red commit body. Green n before red n+1. Greens pushed at once by the facilitator.

## Criterion-by-layer table (outermost first; F-055 doubles named)

| Crit | Layer | Test (tests/test_mob_process.py) | Doubles it consumes | Red-able or born-green |
|---|---|---|---|---|
| 1 | Acceptance: `mob.check_records(root, issues, git)` on a real temporary git repo | `test_record_added_out_of_number_order_is_accepted` | Real git repo from the MobProcessTests setUp; `issue()` stub for the board (no gh); `FROZEN_LEGACY_SHA256` patched (setUp); commit dates fixed by a helper that sets GIT_COMMITTER_DATE through `mock.patch.dict(os.environ)` (otherwise every commit lands in the same second and the strict `<` never fires, so the red would be false) | Red-able now. The old rule checks 214's evidence against max(n<214)=209, whose record was committed later, and reports "predates Card 209's record". Fails by assertion. |
| 1 | Unit: `previous_record` | table of (records added in order, expected previous) including R1 | none (pure; dict and list inputs) | Red-able. The helper is absent, so the QA seat first adds a stub returning the number-lower card (card 208's practice) so the red is an assertion failure, not an import error. |
| 2 | Acceptance, same layer and doubles as 1 | `test_backdated_evidence_is_refused_naming_the_previous_record_in_commit_order`: records 205, 214, 209 added in that order; 209's evidence dated after 205's record and before 214's | as above | Red-able after criterion 1's green, provided criterion 1's green is staged as "candidates = records added earlier, still choosing the largest card number" (205 is chosen, no error raised, test red). The green for 2 then picks the nearest earlier by position. If the backend developer greens criterion 1 straight to the final rule, criterion 2's acceptance test is born-green and must be declared so, with mutation M1b run before its commit. |
| 2 | Acceptance guard: forged record date | `test_a_forged_record_commit_date_cannot_reorder_records`: 209's record commit carries a committer date earlier than 214's but is committed after it; 209's evidence is dated between the two | as above | Born-green guard (needs the final ordering). Kills M3. |
| 3 | Acceptance: full sequence | `test_held_record_sequence_passes` (two variants: re-added in separate commits, and in one commit) | as above; no gh; no real repo files; the held JSON content is not used | Born-green by design (the greens of 1 and 2 satisfy it). It is the integration proof, so it carries the guards and mutations below instead of a red commit (D137 shown by mutation, as card 208's review fix did). |
| 3 | Unit guard: same-commit tie (R2) | `previous_record` table row, plus an acceptance row: 208 and 209 added in one commit, 208's evidence after 214's record and before 209's evidence | none / as above | Born-green. Kills M4. |

Process/CLI layer (`main`, hook, CI): not consumed. The change is inside check_records and run_check calls it unchanged; no new entry point, no gh double. Declared so a reviewer does not look for one.

## Named mutations (card 214's rule: each born-green guard cites one; run and recorded in the commit body)

- M1 revert to `max(n for n < card)`: the criterion 1 and criterion 3 tests must fail.
- M1b candidates filtered to earlier-added records but the largest card number chosen: the criterion 2 chain test must fail (the staging point).
- M2 take the first record in history as "previous" instead of the nearest: the criterion 2 chain (205, 214, 209) must fail, because 209's evidence postdates 205 yet predates 214.
- M3 order records by committer date instead of history position: the forged-date guard must fail.
- M4 same-commit records ordered by card number so one constrains the other: the tie guard must fail.
- M5 position from the latest add (`splitlines()[0]`) instead of the first add: the criterion 3 tests must fail. This is the one that matters for real data (below).
- M6 drop the previous-record comparison entirely: the criterion 2 tests must fail.

## Criterion 3 without committing the held records

Two parts, kept apart so nothing real is added in the card.

A. In the test suite (committed): a fixture repository, built by test helpers, that reproduces the real commit order with synthetic records and the fixture's own commits:
1. base; evidence commits for 205 and 214 (dated); add 205.json; add 214.json (record date after 214's evidence).
2. evidence commits for 209 and 208 (dated after 214's record, as in the real data: 209's earliest evidence is 05:45 PDT, after 214's record at 05:35; 208's earliest is 10:13, after 209's record at 09:30).
3. add 209.json, then remove it ("held back"); add 208.json, then remove it. The real history has exactly these four commits: 5a3d4af, 2bc05f6, 1c850b0, d02114d.
4. add 219.json for a stand-in card whose evidence postdates all of the above (the card's own record lands before the held ones, D159 C6).
5. re-add 209.json and 208.json (variants: two commits, and one commit).
Expected: `check_records` yields no "predates" error for any card, and the reuse, own-commit and closure checks stay silent. The held JSON is never read by a test; only the shape (the six required roles, via `valid_record`) is reused.

Why steps 3 and 4 are in the fixture: the checker takes a record's position from its first add (`--follow --diff-filter=A`, `splitlines()[-1]`). The real 209 and 208 were first added at 09:30 and 16:27 PDT and then removed, so their position is before the new 219 record. If position were the latest add (M5), 209's previous would be 219 and 209's evidence (05:45 to 09:24 PDT) would predate 219's record: the held records would be refused again. That is the real risk to D159 C4 and C6, and it only shows with the add-remove-re-add history, so a plain "214 then 209" fixture is not enough.

B. Ship-gate rehearsal, not committed (delivery lead, ctx-6): in a throwaway `git clone --local` of the card's tip under the scratchpad (a new empty directory; remove it only by its literal path), copy 208.json.held and 209.json.held to .mob/records/208.json and 209.json, then call the changed `mob.check_records(root, issues, git)` from python with `issues` stubbed (no gh call by the seat) and report any "predates" errors; expected none. The result is quoted in the pre-ship gate note, not committed as a test. Read-only check made now on today's history: by first-add order the chain is 214 (05:35), 209 (09:30), 208 (16:27); 209's evidence (earliest 05:45) postdates 214's record and 208's (earliest 10:13) postdates 209's, so the held records pass the new rule on current data. They also pass after 219.json lands, because 219's evidence will be dated after 16:27 PDT (every card 219 commit so far is 16:39 PDT or later). The held records land after card 219 closes (D159 C6): the facilitator adds 209 then 208 in a separate commit that is not a card 219 commit, and re-runs the real check then. The card never commits them.

Residual, unchanged by this card, for the reviewer's loophole lens: first-add position lets a record be committed early, removed and re-added to claim an earlier slot. The checker already behaves this way for dates and the own-commit check; the change does not widen it (an earlier slot only makes the previous record older). Not in scope ("Not in this card": any other evidence check).

## Commit plan (D136: countable limits 15 files and 400 changed lines per commit; three criteria per card)

Files touched: scripts/mob_process.py and tests/test_mob_process.py only (2 countable files). Estimate: about 25 source lines and 140 test lines in total.

0. f4627fc in-flight line (done); this slice turn (docs only, not countable).
1. Red 1: tests only, subject "Card 219 red: out-of-order close is accepted (D159 C1)", body "Criterion: 1", failing run quoted (fails by assertion).
2. Green 1: scripts/mob_process.py, staged as M1b (candidates earlier-added, largest number) with `previous_record` and the rev-list call; "Criterion: 1".
3. Red 2: chain test plus the forged-date guard; "Criterion: 2" (the chain test is red; the guard is born-green, declared with M3 in the body).
4. Green 2: nearest earlier by position; "Criterion: 2".
5. Criterion 3: one test-only commit "Card 219 criterion 3: held-record sequence passes (D159 C4, C6)", born-green, "Criterion: 3", body lists the M1, M4 and M5 proofs (mutation applied, failures counted, restored, run counts). It follows green 2, so green-before-next-red holds.
6. Optional refactor only if leftovers remain. Review-fix commits stay separate (D136).
Subjects start with "Card 219", cite D159 (D145 C2: the checker change cites D159, recorded in an ancestor commit) and D162 where the waiver is relevant, no trailers, no rewrites. The seats do not push; the facilitator pushes greens at once.

Run per commit: `python -m unittest tests.test_mob_process` (this file only; fast, no gh). The full repository suite is the delivery lead's call at ship.

## Slice verdict

SHIP AS IS. One concern (how "previous" is chosen), two files, three criteria, estimated well under 400 lines per commit, no feature flag needed (checker tooling, not user-visible language behaviour). No split. Criterion 1's red is unconditional; criterion 2 is red-able under the staging above; criterion 3 is declared born-green with named mutations (M1, M4, M5) as its guard.

## Domain-expert question at the slice boundary (T2)

Should a member with different domain knowledge or context join now? No. The change is one function in scripts/mob_process.py plus fixture repositories in tests/test_mob_process.py. The risks found at slice (order by history not date, same-commit ties, first-add position for the held records, the 219 record landing before the held ones) are git-history semantics that the architect, QA engineer, backend developer and independent reviewer (audit lens) already hold; the one fact that could have needed outside knowledge (the position of a record added, removed and re-added) was verified directly against the repository history above. If the QA seat finds fixture commit dating cannot be made deterministic on Windows git, any member adds an expert through a Decider (mid-card addition, with the Decider id).
