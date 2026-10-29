# Decision index

Start here for the complete decision history. Records are stored once in their owning area; related areas link to them.

Status vocabulary: `in force`, `applied`, `superseded by Dnnn`, `reverted`.

## In force

### language-and-grammar

- [D20](language-and-grammar.md#d20) — Consumer-observable outcomes: a missing failure-path spec is an error, an endpoint with an output cannot say succeeds
- [D21](language-and-grammar.md#d21) — Async and events stay out of the core grammar: they are declared effects plus base-library keywords, specified with a virtual clock and event capture, kept as a…
- [D29](language-and-grammar.md#d29) — The spec concept is not a keyword yet: measure under today's grammar first, then an ADR and a spike
- [D30](language-and-grammar.md#d30) — The spec concept is named actor, in the use-case sense: anyone or anything outside the system that interacts with it, human persona or system actor
- [D72](language-and-grammar.md#d72) — The scored Phrase arm's documentation is the frozen cheat sheet with recorded hashes
- [D89](language-and-grammar.md#d89) — Language layering: a domain-neutral core plus one extension per application domain, web first; the adapter axis is orthogonal to the domain axis
- [D105](language-and-grammar.md#d105) — Logic sources are layered: core library and syntax first, language adapters' idiomatic implementations second, LLM generation from specs third, native block last

### compiler-and-adapters

- [D4](compiler-and-adapters.md#d4) — Adapters replace providers: a base library plus per-language adapters, with C# built first; synthesis and the human converter wait behind Usable v1 and Vocabulary v1
- [D74](compiler-and-adapters.md#d74) — Direction: keep building Phrase's own deterministic compilers per target through adapters, tests first, then implementation code
- [D95](compiler-and-adapters.md#d95) — The {Record}Store persistence convention (an effect named after a record with an id) is provisional
- [D111](compiler-and-adapters.md#d111) — Traceability is a separate source map and a trace command outside the runtime
- [D112](compiler-and-adapters.md#d112) — The failure observer (a neutral event raised by the generated code, resolved through the source map) is its own card after the source map

### process-and-staffing

- [D12](process-and-staffing.md#d12) — Experiment E1 (true mobbing) stays open and opt-in behind a flag, not the default practice; its numbers are labelled early data. Follows D8
- [D15](process-and-staffing.md#d15) — Tooling standard: a hardened smoke check before first use, non-author review of tooling, rotation authorship with logged takeovers
- [D18](process-and-staffing.md#d18) — Process retro picks: thin slices with locked Done checks before coding, trialled on the C# adapter chain; metric is runs per card and cap hits
- [D22](process-and-staffing.md#d22) — Atomic land-or-discard runs are an experiment (E3); dependency-free work may run in parallel, dependent or shared-state work stays at WIP of one
- [D27](process-and-staffing.md#d27) — Stories name a persona, not a role, with an observable capability and a so-that value
- [D88](process-and-staffing.md#d88) — Standing driver rule: the loop re-probes unavailable models and the driver status table is kept visible
- [R4](process-and-staffing.md#r4) — Staffing is by role first and model second
- [D101](process-and-staffing.md#d101) — Docs state principles and rationale, not who said them; no personal names, no verbatim quotes attributed to a role, no gendered pronouns about the sponsor
- [D109](process-and-staffing.md#d109) — Decider seats are by role, not by model
- [D116](process-and-staffing.md#d116) — Retro experiment E3 (decision log split by area) starts its trial

### benchmarks-and-measurement

- [D32](benchmarks-and-measurement.md#d32) — The frozen head-to-head bar: N, attempt cap, thresholds, token ratio, patch rate 0, INCONCLUSIVE rules and the Goodhart guard
- [D37](benchmarks-and-measurement.md#d37) — Phrase and plain-C# rows are compared only at the same proof level (specs-passed on both arms); a clarification of D32 with no number changed
- [D66](benchmarks-and-measurement.md#d66) — The Phrase arm scores executed outcomes at specs-passed from the shared outcomes file over eight development tasks
- [D79](benchmarks-and-measurement.md#d79) — Holdout run rows stay private: both tools fail closed before launching a model on a holdout-shaped suite unless private results are requested
- [D81](benchmarks-and-measurement.md#d81) — The cheaper-models-with-Phrase hypothesis is a separate pre-registered question beside the frozen bar, not an amendment; nothing is re-scored
- [D82](benchmarks-and-measurement.md#d82) — codebase-size hypothesis; its real-project tier was replaced by D84 and the synthetic tier stays
- [D83](benchmarks-and-measurement.md#d83) — A model that outlives its timeout is stopped with its whole process tree and counts as one unavailable attempt
- [D84](benchmarks-and-measurement.md#d84) — The real-code tier uses pinned public repositories, and a growth study is added
- [D91](benchmarks-and-measurement.md#d91) — The scheduled holdout run is closed
- [D99](benchmarks-and-measurement.md#d99) — Phrase is measured against unaccountable AI-written code, not C# economy: the head-to-head is context only

### board-and-delivery

- [D7](board-and-delivery.md#d7) — The live mob dashboard builds only non-exposing parts (event stream, stall detection, local dashboard)
- [D9/D10](board-and-delivery.md#d9-d10) — The usable-v1 CI job passes while the milestone is pending and fails only on a regression of a ratcheted bar
- [D23](board-and-delivery.md#d23) — The loop runner in mob-defaults: a thin supervisor over card mode with pinned toolchain, one lock, retry then park, spend ceilings
- [D24](board-and-delivery.md#d24) — The loop never ends on its own: new work is discovered in every retro's product category, and it stops only for a STOP file, a one-way door or all allotments out
- [D25](board-and-delivery.md#d25) — The board is a private GitHub Projects board with an issue per card
- [D28](board-and-delivery.md#d28) — Every card is a Story or a Contract; a Contract has a system actor as consumer, enables an existing Story and is never an orphan
- [D102](board-and-delivery.md#d102) — CI on main is part of the line: a red main stops the line, checked after every push and at each loop iteration by a script; the workflow is kept and fixed
- [D113](board-and-delivery.md#d113) — Card 161 closes on its core outcome; the rest becomes five small cards

## All decisions

| ID | Title | Area | Status |
| --- | --- | --- | --- |
| [D1](board-and-delivery.md#d1) | rank bug card 018 at rank 4 | board-and-delivery | applied |
| [D2](board-and-delivery.md#d2) | rank the three retro-002 cards | board-and-delivery | applied |
| [D3](board-and-delivery.md#d3) | assess the board against VISION.md; add milestone M4 Vocabulary v1 | board-and-delivery | applied |
| [D4](compiler-and-adapters.md#d4) | Adapters replace providers | compiler-and-adapters | in force |
| [D5](board-and-delivery.md#d5) | declare card 016 Done not M2; file baseline bug card 032 | board-and-delivery | applied |
| [D6](board-and-delivery.md#d6) | rank the retro-016 cards 033 to 035 | board-and-delivery | applied |
| [D7](board-and-delivery.md#d7) | The live mob dashboard builds only non-exposing parts (event stream, stall detection, local dashboard) | board-and-delivery | in force |
| [D8](process-and-staffing.md#d8) | pause experiment E1 on card 032 after 8 runs | process-and-staffing | applied |
| [D9/D10](board-and-delivery.md#d9-d10) | The usable-v1 CI job passes while the milestone is pending and fails only on a regression of a ratcheted bar | board-and-delivery | in force |
| [D11](benchmarks-and-measurement.md#d11) | declare card 032 Done as a repair; M2 not reached | benchmarks-and-measurement | applied |
| [D12](process-and-staffing.md#d12) | Experiment E1 (true mobbing) stays open and opt-in behind a flag, not the default practice | process-and-staffing | in force |
| [D13](board-and-delivery.md#d13) | amend the card 036 acceptance test CSP regex and the page takeover | board-and-delivery | applied |
| [D14](board-and-delivery.md#d14) | board order after the product owner withdrew a re-rank | board-and-delivery | superseded by D17 |
| [D15](process-and-staffing.md#d15) | Tooling standard | process-and-staffing | in force |
| [D16](board-and-delivery.md#d16) | good starter app: keep Tasks as the bar-1 app, status API after the gate | board-and-delivery | applied |
| [D17](board-and-delivery.md#d17) | board order rounds 2 and 3, replaces the failed round 1 order | board-and-delivery | applied |
| [D18](process-and-staffing.md#d18) | Process retro picks | process-and-staffing | in force |
| [D20](language-and-grammar.md#d20) | Consumer-observable outcomes | language-and-grammar | in force |
| [D21](language-and-grammar.md#d21) | Async and events stay out of the core grammar | language-and-grammar | in force |
| [D22](process-and-staffing.md#d22) | Atomic land-or-discard runs are an experiment (E3) | process-and-staffing | in force |
| [D23](board-and-delivery.md#d23) | The loop runner in mob-defaults | board-and-delivery | in force |
| [D24](board-and-delivery.md#d24) | The loop never ends on its own | board-and-delivery | in force |
| [D25](board-and-delivery.md#d25) | The board is a private GitHub Projects board with an issue per card | board-and-delivery | in force |
| [D27](process-and-staffing.md#d27) | Stories name a persona, not a role, with an observable capability and a so-that value | process-and-staffing | in force |
| [D28](board-and-delivery.md#d28) | Every card is a Story or a Contract | board-and-delivery | in force |
| [D29](language-and-grammar.md#d29) | The spec concept is not a keyword yet | language-and-grammar | in force |
| [D30](language-and-grammar.md#d30) | The spec concept is named actor, in the use-case sense | language-and-grammar | in force |
| [D32](benchmarks-and-measurement.md#d32) | The frozen head-to-head bar | benchmarks-and-measurement | in force |
| [D31](board-and-delivery.md#d31) | measure first, pause the rest behind the head-to-head (its priority was later changed by D86) | board-and-delivery | applied |
| [D36](benchmarks-and-measurement.md#d36) | sealed holdout hashes recorded before any scored run | benchmarks-and-measurement | applied |
| [D35](benchmarks-and-measurement.md#d35) | benchmark change: docs-in-context run mode | benchmarks-and-measurement | applied |
| [D37](benchmarks-and-measurement.md#d37) | Phrase and plain-C# rows are compared only at the same proof level (specs-passed on both arms) | benchmarks-and-measurement | in force |
| [D40](benchmarks-and-measurement.md#d40) | benchmark change: plain-C# baseline arm and write twins | benchmarks-and-measurement | applied |
| [D42](benchmarks-and-measurement.md#d42) | benchmark change: cold runs, same preamble for every model | benchmarks-and-measurement | applied |
| [D44](benchmarks-and-measurement.md#d44) | benchmark changes: reply-head scrub first and C# read and modify twins | benchmarks-and-measurement | applied |
| [D46](benchmarks-and-measurement.md#d46) | Phrase baseline run, cold and docs-in-context, parse-resolve level | benchmarks-and-measurement | applied |
| [D47](benchmarks-and-measurement.md#d47) | benchmark change: C# fix twins and completeness check | benchmarks-and-measurement | applied |
| [D50](benchmarks-and-measurement.md#d50) | benchmark change: plain-C# arm runs cold like the Phrase arm | benchmarks-and-measurement | applied |
| [D51](compiler-and-adapters.md#d51) | stashed runaway-voter emitter edits exported then dropped | compiler-and-adapters | applied |
| [D52](benchmarks-and-measurement.md#d52) | plain C# baseline result: little-room gate not met | benchmarks-and-measurement | applied |
| [D53](compiler-and-adapters.md#d53) | review of card 039: one Int phrase compiles to C# | compiler-and-adapters | applied |
| [D54](compiler-and-adapters.md#d54) | review of card 040: compilation through the adapter contract | compiler-and-adapters | applied |
| [D55](compiler-and-adapters.md#d55) | review of card 041: declared record becomes a C# record | compiler-and-adapters | applied |
| [D56](compiler-and-adapters.md#d56) | review of card 042: List fields compile | compiler-and-adapters | applied |
| [D57](compiler-and-adapters.md#d57) | review of card 043: a phrase fails with its declared code | compiler-and-adapters | applied |
| [D58](compiler-and-adapters.md#d58) | review of card 044: native C# block placed verbatim | compiler-and-adapters | applied |
| [D59](compiler-and-adapters.md#d59) | review of card 045: composite phrase calls leaves in order | compiler-and-adapters | applied |
| [D60](board-and-delivery.md#d60) | rank fix: card 047 directly after card 045 | board-and-delivery | applied |
| [D61](compiler-and-adapters.md#d61) | review of card 047: declared effect injected as a dependency | compiler-and-adapters | applied |
| [D62](compiler-and-adapters.md#d62) | review of card 048: one POST endpoint serves its response | compiler-and-adapters | applied |
| [D63](compiler-and-adapters.md#d63) | review of card 049: declared failure maps to HTTP status | compiler-and-adapters | applied |
| [D64](compiler-and-adapters.md#d64) | review of card 050: every spec outcome kind asserts correctly | compiler-and-adapters | applied |
| [D65](compiler-and-adapters.md#d65) | review of card 087: orders.phrase compiles and its specs pass | compiler-and-adapters | applied |
| [D66](benchmarks-and-measurement.md#d66) | The Phrase arm scores executed outcomes at specs-passed from the shared outcomes file over eight development t | benchmarks-and-measurement | in force |
| [D67](benchmarks-and-measurement.md#d67) | review of card 098: Phrase arm scores write tasks executed | benchmarks-and-measurement | applied |
| [D68](benchmarks-and-measurement.md#d68) | review of card 099: Phrase arm scores read and modify tasks | benchmarks-and-measurement | applied |
| [D69](benchmarks-and-measurement.md#d69) | review of card 100: Phrase arm scores fix tasks | benchmarks-and-measurement | applied |
| [D70](benchmarks-and-measurement.md#d70) | compact cheat sheet and frozen documentation choice before the baseline | benchmarks-and-measurement | applied |
| [D71](language-and-grammar.md#d71) | review of card 104: compact cheat sheet for prompts | language-and-grammar | applied |
| [D72](language-and-grammar.md#d72) | The scored Phrase arm's documentation is the frozen cheat sheet with recorded hashes | language-and-grammar | in force |
| [D73](benchmarks-and-measurement.md#d73) | scored Phrase development baseline at specs-passed | benchmarks-and-measurement | applied |
| [D74](compiler-and-adapters.md#d74) | Direction | compiler-and-adapters | in force |
| [R1](benchmarks-and-measurement.md#r1) | the three sealed holdouts unsealed for card 102 | benchmarks-and-measurement | applied |
| [R2](benchmarks-and-measurement.md#r2) | holdouts are endpoint tasks; cards 110 and 111 add responds scoring | benchmarks-and-measurement | applied |
| [D75](compiler-and-adapters.md#d75) | review of card 106: mutation check of generated specs | compiler-and-adapters | applied |
| [D76](benchmarks-and-measurement.md#d76) | review of card 110: C# arm scores endpoint responds | benchmarks-and-measurement | applied |
| [D77](benchmarks-and-measurement.md#d77) | review of card 111: Phrase arm scores endpoint responds | benchmarks-and-measurement | applied |
| [R3](benchmarks-and-measurement.md#r3) | card 102 sliced into cards 112 and 113 first | benchmarks-and-measurement | applied |
| [D78](benchmarks-and-measurement.md#d78) | review of card 112: holdout file to both arms' task folders | benchmarks-and-measurement | applied |
| [D79](benchmarks-and-measurement.md#d79) | Holdout run rows stay private | benchmarks-and-measurement | in force |
| [D80](benchmarks-and-measurement.md#d80) | fairness of the three holdout prompt pairs | benchmarks-and-measurement | applied |
| [D81](benchmarks-and-measurement.md#d81) | The cheaper-models-with-Phrase hypothesis is a separate pre-registered question beside the frozen bar, not an  | benchmarks-and-measurement | in force |
| [D82](benchmarks-and-measurement.md#d82) | codebase-size hypothesis; its real-project tier was replaced by D84 and the synthetic tier stays | benchmarks-and-measurement | in force |
| [D83](benchmarks-and-measurement.md#d83) | A model that outlives its timeout is stopped with its whole process tree and counts as one unavailable attempt | benchmarks-and-measurement | in force |
| [D84](benchmarks-and-measurement.md#d84) | The real-code tier uses pinned public repositories, and a growth study is added | benchmarks-and-measurement | in force |
| [D85](benchmarks-and-measurement.md#d85) | review of card 119: matching codebases of 1, 10 and 50 endpoints | benchmarks-and-measurement | applied |
| [D86](board-and-delivery.md#d86) | shortest path to Usable v1 leads the board; ZOMBIES warnings card | board-and-delivery | applied |
| [D87](benchmarks-and-measurement.md#d87) | review of card 130: plain C# codebases in ordinary style | benchmarks-and-measurement | applied |
| [D88](process-and-staffing.md#d88) | Standing driver rule | process-and-staffing | in force |
| [D89](language-and-grammar.md#d89) | Language layering | language-and-grammar | in force |
| [D90](compiler-and-adapters.md#d90) | review of card 051: phrase build compiles one file | compiler-and-adapters | applied |
| [D91](benchmarks-and-measurement.md#d91) | The scheduled holdout run is closed | benchmarks-and-measurement | in force |
| [R4](process-and-staffing.md#r4) | Staffing is by role first and model second | process-and-staffing | in force |
| [D92](language-and-grammar.md#d92) | review of card 141: extension map ADR and audit | language-and-grammar | applied |
| [D93](compiler-and-adapters.md#d93) | review of card 142: web extension boundary | compiler-and-adapters | applied |
| [D94](benchmarks-and-measurement.md#d94) | review of card 103: verdict page, headline INCONCLUSIVE | benchmarks-and-measurement | applied |
| [D95](compiler-and-adapters.md#d95) | The {Record}Store persistence convention (an effect named after a record with an id) is provisional | compiler-and-adapters | in force |
| [D96](compiler-and-adapters.md#d96) | review of card 131: GET endpoints | compiler-and-adapters | applied |
| [D97](compiler-and-adapters.md#d97) | review of card 133: PUT and DELETE endpoints | compiler-and-adapters | applied |
| [D98](board-and-delivery.md#d98) | review of card 135: Tasks demo app, logic still native | board-and-delivery | applied |
| [D99](benchmarks-and-measurement.md#d99) | Phrase is measured against unaccountable AI-written code, not C# economy | benchmarks-and-measurement | in force |
| [D100](board-and-delivery.md#d100) | review of card 136: real HTTP behaves as the specs say | board-and-delivery | applied |
| [D101](process-and-staffing.md#d101) | Docs state principles and rationale, not who said them | process-and-staffing | in force |
| [D102](board-and-delivery.md#d102) | CI on main is part of the line | board-and-delivery | in force |
| [D103](benchmarks-and-measurement.md#d103) | review of card 137: LLM-customer check passed | benchmarks-and-measurement | applied |
| [D104](board-and-delivery.md#d104) | review of card 138: Usable v1 reached | board-and-delivery | applied |
| [D105](language-and-grammar.md#d105) | Logic sources are layered | language-and-grammar | in force |
| [D106](board-and-delivery.md#d106) | review of card 158: Tasks demo README | board-and-delivery | applied |
| [D107](language-and-grammar.md#d107) | review of card 159: ensure validation step | language-and-grammar | applied |
| [D108](language-and-grammar.md#d108) | review of card 160: record update form | language-and-grammar | applied |
| [D109](process-and-staffing.md#d109) | Decider seats are by role, not by model | process-and-staffing | in force |
| [D110](compiler-and-adapters.md#d110) | trace header as a switch, off by default | compiler-and-adapters | superseded by D111 |
| [D111](compiler-and-adapters.md#d111) | Traceability is a separate source map and a trace command outside the runtime | compiler-and-adapters | in force |
| [D112](compiler-and-adapters.md#d112) | The failure observer (a neutral event raised by the generated code, resolved through the source map) is its ow | compiler-and-adapters | in force |
| [D113](board-and-delivery.md#d113) | Card 161 closes on its core outcome; the rest becomes five small cards | board-and-delivery | in force |
| [D114](compiler-and-adapters.md#d114) | Review of card 161 (source map), closed on its amended criteria | compiler-and-adapters | applied |
| [D115](compiler-and-adapters.md#d115) | Review of card 167: the source map is never published or served | compiler-and-adapters | applied |
| [D116](process-and-staffing.md#d116) | Retro experiment E3 (decision log split by area) starts its trial | process-and-staffing | in force |
| [D117](compiler-and-adapters.md#d117) | Review of card 168: phrase trace mismatch handling and error paths | compiler-and-adapters | applied |
