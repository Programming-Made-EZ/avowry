# Card 237 sign-off (decider / sign-off, ctx-6, fresh context)

Seat: Claude. Probe: 2026-11-01 18:20 UTC. Read: the slice's signoff task (turn-slice-architect.md), trial/thresholds.json and its git history, decider-thresholds.json, assessment-pm.md, D173 (docs/decisions/process-and-staffing.md), recompute.json, ledger.json, the 12 trial/runs/*.json usage blocks, preship-gate.md (ae02695), record-in-progress.json (3a67bef), board/238. Ran `python scripts/check-wiki-trial.py` (`wiki-trial: OK (12 rows)`). Changed no code. No delete command, no git stash, nothing under .mob/audits/.

Intent (D165): confirm that the card 237 outcome was decided inside what the committed thresholds permit, and that the record is complete enough to close; sign the decision, not the wiki.

Assumes (D166): the recompute's grading of wiki-slice-1 (one miss) is right, since I did not read the miss key; probe: the miss is stated in recompute.json, ledger.json and the assessment alike, and I did not open miss-key.json or its history.

## Verdict: SIGNED OFF (the decision and its record), with one carried gap in the seat token figures (below)

## 1. D173 against the thresholds

I re-added the 12 usage blocks myself by the slice rule (input + cache read + cache creation + output): total 8,868,059; baseline median 911,895.5; wiki median 531,242.5; delta 41.74%; five of six roles at or over 20%. All match recompute.json and the ledger.

- Tokens: adopt side (needs 20% in at least 4 of 6 roles).
- Misses: baseline 0, wiki 1 (slice cell). Adopt needs 0 wiki misses of 6, so adopt is barred. Reject needs an excess of 2; the excess is 1, so reject is barred.
- Blocked messages: 0 and 0 (longest run 124.6 s against 600 s). Vacuous for headless runs, and D173 says so.
- Upkeep: not measured. Adopt cannot be shown within 12,400 tokens and 10 minutes, and unmeasured cannot clear the adopt rule.
- What is left is reshape (or stage B, which the slice also allowed). D173 chose reshape and does not run stage B. That is inside the thresholds. Motion A's text states these same reasons, and its votes (6 Yes; re-run 5 Yes + 1 Support-it; no No) are in D173-votes.json.
- Motion B is conditional on motion A and runs three retros after card 238 lands, with the committed 10-minute line as the stop. Consistent with D172.

## 2. No threshold moved except the cap raise

`git log` on thresholds.json shows exactly two commits: 139f607 (before run 1) and c30b932. The diff between them touches only `cap_stage_a` (1,000,000 to 15,000,000), `cap_stage_ab` (2,000,000 to 30,000,000), and adds `cap_amendment`. Token, miss, blocked and upkeep numbers are unchanged. The run total, 8,868,059, is under the raised cap. D173 itself changes no threshold or cap.

## 3. Gate findings recorded

Both non-blocking findings are in record-in-progress.json deviations (3a67bef): the `rm t.tmp` in wiki-red-1 (with the card 238 handling: add a no-delete line to the wrapper, scan headless runs) and the 12 untracked `*.session.jsonl` files (left untracked, with the reason). The gate's other notes (facilitator context growth and upkeep unmeasured; N=1 per cell) are in D173 and carried to card 238.

## 4. Card 238 carries what motion A ordered

board/238 (Draft, unranked, depends on card 237) has: thin-slice limits on the slice route (done check 1); a staleness check that fails on a fixture (1); upkeep measured on a replay against the committed budget (2); the slice cell re-run under the same method with an independent recompute and zero wiki misses or the result recorded (3). Invariants: no threshold or cap changes, the reviewer recomputes. "Not in this card": stage B and any build, as D173 says. One small gap: motion A says to name the follow-up as ranked by the team, and the card is unranked until the next re-rank; that is as ordered.

## 5. Token and usage evidence

What exists:
- Run usage: 12 provider JSONs in .mob/records/237/trial/runs/ (committed, with 12 tools.jsonl), each with input, cache read, cache creation and output tokens. They sum to 8,868,059 counted tokens (baseline and wiki medians above). This is the run evidence, and it is complete.
- Seat usage in record-in-progress.json `token_usage`: one row only, ctx-1 pm-ready (Copilot CLI: 660.0k in, 617.0k cached, 9.8k out).

What does not exist: subagent or seat token figures in the turn records. I grepped all turn, proposal, ruling, assessment and gate records for token counts; the only figures are the run totals, the card 234 baselines and the thresholds. No figure for ctx-2 to ctx-6 or for the window and decider polling seats is in the repo. The independent reviewer's and my own seat usage are not recorded either.

The closing record needs those from the facilitator, taken from the seat results (the Agent task usage lines) and the Copilot usage lines. I will not write numbers I cannot read. This is carried to the facilitator and does not change the decision, so it does not withhold sign-off; the card must not close with an empty `token_usage` for the seats.

## Carried, not failed

- Seat token usage for ctx-2 to ctx-6 and the poll windows: facilitator fills `token_usage`.
- Facilitator context growth: unmeasured; owned by card 238.
- ctx-6 command scan: for the facilitator on return.
- Role mapping: green = trial run, no product code (as the gate says); do not claim a TDD green for ctx-5.
