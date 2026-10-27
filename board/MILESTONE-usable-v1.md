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
| 6 | 073 Board: create the private Phrase project and its fields (**Done** 2026-10-26) (tooling, thin slice) | Steve: the board moves to GitHub Projects (a real board on his phone) before more loop cards are built on the file picker; Decider D25 | **Steve**, Decider D25 |
| 7 | 074 Board: migrate every card file into one issue (**Done** 2026-10-26) | Steve: the board moves to GitHub Projects (a real board on his phone) before more loop cards are built on the file picker; Decider D25 | **Steve**, Decider D25 |
| 8 | 075 Board: put each issue on the project with its Status, Rank and fields (tooling, thin slice) | Steve: the board moves to GitHub Projects (a real board on his phone) before more loop cards are built on the file picker; Decider D25 | **Steve**, Decider D25 |
| 9 | 079 Board: sync a card's changed text to its issue body (tooling, thin slice) | Steve: a role is not a persona; stories name a persona and a consumer-value so-that; Decider D27 | **Steve**, Decider D27 |
| 10 | 076 Loop: the picker reads the GitHub board (card 063's logic, new source) (tooling, thin slice) | Steve: the board moves to GitHub Projects (a real board on his phone) before more loop cards are built on the file picker; Decider D25 | **Steve**, Decider D25 |
| 11 | 077 Board: the project is the single source; board files become a generated read-only archive (tooling, thin slice) | Steve: the board moves to GitHub Projects (a real board on his phone) before more loop cards are built on the file picker; Decider D25 | **Steve**, Decider D25 |
| 12 | 064 Loop: one durable loop at a time (lock, state, heartbeat) (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 13 | 065 Loop: run one card with a pinned toolchain (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 14 | 066 Loop: bounded retry, park and split on failure (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 15 | 078 Ready check: reject a role-only story before the product manager is asked (tooling, thin slice) | Steve: a role is not a persona; stories name a persona and a consumer-value so-that; Decider D27 | **Steve**, Decider D27 |
| 16 | 067 Loop: on Done run the retro step and re-rank, then the next card (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 17 | 053 Docs: one diagnostics table, not two (docs only, split from 018) | Cheap; removes a contradiction LLMs read | Grok review, retro D18 |
| 18 | 062 Atomic runs: a card runs in a fresh worktree and ends landed or discarded (tooling, experiment E3) | Steve: all-or-nothing atomistic work makes retries easier and more distributable; Bishop's Law parallel class; Decider D22 | **Steve**, Decider D22 |
| 19 | 039 One phrase with one number input compiles to C# and its spec passes (thin slice; stash Decider first) | The first thin slice of the C# chain; proves the whole pipe on the smallest case (D18) | Retro D18, Steve |
| 20 | 040 C# compilation goes through the ITargetAdapter contract (thin slice) | Adapter contract right after the first slice, not a big-bang rename (D4, D18) | Retro D18 |
| 21 | 068 Loop: liveness, publish, STOP file and the Needs-Steve file (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 22 | 072 Loop: pace the work against the finite driver allotments (tooling, thin slice) | Steve: the team finds its own features and the loop never ends; stops only for STOP, a one-way door or allotments out; Decider D24 | **Steve**, Decider D24 |
| 23 | 069 Loop: resume after a crash or reboot (logon task) (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 24 | 070 Loop: run every card atomically (land or discard) (tooling, thin slice) | Steve: the loop belongs in the tooling, not in chat turns; fixes the recurring idle-gap defect (P1); Decider D23 | **Steve**, Decider D23 |
| 25 | 037 Benchmark: docs in the prompt, and cold runs | The zero-shot baseline cannot be moved by documentation cards, and agy's 7 of 8 suggests warm sessions. Decider D11 | **Facilitator analysis** of the stored baseline |
| 26 | 054 Benchmark task 005 gets a failure-path spec (data; suite change, versioned) | Steve: specs must state consumer-observable outcomes; must land before 055; Decider + Goodhart rules | **Steve** (VISION ruling 5), Decider D20 |
| 27 | 055 A declared failure with no failure-path spec is an error, PH336 (thin slice) | The failure path is the most important behaviour to specify | **Steve**, Decider D20 |
| 28 | 056 An endpoint with an output cannot say `succeeds`, PH333 (thin slice) | A bare success on a value-bearing endpoint is no consumer result | **Steve**, Decider D20 |
| 29 | 057 Docs: the consumer-observable outcome rule is written down (docs only) | Docs after the code they describe; fixes card 014's wording | **Steve**, Decider D20 |
| 30 | 022 Keyword model ADR: what a keyword is | Decides the shape of everything the vision asks for; cheap; unblocks 023 and 025 | **Vision** (Steve) |
| 31 | 058 A validation phrase returns its validated value (first slice; contract via the keyword-model decision) | Steve's question (c): validation phrases need a consumer-visible result | **Steve**, Decider D20 |
| 32 | 013 Make GRAMMAR.md say what the resolver does | Cheap; first hypothesis card, measured by 016 | **Model feedback** (Codex, agy, Grok) |
| 33 | 019 Publish exact diagnostic message templates (docs) | All four models hit this gap; 018 fixes the message first; measured by 016 | **Model feedback** (Codex, Copilot, Grok, agy) |
| 34 | 014 Document the native C# environment, add passing-guard examples | Precedes 004: it fixes the `Fail` contract the emitter implements | **Model feedback** (Grok, agy, Codex, Copilot) |
| 35 | 041 A declared record becomes a usable C# record (thin slice) | Types; split from 003 | Retro D18 |
| 36 | 042 List, option and nested-record fields compile (thin slice) | Types; split from 003 | Retro D18 |
| 37 | 043 A phrase can fail with its declared code (thin slice) | Failures; split from 004 | Retro D18 |
| 38 | 044 A native C# block is placed verbatim and builds (thin slice) | Split from 004 | Retro D18 |
| 39 | 045 A composite phrase calls its leaf phrases in order (thin slice) | Split from 004 | Retro D18 |
| 40 | 047 A declared effect is supplied as an injected dependency (thin slice) | Split from 004 | Retro D18 |
| 41 | 048 One POST endpoint serves its successful response (thin slice) | Serves HTTP; split from 005 | Retro D18 |
| 42 | 049 A declared failure maps to its HTTP status, not a 500 (thin slice) | Split from 005 | Retro D18 |
| 43 | 046 Docs: GRAMMAR and README name the adapter contract and the HTTP status mapping (docs only) | Docs card, after the code it describes | Retro D18 |
| 44 | 050 Every spec outcome kind asserts correctly, and a wrong one names itself (thin slice) | Split from 006; CompileEndpointToCSharp goes green | Retro D18 |
| 45 | 017 Benchmark v1: score by running specs | Turns pass rate into real behavior; re-baselines | Steve (data-driven) |
| 46 | 024 Pattern discovery (mine real code and LLM tasks) | Vocabulary grows by evidence; right after the benchmark can measure it | **Vision** (Steve) |
| 47 | 027 Benchmark: catalogue tasks (LLM work removed, learnability) | Measures 'comprehensive' instead of counting it | **Vision** (Steve), Goodhart guard |
| 48 | 060 Catalogue tasks for concurrent work and event publication (data; suite change, versioned) | Measures whether LLMs repeatedly produce these patterns; the keyword cards wait for it | **Steve**, Decider D21 |
| 49 | 020 Did-you-mean suggestions for failure codes and unknown names | Cheap compiler help for fix tasks; needs 017 to measure | **Model feedback** (Codex) |
| 50 | 051 phrase build compiles one file into output projects (thin slice) | Bar 2's first command; split from 007 | Retro D18 |
| 51 | 052 phrase build reports errors as file:line:col and names its target (thin slice) | Split from 007 | Retro D18 |
| 52 | 023 Ordinary control flow: if/else and loops | Loops and if/else are ordinary; the demo app needs them | **Vision** (Steve) |
| 53 | 033 Generic type parameters on phrases | 4/4 models: reusable keywords need `phrase<T>`; the base library cannot start without it. Decider D6 | **Model feedback** (all four) |
| 54 | 034 Behavioral holes and spec test doubles | 4/4 models: retry, transaction and idempotent-execute need an action as input and scripted doubles. Decider D6 | **Model feedback** (all four) |
| 55 | 059 ADR: deterministic asynchronous and event semantics (decision; not on the Usable v1 path) | Steve: async and events may be needed; effects plus keywords, not grammar; evidence first (D21) | **Steve**, Decider D21 |
| 56 | 025 Base library v0: ~12 seed keywords with C# adapter implementations | The library half of 'spec and library'; the demo app is written from it | **Vision** (Steve) |
| 57 | 008 Close the language gaps the demo app exposes | Find what CRUD needs, via the Decider protocol and data | plan |
| 58 | 015 Proposal: optional `when` line (Decider first, then measured) | Token saving; needs the benchmark | **Model feedback** (agy) |
| 59 | 021 Proposal: consistent given/responds syntax (Decider first) | Language change; only with data | **Model feedback** (agy) |
| 60 | 035 Proposal: parameterizable failure codes (Decider first) | Removes per-domain guard clones; changes the failure model. Decider D6 | **Model feedback** (agy, Copilot) |
| 61 | 009 Demo app: Tasks API in .phrase | Bars 1, 3, 4 | plan |
| 62 | 010 End-to-end over real HTTP | Bar 5 | plan |
| 63 | 011 LLM-customer check | Bar 6 | plan |
| 64 | 012 Usable v1 acceptance gate | Bars 1-6 in one test; CI gate; milestone report | plan |
| 65 | 038 Dogfood: the mob status API in Phrase (second demo app, after the Usable v1 gate) | Steve: 'a good starter app'; Decider D16: Tasks stays the bar-1 app, this is a dogfood card after it; dashboard follow-ups are low urgency (30-minute digests cover visibility) | **Steve** (Decider D16) |
| 66 | 026 Catalogue dedup linter | Counter-metric for vocabulary bloat; after Usable v1 | **Vision** (Steve), Goodhart guard |
| 67 | 061 Spike: a scripted scheduler, fake clock and event capture make an async spec deterministic | Proves the spec model before any keyword exists | **Steve**, Decider D21 |
| 68 | 028 Second adapter spike (TypeScript) | Proves 'any implementation language'; after Usable v1 | **Vision** (Steve) |
| 69 | 029 LLM-as-compiler with hash pinning | New targets without hand-written providers; after the spike | **Vision** (Steve) |
| 70 | 030 Synthesize Phrase from existing code (spike) | FUTURE (M5): after Usable v1 and Vocabulary v1; needs the base library | **Vision** (Steve, ruling 1) |
| 71 | 031 Human <-> LLM form converter (spike) | FUTURE (M6): pairs with 030; reading confirmed by Steve (two-way) | **Vision** (Steve, ruling 4) |

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
