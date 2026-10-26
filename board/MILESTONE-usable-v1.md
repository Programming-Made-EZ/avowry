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
| 1 | 002 Resolver malformed-input hardening (**Done**, 8ba3699) | The emitter and benchmark need a resolver that neither lies nor crashes | Grok review |
| 2 | 016 LLM benchmark harness v0 (**in progress**) | The instrument: every feature card needs a baseline first. v0 needs only parse + resolve | Steve (data-driven) |
| 3 | 022 Keyword model ADR: what a keyword is | Decides the shape of everything the vision asks for; cheap; unblocks 023 and 025 | **Vision** (Steve) |
| 4 | 013 Make GRAMMAR.md say what the resolver does | Cheap; first hypothesis card, measured by 016 | **Model feedback** (Codex, agy, Grok) |
| 5 | 018 Spec resolution defects (bug, from the Grok review) | Specs are the truth the emitter turns into tests; accurate spec errors first. Decider D1 passed (3 Yes, 1 Support-it) | Review finding |
| 6 | 019 Publish exact diagnostic message templates (docs) | All four models hit this gap; 018 fixes the message first; measured by 016 | **Model feedback** (Codex, Copilot, Grok, agy) |
| 7 | 003 Emit types and runtime | Smallest emitter slice | plan |
| 8 | 014 Document the native C# environment, add passing-guard examples | Precedes 004: it fixes the `Fail` contract the emitter implements | **Model feedback** (Grok, agy, Codex, Copilot) |
| 9 | 004 Emit phrases, effects, failures | Behavior before HTTP | plan |
| 10 | 005 Emit FastEndpoints endpoints and Program | Serves HTTP | plan |
| 11 | 006 Emit specs as xUnit; first acceptance test green | Phrase compiles its first program | plan |
| 12 | 017 Benchmark v1: score by running specs | Turns pass rate into real behavior; re-baselines | Steve (data-driven) |
| 13 | 024 Pattern discovery (mine real code and LLM tasks) | Vocabulary grows by evidence; right after the benchmark can measure it | **Vision** (Steve) |
| 14 | 027 Benchmark: catalogue tasks (LLM work removed, learnability) | Measures 'comprehensive' instead of counting it | **Vision** (Steve), Goodhart guard |
| 15 | 020 Did-you-mean suggestions for failure codes and unknown names | Cheap compiler help for fix tasks; needs 017 to measure | **Model feedback** (Codex) |
| 16 | 007 CLI `phrase build` | The one command in bar 2 | plan |
| 17 | 023 Ordinary control flow: if/else and loops | Loops and if/else are ordinary; the demo app needs them | **Vision** (Steve) |
| 18 | 008 Close the language gaps the demo app exposes | Find what CRUD needs, via the Decider protocol and data | plan |
| 19 | 015 Proposal: optional `when` line (Decider first, then measured) | Token saving; needs the benchmark | **Model feedback** (agy) |
| 20 | 021 Proposal: consistent given/responds syntax (Decider first) | Language change; only with data | **Model feedback** (agy) |
| 21 | 025 Keyword catalogue v0: structure and ~12 seed keywords | The library half of 'spec and library'; the demo app is written from it | **Vision** (Steve) |
| 22 | 009 Demo app: Tasks API in .phrase | Bars 1, 3, 4 | plan |
| 23 | 010 End-to-end over real HTTP | Bar 5 | plan |
| 24 | 011 LLM-customer check | Bar 6 | plan |
| 25 | 012 Usable v1 acceptance gate | Bars 1-6 in one test; CI gate; milestone report | plan |
| 26 | 026 Catalogue dedup linter | Counter-metric for vocabulary bloat; after Usable v1 | **Vision** (Steve), Goodhart guard |
| 27 | 028 Second provider spike (TypeScript) | Proves 'any implementation language'; after Usable v1 | **Vision** (Steve) |
| 28 | 029 LLM-as-compiler with hash pinning | New targets without hand-written providers; after the spike | **Vision** (Steve) |

Ranks are the PM's and move at each retro; ideas are triaged by expected metric impact. Milestones are in `board/MILESTONES.md`.

## Estimate (as of 2026-10-25, after the vision ruling)

**27 cards remain on the board** (002 is Done; 016 is in progress). Of those, **22 are on the Usable v1 path**
(the original plan plus 022, 023 and 025 inserted for the vision) and **5 belong to Vocabulary v1** (024, 026, 027,
028, 029; cards 024 and 027 sit mid-board because they follow the benchmark). Strictly required for the six bars:
about **16**; the rest are improvements the benchmark will test (013, 014, 015, 017, 019, 020, 021). Expect **2-4
more** from retros. The emitter and CLI (003-007) are still the long pole; 008 is still the biggest unknown.
