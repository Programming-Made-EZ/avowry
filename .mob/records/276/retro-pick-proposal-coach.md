# Retro pick proposal (agile coach seat): a lettered red ends at the first passing green

Card 276 retro, 2026-11-03. Proposer: agile coach seat. For a Decider. Sources: voices .mob/records/276/retro-voices.json; facts .mob/records/276/retro-input.md; .mob/records/276/record-in-progress.json (decisions.cycle_count_ruling, decisions.scope_window_4, deviations); .mob/records/decisions/276-scope-4.json; .mob/records/276/signoff-ctx6.md section 3; D175 and D157 in docs/decisions/process-and-staffing.md.

Decider (number set at recording), the card 276 retro process pick, as an experiment

PICK: a lettered red (4b, 4c) is allowed only before any green has passed the red it corrects. Once a green has passed, the next red for that criterion is a new cycle, counts against the D175 cap, and needs a D175 scope window adopted before it. Proposed by five of seven voices (product manager, QA, developer, decider, coach); the sign-off asks for it (signoff-ctx6.md section 3); the architect voice names it as the one rule to keep. The ledger and the pre-check are deferred.

User benefit: the product owner learns that a card is past its cap before the extra cycle is spent, not from an audit afterwards. Card 276 spent cycles 5 and 6 with no window; scope-4 ratified them after green 0000715 and refactor fdfa821 had landed (cycle_count_ruling).

Why this one:
- It closes the loophole that broke D175 on this card. D175 C4 already says a red past the cap starts only after a window; lettering let reds 25d50b8 and 58b5c26 look like cycle 4 after green 2e56d9f had passed red 5b3292e (cycle_count_ruling). The commit bodies still say "Cycle 4 of 4 ... red amendment 4b/4c" (signoff-ctx6.md section 3).
- It writes down a line the facilitator already drew: 1b and 3b were in-cycle because no green had passed (cycle_count_ruling). D157 C2 pairs each red with its green, so a passed green closes that pair.
- Architect's ledger with automatic checks: a tool, and a ledger still needs this definition to count lettered reds right. It is the follow-up once the rule holds.
- Audit lens's launchability pre-check: it targets empty seat attempts, which are the cmd.exe line limit (proposal 278) and cards 229, 270, 277, not this card's largest process miss.
- Queued proposals 278 and 284 go to re-rank 20, not to this pick (retro-input.md, queued inputs).

HYPOTHESIS: We believe that ending a lettered red at the first passing green makes every red past a cap visible before it starts, so D175 windows come before the red and not after. We will know when reds past the cap with no prior window fall from 2 to 0 over the trial, while scope windows per card and the time from a failing real-evidence check to the next red do not rise.

RULE UNDER TRIAL:
C1. Passed green. A green has passed a red when a commit for the same criterion, after that red, is committed as its green with the red's tests passing on its named run. A later failure on real evidence (trimmed fixture, real log) does not reopen it: green 2e56d9f counts as passed.
C2. Lettered red. A red may carry a letter (nb) only if no green has passed red n. Its commit body says "Corrects red <hash>; no green since". Otherwise it is cycle k+1 and its body and turn record say "Cycle k+1 of <cap> for criterion n" (D175 C5).
C3. Window first. A red that would be cycle k+1 past the cap waits for a D175 C4 window committed before it. Its proposal names the failing evidence and the cycles used.
C4. Remaining cap. One window may set the remaining cap for a criterion ("at most j more cycles") with one reason per cycle, instead of one raise per window (coach voice). It never ratifies cycles already run: a ratification after the fact still counts as a miss on the metric, as scope-4 says.
C5. Check. A person checks, from a record field read in git. The facilitator checks C2 before launching any lettered red: git log for the criterion shows no green after the corrected red. The pre-ship gate reviewer lists each lettered red with that check (D175 accountability check). The auditor re-checks from git. A git-log checker is a follow-up card, red first (QA voice).
C6. Not changed: D175's caps, metric and window, D157, D136, D168, the green commit rule. No new seat, file or meeting.

METRIC: reds past the last adopted cap with no window committed before them (card 276: 2, 25d50b8 and 58b5c26). Target: 0.

COUNTER-METRICS (evidence, not targets): (1) scope windows per card and their all-adopted rate (card 276: 276-scope to 276-scope-4, four, all adopted; all adopted reads as theatre, D175). (2) Minutes from a failing real-evidence check to the next red (not recorded on card 276; the trial records it), so the rule does not add waiting. (3) Hours from first red to sign-off (card 276: 02:18 to 18:22 UTC, about 16, with about 7 blocked on D180). (4) Lettered reds per card: a fall to 0 with no windows may mean letters became silent amendments.

TRIAL SCOPE: the next 3 capped cards (slice with D175 Cap lines) after this Decider is recorded. A card with no red past a passed green gives no reading and the trial retro says so. The trial retro decides keep, revise or drop by Decider.

NO BLUR: D175 keeps its own metric; this pick defines when C4 applies. Fewer windows from C4 are credited only if counter-metric (2) holds.

COST (issue 245): one commit-body line per lettered red, one git-log look by the facilitator before it, one gate line. If the cost shows, C4's remaining-cap form is cut first.

REVERSIBILITY: two-way door. Drop the lines from the QA and gate briefs; nothing else changes.

Deferred, not rejected:
- Pre-window cycle ledger with automatic checks (architect): a tool; build it on C1 and C2 once they hold.
- Seat launchability pre-check (audit lens): an empty-seat lever; with 278, 229, 270, 277 at re-rank 20.
- Full suite per green or a waiver (QA, decider, architect voices): its own tune of the green commit rule.
- Fresh contexts and a pinned model for reds; Grok plan-mode briefs: brief items.
