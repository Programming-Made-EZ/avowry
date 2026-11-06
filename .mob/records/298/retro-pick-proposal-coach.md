# Retro pick proposal (agile coach seat): the gate runs the changed area's narrow suites and checks each step's failure path; a cycle with no red carries a mutation pair (trial)

Card 298 retro, 2026-11-06. Proposer: agile coach seat. For a Decider (number set at recording). Under the product owner's ruling of 2026-11-06 an adopted pick is filed as a story issue (Connextra persona, Gherkin criteria), not as an experiment entry.

Sources: .mob/records/298/retro-voices.json and retro-input.md; the gate (preship-gate.md) and the sign-off (signoff-ctx6.md).

PICK: for the next three cards that change code in mob-crosscli, the pre-ship gate and the cards' cycles follow these rules.

RULE UNDER TRIAL:
G1. The gate brief names the narrow test suites of every module the card touched. The gate runs each, and its record carries, per suite, the command, the test count and the raw result line. A suite the reviewer could not run is labelled blocked, and a SHIP then needs the facilitator-run evidence line (D190).
G2. A cycle whose new tests already pass before any change (no red) is accepted only with a recorded mutation pair: two scratch-copy mutations, each failing the matching test, with the failing assertion text (as in card 298's cycle 2).
G3. For a card that changes lock, registration or cleanup code, the gate also checks the failure path of each step of the changed code (what happens when each step fails, and what a crash between two steps leaves behind) and labels each rerun, record-only or blocked.
G4. Not changed: the board, the Decider protocol (followed exactly as written), the task force, the sign-off seat, D189 and D190.

STORY (to be filed if adopted, with a persona who gains the value): As Morgan, I want the gate to run the changed area's narrow suites and to check each step's failure path, so that the defects a fresh reader would find at sign-off are found at the gate, with raw results I can re-read in git.

Gherkin the story would carry:
- Scenario: the gate runs the narrow suites. Given a card that changed decider.py, When the gate record is written, Then it lists each narrow suite with command, count and raw result line, or marks it blocked with the facilitator-run line.
- Scenario: a no-red cycle is proven. Given a cycle whose tests pass before the change, When it is accepted, Then the record holds two mutations that each fail the matching test.
- Scenario: the failure path is checked. Given a card that changes lock code, When the gate runs, Then each step's failure and each crash gap is labelled rerun, record-only or blocked.

HYPOTHESIS: We believe a gate that runs the changed area's narrow suites and reads each failure path will find the defects that only sign-off finds today. We will know when sign-off-only findings (defects the gate did not report) fall to 0 on 2 of the 3 trial cards.

METRIC: defects found at sign-off and not at the gate, per card. Baseline: cards 303 and 298 (two cards in a row; 298: three defects, now cards 313 to 315).

COUNTER-METRICS (evidence, not targets):
1. Gate duration (reshape the rule if it grows more than twice).
2. Cycles padded with trivial mutations (the mutation pair must name a behaviour the card states).
3. Watchdog idle time while the gate runs.

TRIAL SCOPE: three cards that change mob-crosscli code. The third card's retro decides keep, revise or drop. Owner: agile coach.

User benefit: Morgan reviews a gate record that shows the suites and failure paths were run, so a SHIP is not followed by a sign-off that finds new defects.

Why this one: it is the shared core of the candidates from the QA, audit lens, decider, developer and architect voices; the product manager's and coach's own candidates (a second independent reviewer on all lock cards) cost a seat per card and are left for a later retro.
