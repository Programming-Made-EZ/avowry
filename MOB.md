**Usable v1: 0 of 6 criteria · next card: Docs: one diagnostics table, not two (docs only, split from 018) (053)**

# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** finished
**Card in progress:** Spec resolution defects from the independent review (code only) (018)

## Who did what

- **Codex:** 27 turns (advance x4, green x6, ready x2, refactor x6, ship x6, signoff x1, slice x2)
- **Copilot:** 6 turns (red x3, signoff x3)

## Recent turns

- Turn 14: Codex (product-manager, advance): CONFIRMED — This is a thin vertical bug-fix slice: Phrase source enters the public Resolver.Resolve(string) boundary and returns user-visible…
- Turn 15: Copilot (qa-engineer, red): changed src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 16: Codex (backend-dev, green): changed src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 17: Codex (backend-dev, refactor): changed src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 18: Codex (devops-engineer, ship): SHIP BLOCKED — staged whitespace gate failed because the new test file uses CRLF, which Git reports as trailing whitespace on all 80 lines.
- Turn 19: Codex (backend-dev, green): changed src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 20: Codex (backend-dev, refactor): changed src/Phrase.Compiler/Semantics/Resolver.cs, tests/Phrase.Compiler.Tests/Semantics/Resolved.cs, tests/Phrase.Compiler.Tests/Semantics/ResolverSpecDefectsTests.cs
- Turn 21: Codex (devops-engineer, ship): SHIPPED — commit 8962dc5 (Fix spec resolution diagnostics).
- Turn 22: Codex (backend-dev, green): no file changes
- Turn 23: Codex (backend-dev, refactor): no file changes
- Turn 24: Codex (devops-engineer, ship): SHIP BLOCKED — full local gate failed due to missing Microsoft.AspNetCore.App 8.0 runtime.
- Turn 25: Copilot (qa-engineer, signoff): QA sign-off: process was honest and red-first.
- Turn 26: Codex (product-manager, advance): Spec-resolution fixes were delivered and shipped in commit 8962dc5: accurate PH334 endpoint naming, recursive PH332 literal-data enforcement, single…
- Turn 27: Codex (product-manager, ready): READY — The card has a sound Connextra story, testable Given/When/Then criteria, and the referenced project guidelines and source/test files exist.
- Turn 28: Codex (architect, slice): CONFIRMED — This remains a thin vertical bug-fix slice through Phrase source to user-visible diagnostics at the public Resolver.Resolve(string)…
- Turn 29: Copilot (qa-engineer, red): no file changes
- Turn 30: Copilot (qa-engineer, signoff): QA sign-off recorded in MOB.md.
- Turn 31: Codex (product-manager, advance): Delivered and shipped spec-resolution fixes: PH334 uses the real endpoint name, PH332 rejects nested paths in spec data, duplicate-hole PH331 appears…

## Flags, checks and decisions on this card

- **Baseline (before any work): DONE**: 3 of 3 checks pass

## Decider outcomes (latest)

- Decider D20: Steve's consumer-observable-outcomes direction, refined into cards 054 to 058 (passed)
- Decider D21: Steve's async and events input, refined into cards 059 to 061 (passed)
- Decider D22: atomic runs (land or discard) and Bishop's Law applied, Steve's retro input (passed)

## Board

- **In progress:** Spec resolution defects from the independent review (code only) (018)
- **Next in Ready:** next up: Docs: one diagnostics table, not two (docs only, split from 018) (053); 2nd in Ready: Atomic runs: a card runs in a fresh worktree and ends landed or discarded (tooling, experiment E3) (062); 3rd in Ready: One phrase with one number input compiles to C# and its spec passes (thin slice; stash Decider first) (039)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016); Benchmark baseline is invalid: fix the harness, store a trustworthy baseline (032); Live mob dashboard (tooling, experiment E2; code in mob-defaults; **Done** 2026-10-26, local-only) (036)

## Latest commits

- 8962dc5 Fix spec resolution diagnostics
- f0cfccb Atomic runs experiment (D22): Bishop's Law applied, Depends on lines, card 062; D21 async decision record
- 7558677 Board: async and events (Steve's input, D21): ADR 059, catalogue tasks 060, spike 061; keyword seeds

## What's next

- The card is done. Next: its retro, then the next card from the board.

_Updated 2026-10-26 21:25 UTC. Summary only: no prompts, replies, code or secrets are published._
