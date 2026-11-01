# Card 234 slice 2 of 2: review triage (ctx-2, Claude seat)

Sources read: both reviews (3c6181c independent reviewer, b37a8ad technical writer), slice 1 (turn-slice-architect.md, unchanged, its own Size line), the card, record-in-progress.json, usage/*.json, docs/process-wiki/INDEX.md and briefs, D136, D157, D168, D170. Slice 1 stays the slice for the original build; this file only adds the fixes.

## Review triage (ctx-2)

Reviewer findings are R1 to R10 (review order); writer findings are W1 to W4. Nothing blocking is deferred: R1, R2, R3 and W1 to W3 are blocking and stay in 234.

| Finding | Verdict | Reason |
|---|---|---|
| R1 no real facilitator restart (Done check 3) | FIX, criterion 3 | Done check 3 is false until a fresh context restarts; Q1. |
| R2 checkers never compare a value with its source | FIX, criterion 2 (seat metrics) and criterion 3 (restart entry) | "Fails on partial work" is false today for checks 2 and 3; Q4. |
| R3 Done check 2 has no measurement (all eight not_measured) | FIX, criterion 2 | The adopted condition asks numbers plus method; Q2. |
| R4 token sources are uncommitted .err files and prose | FIX, criterion 2 | Raw completion-report lines get committed and the checker reads them. |
| R5 rule_miss_cases zero is asserted | FIX, criterion 3 | Zero must be recomputed from two committed files; Q1. |
| R6 nothing launches a seat from a brief | FIX, criterion 2 | Without a consumer the saving is not in force; a `seat-prompt` command is the consumer (Q3). |
| R7 wiki can state a false rule and pass | FIX, criterion 1 | Criterion 1 says a rule "resolves to its decision"; a link without the text does not. |
| R8 two oversize reds, no deviation | FIX, criterion 1 (record only) | Q5; recorded in the first fix commit, no code. |
| R9 commit-body gaps (no Proof block, stale "pending", unshown runner) | MOVE TO A FOLLOW-UP: "Proof block and mutation-runner checks on card commits" | Commit text cannot be rewritten (no amends); it needs a pre-push rule, a new concern. |
| R10 criterion 1 mutations not measured | FIX, criterion 1 | The R7 red runs the mutation probes on page copies and commits the counts; no extra size. |
| W1 index does not list the briefs or routes | FIX, criterion 2 | Q3. |
| W2 qa-red and green briefs omit rule pages | FIX, criterion 2 | Q3 route table. |
| W3 slice brief omits thin-slices and one-criterion | FIX, criterion 2 | Q3 route table. |
| W4 writer brief not in index, no rule route | FIX, criterion 2 | Q3 route table. |
| W per-role notes (what a brief-only seat misses) | FIX, criterion 2 | They are the required route sets; the red pins them. |

Only follow-up: draft title "Proof block and mutation-runner checks on card commits" (R9). It owns three lines: a Proof block is required in a green body; a runner named in a body must be a committed script; a "pending" confirmation line must match the record. It changes no 234 criterion, so it is not scope pushed out. For the D168 retro counter-metric: 1 of 15 findings moves out.

## Q1. Done check 3 restart, for real

The fresh facilitator context is a new Claude subagent (agent type claude, no inherited context), started by the running facilitator after green 3. Steps, each its own record or doc commit:

1. Before the run, commit `usage/restart-key-<UTC>.json`: the facilitator's own expected `boundary_hash`, `next_card`, `next_step` and `rules_to_apply` (page ids). Committed first, so it cannot be fitted afterwards.
2. HANDOFF.md is refreshed (it is the 2026-10-29 handoff plus one line) with a "Restart here" section: last boundary hash, open cards, next step. New brief `briefs/facilitator-restart.md`, listed in the index with its route (seat-rotation, stand-downs, safety-and-push, thin-slices).
3. The subagent prompt is built by `seat-prompt facilitator-restart`: the index, HANDOFF.md and the brief, nothing else, plus an output format with the same four fields.
4. Commit the completion report verbatim as `usage/restart-report-<UTC>.txt`, with its `subagent_tokens` line.
5. Tokens: `subagent_tokens` from that report, compared with 965K. Honest label: tokens the restart used, cumulative, not a context-window reading.
6. Misses are computed, not asserted. The checker counts one miss for each page in the key's `rules_to_apply` that the report omits, one for a wrong `next_step`, one for a wrong `boundary_hash`. `rule_miss_cases` in the record must equal that computed list; a zero needs both files, so the zero source is the key and report pair, not a size reading.

The existing entry (782199, restarts[0]) is reclassified `kind: context_reading`, not a restart. The checker accepts a restart only with `kind: restart` plus both files. Until the run, the record says `done_check_3: pending`; the checker allows pending only while the card is not signed off, and the sign-off gate fails on pending. That is the honest downgrade the reviewer allowed, for the interval only.

## Q2. Done check 2 measured, for real

Same seat step, same model (Claude Sonnet; Copilot `-s` hides usage), same commit, same task, run twice on a neutral probe, "list the rules you will apply, then stop". Run A gets the full TEAM_WORKING_AGREEMENT.md path and its role name. Run B gets the output of `seat-prompt <brief>` (brief plus routed pages). The pair is run for three steps: architect-slice, qa-red, backend-developer-green (six runs, one per cell, stated as such, no variance claim). Raw completion-report lines go to `usage/raw/ab-<step>-{full,brief}.txt`. The record holds `seat_ab` with `full_tokens`, `brief_tokens` and `source`; the checker parses each number from its raw file and compares it. Setup is compared with 50K and the ratio brief to full is computed. A brief run above its full run is reported, not hidden, because the Q3 routes grow and the experiment must be able to say no. The end-of-seat median against 125K comes from the per-seat totals (Q4).

## Q3. Brief routing (reverses "index plus own role page only")

Yes, the pin changes. Slice 1 and the criterion 2 red read it as index plus role page. Done check 2 itself says "the index and the needed pages", so the board text is unchanged; only that reading is superseded here. New rule: a brief names the index, the role page and the rule pages its step needs, nothing else. The index gains a Routes table, one row per brief (brief id, pages), so the route is discoverable from the index alone. The checker requires each brief's `Pages:` set to equal its Routes row (set equality, reorder fine, repeats flagged) and every row page to exist and be enabled. Required sets, pinned by the red: ready: seat-rotation, decisions; qa-red: one-criterion, outside-in-tdd, hostile-inputs; architect-slice: thin-slices, one-criterion, hostile-inputs; green: one-criterion, outside-in-tdd, thin-slices, safety-and-push; audit: stand-downs, decisions, safety-and-push; ship: stand-downs, safety-and-push, decisions; signoff: decisions, stand-downs; writer: decisions, one-criterion. The facilitator records the supersession as a routine call by objection window 234-slice-2 (D145), not a new Decider.

## Q4. Checkers compare values with sources

Every recorded number carries `source` and `source_key`. The checker loads the committed file and requires the key to resolve to the same number (JSON: dotted key; text: a `NAME: N` line in the raw file; Copilot "in 937.9k" lines parsed with the k). usage/seats is restructured to rows keyed by context and seat; a context total is the checker-recomputed sum and the ratio to 125K is computed, never typed. `not_measured` is rejected when the source holds a number for that context and seat. A row with no raw file needs `unverified: true`; unverified rows are printed in the checker summary and left out of the 125K median. Mutations that must now exit 1: the four R2 mutations (setup=end=1, facilitator tokens 5 with the delta recomputed, zero source pointed at HANDOFF.md, all seats not_measured), plus a seats row changed by one and a raw file edited.

## Q5. D136 deviations

Record in `deviations` of the card record, in the first fix commit: eb68d95 (red 3, 969 added lines, limit 400) and c12f7f2 (red 1, 603 lines), both oversize, not split at the time, no Slice-exception line, found by the reviewer. Cause: whole test files written in one commit. Counter: every red in this plan stays at or under 350 lines by splitting inside a criterion (a red and green per half), which is allowed because each half carries whole checks of one criterion.

## Fix plan (D157 order; D136: at most 15 countable files and 400 lines per commit)

Criterion 1 (R7, R8, R10), about 9 countable files, 250 lines:
- Red: tests/test_process_wiki.py mutates page copies: a rule line rewritten while keeping its D-token and link fails; a quote not found verbatim in the decision section fails; mutation counts printed. About 90 lines.
- Green: scripts/check-process-wiki.py, a `Quote:` verbatim phrase per rule on the seven rule pages, and the deviations entry. About 160 lines.

Criterion 2 (R2 part, R3, R4, R6, W1 to W4), four commits, each under 350 lines:
- Red 2a: Routes table, set equality, `seat-prompt`, required sets of Q3 (tests/test_process_wiki.py, tests/test_mob_process.py).
- Green 2a: INDEX.md Routes, eight briefs, check-process-wiki.py, `seat-prompt` in scripts/mob_process.py. About 12 files.
- Red 2b: value-versus-source checks and mutations of Q4, `seat_ab` (tests/test_restart_record.py).
- Green 2b: scripts/check-restart-record.py, restructured usage/seats, usage/raw files, the six A/B runs, record seat_metrics. Record and usage files do not count (.mob/records); about 150 countable lines.

Criterion 3 (R1, R2 part, R5), two commits plus one record commit:
- Red 3: restart entry, key and report comparison, miss recomputation, `kind` and `pending` rules (tests/test_restart_record.py, 300 lines at most).
- Green 3: scripts/check-restart-record.py, HANDOFF.md refresh, briefs/facilitator-restart.md, the INDEX route row.
- Record commit by the facilitator after the restart run (Q1 steps 1 to 6): key, report, restart entry, `done_check_3: measured`.

## What the re-review will check

- Run the three test modules and both checkers; confirm the pre-push check calls them.
- Re-run all R2 mutations and the Q4 extras on fresh copies: each must exit 1. Re-run the rule rewrite mutation (R7): exit 1.
- The restart key is committed before the report (git log order and times); the report is a subagent output; the miss list the checker computes equals the record's.
- A/B raw files hold the numbers the record cites; no number is typed without a source; unverified rows are listed.
- Each brief's Pages set equals its Routes row and the writer's per-role sets; `seat-prompt` output contains those pages and no others.
- `git diff --stat` shows no decision or agreement text changed; no CR bytes; each fix commit within D136; each red at or under 350 lines; deviations hold both reds.
- Done check 2 and 3 status in the record matches the evidence (pending allowed only before sign-off); the reviewer, not the author, closes R1 to R3.

Size: 11107 bytes
