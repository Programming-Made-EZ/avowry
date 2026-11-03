# D158 pre-window checklist: window 229-slice (facilitator)

- Proposal: .mob/records/229/turn-slice-architect.md (architect ctx-2, fresh Claude subagent context; read-only; transcript scan clean, 31 commands; size measured 7909 bytes).
- Category: slice decision (D153 objection window). Seats polled: product manager (ready), QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the proposer does not sit (F-054). D171 cites, verbatim (markdown markers count); an unread vote is re-asked once, then replaced. Serial launch.
- No commit to avowry while a voter runs.
- Checks against the adopted task force (.mob/records/decisions/229-taskforce.json): caps 1+1+1; Done check 1 carries both the committed over-limit test and the live Copilot and Grok vote with committed evidence; D182's QA condition (red 2 before green 1, the empty guard).
- Facts the slice verified by reading only: grok 1.0.46 lists --prompt-file; copilot 1.0.105 has no prompt-file option and its .cmd shim runs node.exe with npm-loader.js. Assumptions left for red 1: grok --prompt-file is single-turn without the TUI; a 9.4K multi-line argv survives node's Windows quoting byte-equal.
- Probe: 2026-11-03 22:02 UTC (Copilot and Grok available), plus decider.py's own probe per run.
