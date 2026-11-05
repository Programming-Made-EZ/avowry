# Card 264 slice turn (architect, Codex ctx-2, read-only; commands through cmd.exe; reply extracted)

Facilitator notes (facts): the seat wrote nothing (its reply is the text below, with the Codex CLI banner, prompt echo and a trailing shell-cwd line trimmed). Its Size line says 14568 bytes; the facilitator measured 14567 bytes before that line. The seat ran with the prompt in the scratchpad (264-slice-prompt.txt) and read the CI evidence from .mob/records/264/ci-evidence.md. Probe: 2026-11-05 12:38 UTC.

---

Intent: Make the acceptance harness preserve and report the Phrase.Bench child's actual failure, then prove the read/modify class is deterministic under the parallel load that exposed it, without weakening the outcome contract.

## Premises run on committed HEAD

All commands below ran read-only through `cmd.exe` at `HEAD 22edff6073393e861a2fc2aa358f23e36dc8cfc8` (`git rev-parse HEAD` printed that hash).

1. **The test constructs the private history path, but the Bench child creates and writes it.** Location/size: `PhraseExecutedOutcomesAcceptance.cs:329-352,355-365` is a 24-line process helper followed by an 11-line reader; `tools/Phrase.Bench/Program.cs:813-829` is the 17-line persistence method. Command: `more +324 tests\Phrase.Acceptance.Tests\PhraseExecutedOutcomesAcceptance.cs & rg -n -C 5 privateResults tools\Phrase.Bench\Program.cs`. Output:
   ```text
   info.ArgumentList.Add("--private-results");
   info.ArgumentList.Add(Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl"));
   ...
   var history = Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl");
   foreach (var line in File.ReadAllLines(history))
   ...
   PersistResult(suiteDirectory, model, result, privateResultsPath);
   ...
   Directory.CreateDirectory(historyDirectory);
   File.AppendAllText(historyPath, serializedResult + "\n");
   ```
   Thus the test supplies and later reads the path; only the successfully running Bench child writes the row.

2. **`Run` does not fail on a non-zero child exit or a failed `dotnet run` build.** It waits, captures output, and returns the exit code. The caller reads history before asserting the exit code. Location/behaviour: helper lines 329-352; target test lines 33-37. Same command output:
   ```text
   await process.WaitForExitAsync();
   return (process.ExitCode, await stdoutTask, await stderrTask);
   ...
   var run = await Run(...);
   var row = Assert.Single(ReadHistory(suite).Values);
   Assert.True(run.ExitCode == 0, run.Stdout + run.Stderr + row);
   ```
   A build/launch-time child exit therefore becomes `DirectoryNotFoundException`, masking stdout, stderr, and exit code.

3. **Rows/classes isolate result directories and temp roots, but share Phrase.Bench build outputs.** Each row calls `TempWorkspace.Create`; its root is a GUID and `WriteSuite` puts `bench/results/history.jsonl` below it. No custom output/intermediate path exists in either project file, while every invocation names the same `tools/Phrase.Bench/Phrase.Bench.csproj`. Commands: `type tests\Phrase.Acceptance.Tests\Support\TempWorkspace.cs` and `type tests\Phrase.Acceptance.Tests\Phrase.Acceptance.Tests.csproj & type tools\Phrase.Bench\Phrase.Bench.csproj & rg -n CollectionBehavior tests\Phrase.Acceptance.Tests`. Output:
   ```text
   var root = Path.Combine(Path.GetTempPath(), "phrase-acceptance", Guid.NewGuid().ToString("N"));
   Directory.CreateDirectory(root);
   <Project Sdk="Microsoft.NET.Sdk">
   ...
   <ProjectReference Include="../../tools/Phrase.Bench/Phrase.Bench.csproj" />
   ...
   <Project Sdk="Microsoft.NET.Sdk">
   <OutputType>Exe</OutputType>
   ```
   The `CollectionBehavior` scan printed no match. Under xUnit's default, tests in one class/collection do not overlap, while different classes may; all child `dotnet run` processes use the project's default shared `obj/bin`.

4. **No other acceptance test targets the common `/tmp/phrase-acceptance` parent for deletion.** Only each workspace disposes its own GUID root. Command: `rg -n "Directory.Delete|phrase-acceptance" tests\Phrase.Acceptance.Tests`. Output:
   ```text
   tests\Phrase.Acceptance.Tests\Support\TempWorkspace.cs:11: var root = Path.Combine(Path.GetTempPath(), "phrase-acceptance", Guid.NewGuid().ToString("N"));
   tests\Phrase.Acceptance.Tests\Support\TempWorkspace.cs:35: Directory.Delete(Root, recursive: true);
   ```
   This rules against an in-repo cleaner deleting a sibling row's directory; it does not rule out runner cleanup outside the process.

