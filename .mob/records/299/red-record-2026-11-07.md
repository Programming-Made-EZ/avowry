# Card 299 red record (QA seat)

Red commits in mob-crosscli (main): red 1 3f2b4a6 (six tests fail, two guards pass: a second process holding the checkout is refused before any git write, naming the holder); red 2 f8ee94d (two fail, four pass: a lock whose process is gone is taken over, a late release removes only its own lock); cycle 3 tests 4ae67db (born green after green 2; each of the four mutation patches in .mob/records/299/mutations fails its matching test and applies cleanly).

Green commits that followed: 7afce8e (green 1), 4b6ab57 (green 2); see green-and-mutations-2026-11-07.md. Suites on main after the card: CrossProcessWriters 22, test_writer_isolation 53, test_writer_staging 7, test_decider_runner 43, all OK.
