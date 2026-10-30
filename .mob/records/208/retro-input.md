# Card 208 retro input (facilitator; facts only)

Card: a seat starts only on a fresh, pinned probe, and seats rotate least-recently-used across the available models (corrective for MD-002; code in mob-defaults). Product owner goal (D161): lock the process into tooling and the template.

## Flow metrics (UTC, from commits)
- Task force window 17:13 (66d833f); slice and split 17:31 (6bd8db2; six runner cards filed).
- Criterion 1 (fresh probe before every seat): red 18:10, green 18:40 (30 min).
- Criterion 2 (as first built, Claude refused while any other model had quota): red 18:54, green 19:47 (53 min; second full run after an existing test broke).
- Criterion 3 (pinned probe model): red 20:05, green 20:48 (43 min; one full run).
- Review 20:56 (c2ecd10, CHANGES REQUESTED: F1 to F3, and criterion 2 against the product owner's clarified intent).
- Correction of criterion 2 to least-recently-used rotation (product owner D163, Decider D164): red 21:36, green 22:45 (69 min; first full run failed on shared-fixture pollution), review fix F1 22:47; re-check APPROVE 23:02; pre-ship gate 23:15; sign-off 23:18.
- Window to sign-off: about 6 h. Full mob-defaults suite: about 20 minutes per run; 6 full runs on this card.

## Practice metrics
- D157 one criterion per cycle: held for all cycles, including the correction.
- Outside-in with recorded doubles: held; every unit layer names its doubles.
- D136: one breach, the correction's red (d3125ea, 463 lines).
- Seat-written in-flight lines: own timestamp on every seat start after stand-down 1; cap moves 0 on criteria 1 and 3, 1 on criterion 2 and 1 on the correction.
- Re-probes at 50 minutes: on time all evening.
- Metric for the card (rules the runner enforces versus by hand): 6 of the 7 seat-and-probe rules, up from 0 before the card (sign-off ef393a3).

## Adjustments to review first (D155)
1. Stand-down 1 (0d94bd8): seat-written in-flight lines with probe age; re-probe at 50 minutes.
2. Stand-down 2 (d109aa0): the red lists the existing tests the green must change; fix fixtures before the one full run.
3. D157 and D158 (trial cards 3 and 2).

## Observations for this retro
- The facilitator ended its turn three times while seats ran in the background (watchdog); now blocks with capped waits.
- Order-dependent test pollution: one test mutated a dictionary shared by every module; found only by a full run.
- The criterion 2 rule was built to the letter of the finding (no Claude while others have quota) instead of the product owner's intent (balanced rotation); corrected after sign-off-time clarification.
- A scribe was briefly seated to write the seats' turn records; stopped, and the seats wrote their own.

## Product owner and Dispatch proposal for this retro: the nested loops as feedback loops
See .mob/records/208/retro-proposal-nested-loops.md. Each role maps the seven loops (pairing, unit TDD, acceptance TDD, slice, feature, epic, lifecycle) by their FEEDBACK: who or what gives it, how fast it arrives, what the team does with it; then which loops lack feedback and where drift came from. At most ONE experiment (hypothesis, metric, counter-metric, trial scope). If the team adopts the lens, a card puts it in the template.
