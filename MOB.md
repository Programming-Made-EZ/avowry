# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Fix the benchmark baseline (032)

**Latest cycle:** 1 (driver Codex; navigator Grok; observers Copilot, agy). The full working log is private; this page is a summary.

## Who did what

- **Codex:** 44 turns (checkpoint x3, fix x6, green x5, ready x4, red x6, refactor x4, ship x2, slice x6, teamsignoff x8)
- **Copilot:** 26 turns (checkpoint x13, directions x4, ready x2, teamsignoff x7)
- **Grok:** 33 turns (directions x21, fix x2, red x2, teamsignoff x8)
- **agy:** 22 turns (checkpoint x15, teamsignoff x7)

## Recent turns

- Turn 108: Grok (navigator, directions): gave 5 directions
- Turn 109: Codex (driver, red): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs
- Turn 110: Copilot (observer, checkpoint): signed off
- Turn 111: agy (observer, checkpoint): signed off
- Turn 112: Grok (navigator, directions): gave 5 directions
- Turn 113: Codex (driver, green): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 114: Copilot (observer, checkpoint): signed off
- Turn 115: agy (observer, checkpoint): signed off
- Turn 116: Grok (navigator, directions): gave 6 directions
- Turn 117: Codex (driver, refactor): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 118: Codex (member, teamsignoff): FLAG: bench/results/self-check/history.jsonl contains 24 newly appended duplicate self-check fixture rows and must not ship with this increment.
- Turn 119: Grok (member, teamsignoff): FLAG: bench/results/self-check/history.jsonl adds 24 lines and all 24 duplicate existing self-check fixture rows, so that file must not ship with this increment.
- Turn 120: Copilot (member, teamsignoff): signed off
- Turn 121: agy (member, teamsignoff): FLAG: bench/results/self-check/history.jsonl is dirty with 24 newly appended duplicate self-check-v1 fixture rows that must not be committed with this increment.
- Turn 122: Grok (navigator, directions): gave 3 directions
- Turn 123: Codex (driver, fix): changed tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs, tools/Phrase.Bench/Program.cs
- Turn 124: Codex (member, teamsignoff): FLAG: ProfileArguments passes the prompt file path as Copilot’s literal -p value, so Copilot receives a pathname instead of the task prompt.
- Turn 125: Grok (member, teamsignoff): FLAG: copilot --help defines -p/--prompt as <text>, but return $" -p \"{promptPath}\" -s --allow-all-tools --trust-folder"; passes a temp pathname as that text.

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 4 of 6 checks pass
- **Cycle 1 roles**: driver Codex; navigator Grok; observers Copilot, agy
- **Flag raised at before commit (whole team)**: - codex: FLAG: bench/results/self-check/history.jsonl contains 24 newly appended duplicate self-check fixture rows and must not ship with this increment.

## Decider outcomes (latest)

- Decider D5: declare card 016 Done on its written criteria but not M2; file bug card 032 (invalid baseline) at rank 3; run 032 as the first E1 true-mobbing card… (passed)
- Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two… (passed)
- Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all… (passed)

## Board

- **In progress:** Fix the benchmark baseline (032)
- **Next in Ready:** next up: Live mob dashboard (tooling, experiment E2; code in mob-defaults) (036); 2nd in Ready: Keyword model ADR: what a keyword is (022); 3rd in Ready: Make GRAMMAR.md say what the resolver does (013)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016)

## Latest commits

- 2a32cf4 No Co-Authored-By trailers (Steve's standing coding rule): agreement bullet and commit-msg hook
- 1d13572 Pre-commit check: the published MOB.md must hold no secrets or home paths (public repo)
- a4ab305 Isolate benchmark self-check results
- ce1eb04 Live mob dashboard: tooling card 036 and experiment E2, ranked via Decider D7 (amended after Copilot's ABSOLUTEL

## What's next

- Running. Latest: turn 125 by Grok (teamsignoff). Next: the next stage of the current cycle.

_Updated 2026-10-26 06:22 UTC. Summary only: no prompts, replies, code or secrets are published._
