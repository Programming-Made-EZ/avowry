# Card 237 proposal: the stage-A cap fired after one run (architect, ctx-2)

Intent (D165): give the team's objection window a diagnosis and a costed choice about what to do now that the stage-A cap fired after one run, so the Decider decides on facts. This is a proposal; it decides nothing and changes no trial file.

Assumes (D166): the card 234 files and the one run file are what they say they are; probe: I read the 234 usage json, the run json and the ledger, and the arithmetic below reproduces their numbers. I did not read the miss key or the QA turn. I do not know whether the real limit is a usage quota or dollars; the proposal states both.

## 1. Diagnosis: the estimate was wrong, the rule was not

What card 234 measured. 69701 and 49032 come from two single `claude -p` calls (.mob/records/234/usage/measure-qa-red-full.json and measure-qa-red-brief.json, line 1 of each; named in .mob/records/234/signoff-ctx8.md:12 and :27). Both have num_turns 1, one iteration, no tool calls:
- full: 2 + 69003 cache creation + 696 output = 69701.
- brief: 2 + 30341 cache read + 18311 cache creation + 378 output = 49032.
So 69701 is one question answered in one model turn.

What the trial run is. .mob/records/237/trial/runs/baseline-ready-1.json has num_turns 13 and 16 tool calls (turn-pilot-delivery.md:29, :40). Its usage block sums every turn: 22 fresh + 54841 creation + 788896 cache read = 843759, plus 9513 output = 853272. The `iterations` array holds only the last turn (82837 cache read + 526 + 2 + 242), so it looks like a 234-sized call and hides the sum.

Same unit, missing multiplier. Average input per turn is 843759 / 13 = 64904, close to 234's 69005 for one turn. The per-turn context estimate was right. The turn count was left out: 853272 / 69701 = 12.2, about the 13 turns. A role task that reads the agreement, reads files and writes a record is many turns, not one.

Is the counting rule wrong? No. The slice rule (turn-slice-architect.md:35-39) counts input_total = input + cache read + cache creation, plus output. That is the treatment 234 used. The rule is applied identically to both arms, and the decision metric is a relative delta (thresholds.json:5, 20 percent in 4 of 6 roles), which is scale-free. A rule that counts each turn's re-read is a defensible proxy for how much context a seat carries. One weakness: cache reads cost far less than creation (this run cost $0.393 at list, total_cost_usd), so counted tokens overstate money. That makes it a poor budget unit, not a poor comparison unit.

Where the error sits: turn-slice-architect.md:150 (Basis: 234's pair "per seat turn") and :156 (cap: 6 x 69701 + 6 x 49032 = 712,398, rounded to 1,000,000). I wrote both. I carried a per-turn figure into a per-run cap. signoff-ctx8.md:29 had already warned that Copilot figures "include every agentic step's re-read of context", the same effect, and I did not apply it to the Claude runs.

Knock-ons from the same cause: the upkeep budget (:155, 12400 per card) came from 6 x 20669 as one turn per seat, so it is conservative. The 29.7 percent basis (:150, :152) is one pair with unequal caching (signoff-ctx8.md:27), so it is thin; but the thresholds are relative and this run says nothing against them.

The cap did what it was written to do (thresholds.json:21) and the delivery lead stopped correctly (turn-pilot-delivery.md:22). The failure is my estimate, caught by the first probe, which is what the probe was for.

## 2. Options

Facts for costing. One run (role ready, possibly the lightest) was 853272 counted tokens, 87 s, $0.39 list. Twelve runs that size are about 10.2M counted tokens and about $4.70 list. Heavier roles (red, green) likely take more turns; I have no data, so I add 50 percent headroom.

(a) Reject now as inconclusive. Cost: nothing more. Result: no wiki-vs-baseline data; the committed rule (thresholds.json:21) already bars adopt, so the practical outcome is "do nothing", with the 234 wiki left an unadopted prototype and the context problem unmeasured. The 853272 spent buys only a cost figure. Cheapest and honest, but it ends the question for the wrong reason: our estimate was bad, not the wiki.

(b) Amend the rule, re-estimate, re-run stage A from scratch with a new freeze and key. Cost: 12 runs (about 10M counted tokens), plus a new freeze, a new key by a non-runner, new checker fixtures, reviewer and Decider time, and discarding the one run. Concern: the rule needs no amendment (section 1). Changing it after seeing one baseline run is what the adopted condition (numbers set before the data) warns against; the only reason to change it is to make counts smaller (for example dropping cache reads), chosen having seen how large the baseline was. If b is chosen, keep the rule and change only the cap, which collapses b into d with a larger bill. One benefit survives: a fresh key clears the delivery lead's deviation (key names learned by a checker membership probe, turn-pilot-delivery.md:9 and :34), at the price of rerunning everything.

(c) Close as cannot-afford and file a cheaper evaluation. Cost: a new card (slice, freeze, key) and the same unanswered question. The evidence says it is not unaffordable: about $5 to $8 at list for the whole stage, 87 s a run. If the real limit is a usage quota, I do not know it; the Decider should state it before this option is chosen. Without that, "cannot afford" is not supported.

(d) Keep the freeze, key, rule and delta thresholds. The Decider commits a new cap in thresholds.json before run 2, and the 11 remaining runs go ahead, with baseline-ready-1 as row 1. Proposed numbers: cap_stage_a 15,000,000 (12 x 853272 = 10.24M, times about 1.5), cap_stage_ab 30,000,000, driver stop rule unchanged (total plus largest run so far). Cost: about 10M to 15M counted tokens, $5 to $10 list, about 20 minutes of runs, one committed amendment. Honesty about the condition: this does change a committed number after one data point. Two reasons it is weaker than b: (1) what was seen is the cost of one arm, not an arm-versus-arm outcome or a miss score, so nothing about whether the wiki wins has been seen; (2) the new cap is sized so it cannot bind on a normal outcome, so it cannot steer the result. It does override thresholds.json:21 ("Decider may not adopt" on reaching the cap), so it needs an explicit Decider decision recording the reason and the old and new numbers, landed before run 2. Keep it clean: raise only, change no other field, one commit.

(e) Do nothing: leave the cap-fired state. Same result as (a) with no record; 11 not-run rows sit unexplained. Not recommended.

## 3. Recommendation

Choose (d), with (b) as the fallback in one case.

Reasons. The rule is sound and the thresholds are relative, so the data already collected stays valid; the error was a unit slip in my estimate, not a flaw in the experiment. Rerunning everything buys little: the one thing a restart fixes is the key-name probe deviation, which the reviewer can judge on its own. The cost is a few dollars at list. (a) and (c) give up the question for a reason the evidence does not support.

Conditions on (d):
1. The Decider records the cap change as an amendment before run 2 (old 1,000,000, new 15,000,000; stage AB 30,000,000), stating it was sized from one baseline cost and nothing else.
2. The Decider or reviewer rules on the key-name probe deviation first. If it is judged to compromise the key, switch to (b) with the rule and thresholds unchanged and a new key, not (d).
3. If the real limit is a usage quota below roughly 10M counted tokens, say so before choosing; (d) is then not affordable and (a) becomes the honest pick.
4. Add one line to the next retro: caps for headless runs are estimated from a measured full run, not from a per-turn figure.

Open questions for the window: what is the budget unit and its limit; does the reviewer accept the probe deviation; does anyone want the 29.7 percent basis re-derived from the 12 runs rather than trusted.

Size: 8013 bytes
