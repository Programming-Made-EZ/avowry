# In flight (one line per seat; updated before every wait over about 20 minutes)

- 2026-10-30 10:47 UTC: backend developer ctx-3, card 214, commit 5 (a22b651, local, held) second full acceptance run and a fix for three card-test timeouts under parallel load; cap 10:55 UTC.
- 2026-10-30 11:06 UTC: backend developer ctx-3, card 214, commit 5b (thread-pool floor; run 4 with it: no card-214 failures) and the base comparison of four unrelated acceptance failures; a22b651 still held local; cap 11:30 UTC.
- 2026-10-30 11:09 UTC: independent reviewer ctx-4 (attempt 2, probes 11:07), card 214 review of ec25197..afa4e40; cap 11:45 UTC.
- 2026-10-30 11:11 UTC update (F-062): the 10:55 cap passed with no update; the facilitator's miss. What happened: the developer seat's run 2 showed 6 card-test timeouts; it found thread-pool starvation and made 5b (thread-pool floor); runs 3 and 4 with 5b had no card-214 failures. a22b651 is no longer local: it was rebased and pushed at about 11:06 as 111960d, with 5b as afa4e40 (mapping in .mob/records/214/record-in-progress.json). Main is treated as suspect until the cause question (does 214 cause the timeouts or expose an existing slow path?) is answered with evidence: card 214 does not ship until then. In flight now: reviewer ctx-4 (review plus the cause question), cap 11:45 UTC; CI on afa4e40 (run 41045702766), read once per check at least 120 s apart.
- 2026-10-30 11:42 UTC: reviewer ctx-4 committed a provisional approve (a7e9401); the F-062 cause runs (CAUSES / EXPOSES) continue; cap extended to 12:15 UTC. Card 214 does not ship before that verdict.
- 2026-10-30 12:07 UTC: delivery lead ctx-7, card 214 pre-ship gate (D154: closure-record dry run, every cited hash and path resolves on origin, full suite on the pushed hash 9046262, CI green on the shipping commit); cap 12:45 UTC.
- 2026-10-30 12:3x UTC: card 214 closed (Done); no seat in flight.
- 2026-10-30 13:17 UTC: QA engineer ctx-2, card 209 criterion 1 red (D157) in mob-defaults, working (tests/test_card_mode.py modified, not committed); cap 13:35 UTC.
- 2026-10-30 13:26 UTC: QA ctx-2 done (card 209 criterion 1 red 6836d76, local in mob-defaults, pushed with its green). Now: backend developer ctx-3, card 209 criterion 1 unit layers and green (full suite about 18 min); cap 13:55 UTC.
- 2026-10-30 13:56 UTC: cap 13:55 passed for developer ctx-3 (card 209 criterion 1 green: units 5a2c930, b527e1d, 8e387e3 committed locally; green in the working tree, full suite running); asked for status; new cap 14:15 UTC.
- 2026-10-30 14:16 UTC: cap 14:15 passed; developer ctx-3 reported the full mob-defaults suite (started 13:54, about 17 min) still running with targeted runs green; new cap 14:30 UTC.
- 2026-10-30 14:31 UTC: QA engineer ctx-2, card 209 criterion 2 red (D157), inner loop tests.test_mob_process; cap 14:55 UTC.
- 2026-10-30 14:44 UTC: backend developer ctx-3, card 209 criterion 2 unit layers and green (units 145947b, 147a90a so far); full suite expected to start about 14:50; cap 15:15 UTC (stand-down 1 adjustment: suite start + 25 min).
- Fri Oct 30 15:02:06 UTC 2026: backend developer ctx-3, card 209 criterion 3 full suite started; cap 15:27 UTC.
- 2026-10-30 15:21 UTC: card 209 criteria 1 to 3 green on mob-defaults main (1bb0a83); next: reviewer ctx-4.
- 2026-10-30 15:22 UTC: independent reviewer ctx-4, card 209 review; cap 15:50 UTC.
