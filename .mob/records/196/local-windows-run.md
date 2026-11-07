# Card 196 local Windows run (facilitator, rerun)

- Commit: 5556a0c, run in a clean `git archive` copy (no .git directory), Windows 11 Pro, dotnet 10.0.403.
- Command: `dotnet test Phrase.sln --filter "Status!=Pending"`
- Before (gate-run-before.txt, commit 4d73cf9): Compiler 284 of 284 passed; Acceptance 313 of 316 passed, 3 failed.
- After:

    Passed!  - Failed:     0, Passed:   284, Skipped:     0, Total:   284, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
    Failed!  - Failed:     3, Passed:   313, Skipped:     0, Total:   316, Duration: 10 m 25 s - Avowry.Acceptance.Tests.dll (net8.0)

- Failing names after, the same three as before with the new namespace:
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.git_ignored_suite_refuses_before_the_model_is_launched [3 s]
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.repository_check_passes_here_and_fails_on_a_tracked_holdout_path [7 s]
    Failed Avowry.Acceptance.Tests.HoldoutTaskConversion.The_python_holdout_task_test_suite_passes [4 s]

- Totals and failing names equal the baseline; both Avowry.Compiler.Tests.dll and Avowry.Acceptance.Tests.dll appear in the run. List diff: red 2's test_lists_equal (284 and 317 listed, name for name) passes on this commit. Python results: tests.test_rename_196 15 OK, the five path-bound modules 84 OK (green record 2).
- Linux CI: the push of the rename (7b74bad) ran ci green; the run URL is in the Actions list for that commit. Windows is local only because CI is Linux only.
