# Milestone: Usable v1

Principle: the product owner needs one unmistakable signal for when the language is actually usable to build an application, so the milestone is defined by a test, not by opinion.

**Usable v1 means one automated end-to-end test passes on main.** That test is
`tests/Phrase.Acceptance.Tests/UsableV1.cs` (trait `Milestone=UsableV1`). It checks, in order:

1. **A real app is written entirely in .phrase:** `demo/tasks/Tasks.phrase`, a CRUD API with 3-5 endpoints, validation and a failure path (beyond the orders fixture).
2. **One CLI command compiles it:** `phrase build demo/tasks/Tasks.phrase -o <dir>` produces C#/FastEndpoints.
3. **`dotnet build` passes with zero warnings** on the generated projects.
4. **The generated xUnit tests from the specs all pass,** one test per spec.
5. **The app runs and real HTTP calls behave as the specs say.**
6. **An LLM customer can extend it:** a fresh-context model, given only GRAMMAR.md and the docs, adds a new endpoint with specs; it compiles, builds clean and all specs pass. Evidence is in `docs/usable-v1/llm-check-*.json` (model, spec totals, build warnings, GRAMMAR and CHEATSHEET hashes, result). The test fails if no evidence is successful and complete against both current documents.

**Detection:** CI runs a `usable-v1` job (`scripts/usable-v1-report.py`). All six bars are ratcheted, so the job goes RED if any criterion regresses or if the report cannot be produced (Deciders D9 and D10). Each bar tests the behavior it names. A successful report says `6 of 6 criteria met` and prints a separate line: `USABLE v1 REACHED`, with the demo app's path and how to run it.

## Cards, in PM rank order (the single source of ranks)

