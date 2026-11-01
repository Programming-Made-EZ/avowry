# Card 228 turn: green 1d (backend dev seat green, ctx-5)

Commit: 4e35837 (mob-crosscli, pushed to origin main). Green for red 40555b4. Probe: 2026-11-01 22:50 UTC.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a command that throws away uncommitted work, runs a string it built itself, or hides its Remove-Item target in a splat or a line continuation cannot take out work git cannot restore.

Change (delete_guard.py): git reset --hard, git checkout -f/--force (also a short flag with f) and git checkout -- . are findings ("git command that discards the working tree"); iex/Invoke-Expression is read only when its one argument is a quoted literal (then judged like a nested command), anything built, piped or from a variable is a finding; a Remove-Item alias given an @splat is a finding; a PowerShell backtick line continuation is joined and the joined text is scanned as well; a finding inside an eval is shown as "eval ...". Nothing scanned is run. The 227 allow-list and fail-closed rules are unchanged. eval "$(ssh-agent -s)" is untouched (still reported; open point for the architect).

Tests: red 1d 4 tests OK; the 15 modules 429 tests OK; full `python -m unittest discover -s tests` 1269 tests OK (skipped=1), 1726 s. The log carries the "SMOKE FAILED" lines printed by a smoke test inside the suite; the run itself passed.

Records repo: no unpushed commit was sitting in the shared checkout.

Size: 1470 bytes
