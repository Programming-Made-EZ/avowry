# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** stopped (exit 1)
**Card in progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)

## Who did what

- **Codex:** 82 turns (advance x2, checkpoint x3, fix x8, green x13, ready x5, red x8, refactor x13, ship x11, slice x11, teamsignoff x8)
- **Copilot:** 41 turns (advance x1, checkpoint x17, directions x4, green x1, ready x5, red x3, signoff x3, teamsignoff x7)
- **Grok:** 38 turns (directions x25, fix x2, ready x1, red x2, teamsignoff x8)
- **agy:** 26 turns (checkpoint x18, teamsignoff x8)

## Recent turns

- Turn 170: Codex (backend-dev, refactor): changed bench/results/history.jsonl, bench/results/self-check/history.jsonl, tools/Phrase.Bench/Program.cs
- Turn 171: Codex (devops-engineer, ship): Ship stopped on push failure, as instructed.
- Turn 172: Copilot (qa-engineer, signoff): SIGN-OFF
- Turn 173: Codex (product-manager, advance): IN PROGRESS — The harness now extracts Phrase from narrated/fenced replies, records nophrasefound, invokes all four model CLIs headlessly, isolates…
- Turn 174: Grok (product-manager, ready): I'll read the product-manager turn file and follow its instructions.The turn file is long.
- Turn 175: Codex (architect, slice): CONFIRMED — This remains a thin vertical bug-fix slice: the Phrase.Bench CLI invokes models, validates captured replies, and persists an auditable…
- Turn 176: Copilot (qa-engineer, red): changed tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 177: Codex (backend-dev, green): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 178: Codex (backend-dev, refactor): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 179: Codex (devops-engineer, ship): Shipped commit 1e97647 (Handle empty benchmark replies safely). - Local gate: 251/251 tests passed. - origin/main verified at 1e97647.
- Turn 180: Codex (backend-dev, green): changed bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 181: Codex (backend-dev, refactor): changed bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 182: Codex (devops-engineer, ship): Stopped due to push-launch failure, as instructed. - Local tests: 251/251 passed. - Commit created: afb9161 (Fix agy benchmark invocation).
- Turn 183: Codex (backend-dev, green): changed bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 184: Codex (backend-dev, refactor): changed bench/results/self-check/history.jsonl, bench/results/self-check/self-check-v1.json, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 185: Codex (devops-engineer, ship): Stopped due to push-launch failure, as instructed. - Local tests: 251/251 passed.
- Turn 186: Copilot (qa-engineer, signoff): SIGNED OFF — The red-first path held: the benchmark acceptance tests were observed failing before the fix, then the harness changes were validated…
- Turn 187: Codex (product-manager, advance): STILL IN PROGRESS — Delivered the repaired benchmark harness, trustworthy four-model baseline, self-check isolation, documentation, model-specific…

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 6 of 10 checks pass
- **After cycle 1: not done**: 7 of 10 checks pass
- **After cycle 2: not done**: 7 of 10 checks pass
- **After cycle 3: not done**: 7 of 10 checks pass
- **Not done**: Not done after 3 cycles. The next run picks up from here.

## Decider outcomes (latest)

- Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two… (passed)
- Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all… (passed)
- Decider D8: pause experiment E1 (true mobbing) on card 032 after 8 runs; finish 032 in the standard driver-plus-review mode with a whole-team sign-off before the… (passed)

## Board

- **In progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)
- **Next in Ready:** next up: Live mob dashboard (tooling, experiment E2; code in mob-defaults) (036); 2nd in Ready: Keyword model ADR: what a keyword is (022); 3rd in Ready: Make GRAMMAR.md say what the resolver does (013)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016)

## Latest commits

- 84a4369 Exclude empty-reply models from benchmark baseline
- afb9161 Fix agy benchmark invocation
- 1e97647 Handle empty benchmark replies safely
- 3de336d Board: card 032 reopened a second time (agy still empty; weak check could be passed by a mix); stronger checks

## What's next

- Stopped before the card was done. Next: look at the last flag or check above, then run again.

_Updated 2026-10-26 10:03 UTC. Summary only: no prompts, replies, code or secrets are published._
