# Card 237 assessment and Decider proposal: product manager (ctx-1)

Probe: 2026-11-01 17:01 UTC. Sources: the independent reviewer's rule-by-rule recompute, trial/thresholds.json, ledger.json (now carrying the reviewer's grades and aggregates), recompute.json, the architect's slice and its cap proposal, D172 and its retro-end review proposal, record-in-progress.json. No new numbers are computed here; every result is the reviewer's. The only sums I add are in the cost lines of section 2, from figures given in the architect's records.

Intent (D165): give the team and the Decider one assessment of whether the indexed, just-in-time process wiki solves the problem (tokens per seat, facilitator context growth, messages blocked by long turns), which outcomes the committed thresholds permit, and the exact text to vote on. This is a proposal; it decides nothing and moves no threshold.

Assumes (D166): the reviewer's recompute is the accepted gate and the committed thresholds are read as written, including the cap amendment; probe: I compared the reviewer's table with the ledger and the thresholds file, and the ledger now holds the same grades and medians (wiki-slice-1 misses 1; medians 911895.5 and 531242.5; delta 41.74%). I did not recompute any run, and I did not read the miss key.

## 1. Results by rule (reviewer's numbers)

| Rule (committed threshold) | Result | What the rule implies |
|---|---|---|
| Token delta (adopt: wiki median at least 20% below baseline in at least 4 of 6 roles; reshape 5% to under 20%; reject under 5%) | Median delta 41.74% (baseline 911895.5, wiki 531242.5 counted tokens). Per role: ready 26.89%, slice 57.66%, red 38.82%, green 17.38%, ship 60.26%, signoff 48.98%. Five of six roles at or over 20%. | Adopt side. Not in the ambiguous band. |
| Misses (adopt: wiki at or under baseline and 0 wiki misses of 6; reject: wiki over baseline by 2 or more) | Baseline 0 of 6, wiki 1 of 6 (the slice run: no numeric commit-size limit in the plan). Excess of exactly 1. Cause: the 15-file, 400-line limit is on the green role page only, not on the slice route. A real content gap in the wiki, not run noise. | Not adopt (needs 0). Not reject (needs excess of 2). Exactly 1 is the slice's ambiguous case. |
| Blocked messages (T = 600 s; adopt: wiki at or under baseline; reject: wiki over by 2 or more) | 0 and 0; longest run 124.6 s. | Adopt side, but vacuous: the runs were headless one-shots with nothing queued behind them, so it shows only that no run came near T. It is no evidence about the wiki. |
| Upkeep (at most 12400 tokens per card and 10 minutes added retro time; over budget forces reshape) | Not measured by any run. | No outcome. Adopt cannot be shown to meet it, and the adopt rule needs it. |
| Facilitator context growth (no threshold) | Not measured in stage A. | No outcome. This is one of the three measures the product owner named, so the problem statement is only partly tested. |

Cost basis: stage A counted 8,868,059 tokens against the 15,000,000 cap (reviewer). Caution on units: counted tokens sum every turn's cache re-read, so they move with turn count (the green wiki run took 24 turns against 19 and shows the smallest saving). That is a fair comparison unit, since both arms use it, but a poor money unit.

Limits, stated plainly:
- N = 1 per cell. No variance estimate exists; one run per role per arm.
- One source card (227) supplies the task frame, so the roles are tested on one kind of work.
- Headless runs: no queue, no interruptions, no facilitator. That is why blocked messages are vacuous and facilitator context growth is unmeasured. Both of those measures need a live setting to mean anything.
- Graders read plans, so a pass means the named element is present, not that the plan is good.
- The key predicted no wiki miss; that prediction failed for the slice cell. The reviewer graded as written. The key guard found no read of the key.
- The token saving agrees with card 234's earlier single pair (29.7%), but that pair is also thin.

## 2. Alternatives and cost

Which do the committed thresholds permit? Adopt is barred twice: one wiki miss, and upkeep is unmeasured. Reject is barred: the excess is 1, not 2, and tokens are far above the 5% line. That leaves reshape or stage B (both inside the rules), and doing nothing (always available as inaction, since the baseline stays the default when nothing passes).

| Option | Cost | Permitted by the thresholds? | Assessment |
|---|---|---|---|
| (i) Adopt as is | Nothing new now; the build and upkeep would follow. | No. The misses rule (0 wiki misses needed) and upkeep (unmeasured) both block it. | Would adopt a wiki with a known routing gap and an unknown running cost. |
| (ii) Reshape: put the thin-slice numbers (15 countable files, 400 changed lines) on the slice route, and measure upkeep | A small page edit plus a replay measurement of upkeep (the reviewer lists ways: replay recent retro-changed rules into the wiki, count rule changes per card from history, or run a staleness diff). Then re-run at least the slice cell. Scale: the slice runs counted 775,649 (baseline) and 328,391 (wiki). | Yes. The thresholds give reshape when the improvement is real and another measure fails; the cause here is specific and fixable. | The cheapest route to a clean answer on the one failing rule. |
| (iii) Stage B: 12 more runs inside the 30,000,000 A+B cap | About 8.9M counted tokens (estimate given for this assessment). With stage A that is about 17.8M, under the 30M cap. | Yes. The slice names an excess of exactly 1 as ambiguous, and ambiguous goes to stage B. | Gives N = 2 but tests the same wiki with the same gap, so it would probably repeat the slice miss. It would still not measure upkeep, facilitator context growth or blocked messages, because the runs stay headless. About 8.9M tokens to re-confirm a cause already known. |
| (iv) Reject | Nothing. | No. An excess of 1 is under the reject line and the token delta is far above it. | Throws away a 41.74% median saving over one content gap. |
| (v) Do nothing, keep the prototype as is | Nothing new. The prototype stays unadopted; the context problem stays unaddressed. | Yes, as inaction. | Leaves the question open and the known gap in place. Not a verdict. |

