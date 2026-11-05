# Card 264 task-force declaration proposal (PM)

Card: the read-and-modify acceptance test (PhraseExecutedOutcomesAcceptance, task 004-read-phrase) failed intermittently on push CI on 2026-11-01 and passed on the same code. Shape: Contract, bug, corrective for open audit FAIL 123 (stop-the-line). Enables card 099.

What the test exercises: a four-row Theory (004, 005, 007, 008). Each row writes a temp suite, then its `Run` helper starts `dotnet run --project tools/Phrase.Bench` with `--disable-build-servers` and a model command that prints a file. That child compiles the reply and runs generated specs. The only wait is `WaitForExitAsync`, with no timeout in the test itself. A failure after about 21 s therefore comes from inside the child or from the machine around it.

Real suspects:
- Shared state: every row, and other classes, run `dotnet run` on the same Phrase.Bench project. Concurrent restore or build writes the same obj/bin files.
- Starvation: the pool floor in ThreadPoolFloor.cs is 128, but many classes block pool threads on child processes (the card 217 cause).
- External dependency: `dotnet` and NuGet on a CI runner.
- Timing and ordering are weaker suspects, because the test has no sleeps and rows only share the xunit collection.

## Task force
- Product manager (ready): Copilot, ctx-1. Domain: queue, scope, ready gate. Why: keeps the card to determinism of this one test and records the ready check.
- Architect (slice): Codex (read-only, cmd.exe path), ctx-2. Domain: dotnet run concurrency, MSBuild shared outputs, xunit parallelism, CI log reading. Why: ranks the suspects, fetches the 11:34 and 11:27 CI logs, and chooses the repeat-run method before any red.
- QA engineer (red): Claude, ctx-3. Domain: deterministic test design, repeat-run and load probes. Why: writes one red per done check, with a reproduction under load where feasible.
- Developer (green): Claude, ctx-4. Domain: acceptance test harness (`Run`, TempWorkspace, Bench invocation). Why: Codex cannot run shell commands on this machine, so Claude fills both coding seats.
- Independent reviewer (ship): Copilot, ctx-5. Domain: git-level review. Why: did not author. Checks that no assertion or timeout was weakened and that the root cause is named, not hidden.
- Signoff / Decider: Claude, ctx-6 (a context that did not write the green). Why: final decision and integration under the single-writer lock.

Placement facts: Claude fills every coding seat (red, green) while Codex cannot run shell commands (card 297; the machine fix awaits the product owner). Copilot and Codex (read-only) take ready, slice, ship and voting. Grok stays out of counted windows until card 291. Whoever authors never reviews.

Left out: UX, frontend, database (no user interface or data); security (no sandbox change); technical writer (the root cause goes in the card text); DevOps (a CI runner change would be a separate card); agile coach (observer only, not counted); domain expert (see below).

## Domain-expert question (refinement)
Should an agent with different domain knowledge or context join? No for now. The suspects are MSBuild, xunit and process behaviour, which the architect covers by reading the CI logs. Revisit at each cycle boundary: if the logs show a runner or NuGet fault the repo cannot reproduce, propose a CI or MSBuild specialist through the Decider.

## Ready check
FIX THE CARD TEXT BEFORE READY (small edits, no re-scope):
1. "One observable behaviour" ends mid-sentence ("timing."). Restate it as "the test passes on every repeat run, with or without parallel load."
2. The Actor names a test, which is not a system actor. Change it to the acceptance suite run by dotnet test (locally and in CI), with the test as the Observes subject.
3. Check 1 needs a fixed method. Say whether "20 consecutive runs" is the one test alone or the whole class under parallel load. Alone, it would miss the starvation and shared-build suspects, so require the class run in parallel, with the repeat-run command recorded.
4. Check 3 ("the same test or a different one") is a question, not an observable. Reword it as: "the 11:27 run's log is fetched and the failing test is named in the card. If it is a different test, a new card is carded and this one does not widen."
5. Add "Not in this card: raising timeouts to hide the failure, changing assertions, thread-pool work (card 217)."

The root cause is named by evidence, not guessed. If only the CI logs can show it and it cannot be reproduced locally, check 2 is satisfied by the logged failure plus the fix, and the card says so.

