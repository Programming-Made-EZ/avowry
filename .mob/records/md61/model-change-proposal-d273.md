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

## Second amendment after the amended window (02:49:05 to 02:54:10 UTC)

Results of the amended window:
- architect (qwen): Yes, qwen's first counted seat.
- decider (claude): Yes.
- dev (agy): Yes.
- qa (claude): refused by the D164 rotation rule, because muse, agy and qwen have waited longer (#58).
- coach (agy): no vote; its first line was not parsed.
- audit lens (cursor auto): No again. Its condition: "apply R-ROT's two-roles-per-service limit to task-force seats", because the first amendment let claude hold the red, green, QA and sign-off.

THE SECOND AMENDMENT (meeting that condition). For the rest of #61, no service holds more than two task-force seats:
- **green, fix round 2: cursor auto.** It runs through `cursor-agent -p --force --model auto` in wt-md61, ctx-3e. Its model is declared `auto`, an unpinned size, under F-13. It is a writing seat and it is healthy.
- **review round 3: claude opus (QA, ctx-4c)** and **agy gemini-3.8-flash-medium** (the cross-family reviewer, reading the diff inline; headless agy runs no commands).
- **sign-off: muse, medium effort**, after its quota window resets at 05:05:53 UTC.

That leaves claude with the red and QA (2), cursor with the green (1), agy with one review (1) and muse with the sign-off (1).

CLAUSE (replacing the first): seat cursor auto as #61's fix round 2 green, with the review and sign-off seats above.

Voters for the re-vote (no service holds more than two roles, and claude is limited to the role the rotation rule let it take):
- architect: qwen;
- decider: claude;
- audit lens: cursor auto;
- dev and qa: agy;
- coach: qwen.

## Third amendment after the re-vote (02:54:51 to 03:01:09 UTC)

Results of the re-vote:
- architect (claude): Yes. The qwen architect seat failed to launch ("another local model is loaded; one at a time"), because two qwen roles in one window share one local model; the runner seated claude instead.
- coach (qwen): Yes.
- dev and qa (agy): Yes.
- decider (claude): refused by the D164 rotation rule (qwen, #58).
- audit lens (cursor auto): No. Its condition: "pin the fix round 2 green to a named model (not `auto`) in the seat clause so the writer is auditable from the run record", matching the owner's 2026-11-14 pinned-launch ruling.

Smoke tests at about 03:03 UTC in a throwaway clone:
- cursor `gpt-5.2`: the same monthly usage limit as gpt-5.3-codex (resets 2026-12-14).
- cursor `composer-2.5`: "OK".
- cursor `auto`: "OK".

THE THIRD AMENDMENT (meeting that condition): the fix round 2 green is **cursor, pinned `--model composer-2.5`**, Cursor's own coding model and the only named model that answers today. It runs in wt-md61, ctx-3e. The other seats and the two-seats-per-service limit stand as in the second amendment: review claude opus plus agy, sign-off muse after 05:05:53 UTC.

Voters for this window (qwen takes one role at most; no service holds more than two):
- architect: claude;
- coach: qwen;
- decider and audit lens: cursor;
- dev and qa: agy.

## Third amendment window (03:02:41 to 03:08:04 UTC) and the decider role

Results of the window:
- architect (claude), coach (qwen), audit lens (cursor), dev (agy) and qa (agy): Yes, all counted. The audit lens's condition is met.
- decider (cursor): no counted vote.
- Re-seats of the decider role:
  - claude at 03:08:13: refused by the D164 rotation rule (#58).
  - qwen at 03:09:11 to 03:16:02: it endorsed the change in its text ("I endorse this path forward"), but its first line was not parsed.

FINDING (vote tooling): the decider role's brief (`role-decider.txt`) seats the voter as "the DECIDER / SIGN-OFF seat" and asks whether the decision "(adopt, reshape or reject) will rest on evidence". Voters on agy (four times), cursor and qwen answered with a verdict in prose instead of the required first line (Yes, Support-it or No), and each reply supported the change. Muse passed with the same brief on D272. This goes to mob-defaults beside #58: the role brief must not reframe the reply format.

STATE: five of six roles counted Yes, the decider role is outstanding, and there is no No or amendment open. The decider role is re-seated on muse at its quota reset (2026-11-16 05:05:53 UTC). It is the only healthy service not already holding two roles in this window whose reply format has passed.
