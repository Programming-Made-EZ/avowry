# One phrase with one number input compiles to C# and its spec passes

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Type:** feature (thin slice; exempt from the hypothesis rule)
- **Milestone:** Usable v1 (plumbing for bar 2 to 4) (`board/MILESTONE-usable-v1.md`)
- **Source:** Process retro 2026-10-26 (Decider D18): thin slices, trialled on the C# adapter chain; this card replaces part of old cards 003 to 007. Steve: "I do have a concern the slices are too big."
- **Story:** As an engineer (or an LLM) writing Phrase, I want a phrase with a single Int input and one Given/When/Then spec compiles to a C# API project and a Specs project, and the one generated xUnit test passes, so that the Phrase compiler grows one observable behaviour at a time.
- **One observable behaviour:** A phrase with a single Int input and one Given/When/Then spec compiles to a C# API project and a Specs project, and the one generated xUnit test passes.
- **Done checks (at most 3, each failing on partial work):**
  1. A new acceptance test (trait Slice=039) compiles a one-phrase source to buildable API and Specs projects in a temp folder
  2. Exactly one generated spec runs and passes
  3. The repository gate passes with zero warnings
- **Size rule (the hypothesis under trial):** fits one run of about 3 cycles. If it will not, split it before starting. Runs and cap hits are recorded for the retro.

## Context

- First slice of the thin C# chain. Deliberately tiny: no records, no failures, no endpoints, no adapter rename. Start with the stash Decider (salvage or drop the unreviewed emitter start) before any code.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`. Tests carry the trait `Slice=039`.

## Task force

- Backend author [author]: Codex | why: writes the slice test-first and the code
- Reviewer [reviewer]: Copilot | why: independent review and sign-off; not the author
- Language designer: Grok | why: checks the slice against GRAMMAR.md and the adapter contract

## Done when

All must exit 0, run from the repo root:

```
python scripts/check-slice-tests.py 039 1
dotnet test Phrase.sln --filter "Status!=Pending"
```

The first command is `scripts/check-slice-tests.py`: it fails unless at least one test carries this card's `Slice` trait, so a card cannot pass on the gate alone.
