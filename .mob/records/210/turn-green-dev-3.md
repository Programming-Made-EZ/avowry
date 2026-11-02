# Card 210 turn: green 3 (backend developer seat green, ctx-5, fresh context; D157 Criterion 3, cycle 1 of 1)

Commit: c633d3a (mob-crosscli, pushed to origin main; parent 242677e, red 3). Probe: 2026-11-02 14:11 UTC. Records repo: main = origin/main, no unpushed commit from another seat. MOB.md was not stale (`mob_process.py check` prints OK).

Intent (D165): the record check stops accepting a truthy member or retro (audit finding MD-003): each task-force member says what it is for, the copied template roster is not a declared task force, a Claude seat from card 208 says why it started, and the retro metrics must be real structures.

Change (mob_process.py only): new `TEMPLATE_ROSTER`, `BLANK_WORDS`, `_blank`, `_member_problems(task_force, card_number)` and `_retro_problems(retro)`. check_record calls both. The existing context check now uses the same blank rule (whitespace, "true", "false", placeholders). A retro that is not an object is refused (it raised before). Open-finding and added-member retro rules are unchanged, just run only when the retro is an object.

Counts: red 3 (tests.test_record_members_retro) 28 tests OK, was 56 failures and 7 errors. The 6 named modules: 199 tests OK.
Time from tests passing (15:04:40 UTC) to push: about 1 minute (commit and push done by 15:05:40 UTC).
Full suite on c633d3a: Ran 1545 tests in 1745 s, OK (skipped=1). No test_loop_state lock failure this run. The "SMOKE FAILED" lines in its log are printed by a negative smoke test, not failures.

Refactor (green 3): none needed, because the change is one helper (`_blank`) used by the member checks and the context check, one constant per template fact, and no dead code. The only duplication is BLANK_WORDS repeating facilitator.TASK_FORCE_PLACEHOLDERS (facilitator imports mob_process, so mob_process cannot import it back); merging them touches facilitator and is outside the smallest green change. Named for the retro.

Gap: a Claude seat planned but never started fails by design (follow-up F2 in the slice).

Size: 2027 bytes
