# Process retro prep: defect and rework table (2026-10-26)

Input to the process retro that follows "live mob dashboard (036)". the product owner's diagnosis frames it: "it just seemed like we weren't getting very far
because there were a lot of mistakes and new patches to fix those." So the cost to examine is **rework**: mistakes, then patches, then more mistakes.

Every stop, reopen, revert and incident since the resolver (2026-10-25 and 26), hand-classified from the working logs (`.mob/*/MOB.md`), git history and the
facilitator's own record. Reverts: none (no commit was reverted); stashes and reopenings stand in for them. The count is per listed item; items that caused several stops are
marked.

## Classes

| Code | Class |
|---|---|
| T | Facilitator or tooling bug (prompt, parser, launcher, stale log, environment) |
| X | Facilitator operating slip (an error in how the facilitator operated, not a tooling defect) |
| W | Done check too weak |
| H | Harness or data capture |
| M | Model misbehavior |
| S | Safety slip |
| R | Run shape: a long or data-heavy task against the driver turn or the cycle cap |

## The ledger

| # | Card | What happened | Class | Stops it caused |
|---|---|---|---|---|
| 1 | resolver | The .NET 8 runtime was missing in the mob's shell, so the gate could not run; QA turns reported DEFECTS twice; about 5 days of waiting | T | 2 |
| 2 | resolver | Done-when passed with 5 of 7 review findings open; the card was reopened | W | reopen |
| 3 | resolver | The PM turn said "no pending resolver tests remaining" while two classes were still Pending | M | 0 (caught by re-run) |
| 4 | resolver | The rewritten Done-when passed trivially (PH335 was already in GRAMMAR.md) | W | reopen, split |
| 5 | resolver | The first independent review (Grok) returned empty because of nested quoting in the launcher; about 40 minutes lost | T | 0 (time) |
| 6 | tooling | The agreement guard added to card mode broke 18 tests whose temp repos had no agreement | T | 0 (found at push) |
| 7 | resolver | A facilitator commit swept the mob's staged files into an unrelated commit | X | 0 |
| 8 | benchmark v0 | Done-when passed on a skeleton (1 task, no runner) | W | reopen |
| 9 | benchmark v0 | Five cycles spent on runner features; the suite itself was not authored (sequencing) | R | 5 cycles |
| 10 | benchmark v0 | A Done-when check ("at least 3 models") was satisfiable by the fake self-check rows | W | 0 (found on verify) |
| 11 | benchmark fix | The stored baseline was invalid: Codex invocation flag, Grok max-turns, parser scoring chatter, Copilot passed a file path | H | 4 |
| 12 | benchmark fix | agy's 22 empty replies were recorded as `completed` language failures | H | reopen |
| 13 | benchmark fix | The empty-reply check failed only if ALL rows were empty, so a mix passed it | W | reopen |
| 14 | benchmark fix | `reply_head` was cut to 200 characters before scrubbing, and a `~\AppData` path reached a public repo | H | flag at sign-off |
| 15 | benchmark fix | Three runs hit the 3-cycle cap; a four-model baseline outlasts any driver turn | R | 3 |
| 16 | E1 (mobbing) | Five tooling bugs: verdict parsed from the first word (Grok narrates); no stage expectations at the red checkpoint; later-increment checks flagged; whole MOB.md sent to every seat (1.45M tokens); leftover uncommitted work shown as the cycle's delta | T x5 | 7 |
| 17 | E1 | A driver changed code, README and results in the red step | M | 1 |
| 18 | E1 | A seat re-flagged expected-red checks after being told they were expected | M | 2 |
| 19 | Deciders | Vote parsing traps: narration before the vote, a conditional "No ... yes if", an echoed codex transcript | T x3 | 0 (voided votes) |
| 20 | Deciders | A launcher bug (PowerShell flattened a one-item list) sent a voter the bare prompt "d11" | T | 1 incident |
| 21 | Deciders | That voter then ran with write access in the real tree and edited source files (no commit, no push) | S | 1 incident |
| 22 | tooling | The facilitator ran `rm -rf` on a variable path (nothing deleted) | S | 0 |
| 23 | Deciders | The facilitator stashed edits it wrongly assumed came from another session, then restored them | X | 0 |
| 24 | tooling | A flow-metrics commit said "green" while one test failed (a chained command did not stop on the failure); a stray control character was the cause | X | 0 |
| 25 | dashboard | Three starts were wasted: the card changed but the stale working log was reused, and cross-repo references could not be verified by the PM | T | 3 |
| 26 | dashboard | The first run hit the 3-cycle cap; each cycle's checks take about 13 minutes | R | 1 |
| 27 | all | The facilitator went idle after reporting (twice) and needed a nudge | X | 2 idle gaps |
| 28 | all | Repeated edit failures from backslash and escape handling when writing scripts through the shell tool | X | about 8, minutes each |

