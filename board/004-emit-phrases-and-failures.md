# C# adapter: phrases, effects and failures

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1 (plumbing for bar 2-4; exempt from the hypothesis rule) (`board/MILESTONE-usable-v1.md`)
- **Story:** As an engineer (or an LLM), I want each phrase emitted as a C# method with typed failures and declared effects, so that the library of behavior is callable from endpoints.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a leaf phrase with no failures, When emitted, Then a method with the right signature is produced
  - One: Given the `orders.phrase` phrases, When emitted, Then Phrases.cs matches `reference/csharp/Orders.Api/Phrases.cs` in behavior (its generated specs pass)
  - Many: Given a composite calling three phrases, When emitted, Then calls run in order and the union of failures is surfaced
  - Boundaries: Given a phrase with a native C# block, When emitted, Then the block is placed verbatim and builds
  - Interface: Given an effect (Db), When emitted, Then the method takes the declared dependency by interface
  - Exceptions: Given a failure code, When the phrase fails, Then the typed failure result carries that code
  - Interface (adapter contract, Decider D4): Given the adapter contract from card 003, When this card lands, Then emission goes through it and adds no C#-only hook to the compiler core

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check that the card's new tests exist.
