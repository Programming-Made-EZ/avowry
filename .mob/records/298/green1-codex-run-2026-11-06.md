# Card 298 green 1 (cycle 1): the second Codex build seat (2026-11-06)

Seat: Codex ctx-4 (gpt-5.6-sol), `codex exec --json` through the machine-local launcher, `danger-full-access`, started 20:09 UTC, ended about 20:12 UTC (about 3 minutes; the facilitator wrote nothing in the mob-crosscli checkout while it ran). Brief: the slice file plus the rules (no recursive delete on a variable path, LF, explicit `git add` paths, no trailers, touch only decider.py and facilitator.py). Event stream: .mob/records/298/green-codex-events.jsonl (13 command_execution events).

## What happened
- The seat implemented the slice's design (a): one holder file per run in `mob-decider.lock.d` (decider.py `hold_vote_lock`; facilitator.py `decider_holders_dir`, `decider_holders`, and `decider_lock_live` reading the holders first and then the legacy file).
- It ran the brief's three commands. Its report (record-only for the gate): LiveVote 8 tests OK in 14.855 s; test_writer_isolation 31 tests OK in 51.833 s; the new LiveVoteOverlap test FAILED: the lock-refusal assertions passed but the clean-record assertion failed on the seat-use history.
- **It did not commit or push**, as its brief required all three commands to pass ("No commit created because all required tests did not pass"). The seat followed its brief; the working tree held its change uncommitted.
- The facilitator reran the new test against that change (rerun): the refusal assertions pass, and the failure is `5 != 0 : {'ok': False, 'changes': ['... the seat-use history .mob/records/seat-use.jsonl is not append-only ...']}` (.mob/records/298/overlap-seat-use-failure.txt). That message appeared in the QA seat's first run of the red, before any green: two overlapping runs fail each other's seat-use append-only check, a separate pre-existing defect and not part of the card's Done check 1. It is filed as bug card 307.
- **Red 1b** (QA seat, same context, test file only): the test's last loop asserts what the card says (both runs' head_start and head_end full, equal and equal to HEAD; refused commit and push), not rc 0 and not integrity ok. This is a correction of an over-asserting red, stated here and in the commit message; no assertion of card 230 was touched.
- Commits in mob-crosscli (made by the facilitator, because the seat stopped before committing): red 1 0cd4830, red 1b 4b05fa6, green 1 fd4c702.

## Delete check by the facilitator (the product owner's rule; card 305 not yet built)
The command_execution events were scanned for rm, rmdir, del, erase, Remove-Item, rd, unlink and `git clean`: none found in the 13 commands (rerun by the facilitator, a hand scan, not the delete scan).

## Facts to carry to the next cycles
- The commit-and-push leg of a Codex build seat was NOT exercised this time (the seat stopped on a failing required test, correctly). It worked end to end on card 303.
- The quota of Codex was not read before the seat (unrun); the seat ran without a quota error.
