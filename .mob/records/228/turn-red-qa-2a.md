# Card 228 turn: red 2a (QA seat red, ctx-3)

Commit: dc58ebd (mob-crosscli, pushed to origin main). Probe: 2026-11-02 01:20 UTC. Run against committed code only: git status showed the untracked .mob/ folder and the new test file (untracked until the commit); no source file was modified. The records repo had no unpushed commit from another seat. MOB.md was current (mob_process.py check: OK).

Plan for criterion 2 (two cycles, after criterion 1 grew to seven): 2a is written-then-run file contents in Claude transcripts (this turn). 2b is the Copilot events.jsonl reader (create/edit/apply_patch and the powershell start feed the same writes list; found by cwd and startTime, two matches or none is "no command source"), in its own commit as slice line 68 says. If the green for 2a passes 400 lines, the Copilot reader moves to a follow-up card named at that commit.

Intent (D165): D169, recursive deletes never target anything outside the working tree; a seat that writes clean.sh with rm -rf "$D" in one call and runs bash clean.sh in a later one hides the delete from a scan that reads only command text (the 2026-10-08 home-folder wipe), so the contents of a file that is written and then run must be judged, and only then.

Assumption (D166): the transcript reader can keep one ordered list of file writes (Write, the last Write plus every later Edit new_string, cat > f heredoc, tee f, >>) with a 1-based call number over every tool call; a run of that file is judged with the same scan_command on the content written before the run, so the finding line names the file, the write call and the run call.

File: tests/test_command_scan_files_2a.py (new, 22 tests). Fixture: call 3 writes clean.sh (rm -rf "$D"), call 9 runs bash clean.sh. Asserts: exit 1 and one line "red ctx-3: <kind>: rm -rf "$D" (source <path>)" containing "file clean.sh, written call 3, run call 9"; scan_entry gives the same text; the benign twin is clean.
- Z: a file written and never run is clean (also a test_x.py naming rmtree); cat, ls, grep, vim, cp, chmod of a dirty file are not runs.
- O: one Write then one run, shell and python.
- M: a second Write replaces the first (both orders); the last Write plus every later Edit is judged; an edited file whose last version is clean is clean.
- B: a run before the write is no hit; a run judges the content at that time (clean, run, dirty write, run: one hit at the later run); written 30 calls before the run; a/clean.sh and b/clean.sh are two files; a repeated run is reported once.
- I: seven shell run forms (bash, sh, source, ., ./f, bash ./f, sudo bash), six interpreter forms (python, python3, node, pwsh -File, powershell -File, cmd /c), each with a clean twin; cat > f, tee f and >> heredocs feed the same list.
- E: a run file absent from the transcript is read from disk (file_source: disk), a gone one is a finding ("cannot be read"), an unparsable .py is scanned as text.

Failing output: python -m unittest tests.test_command_scan_files_2a gives Ran 22 tests, FAILED (failures=27): every dirty case prints "clean: red ctx-3, N command(s) scanned" (exit 0, expected 1), and scan_entry returns status clean. Z tests, benign twins and the not-a-run test pass today.

Modules run (19: the 18 of turn-red-qa-1g.md plus tests.test_command_scan_files_2a; 469 tests, FAILED failures=27, all in the new module). Full discover not run. Scratch: C:\Users\<user>\AppData\Local\Temp\claude\qa228-2a-125166755 (left in place).

Size: 3479 bytes
