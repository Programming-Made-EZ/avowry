# Card 214 retro input (facilitator; facts only, from git and the session)

## Flow metrics (UTC, from commit and issue timestamps)
- Issue 214 created 08:07. Window 214-rerank-4 adopted 214 first (8b5dfd1..e6e2ab8). Task-force window 214-taskforce did not settle (delivery lead's one-commit condition), full Decider D156 passed both rounds (326c77f).
- ZOMBIES by QA at ready 08:55 (815d4e6). Slice 08:59 (a71d197), slice window with one adopted condition and a re-poll (e6fe23d, decision 26f82d7).
- Seams commit 1 09:08 (ec25197). First red 09:30 (26681e1); last red 09:47 (391828f). Issue to first red: 83 minutes (card 205 baseline: 24 minutes).
- Green commit 5 made locally about 10:35, held because 3 then 6 card tests timed out under the full local suite; thread-pool floor 5b; both pushed 11:06 as 111960d and afa4e40 (rebased; bodies cite a22b651 and 3aed9dc, mapped in the record).
- Review: attempt 1 stopped (seated on 110-minute-old probes, F-040); attempt 2 provisional 11:41 (a7e9401), final 12:01 (a477bc6): APPROVE with one medium fix; F-062 cause verdict EXPOSES. Fix 5c 12:04 (d18194c).
- Pre-ship gate 12:25 (917bfc6): six PASS; CI run 41052829472 green. Sign-off 12:27 (602718c). Issue to sign-off: 4 h 20 min.
- Size per commit (D136): largest 348 lines (26681e1); all within limits.

## Practice metrics
- Red before fix: yes for all three Done checks (26681e1, 1fcc793, 391828f before 111960d).
- One acceptance criterion at a time: NO (F-060, audit 28): all three Done checks' red tests (about 800 lines, ten guards) before any green; a single green.
- Outside-in with recorded doubles (F-055): layers and doubles named in every commit body.
- Born-green guards: ten, each shown red by a named mutation in the commit body (window 214-slice condition from ctx-3).
- Whole team: seven roles in seven contexts, all Claude with a recorded reason (probes 08:21, 09:18, 11:07, 12:03). Non-author review: yes (ctx-4).
- Findings and flags raised during the card: F-058 (213 domain-expert answer missing at a boundary; the stand-down entry has no field for it), F-060 (one criterion at a time), F-061 (40.6 min without a commit or a blocker note), F-062 (green held locally, in-flight cap passed without update; possible new flake). Facilitator misses: the stale-probe reviewer seat; regenerating the board archive from a scratch worktree wrote its path into 36 archived cards (06d21b0, restored later); a re-rank note briefly cited pre-cherry-pick hashes; the GitHub REST quota ran out 08:22 to 08:33 (mob_process check costs 92 REST calls; note d70fb97).

## Adjustments to review first (D155)
1. Card 213 stand-down 2 adjustment (fcfdb7b): keep module-only runs and start the resumed card with one full Python suite run on a clean checkout, citing a pushed hash. Not yet applied (213 is paused).
2. D154 trial (claim-equals-commit and pre-ship gate), card 205 retro pick, trial card 2 of 5 (213 was card 1, still open): applied on 214 (917bfc6).
3. D153 objection-window path (experiment): windows on 214: 214-rerank-4, 214-taskforce (unsettled, went to D156), 214-slice (one condition, re-poll), 216-rerank-5, 217-rerank-6, 217-rerank-6b.
4. Facilitator practice changes during the card: one push per boundary, no manual check before a push, CI read at least 120 s apart (rate-limit observation d70fb97); in-flight notes for waits over 20 minutes (F-061).
