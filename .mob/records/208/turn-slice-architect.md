# Card 208 slice (architect, ctx-2)

Decision: SPLIT under D136 and D157. Card 208 keeps one concern, the first (a seat starts only on a fresh, pinned probe, and a Claude seat only while no other model has quota; token MD-002). Six new one-concern cards (220 to 225, numbers provisional until the board assigns them) take the rest, each citing F-008 as its corrective finding. Every card has at most three Done checks, numbered 1 to 3 here before any red (D157), at most three increments and one repository. The code is in mob-defaults (C:\Users\ringe\mob-crosscli) for all seven; phrase-lang gets only records, notes and one Avowry config file at the end of 224.

Seat: architect (slice), context ctx-2, Claude; reason for a Claude seat: probes at 17:13 UTC (.mob/records/probes/2026-10-30.jsonl) show Codex, Copilot, Grok and agy without usable quota (copilot has_quota=False remaining=-5; grok 402 balance exhausted; agy 429 reset in 52 h; codex exit 1). The probes are 15 minutes old at this seat (under 60).

## 0. What the code does today (read at mob-crosscli HEAD e239b12)

These facts decided the split; four of them correct the card text.

- Gap 1 is real only for bulleted notes. Card 208's own text (Left out, Assignee note and Domain-expert question written as plain paragraphs) already parses to exactly the seven roles and check_task_force returns no problem. Writing "- Left out:" and "- Assignee note:" as bullets makes parse_task_force return two false roles and check_task_force refuse the card ("role 'Left out' has no 'why:'"). A loud refusal, so it is the least drift of the five.
- Gap 5 is worse than the card says. run_loop calls stop_the_line_message(load_findings(root)) at the top of the loop, before deps.pick(), so a blocking finding stops the loop with exit 5 before it knows which card is next, and the 205 exemption (corrective_gate_message, read_card_issue) lives only in facilitator.py. While MD-001, MD-002, MD-003, MD-010 and F-008 block, the loop cannot start any corrective card, which is every card of this lock-down.
- A new hole the card does not list: run_loop discards the return value of deps.retro(card) and deps.rerank(). A retro or re-rank command that fails does not stop the loop; the next card starts on a skipped ceremony. This goes into 222.
- Probes: CardRun.refresh_probes runs only at the top of each cycle (facilitator.py run_mobbed line 1352, run_plain line 1417), never before a seat. A cycle has up to ten seat turns (navigator, driver, observers, fix rounds, refactor, ship) and a driver turn may run 30 minutes (--turn-timeout 1800), so a seat can start on a probe far over 60 minutes old. This is the F-040 hole; card 214's review attempt 1 was stopped for exactly it (seated on 110-minute-old probes, docs/retros/card-214-2026-10-30.md). probe_is_stale uses `>`, so a probe exactly 60 minutes old counts as fresh while the card wants 60 or more to be stale.
- Quota-first is checked once, at card start, and only for an all-Claude roster (claude_only_problem). A mixed roster with a Claude seat while Codex has quota passes. MD-002 asks for per-seat.
- The probe model is implicit: probe_backend calls run_backend(..., "default", ...), and run_backend falls back to `next(iter(models.values()))` when the key is missing. Claude has no "default" key (haiku, sonnet, opus), so its probe model is whichever key is first in backends.json; reordering the file would silently probe with opus. Pinning means an explicit `probe_model` per backend.
- phrase-lang already checks the product side of this (D145 C6: a Claude seat needs a probe line under 60 minutes old for every non-Claude model; scripts/probe_drivers.py pins Codex to the seat model). mob-defaults, the template every other mob inherits, has none of it. D145 C4 names the record field: seats carry `quota_reason`.
- Overlap with card 210 (MD-003): its check 3 says "a Claude seat needs a reason". This slice does not build a second checker. Card 208 writes `quota_reason` into the record from the fresh probe; card 210's check requires it. 210's "Enables: 208" line is stale (D160 put 208 first) and should read the other way.

## 1. The split

