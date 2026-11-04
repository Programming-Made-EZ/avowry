# Card 290 retro input (facilitator; facts only)

Card: the decider's result counts only votes with a valid cite; code in mob-defaults 38b8eb5..73eea8a (8 commits); signed off with conditions (.mob/records/290/signoff-ctx6.md).

## Flow metrics (UTC, from commits)
- Ready about 08:00, task-force window 08:08 to about 08:50 (two rounds and a re-ask), slice 08:53 to 10:27 (attempt 1 stopped at 46 minutes, attempt 2 narrow), slice window 10:05, red 1 10:19, scope window 10:43, red 1b 10:58, green 1 stopped 11:15 then declaration-fix window 11:19 and green 1 11:45, reds 2 and 3 11:50, green 3 stopped 12:00, scope-2 window 12:12, red 1c 12:35, green 1b 12:45, green 3 13:10, full suite 1788 OK, gate SHIP 13:40, sign-off about 14:00: about 6 hours for about 30 lines of production code.
- Caps: slice 1+1+1; criterion 1 raised to 2 by window 290-scope-2 (green 1 had dropped never-answered votes). Windows: task force (2 rounds), slice, scope, declaration fix, scope-2 (re-ask). Grok skipped in counted windows from the slice on (window 290-taskforce).
- Green stops: three (green 1: the call site was outside the scope; green 3 attempt 1: green 1's never-answered regression; green 3 attempt 2: the zero-voter test, resolved by the slice's ZOMBIE E).

## Adjustments to review first (D155)
1. D184 (each scope premise run in code on HEAD), first card under it: both scope proposals ran their premises and quoted them; the slice's attempt 1 ran its premises by building the fix (46 minutes, stopped); the slice's P5 ("the change is two lines in result_of") was wrong and stopped the green twice.
2. D181 (a lettered red ends at the first passing green), second card of its trial: red 1b before any green (in-cycle); red 1c after green 1 as cycle 2 with its window first. Reds past a cap without a prior window: 0.
3. The Grok skip in counted windows (window 290-taskforce): every later window ran on Claude, Copilot and Codex only.
4. D183 follow-ups: bug card 293 (cite read from a prompt echo); retro experiments for a quorum, a CITE ask in the prompt, a seat time cap, the Codex echo.

## Observations
- Codex returned (quota reset) and sat in every window; its stored replies carry the CLI banner and prompt echo (trimmed by the facilitator from records), which hides a latent counting defect (card 293).
- A Copilot audit-lens OBJECTION in window 290-scope-2 was dropped as unread because narration came before it (the first-line runner).
- The facilitator's declaration-fix text contradicted the adopted task-force condition ("as none"); six seats passed it; the green overrode it correctly.
- Scanner false findings twice (an "unreadable file" on a piped test command; "rmtree" inside a report text).
- F-087: an in-flight commit stayed unpushed 19 minutes because a push failure was hidden by reading one output line; inflight.sh now verifies its push.

## Standing section: the feedback-loop lens (D165)
Each voice names, for the 7 loops, who or what gives the feedback, its latency, what the team does with it, and MISSING where none exists.
