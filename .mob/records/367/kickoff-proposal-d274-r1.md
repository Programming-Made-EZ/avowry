# D274: adopt card 367 with its task force, and rank the five unranked Ready cards last

Proposer: the facilitator, 2026-11-16 about 17:00Z, at the next kickoff after mob-defaults #61, #58, #63 and #62 closed.

## Part A: card 367 and its task force

Card 367 (the four model vendors' published terms are read for what they say about publishing model output) is the first eligible Ready card by Rank (rank 115, no dependencies). The kickoff probe at 16:53:45Z wrote one probe row per service: agy, claude, cursor and muse ok; codex and copilot at their limit; grok 402; qwen not launched because play-tracker's heavy run holds the shared lock. kickoff_check.py passed on the declaration in .mob/records/367/declaration.json.

Task force:
- red: muse, default model, effort medium. One acceptance test over the new docs page.
- green (research and page): claude sonnet. Reads each vendor's current published terms by web fetch only and writes docs/release/vendor-terms.md.
- QA review: claude opus. Re-fetches every cited URL and checks each quote and blocker call.
- cross-family review: agy gemini-3.8-flash-medium, run before the QA review is written.
- sign-off: cursor composer-2.5, read-only.
No service holds more than two seats.

## Part B: the five unranked Ready cards

The loop picker refuses to pick while a Ready card has no Rank. Cards 352, 353, 357, 372 and 373 are Ready with no Rank. They get Ranks 298 to 302 in that order, after the board's highest Rank (297). No ranked card moves, so the queue in front of them is unchanged. A later Decider or the product owner can move them up.

## Vote

Yes adopts both parts. A No names which part and what would change it.
