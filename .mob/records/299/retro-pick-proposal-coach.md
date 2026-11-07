# Retro pick proposal (agile coach seat): the agreement keeps one Experimental section, and an experiment past its trial is decided at the next retro

Card 299 retro, 2026-11-07. Proposer: agile coach seat. For Decider D199. This also applies the product owner's direction of 2026-11-07 ("Experimental values, principles and practices"). It is a change to TEAM_WORKING_AGREEMENT.md, not an issue.

Sources: docs/retros/card-299-2026-11-07.md (section 6), .mob/records/299/experiments-inventory.md (24 experiment lines; 14 expired by count; none ever decided by a Decider), and the committed patch .mob/records/299/agreement-restructure.diff (candidate text: .mob/records/299/agreement-candidate.md). Voices: architect, decider, audit lens and coach proposed this; QA and the product manager proposed evidence labels (D190 already in trial); the developer proposed a commit check (kept as an individual practice).

PICK, one change in one patch:
P1. A new section "Experimental values, principles and practices (on trial; not yet permanent)" before "4. Project specifics". Each entry states hypothesis, metric with its unit, trial length and start date; an experiment past its trial length is decided at the next retro by Decider (adopt, revise and extend, or drop) and the decision is recorded with its evidence. Sixteen scattered experiment lines move into it with hypothesis, metric and trial filled in (D22, D150 x2, D154, D178, D188, D190, D176, D174, D168, D167, D166, D158, D157, D193, D196).
P2. Six lines are adopted into the permanent text on their evidence (D151 stand-down, D181 lettered red, D184 scope premise, D189 numbers with commands pinned to a commit, D175 cycle cap, D171 cite-a-point).
P3. Three are dropped: D187 (targeted re-ask, its lighter path is gone), the intent line before the first red (D165, to be re-added if a retro shows a need), and the D153 lighter path, whose paragraph is replaced by "Decisions take the full Decider protocol" (the Decider already runs that way).
P4. The patch also fixes a stray wrapped line (the Definition of Ready bullet had been split by an earlier insertion).
Not changed: every other value, principle, practice and standing rule (reviewed at this retro: kept, no change, except Metrics are evidence gaining "every trial states a metric and a unit"); the Decider protocol text; D193 and D196 stay in trial with their counts.

HYPOTHESIS: forcing a decision when a trial ends keeps the agreement short enough to read and stops expired experiments lingering. We will know when, at the next three retros, the count of experiment lines past their trial and undecided is 0 and the standing text in the permanent sections is no longer than today's.
METRIC: experiment lines past their trial and undecided (target 0); minutes of retro time spent on them (counter-metric: at most 15).
COUNTER-METRICS: a dropped or adopted line that later costs a defect (any one reopens it); loss of a rule the team still needs.
TRIAL SCOPE: the next three retros. Owner: agile coach.
User benefit: the product owner reads one short agreement; every rule on trial shows what it is testing, and nothing stays on trial by neglect.
