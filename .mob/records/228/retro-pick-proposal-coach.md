# Retro pick proposal (agile coach seat): a written increment cap in every slice

Card 228 retro, 2026-11-02. Proposer: agile coach seat. For a Decider. Sources: docs/retros/card-228-2026-11-02.md section 5; voices in .mob/records/228/retro-voices.json; facts in .mob/records/228/retro-input.md; the sign-off .mob/records/228/signoff-ctx6.md section 5; the gate .mob/records/228/preship-gate.md (cycle table); the in-progress record .mob/records/228/record-in-progress.json.

Decider (number set at recording), the card 228 retro process pick, as an experiment

PICK: every slice carries a written Cap line for each acceptance criterion, and every item of a slice's hostile-input list is labelled SEEN or INSURANCE. A cycle past a criterion's cap needs a scope window adopted before its red starts, not after. (Proposed by the architect voice; the sign-off seat put forward the same pick; the stand-down 1 verdict was "tune" from four of seven voices.) The test-impact lever, the single decider for reversible parameters, the ruling-as-red-test rule, the bypass corpus and the cards for re-rank 18 are deferred or routed elsewhere, not rejected.

User benefit: the product owner and the team learn the maximum cost of a card before it starts, not from the auditor after the fact. Card 228 cost about 13 hours and twelve increments plus a fix, against a three-increment rule and a slice that said one cycle per criterion.

