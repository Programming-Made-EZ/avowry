# mob-defaults #61: model allocation per seat (declared before each launch; actual after; F-13: the flag stands as actual until the CLI reports one)

| Seat | Role | Service | Model declared | Reason | Actual |
|---|---|---|---|---|---|
| probe | facilitator | probe_all.py | - | the live probe before the seat plan | about 23:22Z: agy, claude, cursor available; qwen probe-only; codex, copilot, muse out; grok unavailable (402) |
| D269 vote | six voters | agy (CLI, a fresh context each) | gemini-3.8-flash-medium (--model) | a vote on committed records, mid tier | six of six Yes, 23:22:26 to 23:26:59Z |
| slice | facilitator | the facilitator session | claude opus = claude-opus-5-5 | a small change with the row format given by play-tracker's log; written into the red brief | done |
| red | dev, red | claude (subagent, a fresh context for ctx-2, worktree wt-md61) | sonnet = claude-sonnet-5-5 (pinned with the Agent model flag) | the scenarios and slice name rows and fields, mid tier | the flag; 53 tests then an every-seat addendum of 18 (62 fail, 9 pass), reproduced by the facilitator; two heredoc uses recorded (one edit of its own test file, one no-op print); red eaee2dd in mob-defaults |
| green | dev, green | cursor (cursor-agent -p, a fresh context for ctx-3, worktree wt-md61) | gpt-5.3-codex (pinned with --model) | the writer, the rotation reader, the decider outcome lines and seat_log.py, code-writing tier | the flag (no CLI model line); 71 of 71, rotation and decider tests 151 pass, rerun by the facilitator |
| review round 1 | QA | claude (subagent, a fresh context for ctx-4, throwaway clone of wt-md61) | opus = claude-opus-5-5 (pinned with the Agent model flag) | rotation and append-only history break subtly, strong tier | |
