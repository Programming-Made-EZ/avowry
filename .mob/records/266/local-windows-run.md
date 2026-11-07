# Card 266 local Windows run (facilitator, rerun, isolated)

- Commit: e1f798d (records on top of the green 29efd59; the code and docs are the five card commits), run in a clean git archive copy (no .git directory), Windows 11 Pro, dotnet 10.0.403. Start 19:51:44Z UTC.
- Command: dotnet test Avowry.sln --filter "Status!=Pending"
- Load: no push and no dotnet command from another seat ran during this run. The push finished before it started (the chain). The cold-reader seat (Read tool on two files) and the reviewer's git and python work that does not call dotnet ran during the run; the reviewer was held from every dotnet command until this run's done-file. The idle build servers were not restarted by anything else.
- Before (baseline/gate-run-before.txt, at the red-2 commit): Compiler 284 passed; Acceptance 312 passed, 4 failed (the three known failures plus the dotnet wrapper of the demo README test, red by design on red 2). The equality baseline is card 197's isolated 313 passed and 3 failed.
- After:

    Passed!  - Failed:     0, Passed:   284, Skipped:     0, Total:   284, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
    Failed!  - Failed:     3, Passed:   313, Skipped:     0, Total:   316, Duration: 9 m 26 s - Avowry.Acceptance.Tests.dll (net8.0)

- Failing names after (the same three known failures; the TasksDemoReadme wrapper passes again):
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.git_ignored_suite_refuses_before_the_model_is_launched [4 s]
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.repository_check_passes_here_and_fails_on_a_tracked_holdout_path [7 s]
    Failed Avowry.Acceptance.Tests.HoldoutTaskConversion.The_python_holdout_task_test_suite_passes [3 s]

- Totals and failing names equal the baseline. The card 304 timing guard passed. Python results (facilitator): tests.test_live_docs_266 17 OK, the path-bound modules 108 OK, scripts/check-process-wiki.py OK (18 pages), in the green worktree before the merge.
- CI: the push of the card's commits ran the ci workflow (see the reviewer's check 8 for the run list).
