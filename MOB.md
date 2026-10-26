**Usable v1: 0 of 6 criteria · next card: Spec resolution defects from the independent review (code only) (018)**

# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Spec resolution defects from the independent review (code only) (018)

## Who did what

- **Codex:** 9 turns (green x3, ready x1, refactor x2, ship x2, slice x1)
- **Copilot:** 1 turns (red x1)

## Recent turns

- Turn 1: Codex (product-manager, ready): READY — The card has a sound Connextra story, testable Given/When/Then criteria, and all referenced guidance/source/test files exist.
- Turn 2: Codex (architect, slice): CONFIRMED — This is a thin vertical bug-fix slice: Phrase source enters the public Resolver.Resolve(string) boundary and produces user-visible…
- Turn 3: Copilot (qa-engineer, red): changed VISION.md, docs/GRAMMAR.md, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 4: Codex (backend-dev, green): changed VISION.md, docs/GRAMMAR.md, src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 5: Codex (backend-dev, refactor): changed VISION.md, docs/GRAMMAR.md, src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 6: Codex (devops-engineer, ship): SHIP BLOCKED — required local gate failed.
- Turn 7: Codex (backend-dev, green): changed src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 8: Codex (backend-dev, refactor): changed src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 9: Codex (devops-engineer, ship): SHIP BLOCKED — required local gate failed.
- Turn 10: Codex (backend-dev, green): changed board/MILESTONE-usable-v1.md, src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, board/059-adr-async-and-events.md, board/060-catalogue-task-pack-async-events.md, board/061-spike-deterministic-async-harness.md

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 1 of 3 checks pass
- **After cycle 1: not done**: 2 of 3 checks pass
- **After cycle 2: not done**: 2 of 3 checks pass

## Decider outcomes (latest)

- Decider D17: board order, rounds 2 and 3 (passed)
- Decider D18: process retro picks (thin slices trialled on the C# adapter chain; keep the listed seeds) (passed)
- Decider D20: Steve's consumer-observable-outcomes direction, refined into cards 054 to 058 (passed)

## Board

- **In progress:** Spec resolution defects from the independent review (code only) (018)
- **Next in Ready:** next up: Docs: one diagnostics table, not two (docs only, split from 018) (053); 2nd in Ready: One phrase with one number input compiles to C# and its spec passes (thin slice; stash Decider first) (039); 3rd in Ready: C# compilation goes through the ITargetAdapter contract (thin slice) (040)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016); Benchmark baseline is invalid: fix the harness, store a trustworthy baseline (032); Live mob dashboard (tooling, experiment E2; code in mob-defaults; **Done** 2026-10-26, local-only) (036)

## Latest commits

- 254ad71 Steve's direction: consumer-observable outcomes (VISION ruling 5, GRAMMAR rule 6); cards 054-058; D20
- 636f72d Board: 018 trimmed to code with task force and red-first Done-when; docs split to 053
- c1d811a Process retro 2026-10-26: thin slices (D18); cards 003-007 superseded by 039-052; slice-test check script
- 1706ce7 Board: card 038 (dogfood status API in Phrase) and re-rank (Deciders D14-D17); D15 tooling standard passed

## What's next

- Running. Latest: turn 10 by Codex (green). Next: the next stage of the current cycle.

_Updated 2026-10-26 20:42 UTC. Summary only: no prompts, replies, code or secrets are published._
