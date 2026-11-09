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
<!-- board-table-sha256:229c4fb5a5a47654ab4d89f2b723fe888f8590486513de4a4610ced91c4785a7 -->
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
| 61 | 161 Generated failures and endpoint classes point back to the Phrase source | Done | 138 |
| 62 | 167 The source map is never published or served: tests for publish output and a 404 | Done | 161 |
| 63 | 168 The trace command: mismatch handling and error paths, never an unhandled exception | Done | 161 |
| 64 | 170 Step ids: an independent recompute and one id per failure per endpoint | Done | 161 |
| 65 | 173 The phrase CLI test helper fails loudly when the built CLI is older than its sources | Done | none |
| 66 | 174 An edge-case fixture corpus that every compiler and adapter change runs against | Done | none |
| 67 | 175 More edge-case fixtures: Zero, Boundary and Exception shapes | Done | 174 |
| 68 | 198 Rename the repository to avowry now (right after card 175) | Done | 197, 177 |
| 69 | 190 Rename the product to Avowry in user-facing documents and legal files | Done | none |
| 70 | 191 The command is avowry; tool output and the map file say Avowry | Done | 190 |
| 71 | 199 The language keyword is avow, with optional filler words, and every source switches to .avowry (one mechanical commit) | Done | 191 |
| 72 | 193 Generated code uses Avowry names (runtime, failure type, build id key) | Done | 166, 162 |
| 73 | 194 Rename the compiler project and namespace to Avowry.Compiler | Done | 193 |
| 74 | 195 Rename the C# adapter and CLI projects to Avowry.* | Done | 194 |
| 75 | 205 The facilitator starts only corrective cards while a finding blocks | Done | none (A2, D, C and E depend on this card, in that order) |
| 76 | 214 The generated API binds its own port in acceptance tests | Done | none |
| 77 | 209 A task force member leaves only at the retro | Done | none |
| 78 | 208 A seat starts only on a fresh, pinned probe, and seats rotate least-recently-used across the available models | Done | none |
| 79 | 219 The record checker accepts cards that close out of number order | Done | none |
| 80 | 226 Decision seats start on a fresh probe and rotate like card seats | Done | none (card 208, which built the rules for card-mode seats, is done) |
| 81 | 227 Recursive deletes on variable paths are blocked and every commit is pushed | Done | none |
| 82 | 234 An indexed process wiki with just-in-time reading | Done | none |
| 83 | 237 Evaluate whether an indexed, just-in-time process wiki solves the context problem | Done | card 234 (its prototype and measurements are evidence) |
| 84 | 228 Recursive-delete scan covers hidden, encoded and non-shell forms, and the contents of files a seat writes | Done | card 227 (its command-position scan, literal-path allow-list and fail-closed coverage) |
| 85 | 210 The card record check rejects an empty task force and empty probes | Done | none |
| 86 | 211 A whole-team review of the mob-defaults rulings commits | Done | none |
| 87 | 276 Muse Code joins the mob as a trial driver | Done | none |
| 88 | 229 A Copilot decision seat receives its whole prompt and casts a vote | Done | none |
| 89 | 290 The decider's result counts only votes with a valid cite | Done | card 229 (counted and cite_ok exist from green a2bb1d4) |
| 90 | 230 The facilitator and seats never write the same checkout at once | Done | none |
| 91 | 297 A Codex seat on Windows runs shell commands in its sandbox, and the probe proves it | Done | none |
| 92 | 264 The read-and-modify acceptance test is not flaky | Done | none |
| 93 | 217 Acceptance tests do not starve the thread pool | Done | none |
| 94 | 303 The C# baseline endpoint tests pass on the Windows development machine | Done | none |
| 95 | 298 A second decision run on the same checkout never votes unprotected | Done | card 230 (the vote lock) |
| 96 | 313 A decision run that starts as another run ends can be left without a vote lock | Done | card 298 (the holder format) |
| 97 | 299 A second writer on one checkout is refused across processes | Done | card 230 (Writers and the vote lock) |
| 98 | 196 Rename the two test projects to Avowry.*.Tests | Done | 195 |
| 99 | 197 Rename the solution, CI jobs, scripts and paths to Avowry | Done | 196 |
| 100 | 266 Live docs and the README use the name Avowry | Done |  |
| 101 | 324 A compiler diagnostic calls the unit an avow, not a Phrase | Done | none |
| 102 | 176 Package the CLI as a dotnet tool and version it | Done | none |
| 103 | 178 A prerequisites page that matches what CI tests | Done | 176 |
| 104 | 186 Pre-publication checks: name, licences, history, vendor terms | Done | none |
| 105 | 179 Rewrite the README and write the getting-started guide | Done | 178 |
| 106 | 189 An unsupported construct is a diagnostic, not an unhandled exception | Done | none |
| 107 | 202 A native block that fails with an undeclared code is a diagnostic | Done | none |
| 108 | 203 Names reserved by generated code are rejected or escaped | Done | none |
| 109 | 361 Diagnostic codes use the AV prefix before the v0.1 preview | Done | none. Ranked ahead of the v0.1 publish card (188) and immediately before card 162, so that the 162 re-freeze ratifies AV codes in docs/GRAMMAR.md (D127: 162 is the single final re-freeze; this card must not follow it). |
| 110 | 366 The NOTICE and TRADEMARKS files name the holder and the year, not placeholders | Ready | none |
| 111 | 371 The pre-publication page shows the owner's answers and the trademark search brief | Ready | none |
| 112 | 370 Tracked files carry no home path of a local account | Ready | none |
| 113 | 368 New bench rows no longer publish a reply prefix | Ready | none |
| 114 | 369 The licences of the packages the first preview restores are reviewed and listed | Ready | none |
| 115 | 367 The four model vendors' published terms are read for what they say about publishing model output | Ready | none |
| 116 | 162 Fold the pattern steps into the grammar and cheat sheet: re-freeze and rerun the LLM check | Ready | 160 |
| 117 | 201 Migrate the sealed holdouts to avow and record a new baseline | Ready | 199 |
| 118 | 169 The map bytes do not depend on the culture | Ready | 161 |
| 119 | 171 The CLI reads method and type names from the adapter; Program.cs is tidy | Ready | 161 |
| 120 | 172 Trace hardening follow-ups and one shared test process helper | Ready | 168 |
| 121 | 165 Failure observer: the generated API raises a neutral failure event; tracing plugs in outside the runtime | Ready | 161 |
| 122 | 166 Move the generated [Phrase] link into the source map; the grammar and the bench scorer follow | Ready | 161, 162 |
| 123 | 177 CI installs the packed tool on Windows and Linux | Ready | 176 |
| 124 | 200 An avowry fmt command that rewrites sources to the canonical form | Ready | 199, 192 |
| 125 | 180 A test that runs the getting-started guide's commands | Ready | 179, 176 |
| 126 | 181 A cold model completes the getting-started guide | Ready | 179, 180 |
| 127 | 187 A known-limitations page | Ready | none |
| 128 | 163 Generated endpoint files are named *.cs.cs: decide and document the convention | Ready | none |
| 129 | 182 Actionable messages on the getting-started path | Ready | 179 |
| 130 | 164 #line directives: the debugger and C# compiler errors point at .phrase lines | Ready | 161 |
| 131 | 184 Version 0.1.0 and a changelog | Ready | 176 |
| 132 | 153 Verdict page states the success criteria | Ready | 138 |
| 133 | 185 An issues route, a contributing note and a security note | Ready | none |
| 134 | 188 Publish the first version | Ready | 176, 177, 179, 183, 184, 185, 186, 187 |
| 135 | 344 Install Avowry as one native binary so a machine without .NET can compile Avowry source | Ready | 176 (done) |
| 136 | 360 Avowry evaluates a program's specs itself, with no target toolchain | Ready | none for the C# path. Prerequisite of card 345's "specs passing under a WASI host". Independent of card 344 (344 compiles without .NET and leaves specs as target-toolchain tests). The Wasm load uses card 345's component when that card exists. |
| 137 | 345 Verify a tiny program as a Wasm component under a WASI host | Ready | card 344 |
| 138 | 346 Produce passing specs for a tiny program in Python or TypeScript | Ready | card 344 (packaging only) |
| 139 | 347 Add the C# adapter so the .NET SDK is a requirement of that adapter alone | Ready | card 344 and card 345 or card 346 |
| 140 | 348 Ship ensure and record update as readable Avowry source with the toolchain | Ready | card 344 |
| 141 | 349 Expose check, build, spec and trace as MCP tools in the same binary | Ready | card 344 |
| 142 | 325 Test and code identifiers name the product Avowry, not Phrase | Ready | card 266 (the guard and the history decision) |
| 143 | 155 Verdict page as a judgment backed by data | Ready | 153 |
| 144 | 204 Reconcile the personas with the v0.1 bar in Avowry terms, then rerun the LLM check for Ari | Ready | 162 |
| 145 | 145 Store operations as Phrase steps, not native code | Ready | 135 |
| 146 | 146 Validation rules as Phrase: not blank and a maximum length | Ready | 145 |
| 147 | 147 A list endpoint without a fake flag | Ready | 145 |
| 148 | 148 Composite steps: a phrase calls a phrase and changes one field of a record | Ready | 145 |
| 149 | 134 ZOMBIES spec-coverage warnings | Ready | 138 |
| 150 | 149 Undetected-defect rate: planted defects against Phrase specs and model-written C# tests | Ready | 134 |
| 151 | 150 Intent recovery: can a fresh reader state what each unit must do | Ready | 134 |
| 152 | 151 Review speed and accuracy: approving and rejecting AI-written changes | Ready | 134 |
| 153 | 289 Drift resistance: planted unintended changes across edits and model regenerations | Ready | 134 |
| 154 | 140 Cheat sheet plus library index: the hybrid prompt, measured | Ready | 134 |
| 155 | 143 Split the documentation: a smaller core cheat sheet and a web sheet | Ready | 142, 140 |
| 156 | 144 Rerun Copilot's unavailable holdout rows after its quota returns | Ready | 102 |
| 157 | 107 Spec to code with a model: does a Phrase spec get verified-correct code more reliably | Ready | 106 |
| 158 | 108 A second target's Specs project from the same specs | Ready | 106 |
| 159 | 109 C# implementation emission one phrase kind at a time | Ready | 108 |
| 160 | 115 Which cheaper models can each CLI run headless and cold | Ready | 102 |
| 161 | 116 Pre-register the cheaper-model experiment before any row | Ready | 115 |
| 162 | 117 Run the cheaper models in both arms | Ready | 116 |
| 163 | 118 Cost per passing task: the cheaper-model report | Ready | 117 |
| 164 | 120 Pre-register the codebase-size experiment before any row | Ready | 119, 116 |
| 165 | 123 Run the codebase-size experiment in both arms | Ready | 120 |
| 166 | 124 Codebase-size report: how cost and pass rate change with size | Ready | 123 |
| 167 | 125 Choose and pin public C# repositories for the real-code tier | Ready | 120 |
| 168 | 126 Port a representative slice of the pinned public repo to Phrase | Ready | 125 |
| 169 | 127 Write the growth-study backlog and pre-register the study | Ready | 120 |
| 170 | 128 Run the growth study in both arms from shared checkpoints | Ready | 127, 119 |
| 171 | 129 Growth-study curve: cost and pass rate against codebase size | Ready | 128 |
| 172 | 089 Option and nested-record fields compile | Ready | 042 |
| 173 | 053 Docs: one diagnostics table, not two | Ready | none |
| 174 | 054 Benchmark task 005 gets a failure-path spec (suite change, versioned) | Ready | none |
| 175 | 056 An endpoint with an output cannot say `succeeds` (PH333) | Ready | 134 |
| 176 | 022 Keyword model ADR: what a keyword is | Ready | none |
| 177 | 058 A validation phrase returns its validated value (first slice, language decision first) | Ready | 022, 134 |
| 178 | 013 Make GRAMMAR.md say what the resolver does | Ready | none |
| 179 | 019 Publish exact diagnostic message templates in GRAMMAR.md | Ready | 018 |
| 180 | 014 Document the native C# environment and show passing-guard examples | Ready | none |
| 181 | 046 Docs: GRAMMAR and README name the adapter contract and the HTTP status mapping | Ready | 040, 049 |
| 182 | 017 Benchmark v1: score by running the generated specs | Ready | 016, 050 |
| 183 | 024 Pattern discovery: find the recurring patterns worth a keyword | Ready | 017 |
| 184 | 027 Benchmark: catalogue tasks that measure LLM work removed and learnability | Ready | 017, 024 |
| 185 | 060 Catalogue tasks for concurrent work and event publication (measure the demand) | Ready | 022, 033, 034 (language foundations) |
| 186 | 081 Measure stored and published outcomes under today's grammar | Ready | 037, 054 |
| 187 | 020 Did-you-mean suggestions for failure codes and other unknown names | Ready | 017 |
| 188 | 052 phrase build reports errors as file:line:col and names its target | Ready | 051 |
| 189 | 023 Ordinary control flow: if/else and loops | Ready | 022, 050 |
| 190 | 033 Generic type parameters on phrases | Ready | 022, 032, 040 |
| 191 | 034 Behavioral holes and spec test doubles | Ready | 033 |
| 192 | 059 ADR: deterministic asynchronous and event semantics | Ready | 022, 033, 034 (language foundations) |
| 193 | 082 ADR: a spec actor is who observes an outcome | Ready | 022, 058, 059, 081 |
| 194 | 025 Base library v0: structure and a seed of about 12 keywords, with C# adapter implementations | Ready | 024, 034, 050 |
| 195 | 015 Proposal: make the spec `when` line optional | Ready | 037 |
| 196 | 021 Proposal: consistent given/responds syntax | Ready | 017 |
| 197 | 035 Proposal: parameterizable failure codes | Ready | 033 |
| 198 | 038 Dogfood: the mob status API, written in Phrase | Ready | 138, 036 |
| 199 | 026 Catalogue dedup linter: flag near-duplicate keywords | Ready | 138, 025 |
| 200 | 061 Spike: a scripted scheduler, fake clock and event capture make an async spec deterministic | Ready | 022, 033, 034 (language foundations); 025, 047 (a base library and the first effect) |
| 201 | 083 Spike: an in-memory store and an event capture assert one scripted observation | Ready | 082, 034, 047, 061 |
| 202 | 028 Second adapter spike: prove the keyword model is language-neutral | Ready | 138, 025 |
| 203 | 029 LLM-as-compiler with hash pinning for keywords without an adapter implementation | Ready | 028 |
| 204 | 030 Synthesize Phrase from existing code (spike: C# to Phrase) | Ready | 138, 024, 025 |
| 205 | 295 A legacy system's exercised behaviour is pinned as reviewable avowries before anyone changes it | Ready | 030 (synthesize Avowry from existing code; this card supplies the behaviour oracle that synthesis is checked against) |
| 206 | 296 Behaviour characterized from a legacy system runs the same on a second target | Ready | 295; 108 (a second target's specs project); a real, non-spike second adapter (no card yet: card 028 is its spike and informs risk only, it is not evidence of a supported adapter) |
| 207 | 031 Converter between LLM-optimized Phrase and a human-friendly rendering (spike) | Ready | 030 |
| 208 | 315 A decision run that cannot register its vote lock starts without any warning | Ready | card 298 (the holder format) |
| 209 | 307 Two overlapping decision runs on one checkout do not fail each other's seat-use history check | Ready | card 298 (the overlap holder format) |
| 210 | 156 Personas capture the same ten fields | Ready | 154 |
| 211 | 304 The two card-214 readiness tests pass without the thread-pool floor on the development machine | Ready | card 217 (the shared async runner, the guard and the run-pair record) |
| 212 | 302 The C# baseline benchmark children never build concurrently | Ready | card 264 (the launcher) |
| 213 | 300 A legacy turn on a Codex machine whose shell cannot start is refused before launch | Ready | card 297 (the command probe and the placement gate) |
| 214 | 301 The command probe cannot be passed by copying its nonce from the prompt | Ready | card 297 (the command probe) |
| 215 | 286 A NO OBJECTION reply is stored as no objection, not as a No vote | Ready | none |
| 216 | 232 A seat driver never opens a browser or other UI on the shared machine | Ready | none |
| 217 | 323 A plain git commit or push is refused while another writer or a vote holds the checkout | Ready | card 299 (the writer lock file mob-writer.lock and the existing vote lock) |
| 218 | 316 The gate runs the changed area's narrow suites and checks each step's failure path | Done | none |
| 219 | 317 A card that names no consumer value cannot reach Ready | Done | none |
| 220 | 305 A Codex build seat's commands go through the delete scan | Done | card 297 (the command probe and placement), card 228 (the scan for hidden, encoded and file-content forms) |
| 221 | 312 A Decider record is rejected when its result does not follow from its votes | Done | none |
| 222 | 311 A Decider vote is scored as written and the agile coach flags any deviation at the vote | Done | none |
| 223 | 310 The board keeps moving when one driver's quota runs out, with Muse trialled as a fourth driver | Done | none |
| 224 | 308 The product owner's rulings and the team's replies are recorded where a check can verify them | Done | none |
| 225 | 309 A reviewer reads a card cold in the format that gives the fewest misreads | Done | none |
| 226 | 306 The zero-harness-tests diagnostic is capped like the other failure summaries | Done | card 303 |
| 227 | 258 The loop-state lock test is not flaky | Done | none |
| 228 | 268 The mob summary publish tests pass in every shell | Done | none |
| 229 | 291 A Grok decision seat ends its turn with a vote, and a vote after narration is read | Done | card 229 (the whole-prompt launch) |
| 230 | 274 The final-review record can say unclear and keep the reviewer's verdict | Done | none |
| 231 | 252 Seat transcripts are scanned automatically for subagent and headless seats | Done | card 228 (the scan) |
| 232 | 253 Codex rollouts are read by the delete scan | Done | card 228 (the readers) |
| 233 | 283 The delete guard does not flag a read-only python command | Done | none |
| 234 | 254 The delete scan cuts its known false positives | Done | card 228 |
| 235 | 231 Uncommitted work in a watched repository older than about 30 minutes fails the checks | Done | none |
| 236 | 281 Muse seats run with a known workspace trust | Done | none |
| 237 | 270 A Grok decision seat receives its whole prompt and casts a vote | Superseded/Withdrawn | none (card 229 is the Copilot twin; the team may fold the two at ready) |
| 238 | 282 A seat's prompt reaches it without placeholder rewriting | Superseded/Withdrawn | none |
| 239 | 277 An empty decision vote opens a diagnosis; only quota or unavailability excludes a model | Superseded/Withdrawn | none |
| 240 | 265 Decision windows launch voters in parallel safely | Superseded/Withdrawn | card 226 (the decision-seat gate) |
| 241 | 269 mob_process.py keeps LF line endings | Superseded/Withdrawn | none |
| 242 | 238 The reshaped process wiki carries the slice limits and a measured upkeep | Superseded/Withdrawn | card 237 |
| 243 | 236 The process wiki closes the writer's seven content gaps | Superseded/Withdrawn | card 234 |
| 244 | 255 The delete scan covers the residual forms and the path forms | Superseded/Withdrawn | card 228 |
| 245 | 233 Evaluate side-by-side pairing tooling before building pairing | Superseded/Withdrawn | none |
| 246 | 235 Green commit bodies carry a checked Proof block | Superseded/Withdrawn | none |
| 247 | 256 Command-scan reports are readable and trusted | Superseded/Withdrawn | card 228 |
| 248 | 257 Git-state detectors catch unpushed and hidden work | Superseded/Withdrawn | card 227 (push detector) |
| 249 | 267 mob-defaults docs contain no dates | Superseded/Withdrawn |  |
| 250 | 216 The default templates carry the pending process rulings | Superseded/Withdrawn | none |
| 251 | 220 The loop runner starts a corrective card while a finding blocks | Superseded/Withdrawn | none (card 205 is done) |
| 252 | 221 The loop runs a card in the repository its Repository line names | Superseded/Withdrawn | none |
| 253 | 222 The loop wires its retro and re-rank steps from the repository and stops when one fails | Superseded/Withdrawn | none |
| 254 | 223 The loop takes the per-card retro from every role, each in its own context | Superseded/Withdrawn | 208 (every retro seat starts on a fresh pinned probe), 222 (the loop calls the command and stops when it fails) |
| 255 | 224 The loop re-ranks Ready through the product manager and a Decider after every card | Superseded/Withdrawn | 208 (every re-rank seat starts on a fresh pinned probe), 222 (the loop calls the command and stops when it fails) |
| 256 | 225 Card mode reads only role lines as the task force | Superseded/Withdrawn | none |
| 257 | 213 The checker rejects an incomplete objection window | Superseded/Withdrawn | none |
| 258 | 218 A mid-card retro removal does not drop the seat before the retro | Superseded/Withdrawn | 209 (the refusal of every non-retro removal lands first; this card closes the one accepted form that still removes a seat early) |
| 259 | 212 Every mob re-ranks at every loop boundary | Superseded/Withdrawn | 208 (the runner gaps, including its retro and re-rank commands: this card hardens the re-rank step that 208 wires in). |
| 260 | 215 Every mob reviews each previous adjustment at stand-downs and retros | Superseded/Withdrawn | 208 (the runner gaps, including its retro command: this card adds record fields to the step 208 wires in). |
| 261 | 206 The findings file mirrors the audit register | Superseded/Withdrawn | 205 (the facilitator start gate must exist so that this card, a corrective card, is the kind that starts while F-001 and F-008 block). 207 depends on this card. |
| 262 | 207 The findings file history rules are checked | Superseded/Withdrawn | 206 (the findings file and its schema check must exist before its history can be checked). |
| 263 | 192 Source files end in .avowry; the repository's own sources switch in the same card | Superseded/Withdrawn | 191 |
| 264 | 183 Add the licence file | Done | none |
| 265 | 154 Docs state principles and rationale, not who said them | Done | 153 |
| 266 | 139 Loop step: re-probe unavailable drivers and restore them | Superseded/Withdrawn | 064 |
| 267 | 106 Mutation check: the generated C# specs catch broken implementations | Done | 087 |
| 268 | 119 Generate matching Phrase and C# codebases of 1, 10 and 50 endpoints | Done | 111 |
| 269 | 130 Plain C# codebases in ordinary hand-written style | Done | 119 |
| 270 | 121 Snapshot a real project slice privately and scrub it | Superseded/Withdrawn | 120 |
| 271 | 122 Port the real slice to Phrase in the private folder | Superseded/Withdrawn | 121 |
| 272 | 090 Benchmark: a docs-in-context mode that gives a model the real docs | Done | 085 |
| 273 | 091 Benchmark: cold runs, a fresh session in an empty folder for every attempt | Done | 090 |
| 274 | 092 Benchmark: scrub a stored reply before cutting it to 200 characters | Done | 085 |
| 275 | 037 Benchmark: docs in the prompt, and cold runs | Done | 090, 091, 092 (sliced: docs-in-context mode, cold runs and the reply-head scrub moved to those cards; this card is now the baseline RUN) |
| 276 | 088 Head-to-head: Phrase vs plain C# on the frozen bar | Superseded/Withdrawn | 085, 086, 094, 087, 037 |
| 277 | 065 Loop: run one card with a pinned toolchain | Superseded/Withdrawn | 064 |
| 278 | 066 Loop: bounded retry, park and split on failure | Superseded/Withdrawn | 065 |
| 279 | 078 Ready check: reject a role-only story before the product manager is asked | Superseded/Withdrawn | 065 |
| 280 | 067 Loop: on Done run the retro step and re-rank, then the next card | Superseded/Withdrawn | 066 |
| 281 | 062 Atomic runs: a card runs in a fresh worktree and ends landed or discarded | Superseded/Withdrawn | none |
| 282 | 068 Loop: liveness, publish, STOP file and the Needs-product-owner file | Superseded/Withdrawn | 067 |
| 283 | 072 Loop: pace the work against the finite driver allotments | Superseded/Withdrawn | 066 |
| 284 | 069 Loop: resume after a crash or reboot (logon task) | Superseded/Withdrawn | 068 |
| 285 | 070 Loop: run every card atomically (land or discard) | Superseded/Withdrawn | 062, 067 |
| 286 | 055 A declared failure with no failure-path spec is an error (PH336) | Superseded/Withdrawn | 054, 018 |
| 287 | 057 Docs: the consumer-observable outcome rule is written down | Superseded/Withdrawn | 055 |
| 288 | 008 Close the language gaps the demo app exposes | Superseded/Withdrawn | 025, 050, 052 |
| 289 | 009 Demo app: a Tasks API written entirely in .phrase | Superseded/Withdrawn | 008 |
| 290 | 010 End-to-end: real HTTP behaves as the specs say | Superseded/Withdrawn | 009 |
| 291 | 011 LLM-customer check: a model adds an endpoint from the docs alone | Superseded/Withdrawn | 009, 010, 057 |
| 292 | 012 Usable v1: acceptance test passes on main and gates CI | Superseded/Withdrawn | 009, 010, 011 |
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
