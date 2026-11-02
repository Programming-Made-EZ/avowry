# Retro pick proposal (agile coach seat): a breaker for decision-seat rotation

Card 211 retro, 2026-11-02. Proposer: agile coach seat. For a Decider. Sources: docs/retros/card-211-2026-11-02.md section 5; voices in .mob/records/211/retro-voices.json; facts in .mob/records/211/retro-input.md; the sign-off .mob/records/211/signoff-ctx6.md; the windows .mob/records/decisions/211-taskforce.json and 211-slice.json; the queued inputs .mob/records/notes/2026-11-01-queued-retro-and-template-inputs.md (rotation observation) and the open proposal-labelled issues (242 point 2).

Decider (number set at recording), the card 211 retro process pick, as an experiment

PICK: decision-seat rotation gets a breaker. A model trips when two decision-seat replies in a row (any windows) are empty or carry no vote word, or when its CLI reports its quota reached (one event is enough). A tripped model is placed in no decision seat; a quota trip also keeps it out of every other rotation-placed seat, including retro voices. It returns when a probe vote parses. Proposed by five of seven voices (product manager, architect, developer, audit lens, coach; issue 242 point 2, the queued rotation input). A per-model budget, a stricter vote-cite rule, a pin of the final-state review to HEAD and the decision-seat fixes (cards 229, 270) are deferred or already cards, not rejected.

User benefit: the product owner gets decision windows that spend their minutes on votes and not on seats known to be silent. Card 211's two windows placed 19 empty Grok, Copilot and agy attempts (9 and 10); in the slice window about 2 minutes 10 seconds of each 3.5 minute seat went on the two empties before Claude answered (from the attempt start times).

Why this one: it is the largest recurring waste on this card, and the one the most voices named.
- Window 211-taskforce 19:19 to 19:42 UTC (22.5 minutes): 9 empty attempts (Grok 4, Copilot 4, agy 1), 5 counted votes (agy 1, Claude 4). Window 211-slice 19:52 to 20:10 (17.8 minutes): 10 empty (Grok 5, Copilot 5), 5 counted, all Claude. Card 210's windows: Copilot 0 of 6 placed seats. The Decider: all six votes Claude.
- Rotation burned agy: about 28 calls in 21 hours until "Individual quota reached" at 19:44 UTC (resets in about 145 hours), because least-recently-used rotation re-seated onto agy each time Grok or Copilot returned nothing. The architect seat had to be re-seated on Copilot.
- Grok and Copilot are silent only on the decision-seat path. In this retro Grok (3) and Copilot (2) answered as voices, and Copilot's CLI answered ready, slice, review and gate. So the breaker is per path: it must not take the models out of the seats where they work.
- The breaker cannot create model diversity; it stops paying for its absence. Diversity returns when cards 229 and 270 land and a probe parses.

HYPOTHESIS: We believe that a breaker on decision-seat rotation removes the empty attempts a window pays for, and so shortens windows, without hiding a fixed model from the rotation for longer than one probe interval. We will know when empty or voteless counted attempts fall from 9 and 10 per window to 0, probe empties stay at most one per tripped model per day, and window minutes do not rise.

RULE UNDER TRIAL:
C1. Trip. Counted per model over seats placed through decider.py (task-force, slice, re-rank, Decider and pick windows). Empty is no text after the CLI call; voteless is no first-line vote word the window accepts. A reply with a vote word is a reply whatever the vote: a No or OBJECTION never trips. Two in a row trips; "quota reached" in the CLI text trips at once.
C2. Effect. Rotation skips a tripped model; the seat goes to the next least-recently-used model not tripped. A quota trip applies to every rotation-placed seat, including retro voices; an empty trip applies to decision seats only. Seats not placed by rotation (ready, review, gate, sign-off on the Copilot CLI) are unchanged.
C3. Return. A tripped model gets one probe vote, before a window opens, at most once per 24 hours, or when a fix card for it lands (229, 270). The probe is a fixed one-line proposal sent through the same decider.py path, not counted as a vote. The model returns when its first line parses. A quota trip is probed only after the reset the CLI message names (agy: about 2026-11-08).
C4. Record. The window's seat-use lines carry one "Breaker:" line per trip or return: model, trigger, time, and for a quota trip the calls since the model's last reset, counted from the seat-use lines. That number is what a per-model budget would need.
C5. Starting state. Grok and Copilot tripped for decision seats (the record above; cards 229, 270). agy tripped for quota until its reset. The facilitator applies C1 to C4 by hand during the trial; the decider.py change is a follow-up card.
C6. Not changed: D168, D171, D174, D175, the counted-vote rules, seat roles, sign-off and the auditor. No new seat, meeting or file.

METRIC: empty or voteless counted attempts per decision window. Baseline: 9 (211-taskforce), 10 (211-slice), 6 over card 210's two windows. Target: 0 in each of the next 3 windows. A probe's empty reply is counted apart, at most one per tripped model per 24 hours. Context figure, not a target: minutes per window (22.5, 17.8; D174's parallel launch, card 265, is a separate lever).

COUNTER-METRICS: (1) share of counted votes from a model other than Claude (card 211: 1 of 10 window votes, Decider 0 of 6; card 210: 5 of 10 from agy). It will read near 0 while all three are tripped; it is a reading, set against model diversity, not a target. (2) False trips: a tripped model whose first probe parses and whose earlier empties were transient. (3) Days between a fix landing (229, 270) and the model's return. (4) Calls per day to a model that returns after a quota trip (agy), to feed the budget candidate. (5) Window minutes must not rise.

TRIAL SCOPE: the next 3 decision windows after this Decider is recorded, of any category. If every model is untripped by then, there is no reading and the trial retro says so. The trial retro decides keep, revise or drop by Decider.

ACCOUNTABILITY CHECK: at sign-off, the sign-off seat checks each window record: no attempt on a model tripped at that time unless a "Breaker:" return line precedes it, and a probe line for each probe. The auditor re-checks the same from the window json; a window failing it counts as not run.

NO BLUR: fixes to cards 229 and 270 are credited to those cards, not to this pick. Window minutes also move with D174 and card 265; they are reported, not scored. D171's cite counts are scored by D171.

COST (issue 245): one Breaker line per trip or return, and at most one probe per tripped model per day (one short prompt; an empty one costs about 1 to 2 minutes). No extra seat, file or meeting. If the cost shows, the probe is the first thing the retro cuts.

REVERSIBILITY: two-way door. Stop writing the lines and place seats as before; no shared state, checker or earlier D-record changes.

VOTING RULE: reply first line Yes, Support-it or No, then P1: to Pn: one line each (one per proposal point). A No names ONE fixed condition (CONDITION: ...). A reasonable, reversible condition is adopted and the proposal re-polled; a later round may not add new demands. Only an irreconcilable purpose or model conflict escalates. Independent fresh-seat re-run before recording. Each reply ends with one line "CITE: <point>" as D171 C1 says.

Deferred, not rejected:
- A per-model budget (queued input; developer, coach, decider, QA voices): no quota size is known, only 28 calls in 21 hours for one model. C4 records the number at the next quota trip; the budget is the next candidate.
- Heavy retro voices off quota-limited models: covered after exhaustion by C2; a prospective rule needs the budget.
- A vote must cite what the voter read (Dispatch; decider voice): a standing-rule change for its own Decider, separate from D168 and D171.
- The final-state review pinned to HEAD, with its resolved model id in the record (QA, decider, audit lens, sign-off): a card.
- Cards 229 and 270, safe parallel windows (265), the other proposal issues: already cards or for the re-rank.
Size: 8203 bytes
