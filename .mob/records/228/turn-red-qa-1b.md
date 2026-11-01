# Card 228 turn: red 1b (QA seat red, ctx-3)

Commit: 82c1e84 (mob-crosscli, pushed to origin main). Probe: 2026-11-01 21:14 UTC. Run against committed code only: git status showed only the untracked .mob/ folder and the new test file.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a hidden delete like the 2026-10-08 home-folder wipe cannot take out work git cannot restore.

Assumption (D166): a base64 payload under -EncodedCommand (UTF-16LE; prefixes -enc, -ec) or piped through base64 -d into a shell is the command that will run, so decoding it to depth 6 and scanning the result is the same judgment as the plain text; a payload that cannot be decoded (bad base64, UTF-8 under -EncodedCommand) is a finding, never clean.

Criterion 1, increment 1b: the ENCODED group (first red only). File: tests/test_command_scan_encoded_1b.py (new).
Covered: powershell -EncodedCommand, pwsh -enc, pwsh -ec (UTF-16LE of Remove-Item -Recurse -Force $D); echo <b64> | base64 -d | sh, base64 --decode | bash; double encoding; undecodable: !!!notbase64, UTF-8 under -EncodedCommand, invalid base64 into bash. Decoded cases show the decoded delete in the snippet. Twins that must stay clean: -EncodedCommand of Get-ChildItem and Write-Output, base64 -d | sh of ls, base64 -d with no shell consumer (screen, and > file), a decoded rm -rf scratch/x, echo. Line shape: `seat: kind: snippet (source <path>)`. Fixtures are text given to the scanner via temp transcripts; nothing decoded is executed.
Left for later increments: iex/Invoke-Expression and eval of a built string ([char], "Remove"+"-Item"), xxd -r -p / printf hex | sh, $'\x72\x6d', interpreters (node/perl/ruby/php/python), wrappers (busybox, <(...), heredoc into sh, bash < f, depth 7), alias, pipeline-fed/splatted Remove-Item, git reset --hard / checkout -f / clean, rsync --delete, path forms.

Failing output: `python -m unittest tests.test_command_scan_encoded_1b` gives Ran 4 tests, FAILED (failures=10). Every encoded form prints "clean: red ctx-3, N command(s) scanned" (exit 0, expected 1): 6 decodable, 3 undecodable, plus the many-forms test. The twins test passes (born green guard).

Modules run (12, 417 tests, OK): tests.test_command_scan tests.test_command_scan_blocks tests.test_command_scan_entry tests.test_command_scan_forms2 tests.test_command_scan_guards_1d tests.test_command_scan_hidden_1a tests.test_command_scan_limits tests.test_command_scan_wiring tests.test_command_scans_record tests.test_command_scans_record_strict tests.test_command_source tests.test_command_source_tools. Full discover not run (green seat runs it).

Size: 6447 bytes
