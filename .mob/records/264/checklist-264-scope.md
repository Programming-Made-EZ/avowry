# D158 pre-window checklist: window 264-scope (facilitator)

- Proposal: .mob/records/264/scope-proposal-architect.md (architect, Codex ctx-2, read-only through cmd.exe; 5536 bytes before its Size line).
- Trigger: green 1 attempt 1 stopped on its own (the class has 17 rows, not 4; a loaded run takes 339 s; 20 runs are about 112 minutes). A D175 cap event: criterion 1 raised from 1 to 2 cycles (caps 1+1+1 become 2+1+1, above the limit of 3) needs a scope window before any further red or green on it.
- Category: scope window (D153 objection window; D175). Seats polled: product manager (ready), QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the architect (proposer) does not sit. D171 cites verbatim and case-sensitive (the instruction is in the prompt); an unread vote is re-asked once; D187 for one condition.
- Facts for the seats: the untracked files in the working tree (scripts/check-repeat-evidence.py, scripts/repeat-run-264.py, a partial repeat-evidence.json whose run 1 is mislabelled FAILED) are not committed and are not evidence; CI runs no Python unit tests (only the process check); the two real-record tests stay red on main until cycle 1b under the proposal.
- Seats run with Grok disabled (backends-nogrok.json) until card 291.
- No commit to avowry while a voter runs.
- Probe: taken before the window (the last was 12:38 UTC), plus decider.py's own probe per run.
