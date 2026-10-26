# Mob status

**Project:** Phrase &nbsp;|&nbsp; **State:** running
**Card in progress:** Fix the benchmark baseline (032)

**Latest cycle:** 1; the full working log is private, this page is a summary.

## Who did what

- **Codex:** 39 turns (checkpoint x3, fix x5, green x4, ready x4, red x6, refactor x3, ship x2, slice x6, teamsignoff x6)
- **Copilot:** 24 turns (checkpoint x12, directions x4, ready x2, teamsignoff x6)
- **Grok:** 28 turns (directions x18, fix x2, red x2, teamsignoff x6)
- **agy:** 20 turns (checkpoint x14, teamsignoff x6)

## Recent turns

- Turn 94: Grok (member, teamsignoff): signed off
- Turn 95: Copilot (member, teamsignoff): signed off
- Turn 96: agy (member, teamsignoff): signed off
- Turn 97: Codex (devops-engineer, ship): STOPPED — commit `a4ab305` was pushed and `origin/main` matches it. Local gates passed (247 tests), and CI’s test job passed, but the `usable-v1` job 
- Turn 98: Copilot (navigator, directions): gave 5 directions
- Turn 99: Grok (driver, red): changed bench/results/self-check/history.jsonl
- Turn 100: Codex (observer, checkpoint): FLAG
- Turn 101: agy (observer, checkpoint): FLAG
- Turn 102: Copilot (navigator, directions): gave 5 directions
- Turn 103: Grok (driver, fix): changed bench/results/history.jsonl, bench/results/self-check/history.jsonl, tools/Phrase.Bench/Program.cs, bench/README.md, bench/results/copilot-auto.json
- Turn 104: Codex (observer, checkpoint): FLAG
- Turn 105: agy (observer, checkpoint): FLAG
- Turn 106: Codex (product-manager, ready): READY — The card has a valid Connextra story, sound Given/When/Then ZOMBIES acceptance criteria, and all referenced project guidance exists. Assumptio
- Turn 107: Codex (architect, slice): CONFIRMED — This is a thin vertical bug-fix slice spanning model invocation, scoring, persisted results, and documentation through the benchmark CLI. 
- Turn 108: Grok (navigator, directions): gave 5 directions
- Turn 109: Codex (driver, red): changed bench/results/self-check/history.jsonl, tests/Phrase.Acceptance.Tests/BenchmarkHarnessAcceptance.cs
- Turn 110: Copilot (observer, checkpoint): signed off
- Turn 111: agy (observer, checkpoint): signed off

## Flags, checks and decisions on this card

- **Flag unresolved at after green**: - copilot: FLAG: criterion failed: model set is {'agy','codex','grok'} instead of {'agy','codex','copilot','grok'} and unavailable=21/40 exceeds the 10% cap (un*10<=len(rows) is false).
- **Experiment E1 metrics**: turns: 11, cycles: 1, flags: 2, fix_rounds: 1, est_tokens: 108641, seconds: 628.4 (tokens are estimated from prompt and reply size)
- **Baseline (before any work): not done**: - `python -c "import json,sys; rows=[json.loads(l) for l in open('bench/results/history.jsonl',encoding='utf-8') if l.strip()]; sys.exit(1 if any(str(r.get('model','')).startswith('self-check') for r 
- **Cycle 1 roles**
- **Flag raised at before commit (whole team)**: - codex: FLAG: `bench/results/self-check-v1.json` is leaked self-check fixture data outside `bench/results/self-check/`.
- **After cycle 1: not done**: - `python -c "import json,sys; rows=[json.loads(l) for l in open('bench/results/history.jsonl',encoding='utf-8') if l.strip()]; sys.exit(1 if any(str(r.get('model','')).startswith('self-check') for r 
- **Cycle 2 roles**
- **Flag raised at after red**: - codex: FLAG: No new failing test was added or proved red; only generated self-check history rows changed.
- **Flag unresolved at after red**: - codex: FLAG: Production code changed at the RED checkpoint (`tools/Phrase.Bench/Program.cs`).
- **Experiment E1 metrics**: turns: 28, cycles: 2, flags: 8, fix_rounds: 2, est_tokens: 276034, seconds: 3116.9 (tokens are estimated from prompt and reply size)
- **Baseline (before any work): not done**: - `python -c "import json,sys; rows=[json.loads(l) for l in open('bench/results/history.jsonl',encoding='utf-8') if l.strip()]; sys.exit(1 if any(str(r.get('model','')).startswith('self-check') for r 
- **Cycle 1 roles**

## Decider outcomes (latest)

- Decider D5: declare card 016 Done on its written criteria but not M2; file bug card 032 (invalid baseline) at rank 3; run 032 as the first E1 true-mobbing card wi (passed)
- Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two  (passed)
- Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all mo (passed)

## Board

- **In progress:** Fix the benchmark baseline (032)
- **Next in Ready:** next up: Benchmark baseline is invalid: fix the harness, store a trustworthy baseline (032); 2nd in Ready: Live mob dashboard (tooling, experiment E2; code in mob-defaults) (036); 3rd in Ready: Keyword model ADR: what a keyword is (022)
- **Done:** Resolver malformed-input hardening (002); LLM benchmark harness v0 (016)

## Latest commits

- 1d13572 Pre-commit check: the published MOB.md must hold no secrets or home paths (public repo)
- a4ab305 Isolate benchmark self-check results
- ce1eb04 Live mob dashboard: tooling card 036 and experiment E2, ranked via Decider D7 (amended after Copilot's ABSOLUTEL
- bc299b5 Fix benchmark reply source extraction

## What's next

- Running. Latest: turn 111 by agy (checkpoint). Next: the next stage of the current cycle.

_Updated 2026-10-26 06:10 UTC. Summary only: no prompts, replies, code or secrets are published._
