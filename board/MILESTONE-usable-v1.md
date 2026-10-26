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

**Detection:** CI runs a `usable-v1` job (`scripts/usable-v1-report.py`). While the milestone is pending the job is GREEN and reports `Usable v1 not reached: X of 6 criteria`; it goes RED only if a bar listed in `board/usable-v1-ratchet.json` regresses (a card adds its bar to the ratchet in the commit that makes the bar's real test pass), or if the report cannot be produced (Deciders D9 and D10). Each bar is tested for what it says, never by a file-existence probe; bars not wired yet count as not met. When the test first passes on main, the mob reports a separate line: `USABLE v1 REACHED`, with the demo app's path and how to run it.

## Cards, in PM rank order (the single source of ranks)

_Re-ranked 2026-10-26 on Steve's direction: product first. The Usable v1 path is in dependency order at the top; process and tooling cards come after it unless a bar depends on them._

| Rank | Card | Why here | Origin |
|---|---|---|---|
| 1 | 002 Resolver malformed-input hardening (**Done**, 8ba3699) | The emitter and benchmark need a resolver that neither lies nor crashes | Grok review |
| 2 | 016 LLM benchmark harness v0 (**Done** on its written criteria; baseline invalid, see 032) | The instrument: every feature card needs a baseline first. v0 needs only parse + resolve | Steve (data-driven) |
| 3 | 032 Benchmark baseline is invalid: fix the harness, store a trustworthy baseline (**Done**, cfb1952; zero-shot baseline, see 037) | Every hypothesis card needs a trustworthy baseline; first card under experiment E1 (true mobbing). Decider D5 | Facilitator check of 016 |
| 4 | 036 Live mob dashboard (tooling, experiment E2; **in progress**: finish the local-only version, then stop dashboard work) | Stalls cost tens of minutes and Steve has to ask for progress; Decider D7 | **Steve (raw input)** |
| 5 | 003 Adapter contract and C# adapter: types and runtime | First on the Usable v1 path (Steve: product first). The stash Decider (salvage or drop the unreviewed emitter start) happens at its start | plan |
| 6 | 004 C# adapter: phrases, effects, failures | Behavior before HTTP | plan |
| 7 | 005 C# adapter: FastEndpoints endpoints and Program | Serves HTTP | plan |
| 8 | 018 Spec resolution defects (bug, from the Grok review) | Spec errors must be accurate before specs become tests (before the specs card) | Review finding |
| 9 | 006 C# adapter: specs as xUnit; first acceptance test green | Phrase compiles its first program | plan |
| 10 | 007 CLI `phrase build` | The one command in bar 2 | plan |
| 11 | 023 Ordinary control flow: if/else and loops | Loops and if/else are ordinary; the demo app needs them | **Vision** (Steve) |
| 12 | 022 Keyword model ADR: what a keyword is | The keyword model ADR; needed before the base library | **Vision** (Steve) |
| 13 | 025 Base library v0: ~12 seed keywords with C# adapter implementations | The library half of 'spec and library'; the demo app is written from it. If a retry-style keyword needs generics or behavioral holes, pull 033/034 ahead first | **Vision** (Steve) |
| 14 | 008 Close the language gaps the demo app exposes | Find what CRUD needs, via the Decider protocol and data | plan |
| 15 | 009 Demo app: Tasks API in .phrase | Bars 1, 3, 4 | plan |
| 16 | 010 End-to-end over real HTTP | Bar 5 | plan |
| 17 | 013 Make GRAMMAR.md say what the resolver does | Bar 6 (a model extends the app from the docs) needs honest docs: before the LLM-customer check | **Model feedback** (Codex, agy, Grok) |
| 18 | 014 Document the native C# environment, add passing-guard examples | Bar 6 needs documented native code and examples: before the LLM-customer check | **Model feedback** (Grok, agy, Codex, Copilot) |
| 19 | 019 Publish exact diagnostic message templates (docs) | Bar 6 needs published diagnostics: before the LLM-customer check | **Model feedback** (Codex, Copilot, Grok, agy) |
| 20 | 011 LLM-customer check | Bar 6 | plan |
| 21 | 012 Usable v1 acceptance gate | Bars 1-6 in one test; CI gate; milestone report | plan |
| 22 | 037 Benchmark: docs in the prompt, and cold runs | Only if a Usable v1 bar actually depends on it; otherwise after the Usable v1 path (Steve, 2026-10-26). The benchmark is not one of the six bars | **Facilitator analysis** of the stored baseline |
| 23 | 017 Benchmark v1: score by running specs | Turns pass rate into real behavior; re-baselines | Steve (data-driven) |
| 24 | 020 Did-you-mean suggestions for failure codes and unknown names | Cheap compiler help for fix tasks; needs 017 to measure | **Model feedback** (Codex) |
| 25 | 024 Pattern discovery (mine real code and LLM tasks) | Vocabulary grows by evidence; right after the benchmark can measure it | **Vision** (Steve) |
| 26 | 027 Benchmark: catalogue tasks (LLM work removed, learnability) | Measures 'comprehensive' instead of counting it | **Vision** (Steve), Goodhart guard |
| 27 | 033 Generic type parameters on phrases | 4/4 models: reusable keywords need `phrase<T>`; the base library cannot start without it. Decider D6 | **Model feedback** (all four) |
| 28 | 034 Behavioral holes and spec test doubles | 4/4 models: retry, transaction and idempotent-execute need an action as input and scripted doubles. Decider D6 | **Model feedback** (all four) |
| 29 | 015 Proposal: optional `when` line (Decider first, then measured) | Token saving; needs the benchmark | **Model feedback** (agy) |
| 30 | 021 Proposal: consistent given/responds syntax (Decider first) | Language change; only with data | **Model feedback** (agy) |
| 31 | 035 Proposal: parameterizable failure codes (Decider first) | Removes per-domain guard clones; changes the failure model. Decider D6 | **Model feedback** (agy, Copilot) |
| 32 | 026 Catalogue dedup linter | Counter-metric for vocabulary bloat; after Usable v1 | **Vision** (Steve), Goodhart guard |
| 33 | 028 Second adapter spike (TypeScript) | Proves 'any implementation language'; after Usable v1 | **Vision** (Steve) |
| 34 | 029 LLM-as-compiler with hash pinning | New targets without hand-written providers; after the spike | **Vision** (Steve) |
| 35 | 030 Synthesize Phrase from existing code (spike) | FUTURE (M5): after Usable v1 and Vocabulary v1; needs the base library | **Vision** (Steve, ruling 1) |
| 36 | 031 Human <-> LLM form converter (spike) | FUTURE (M6): pairs with 030; reading confirmed by Steve (two-way) | **Vision** (Steve, ruling 4) |

Ranks are the PM's and move at each retro; ideas are triaged by expected metric impact. Milestones are in `board/MILESTONES.md`.

## Estimate (as of 2026-10-25, after the vision ruling)

**33 cards remain on the board** (002, 016 and 032 are Done; 036 is tooling; 037 is new) (002 and 016 are Done; 032, 033, 034 and 035 are new). Of those, **26 are on the Usable v1 path**
(the original plan plus 022, 023 and 025 inserted for the vision) and **5 belong to Vocabulary v1** (024, 026, 027,
028, 029; plus 2 FUTURE cards, 030 and 031, after both; cards 024 and 027 sit mid-board because they follow the benchmark). Strictly required for the six bars:
about **16**; the rest are improvements the benchmark will test (013, 014, 015, 017, 019, 020, 021). Expect **2-4
more** from retros. The emitter and CLI (003-007) are still the long pole; 008 is still the biggest unknown.

## Roadmap after Usable v1 (displaces nothing)

1. **Usable v1** (M1): the six bars.
2. **Vocabulary v1** (M4): the catalogue measured by LLM work removed.
3. **Synthesis** (M5, future): Phrase synthesized from existing code (card 030).
4. **Human view** (M6, future): the human-friendly converter (card 031).
