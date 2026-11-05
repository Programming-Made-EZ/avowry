# Card 264 second scope proposal turn (architect, Codex ctx-2, read-only; commands through cmd.exe; reply extracted)

Facilitator notes (facts): the seat wrote nothing (its reply is the text below, with the Codex CLI banner, prompt echo and a trailing shell-cwd line trimmed). Its Size line says 5236 bytes; the facilitator measured 5235 before that line. Trigger: green 2 (a7ad8c5) fixed the masking defect and proved the shared-build collision locally (findings.md), but did not fix the collision; the card text requires the root cause fixed. The full PhraseExecutedOutcomesAcceptance class passed 17 of 17 on a7ad8c5 (facilitator run, 5 m 40 s).

---

# D175 scope proposal — card 264, criterion 2b

## Premises (D184)

Committed HEAD:

```text
> git rev-parse HEAD
e0c2a428ec62ea7a8e461ec022fb3180c8678b38
```

1. There are **five Phrase.Bench start sites in four acceptance classes**: two in `BenchmarkHarnessAcceptance.cs`, and one each in `PhraseCheatSheet.cs`, `PhraseEndpointExecutedOutcomes.cs`, and `PhraseExecutedOutcomesAcceptance.cs`. The match at BenchmarkHarnessAcceptance:77 only asserts existence.

```text
> findstr /s /n "Phrase.Bench.csproj" tests\Phrase.Acceptance.Tests\*.cs
BenchmarkHarnessAcceptance.cs:77,1500
PhraseCheatSheet.cs:274
PhraseEndpointExecutedOutcomes.cs:88
PhraseExecutedOutcomesAcceptance.cs:332
```

Inspection shows starts at BenchmarkHarnessAcceptance:1420,1506; PhraseCheatSheet:282; PhraseEndpointExecutedOutcomes:97; and PhraseExecutedOutcomesAcceptance:342.

2. The acceptance project builds Phrase.Bench through a normal `ProjectReference`, so a built test configuration has matching Bench output for `--no-build`.

```text
> type tests\Phrase.Acceptance.Tests\Phrase.Acceptance.Tests.csproj
<ProjectReference Include="../../tools/Phrase.Bench/Phrase.Bench.csproj" />
```

3. CI builds and tests **Release**. Children omit `-c`; `dotnet run` therefore defaults to **Debug**.

```text
> findstr /n /c:"dotnet build" /c:"dotnet test" .github\workflows\ci.yml
... dotnet build Phrase.sln --no-restore -c Release
... dotnet test Phrase.sln --no-build -c Release ...
> dotnet run --help
-c, --configuration ... The default for most projects is 'Debug'.
--no-build  Do not build the project before running. Implies --no-restore.
```

Thus the failing trace's `bin/Release` describes the outer test build, while an unqualified child selects Debug.

4. `--no-build -c Debug` finds output after a Debug test build; `-c Release` finds it after CI's Release build. Both exist here:

```text
> dir /b tools\Phrase.Bench\bin\Debug\net8.0\Phrase.Bench.dll
Phrase.Bench.dll
> dir /b tools\Phrase.Bench\bin\Release\net8.0\Phrase.Bench.dll
Phrase.Bench.dll
```

The clean-CI execution of the proposed behavior is **unrun**; it requires a CI runner.

## Options and recommendation

- **No-build, configuration-coupled shared launcher.** Route all five starts through one helper, derive configuration from the running test assembly, add `--no-build -c <same>`, and preflight output. Cost: consolidate five sites and test configuration/freshness. Missing or stale output fails before launch with configuration, path, and remediation; no fallback build.
- **Cross-process build-once guard.** Lock, build once, then launch no-build. Cost: portable lock naming, abandoned-owner recovery and failure handling. Missing/stale output is rebuilt once and build failure is reported; a bad lock can deadlock or serialize unrelated checkouts.
- **Per-invocation output paths.** Isolate every build. Cost: repeated compilation, path/cleanup plumbing, disk and runtime. Missing/stale output is rebuilt in isolation and errors surface, but dependency mistakes may be hidden.

**Recommend no-build with the matching configuration.** The outer test build already owns compilation. The helper must reject absent or detectably stale output clearly and must never remove `--no-build` or build in a child.

## Amendment

Add criterion **2b**, ordered **1a (done), 2 (done), 2b, 3, 1b**. Caps become criterion 1: two; criterion 2: two; criterion 3: one—sum **5**, two above D175's limit of 3. The window is asked to approve the total two-cycle excess: this additional criterion-2 corrective cycle while retaining the already approved criterion-1 split.

Deterministic red for 2b:

1. a helper/acceptance test proves every Bench child gets `--no-build` and the running test assembly's configuration; current launchers fail;
2. missing and stale matching output each fail clearly without invoking a build; and
3. the forced concurrent-build reproduction, `python .mob/records/264/diag-concurrent-builds.py`, is red before the fix and passes afterward with `anomalies=0`. It may be revised only to exercise the production launcher instead of its current raw building command.

Green may touch only the acceptance harness and shared helper. No timeout, outcome assertion, or skip/filter changes; no weakened reproduction. Phrase.Bench stays unchanged unless a new finding proves it at fault.

## Effect on 1b

The 20-run batch runs on the post-2b tree. Target, load, concurrency, timeout boundary and failure semantics are unchanged: every failure is retained and reported; no failed sequence is replaced or relabelled.

## Reversal triggers and cost

Reverse if configuration cannot be derived on both runners, a clean CI ProjectReference build lacks selected output, freshness cannot be rejected without false positives, or the forced reproduction still shows a build/collision. Then use the build-once guard; use isolated artifacts only if locking is demonstrably unsafe.

Cost: one bounded cycle to consolidate five sites, add missing/stale/configuration reds, run the seven BenchRunHarness tests and 17-test class, and rerun the six-child diagnostic. Reversal costs another scope window plus replacement implementation and reproduction; 1b waits for the replacement fixed tree.
Size: 5236 bytes
