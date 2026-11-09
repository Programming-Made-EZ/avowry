# Card 303 task-force declaration proposal (PM)

Card: the C# baseline endpoint tests pass on the Windows development machine (bug, Contract shape, Draft). It is owned for card 217's Done check 3 (F-065): 217 closed with these two tests as the only owned local failures. The card text is the issue as read with `gh issue view 303` at HEAD 2efe36c.

All numbers below come from committed HEAD 2efe36c. `git status --short` listed untracked files only, and I changed nothing.

## What I ran and read
- **The one allowed run.** `dotnet test tests/Phrase.Acceptance.Tests --filter "FullyQualifiedName~CSharpBaselineAcceptance.endpoint_reply_passes_when_response_record_and_status_match"`, with TEMP and TMP set to C:/Users/<user>/AppData/Local/Temp/claude/ready303. Output lines: `Failed ... [36 s]`, `endpoint-correct-v1: failed 020-endpoint-greetings`, and a result JSON with `"attempts":3`, `"diagnostics":["tests_did_not_run","tests_did_not_run","tests_did_not_run"]`, `"model_test_count":0`, `"harness_test_count":0`, `"status":"completed"`, `"proof_level":"none"`. Final line: `Failed! - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 36 s`. About 12 s per attempt is too fast for a network restore.
- **Where `tests_did_not_run` comes from.** `git grep -n tests_did_not_run` finds the producer at tools/Phrase.BenchCs/Program.cs:247. The label is only reached after `results.trx` exists. If the file is missing, line 245 returns `CompactFailureSummary(output)` instead. So the generated project restored and built, and `dotnet test` wrote a trx. The label then means harness tests found are fewer than the outcome count (`result.HarnessTests < outcomes.Count`). Here the count is 0.
- **Why the diagnostics hide the cause.** The child runs with `--verbosity quiet`. Its stdout and stderr are read but are not appended when the trx exists, so a test-host crash message is dropped.
- **Runtimes.** `dotnet --list-runtimes` gives Microsoft.NETCore.App 8.0.33 and 10.0.14, Microsoft.AspNetCore.App **10.0.14 only**, and WindowsDesktop 8.0.33 and 10.0.14. `dotnet --list-sdks` gives 10.0.403 only.

## Real suspects, ranked
1. **Missing ASP.NET Core 8 runtime (leading, not yet proven).** `EndpointVerificationProject()` (Program.cs:312) targets net8.0 with `<FrameworkReference Include="Microsoft.AspNetCore.App" />`. The machine has no Microsoft.AspNetCore.App 8.x. The plain baseline arm needs only Microsoft.NETCore.App 8, which is present, which fits the other baseline tests passing. A net8 test host cannot roll forward to the AspNetCore 10 runtime by default, so it would abort and write a trx with 0 results. I did not capture the host's own error text. That is "unrun: needs the generated project's `dotnet test` output without `--verbosity quiet`", and it is the red's job.
2. **Path or quoting, long path, Temp folder.** The scratch directory sits under the test's Temp folder, and the run with TEMP set to a short path failed the same way. Weak.
3. **Restore needing network or a NuGet source.** Weak. FastEndpoints 8.3.0 is in ~/.nuget/packages (`ls ~/.nuget/packages/fastendpoints` gives 7.2.0 and 8.3.0). The ~12 s attempts show no network wait.
4. **Antivirus or file locks.** Possible but would not give a clean 0 count three times. Check only if suspect 1 is ruled out.
5. **Line endings in generated files.** Weak. The generated files are written by `File.WriteAllTextAsync` and the other baseline tests that use the same writer pass.
6. **dotnet 10 SDK with net8 roll-forward.** This is suspect 1 seen from the SDK side. The SDK is 10 only, and the AspNetCore 8 targeting or runtime pack gap is the likely mechanism.

Whichever suspect is confirmed, the product defect is the same: a bare label that hides why. The fix may be an environment fact (install the 8.x runtime, which only the product owner can do) or a harness change. A harness change could be `RollForward=Major` in the generated project, or surfacing the host output when the count is 0.

