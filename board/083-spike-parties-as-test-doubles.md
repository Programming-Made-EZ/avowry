# Spike: an in-memory store and an event capture assert one scripted observation

- **Status:** Ready (rank: see `C:\Users\ringe\projects\phrase-lang\board\MILESTONE-usable-v1.md`, read-only)
- **Shape:** Story
- **Type:** spike; thin slice; authored by the driver rotation, reviewed by a non-author (D15); smoke-tested before it touches a real card
- **Milestone:** Vocabulary v1: spec parties (a hypothesis, bootstrap phase)
- **Source:** Steve and Dispatch, 2026-10-26 (proposal B): named actor types in the language; every then names the party and what it observes, which makes then-succeeds unwritable and maps each party to a standard test double. Team review: Decider D29 (Codex Support-it, Copilot Yes, Grok Support-it; agy no response). Bootstrap phase: a hypothesis to test with a spike and LLM-customer evidence, not a feature.
- **Depends on:** 082, 034, 047, 061
- **Story:** As Riley, I want a C# spec that supplies an in-memory store and an event capture as parties and asserts one scripted observation, with the same trace every run, so that I can see a party map to a real test double and give a deterministic result with no real infrastructure.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. Given the tests with fakes and fixtures, When the behaviour above is exercised, Then the generated test supplies the store and event doubles and asserts the stored record and the published event (trait Slice=083); no real I/O
  2. Given the tests with fakes and fixtures, When the behaviour above is exercised, Then twenty consecutive runs give the identical trace
  3. Given the tests with fakes and fixtures, When the behaviour above is exercised, Then gaps found are filed as cards and the repository gate passes with zero warnings

## Context

- Safety rules apply in full (never delete, default dry run, only the Phrase repo, no other mob). Bootstrap phase: no metric-based rejection until a Decider declares the baseline.
- Phrase board and docs (read-only, absolute paths): `C:\Users\ringe\projects\phrase-lang\board\`, `C:\Users\ringe\projects\phrase-lang\PERSONAS.md`, `C:\Users\ringe\projects\phrase-lang\VISION.md`, `C:\Users\ringe\projects\phrase-lang\docs\decisions\DECIDERS.md`.

## Task force

- Author [author]: Codex | why: next in the rotation (D15)
- Reviewer [reviewer]: Copilot | why: non-author review of the diff (D15)
- Language designer: Grok | why: checks the work against GRAMMAR.md, 022 and the outcome rule
- Facilitator: Claude | why: runs the checks and any real apply after review; takeovers are logged

## Done when

All must exit 0, run from the phrase-lang repo root:

```
python scripts/check-slice-tests.py 083 2
dotnet test Phrase.sln --filter "Status!=Pending"
```
