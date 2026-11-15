# Card 422: model allocation per seat (declared before each launch; actual after; the F-13 rule: the flag stands as actual until the CLI reports one)

| Seat | Role | Service | Model declared | Reason | Actual |
|---|---|---|---|---|---|
| probe | facilitator | probe_all.py | - | the live probe before the seat plan | 2026-11-15 20:08Z: agy, claude, cursor available; qwen probe-only; codex, copilot, grok, muse out |
| D268 vote | six voters | agy (CLI, a fresh context each) | gemini-3.8-flash-medium (--model) | a vote on committed records, mid tier | six of six Yes, 20:10:28 to 20:14:37Z |
| architect slice | architect | claude (subagent, a fresh context for ctx-1) | opus = claude-opus-5-5 (pinned with the Agent model flag) | text recognisers over MSBuild, page prose and lockfiles: real-shape corpus with encodings and a reference measured against it, strong tier | the flag (the subagent reports no model line); 176 corpus rows in 6 families, 37 of 37 mutants killed, real files pass, offline checks 209 ms; about 23 min |
| red | dev, red | claude (subagent, a fresh context for ctx-2, linked worktree wt-422) | sonnet = claude-sonnet-5-5 (pinned with the Agent model flag) | rows given by the slice, mechanical test writing in Python and C#, mid tier | |
