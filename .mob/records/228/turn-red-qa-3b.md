# Card 228 turn: red 3b (QA seat red, ctx-3, fresh context)

Commit: 07132a0 (mob-crosscli, pushed to origin main). Probe: 2026-11-02 05:07 UTC. Run against committed code only (de65fe5 plus the new untracked test file; git status showed only the untracked .mob/ and that file, no modified source). Neither repo had an unpushed commit from another seat, so nothing of another seat's was pushed or rewritten. MOB.md was current (mob_process.py check: OK), so it was not regenerated. This is the last Criterion 3 cycle (stand-down 1 cap).

Intent (D165): D169, a delete outside the working tree must be refused before it runs, but only a human may switch that on, so nobody's Claude Code settings change behind their back: the hook prints what to paste and where, says it is not installed, writes nothing, and no repo file turns it on.

Assumption (D166): my pins, not in the slice. `--print-snippet` is an argument of the hook file itself (`python hooks/delete_guard_hook.py --print-snippet`), reads no stdin, prints to stdout, exits 0, stderr empty. The text contains PreToolUse inside hooks, settings.json under .claude, the hook's absolute path, "not installed" and "paste", and a JSON object that parses to a PreToolUse command hook. Wording and layout are not pinned. "Writes nothing" = HOME, USERPROFILE, APPDATA and LOCALAPPDATA (and the cwd) at an empty temp folder stay empty; a settings.json already there keeps bytes and mtime. "Off by default" = no git-tracked file except the hook, its tests and *.md names `delete_guard_hook` (settings*.json, templates, core, facilitator.py, facilitator.orig.py, mob_process.py, delete_guard.py named explicitly).

File: tests/test_delete_guard_hook_3b.py (new, 16 tests). Fixtures are text or JSON on stdin; nothing is deleted; no real settings file is read or written.
- PrintSnippet (7): exit 0 with text and empty stderr; key and file; not installed and paste; absolute hook path; the JSON part is a PreToolUse command hook; stdin ignored (a delete command or non-JSON on stdin does not change it); without the flag the hook still judges stdin.
- WritesNothing (5): empty folder stays empty after the snippet, after hook runs (allow, tool without command, refused delete, bad JSON, empty stdin), and after both; an existing settings.json is byte- and mtime-identical; APPDATA apart from HOME.
- OffByDefault (4): no tracked mention outside hook, its tests and markdown; named candidates clean; no tracked settings file registers hooks; the hook file exists.

Failing output (one line): "Ran 16 tests ... FAILED (failures=10)", e.g. "AssertionError: 2 != 0 : --print-snippet must exit 0, stderr: 'delete_guard_hook: stdin is not JSON (Expecting value: line 1 column 1 (char 0)); refused'" (the flag is unknown, so the hook reads empty stdin and refuses). 6 born-green guards pass (hook runs write nothing, off by default holds today).

Modules run: 23 = the 22 of turn-red-qa-3a.md (the 21 of turn-red-qa-2c.md plus tests.test_delete_guard_hook_3a) plus tests.test_delete_guard_hook_3b. 594 tests, FAILED (failures=10), all in the new module (104 s). Without the new module: 578 tests OK. Full discover not run. Scratch: C:\Users\ringe\AppData\Local\Temp\claude\qa228-3b-7c41 (left in place; the tests use their own temp folders).
Size: 3298 bytes
