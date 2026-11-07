# Usable v1 burn-up and forecast (facilitator report, read-only; 2026-11-06 local date, data pulled 2026-11-07 06:10 UTC)

Milestone: M1 Usable v1 (board/MILESTONES.md; bars in board/MILESTONE-usable-v1.md). It is the milestone the team tracks as the first usable release. Method: scope is every issue whose Milestone line starts with "Usable v1" (or "Vocabulary v1 and Usable v1 plumbing") and whose story actor is a consumer persona (Ari, Dana, Morgan or the product owner), minus issues closed as not planned; done is closed as completed; dates are US Pacific local dates (UTC-7 until 2026-11-01, UTC-8 after). Data: .mob/records/reports/usable-v1-burnup.csv (local_date, scope, done).

## Headline
- Scope 49 issues (35 on 2026-10-26, 49 from 2026-10-28), done 21, open 28. Six of the 28 are open issues whose text says Superseded (cards 8, 10, 11, 12, 55, 57), so the live remaining work is 22 Ready stories; adjusted scope 43, done 21.
- Done by date: 11 on 2026-10-27, 21 on 2026-10-28, then nothing: 0 closed in the last 9 local days (2026-10-29 to 2026-11-06). Scope: +14 on 2026-10-28, +0 since.
- Throughput over the last 7 days: 0 per day. Over the last 10 days: 1.0 per day (all of it on the first two days of the window), against scope growth of 1.4 per day over the same 10 days (all of it on one day). So in the 10-day window scope grew faster than done; in the last 7 days neither moved.
- Projection: none can be given. With zero throughput in the last 7 days there is no completion date; counting the 10-day rate and ignoring scope growth would give about 2026-12-04, but that rate belongs to 2026-10-27 and 2026-10-28 only and should not be used as a forecast. The date comes back only when product stories in this milestone start closing.
- Plainly: since 2026-10-28 the mob has closed no Usable v1 story. Its closed cards in the period were process and tooling cards (the board migration, the loop, the decider, the vote lock and others), which are not in this scope.

## Data quality
- Open stories with a consumer persona that look like product work but have no milestone line: 266 (docs and code use the name Avowry consistently). The other open stories without a milestone line are the process stories 308, 310, 311, 312, 316, 317, 318, 320, 322 and card 267, which the product owner's ruling of 2026-11-07 moves out of the board (process improvements are not issues; see docs/retros/experiments.md).
- Milestone items with no consumer actor: card 9 (the demo app: a Tasks API written entirely in Phrase), open and marked Superseded.
- Caveat: milestone membership is read from the current text of each issue's Milestone line, not from a field with history, so scope on earlier dates is reconstructed from creation dates; no Usable v1 issue was closed as not planned (superseded ones were left open), so "minus not-planned" removed none; the six superseded ones are shown in the adjusted figure above.
- Discrepancy to settle: board/MILESTONES.md still says M1 "Not reached (cards 002-017)" while the `usable-v1` CI job is green on every recent main commit; the report script was not run for this summary.
