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
