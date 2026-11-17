# D281: card 201 waits for the product owner's answer; card 171 is adopted with its task force

Proposer: the facilitator, 2026-11-17 about 01:55Z, after card 169 closed. The loop picker's next card is 201 (rank 117).

## Part A: card 201 waits
Card 201 converts "the sealed holdout tasks, which live outside the repository" and requires that "none is opened by an author or reviewer seat". Record R1 (docs/decisions/benchmarks-and-measurement.md, 2026-10-27): Dispatch unsealed H1-write, H2-modify and H3-write for card 102 and the mob ran them; local copies are in bench/holdouts-unsealed/ (gitignored). The card was written on 2026-10-29 as if they were still sealed. Whether to convert the already-unsealed holdouts or seal new ones is the product owner's decision; the facilitator asks it as one word. Card 201 takes Rank 121.2 (after card 165) until the answer; no other card moves.

## Part B: card 171 and its task force
Card 171 (the CLI reads method and type names from the adapter; Program.cs is tidy) depends on card 161 (done). Probe at 01:51:57Z: agy, claude, cursor and grok ok; codex, copilot and muse at their limits; qwen gate busy (play-tracker's heavy run). kickoff_check.py passed on .mob/records/171/declaration.json (committed before this vote).
- red: claude sonnet (claude-sonnet-5-5); a true red where the CLI hardcodes names today; style criterion 2 by a source scan with planted rows; criterion 3 by the recorded orders hash.
- green: cursor, pinned composer-2.5.
- QA review: claude opus (claude-opus-5-5).
- cross-family review: grok grok-4.6, normal mode in a throwaway clone, before the QA review is written.
- sign-off: agy gemini-3.8-flash-medium, read-only.
No service holds more than two seats. dotnet runs are the facilitator's, under Avowry's heavy lock.

## Vote

Yes adopts both parts. A No names which part and what would change it.

After the vote and any sentence, end with one line: `CITE: ` followed by a phrase of six or more words copied exactly from this proposal.

## Applied (facilitator, 02:05Z)
The loop picker reads Ranks as whole numbers, so 121.2 collided with card 165's 121 ("Duplicate Ready rank 121"). Card 201 took Rank 306, the first free whole Rank, which still puts it after card 165 and moves no other card. It returns to Rank 117 on the product owner's answer.
