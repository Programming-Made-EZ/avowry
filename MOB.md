# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)

## Who did what

- **Codex:** 91 turns (advance x2, checkpoint x3, fix x8, green x15, ready x6, red x8, refactor x16, ship x13, slice x12, teamsignoff x8)
- **Copilot:** 43 turns (advance x1, checkpoint x17, directions x4, green x2, ready x5, red x4, signoff x3, teamsignoff x7)
- **Grok:** 38 turns (directions x25, fix x2, ready x1, red x2, teamsignoff x8)
- **agy:** 26 turns (checkpoint x18, teamsignoff x8)

## Recent turns

- Turn 181: Codex (backend-dev, refactor): changed bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 182: Codex (devops-engineer, ship): Stopped due to push-launch failure, as instructed. - Local tests: 251/251 passed. - Commit created: afb9161 (Fix agy benchmark invocation).
- Turn 183: Codex (backend-dev, green): changed bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 184: Codex (backend-dev, refactor): changed bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 185: Codex (devops-engineer, ship): Stopped due to push-launch failure, as instructed. - Local tests: 251/251 passed.
- Turn 186: Copilot (qa-engineer, signoff): SIGNED OFF — The red-first path held: the benchmark acceptance tests were observed failing before the fix, then the harness changes were validated…
- Turn 187: Codex (product-manager, advance): STILL IN PROGRESS — Delivered the repaired benchmark harness, trustworthy four-model baseline, self-check isolation, documentation, model-specific…
- Turn 188: Codex (product-manager, ready): READY — The card has a valid Connextra story, sound Given/When/Then ZOMBIES acceptance criteria, and its referenced project guidelines and sources…
- Turn 189: Codex (architect, slice): CONFIRMED — This remains a thin vertical bug-fix slice spanning real model invocation, reply scoring, persisted results, and baseline documentation.
- Turn 190: Copilot (qa-engineer, red): changed bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json
- Turn 191: Copilot (backend-dev, green): changed MOB.md, bench/results/agy-gemini-4-argon-medium.json, bench/results/history.jsonl, bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json, tools/Phrase.Bench/Program.cs
- Turn 192: Codex (backend-dev, refactor): changed bench/results/agy-gemini-4-argon-medium.json, bench/results/history.jsonl, bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json, tools/Phrase.Bench/Program.cs
- Turn 193: Codex (devops-engineer, ship): SHIPPED — Commit e5d7619 (Keep benchmark self-check results isolated).
- Turn 194: Codex (backend-dev, green): changed bench/results/history.jsonl
- Turn 195: Codex (backend-dev, refactor): changed bench/results/history.jsonl
- Turn 196: Codex (devops-engineer, ship): SHIPPED — Commit b062170 (Restore complete benchmark baseline history). - Local gates: 16/16 benchmark tests and 251/251 full-suite tests passed.
- Turn 197: Codex (backend-dev, green): changed bench/results/history.jsonl, bench/results/self-check/history.jsonl
- Turn 198: Codex (backend-dev, refactor): changed bench/results/history.jsonl, bench/results/self-check/history.jsonl

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 7 of 10 checks pass
- **After cycle 1: not done**: 7 of 10 checks pass
- **After cycle 2: not done**: 7 of 10 checks pass
- **After cycle 3: not done**: 8 of 10 checks pass

## Decider outcomes (latest)

- Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two… (passed)
- Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all… (passed)
- Decider D8: pause experiment E1 (true mobbing) on card 032 after 8 runs; finish 032 in the standard driver-plus-review mode with a whole-team sign-off before the… (passed)

## Board

- **In progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)
- **Next in Ready:** next up: Live mob dashboard (tooling, experiment E2; code in mob-defaults) (036); 2nd in Ready: Keyword model ADR: what a keyword is (022); 3rd in Ready: Make GRAMMAR.md say what the resolver does (013)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016)

## Latest commits

- b062170 Restore complete benchmark baseline history
- e5d7619 Keep benchmark self-check results isolated
- 84a4369 Exclude empty-reply models from benchmark baseline
- afb9161 Fix agy benchmark invocation

## What's next

- Running. Latest: turn 198 by Codex (refactor). Next: the next stage of the current cycle.

_Updated 2026-10-26 11:23 UTC. Summary only: no prompts, replies, code or secrets are published._
