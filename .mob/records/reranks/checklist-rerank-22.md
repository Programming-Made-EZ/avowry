# D158 pre-window checklist: loop-boundary re-rank 22 (facilitator)

- Proposal: .mob/records/reranks/2026-11-05-22.md (product manager seat, fresh Claude context, read-only reply; 5 tool uses, no delete command).
- Rule: D153 objection window (loop-boundary re-rank), D148, D171 (a cite or the format line; one re-ask for an unread reply), D187 (one condition: re-ask only that seat; several or conflicting: full re-poll).
- Window seats: architect, QA engineer, backend developer, independent reviewer (audit lens), decider, agile coach; the proposer (product manager) does not vote. Reply: NO OBJECTION or OBJECTION with one CONDITION. Grok stays disabled (backends-nogrok.json).
- Open question put to the window: the head-size guard (re-rank 21: at most 4 bugs above the first product card). The proposal puts 6 above the rename's first card and gives a fallback (300 and 301 after 266).
- Dry run: rerank.py --after 217 --cards 298,299,300,301 placed 298 to 301 at ranks 94 to 97 and shifted 196 and later down four places; nothing was applied.
- Probe: taken 2026-11-05 09:49 UTC (copilot and codex available); inflight.sh now refuses a seat start on a probe older than 50 minutes (D188).
- No commit to avowry while a window seat runs.
