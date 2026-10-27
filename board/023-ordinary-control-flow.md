# Ordinary control flow: if/else and loops

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1
- **North star:** `VISION.md`
- **Source:** Steve's purpose ruling: loops and if/else are ordinary. Decider D3.
- **Depends on:** 022, 050
- **Story:** As Ari, I want plain, obvious constructs for if/else and loops, so that I can express ordinary logic without inventing native code.
- **Hypothesis:** We believe adding plain if/else and for-each to the core will remove native-code lines and attempts-to-green on modify and write tasks for LLM authors. We'll know when native-code lines per task on the benchmark's write tasks that need branching or iteration move from the card-016/017 baseline to at most half of it, with pass rate unchanged and the construct count in the grammar growing by no more than 2.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given an empty list, When a for-each step runs, Then the body runs zero times and the specs say so
  - One: Given a condition, When an if/else step runs, Then exactly one branch runs and the resolver type-checks the condition as Bool
  - Many: Given nested if/else inside a for-each, When resolved and emitted, Then scope, failures and effects propagate correctly (PH324 reachability, PH327 cycles still hold)
  - Boundaries: Given a loop that builds a value (sum, filter, map), When written, Then it has one obvious form (no second way to loop)
  - Interface: Given the C# adapter, When emitted, Then the output is idiomatic and the same specs pass
  - Exceptions: Given a malformed construct, When parsed or resolved, Then a single diagnostic with the valid forms is reported and the resolver never throws

## Context

- Depends on the emitter (cards 003-006) and the keyword model ADR (022). Parser, resolver and emitter slices, one cycle each.
- Goodhart: judged by native lines removed on the benchmark, not by construct count.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled (mechanical checks that each criterion's evidence exists, plus the gate `dotnet test Phrase.sln --filter "Status!=Pending"`).
