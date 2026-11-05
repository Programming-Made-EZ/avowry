# Card 217 slice turn (architect, Codex ctx-2, read-only; commands through cmd.exe; reply extracted)

Facilitator notes (facts): the seat wrote nothing (its reply is the text below, with the Codex CLI banner, prompt echo and a trailing shell-cwd line trimmed). Its Size line says 13597 bytes; the facilitator measured 13596 before that line. The seat ran on HEAD b3a383d. D189 (numbers carry their commands) was in force: see the numeric-claims table in .mob/records/217/checklist-217-slice.md.

---

Intent: Make acceptance-process waits release worker threads while preserving every assertion, diagnostic, timeout budget, and output-order guarantee; then prove the suite is load-independent and decide the temporary pool floor.

## Premises

All commands below ran through cmd.exe on committed HEAD b3a383d.

- Blocking surface. Command: git grep -n ReadToEnd -- tests/Phrase.Acceptance.Tests followed by git grep -n WaitForExit -- tests/Phrase.Acceptance.Tests. Relevant output:
  - ExtensionMap.cs:32-34: two ReadToEnd calls and WaitForExit(120000)
  - LibraryBindingLayering.cs:32-34: two ReadToEnd calls and WaitForExit(120000)
  - LlmCustomerCheck.cs:171-173: two ReadToEnd calls and WaitForExit(180000)
  - PrivateHoldoutResults.cs:309-311: two ReadToEnd calls and WaitForExit()
  - DocumentationPrinciples.cs:31, HeadToHeadVerdict.cs:30, HoldoutTaskConversion.cs:31: WaitForExit(120000)
  Output therefore contains 15 synchronous lines forming 7 blocking sites in 7 files; 5 waits are bounded and 1 is unbounded. ReadToEndAsync/WaitForExitAsync hits were also printed but are exclusions. Command: git grep -n GetAwaiter -- tests/Phrase.Acceptance.Tests; output: none. Command: git grep -n Thread.Sleep -- tests/Phrase.Acceptance.Tests; output: tests/Phrase.Acceptance.Tests/Support/TempWorkspace.cs:54: Thread.Sleep(100).
- Parallelism. Command: git grep -n -E "Collection\(|CollectionDefinition|CollectionBehavior|assembly:" -- tests/Phrase.Acceptance.Tests. Output: LlmCustomerCheck.cs:7:[CollectionDefinition("LlmCustomerCheck", DisableParallelization = true)] and :16:[Collection("LlmCustomerCheck")]; no other output. Thus LlmCustomerCheck is the only named site isolated from parallel execution; the other 6 classes use xUnit's default parallel test-collection behavior and can execute beside other classes. Command: git grep -n -E "xunit|Microsoft.NET.Test.Sdk" -- tests/Phrase.Acceptance.Tests/Phrase.Acceptance.Tests.csproj. Output names Microsoft.NET.Test.Sdk 17.11.1, xunit 2.9.2, and xunit.runner.visualstudio 2.8.2.
- Floor. Command: type tests\Phrase.Acceptance.Tests\Support\ThreadPoolFloor.cs. Output: [ModuleInitializer] Raise calls GetMinThreads(out worker, out io), then SetMinThreads(Math.Max(worker, 128), Math.Max(io, 128)). It changes both worker and I/O minima process-wide before tests run, never lowering an initially higher value. Because SetMinThreads is process-wide and there is no API that restores an unknowable runtime default after the initializer has raised it, the default-minimum probe needs a child dotnet process that does not load the acceptance assembly.
- Test cases. Command: git grep -n Fact separately over the 7 files, plus git grep -n Theory and git grep -n InlineData for LlmCustomerCheck.cs. Output: PrivateHoldoutResults Facts at 17,116,147; LlmCustomerCheck Facts at 53,123,153, Theory at 86, InlineData at 88,89; one Fact each in LibraryBindingLayering:12, ExtensionMap:12, HoldoutTaskConversion:11, HeadToHeadVerdict:10, DocumentationPrinciples:11. Those files therefore contribute respectively 3, 5, 1, 1, 1, 1, 1 discovered cases: 13 total. Whole-project discovered-case count is unrun: this read-only seat cannot safely build or let dotnet test write build/test artifacts, and no current-HEAD list-tests artifact is committed. If 13 or the eventual whole-project total is false, green must correct the inventory before converting a site and must use the actual list-tests output in the evidence.
- Existing helpers. Command: git grep -n -E "ReadToEnd|WaitForExit|CancellationToken" -- tests/Phrase.Acceptance.Tests/Support/DotnetCli.cs tests/Phrase.Acceptance.Tests/Support/GeneratedApi.cs tests/Phrase.Acceptance.Tests/Support/PhraseCli.cs tests/Phrase.Acceptance.Tests/Support/BenchChild.cs. Output includes DotnetCli.cs:42-48 and PhraseCli.cs:64-70 starting both ReadToEndAsync operations before WaitForExitAsync(cts.Token); BenchChild.cs:27-29 does the same without a timeout; GeneratedApi.cs:234-238 starts both reads before WaitForExitAsync().WaitAsync(timeout). These establish the safe ordering but expose different result types and timeout cleanup.
- CI surface and duration. Command: git grep -n Phrase.Acceptance.Tests -- .github/workflows; output: none. Command: git grep -n dotnet.test -- .github/workflows; output: .github/workflows/ci.yml:70 runs dotnet test Phrase.sln --no-build -c Release --filter "Status!=Pending". Command: git show HEAD:.github/workflows/ci.yml; output places that command in job test, step Test. Therefore 1 CI job and 1 step run the acceptance project, indirectly through the solution. Current-HEAD CI duration is unrun: it needs CI/network, which this seat lacks; green records the two actual CI run durations and treats a material regression as a finding. The committed card-264 retro reports about 16 minutes per push, but that is historical, not a b3a383d measurement.

