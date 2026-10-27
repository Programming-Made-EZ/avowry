# Pattern discovery: find the recurring patterns worth a keyword

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Vocabulary v1
- **North star:** `VISION.md`
- **Source:** Steve's purpose ruling: a comprehensive vocabulary found by evidence. Decider D3.
- **Story:** As Riley, I want a repeatable way to find recurring patterns in real codebases and LLM task logs, so that I can propose vocabulary that removes demonstrated work from real tasks.
- **Hypothesis:** We believe mining real code and LLM tasks will find patterns whose keywords remove more LLM work than hand-picked ones. We'll know when the top 10 mined candidates, turned into keywords, cut tokens written per task on the catalogue benchmark (card 027) by more than the same number of hand-picked keywords do, with the near-duplicate rate (card 026) not higher.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a source with no recurring pattern, When mined, Then the report says so rather than inventing candidates
  - One: Given one real repository, When mined, Then the output lists candidate patterns with the evidence (file, line, frequency) and a proposed sentence-name
  - Many: Given at least 5 repositories and a set of LLM task transcripts (the benchmark tasks and mob logs), When mined, Then candidates are ranked by frequency times the LLM work they would remove
  - Boundaries: Given two candidates that do the same thing, When ranked, Then they are merged and the merge is recorded
  - Interface: Given the candidate list, When the PM reads it, Then each candidate is ready to become a catalogue card (name, holes, example, specs sketch)
  - Exceptions: Given an unreadable repository, When mined, Then it is skipped with a logged reason

## Context

- A script plus a short method note in `docs/pattern-discovery.md`; results in `catalogue/candidates/`.
- Metric and counter-metric are in the hypothesis; the method is itself an experiment the retro reviews.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled (mechanical checks that each criterion's evidence exists, plus the gate `dotnet test Phrase.sln --filter "Status!=Pending"`).
