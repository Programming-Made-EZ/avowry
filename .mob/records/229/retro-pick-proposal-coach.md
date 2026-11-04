# Retro pick proposal (agile coach seat): a scope premise is run in code before its window

Card 229 retro, 2026-11-04. Proposer: agile coach seat. For a Decider. Sources: .mob/records/229/retro-voices.json; retro-input.md; record-in-progress.json (decisions.green_2_review, scope_window, green_2_cycle_2, scope_window_2; deviations); turn-scope-architect.md; turn-scope2-architect.md; green2-cycle2-attempt.diff; signoff-ctx6.md sections 2 to 4; D181 and D183 in docs/decisions/process-and-staffing.md.

Decider (number set at recording), the card 229 retro process pick, as a working-agreement experiment (D183: not a card)

PICK: each premise a scope proposal rests on is run in code on HEAD before the scope window opens, as a throwaway probe or a test, and the proposal quotes the command and its output. A premise that cannot be run is labelled "unrun" and the proposal says what the green does if it is false. Proposed by four of seven voices (product manager, QA, developer, coach).

User benefit: the product owner gets a scope that holds on its first green, not one that a green disproves and a second window replaces. Card 229 spent green 2 cycle 2 (33 commands, no commit) and window 229-scope-2 on premises nobody had run.

Why this one:
- It is this card's largest avoidable cost. 229-scope's probe said "scrub leaves its inputs unchanged"; mob_summary.py:33 masks any 48-character run, and red 2 uses "x" * 296. Its result rule broke 27 tests whose doubles vote an uncited Yes (green_2_cycle_2). One run of red 2 with scrub-then-trim would have shown both.
- The evidence was already there: green_2_review records that the green 2 seat skipped scrub "because it broke red 2's 300-character test". The premise contradicted a recorded fact.
- 229-scope-2 shows the fix works: its Assumes are "read in code, not inferred", with line numbers, and its green passed first time.
- It serves the story being worked (D183): it changes how a scope proposal is written, adds no seat, file type or card.

The others:
- Built-prompt sha before a live vote (architect): this card already met it at sign-off (prompt_sha matched both records, section 1). A retro experiment for the next live-vote card, a brief line, not this pick.
- Seat policy plus one vote parser (audit lens): the parser half is bugs already carded, card 291 (a vote after narration is read) and card 286 (NO OBJECTION is not stored as No). The seat-policy half is a retro experiment, below.
- Per-card backend exclusion in the gate and a gate seat that is not the card's subject (decider): retro experiments by the sign-off (section 4). Until then the facilitator names the excluded backend in the gate brief by hand.
- Card 290 (the result counts only cited votes) is the bug the second premise moved out; it is carded and needs nothing from this pick.
- Proposals 278 and 284 are not this pick's business; they go to their re-rank.

HYPOTHESIS: We believe that running each scope premise in code before its window stops a green from disproving its scope. We will know when scope windows caused by a wrong premise fall from 1 to 0 over the trial, while minutes from ready to first red and from failing evidence to a scope proposal do not rise past the bounds below.

RULE UNDER TRIAL:
C1. Premise. Every Assumes or Probe line in a scope proposal (D175 C4 window) that states how existing code or tests behave is a premise.
C2. Run it. Before the window opens, the proposer runs each premise on HEAD: a python -c line, a one-off unittest run, or the existing red with the planned change applied locally and discarded (no commit). The proposal quotes the command, HEAD hash and the output line that settles it.
C3. Unrun. A premise that cannot be run in the time is marked "unrun", and the stop rule says what happens if it is false.
C4. Check. The facilitator checks C2 before launching the window, from the proposal file in git: each premise line has a command, HEAD and output, or "unrun". A missing one sends the proposal back before any vote. The sign-off lists each premise and whether the green confirmed it. The auditor re-checks from git.
C5. Not changed: D175 caps and windows, D181, D157, the green commit rule, the stop rule. No new seat, file or meeting.

METRIC: scope windows caused by a premise the next green disproved (card 229: 1, window 229-scope-2 after 229-scope; 2 scope windows in all). Target: 0.

COUNTER-METRICS (evidence, not targets):
1. Minutes from ready to first red (card 229: 21:00 to 22:39, 99). A rise over 20 minutes per card means the probe has become ceremony.
2. Minutes from failing evidence to a scope proposal (card 229: about 10 each). Must not double.
3. Premise lines per proposal (229-scope: 2; 229-scope-2: 3). A fall to 0 with the same scope size means premises went unstated, not checked: theatre.
4. Scope windows per card. Fewer windows count only if 1 and 2 hold.

TRIAL SCOPE: the next 3 cards with a scope window or that touch existing code, after this Decider is recorded. A card with no scope window still records counter-metric 1. The trial retro decides keep, revise or drop by Decider.

NO BLUR: D181's trial (cards 2 and 3 of 3) keeps its own metric; this pick does not change when a window is needed, only what a proposal carries into it.

COST: one probe run per premise, a quoted line each, one facilitator look. If the cost shows, C3's "unrun" label is the release valve.

REVERSIBILITY: two-way door. Drop the lines from the scope-proposal and facilitator briefs.

Deferred, not rejected (retro experiments, D183 retro log with owner and trigger):
- Built-prompt sha before a live vote (architect): trigger, the next live-vote card.
- Seat policy and per-card backend exclusion, a gate seat off the subject (audit lens, decider): trigger, the next card whose subject is a decision backend.
- Probe-age alert (developer): a brief item.
