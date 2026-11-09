# Retro pick proposal (agile coach seat): adopt the product-owner inbox as the decision channel (experiment)

Card 303 retro, 2026-11-06, revision 1. Proposer: agile coach seat. For a Decider. Sources: .mob/records/303/retro-voices.json and retro-input.md; the inbox proposal itself (C:/Users/<user>/.mob-inbox/avowry.md, "Process proposal for retro: adopt this inbox", routed to docs/retros/experiments.md); the product owner's correction that turns end at card and cycle boundaries; memory of the earlier incident (decisions sat undelivered for over an hour).

Decider (number set at recording), the card 303 retro process pick, as a working-agreement experiment (D183: not a card)

PICK: adopt the product-owner inbox file (outside the repo, C:/Users/<user>/.mob-inbox/avowry.md) as the channel for product-owner decisions, questions and process proposals, for the next three cards.

RULE UNDER TRIAL:
C1. The facilitator reads the inbox at every card and cycle boundary and at every retro (as it has since 2026-11-06).
C2. Each entry is acknowledged in the file within one cycle: moved under ACKNOWLEDGED with a UTC timestamp and where it went (a card or closure record for a decision; docs/retros/experiments.md for a process proposal or question, weighed at the next retro through Decider).
C3. GitHub issues stay bugs and stories only (D183); an inbox item is never filed as an issue.
C4. A product-owner decision that changes a seat or a rule mid-window is applied at the next boundary or as a stated amendment that is polled in full (as on card 303's task-force window, D187), not by quietly editing a polled text.
C5. Not changed: the board, the windows, the product owner's right to message the facilitator directly in chat.

HYPOTHESIS: We believe a file read at boundaries, with a written acknowledgement, gets product-owner decisions to the team within one cycle instead of hours. We will know when, over the next three cards, every entry is acknowledged inside the cycle it arrived in.

METRIC: entries acknowledged within one cycle of arrival (card 303: 3 of 3, each at the boundary where it was read: the Codex build-seat decision, the card-format question, the Muse proposal; the retro input counted four because the machine action arrived by chat). Target: all of them on the next three cards.

COUNTER-METRICS (evidence, not targets):
1. Unacknowledged entries at each retro (target 0).
2. Decisions that skip the board or the records (an entry applied with no card or closure record named in its acknowledgement).
3. Mid-card policy changes after a window has opened (card 303: one, the Codex build seat, handled as a polled amendment).

TRIAL SCOPE: the next 3 cards. The retro of the third card decides keep, revise or drop.

User benefit: the product owner's decisions reach the team within a cycle, and the product owner can see in one file what was decided and where it went.

Why this one: five of the seven voices keep it or want it adopted (product manager, developer, coach, QA, the architect with a tune); the decider and the audit lens ask for a tune (freeze changes after a window opens: C4). Other candidates, logged in docs/retros/experiments.md: a slice check that its red can fail or pass on any machine (developer), a gate with a blocked shell is labelled "SHIP on facilitator-run output" every time (QA, audit lens; this is D190's own rule C3, not followed in words on this card), an evidence box per card (decider), an audit bundle (audit lens), one task-force round on bug cards whose cause the probe confirmed (coach).

## Revision 2 (after round 1: Yes 2 (product manager, developer), Support-it 2 (architect, decider), No 2 (QA, audit lens); every cite valid)
Round 1 (.mob/records/303/pick-votes-round1.json) raised one common point from four seats, so by D187 this is a full re-poll of the amended text. Common point: the inbox is outside the repo, so arrival and acknowledgement cannot be checked from git (QA, audit lens: "would vote yes if" arrival and acknowledgement were committed evidence; architect and decider: copy the acknowledgement into the records, define "cycle", give the baseline). The amendment adds to C2 and C1, and changes nothing else:
C1b. "Cycle" and "arrival" are fixed: an entry arrives when the facilitator first reads it at a boundary (its UTC time is the entry's `read_at`); it is acknowledged in the same boundary's cycle, before the next card or seat is started. The baseline is the earlier incident the inbox proposal names: product-owner decisions that sat undelivered for more than an hour (the proposal's own text).
C2b. Each acknowledgement is also appended, as one line, to the committed file .mob/records/inbox-log.jsonl: title, `read_at`, `acknowledged_at` (UTC), and the destination (a card or closure record path, or docs/retros/experiments.md). The line is committed in the same boundary's commit, so git history gives arrival-side evidence the external file cannot. The retro computes the metric from this file, and a line missing for an entry that the inbox file shows as acknowledged counts as unacknowledged.
Not added: a failing-fixture checker for the log (QA's and the audit lens's stronger ask). It is logged as a follow-up experiment in docs/retros/experiments.md; if the trial shows lines being skipped, the retro revises to the checker.
