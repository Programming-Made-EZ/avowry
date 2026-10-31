# Card 219 turn record: backend developer [author] (ctx-4, Claude seat)

Card 219 greens, written for the record. This turn record is written at close from those commits.

## Unit layers and greens

| Criterion | Unit layer | Green |
|---|---|---|
| 1 | ab94601 (previous_record helper, red against a temporary stub; 5 subtests failed by assertion) | b374ef7 (staged M1b: earlier-added records, largest card number; commit_order from one git rev-list --topo-order --reverse HEAD) |
| 2 | 66904ab (table row "nearest earlier by position, not the largest number"; red 214 != 216) | 1a08bd6 (final rule: nearest earlier record by history position) |

Seat-start line: 9cd94a0 (criterion 2 seat).

## Doubles

- previous_record unit tables (ab94601, 66904ab): none; pure dict inputs (record_commits, commit_order).
- Acceptance tests (451f4ea, 55b5794): real temporary git repo, the board issue stub, FROZEN_LEGACY_SHA256 patched, fixed commit dates through GIT_COMMITTER_DATE. No gh.

## Suite and check runs

- b374ef7: tests.test_mob_process 75 tests OK; whole suite 311 tests OK, 7 skipped; check run once, only error was MOB.md staleness, fixed by regeneration.
- 1a08bd6: tests.test_mob_process 77 tests OK; whole suite (python -m unittest discover -s tests) 313 tests OK, 7 skipped; python scripts/mob_process.py check run once after the commit: "mob-process: OK (94 closed cards, 3 records, 91 legacy)".

## Mutation tables

Criterion 1 (b374ef7; 3 relevant tests, restored file 3 tests OK):
- M1 revert to max(n < card): 5 failures.
- "<=" instead of "<": 6 failures.
- commit missing from commit_order counted as earlier: 1 failure.
- own commit unknown no longer returns None: 1 failure.
- M6 not run (criterion 2 tests did not exist yet).

Criterion 2 (1a08bd6; 5 relevant tests, restored byte for byte, 5 tests OK):
- M1 max card number below this card: failures=3.
- M5 position from the latest add: failures=1 (held-record sequence).
- M6 drop the previous-record comparison: failures=1 (backdated evidence).
- M2 first record in history instead of nearest: failures=3.
- M4 same-commit records constrain each other: failures=1, errors=1.
- M3 committer-date ordering: not run; the forged-date guard test does not exist.

This turn record is written at close from the commits listed above; it was not written during the turns.
