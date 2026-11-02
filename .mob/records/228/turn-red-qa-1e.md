# Card 228 turn: red 1e (QA seat red, ctx-3)

Commit: 025b178 (mob-crosscli, pushed to origin main). Probe: 2026-11-01 23:43 UTC. Run against committed code only: git status showed only the untracked .mob/ folder. The records repo had no unpushed commit from another seat.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a delete spelled in an interpreter one-liner or made by rsync --delete cannot take out work git cannot restore (the 2026-10-08 home-folder wipe).

Assumption (D166): the scan sees only command text, so an interpreter call that removes a tree or a glob with a non-literal target, and rsync with a delete flag, are findings whatever the arguments; a call that only prints or removes one literal file is clean.

Correction to the slice (from the architect ruling, ruling-sshagent-architect.md): slice line 63 says `eval "$(ssh-agent -s)"` "keeps its card 227 result". The ruling found no card 227 test pins it. Per the ruling it is NOT a clean twin: it stays a recorded finding ("recursive delete on a variable or unresolved path"), fail closed, and no green in card 228 teaches an allow. Read line 63's twin list without that form. The slice file is not edited.

Criterion 1, increment 1e, first part (D157): the INTERPRETER and rsync forms, split off from the wrappers, hex and alias so the green stays small. File: tests/test_command_scan_interp_1e.py (new). 14 forms: ruby FileUtils.rm_rf (two), php system/exec of rm -rf (two), deno eval and deno -e Deno.removeSync recursive, bun -e rmSync recursive, awk system("rm -rf"), python - <<EOF with shutil.rmtree, perl unlink glob, rsync --delete, --del, --delete-after, --remove-source-files. Line shape `seat: kind: snippet (source <path>)`. Twins that must stay clean (12): ruby puts and File.delete of one file, php echo, deno and bun console.log, awk print, python heredoc print, perl print and unlink of one file, rsync -a without a delete flag, grep rm_rf, echo.

Born-green guard (honest note): python - <<EOF with shutil.rmtree is already reported today, so 1 of 14 forms passes now as a regression guard; the twins test and the no-commands test pass now.

Failing output: python -m unittest tests.test_command_scan_interp_1e gives Ran 4 tests, FAILED (failures=14): 13 forms print "clean" (exit 0, expected 1) and the all-forms test sees 1 line, expected 14.

Modules run (16 counting the new one: the 15 from turn-red-qa-1d.md plus tests.test_command_scan_interp_1e; 433 tests, FAILED failures=14, all in the new module; the 15 earlier modules pass, 429 tests). Full discover not run.

Left in criterion 1 after 1e: for 1f, hex into a shell (xxd -r -p | sh, printf '\x72\x6d' | sh, $'\x72\x6d'), wrappers not yet reported (<(...), bash < f), alias, [char]114+[char]109 (busybox, heredoc into sh, wsl and ssh are reported today and need only guards). For the last cycle: the B ZOMBIES (depth 7 is a finding, a command over 500,000 bytes is a finding), the I ZOMBIE (scan_entry, the CLI and facilitator.run_backend give the same findings), the M ZOMBIE (ten forms in a file, duplicates once) across all cycles, and the D167 path forms (./scratch, scratch/.., Windows trimming, Cyrillic, NUL, UNC, ~, $HOME). Then criteria 2 and 3.

Size: 3269 bytes
