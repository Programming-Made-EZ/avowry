# mob-defaults #58: model allocation per seat (declared before each launch; actual after; F-13: the flag stands as actual until the CLI reports one)

| Seat | Role | Service | Model declared | Reason | Actual |
|---|---|---|---|---|---|
| probe | facilitator | probe_all.py | - | the eight-service kickoff probe | 08:51:46Z: agy, claude, cursor, muse, qwen available; codex, copilot quota-reached; grok 402 |
| red, red fix | dev, red | muse (muse_write.py, wt-md58, ctx-58r) | default, effort medium | a writing seat with a written slice; claude kept for green and QA (R-ROT) | the flag; 9 pending, 3 characterisation; red fix exact to R58-1 |
| green, fix round 1 | dev, green | claude (subagent, ctx-58g) | sonnet = claude-sonnet-5-5 (Agent model flag) | rotation code, mid tier | the flag; green 982 s, fix round 1 1270 s, test-first (R58-2) |
| review round 1 | QA | claude (subagent, throwaway clone) | opus = claude-opus-5-5 (Agent model flag) | strong tier for rotation semantics | the flag; CHANGES, 2 BLOCKING, 4 MINOR |
| review round 1 (cross-family) | QA, second reviewer | agy (plan mode, clone) | gemini-3.8-flash-medium (--model) | a non-author family | the flag; nothing Opus missed |
| review round 2 | QA | agy (plan mode, clone) | gemini-3.8-flash-medium (--model) | R-ROT: claude held two seats; qwen gate-blocked | the flag; SIGN-OFF, 1 MINOR (named limit, #65) |
| sign-off | sign-off | muse (read-only, ctx-58s) | default, effort medium | a service other than the green's | the flag; SIGN-OFF, 89 passed, 19 subtests |
| gate | facilitator | full suite under the heavy lock, 12:28 to 13:14Z | - | the merge gate | 10 failed, 2371 passed: main's 8 plus a live-process gate test that passes alone (#66); no new failure; merged 69041af |
