# Behavioral holes and spec test doubles

- **Status:** Ready (rank: see `board/MILESTONE-usable-v1.md`)
- **Milestone:** Usable v1 (base library needs it)
- **North star:** `VISION.md`
- **Source:** Retro 016 model feedback: Codex, Copilot, Grok, agy (4 of 4). Decider D6.
- **Depends on:** 033
- **Story:** As Dana, I want keywords such as retry, transaction and idempotent-execute to accept an action that specs can control, so that I can rely on their effectful behavior across target languages.
- **Hypothesis:** We believe behavioral holes plus spec test doubles will make retry-style keywords expressible in the base library for LLM authors. We'll know when the retry, pagination and idempotent-execute seed keywords pass identical specs on the C# adapter without a native block in the keyword's definition, and native lines per task on the catalogue tasks fall from the baseline.
- **Acceptance criteria (ZOMBIES):**
  - Zero: Given a keyword with no behavioral hole, When resolved, Then behavior is unchanged
  - One: Given `retry {action: Action<T>} up to {attempts: Int} times`, When specified with `given action fails 2 times then returns 7`, Then the spec runs as a test and passes with 3 calls to the action
  - Many: Given delays and calls as observable events, When asserted (`then calls action 3 times`), Then the generated test checks them without real waiting
  - Boundaries: Given an action that always fails, When retried, Then the keyword's declared failure is returned after the last attempt
  - Interface: Given the C# adapter, When emitted, Then the action is a delegate and the test double is generated; the shared specs would run unchanged on a second adapter
  - Exceptions: Given a spec that scripts an impossible sequence, When resolved, Then one diagnostic explains it and PH332 (specs use literal data) still holds for ordinary values

## Context

- Needs generic type parameters (033) and the adapter contract (003). Grammar change: Decider with the benchmark baseline recorded first.

## Task force

Assembled at pull time by the facilitator (a Decider proposal; see `TEAM_WORKING_AGREEMENT.md`).

## Done when

The facilitator writes concrete, red-first commands when the card is pulled, including one negative check that the criteria cannot be satisfied by fixtures or fake data.
