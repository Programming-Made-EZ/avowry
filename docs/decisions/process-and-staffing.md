# Process And Staffing decisions

Team practices, experiments, staffing, review, and decision-making rules.
Product delivery and benchmark rules belong in their own area files.

## In force

- [D12](#d12) — Experiment E1 (true mobbing) stays open and opt-in behind a flag, not the default practice; its numbers are labelled early data. Follows D8
- [D15](#d15) — Tooling standard: a hardened smoke check before first use, non-author review of tooling, rotation authorship with logged takeovers
- [D18](#d18) — Process retro picks: thin slices with locked Done checks before coding, trialled on the C# adapter chain; metric is runs per card and cap hits
- [D22](#d22) — Atomic land-or-discard runs are an experiment (E3); dependency-free work may run in parallel, dependent or shared-state work stays at WIP of one
- [D27](#d27) — Stories name a persona, not a role, with an observable capability and a so-that value
- [D88](#d88) — Standing driver rule: the loop re-probes unavailable models and the driver status table is kept visible
- [R4](#r4) — Staffing is by role first and model second
- [D101](#d101) — Docs state principles and rationale, not who said them; no personal names, no verbatim quotes attributed to a role, no gendered pronouns about the sponsor
- [D109](#d109) — Decider seats are by role, not by model

Related decisions: [D5](board-and-delivery.md#d5), [D7](board-and-delivery.md#d7), [D23](board-and-delivery.md#d23), [D24](board-and-delivery.md#d24), [D28](board-and-delivery.md#d28), [D113](board-and-delivery.md#d113)

## Records

<a id="d8"></a>
#### Decider D8: pause experiment E1 (true mobbing) on card 032 after 8 runs; finish 032 in the standard driver-plus-review mode with a whole-team sign-off before the final ship; keep E1 open for its retro after 032; the live mob dashboard (036) proceeds next in standard mode

- Date: 2026-10-26. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Evidence put to the vote: 127 turns, about 2.7M estimated tokens, about 2.4 hours, 2 increments shipped; the whole-team gate caught real defects; 5 of the stops were mobbing-tooling bugs, now fixed with tests.
- Votes: Codex Yes; Copilot Support-it; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

<a id="d12"></a>
#### Decider D12: experiment E1 (true mobbing) stays OPEN and opt-in under the bootstrap-phase rule: not the default practice, no TWA rule, tooling kept behind --true-mobbing for high-risk increments, numbers labeled 'early data, setup-heavy, not final', re-evaluated after the baseline is established; a final whole-team sign-off before a card's last ship is a recommendation

- Date: 2026-10-26. Proposer: PM (facilitator drafted). Seats: Codex, Copilot, Grok, agy (parallel, read-only).
- Votes: Codex Yes; Copilot Support-it; Grok Yes; agy Yes.
- Result: passed. Outliers: none.

<a id="d15"></a>
#### Decider D15: tooling standard (hardened smoke.py as a pre-use check; non-author review of tooling; rotation authorship with logged takeovers)

- Date: 2026-10-26. Optional proposal from Dispatch; product owner left it to the team. Votes: Codex Yes; Copilot Yes; Grok Yes; agy Yes. Result: passed. Outliers: none. The real-CLI smoke run passed (all four seats, toy card, Decider runner).

<a id="d18"></a>
#### Decider D18: process retro picks (thin slices trialled on the C# adapter chain; keep the listed seeds)

- Date: 2026-10-26. Proposer: PM (facilitator). Seats: Codex, Copilot, Grok, agy. Votes: Codex Yes; Grok Yes; agy Yes; Copilot Support-it (pre-split cards 003 to 007 now, with locked Done checks before any coding; adopted). Result: passed. Metric: runs per card and cap hits; counter-metric: per-card ceremony overhead; review after the first three thin cards.
- D19 (follow-up, no ruling needed): the seats gave retro feedback and proposed thin slice lists; the facilitator synthesized them into cards 039 to 052. Codex and Copilot Yes; Grok's reply parsed as No by the runner (a parse trap: its reply began "No files will be changed." before "Yes"), agy no response; neither is an objection to the slicing.

<a id="d22"></a>
#### Decider D22: atomic runs (land or discard) and Bishop's Law applied, the product owner's retro input

- Date: 2026-10-26. Seats: Codex Support-it; Copilot Support-it; Grok Support-it; agy no response. "HEAD moved" was my own commits during the vote. Result: passed with changes (no objections).
- Adopted: the product owner's reading of Bishop's Law (atomistic, dependency-free work is the parallel class; dependent or shared-state work is the concurrent class and stays WIP-of-one) is written into the agreement; every thin card now has a `Depends on:` line; land-or-discard is an EXPERIMENT (E3), not doctrine, trialled first on the dependent chain at WIP-of-one for clean retries, with independent cards (053, 054) as the first parallel candidates after the tooling exists; worktree removal through `git worktree remove` plus `safe_delete` only for a strict child of a scratch root; stashes untouched; restore and tests run as a harness job; merges serialized as fast-forwards; a conflict reclassifies the pair as concurrent. Card 062 builds the tool (authored by the rotation, non-author review, smoke run). Review after the first three thin cards that use it.

<a id="d27"></a>
#### Decider D27: the product owner's story standard: a persona, not a role (PERSONAS.md, template, rewrites)

- Date: 2026-10-26. Lenses: PM, language designer, UX. Seats: Codex Yes; Grok Yes; Copilot Support-it; agy no response. Result: passed with changes. (The first attempt returned no response from all four seats because the proposal text contained double quotes, which the Windows CLI launchers mangle; the vote was re-run with the quotes removed. That is a Decider-runner defect, seeded for a fix.)
- Agreed: a small persona set in `PERSONAS.md` linked from VISION.md: Ari (cold-start LLM coding agent), Dana (developer directing the agent), Morgan (maintainer and reviewer), Riley (the mob as dogfood user), plus product owner (sponsor on a phone) for board and tooling stories. A story is `As <persona>, I want <observable capability>, so that <value that persona gets>`; the Ready check REJECTS a role-only story or a design-goal benefit. Definition of Ready, the mob-defaults template and the PM charter updated; `scripts/check-story-persona.py` checks the persona and the so-that clause mechanically. Done cards keep their stories; the 59 open cards are rewritten (Codex authors, the facilitator reviews) and their issue bodies synced by card 079 before the cutover (077) so there is one source at a time. Cards 078 (Ready lint in the facilitator) and 079 (sync bodies).

<a id="d88"></a>
#### Decider D88: driver re-probing as a standing rule, the Copilot verdict timing, and the cheat-sheet-plus-index hypothesis

- Date: 2026-10-28. Inputs from product owner via Dispatch: query Copilot's quota and reset time; hold the verdict page until the reset unless it is more than about 48 hours away (then publish with Copilot unavailable and rerun later); a tracked re-probe; as a standing rule the loop always tests for unavailable models to be available again, built into the loop and the default template; and a hypothesis, not a directive: the core grammar and base library as a compact cheat sheet always in the prompt, extensions and adapters as an index of phrase signatures with full definitions pulled on demand, claim that prompt tokens stay roughly flat as the library grows, lookup tokens and round trips charged, pre-registered, not ahead of the prototype path.
- Facts: the GitHub account endpoint (gh api copilot_internal/user) reports Copilot premium interactions entitlement 1500, used 1504, has_quota false, quota_reset_date_utc 2026-11-01T00:00:00Z, about 88 hours from the probe (more than the 48 hour threshold); the CLI prints only You have no quota with a request id (no reset time) and its usage view exists only in the interactive UI; agy is out too: its CLI error text says the individual quota resets in 108h50m from the probe at about 08:50 UTC on 2026-10-28, which is about 2026-11-01 21:40 UTC.
- Seats: round 1 Codex No (card 139 belongs with mob-defaults process work and probes need bounded timeouts, failure logging and fake-probe tests) and Grok Support-it; round 2 both No (the files were not yet committed when they read the snapshot; Grok: card 139 must not sit inside the Usable v1 path fixed by D86 and card 140 must state it does not amend D32). Both points were applied: files committed, card 139 ranked with the other loop cards below the ZOMBIES card 134, card 140 states that it is a separate question that does not amend D32 or rescore stored rows. Round 3: Codex No (agy was not being re-probed), Grok no vote; applied: agy probed, DRIVERS.md updated, a tracked 60 minute agy re-probe started. Round 4: Codex No (this record stale and incomplete) and Grok No (the agy reset arithmetic was eight hours late, no flag file named, the record still said about 1 November); applied in this record and in DRIVERS.md. Round 5: Codex No and Grok No (decision (a) named only Copilot's reset, agy-back.flag unnamed, no final votes recorded); applied here. Result: see the final line.
- Decision: (a) the holdout verdict is published INCONCLUSIVE by the frozen bar (fewer than 3 models, Copilot's holdout rows unavailable) naming BOTH unavailable drivers and their resets (Copilot 2026-11-01 00:00 UTC from the account endpoint; agy about 2026-11-01 21:40 UTC from its error text), and is regenerated from the same frozen numbers after the unavailable rows are rerun: Copilot's holdout rows are rerun when it returns; agy ran no holdout rows (it was never an included model of this run), so its return restores its seats and adds rows only if product owner asks; a probe that succeeds before publication runs Copilot's holdout rows first; Codex and Grok rows stay valid. A tracked 30 minute re-probe of the account endpoint runs now (no quota used) and writes copilot-back.flag when quota returns; a tracked 60 minute agy re-probe writes agy-back.flag when it answers (both in the facilitator scratch folder, named in DRIVERS.md); the flag completions wake the facilitator. (b) The Driver availability rule is in TEAM_WORKING_AGREEMENT.md and the mob-defaults default template; DRIVERS.md is the visible status table and event log (MOB.md is generated); templates/DRIVERS.default.md is added to mob-defaults. (c) Card 139: the loop step with fake-probe tests, ranked after card 134 with the loop cards. (d) Card 140: the hybrid prompt measurement, pre-registered, ranked after card 139.
- Final: round 6: Codex Yes, Grok Yes (the record, both resets, the ranks 051 at 47, 134 at 55, 139 at 56 and 140 at 57 verified from the committed files). Result: passed.

<a id="r4"></a>
Record R4 (the product owner's standing rule, 2026-10-28, folded in as instructed; not a vote): staffing is by ROLE first and by model second (Staffing: roles over models in TEAM_WORKING_AGREEMENT.md and the mob-defaults template). Roles are the primary perspectives; when a model is out of quota its roles move to available models; Decider votes and reviews are seated by role, so an outage does not make a vote smaller; one model may fill several roles only in separate fresh contexts; Claude (Sonnet by default) is a normal member of the rotation and takes seats while Copilot is out, but is the sole driver only when every other driver is out; model diversity is preferred but never blocks. The headless claude CLI is not logged in (checked 2026-10-28: Not logged in), so Claude is seated as a fresh-context subagent given the role charter and the proposal. From now on this repository's reviews seat the role of the missing Copilot reviewer with Claude.

<a id="d101"></a>
#### Decider D101: review of card 154 (docs state principles and rationale, not who said them) and the docs/GRAMMAR.md re-freeze

- Date: 2026-10-28. Question: do the card's criteria hold on commits be71a65, 6308cac, 7bd68c2 and 3e2758a plus the card-issue rewrites; is the one-line change to docs/GRAMMAR.md acceptable under the D72 freeze; is card 154 Done. Seats by role: reviewer (Claude, Sonnet, fresh context): Yes with fixes (verbatim quotes attributed to the role and gendered pronouns remained); reviewer (Codex): Yes; reviewer (Grok): no vote in the reply (not counted). The fixes were made in round 3 (authored by Codex) and the checks extended. Result: passed.
- Evidence: scripts/check-no-personal-names.py passes on the tracked files and now also fails on the role followed by a colon and a quotation, and on gendered pronouns on lines about the product owner or sponsor (tests in tests/test_no_personal_names.py); the check is wired into .githooks/pre-commit; card issues were rewritten with reviewed phrase rules (scripts/board/depersonalize_cards.py; the generic rewrite is opt-in) and the archive regenerated; the persona Steve and the story opener As Steve stay. Python tests 172 pass; the compiler (243) and acceptance (156) gates pass.
- Re-freeze (D72): docs/GRAMMAR.md changed on one line (a rule's attribution and quotation removed, the rule text unchanged), sha256 0d78e599d63ce04bab693833bae38410ddfd1deb0f2db52e1cc5bb35b70ae50b to ad0f6e51fe1d61968fecb7ed3609bb00b204f3aa482defcf38f6fd6f6728dc9f; docs/CHEATSHEET.md (the scored document) and orders.phrase are byte-identical, so no new baseline is needed.

<a id="d109"></a>
#### Decider D109: Decider seats are by role, and decisions stay reversible

- Date: 2026-10-28. Question: how is the Decider seated when a model runs out of credits, and what keeps a wrong call cheap now that fewer models review. Decided as a standing principle by the product owner (no design dispute); recorded here and in the working agreement and the mob-defaults template. Result: passed.
- Decision: (1) Decider seats are by role, not by model. Every role is seated; each seat is filled by an available model in a fresh context; one model may fill several role-seats (never reviewing its own authorship); a vote is never taken with fewer seats because a model is out; the decision record names which model filled which role-seat. Reason: the diverse perspectives come from the roles, and a shrinking vote quietly weakens every decision. (2) Decisions stay reversible: each card, commit and decision is small enough to revert cleanly on its own; a decision that cannot be undone cheaply is flagged as a one-way door and escalated as the Escalation section says, otherwise it is split until it can be undone; the record names the revert step. Reason: with fewer models reviewing a wrong call is more likely, and cheap reversal is the safety net.
- Effect now: Grok is out (usage balance exhausted); the roles it filled are filled by Codex and fresh-context Claude seats. D100 to D108 listed the seats that voted; from D110 each record lists the role, the model that filled it and the context (fresh or not).

<a id="d116"></a>
#### Decider D116: retro experiment E3 (the decision log split by area) starts its trial; review fixes applied

- Date: 2026-10-29. Question: does the split by area (five area files, index, mapping, migration proof) hold on commit 6c5e0f4 and fixes; may the trial start. Seats by role (D109): reviewer, Claude (Sonnet), fresh context: Yes with fixes (the stub carries no decision numbers so references of the form DECIDERS.md D86 land on a page without them; two stale line citations in the extension map ADR; D31 recorded as superseded by D86 though D86 never names it; D82 recorded as fully superseded though D84 replaces only the real-project tier; a naming collision with the earlier use of E3 in D22); the facilitator found and fixed an earlier defect (a sign-off section with no decision id fell between two records and was lost; the recorded hash is now the sha256 of the original log's records, computed from git, and two regression tests cover it) and ran the full Python suite (211 tests). Result: passed.
- Applied: the stub now says to find a decision by number in INDEX.md; the extension map ADR cites the area file and record number; D31 is recorded as applied (its priority later changed by D86) and D82 as in force (its real-project tier was replaced by D84, the synthetic tier stays); E3 carries a naming note. Verified independently by the reviewer: 106 records, each in exactly one file, the region hash equals the recorded value, the area files reproduce the original record text apart from added link anchors.
- Trial: until the next retro, measure the tokens a fresh seat loads for one area plus the index (for example compiler seat: index 20 KB plus 37 KB, against 143 KB before). New records go to their area file with one areas.json entry (this record and D115 are the first). The Decider keeps, revises or drops E3 at the retro; the mob-defaults template keeps the single-file form until then.
- Reversal: one commit adds the split; reverting it restores the single file (the original is in history).

<a id="d118"></a>
#### Decider D118: the stale-CLI test helper fix is its own card, ranked next (card 173)

- Date: 2026-10-29. Question: a stale phrase CLI binary gave a false red in card 170 and can give a false green, because tests trust whatever phrase.dll is on disk; pull the fix out of card 172 and rank it next, or leave it in 172, or add a rule only. Options: A its own small card ranked ahead of 169 and 171 (the helper fails loudly when any CLI-relevant source is newer than the dll); B inside 172 after 171 with an interim rebuild rule; C a rule only. Seats by role (D109): architect, Codex, fresh context: Support-it for A (a small reversible safeguard against costly false greens in every incremental local gate); test-quality reviewer, Claude (Sonnet), fresh context: A (34 calls in seven test files trust the binary on disk, including all the trace and source-map tests; fail loudly rather than rebuild in the helper, because a second test process would race on the shared build folders; compare the newest source write time to the dll with a small tolerance; prove it with injected timestamps; risk: a false stale failure after a touch, so the message must say what to run); delivery lead, Claude (Sonnet), fresh context: B (CI builds clean and the facilitator rebuilds before every gate, so exposure is narrow; 172 touches the same helper file; risk: a false green before 172 lands). Grok is out of balance. Votes 2 to 1 for A. Result: passed.
- Decision: A. Card 173 holds the one fix and is ranked first in the queue, ahead of 169 and 171, with the helper failing loudly (not rebuilding) and one injectable-timestamp test. Interim rule until it lands: every local run rebuilds src/Phrase.Cli first (already the facilitator's practice before each gate; the dissent is recorded as the reason the rule stays). Card 172 keeps the shared process helper and the trace hardening; it no longer carries the stale-binary check.
- Reversal: the card is additive test-support code; reverting it restores the existence-only check.
