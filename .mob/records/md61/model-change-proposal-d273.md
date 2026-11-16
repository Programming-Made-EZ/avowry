# Proposal for Decider D273: mob-defaults #61's green seat for fix round 2 moves from muse to claude sonnet

Facilitator, 2026-11-16 UTC. VOTE on this mid-card model change. Under the owner's 2026-11-14 ruling, a change of model mid-card goes through a Decider.

EVIDENCE: the fix round 2 green on muse (D272, ctx-3d) stopped at once with "API error 429 ... Subscription quota exhausted. Your usage window resets at 2026-11-16T05:05:53Z". It changed nothing, and the worktree is clean at the fix round 2 red, 33b9ecb, with 13 tests failing.

The other writing services:
- cursor gpt-5.3-codex is out until 2026-12-14; cursor auto is healthy, but its model is unpinned;
- codex and copilot are out of quota, and grok is out (402);
- agy's headless mode cannot run commands, so it cannot run the tests;
- qwen has an open driver finding.

THE CHANGE: the green for fix round 2 is **claude sonnet = claude-sonnet-5-5**, an Agent-tool subagent pinned by the model flag, in a fresh context, ctx-3e. That context differs from the red's (ctx-2c).

Why claude: it is healthy. Ruling R-ROT puts it back into rotation as a normal seat. Its subagents met their briefs on this card without the shortcuts muse took in fix round 1.

The brief keeps D272's forbidden patterns: no history rewrite, no old-format special case, no test edits. The facilitator checks the diff for history writes before accepting it.

Muse returns to rotation after its 05:05 UTC reset. It is re-probed at the next kickoff, as every service is.

CLAUSE: seat claude sonnet (ctx-3e) as #61's fix round 2 green.

Voters, per ruling R-ROT (no service holds more than two roles; muse is out):
- architect and qa: claude;
- audit lens and dev: cursor auto;
- coach and decider: agy.
