# Board refinement 2026-11-04: legacy characterization (cards 295 and 296)

- Input: a product idea relayed by Dispatch for refinement: characterization specs for existing code stabilise legacy systems and, since avowries generate code, carry their behaviour to other targets. Recorded on its merits; no attribution in the cards.
- Seats (each fresh, read-only):
  - product manager draft: Codex gpt-5.6-sol, 18:17 to 18:18 UTC (one card; Morgan; after card 030; flag on);
  - domain expert (characterization testing): Claude subagent (split capture from portability; add seams, scrubbers, overfitting review, known-quirk tags, a coverage measure; Morgan needs widening for non-Avowry code, Dana owns portability);
  - architect: Copilot, session 4af2f746 (split; card 028 is a spike, not a supported adapter; flag on the CLI synthesize path; rank after 030 for the capture slice only). Scan clean.
- Result: card 295 (capture, Morgan) and card 296 (portability, Dana), both Draft, flag default-off. The facilitator folded every review fix into the drafts.
- Placement on the product owner's direction relayed by Dispatch ("rank it on the board by value"; rerank.py allows a placement on that direction): 295 and 296 right after card 030 (ranks 197 and 198; 57 cards below moved down two). Card 296 sits after 295 rather than after card 028, because 028 ranks above 030 and 296 depends on 295.
- Persona, decided: Morgan is widened to inherited C# code (PERSONAS.md), matching the one supported adapter; other source languages come into scope as their adapters exist. Card 295 keeps Morgan and states that boundary in its story, out-of-scope line and context (issue body edited the same day).
