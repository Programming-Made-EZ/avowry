# Card 313: the task-force declaration is stalled on two conflicting conditions (facilitator; escalated to the product owner)

## Where the full Decider stands (standing rule, because the D153 lighter path expired by count; see window-status-check.md)
- Round A (revision 2): developer Yes, decider Yes, QA, coach, architect Support-it; audit lens No (condition: a derived command list alone cannot prove from git that no command was omitted). Adopted (A2, revision 3).
- Round B (revision 3): developer Yes, five Support-it, no No. Independent re-run: QA No (explicit red-first commit and push refusal tests; a hash stored outside the file it covers). Adopted (B1, B2, revision 4).
- Round C (revision 4): decider Yes, four Support-it, audit lens No (commit a scrubbed but structurally complete source index so git alone shows no omitted tool call). Adopted (B3, revision 5).
- Round D (revision 5): developer Yes, five Support-it, no No; every cite verbatim after one re-ask. Independent re-run with fresh seats (tag d313h): developer Yes, audit lens, coach, decider(cite unread), QA(cite unread) Support-it/Yes, and the architect **No**: "the audit trail grew from a simple no-write check into duplicative manifests, transcript indexes, tool-call manifests, extraction/verify scripts, and hash-count choreography, which is brittle and creates false-positive risk"; what would bring it to Yes: "a smaller, deterministic proof: pre/post repo manifests plus one verified tool-call extraction test for the green turn, without redundant hash chains."

## Why the facilitator does not settle it alone
- The two conditions conflict: the audit lens's round-C condition (B3, the structural index) is what the architect calls over-specified; removing B3 reopens the audit lens's No, keeping it keeps the architect's No. A reasonable, reversible condition is adopted, but one that contradicts another seat's condition is not something the facilitator may pick between (strict reading: a No is a No).
- The audit lens's condition ("git alone can verify that no tool call was omitted") cannot be fully met by anything committed from the facilitator's own transcript, since git cannot see the transcript; B3 only makes an omission detectable to a reader who also holds the transcript. Each round has added a demand (D145 says a later round may not add new demands).
- The mechanism is out of proportion to the fix: card 313 is a one-function retry in decider.py (a bounded loop around the registration). The evidence about the facilitator's own commands during one green turn was added to answer the audit lens's first objection.

## Options put to the product owner
1. Adopt the architect's smaller form: pre and post repo manifests plus the reflog (A1), the facilitator's command list for the turn (A2 without the index B3 and without the second manifest hash B2), and one extraction test. Record that the audit lens's maximal condition is not met and cannot be from git alone; the audit lens's No stands, so under the protocol the declaration would fail unless the product owner rules on the conflict.
2. Keep revision 5 (all of A1, A2, B2, B3) and override the architect's No as a conflict the product owner settles.
3. Drop the facilitator-trace condition entirely: the green seat works in a separate git worktree of mob-crosscli (as the card 230 scratch worktrees did), so the main checkout is untouched by construction; before and after manifests of the main checkout prove it and no extraction script is needed.
Recommendation: option 3 if the product owner wants the proof; otherwise option 1. Either removes the new script and its hash choreography, which no consumer value in the card needs.

## Status
- Card 313 stays in progress; no green seat, no code. The records of every round (votes in the scratchpad runs d313a, d313b, d313c, d313e, d313f, d313g, d313r, d313h) are committed with the next record; the decision is not recorded as passed or failed until the product owner rules.
