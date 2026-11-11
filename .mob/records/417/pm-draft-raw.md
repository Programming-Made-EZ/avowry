I've drafted the card. The main finding is that the bug undercounts the overlap. Five bench-launching classes lack `[Collection("Bench processes")]`, not two. The bug's literal wording would also cover about 55 more classes, because almost every helper sets a 2–5 minute hard limit. So the card needs a narrower, safe definition.

## (1) Key facts verified

**The timing test and why it fails**
- `timed_out_model_tree_is_stopped_and_recorded_in_both_arms` is at `BenchmarkHarnessAcceptance.cs` lines 15–72. It checks the 20 second limit at line 48. `RunBenchmarkArm` (lines 1441–1464) sets a 20 second cancellation at line 1453 and fails with "did not finish within 20 seconds" at line 1461.
- The test's C# arm goes through `BenchLauncher.CreateStartInfoFor("Avowry.BenchCs")` (`Support/BenchLauncher.cs` lines 45–60). For Avowry.BenchCs that keeps a `dotnet run` that can build. Only Avowry.Bench gets `--no-build`.
- `CSharpBaselineAcceptance.cs` lines 925–955 run the same build-capable `dotnet run --project Avowry.BenchCs`, with no limit in the test. `PrivateHoldoutResults.cs` lines 272–296 do the same for both arms. Every recorded failure names Avowry.BenchCs: card 368 `signoff-attempt-1.md` line 3, and card 418 `gate-green-fix-note.md` line 3 ("Avowry.BenchCs did not finish within 20 seconds", 36 s).
- Alone, the test passes in 9 s, twice (`.mob/records/418/gate-green/timing-test-alone.txt` lines 3–5). Recorded full runs took 10 m 26 s (card 368), 11 m 9 s and 9 m 47 s (card 418).
- So "under a hard time limit" describes the victim, not the two named classes. CSharpBaselineAcceptance and PrivateHoldoutResults add load; they are not time-limited themselves.

**Classes with `[Collection("Bench processes")]` today**

| File and line | What it starts |
|---|---|
| `BenchmarkHarnessAcceptance.cs:9` | the bench tools |
| `NewBenchRowsCarryNoReply.cs:8` | the bench tools (via ProcessRunner at line 424) |
| `BenchRowsHoldNoFolderPath.cs:8` | the bench tools (via ProcessRunner at line 889) |
| `RestoredPackageLicences.cs:9` | python or `dotnet restore`, 120 s limit at line 89 |
| `RestoredPackageLicencesLockedRestore.cs:11` | `dotnet restore`, `RestoreLimit` at line 168 |

No `[CollectionDefinition("Bench processes")]` exists anywhere. xunit creates the collection implicitly by name, so a misspelt name silently becomes a separate collection.

**Classes that start the bench tools without the collection (five, not two)**
- `CSharpBaselineAcceptance.cs`: `Process.Start` at lines 934 and 950; 44 test rows and 26 bench runs.
- `PrivateHoldoutResults.cs`: `CreateStartInfoFor` at line 275, `Process.Start` at line 291.
- `PhraseExecutedOutcomesAcceptance.cs`: `BenchLauncher.CreateStartInfo` at line 332, `BenchChild.RunAsync` at line 344.
- `PhraseEndpointExecutedOutcomes.cs`: `BenchLauncher.CreateStartInfo` at line 88, `Process.Start` at line 95.
- `PhraseCheatSheet.cs`: `BenchLauncher.CreateStartInfo` at line 274, `Process.Start` at line 299.

**Classes that start other processes under a hard limit, without the collection**
- **Through ProcessRunner with a limit:** `NoPhCodesInTheLiveTree` (git, 60 s), `NoAccountNameInTrackedFiles`, `TreeScanGatesByExitCode` (300 s), `AccountNameWithASpace` (300 s), `NoHomePathsInTrackedFiles` (300 s), `PatternDocs`, `PreCommitRefusesAHomePath`, `PrePublicationOwnerItems`, `LicenseFiles`, `LibraryBindingLayering`, `HoldoutTaskConversion`, `HeadToHeadVerdict`, `ExtensionMap`, `DocumentationPrinciples` (120 s each), `InstallFromAnEmptyFeedFailsWithoutReachingAPublicSource` and `PackedToolIsVersionedInstallsAndBuildsTheDemo` (5 min each), and `StarvationProbe` (90 s watchdog, line 28).
- **LlmCustomerCheck** (python, 180 s at line 170) has its own collection, created with `DisableParallelization = true` (`LlmCustomerCheck.cs` lines 7 and 16).
- **Through `DotnetCli.TestAsync`** (5 min limit, `Support/DotnetCli.cs` line 15): `PutAndDeleteEndpointsCompile`, `UsableV1`, `EdgeCaseFixtureCorpus`, `TasksDemoApp`, `RecordUpdateFormCompiles`, `ReferenceTarget`, `PersistenceEffectCompiles`, `OneNumberPhraseCompiles`, `OrdersPhraseCompiles`, `NativeCSharpBlockCompiles`, `MatchingCodebases`, `ListFieldCompiles`, `LeafPhraseFailureCompiles`, `GetEndpointsCompile`, `GeneratedCodeUsesAvowryNames`, `EverySpecOutcomeAsserts`, `EnsureStepCompiles`, `DeclaredEffectDependencyCompiles`, `DeclaredRecordCompiles`, `CompositePhraseOrderCompiles`, `CompileEndpointToCSharp`, and `GeneratedSpecsMutationCheck` (via `Support/CSharpMutationCheck.cs` line 60).
- **Through `PhraseCli.RunAsync`** (5 min limit, `Support/PhraseCli.cs` line 17): `UndeclaredNativeFail`, `UnsupportedCsharpConstructs`, `ReservedGeneratedNames`, `PhraseBuildOneFile`, `CompositeReturnTypeStaysAV325`, `CompilerMessagesNameTheAvow`, `CliKeepsDiagnosticShape`, `AvPrefixedDiagnostics`, `TasksDemoGeneratedCsharp`, `TasksOverHttp`, `SourceMapTraceability`, `SourceMapDeployment`, `SourceMapTraceCommand`, `ProductNamedToolBoundary`, `PhraseCliFreshness`, `AvowryCommand`.
- **Through `GeneratedApi.StartAsync`** (2 min startup limit): `TasksOverHttp`, `GetEndpointsCompile`, `PutAndDeleteEndpointsCompile`, `SourceMapDeployment`, `EndpointFailureMapsToHttpStatus`, `GeneratedApiBoundAddress`.
- **Direct `Process.Start`** (I did not check whether each has a limit): `PrePublicationChecks`, `GettingStartedDocs`, `PrerequisitesPage`, `TasksDemoReadme`, `MobProcessAccountability`.

