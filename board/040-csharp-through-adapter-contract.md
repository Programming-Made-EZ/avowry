# C# compilation goes through the ITargetAdapter contract

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** refactor (thin slice; exempt from the hypothesis rule)
- **Milestone:** Usable v1 (plumbing for bar 2 to 4) (`board/MILESTONE-usable-v1.md`)
- **Source:** Process retro 2026-10-26 (Decider D18): thin slices, trialled on the C# adapter chain; this card replaces part of old cards 003 to 007. Steve: "I do have a concern the slices are too big."
- **Depends on:** 039
- **Story:** As an engineer (or an LLM) writing Phrase, I want choosing the csharp target produces the same working number-phrase program through ITargetAdapter, with no C#-only hook in the compiler core (Decider D4), so that the Phrase compiler grows one observable behaviour at a time.
- **One observable behaviour:** Choosing the csharp target produces the same working number-phrase program through ITargetAdapter, with no C#-only hook in the compiler core (Decider D4).
- **Done checks (at most 3, each failing on partial work):**
  1. ITargetProvider becomes ITargetAdapter and Phrase.Providers.CSharp becomes Phrase.Adapters.CSharp; the 039 acceptance test passes unchanged
  2. A compiler-core test with a substitute adapter proves the core receives and hands over the resolved model without C# knowledge
  3. The repository gate passes with zero warnings
- **Size rule (the hypothesis under trial):** fits one run of about 3 cycles. If it will not, split it before starting. Runs and cap hits are recorded for the retro.

## Context

- Lands immediately after 039, never as a big-bang rename. Docs naming the adapter are their own card (046).
- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`. Tests carry the trait `Slice=040`.

## Task force

- Backend author [author]: Codex | why: writes the slice test-first and the code
- Reviewer [reviewer]: Copilot | why: independent review and sign-off; not the author
- Language designer: Grok | why: checks the slice against GRAMMAR.md and the adapter contract

## Done when

All must exit 0, run from the repo root:

```
python scripts/check-slice-tests.py 040 1
dotnet test Phrase.sln --filter "Status!=Pending"
```

The first command is `scripts/check-slice-tests.py`: it fails unless at least one test carries this card's `Slice` trait, so a card cannot pass on the gate alone.
