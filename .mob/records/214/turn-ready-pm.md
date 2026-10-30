# Card 214 ready turn (product manager, ctx-1)

Written at close from commits 9803190 and 326c77f, not at the time of the turn.

## What I did at ready
- Task-force declaration: proposed seven roles, all Claude seats in distinct contexts (PM ready ctx-1, QA red ctx-2, backend developer ctx-3, reviewer ctx-4 with a test-infrastructure and Kestrel hosting lens, PM signoff ctx-5, architect ctx-6, delivery lead ctx-7), a Left-out list with reasons, the domain-expert answer (no separate .NET/Kestrel expert; developer and reviewer cover it) and the window rule (every declared role polled, F-054; one fixed condition per objection, D145). Claude seats because the 08:21 UTC probes showed the other models without usable quota. Commit 9803190 (window 214-taskforce proposal).
- Card-text changes: the second hazard (WaitUntilListeningAsync accepting a foreign process on the port) folded into Done checks 1 and 2, keeping the card at three checks; the F-056 closure evidence named in check 3 (commit removing FreeLoopbackPort plus a green CI run on the last commit); Not-in-this-card extended to product code and Kestrel configuration. Also in 9803190.
- D156 proposal: when window 214-taskforce did not settle, I wrote the full Decider answering the delivery lead's objection. Adopted: every 214 commit within D136 limits and no ceremony beyond D153/D154. Not adopted: one commit for red and green, which contradicts D137 and condition 3 of window 214-rerank-4 (red committed before the fix). Commit 326c77f.

## Commits
- 9803190: window 214-taskforce proposal and card-text changes
- 326c77f: D156
