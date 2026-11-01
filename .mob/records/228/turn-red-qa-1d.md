# Card 228 turn: red 1d (QA seat red, ctx-3)

Commit: 40555b4 (mob-crosscli, pushed to origin main). Probe: 2026-11-01 22:50 UTC. Run against committed code only: git status showed only the untracked .mob/ folder.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a command that throws away uncommitted work, runs a string it built itself, or feeds Remove-Item from a pipeline or a splat cannot take out work git cannot restore (the 2026-10-08 home-folder wipe).

Assumption (D166): the scan sees only command text, so git reset --hard, checkout -f/--force and checkout -- . are findings whatever their arguments; iex/Invoke-Expression of a built or unreadable string is a finding (fail closed); a splatted or backtick-continued Remove-Item has no literal target and is judged like a variable.

Criterion 1, increment 1d: the group slice line 27 keeps in this card (first red only). File: tests/test_command_scan_gitpipe_1d.py (new). 17 forms: git reset --hard (two spellings), checkout -f, checkout --force, checkout -- ., clean -fdx, clean -f; iex with a concatenated string; Invoke-Expression of a variable (inside pwsh -c) and of a concatenation; eval of a quote-split rm and of a variable; gci | ri -r; Get-ChildItem | Remove-Item -Recurse; Remove-Item @a; Remove-Item -Recurse @args; Remove-Item with backtick line continuation. Line shape `seat: kind: snippet (source <path>)`. Twins that must stay clean (14): git reset --soft and reset HEAD file, checkout -- file, checkout main, clean -n, status, iex 'Get-Date', Invoke-Expression "Get-Date", Get-ChildItem | Select-Object, a Where-Object/Measure-Object pipeline, Remove-Item file.txt, Remove-Item scratch/x -Recurse, echo.

Born-green guards (honest note): git clean -f/-fdx, eval of a variable, eval of a quote-split rm, gci | ri -r, Get-ChildItem | Remove-Item -Recurse and Remove-Item -Recurse @args are already reported today (so 6 of 17 forms pass now, as regression guards), and the twins test passes now. The red is the other 11 forms plus the all-forms count test (12 failures).

Open question for the architect: the slice says `eval "$(ssh-agent -s)"` "keeps its card 227 result", but no card 227 test pins it and the scan reports it today ("recursive delete on a variable or unresolved path"). I did NOT pin it as a twin. Decide: clean (green must teach it) or a recorded finding.

Failing output: python -m unittest tests.test_command_scan_gitpipe_1d gives Ran 4 tests, FAILED (failures=12): 11 forms print "clean" (exit 0, expected 1) and the count test sees 7 lines, expected 17.

Modules run (15, 429 tests, FAILED failures=12, all in the new module; the 14 modules from turn-red-qa-1c.md pass, 425 tests). Full discover not run.

Left in criterion 1 after 1d: hex into a shell (xxd -r -p, printf '\x72\x6d', $'rm'); ruby/php/deno/bun -e, awk system(), python - <<EOF, perl unlink glob; rsync --delete/--del/--remove-source-files; wrappers (busybox, <(...), heredoc into sh, bash < f, wsl, ssh); alias; [char]114+[char]109; the B ZOMBIES (depth 7 is a finding, a command over 500,000 bytes is a finding); the I ZOMBIE (scan_entry, the CLI and facilitator.run_backend give the same findings); the M ZOMBIE (ten forms in a file, duplicates once) across all cycles; the D167 path forms (./scratch, scratch/.., Windows trimming, Cyrillic, NUL, UNC, ~, $HOME). Then criteria 2 and 3.

Size: 3413 bytes
