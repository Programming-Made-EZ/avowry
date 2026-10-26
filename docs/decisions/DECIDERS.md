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
