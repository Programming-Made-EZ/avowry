# Retro input: card 361 (diagnostic codes use the AV prefix), facts from git and run logs (2026-11-08 and 09 local times)

Names, addresses, home paths and secret values are not printed here. Read the records named.

## What shipped
Every diagnostic code prefix renamed from PH to AV (numbers, lines and messages unchanged; no alias): 66 tracked files outside the history paths, 575 lines, one atomic green (8ed59b1, 68 files changed) with the architect's two-pass rewrite rule, one file move and one hand edit; two Pending criterion tests (the CLI prints AVnnn with the same numbers; the live tree holds no PH code) with a scanner, a history allow-list test and a prefix-free number snapshot. Gate review SHIP (the rename reproduced byte for byte; history untouched; a mutation table); sign-off SIGNED OFF WITH CONDITIONS (two for card 162, one for this retro).

## Flow metrics and process against build time (local times)
- Ready draft: the Muse attempt (20:53 to 21:08, 840 s cap, no output; a launch before it died at once on a PATH error) then Cursor through cursor_reply.py with the measured reach in the brief: draft at about 21:10. Decider D229 (the D228 preflight applied, seats one at a time, six Yes, no re-ask) recorded 21:24: about 31 minutes from the first launch to the record, 15 of them lost to the Muse attempt.
- Build: slice 21:37, characterisation 21:45, red 21:56, green 22:13: about 49 minutes from the record to the green.
- Process against build before the gates: about 0.65 to 1 (31 against 49 minutes). After the green: baseline gate and push 22:24, independent gate review 22:45, sign-off 23:09 (about 45 minutes for the two).
- Red-first held: characterisation declared before the red; both Pending tests failed by assertion on the pre-change tree (the facilitator reran them on main; the gate reviewer replayed them); the green changed the red files by one line each. No born-green finding.

## Seat mix
Card seats: Ready draft Cursor (after a failed Muse attempt); slice, red (own worktree), green (a different context), gate reviewer, sign-off all Claude: five of six non-vote seats. Votes in D229: Muse 2, Cursor 2, Qwen 1, Claude 1. Reasons: Grok, Codex, Copilot and agy are out of quota; red and green are separate contexts and worktrees; the gate review and sign-off must run the gate, which Cursor (ask mode) and Qwen (reply-only) cannot; Muse cannot finish a repository-reading brief (it timed out twice, on cards 203 and 361) but gives short counted votes.

## Experimental entries this card exercised
D228 preflight (first exercise): the window was counted at once, no seat re-asked, about 13 minutes of vote time against 55 for D228 and about 75 lost on D227; harness-failed windows per card: 0 (target 0). D222 (a Ready row naming two constructs becomes two ids): used for the id lists C, L and H; no dropped half. The adoption of D222 as permanent practice has not yet gone to a Decider.

## Findings
- The architect's slice verified the draft's reach by command and found it wrong (66 files and 575 lines against the draft's counts; a sixth src file; four bare-`PH` prose lines) and noted the quoting trap (`git grep -E 'PH\d'` silently matches nothing). The draft-then-slice chain corrected itself; the red then needed no revision (card 203's red needed one).
- Two test limits carried: the `test_getting_started` regex narrowing is defended by no committed test; the numbers of AV020, AV337, AV340 and AV341 rest on the CLI rows alone.
- The sign-off started a Python run beside its gate by accident (the tool backgrounds any run past 180 s) and saw the load-sensitive benchmark timeout flake; its clean rerun was green. The baseline gate and the reviewer's gate did not flake.
- Idle minutes on this card: about 0 with nothing running (every wait had a monitor that matched success, failure and expiry; the launchers' first output was checked within 60 seconds); one startup failure (Muse launch on a PATH error) caught by the monitor at 45 seconds. No Dispatch nudge on this card so far.
- Condition for card 162: stale llm-check pins; one sentence on the AV convention in GRAMMAR; a "formerly PH" pointer for readers of old retros.

## Carried
The look-back at D208, D209, D210; D222's adoption; the product owner's entry on idle minutes (the root cause across the six cases and a fix: the next retro's pick), the follow-ups of card 203 (bugs 372, 373, 374, 365; the four follow-up cards).

## Questions for the voices
The adjustment review of D228 (first exercise), D222, D204, D202 and the Decider protocol step 6; the process-against-build ratio; the idle minutes and what keeps the cycle moving without a Dispatch nudge (the product owner's entry); drivers left (Claude, Cursor, Qwen, Muse for short votes); one process improvement candidate.
