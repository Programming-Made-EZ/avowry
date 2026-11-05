# D158 pre-window checklist: window 297-slice (facilitator)

- Proposal: .mob/records/297/turn-slice-architect.md (architect, slice, Codex gpt-5.6-sol ctx-2, read-only sandbox, commands through cmd.exe; it wrote nothing, its reply is the file; 6092 bytes).
- Card: 297. Category: slice decision (D153 objection window). Seats polled: product manager (ready), QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the proposer does not sit (F-054). D171 cites verbatim (at least 6 words); one targeted re-ask for an unread vote (D187).
- Seats run with Grok disabled (backends-nogrok.json).
- Facilitator's notes, facts only: (1) premise 2 shows the slice seat could not reach GitHub from its sandbox, so it read the card text from the task-force proposal; (2) premise 7 says core/backends.json builds a command-running Codex invocation with --dangerously-bypass-approvals-and-sandbox, while the card's failure is under ~/.codex/config.toml windows sandbox "elevated"; the red seat should check on HEAD which invocation a Codex build seat actually gets; (3) the architect's replay of D184 premises ran on mob-crosscli HEAD 1732a82.
- In-flight line for the window is written first. No commit to avowry while a voter runs.
- Probe: 2026-11-05 04:10 UTC (Copilot and Codex available), plus decider.py's own probe per run.
