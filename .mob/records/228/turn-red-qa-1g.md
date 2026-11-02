# Card 228 turn: red 1g (QA seat red, ctx-3)

Commit: 9fe71ca (mob-crosscli, pushed to origin main). Probe: 2026-11-02 01:20 UTC. Run against committed code only: git status showed the untracked .mob/ folder and the new test file (untracked until the commit); no source file was modified. The records repo had no unpushed commit from another seat. MOB.md was current (mob_process.py check: OK).

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a seat's delete is reported the same whether the CLI, the unit entry or the facilitator reads the transcript, and no entry point can miss what another reports (the 2026-10-08 home-folder wipe).

Assumption (D166): the three entry points share one transcript reader and one scan_command, so one transcript gives the same findings in file order, a repeated command once. scan_entry holds one string per command (its findings joined with "; ") and the CLI prints one line per finding, so parity compares the CLI lines of a command, joined, to the scan_entry string.

Scope (decision 228-c1-scope, option b): the last criterion-1 cycle, only the I and M ZOMBIES. No new delete form, no path forms (follow-up F4). File: tests/test_command_scan_parity_1g.py (new, 8 tests). Forms are the 20 of cycle 1f plus ten distinct forms from cycles 1a, 1d, 1e and 1f (rm -rf, r""m, git reset --hard, git clean -fdx, rsync --delete, perl unlink glob, busybox, wsl, bash <(...), Remove-Item).
- I: each of the 20 forms alone gives the same findings through scan_entry and the CLI; the whole file (forms, repeats, twins) gives the same findings through scan_entry, the CLI and run_backend (via tests/fakes/fake_seat.py, one real subprocess run, a light double); a benign file and a zero-command file are clean through all three with the same command count.
- M: ten forms alone are one line each; ten forms with repeats and clean commands give exactly ten lines in file order through the CLI, ten findings through scan_entry and run_backend; one form repeated 40 times is one line.

Result: BORN GREEN, nothing fails. Both ZOMBIES already hold, so all 8 tests are guards and there is no failing output to show. One test assertion was dropped before the commit: it demanded ten textually different lines, but distinct commands (busybox, wsl, r""m) print the same kind and snippet; ten lines is the count the slice asks for.

Modules run (18 counting the new one: the 17 of turn-red-qa-1f.md plus tests.test_command_scan_parity_1g; 447 tests, OK). Full discover not run.

Note for the slice: with 1g green-by-construction, criterion 1 needs no green 1g; the next work is the F4 path forms follow-up, then criteria 2 and 3.

Size: 2706 bytes
