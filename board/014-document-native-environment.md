# Document the native C# environment and show passing-guard examples

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1
- **Source:** Retro 001 model feedback: Grok and agy (native `Fail` vs `PhraseFailure`), Codex, Copilot and Grok (no passing-guard / `succeeds` example).
- **Story:** As an LLM writing a leaf phrase, I want to know exactly what a `native csharp` block can call and how types map, so that I do not have to reverse-engineer it from the fixture.
- **Hypothesis:** We believe documenting the native C# environment (`Fail`, type mapping) and showing passing-guard examples will raise first-attempt pass rate on leaf-phrase write and fix tasks for LLM authors. We'll know when that pass rate moves from the card-016 baseline to baseline plus 15 points on bench suite v1.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a native block, When the docs are read, Then the one failure form is stated (`Fail("Code")`) and its relation to `PhraseFailure` (line 207) is explained in one sentence
  - One: Given a `List<LineItem>` hole, When used in a native block, Then the docs show its C# type and the names of the hole variables in scope
  - Many: Given `orders.phrase`, When read, Then every guard has a passing spec (a spec with an observable outcome: `returns`, `responds` or `fails with`) and a failing one, and there is a value-returning phrase with a numeric spec (for example an average)
  - Boundaries: Given a failure code, When used in `fails with X` and in `Fail("X")`, Then the docs say the two must match and the resolver reports a mismatch if it can detect one (otherwise a card is filed)
  - Exceptions: Given a typed-constant form for failure codes (`Fail(Failure.EmptyOrder)`, agy), When considered, Then the decision is recorded here: deferred to emitter card 004, which can generate the constants without a grammar change

## Context

- Record the baseline from `bench/results/` before starting, and re-run the benchmark after; keep, revise or revert by the result and note why in MOB.md.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `tests/Phrase.Acceptance.Tests/Fixtures/orders.phrase`.
- Stage explicit paths only, never `.mob/`. Grammar changes need a recorded reason (Decider protocol if models disagree).


## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check of the doc change.
