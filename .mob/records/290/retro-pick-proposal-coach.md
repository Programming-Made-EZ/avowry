# Retro pick proposal (agile coach seat): a typed premise table, probed in 15 minutes, before the slice

Card 290 retro, 2026-11-04, revision 2. Proposer: agile coach seat. For a Decider. Sources: .mob/records/290/ retro-voices.json, retro-input.md, record-in-progress.json, signoff-ctx6.md; D181, D183, D184; the facilitator's window minutes.

Decider (number set at recording), the card 290 retro process pick, as a working-agreement experiment (D183: not a card)

PICK: before a slice is written, its seat fills a premise table: one row per premise, typed location, size or behaviour, each with typed evidence from a command on committed HEAD that the auditor can re-run. A 15-minute cap is the only bound; the probe builds nothing. Proposed by five of seven voices (product manager, QA, developer, decider, coach).

User benefit: the product owner gets a slice whose greens do not stop on a fact the code already held. Card 290's three green stops came from premises nobody checked.

Why this one:
- Dispatch Q1 (did D184 catch it?): partly. Both scope proposals ran their premises and none failed. D184 covers only scope proposals, so the slice's P5 ("two lines in result_of") went unchecked and failed three ways: the cite minimum sat in cite_ok (red 1), the call site at decider.py:340 (green 1 stop), and that call site dropped "did not answer" votes (green 3 stop). D184 C2 also let a locally applied change count as a run: slice attempt 1 built 405 lines and was stopped at 46 minutes (cap 30).
- Dispatch Q2 (lighter path for a declaration fix?): not now. 290-declfix took 8.1 minutes; the cost was the stop before it, and six seats passed its contradicting text, so fewer eyes is the wrong lever. A location row finds the call site before the slice. Deferred below.
- Dispatch Q3 (Grok-free windows): single-round 8.1 to 12.1 minutes (mean 9.5), 0 to 2 extra attempts, against 17.5 to 24.6 (mean 20.0), 3 to 12 extra, with Grok. Two-round windows unchanged (41.0 against 41.4 and 49.1; Grok still in 290-taskforce). Windows took about 79 of 360 minutes; rework from wrong premises cost more.
- D183: it changes a brief; no seat, file type or card.

HYPOTHESIS: We believe a typed, re-runnable premise table before the slice stops greens from tripping on facts the code already holds. We will know when green stops caused by a premise fall from 3 to at most 1 per card, while minutes from ready to first red do not rise past the bound below.

RULE UNDER TRIAL:
C1. Rows. Each slice line that states where the change goes (location), how much it touches (size), or how existing code or tests behave (behaviour) is one row. Intent, naming and rationale lines are not premises and are exempt.
C2. Typed evidence, on committed HEAD (hash in the row). Location and size: one static search command (for example git grep -n "result_of(" <hash>) and its output lines; "call site" means a static reference that command finds. Behaviour: one reproducible command against committed code (an existing test, or python -c importing it) and its settling output line. Behaviour rows cover each case the adopted condition or Done checks name (on card 290: cited, uncited, never answered).
C3. Exclusions. Dynamic dispatch, callbacks, aliases, generated code and anything outside the repo are not searched: the row says "unrun: <reason>" and what the green does if it is false.
C4. Measure, don't build; time is the only bound. No file edits except a scratch copy, discarded, never evidence. At 15 minutes the seat stops and marks open rows "unrun"; the facilitator stops a probe at 20.
C5. Check. The facilitator checks the schema, not completeness, before the slice window: each row has a type, HEAD hash, command and output, or "unrun" with a reason; behaviour rows name each case from the condition text. A gap goes back before any vote. The auditor re-runs the recorded commands at the recorded hash; a different output is a finding. The gate names each green stop a premise caused.
C6. Not changed: D175, D181, D184 for scope proposals, the stop rule. No new seat, file or meeting.

METRIC: green stops caused by a slice or scope premise (card 290: 3). Target: at most 1. Windows per card (card 290: 5).

COUNTER-METRICS (evidence, not targets):
1. Minutes from ready to first red (card 290: 139). A rise means ceremony.
2. Probe overruns past 15 minutes; more than one in the trial means C4 failed.
3. Rows per slice; 0 with the same scope means premises went unstated (theatre).
4. Regressions found after the gate; fewer stops count only if this stays 0.

TRIAL SCOPE: the next 3 cards with a slice that changes existing code. The trial retro decides keep, revise or drop by Decider.

COST: up to 15 minutes per slice and one facilitator schema look; card 290 lost 46 minutes to slice attempt 1.

REVERSIBILITY: two-way door. Drop the lines from the slice and facilitator briefs.

Deferred (D183 retro log): a lighter declaration-fix path (trigger: a declaration window on a trial card); Grok's return (trigger: card 291 lands; compare with the figures above); a checker script for the table schema (trigger: the trial keeps the rule).

## Revision after the first vote

- Architect (No): premises are now typed rows with typed evidence (C1, C2), non-premise lines are exempt, exclusions are explicit (C3), and the facilitator checks the schema, not semantic completeness (C5).
- Audit lens (No, uncounted): evidence is a command on committed HEAD with the hash recorded; scratch edits are never evidence (C4); the auditor re-runs them (C5). Call sites are what the recorded static search finds; behaviour cases come from the adopted condition, not the seat.
- Backend developer (No): call-site scope is static only, with dynamic dispatch, callbacks, aliases and generated code named as unrun (C3); non-behavioural premises are exempt (C1); time is the only bound (C4).
