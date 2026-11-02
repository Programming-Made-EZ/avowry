# Card 210 turn: green 2 (backend developer seat, ctx-5, fresh context; D157 Criterion 2, cycle 1 of 1)

Intent (D165): the record check stops trusting the self-reported cycles field (audit finding MD-003); the domain-expert boundaries come from MOB.md `#### Cycle N` headings and the seat log, and a mismatch, an unverifiable claim or a wrong-typed field is a refusal, never a raise.

Commit: 1baece8 (mob-crosscli, pushed to origin main; parent edc0837, red 2). Green for red edc0837. Git status before: only the untracked .mob/; main = origin/main, no other seat's commit unpushed.

Change:
- mob_process.py: cycles_from_evidence(mob_text, seat_log) (None when both None, else the largest `#### Cycle N` or integer seat-log `cycle`, else 0); _cycles_problems compares the field with it (record says X / MOB.md shows Y; claimed but none found = unverifiable; non-integer incl. bool = refused by type); check_record gains seat_log and builds the boundaries from the derived count; main reads the card's lines of seat-use.jsonl (card_seat_log) and always supplies evidence.
- facilitator.py: the sequential loop writes `#### Cycle N` at the start of each cycle. CRLF kept: 1728 CR before, 1729 after (one added line), LF count equal.
- Fixture change, named (a conflict in the tests, not a weakened check): red 1's run_record (tests/test_record_task_force_probes.py) and RecordCommandRefusesACycleRemoval.run_record (tests/test_mob_process.py) ran the CLI on a record claiming cycles 1 with no MOB.md and no seat log, and expected "ok"; red 2 requires exactly that situation to be "unverifiable" (test_e_no_mob_file_and_no_seat_log_file_still_counts_as_evidence_supplied, same files on disk). Both cannot pass, so each helper now writes a MOB.md with two turns and `#### Cycle 1 roles`, the same fix QA made in three assertions. Flag for QA/architect: these edits are in test files.

Counts: the 5 modules (test_mob_process, test_command_scans_record, test_command_scans_record_strict, test_record_task_force_probes, test_record_cycles_evidence): 171 tests OK (41 red tests now pass).
Time from tests passing (14:18:47 UTC) to push (14:18:56 UTC): 9 s.
Full suite on 1baece8: Ran 1517 tests in 1738.8 s, OK (skipped=1). No test_loop_state failure seen.

Refactor (green 1 and green 2): none needed, because the new code is three small helpers (cycles_from_evidence, _cycles_problems, card_seat_log) each used once with one job, green 1's _parses/_task_force_problems/_probe_problems share _parses and _text already, no dead code or duplicate logic was found, and the two test fixture edits are two lines each in separate files with different helpers.
Size: 2651 bytes
