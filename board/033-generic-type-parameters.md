# Generic type parameters on phrases

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1 (base library needs it)
- **North star:** `VISION.md`
- **Source:** Retro 016 model feedback: Codex, Copilot, Grok, agy (4 of 4). Decider D6.
- **Story:** As Ari, I want reusable phrases to accept type parameters such as `phrase<T>`, so that I can apply one known keyword to different domain types without cloning it.
- **Hypothesis:** We believe type parameters will let one base keyword replace per-domain clones for LLM authors. We'll know when the number of near-duplicate phrase clones in the fixtures and seed library (the dedup count) drops to zero for the guards covered, and lines written per write task on the benchmark's validation tasks fall from the baseline, with no rise in resolver diagnostics per attempt.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a phrase with no type parameter, When resolved, Then behavior is unchanged and every existing test passes
  - One: Given `phrase<T> "ensure {items: List<T>} is not empty"`, When called with `List<LineItem>` and `List<Text>`, Then it resolves for both, T is inferred from the argument, and PH010 does not fire for the two uses
  - Many: Given Option<T> and List<T> nested in holes and return types, When resolved, Then T is bound consistently across the template
  - Boundaries: Given a type parameter that cannot be inferred, When resolved, Then one diagnostic names the parameter and the expected shapes
  - Interface: Given the C# adapter, When emitted, Then generic methods are produced and the same specs pass for two instantiations
  - Exceptions: Given a malformed type parameter list, When parsed, Then a single diagnostic is reported and the resolver never throws

## Context

- Needs the adapter contract (003) and the keyword model ADR (022). Changes the grammar: it goes through the Decider with the benchmark baseline recorded first (card 032).

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled, including one negative check that the criteria cannot be satisfied by fixtures or fake data.
