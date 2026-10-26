# Benchmark: catalogue tasks that measure LLM work removed and learnability

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Vocabulary v1
- **North star:** `VISION.md`
- **Source:** Steve's purpose ruling: judge coverage by LLM work removed. Decider D3.
- **Story:** As the mob, I want benchmark tasks that measure how much work the vocabulary removes and whether a cold model finds the right keyword, so that 'comprehensive' is measured, not counted.
- **Hypothesis:** We believe tasks that compare the same job with and without the catalogue will show how much LLM work each keyword removes, and a find-and-use task will show whether it is learnable. We'll know when the suite reports, per keyword and per model, tokens and native lines written at equal pass rate, and the share of cold runs that find and use the right keyword, each against a stored baseline.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a task that no keyword covers, When run, Then the with-catalogue and without-catalogue results are the same and no reduction is claimed
  - One: Given one keyword and its task, When run with and without it, Then tokens written, native lines and pass rate are recorded for both
  - Many: Given the full catalogue and all four models, When run, Then results are stored in `bench/results/` for trend comparison
  - Boundaries: Given a holdout subset of catalogue tasks, When developing a keyword, Then the holdout is used only as the keep-or-revert check (Goodhart guard)
  - Interface: Given a cold model with only the docs, When asked for a job a keyword covers, Then the run records whether it found and used the keyword (learnability)
  - Exceptions: Given a model that is unavailable, When run, Then it is recorded `unavailable` and the others still run

## Context

- Extends cards 016/017; versioned suite, so a change needs a Decider and a MOB.md note and is never bundled with the keyword it measures.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled (mechanical checks that each criterion's evidence exists, plus the gate `dotnet test Phrase.sln --filter "Status!=Pending"`).
