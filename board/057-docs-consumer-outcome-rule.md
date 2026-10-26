# Docs: the consumer-observable outcome rule is written down

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** docs (thin slice; exempt from the hypothesis rule)
- **Milestone:** Vocabulary v1 and Usable v1 plumbing (consumer-observable outcomes, VISION ruling 5)
- **Source:** Steve, 2026-10-26 (VISION ruling 5): "I feel like we aren't adhering to consumer related output. 'succeeds' isn't really a result the customer gets value from." Team review: Decider D20 (Codex Support-it, Copilot Support-it, Grok Yes; agy no response).
- **Story:** As a consumer of Phrase programs (an engineer, an LLM, or an end customer), I want a reader of the docs sees that a consumer-visible outcome is a returned value, a response or a declared failure, and that `succeeds` remains for a unit with nothing to return, so that specs state a result that has value to the consumer.
- **Done checks (at most 3, each failing on partial work):**
  1. VISION ruling 5 and GRAMMAR design rule 6 exist (written 2026-10-26), and the GRAMMAR diagnostics table has the PH336 row card 055 adds
  2. Card 014's criterion no longer requires `then succeeds` on every guard
  3. No source file changes; the repository gate passes with zero warnings

## Context

- Audit result for the record (visible tree, 2026-10-26): the only outcome-less specs are bench task 005 (card 054), the parser fixtures (kept as syntax coverage, not examples) and card 014's wording (this card); orders.phrase and reference/csharp have none; demo/tasks is not in the repo yet.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `VISION.md` (ruling 5), `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Task force

- Docs author [author]: Copilot | why: edits docs
- Reviewer [reviewer]: Grok | why: checks wording against the resolver

## Done when

All must exit 0, run from the repo root:

```
python -c "import sys;g=open('docs/GRAMMAR.md',encoding='utf-8').read();c=open('board/014-document-native-environment.md',encoding='utf-8').read();sys.exit(0 if 'PH336' in g and 'then succeeds' not in c else 1)"
dotnet test Phrase.sln --filter "Status!=Pending"
```