## Design decisions

1. Add one general Support process runner and migrate all 7 sites to it. It must start stdout and stderr ReadToEndAsync immediately after Process.Start, await exit asynchronously, then await both drains before returning an immutable (exit code, stdout, stderr) result. This preserves independent stream draining (no pipe deadlock), complete output, and the sites' existing concatenation/assertion order. Keep narrow wrappers only where their start-info construction differs.
2. Translate bounded WaitForExit(milliseconds) to WaitForExitAsync(cancellationToken), with a CancellationTokenSource using exactly the existing duration. On cancellation, kill the process tree if still running, await termination and both read tasks, then report timeout without reading ExitCode early. The unbounded site remains unbounded. Do not convert a boolean timeout result into an unhandled OperationCanceledException where that would change diagnostics; the helper should expose a typed timed-out result or a stable exception consumed by the site.
3. Implement Done check 2 as a C# acceptance source-guard, beside GeneratedApiSourceScan, because it runs in the existing gate and can name repository-relative file and line. Scan production test sources, excluding bin/obj and the guard's own fixture/text, for synchronous .ReadToEnd(, .WaitForExit( with any arguments, .Result, .Wait(, and GetAwaiter().GetResult(. Match syntax/token boundaries so ReadToEndAsync and WaitForExitAsync are allowed. Seed each forbidden form in data-driven temporary source text so the guard proves its matcher and diagnostics.
4. Put the starvation probe in a tiny child executable/test-host mode that does not reference the acceptance assembly. In the child, record the runtime's initial worker minimum, clamp maximum workers to that value for determinism, start N equal to that minimum synthetic blocking process-wait equivalents, and use a barrier to prove all N have entered before queueing a readiness continuation. Red mode uses the synchronous seam and must miss a documented short window; green mode uses the async seam and must enter the window. Exercise N=min and N=min+1 and multiple readiness waiters, release blockers in finally, and give the parent a generous watchdog.
5. If the probe cannot go red after controlling admission and worker maximum, do not tune the window until it flakes. Commit the red probe and its environment/output as the criterion finding, keep the floor, and stop for a criterion-boundary decision: either accept evidence that the runtime defeats the premise or approve a better seam. Never silently replace the required red with a source-only assertion.

## Criterion 1 — deterministic starvation probe

Given the isolated child process, when synchronous blockers occupy its worker capacity, then readiness misses its window; with the async wait seam it meets the same window.

- Z: zero blockers completes readiness.
- O: one blocker per available worker makes the synchronous seam red.
- M: min+1 blockers and several readiness waiters remain classified correctly.
- B: N=min and N=min+1 are explicit cases.
- I: the child does not load ThreadPoolFloor and prints initial/min/max values.
- E: inability to produce red is retained as evidence and stops the criterion.
- S: only the wait seam changes between red and green.

Red-first plan: add one probe file (a compile failure against the not-yet-existing async seam is acceptable, per card 264), then run it against current tree. The behavioral red fails because current synchronous process waits consume pool workers; if the first red is compile-red, the next run after adding only the seam must demonstrate the missed window before implementation changes.

Cap: criterion 1, at most 1 cycle

## Criterion 2 — no blocking waits

Given acceptance source, when the guard scans process waits, then every forbidden synchronous form is named and the committed tree has no hit.

