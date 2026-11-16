# mob-defaults #62: model allocation per seat (declared before each launch; actual after; F-13: the flag stands as actual until the CLI reports one)

| Seat | Role | Service | Model declared | Reason | Actual |
|---|---|---|---|---|---|
| probe | facilitator | probe_all.py | - | the eight-service kickoff probe | 14:43:10Z: agy, claude, cursor, muse, qwen ok; codex, copilot quota-reached; grok 402. qwen ok but given a no-fit reason in the declaration |
| red, red fixes, fix round 1 red | dev, red | claude (subagent, ctx-62r) | sonnet = claude-sonnet-5-5 (Agent model flag) | several small units with fakes, mid tier | the flag; 58 pending (after R62-1's real lock shapes), 19 fix-round tests; one helper bug (R62-3); about 1108 s for the first red, short follow-ups |
| green | dev, green | cursor (cursor-agent -p --force, ctx-62g) | composer-2.5 (--model) | first-choice cursor writer | the flag; 356 s, 66 of 66, 269 lines over 5 files; took the seat-use lock with wait=0 (B1) |
| fix round 1 green | dev, green | cursor (ctx-62g2) | composer-2.5 (--model) | same writer (R62-2) | the flag; 477 s; fixed B1 and M1-M6; stopped on the R62-3 test bug and reported it without editing the test |
| review round 1 and 2 | QA | claude (subagent, clone, ctx-62q) | opus = claude-opus-5-5 (Agent model flag) | a refusal rule over history rows, strong tier | the flag; round 1 CHANGES (B1, M1-M6, B1 reproduced with a held lock); round 2 SIGN-OFF with N1-N3 (#67) |
| review round 1 (cross-family) | QA, second reviewer | agy (plan mode, clone, ctx-62x) | gemini-3.8-flash-medium (--model) | a non-author family | the flag; CHANGES with the same seven findings and labels as Opus; review.md was in the folder it read, so it is not independent evidence |
| sign-off | sign-off | muse (muse_write.py, read-only, ctx-62s) | default, effort medium | a service other than the green's and reviewers' | the flag; SIGN-OFF, 220 passed; worktree unchanged; brief's diff range started at f281d53 instead of 1ff374f (a superset, facilitator slip) |
| gate | facilitator | full suite under the heavy lock, 15:37 to 16:21Z | - | the merge gate | 9 failed, 2499 passed: all known on main; none new; merged 98ccece |
