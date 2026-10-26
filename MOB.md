**Usable v1: 0 of 6 criteria · next card: Loop: pick the next eligible card from the board (063)**

# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Loop: pick the next eligible card from the board (063)

## Who did what

- **Codex:** 4 turns (green x1, ready x1, refactor x1, slice x1)
- **Copilot:** 1 turns (red x1)

## Recent turns

- Turn 1: Codex (product-manager, ready): READY — The card has a valid Connextra story, three sound Given/When/Then acceptance criteria, and all referenced project guidelines and source files…
- Turn 2: Codex (architect, slice): THIN SLICE — it delivers one end-to-end tooling behavior: given the ranked board and card/run files, return the single card the mob should work next.
- Turn 3: Copilot (qa-engineer, red): changed tests/test_loop_pick.py
- Turn 4: Codex (backend-dev, green): changed loop.py, tests/test_loop_pick.py
- Turn 5: Codex (backend-dev, refactor): changed loop.py, tests/test_loop_pick.py

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 0 of 2 checks pass
- **After cycle 1: not done**: 0 of 2 checks pass

## Decider outcomes (latest)

- Decider D22: atomic runs (land or discard) and Bishop's Law applied, Steve's retro input (passed)
- Decider D23: Steve's "it should be a loop": the loop runner in mob-defaults, cards 063 to 070 (passed)
- Decider D24: amendment to D23 (Steve's correction: the loop never ends; discovery instead of stop; allotment pacing) (passed)

## Board

- **In progress:** Loop: pick the next eligible card from the board (063)
- **Next in Ready:** next up: Loop: one durable loop at a time (lock, state, heartbeat) (tooling, thin slice) (064); 2nd in Ready: Loop: run one card with a pinned toolchain (tooling, thin slice) (065); 3rd in Ready: Loop: bounded retry, park and split on failure (tooling, thin slice) (066)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016); Benchmark baseline is invalid: fix the harness, store a trustworthy baseline (032); Live mob dashboard (tooling, experiment E2; code in mob-defaults; **Done** 2026-10-26, local-only) (036); Spec resolution defects (bug, from the Grok review) (018)

## Latest commits

- 97636a1 Tooling cards: absolute Phrase paths and Given/When/Then criteria (run 063 stopped at the Ready check); seed
- d190f0b Loop: no separate discovery step (Steve's simplification); 071 withdrawn, folded into 067; D24 amended
- 222482c Loop cards: continuous, never ends (Steve); discovery 071 and budget pacing 072; 067/068 amended; D24
- 51ddf45 Board: the loop runner cards 063-070 (Steve: it should be a loop; D23); 018 Done

## What's next

- Running. Latest: turn 5 by Codex (refactor). Next: the next stage of the current cycle.

_Updated 2026-10-26 22:22 UTC. Summary only: no prompts, replies, code or secrets are published._
