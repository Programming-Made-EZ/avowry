# An endpoint with an output cannot say `succeeds` (PH333)

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** feature (thin slice; exempt from the hypothesis rule)
- **Milestone:** Vocabulary v1 and Usable v1 plumbing (consumer-observable outcomes, VISION ruling 5)
- **Source:** Steve, 2026-10-26 (VISION ruling 5): "I feel like we aren't adhering to consumer related output. 'succeeds' isn't really a result the customer gets value from." Team review: Decider D20 (Codex Support-it, Copilot Support-it, Grok Yes; agy no response).
- **Story:** As a consumer of Phrase programs (an engineer, an LLM, or an end customer), I want `then succeeds` on an endpoint that has an output reports PH333 naming the endpoint, while a no-output endpoint may still say `succeeds`, so that specs state a result that has value to the consumer.
- **Done checks (at most 3, each failing on partial work):**
  1. `then succeeds` on an endpoint with an output reports PH333 naming the real endpoint (trait Slice=056)
  2. A no-output endpoint that says `succeeds` resolves clean
  3. The repository gate passes with zero warnings

## Context

- A value-returning phrase that says `succeeds` is already PH333. `and succeeds` stays PH335 (a second outcome); not part of this card. Depends on 055 only for ordering.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `VISION.md` (ruling 5), `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Task force

- Backend author [author]: Codex | why: writes it test-first
- Reviewer [reviewer]: Copilot | why: independent review

## Done when

All must exit 0, run from the repo root:

```
python scripts/check-slice-tests.py 056 2
dotnet test Phrase.sln --filter "Status!=Pending"
```
