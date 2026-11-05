# D158 pre-window checklist: window 264-slice (facilitator)

- Proposal: .mob/records/264/turn-slice-architect.md (architect, Codex ctx-2, read-only through cmd.exe, commands run on HEAD 22edff6; 14567 bytes before its Size line, under the 15360 limit).
- Card: 264 (text as fixed by window 264-taskforce). Declaration and two conditions adopted there (findings file with exact commands; closure only after the merged fix passes the reproduction).
- Category: slice decision (D153 objection window). Seats polled: product manager (ready), QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the architect (proposer) does not sit. D171 cites verbatim, case-sensitive (the instruction is in the prompt); an unread vote is re-asked once; D187 for one condition.
- Premises (D184): seven, each with a command and output; premise 7 is marked unrun (a CI runner is needed). The facilitator re-ran none; the QA seat's red will re-run them.
- Caps (D175): 1+1+1 (criterion 1, 2 and 3 one cycle each). Criterion 1's red is a repeat/load evidence check (the behavioural test may already pass); criterion 2's red needs a forced reproduction (an injected child that exits early); criterion 3's red is a record check against ci-evidence.md.
- Facilitator's notes for the seats, facts only: the 11:27 run's failing job was mob-process (run 41341801479); the 11:34 failure is a missing history.jsonl at ReadHistory line 359 (run 41342389875). Several other ci runs failed on 2026-11-02 to 2026-11-04 (not inspected for this test; listed in ci-evidence.md).
- Seats run with Grok disabled (backends-nogrok.json) until card 291.
- No commit to avowry while a voter runs.
- Probe: 2026-11-05 12:38 UTC (Copilot and Codex available), plus decider.py's own probe per run.
