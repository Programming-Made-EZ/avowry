# Driver availability

Standing rule: see TEAM_WORKING_AGREEMENT.md, Driver availability. A driver is never assumed permanently out; it is re-probed at every card boundary and every 30 to 60 minutes while out.

| Driver | Status | Last probe | Known reset | Note |
|---|---|---|---|---|
| Codex | available | 2026-10-28 | none | author and voter |
| Grok | available | 2026-10-28 | none | author and voter; sometimes gives no vote when reviewing |
| Claude | available (seated as a fresh-context subagent; the headless CLI is not signed in) | 2026-10-28 | none | normal rotation member by Steve's ruling (Sonnet by default); covers role seats while Copilot and agy are out; not the sole driver while Codex and Grok have quota |
| Copilot | UNAVAILABLE (no quota) | 2026-10-28 01:30 local | 2026-11-01 00:00 UTC (account endpoint) | premium interactions 1504 of 1500 used; a tracked re-probe runs every 30 minutes and writes copilot-back.flag (facilitator scratch folder) when quota returns; seats rotated to Codex and Grok; holdout rows marked unavailable to rerun on return |
| agy | UNAVAILABLE (quota) | 2026-10-28 01:50 local | resets in 108h50m from the probe at 2026-10-28 about 08:50 UTC, so about 2026-11-01 21:40 UTC (from the error text) | the CLI error text names the reset time; a tracked 60 minute re-probe runs and writes agy-back.flag (facilitator scratch folder) when it answers; seats were already rotated away |

## Events

- 2026-10-28: Steve's staffing rule: roles over models; Claude is a normal rotation member (not sole driver while others have quota). Claude seats Copilot's roles while it is out; the headless claude CLI is not logged in, so Claude is seated as a fresh-context subagent.
- 2026-10-28: agy probed (error: Individual quota reached, resets in 108h50m) and put on a tracked 60 minute re-probe; Copilot re-probe runs every 30 minutes.
- 2026-10-28: Copilot quota exhausted (premium interactions 1504 of 1500). Seats rotated to Codex and Grok. Holdout run: Copilot rows marked unavailable (30 rows, no quota); to be rerun on return. The frozen bar (fewer than 3 models is INCONCLUSIVE) is not changed.
