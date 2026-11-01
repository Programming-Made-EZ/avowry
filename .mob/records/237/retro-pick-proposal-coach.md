# Retro pick proposal (agile coach seat): parallel voter dispatch for vote windows

Card 237 retro, 2026-11-01. Proposer: agile coach seat. For a Decider. Sources: docs/retros/card-237-2026-11-01.md section 5; voices in .mob/records/237/retro-voices.json; facts in .mob/records/237/retro-input.md; Dispatch's queued proposal in .mob/records/notes/2026-11-01-queued-retro-and-template-inputs.md.

Decider (number set at recording), the card 237 retro process pick, as an experiment

PICK: parallel voter dispatch for every vote window. The facilitator probes once, assigns the whole rotation up front, writes every seat-use line in one commit, then launches all voters of the window at the same time. (Proposed by the QA engineer and agile coach voices; the architect and decider voices named the serial windows as a team cost; Dispatch queued it.) The cap estimated from a measured pilot run, the git-checkable brief preflight and live-card context measurement are deferred, not rejected.

Why: since the decision-seat gate landed, voters run one at a time. On card 237 the five decision windows each took about 10 to 14 minutes, and the threshold confirmation and the two rounds of Decider D173 ran the same way. The retro's own seven voices started one after another from 18:28:59 to 18:37:52, about 9 minutes before the last one began. Before the gate a window took about 3 minutes; on card 234 the task-force and slice windows took 36 and 45 minutes. The slowdown is a side effect of the gate, not a choice. Dispatch expects parallel launch to make votes more independent and to cut same-checkout write collisions (card 230); card 237 recorded one overlap of two decision runs on the shared seat-use history, which caused no refusal but could.

Why this one and not the others: it is the only candidate with a measured baseline on every window of the card, a before-the-gate figure to compare with, and counter-metrics that already exist in the record. D171 named it as the next pick after the cite-a-point trial retro; that trial is now complete, so the blur that held it back (window minutes were D171's counter-metric) no longer applies. The cap estimate cost one stretch of about 2 hours (about 14:20 to 16:24) on one card, the cap did its job, and the measured figure now exists (8,868,059 tokens over 12 runs, about 739,000 per run), so card 238's slice can use it without a new rule. The brief preflight bundles three levers (cap, cite check, no-commit marker), and the D157 and D158 lesson is one change per trial. Live-card context measurement is already part of card 238 (upkeep and facilitator context growth are in D173 motion A).

HYPOTHESIS: We believe probing once, assigning the whole rotation up front, writing the seat-use lines in one commit and launching all voters in parallel cuts the time of a vote window back toward 3 minutes without costing fairness or independence. We will know when the median minutes per vote window fall from 10 to 14 (card 237) to 5 or fewer over the trial, with no least-recently-used violation.

RULE UNDER TRIAL:
C1. Before a vote window opens, the facilitator checks that the Codex, Copilot, Grok and agy probes are under 60 minutes old (the standing rule, F-040) and re-probes first if any is older. Probe evidence is committed as today.
C2. The facilitator assigns every voter of the window in least-recently-used order in one step, and writes all the seat-use lines together with the proposal file in one window commit. A seat that is disabled or unknown at that point is skipped as today; the skip is recorded in that commit.
C3. After the window commit, all voters are launched at the same time, each in its own fresh context through the decision-seat gate. No voter sees another voter's reply. The existing per-seat timeout stays; a seat that returns nothing by the timeout is no vote. The window closes when every seat has replied or timed out.
C4. D171 stays as it is, whatever its trial verdict: each vote still carries its CITE line, and an unread vote is re-asked or replaced as D171 C3 says. Re-asks run in parallel as a second batch; a replacement seat is the next in least-recently-used order and its seat-use line goes in one follow-up commit.
C5. The rule "no commit while a voter runs" stands and binds seats as well as the facilitator (card 237 observation). The launcher must not write the shared seat-use history from two runs at the same time; if it cannot be made safe, that window falls back to serial launch and the trial records it as a fallback, not as a pass.
C6. This is not D171's cite-a-point rule, D168's slice size cap or D167's hostile-input list. It changes only when and how voters start. The proposal text, the voting rule, the number of voters and the quorum are unchanged. No new file, seat or meeting.
C7. Copilot seats placed by the gate that return no reply (card 229) are handled as today: no vote, replaced in least-recently-used order. This pick does not try to fix card 229.

METRIC: minutes per vote window, from the window commit to the last counted vote, taken from the votes file timestamps and the commit times (card 237: about 10 to 14 each; card 234: 36 and 45; before the gate: about 3). Target: median 5 or fewer.

COUNTER-METRICS: least-recently-used violations per window (target 0, from the seat-use lines); stale-probe launches per window (a probe over 60 minutes old; target 0); unread or uncited votes per window (must not rise above the D171 rate; card 237: 4 seats with an unread first reply, 5 unread replies, across five windows, the threshold confirmation and D173); same-checkout write collisions or gate refusals per window (card 237: 1 overlap, 0 refusals); fallbacks to serial launch. Evidence, not targets, except the first two.

TRIAL SCOPE: the next 3 vote windows of any kind (task-force, slice or Decider; a re-run counts with its window), the window of D150 to D158 and D165 to D171. The trial retro decides keep, revise or drop by Decider.

ACCOUNTABILITY CHECK: per window, git shows one window commit that holds the proposal and every seat-use line before the first voter result, and the votes file shows every seat's start time within a few seconds of the others. A window launched one voter at a time, or with seat-use lines written after a voter started, counts as not run; the auditor tick may FAIL it. The checker is the reviewer at review, re-checked by the auditor tick.

NO BLUR: D157, D158, D165 to D168 and D171 keep their own metrics and cards. Retros score each on its own record: D171 from votes CITE lines, this pick from window minutes and the seat-use commits. A fall in minutes is credited here; a fall in unread votes stays with D171.

BASELINE (card 237): five decision windows (237-taskforce, 237-slice, 237-key-secrecy, 237-cap-fired, 237-amendment-ordering) at about 10 to 14 minutes each with voters one at a time; the decider threshold confirmation and the two-round D173 on the same footing; one overlap of two decision runs on the shared seat-use history; Copilot returned no reply in every window (card 229). Card 234: 36 and 45 minutes. Before the gate: about 3.

REVERSIBILITY: two-way door. Go back to launching voters one at a time; no shared state, checker or earlier D-record changes. Votes already counted stay as recorded.

VOTING RULE: reply first line Yes, Support-it or No, then P1: to Pn: one line each (one per proposal point). A No names ONE fixed condition (CONDITION: ...). A reasonable, reversible condition is adopted and the proposal re-polled; a later round may not add new demands. Only an irreconcilable purpose or model conflict escalates. Independent fresh-seat re-run before recording. Each reply ends with one line "CITE: <point>" as D171 C1 says.

Size: 7736 bytes
