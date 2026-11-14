# Card Ready and kickoff

When: at refinement, before a card is Ready; and at kickoff, before its first work commit.

- The card names a persona from PERSONAS.md and the goal it serves, in a Connextra story. Gate: `card_ready.py`
- Acceptance criteria are Gherkin, each tagged with its ZOMBIES letters, and every letter is covered or N/A with a reason. Gate: `card_ready.py`
- The four Dimensions of Difficulty are answered, each YES with its mitigation in its fixed form, and every spike resolved. Gate: `card_ready.py`. Evidence: each listed spike card is closed on the board (the gate reads only the card's `(resolved)` marker).
- A spike exists only for the story prioritized next. Evidence: the board order at refinement (the coach, row I13).
- The task force is declared at kickoff: roles sized to the card, each with its domain, its why and its own context. Gate: `facilitator.py` (card mode refuses a card with no `## Task force`). Gate: `mob_process.py record`. Evidence: the card's Task force section shows each role's domain and why and that the roles fit the work (the gates check only that roles, whys and contexts exist).
- The domain-expert question is asked and answered at refinement. Gate: `mob_process.py record`
- The Gherkin criteria drive the outside-in acceptance test, whose BDD comments are their text. Evidence: the acceptance test's diff.
- Any Decider run at refinement follows `checklists/decider.md`. Evidence: the Decider record.
