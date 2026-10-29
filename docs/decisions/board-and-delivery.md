# Board And Delivery decisions

Board ordering, milestones, continuous delivery, and shipped-card decisions.
Process, compiler, and measurement policy belong in their own area files.

## In force

- [D7](#d7) — The live mob dashboard builds only non-exposing parts (event stream, stall detection, local dashboard)
- [D9/D10](#d9-d10) — The usable-v1 CI job passes while the milestone is pending and fails only on a regression of a ratcheted bar
- [D23](#d23) — The loop runner in mob-defaults: a thin supervisor over card mode with pinned toolchain, one lock, retry then park, spend ceilings
- [D24](#d24) — The loop never ends on its own: new work is discovered in every retro's product category, and it stops only for a STOP file, a one-way door or all allotments out
- [D25](#d25) — The board is a private GitHub Projects board with an issue per card
- [D28](#d28) — Every card is a Story or a Contract; a Contract has a system actor as consumer, enables an existing Story and is never an orphan
- [D102](#d102) — CI on main is part of the line: a red main stops the line, checked after every push and at each loop iteration by a script; the workflow is kept and fixed
- [D113](#d113) — Card 161 closes on its core outcome; the rest becomes five small cards

Related decisions: [D8](process-and-staffing.md#d8), [D11](benchmarks-and-measurement.md#d11), [D15](process-and-staffing.md#d15), [D22](process-and-staffing.md#d22), [D27](process-and-staffing.md#d27), [D74](compiler-and-adapters.md#d74), [D90](compiler-and-adapters.md#d90), [D103](benchmarks-and-measurement.md#d103)

## Records

<a id="d1"></a>
#### Decider D1: rank bug card 018 (spec resolution defects) at rank 4, after card 013 and before the emitter

- Date: 2026-10-25. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (votes collected in parallel, read-only).
- Votes: Codex Yes; Copilot Yes; agy Yes; Grok Support-it.
- Result: passed. Outliers: none.

<a id="d2"></a>
#### Decider D2: rank the three retro-002 cards: 019 at rank 5 (after 018), 020 after 017, 021 after 015 (Decider first)

- Date: 2026-10-25. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Support-it; Grok Yes; Agy Yes.
- Result: passed. Outliers: none.

<a id="d3"></a>
#### Decider D3: assess the board against VISION.md: insert 022, 023, 025 on the Usable v1 path; add milestone M4 Vocabulary v1 (024, 026, 027, 028, 029); Goodhart-guarded 'comprehensive'

- Date: 2026-10-25. Proposer: PM and domain expert (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Yes; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

<a id="d5"></a>
#### Decider D5: declare card 016 Done on its written criteria but not M2; file bug card 032 (invalid baseline) at rank 3; run 032 as the first E1 true-mobbing card with a Codex/Grok/Copilot/agy task force

- Date: 2026-10-25. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Support-it; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

<a id="d6"></a>
#### Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two criteria into 022; record raw candidates

- Date: 2026-10-25. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Yes; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

<a id="d7"></a>
#### Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all mobs; build only what exposes nothing (event stream, stall detection, allow-list snapshot with test, LOCAL dashboard); choose and use no hosting until product owner gives a one-word OK on one specific option

- Date: 2026-10-25. Proposer: PM, architect (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Round 1 (public status-only repo with GitHub Pages recommended by the architect): Codex Yes; Grok Yes; agy Yes; Copilot ABSOLUTELY NO ("verified privacy-safe design, no code or personal exposure, a secured hosting model, and the product owner's explicit approval first"). Result: failed.
- Amended by the proposer to address the outlier (hosting becomes a decision made WITH product owner before any exposure; the card builds the non-exposing parts and a local dashboard). Round 2: Codex Yes; Copilot Support-it; Grok Yes; agy Yes. Result: passed.

<a id="d9-d10"></a>
#### Decider D9 (failed) and D10 (passed): make the usable-v1 CI job pass while the milestone is pending and fail only on a regression

- Date: 2026-10-26. Proposer: PM (facilitator drafted), on the product owner's direction via the user that a permanently red check trains everyone to ignore red; the details were left to a Decider. Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- D9 round 1 (job green with 'not reached: X of 6 criteria', fails only if a ratcheted bar regresses, per-bar tests): Codex Yes; Copilot Yes; agy Yes; Grok No (a bar must count as met, and enter the ratchet, only when its test checks what the written bar says, not a file-existence probe).
- D9 round 2 (amended: every bar tested for what it says; unwired bars count as not met): Codex Yes; Copilot Yes; agy Yes; Grok No (bar 2 must be tested against the real demo app so bars 3 to 5 verify one pipeline). Result: failed (an outlier remained after the amendment).
- D10 (narrow, adopting Grok's condition: bar 2 is met only when `phrase build demo/tasks/Tasks.phrase` produces the projects that bars 3 to 5 then build, test and call): Codex Yes; Copilot Yes; Grok Yes; agy Yes. Result: passed. Outliers: none.

#### Whole-team sign-off on card 032, second round (after the harness fixes and the re-run baseline, cfb1952)

- Date: 2026-10-26. Members: Codex, Copilot, Grok, agy, reading the stored rows and the commits against the card (read-only). The first round (earlier the same day) FLAGGED two real defects: agy's empty replies and Copilot's missing task row.
- Result: SIGN-OFF from all four seats.

<a id="d13"></a>
#### Decider D13: amend the hash-locked acceptance test of card 036 (CSP regex), and the facilitator takeover that fixed the page

- Date: 2026-10-26. Proposer: PM (facilitator). Seats: Codex, Copilot, Grok, agy (sandboxed runner, read-only).
- Proposal: the test's regex excluded both quote characters inside the captured Content-Security-Policy value, so `default-src 'self'` could never match for any valid policy. Amend only that regex to be quote-aware; keep every assertion after it; update the sha256 in the card (`e68b9352…`).
- Votes: Codex Yes; Copilot Yes; Grok Yes; agy Yes. Result: passed. Outliers: none.
- Consequence: the corrected test then caught two real page weaknesses against its stricter source list; the page dropped `data:` from img-src and `object-src 'none'` (default-src 'self' still applies). TAKEOVER (facilitator, small fix after the 5-cycle cap and time-box; logged per the tooling authorship rule).
- Result of the card: Done-when green: 15 of 15 acceptance tests, hashes intact, whole mob-defaults suite 120 OK (1 skipped).

<a id="d14"></a>
#### Decider D14: board order after the product owner withdrew their re-rank (round 1: restored order as is)

- Date: 2026-10-26. Proposer: PM (facilitator). Seats: Codex, Copilot, Grok, agy. Votes: Codex Yes; Copilot Yes; Grok Yes; agy No (0 of 6 Usable v1 bars met; put the C# adapter chain before the benchmark-docs card). Result: failed; raised again amended (D17).

<a id="d16"></a>
#### Decider D16: the product owner's "good starter app" input: the mob status API in Phrase

- Date: 2026-10-26. Lenses: PM, language designer, architect. Votes: Codex Yes (option B); Copilot Support-it (Tasks fits bar 1; the dashboard is better as a follow-on dogfood); Grok Yes (B, first slice of records, in-memory); agy Yes. Result: passed. Option B: keep the Tasks API (009) as the Usable v1 bar-1 app; new card 038 (dogfood status API) after the Usable v1 gate. The product owner's later input (the 30-minute digests cover their visibility need) lowers the urgency of dashboard follow-ups (live wiring, Meshnet bind): they stay seeds, no card.

<a id="d17"></a>
#### Decider D17: board order, rounds 2 and 3

- Round 2 (018, 003 first; 038 after 012): Codex Yes; Copilot Yes; agy Yes; Grok No (033 and 034 must stay before 025, per D6). Round 3 (amended): Codex, Copilot, Grok, agy all Yes. Result: passed. New order: 018 next up, then 003, 037, 022, 013, 019, 014, 004, 005, 006, the benchmark-dependent cards, 007, 023, 033, 034, 025, 008, 009, 010, 011, 012, then 038. Card 003 still starts with the stash Decider (salvage or drop).

<a id="d23"></a>
#### Decider D23: the product owner's "it should be a loop": the loop runner in mob-defaults, cards 063 to 070

- Date: 2026-10-26. Lenses: architect, safety, PM. Seats: Codex Yes; Grok Yes; Copilot Support-it with changes (its reply was corrupted mid-stream by a CLI glitch but ended Support-it, and the runner read "No" from the garbled text; the votes below are read from the replies); agy no response. Result: passed with changes.
- Agreed design: `loop.py` in mob-defaults is a thin supervisor over card mode; state in the gitignored `.mob/loop/` (atomic state.json, heartbeat.json, one OS file lock); the picker reads the ranked board table, skips Done and SUPERSEDED, honours `Depends on:` and resumes an in-flight card first; toolchain (.NET 8) pinned in a checked-in file and verified before dispatch; on Done run the retro step and the re-rank hook once; on cap or failure one retry, then park with a split proposal and move to the next independent card; exits 2 and 3 are parked, exit 4 gets one Decider then one retry, exit 5 and Safety flags halt; escalate only for product direction, an unresolved Decider conflict or the safety carve-out, through a Needs-product-owner file in the published MOB.md and a one-word reply file; spend ceilings per card and per day; STOP file. Registering a Windows logon task changes system configuration: the product owner's one-word ask, written but not run by the tool.
- Cards (all tooling, thin, rotation authorship with non-author review and the smoke run): 063 picker, 064 lock/state/heartbeat, 065 pinned toolchain run, 066 failure policy, 067 Done path, 068 liveness/STOP/escalation, 069 resume after reboot, 070 atomic runs. Ranked: 063 to 067 right after 018 (the engine for everything after), 068 to 070 after the first C# slices (039, 040) so product moves too.

<a id="d24"></a>
#### Decider D24: amendment to D23 (the product owner's correction: the loop never ends; discovery instead of stop; allotment pacing)

- Date: 2026-10-26. Principle: the loop continuously discovers work because the team can identify product features when Ready is low or empty. The facilitator/PM amendment was folded into the cards as product direction; any seat may challenge it, which would trigger a Decider. The loop stops ONLY for a STOP file or the product owner saying stop, a one-way door that needs the product owner's OK, or all driver allotments out (it pauses). When Ready is low or empty it runs a discovery step (the product-category retro with LLM-customer feedback from every model, benchmark gaps, pattern mining from VISION's direction, seeds); the PM and domain expert refine the output into cards through a sandboxed Decider, rank them, and the loop continues.
- Cards: 067 amended (empty Ready starts discovery, never a stop); 068 amended (the only stops); new 071 (discovery step) and 072 (pace against the finite allotments, a counter-metric: cards landed per allotment). Ranks: 072 right after 068, 071 after 070.
- Result: passed (recorded as a facilitator/PM call under the lean Decider rule; product direction for the product owner's product).

- **Amendment (same day, the product owner's simplification):** "during every retro we have the product category where new features should be identified." No separate discovery step: card 071 is WITHDRAWN and folded into 067 (the retro's product category produces new cards through a passed Decider; an empty Ready list is a signal that the category is under-producing, raised at the next retro, never a special mode or a stop). Card 072 (allotment pacing) stands. The loop stops only for a STOP file or product owner saying stop, a one-way door that needs the product owner, or all allotments out (it pauses).

<a id="d25"></a>
#### Decider D25: the product owner's board move: Phrase's board becomes a GitHub Projects board (cards 073 to 077)

- Date: 2026-10-26. Seats: Codex Yes; Copilot Yes; Grok Yes; agy no response. Result: passed. Outlier on one point only, project visibility: Codex and Copilot recommended a public project; Grok recommended PRIVATE (a public project is a live operational feed and needs the product owner's one-word OK; the product owner sees a private project while logged in on their phone; the issue text is already public in the repo). Decided PRIVATE under the safety rules (privacy exposure is a safety-carve-out class; the cheaper, reversible default wins); the scripts have no public setting; making it public is a one-word ask for the product owner.
- Agreed design: Project v2 `Phrase Board` under Programming-Made-EZ with fields Rank, Card number, Depends on, Type, Milestone, Author and Status (Ready, In Progress, Done, Superseded/Withdrawn); issue per card titled `(NNN) <title>`, body the card file after its title with a `<!-- phrase-card:NNN -->` marker, nothing from `.mob/`; tooling cards stay issues in phrase-lang; the project is canonical after a verified cutover and `board/*.md` plus the rank table are generated, read-only; the loop picker (card 063's logic) reads the project through `gh api graphql`; the re-rank hook writes Rank after a passed Decider and skips cards whose Rank is newer than its snapshot; migration never deletes, never touches other repos or mobs, default dry run, idempotent by marker; rollback is a git revert (issues stay).
- Cards: 073 create the private project and fields; 074 migrate every card file into one issue; 075 put each issue on the project with its fields; 076 the picker reads the GitHub board (with a real-board check); 077 cutover to generated archive. Ranked immediately after 063 (in flight); 064 now depends on 076; 065 to 072 keep their order behind it.

- **D25 follow-up (2026-10-26):** the real apply found that GitHub reserves the field names Type, Milestone and Author on every project, so the fields are named Kind, Phase and Card author (card 073 and the test now guard this). The private project is live: https://github.com/orgs/Programming-Made-EZ/projects/1. The non-author review of the 063 picker (Decider D26: Codex, Copilot, Grok No; agy no response) found it parses a synthetic board and expects a pid the facilitator never writes; card 063 is SUPERSEDED by 076 (the picker reads the GitHub board) with the defects carried over as criteria.

<a id="d28"></a>
#### Decider D28: the product owner's two card shapes, Story and Contract (proposal A)

- Date: 2026-10-26. Seats: Codex Support-it; Copilot Support-it; Grok Support-it; agy no response. Result: passed with changes (no objections). Adopted: every card is a STORY (a persona from PERSONAS.md, a so-that that persona values, criteria the persona observes) or a CONTRACT (Shape: Contract; Consumer is a COMPONENT, never a person; Given, Observes, Fails with, Invariants, Enables). A card is a Contract only when no persona can observe the outcome; persona-observable compiler work stays a Story. No orphans: every Contract enables at least one existing Story and is cut from a failing persona-level test (outside-in TDD, consumer-driven contracts); a Contract ranks behind the Story it enables unless that is Done. The Ready check accepts either shape and rejects role-only stories, design-goal benefits, a person as consumer, bare-success observations and orphan contracts. Written into the Definition of Ready (this agreement and the mob-defaults template), the PM charter, and `scripts/check-story-persona.py` (with tests). Re-cut: only 040 (the adapter seam) is a Contract (Consumer PhraseCompiler, enables 039); the rest of the thin C# chain stays Stories; loop and board cards are Stories for product owner or Riley. Board fields Shape and Enables: card 080.

<a id="d31"></a>
#### Decider D31: the product owner's priority ruling: measure first (cards 084 to 089), pause the rest behind the head-to-head result

- Date: 2026-10-26. Seats: Codex Support-it (its reply partly failed to read files in the sandbox; its card list matched the plan); Copilot Support-it; Grok Support-it (the fullest plan, adopted as the base); agy no response. The integrity note (HEAD moved) was the facilitator committing during the vote. Result: passed with changes (no objections).
- Cards (the next work after 064, in this order): 084 reference target builds and every orders spec passes (Story, Morgan; README made truthful first); 086 plain C# baseline on the same tasks (Story, Riley; run first among the measurements; holds the pre-registered bar); 085 a missing Phrase reply is a model failure (Story, Riley; the Grok no_phrase_found rows are a harness or prompt problem, not a language result); 084, 085 and 086 are dependency-free and may run as parallel atomic tasks. Then the existing thin chain 039, 040, 041, 042 (List only), 043, 044, 045, 048, 049, 050 as the increments; 087 orders.phrase compiles and its generated specs pass (Story, Dana; sits on top of the chain); 037 docs in the prompt and cold runs (after 087, before 088); 088 head-to-head Phrase vs plain C# (Story, product owner; one page, WIN, LOSS, TIE or INCONCLUSIVE). New 089 option and nested-record fields (moved out of 042, behind the head-to-head). Shape note: product owner proposed card 2 as a Contract; by the D28 rule a card is a Contract only when no persona can observe the outcome, and Riley observes benchmark rows, so 085 is a Story.
- PAUSED behind 088 (kept on the board, ranked after it; the product owner's lean, adopted): vocabulary and grammar growth (013, 014, 015, 019 to 027, 033 to 035, 054 to 061, 081 to 083, 089), the other adapters, synthesis and the converter (028 to 031), full-suite spec scoring (017), the demo and dogfood cards (008 to 012, 038), docs and CLI slices (046, 047, 051, 052), and the further loop cards (062, 065 to 070, 072, 078). Retros stay short on this path (flow numbers and whether the slice held to about three cycles). Fits the agile ruling: small working increments, a bar fixed once, the language specified no further than the next slice.
- Holdouts (needed before 086 starts): a person who will not drive or review 086 to 088 writes the three tasks and gives only their hashes. Asked of product owner in the report.

<a id="d60"></a>
#### Decider D60: rank fix, card 047 directly after card 045

- Date: 2026-10-27. Question: the board ranked 048 (rank 29) before its dependency 047 (rank 58), so the picker skipped 048 and fell through to the paused 089; move 047 directly after 045. Seats: Codex Yes; Copilot Yes. Result: passed. Executed with scripts/board/rerank.py --after 045 --cards 047; order is now 045, 047, 048, 049, 050, 087, 088, 089. Process note: a rank that contradicts the written dependencies is a card-writing slip; the picker's fall-through to a paused card was the symptom.

<a id="d86"></a>
#### Decider D86: the shortest path to a real prototype (Usable v1) leads the board; ZOMBIES spec-coverage warnings

- Date: 2026-10-28. Inputs from product owner via Dispatch: get to a real prototype first and hold new hypothesis experiments until a working multi-endpoint Phrase app compiles, builds, passes its generated specs and runs; the growth study may double as the prototype; a ZOMBIES (Zero, One, Many, Boundaries, Interface, Exceptions, Simple scenarios) spec-coverage check in the compiler that WARNS (never errors) on obvious gaps, prompted by the card 106 survivor. Seats: Codex No (the path is nearly shortest but the in-memory store must precede or be a dependency of GET, GET must cover both collection and route-parameter forms, card 057 cannot proceed while it depends on the deferred card 055, and 055's hard error conflicts with warnings-only; the ZOMBIES card is ranked right if its boundary rules are syntax-aware and its checks distinguish below, at and above the boundary); Grok Support-it (order 051, 132, 131, 133, then the demo, real HTTP, the LLM check and the acceptance; 057 is wrongly on the path because the outcome rule is already in GRAMMAR.md; 134 right after the acceptance card and warnings must stay off the compile-failing diagnostic path). Result: passed after four rounds (round 1 Codex No and Grok Support-it; round 2 Codex Yes and Grok No on a circular dependency in card 135, card 055 left Ready and the wording of card 134; round 3 Codex Yes and Grok No on cards 026, 028, 030, 038, 056 and 058 still depending on superseded cards; round 4 Grok Yes and Codex No on one stale sentence in card 056, fixed afterwards in the card; every objection was applied; Copilot out of quota). Final order and decision below.
- Decision: replace the chain of cards 008 to 012 and 057 (which pulled in the base-library line and the deferred rule card) with a dependency-correct path ranked directly after card 103, in this order: 051 (the CLI build, existing), 132 (a persistence effect with a generated in-memory implementation), 131 (GET: a collection and one item by route parameter), 133 (PUT and DELETE), 135 (the Tasks demo app, replaces 009), 136 (real HTTP, replaces 010), 137 (the LLM-customer check, replaces 011), 138 (the Usable v1 acceptance test on main, replaces 012), then 134 (ZOMBIES spec-coverage warnings, the user-observable criterion: compiling orders.phrase reports the positive-quantity phrase is missing a One/Boundary success spec; warnings only; replaces the role of card 055). Cards 008, 009, 010, 011, 012 and 057 are superseded. Everything else (tests-first 107 to 109, cheaper models 115 to 118, codebase size 120 to 130, loop-runner, vocabulary and ADR cards, 052) drops below this path in its current order; in-flight work (102, 103, 130) finishes. The Tasks app is growth checkpoint one: card 127's backlog starts with the Tasks app's features. Further language gaps the demo exposes become thin cards just in time.

<a id="d98"></a>
#### Decider D98: review of card 135 (Tasks demo app: a CRUD API written entirely in Phrase)

- Date: 2026-10-28. Question: do the card's criteria hold as written; is it right to mark card 135 Done while telling product owner plainly that bar 1 is met in structure with the logic still native; is card 135 Done. Seats by role: reviewer (Claude, Sonnet, fresh context, which built and ran the app): Yes; reviewer (Grok): Yes (all three); the author (Codex) did not vote. Result: passed.
- Evidence: phrase build demo/tasks/Tasks.phrase exits 0; the generated API builds with 0 warnings; all 11 generated specs pass; 5 endpoints (Create, Get, List, Complete, Delete), each with a success and a failure spec (Create has EmptyTitle and TitleTooLong); no .cs file under demo/; five native csharp blocks; the generated store has four locked methods; two emitter fixes (a Bool default for DELETE spec seeding; qualifying the async Task type when a record is named Task), conditional and tested only through the Tasks specs; frozen files untouched; 243 compiler tests and 153 acceptance tests, zero warnings (the first full gate failed once on a stale CLI dll in a test helper, retro seed written; the rerun passed).
- Honest finding for product owner: the prototype is real and runs, but it shows the contract and spec machinery working, not Phrase expressing logic: every behaviour (validation, save, find, list, complete, delete) is in native C#, and ListTasks has an artificial available input only to have a failure spec. Bar 1 is met in structure, logic still native. Cards 145 (store operations as Phrase steps), 146 (validation as Phrase), 147 (a list endpoint without a fake flag) and 148 (composite steps and a record copy) are filed and ranked after card 138, before the ZOMBIES card 134. Other follow-ups: direct tests for the two emitter fixes; the Tasks.phrase header should also list the Save-seeding and endpoint-injection uses of the provisional convention.

<a id="d100"></a>
#### Decider D100: review of card 136 (Real HTTP behaves as the specs say)

- Date: 2026-10-28. Question: do the card's criteria hold on the commit starting 136: (authored by Grok, test-only); are the weaknesses acceptable as follow-ups; is card 136 Done. Seats by role: reviewer (Claude, Sonnet, fresh context, which ran the slice): Yes; reviewer (Codex): Yes (all three); the author (Grok) did not vote. Result: passed.
- Evidence: the 11 specs of demo/tasks/Tasks.phrase come from the compiled model (count asserted); each request is built from the spec's given input; state a spec assumes is prepared only through the app's own POST endpoint; responds specs are compared structurally with status 200, failure specs expect 422 with the code in the body, then succeeds expects 200 with an empty body and a following GET gives 422 NotFound; each endpoint group runs in a fresh app process whose recorded process id is stopped and asserted gone; a concurrency check posts 20 tasks in parallel and expects exactly 20 listed; 243 compiler tests and 155 acceptance tests, zero warnings (the one gate failure was a build-file lock between two tests that build the benchmark tool at once under load; it passes alone and is logged as a retro seed). Follow-ups for card 138 or a hardening card: parse the code field instead of a substring match, compare property-name casing exactly, do not pre-seed the create spec. The first run was green with no red because the generated app already matched; the finding is that card 135's app behaves over real HTTP as its specs say.

<a id="d102"></a>
#### Decider D102: CI on main was red for the whole project; keep and fix the workflow, add stop-the-line detection

- Date: 2026-10-28. Question: why did CI fail while the local gate passed, should the ci workflow be fixed or replaced, and how is a red main made impossible to miss. Decided by the facilitator under the product owner's stop-the-line instruction (no design dispute: the workflow is not obsolete, it is the only Linux check). Result: passed.
- Evidence: about 97 of the last 100 runs on main failed; two acceptance tests built Windows-only fake models (a cmd shim, a quoted prompt path); both now have a Linux branch (commit 46fdfef) and run 40714919880 passed both jobs. Detection: scripts/check-ci-main.py after every push and at the start of each loop iteration, red stops the line; rule "CI on main is part of the line" in the working agreement and the mob-defaults template; bug card 157; retro seed written.

<a id="d104"></a>
#### Decider D104: review of card 138 (Usable v1: the acceptance test passes on main and gates CI) and the milestone is reached

- Date: 2026-10-28. Question: do the card's criteria hold on commits b562cdb and 8c06a12 (authored by Codex); is card 138 Done; is Usable v1 reached. Seats by role: reviewer (Claude, Sonnet, fresh context): Yes with one fix (the report script could exit green with a missing or unreadable ratchet or with failing bars), applied in 8c06a12; the facilitator ran the full gate (243 compiler, 168 acceptance) and watched CI. Result: passed.
- Evidence: six Bar1_ to Bar6_ tests with the milestone trait and no Pending trait; all six bars are in board/usable-v1-ratchet.json and the report fails on an unreadable or incomplete ratchet and on any unmet bar; CI run 40732540373 on ubuntu-latest passed both jobs and the usable-v1 job printed 6 of 6 criteria met, USABLE v1 REACHED. Bar 6 rests on docs/usable-v1/llm-check-codex.json and llm-check-grok.json (Decider D103): it is self-reported evidence tied to the doc hashes, so a change to docs/GRAMMAR.md or docs/CHEATSHEET.md makes it stale and requires a rerun.

<a id="d106"></a>
#### Decider D106: review of card 158 (Tasks demo README: how to run the demo yourself)

- Date: 2026-10-28. Question: do the card's criteria hold on commits 541532c and f1a0284 (authored by Grok); is card 158 Done. Seats by role: reviewer (Claude, Sonnet, fresh context): Yes with fixes (the drift test did not tie the spec count, every declared failure name or the port to the source), applied in f1a0284 with mutation tests; the facilitator ran the full gate (243 compiler, 170 acceptance), ran the README steps by hand before the card was filed, and watched CI. Result: passed.
- Evidence: demo/tasks/README.md gives the prerequisites, the exact commands and the expected output for compile, build (0 warnings), specs (11 passed), start and the curl calls; tests/test_demo_readme.py and TasksDemoReadme.cs check every command, route, verb, field, failure name, the spec count and the port against demo/tasks/Tasks.phrase; scripts/usable-v1-report.py points to the README only when it exists. CI run on f1a0284 (and 541532c) on ubuntu-latest finished green.

<a id="d113"></a>
#### Decider D113: card 161 closes on its core outcome; the rest becomes five small cards

- Date: 2026-10-28. Question: card 161 is on its third author round after two independent reviews said No, and the finding list keeps growing; close it on the core outcome and split the rest, or let the long round finish. Options: S stop the long round, relaunch with a short goal for the core pair (a live test that all four demo failures and every endpoint resolve in the source map to the exact Phrase line, and a mutation proof that the no-knowledge assertions fail on a reintroduced leak), then close 161 and file small cards; N let the long round finish as one change; M let it finish, then close and file the remaining gaps. Seats by role (D109): delivery lead, Claude (Sonnet), fresh context: S (long goals get partly ignored, the committed code already holds the core outcome, small cards review and revert alone; risk: B or C slipping, so file them first and state the criteria are partly verified until they land); maintainer persona Morgan, Claude (Sonnet), fresh context: S (a mixed A-to-H diff cannot be reviewed accurately or reverted in part; the map is a build artifact and the missing pieces are test proof, not new exposure; risk: criterion 3 closed without its test, so rank that card first); architect, Codex, fresh context: No to S, M preferred (closing leaves the card's own criteria on exposure and trace handling unmet; risk: declaring success without proof the map is unserved or that trace fails safely). Grok is out of balance. Votes 2 to 1 for S. Result: passed.
- Decision: S. The long round was stopped and not imported (stopped by task id, no processes left). Card 161 is amended: it closes on the core outcome (the live test and the mutation proof, plus what is already committed in 1c22358). Its second and third acceptance criteria (the trace command's error handling and mismatch behaviour, and the map being unpublished and unserved) are marked partly verified until cards 167 (publish and 404 tests, ranked first) and 168 (trace error paths) land. Also filed and ranked right after: 170 (step ids recomputed independently and unique per endpoint), 169 (culture-independent order), 171 (CLI reads names from the adapter and tidy). Card 165 (the failure observer) now depends on 170, because it reuses the step ids. The architect's objection is recorded as the reason the amended card says plainly what is not yet proven.
- Coaching note for the retro: the author omitted tests its goal listed in two rounds. Coach, never bench: goals carry one required test each, the author reports the test names, and the facilitator checks the diff for them before the gate.
- Reversal: filing the cards and amending the card text are reversible; nothing generated changes in this decision.

