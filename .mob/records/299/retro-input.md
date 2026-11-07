# Card 299 retro input (facilitator; facts only)

Card: a second writer on one checkout, in any process, is refused with the holder's name before it writes (bug that put the team's ability to ship at risk; consumer: the product owner). Signed off with conditions (.mob/records/299/signoff-ctx6.md). Code in mob-crosscli: `Writers` takes `mob-writer.lock` (atomic `os.link`, stale takeover decided by the single successful unlink of the renamed stale file). The git-hooks half was split into bug card 323 (Decider D198).

## Flow metrics (UTC; seat start files and commit times)
- Task-force decision (card 299's ready proposal, Decider D198): the vote ran 08:22 to 08:32 (round 1) and 08:34 to 08:43 (independent re-run): about 21 minutes of seat time for one decision, no outlier, no amendment, no cite re-ask; plus the product manager seat's proposal before it.
- Slice (architect, with three probes) about 09 minutes; red 1 to the last tests commit: 08:56 to 09:08, about 12 minutes (red 1 08:56, green 1 09:00, red 2 09:02, green 2 09:05, cycle 3 tests 09:08). Decision against build: about 1.7 to 1 (card 313 was about 7 to 1).
- Gate about 25 minutes (reviewer SHIP with record-only items, then the facilitator's probes), sign-off about 10 minutes.
- Re-ask causes: none this card. Cite format trouble did not occur (round 1 and re-run all verbatim).

## Rulings applied during the card
- Process improvements are not issues; product first; the Decider aligned with the original protocol; retro improvement and review (every retro: four areas, and a review of the permanent agreement with "kept, no change" visible); an Experimental section is proposed for this retro's vote (docs/retros/experiments.md). Re-rank 27 (D197) closed twelve process issues and put this bug first.

## D193 trial, second of three cards (G1 suite table, G2 mutation pair, G3 steps and gaps)
- G1: the reviewer's counts were reruns or quoted from records; the facilitator reran all four narrow suites (22, 53, 7, 43 OK). G2: four git-apply-clean patches (card 313's header line problem fixed); the reviewer could not reproduce them in its shell; the facilitator and the sign-off did. G3: 17 of 31 reviewer items record-only, two G3 lines marked HOLD (the restore gap, release's read-unlink gap); the facilitator's probes closed them (the restore gap is real and accepted: two live holders for milliseconds with three claimers and a stale lock).
- Sign-off-only findings: no defect that the gate missed; three record corrections (a private `_writer_vote_live` copy, `checkout_root` rewritten, 44 LF lines in a CRLF file) and the restore gap not yet stated on the card (fixed before the read). Tag: G3 (the gap).

## Process notes (this card)
- The facilitator's first green launch ran from the wrong directory (no `cd` into the worktree); stopped after seconds, checked clean, relaunched.
- Two commits failed silently inside background commands (the personal-name check on a Codex stream line and on the proposal note); each was noticed afterwards and fixed; the rule now: check `git log` after every commit before a vote or a launch.
- The reviewer's gate and the facilitator's probes again disagreed on what "record-only" means for a suite run reported by a record; the facilitator's rerun settled it.
- The Codex green seats stayed in their lane in the worktree (19 and 14 commands, no delete, no push, no main checkout path); the line-endings rule held better than on card 313 (diffs 120/11 and 46/5), though 44 LF lines landed among CRLF lines.

## For the permanent-agreement review and the Experimental section
- The inventory of every experiment line in TEAM_WORKING_AGREEMENT.md (hypothesis, metric, trial length and unit, start date, cards closed since, status, retro verdicts, recommendation) is .mob/records/299/experiments-inventory.md (24 items: 8 in trial or unknown, 14 expired by count, none ever decided by a Decider).
- The agreement's permanent sections to review this retro: Values, Principles, the Practices subsections, the standing rules (Decider protocol, Escalation, Continuous loop, LLMs are the customers, Milestones, Metrics are evidence, Task force before every card, CI on main, Driver availability, Staffing, Decisions stay reversible, Driver rotation and trust ladder, Safety). For each: update, still earns its place, or remove; "kept, no change" is recorded.
