# Card 441: model allocation per seat (declared before each launch; actual after; the F-13 rule: the flag stands as actual until the CLI reports one)

| Seat | Role | Service | Model, pinned | Reason | Actual |
|---|---|---|---|---|---|
| probe | facilitator | all drivers | each backend's probe model | the live probe before the seat plan (probe-all-2026-11-15.jsonl) | |
| D264 vote | architect, audit lens, coach, decider, dev, qa | agy (decider.py, a fresh context per role) | gemini-3.8-flash-medium | a judgement vote, mid tier; the only seatable service for a vote (claude refused by rotation, cursor's vote launcher unpinnable, qwen heavy, codex, copilot, grok and muse out) | the flag (agy prints no model line); six of six Yes |
| green | dev, green | cursor (cursor-agent -p --force, linked worktree) | gpt-5.3-codex (`--model`, D264) | ports the architect's reference | the flag (no model line); 08:51:20Z to 08:53:55Z; Slice=441 111 of 111, Slice=439|435 341 of 341, sweeps equal the reference |
| QA review | qa | claude (subagent, a fresh context ctx-4) | opus = claude-opus-5-5 | adversarial review, strong tier (agy's strong tier out of quota) | |
| green, fix round 1 | dev, green | cursor (cursor-agent -p --force, linked worktree) | gpt-5.3-codex (`--model`, D264) | ports the fixed reference | the flag (no model line); Slice=441 140 of 140, sweeps equal the reference; it left two Pending traits (removed by the facilitator) |
| QA review, round 2 | qa | claude (subagent, a fresh context for ctx-4) | opus = claude-opus-5-5 (pinned with the Agent model flag) | adversarial review after fix round 1, strong tier (agy strong tier out of quota) | the flag (the subagent reports no model line) |
