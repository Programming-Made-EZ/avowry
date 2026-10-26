# A validation phrase returns its validated value (first slice, language decision first)

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** feature (thin slice; exempt from the hypothesis rule)
- **Milestone:** Vocabulary v1 and Usable v1 plumbing (consumer-observable outcomes, VISION ruling 5)
- **Source:** Steve, 2026-10-26 (VISION ruling 5): "I feel like we aren't adhering to consumer related output. 'succeeds' isn't really a result the customer gets value from." Team review: Decider D20 (Codex Support-it, Copilot Support-it, Grok Yes; agy no response).
- **Depends on:** 022, 055
- **Story:** As a consumer of Phrase programs (an engineer, an LLM, or an end customer), I want a validation phrase such as `ensure {name: Text} has a value` returns the validated Text on success and fails with its declared code on invalid input, so its spec asserts a value instead of `succeeds`, so that specs state a result that has value to the consumer.
- **Done checks (at most 3, each failing on partial work):**
  1. A Decider records the contract (validation phrases return the validated or normalized value; existing `ensure` phrases are migrated deliberately, not silently)
  2. One valid-input spec asserts `then returns "Ada"` and one invalid-input spec asserts `then fails with MissingName`, resolved and, once card 039 is done, generated and passing (trait Slice=058)
  3. The repository gate passes with zero warnings

## Context

- Steve's question (c): should validation-only phrases return the validated or normalized value? Team answer (D20): yes (Codex, Copilot). Establish the contract through the keyword-model decision (card 022), then do this first slice. Not every existing `ensure` is changed silently.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `VISION.md` (ruling 5), `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Task force

- Language designer [author]: Grok | why: owns the contract wording and the resolver rule
- Backend author: Codex | why: implements the first slice test-first
- Reviewer [reviewer]: Copilot | why: independent review

## Done when

All must exit 0, run from the repo root:

```
python scripts/check-slice-tests.py 058 2
dotnet test Phrase.sln --filter "Status!=Pending"
```
