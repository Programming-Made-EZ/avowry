# Decider log

Durable copy of Decider runs. Per-card MOB.md files are gitignored, so results that outlive a card are kept here.
Format: `Decider D<n>: <proposal>`, votes, `Result: passed|failed`. All-Yes results may be a single line.

#### Decider D1: rank bug card 018 (spec resolution defects) at rank 4, after card 013 and before the emitter

- Date: 2026-10-25. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (votes collected in parallel, read-only).
- Votes: Codex Yes; Copilot Yes; agy Yes; Grok Support-it.
- Result: passed. Outliers: none.

#### Decider D2: rank the three retro-002 cards: 019 at rank 5 (after 018), 020 after 017, 021 after 015 (Decider first)

- Date: 2026-10-25. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Support-it; Grok Yes; Agy Yes.
- Result: passed. Outliers: none.

#### Decider D3: assess the board against VISION.md: insert 022, 023, 025 on the Usable v1 path; add milestone M4 Vocabulary v1 (024, 026, 027, 028, 029); Goodhart-guarded 'comprehensive'

- Date: 2026-10-25. Proposer: PM and domain expert (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Yes; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

#### Decider D4: adapters replace providers; base library plus adapters; C# built as the first adapter (reshape 003-007, 022, 025, 028); future cards 030 (synthesis, M5) and 031 (human converter, M6) after Usable v1 and Vocabulary v1

- Date: 2026-10-25. Proposer: PM, domain expert and architect (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Support-it; Grok Yes; agy Yes.
- Result: passed. Outliers: none. Open item closed 2026-10-25: Steve confirmed ruling 4 is a two-way converter between the LLM-optimized form and a human-friendly rendering.

#### Decider D5: declare card 016 Done on its written criteria but not M2; file bug card 032 (invalid baseline) at rank 3; run 032 as the first E1 true-mobbing card with a Codex/Grok/Copilot/agy task force

- Date: 2026-10-25. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Support-it; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

#### Decider D6: rank retro-016 cards: 033 (generics) and 034 (behavioral holes) after 023 and before 008; 035 (failure-code holes, Decider first) after 021; fold two criteria into 022; record raw candidates

- Date: 2026-10-25. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Yes; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

#### Decider D7: file the live mob dashboard as tooling card 036 (experiment E2) at 2nd in Ready after the baseline fix; code in mob-defaults; one dashboard for all mobs; build only what exposes nothing (event stream, stall detection, allow-list snapshot with test, LOCAL dashboard); choose and use no hosting until Steve gives a one-word OK on one specific option

- Date: 2026-10-25. Proposer: PM, architect (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Round 1 (public status-only repo with GitHub Pages recommended by the architect): Codex Yes; Grok Yes; agy Yes; Copilot ABSOLUTELY NO ("verified privacy-safe design, no code or personal exposure, a secured hosting model, and Steve's explicit approval first"). Result: failed.
- Amended by the proposer to address the outlier (hosting becomes a decision made WITH Steve before any exposure; the card builds the non-exposing parts and a local dashboard). Round 2: Codex Yes; Copilot Support-it; Grok Yes; agy Yes. Result: passed.

#### Decider D8: pause experiment E1 (true mobbing) on card 032 after 8 runs; finish 032 in the standard driver-plus-review mode with a whole-team sign-off before the final ship; keep E1 open for its retro after 032; the live mob dashboard (036) proceeds next in standard mode

- Date: 2026-10-26. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Evidence put to the vote: 127 turns, about 2.7M estimated tokens, about 2.4 hours, 2 increments shipped; the whole-team gate caught real defects; 5 of the stops were mobbing-tooling bugs, now fixed with tests.
- Votes: Codex Yes; Copilot Support-it; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

#### Decider D9 (failed) and D10 (passed): make the usable-v1 CI job pass while the milestone is pending and fail only on a regression

- Date: 2026-10-26. Proposer: PM (facilitator drafted), on Steve's direction via the user that a permanently red check trains everyone to ignore red; the details were left to a Decider. Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- D9 round 1 (job green with 'not reached: X of 6 criteria', fails only if a ratcheted bar regresses, per-bar tests): Codex Yes; Copilot Yes; agy Yes; Grok No (a bar must count as met, and enter the ratchet, only when its test checks what the written bar says, not a file-existence probe).
- D9 round 2 (amended: every bar tested for what it says; unwired bars count as not met): Codex Yes; Copilot Yes; agy Yes; Grok No (bar 2 must be tested against the real demo app so bars 3 to 5 verify one pipeline). Result: failed (an outlier remained after the amendment).
- D10 (narrow, adopting Grok's condition: bar 2 is met only when `phrase build demo/tasks/Tasks.phrase` produces the projects that bars 3 to 5 then build, test and call): Codex Yes; Copilot Yes; Grok Yes; agy Yes. Result: passed. Outliers: none.

#### Whole-team sign-off on card 032, second round (after the harness fixes and the re-run baseline, cfb1952)

- Date: 2026-10-26. Members: Codex, Copilot, Grok, agy, reading the stored rows and the commits against the card (read-only). The first round (earlier the same day) FLAGGED two real defects: agy's empty replies and Copilot's missing task row.
- Result: SIGN-OFF from all four seats.

#### Decider D11: declare card 032 Done as a zero-shot baseline repair; milestone M2 'Measured' is NOT declared reached (aligned with the bootstrap exit: reached when a Decider declares the baseline established); no metric-based keep/revert; file bug card 037 (docs in the prompt, and cold runs, plus the reply-head scrub defect) as 3rd in Ready

- Date: 2026-10-26. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only). An earlier D11 vote, taken on stale facts before the data fix, was voided.
- Round 1 (declare 032 Done AND M2 reached, with caveats): Codex Yes; Grok Yes; agy Yes; Copilot ABSOLUTELY NO (declaring M2 is premature while the bootstrap exit criteria, docs-in-context and cold runs, are unmet). Result: failed.
- Amended by the proposer to meet the outlier (032 Done as a repair only; M2 aligned with the bootstrap exit and not declared; 037 also carries the reply-head scrub defect). Round 2: Codex Yes; Copilot Support-it; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

#### Decider D12: experiment E1 (true mobbing) stays OPEN and opt-in under the bootstrap-phase rule: not the default practice, no TWA rule, tooling kept behind --true-mobbing for high-risk increments, numbers labeled 'early data, setup-heavy, not final', re-evaluated after the baseline is established; a final whole-team sign-off before a card's last ship is a recommendation

- Date: 2026-10-26. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Support-it; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

#### Decider D13: amend the hash-locked acceptance test of card 036 (CSP regex), and the facilitator takeover that fixed the page

- Date: 2026-10-26. Proposer: PM (facilitator). Seats: Codex, Copilot, Grok, agy (sandboxed runner, read-only).
- Proposal: the test's regex excluded both quote characters inside the captured Content-Security-Policy value, so `default-src 'self'` could never match for any valid policy. Amend only that regex to be quote-aware; keep every assertion after it; update the sha256 in the card (`e68b9352…`).
- Votes: Codex Yes; Copilot Yes; Grok Yes; agy Yes. Result: passed. Outliers: none.
- Consequence: the corrected test then caught two real page weaknesses against its stricter source list; the page dropped `data:` from img-src and `object-src 'none'` (default-src 'self' still applies). TAKEOVER (facilitator, small fix after the 5-cycle cap and time-box; logged per the tooling authorship rule).
- Result of the card: Done-when green: 15 of 15 acceptance tests, hashes intact, whole mob-defaults suite 120 OK (1 skipped).

#### Decider D14: board order after Steve withdrew his re-rank (round 1: restored order as is)

- Date: 2026-10-26. Proposer: PM (facilitator). Seats: Codex, Copilot, Grok, agy. Votes: Codex Yes; Copilot Yes; Grok Yes; agy No (0 of 6 Usable v1 bars met; put the C# adapter chain before the benchmark-docs card). Result: failed; raised again amended (D17).

#### Decider D15: tooling standard (hardened smoke.py as a pre-use check; non-author review of tooling; rotation authorship with logged takeovers)

- Date: 2026-10-26. Optional proposal from Dispatch; Steve left it to the team. Votes: Codex Yes; Copilot Yes; Grok Yes; agy Yes. Result: passed. Outliers: none. The real-CLI smoke run passed (all four seats, toy card, Decider runner).

#### Decider D16: Steve's "good starter app" input: the mob status API in Phrase

- Date: 2026-10-26. Lenses: PM, language designer, architect. Votes: Codex Yes (option B); Copilot Support-it (Tasks fits bar 1; the dashboard is better as a follow-on dogfood); Grok Yes (B, first slice of records, in-memory); agy Yes. Result: passed. Option B: keep the Tasks API (009) as the Usable v1 bar-1 app; new card 038 (dogfood status API) after the Usable v1 gate. Steve's later input (the 30-minute digests cover his visibility need) lowers the urgency of dashboard follow-ups (live wiring, Meshnet bind): they stay seeds, no card.

#### Decider D17: board order, rounds 2 and 3

- Round 2 (018, 003 first; 038 after 012): Codex Yes; Copilot Yes; agy Yes; Grok No (033 and 034 must stay before 025, per D6). Round 3 (amended): Codex, Copilot, Grok, agy all Yes. Result: passed. New order: 018 next up, then 003, 037, 022, 013, 019, 014, 004, 005, 006, the benchmark-dependent cards, 007, 023, 033, 034, 025, 008, 009, 010, 011, 012, then 038. Card 003 still starts with the stash Decider (salvage or drop).

#### Decider D18: process retro picks (thin slices trialled on the C# adapter chain; keep the listed seeds)

- Date: 2026-10-26. Proposer: PM (facilitator). Seats: Codex, Copilot, Grok, agy. Votes: Codex Yes; Grok Yes; agy Yes; Copilot Support-it (pre-split cards 003 to 007 now, with locked Done checks before any coding; adopted). Result: passed. Metric: runs per card and cap hits; counter-metric: per-card ceremony overhead; review after the first three thin cards.
- D19 (follow-up, no ruling needed): the seats gave retro feedback and proposed thin slice lists; the facilitator synthesized them into cards 039 to 052. Codex and Copilot Yes; Grok's reply parsed as No by the runner (a parse trap: its reply began "No files will be changed." before "Yes"), agy no response; neither is an objection to the slicing.

#### Decider D20: Steve's consumer-observable-outcomes direction, refined into cards 054 to 058

- Date: 2026-10-26. Lenses: PM, language designer, QA. Seats: Codex Support-it; Copilot Support-it; Grok Yes (its reply began "No files will be changed." and the runner parsed that as No; fixed in mob-defaults, a logged takeover); agy no response. The runner's integrity check also reported "HEAD moved" because the 018 mob was committing in the same repo; noise, not a seat write (seats wrote nothing). Result: passed with changes, the cards below.
- Agreed: bare `then succeeds` stays legal where there is nothing to return and no output; a missing failure-path spec is an ERROR (PH336), not a warning, because the compiler has no warning channel (Grok); `and succeeds` stays PH335; an endpoint with an output cannot say `succeeds` (PH333); validation phrases should return the validated or normalized value (Codex, Copilot), established via the keyword-model decision (card 022) and not applied to every `ensure` silently.
- Audit (b), visible tree: the only outcome-less spec in examples, fixtures and benchmark tasks is bench task 005 (also declares MissingName with no failure spec); the parser fixtures keep `then succeeds` as syntax coverage; card 014's wording is fixed; orders.phrase and reference/csharp have none; demo/tasks is not in the repo yet. Benchmark change: card 054, with its own Decider, suite version bump, holdouts untouched, zero-shot rows kept and labeled.
- Cards: 054 benchmark task 005 gets a failure spec; 055 PH336; 056 endpoint-with-output cannot say succeeds; 057 docs; 058 validation phrases return their value. Ranked after 037 and before 022 (054 to 057) and right after 022 (058); 054 must precede 055.

#### Decider D21: Steve's async and events input, refined into cards 059 to 061

- Date: 2026-10-26. Lenses: language designer, architect, PM. Seats: Codex Support-it; Copilot Support-it; Grok Yes; agy no response (agy has now not answered D19 to D22: a seat-health item for the next probe). Integrity note: "HEAD moved" was the 018 mob committing in the same repo, not a seat write. Result: passed with changes (no objections).
- Agreed: async and events stay OUT of the core grammar; they are declared effects (Scheduler, Clock, Cancellation, Events) plus base-library keywords, async being an adapter execution property; the spec model is a virtual clock, a scripted scheduler and event capture (no wall time, no OS interleaving); the main risk is standardizing C# scheduling semantics (card 028 is the check). Usable v1 needs none of it: the Tasks demo (009) no; the status API (038) first slice no. LLMs-as-customers: no evidence yet for parallelism or events (retry is the repeated candidate), so the evidence is made first; bootstrap phase: kept as a hypothesis.
- Cards: 059 ADR (after 034, before 025); 060 catalogue tasks (right after 027; suite change, versioned, own Decider); 061 spike (after 025, before 028). Keyword cards stay seeds until the ADR, spike and evidence exist.

#### Decider D22: atomic runs (land or discard) and Bishop's Law applied, Steve's retro input

- Date: 2026-10-26. Seats: Codex Support-it; Copilot Support-it; Grok Support-it; agy no response. "HEAD moved" was my own commits during the vote. Result: passed with changes (no objections).
- Adopted: Steve's reading of Bishop's Law (atomistic, dependency-free work is the parallel class; dependent or shared-state work is the concurrent class and stays WIP-of-one) is written into the agreement; every thin card now has a `Depends on:` line; land-or-discard is an EXPERIMENT (E3), not doctrine, trialled first on the dependent chain at WIP-of-one for clean retries, with independent cards (053, 054) as the first parallel candidates after the tooling exists; worktree removal through `git worktree remove` plus `safe_delete` only for a strict child of a scratch root; stashes untouched; restore and tests run as a harness job; merges serialized as fast-forwards; a conflict reclassifies the pair as concurrent. Card 062 builds the tool (authored by the rotation, non-author review, smoke run). Review after the first three thin cards that use it.

#### Decider D23: Steve's "it should be a loop": the loop runner in mob-defaults, cards 063 to 070

- Date: 2026-10-26. Lenses: architect, safety, PM. Seats: Codex Yes; Grok Yes; Copilot Support-it with changes (its reply was corrupted mid-stream by a CLI glitch but ended Support-it, and the runner read "No" from the garbled text; the votes below are read from the replies); agy no response. Result: passed with changes.
- Agreed design: `loop.py` in mob-defaults is a thin supervisor over card mode; state in the gitignored `.mob/loop/` (atomic state.json, heartbeat.json, one OS file lock); the picker reads the ranked board table, skips Done and SUPERSEDED, honours `Depends on:` and resumes an in-flight card first; toolchain (.NET 8) pinned in a checked-in file and verified before dispatch; on Done run the retro step and the re-rank hook once; on cap or failure one retry, then park with a split proposal and move to the next independent card; exits 2 and 3 are parked, exit 4 gets one Decider then one retry, exit 5 and Safety flags halt; escalate only for product direction, an unresolved Decider conflict or the safety carve-out, through a Needs-Steve file in the published MOB.md and a one-word reply file; spend ceilings per card and per day; STOP file. Registering a Windows logon task changes system configuration: Steve's one-word ask, written but not run by the tool.
- Cards (all tooling, thin, rotation authorship with non-author review and the smoke run): 063 picker, 064 lock/state/heartbeat, 065 pinned toolchain run, 066 failure policy, 067 Done path, 068 liveness/STOP/escalation, 069 resume after reboot, 070 atomic runs. Ranked: 063 to 067 right after 018 (the engine for everything after), 068 to 070 after the first C# slices (039, 040) so product moves too.

#### Decider D24: amendment to D23 (Steve's correction: the loop never ends; discovery instead of stop; allotment pacing)

- Date: 2026-10-26. Steve: "don't forget the team can find their own features to add. so theoretically the loop is continuous and never ends." Facilitator/PM amendment folded into the cards on his direction (his correction to his own product; no design dispute); any seat may challenge it, which would trigger a Decider. The loop stops ONLY for a STOP file or Steve saying stop, a one-way door that needs Steve's OK, or all driver allotments out (it pauses). When Ready is low or empty it runs a discovery step (the product-category retro with LLM-customer feedback from every model, benchmark gaps, pattern mining from VISION's direction, seeds); the PM and domain expert refine the output into cards through a sandboxed Decider, rank them, and the loop continues.
- Cards: 067 amended (empty Ready starts discovery, never a stop); 068 amended (the only stops); new 071 (discovery step) and 072 (pace against the finite allotments, a counter-metric: cards landed per allotment). Ranks: 072 right after 068, 071 after 070.
- Result: passed (recorded as a facilitator/PM call under the lean Decider rule; Steve's own direction on his product).

- **Amendment (same day, Steve's simplification):** "during every retro we have the product category where new features should be identified." No separate discovery step: card 071 is WITHDRAWN and folded into 067 (the retro's product category produces new cards through a passed Decider; an empty Ready list is a signal that the category is under-producing, raised at the next retro, never a special mode or a stop). Card 072 (allotment pacing) stands. The loop stops only for a STOP file or Steve saying stop, a one-way door that needs Steve, or all allotments out (it pauses).

#### Decider D25: Steve's board move: Phrase's board becomes a GitHub Projects board (cards 073 to 077)

- Date: 2026-10-26. Seats: Codex Yes; Copilot Yes; Grok Yes; agy no response. Result: passed. Outlier on one point only, project visibility: Codex and Copilot recommended a public project; Grok recommended PRIVATE (a public project is a live operational feed and needs Steve's one-word OK; Steve sees a private project logged in on his phone; the issue text is already public in the repo). Decided PRIVATE under the safety rules (privacy exposure is a safety-carve-out class; the cheaper, reversible default wins); the scripts have no public setting; if Steve wants it public it is his one-word ask.
- Agreed design: Project v2 `Phrase Board` under Programming-Made-EZ with fields Rank, Card number, Depends on, Type, Milestone, Author and Status (Ready, In Progress, Done, Superseded/Withdrawn); issue per card titled `(NNN) <title>`, body the card file after its title with a `<!-- phrase-card:NNN -->` marker, nothing from `.mob/`; tooling cards stay issues in phrase-lang; the project is canonical after a verified cutover and `board/*.md` plus the rank table are generated, read-only; the loop picker (card 063's logic) reads the project through `gh api graphql`; the re-rank hook writes Rank after a passed Decider and skips cards whose Rank is newer than its snapshot; migration never deletes, never touches other repos or mobs, default dry run, idempotent by marker; rollback is a git revert (issues stay).
- Cards: 073 create the private project and fields; 074 migrate every card file into one issue; 075 put each issue on the project with its fields; 076 the picker reads the GitHub board (with a real-board check); 077 cutover to generated archive. Ranked immediately after 063 (in flight); 064 now depends on 076; 065 to 072 keep their order behind it.

- **D25 follow-up (2026-10-26):** the real apply found that GitHub reserves the field names Type, Milestone and Author on every project, so the fields are named Kind, Phase and Card author (card 073 and the test now guard this). The private project is live: https://github.com/orgs/Programming-Made-EZ/projects/1. The non-author review of the 063 picker (Decider D26: Codex, Copilot, Grok No; agy no response) found it parses a synthetic board and expects a pid the facilitator never writes; card 063 is SUPERSEDED by 076 (the picker reads the GitHub board) with the defects carried over as criteria.

#### Decider D27: Steve's story standard: a persona, not a role (PERSONAS.md, template, rewrites)

- Date: 2026-10-26. Lenses: PM, language designer, UX. Seats: Codex Yes; Grok Yes; Copilot Support-it; agy no response. Result: passed with changes. (The first attempt returned no response from all four seats because the proposal text contained double quotes, which the Windows CLI launchers mangle; the vote was re-run with the quotes removed. That is a Decider-runner defect, seeded for a fix.)
- Agreed: a small persona set in `PERSONAS.md` linked from VISION.md: Ari (cold-start LLM coding agent), Dana (developer directing the agent), Morgan (maintainer and reviewer), Riley (the mob as dogfood user), plus Steve (sponsor on a phone) for board and tooling stories. A story is `As <persona>, I want <observable capability>, so that <value that persona gets>`; the Ready check REJECTS a role-only story or a design-goal benefit. Definition of Ready, the mob-defaults template and the PM charter updated; `scripts/check-story-persona.py` checks the persona and the so-that clause mechanically. Done cards keep their stories; the 59 open cards are rewritten (Codex authors, the facilitator reviews) and their issue bodies synced by card 079 before the cutover (077) so there is one source at a time. Cards 078 (Ready lint in the facilitator) and 079 (sync bodies).

#### Decider D28: Steve's two card shapes, Story and Contract (proposal A)

- Date: 2026-10-26. Seats: Codex Support-it; Copilot Support-it; Grok Support-it; agy no response. Result: passed with changes (no objections). Adopted: every card is a STORY (a persona from PERSONAS.md, a so-that that persona values, criteria the persona observes) or a CONTRACT (Shape: Contract; Consumer is a COMPONENT, never a person; Given, Observes, Fails with, Invariants, Enables). A card is a Contract only when no persona can observe the outcome; persona-observable compiler work stays a Story. No orphans: every Contract enables at least one existing Story and is cut from a failing persona-level test (outside-in TDD, consumer-driven contracts); a Contract ranks behind the Story it enables unless that is Done. The Ready check accepts either shape and rejects role-only stories, design-goal benefits, a person as consumer, bare-success observations and orphan contracts. Written into the Definition of Ready (this agreement and the mob-defaults template), the PM charter, and `scripts/check-story-persona.py` (with tests). Re-cut: only 040 (the adapter seam) is a Contract (Consumer PhraseCompiler, enables 039); the rest of the thin C# chain stays Stories; loop and board cards are Stories for Steve or Riley. Board fields Shape and Enables: card 080.

#### Decider D29: Steve and Dispatch's named actor types in specs (proposal B)

- Date: 2026-10-26. Seats: Codex Support-it (name: observer); Copilot Yes (name: party); Grok Support-it (name: party); agy no response. Result: passed with changes. Name (as voted): PARTY (two of three). OVERRULED by Steve the same day (see the ruling below): the concept is ACTOR. Catalogue: Codex and Copilot kept all seven of Steve's candidates (caller, client, store, event bus, clock, scheduler, external service); Grok argued for a closed core of four (caller, client, store, events), clock and scheduler staying declared effects and spec givens (their outcomes are a caller result, a failure code or an events observation) and external service admitted only by an adapter and a Decider. The ADR (082) decides, informed by evidence, and records the dissent. Agreed: not a keyword and not a feature yet (bootstrap phase): measure under today's grammar first (081), write the ADR (082), spike the doubles (083); returns, responds and fails with stay; succeeds stays legal only for the D20 remainder (no return type, no endpoint output, effects none); a later step may replace returns and responds with party sentences in one step (no alias), decided after the evidence.
- Cards and ranks: 080 board Shape/Enables fields (right after 079); 081 measure (right after 060); 082 ADR (right after 059); 083 spike (right after 061).

- **Steve's ruling on the name (2026-10-26):** "I don't like the name 'party'. I said actor because it matches use case concepts." The concept is called ACTOR, in the UML and use-case sense: anyone or anything outside the system that interacts with it, covering human personas and system actors (an adapter, the compiler, another service). Renamed everywhere (cards 081 to 083, the ADR file, the Contract template: its Consumer label is now Actor, a system actor; PERSONAS.md; this record). The catalogue and design as the team decided stand. The concurrency actor model is a different concept; the ADR (082) says so explicitly and keeps the word for the use-case meaning. Where this record above says party or parties, read actor and actors.

#### Decider D30: the name of the spec concept (Steve: not 'party', too unofficial sounding; 'actor' is his suggestion, not a mandate)

- Date: 2026-10-26. Constraints: not party; formal or official sounding; covers human personas and system actors; reads in a spec sentence; avoids collisions (concurrency actor model, role, agent, observer, persona, consumer). Seats: Codex Yes; Copilot Yes; Grok Yes; agy no response. Result: passed.
- Chosen: ACTOR (all three ranked actor first, participant second, counterpart third). Why: it is the established formal use-case (UML) term for anyone or anything outside the system that interacts with it, so one word covers a human persona (Ari, Dana, Morgan, Riley, Steve) and a system actor (the compiler, an adapter, the caller, the client, the store); it reads in a spec ('then the client is answered with 201'); participant sounds like a workshop, counterpart fails for a store or a clock. The concurrency actor model is not a term in use here (card 059 names Scheduler, Clock, Cancellation and Events) and the ADR (082) keeps the two apart in one sentence. Nothing to rename: the rename to actor was already applied (5c6786c); one clause in card 082 that kept the earlier vote's history was dropped.

#### Decider D32: the pre-registered bar for the Phrase-versus-plain-C# head-to-head (recorded BEFORE any result row exists)

- Date: 2026-10-26, before any card in the measurement chain is started. Seats: Codex Support-it; Copilot Support-it; Grok Support-it; agy no response. Result: passed with changes (no objections; the changes are amendments from the seats, adopted below). Any seat may challenge it before the first scored row; after the first row a change voids the stored rows.
- THE BAR (frozen): N = 5 scheduled runs per model per task (harness-error rows stay in the count; Codex and Copilot asked for 5, Grok for scheduled-runs wording); attempt cap 3 (a run that never passes counts as 3 attempts and a fail); every run is scored and a failed run is charged the tokens it consumed through attempt 3 (Codex); means are task-macro averages (Codex); pass rate is the unweighted mean of per-model rates (Grok). Pass rate: Phrase within 5 points of plain C# or better is WIN-eligible (Codex: 5, not 10), 5 to 10 points worse is a TIE, worse than 10 points is a LOSS (Grok's thresholds), and a read-task gap worse than 10 points blocks WIN. Tokens: Phrase mean total tokens, documentation and cheat-sheet tokens included and charged on every attempt, at most 75 percent of plain C# on tasks passing in both arms, on write tasks AND separately on change tasks. Attempts: Phrase mean at most the C# mean on each of those groups. Patch rate 0 (Grok): a run that goes green only by editing the generated C# counts as a Phrase fail and any patch share above 0 blocks WIN. INCONCLUSIVE if fewer than 3 models, fewer than 3 paired task-runs in a group, or more than 10 percent harness-error rows. A write-only saving is a TIE. Holdout: 3 tasks written outside the mob (one write, one change, one that fits no pattern; Steve asked for 2 or 3, Codex wanted 6: 3 now, more may be added by a new Decider before the first scored run), hashes recorded before the first scored run, judged on the same bar; an INCONCLUSIVE holdout makes the whole call INCONCLUSIVE. The result is exactly one of WIN, LOSS, TIE, INCONCLUSIVE, with absolute pass rates beside it. Frozen with the bar: model versions, prompts, context limits, task hashes, timeouts, token accounting, retry policy. Little-room gate: if every included model is at 90 percent pass rate or more and 2 mean attempts or fewer on plain C#, and change-task tokens are at most 2 times write-task tokens, the emitter chain pauses for one Decider.
- Goodhart guard: task list, suite hash, holdout hashes and prompts frozen before any run; no post-hoc task edits; a later change to any number is a new Decider and voids rows already stored.

#### Decider D31: Steve's priority ruling: measure first (cards 084 to 089), pause the rest behind the head-to-head result

- Date: 2026-10-26. Seats: Codex Support-it (its reply partly failed to read files in the sandbox; its card list matched the plan); Copilot Support-it; Grok Support-it (the fullest plan, adopted as the base); agy no response. The integrity note (HEAD moved) was the facilitator committing during the vote. Result: passed with changes (no objections).
- Cards (the next work after 064, in this order): 084 reference target builds and every orders spec passes (Story, Morgan; README made truthful first); 086 plain C# baseline on the same tasks (Story, Riley; run first among the measurements; holds the pre-registered bar); 085 a missing Phrase reply is a model failure (Story, Riley; the Grok no_phrase_found rows are a harness or prompt problem, not a language result); 084, 085 and 086 are dependency-free and may run as parallel atomic tasks. Then the existing thin chain 039, 040, 041, 042 (List only), 043, 044, 045, 048, 049, 050 as the increments; 087 orders.phrase compiles and its generated specs pass (Story, Dana; sits on top of the chain); 037 docs in the prompt and cold runs (after 087, before 088); 088 head-to-head Phrase vs plain C# (Story, Steve; one page, WIN, LOSS, TIE or INCONCLUSIVE). New 089 option and nested-record fields (moved out of 042, behind the head-to-head). Shape note: Steve proposed card 2 as a Contract; by the D28 rule a card is a Contract only when no persona can observe the outcome, and Riley observes benchmark rows, so 085 is a Story.
- PAUSED behind 088 (kept on the board, ranked after it; Steve's lean, adopted): vocabulary and grammar growth (013, 014, 015, 019 to 027, 033 to 035, 054 to 061, 081 to 083, 089), the other adapters, synthesis and the converter (028 to 031), full-suite spec scoring (017), the demo and dogfood cards (008 to 012, 038), docs and CLI slices (046, 047, 051, 052), and the further loop cards (062, 065 to 070, 072, 078). Retros stay short on this path (flow numbers and whether the slice held to about three cycles). Fits the agile ruling: small working increments, a bar fixed once, the language specified no further than the next slice.
- Holdouts (needed before 086 starts): a person who will not drive or review 086 to 088 writes the three tasks and gives only their hashes. Asked of Steve in the report.

#### Decider D36: the sealed holdout hashes for the Phrase-versus-plain-C# head-to-head (card 093, holdout-hashes)

- Date: 2026-10-27, recorded BEFORE any scored run and before card 086 starts (the pre-registration of Decider D32 required it). Recorded by the facilitator/PM as a data record under the lean Decider rule, on Dispatch's report; any seat may challenge it before the first scored row.
- The three hashes (SHA-256 of the whole sealed file, exact bytes, lower-case hex), also in `bench/holdout-hashes.txt`:
  - H1 write 3bf0847e85a82956d8f78d3bde38c231da6533b1e2f95c15f66a750057d42ec1
  - H2 modify 9d4b1e8ca2cb7dcd1780bc6a4acaee2571b2f12830811d9a1ec4f3f585c17fc5
  - H3 write e29a752e6353aa94ce49c01c551eb4285bacdf86d7428b1c63469332110faaaf
- Provenance, as reported by Dispatch: a separate Cowork session outside the mob wrote all three; it will not drive or review cards 086 to 088; the sealed files are held by Dispatch and Steve, off the repository; they are not committed and no path to them is recorded here. The mob does not read their content before card 088 unseals them. At unsealing each file's SHA-256 must equal the hash above; a mismatch voids that holdout.
- Result: passed (recorded).
