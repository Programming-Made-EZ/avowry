# D158 pre-window checklist: window 228-taskforce (facilitator)

- Seats polled: all five non-proposer declared roles, namely the architect (slice, ctx-2), QA engineer (red, ctx-3), backend developer (green, ctx-5), independent reviewer (audit lens, ship, ctx-4) and decider/sign-off (ctx-6). The proposer is the product manager (ready) ctx-1, a Copilot seat (usage: 0.25 Premium, 225.2k in, 188.2k cached, 7.1k out). F-054 applies.
- D171 is in force. Every reply ends with "CITE: <point>": at least 6 words copied exactly from the proposal, or "line N: <first 4 words>". An unread vote is re-asked once, then the seat is replaced in least-recently-used order.
- D174 is in force, its second window. decider.py's append-only check (verify_history) makes parallel runs on one checkout refuse each other, so this window runs serially and is recorded as a FALLBACK, not a pass. Window minutes are recorded.
- No commit to avowry while a voter runs; seats are bound too.
- Proposal: scratch 228-taskforce-proposal.md (6151 bytes, LF), carried in the prompt. The trail is written once all replies are in.
- Probes: 20:00 UTC (19:59 by the probe's own clock), plus decider.py's own probe per run. Codex is left out of the facilitator's probes after its 13:53 probe hung.
