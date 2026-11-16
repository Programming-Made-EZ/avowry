# mob-defaults #63: model allocation per seat (declared before each launch; actual after; F-13: the flag stands as actual until the CLI reports one)

| Seat | Role | Service | Model declared | Reason | Actual |
|---|---|---|---|---|---|
| probe | facilitator | probe_all.py | - | the eight-service kickoff probe | 13:16:39Z: agy, claude, cursor, muse, qwen available; codex, copilot quota-reached; grok 402 |
| red, red fix, fix round 1 red | dev, red | claude (subagent, ctx-63r) | sonnet = claude-sonnet-5-5 (Agent model flag) | a small precise detection change, mid tier | the flag; 22 pending, 16 characterisation (1184 s, mostly a 17-min regression run); one wrong expected value (R63-1); fix round 1 red 19 s |
| green | dev, green | cursor (cursor-agent -p --force, ctx-63g) | composer-2.5 (--model) | first-choice cursor writer | the flag; 224 s, slice exact, no test edit; left the R63-1 test failing and reported it |
| fix round 1 green | dev, green | cursor (ctx-63g2) | composer-2.5 (--model) | same writer (R63-2) | the flag; 52 s, one character class |
| review round 1 | QA | claude (subagent, clone, ctx-63q) | opus = claude-opus-5-5 (Agent model flag) | detection-rule edges, strong tier | the flag; SIGN-OFF, 2 MINOR, reproduced 9 reply shapes; 141 s |
| review round 1 (cross-family) | QA, second reviewer | agy (plan mode, clone, ctx-63x) | gemini-3.8-flash-medium (--model) | a non-author family | the flag; SIGN-OFF, the same MINOR 1 found independently; 377 s |
| review round 2 | QA | agy (plan mode, clone, ctx-63x2) | gemini-3.8-flash-medium (--model) | narrow fix-round review; claude held two seats (R-ROT) | the flag; SIGN-OFF; 154 s |
| sign-off | sign-off | muse (muse_write.py, read-only, ctx-63s) | default, effort medium | a service other than the green's and reviewers' | the flag; SIGN-OFF, 88 passed, 19 subtests; worktree unchanged; 140 s |
| gate | facilitator | full suite under the heavy lock, 13:58 to 14:40Z | - | the merge gate | 9 failed, 2414 passed: all known on main (7 plus #64, #66); none new; merged 7658bf2 |
