# D158 pre-window checklist: loop-boundary re-rank 23 (facilitator)

- Proposal: .mob/records/reranks/2026-11-05-23.md (product manager seat, fresh Claude context, read-only reply; 3 tool uses, no delete command).
- Rule: D153 objection window (loop-boundary re-rank), D148, D171 (verbatim, case-sensitive cites; one re-ask for an unread reply), D187 (one condition: re-ask only that seat; several or conflicting: full re-poll).
- Window seats: architect, QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the proposer (product manager) does not vote. Reply: NO OBJECTION or OBJECTION with one CONDITION, then a CITE line. Grok stays disabled (backends-nogrok.json).
- The placement is two commands, `rerank.py --after 217 --cards 303` then `rerank.py --after 266 --cards 302`; bugs above the first product card (196) are 217, 303, 298, 299 (4, the head-size guard of re-rank 21 as written).
- Facts for the seats: card 217's text names the two C# baseline endpoint tests (F-065) in its third done check; card 264's findings ruled thread-pool starvation out for the 2026-11-01 failure and left the 128-thread floor as a mitigation; the Codex machine fix (cards 300 and 301 depend on it) still waits on the product owner.
- Probe: 2026-11-05 20:21 UTC (copilot and codex available), plus decider.py's own probe per run.
- No commit to avowry while a window seat runs.
