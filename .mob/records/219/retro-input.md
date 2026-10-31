# Card 219 retro input (facilitator; facts only)

Card: the record checker accepts cards that close out of number order (implements D159; started under the product owner's one-card waiver D162). Goal: release the held records of the fresh-probe seat gate (208) and the retro-only removal (209).

## Flow metrics (UTC)
- Task force window 23:39 to 23:42; slice 23:46 (3b04e20), slice window with the delivery lead's condition (fold Done check 3 into criterion 2's cycle) adopted and re-polled 23:48 to 23:51 (amended 37e456c).
- Intent line (D165, first use) 23:53, confirmed 23:54 (bca5510, e4cda78).
- Criterion 1: red 23:57 (451f4ea), unit 00:02 (ab94601), green 00:08 (b374ef7). The green was held once because MOB.md was stale.
- Criterion 2 with Done check 3: red 00:24 (55b5794), unit 00:33 (66904ab), green 00:34 (1a08bd6).
- Review APPROVE 00:47 (e60a511), with the held records passing in a throwaway rehearsal; pre-ship gate 00:51 (f92a913), CI pending at that time.
- Window to green: about 55 minutes; whole Python suite about 2 minutes; record check about 92 GitHub calls.

## Practice metrics
- D157: two cycles as the amended slice planned; green before the next red.
- D165 intent line: about 14 seconds from line to confirmation; caught nothing, because the slice already quoted the rule.
- D136: within limits on every commit.
- Seat-written in-flight lines on every seat start; MOB.md regenerated at fixed steps after stand-down 1 (no green held after that).
- Metric: held records 2 until the card closes (target 0).

## Adjustments to review first (D155)
1. Card 219 stand-down 1 (dd22ebb): MOB.md regenerated at fixed steps; intent line and confirmation in one commit; in-flight stamps from date -u at commit time.
2. D165 intent line (trial card 1 of 3).
3. D158 checklist.

## Observations
- The record checker assumed cards close in number order; re-ranking made out-of-order closes normal, and two signed-off cards (209, 208) could not land their records. The facilitator missed that ordering 208 before 210 (D160) would not help once 214 had closed.
- The forged-date guard the slice planned (M3) was never written; the reviewer judged it unneeded because ordering uses history position, not dates.

## The feedback-loop lens (D165 standing section)
Map the seven loops by feedback (who or what gives it, latency, what the team does with it), from your role, and name missing or slow loops.
