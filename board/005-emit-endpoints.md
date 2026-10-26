# C# adapter: FastEndpoints endpoints and Program

- **Status:** SUPERSEDED 2026-10-26 by thin cards 039 to 052 (process retro, Decider D18). Kept as the source of the acceptance cases; do not start this card.
- **Milestone:** Usable v1 (plumbing for bar 2-5; exempt from the hypothesis rule) (`board/MILESTONE-usable-v1.md`)
- **Story:** As an engineer (or an LLM), I want each endpoint emitted as a FastEndpoints class with its route, input, output and failure responses, so that the generated app serves real HTTP.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given no endpoints, When emitted, Then Program.cs builds and the app starts with no routes
  - One: Given `POST /orders`, When emitted, Then a FastEndpoints endpoint with the verb, route, input and output types is produced (compare `reference/csharp/Orders.Api/CreateOrder.cs`)
  - Many: Given 3-5 endpoints with different verbs, When emitted, Then each has its own class and route
  - Boundaries: Given route parameters and an endpoint with no output, When emitted, Then they bind and respond correctly
  - Interface: Given a `respond with` step and a `fail with` step, When called, Then HTTP statuses match the language's mapping, documented in GRAMMAR.md
  - Exceptions: Given invalid input, When called, Then the declared failure is returned, not a 500
  - Interface (adapter contract, Decider D4): Given the adapter contract from card 003, When this card lands, Then emission goes through it and adds no C#-only hook to the compiler core

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check that the card's new tests exist.
