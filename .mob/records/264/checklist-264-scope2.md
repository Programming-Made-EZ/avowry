# D158 pre-window checklist: window 264-scope-2 (facilitator)

- Proposal: .mob/records/264/scope2-proposal-architect.md (architect, Codex ctx-2, read-only through cmd.exe; 5235 bytes before its Size line).
- Trigger: the root cause named by green 2 (concurrent dotnet run builds of Phrase.Bench collide on the shared obj output) is demonstrated locally and not fixed; Done check 2 and the adopted closure condition require the corrective change. Cycle 2b is added: caps become 2 (criterion 1) + 2 (criterion 2) + 1 (criterion 3) = 5, above the D175 limit of 3. The window is asked to approve the extra cycle (the criterion-1 split was approved by window 264-scope).
- Category: mid-card addition (D153 objection window; D175). Seats polled: product manager (ready), QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the architect (proposer) does not sit. D171 cites verbatim, case-sensitive; an unread vote is re-asked once; D187 for one condition.
- Facts for the seats: green 2's findings (.mob/records/264/findings.md) and its diagnostic helper; CI builds Release but the children run Debug by default (so each child builds Debug Phrase.Bench itself); five Bench start sites in four acceptance classes; the 17-row class passes on HEAD; the untracked partial record .mob/records/264/repeat-evidence.json is not evidence.
- Seats run with Grok disabled (backends-nogrok.json) until card 291.
- No commit to avowry while a voter runs.
- Probe: 2026-11-05 14:44 UTC (Copilot and Codex available), plus decider.py's own probe per run.
