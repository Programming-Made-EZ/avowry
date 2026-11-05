# D158 pre-window checklist: window 217-taskforce (facilitator)

- Proposal: .mob/records/217/taskforce-proposal-pm.md (product manager, ready, fresh Claude ctx-1 subagent; it wrote nothing, the facilitator committed its reply and added the last section as framing; counts run in code on HEAD 082d2f5: 7 blocking sites in 7 files, 278 test cases, no full run).
- Card: 217, ranked first at the head of the board (re-rank 23); corrective for open FAILs F-063 and F-065.
- Category: task-force declaration, with the card-text fix its Ready check lists (D153 objection window). Seats polled: architect, QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the proposer does not sit (F-054). D171 cites verbatim and case-sensitive (the instruction is in the prompt); an unread vote is re-asked once; D187 for one condition.
- Seats run with Grok disabled (backends-nogrok.json) until card 291.
- Facilitator's notes for the seats, facts only: Codex cannot run shell commands on this machine (card 297), so Claude fills red and green; card 264's findings ruled thread-pool starvation out for the 2026-11-01 failure and left the 128-thread floor as a mitigation; card 303 owns the two CSharpBaseline endpoint tests locally, card 162 will refresh the Bar6 evidence; the proposal lists Copilot as ready seat, while the actual ready seat was a Claude subagent (as on cards 297 and 264).
- D189 is in force for the slice (numbers carry their commands); this proposal's numbers carry theirs.
- Probe: taken before the window (the last was 20:21 UTC), plus decider.py's own probe per run.
- No commit to avowry while a voter runs.
