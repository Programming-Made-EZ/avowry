# Card 228 turn: green 2c (backend developer seat green, ctx-5, fresh context)

Intent (D165): D169, recursive deletes never target anything outside the working tree; a seat that only greps a file whose text mentions a delete is no longer accused, and a live Copilot seat the facilitator launches is now scanned instead of ignored.

Commit: f6a76f5 (mob-crosscli, pushed to origin main), green for red 3d63e09. Probe: 2026-11-02 04:05 UTC. No other seat's commit was unpushed in either repo; MOB.md was not regenerated.

Change:
- command_source.py: the clause split is now quote-aware and escape-aware (an escaped or quoted pipe belongs to a grep pattern; an unbalanced quote falls back to the old split). Reading verbs (grep, cat, head, tail, sed, wc, less, type, Get-Content and kin) name files without running them. A bare shell or interpreter fed by a pipe from cat, type or Get-Content counts as a run of those files (cat x.sh | bash). locate_copilot accepts a session.start from the launch time up to 10 seconds after it (never before); an unparsable time falls back to equality.
- facilitator.py: run_backend takes a launch time (ISO-8601 UTC, milliseconds, Z) before the process starts and, for command_source "copilot", calls scan_entry with source copilot, the seat's cwd and that time, with no {SESSION} needed. Claude and none backends unchanged. CRLF kept (CR bytes 1723 to 1728, equal to the line count).
- Card 227 allow-list and fail-closed rules untouched; no scanned file is executed.

Tests: red 2c tests.test_command_scan_files_2c 20 OK (was 15 failures). The 21 modules 537 tests OK. Full discover 1377 tests OK (skipped=1), 1841 seconds. Scratch: C:\Users\ringe\AppData\Local\Temp\claude\dev228-2c-7f3a (left in place).
Size: 1760 bytes
