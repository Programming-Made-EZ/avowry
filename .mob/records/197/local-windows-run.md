# Card 197 local Windows run (facilitator, rerun, isolated)

- Commit: 522c5b7 (the gate-brief commit on top of the green, red 2 and red 3; docs only above dc1193f), run in a clean `git archive` copy (no .git directory), Windows 11 Pro, dotnet 10.0.403. Start 17:04:06Z UTC.
- Command: `dotnet test Avowry.sln --filter "Status!=Pending"`
- Load: no push, no seat build and no dotnet command from any other seat ran during this run (the reviewer was held to git and python work that does not call dotnet until the done-file existed; the earlier idle build servers were shut down first with `dotnet build-server shutdown`).
- Before (baseline/gate-run-before.txt, Phrase.sln): Compiler 284 of 284 passed; Acceptance 313 of 316 passed, 3 failed.
- After:

    Passed!  - Failed:     0, Passed:   284, Skipped:     0, Total:   284, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
    Failed!  - Failed:     3, Passed:   313, Skipped:     0, Total:   316, Duration: 10 m 8 s - Avowry.Acceptance.Tests.dll (net8.0)

- Failing names after (the same three as before, under the Avowry namespace):
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.git_ignored_suite_refuses_before_the_model_is_launched [3 s]
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.repository_check_passes_here_and_fails_on_a_tracked_holdout_path [6 s]
    Failed Avowry.Acceptance.Tests.HoldoutTaskConversion.The_python_holdout_task_test_suite_passes [3 s]

- Both bench dlls were built by the solution: tools/Avowry.Bench/bin/Debug/net8.0/Avowry.Bench.dll and tools/Avowry.BenchCs/bin/Debug/net8.0/Avowry.BenchCs.dll.
- The card 304 timing guard (timed_out_model_tree_is_stopped_and_recorded_in_both_arms) passed in this isolated run.
- Totals and failing names equal the baseline; python results: tests.test_rename_197 19 OK (red 3's report), tests.test_rename_196 15 OK and the path-bound modules 100 OK (green record 1).
- Linux CI: the pushed rename (52a66e6) ran the ci workflow green against Avowry.sln; the later pushes are in the Actions list.