## The count

| Class | Items | Share of 28 rows (items weighted by sub-bugs: T counts its sub-items) |
|---|---|---|
| T facilitator or tooling bug | 13 (rows 1, 5, 6, 16 x5, 19 x3, 20, 25) | the largest |
| X facilitator operating slip | 5 (rows 7, 23, 24, 27, 28) | |
| **T + X together (the facilitator side)** | **18 of 34** | **53%** |
| W Done check too weak | 5 (rows 2, 4, 8, 10, 13) | |
| H harness or data capture | 3 (rows 11, 12, 14) | |
| M model misbehavior | 3 (rows 3, 17, 18) | |
| R run shape | 3 (rows 9, 15, 26) | |
| S safety slip | 2 (rows 21, 22) | |

By stops caused, tooling bugs lead as well: the five E1 tooling bugs alone account for about 7 stopped runs, the stale log for 3 wasted starts, the environment for 2.
Model misbehavior is the smallest of the real causes (3 items). the product owner's read is right: most of the rework traces to the tooling and the facilitator's own operating, not to the models.

## Five whys on the largest class (facilitator and tooling bugs)

1. **Why did tooling bugs stop real cards?** The changed tooling was used on a real card at once, so a real card was the first test.
2. **Why was a real card the first test?** The unit tests use fake CLIs that do not behave like the real ones: they do not narrate before answering, echo a transcript, mangle Windows
   quoting, run long, keep session state, or ignore restricted modes. There was no smoke tier that runs the real CLIs on a toy card.
3. **Why no smoke tier?** The tooling was changed reactively: a stop, a patch sized to that symptom, a rerun of the live card as the "test". Each patch fixed the last failure, and the
   next real failure was a different one.
4. **Why reactive and unreviewed?** The tooling had no standard and no board card: it was written as standing rules arrived, by the facilitator, with no review by a non-author model.
   The facilitator is both author and operator (84% of code lines in the flow numbers).
5. **Why author and operator?** Tooling work was never put through the mob (rotating drivers, review, a task force) because driver runs on it were slow and unreliable (stalls, the cycle cap)
   and because it was not on the board until the dashboard card.

**Root cause:** tooling changes did not go through the product's own pipeline (realistic tests, independent review, a staging run on a toy card), and the author was also the operator.

## Candidate fix (the retro picks; any seat may challenge it)

The cause, not another symptom: **hold the tooling to the product's standard (Dispatch's P6), with the missing piece built.**

1. A **real-CLI smoke run** (`smoke.py`): a toy card (a scratch repo, one trivial acceptance check) run through the real facilitator with each seat's real CLI, about 10 minutes, covering
   narration, transcript echo, Windows quoting, restricted modes and the Decider runner. Run before any change to `facilitator.py`, prompts or the harness touches a real card.
2. **Realistic fixtures** captured from the real CLIs' outputs (narration, echo, long replies, quoting) added to the fake CLIs, so unit tests cover real behavior.
3. **Non-author review** of every tooling diff, by a seat that did not write it.
4. Tooling changes go through the same task-force rules as product cards (rotating authors); the facilitator stops being the default author of tooling.

Metric: tooling-caused stops per card (baseline above: about 10 stops across five cards). Counter-metric: time to land a tooling change. It rides alongside the next product cards: the smoke run
is the first thing run before the mob starts the adapter card.

## Other seeds competing for the pick (Dispatch)

P1 no turn ends without the next action running; P2 fit the run shape to the task (the run-shape class R above is 3 items and a long-task problem); P3 log facilitator takeovers (this ledger
and the flow numbers already show the 84% share); P4 auto-fresh on card change (row 25); P5 reports written for the reader. Several map directly onto classes above: P2 to R, P3 and P6 to T and X, P4 to row 25.
