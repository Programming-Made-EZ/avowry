# D278: cards 162 and 201 wait behind card 165 until they can run; the loop takes card 169 next

Proposer: the facilitator, 2026-11-16 about 20:15Z, after card 367 closed.

The loop picker's next eligible Ready cards by Rank are 162 (rank 116), 201 (117), 169 (118), 171 (119), 172 (120) and 165 (121).

- Card 162's criterion 2 reruns the LLM-customer check with Codex and Grok specifically, and the card says to re-freeze and rerun in one change because a changed document alone turns Usable v1 bar 6 red. The probe at 20:27:56Z shows grok ok again but codex still at its usage limit (reset about 2026-11-18). The card needs both, so it cannot finish now.
- Card 201 is owner-run: the mob does not open the sealed holdouts, and the product owner runs the conversion. The mob cannot finish it alone.

Change: card 162 takes Rank 121.1 and card 201 takes Rank 121.2, right after card 165. No other card moves. Card 162 moves back to Rank 116 at the first kickoff whose probe shows both codex and grok ok; card 201 moves back to Rank 117 when the product owner is ready to run the conversion. Card 169 (the map bytes do not depend on the culture) becomes the next eligible card.

## Vote

Yes adopts the change. A No says what would change it.

After the vote and any sentence, end with one line: `CITE: ` followed by a phrase of six or more words copied exactly from this proposal.
