**Usable v1: 0 of 6 criteria · next card: Spec resolution defects from the independent review (code only) (018)**

# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Spec resolution defects from the independent review (code only) (018)

## Who did what

- **Codex:** 11 turns (green x3, ready x1, refactor x3, ship x3, slice x1)
- **Copilot:** 2 turns (red x1, signoff x1)

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
- Turn 11: Codex (backend-dev, refactor): changed src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 12: Codex (devops-engineer, ship): SHIP BLOCKED — required local gate failed.
- Turn 13: Copilot (qa-engineer, signoff): I’m validating the stored mob state against the actual repo and will either complete the remaining defect fixes or sign off with the precise QA…

## Flags, checks and decisions on this card

- None yet.

## Decider outcomes (latest)

- Decider D20: Steve's consumer-observable-outcomes direction, refined into cards 054 to 058 (passed)
- Decider D21: Steve's async and events input, refined into cards 059 to 061 (passed)
- Decider D22: atomic runs (land or discard) and Bishop's Law applied, Steve's retro input (passed)

## Board

- **In progress:** Spec resolution defects from the independent review (code only) (018)
- **Next in Ready:** next up: Docs: one diagnostics table, not two (docs only, split from 018) (053); 2nd in Ready: Atomic runs: a card runs in a fresh worktree and ends landed or discarded (tooling, experiment E3) (062); 3rd in Ready: One phrase with one number input compiles to C# and its spec passes (thin slice; stash Decider first) (039)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016); Benchmark baseline is invalid: fix the harness, store a trustworthy baseline (032); Live mob dashboard (tooling, experiment E2; code in mob-defaults; **Done** 2026-10-26, local-only) (036)

## Latest commits

- f0cfccb Atomic runs experiment (D22): Bishop's Law applied, Depends on lines, card 062; D21 async decision record
- 7558677 Board: async and events (Steve's input, D21): ADR 059, catalogue tasks 060, spike 061; keyword seeds
- 254ad71 Steve's direction: consumer-observable outcomes (VISION ruling 5, GRAMMAR rule 6); cards 054-058; D20
- 636f72d Board: 018 trimmed to code with task force and red-first Done-when; docs split to 053

## What's next

- Running. Latest: turn 13 by Copilot (signoff). Next: the next stage of the current cycle.

_Updated 2026-10-26 20:48 UTC. Summary only: no prompts, replies, code or secrets are published._
