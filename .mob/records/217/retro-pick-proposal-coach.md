# Retro pick proposal (agile coach seat): a pre-ship gate says SHIP only on commands it ran or named

Card 217 retro, 2026-11-06, revision 1. Proposer: agile coach seat. For a Decider. Sources: .mob/records/217/ (retro-voices.json, retro-input.md, signoff-ctx6.md, preship-gate.md); the card 264 gate (.mob/records/264/preship-gate.md); D154 and the working agreement's pre-ship gate; the retro experiments log.

Decider (number set at recording), the card 217 retro process pick, as a working-agreement experiment (D183: not a card)

PICK: every check in a pre-ship gate report is labelled "rerun" (the command and its output lines are pasted), "record-only" (the committed file it was read from is named) or "blocked: <why>". A gate whose required commands are all "blocked" or unlabelled may not say SHIP: its verdict is HOLD (no evidence), and the facilitator either re-seats the gate on another seat, or runs the required commands itself and records them in its notes, after which the sign-off seat is told the gate is a read-only review. Proposed by six of seven voices in this form (product manager, architect, QA, backend developer, audit lens, decider).

User benefit: the product owner can rely on a SHIP; today the word can be written by a reviewer that ran nothing.

Why this one:
- Card 217: the Copilot reviewer reported that shell execution was blocked in its session, ran none of the commands its brief required, and still wrote "Verdict: SHIP". It also wrote "A targeted grep shows no added/removed line weakening assertions" with no command behind it. The facilitator ran the checks and recorded them (the assertion diff showed five removed `Assert.NotNull(process)` lines, covered because the shared runner throws when a process does not start); the sign-off seat called the gate an overstatement. On card 264 the same seat ran its commands, but its safety line (no assertion or timeout weakened) had no diff behind it and it missed eight CRLF files; the facilitator ran the diff and the scan.
- Existing rules ask the reviewer to run commands (the brief) but nothing checks that a SHIP rests on them. D154's claim-equals-commit rule covers commits, not command output.

HYPOTHESIS: We believe a gate that must label each check cannot say SHIP on air. We will know when SHIP gates with an unlabelled or blocked required command fall from 1 of the last 2 gates (card 217) to 0 on the next three cards.

RULE UNDER TRIAL:
C1. The gate brief names the required commands (as today). The report lists each one as rerun / record-only / blocked, with the output lines for "rerun".
C2. A report with no "rerun" line for any required command cannot carry SHIP. The facilitator's checklist reads the labels before the sign-off seat is briefed; a missing label is sent back once to the same seat.
C3. When the reviewer is blocked (no shell), the facilitator runs the commands itself and puts them in the report's notes (as it did on card 217); the verdict is then written by the facilitator as "SHIP on facilitator-run output" and the sign-off seat is told so. Nothing is hidden: the notes say who ran what.
C4. Not changed: the reviewer's independence, the brief's content, D154, the sign-off, D175. No new seat or file.

METRIC: gates whose SHIP rests on an unlabelled or blocked required command (card 217: 1 of 1; card 264: 0 of 1 for commands, with 1 unsupported safety claim). Target: 0 on the next 3 cards.

COUNTER-METRICS (evidence, not targets):
1. Minutes added to the gate step (card 217: the facilitator's checks took about 6 minutes). More than 15 means ceremony.
2. Gates sent back or re-seated per card (more than 1 means the labels are being gamed or the brief is unclear).
3. "blocked" labels that the facilitator then ran successfully in under 5 minutes (a signal that the reviewer's environment, not the check, was the problem).

TRIAL SCOPE: the next 3 cards that have a pre-ship gate. Its retro decides keep, revise or drop.

COST: one labelled list in the report and one read of it in the facilitator's checklist.

REVERSIBILITY: two-way door. Drop the labels from the brief.

Deferred (D183 retro log): a checker that rejects a gate record with no command output lines (architect and audit lens); a line-ending check at commit (existing in the experiments log from card 264); an "evidence inventory" before any scope window (agile coach's own candidate); a cap on seat time that the runner enforces (card 290) with the 90-minute 3a overrun on this card as new evidence; an automated check of seat briefs for the LF instruction.
