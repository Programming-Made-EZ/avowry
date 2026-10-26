# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)

**Latest cycle:** 1 (driver Codex; navigator Grok; observers Copilot, agy). The full working log is private; this page is a summary.

## Who did what

- **Codex:** 51 turns (checkpoint x3, fix x8, green x5, ready x5, red x8, refactor x4, ship x2, slice x8, teamsignoff x8)
- **Copilot:** 31 turns (checkpoint x17, directions x4, ready x3, teamsignoff x7)
- **Grok:** 37 turns (directions x25, fix x2, red x2, teamsignoff x8)
- **agy:** 26 turns (checkpoint x18, teamsignoff x8)

## Recent turns

- Turn 128: Codex (architect, slice): CONFIRMED — This remains a thin vertical bug-fix slice: the Phrase.Bench CLI invokes real model CLIs, scores their replies, and persists a…
- Turn 129: Grok (navigator, directions): gave 3 directions
- Turn 130: Codex (driver, red): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 131: Copilot (observer, checkpoint): FLAG: Criterion Many/Interface not met: the benchmark check failed because model coverage isn’t complete and bench/README.md is still too short or absent.
- Turn 132: agy (observer, checkpoint): signed off
- Turn 133: Grok (navigator, directions): gave 3 directions
- Turn 134: Codex (driver, fix): changed tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 135: Copilot (observer, checkpoint): FLAG: tools/Phrase.Bench/Program.cs was edited during the red step, violating the rule that only the test may change.
- Turn 136: Copilot (product-manager, ready): READY — The card has a valid Connextra story, complete Given/When/Then ZOMBIES acceptance criteria, and the referenced guidelines and sources exist.
- Turn 137: Codex (architect, slice): CONFIRMED — This is a thin vertical bug-fix slice spanning real CLI invocation, scoring, persisted results, and documented interpretation.
- Turn 138: Grok (navigator, directions): gave 3 directions
- Turn 139: Codex (driver, red): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 140: Copilot (observer, checkpoint): FLAG: tools/Phrase.Bench/Program.cs was modified in the red checkpoint, violating the no-production-code-during-red rule.
- Turn 141: agy (observer, checkpoint): FLAG: git status --short shows MM tools/Phrase.Bench/Program.cs (staged production changes remain in the index) and M bench/results/self-check/history.jsonl during the…
- Turn 142: Grok (navigator, directions): gave 4 directions
- Turn 143: Codex (driver, fix): changed tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs
- Turn 144: Copilot (observer, checkpoint): FLAG: criterion not met: python -c "import json,sys; rows=[json.loads(l) for l in open('bench/results/history.jsonl',encoding='utf-8') if l.strip()]…
- Turn 145: agy (observer, checkpoint): signed off

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 3 of 6 checks pass
- **Cycle 1 roles**: driver Codex; navigator Grok; observers Copilot, agy
- **Flag raised at after red**: - copilot: FLAG: tools/Phrase.Bench/Program.cs was modified in the red checkpoint, violating the no-production-code-during-red rule.
- **Flag unresolved at after red**: - copilot: FLAG: criterion not met: python -c "import json,sys; rows=[json.loads(l) for l in open('bench/results/history.jsonl',encoding='utf-8') if l.strip()]; m={r['model'].split('-')[0] for r in…
- **Experiment E1 metrics**: turns: 8, cycles: 1, flags: 3, fixrounds: 1, esttokens: 79971, seconds: 430.3 (tokens are estimated from prompt and reply size)

## Decider outcomes (latest)

- Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two… (passed)
- Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all… (passed)
- Decider D8: pause experiment E1 (true mobbing) on card 032 after 8 runs; finish 032 in the standard driver-plus-review mode with a whole-team sign-off before the… (passed)

## Board

- **In progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)
- **Next in Ready:** next up: Live mob dashboard (tooling, experiment E2; code in mob-defaults) (036); 2nd in Ready: Keyword model ADR: what a keyword is (022); 3rd in Ready: Make GRAMMAR.md say what the resolver does (013)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016)

## Latest commits

- d8b9444 Decider D8: pause E1 true mobbing on card 032 after 8 runs; E1 data recorded; 032 task force tagged for standard
- 2a32cf4 No Co-Authored-By trailers (Steve's standing coding rule): agreement bullet and commit-msg hook

## What's next

- Running. Latest: turn 145 by agy (checkpoint). Next: the next stage of the current cycle.

_Updated 2026-10-26 06:45 UTC. Summary only: no prompts, replies, code or secrets are published._
