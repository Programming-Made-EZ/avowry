# Card 217 sign-off (sign-off, ctx-6, fresh context)

Seat: Claude (a fresh subagent, read-only; reply recorded by the facilitator, command scan clean). Environment: Windows 11 Pro 10.0.26300, Python 3.15.1, dotnet 10.0.403 (tests are net8.0 and ran through roll-forward), HEAD 65c401e; the tracked tree was clean. It ran: `dotnet test ... --filter "StarvationProbe|NoBlockingProcessWaits|GeneratedApiSourceScan"` (44 passed, 0 failed); `python -m unittest tests.test_run_pairs_217 tests.test_outcome_extractors_217` (53 OK); `python scripts/check-run-pairs-217.py --root .` (run-pairs: OK); `python scripts/mob_process.py --skip-regeneration check` (OK, 110 closed cards, 19 records, 91 legacy); `gh run list` for every card 217 head. It did not run the whole acceptance project or the run pairs.

## Verdict: SIGNED OFF with conditions

The Done checks are met as written. The card's observable is not shown: a test's result still depends on load locally.

| # | Verdict | Evidence |
|---|---|---|
| 1 Probe | Met as worded; the probe models the pool seam and does not exercise ProcessRunner against the real suite | tests/Fixtures/StarvationProbe/Program.cs implements the adopted admission protocol (records m, clamps the worker maximum to m, counts the barrier up to min(N,m), observes the extra blocker queued, queues readiness after the barrier, releases blockers in a finally); the child never loads the acceptance assembly. The seam is ManualResetEventSlim.Wait (sync) against awaiting a TaskCompletionSource (async). tests/Phrase.Acceptance.Tests/StarvationProbe.cs covers sync at m and m+1 missing the window, sync at m-1 meeting it, async at m and m+1 meeting it, only the seam differing, and a barrier timeout. Deterministic by construction apart from the 3 s window. The red-to-green order rests on the commits (a546ebe red, 197cdb4 green); the seat did not run the probe on a pre-green tree. |
| 2 Source guard | Met, with a stated gap | NoBlockingProcessWaits.cs forbids `.ReadToEnd(`, `.WaitForExit(` with any arguments, `.Result`, `.Wait(` and `GetAwaiter().GetResult(`; it blanks comments and literals first; the interpolation-hole gap is stated in its header. The seven sites use Support/ProcessRunner.cs (3c5712e). It does not cover synchronous Process.Start, Thread.Sleep (still at Support/TempWorkspace.cs:54) or synchronous file I/O. |
| 3 Run pairs | Met as written, not in spirit | run-pairs.json and scripts/check-run-pairs-217.py pass. Local runs 1 and 2 on head 2bb2f30: 316 tests each, only the three F-065 tests failed (owners 162 and 303); CI runs 1 and 2 of run 42146757413: 315 of 315. Floor decision 'kept' with its reason. |

## Floor decision

'Kept with a recorded reason' meets the literal text of Done check 3 ('the floor is removed or kept with a recorded reason'). It does not meet the card's observable: with the floor removed (d6e8f26), local run 1 failed two card-214 tests (GeneratedApiBoundAddress.I_readiness_comes_from_the_childs_own_output... at 20 s and GeneratedApiHazards.Guard_two_parallel_starts_get_distinct_base_urls at 32 s); CI passed 315 of 315 on both no-floor attempts (run 42142783699). The remaining blocking is asserted, not identified: GeneratedApi.cs is already asynchronous (ReadLineAsync pumps, WaitForExitAsync, a RunContinuationsAsynchronously completion source), and no trx pool-state analysis, thread dump or probe against those two tests exists. Untested candidates: synchronous Process.Start (23 call sites in the project), Thread.Sleep and synchronous file I/O (27 files), and CPU or disk saturation from parallel dotnet builds, which is not thread-pool starvation at all. The record is honest (the reason says the remaining blocking is a follow-up); the card text still implies the problem is solved.

## Windows

- 217-taskforce: the eight card-text fixes are present in board/217-*.md, except that its Source and Context lines still describe the old floor story and do not state the fix of the fourth failure (2f331cd). Cards 303 and 162 are still open, so F-065 stays open.
- 217-slice: the admission protocol is implemented as adopted; caps were 1+1+1.
- 217-scope: 3a then 3b in order; the failure semantics were followed: the floor restored in 2bb2f30, 'kept' recorded, the with-floor pairs on the restored head green.

## Conditions for the closing record

1. Do not close the card as having met its observable. State: Done checks met literally; the thread-pool floor is kept; local load-independence is not shown, because two tests failed once without the floor.
2. Open a follow-up bug card: identify and remove the remaining local thread-pool starvation behind two card-214 readiness tests, then retire the floor; it starts from a trx or thread dump of those two tests, tests synchronous Process.Start, Thread.Sleep and parallel-build load, and may extend the guard.
3. The card text should change (Done check 1: the probe models the pool seam and does not run ProcessRunner against the real suite; the invariant on the floor: kept with reason, retirement moved to the follow-up; the Source line on the floor). The facilitator records the change as a proposal for the next card-text window and carries the limits in the closing comment.
4. State the guard's gaps: the interpolation hole, and no coverage of synchronous Process.Start, Thread.Sleep or file I/O.
5. Red 1 and red 2 were pushed with failing CI on main: red 1 went out with c3f93b2 (run 42126690611, a compile red, failed after 57 s); red 2 (a536e13, run 42129600123) failed at 15 m 38 s, and the in-flight line after it (run 42130921151) also failed. Every later head is green (2bb2f30, b15edf0, c684ec9, 16f2736).
6. Deviations to state: the gate reviewer ran no command and the facilitator's checks at the top of preship-gate.md are the only runs; green 3a took about 90 minutes against the 60-minute cap (cap 01:59:55); the 75-minute cap for 3b was written into an in-flight line (.mob/records/blockers/in-flight.md:517) with no window; the caps sum to 4 against the D175 limit of 3 (window 217-scope approved it); no probe over 60 minutes among the 17 in-flight lines of the card (the highest ages 50 and 46 minutes); the CI-versus-local split (Bar6 is Pending in CI; CI ran 315 tests against 316 locally; the baseline endpoint tests fail locally only); HoldoutTaskConversion.cs stays CRLF; the follow-ups are cards 302, 303 and 162 and the new card.
7. The gate overstated ('no blocking findings', SHIP) though it ran nothing: treat it as a read-only review only.
