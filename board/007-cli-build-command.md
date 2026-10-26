# Phrase CLI: one command compiles a program

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1 (`board/MILESTONE-usable-v1.md`)
- **Story:** As an engineer (or an LLM), I want one command, `phrase build <file.phrase> -o <dir>`, so that I can compile without writing C# glue code.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a missing file, When run, Then exit code 2 and a clear message
  - One: Given `orders.phrase`, When run, Then the output dir has the API and Specs projects and exit code is 0
  - Many: Given a folder of several .phrase files, When run on the folder, Then all are compiled as one module
  - Boundaries: Given an output dir that already has generated files, When run, Then they are replaced and hand-written files are untouched
  - Interface: Given compile errors, When run, Then each is printed as `file:line:col PHnnn message` and exit code is 1
  - Exceptions: Given an unwritable output dir, When run, Then exit code 2 and no partial output

## Context

- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `HANDOFF.md`, the milestone doc and the previous card.
- `reference/csharp` is the target the emitter must match in behavior.
- Outside-in TDD with Given/When/Then comments; stage explicit paths only, never `.mob/`.

## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check that the card's new tests exist.
