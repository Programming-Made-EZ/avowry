# Proposal for Decider D252: the task-force declaration and card text for card 417 (every class that starts the bench tools shares the "Bench processes" collection, and a guard test fails when one does not)

Facilitator, 2026-11-11 UTC, main at 6d9ef5b. VOTE on this declaration: adopt the Ready card text of .mob/records/417/ready-body.md (the product manager seat's draft with the facilitator's rulings; the issue body is the same text) as card 417's Ready text, with the task force below. Card 417 is a bug (the timing test timed_out_model_tree_is_stopped_and_recorded_in_both_arms failed in the full gate of cards 368, 369 and 418 and costs a rerun of about ten minutes each time); the card 418 retro ranked it next because it puts the team's ability to ship at risk. Size S. It changes no agreement text now (the retro of this card amends the D247 entry's wording if needed); Decider protocol step 6 (D224) applies with the D232 paragraph (trigger (d) here if no agreement change: the re-run with swapped seats was not run); the D228 preflight and the D234 evidence block apply; the seat plan cites .mob/records/417/probe-all-2026-11-11.jsonl (taken 2026-11-11 06:21 UTC; available: agy, claude, cursor, grok, qwen; out: codex, copilot, muse until 2026-11-16).

STANDING-RULES PREFLIGHT: Card format rule (the scenarios cover Z, O, M, B, I, E, S, each titled with its letters); TDD red-first (the guard's live-tree test is watched failing on today's tree because five classes lack the attribute; the guard's fixture tests work on strings in memory and are green when written: declared up front); D204 (one red commit with the live-tree test Pending, then the green); D206; D219/D226 step 0 in the gate review; D222 id lists in the red record; D234 evidence block; D236 live probe cited; D243 merge verification before every gate; D245 does not apply; D247 is the entry this card automates (the guard replaces the gate reviewer's hand check); D249 applies only if the card adds a new kind of tracked file (it adds one .cs file in an existing folder: no); D251 (the toolchain stamp, by hand until gate_stamp.py exists) applies to every baseline gate and the sign-off of this card: the facilitator prints dotnet --version, python --version and the sorted names of the dotnet SDK folders before and after each run and the sign-off prompt asks the same, and a changed stamp voids the run. The product owner's account-name ruling (stricter) holds in every brief: never type, echo, search for, pass or write the local account name; leaks are counted by scripts that read the name from the environment. Consumer value: the gate stops costing a ten-minute rerun per card, so the publish chain that delivers Dana's first preview is not held up by a test that passes alone in 9 seconds.

## Facilitator rulings (main at 6d9ef5b)

- Facts: the product manager found five classes that start the bench tools without the collection (CSharpBaselineAcceptance, PrivateHoldoutResults, PhraseExecutedOutcomesAcceptance, PhraseEndpointExecutedOutcomes, PhraseCheatSheet), five that carry it, no CollectionDefinition (a misspelt name silently makes a separate collection), about 55 other classes that start git, python, the CLI or dotnet under their own limits (NOT in scope), and the failure of the timing test every time in the C# bench tool's `dotnet run` (build-capable) under load; the slice, the red and the gate reviewer recompute every figure.
- Ruling 1, scope: all five classes and whatever the guard's definition finds; the definition (a class that starts a process and names a bench tool) as drafted; the duration ceiling of 15 minutes of acceptance time per full run is provisional: the slice proposes the final ceiling after measuring per-class durations (a trx logger on one build, which the red or the facilitator's gate runs); if the five classes in one lane exceed it, the fallback (only the timing test in its own collection with parallelisation disabled, the LlmCustomerCheck pattern) is a change of D247's entry and goes through a Decider.
- Ruling 2, the seats: read-only architect slice by a fresh Claude context; the RED by a Grok WRITING seat (grok_write.py, own linked worktree); the GREEN by a Cursor WRITING seat (cursor_write.py, own linked worktree, changes left uncommitted and the facilitator committing them); red and green on different seats; no Claude stopgap; the facilitator runs every gate command in person in an isolated clone with D243 merge verification and the D251 stamp, with nothing else running during a full suite; a second read-only review (told to try to break the change with its own inputs, the card 418 lesson), an independent gate reviewer and the sign-off are fresh Claude contexts; a facilitator takeover is logged and, when it changes production code or a check, comes with its failing test committed first.
- Ruling 3, every seat brief repeats: keep each file's existing line endings; the account-name rule exactly as worded (never type, echo, search for, pass as an argument or write the local account name or any personal name, email address or home path; use <home> or placeholders; check leaks with a script that reads the name from the environment); no recursive delete; no commit trailer; a seat that finds a flaw in a test it was not asked to change stops and reports it; a seat prints the toolchain stamp before and after a full run.
- Ruling 4, the retro (seven voices at once; the retro input committed BEFORE the window) is due after the sign-off with the idle-minutes and Dispatch-nudge lines, the Evidence (per card) section and the list of drivers sidelined for behaviour; it also reads the D247 trial counter and decides whether the entry's wording is amended (the guard decides the set; the hand check is dropped).

## The card (verbatim)

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

