# Card 237 slice: architect (ctx-2, fresh context)

Sources read: board/237-evaluate-whether-an-indexed-just-in-time-process-wiki-solves-the-context-problem.md; .mob/records/237/record-in-progress.json; .mob/records/decisions/237-taskforce.json; docs/decisions/process-and-staffing.md (D136, D157, D166, D167, D168, D171, D172); .mob/records/234.json; .mob/records/234/usage/*; .mob/records/234/signoff-ctx8.md; scripts/check-restart-record.py; docs/process-wiki/*.

## Scope and T2 domain-expert answer

This slice defines the evaluation run for card 237 (evaluate, not build, per D172), with three criteria (D157), one fixed measurement frame, alternatives, cost method, and Decider decision rule.

T2 answer: **we do not need a wiki by default unless it beats simpler alternatives on the same tasks with equal-or-better miss rates and acceptable upkeep**. The evaluation must prove that.

## Criteria (Done checks 1 to 3, with evaluation red/green meanings)

1. **Problem measures from committed sources.**  
   Red means a committed measurement plan and checks that fail until committed measurements exist (tokens per seat over N runs per role, facilitator context growth over a card, blocked-message count).  
   Green means committed measurements plus an assessment tied to those sources.
2. **Alternatives compared on the same task and costed.**  
   Red means committed comparison checks that fail when any alternative is missing, uses a different task, or has no cost method (build, upkeep, tokens/turn).  
   Green means all alternatives measured on the same task frame with one ledger and cost table.
3. **Decider verdict and retro-end wiki-review proposal decision.**  
   Red means a committed decision template/check that fails until adopt/reshape/reject and the product owner proposal decision are recorded with metric and counter-metric.  
   Green means Decider outcome recorded, with rationale and next-card direction if adopt/reshape.

## Adopted condition made concrete (pre-trial commit package)

Before any trial run, commit these four items (from 237-taskforce adopted condition):

1. **Fixed tasks (one per role, frozen for all alternatives):**
   - ready: declare task-force/evaluation frame from committed inputs.
   - slice: write criterion plan + assumptions + hostile-input lines.
   - red: define failing checks and measurement schema.
   - green: execute one trial run and fill one measurement row.
   - ship: compile ledger and cost table from committed rows.
   - signoff: record adopt/reshape/reject decision against thresholds.
2. **Baseline:** full agreement ("do nothing") arm on the same role tasks, same role order, same repository state, same prompts, same run count N.
3. **Token-counting rule (same cached-input handling for Copilot and Claude):**
   - counted_tokens = input_total + output_total.
   - Copilot: input_total is provider-reported input (already includes cached input); record cached_input as provider-reported subset when present.
   - Claude: input_total = input_tokens + cache_read_input_tokens + cache_creation_input_tokens (same conceptual treatment: cached input is included in input_total); output_total = output_tokens.
   - Ledger stores input_total, cached_input, output_total, counted_tokens for both providers.
4. **Miss test:** seed one must-read rule per run by run key (unknown to runner). Key writer is a seat that does not know the expected task answer (independent reviewer writes key after fixed tasks are frozen, before run starts). Miss if required rule page is not read/applied or wrong next step is produced.

## Independent reviewer recompute (ledger cannot be self-certified)

Independent reviewer recomputes each row from committed provider artifacts and committed run reports only:

- recompute counted_tokens with the fixed rule above;
- recompute miss count from seeded key vs report (same model as `compute_restart_misses` in `scripts/check-restart-record.py`);
- verify baselines, deltas, medians, and per-alternative aggregates;
- reject any row with missing source, uncommitted source, or mismatched recompute.

Delivery lead may run/cost/compare, but reviewer recompute is the accepting gate.

## Alternatives (all measured on the same tasks and run protocol)

1. **Do nothing:** full TEAM_WORKING_AGREEMENT + current decision docs (baseline arm).
2. **Prototype wiki:** `docs/process-wiki/INDEX.md` + role briefs/pages (card 234 artifact).
3. **Shorter agreement:** compact agreement reduced to the minimum rules needed for the fixed tasks.
4. **Per-role excerpts:** role-specific extracted agreement text without wiki index routing.
5. **Decision-index + handoff only (justified extra):** decision anchors + handoff notes without role wiki prose; tests whether routing alone is enough.

Each alternative runs the same role tasks, same order, same run count N, same scoring.

## Problem measures and committed sources

- **Tokens per seat over N runs per role:** choose **N=5**. Justification: enough repeats to smooth single-run cache/warm variance while staying feasible for one evaluation card. Source: committed provider usage artifacts and run reports; ledger fields must carry source keys.
- **Facilitator context growth over a card:** measure start, boundary points, and end context tokens in one card run per alternative. Source: committed facilitator context readings (same kind used in `.mob/records/234/usage/facilitator-2026-11-01-0624.json`).
- **Messages blocked by long turns:** count seat messages delayed beyond threshold T due to long-running turns (T fixed in red check). Source: committed session/event timestamps from run logs/reports, not narrative claims.

## Cost estimate method

For each alternative:

- **Build cost:** one-time setup effort to create/prepare the format and checks (time + tokens).
- **Upkeep cost:** recurring retro/update effort per card (time + tokens), including staleness review if adopted.
- **Tokens per turn cost:** median counted_tokens per role turn from N runs.

Total cost view = build amortized over next K cards + upkeep per card + tokens/turn burn.

## Decider rule (adopt / reshape / reject)

- **Adopt** when it beats baseline on tokens/seat and blocked messages, keeps miss rate at or below baseline, and upkeep stays within agreed budget.
- **Reshape** when it improves one or more core measures but fails another (typically miss rate or upkeep); Decider names required changes and orders a follow-up evaluation/build card.
- **Reject** when it does not beat baseline or adds unacceptable upkeep/ceremony.

Product owner proposal (retro-end wiki review) is decided inside criterion 3:
- metric: wiki staleness (rules changed in retros but not reflected in wiki);
- counter-metric: added retro time;
- outcome: adopt/reshape/reject as experiment rule.

## Open point resolution: six-role checker vs no developer seat

Record checker requires ready/slice/red/green/ship/signoff; this evaluation task force has no developer seat.  
Mapping for this card:

- ready -> product manager (ctx-1)
- slice -> architect (ctx-2)
- red -> QA engineer (ctx-3)
- green -> delivery lead (ctx-5) for trial execution/measurement completion
- ship -> independent reviewer (ctx-4) for ledger recompute gate
- signoff -> decider (ctx-6)

If checker insists on role-name history, record this as an explicit evaluation-card deviation: "green role fulfilled by delivery lead; no product-code implementation role required."

## D166 assumption line(s)

Assumes: provider usage artifacts expose enough token fields to normalize cached input consistently across Copilot and Claude; probe: card 234 usage artifacts show both cached and non-cached components required by the counting rule.

Assumes: miss recompute can be derived from committed key/report fields, not seat memory; probe: `scripts/check-restart-record.py` computes misses from key/report and flags mismatches.

## D167 Inputs lines (where criteria take lists/maps)

Criterion 1 Inputs: disabled: source file marked disabled is excluded from measures; unknown: unknown source key fails row; repeated: duplicate row key rejected; reordered: row order must not change aggregates.

Criterion 2 Inputs: disabled: disabled alternative still listed but scored as not-run with reason; unknown: unknown alternative id fails comparison; repeated: duplicate alternative id fails; reordered: order-independent aggregate must match.

Criterion 3 Inputs: no list input.

## D136 commit plan (thin, one criterion at a time)

1. Red 1 (Criterion: 1): commit failing checks for required measurement sources and N-run-per-role presence.
2. Green 1 (Criterion: 1): commit measured rows and criterion-1 assessment.
3. Red 2 (Criterion: 2): commit failing checks for missing alternatives/same-task/cost fields.
4. Green 2 (Criterion: 2): commit full alternative comparison and cost table.
5. Red 3 (Criterion: 3): commit failing check for missing Decider verdict and retro-end proposal decision.
6. Green 3 (Criterion: 3): commit adopt/reshape/reject record with rationale and proposal outcome.

Slice verdict: **SHIP AS IS** (evaluation-only slice; no product-code changes).

## Amendment for window 237-slice (ctx-2)

Meets the three adopted conditions of the window (D145, D168). This amendment supersedes the earlier "N=5" and the five-alternative run above; those become the escalation path only.

### 1. Pilot first

- Arms: two. Prototype wiki (docs/process-wiki) against the full-agreement baseline. Same frozen tasks, same repository state, same prompts, same role order.
- Roles: the six mapped above (ready, slice, red, green, ship, signoff).
- N: stage A is N=1 per role per arm, so 12 runs. Stage B (only if stage A is ambiguous) adds a second run per role per arm, 12 more runs.
- Ambiguous means any of: median token delta from 5% up to but not including 20%; wiki misses exceed baseline misses by exactly 1; the token delta and the blocked-message result point opposite ways.
- Escalation: A clear adopt or reject ends the trial after stage A. Ambiguous goes to stage B. Still ambiguous after B goes to the Decider, who may add alternatives 3 to 5 only by committing a new cap first (condition 3 applies again). No other escalation, no N above 2.

### 2. Pre-trial red check

- Check: scripts/check-wiki-trial.py, committed failing before any run. It reads .mob/records/237/trial/thresholds.json, freeze.json, miss-key.json, ledger.json and recompute.json, and uses git history (`git log` and `git merge-base --is-ancestor`) for ordering.
- freeze.json: hash of the six frozen task texts and the arm list. miss-key.json: one seeded must-read rule per run, with author seat. recompute.json: the reviewer's own recompute of each ledger row, from committed provider artifacts and run reports only.
- It fails when: a file is missing; miss-key author is a runner seat (delivery lead, ctx-5) or any seat that wrote the freeze; the key commit is not a descendant of the freeze commit; the key commit is not an ancestor of the first run commit; the thresholds commit is not an ancestor of the first run commit or of the ledger; a ledger row has no committed source; a recompute (counted_tokens, misses, delta, median) differs from the ledger row by any amount; a duplicate, unknown or disabled row key (D167); the sum of counted_tokens exceeds the cap.
- Self-test: the check ships with its own failing fixtures for each case above, so the red is real and not an absent file.

### 3. Numbers committed before any run (by reviewer and Decider, in thresholds.json)

Basis: card 234 measured full agreement 69701 tokens and brief 49032 tokens: a saving of 20669 per seat turn, 29.7%. One pair of runs is thin evidence, so thresholds sit below it.

- Token delta: adopt needs wiki median counted_tokens at least 20% below baseline, in at least 4 of 6 roles. Margin: 9.7 points below the 29.7% seen, to absorb cache and run variance. Reshape from 5% to under 20%. Reject under 5% (inside noise).
- Misses: adopt needs wiki misses at or under baseline, and 0 wiki misses of 6 keys in stage A. Reject if wiki misses exceed baseline by 2 or more. A saving that loses rules is not a saving.
- Blocked messages: a message is blocked when it waits over 600 s (10 minutes) behind a long turn, from event timestamps. Adopt needs wiki count at or under baseline. Reject if wiki is over baseline by 2 or more. Reason: 12 runs cannot give a rate, so this only guards against a regression.
- Upkeep budget: at most 12400 tokens per card (10% of a 6-seat card's saving, 6 x 20669 = 124014) and at most 10 minutes added retro time (the product owner's counter-metric). Above budget forces reshape. Reason: upkeep must leave 90% of the saving.
- Total-token cap: stage A 1,000,000 counted tokens, stages A and B together 2,000,000. Estimate: stage A is 6 x 69701 + 6 x 49032 = 712,398; add about 100,000 for the key writer, recompute and tool overhead, and rounding up gives 1,000,000 (worst case, wiki as costly as baseline, is 836,412). Stage B doubles the run share. Payoff: the saving is 124,014 per card, so stage A breaks even in about 8 cards and the full cap in about 16. On reaching the cap the trial stops, unfinished rows are recorded as not-run, and the Decider may not adopt.

### Commit table (replaces the D136 list above; step 0 is new)

0a. Pre-trial red: check-wiki-trial.py with failing fixtures (QA). 0b. thresholds.json and cap (reviewer, Decider). 0c. freeze.json. 0d. miss-key.json by a non-runner; check goes green. Then steps 1 to 6 above unchanged, each run commit after 0d. Verdict stays SHIP AS IS with this amendment.

Size: 13606 bytes
