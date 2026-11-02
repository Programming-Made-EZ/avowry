# Card 228 turn: red 1f (QA seat red, ctx-3)

Commit: ca59449 (mob-crosscli, pushed to origin main). Probe: 2026-11-02 00:35 UTC. Run against committed code only: git status showed only the untracked .mob/ folder (the new test file was untracked until the commit, no source file modified). The records repo had no unpushed commit from another seat.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a delete hidden in hex, a script piped to a shell, an alias or a [char] sum cannot take out work git cannot restore (the 2026-10-08 home-folder wipe).

Assumption (D166): the scan sees only command text, so hex that decodes to a recursive delete is a finding and shows the decoded text; unreadable hex, a shell fed from <(...) or from a redirected file, and an alias or [char] sum that makes a recursive delete are findings whatever follows; hex or an alias that makes a harmless command, and a redirect into cat or sort, are clean.

Criterion 1, increment 1f (D157): the hex and wrapper group left by 1e. File: tests/test_command_scan_hexwrap_1f.py (new). 20 forms: hex into a shell (xxd -r -p from echo and from a here-string, printf and echo -e with hex escapes, a hex-escaped verb in dollar-quotes, unreadable xxd input), bash <(echo rm -rf), source <(...), bash <(curl ...), bash < clean.sh, sh < /tmp/x.sh, alias d=rm, alias d='rm -rf', Set-Alias d Remove-Item, two [char]114+[char]109 forms, and four regression guards reported today (busybox, heredoc into sh, wsl, ssh). 18 twins stay clean (hex that decodes to ls or to a scratch child, hex to the screen or a file, cat/sort/wc redirects, diff and cat of <(...), harmless aliases, harmless [char] sums). Line shape `seat: kind: snippet (source <path>)`.

Folded in (small): the B ZOMBIES through the CLI line: 6 nested wrappers clean and a 7th one finding line; a 400,000-byte command clean and a 500,001-byte command one finding line under 5,000 characters. Both pass today (card 227 behavior), so they are born-green guards.

Failing output: python -m unittest tests.test_command_scan_hexwrap_1f gives Ran 6 tests, FAILED (failures=17): 16 forms print "clean: red ctx-3, 2 command(s) scanned" (exit 0, expected 1), and the all-forms test sees 4 lines, expected 20. Twins, no-commands and both B tests pass.

Modules run (17 counting the new one: the 16 from turn-red-qa-1e.md, which are every test_command_scan*, test_command_scans_record* and test_command_source* module, plus tests.test_command_scan_hexwrap_1f; 439 tests, FAILED failures=17, all in the new module; the 16 earlier modules pass, 433 tests). Full discover not run.

Needs a 1g: the I ZOMBIE (scan_entry, the CLI and facilitator.run_backend give the same findings for one file), the M ZOMBIE across all cycles (ten forms in one file, duplicates once), and the D167 path forms (./scratch, scratch/.., Windows trimming, Cyrillic, NUL, UNC, ~, $HOME). Not folded in: they touch the facilitator wiring and a new path surface, not the command text forms. Then criteria 2 and 3.

Size: 3058 bytes
