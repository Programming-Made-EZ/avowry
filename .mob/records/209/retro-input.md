# Card 209 retro input (facilitator; facts only, from git and the session)

## Flow metrics (UTC, from commit timestamps; mob-defaults commits marked md)
- Pulled at the loop boundary after card 214 (re-rank 7, 12:43). Task-force window 12:45 (proposal revised once before opening: slice and ship seats added after the facilitator stated the schema-1 rule); slice 12:51 (dd49100); slice window with two adopted conditions and a re-poll (card 218 filed; D136 thin-commit line).
- Criterion 1: red 13:24 (md 6836d76), units 5a2c930, b527e1d, 8e387e3, green 14:26 (md 7d371fa). Red to green 62 min; two cap moves.
- Criterion 2: red 14:33 (md 44112b4, plus 37b1273), units 145947b, 147a90a, green 14:54 (md 18f3962). Red to green 21 min; no cap move (the facilitator's cap came from an estimate).
- Criterion 3: red 15:01 (md 794a4a7), green 15:20 (md 1bb0a83). Red to green 19 min; the seat wrote its own in-flight line.
- Review 15:30 (a4475fc, CHANGES REQUESTED: F1, F2); fixes 73026aa, 7fdb168, e239b12; full suite at e239b12 338 tests OK (verification 15:59 to 16:16, after a first verification ran the wrong repository); re-check APPROVE 16:18 (61a4741); pre-ship gate 16:22 (05a71d2); sign-off 16:24 (6609645).
- Pull to first red: 41 min (card 214: 83; card 205: 24). Pull to sign-off: 3 h 41 min.
- Full mob-defaults suite: about 17 minutes per run, five runs on this card.

## Practice metrics
- D157 (one criterion per red-green cycle): held for all three criteria; "Criterion: n" in every red body; each green pushed before the next red. Gap: the review-fix commits carry no Criterion line (sign-off flags it as the most substantive deviation).
- Outside-in with recorded doubles (F-055): layer 3 consumed a recorded fake removal_problem; leaves had no doubles.
- Born-green guards G1 to G5, each shown red by a named mutation.
- D136: one breach, the whole-file line-ending churn in md 44112b4 and 37b1273 (net 123 lines).
- No history rewrites: one amend of an unpushed commit by the developer on criterion 1; a correction commit (147a90a) used instead on criterion 2.
- Whole team: seven roles in seven contexts, all Claude (probes 12:03, 12:48, 13:45, 14:54, 15:33, 16:24 UTC). Non-author review: yes.

## Adjustments to review first (D155)
1. Stand-down 1 (fac4808): green cap = suite start + 25 min. Stand-down 2 judged it "tune".
2. Stand-down 2 (10495dd): the seat writes its own in-flight line with its real start time. Applied on criterion 3 (38762d2) and the review fixes (1e8698d).
3. D157 (card 214 retro pick), trial card 1 of 5 here.
4. Card 213 stand-down 2 (fcfdb7b): clean-checkout full suite on resume. Still not applied (213 not resumed).

## Facilitator misses on this card
- The task-force proposal first left out the slice and ship seats; the facilitator caught it before the window.
- The criterion 2 in-flight cap was written from an estimate.
- A verification run executed the wrong repository (a background cd that did not take effect); recorded, rerun.
- The facilitator told the criterion 2 developer the wrong line endings for a file and corrected it.

## Inputs for the next re-rank (not decisions)
- The auditor proposes 217 right after 209 (flaky-tests rule; a known red taxes every gate run), and notes the three gate failures look like staleness rather than flakes (Bar6 is the suspended Usable-v1 bar 6 that card 162 restores; the two CSharpBaseline tests report tests_did_not_run).
- Card 218 (mid-card retro removal drops the seat) is filed and unranked.
