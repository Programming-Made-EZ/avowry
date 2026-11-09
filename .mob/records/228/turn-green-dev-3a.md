# Card 228 turn: green 3a (backend developer seat green, ctx-5, fresh context)

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a mis-quoted delete (the 2026-10-08 home-folder wipe) cannot take out work git cannot restore; the opt-in hook refuses before the command runs, and nothing installs it.

Commit: de65fe5 (mob-crosscli, pushed to origin main), green for red 8063a74. Probe 2026-11-02 05:07 UTC. No other seat's commit was unpushed in either repo.

Change: delete_guard.py gets inside_tree(path, root, cwd), tree_verdict(command, cwd) and a tree-mode switch in _allowed (off for every other caller, so card 227's allow-list and fail-closed rules are unchanged); a base64-decoded delete is never tree-allowed; the one eval let through is eval "$(ssh-agent -s)". New hooks/delete_guard_hook.py (stdin JSON, exit 2 with findings on stderr, fail closed on odd input). No settings file, template or facilitator touched; --print-snippet is cycle 3b. Run files are read from disk, never executed.

Module counts: red 3a 41 OK (was 27 failures, 13 errors). The 22 named modules plus tests.test_delete_guard: 716 tests OK (skipped=1).
Time from tests passing (05:39 UTC) to push (05:41:33 UTC): about 2 min 18 s.
Full suite, python -m unittest discover -s tests, run on committed de65fe5: 1418 tests OK (skipped=1), 1851 s (05:41:38 to 06:12:30 UTC).
Scratch: C:\Users\<user>\AppData\Local\Temp\claude\dev228-3a-5c1e (left in place).
Size: 1491 bytes
