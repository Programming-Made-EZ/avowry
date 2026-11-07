# Retro pick proposal (agile coach seat): the cite check stops costing rounds on format (experiment)

Card 313 retro, 2026-11-07. Proposer: agile coach seat. For a Decider (number set at recording). Under the product owner's rulings an adopted pick is filed as a story issue (the product owner as the actor, "so that cards reach users sooner") with Gherkin and ZOMBIES.

Sources: .mob/records/313/retro-voices.json and retro-input.md; the product owner's note on flow cost (decision windows against red, green and refactor). The card's decision took about 111 minutes of seat time across 106 seat runs for a change of about 15 minutes of slice, red and green. Most re-asks were format, not substance: a capital letter, a trailing full stop, a driver echoing the prompt's placeholder cite (three or four times), one empty reply.

PICK: for the next three mob-crosscli cards, the cite rule is applied as follows, and nothing else about the Decider changes.

RULE UNDER TRIAL:
K1. The cite check (D171) matches a cited point of at least six words after normalising case, markdown emphasis characters and trailing punctuation on both sides; the words themselves must still be copied from the proposal in order. A cite that fails after normalisation is still re-asked once, then the seat is replaced.
K2. A reply whose cite is the prompt's placeholder text, or whose reply is the prompt echoed back, is recorded as "echo" (a missing cite), is re-asked once on a different driver than the one that echoed, and counts toward the seat's reply only after that. The seat-use record names the echoing driver.
K3. Not changed: what must be cited, the six-word minimum, the one re-ask, the replacement of a seat that fails twice, the Decider protocol (followed as written), and the product owner's rulings.

HYPOTHESIS: We believe that normalising case and trailing punctuation, and re-asking an echo on a different driver, removes most format-caused re-asks without letting an uncited vote count. We will know when format-caused re-asks per Decider fall from this card's baseline (10 cite re-asks in the Decider runs d313c, d313g, d313j, d313k and d313m, out of 106 seat runs) to at most 2 per Decider over three cards.

METRIC: format-caused re-asks per Decider (case, punctuation, echo, empty), and seat minutes per decision.

COUNTER-METRICS (evidence, not targets):
1. Votes counted without a real cited point (target 0; the checker must still fail a cite that does not appear in the proposal).
2. Wrong-seat attribution in the seat-use record (target 0).
3. Sign-off-only findings traced to a loosened cite check (target 0; any one ends the trial).

TRIAL SCOPE: the next three cards that change mob-crosscli code or hold a Decider. The third card's retro decides keep, revise or drop. Owner: agile coach.

User benefit: the product owner's cards reach users sooner, because decision rounds stop being spent on capital letters and full stops.

Why this one: it is the shared core of the product manager's, architect's and agile coach's candidates; the lighter-path renewal (audit lens), the one-condition cap (QA, decider; story 320) and the patch and line-ending hygiene (developer) are separate items: 320 is already filed, the hygiene is a brief fix, and the lighter path is a standing-rule question for its own proposal.
