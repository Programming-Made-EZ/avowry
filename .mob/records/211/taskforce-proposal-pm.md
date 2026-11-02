# Card 211 task-force declaration proposal

Purpose: corrective for MD-010 on mob-defaults commits `687410e..e3736d1` (27 commits by one identity), producing records only: a Decider with all declared role seats, an independent re-run, and a final-state review committed by an identity that authored none of those 27 commits.

## Team
- Product manager (ready): **Copilot**, **ctx-1**. Why: owns scope, keeps MD-010 corrective intent explicit, and opens the window with the D165 intent line.
- Architect (slice): **agy**, **ctx-2**. Why: writes the records-only slice, Done checks, D175 cap lines, and hostile-input labels (SEEN/INSURANCE).
- QA engineer (independent re-run): **Claude**, **ctx-3**. Why: build seat; runs the independent suite/audit re-run with command-reader evidence.
- Independent reviewer (final-state review): **Copilot**, **ctx-4**. Why: build/review seat with command reader; writes and verifies the final-state review record against git history.
- Decider facilitator (decision window): **agy**, **ctx-5**. Why: decision-only seat under 228-agy-seats; runs vote collection and D171 cite checks.
- Product manager (sign-off): **Claude**, **ctx-6**. Why: fresh-context sign-off and blocker call on unresolved identity constraints.

Rotation note: seats rotate by least-recently-used across Copilot, agy, Claude (D163/D164). Build seats stay on command-reader models (Claude/Copilot) per window 228-agy-seats; agy remains decision-only.

## Left out and why
- Codex: no quota in the current probe window.
- Grok: no command reader; decision replies were empty in prior windows (card 270), so not used for build/review evidence.
- Backend/frontend/UX/database/security/devops/technical-writer/language/performance roles: this card is records-only and does not change product code or UI; extra roles would add ceremony without improving the MD-010 corrective evidence.

## Domain-expert question (T2)
Can Done check 3 (non-author final-state review **by identity**) be satisfied when all seats currently commit under the same git identity, and if not, what approved non-author identity path is allowed?

## Value, metric, counter-metric
- Value: restore independence/accountability evidence for the 27-commit range without rewriting history.
- Metric: all three Done checks pass in one recorded trail with no reviewer-identity ambiguity.
- Counter-metric: decision/review churn (re-opened windows, repeated re-runs, or HOLD-to-SHIP reversals caused by unclear identity evidence).

## Done checks (D157) and D175 caps
1. **Decider with all role seats**: a Decider record + votes file includes every declared role seat, each with reason/cite and distinct context. Partial work fails if any role seat is missing, uncited, or not attributable to its context.
   - **Cap: criterion 1, at most 1 cycle**
2. **Independent re-run**: a non-author context re-runs suite + audit checks for `687410e..e3736d1` and head, with commands/commit/results recorded.
   Partial work fails if only part of the range is checked, commands are missing, or independence is not evidenced.
   - **Cap: criterion 2, at most 1 cycle**
3. **Non-author final-state review in git (identity-level)**: review record is committed by an identity that authored none of the 27 commits, cites MD-010 and range, and lists author identities from git log.
   Partial work fails if reviewer uses the same committing identity as the 27 commits, even with a different seat/context.
   - **Cap: criterion 3, at most 1 cycle**

Cap total is 3 (D175-compliant). Any red beyond a cap requires a scope window committed before that red.

Hostile forms (D167/D175 labeling for the slice):
- **SEEN**: same git identity authored all 27 commits.
- **SEEN**: all current seats commit under that same identity.
- **INSURANCE**: review from a different context but same identity incorrectly treated as independent.
- **INSURANCE**: co-author/trailer text mistaken for distinct committing identity.

## Which rules apply
- **D165** applies: include explicit intent line before first red/re-run criterion.
- **D166** applies: slice must state assumption about identity independence and probe it against git history.
- **D167** applies: hostile-input list required (with SEEN/INSURANCE labels and entry-by-entry handling).
- **D168** applies: keep slice under 15 KB and reject missing required slice fields.
- **D171** applies: vote windows require valid CITE lines; unread votes do not count.
- **D175** applies directly: cap line per criterion, caps sum <=3, hostile labels, and pre-red scope window for any over-cap cycle.

## Done check 3 feasibility and blocker call
With today's single shared committing identity, Done check 3 cannot be met by seat/context rotation alone. Meeting check 3 requires either:
1) a real non-author committing identity to perform the final-state review, or
2) a product-owner ruling that changes or waives the identity-level requirement for this corrective.

This is a **product-owner decision blocker**, not a team-only decision.

## Open points
- Which approved non-author identity can commit the final-state review now?
- If no separate identity is available, does the product owner grant a one-card waiver/change for Done check 3?
- If a waiver is granted, what replacement evidence is required so MD-010 remains auditable?
- Is a dedicated identity/provenance checker needed in follow-up to prevent recurrence?