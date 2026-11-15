# Card 430: model allocation per seat (declared before each launch; actual after; the F-13 rule: the flag stands as actual until the CLI reports one)

| Seat | Role | Service | Model, pinned | Reason | Actual |
|---|---|---|---|---|---|
| probe | facilitator | all drivers (probe_all.py) | each backend's probe model | the live probe before the seat plan (probe-all-2026-11-15.jsonl, 15:24 UTC) | agy, claude, cursor available; qwen probe-only (heavy, unseated); codex, copilot, grok, muse out |
| D266 vote | architect, audit lens, coach, decider, dev, qa | agy (decider.py, a fresh context per role) | gemini-3.8-flash-medium | a judgement vote, mid tier; the only seatable service for a vote (claude refused by rotation, cursor's vote launcher unpinnable, qwen heavy, codex, copilot, grok and muse out) | the flag (agy prints no model line); six of six Yes |
| architect slice | architect | claude (subagent, a fresh context for ctx-1) | opus = claude-opus-5-5 (pinned with the Agent model flag) | the guard is a text scanner: real-shape corpus from the real tests and a reference measured against it, strong tier | |
