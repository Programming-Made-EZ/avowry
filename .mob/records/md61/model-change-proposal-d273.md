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

## Amendment after the first window (02:43:38 to 02:48:11 UTC)

Results of the first window:
- coach (agy): Yes.
- audit lens (cursor auto): No. Its condition for Yes: "keep every service at most two roles under R-ROT when seating green".
- architect and qa (claude): no vote. The D164 rotation rule refused both, because qwen "has waited longer". qwen probes ok but has never been seated, which is mob-defaults #58's defect.
- dev (cursor) and decider (agy): no vote; their replies' first lines were not parsed.

THE AMENDMENT (meeting the audit lens's condition):
- Ruling R-ROT's limit of two roles per service counts the six Decider voting roles.
- The fix round 2 green is a task-force seat, not a voting role.
- Over the whole of #61's remaining task force, claude would hold the red (ctx-2c), the green (ctx-3e), QA review (ctx-4c) and the sign-off. That is four seats in four separate contexts, with cursor auto as the second, cross-family reviewer of every change. If the voters judge that too many claude seats, they may say No with a condition naming another service for the green.

The amended window seats the voters across four services, with no service holding more than two roles:
- architect: qwen (the gate is idle);
- qa and decider: claude;
- audit lens: cursor auto;
- dev and coach: agy.
