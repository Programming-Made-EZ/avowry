# Card 228 turn: red 2c (QA seat red, ctx-3, fresh context)

Commit: 3d63e09 (mob-crosscli, pushed to origin main). Probe: 2026-11-02 03:25 UTC. Run against committed code only: git status showed the untracked .mob/ folder and the new test file (untracked until the commit); no source file was modified. The records repo had no unpushed commit from another seat (main = origin/main) and the code repo likewise, so nothing of another seat's was pushed or rewritten. MOB.md was current (mob_process.py check: OK), so it was not regenerated.

Intent (D165): D169, recursive deletes never target anything outside the working tree; a seat that only greps a file whose text mentions a delete is wrongly accused, and a live Copilot seat the facilitator launches is never scanned at all, so both the accusation and the blind spot must go.

Assumption (D166): (1) the real case was grep -n "17 \|modules\|python -m" on a .md file; the escaped pipe was split as a pipe, so python -m ... file.md looked like a run. Reading verbs (grep, cat, head, tail, sed -n, wc, less, type, Get-Content) make their file arguments reads; bash, sh, source, a leading dot, python, node, pwsh -File, `cat x.sh | bash` and `grep x f; bash f` stay runs. (2) run_backend calls scan_entry(backend, session, "copilot", cwd=<seat cwd>, started=<launch time, ISO-8601 UTC string, taken just before the process starts>); a Copilot seat's own session.start comes a moment later, so locate_copilot must accept a start a few seconds after the launch time (green picks the tolerance) and reject a session started long before; one match is read, none or two is "no command source". Also pinned: the Copilot invocation needs no {SESSION}. The launch-time string and tolerance are my inference, not in the slice.

File: tests/test_command_scan_files_2c.py (new, 20 tests; fixtures are text under the test's temp dir; the only thing run is a tiny seat double written into the temp dir and launched through facilitator.run_backend, with scan_entry wrapped to read a fake session-state folder).
- Reads are not runs: the real case (scan_entry and the CLI); quoted, single-quoted and unquoted patterns with python in them; each reading command; a missing file named by a read is not an unreadable-file finding; reads in a pipeline or chain.
- Real runs stay: each run form, `cat x.sh | bash`, `grep x f; bash f`, a run after a quoted grep in one command, a clean run file stays clean.
- Live Copilot: run_backend passes source copilot, the seat's cwd and a launch-time string; a delete, a clean seat, a created-then-run file are scanned; no session or two sessions give "no command source"; an old session and another-cwd session do not match; a Claude seat still gets its session and a none backend stays unscanned.

Failing output (one line): the 21 modules ran 537 tests, FAILED (failures=15), all in the new module, e.g. "('findings', ['recursive delete on a variab...') != ('clean', [])" for the grep case, "('copilot', None) != ('copilot', 'copilot')" for the source run_backend passes, and 'unscanned' != 'no command source'. Born-green guards pass (read-then-run, run forms, missing-file, Claude and none backends).
Note for green: `cat x.sh | bash` reports clean today, so a shell reading stdin from a piped read must count as a run of that file.

Modules run (21 = the 20 of turn-red-qa-2b.md plus tests.test_command_scan_files_2c). Without the new module: 517 tests OK. Full discover not run. Scratch: C:\Users\<user>\AppData\Local\Temp\claude\qa228-2c-8e41 (left in place).

Size: 3555 bytes
