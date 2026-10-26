# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)

**Latest cycle:** 1 (driver Codex; navigator Grok; observers Copilot, agy). The full working log is private; this page is a summary.

## Who did what

- **Codex:** 50 turns (checkpoint x3, fix x7, green x5, ready x5, red x8, refactor x4, ship x2, slice x8, teamsignoff x8)
- **Copilot:** 30 turns (checkpoint x16, directions x4, ready x3, teamsignoff x7)
- **Grok:** 36 turns (directions x24, fix x2, red x2, teamsignoff x8)
- **agy:** 25 turns (checkpoint x17, teamsignoff x8)

## Recent turns

- Turn 124: Codex (member, teamsignoff): FLAG: ProfileArguments passes the prompt file path as Copilot’s literal -p value, so Copilot receives a pathname instead of the task prompt.
- Turn 125: Grok (member, teamsignoff): FLAG: copilot --help defines -p/--prompt as <text>, but return $" -p \"{promptPath}\" -s --allow-all-tools --trust-folder"; passes a temp pathname as that text.
- Turn 126: agy (member, teamsignoff): FLAG: return $" -p \"{promptPath}\" -s --allow-all-tools --trust-folder"; in [Program.cs](file:///C:/home/<user>) passes a leaked global temp file path…
- Turn 127: Codex (product-manager, ready): READY — The card has a valid Connextra story, sound Given/When/Then ZOMBIES acceptance criteria, and all referenced project guidance and sources…
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

## Flags, checks and decisions on this card

- **Baseline (before any work): not done**: 3 of 6 checks pass
- **Cycle 1 roles**: driver Codex; navigator Grok; observers Copilot, agy

## Decider outcomes (latest)

- Decider D5: declare card 016 Done on its written criteria but not M2; file bug card 032 (invalid baseline) at rank 3; run 032 as the first E1 true-mobbing card… (passed)
- Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two… (passed)
- Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all… (passed)

## Board

- **In progress:** Benchmark baseline is invalid: fix the harness and store a trustworthy baseline (032)
- **Next in Ready:** next up: Live mob dashboard (tooling, experiment E2; code in mob-defaults) (036); 2nd in Ready: Keyword model ADR: what a keyword is (022); 3rd in Ready: Make GRAMMAR.md say what the resolver does (013)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016)

## Latest commits

- 2a32cf4 No Co-Authored-By trailers (Steve's standing coding rule): agreement bullet and commit-msg hook
- 1d13572 Pre-commit check: the published MOB.md must hold no secrets or home paths (public repo)
- a4ab305 Isolate benchmark self-check results

## What's next

- Running. Latest: turn 141 by agy (checkpoint). Next: the next stage of the current cycle.

_Updated 2026-10-26 06:40 UTC. Summary only: no prompts, replies, code or secrets are published._
