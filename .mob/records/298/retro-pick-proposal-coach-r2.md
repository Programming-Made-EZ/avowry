# Retro pick proposal (agile coach seat), revision 2 after round 1: the gate runs the changed area's narrow suites and checks each step's failure path; a cycle with no red carries a mutation pair (trial)

Card 298 retro, 2026-11-06. Proposer: agile coach seat. For a Decider (number set at recording). Under the product owner's ruling of 2026-11-06 an adopted pick is filed as a story issue (Connextra persona, Gherkin criteria), not as an experiment entry.

Revision 2 (after round 1: pm Support-it, architect Support-it, developer Yes, decider Support-it, QA No, audit lens No; votes in .mob/records/298/pick-votes-round1.json): the amendment resolves the two No conditions (a suite table and enumerated steps, preserved mutations that name a behaviour, no SHIP on record-only failure paths) and the architect's and decider's tunes (G3 lists the steps; G3b tags defects to rules). By D187 the conditions came from two sources, so all six are re-polled. NOTE TO THE SEAT: the cite is matched case-sensitively and character for character; copy a phrase of at least 6 consecutive words exactly as written in the proposal.

Sources: .mob/records/298/retro-voices.json and retro-input.md; the gate (preship-gate.md) and the sign-off (signoff-ctx6.md).

PICK: for the next three cards that change code in mob-crosscli, the pre-ship gate and the cards' cycles follow these rules.

RULE UNDER TRIAL:
G1. The gate brief carries a committed table made by the facilitator from `git diff --name-only`: for each changed code file, the test files that name it (found with `git grep`) and the narrow suites they belong to. The gate runs every suite in the table, and its record carries, per suite, the command, the test count and the raw result line. A suite the reviewer could not run is labelled blocked, and a SHIP then needs the facilitator-run evidence line (D190). A suite the table omitted and sign-off later shows to cover the change counts as a gate defect.
G2. A cycle whose new tests already pass before any change (no red) is accepted only with a recorded mutation pair: two scratch-copy mutations, each committed as a patch file and each failing the matching test, with the failing assertion text (as in card 298's cycle 2). Each mutation names the Done check or stated behaviour it breaks; a mutation that names none does not count.
G3. For a card that changes lock, registration or cleanup code, the facilitator lists in the gate brief one line per step of the changed code and one line per gap between two steps (from the code diff), so the gate cannot narrow the scope. The gate checks, for each line, what happens when the step fails or a crash falls in the gap, and labels it rerun (an injected failure or a probe it ran), record-only or blocked. A line labelled record-only or blocked makes the verdict HOLD until the facilitator runs the probe and attaches its output as facilitator-run evidence.
G3b. The retro tags each defect found at sign-off or later to the rule (G1, G2 or G3) that should have caught it, so the trial can credit one rule.
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
