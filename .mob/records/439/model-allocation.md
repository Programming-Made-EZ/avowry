# Card 439: model allocation per seat (declared before each launch; owner rulings of 2026-11-14)

| Seat | Role | Service | Model, pinned | Reason | Actual |
|---|---|---|---|---|---|
| probe | facilitator | all drivers | each backend's probe model | the live probe before the seat plan (probe-all-2026-11-15.jsonl) | |
| D261 vote | architect, audit lens, coach, decider, dev, qa | agy (decider.py, a fresh context per role) | gemini-3.8-flash-medium | a judgement vote, mid tier; the only seatable service for a vote now: claude refused by the rotation rule (mob-defaults #58), cursor's vote launcher unpinnable (#57), qwen blocked by a play-tracker train, codex, copilot, grok and muse out | gemini-3.8-flash-medium (the flag); six of six Yes after one re-ask |
| green | dev, green | cursor (cursor-agent -p --force, linked worktree) | gpt-5.3-codex (`--model`, declared by D261) | implements the slice's scanner, a code-writing tier | the flag (the CLI printed no model line); 01:29:37Z to 02:13:55Z, Slice=439 163 of 163, sweeps 0 new leaks |
| green, fix round | dev, green | cursor (cursor-agent -p --force, linked worktree) | gpt-5.3-codex (`--model`, D261) | ports the architect's fixed reference (ruling 4) | the flag (no model line); 03:45:19Z to 03:53:03Z; Slice=439 180 of 180, sweeps equal the reference (4559, 2679) |
| D262 vote | six roles | agy (decider.py, fresh context per role) | gemini-3.8-flash-medium | the QA model change mid-card (the owner's rule: like any task-force change) | |
| green, fix round 2 | dev, green | cursor (cursor-agent -p --force, linked worktree) | gpt-5.3-codex (`--model`, D261) | ports the second fixed reference | the flag (no model line); 05:07:55Z to 05:12:43Z; Slice=439 209 of 209, sweeps equal the reference |
| green, fix round 3 | dev, green | cursor (cursor-agent -p --force, linked worktree) | gpt-5.3-codex (`--model`, D261) | ports the third fixed reference | the flag (no model line); Slice=439 229 of 229, sweeps equal the reference |
