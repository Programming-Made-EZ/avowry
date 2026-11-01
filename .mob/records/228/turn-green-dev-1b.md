# Card 228 turn: green 1b (backend dev seat green, ctx-5)

Commit: fdff54f (mob-crosscli, pushed to origin main). Green for red 82c1e84. Probe: 2026-11-01 21:14 UTC.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so an encoded delete cannot hide a home-folder wipe from the scan.

Change (delete_guard.py): a powershell/pwsh -EncodedCommand/-enc/-ec (any prefix of -EncodedCommand) payload is decoded as UTF-16LE, and `echo <b64> | base64 -d|--decode|-D | <shell>` as UTF-8; the decoded text is scanned as typed (recursion through _scan_text, so double encoding works, bounded by the existing depth limit 6). Bad base64, wrong encoding, a missing payload, a payload with no readable source piped to a shell, or depth exceeded is a finding. Decoding is pure string work; nothing decoded is run or evaluated. The 227 allow-list and fail-closed rules are unchanged.

Tests: red 1b 4 tests OK; 13 modules (12 from the red plus 1b) 421 tests OK; full `python -m unittest discover -s tests` 1261 tests OK (skipped=1), 1721 s. The log shows a "SMOKE FAILED" line printed by a smoke test inside the suite; the run itself passed.

Size: 3522 bytes
