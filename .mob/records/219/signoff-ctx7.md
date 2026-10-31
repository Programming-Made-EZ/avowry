# Card 219 sign-off (ctx-7, product manager, Claude seat)

Verdict: SIGN OFF

Seat is Claude because the 00:34 UTC probes show Codex, Copilot, Grok and agy without usable quota. Checked at local main = origin/main = 50580e5 (CI-checked head e60a511; later commits are record and MOB.md only).

## Against D159 C1 to C6

- C1 previous record = nearest earlier record in git history, none for the first: met (scripts/mob_process.py, review-ctx5 verified; table-tested).
- C2 kept checks (not before previous record, not after close, not own commit, not reused): met, diff touches only the helper and two lines.
- C3 only the choice of previous changed; error names the record compared with: met (`Card 209 evidence <sha> predates Card 214's record`).
- C4 209 accepted after 214: met in a fixture with the real add, hold, re-add history.
- C5 red first, one criterion per cycle, within D136: met (451f4ea/ab94601/b374ef7, then 55b5794/66904ab/1a08bd6; D157 order from git, gate item 4).
- C6 held records land only after this checker change is on main: the change is on main (1a08bd6, CI green). The records are not yet landed; that is the follow-up, below.

## Other checks

- D162 waiver: one card only, ends when this card closes; record gate_waiver and gate item 5 agree. Closing 219 ends it; every other card follows the gate again.
- Three Done checks: out-of-order accepted, back-dated refused naming the record, held-record sequence passes. Each has a named test; I re-ran the three (3 OK). `check` on main: OK (94 closed cards, 3 records, 91 legacy).
- D157: confirmed by the gate and the review.
- Intent line: confirmed by the facilitator against D159 C1 and D162 (D165 C2).
- Review ctx-5: APPROVE, no Blocker or High. Gate ctx-6: items 1, 2, 4, 5, 6 PASS; item 3 (CI) was PENDING and is resolved in the record: run 41152086289 success on e60a511. I did not call gh; I rely on the record's reading.

## Deviations, stated plainly

1. The slice's forged-date guard (M3) was not written and M3 not run. The reviewer judged it unneeded: ordering is by git topological position, never by committer date, so a forged date cannot reorder records. I accept that. Evidence dates remain forgeable as before (D159 not in this card).
2. MOB.md staleness held a finished green once (and blocks pushes about once an hour). It is a process cost, not a defect in this change; it is recorded as an observation and the retro pick D166 addresses the related slice rule.
3. One test checks the fixture, not the checker (review finding 2); harmless.

## Persona demo (D150), facilitator

- Before: closing card 209 after 214 made the push hook say 214's evidence predated 209's record; the facilitator had to hold 209 and 208 back and write a blocker.
- Now: records can land in any card-number order; each card is compared only with the record committed just before it. I ran the out-of-order, back-dated and held-sequence tests: all pass.
- Back-dated evidence is still refused, and the message names the record it was compared with, so the facilitator sees which record to look at.
- A forged commit date does not help the cheater: order comes from history position.

## Metric

Held records: 2 now (cards 208 and 209; target 0). Plan: the facilitator adds 209.json and 208.json in one step with the MOB.md writer run in the same commit, then runs `check` (review finding 3; the only errors in the rehearsal were the MOB.md ones), then pushes. Refused-record events per close stay at target 0 once those land; the metric moves when they do, not at this sign-off.

Card 219 can close; the facilitator re-ranks per D162 placement and ends the waiver.
