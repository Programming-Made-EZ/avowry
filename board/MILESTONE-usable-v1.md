# Milestone: Usable v1

Steve: "let me know when the language is actually usable to build an application."

**Usable v1 means one automated end-to-end test passes on main.** That test is
`tests/Phrase.Acceptance.Tests/UsableV1.cs` (traits `Milestone=UsableV1` and `Status=Pending` until card 012). It checks, in order:

1. **A real app is written entirely in .phrase:** `demo/tasks/Tasks.phrase`, a CRUD API with 3-5 endpoints, validation and a failure path (beyond the orders fixture).
2. **One CLI command compiles it:** `phrase build demo/tasks/Tasks.phrase -o <dir>` produces C#/FastEndpoints.
3. **`dotnet build` passes with zero warnings** on the generated projects.
4. **The generated xUnit tests from the specs all pass,** one test per spec.
5. **The app runs and real HTTP calls behave as the specs say.**
6. **An LLM customer can extend it:** a fresh-context model, given only GRAMMAR.md and the docs, adds a new endpoint with specs; it compiles, builds clean and all specs pass. Evidence is `docs/usable-v1/llm-check.json` (models, GRAMMAR hash, result). The test fails if the evidence is missing, failed, or made against an older GRAMMAR.md.

**Detection:** CI runs a `usable-v1` job (non-blocking until card 012 makes it a gate). When the test first passes on main, the mob reports a separate line: `USABLE v1 REACHED`, with the demo app's path and how to run it.

## Cards, in PM rank order (the single source of ranks)

| Rank | Card | Why here | Origin |
|---|---|---|---|
| 1 | 002 Resolver malformed-input hardening | The emitter and benchmark need a resolver that neither lies nor crashes | Grok review |
| 2 | 016 LLM benchmark harness v0 | The instrument: every feature card needs a baseline first. v0 needs only parse + resolve | Steve (data-driven) |
| 3 | 013 Make GRAMMAR.md say what the resolver does | Cheap; first hypothesis card, measured by 016 | **Model feedback** (Codex, agy, Grok) |
| 4 | 018 Spec resolution defects (bug, from the Grok review) | Specs are the truth the emitter turns into tests; accurate spec errors first. Decider D1 passed (3 Yes, 1 Support-it) | Review finding |
| 5 | 003 Emit types and runtime | Smallest emitter slice | plan |
| 6 | 014 Document the native C# environment, add passing-guard examples | Precedes 004: it fixes the `Fail` contract the emitter implements | **Model feedback** (Grok, agy, Codex, Copilot) |
| 7 | 004 Emit phrases, effects, failures | Behavior before HTTP | plan |
| 8 | 005 Emit FastEndpoints endpoints and Program | Serves HTTP | plan |
| 9 | 006 Emit specs as xUnit; first acceptance test green | Phrase compiles its first program | plan |
| 10 | 017 Benchmark v1: score by running specs | Turns pass rate into real behavior; re-baselines | Steve (data-driven) |
| 11 | 007 CLI `phrase build` | The one command in bar 2 | plan |
| 12 | 008 Close the language gaps the demo app exposes | Find what CRUD needs, via the Decider protocol and data | plan |
| 13 | 015 Proposal: optional `when` line (Decider first, then measured) | Token saving; needs the benchmark | **Model feedback** (agy) |
| 14 | 009 Demo app: Tasks API in .phrase | Bars 1, 3, 4 | plan |
| 15 | 010 End-to-end over real HTTP | Bar 5 | plan |
| 16 | 011 LLM-customer check | Bar 6 | plan |
| 17 | 012 Usable v1 acceptance gate | Bars 1-6 in one test; CI gate; milestone report | plan |

Ranks are the PM's and move at each retro; ideas are triaged by expected metric impact. Milestones are in `board/MILESTONES.md`.

## Estimate (as of 2026-10-25)

**17 cards are on the board** between now and Usable v1 (002-018). Expect **2-4 more** from retros and from grammar gaps found in cards 008 and 009: about **19-21 cards, range 17-23**. Cards 003-007 are the long pole (the emitter and CLI do not exist yet); 008 is the biggest unknown.
