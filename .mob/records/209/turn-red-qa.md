# Card 209: red seat turn record (QA engineer, ctx-2)

Evidence for Card 209 (MD-001, D145, D157). The red work is in mob-defaults (C:\Users\ringe\mob-crosscli).

## Commits

| Commit | Subject (short) | Criterion line |
|---|---|---|
| 6836d76 | red: every removal except "removed: retro" is refused by name (tests/test_card_mode.py, 111 lines) | Criterion: 1 |
| 44112b4 | red: the record checker refuses the same set with the same message (tests/test_mob_process.py) | Criterion: 2 |
| 37b1273 | restore the CRLF line endings of tests/test_mob_process.py | none (changes no test) |
| 794a4a7 | red: template scan (tests/test_template_removal.py, 79 lines) | Criterion: 3 |

## Criterion order

Criterion lines read 1, 2, 3 across my reds. Each red was followed by its green before the next red (green 1, then 2 at 18f3962, then 3 at 1bb0a83). 37b1273 carries no Criterion line and says so.

## Born-green guards and their mutations (applied in scratch copies, never committed)

- G1 test_guard_no_removal_line_leaves_the_roster_unchanged: facilitator.py:713 `if joined and removed and removed.lower() != "retro":` changed to `if removed.lower() != "retro":` turns it red (also G2 and G3).
- G2 test_guard_retro_removal_is_accepted_in_every_spelling: same line changed to `if removed and removed != "retro":` (drops the case-fold) turns it red for 'Retro' and 'RETRO'.
- G3 test_guard_retro_removal_keeps_the_member_in_the_record: facilitator.py:1041 list filter gains `if not r.removed` and the guard turns red.
- G4 test_guard_record_without_removals_and_with_retro_has_no_removal_problem and test_guard_record_command_accepts_a_record_with_only_a_retro_removal: mob_process.py check_record flags every remove regardless of cycle, and both turn red.
- G5 test_guard_template_still_documents_the_retro_removal (794a4a7): replacing "(`| removed: retro`)" with "(at the retro)", "Added members stay until the retro" with "Added members stay", or "removed: retro" with "removed: later", in a copy of the cleaned template, turns it red (1 failure each); the unmutated cleaned copy is OK (4 tests).

## Line-ending slip and its correction

44112b4 rewrote every line ending of tests/test_mob_process.py from CRLF to LF (a sed -i slip while renaming one test, caught after the commit), making a 121-line change look like 1181 lines against D136. 37b1273 restored CRLF with no content change, as a new commit; no history was rewritten or amended. From then on I compared `git diff --cached --stat` with `git diff --cached --ignore-cr-at-eol --stat` before each commit (794a4a7 is a new LF file: 79 lines in both).

This turn record is written at close from those commits.