## 3. Recommended outcome: reshape

Recommend the Decider rule RESHAPE, as the committed thresholds define it: the wiki improves a core measure (tokens) but fails another (misses, one miss with a known cause), and upkeep is unproven so adopt is barred either way.

Why reshape over stage B: both are rule-permitted. Stage B costs about 8.9M tokens, cannot clear the miss rule unless the wiki changes, and cannot test the unmeasured rules because its runs are headless. Reshape fixes the specific cause first and measures what stage A could not. If the Decider prefers the slice's stage B route, that is also inside the rules; I rank it second.

Required changes the ruling should name:
1. Put the thin-slice limits on the slice route (the slice role page and the thin-slices rule page).
2. Measure upkeep against the committed budget (12400 tokens per card, 10 minutes added retro time) before any adopt decision.
3. Measure facilitator context growth and blocked messages in a live card, not a headless one-shot, since the stage A design cannot see either.
4. Add a staleness check that compares the agreement's rules with the wiki routes. It would have caught the slice gap and is the natural tool for motion B.

Follow-up card (its own card, ranked by the team, per D172): "Reshape the process wiki and measure upkeep". Scope: items 1, 2 and 4, and a re-run of the slice cell (both arms), with a live-card measurement of context growth and blocked messages if the facilitator can supply it. No threshold moves; the new run is graded against the same committed numbers with the same independent recompute. A build as a mandate is in neither this card nor that one; adoption would need a later card after the reshaped trial passes. The reshaped trial is a new arm, so the Decider should require a fresh miss key from a non-runner seat.

## 4. Retro-end wiki review proposal (product owner proposal, D172): recommend reshape

Proposal as written: at the end of every retro, review the wiki and record additions, updates and removals, with evidence (a wiki commit or an explicit "no changes" note). Metric: wiki staleness (rules changed in retros but not reflected in the wiki). Counter-metric: added retro time.

For it: the one wiki miss is a staleness-type defect (a rule's numbers sit on one page and are absent where the role looks). Some review would catch that.

Why not adopt as written: the wiki is an unadopted prototype, and a standing step every retro costs retro time (the counter-metric) for an artifact the team may not keep. The committed upkeep budget already caps added retro time at 10 minutes, but that line only applies if the wiki is adopted.

Recommended shape (reshape):
- Run it as a time-boxed experiment on the reshaped wiki, not on the prototype. It starts only if motion A passes, and covers a fixed number of retros the Decider names (I propose the next three; a proposal, not a committed threshold).
- Prefer a staleness check over a manual read: the retro lists rules changed, the check lists those not reflected in the wiki, and the retro records the result or "no changes" with the evidence.
- Metric: wiki staleness, counted per retro as rules changed in the retro but not reflected in the wiki. Counter-metric: added retro time, held to the existing 10-minute line. If added time passes the line, or the check finds nothing across the experiment, the experiment ends and the review lapses.
- Goodhart guard: staleness is counted by the check from the rule diff, not by the seat doing the review, so a seat cannot lower the count by listing fewer rule changes.

If motion A fails, or the Decider chooses reject or do-nothing, motion B lapses with it.

## 5. Text to vote on

Motion A (evaluation outcome). "Card 237 outcome: RESHAPE. The stage A result is accepted as the independent reviewer recomputed it (median token delta 41.74%, five of six roles at or over 20%; one wiki miss against zero baseline misses, caused by the thin-slice limits being absent from the slice route; blocked messages vacuous at 0 and 0; upkeep and facilitator context growth not measured). Adopt is not permitted by the committed thresholds (one wiki miss; upkeep unmeasured) and reject is not (the excess is 1, not 2). No threshold or cap changes. Stage B is not run now. The team orders one follow-up card, ranked by the team, to put the thin-slice limits on the slice route, measure upkeep against the committed budget, add a staleness check, and re-run the slice cell under the same thresholds and an independent recompute. No build is ordered by this motion."

Motion B (retro-end wiki review). "Retro-end wiki review: RESHAPE and adopt as a time-boxed experiment, only if motion A passes. It runs on the reshaped wiki for a number of retros the Decider names (proposed: the next three). At each retro-end the team records the output of a staleness check (rules changed in the retro but not reflected in the wiki) and the wiki commit it caused, or an explicit 'no changes' note. Metric: wiki staleness. Counter-metric: added retro time, held to the committed 10-minute upkeep line. It ends early if the line is passed, and lapses if motion A fails."

Size: 11468 bytes
