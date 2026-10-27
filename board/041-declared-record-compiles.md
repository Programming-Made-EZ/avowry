# A declared record becomes a usable C# record

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** feature (thin slice; exempt from the hypothesis rule)
- **Milestone:** Usable v1 (plumbing for bar 2 to 4) (`board/MILESTONE-usable-v1.md`)
- **Source:** Process retro 2026-10-26 (Decider D18): thin slices, trialled on the C# adapter chain; this card replaces part of old cards 003 to 007. Steve: "I do have a concern the slices are too big."
- **Depends on:** 040
- **Story:** As Ari, I want a phrase with a declared record input to compile to C# and pass a spec that constructs the record, so that I can use domain-shaped input in working generated behavior.
- **One observable behaviour:** A phrase that takes a declared record input compiles and its spec constructs that record and passes; PhraseRuntime is emitted even when nothing fails.
- **Done checks (at most 3, each failing on partial work):**
  1. Types.cs contains the record with the declared field names and mapped types (compare reference/csharp Types.cs)
  2. A generated record-based spec passes (trait Slice=041)
  3. The repository gate passes with zero warnings
- **Size rule (the hypothesis under trial):** fits one run of about 3 cycles. If it will not, split it before starting. Runs and cap hits are recorded for the retro.

## Context

- Zero declared types still builds (the Zero case from old card 003 is a test here).
- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`. Tests carry the trait `Slice=041`.

## Task force

- Backend author [author]: Codex | why: writes the slice test-first and the code
- Reviewer [reviewer]: Copilot | why: independent review and sign-off; not the author
- Language designer: Grok | why: checks the slice against GRAMMAR.md and the adapter contract

## Done when

All must exit 0, run from the repo root:

```
python scripts/check-slice-tests.py 041 1
dotnet test Phrase.sln --filter "Status!=Pending"
```

The first command is `scripts/check-slice-tests.py`: it fails unless at least one test carries this card's `Slice` trait, so a card cannot pass on the gate alone.
