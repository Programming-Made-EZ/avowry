# Card 196 paired full gate (facilitator, condition 1 of the sign-off)

Three full gates back to back in built clean archive copies (`dotnet test Phrase.sln --no-build --filter "Status!=Pending"`), nothing else running (no pushes, no seats), script pairgate.sh. Start times UTC:

- after (start 14:23:39Z):
    Passed!  - Failed:     0, Passed:   284, Skipped:     0, Total:   284, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
    Failed!  - Failed:     3, Passed:   313, Skipped:     0, Total:   316, Duration: 9 m 15 s - Avowry.Acceptance.Tests.dll (net8.0)
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.git_ignored_suite_refuses_before_the_model_is_launched [8 s]
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.repository_check_passes_here_and_fails_on_a_tracked_holdout_path [6 s]
    Failed Avowry.Acceptance.Tests.HoldoutTaskConversion.The_python_holdout_task_test_suite_passes [2 s]

- before (start 14:32:57Z):
    Passed!  - Failed:     0, Passed:   284, Skipped:     0, Total:   284, Duration: 1 s - Phrase.Compiler.Tests.dll (net8.0)
    Failed!  - Failed:     3, Passed:   313, Skipped:     0, Total:   316, Duration: 9 m 8 s - Phrase.Acceptance.Tests.dll (net8.0)
    Failed Phrase.Acceptance.Tests.PrivateHoldoutResults.git_ignored_suite_refuses_before_the_model_is_launched [3 s]
    Failed Phrase.Acceptance.Tests.PrivateHoldoutResults.repository_check_passes_here_and_fails_on_a_tracked_holdout_path [6 s]
    Failed Phrase.Acceptance.Tests.HoldoutTaskConversion.The_python_holdout_task_test_suite_passes [2 s]

- after2 (start 14:42:07Z):
    Passed!  - Failed:     0, Passed:   284, Skipped:     0, Total:   284, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
    Failed!  - Failed:     3, Passed:   313, Skipped:     0, Total:   316, Duration: 9 m 10 s - Avowry.Acceptance.Tests.dll (net8.0)
    Failed Avowry.Acceptance.Tests.HoldoutTaskConversion.The_python_holdout_task_test_suite_passes [2 s]
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.git_ignored_suite_refuses_before_the_model_is_launched [7 s]
    Failed Avowry.Acceptance.Tests.PrivateHoldoutResults.repository_check_passes_here_and_fails_on_a_tracked_holdout_path [7 s]

5556a0c (after, after2) and the pre-rename 4d73cf9 (before) all gave Compiler 284 of 284 and Acceptance 313 passed, 3 failed (the same three known names). The earlier failures (reviewer: 312/4; sign-off: 312/4 twice) came with other work on the machine; the extra failure is the 20 second guard of timed_out_model_tree_is_stopped_and_recorded_in_both_arms. Tally at 5556a0c: 6 full runs, 3 failed on that guard; at 4d73cf9: 3 runs, 0 failed. Cause not identified; recorded on card 304.
