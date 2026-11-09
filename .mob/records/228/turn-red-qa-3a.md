# Card 228 turn: red 3a (QA seat red, ctx-3, fresh context)

Commit: 8063a74 (mob-crosscli, pushed to origin main). Probe: 2026-11-02 05:07 UTC. Run against committed code only: git status showed the untracked .mob/ folder and the new test file (untracked until the commit); no source file was modified. Neither repo had an unpushed commit from another seat (main = origin/main in both), so nothing of another seat's was pushed or rewritten. MOB.md was current (mob_process.py check: OK), so it was not regenerated.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a mis-quoted delete like the 2026-10-08 home-folder wipe cannot take out work git cannot restore; inside the tree the scan only reports, and the opt-in hook is where the refusal happens, before the command runs.

Assumption (D166): the names and shapes below are my pins, not in the slice, and green may not rename them. `delete_guard.tree_verdict(command, cwd)` returns `(verdict, reasons)`, "allow" or "refuse", reasons a list of strings. `delete_guard.inside_tree(path, root, cwd=None)` is a pure string comparison, True only strictly below root, False for anything not a literal. The hook finds the git root by walking up from cwd; no root (or a missing or odd cwd) refuses a delete but does not block a benign command. A delete is allowed only when every target is a literal strictly inside the git root (even outside scratch); the root itself, `.` and globs are refused; every other scan finding refuses. A refusal prints the finding kind to stderr, a tree refusal also "working tree". Exception or an unreadable run file: exit 2.

## Two-cycle plan for Criterion 3 (stand-down 1 cap)
- 3a (this turn): hook stdin and exit code, tree_verdict, inside_tree, fail-closed cases.
- 3b: `--print-snippet` (names the settings key and file, says it is not installed; writes nothing), plus the proof that no code path writes a settings file: with HOME, USERPROFILE and APPDATA at an empty temp folder, running the hook and `--print-snippet` leaves it empty; and nothing (settings.json, templates, facilitator.py) references the hook path, so it is off by default. A third cycle would need a scope window.

File: tests/test_delete_guard_hook_3a.py (new, 41 tests). Fixtures are JSON or text on stdin to the hook, never executed; temp git repos made with `git init` under the test's temp folder; nothing is deleted. The hook runs with HOME and APPDATA at an empty temp folder.
- Z: Read/Glob/Grep/Write with no command exit 0 when the JSON parsed; empty stdin, unparsable JSON, non-object JSON and non-string commands exit 2.
- O: benign commands exit 0; `rm -rf "$HOME/x"` and each criterion-1 form (hidden verb, base64 to sh, -EncodedCommand, node, perl, python, git reset --hard, find -delete, rsync --delete, eval) exit 2 with the finding kind on stderr; a run of a flagged .sh/.py read from disk exits 2, a clean one 0, a file only read (grep, cat) is not a run.
- M: the worst command in a chain decides.
- B: `R/a` and relative and /c/ spellings inside allowed; `R2/a`, `R-old/a`, `R/../x`, `../x`, a wrong-drive /z/ path, the root, `.`, globs, `~`, `$HOME` refused; cwd below the root; no git root refuses a delete and not `ls`; missing or odd cwd refuses a delete; 6 nested wrappers and a 500,001-byte command refuse.
- E: a missing or directory run file exits 2; tree_verdict refuses when scan_command or scan_file raises (the timeout shares that path and is not run).
- Layers: tree_verdict (5 tests) and inside_tree as a string unit (6 tests).

Failing output (one line): "Ran 41 tests ... FAILED (failures=27, errors=13)", e.g. the hook is missing so every exit code is Python's "can't open file" 2 (the allow cases fail with 2 != 0, the refuse cases fail on the stderr finding), and `AttributeError: module 'delete_guard' has no attribute 'tree_verdict'` / `inside_tree`. One born-green guard passes: the scan still reports in-tree deletes.

Modules run (22 = the 21 of turn-red-qa-2c.md plus tests.test_delete_guard_hook_3a). Without the new module: 537 tests OK (75 s). Full discover not run. Scratch: C:\Users\<user>\AppData\Local\Temp\claude\qa228-3a-390315620 (left in place).
Size: 4214 bytes
