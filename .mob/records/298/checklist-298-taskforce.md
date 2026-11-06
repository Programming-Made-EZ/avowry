# D158 pre-window checklist: window 298-taskforce (facilitator)

- Proposal: .mob/records/298/taskforce-proposal-pm.md (product manager, ready, fresh Claude ctx-1 subagent; it wrote nothing, the facilitator committed its reply and added the last section as framing; one allowed test run, run twice and disclosed; numbers carry commands on mob-crosscli HEAD e40962c).
- Card: 298, ranked first at the head of the board (re-rank 25, the next pull); a bug in mob-defaults (decider.py's vote lock).
- Category: task-force declaration, with the card-text fix its Ready check lists (D153 objection window). Seats polled: architect, QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the proposer does not sit (F-054). D171 cites verbatim and case-sensitive; one condition re-asks only that seat; conditions from 2 or more seats a full re-poll (D187). The Decider protocol is followed as written, a No is a No (product owner ruling 2026-11-06).
- Seats run with Grok disabled (backends-nogrok.json) until card 291.
- Facilitator's notes for the seats, facts only: the green seat is declared as Codex (full access through the launcher, commands not yet covered by the delete scan, card 305) with the Claude fallback; the lock under test is the one decider.py takes, so no window runs while a seat edits mob-crosscli.
- D189: the proposal's line numbers were re-run by the facilitator (all agree except pid_alive at 1101); D190 in force for the gate later.
- Inbox read at this boundary: no open entry (the coach-conformance finding was acknowledged at 20:05 UTC and routed to the next retro).
- Probe: copilot and codex 2026-11-06 (see .mob/records/probes/2026-10-30.jsonl), plus decider.py's own probe per run.
- No commit to avowry while a voter runs.
