# Card 228 turn: red 1a (QA seat red, ctx-3)

Commit: 05a838fb6abeadd1a3822a06948623d807a4daf0 (mob-crosscli, pushed to origin main). Probe: 2026-11-01 20:31 UTC.

Intent (D165): D169, recursive deletes never target anything outside the repository working tree, so a mis-quoted delete like the 2026-10-08 home-folder wipe cannot take out work git cannot restore; the scan must judge a command it can resolve and report one it cannot, never call it clean.

Assumption (D166): a verb spelled with quotes, a backslash, a parameter default or an assigned variable still resolves to a literal rm at command position, so the scan can judge it like the plain form.

Criterion 1, increment 1a (first red only). File: tests/test_command_scan_hidden_1a.py (new, 83 lines).

D167 entries covered: hidden verbs r""m, r\m, 'r'm, "rm", ${RM:-rm}, `v=rm; $v -rf x`. Twins that must stay clean: rm file.txt, r""m file.txt, echo, v=ls; $v -la x, r""m -rf scratch/x, ${RM:-ls} -la x. ZOMBIES: Z zero commands is clean, O one line per form, M seven forms plus a repeat give seven lines. New line shape: `seat: kind: snippet (source <path>)`. Not in 1a (later increments): encodings, interpreters, wrappers, path forms.

Acceptance: fixture transcripts generated into a temp folder, run through `python delete_guard.py scan-transcript <file> --seat "red ctx-3"`; delete commands are text, never executed.

Failing output: `python -m unittest tests.test_command_scan_hidden_1a` gives 4 tests, 8 failures. Six hidden forms print "clean: red ctx-3, 2 command(s) scanned" (exit 0, expected 1); "rm" -rf $D is found but its line lacks the "(source <path>)" suffix; the many-forms test sees 1 line, expected 7. The twins and zero-command tests pass (born green guards).

Existing tests: 10 command-scan and command-source modules, 522 tests, OK (1 skipped). The full discover run exceeded 2 minutes and was not awaited.

Size: 4231 bytes
