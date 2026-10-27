# End-to-end: real HTTP behaves as the specs say

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Depends on:** 009
- **Milestone:** Usable v1 (plumbing for bar 5; exempt from the hypothesis rule) (`board/MILESTONE-usable-v1.md`)
- **Story:** As Dana, I want the generated app to run and respond over real HTTP as its specs say, so that I can trust the specified behavior is what consumers receive from the service.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given the app started on a free port, When an empty list is requested, Then it responds
  - One: Given a valid create request, When posted, Then the response status and body match the endpoint's spec
  - Many: Given every endpoint's specs replayed as HTTP calls, When run, Then every response matches
  - Boundaries: Given invalid input and an unknown id, When called, Then the declared failure statuses are returned
  - Exceptions: Given the app fails to start, When the test runs, Then it fails with the app's log in the message; the test stops only the process it started, by PID

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check that the card's new tests exist.
