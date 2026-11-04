# Retro pick proposal (agile coach seat): a time-boxed premise probe that measures and does not build

Card 290 retro, 2026-11-04. Proposer: agile coach seat. For a Decider. Sources: .mob/records/290/ retro-voices.json, retro-input.md, record-in-progress.json, signoff-ctx6.md; D181, D183, D184; the facilitator's window minutes.

Decider (number set at recording), the card 290 retro process pick, as a working-agreement experiment (D183: not a card)

PICK: before a slice or scope proposal is written, its seat runs a premise probe of at most 15 minutes that measures and does not build: it traces every call site of each function the change touches, runs one regression probe on HEAD, and quotes both. Proposed by five of seven voices (product manager, QA, developer, decider, coach).

User benefit: the product owner gets a slice whose greens do not stop on a fact the code already held. Card 290's three green stops came from premises nobody traced.

Why this one:
- Dispatch Q1 (did D184 catch it?): partly. Both scope proposals ran and quoted their premises; none of their premises failed. D184 C1 covers only scope proposals, so the slice's P5 ("two lines in result_of") was never run, and it was the one that failed: the cite minimum sat in cite_ok (red 1), the call site sat at decider.py:340 (green 1 stop), and that call site dropped "did not answer" votes (green 3 stop). A caller grep and one never-answered run would have shown all three. D184 C2 also let "the planned change applied locally" count as a run: slice attempt 1 did that, built 405 lines in 8 files and was stopped at 46 minutes against a 30-minute cap. So yes, a sharper brief: measure, don't build, and apply it to the slice.
- Dispatch Q2 (lighter path for a declaration fix?): not in this pick. The 290-declfix window cost 8.1 minutes; the cost was the stop before it. Its text contradicted the adopted condition and six seats passed it: fewer eyes is the wrong lever. A call site found by the probe is in the slice from the start, so no declaration window arises. A lighter path is deferred below.
- Dispatch Q3 (Grok-free windows): single-round windows ran 8.1 to 12.1 minutes (mean 9.5) with 0 to 2 extra attempts, against 17.5 to 24.6 (mean 20.0) with 3 to 12 extra when Grok was in rotation: about half. Two-round windows did not change (290-taskforce 41.0 with Grok still in rotation, against 41.4 and 49.1). Small n; Codex's return confounds it. The five windows took about 79 of 360 minutes, so windows are no longer the main cost; the work that wrong premises sent back is.
- D183: it changes a brief; no seat, file type or card.

The others: counted quorum (audit lens) is already a retro experiment; the counted-vote checklist (architect) is pinned by red 1 and is a brief item.

HYPOTHESIS: We believe a bounded, non-building premise probe on the slice stops greens from tripping on facts the code already holds. We will know when green stops caused by a premise fall from 3 to at most 1 per card, and windows from 5 toward 3, while minutes from ready to first red do not rise past the bound below.

RULE UNDER TRIAL:
C1. Premise. Every slice or scope line that states where a change goes, how big it is, or how existing code or tests behave is a premise (extends D184 C1 to the slice).
C2. Probe, 15 minutes. Before writing the slice, the seat: (a) lists each function the change touches and every call site of it on HEAD, file:line; (b) names each input class that reaches the change (for example cited, uncited, never answered) and runs one existing or throwaway test per class on HEAD; (c) quotes command, HEAD hash and output line.
C3. Measure, don't build. The probe edits no file outside a scratch copy and changes at most 5 lines there, discarded. A premise that needs more is a finding ("not small"), written into the slice, not built.
C4. Overrun. At 15 minutes the seat stops and marks what is left "unrun", with what the green does if it is false (D184 C3). The facilitator stops a probe at 20.
C5. Check. The facilitator checks C2 in the slice file in git before the slice window opens: each premise has call sites, a probe line and output, or "unrun"; a missing one goes back before any vote. The gate lists each green stop and whether a premise caused it; the auditor re-checks from git.
C6. Not changed: D175 caps and windows, D181, D184 for scope proposals, the stop rule. No new seat, file or meeting.

METRIC: green stops caused by a slice or scope premise (card 290: 3). Target: at most 1. Windows per card (card 290: 5).

COUNTER-METRICS (evidence, not targets):
1. Minutes from ready to first red (card 290: 08:00 to 10:19, 139). A rise means the probe became ceremony.
2. Probe minutes and overruns past 15. More than one overrun in the trial means C3 failed.
3. Premise lines with call sites per slice. A fall to 0 with the same scope means premises went unstated: theatre.
4. Regressions found after the gate. Fewer stops count only if this stays 0.
5. Fewer windows count only if 1 and 4 hold.

TRIAL SCOPE: the next 3 cards with a slice that changes existing code, after this Decider is recorded. The trial retro decides keep, revise or drop by Decider.

NO BLUR: D181 (trial card 3 of 3 next) and D184 keep their own metrics; this pick changes what a slice carries, not when a window is needed.

COST: about 15 minutes per slice, one grep and one test run per input class, one facilitator look. Card 290 lost 46 minutes to slice attempt 1.

REVERSIBILITY: two-way door. Drop the lines from the slice and facilitator briefs.

Deferred, not rejected (retro experiments, D183 retro log with owner and trigger):
- Lighter path for a declaration fix that only adds a call site a red needs (Dispatch Q2): trigger, a declaration window on a trial card despite the probe
- Grok's return to counted windows: trigger, card 291 lands; compare window minutes and extra attempts with the figures above.
- Seat time cap for all turns: C4 trials it on the slice first.
