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
- [D126](compiler-and-adapters.md#d126) — The in-language unit is an avow; declaration form and filler words

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
- [D118](process-and-staffing.md#d118) — The stale-CLI test helper fix is its own card, ranked next (card 173)
- [D120](process-and-staffing.md#d120) — Edge-case fixture corpus: card 174 ranked after 173, card 175 for the rest
- [D121](process-and-staffing.md#d121) — The v0.1 public preview bar and its gap cards (176 to 188)
- [D124](process-and-staffing.md#d124) — Licence decision (Apache-2.0 plus trademark policy) and review of card 183
- [D125](process-and-staffing.md#d125) — The public name is Avowry; the rename as nine small cards
- [D127](process-and-staffing.md#d127) — No backward compatibility for the rename; the repository is renamed first
- [D134](process-and-staffing.md#d134) — Mob-process accountability that does not depend on the facilitator
- [D135](process-and-staffing.md#d135) — Review of the mob-process checker (rounds 1 to 3); detection now, prevention as the destination
- [D136](process-and-staffing.md#d136) — Thin slices, numeric limits, flags in a compiler repo, retro timing (forward-only from card 195)
- [D137](process-and-staffing.md#d137) — Outside-in TDD under trunk-based main: the pending-test mechanism
- [D142](process-and-staffing.md#d142) — Re-vote of D136 parts 2 to 4 on the final wording; flag-scoring clause amended
- [D143](process-and-staffing.md#d143) — Re-vote of D137 on the final wording, item 7 included; item 7 amended
- [D145](process-and-staffing.md#d145) — Findings mirroring and staffing, quota-first and task-force checker rules (Spotcheck now, Sandbox as experiment E7)
- [D148](process-and-staffing.md#d148) — Product owner rulings: 205 to 208 confirmed corrective; priority re-evaluated at every loop boundary
- [D150](process-and-staffing.md#d150) — Ceremonies besides the retro: Three Amigos refinement and a demo after Done as experiments; planning, backlog refinement and standup dropped
- [D151](process-and-staffing.md#d151) — Stand-down: a micro-retro at each cycle boundary within a card, replaced by the full retro at Done (amends D150)
- [D153](process-and-staffing.md#d153) — A lighter path for routine per-card decisions: objection windows; full Deciders for objections, one-way doors and rule changes
- [D154](process-and-staffing.md#d154) — Claim-equals-commit and pre-ship evidence gate, a 5-card experiment (card 205 retro pick)
- [D155](process-and-staffing.md#d155) — Every stand-down and retro opens by reviewing each previous adjustment (product owner ruling)
- [D157](process-and-staffing.md#d157) — One acceptance criterion per red-green cycle, checked from git, a 5-card experiment (card 214 retro pick)
- [D158](process-and-staffing.md#d158) — Facilitator pre-window checklist: seats, caps, working directory, line endings (card 209 retro pick, 5-card experiment)
- [D159](process-and-staffing.md#d159) — Record checker: evidence is compared with the record committed before this card's record (out-of-order closes)
- [D161](process-and-staffing.md#d161) — Product owner direction: lock the mob process into tooling and the mob-defaults template before product work; reports describe, not cite
- [D162](process-and-staffing.md#d162) — Product owner ruling: one-card waiver of the stop-the-line gate for card 219 (record-order fix)
- [D163](process-and-staffing.md#d163) — Product owner clarification: balanced model rotation (least-recently-used available model first), not a rule against Claude
- [D164](process-and-staffing.md#d164) — Correction to card 208: least-recently-used model per seat from a tracked seat-use history, replacing the per-seat ban on Claude (D163)
- [D165](process-and-staffing.md#d165) — Intent line before the first red, confirmed against recorded direction; feedback-loop lens as a standing retro section (card 208 retro pick)
- [D166](process-and-staffing.md#d166) — An assumption line with a probe in the slice of any card that changes a rule, the ranking or the checker (card 219 retro pick)
- [D167](process-and-staffing.md#d167) — A hostile-input list in the slice of any criterion that takes a list or map of names, answered entry by entry in the red (card 226 retro pick)
- [D168](process-and-staffing.md#d168) — A slice size cap of 15 KB, larger cards split into numbered slices (card 227 retro pick)
- [D169](process-and-staffing.md#d169) — Product owner ruling: detection after the fact is acceptable for the deletion-ban corrective if work is committed and pushed regularly and no recursive delete targets anything outside the working tree
- [D170](process-and-staffing.md#d170) — Product owner ruling: the indexed process wiki (card 234) is the highest priority, with a one-card stop-the-line waiver (corrected by D172: evaluate, not build)
- [D171](process-and-staffing.md#d171) — Cite-a-point votes: every vote quotes a point from the proposal, checkable in git; an unread vote does not count (card 234 retro pick)
- [D172](process-and-staffing.md#d172) — Product owner correction: the indexed process wiki is to be evaluated (adopt, reshape or reject by Decider), not built as a mandate; the waiver applies to the evaluation (card 237)
- [D173](process-and-staffing.md#d173) — Card 237 outcome: reshape the indexed process wiki (slice-route limits, upkeep measure, staleness check, slice-cell re-run in a follow-up card); retro-end wiki review as a three-retro experiment
- [D174](process-and-staffing.md#d174) — Parallel voter dispatch: probe once, assign the whole rotation and write every seat-use line in one window commit, launch all voters at once (card 237 retro pick, experiment, next 3 vote windows)
- [D175](process-and-staffing.md#d175) — Written cycle cap per acceptance criterion in every slice, hostile forms labelled SEEN or INSURANCE, a scope window before any red past a cap (card 228 retro pick, experiment, next 3 cards)
- [D176](process-and-staffing.md#d176) — Pre-ship gate reports state each test run's environment and re-run a failing module the card did not touch in the reference shell before the verdict (card 210 retro pick, experiment, next 3 gates)
- [D177](process-and-staffing.md#d177) — Product owner ruling: card 211 Done check 3 waives the different-committing-identity requirement; replacement evidence is a fresh-context, non-author seat's final-state review with seat attribution, cited from the Decider and closing records
- [D178](process-and-staffing.md#d178) — A breaker for decision-seat rotation: a model that returns two empty or voteless replies in a row, or reports its quota reached, leaves decision seats until a probe vote parses; per path, so it keeps voice, ready, review and gate seats (card 211 retro pick, experiment, next 3 decision windows)
- [D179](process-and-staffing.md#d179) — Product owner ruling: card the Muse Code trial (card 276) and rank it next after card 211; first slice install, device-code sign-in, probe, event-log reader (voting seats first)
- [D180](process-and-staffing.md#d180) — Product owner ruling: card 276 stays open (clean scan not waived) until Muse's Windows shell sandbox is set up by Dispatch; then cycle 4 runs as adopted and the card is re-gated
- [D181](process-and-staffing.md#d181) — A lettered red is allowed only before any green has passed the red it corrects; after that the next red is a new cycle under the D175 cap and window; one window may set the remaining cap but never ratifies cycles already run (card 276 retro pick, experiment, next 3 capped cards)
- [D183](process-and-staffing.md#d183) — Process improvements serve the user story being worked and are not board cards; the board holds only user stories and bugs; the next re-rank sorts existing process cards (real safety, decision-vote and test defects stay as bugs, the rest become retro experiments); merit review by Decider follows
- [D184](process-and-staffing.md#d184) — Each premise a scope proposal rests on is run in code on HEAD before its window, with the command and output quoted, or labelled unrun with what the green does if it is false (card 229 retro pick, experiment)
- [D185](process-and-staffing.md#d185) — Re-rank 21: the D183 sort applied (98 product, 16 process bugs stay; 40 retro experiments to docs/retros/experiments.md with trigger keys; 282 delivered; 067, 063, 071, 003 to 007 superseded); next card 290

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
| [D118](process-and-staffing.md#d118) | The stale-CLI test helper fix is its own card, ranked next (card 173) | process-and-staffing | in force |
| [D119](compiler-and-adapters.md#d119) | Review of card 170: step ids recomputed independently and unique per failure per endpoint | compiler-and-adapters | applied |
| [D120](process-and-staffing.md#d120) | Edge-case fixture corpus: card 174 ranked after 173, card 175 for the rest | process-and-staffing | in force |
| [D121](process-and-staffing.md#d121) | The v0.1 public preview bar and its gap cards (176 to 188) | process-and-staffing | in force |
| [D122](compiler-and-adapters.md#d122) | Review of card 173: the CLI test helper fails loudly when stale | compiler-and-adapters | applied |
| [D123](compiler-and-adapters.md#d123) | Review of card 174: the edge-case fixture corpus | compiler-and-adapters | applied |
| [D124](process-and-staffing.md#d124) | Licence decision (Apache-2.0 plus trademark policy) and review of card 183 | process-and-staffing | in force |
| [D125](process-and-staffing.md#d125) | The public name is Avowry; the rename as nine small cards | process-and-staffing | in force |
| [D126](compiler-and-adapters.md#d126) | The in-language unit is an avow; declaration form and filler words | compiler-and-adapters | in force |
| [D127](process-and-staffing.md#d127) | No backward compatibility for the rename; the repository is renamed first | process-and-staffing | in force |
| [D128](compiler-and-adapters.md#d128) | Review of card 175: more edge-case fixtures | compiler-and-adapters | applied |
| [D129](process-and-staffing.md#d129) | Review of card 190: the product is called Avowry in user-facing documents | process-and-staffing | applied |
| [D130](process-and-staffing.md#d130) | Review of card 191: the command is avowry; output and the map file say Avowry | process-and-staffing | applied |
| [D131](compiler-and-adapters.md#d131) | Review of card 199: the avow keyword, filler words and the .avowry extension | compiler-and-adapters | applied |
| [D132](compiler-and-adapters.md#d132) | Review of card 193: generated code uses Avowry names | compiler-and-adapters | applied |
| [D133](compiler-and-adapters.md#d133) | Review of card 194: the compiler project and namespace are Avowry.Compiler | compiler-and-adapters | applied |
| [D134](process-and-staffing.md#d134) | Mob-process accountability that does not depend on the facilitator | process-and-staffing | in force |
| [D135](process-and-staffing.md#d135) | Review of the mob-process checker (rounds 1 to 3); detection now, prevention as the destination | process-and-staffing | in force |
| [D136](process-and-staffing.md#d136) | Thin slices, numeric limits, flags in a compiler repo, retro timing (forward-only from card 195) | process-and-staffing | in force |
| [D137](process-and-staffing.md#d137) | Outside-in TDD under trunk-based main: the pending-test mechanism | process-and-staffing | in force |
| [D138](process-and-staffing.md#d138) | Review and signoff of Card 195 (first card-mode card since 063) | process-and-staffing | applied |
| [D139](process-and-staffing.md#d139) | Mirror audit findings and make the staffing, quota-first and task-force rules checkable (failed; re-proposed as D140) | process-and-staffing | applied |
| [D142](process-and-staffing.md#d142) | Re-vote of D136 parts 2 to 4 on the final wording; flag-scoring clause amended | process-and-staffing | in force |
| [D143](process-and-staffing.md#d143) | Re-vote of D137 on the final wording, item 7 included; item 7 amended | process-and-staffing | in force |
| [D140](process-and-staffing.md#d140) | Findings mirroring and checker rules, re-proposal of D139 (failed; re-proposed as D141) | process-and-staffing | applied |
| [D141](process-and-staffing.md#d141) | Findings mirroring and checker rules, second re-proposal (failed; re-proposed as D144) | process-and-staffing | applied |
| [D144](process-and-staffing.md#d144) | Findings mirroring and checker rules, third re-proposal (failed; probe-log verification escalated) | process-and-staffing | applied |
| [D145](process-and-staffing.md#d145) | Findings mirroring and staffing, quota-first and task-force checker rules (Spotcheck now, Sandbox as experiment E7) | process-and-staffing | in force |
| [D146](process-and-staffing.md#d146) | Task force of card 205 (D145 card A1); slice split into cards 205, 206, 207 | process-and-staffing | applied |
| [D147](process-and-staffing.md#d147) | Re-rank of the corrective cards under audit 18 (209, 210, 211, 208 first; 205 to 207 resume on the auditor's closures) | process-and-staffing | applied |
| [D148](process-and-staffing.md#d148) | Product owner rulings: 205 to 208 confirmed corrective; priority re-evaluated at every loop boundary | process-and-staffing | in force |
| [D150](process-and-staffing.md#d150) | Ceremonies besides the retro: Three Amigos refinement and a demo after Done as experiments; planning, backlog refinement and standup dropped | process-and-staffing | in force |
| [D149](process-and-staffing.md#d149) | Re-rank at the loop boundary: 205, 209, 210, 208, 211, 212 | process-and-staffing | applied |
| [D151](process-and-staffing.md#d151) | Stand-down: a micro-retro at each cycle boundary within a card, replaced by the full retro at Done (amends D150) | process-and-staffing | in force |
| [D152](process-and-staffing.md#d152) | Mid-card task-force declaration fix on card 205 | process-and-staffing | applied |
| [D153](process-and-staffing.md#d153) | A lighter path for routine per-card decisions: objection windows; full Deciders for objections, one-way doors and rule changes | process-and-staffing | in force |
| [D154](process-and-staffing.md#d154) | Claim-equals-commit and pre-ship evidence gate, a 5-card experiment (card 205 retro pick) | process-and-staffing | in force |
| [D155](process-and-staffing.md#d155) | Every stand-down and retro opens by reviewing each previous adjustment (product owner ruling) | process-and-staffing | in force |
| [D156](process-and-staffing.md#d156) | Card 214 task force, card text and the delivery lead's objection | process-and-staffing | applied |
| [D157](process-and-staffing.md#d157) | One acceptance criterion per red-green cycle, checked from git, a 5-card experiment (card 214 retro pick) | process-and-staffing | in force |
| [D158](process-and-staffing.md#d158) | Facilitator pre-window checklist: seats, caps, working directory, line endings (card 209 retro pick, 5-card experiment) | process-and-staffing | in force |
| [D159](process-and-staffing.md#d159) | Record checker: evidence is compared with the record committed before this card's record (out-of-order closes) | process-and-staffing | in force |
| [D160](process-and-staffing.md#d160) | No gate exemption for card 219; card 208 runs before card 210 so no corrective close is refused | process-and-staffing | applied |
| [D161](process-and-staffing.md#d161) | Product owner direction: lock the mob process into tooling and the mob-defaults template before product work; reports describe, not cite | process-and-staffing | in force |
| [D162](process-and-staffing.md#d162) | Product owner ruling: one-card waiver of the stop-the-line gate for card 219 (record-order fix) | process-and-staffing | in force |
| [D163](process-and-staffing.md#d163) | Product owner clarification: balanced model rotation (least-recently-used available model first), not a rule against Claude | process-and-staffing | in force |
| [D164](process-and-staffing.md#d164) | Correction to card 208: least-recently-used model per seat from a tracked seat-use history, replacing the per-seat ban on Claude (D163) | process-and-staffing | in force |
| [D165](process-and-staffing.md#d165) | Intent line before the first red, confirmed against recorded direction; feedback-loop lens as a standing retro section (card 208 retro pick) | process-and-staffing | in force |
| [D166](process-and-staffing.md#d166) | An assumption line with a probe in the slice of any card that changes a rule, the ranking or the checker (card 219 retro pick) | process-and-staffing | in force |
| [D167](process-and-staffing.md#d167) | A hostile-input list in the slice of any criterion that takes a list or map of names, answered entry by entry in the red (card 226 retro pick) | process-and-staffing | in force |
| [D168](process-and-staffing.md#d168) | A slice size cap of 15 KB, larger cards split into numbered slices (card 227 retro pick) | process-and-staffing | in force |
| [D169](process-and-staffing.md#d169) | Product owner ruling: detection after the fact is acceptable for the deletion-ban corrective if work is committed and pushed regularly and no recursive delete targets anything outside the working tree | process-and-staffing | in force |
| [D170](process-and-staffing.md#d170) | Product owner ruling: the indexed process wiki (card 234) is the highest priority, with a one-card stop-the-line waiver (corrected by D172: evaluate, not build) | process-and-staffing | in force |
| [D171](process-and-staffing.md#d171) | Cite-a-point votes: every vote quotes a point from the proposal, checkable in git; an unread vote does not count (card 234 retro pick) | process-and-staffing | in force |
| [D172](process-and-staffing.md#d172) | Product owner correction: the indexed process wiki is to be evaluated (adopt, reshape or reject by Decider), not built as a mandate; the waiver applies to the evaluation (card 237) | process-and-staffing | in force |
| [D173](process-and-staffing.md#d173) | Card 237 outcome: reshape the indexed process wiki (slice-route limits, upkeep measure, staleness check, slice-cell re-run in a follow-up card); retro-end wiki review as a three-retro experiment | process-and-staffing | in force |
| [D174](process-and-staffing.md#d174) | Parallel voter dispatch: probe once, assign the whole rotation and write every seat-use line in one window commit, launch all voters at once (card 237 retro pick, experiment, next 3 vote windows) | process-and-staffing | in force |
| [D175](process-and-staffing.md#d175) | Written cycle cap per acceptance criterion in every slice, hostile forms labelled SEEN or INSURANCE, a scope window before any red past a cap (card 228 retro pick, experiment, next 3 cards) | process-and-staffing | in force |
| [D176](process-and-staffing.md#d176) | Pre-ship gate reports state each test run's environment and re-run a failing module the card did not touch in the reference shell before the verdict (card 210 retro pick, experiment, next 3 gates) | process-and-staffing | in force |
| [D177](process-and-staffing.md#d177) | Product owner ruling: card 211 Done check 3 waives the different-committing-identity requirement; replacement evidence is a fresh-context, non-author seat's final-state review with seat attribution, cited from the Decider and closing records | process-and-staffing | in force |
| [D178](process-and-staffing.md#d178) | A breaker for decision-seat rotation: a model that returns two empty or voteless replies in a row, or reports its quota reached, leaves decision seats until a probe vote parses; per path, so it keeps voice, ready, review and gate seats (card 211 retro pick, experiment, next 3 decision windows) | process-and-staffing | in force |
| [D179](process-and-staffing.md#d179) | Product owner ruling: card the Muse Code trial (card 276) and rank it next after card 211; first slice install, device-code sign-in, probe, event-log reader (voting seats first) | process-and-staffing | in force |
| [D180](process-and-staffing.md#d180) | Product owner ruling: card 276 stays open (clean scan not waived) until Muse's Windows shell sandbox is set up by Dispatch; then cycle 4 runs as adopted and the card is re-gated | process-and-staffing | in force |
| [D181](process-and-staffing.md#d181) | A lettered red is allowed only before any green has passed the red it corrects; after that the next red is a new cycle under the D175 cap and window; one window may set the remaining cap but never ratifies cycles already run (card 276 retro pick, experiment, next 3 capped cards) | process-and-staffing | in force |
| [D182](process-and-staffing.md#d182) | Re-rank 20: the cmd.exe vote fix first (278); drift resistance named as a direction with its measure to pre-register (284, in part); a cut line replacing line 36's process-first rule, conditional on the product owner's confirmation (288); card 279 closed as delivered by 276; next card 229 | process-and-staffing | in force (superseded in part by D183) |
| [D183](process-and-staffing.md#d183) | Process improvements serve the user story being worked and are not board cards; the board holds only user stories and bugs; the next re-rank sorts existing process cards (real safety, decision-vote and test defects stay as bugs, the rest become retro experiments); merit review by Decider follows | process-and-staffing | in force |
| [D184](process-and-staffing.md#d184) | Each premise a scope proposal rests on is run in code on HEAD before its window, with the command and output quoted, or labelled unrun with what the green does if it is false (card 229 retro pick, experiment) | process-and-staffing | in force |
| [D185](process-and-staffing.md#d185) | Re-rank 21: the D183 sort applied (98 product, 16 process bugs stay; 40 retro experiments to docs/retros/experiments.md with trigger keys; 282 delivered; 067, 063, 071, 003 to 007 superseded); next card 290 | process-and-staffing | in force |
| [D186](process-and-staffing.md#d186) | Card 290 retro pick (a measure-only premise probe before the slice): not adopted after three votes; logged as a retro experiment with its objections | process-and-staffing | not adopted |
| [D187](process-and-staffing.md#d187) | Card 230 retro pick: trial the targeted re-ask (one condition, no No) in the next three cards' decision windows | process-and-staffing | experiment |
| [D188](process-and-staffing.md#d188) | Card 297 retro pick: the in-flight command refuses a seat start on a probe older than 50 minutes (F-085), next three cards | process-and-staffing | experiment |
| [D189](process-and-staffing.md#d189) | Card 264 retro pick: a slice's numbers carry the commands that produced them, next three bug cards | process-and-staffing | experiment |
| [D190](process-and-staffing.md#d190) | Card 217 retro pick: a pre-ship gate says SHIP only on commands it ran or named, next three cards with a gate | process-and-staffing | experiment |
| [D191](process-and-staffing.md#d191) | Card 303 retro pick: adopt the product-owner inbox as the decision channel (fails the strict count: one unresolved No), re-voted as D192 | process-and-staffing | not adopted |
| [D192](process-and-staffing.md#d192) | Card 303 retro pick re-vote: adopt the product-owner inbox with a committed log and a mechanical check (fails: one unresolved No; escalated to the product owner) | process-and-staffing | not adopted |
| [D193](process-and-staffing.md#d193) | Card 298 retro pick: the gate runs the changed area's narrow suites and checks each step's failure path; a cycle with no red carries a mutation pair (three-card trial; story 316) | process-and-staffing | adopted |
| [D194](process-and-staffing.md#d194) | Card 313 task-force declaration with the facilitator-trace conditions (an outlier unresolved after the amendments; a new proposal follows as D195) | process-and-staffing | not adopted |
| [D195](process-and-staffing.md#d195) | Card 313 task-force declaration, new proposal: the green seat works in its own git worktree | process-and-staffing | adopted |
| [D196](process-and-staffing.md#d196) | Card 313 retro pick: the cite check stops costing rounds on format (three-card trial; story 322) | process-and-staffing | adopted |
| [D197](process-and-staffing.md#d197) | Re-rank 27: product first; process improvements moved off the board; the order of product stories and bugs | process-and-staffing | adopted |
| [D198](process-and-staffing.md#d198) | Card 299 task-force declaration: the cross-process writer lock, split from the git hooks | process-and-staffing | adopted |
| [D199](process-and-staffing.md#d199) | Card 299 retro pick: the Experimental section, six lines adopted, three dropped, sixteen moved | process-and-staffing | adopted |
| [D200](process-and-staffing.md#d200) | Re-rank 28: bug card 323 ranks after card 232, below every product story; pull card 196 | process-and-staffing | adopted |
| [D201](process-and-staffing.md#d201) | Card 196 task-force declaration and card text: rename the two test projects, their namespaces and the paths that name them | process-and-staffing | adopted |
| [D202](process-and-staffing.md#d202) | Card 196 retro pick: mechanical rewrites carry a byte-parity check; gate runs are isolated with a load line | process-and-staffing | adopted |
| [D203](process-and-staffing.md#d203) | Card 197 task-force declaration and card text: rename the solution and the benchmark tool projects to Avowry | process-and-staffing | adopted |
| [D204](process-and-staffing.md#d204) | Card 197 retro pick: red before the atomic green; the D196 cite check adopted | process-and-staffing | adopted |
| [D205](process-and-staffing.md#d205) | Card 266 task-force declaration, the five decisions, the split into 266 and two new cards, and the card text | process-and-staffing | adopted |
| [D206](process-and-staffing.md#d206) | Card 266 retro pick: guard tests read committed blobs; a cold-reader seat for docs cards; a red a CI test wraps is pushed with its green | process-and-staffing | adopted |
| [D207](process-and-staffing.md#d207) | Card 324 task-force declaration and card text: compiler diagnostics call the unit an avow | process-and-staffing | adopted |
| [D208](process-and-staffing.md#d208) | Card 324 retro pick, first form: gate preparation, byte-parity permanent, Dimensions of Difficulty trial (failed; two parts re-proposed as D209) | process-and-staffing | failed |
| [D209](process-and-staffing.md#d209) | Card 324 retro pick, second form: gate preparation with committed gate evidence; byte-parity permanent (failed; byte-parity re-proposed alone) | process-and-staffing | failed |
| [D210](process-and-staffing.md#d210) | Card 324 retro pick, third form: the byte-parity check becomes a permanent practice (failed; the practice stays on trial) | process-and-staffing | failed |
| [D211](process-and-staffing.md#d211) | Re-rank 29 (after card 324 closed): place cards 176, 178, 186 and 179 after card 324 and pull card 176 | process-and-staffing | adopted |
| [D212](process-and-staffing.md#d212) | Card 176 task-force declaration and card text: package the CLI as a dotnet tool and version it (failed; corrected and re-proposed as D213) | process-and-staffing | failed |
| [D213](process-and-staffing.md#d213) | Card 176 task-force declaration and card text, corrected after D212 (failed; one condition re-proposed as D214) | process-and-staffing | failed |
| [D214](process-and-staffing.md#d214) | Card 176 task-force declaration and card text, second correction (adopted after the outliers' confirmation) | process-and-staffing | adopted |
| [D215](process-and-staffing.md#d215) | Card 176 retro pick: a task-force declaration and every amendment carry a standing-rules preflight (failed on the architect's condition; kept as the facilitator's practice) | process-and-staffing | failed |
| [D216](process-and-staffing.md#d216) | Card 178 task-force declaration and card text: a prerequisites page that matches what CI tests (adopted at the confirmation step) | process-and-staffing | adopted |
| [D217](process-and-staffing.md#d217) | Card 178 retro pick: the confirmation step replaces the separate independent re-run (adopted as a five-Decider trial) | process-and-staffing | adopted |
| [D218](process-and-staffing.md#d218) | Card 179 task-force declaration and card text (README and getting-started guide) | process-and-staffing | adopted |
| [D219](process-and-staffing.md#d219) | Card 179 retro pick: on a docs card the independent gate review starts on the final tip (adopted as a three-docs-card trial) | process-and-staffing | adopted |
| [D220](process-and-staffing.md#d220) | Re-rank 31: place cards 162 and 201 after card 203 so the head of the board is a card the mob can pull | process-and-staffing | adopted |
| [D221](process-and-staffing.md#d221) | Card 189 task-force declaration and card text (an unsupported construct is a diagnostic, not an unhandled exception) | process-and-staffing | adopted |
| [D222](process-and-staffing.md#d222) | Card 189 retro pick: a Ready table row that names two constructs becomes two ids (adopted as a three-code-card trial) | process-and-staffing | adopted |
| [D223](process-and-staffing.md#d223) | Card 202 task-force declaration and card text (a native block that fails with an undeclared code is a diagnostic) | process-and-staffing | adopted |
| [D224](process-and-staffing.md#d224) | Card 202 retro pick: adopt the D217 trial, the confirmation step in place of the separate independent re-run | process-and-staffing | adopted |
| [D225](process-and-staffing.md#d225) | Card 186 task-force declaration and card text (pre-publication checks: name, licences, history, vendor terms) | process-and-staffing | adopted |
| [D226](process-and-staffing.md#d226) | Card 186 retro pick: tune D219, every review of a docs card page finishes before the gate review starts | process-and-staffing | adopted |
| [D227](process-and-staffing.md#d227) | Card 203 task-force declaration and card text (names reserved by generated code are rejected) | process-and-staffing | adopted |
| [D228](process-and-staffing.md#d228) | Card 203 retro pick: a preflight before every vote window (tune D174) | process-and-staffing | adopted |
| [D229](process-and-staffing.md#d229) | Card 361 task-force declaration and card text (diagnostic codes use the AV prefix) | process-and-staffing | adopted |
| [D230](process-and-staffing.md#d230) | Card 361 retro pick: no idle between steps | process-and-staffing | adopted |
| [D231](process-and-staffing.md#d231) | Card 366 task-force declaration and card text (the NOTICE and TRADEMARKS files name the holder and the year) | process-and-staffing | adopted |
| [D232](process-and-staffing.md#d232) | Card 366 retro pick: a No whose condition a rule in force meets costs one question to the objector | process-and-staffing | adopted |
| [D233](process-and-staffing.md#d233) | Card 371 task-force declaration and card text (the pre-publication page shows the owner answers and the trademark search brief) | process-and-staffing | adopted |
| [D234](process-and-staffing.md#d234) | Card 371 retro pick: a task-force declaration carries the standing evidence | process-and-staffing | adopted |
| [D235](process-and-staffing.md#d235) | Card 370 task-force declaration and card text (tracked files carry no home path of a local account) | process-and-staffing | adopted |
| [D236](process-and-staffing.md#d236) | Card 370 retro pick: seat plans follow a live probe; a misbehaving driver is repaired, not benched | process-and-staffing | adopted |
| [D237](process-and-staffing.md#d237) | Card 162 task-force declaration and card text (fold the pattern steps into the grammar and cheat sheet; re-freeze; rerun the LLM check with Codex and Grok) | process-and-staffing | adopted |
| [D238](language-and-grammar.md#d238) | Card 162 re-freeze of the documents the LLM-customer check reads (D72, D127) | language-and-grammar | adopted |
| [D239](process-and-staffing.md#d239) | Card 376 task-force declaration and card text (the account name standing alone is gone from tracked files; the tree scan gates by exit code; an account name with a space) | process-and-staffing | adopted |
| [D240](process-and-staffing.md#d240) | Card 376 retro pick: prompts to an external driver are checked for the local account name before launch | process-and-staffing | adopted |
| [D241](process-and-staffing.md#d241) | Card 365 task-force declaration and card text (concurrent seat-use appends never fail the append-only check; bug 374 right after) | process-and-staffing | adopted |
| [D242](process-and-staffing.md#d242) | Card 365 retro pick: a writing seat keeps each file's existing line endings | process-and-staffing | adopted |
| [D244](process-and-staffing.md#d244) | Remove the one-seat-at-a-time sentence from the Parallel voter dispatch entry, now that bugs 365 and 374 are fixed | process-and-staffing | adopted |
| [D243](process-and-staffing.md#d243) | Card 374 retro pick: a merge is verified before a gate starts | process-and-staffing | adopted |
| [D245](process-and-staffing.md#d245) | Card 379 retro pick: a change to how seats start carries a live start-proof | process-and-staffing | adopted |
| [D246](process-and-staffing.md#d246) | Card 368 declaration: task force and card text (new bench rows publish no reply prefix) | process-and-staffing | adopted |
| [D247](process-and-staffing.md#d247) | Card 368 retro pick: test classes that start a tool under a hard time limit share one xunit collection | process-and-staffing | adopted |
| [D248](process-and-staffing.md#d248) | Card 369 declaration: task force and card text (the licences of every package a generated service and its specs restore are reviewed and listed) | process-and-staffing | adopted |
| [D249](process-and-staffing.md#d249) | Card 369 retro pick: the slice names the guard tests that scan any new kind of tracked file | process-and-staffing | adopted |
| [D250](process-and-staffing.md#d250) | Card 418 declaration: task force and card text (new bench rows name files by their short name and hold no folder from the machine that ran them) | process-and-staffing | adopted |
| [D251](process-and-staffing.md#d251) | Card 418 retro pick: a gate records the toolchain version before and after and refuses a verdict when it changed | process-and-staffing | adopted |
| [D252](process-and-staffing.md#d252) | Card 417 declaration: task force and card text (every class that starts the bench tools shares the Bench processes collection, and a guard test fails when one does not) | process-and-staffing | adopted |
| [D253](process-and-staffing.md#d253) | Card 417 revised after the measurement: the timing test alone runs in an isolated collection, with a guard for any class that asserts a wall-clock limit | process-and-staffing | adopted |
| [D254](process-and-staffing.md#d254) | Card 417 retro pick: the D247 entry is rewritten (a test that asserts a wall-clock limit runs alone in an isolated collection) | process-and-staffing | adopted |
| [D255](process-and-staffing.md#d255) | Card 426: task force declaration and Ready text (the bench path scrub keeps the words around a path, covers more path forms, and its label and save-step behaviour is tested) | process-and-staffing | adopted |
| [D256](process-and-staffing.md#d256) | Card 426 retro pick: a change to a function that removes, rewrites or recognises text is checked against its old version on generated inputs | process-and-staffing | adopted |