Order against card 217: follow 217 on the merits, not wait. Both are the same family. Card 217's own Context says that if the cause is thread-pool starvation, this class of flake is fixed there. But 264 has an open audit FAIL, so it cannot be parked behind a Draft card. Run 264's diagnosis first (the architect's log reading is cheap). If the logs show starvation, close 264 as covered by 217 with the evidence and move 217 up. If they show a shared-build collision or something else, 264 fixes it on its own.

Narrow run (the seat's own check): `dotnet test tests/Phrase.Acceptance.Tests --filter "FullyQualifiedName~Correct_read_and_modify_replies_pass_and_record_the_starting_code"` with TEMP and TMP set to C:/Users/ringe/AppData/Local/Temp/claude/ready264. Result: Passed 4 of 4 in 1 m (1 m 17 s wall, including the build). One run, so it proves nothing about flakiness. It shows the test is slow at about 15 s per row, which leaves little slack under load. The seat made no changes.

## ZOMBIES, one criterion per cycle
Cycle 1, check 1 (20 consecutive passes, class run in parallel):
- Z: zero failures in 20 runs; one run with no other class beside it passes.
- O: one full run of the class's four rows passes.
- M: 20 repeat runs, plus the same under parallel load from another process-starting class, all pass.
- B: the slowest run stays inside the child's window; a run with two builds started at once still passes.
- I: the repeat command is recorded in the card and runs the same way locally and in CI.
- E: a run that fails is reported with the test name and run number, so a red repeat check names what failed.
- S: the plain repeat loop returns 20 of 20.

Cycle 2, check 2 (root cause named, red before fix):
- Z: no failing reproduction found: the card states that and names the evidence instead.
- O: one named cause, with a reproduction that fails on the tree before the fix.
- M: two suspects (shared build, starvation) each ruled in or out by a run or a log line.
- B: the reproduction forces the boundary (concurrent builds of Phrase.Bench) and fails on the unfixed tree.
- I: the fix sits in the harness (`Run`, workspace), not in the product.
- E: the fixed tree passes the same reproduction, and no assertion or timeout changed.
- S: the card text names the cause in one sentence.

Cycle 3, check 3 (the 11:27 run):
- Z: the log is not retrievable: say so, and the check is recorded as unverifiable, not passed.
- O: the log is fetched and the failing test is named.
- M: the same test as 11:34 in more than one row, or a different test.
- B: a different test leads to a new card, not scope growth.
- I: the finding is written in the card, linked to the run.
- E: the CI run is gone (retention): fall back to the commit and job record.
- S: one sentence saying same test or different.

## Value, metric, counter-metric
Value: push CI stops turning red on docs-only commits, the audit FAIL closes, and card 099 can trust its scores.
Metric: 0 failures in 20 or more consecutive parallel repeat runs, and zero unexplained red runs of this test over the next 10 pushes.
Counter-metric: runtime of the class (should not grow), and tests weakened or skipped to get green (must stay 0).

## What the window decides (facilitator's framing, not the seat's text)

- The task-force declaration above (roles, assignees, left out, domain-expert answer).
- The five card-text fixes in the Ready check, taken as one card-text fix in the same window (D153 lists a declaration and a card-text fix as objection-window decisions): the card text is edited on the issue after the window, in the words the Ready check gives.
- The order against card 217: 264 runs first, its architect reads the CI logs first, and a starvation finding closes 264 as covered by 217.

## Adopted conditions (round 1 of the window; this is the text re-polled, D145: no new demands)

Round 1: NO OBJECTION from QA, backend developer, decider and agile coach; OBJECTION from the architect and the audit lens, each with one condition. Both are added to the card's Done checks and to the declaration as written here:

1. Method and findings (architect). The repeat command, the load class, its concurrency and the timeout boundary are recorded exactly in the card record. A findings file under .mob/records/264/ maps each suspect (shared build, starvation, external dependency, timing and ordering) to cited evidence (a command with its output, or a CI log line) and a result of ruled in or ruled out.
2. Closure (audit lens). Card 264 and audit FAIL 123 stay open until the corrective change is merged on main, the same reproduction passes on it, and the root-cause evidence with its commands and results is committed in the repository. If the architect's log reading shows thread-pool starvation, the card closes as covered by card 217 only after card 217's fix has passed the same reproduction.
