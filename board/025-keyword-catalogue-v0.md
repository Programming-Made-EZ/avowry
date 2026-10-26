# Base library v0: structure and a seed of about 12 keywords, with C# adapter implementations

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1
- **North star:** `VISION.md`
- **Source:** Steve's purpose ruling: the catalogue is the library. Decider D3.
- **Story:** As an LLM building an app in Phrase, I want a base library of universal patterns with spec-backed definitions, implemented by the C# adapter, so that I reuse a keyword instead of writing the pattern.
- **Hypothesis:** We believe a seed catalogue of common patterns will cut native-code lines and attempts-to-green on write tasks for LLM authors. We'll know when native-code lines per write task on the demo-app tasks move from the card-016/017 baseline to at most 60% of it, with pass rate unchanged and no two seed keywords flagged as near-duplicates.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given the catalogue directory, When listed, Then every keyword has a name sentence, typed holes, specs, a C# implementation and a one-line description, and an empty catalogue is valid
  - One: Given one keyword (for example 'paginate {items} by {size}'), When used in a program, Then it resolves, emits and its specs pass as tests
  - Many: Given about 12 seed keywords drawn from CRUD, validation, pagination, retry and list handling, When the catalogue is built, Then all their specs pass on the C# target
  - Boundaries: Given a keyword used in a program and also defined locally, When resolved, Then PH002 (ambiguous) or PH010 (duplicate) applies as for any phrase
  - Interface: Given the demo app (card 009), When written, Then it uses catalogue keywords for the patterns the catalogue covers and native blocks only for the rest
  - Exceptions: Given a keyword whose C# implementation fails its specs, When the catalogue is checked, Then the gate fails naming the keyword
  - Interface (base library and adapters, Decider D4): Given the base library, When the C# adapter is built, Then it implements every base keyword and the shared specs verify each one; adapter-specific keywords (for example FastEndpoints) live in the adapter, spec-backed, and are not in the base library

## Context

- Depends on the keyword model ADR (022) and the emitter (003-006). Layout under `catalogue/`; seeds chosen from card 024's candidates where available, else from the demo app's needs, recorded with reasons.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled (mechanical checks that each criterion's evidence exists, plus the gate `dotnet test Phrase.sln --filter "Status!=Pending"`).
