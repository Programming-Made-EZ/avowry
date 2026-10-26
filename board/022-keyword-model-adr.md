# Keyword model ADR: what a keyword is

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1
- **North star:** `VISION.md`
- **Source:** Steve's purpose ruling (VISION.md); Decider D3.
- **Story:** As the language designer, I want one written definition of a keyword, so that the grammar, the catalogue and the providers agree on what we are building.
- **Hypothesis:** exempt: decision/plumbing card that unblocks the cards that carry hypotheses.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given GRAMMAR.md rule 3 ('small fixed core, open phrase library'), When the ADR is accepted, Then rule 3 is restated so that keywords are the standard-library phrases that ship with Phrase and the core stays small
  - One: Given one example keyword (for example 'paginate {items} by {size}'), When described, Then the ADR shows its sentence-name, typed holes, specs, and its C# implementation, and says what a provider must do to support it
  - Many: Given control flow (if/else, loops), When classified, Then the ADR says they are core constructs, not catalogue keywords, with the reason
  - Boundaries: Given a keyword with no hand-written provider implementation, When the target needs it, Then the ADR defines the LLM-produced path: pinned by hash, held to the same specs
  - Interface: Given the keyword model, When the grammar and IR are checked, Then the ADR lists the grammar/IR changes needed (if any) as follow-up cards, not as hidden work
  - Exceptions: Given a disagreement among the models, When the ADR is written, Then it goes through the Decider and the record is linked

## Context

- A short ADR in `docs/adr/0001-keyword-model.md`, written by the architect and the domain expert (language designer), reviewed by the task force via the Decider.
- Read first: `VISION.md`, `docs/GRAMMAR.md`, `reference/csharp`.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled (mechanical checks that each criterion's evidence exists, plus the gate `dotnet test Phrase.sln --filter "Status!=Pending"`).
