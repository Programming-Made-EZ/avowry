# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** finished
**Card in progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)

## Who did what

- **Codex:** 71 turns (advance x1, checkpoint x3, fix x8, green x10, ready x5, red x8, refactor x10, ship x8, slice x10, teamsignoff x8)
- **Copilot:** 39 turns (advance x1, checkpoint x17, directions x4, green x1, ready x5, red x2, signoff x2, teamsignoff x7)
- **Grok:** 37 turns (directions x25, fix x2, red x2, teamsignoff x8)
- **agy:** 26 turns (checkpoint x18, teamsignoff x8)

## Recent turns

- Turn 156: Codex (backend-dev, refactor): changed bench/results/codex-gpt-5.6-sol.json, bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 157: Codex (devops-engineer, ship): Stopped: CI failed.
- Turn 158: Copilot (backend-dev, green): changed bench/results/agy-gemini-4-argon-medium.json, bench/results/codex-gpt-5.6-sol.json, bench/results/grok-4.7.json, bench/results/history.jsonl, bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs
- Turn 159: Codex (backend-dev, refactor): changed bench/results/agy-gemini-4-argon-medium.json, bench/results/codex-gpt-5.6-sol.json, bench/results/grok-4.7.json, bench/results/history.jsonl, bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs
- Turn 160: Codex (devops-engineer, ship): Shipped successfully.
- Turn 161: Copilot (qa-engineer, signoff): SIGN-OFF — The red-to-green path was proven: the fenced/narrated Phrase acceptance test failed first, the fix was then validated green, and the…
- Turn 162: Copilot (product-manager, advance): DONE — Delivered the trustworthy four-model benchmark baseline: fenced/narrated Phrase extraction is scored correctly, self-check rows are isolated…
- Turn 163: Copilot (product-manager, ready): READY — The card is ready: it has a clear Connextra story and sound Given/When/Then acceptance criteria, the referenced project guidelines and source…
- Turn 164: Codex (architect, slice): CONFIRMED — This is a thin vertical bug-fix slice spanning real CLI invocation, reply parsing/scoring, result persistence, and user-facing benchmark…
- Turn 165: Copilot (qa-engineer, red): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 166: Codex (backend-dev, green): changed bench/results/copilot-default.json, bench/results/history.jsonl, bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 167: Codex (backend-dev, refactor): changed bench/results/copilot-default.json, bench/results/history.jsonl, bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 168: Codex (devops-engineer, ship): Shipped successfully.
- Turn 169: Codex (backend-dev, green): changed bench/results/history.jsonl, bench/results/self-check/history.jsonl, tools/Phrase.Bench/Program.cs
- Turn 170: Codex (backend-dev, refactor): changed bench/results/history.jsonl, bench/results/self-check/history.jsonl, tools/Phrase.Bench/Program.cs
- Turn 171: Codex (devops-engineer, ship): Ship stopped on push failure, as instructed.
- Turn 172: Copilot (qa-engineer, signoff): SIGN-OFF
- Turn 173: Codex (product-manager, advance): IN PROGRESS — The harness now extracts Phrase from narrated/fenced replies, records nophrasefound, invokes all four model CLIs headlessly, isolates…

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 6 of 8 checks pass
- **After cycle 1: not done**: 7 of 8 checks pass
- **After cycle 2: DONE**: 8 of 8 checks pass

## Decider outcomes (latest)

- Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two… (passed)
- Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all… (passed)
- Decider D8: pause experiment E1 (true mobbing) on card 032 after 8 runs; finish 032 in the standard driver-plus-review mode with a whole-team sign-off before the… (passed)

## Board

- **In progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)
- **Next in Ready:** next up: Live mob dashboard (tooling, experiment E2; code in mob-defaults) (036); 2nd in Ready: Keyword model ADR: what a keyword is (022); 3rd in Ready: Make GRAMMAR.md say what the resolver does (013)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016)

## Latest commits

- 6f7a978 Fix agy benchmark prompt invocation
- c972981 Complete Copilot benchmark coverage
- 9142d84 Bootstrap phase (Steve's standing rule): no metric-based rejections until a baseline is established; exit criter
- 0a0e965 Board: card 032 reopened after the whole-team sign-off (agy empty replies; copilot missing a task); two new Done

## What's next

- The card is done. Next: its retro, then the next card from the board.

_Updated 2026-10-26 09:25 UTC. Summary only: no prompts, replies, code or secrets are published._
