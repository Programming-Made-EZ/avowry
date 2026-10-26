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

| Rank | Card | Why here | Origin |
|---|---|---|---|
| 1 | 002 Resolver malformed-input hardening (**Done**, 8ba3699) | The emitter and benchmark need a resolver that neither lies nor crashes | Grok review |
| 2 | 016 LLM benchmark harness v0 (**Done** on its written criteria; baseline invalid, see 032) | The instrument: every feature card needs a baseline first. v0 needs only parse + resolve | Steve (data-driven) |
| 3 | 032 Benchmark baseline is invalid: fix the harness, store a trustworthy baseline (**Done**, cfb1952; zero-shot baseline, see 037) | Every hypothesis card needs a trustworthy baseline; first card under experiment E1 (true mobbing). Decider D5 | Facilitator check of 016 |
| 4 | 036 Live mob dashboard (tooling, experiment E2; code in mob-defaults; **Done** 2026-10-26, local-only) | Stalls cost tens of minutes and Steve has to ask for progress; Decider D7 | **Steve (raw input)** |
| 5 | 018 Spec resolution defects (bug, from the Grok review) (**Done** 2026-10-26) | Specs are the truth the emitter turns into tests; accurate spec errors first. Decider D1 passed (3 Yes, 1 Support-it) | Review finding |
| 6 | 063 Loop: pick the next eligible card from the board (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 7 | 064 Loop: one durable loop at a time (lock, state, heartbeat) (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 8 | 065 Loop: run one card with a pinned toolchain (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 9 | 066 Loop: bounded retry, park and split on failure (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 10 | 067 Loop: on Done run the retro step and re-rank, then the next card (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 11 | 053 Docs: one diagnostics table, not two (docs only, split from 018) | Cheap; removes a contradiction LLMs read | Grok review, retro D18 |
| 12 | 062 Atomic runs: a card runs in a fresh worktree and ends landed or discarded (tooling, experiment E3) | Steve: all-or-nothing atomistic work makes retries easier and more distributable; Bishop's Law parallel class; Decider D22 | **Steve**, Decider D22 |
| 13 | 039 One phrase with one number input compiles to C# and its spec passes (thin slice; stash Decider first) | The first thin slice of the C# chain; proves the whole pipe on the smallest case (D18) | Retro D18, Steve |
| 14 | 040 C# compilation goes through the ITargetAdapter contract (thin slice) | Adapter contract right after the first slice, not a big-bang rename (D4, D18) | Retro D18 |
| 15 | 068 Loop: liveness, publish, STOP file and the Needs-Steve file (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 16 | 069 Loop: resume after a crash or reboot (logon task) (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 17 | 070 Loop: run every card atomically (land or discard) (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 18 | 037 Benchmark: docs in the prompt, and cold runs | The zero-shot baseline cannot be moved by documentation cards, and agy's 7 of 8 suggests warm sessions. Decider D11 | **Facilitator analysis** of the stored baseline |
| 19 | 054 Benchmark task 005 gets a failure-path spec (data; suite change, versioned) | Steve: specs must state consumer-observable outcomes; must land before 055; Decider + Goodhart rules | **Steve** (VISION ruling 5), Decider D20 |
| 20 | 055 A declared failure with no failure-path spec is an error, PH336 (thin slice) | The failure path is the most important behaviour to specify | **Steve**, Decider D20 |
| 21 | 056 An endpoint with an output cannot say `succeeds`, PH333 (thin slice) | A bare success on a value-bearing endpoint is no consumer result | **Steve**, Decider D20 |
| 22 | 057 Docs: the consumer-observable outcome rule is written down (docs only) | Docs after the code they describe; fixes card 014's wording | **Steve**, Decider D20 |
| 23 | 022 Keyword model ADR: what a keyword is | Decides the shape of everything the vision asks for; cheap; unblocks 023 and 025 | **Vision** (Steve) |
| 24 | 058 A validation phrase returns its validated value (first slice; contract via the keyword-model decision) | Steve's question (c): validation phrases need a consumer-visible result | **Steve**, Decider D20 |
| 25 | 013 Make GRAMMAR.md say what the resolver does | Cheap; first hypothesis card, measured by 016 | **Model feedback** (Codex, agy, Grok) |
| 26 | 019 Publish exact diagnostic message templates (docs) | All four models hit this gap; 018 fixes the message first; measured by 016 | **Model feedback** (Codex, Copilot, Grok, agy) |
| 27 | 014 Document the native C# environment, add passing-guard examples | Precedes 004: it fixes the `Fail` contract the emitter implements | **Model feedback** (Grok, agy, Codex, Copilot) |
| 28 | 041 A declared record becomes a usable C# record (thin slice) | Types; split from 003 | Retro D18 |
| 29 | 042 List, option and nested-record fields compile (thin slice) | Types; split from 003 | Retro D18 |
| 30 | 043 A phrase can fail with its declared code (thin slice) | Failures; split from 004 | Retro D18 |
| 31 | 044 A native C# block is placed verbatim and builds (thin slice) | Split from 004 | Retro D18 |
| 32 | 045 A composite phrase calls its leaf phrases in order (thin slice) | Split from 004 | Retro D18 |
| 33 | 047 A declared effect is supplied as an injected dependency (thin slice) | Split from 004 | Retro D18 |
| 34 | 048 One POST endpoint serves its successful response (thin slice) | Serves HTTP; split from 005 | Retro D18 |
| 35 | 049 A declared failure maps to its HTTP status, not a 500 (thin slice) | Split from 005 | Retro D18 |
| 36 | 046 Docs: GRAMMAR and README name the adapter contract and the HTTP status mapping (docs only) | Docs card, after the code it describes | Retro D18 |
| 37 | 050 Every spec outcome kind asserts correctly, and a wrong one names itself (thin slice) | Split from 006; CompileEndpointToCSharp goes green | Retro D18 |
| 38 | 017 Benchmark v1: score by running specs | Turns pass rate into real behavior; re-baselines | Steve (data-driven) |
| 39 | 024 Pattern discovery (mine real code and LLM tasks) | Vocabulary grows by evidence; right after the benchmark can measure it | **Vision** (Steve) |
| 40 | 027 Benchmark: catalogue tasks (LLM work removed, learnability) | Measures 'comprehensive' instead of counting it | **Vision** (Steve), Goodhart guard |
| 41 | 060 Catalogue tasks for concurrent work and event publication (data; suite change, versioned) | Measures whether LLMs repeatedly produce these patterns; the keyword cards wait for it | **Steve**, Decider D21 |
| 42 | 020 Did-you-mean suggestions for failure codes and unknown names | Cheap compiler help for fix tasks; needs 017 to measure | **Model feedback** (Codex) |
| 43 | 051 phrase build compiles one file into output projects (thin slice) | Bar 2's first command; split from 007 | Retro D18 |
| 44 | 052 phrase build reports errors as file:line:col and names its target (thin slice) | Split from 007 | Retro D18 |
| 45 | 023 Ordinary control flow: if/else and loops | Loops and if/else are ordinary; the demo app needs them | **Vision** (Steve) |
| 46 | 033 Generic type parameters on phrases | 4/4 models: reusable keywords need `phrase<T>`; the base library cannot start without it. Decider D6 | **Model feedback** (all four) |
| 47 | 034 Behavioral holes and spec test doubles | 4/4 models: retry, transaction and idempotent-execute need an action as input and scripted doubles. Decider D6 | **Model feedback** (all four) |
| 48 | 059 ADR: deterministic asynchronous and event semantics (decision; not on the Usable v1 path) | Steve: async and events may be needed; effects plus keywords, not grammar; evidence first (D21) | **Steve**, Decider D21 |
| 49 | 025 Base library v0: ~12 seed keywords with C# adapter implementations | The library half of 'spec and library'; the demo app is written from it | **Vision** (Steve) |
| 50 | 008 Close the language gaps the demo app exposes | Find what CRUD needs, via the Decider protocol and data | plan |
| 51 | 015 Proposal: optional `when` line (Decider first, then measured) | Token saving; needs the benchmark | **Model feedback** (agy) |
| 52 | 021 Proposal: consistent given/responds syntax (Decider first) | Language change; only with data | **Model feedback** (agy) |
| 53 | 035 Proposal: parameterizable failure codes (Decider first) | Removes per-domain guard clones; changes the failure model. Decider D6 | **Model feedback** (agy, Copilot) |
| 54 | 009 Demo app: Tasks API in .phrase | Bars 1, 3, 4 | plan |
| 55 | 010 End-to-end over real HTTP | Bar 5 | plan |
| 56 | 011 LLM-customer check | Bar 6 | plan |
| 57 | 012 Usable v1 acceptance gate | Bars 1-6 in one test; CI gate; milestone report | plan |
| 58 | 038 Dogfood: the mob status API in Phrase (second demo app, after the Usable v1 gate) | Steve: 'a good starter app'; Decider D16: Tasks stays the bar-1 app, this is a dogfood card after it; dashboard follow-ups are low urgency (30-minute digests cover visibility) | **Steve** (Decider D16) |
| 59 | 026 Catalogue dedup linter | Counter-metric for vocabulary bloat; after Usable v1 | **Vision** (Steve), Goodhart guard |
| 60 | 061 Spike: a scripted scheduler, fake clock and event capture make an async spec deterministic | Proves the spec model before any keyword exists | **Steve**, Decider D21 |
| 61 | 028 Second adapter spike (TypeScript) | Proves 'any implementation language'; after Usable v1 | **Vision** (Steve) |
| 62 | 029 LLM-as-compiler with hash pinning | New targets without hand-written providers; after the spike | **Vision** (Steve) |
| 63 | 030 Synthesize Phrase from existing code (spike) | FUTURE (M5): after Usable v1 and Vocabulary v1; needs the base library | **Vision** (Steve, ruling 1) |
| 64 | 031 Human <-> LLM form converter (spike) | FUTURE (M6): pairs with 030; reading confirmed by Steve (two-way) | **Vision** (Steve, ruling 4) |

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