5. **When the child exits before persistence, this test immediately reads a file that may not exist.** Command: `more +324 tests\Phrase.Acceptance.Tests\PhraseExecutedOutcomesAcceptance.cs`. Output:
   ```text
   await process.WaitForExitAsync();
   return (process.ExitCode, await stdoutTask, await stderrTask);
   ...
   foreach (var line in File.ReadAllLines(history))
   ```
   The 11:34 CI record confirms exactly that path: `DirectoryNotFoundException` at `ReadHistory` line 359 after the 004 row's child ended. The child's own output is absent from the CI log because the exit assertion was never reached.

6. **Starvation mitigation exists but is explicitly not a fix.** Location/size: `Support/ThreadPoolFloor.cs` is 19 lines and raises worker and IO minima to 128. Command: `rg -n -C 3 ThreadPool tests\Phrase.Acceptance.Tests`. Output:
   ```text
   // A mitigation, not a fix: the floor stays only until card 217 removes the blocking waits
   ThreadPool.SetMinThreads(Math.Max(worker, 128), Math.Max(io, 128));
   ```
   `Run` itself uses asynchronous stream reads and `WaitForExitAsync`; starvation may still come from parallel classes, but the logged failure is not a readiness timeout.

7. **CI-only scheduling pressure is unrun: it requires a CI runner.** Stop rule: do not name shared build or starvation as root cause until the deterministic forced reproduction or the recorded parallel command distinguishes them; if neither fails, record both ruled out locally and retain the CI log as the observed failure.

## Ranked suspects

1. **Missing-results-file path after an early child failure (ruled in as failure mechanism; underlying trigger open).** For: the 11:34 log is a missing `bench/results/history.jsonl` at `ReadHistory`; the child alone writes it; `Run` returns non-zero without throwing and the caller reads first. Against: the CI log omitted child stdout/stderr, so it does not identify why the child stopped. This is the strongest fixable harness defect regardless of trigger.
2. **Shared Phrase.Bench build (`obj/bin`) collision.** For: parallel classes all run/build the same project; `--disable-build-servers` disables servers, not shared output directories; isolated result GUIDs do not isolate build artifacts. Against: rows of this class do not overlap by default, sibling 007 passed, and no MSBuild collision appears in the retained log. Force concurrent cold builds before ruling it in.
3. **Thread-pool starvation (card 217).** For: card 217 records many blocking process tests and prior load-only flakes; the floor of 128 is labelled mitigation. Against: this helper is async, there is no test timeout, the failure reached `ReadHistory` after child exit, and the log is not a delayed-continuation/readiness signature. If a no-floor starvation probe reproduces this exact missing-file outcome, ownership moves to 217 only under the adopted closure condition.
4. **External dependency/runner state.** For: `dotnet run` may restore/build and runner cleanup can affect `/tmp`; CI alone failed. Against: fake model commands need no network, adjacent same-code runs passed, and no NuGet, disk, permission, or cleanup error was retained. It remains unproven.
5. **Timing and ordering inside the test.** For: load changes process/build overlap and the row takes roughly 21-28 seconds. Against: unique GUID results, same-class serialization, no sleeps, and no other repo test deletes the parent. Timing is an amplifier, not yet a cause.

## Criterion 1 - 20 repeat runs in parallel

Acceptance: the whole `PhraseExecutedOutcomesAcceptance` class passes 20 consecutive runs while one named process-starting acceptance class runs beside it; the record gives both exact commands, load concurrency, per-process timeout boundary, run numbers, and slowest duration.

- Z: zero unexplained failures; an unloaded single pass is insufficient.
- O: one complete four-row class run passes and reports all rows.
- M: 20/20 class runs pass both unloaded and with the recorded load.
- B: simultaneous Phrase.Bench builds and the slowest run remain inside the declared outer command boundary; the product test itself gains no timeout.
- I: local and CI recipes use the same filter, configuration, load class, concurrency, and result naming.
- E: any failure prints repeat number, fully qualified test/row, child exit code, stdout/stderr, and whether history exists.
- S: the committed record contains the exact commands and a machine-countable 20-success summary.

Red first: add a repeat/load evidence check that rejects a missing run, fewer than 20, unnamed load/concurrency, or an unexplained failure. It fails on the current tree because no qualifying 20-run record exists; the behavioural test may happen to pass and is not itself a reliable red.

Cap: criterion 1, at most 1 cycle

## Criterion 2 - root cause and findings

Acceptance: a committed findings file maps shared build, starvation, external dependency, timing/ordering, and missing-results/early-exit to cited command or CI evidence and `ruled in`/`ruled out`; it names one root cause, demonstrates the pre-fix failure where feasible, and the same reproduction passes after the fix.

