# Re-rank 20 inputs (runs at card 276's boundary, after it closes)

Queued by the facilitator, 2026-11-03. Re-rank 20 does not interrupt card 276.

## Proposals for the Decider

- Issue 278 (label proposal, from the product owner; a proposal, not a ruling): rank the cmd.exe line-limit fix right after card 276. That fix is cards 229 (Copilot), 270 (Grok) and 277 (diagnosis; exclude only for quota or unavailability).
  - Evidence: audit 187. Window 276-scope paid 20 empty Grok and Copilot attempts across two rounds, about 38 minutes of wall clock for about 20 minutes of votes.
  - Root cause: .mob/records/notes/2026-11-03-empty-decision-votes-root-cause.md.
  - This goes to the Decider at re-rank 20 as a proposal.
- Issue 284 (label proposal, from the product owner; a proposal, not a ruling): name drift resistance (code vs requirements, docs vs behaviour, model output across versions) as an Avowry benefit, with a pre-registered, Goodhart-guarded measure: planted unintended changes flagged by generated checks against a plain-language baseline, reported with the false-alarm rate, pre-registered like cards 149 to 151. The Decider decides whether to add it to VISION and the verdict page's success criteria, and whether to card a measurement design.
- Issue 288 (label proposal, from the product owner; a proposal, not a ruling): draw a cut line at re-rank 20. Only truly blocking process cards (safety, the vote fixes, the flaky tests) rank above product; product starts next with the rename (cards 196 and 197); the remaining process cards interleave with product; new process follow-ups land below the cut by default. Measure: product cards closed per day, guarded so open FAILs and per-card findings must not rise. To the Decider at re-rank 20 with 278 and 284, after card 276 closes.
- The product owner's coach-don't-bench note on card 277 (posted on issue 229): weigh ranking the prompt fix (229/270) ahead of the exclusion-enforcement work.

## Cards to place

- 266, 267, 270 to 275, 277, and the card 276 follow-ups filed 2026-11-03: 279 (the Muse reader reads real tool records; depends on the Windows sandbox setup), 280 (Muse usage text sign-in states), 281 (Muse seat workspace trust), 282 (seat prompts reach seats without placeholder rewriting), 283 (the delete guard does not flag a read-only python command; filed in cycle 4). Card 279: close as delivered by 276 (commits 25d50b8 to 0000715; sign-off section 5). From card 276's sign-off: 285 (Muse's sandboxed shell completes a command), 286 (a NO OBJECTION reply is stored as no objection), 287 (locate_muse takes one kind of root).

## Deferred while card 276 is held (facilitator, 2026-11-03 16:05 UTC)

D180 holds card 276 open, and the product owner did not rule on the re-rank. The facilitator defers re-rank 20 for three reasons. First, D148 places a re-rank at a loop boundary (a card's Done and retro, before the next pull), and card 276 has not reached one. Second, the working agreement keeps WIP at one card and has no rule for parking a blocked card, so a re-rank now would rank a pull the team cannot make. Third, Dispatch routed issue 278 to the re-rank after card 276 closes. If the hold outlasts the in-flight cap (2026-11-04 04:05 UTC), the facilitator will put a 'park a card blocked on an outside action' proposal to the Decider instead of pulling early.

## After re-rank 20 (facilitator, 2026-11-03 22:25 UTC)

The product owner confirmed D182's condition on 288 in the product owner's own wording (D183, issue 288 comment). Line 36 now says process improvements serve the story being worked and the board holds only user stories and bugs. The next re-rank, after card 229, sorts the existing process cards: real defects in safety, decision votes or tests stay as bugs; the rest leave the board and become retro experiments.
