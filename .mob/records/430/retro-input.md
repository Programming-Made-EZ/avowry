# Card 430 retro input (facilitator, from the card's records; committed before the voices)

All times are UTC, taken from the commit log and from the seats' start and done files.

## Flow metrics (first: constraint first, D258 clause 4)

**Lead time.** From the kickoff (Decider D266, 15:30Z) to the sign-off (19:38Z) and the merge to main (19:40Z) took about 4 h 10 min. That covers one card, one red and green, and ONE fix round. Card 441 took about 6 h 41 min with two fix rounds.

**Time by step.**

| Step | Time |
|---|---|
| Kickoff probe and D266 | about 10 min |
| Architect slice (Opus) | about 14 min (slice committed 15:45Z) |
| Red: tests written | by about 15:50Z |
| Red: held behind play-tracker's lock | until the facilitator ran it at about 16:55Z (commit 16:53Z) |
| Clean-clone reproduction | about 10 min (one invalid first attempt) |
| Green port (Cursor gpt-5.3-codex) | 1 min 39 s (17:07:57Z to 17:09:36Z) |
| Gate (green) | about 14 min under Avowry's lock |
| Review round 1 (Opus) | about 14 min |
| Fix slice (Opus) | about 14 min |
| Fix red (Sonnet) | about 2.5 min |
| Fix green (Cursor) | 1 min 23 s |
| Review round 2 (Opus) | about 9 min |
| Fix gate | 22 min, its [S] reading invalid |
| Sign-off (Sonnet) | about 16 min, including a valid [S] |

**Waiting on the other mob's heavy lock.**
- About 65 min idle: the red's build and filtered runs waited behind play-tracker ship-train 62 (15:43Z onwards). The facilitator sat idle until the 16:51Z watchdog ruled those runs light.
- About 13 min: the fix gate waited behind play-tracker #1065.
- The fix architect also stopped once on the lock. It resumed after the facilitator applied the light-work ruling.

**Rounds.**
- Review BLOCKING by round: 2, 0. MINOR by round: 8, 5.
- Card 441 for comparison: BLOCKING 3, 2, 0 over three rounds.
- Both round 1 BLOCKING findings were OLDER real classes outside the card's spelling families: limits enforced in a helper or a child process. Neither was a neighbour of a corpus family. The rest of round 1 was one new super-linear pattern (MINOR 6) and one new false-name family (MINOR 4).

**D265, second trial.** The slice commits a corpus covering each input encoding before the red, and each fix slice commits its changed rules' neighbour variants before its red.
- The slice committed 160 rows in 8 families, with 12 encoding rows per new spelling family, before the red.
- The fix slice committed 196 neighbour rows (`fix-time` 46, `fix-waitasync` 150) before the fix red. 39 of those rows were wrong on the round 0 guard.
- Review round 2 found no BLOCKING. Its 5 MINOR were contrived or older forms, plus one leading-whitespace form of the WaitAsync token rule (MINOR 1) that no encoding row covered.
- Mutants: 27 of 28 killed in round 0 and 40 of 41 after the fix. The one survivor each time was the timing-only `unbounded` mutant.

**[S] ceiling** (`Status!=Pending`, 15 min).

| Run | Result |
|---|---|
| Green gate | 13.47 min |
| Fix gate | 21.64 min, INVALID |
| Sign-off | 13.92 min (836 s) on a machine confirmed quiet |
| Card 441's gates, for comparison | 14.05 and 15.28 min |

The fix gate's run was invalid because play-tracker's lock PID was dead, so the helper printed free while three of play-tracker's testhost processes were still running. Unrelated classes ran 1.5 to 2 times slower in that run, while the two newly isolated classes ran faster.

**Pushes.**
- Every push was read unpiped, with the remote head checked equal to the local commit (D260).
- Three card-branch pushes were refused for a stale MOB.md, at 4 non-MOB commits. A mid-card branch accumulates gate and record commits.

**Constraint.** First, the cross-mob heavy-run lock: the waiting itself, and a lock that reads free while the other mob's tests still run. Second, the facilitator's own idle while light work waited behind that lock.

**Idle and nudges.**
- Idle minutes over 10 with work available: one stretch of about 65 min, before the 16:51Z watchdog.
- Dispatch nudges since the kickoff: 1 (the 16:51Z watchdog).

## What went well

- Every seat declared its service, exact model and reason before launch (D266). The actual model per seat is recorded in `model-allocation.md`. There was no mid-card model change.
- The green seat PORTED the reference both times, in under 2 minutes each. The facilitator checked each changed rule against the reference, and there was no takeover.
- The fix round's pattern changes took the worst case at 80k characters from 37.5 s to 3 ms. Review round 2 measured 40 shapes as linear up to 320k characters.
- No existing row changed its outcome in any round: 160 corpus rows, 196 fix rows and 23 committed 417 rows.
- The facilitator caught both an invalid clean-clone reproduction (a long-path checkout failure that silently reran in the worktree) and an invalid [S] reading (machine load), and reran both.
- The [S] check ran on a quiet machine and stayed under the ceiling with all nine classes isolated.

## What went wrong

- **Two drivers in one worktree.** The red seat's lock watcher woke up and built in wt-430 while the Cursor green seat was editing the same tree. Nothing was committed and no record was affected. Lesson: stop a waiting seat before the facilitator takes over its runs.
- **A recursive delete.** The review round 2 seat ran `rm -rf bin obj` in its own new probe folder. It deleted nothing, because the folders did not exist yet, and the seat reported it itself.
- **The lock helper's liveness check** read play-tracker's lock as free on a dead PID while their tests still ran. That cost one invalid 22 min heavy run.
- **The first clean-clone reproduction was invalid.** A long board filename failed the checkout, and the `&&` chain then ran the tests in the worktree's directory instead.
- **The gate summary's grep missed the pass counts again,** for the normal logger this time. The counts came from the filter logs.
- **Idle facilitator.** About 65 min passed with light work available, the red's own filtered runs, before the watchdog.

## Model allocation review (input for the voices)

See `.mob/records/430/model-allocation.md`.

| Seat | Service and model | Work and outcome |
|---|---|---|
| Architect, ctx-1 and ctx-1b | claude opus | Slice and fix slice. Both reference changes held with no outcome change. |
| Red, ctx-2 and ctx-2b | claude sonnet | Red and fix red. Rows exact both times, 0 expectation errors. The round 0 seat never ran its own runs because of the lock, and its late watcher caused the two-driver event. |
| Green, ctx-3 and ctx-3b | cursor gpt-5.3-codex, pinned | Two ports, each under 2 min, equal to the reference. The CLI never reports a model (F-13). |
| QA, ctx-4 and ctx-4b | claude opus | Round 1 found the two older real leaks plus the new super-linear time; round 2 found the leading-whitespace token form. The round 2 seat made one rule breach. |
| Sign-off, ctx-5 | claude sonnet | Waited for a quiet machine itself and judged six of six. |

## Retro voices, declared before launch

| Voices | Service | Model, pinned | Reason |
|---|---|---|---|
| product manager, audit lens, coach | agy (CLI, a fresh context each) | gemini-3.8-flash-medium (`--model`) | reflective judgement on committed records, mid tier |
| architect, qa, dev, decider | claude (subagent, a fresh context each) | sonnet = claude-sonnet-5-5 (the Agent model flag) | the same, mid tier; two services so the voices are not one model |
