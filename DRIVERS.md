# Driver availability

Standing rule: see TEAM_WORKING_AGREEMENT.md, Driver availability. A driver is never assumed permanently out; it is re-probed at every card boundary and every 30 to 60 minutes while out.

| Driver | Status | Last probe | Known reset | Note |
|---|---|---|---|---|
| Codex | available | 2026-10-28 | none | author and voter |
| Grok | UNAVAILABLE (usage balance exhausted, API status 402 Payment Required) | 2026-10-28 | unknown (no reset time given; a balance top-up or a new billing period) | a tracked re-probe runs every 30 minutes and writes grok-back.flag; seats rotate to Codex and Claude; cards in flight are reassigned |
| Claude | available (seated as a fresh-context subagent; the headless CLI is not signed in) | 2026-10-28 | none | normal rotation member by the product owner's ruling (Sonnet by default); covers role seats while Copilot and agy are out; not the sole driver while Codex and Grok have quota |
| Copilot | UNAVAILABLE (no quota) | 2026-10-28 01:30 local | 2026-11-01 00:00 UTC (account endpoint) | premium interactions 1504 of 1500 used; a tracked re-probe runs every 30 minutes and writes copilot-back.flag (facilitator scratch folder) when quota returns; seats rotated to Codex and Grok; holdout rows marked unavailable to rerun on return |
| agy | UNAVAILABLE (quota) | 2026-10-28 01:50 local | resets in 108h50m from the probe at 2026-10-28 about 08:50 UTC, so about 2026-11-01 21:40 UTC (from the error text) | the CLI error text names the reset time; a tracked 60 minute re-probe runs and writes agy-back.flag (facilitator scratch folder) when it answers; seats were already rotated away |
| Local (Qwen3.6-35B-A3B Q4 via llama.cpp and aider) | NOT LIVE YET (model downloading, runtime and agent installed) | 2026-10-29 | none: a local seat never runs out of credits | enters at the bottom of the trust ladder; loads only while the test gate is idle (gate lock or a running dotnet test), one model at a time, stopped by its own process id; models are entries in mob-defaults core/local_llm.json; live after the first successful probe and a trial on solved cards (experiment E6) |

## Events

- 2026-10-28: the product owner's staffing rule: roles over models; Claude is a normal rotation member (not sole driver while others have quota). Claude seats Copilot's roles while it is out; the headless claude CLI is not logged in, so Claude is seated as a fresh-context subagent.
- 2026-10-28: agy probed (error: Individual quota reached, resets in 108h50m) and put on a tracked 60 minute re-probe; Copilot re-probe runs every 30 minutes.
- 2026-10-28: Copilot quota exhausted (premium interactions 1504 of 1500). Seats rotated to Codex and Grok. Holdout run: Copilot rows marked unavailable (30 rows, no quota); to be rerun on return. The frozen bar (fewer than 3 models is INCONCLUSIVE) is not changed.
- 2026-10-28: Grok usage balance exhausted (402 Payment Required) at the start of card 161. Card 161 reassigned to Codex; reviews use Codex and fresh-context Claude seats. A tracked re-probe (30 minutes) writes grok-back.flag; on return the seat and the author rotation are restored.
- 2026-10-29: a local-model driver seat was approved. Runtime (llama.cpp b11972, CUDA 12.4), agent CLI (aider in an isolated Python 3.12 environment) and the on-demand wrapper (local_driver.py in mob-defaults) are in place; the 21 GB model is downloading at about 3 MB/s (an estimated two hours). It becomes available after a successful probe; real cards only after the trial.