**Classes that name a bench tool but start no process (must not be flagged)**
- `PathScrubForms.cs` lines 121–122 only read files.
- `BenchLauncherContract.cs` only builds start info against a temporary layout.
- `BenchRunHarness.cs` starts `cmd.exe` or `sh` (line 169) and names Avowry.Bench only in a comment.

**Existing scan patterns the guard can copy**
- `NoBlockingProcessWaits.cs`:
  - It blanks comments and string contents but keeps newlines, so line numbers stay true (lines 9–11 and 176–242).
  - It skips `bin`, `obj`, `.git`, `node_modules` and `TestResults` (line 15), and walks the project through `RepoPaths.Find` (lines 162–174).
  - It has a self-check that it does not flag its own file (lines 124–129).
- `GeneratedApiSourceScan.cs`:
  - It builds its search strings by concatenation, for example `"Process" + ".Start("` at line 15, and excludes its own file by name (lines 12 and 109).
  - It keeps an exact allow-list of 23 `Process.Start` callers (lines 22–30), checked by `Guard_checked_set_of_process_start_callers` (lines 78–90).
  - A new guard file containing that literal string would break this check.

**History**
- `TEAM_WORKING_AGREEMENT.md` lines 519–520 hold the entry; its trial runs for three cards.
- `docs/decisions/process-and-staffing.md` lines 1400–1404 record Decider D247.
- Card 368 retro, lines 11 and 38: the timing test failed in two of four full runs.
- Card 369 retro, line 31: trial counter 1 of 3, no timing failure that card.
- Card 418 retro, line 35: it failed in two baseline gates; all seven voters want bug 417 pulled ahead.

**Run-time cost**
- xunit already runs the tests inside one class one after another. The parallel unit is the collection, and by default each class is its own collection.
- Joining classes into one collection makes that lane's time the sum of the classes' times. The suite can never finish faster than its longest lane.
- No record holds per-class durations; the gate logs are summaries only, so I can't give measured numbers.
- The heavy bench classes are BenchmarkHarnessAcceptance (44 launches), CSharpBaselineAcceptance (26 build-capable runs, each scoring with a nested build and test) and PhraseExecutedOutcomesAcceptance (20 rows, executed scoring plus `DotnetCli`).
- **Five bench classes added (the narrow set):** worst case, the collection lane becomes longer than today's 10 minutes and sets the suite time.
- **All roughly 60 classes (the literal wording):** worst case, most of the suite runs serially, many times 10 minutes. That option should be rejected.

## (2) Revised card

**Bug 417: every class that starts the bench tools shares the "Bench processes" collection, and a guard test fails when one does not**

As Dana, I want the release gate's full test run to pass on its first attempt, so that the first preview I am waiting for is not delayed by a ten-minute rerun on every card.

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

**Consumer value:** the gate stops costing a ten-minute rerun per card, which happened on cards 368 and 418. So the publish chain that delivers Dana's first preview is not held up by a test that passes alone in 9 s.

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

## (3) Open points for the facilitator
- The overlap is five classes, not two. Confirm the card covers all five; the D247 entry names only two.
- Confirm the "starts the bench tools" definition above. The literal "any external process under a hard time limit" covers about 60 classes and would serialise most of the suite.
- Set the duration ceiling for criterion 3. Per-class durations were never recorded, so the slice should first measure them with `--logger trx` on one build.
- There is a cheaper alternative to weigh: move only the timing test into its own collection created with `DisableParallelization = true`, the pattern at `LlmCustomerCheck.cs` line 7. That costs about 10–20 s instead of a longer serial lane, but it would amend D247.
- My root-cause guess is concurrent build-capable `dotnet run` of Avowry.BenchCs from three classes, because every failure names that tool. I have not verified it; the slice should confirm it or rule it out.
- Persona: I kept Dana, because she "judges a slice by one command's exit" (`PERSONAS.md` line 23). Confirm this passes `scripts/check-story-persona.py`.
- Closing this card makes the guard the automated check D247 asked for. The gate reviewer's hand check can then be dropped at the trial's third-card review.
