# Card 237 retro input (facilitator; facts only)

Card: evaluate whether an indexed, just-in-time process wiki solves the context problem (D172). Outcome: Decider D173, motion A RESHAPE (follow-up card 238), motion B retro-end wiki review as a three-retro experiment on the reshaped wiki.

## Flow metrics (UTC, from commits)
- Card filed and ranked first: 11:23 to 11:25 (bfe366b, 5a7f6bd, re-rank 16). Record carried to 18:26 (88b3148): about 7 hours.
- Card 237 commits: 80. MOB.md regeneration commits in the same span: 22 (the pre-push check refuses at 4 non-MOB commits).
- Steps in order: task-force window; slice and amendment (13606 bytes); pre-trial red (744e9e3); thresholds (139f607) with decider confirmation; freeze (01b3058); key (b0014b1); key-secrecy window; run 1 (cap fired); cap-fired window; key-probe ruling (327d635); cap raise (c30b932); amendment-ordering window and eight red/green cycles (08aff53 to 3536229); runs 2 to 12; independent recompute (96658ea); ledger correction (ca56511); assessment (d2ad79f); Decider D173; gate SHIP (ae02695); sign-off (c13faa8).
- Unplanned stretch: the stage-A cap fired after one run, which added two windows, a ruling, a cap raise and eight checker cycles (about 14:20 to 16:24) before run 2.

## Practice and experiment metrics (from the record and the trial files)
- Trial: 12 runs, 8,868,059 counted tokens of a 15,000,000 cap; median delta 41.74% in favour of the wiki; 5 of 6 roles at or over 20%; misses wiki 1, baseline 0; 0 runs over 600 s; upkeep and facilitator context growth unmeasured.
- Seat turns with reported figures: 16 seat turns, 1,562,733 subagent tokens (record token_usage); slice-phase seats and decision-window seats not summed (gaps named in the record).
- Decision windows: five (237-taskforce, 237-slice, 237-key-secrecy, 237-cap-fired, 237-amendment-ordering) plus the decider threshold confirmation and the two-round Decider D173. Each window took about 10 to 14 minutes with voters run one at a time.
- Copilot: every Copilot seat placed by the decision-seat gate returned no reply, in every window and vote of the card (card 229). Codex, Grok and agy unavailable at every probe; the Codex probe hung once (13:53) past its timeout.

## Adjustments to review first (D155)
1. D171, cite-a-point votes (experiment, next 3 vote windows; this card ran more than 3). Unread votes caught: slice round 2 (two: a 2-word cite; a cite not in the proposal), threshold confirmation (one: cited the slice), cap-fired round 1 (one: cited the slice). One seat was replaced after two unread replies (it cited a commit subject). Window time is in the flow metrics.
2. D168, slice size cap: the slice was 13606 bytes, within the cap; the cap-fired proposal 8013 and the ordering proposal 8623.
3. D167 hostile-input lists: every seat turn record carried one; the amendment-ordering work turned its list into one red test per case.
4. D169 push on every commit with the detector: no commit sat unpushed more than a few minutes; two refusals (a closed-list category; MOB.md stale) were fixed with new commits.

## Observations for this retro
- The cap estimate took card 234's single-turn figure as a per-run cost; a headless role run was about 13 turns (architect proposal, c91b599). The cap did its job: the run stopped at the rule.
- The runner learned key names by probing the checker; ruled harmless with a guard. A pilot run deleted its own temp file in its clone (`rm t.tmp`): the no-delete rule did not reach headless runs.
- A seat committed the cap raise though its brief said to stop if the ordering check failed; the check was masked by a missing file (fixed in cycle 1).
- The architect pushed a one-line fix while a vote window ran; no vote was voided, but the rule "no commit while a voter runs" binds seats too, not only the facilitator.
- MOB.md staleness forced 22 extra commits.

## Queued inputs to weigh (.mob/records/notes/2026-11-01-queued-retro-and-template-inputs.md)
- Parallel voters (Dispatch): windows ran about 10 to 14 minutes each on this card.
- Pairing (product owner): driver/navigator, each model through its own CLI on its quota.
- Template path: no defaults promotion; D173 orders no build (D172).

## Standing section: the feedback-loop lens (D165)
Each voice names, for the 7 loops, who or what gives the feedback, its latency, what the team does with it, and MISSING where none exists.