## Task force
- **Product manager (ready):** Copilot, ctx-1. Domain: queue, scope, ready gate. Why: keeps the card to the three checks and records the ready check.
- **Architect (slice):** Codex (read-only, cmd.exe path), ctx-2. Domain: dotnet runtimes and roll-forward, the SDK against shared frameworks, MSBuild and the trx. Why: picks the probe (run the generated project's `dotnet test` verbosely) and chooses between fixing the environment and the harness.
- **QA engineer (red):** Claude, ctx-3. Domain: diagnostics-first test design. Why: writes one red per done check.
- **Developer (green):** Claude, ctx-4. Domain: tools/Phrase.BenchCs scoring. Why: Claude fills red and green. Codex takes no build seat until card 305, because its commands are not covered by the delete scan.
- **Independent reviewer (ship):** Copilot, ctx-5. Domain: git-level review. Why: it did not author. It checks that no assertion was weakened and that CI Linux is unchanged.
- **Signoff / Decider:** Claude, ctx-6, a context that did not write the green. Why: final decision and integration under the single-writer lock.

Placement facts: Grok stays out of counted windows until card 291. Whoever authors never reviews.

## Left out
- **UX, frontend, database:** no user interface or data.
- **Security:** no sandbox or product change.
- **Technical writer:** the cause goes in the card record.
- **DevOps:** the CI runner is unchanged. Installing a runtime on the development machine is the product owner's machine action, not a team seat.
- **Agile coach:** observer only, not counted.
- **Domain expert:** see below.

## Domain-expert question
Should an agent with different domain knowledge join? No for now. The suspects are dotnet runtime resolution and MSBuild, which the architect covers. Revisit at the first cycle boundary if suspect 1 is ruled out and the cause looks like antivirus or locking.

## Ready check
FIX THE CARD TEXT BEFORE READY (small edits, no re-scope):
1. **Source line.** It names ci run 42058818877 at head 2f331cd. Card 217's records use ci run 42146757413 attempts 1 and 2. Add the later run as the current evidence.
2. **Check 2 as worded does not fail on partial work.** "Pass locally, or the failure message names the cause" is satisfied by a new message alone. Reword: "The two tests pass locally. If the cause is the machine's missing runtime, the tests fail with a message naming it and the card records the product-owner install, and the suite then passes after the install." The card must name which branch applies once check 1 is known.
3. **Check 1 needs a method.** Say: "The cause is shown by running the generated project's `dotnet test` without `--verbosity quiet`, with the command and its error lines committed." Without it, the cause could be named from guesswork.
4. **Not in this card.** Add "UsableV1.Bar6 (card 162), weakening an assertion, skipping on Windows."
5. **Check 3 is vague.** State it as: "the CI run on the change passes the two tests on Linux, and the per-test outcome lists for Linux are unchanged".
6. **Repository line.** It reads "avowry (product tests: CSharpBaselineAcceptance)", which is fine. Note that the change is in tools/Phrase.BenchCs.

The three done checks are otherwise right: cause named, passes locally or a named reason, CI stays green.

## ZOMBIES, one criterion per cycle
**Cycle 1, check 1 (cause named with the command):**
- Z: no cause is found, and the card says so with the evidence.
- O: the verbose `dotnet test` of the generated project prints the host's error, and the card names it.
- M: suspects 1 and 2 to 5 are each ruled in or out by a command.
- B: the same project built with a TargetFramework of net10.0 passes, which separates runtime from code.
- I: the committed record carries the exact command and its output lines.
- E: the output is empty, and the card says that instead of guessing.
- S: one sentence names the cause.

**Cycle 2, check 2 (pass or a named reason):**
- Z: the counts are 0 and the message names why.
- O: the red is the two tests failing with a message that names the cause, not `tests_did_not_run`.
- M: both tests behave the same way.
- B: a reply that really fails still reports its failing case (`greets_ada`).
- I: the fix is in tools/Phrase.BenchCs or the generated project, not the assertions.
- E: a host crash with a trx of 0 results is reported with the host's output lines.
- S: the two tests pass locally, or fail naming the cause.

**Cycle 3, check 3 (CI stays green):**
- Z: no Linux test changes outcome.
- O: one CI run passes the two tests.
- M: two consecutive CI runs at the same head have identical outcome lists.
- B: the Linux path does not take any Windows-only branch.
- I: the CI run id is recorded in the card.
- E: a new Linux failure gets its own card.
- S: CI green, with no assertion weakened.

## Value, metric, counter-metric
Value: a local full run is green apart from the card 162 test, and a future `tests_did_not_run` carries a reason.

Metric: 2 of 2 tests pass locally, or both fail naming the cause, and 2 of 2 CI runs stay green.

Counter-metric: tests skipped or weakened to get green (must stay 0), and Linux run time (must not grow).

## What the window decides (facilitator's framing, not the seat's text)

- The task-force declaration above (roles, assignees, left out, domain-expert answer).
- The six card-text fixes of the Ready check, taken as one card-text fix in the same window (D153): the card text is edited on the issue after the window, in the words the Ready check gives.
- Facilitator's fact for the seats: the seat reported runtimes from `dotnet --list-runtimes` (no Microsoft.AspNetCore.App 8.x); the facilitator re-ran it at HEAD 2efe36c and got the same list (AspNetCore 10.0.14 only; NETCore 8.0.33 and 10.0.14; one SDK, 10.0.403), and `git grep -n AspNetCore tools/Phrase.BenchCs/Program.cs` finds the FrameworkReference at line 326 (the seat said line 312 for the function that holds it). Suspect 1 is a lead, not a finding; the slice and the red prove or rule it out. The product owner's inbox decisions of 2026-11-06 do not touch this card.
