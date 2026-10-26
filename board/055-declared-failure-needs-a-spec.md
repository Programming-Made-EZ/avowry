# A declared failure with no failure-path spec is an error (PH336)

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** feature (thin slice; exempt from the hypothesis rule)
- **Milestone:** Vocabulary v1 and Usable v1 plumbing (consumer-observable outcomes, VISION ruling 5)
- **Source:** Steve, 2026-10-26 (VISION ruling 5): "I feel like we aren't adhering to consumer related output. 'succeeds' isn't really a result the customer gets value from." Team review: Decider D20 (Codex Support-it, Copilot Support-it, Grok Yes; agy no response).
- **Depends on:** 054, 018
- **Story:** As a consumer of Phrase programs (an engineer, an LLM, or an end customer), I want resolving a phrase whose `fails with` code has no `then fails with` spec on that phrase reports PH336, naming the phrase and the code, so that specs state a result that has value to the consumer.
- **Done checks (at most 3, each failing on partial work):**
  1. The MissingName shape (only `then succeeds`) reports PH336 (trait Slice=055); a phrase with two codes and one failure spec reports only the missing code
  2. orders.phrase still resolves clean; the test prelude's guards and helpers each gain a covering failure spec so exact-code tests stay about their own codes
  3. The repository gate passes with zero warnings

## Context

- Error, not warning: the compiler has no warning channel (Diagnostic has no severity, and StopOnDiagnostics rejects every diagnostic), so a warning would be a second behaviour (Grok, D20). Propagated endpoint and composite failures stay out of this rule. Depends on card 054 (the benchmark source must carry a failure spec first).
- Read first: `TEAM_WORKING_AGREEMENT.md`, `VISION.md` (ruling 5), `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Task force

- Backend author [author]: Codex | why: writes the rule test-first
- Reviewer [reviewer]: Copilot | why: independent review
- Language designer: Grok | why: checks the PH number and message against GRAMMAR.md

## Done when

All must exit 0, run from the repo root:

```
python scripts/check-slice-tests.py 055 2
dotnet test Phrase.sln --filter "Status!=Pending"
```
