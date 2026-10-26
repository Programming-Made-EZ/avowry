# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)

## Who did what

- **Codex:** 62 turns (checkpoint x3, fix x8, green x8, ready x5, red x8, refactor x8, ship x5, slice x9, teamsignoff x8)
- **Copilot:** 34 turns (checkpoint x17, directions x4, green x1, ready x4, red x1, teamsignoff x7)
- **Grok:** 37 turns (directions x25, fix x2, red x2, teamsignoff x8)
- **agy:** 26 turns (checkpoint x18, teamsignoff x8)

## Recent turns

- Turn 142: Grok (navigator, directions): gave 4 directions
- Turn 143: Codex (driver, fix): changed tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs
- Turn 144: Copilot (observer, checkpoint): FLAG: criterion not met: python -c "import json,sys; rows=[json.loads(l) for l in open('bench/results/history.jsonl',encoding='utf-8') if l.strip()]…
- Turn 145: agy (observer, checkpoint): signed off
- Turn 146: Copilot (product-manager, ready): READY — The card has a valid Connextra story, complete Given/When/Then ZOMBIES acceptance criteria, correct rank 3, and the referenced guidelines and…
- Turn 147: Codex (architect, slice): CONFIRMED — This remains a thin vertical bug-fix slice: real model CLI invocation flows through extraction, scoring, result persistence, and baseline…
- Turn 148: Copilot (qa-engineer, red): changed tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs
- Turn 149: Codex (backend-dev, green): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 150: Codex (backend-dev, refactor): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 151: Codex (devops-engineer, ship): Stopped: CI had a failing usable-v1 job.
- Turn 152: Codex (backend-dev, green): changed bench/results/self-check/history.jsonl, bench/README.md
- Turn 153: Codex (backend-dev, refactor): changed bench/results/self-check/history.jsonl, bench/README.md
- Turn 154: Codex (devops-engineer, ship): Stopped: CI failed.
- Turn 155: Codex (backend-dev, green): changed bench/results/codex-gpt-5.6-sol.json, bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 156: Codex (backend-dev, refactor): changed bench/results/codex-gpt-5.6-sol.json, bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 157: Codex (devops-engineer, ship): Stopped: CI failed.
- Turn 158: Copilot (backend-dev, green): changed bench/results/agy-gemini-4-argon-medium.json, bench/results/codex-gpt-5.6-sol.json, bench/results/grok-4.7.json, bench/results/history.jsonl, bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs
- Turn 159: Codex (backend-dev, refactor): changed bench/results/agy-gemini-4-argon-medium.json, bench/results/codex-gpt-5.6-sol.json, bench/results/grok-4.7.json, bench/results/history.jsonl, bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 3 of 6 checks pass
- **After cycle 1: not done**: 4 of 6 checks pass
- **After cycle 2: not done**: 5 of 6 checks pass
- **After cycle 3: not done**: 5 of 6 checks pass
- **After cycle 4: DONE**: 6 of 6 checks pass

## Decider outcomes (latest)

- Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two… (passed)
- Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all… (passed)
- Decider D8: pause experiment E1 (true mobbing) on card 032 after 8 runs; finish 032 in the standard driver-plus-review mode with a whole-team sign-off before the… (passed)

## Board

- **In progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)
- **Next in Ready:** next up: Live mob dashboard (tooling, experiment E2; code in mob-defaults) (036); 2nd in Ready: Keyword model ADR: what a keyword is (022); 3rd in Ready: Make GRAMMAR.md say what the resolver does (013)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016)

## Latest commits

- e0f1dfa Handle locked benchmark scratch cleanup
- e00a6d9 Document Phrase benchmark harness
- 790ab41 Fix Copilot benchmark prompt argument
- d8b9444 Decider D8: pause E1 true mobbing on card 032 after 8 runs; E1 data recorded; 032 task force tagged for standard

## What's next

- Running. Latest: turn 159 by Codex (refactor). Next: the next stage of the current cycle.

_Updated 2026-10-26 08:15 UTC. Summary only: no prompts, replies, code or secrets are published._