| Card | One concern | Finding | Checks | Depends on | Order |
|---|---|---|---|---|---|
| 208 (kept, retitled) | A seat starts only on a fresh pinned probe, and a Claude seat only while no other model has quota, with `quota_reason` in the record | MD-002 (F-008 also named in the issue) | 3 | none | 1 |
| 220 | The loop runner starts a corrective card while a finding blocks (gate after pick, issue read, fail closed) | F-008 | 3 | none | 2 |
| 221 | The loop runs a card in the repository its Repository line names (records, findings and probes stay in the loop root) | F-008 | 3 | none | 3 |
| 222 | The loop reads its retro, re-rank and discovery commands from the repository and stops when a step fails or is missing | F-008 | 3 | none | 4 |
| 223 | The per-card retro: feedback from every active role, each in its own context, kept in the retro file and the record | F-008 | 3 | 208, 222 | 5 |
| 224 | The re-rank: the product manager leads, a Decider decides, a note is committed, the order is applied only after a pass | F-008 | 3 | 208, 222 | 6 |
| 225 | Card mode reads only role lines of the declared format as the task force | F-008 | 3 | none | 7 |

Drafts: C:\Users\ringe\AppData\Local\Temp\claude\C--Users-ringe\73298da3-f8ff-4a30-af55-b3e95ffa16f4\scratchpad\220-*.md to 225-*.md (Contract shape).

### Why this order (which gap, once enforced, stops the most drift first)

Two facts rank the cards. First, a rule bites only if something runs it: hand-run cards (as this one) are bound only by the record checker and the pre-push hook, so the staffing gate in facilitator.py bites when the loop starts a card; the first cards must therefore be the ones that make the runner usable, but ranked among themselves by silent drift, not by loud refusal. Second, a loud refusal is not drift (the operator sees it and rewords); a silent pass is.

