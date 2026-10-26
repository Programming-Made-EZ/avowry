# Adapter contract and C# adapter: types and runtime

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1 (plumbing for bar 2-4; exempt from the hypothesis rule) (`board/MILESTONE-usable-v1.md`)
- **Story:** As an engineer (or an LLM) writing Phrase, I want a target-adapter contract and a C# adapter that emits my declared types and the phrase runtime, so that generated code has real, named types to build on and C# is the first adapter, not a one-off emitter.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a program with no types, When emitted, Then Types.cs and PhraseRuntime.cs are still produced and build
  - One: Given `Order` with three fields, When emitted, Then a C# record with the same field names and mapped types is produced (compare `reference/csharp/Orders.Api/Types.cs`)
  - Many: Given Option, List and nested record fields, When emitted, Then they map to nullable, `IReadOnlyList<T>` and records
  - Boundaries: Given a type named like a C# keyword, When emitted, Then it is escaped and still builds
  - Exceptions: Given a model with resolver diagnostics, When emitted, Then nothing is emitted and the diagnostics are returned
  - Interface (adapter contract, Decider D4): Given `ITargetProvider`, When this card lands, Then it is renamed `ITargetAdapter`, `Phrase.Providers.CSharp` becomes `Phrase.Adapters.CSharp`, and the contract is what a second adapter would implement, so a TypeScript adapter needs no change to the compiler

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

- **Before starting (team decision, not Steve's):** run a Decider on the stashed, unreviewed emitter start (the git stash named 'UNREVIEWED edits by a runaway Copilot VOTER job', about +580 lines in `CSharpProvider.cs` plus `ITargetProvider`/`PhraseCompiler` changes and a test-support tweak): salvage it as a reviewed starting point, or drop it. Keep the stash until then; do not apply, ship or delete it before the vote.

## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check that the card's new tests exist.
