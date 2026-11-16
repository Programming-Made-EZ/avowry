# Card 422 retro input (facilitator, from the card's records; committed before the voices)

All times are UTC, from the commit log and the seats' start and done files.

## Flow metrics (first: constraint first, D258 clause 4)

**Lead time.** From the kickoff (D268, 20:14Z) to the sign-off (23:48Z, sign-off 34eacd9; merged 1354953) took about 3 h 34 min. That covered one red and green and TWO fix rounds. Of that, the work was done by 22:32Z, about 2 h 18 min. The rest was the sign-off waiting on play-tracker's heavy lock (#1069, held from 22:51Z to 23:28Z).

**Time by step.**

| Step | Time |
|---|---|
| Kickoff (probe, card template, D268) | about 6 min |
| Slice (Opus) | about 23 min |
| Red | about 10.5 min |
| Green port (Cursor) | 2 min 55 s |
| Gate (round 0, quiet machine) | about 15 min |
| Review round 1 (Opus) | about 11 min |
| Fix slice 1 | about 14 min |
| Fix red | about 8.5 min |
| Fix green | 1 min 30 s |
| Fix gate | 17 min |
| Review round 2 | about 9 min |
| Fix round 2 (Sonnet) | about 2.5 min |
| Final gate with the depth-1 Python run | about 15 min |
| Review round 3 | about 10 min |
| Sign-off | about 52 min in all: about 38 min waiting on the lock, then a 19.8 min gate, after waiting on the lock |

**Rounds.**
- Review BLOCKING by round: 2, 1, 0. MINOR by round: 10, 1, 3.
- Card 430 had 2, 0; card 441 had 3, 2, 0.
- Round 1's two BLOCKING findings were not recognition gaps. The [E] and [O] helpers were ported, but nothing on the networked path called them. The facilitator's green brief named no caller, and the red's rows tested the helpers in isolation.
- Round 2's one BLOCKING finding was a new test row that failed on a shallow checkout, which is the shape CI uses. The laptop gates ran in full clones and could not see it.

**D265, third trial.** The slice commits a corpus covering each encoding before the red, including whitespace and line-break forms; each fix slice commits neighbour variants before its red.
- The slice committed 176 rows in 6 families and 25 encodings before the red. The old checks wrongly passed 81 of them.
- Fix slice 1 committed 83 neighbour rows before its red, 39 failing on the round 0 code. Seven of them read the source for the calls themselves.
- No BLOCKING finding in any round was a recognition gap of the corpus families or encodings. Both of round 1's were wiring; round 2's was a test-design gap against the CI shape.
- Mutants: 37 of 37 killed, then 27 of 27.

**Heavy runs.**
- Four gates ran on a quiet machine under Avowry's lock: the round 0 gate, the fix gate, the final gate and the sign-off. There were no invalid runs; the quiet check (lock free and no play-tracker testhost running) held.
- Acceptance project wall time: 14.5, 17.0 and 13.9 min.

**Pushes.** All were read unpiped, with the remote head checked equal to the local commit (D260).

**Constraint.** Not the heavy lock this time; the work finished before the lock was taken. It was the seams between seats:
- a brief that left the wiring unnamed;
- a gate whose clone shape differed from CI's.

**Idle and nudges.**
- Idle minutes over 10 with work available: one stretch of 37 min, before the 23:20Z watchdog. The facilitator waited on the sign-off seat while it waited on the lock, instead of starting light work.
- Dispatch nudges since the kickoff: 4 (the owner's dispatch audit, the 23:20Z watchdog, the Muse question and the rotation-pool addendum).
- Owner rulings that landed during the card: the service rotation pool (eight services, codex in) and the blocking kickoff probe. They are filed as mob-defaults #61 (every seat logs a row with its model and outcome) and #62 (the kickoff probe and refusal).

## What went well

- Every seat declared its service, exact model and reason before launch (D268), and every actual is recorded.
- The fix-slice design (pure functions plus source-scan wiring rows, with the red doing a behaviour-preserving rewire first) made the wiring impossible to skip. Fix round 1 then closed both BLOCKING findings in one pass.
- The facilitator checked each port against the reference: a corpus evaluator over the ported Python found 0 mismatches.
- The reviewer in round 2 found a CI-only failure that the laptop could not show. The gate now runs the Python module in a depth-1 clone.
- Light work went ahead of heavy runs, and no seat waited on the lock except the sign-off. D267 held: no two drivers in one tree.

## What went wrong

- The facilitator's green brief named no caller for the helpers. That cost one fix round.
- A new test row depended on git history, which would have turned CI red on main.
- The architect used one shell heredoc on its own probe file, and the review seat left four throwaway folders it could not delete.
- The facilitator was idle for 37 min while the sign-off waited on play-tracker's lock.

## Model allocation review (input for the voices)

See `.mob/records/422/model-allocation.md`.

| Seat | Service and model | Work and outcome |
|---|---|---|
| Architect, ctx-1 and ctx-1b | claude opus | Slice and fix slice, both with no unintended outcome change. |
| Red, ctx-2, ctx-2b and ctx-2c | claude sonnet | Three reds, including the behaviour-preserving rewire and the test-design fix. 0 expectation errors, but one row failed on a shallow checkout. |
| Green, ctx-3 and ctx-3b | cursor gpt-5.3-codex, pinned | Two ports, each under 3 min, equal to the reference. The CLI never reports a model (F-13). |
| QA, ctx-4, ctx-4b and ctx-4c | claude opus | 2, 1, 0 BLOCKING. Round 1 found the unwired helpers; round 2 found the CI shape. |
| Sign-off, ctx-5 | claude sonnet | Waited for a quiet machine itself (no watcher left), ran the gate with the depth-1 Python run and the networked F5 and F6, and judged seven of seven. The [O] CI observation on main after the merge is pending by design. |

## Retro voices, declared before launch (the eight-service pool ruling; the probe of 2026-11-16 00:05:46 UTC)

| Voice | Service | Model, pinned | Reason |
|---|---|---|---|
| product manager | muse (muse_write.py, a fresh context in a throwaway folder) | muse default, `--reasoning-effort medium`, 60 steps | back after its quota reset (00:05 probe ok); relieves agy; reflective judgement, mid tier |
| audit lens | qwen (local_driver.py, under Avowry's heavy lock while the gate is idle) | qwen local model from core/local_llm.json | probe ok and the gate idle; its first seat in more than four days |
| coach | agy (CLI, a fresh context) | gemini-3.8-flash-medium (`--model`) | mid tier, one voice only, to spread the load |
| dev | cursor (cursor_reply.py --mode ask, a fresh context) | gpt-5.3-codex (`--model`) | read-only ask mode; a different family from the claude voices |
| architect, qa, decider | claude (subagent, a fresh context each) | sonnet = claude-sonnet-5-5 (the Agent model flag) | mid tier |

Not seated, each with its probe evidence: codex and copilot (quota reached at 00:05), grok (unavailable, a 402 balance error).
