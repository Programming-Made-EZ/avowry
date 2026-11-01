# Card 228 turn: red 1c (QA seat red, ctx-3)

Commit: c743784 (mob-crosscli, pushed to origin main). Probe: 2026-11-01 22:06 UTC. Run against committed code only: git status showed only the untracked .mob/ folder.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a delete made through node, perl, python or .NET, like the 2026-10-08 home-folder wipe, cannot take out work git cannot restore.

Assumption (D166): a recursive delete call named in an interpreter one-liner (node -e, perl -e, python -c, pwsh -c) or run as rimraf is the command that will run, and is judged like plain rm -rf (only a literal strict child of scratch is allowed); an argv list or concatenated string spelling a delete counts.

Criterion 1, increment 1c: the NON-SHELL group (first red only). File: tests/test_command_scan_nonshell_1c.py (new). Covered (13 forms): node -e/--eval with fs.rmSync, rmdirSync, fs.rm and recursive; npx rimraf; rimraf ../build; perl rmtree and File::Path remove_tree; [IO.Directory]::Delete($p,$true); os.removedirs; os.system with 'rm -rf '+p; subprocess.run argv lists (["rm","-rf",p] and ["rm","-r","-f",p]). Line shape: `seat: kind: snippet (source <path>)`; the kind is not pinned (green chooses), the snippet must show the call. Twins that must stay clean: fs.rmSync without recursive, [IO.Directory]::Delete(p), os.rmdir, Path.unlink, os.system/subprocess of ls, a string list naming rmtree/rimraf, grep rmSync, grep rimraf, rimraf scratch/x, echo; an empty transcript is clean.
Note for green: the twin print(['rmtree','rimraf']) is reported today (text match), so the twins test is red too, not a born-green guard; the green needs the python -c text judged by an ast walk for calls (slice layer 2 _interpreter).
Left for later increments: iex/Invoke-Expression and eval of a built string; hex into a shell (xxd -r -p, printf, $'rm'); ruby/php/deno/bun/awk and python - <<EOF; wrappers (busybox, <(...), heredoc into sh, bash < f, depth 7); alias; pipeline-fed and splatted Remove-Item; git reset --hard, checkout -f, clean; rsync --delete; path forms.

Failing output: `python -m unittest tests.test_command_scan_nonshell_1c` gives Ran 4 tests, FAILED (failures=15): all 13 forms print "clean" (exit 0, expected 1), the many-forms test fails, and the twins test fails on "red ctx-3: recursive delete on a variable or unresolved path: rmtree','rimraf']) (source ...)" (a false positive).

Modules run (14, 425 tests, FAILED failures=15, all in the new module; the other 13 modules pass, 421 tests): tests.test_command_scan tests.test_command_scan_blocks tests.test_command_scan_entry tests.test_command_scan_forms2 tests.test_command_scan_guards_1d tests.test_command_scan_hidden_1a tests.test_command_scan_limits tests.test_command_scan_wiring tests.test_command_scans_record tests.test_command_scans_record_strict tests.test_command_source tests.test_command_source_tools tests.test_command_scan_encoded_1b tests.test_command_scan_nonshell_1c. Full discover not run (green seat runs it).

Size: 3046 bytes
