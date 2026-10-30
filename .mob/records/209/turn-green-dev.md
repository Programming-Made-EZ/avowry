# Card 209: green seat turn record (backend developer, ctx-3)

Card 209 green evidence. Code is in mob-defaults (C:\Users\ringe\mob-crosscli, local main, not pushed).

## Commits per criterion

Criterion 1 (every removal except "removed: retro" is refused by name):
- Red (QA ctx-2): 6836d76
- Unit layer 3, 5a2c930: check_task_force -> removal_problem. Double consumed: a recorded fake removal_problem
  (unittest.mock.patch.object on facilitator.removal_problem, logging role, removed, on_task_force per call).
- Unit layer 4, b527e1d: parse_task_force keeps a blank "removed:" as "(blank)". No doubles (pure leaf).
- Unit layer 5, 8e387e3: mob_process.removal_problem, the one shared rule. No doubles (pure leaf).
- Green: 7d371fa

Criterion 2 (the record checker refuses the same removals with the same message):
- Red (QA ctx-2): 44112b4, then 37b1273 (CRLF restore of tests/test_mob_process.py)
- Unit layer 3, 145947b: check_record -> removal_problem. Double consumed: a recorded fake removal_problem
  (patch.object on mob_process.removal_problem, same shape as above).
- Unit layer 3 correction, 147a90a: the record's task_force list alone decides who is on the task force. Same double.
- Green: 18f3962

Criterion 3 (the template documents removal only at the retro):
- Red: 794a4a7
- Green: 1bb0a83

Review fixes (reviewer ctx-4, review-ctx4.md):
- F1 73026aa: the missing-Decider removal test uses "removed: retro"; mutation of the Decider loop in
  sync_task_force fails it, the real code passes.
- F2 red 7fdb168: the removal scan also covers templates/TASK_FORCE.md and fails on it.
- F2 green e239b12: templates/TASK_FORCE.md documents removal only at the retro.

## Full-suite runs (python -m unittest discover -s tests), as recorded in the commit bodies
- 7d371fa: Ran 315 tests, OK (skipped=1).
- 18f3962: Ran 332 tests in 1031s, OK (skipped=1), 14:36 to 14:53 UTC.
- 1bb0a83: Ran 336 tests in 1031.6s, OK (skipped=1); "SMOKE FAILED" lines are output of nested smoke-test fixtures.
- Review-fix run (start 15:33:26 UTC): its output carried no summary line when I read it, so there is no count
  from it. Not claimed as a pass. The 16:24 UTC rerun at e239b12 in in-flight.md is the evidence for the review fixes.

## Amend and correction
- The unpushed layer-3 commit of criterion 2 (b21d94e) was amended; 145947b replaced it (before any push).
- 147a90a is a separate correction commit to 145947b (no amend), after QA's record test showed the on-task-force
  assumption was wrong.

This turn record is written at close from those commits.
