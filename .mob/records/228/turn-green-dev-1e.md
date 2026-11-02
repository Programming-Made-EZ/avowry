# Card 228 turn: green 1e (backend dev seat green, ctx-5)

Commit: 53666c3 (mob-crosscli, pushed to origin main). Green for red 025b178. Probe: 2026-11-01 23:43 UTC.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a delete spelled in a ruby, php, deno, bun, awk or perl one-liner, or made by rsync with a delete flag, cannot take out work git cannot restore.

Change (delete_guard.py only, LF kept): ruby FileUtils rm_rf/rm_r/rmtree/remove_dir is a finding unless its target is a literal strict child of scratch; php and awk system/exec/shell_exec of rm -r/-f is a finding; deno and bun are read like node (removeSync joins rmSync, recursive needed); perl unlink of a glob, list or map is a finding (unlink of one literal file stays clean); rsync with --delete, --del, --delete-after, --remove-source-files or --remove-sent-files is a finding. An unreadable code word is a finding for ruby, php, deno, bun (fail closed). Nothing scanned is run. The 227 allow-list is unchanged. eval "$(ssh-agent -s)" is untouched and still a finding, per the architect ruling.

Tests: red 1e 4 tests OK; the 16 modules 433 tests OK; full `python -m unittest discover -s tests` 1273 tests OK (skipped=1), 1714 s. The log carries the "SMOKE FAILED" lines printed by a smoke test inside the suite; the run itself passed.

Records repo: no unpushed commit from another seat was sitting in the shared checkout.

Size: 1451 bytes
