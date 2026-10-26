# Make GRAMMAR.md say what the resolver does

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1
- **Source:** Retro 001 model feedback: Codex and agy (bare-word rule), Grok (`and` rule).
- **Depends on:** none
- **Story:** As an LLM writing Phrase, I want the EBNF and the prose to agree, so that I never have to guess which one is right.
- **Hypothesis:** We believe making the EBNF and prose agree will cut grammar-misread errors for LLM authors/readers. We'll know when first-attempt diagnostics per write task on the write tasks of bench suite v1 move from the card-016 baseline to at most half of it, for at least three of four models.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given the `argument` rule in the EBNF, When read, Then it includes a bare word naming a variable in scope, matching Section 4 (today it omits it)
  - One: Given `ensure input.items is not empty`, When read against the grammar, Then it is clear whether `input.items` is an argument or a bare word, with one worked example
  - Many: Given a spec with two hole bindings, When read, Then `and` is shown as adding bindings only, and a second outcome is a second `spec` (resolve line 112 vs the PH335 rule one way and make both places say it)
  - Boundaries: Given every EBNF rule, When the test reads GRAMMAR.md, Then a mechanical check confirms the rules named here match the parser's accepted forms (add parser tests for each)
  - Exceptions: Given a program that follows only the docs, When parsed, Then a docs/parser disagreement fails a test instead of reaching a user

## Context

- Record the baseline from `bench/results/` before starting, and re-run the benchmark after; keep, revise or revert by the result and note why in MOB.md.
- Read first: `TEAM_WORKING_AGREEMENT.md`, `docs/GRAMMAR.md`, `tests/Phrase.Acceptance.Tests/Fixtures/orders.phrase`.
- Stage explicit paths only, never `.mob/`. Grammar changes need a recorded reason (Decider protocol if models disagree).


## Done when

The PM writes the concrete commands at the Ready check. The gate is `dotnet test Phrase.sln --filter "Status!=Pending"` with zero warnings, plus a mechanical check of the doc change.
