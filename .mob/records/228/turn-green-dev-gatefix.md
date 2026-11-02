# Card 228 turn: green gate-fix (backend developer seat green, ctx-5, fresh context; window 228-gate-fix; green for red 52451b6)

Commit: e4f7c29 (mob-crosscli, pushed to origin main; parent 52451b6). Probe: 2026-11-02 07:37 UTC. git status before work: only the untracked .mob/ in mob-crosscli; records repo showed only untracked files (body.tmp left alone). No other seat's commit was unpushed in either repo, so nothing was pushed or rewritten for another seat. MOB.md was current (mob_process.py check: OK), so it was not regenerated.

Intent (D165): a flagged command must be blocked before it runs, so the hook no longer lets eval "$(ssh-agent -s)" through (a shell function or PATH shadow named ssh-agent cannot carry a delete past it), and a live Copilot seat is found even when its session.start lands up to 60 s after the recorded launch.

Change:
- delete_guard.py: removed the _SSH_AGENT_EVAL allow and its filter in tree_verdict; the scan finding now refuses (exit 2, finding on stderr). Card 227's allow-list and fail-closed rules untouched.
- command_source.py: START_TOLERANCE 10 s to 60 s. Start before the launch, more than 60 s after, and two matches are unchanged.
- tests/test_delete_guard_hook_3b.py (one line, D166): the opt-in meta test (no tracked file but the hook, its tests and markdown names the hook) failed on QA's new tests/test_delete_guard_gatefix.py, which imports test_delete_guard_hook_3a. I added that file to the meta test's allowed list. QA may want to confirm this reading.
Line endings stay LF. No settings edited, hook not installed.

Counts: first run of the 24 modules: 610 tests, 1 failure (the 3b meta test above, 98 s); the 15 red failures were all fixed. After the 3b edit, 3b alone: 16 OK; the other 23 modules had passed in the same run. Fix red: 16 tests OK.
Time from tests passing (08:19:19 UTC, 3b rerun; the 24-module run ended 08:18:54) to push: about 10 s (08:19:29).

Full suite on committed e4f7c29 (python -m unittest discover -s tests, 1929 s): Ran 1450 tests, FAILED (failures=1, skipped=1). The one failure is test_loop_state.LoopStateTests.test_lock_released_when_holder_exits (acquire_lock returned a handle while the child should hold the lock): a timing race (the test sleeps 0.2 s for a child python to import and take the lock) in code this change does not touch; tests.test_loop_state alone passes (8 OK). Not fixed in a new commit: I judge it a flake in another card's test and need the facilitator's call. The smoke output also printed "SMOKE FAILED" (MOB.md check: the facilitator produced no run transcript) from the fake-backend tests inside the suite; it is expected noise from those tests, not a failure count. Scratch: C:\Users\ringe\AppData\Local\Temp\claude\dev228-gf-7391 (full.txt, left in place).
Size: 2798 bytes
