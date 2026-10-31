# Card 226 retro input (facilitator; facts only)

Card: decision seats (Decider voters and objection-window seats) start on a fresh probe and rotate least-recently-used like card seats, and the decision record lets an auditor recompute rotation from git (corrective for MD-002 after audit flag F-070; code in mob-defaults decider.py). Product owner goal (D161): lock the process into tooling and the template.

## Flow metrics (UTC, from commits)
- Task force window 01:31 to 01:33 (2d908fa); slice 01:39 (f0c0d63), amended 01:40 (4bb7d00); intent line 01:45 (52d4676).
- Criterion 1 (fresh probe before every decision seat): red 01:53 (8357f14), green 02:30 (2189ca2), 37 min.
- Structural commit (gate_seat split into check_seat and commit_seat, no behaviour change): 02:44 (d3826ea).
- Criterion 2 (least-recently-used rotation for decision seats): red 02:56 (a54c9d9), green 03:44 (ed5b0e1), 48 min.
- Criterion 3 (the record names model, probe time and reason): red 04:07 (9c8bb71), green 04:47 (165fde8), 40 min.
- Review 05:01 (69ff413, CHANGES REQUESTED: F1 seat-use lines for seats that never start; F2 voter model tied to the probe only by key order; F3 unscrubbed probe evidence). F1 red 05:12 and green 05:40; M4 guard 05:44; F2 and F3 red 05:50 and green 06:18; re-check 06:47 (CHANGES REQUESTED, R1: the M4 guard was racy); R1 guard 06:58; second re-check 07:08 APPROVE (6f73e2e).
- Pre-ship gate 07:17 (7b1ffde, PASS); sign-off 07:24 (f4f2835, SIGN OFF); Done check 3 text names the probe field at close (issue edit).
- Window to sign-off: about 5 h 53 min. Full mob-defaults suite: 5 runs (466, 497, 538, 565, 583 tests; 21 to 23 min each), all clean on the first try.

## Practice metrics
- D157 one criterion per cycle: held; the review fixes ran as new cycles under criteria 2 and 3.
- Outside-in with recorded doubles: 11 unit layers, each red against a temporary stub.
- D136: held on all 24 mob-defaults commits (largest 333 lines).
- Mutation runs: on every green; two guards were found weak (e6d24bb's guard could not fail; c65ec21 was racy), both by mutation, both fixed.
- In-flight lines written by command (stand-down 1 adjustment): stamps and probe ages exact; the carriage-return check moved from grep to tr after stand-down 2 found grep blind.
- Re-probes before 50 minutes: on time all night (02:10, 03:00, 03:29, 04:09, 04:46, 05:31, 06:10, 06:49); Codex, Copilot, Grok and agy without quota at every probe, so every seat was Claude with the reason recorded.
- MOB.md staleness refused two pushes (a seat's in-flight line at 04:07, the facilitator's at 05:02); the in-flight script now regenerates at a count of 2.

## Adjustments to review first (D155)
1. Stand-down 1 (2eec2df): numbers and paths in records come from commands; seats leave their scratch alone; the structural commit before red 2.
2. Stand-down 2 (a234db8): every claim names its command and count or says "not measured"; red 3 asserts the same-run reason and the facilitator confirms the reading before the green; the green pastes a recompute from git; the tr carriage-return check.
3. D165 intent line and D166 assumption line (second card using both).

## Observations for this retro
- The red a54c9d9's body said the other suites "stayed green" when only three modules were measured (self-reported, recorded, not amended).
- A layer commit (e6d24bb) shipped a guard holding a stray backspace byte, so it could never fail; its body also miscounted (21 for 18). Found by the developer's own mutation run in the next green.
- The guard c65ec21 passed review-fix scrutiny once but was racy (fails 6 of 8 on the mutant); the reviewer found it at re-check.
- A seat ran rm -rf on a variable path (criterion 1) and later rm -f /dev/null (criterion 3); nothing was deleted.
- The review found three real gaps after three clean greens: the reds asserted what the slice named, not what the runner does with disabled, unknown or repeated seat names, or with a reordered models map.
- This retro is the first real run of the new decision-seat gate: each voice runs through decider.py with the fresh-probe gate, the rotation rule and the seat-use history at .mob/records/seat-use.jsonl, one at a time because the append-only check refuses a history another run changed meanwhile.

## Standing section: the feedback-loop lens (D165)
For each of the seven loops (pairing, unit TDD, acceptance TDD, slice, feature, epic, lifecycle): who or what gives the feedback, how fast it arrives, what the team does with it, and MISSING where there is none; then where drift came from.
