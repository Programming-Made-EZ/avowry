# Card 230 task-force declaration proposal (PM)

This card is the same-checkout bug: two writers on one checkout, index-lock collisions, push races, a facilitator commit that swept a seat's staged file, and a facilitator HEAD move under a live Decider voter. Git log shows the current writing pattern clearly: the facilitator writes `In-flight: ...` lines while seats write `Card ...` commit bodies; the bug is the edge when they share one checkout.

## Task force
- Product manager (ready): Copilot, ctx-1. Why: queue owner and proposal owner; keeps the card bounded and records the ready gate.
- Architect (slice): Codex, ctx-2. Why: owns the mechanism choice and the red/green premise proof before any window.
- QA engineer (red): Claude, ctx-3. Why: writes the deterministic red for the collision and push-race path, plus the partial-work checks.
- Backend developer (green): Codex, ctx-4. Why: owns the write-isolation fix and the helper/integration logic that prevents shared-checkout writes.
- Audit lens / independent reviewer: Copilot, ctx-5. Why: does the git-level review of the shared-checkout invariants and the cap check.
- Decider / facilitator: Claude, ctx-6. Why: owns the final decision, the integration step, and the readonly real-tree check while a seat is voting.
- Observer only: agile coach, not counted in implementation windows; retro policy keeps it out of counted windows unless a new rule says otherwise.

Placement facts: Copilot, Codex and Claude may sit counted decision seats. Grok stays out of counted windows until card 291; the backends file disables Grok for this card. That is a process rule, not a design choice.

Left out: UX, frontend, database, technical writer, performance, language design, and DevOps. This card is not about product UX or compiler rules; it is about Git-write isolation and voting integrity.

## Domain-expert T2 question
What proof is enough that each seat writes in an isolated checkout, the facilitator never sweeps a staged file, and a live Decider vote never sees a facilitator `HEAD` move?

## Value
The mob can safely run multiple seat writers without sharing a single checkout. The card's value is that no seat or facilitator can silently overwrite, push over, or steal one another's staged index state.

Metric: 0 same-checkout collision events in the red harness; 0 push-race events between seat and facilitator.
Counter-metric: false positives from a write-isolation guard, or a lock that blocks all work even when there is no collision.

## Done checks and caps
Cap total: 1 + 1 + 1 = 3.

1. A red reproduces a same-checkout collision deterministically.
   - Required: the red reproduces both an index-lock collision and a push race with no sleeps, no timing races, and no retry loops. Flaky tests are bugs here.
   - How: the red harness uses a fixed repo state, a staged file, and a controlled integration sequence; it asserts the collision in the same order every run.
   - Partial-work failure: it covers only the lock or only the push race, or relies on sleep-based timing instead of a deterministic order.

2. No commit includes another writer's staged file.
   - Required: a seat's staged state is preserved; the facilitator integration path or helper commit cannot sweep or include that staged file.
   - Partial-work failure: the fix prevents one collision but still includes a sibling's file in the merged commit, or only blocks the lock but not the index sweep.

3. A Decider vote never sees a facilitator commit in the real tree.
   - Required: a live vote sees stable `HEAD` from start to finish; no facilitator commit or push is interleaved during the vote.
   - Partial-work failure: the fix handles a lock collision but not a real-tree Decider case, or it checks only a mocked tree.

## Applicable decisions
- D175: cap lines and a window before any red past a cap; this card's cap total is three.
- D181: any lettered red ends at the first passing green; after a green, the next red is a new cycle and counts against the cap.
- D184: each scope premise must be run in code on HEAD before the window; we must test the mechanism choice on the actual repo state.
- D185: re-rank 21 put card 230 next after card 290; this is the ordering we follow.
- D109: decision seats are by role, not model; the log names which model occupied each countable seat.
- D216, D227 and the board's card 230 itself: they define the default one-writer rule and the push/scan safety baseline.
- D186 is not adopted, so the measure-only premise probe is not used in this card's slice.

## Open points
- Mechanism choice: prefer seat worktrees with the facilitator integrating, not a shared checkout. A runner-owned lock is a fallback only if worktrees cannot be used; it is weaker because the commit still lives in a single shared checkout.
- Where it lives: the rule belongs in both places: mob-defaults defines the default process contract, and avowry scripts enforce it in the actual repository workflow. Doing it in one repo only is too easy to bypass.
- Out of scope: product features, UX, schemas, language changes, unrelated audits, and any broad cleanup. This card is not a general Git discipline rewrite; it is the write-isolation guarantee the mob needs for a safe card run.

## Evidence to preserve
The current git pattern is the rule to preserve: the facilitator writes `In-flight: ...` lines, and the seats write `Card ...` commit messages. The fix should make those two writer classes mutually exclusive on a single checkout, and the task-force record should state which model filled each counted seat and which context each seat used.
