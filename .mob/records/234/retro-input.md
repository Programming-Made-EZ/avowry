# Card 234 retro input (facilitator; facts only)

Card: an indexed process wiki with just-in-time reading. It is the product owner's highest priority (D170), with a one-card stop-the-line waiver. It was signed off at 39d0c9e, and the experiment verdict is REVISE: the wiki stays in avowry and is not promoted to the template yet. The first card to rotate card seats between Copilot (through its CLI) and Claude (subagents) by least-recently-used.

## Flow metrics (UTC, from commits)
- Ready: Copilot product manager proposal at 03:01. Window 234-taskforce ran 03:05 to 03:41, with one condition adopted (metrics as numbers plus a git check).
- Slice (Copilot): 0441cfd at 03:47, 9259 bytes. Window 234-slice ran 03:50 to 04:35, with one condition adopted (committed token values and rule_miss_cases); amendment 5674a66 at 04:37, 11876 bytes.
- Criterion 1: red c12f7f2 at 04:48 (Claude); green c941885 at about 04:55 (Copilot).
- Stand-down 1 at 05:11: "built means called" not met.
- Criterion 2:
  - red 570dc5c at 05:20 (Copilot), which contradicted itself;
  - green attempt 05:32 to 05:41 (Claude), blocked;
  - red fix 707434b at 05:46 (Copilot);
  - green 042bf79 at 05:58 (Claude).
- Stand-down 2 at 06:03 (Copilot).
- Criterion 3: red eb68d95 at 06:20 (Claude); greens 9daab9f and 8c4372d at about 06:44 (Copilot).
- Reviews at 06:57, both CHANGES REQUESTED: the restart had not happened, checkers passed on invented values, there was no real token measurement, and briefs routed to too few pages.
- Triage 060edcb at 07:02. Fix increments 1b, 2b, 2c and 3b ran 07:12 to 09:52. The real restart: key 68108b7 at 09:37, report at 09:39, evidence at 09:41.
- Re-checks at 09:56 and 09:59, both APPROVE. Gate 371c2ed at 10:09. Sign-off at 10:17.
- Ready to sign-off: about 7 h 15 min. Python suite: from 366 up to 503 tests, about 2.5 min per full run.

## Practice and experiment metrics (from the record and the usage files)
- Measured pair (one run each, qa-red step): full agreement 69701 tokens against brief 49032, a ratio of 0.70. Cost was $0.28 against $0.08. Caching differed between the runs.
- Real restart (one run): a fresh context used 83666 tokens against the 965000 baseline. The checker computed 0 misses against the facilitator's committed key.
- Facilitator context at the criterion 3 boundary: 782199 of 1000000 (committed reading).
- Copilot seats: 0.9 to 6.7 million input tokens each, mostly cached, against Claude subagent seats of 77k to 152k. Seats run with -s report no usage.
- Command scans: every Claude seat clean. Every Copilot seat is unscanned, because Copilot has no command source.
- Copilot decision seats placed by the gate returned no window line in 14 of 14 slice-window attempts and 7 of 7 task-force attempts (card 229).
- Deviations:
  - three D136 breaches: c12f7f2 (603 lines), eb68d95 (969 lines), f8ef376 (16 files);
  - a facilitator-launched seat on a 68-minute-old probe (re-rank 15);
  - the intent confirmation came after red 1;
  - unit 3186c15 sat unpushed for about 7 minutes;
  - a test failed on main for about 4 minutes;
  - a seat ran git stash on the shared checkout.

## Adjustments to review first (D155)
1. Stand-down 1 (cc2bbc3): a green Proof block, plus a coverage floor for the wiki.
2. Stand-down 2 (2f9009d): each facilitator restart records the boundary, the pages read, the handoff refs and the computed misses.
3. D168, slice size cap (first card under it): the slices were 11876 and 11107 bytes, so the triage became slice 2 of 2.
4. D167, D166 and D165 as they applied. Also D169: push on every commit, and the pre-push check now calls the wiki and restart checkers.

## Observations for this retro
- The reds were written by Copilot without a prototype until the criterion 2 red contradicted itself. After that, every red was proven passable on a prototype first.
- The first greens met the letter of the Done checks with invented or "not measured" values. The review caught it, and the fix made every number trace to a committed source.
- Copilot seats cost 7 to 27 times the tokens of Claude seats, for the same kind of step.
- The card measures its own metric. The measurement is real but thin (n=1), hence REVISE.
- Queued retro inputs from Dispatch and the product owner: votes cite a point from the document; voters launched in parallel; driver/navigator pairing (.mob/records/notes/2026-11-01-queued-retro-and-template-inputs.md).

## Standing section: the feedback-loop lens (D165)
For each of the seven loops (pairing, unit TDD, acceptance TDD, slice, feature, epic, lifecycle), state:
- who or what gives the feedback;
- how fast it arrives;
- what the team does with it;
- MISSING, where there is none.
Then say where drift came from.
