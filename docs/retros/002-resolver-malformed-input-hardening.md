# Retro: card 002, resolver malformed-input hardening (2026-10-25)

## Flow metrics (from git timestamps)

- Card filed 19:17 (7543e60); mob run started about 19:22; shipped 19:33 (8ba3699). About 11-16 minutes from run
  start to main: one increment, 234 tests (from 225), CI green, zero warnings.
- Independent review wait: about 1 h 35 m (19:33 shipped, verdict about 21:08). Most of it was lost to a quoting
  error that made the first Grok review return empty (about 40 minutes), then a rerun. Minutes lost to failed or
  retried cycles: 0 for the card, about 40 for the review tooling.
- Review findings after the fact: 4 (Grok: wrong endpoint name in PH334, nested spec paths not rejected, duplicate
  binding diagnostics, a duplicated docs table). Reverts: 0. Rework: bug card 018 filed.

## Individual

- agy: shipped all 7 acceptance criteria in one cycle, red-first, fuzz green; the review found issues in the spec
  half (a47ed6a, Copilot's) as well as one wording problem of its own. Adjustment: grep for placeholders such as
  `Probe` before declaring a message done.
- Grok: the review was precise and evidenced (revert-only red check). Rung 1 clean cycle count unchanged (it
  reviewed, it did not author this card).
- Codex: QA turn for 002 passed work whose placeholder message survived. Adjustment: probe error messages with a
  real second case, not only the one in the test.
- Facilitator: the first Grok review failed silently (nested quoting) and I did not notice for about 40 minutes.
  Adjustment: every background reviewer writes a result file and a non-empty check runs before waiting on it.

## Team (mob)

- Review independence held: agy authored, Grok reviewed, Copilot-authored a47ed6a was reviewed in the same pass.
- The mob's Done-when for 016 was again satisfied by existing text; fixed by the concrete checks written after
  the first cycle.

## Organization

- Escalation, Decider and task-force rules all landed this card's hour in the agreement and in mob-defaults
  (49 tests); the rule changes cost more mob time than the card did. Hypotheses-not-doctrine now gates new
  process ideas.
- Allotments: all four drivers used; Claude not signed in.

## Product: LLMs-as-customers feedback (round 2)

All four models read GRAMMAR.md and orders.phrase, explained CreateOrder, and fixed a broken snippet (typo in a
failure code), saying which diagnostic they expect. They all predicted PH334 correctly from the docs, so the
language is already guessable; the friction is the message wording.

| Finding | Models | Outcome |
|---|---|---|
| Diagnostic codes are documented but not the exact message text; models guess the wording | Codex, Copilot, Grok, agy (4/4) | **Card 019**, rank 5 |
| Closest-match suggestion for a mistyped failure code | Codex | **Card 020**, rank 12 |
| Inconsistent `given input {...}` vs `given items = ...` and `then responds {...}` vs `respond with {...}` | agy | **Card 021**, rank 16, Decider on the grammar change first |
| PH326 (composite fails with an undeclared code) vs endpoints that have no `fails with` clause is unclear | agy | **Folded into card 019** |
| Add a failing-spec example (`fails with` + `then fails with`) | Copilot | **Folded into card 014** (examples) |
| A lone snippet needs `module` and `LineItem`, so it looks like extra errors (PH202, PH300) | Grok | **Rejected for now**: this is the language working as designed (whole-file units); a snippet mode would add a second entry point. Revisit if the benchmark shows snippet tasks failing for this reason |

Decider D2 ranked the three new cards: Codex Yes; Copilot Support-it; Grok Yes; Agy Yes; passed (see docs/decisions/DECIDERS.md).

## Experiments review

- E1 (true mobbing): not started yet; runs on the next card after 016.
- Mid-card task-force additions on this card: none.

## One process improvement (area: Organization)

Every background reviewer or voter writes to a result file and the facilitator checks it is non-empty before
relying on it (the silent empty Grok review cost about 40 minutes). Counter-metric: facilitator turns spent on
checking. Trial scope: the next 3 cards; the retro decides keep, revise or drop.