Why this one: it answers the largest measured cost of the card, and the only one the team can change inside the slice.
- The slice (639c5e2, 14,847 bytes) listed hostile forms with no cap. Criterion 1 ran seven cycles (1a to 1g) against a slice of one, from 12:51 to 17:54 local, about 5 hours of the 13. 1g was born green and added guards, not behaviour. Criterion 2 ran three, criterion 3 two, and a fix cycle followed: twelve increments plus a fix against D136's three.
- No cap was written before the overrun. The auditor's FLAG caught the first; the later two were decided at the boundary they hit (window 228-c2-scope, stand-down 1, window 228-gate-fix).
- The forms were never priced. No record counts how many of the forms of 1a to 1f have appeared in a real seat transcript: the gate re-ran the scanner on 25 real transcripts (627 Bash calls) and found no real delete, only false positives. If none, the extra increments were insurance.
- Every voice's view of stand-down 1 points here: tune from the PM-ready, QA, audit-lens and decider voices; keep from the architect, developer and coach. The architect's candidate is this pick; the sign-off asks for the same cap line and the same SEEN/INSURANCE split.
- It extends two rules that exist (D136's increment limit, D167's hostile-input list) and adds no gate, seat, file or meeting (issue 245's test: prefer extending a mechanism).

Why not the others: the test-impact lever (two voices) is a tooling change and the commit-then-suite rule from green 3a already removed the commit wait; the single decider and parallel windows belong to a decider.py card (issue 247) and a standing-rule Decider; the ruling-as-red-test rule rests on one event the gate caught. Details in the retro, section 5.

HYPOTHESIS: We believe that writing the increment cap per criterion into the slice, and labelling each hostile form SEEN or INSURANCE, keeps a card inside the cycles its slice promised and makes extra cycles an explicit decision made before the red, without pushing the risk into later cards. We will know when, over the trial, no card runs a red past its Cap line without a scope window adopted before that red, and the counter-metrics do not rise.

RULE UNDER TRIAL:
C1. The architect's slice carries, under each acceptance criterion, one line "Cap: criterion n, at most k cycles". The k values sum to at most three for the card (D136); a card that needs more is split (D168). A fix cycle after the gate counts against the card's total and is named in the window that adopts it.
C2. A slice with a hostile-input list (D167) labels each item SEEN or INSURANCE. SEEN carries a pointer: a commit, a seat transcript id, an audit or an incident that shows the form. INSURANCE means no real occurrence is known. Forms the cap cannot cover go to a named follow-up row in the same slice, as the card 228 slice did with F1 to F6.
C3. The reviewer who reads the slice (D168) rejects a slice with a criterion that has no Cap line, a Cap line sum over three, or a list item with no label. A slice that fails is re-written; no red starts on it.
C4. A red that would be cycle k+1 or later for its criterion starts only after a scope window (Decider) has adopted it. That window is committed before the red. Its proposal states the cycles used and the hours used, the forms the extra cycle covers with their labels, and what a stop would leave open. A hostile form found during a cycle that is not in the slice goes to the follow-up rows unless such a window adopts it (the rule window 228-c1-scope wrote after the fact).
C5. The red turn record opens with one line "Cycle i of k for criterion n", copied from the Cap line. Reds already name "Criterion: n" in the commit message, so the count can be read from git.
C6. During the trial the facilitator adds C1 to C5 to the architect's and the QA engineer's briefs. Only a keep verdict goes to the mob-defaults template, through the normal template-change path including the template review.
C7. Not changed: D136's other limits (400 lines, 15 files, three criteria), D167's requirement to list hostile inputs, D168's 15360-byte slice cap (the new lines count toward it), D151 stand-downs, the green commit-then-suite rule, D171 and D174. No new meeting, seat, file or gate. This pick does not change what a scan covers or how many forms the scanner must handle.

METRIC: reds past a criterion's Cap line, per card, that start without a scope window committed before them (card 228: criterion 1 had 6 beyond a slice of one, criterion 2 had 2 beyond one; every one was decided after the overrun began or at the boundary it hit). Target: 0. Read from the red commit messages ("Criterion: n"), the slice's Cap lines and the scope window commit times. Context figure, not a target: increments per card (card 228: twelve plus a fix; three allowed).

COUNTER-METRICS: pre-ship gate HOLDs per card (card 228: 1, the ssh-agent bypass); follow-up cards or rows opened per card (card 228: six, F1 to F6, fifteen items); hours from first card commit to sign-off (card 228: about 13); scope windows adopted per card and how many were refused (card 228: three, none refused; if every window is adopted the cap is theatre); a delete form or false-positive class found after the card closes that the slice labelled INSURANCE and left to a follow-up. A cap met by sending the risk to later cards would show in the second and fifth. Evidence for the trial retro, not targets.

TRIAL SCOPE: the next 3 cards whose slice is written after this Decider is recorded, of any kind; a card with a split slice counts once. The trial retro decides keep, revise or drop by Decider.

ACCOUNTABILITY CHECK: at slice read, the reviewer checks one Cap line per criterion, a sum of at most three, and a label on each list item. At the pre-ship gate, the independent reviewer counts reds per criterion from "Criterion: n" against the Cap lines and lists each red past a cap with the commit of the window that adopted it. A red past a cap whose window was committed after it counts as not run; the auditor tick may FAIL it. The checker is the reviewer, re-checked by the auditor.

NO BLUR: stand-down 1 of card 228 (two cycles per remaining criterion) was a one-card adjustment; its verdict stays in the card 228 retro. D136, D167 and D168 keep their own metrics. This pick is scored on reds past a cap, not on card duration; a fall in duration is credited only if the counter-metrics hold.

BASELINE (card 228): slice of one cycle per criterion with no Cap lines; criterion 1 seven cycles (1a to 1g), criterion 2 three (2a to 2c), criterion 3 two (3a, 3b), one gate-fix cycle; windows 228-c1-scope, 228-c2-scope and 228-gate-fix at 15.5, 18.6 and 16.8 minutes; about 13 hours first commit to sign-off; one gate HOLD; six follow-up cards unfiled at sign-off.

COST (issue 245): about one line per criterion and one label per list item in the slice, one line in each red turn record, and a scope window only when a cycle is added. No extra seat or meeting. If the cost shows in the trial it is the first thing the retro cuts.

REVERSIBILITY: two-way door. Drop the lines from the briefs; no shared state, checker or earlier D-record changes. Slices already written keep their lines or lose them with no effect.

VOTING RULE: reply first line Yes, Support-it or No, then P1: to Pn: one line each (one per proposal point). A No names ONE fixed condition (CONDITION: ...). A reasonable, reversible condition is adopted and the proposal re-polled; a later round may not add new demands. Only an irreconcilable purpose or model conflict escalates. Independent fresh-seat re-run before recording. Each reply ends with one line "CITE: <point>" as D171 C1 says.

Size: 9130 bytes
