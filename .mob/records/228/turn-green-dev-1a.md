# Card 228 green 1a: backend developer (green, ctx-5)

Intent (D165): D169, recursive deletes never target anything outside the repository working tree; a delete spelled so the scan cannot see it must be reported like the plain form, never called clean.

Green for red 05a838f (tests/test_command_scan_hidden_1a.py). Commit 63e4f61 in mob-crosscli, pushed to origin main. Probe 2026-11-01 20:31 UTC.

## The change (delete_guard.py only; command_source.py unchanged)
- New `_hidden_verb`: a command-position word spelled with quotes, a backslash, a `${V:-rm}` default, or a variable assigned earlier on a clause of its own (`v=rm; $v`) resolves to its literal; it counts only when the result is a delete verb, so no other word changes. `_scan_text` tracks such assignments and calls `_scan_words` with the resolved verb and the original text as the snippet (new optional `shown` argument).
- `scan-transcript` prints one line per finding: `seat: kind: snippet (source <path>)`.
- Card 227's allow-list (literal strict child of scratch) and fail-closed rules are untouched. Line endings: LF before and after (0 CR bytes), as in card 227's greens.

## Test counts
- Red 1a: 4 of 4 pass.
- Command-scan and command-source modules (12 files, test_command_scan*, test_command_scans_record*, test_command_source*): 417 tests OK. (QA counted 522; the glob here gives 417, including the 4 of red 1a.)
- Full suite `python -m unittest discover -s tests`: 1257 tests, OK, 1 skipped.

Commit: 63e4f61
Size: 3665 bytes (git diff HEAD~1 HEAD of the commit, wc -c)