1. 208 (seat probe gate). The only silent staffing drift and the only FAIL among the five. Every card since the audit has been staffed by hand on probes of unknown age (F-040 hit card 214's review), Claude seats carry a free-text reason, and a mixed roster is never checked. After it lands, no seat starts on a probe of 60 minutes or more, no Claude seat starts while another model has quota, and the record carries a derived `quota_reason` that card 210 then requires. It is also the gate that 223 and 224 inherit for the seats they start.
2. 220 (exemption). Without it the loop stops with exit 5 on every corrective card for as long as any lock-down finding is open, so the operator falls back to hand-running, which is the takeover D161 wants gone. It also removes two gates with two different rules (facilitator exempts, loop does not). Smallest of the loop cards, three checks already written in the parent.
3. 221 (repository). A silent wrong-tree hazard: loop.py hands every card --repo <loop root>, so a mob-defaults card would have its seats edit the product tree. Every remaining lock-down card lives in mob-defaults, so this must precede the loop ever running one.
4. 222 (steps and stop). Closes the silent hole where a failed retro or re-rank is ignored, and removes the hand-supplied --retro-cmd/--rerank-cmd that make `--run` exit 2 today.
5. 223 (retro) then 6. 224 (re-rank): each turns a ceremony done by hand (retro-voices.json and notes written by the facilitator session) into a runner step. Retro first because the loop order is card, Done, retro, re-rank, and a finding raised in a retro changes what the re-rank may pick. They convert one by-hand rule each, so they come after the cards that stop silent drift; they are also the largest.
7. 225 (role lines). A loud refusal with a one-line workaround (write notes as paragraphs, as the board generator does); last.

Closure order for the findings: MD-002 closes after 208 lands. F-008 stays open until 225 lands (the last F-008 card), because a new card is startable during stop-the-line only while its issue names a finding that still blocks (205). The delivery lead of 225 asks the auditor for F-008 closure; no earlier card does. If another finding is the only one left blocking at that point, the cards that follow must name that one.

### Where the parent's Done checks and the MD-002 extras go (the sum is kept)

| Parent Done check | Lands in |
|---|---|
| 1 role lines only (gap 1) | 225 criteria 1 to 3 |
| 1 runs in the card repository (gap 2) | 221 criteria 1 to 3 |
| 2 retro without hand-supplied command (gap 3) | 222 criterion 1, 223 criteria 1 to 3 |
| 2 re-rank through a Decider (gap 3) | 222 criterion 1, 224 criteria 1 to 3 |
| 2 fresh probes before every seat (gap 4) | 208 criterion 1 |
| 3 corrective exemption in loop_runner.py (gap 5) | 220 criteria 1 to 3 |
| MD-002: quota-first per seat, reason per Claude seat | 208 criterion 2 |
| MD-002: pinned probe model | 208 criterion 3 |

## 2. Card 208's own Done checks (numbered, in red-green order)

The order is the order of value and dependency: 1 gives the fresh probe that 2 reads; 3 hardens the probe both use and depends on neither.

1. A seat starts only on a probe under 60 minutes old (F-040, MD-002). Before every seat turn (seat_turn and turn) the facilitator checks the age of the last probe; at 60 minutes or more it probes again first; if it cannot probe (the probe raises or its evidence cannot be written) the seat is refused with exit 2 and a named reason, before any backend call. The seat's MOB.md entry and events line cite the probe's taken_at. ZOMBIES: Z (the card-start probe is the only one: no second probe before seat 1), O (one seat on a fresh probe: no re-probe), M (several seats, the third past 60 minutes: one re-probe, before it), B (exactly 60 minutes is stale, 59 minutes 59 seconds is fresh), E (probing fails: refused, no backend call, never staffed on the stale probe), I (every seat's cited probe is under 60 minutes old at its start). N/A: S, covered by the existing probe tests.
2. A Claude seat starts only while the fresh probe shows no other model with quota, and the record says why (MD-002). A seat on Claude is refused (exit 2, naming role, context and the model with quota) when any non-Claude backend's probe succeeded, in an all-Claude roster as today and now in a mixed roster; each Claude seat's entry in `.mob/records/NNN.json` gets `quota_reason`, built from the probe it relied on (taken_at plus each non-Claude backend's evidence), never typed by a seat. ZOMBIES: Z (no Claude seat: no refusal, no `quota_reason`), O (one Claude seat, all four others down: starts, reason cites the probe), M (three Claude seats across a re-probe: each reason cites its own probe), B (exactly one other model with quota refuses; a backend disabled in backends.json counts as no quota), E (a non-Claude backend missing from the results is refused, never read as no quota), I (the refusal text names the seat and the model; exit 2). N/A: S, covered by the existing claude-only tests.
3. The probe model is pinned (MD-002). Each enabled backend in core/backends.json names `probe_model`, a key of its own models; probe_backend uses it (not the first dict entry) and the probe result records the model; a missing or unknown pin refuses the card at start (exit 2, naming the backend). ZOMBIES: Z (an enabled backend without `probe_model` is refused by name), O (one backend pinned, the model in its result), M (all five pinned, each result carries its model), B (a pin that is not a key of models is refused; reordering models does not change the probe model), E (a disabled backend needs no pin), I (the shipped backends.json passes). N/A: S, covered by the existing probe tests.

Not in this card: a seat whose own declared model is unavailable keeps today's behaviour (the seat reports it, role turns fail over); the record checker's requirement of `quota_reason` (card 210); retro and re-rank seats (223 and 224 inherit the gate); phrase-lang's probe_drivers.py.

## 3. Criterion-by-layer table (outermost first, doubles per layer, F-055)

Layers: L1 facilitator card mode as a subprocess (test_card_mode.py style); L2 `CardRun.gate_seat` (new, called at the top of seat_turn and turn); L3 pure rules in mob_process.py (`probe_is_stale`, `quota_first_problem`, `quota_reason`, `probe_model_problem`); L4 the probe boundary (`probe_backend`, `run_backend`, core/backends.json); L5 the written record (`write_record` to .mob/records/NNN.json); L6 loop_runner exit-code mapping (GATE_EXIT_CODES).

| Done check | Layers needed, outermost first | Red-able today? | Doubles consumed |
|---|---|---|---|
| 1 O/M order: a re-probe heading appears in MOB.md before the seat entry that needed it | L1 -> L2 -> L4 | red-able (today probes only at cycle top; with `--probe-max-age-minutes 0` the MOB.md order shows no probe between seat turns) | fake_cli.py behind a temp backends.json; `--probe-max-age-minutes 0` as clock double (every probe stale); fake_probes.log probe counter (all existing) |
| 1 M (three seats, third stale) | L2 | red-able (stub gate_seat from the seam commit does nothing, so the assertion on probe calls fails, not an import error) | seam S1 scripted clock, seam S2 scripted probe function |
| 1 B exactly 60 min stale; 59m59s fresh | L3 | 60 min red-able (`>` today); 59m59s born-green guard | none (datetimes) |
| 1 E probe raises: refused before any backend call | L2 (and L1 exit code) | red-able (today the seat is called) | S2 probe function that raises; fake_calls.log must stay unchanged |
| 1 Z/O fresh probe is not repeated | L1 | born-green (today the 60-minute test already passes) | existing counter; guard, see section 4 |
| 1 I cited probe under 60 minutes | L1 -> L5 | red-able (no citation today) | existing fake_cli, record read-back |
| 2 B mixed roster refused (Codex seat, Claude seat, agy up) | L1 -> L2 -> L3 | red-able (only all-Claude is refused today) | fake_cli, FAKE_FAIL_BACKEND / FAKE_LIMIT_BACKEND (existing) |
| 2 O quota_reason present, cites probe | L1 -> L5 | red-able (no field today) | fake_cli, FAKE_FAIL_BACKEND for the other four |
| 2 M three Claude seats across a re-probe, different taken_at | L2 -> L5 | red-able | S1 clock, S2 probe function (two scripted probe sets) |
| 2 E backend missing from results | L2 -> L3 | red-able | S2 returns a dict without codex |
| 2 Z no Claude seat, no field | L5 | born-green (guard) | fake_cli |
| 2 I refusal text, exit 2, loop stop | L1; L6 | text red-able; the loop mapping born-green (2 is already in GATE_EXIT_CODES) | existing test_loop_runner exit-code fake |
| 3 Z enabled backend without pin refused by name | L4 -> L1 | red-able | temp backends.json without `probe_model` |
| 3 B reordered models keep the probe model | L4 | red-able (claude probes opus when listed first) | new double in red 3: fake_cli logs the `--model` it was started with into fake_probes.log (FAKE_PROBE_MODEL_LOG) |
| 3 B unknown pin refused | L3 | red-able | plain dict |
| 3 E disabled backend needs no pin | L4 | born-green (guard) | temp backends.json |
| 3 I shipped backends.json passes, results carry model | L4 -> L5 | red-able until the green adds the five pins | real core/backends.json read, no double |

Overlaps: criterion 2 reuses criterion 1's re-probe and adds no new probe path; the three existing claude-only tests in test_card_mode.py (DriverProbes) are kept unchanged and are the proof that the all-Claude refusal at card start still holds.

## 4. Born-green guards and the mutation rule (window 214-slice condition, applied)

A born-green test passes on the old code, so it cannot be seen red by running it alone. Each carries a named one-line mutation, applied in a scratch working copy (never committed), which must turn that test red and no other. The failing output (test name, assertion text, the mutation as a two-line diff) is pasted in the body of the commit that adds the test, before it lands. For a guard that its criterion's green rewrites (G1, G3, G5, G7), the mutation is repeated on the green tree and pasted in the green's body. The reviewer (ctx-5) may re-run any. A guard whose mutation does not turn it red is rewritten or removed in the same commit. Count: 8 new guard tests (G1 to G4 and G6 to G9), named `test_guard_<what>`; the 3 existing claude-only tests are reused as guard G5 and are not counted again.

| # | Test | Guard it protects | Mutation that must turn it red | Lands in |
|---|---|---|---|---|
| G1 | test_guard_a_fresh_probe_is_not_repeated_before_a_seat | gate_seat re-probes only at 60 minutes or more | make the staleness test always true | red 1 (guard), mutate old tree and the green 1 tree |
| G2 | test_guard_59_minutes_59_seconds_is_fresh | the threshold is 60 minutes, not lower | change the limit 60 to 59 | red 1 |
| G3 | test_guard_a_refused_seat_makes_no_backend_call | the gate runs before run_backend | call the backend first, then gate | red 1, mutate green 1 tree |
| G4 | test_guard_no_claude_seat_means_no_quota_reason | `quota_reason` only for Claude seats | write it for every seat | red 2 |
| G5 | the three existing claude-only tests (DriverProbes) | all-Claude roster still refused at card start, exit 2 | delete the claude_only_problem call in card_start_checks | already on main; mutate before red 2 and on green 2 |
| G6 | test_guard_a_claude_seat_starts_when_every_other_model_is_down | the new refusal does not over-refuse | refuse every Claude seat | red 2, mutate green 2 tree |
| G7 | test_guard_exit_two_from_a_seat_stops_the_loop_as_gate_refused | the loop reads exit 2 as a gate refusal, no retry | remove 2 from GATE_EXIT_CODES | red 2 (lives in test_loop_runner.py) |
| G8 | test_guard_a_disabled_backend_needs_no_probe_model | no pin for a disabled backend | require the pin for disabled | red 3 |
| G9 | test_guard_the_shipped_backends_json_passes_probe_model_check | the five real pins stay valid | delete one pin from a copy of core/backends.json | green 3 |

Ordering rule for red-before-fix: every guard is committed in a red commit, strictly before the green of its criterion, except G9, which needs the pins and lands in green 3 with its mutation in the body. At the head of each red commit only that criterion's red-able tests fail and every guard passes; at each green the reviewer sees all of that criterion's tests green, every earlier guard still green, and the recorded mutation output.

## 5. Commit plan (each within D136: at most 15 countable files, 400 changed lines; D157: Criterion n in each red body, green n before red n+1, greens pushed at once)

No red is committed in advance. Subjects: `Card 208 <red|green|seam> (MD-002, D145, D136[, D157 Criterion n]): <what>`; no trailers.

0. Seam (green, no behaviour change; part of increment 1): CardRun.clock (default the real UTC clock), CardRun.probe_fn (default probe_backend), a gate_seat stub that returns the current probe unchanged and is called at the top of seat_turn and turn; a guard that the stub is called once per seat. facilitator.py plus one test file; about 45 lines.
1. Red 1 (Criterion: 1): tests/test_seat_gate.py (L2 and L3 cases, G1, G2, G3) and a SeatProbes class in tests/test_card_mode.py (O ordering, I citation); 2 files, about 190 lines.
2. Green 1: facilitator.py gate_seat body (re-probe at `>=` 60 minutes, refuse on probe failure, cite taken_at) and mob_process.probe_is_stale `>=`; 2 files, about 60 lines.
3. Red 2 (Criterion: 2): tests for quota-first and `quota_reason` (L1, L2, L5), G4, G5 mutation record, G6, G7; 3 files, about 210 lines.
4. Green 2: mob_process.quota_first_problem and quota_reason, gate refusal in gate_seat, `quota_reason` written in write_record; 2 files, about 90 lines.
5. Red 3 (Criterion: 3): tests/fakes/fake_cli.py FAKE_PROBE_MODEL_LOG, pin tests (L4), G8; 3 files, about 130 lines.
6. Green 3: core/backends.json five `probe_model` entries, probe_backend uses and records the pin, card-start refusal, G9; 3 files, about 70 lines.
7. Review fixes (if any): `Card 208 review fixes`, a separate commit under the same limits.
8. Closure evidence, no code (phrase-lang): the record, the probe lines cited by the cited seats, the board archive.

Largest commit about 210 lines and 3 files. If red 2 passes 400 lines, split it at Z/O/M versus B/E/I (the planned seam); a second red for the same criterion is allowed, still before green 2.

## 6. Domain-expert question at the slice boundary (T2)

Should a member with different domain knowledge or context join now? No, for card 208's slice and for the roster of the six new cards as drafted. Card 208 changes Python in facilitator.py, mob_process.py and one JSON file; every risk it has (the gate runs before the backend call, exit codes, a probe that cannot be taken, a clock boundary at exactly 60 minutes) is a ZOMBIES case with a double in section 3, and the reviewer seat carries the runner lens. The same roster (ctx-1 to ctx-7) carries over to each new card unless that card's refinement proposes another by window, as the 208-taskforce proposal says.

Two pre-flagged triggers move to the cards where they live, as questions to ask afresh at those refinements, not as answers now: card 221 (cross-repository git, working directory and path behaviour on Windows: drive letters, spaces, `git -C`, safe.directory) is the likeliest place a seat with that knowledge is needed, and the call is the 221 product manager's at ready; card 224 (how a Decider is invoked and applied to the board) is covered by the backend seat, who works in decider.py and scripts/board/rerank.py already. Adding a member later stays a mid-card Decider.

## 7. Housekeeping for the facilitator (nothing here changes a rule)

- Card 208 itself: retitle and replace the three Done checks with section 2 (a card-text fix through a window, D153 L1); the parent paragraph "Proposed split" and the five gaps move to Context as "split into 220 to 225". Issue first line stays "Corrective action for MD-002 and F-008".
- Each new issue: first line "Corrective action for F-008 (<what>)", the exact token F-008 in the body (205's test), as 210 does for MD-003.
- Card 210: its Enables line points at 208 the wrong way round; its check 3 reads the record field `quota_reason` that 208 criterion 2 writes (name it in 210's ZOMBIES).
- Card 212 "Depends on 208 (retro and re-rank commands)" becomes 224 and 222; card 215 "Depends on 208 (its retro command)" becomes 223 and 222. 212's check 3 (a no-change re-rank still writes a note) overlaps 224 criterion 2; keep 212's side as the gate-side test (the runner accepts the note).
- Number order: the new cards start at 220 only if 219 is the last number taken; the board assigns the real numbers.

Slice field: SPLIT | 208 plus 220 to 225; about 5 to 7 files per card, no commit above about 210 lines | 3 increments per card (one per criterion; 208's seam commit belongs to its increment 1) | one concern per card: 208 the probe that gates a seat; 220 the loop's corrective exemption; 221 where a card runs; 222 the loop's steps and their failure; 223 the retro; 224 the re-rank; 225 which lines are roles
