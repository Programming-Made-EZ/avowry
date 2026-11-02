# Card 228 turn: green 1f (backend dev seat green, ctx-5)

Commit: 844f083 (mob-crosscli, pushed to origin main). Green for red ca59449. Probe: 2026-11-02 00:35 UTC. The records repo had no unpushed commit from another seat.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a delete hidden in hex, a script fed to a shell, an alias or a [char] sum is reported from executed commands.

Change (delete_guard.py only, decoding is pure string work, nothing scanned or decoded is ever run):
- Hex or escapes fed to a shell (xxd -r -p from echo, a here-string or a file; printf and echo -e with hex escapes) are decoded and the decoded text is scanned; unreadable hex into a shell is a finding. A dollar-quoted hex word at command position is read as its verb.
- bash/source <(...) scans an echo/printf body and reports any other substitution; bash < file is a finding. alias and Set-Alias definitions are tracked and the expanded command is judged. A [char] sum that spells a delete verb is a finding.
- Card 227 allow-list and fail-closed rules untouched; eval "$(ssh-agent -s)" is still a finding.

Tests: red 1f tests.test_command_scan_hexwrap_1f 6 tests OK (was 17 failures). The 17 modules 439 tests OK. Full discover 1279 tests OK (skipped=1), about 29 minutes (its output includes a SMOKE FAILED line from a fixture smoke test; the run still reports OK).

Needs next: green or red 1g (I and M ZOMBIES, D167 path forms), then criteria 2 and 3.

Size: 1505 bytes