<!-- board-table:start -->
<!-- board-table-sha256:7a29bc37db9e2b33ecebe044bc11865afccbaa50c3faf9d900d957751ff92a24 -->
| Rank | Card | Status | Depends on |
|---|---|---|---|
| 1 | 002 Complete resolver malformed-input hardening | Done | 001 |
| 2 | 016 LLM benchmark harness v0: measure how well models use Phrase | Done | none |
| 3 | 032 Benchmark baseline is invalid: fix the harness and store a trustworthy baseline | Done |  |
| 4 | 036 Live mob dashboard: see every seat's status from a phone | Done |  |
| 5 | 018 Spec resolution defects from the independent review (code only) | Done | none |
| 6 | 073 Board: create the private Phrase project and its fields | Done | none |
| 7 | 074 Board: migrate every card file into one issue | Done | 073 |
| 8 | 075 Board: put each issue on the project with its Status, Rank and fields | Done | 074 |
| 9 | 079 Board: sync a card's changed text to its issue body (one source at a time) | Done | 074 |
| 10 | 080 Board: show a card's Shape and what it Enables | Done | 075, 079 |
| 11 | 076 Loop: the picker reads the GitHub board (card 063's logic, new source) | Done | 075 (063 is superseded by this card) |
| 12 | 077 Board: the project is the single source; board files become a generated read-only archive | Done | 075, 076 |
| 13 | 064 Loop: one durable loop at a time (lock, state, heartbeat) | Done | 076 (the picker reads the GitHub board; D25) |
| 14 | 093 Record the sealed holdout hashes before the baseline starts | Done | none |
| 15 | 086 Plain C# baseline suite: the eight development tasks as C# twins and a runner | Done | 093 (Done: the sealed holdout hashes are recorded, D36) |
| 16 | 095 Plain C# twins for the read and modify tasks | Done | 086 |
| 17 | 096 Plain C# twins for the fix tasks, and a suite completeness check | Done | 095 |
| 18 | 097 Plain C# arm: run cold, exactly like the Phrase arm | Done | 096 |
| 19 | 094 Run the plain C# baseline: N=5 runs per model on the development tasks | Done | 086, 095, 096, 097 |
| 20 | 084 Reference target builds and every orders spec passes | Done | none |
| 21 | 085 A missing Phrase reply is a model failure, not a harness error | Done | none |
| 22 | 039 One phrase with one number input compiles to C# and its spec passes | Done | none (the stash Decider runs first) |
| 23 | 040 C# compilation goes through the ITargetAdapter contract | Done | 039 |
| 24 | 041 A declared record becomes a usable C# record | Done | 040 |
| 25 | 042 List fields compile | Done | 041 |
| 26 | 043 A phrase can fail with its declared code | Done | 040 |
| 27 | 044 A native C# block is placed verbatim and builds | Done | 043 |
| 28 | 045 A composite phrase calls its leaf phrases in order | Done | 043 |
| 29 | 047 A declared effect is supplied as an injected dependency | Done | 043 |
| 30 | 048 One POST endpoint serves its successful response | Done | 045, 047 |
| 31 | 049 A declared failure maps to its HTTP status, not a 500 | Done | 048 |
| 32 | 050 Every spec outcome kind asserts correctly, and a wrong one names itself | Done | 049 |
| 33 | 087 orders.phrase compiles and its generated specs pass | Done | 039, 040, 041, 043, 044, 045, 048, 049, 050, 084 |
| 34 | 098 Phrase arm scores executed outcomes for the write tasks | Done | 087, 097 |
| 35 | 099 Phrase arm scores executed outcomes for the read and modify tasks | Done | 098 |
| 36 | 100 Phrase arm scores executed outcomes for the fix tasks | Done | 099 |
| 37 | 104 A compact Phrase cheat sheet for model prompts | Done | 100 |
| 38 | 105 Pilot the cheat sheet against full documentation and freeze the choice | Done | 104 |
| 39 | 101 Run the Phrase development baseline at the specs-passed level | Done | 100, 094 |
| 40 | 110 Plain C# arm scores endpoint responds outcomes with status | Done | 094 |
| 41 | 111 Phrase arm scores endpoint responds outcomes with status | Done | 110 |
| 42 | 112 Turn a holdout file into both arms' task folders with one wrapper | Done | 111 |
| 43 | 113 Holdout run rows stay private: numbers are published, replies are not | Done | 111 |
| 44 | 114 A model that outlives its timeout is stopped with its whole process tree | Done | 113 |
| 45 | 102 Head-to-head on the sealed holdouts: both arms at N=5 | Done | 101 |
| 46 | 103 Head-to-head verdict page: WIN, LOSS, TIE or INCONCLUSIVE | Done | 102 |
| 47 | 051 phrase build compiles one file into output projects | Done | 040 |
| 48 | 141 Extension map: the layering ADR and the audit of the core | Done | 051 |
| 49 | 142 Web extension boundary: the compiler partitions core and web | Done | 141 |
| 50 | 132 A persistence effect with a generated in-memory implementation | Done | 047 |
| 51 | 131 GET endpoints: a collection and one item by route parameter | Done | 132 |
| 52 | 133 PUT and DELETE endpoints: update and remove a stored item | Done | 131 |
| 53 | 135 Tasks demo app: a CRUD API written entirely in Phrase | Done | 051, 131, 132, 133 |
| 54 | 136 Real HTTP behaves as the specs say | Done | 135 |
| 55 | 137 LLM-customer check: a model adds an endpoint from the docs alone | Done | 135, 136 |
| 56 | 138 Usable v1: the acceptance test passes on main and gates CI | Done | 135, 136, 137 |
| 57 | 152 Library bindings and project profile: the layering ADR | Done | 138 |
| 58 | 158 Tasks demo README: how to run the demo yourself | Done | none |
| 59 | 159 A validation step in Phrase: ensure a condition or fail with a declared name | Done | 138 |
| 60 | 160 A record update form: a copy of a record with one field changed | Done | 159 |
| 61 | 161 Generated failures and endpoint classes point back to the Phrase source | Ready | 138 |
| 62 | 162 Fold the pattern steps into the grammar and cheat sheet: re-freeze and rerun the LLM check | Ready | 160 |
| 63 | 163 Generated endpoint files are named *.cs.cs: decide and document the convention | Ready | none |
| 64 | 153 Verdict page states the success criteria | Ready | 138 |
| 65 | 155 Verdict page as a judgment backed by data | Ready | 153 |
| 66 | 156 Personas capture the same ten fields | Ready | 154 |
| 67 | 154 Docs state principles and rationale, not who said them | Done | 153 |
| 68 | 145 Store operations as Phrase steps, not native code | Ready | 135 |
| 69 | 146 Validation rules as Phrase: not blank and a maximum length | Ready | 145 |
| 70 | 147 A list endpoint without a fake flag | Ready | 145 |
| 71 | 148 Composite steps: a phrase calls a phrase and changes one field of a record | Ready | 145 |
| 72 | 134 ZOMBIES spec-coverage warnings | Ready | 138 |
| 73 | 149 Undetected-defect rate: planted defects against Phrase specs and model-written C# tests | Ready | 134 |
| 74 | 150 Intent recovery: can a fresh reader state what each unit must do | Ready | 134 |
| 75 | 151 Review speed and accuracy: approving and rejecting AI-written changes | Ready | 134 |
| 76 | 139 Loop step: re-probe unavailable drivers and restore them | Ready | 064 |
| 77 | 140 Cheat sheet plus library index: the hybrid prompt, measured | Ready | 134 |
| 78 | 143 Split the documentation: a smaller core cheat sheet and a web sheet | Ready | 142, 140 |
| 79 | 144 Rerun Copilot's unavailable holdout rows after its quota returns | Ready | 102 |
| 80 | 106 Mutation check: the generated C# specs catch broken implementations | Done | 087 |
| 81 | 107 Spec to code with a model: does a Phrase spec get verified-correct code more reliably | Ready | 106 |
| 82 | 108 A second target's Specs project from the same specs | Ready | 106 |
| 83 | 109 C# implementation emission one phrase kind at a time | Ready | 108 |
| 84 | 115 Which cheaper models can each CLI run headless and cold | Ready | 102 |
| 85 | 116 Pre-register the cheaper-model experiment before any row | Ready | 115 |
| 86 | 117 Run the cheaper models in both arms | Ready | 116 |
| 87 | 118 Cost per passing task: the cheaper-model report | Ready | 117 |
| 88 | 119 Generate matching Phrase and C# codebases of 1, 10 and 50 endpoints | Done | 111 |
| 89 | 130 Plain C# codebases in ordinary hand-written style | Done | 119 |
| 90 | 120 Pre-register the codebase-size experiment before any row | Ready | 119, 116 |
| 91 | 121 Snapshot a real project slice privately and scrub it | Superseded/Withdrawn | 120 |
| 92 | 122 Port the real slice to Phrase in the private folder | Superseded/Withdrawn | 121 |
| 93 | 123 Run the codebase-size experiment in both arms | Ready | 120 |
| 94 | 124 Codebase-size report: how cost and pass rate change with size | Ready | 123 |
| 95 | 125 Choose and pin public C# repositories for the real-code tier | Ready | 120 |
| 96 | 126 Port a representative slice of the pinned public repo to Phrase | Ready | 125 |
| 97 | 127 Write the growth-study backlog and pre-register the study | Ready | 120 |
| 98 | 128 Run the growth study in both arms from shared checkpoints | Ready | 127, 119 |
| 99 | 129 Growth-study curve: cost and pass rate against codebase size | Ready | 128 |
| 100 | 090 Benchmark: a docs-in-context mode that gives a model the real docs | Done | 085 |
| 101 | 091 Benchmark: cold runs, a fresh session in an empty folder for every attempt | Done | 090 |
| 102 | 092 Benchmark: scrub a stored reply before cutting it to 200 characters | Done | 085 |
| 103 | 037 Benchmark: docs in the prompt, and cold runs | Done | 090, 091, 092 (sliced: docs-in-context mode, cold runs and the reply-head scrub moved to those cards; this card is now the baseline RUN) |
| 104 | 088 Head-to-head: Phrase vs plain C# on the frozen bar | Superseded/Withdrawn | 085, 086, 094, 087, 037 |
| 105 | 089 Option and nested-record fields compile | Ready | 042 |
| 106 | 065 Loop: run one card with a pinned toolchain | Ready | 064 |
| 107 | 066 Loop: bounded retry, park and split on failure | Ready | 065 |
| 108 | 078 Ready check: reject a role-only story before the product manager is asked | Ready | 065 |
| 109 | 067 Loop: on Done run the retro step and re-rank, then the next card | Ready | 066 |
| 110 | 053 Docs: one diagnostics table, not two | Ready | none |
| 111 | 062 Atomic runs: a card runs in a fresh worktree and ends landed or discarded | Ready | none |
| 112 | 068 Loop: liveness, publish, STOP file and the Needs-product-owner file | Ready | 067 |
| 113 | 072 Loop: pace the work against the finite driver allotments | Ready | 066 |
| 114 | 069 Loop: resume after a crash or reboot (logon task) | Ready | 068 |
| 115 | 070 Loop: run every card atomically (land or discard) | Ready | 062, 067 |
| 116 | 054 Benchmark task 005 gets a failure-path spec (suite change, versioned) | Ready | none |
| 117 | 055 A declared failure with no failure-path spec is an error (PH336) | Superseded/Withdrawn | 054, 018 |
| 118 | 056 An endpoint with an output cannot say `succeeds` (PH333) | Ready | 134 |
| 119 | 057 Docs: the consumer-observable outcome rule is written down | Superseded/Withdrawn | 055 |
| 120 | 022 Keyword model ADR: what a keyword is | Ready | none |
| 121 | 058 A validation phrase returns its validated value (first slice, language decision first) | Ready | 022, 134 |
| 122 | 013 Make GRAMMAR.md say what the resolver does | Ready | none |
| 123 | 019 Publish exact diagnostic message templates in GRAMMAR.md | Ready | 018 |
| 124 | 014 Document the native C# environment and show passing-guard examples | Ready | none |
| 125 | 046 Docs: GRAMMAR and README name the adapter contract and the HTTP status mapping | Ready | 040, 049 |
| 126 | 017 Benchmark v1: score by running the generated specs | Ready | 016, 050 |
| 127 | 024 Pattern discovery: find the recurring patterns worth a keyword | Ready | 017 |
| 128 | 027 Benchmark: catalogue tasks that measure LLM work removed and learnability | Ready | 017, 024 |
| 129 | 060 Catalogue tasks for concurrent work and event publication (measure the demand) | Ready | 022, 033, 034 (language foundations) |
| 130 | 081 Measure stored and published outcomes under today's grammar | Ready | 037, 054 |
| 131 | 020 Did-you-mean suggestions for failure codes and other unknown names | Ready | 017 |
| 132 | 052 phrase build reports errors as file:line:col and names its target | Ready | 051 |
| 133 | 023 Ordinary control flow: if/else and loops | Ready | 022, 050 |
| 134 | 033 Generic type parameters on phrases | Ready | 022, 032, 040 |
| 135 | 034 Behavioral holes and spec test doubles | Ready | 033 |
| 136 | 059 ADR: deterministic asynchronous and event semantics | Ready | 022, 033, 034 (language foundations) |
| 137 | 082 ADR: a spec actor is who observes an outcome | Ready | 022, 058, 059, 081 |
| 138 | 025 Base library v0: structure and a seed of about 12 keywords, with C# adapter implementations | Ready | 024, 034, 050 |
| 139 | 008 Close the language gaps the demo app exposes | Superseded/Withdrawn | 025, 050, 052 |
| 140 | 015 Proposal: make the spec `when` line optional | Ready | 037 |
| 141 | 021 Proposal: consistent given/responds syntax | Ready | 017 |
| 142 | 035 Proposal: parameterizable failure codes | Ready | 033 |
| 143 | 009 Demo app: a Tasks API written entirely in .phrase | Superseded/Withdrawn | 008 |
| 144 | 010 End-to-end: real HTTP behaves as the specs say | Superseded/Withdrawn | 009 |
| 145 | 011 LLM-customer check: a model adds an endpoint from the docs alone | Superseded/Withdrawn | 009, 010, 057 |
| 146 | 012 Usable v1: acceptance test passes on main and gates CI | Superseded/Withdrawn | 009, 010, 011 |
| 147 | 038 Dogfood: the mob status API, written in Phrase | Ready | 138, 036 |
| 148 | 026 Catalogue dedup linter: flag near-duplicate keywords | Ready | 138, 025 |
| 149 | 061 Spike: a scripted scheduler, fake clock and event capture make an async spec deterministic | Ready | 022, 033, 034 (language foundations); 025, 047 (a base library and the first effect) |
| 150 | 083 Spike: an in-memory store and an event capture assert one scripted observation | Ready | 082, 034, 047, 061 |
| 151 | 028 Second adapter spike: prove the keyword model is language-neutral | Ready | 138, 025 |
| 152 | 029 LLM-as-compiler with hash pinning for keywords without an adapter implementation | Ready | 028 |
| 153 | 030 Synthesize Phrase from existing code (spike: C# to Phrase) | Ready | 138, 024, 025 |
| 154 | 031 Converter between LLM-optimized Phrase and a human-friendly rendering (spike) | Ready | 030 |
<!-- board-table:end -->

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
