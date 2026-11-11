<!-- phrase-card:417 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** bug
- **Repository:** avowry (tests/Avowry.Acceptance.Tests)
- **Source:** issue 417 (card 369 retro, Decider D247) and the retros of cards 368, 369 and 418: the timing test failed in the full gate of three cards in a row; ranked next by the card 418 retro (a bug that puts the team's ability to ship at risk).
- **Depends on:** none
- **Story:** As Dana, I want the release gate's full test run to pass on its first attempt, so that the first preview I am waiting for is not delayed by a ten-minute rerun on every card.
- **Consumer value:** the gate stops costing a ten-minute rerun per card, which happened on cards 368 and 418. So the publish chain that delivers Dana's first preview is not held up by a test that passes alone in 9 s.

**Definition** (one rule the guard enforces and the slice cites): A test class file "starts the bench tools" when both of these are true:
- **It starts a process.** With comments and strings blanked, its code calls `Process.Start(`, `ProcessRunner.RunAsync(` or `BenchChild.RunAsync(`.
- **It names a bench tool.** With only comments blanked, so string literals still count, it contains `BenchLauncher.` or the project name `Avowry.Bench` or `Avowry.BenchCs`.

Such a class must carry exactly `[Collection("Bench processes")]`. Extra classes that carry the attribute, such as the two `RestoredPackageLicences` classes, are allowed.

**Scenarios**
- **Scenario: [Z] no class starts the bench tools.** Given a source set where no class both starts a process and names a bench tool; When the guard runs; Then it passes and lists nothing.
- **Scenario: [O][E] one class lacks the collection.** Given one class that calls `BenchLauncher.CreateStartInfo` and starts a process with no collection attribute; When Dana runs the guard; Then the command exits non-zero and its output names that class's file and class.
- **Scenario: [M] today's tree.** Given the repository as it is today; When Dana runs the guard with its Pending trait removed; Then it fails and names exactly CSharpBaselineAcceptance, PrivateHoldoutResults, PhraseExecutedOutcomesAcceptance, PhraseEndpointExecutedOutcomes and PhraseCheatSheet, one line each.
- **Scenario: [B] near misses.** Given four classes:
  - one that names `Avowry.BenchCs` only inside `RepoPaths.Find("tools", "Avowry.BenchCs", ...)` and starts `dotnet run`;
  - one that carries a different collection name (`"Bench Processes"` or `"LlmCustomerCheck"`);
  - one that names a bench tool but starts nothing (as in PathScrubForms and BenchLauncherContract);
  - one that starts `sh` and names a bench tool only in a comment (as in BenchRunHarness);

  When the guard runs; Then the first two are named, and the last two are not.
- **Scenario: [I] the guard's own text.** Given the guard's source file, whose search strings and fixtures name what it forbids; When the guard runs; Then its own file is not named, and `GeneratedApiSourceScan.Guard_checked_set_of_process_start_callers` and `NoBlockingProcessWaits` still pass.
- **Scenario: [S] all classes share the collection.** Given the five classes carry `[Collection("Bench processes")]`; When Dana runs `dotnet test Avowry.sln --filter "Status!=Pending"` alone; Then it exits 0, with `timed_out_model_tree_is_stopped_and_recorded_in_both_arms` passing in the full run, and the recorded acceptance duration within the agreed ceiling.

ZOMBIES coverage: Z, O, M, B, I, E and S all appear in the scenarios above. E is covered by the guard's failure output, which names the class.

**Acceptance criteria**
1. **The guard exists and is offline and build-free.** The new file `BenchProcessClassesShareOneCollection.cs` (Slice 417) scans `tests/Avowry.Acceptance.Tests/**/*.cs` as text, with no build and no process.
   - It applies the definition above and requires the exact collection name.
   - It names each offending file and class.
   - Its fixture tests cover every row of the B scenario.
   - It fails if it catches the `BenchLauncher` form but misses the literal project-path form.
2. **All five classes carry the attribute, and the guard runs ungated.** CSharpBaselineAcceptance, PrivateHoldoutResults, PhraseExecutedOutcomesAcceptance, PhraseEndpointExecutedOutcomes and PhraseCheatSheet each carry `[Collection("Bench processes")]`. The guard's live-tree test has no Pending trait and passes. Adding only the two classes the original bug named still fails it.
3. **The full gate passes and stays within the ceiling.** Two full runs alone of `dotnet test Avowry.sln --filter "Status!=Pending"`, on the final head in a fresh clone, both exit 0. Each record states the acceptance duration and the timing test's result, and the duration is within the ceiling the facilitator sets.

**Red-first plan**
- **Red:** add the guard file.
  - Its fixture tests (Z, O, B and I) work on strings in memory and are green when written; declare them so.
  - Its live-tree test carries `[Trait("Status", "Pending")]` and fails by assertion, naming the five classes.
  - Build its search strings by concatenation, as `GeneratedApiSourceScan` does. Otherwise that file's `Process.Start` allow-list breaks.
- **Green:** add the attribute line, with a D247 comment, to the five classes, and remove the Pending trait. No production code changes.

**Tests that must keep passing**
- The whole gate.
- `BenchmarkHarnessAcceptance`, especially the timing test.
- The five moved classes, including `PrivateHoldoutResults`' Criterion 368-2.
- `NewBenchRowsCarryNoReply`, `BenchRowsHoldNoFolderPath`, both `RestoredPackageLicences*` classes and `LlmCustomerCheck`.
- `GeneratedApiSourceScan`, `NoBlockingProcessWaits`, and the tree scans `NoPhCodesInTheLiveTree`, `NoAccountNameInTrackedFiles` and `NoHomePathsInTrackedFiles`.


**Not in this card:**
- changing the 20 second limit;
- making the C# arm or CSharpBaselineAcceptance use `--no-build`;
- serialising the roughly 55 classes that start git, python, `PhraseCli`, `DotnetCli` or `GeneratedApi` under 1–5 minute limits;
- changing LlmCustomerCheck's own collection;
- any production code.

**Size:** S. One guard file of about 150 lines plus five attribute lines. The cost is mostly gate time.

**Done when:**

```bash
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --filter "Slice=417" && dotnet test Avowry.sln --filter "Status!=Pending"
```

Run it alone, twice, in a fresh clone.

- **Rulings (facilitator, 2026-11-11):** (1) all five classes the product manager found are in scope, and the guard's definition decides the set (the D247 entry named two: it is amended in the retro of this card to say the guard decides); (2) the definition of "starts the bench tools" is confirmed as drafted (the narrow set: a class that starts a process and names a bench tool); the roughly 55 other classes that start git, python, the CLI or dotnet under their own limits are NOT serialised; (3) the duration ceiling of criterion 3 is provisional: 15 minutes of acceptance time per full run (today about 10); the slice first measures per-class durations with a trx logger on one build and proposes the final ceiling and, if the five classes in one lane exceed it, the fallback: only the timing test in its own collection with parallelisation disabled (the LlmCustomerCheck pattern), which would be a change of D247's entry to be put through a Decider; (4) the root cause is a guess until the slice confirms or rules it out (concurrent build-capable `dotnet run` of the C# bench tool from several classes); the red seat does not depend on it; (5) persona Dana; size S; D251 applies to every gate and the sign-off of this card (the toolchain version printed before and after, by hand until gate_stamp.py exists); the account-name rule (stricter) is in every brief.