- Z: clean fixture yields no diagnostic.
- O: each single forbidden form is caught with file and line.
- M: all current 15 synchronous lines across all 7 sites are removed.
- B: bounded WaitForExit(timeout) remains forbidden; Async suffixes remain allowed.
- I: exclusions are only generated/build output and guard fixture text.
- E: seeded .Result, .Wait(, and GetAwaiter().GetResult are caught even though current HEAD has no such process site.
- S: assertions, timeout values, and output order are byte-for-byte unchanged outside mechanical await/signature changes.

Red-first plan: add the guard plus seeded matcher cases. On current tree it fails, naming the 7 files and 15 lines. Green is one cycle: introduce the shared runner and migrate all sites together; no split or later scope window is proposed because partial migration cannot satisfy this criterion.

Cap: criterion 2, at most 1 cycle

## Criterion 3 — load-independent results and floor decision

Given a fixed HEAD and environment, when the full acceptance project runs twice consecutively locally and twice consecutively in CI, then each within-environment pair has identical per-test outcome lists; only the 3 owned F-065 failures may remain, and the floor decision is recorded.

- Z: no failure outside the allowed owner list.
- O: one run produces a complete TRX outcome list but cannot close.
- M: both local runs and both CI runs match within their environment.
- B: any second-run difference or unnamed fourth failure is a finding.
- I: evidence links cards 162 and 303 and records TEMP/TMP.
- E: every new failure gets an owner before closure.
- S: remove ThreadPoolFloor only after both no-floor pairs pass; otherwise keep it with evidence and reason.

Red-first plan: first add/execute the pair comparison check against current evidence; it fails because current HEAD has no same-HEAD pair artifacts and no justified floor decision. Then run and commit the four TRX-derived outcome lists. The facilitator's prior dc1c39c run was 278 tests in 10 m 50 s: 274 passed and 4 failed, the 3 F-065 tests plus GeneratedApiSourceScan.Guard_checked_set_of_process_start_callers, since fixed in 2f331cd.

Cap: criterion 3, at most 1 cycle

## Hostile inputs

- SEEN: redirected stdout and stderr can fill independently.
- SEEN: bounded waits expire while a child remains alive.
- SEEN: an unbounded WaitForExit exists.
- SEEN: xUnit classes execute concurrently; only LlmCustomerCheck disables parallelization.
- SEEN: the module initializer raises both minima process-wide.
- SEEN: a child can exit before producing expected files, and diagnostics must retain exit/stdout/stderr.
- INSURANCE: cancellation races natural exit.
- INSURANCE: kill throws because the process already exited.
- INSURANCE: a descendant inherits a pipe and delays EOF.
- INSURANCE: output lacks a trailing newline or contains large/binary-looking text.
- INSURANCE: source tokens appear in comments, strings, generated output, or Async method names.
- INSURANCE: the runtime's initial minimum exceeds processor count or worker injection occurs during the probe.

## Domain expert and assumption

Domain-expert answer: no additional expert joins now. Repo evidence in card 264 rules starvation out for the 2026-11-01 failure and identifies a harness masking defect plus shared-build collision; card 217 is pool hygiene and mitigation review. Revisit at the criterion boundary if the isolated probe cannot be deterministic.

Assumption: a child process with its worker maximum clamped to its observed initial minimum can make admission deterministic while representing the same sync-versus-async wait distinction. Probe: print initial/min/max, barrier-confirm every blocker entered, repeat min and min+1 cases, and require sync red/async green under one unchanged window; falsification triggers the recorded-stop path in criterion 1.

## Gate environment and whole-suite duty

Every gate record states OS, dotnet version, exact command, and short HEAD (D176), plus TEMP and TMP for full runs. Historical planning datum from the facilitator: on dc1c39c the full acceptance project ran 278 tests in 10 m 50 s, with 274 passed and 4 failed (the 3 F-065 tests and a guard since fixed). Any green touching a shared Support helper must run the whole acceptance project once before the facilitator pushes; card 264 showed that a single-class run can miss a CI-red guard.

## Must not

Do not raise a timeout, change an assertion, skip or de-parallelize a test, weaken diagnostics, or change product/generated API code. Do not change outside the acceptance harness and its Support helpers unless a finding proves product code at fault. Do not remove ThreadPoolFloor before criterion 3's local and CI no-floor run pairs are green.
Size: 13597 bytes

## Adopted condition (round 1 of the window; this is the text re-polled, D145: no new demands)

Round 1: NO OBJECTION from the product manager, backend developer, audit lens and agile coach; OBJECTION from the QA engineer and the decider, with the same condition: the N=min+1 case of the probe cannot complete admission under a worker maximum clamped to min, because a barrier that waits for all N blockers to enter never completes; specify a runnable admission protocol.

Adopted as the admission protocol of Criterion 1 (it replaces "barrier-confirm every blocker entered" and "N equal to that minimum" wherever the slice says them):
- The child records the initial worker minimum `m` and clamps the worker maximum to `m`.
- The barrier counts entries up to `m` only: the probe proceeds when exactly `m` blockers have entered (the pool's whole worker capacity), never when N have.
- Case N=m: `m` blockers enter; the barrier completes.
- Case N=m+1: `m` blockers enter and the extra blocker is queued, not entered; the probe observes it queued (it has not entered after a short settle check) and proceeds. The barrier never waits for more than `m` entries.
- After the barrier, the probe queues the readiness continuation(s) and measures whether they run inside the window: red mode (synchronous seam) must miss it; green mode (async seam) must meet it.
- Blockers are released in a `finally`, and the parent keeps its generous watchdog. A probe whose barrier does not complete within the watchdog fails with the entry count it reached (never hangs).
