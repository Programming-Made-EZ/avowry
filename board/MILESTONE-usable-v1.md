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

<!-- board-table:start -->
<!-- board-table-sha256:d3659d56e9223642dac218c8da29a940fa35a6ddaec4989295a7ceb339cbc01c -->
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
| 14 | 093 Record the sealed holdout hashes before the baseline starts | Ready | none |
| 15 | 086 Plain C# baseline on the same tasks | Ready | 093 (the sealed holdout hashes are recorded first) |
| 16 | 084 Reference target builds and every orders spec passes | Done | none |
| 17 | 085 A missing Phrase reply is a model failure, not a harness error | Done | none |
| 18 | 039 One phrase with one number input compiles to C# and its spec passes | Ready | none (the stash Decider runs first) |
| 19 | 040 C# compilation goes through the ITargetAdapter contract | Ready | 039 |
| 20 | 041 A declared record becomes a usable C# record | Ready | 040 |
| 21 | 042 List fields compile | Ready | 041 |
| 22 | 043 A phrase can fail with its declared code | Ready | 040 |
| 23 | 044 A native C# block is placed verbatim and builds | Ready | 043 |
| 24 | 045 A composite phrase calls its leaf phrases in order | Ready | 043 |
| 25 | 048 One POST endpoint serves its successful response | Ready | 045, 047 |
| 26 | 049 A declared failure maps to its HTTP status, not a 500 | Ready | 048 |
| 27 | 050 Every spec outcome kind asserts correctly, and a wrong one names itself | Ready | 049 |
| 28 | 087 orders.phrase compiles and its generated specs pass | Ready | 039, 040, 041, 043, 044, 045, 048, 049, 050, 084 |
| 29 | 090 Benchmark: a docs-in-context mode that gives a model the real docs | Ready | 085 |
| 30 | 091 Benchmark: cold runs, a fresh session in an empty folder for every attempt | Ready | 090 |
| 31 | 092 Benchmark: scrub a stored reply before cutting it to 200 characters | Ready | 085 |
| 32 | 037 Benchmark: docs in the prompt, and cold runs | Ready | 090, 091, 092 (sliced: docs-in-context mode, cold runs and the reply-head scrub moved to those cards; this card is now the baseline RUN) |
| 33 | 088 Head-to-head: Phrase vs plain C# on the frozen bar | Ready | 085, 086, 087, 037 |
| 34 | 089 Option and nested-record fields compile | Ready | 042 |
| 35 | 065 Loop: run one card with a pinned toolchain | Ready | 064 |
| 36 | 066 Loop: bounded retry, park and split on failure | Ready | 065 |
| 37 | 078 Ready check: reject a role-only story before the product manager is asked | Ready | 065 |
| 38 | 067 Loop: on Done run the retro step and re-rank, then the next card | Ready | 066 |
| 39 | 053 Docs: one diagnostics table, not two | Ready | none |
| 40 | 062 Atomic runs: a card runs in a fresh worktree and ends landed or discarded | Ready | none |
| 41 | 068 Loop: liveness, publish, STOP file and the Needs-Steve file | Ready | 067 |
| 42 | 072 Loop: pace the work against the finite driver allotments | Ready | 066 |
| 43 | 069 Loop: resume after a crash or reboot (logon task) | Ready | 068 |
| 44 | 070 Loop: run every card atomically (land or discard) | Ready | 062, 067 |
| 45 | 054 Benchmark task 005 gets a failure-path spec (suite change, versioned) | Ready | none |
| 46 | 055 A declared failure with no failure-path spec is an error (PH336) | Ready | 054, 018 |
| 47 | 056 An endpoint with an output cannot say `succeeds` (PH333) | Ready | 055 |
| 48 | 057 Docs: the consumer-observable outcome rule is written down | Ready | 055 |
| 49 | 022 Keyword model ADR: what a keyword is | Ready | none |
| 50 | 058 A validation phrase returns its validated value (first slice, language decision first) | Ready | 022, 055 |
| 51 | 013 Make GRAMMAR.md say what the resolver does | Ready | none |
| 52 | 019 Publish exact diagnostic message templates in GRAMMAR.md | Ready | 018 |
| 53 | 014 Document the native C# environment and show passing-guard examples | Ready | none |
| 54 | 047 A declared effect is supplied as an injected dependency | Ready | 043 |
| 55 | 046 Docs: GRAMMAR and README name the adapter contract and the HTTP status mapping | Ready | 040, 049 |
| 56 | 017 Benchmark v1: score by running the generated specs | Ready | 016, 050 |
| 57 | 024 Pattern discovery: find the recurring patterns worth a keyword | Ready | 017 |
| 58 | 027 Benchmark: catalogue tasks that measure LLM work removed and learnability | Ready | 017, 024 |
| 59 | 060 Catalogue tasks for concurrent work and event publication (measure the demand) | Ready | 022, 033, 034 (language foundations) |
| 60 | 081 Measure stored and published outcomes under today's grammar | Ready | 037, 054 |
| 61 | 020 Did-you-mean suggestions for failure codes and other unknown names | Ready | 017 |
| 62 | 051 phrase build compiles one file into output projects | Ready | 040 |
| 63 | 052 phrase build reports errors as file:line:col and names its target | Ready | 051 |
| 64 | 023 Ordinary control flow: if/else and loops | Ready | 022, 050 |
| 65 | 033 Generic type parameters on phrases | Ready | 022, 032, 040 |
| 66 | 034 Behavioral holes and spec test doubles | Ready | 033 |
| 67 | 059 ADR: deterministic asynchronous and event semantics | Ready | 022, 033, 034 (language foundations) |
| 68 | 082 ADR: a spec actor is who observes an outcome | Ready | 022, 058, 059, 081 |
| 69 | 025 Base library v0: structure and a seed of about 12 keywords, with C# adapter implementations | Ready | 024, 034, 050 |
| 70 | 008 Close the language gaps the demo app exposes | Ready | 025, 050, 052 |
| 71 | 015 Proposal: make the spec `when` line optional | Ready | 037 |
| 72 | 021 Proposal: consistent given/responds syntax | Ready | 017 |
| 73 | 035 Proposal: parameterizable failure codes | Ready | 033 |
| 74 | 009 Demo app: a Tasks API written entirely in .phrase | Ready | 008 |
| 75 | 010 End-to-end: real HTTP behaves as the specs say | Ready | 009 |
| 76 | 011 LLM-customer check: a model adds an endpoint from the docs alone | Ready | 009, 010, 057 |
| 77 | 012 Usable v1: acceptance test passes on main and gates CI | Ready | 009, 010, 011 |
| 78 | 038 Dogfood: the mob status API, written in Phrase | Ready | 012, 036 |
| 79 | 026 Catalogue dedup linter: flag near-duplicate keywords | Ready | 012, 025 |
| 80 | 061 Spike: a scripted scheduler, fake clock and event capture make an async spec deterministic | Ready | 022, 033, 034 (language foundations); 025, 047 (a base library and the first effect) |
| 81 | 083 Spike: an in-memory store and an event capture assert one scripted observation | Ready | 082, 034, 047, 061 |
| 82 | 028 Second adapter spike: prove the keyword model is language-neutral | Ready | 012, 025 |
| 83 | 029 LLM-as-compiler with hash pinning for keywords without an adapter implementation | Ready | 028 |
| 84 | 030 Synthesize Phrase from existing code (spike: C# to Phrase) | Ready | 012, 024, 025 |
| 85 | 031 Converter between LLM-optimized Phrase and a human-friendly rendering (spike) | Ready | 030 |
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