- Z: no silent success and no guessed cause; if no local trigger reproduces, say `not reproduced locally` and use the logged failure as observation.
- O: one early child failure produces a diagnostic naming exit code/output instead of `DirectoryNotFoundException`.
- M: every adopted suspect has evidence and a disposition.
- B: failed child build/launch with no results file, plus concurrent builds under load, are exercised.
- I: the harness preserves the real child result before any history read; normal Bench persistence remains end-to-end.
- E: non-zero expected scoring exits that do write history remain valid, while unexpected non-zero/no-file exits fail with captured diagnostics.
- S: one sentence names the cause and the findings table and reproduction are committed.

Red first: add a harness-level acceptance test that invokes `Run` with a deliberately nonexistent/failing Bench project (or an injected child command that exits 42 before creating results) and expects a failure containing `42`, stdout/stderr, and the missing history path. On the current tree this requires a forced reproduction because `Run` has no injection seam; the red may first fail to compile on the proposed project/command override, then, once wired only enough to execute, fails with the current masked `DirectoryNotFoundException`. This is deterministic and does not depend on CI timing. Separately run two cold `dotnet run --project tools/Phrase.Bench` processes concurrently to test the shared-build trigger.

Cap: criterion 2, at most 1 cycle

## Criterion 3 - classify the 11:27 run

Acceptance: the card/record names run 41341801479's failure as `mob-process`, states its `test` job succeeded, links the retained evidence, and, because it is not this test, cards it separately only if not already owned; card 264 does not widen.

- Z: no claim that an uninspected workflow failure was this test.
- O: run ID, SHA, failing job, and successful test job are stated.
- M: 11:27 is contrasted with 11:34's named 004 row and missing-file stack.
- B: a different job cannot add product scope to 264.
- I: the statement cites `.mob/records/264/ci-evidence.md`.
- E: if the external log later expires, the committed evidence and SHA remain the fallback.
- S: one sentence answers "different": mob-process failed, not this test.

Red first: a record check compares the card's 11:27 statement with `ci-evidence.md` and requires the exact run/job classification. It fails on the current card text, which still says "11:27 not inspected"; no forced runtime reproduction is appropriate because the CI fact is already retained.

Cap: criterion 3, at most 1 cycle

## Hostile inputs (D167)

- SEEN - child exits before creating `results/history.jsonl`.
- SEEN - 004 row fails while sibling 007 passes in the same CI run.
- SEEN - workflow is red although the test job passed (11:27 mob-process).
- SEEN - multiple acceptance classes start child processes against one Phrase.Bench project.
- INSURANCE - child build fails with non-zero exit and useful stdout/stderr.
- INSURANCE - child exits 42 with no result directory at all.
- INSURANCE - a legitimate scoring exit 3 writes history and must not be reclassified as infrastructure failure.
- INSURANCE - two cold builds overlap on shared `obj/bin`.
- INSURANCE - runner/NuGet restore is unavailable.
- INSURANCE - temp root contains spaces; each GUID workspace remains isolated.
- INSURANCE - history exists but is empty, partial, or malformed.
- INSURANCE - load completes in a different order without changing outcomes.

## Domain expert and assumption

Domain-expert answer: no additional expert now. The repo and log narrow the immediate defect to process/MSBuild/xUnit harness behaviour: the child is the writer, the test masks an early exit by reading first, and 11:27 is unrelated. Revisit after criterion 2; invite a CI/MSBuild specialist only if the forced early-exit diagnostic shows a runner, restore, or unexplained MSBuild fault that the repo cannot reproduce.

Assumes: xUnit's default class/collection serialization keeps rows in this class from overlapping while other classes may overlap, and default SDK output paths make their child builds share Phrase.Bench `obj/bin`; probe: run the recorded load with timestamped child starts and distinct GUID result roots, then repeat with per-invocation MSBuild output isolation. If overlap is absent or isolation does not change the failure, do not use shared build as the cause.

## Gate environment (D176)

Every gate test run must open with an Environment line stating OS, `dotnet --version`, exact command, working directory, and HEAD. Current read-only probe: Windows `10.0.26300.9818`; .NET SDK `10.0.403`; cwd `C:\Users\ringe\projects\phrase-lang`; HEAD `22edff6073393e861a2fc2aa358f23e36dc8cfc8`. These are not substitute results for the future red/green gates.

## Guardrail for green

The facilitator must reject a green that raises any timeout, changes an outcome assertion, skips or filters away a test, reduces the required load/repeats, or changes code outside the acceptance-test harness and Phrase.Bench results handling unless a committed finding proves product code is at fault. It must not treat a nicer missing-file message alone as root-cause closure, nor close 264/FAIL 123 before the corrective change is merged on main and the same reproduction and evidence pass there.
Size: 14568 bytes
