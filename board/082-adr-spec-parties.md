# ADR: a spec party is who observes an outcome

- **Status:** Ready (rank: see `C:\Users\ringe\projects\phrase-lang\board\MILESTONE-usable-v1.md`, read-only)
- **Shape:** Story
- **Type:** decision; thin slice; authored by the driver rotation, reviewed by a non-author (D15); smoke-tested before it touches a real card
- **Milestone:** Vocabulary v1: spec parties (a hypothesis, bootstrap phase)
- **Source:** Steve and Dispatch, 2026-10-26 (proposal B): named actor types in the language; every then names the party and what it observes, which makes then-succeeds unwritable and maps each party to a standard test double. Team review: Decider D29 (Codex Support-it, Copilot Yes, Grok Support-it; agy no response). Bootstrap phase: a hypothesis to test with a spike and LLM-customer evidence, not a feature.
- **Depends on:** 022, 058, 059, 081
- **Story:** As Morgan, I want one written decision on named parties in specs: the name (party), the closed catalogue, the then forms and what each adapter must provide, so that I can read any spec and tell who observes the outcome and what a test double for that party must record.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given the tests with fakes and fixtures, When the behaviour above is exercised, Then docs/decisions/ADR-spec-parties.md names the concept (party; observer and actor were rejected: actor collides with the concurrency model, observer is already the event-handler word), the closed catalogue with each party's test double and adapter obligation (caller, client, store, events as the core candidates; clock and scheduler stay declared effects and spec givens; external service only by an adapter-added Decider), and records the dissent on seven versus four parties
  2. Given the tests with fakes and fixtures, When the behaviour above is exercised, Then it keeps fails with and the D20 succeeds remainder (no return type, no endpoint output, effects none) and records the unranked grammar seed that may later replace returns and responds with party sentences in one step, with no alias and no second spelling
  3. Given the tests with fakes and fixtures, When the behaviour above is exercised, Then a Decider approves it, informed by the measure card's results (the bootstrap phase: the numbers inform, they do not keep or revert anything)

## Context

- Safety rules apply in full (never delete, default dry run, only the Phrase repo, no other mob). Bootstrap phase: no metric-based rejection until a Decider declares the baseline.
- Phrase board and docs (read-only, absolute paths): `C:\Users\ringe\projects\phrase-lang\board\`, `C:\Users\ringe\projects\phrase-lang\PERSONAS.md`, `C:\Users\ringe\projects\phrase-lang\VISION.md`, `C:\Users\ringe\projects\phrase-lang\docs\decisions\DECIDERS.md`.

## Task force

- Author [author]: Grok | why: next in the rotation (D15)
- Reviewer [reviewer]: Copilot | why: non-author review of the diff (D15)
- Language designer: Grok | why: checks the work against GRAMMAR.md, 022 and the outcome rule
- Facilitator: Claude | why: runs the checks and any real apply after review; takeovers are logged

## Done when

All must exit 0, run from the phrase-lang repo root:

```
python -c "import sys;t=open('docs/decisions/ADR-spec-parties.md',encoding='utf-8').read();sys.exit(0 if all(w in t for w in ('party','caller','client','store','events','clock','scheduler','adapter')) else 1)"
python scripts/check-decider-recorded.py ADR-spec-parties
```
