# Card 196 sign-off brief (a fresh Claude context that did not write any part of the card; READ-ONLY on C:\Users\ringe\projects\phrase-lang)

You are the final sign-off. Do not edit, create or delete anything under that repository; do all runs in scratch copies in your own temp directory. Reply in text (under 700 words); the facilitator commits it as signoff-ctx6.md. Label each line rerun, record-only or blocked.

Read the card (`gh issue view 196`), .mob/records/196/preship-gate.md (the gate: SHIP WITH CONDITIONS, with the facilitator's evidence for both conditions), green-record-1.md, green-record-2.md, local-windows-run.md, red-record-1/2/3.md, and docs/decisions/process-and-staffing.md section D201 (the adopted conditions).

Verify the card's Done-when and its six scenarios against the commit 5556a0c (HEAD at the gate; later commits add only records), each by your own command:
- [Z] the old project names are gone from tracked live files and tests/Avowry.* are tracked (27 and 100 files);
- [O][M] the sorted test lists equal the committed baseline name for name (284 and 317);
- [B] the StarvationProbe prefix, `git check-attr text` unset on the 14 corpus fixtures, R100 renames, line endings unchanged;
- [I] the five path-bound python modules and `Phrase.sln` (names, paths, the original GUID pairing);
- [E] partial work is refused: re-apply 3 of the mutation patches of your choice from .mob/records/196/mutations onto fresh `git archive` copies (and the history ones you can run) and show the matching test fails;
- [S] the plain route: CI on the pushed commits is green (use `gh run list`), and the Windows local gate totals equal the baseline (local-windows-run.md vs baseline/gate-run-before.txt).
Also: run `dotnet test tests/Avowry.Acceptance.Tests --no-build --filter "FullyQualifiedName~timed_out_model_tree_is_stopped_and_recorded_in_both_arms"` five times in a built clean archive copy and report the results (the gate saw one load-sensitive failure of this test). For a full dotnet gate run in your own clean archive copy, wait first for the file C:\Users\ringe\AppData\Local\Temp\claude\1ff08f82160bcdeb\2ca12683-042f-40b7-b61a-623ae180176b\scratchpad\push-quiet.done (the facilitator's push hook is using the machine; poll with sleep 30 up to 25 minutes), then run `dotnet test Phrase.sln --filter "Status!=Pending"` in the background (about 11 minutes) and report the totals and failing names against the baseline.

Verdict: SIGNED OFF, SIGNED OFF WITH CONDITIONS or NOT SIGNED OFF, one paragraph. State any promise of the card that the evidence does not show, and anything that should go to the retro.
