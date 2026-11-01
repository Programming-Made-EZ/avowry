# Card 228 turn: green 1c (backend dev seat green, ctx-5)

Commit: 9fe3913 (mob-crosscli, pushed to origin main). Green for red c743784. Probe: 2026-11-01 22:06 UTC.

Intent (D165): D169, recursive deletes never target anything outside the working tree, so a delete made through node, perl, python or .NET cannot hide a home-folder wipe from the scan.

Change (delete_guard.py): python -c and python heredoc text is now judged by an ast walk, not a text match, so print(['rmtree','rimraf']) is clean while rmtree/removedirs (called, assigned, passed, imported, getattr) stay findings; os.system/popen/subprocess calls whose string (concatenation) or argv list spells a delete are findings, shown as the call; exec/eval of a literal is read as Python; text that does not parse falls back to the old text match. node -e/--eval with fs.rmSync/rmdirSync/rm/rmdir and recursive:true, perl -e rmtree/remove_tree, and [IO.Directory]::Delete(p,$true) are findings unless the target is a literal strict child of scratch; rimraf (and npx/runner rimraf) is judged like rm -r. Nothing scanned is run. The 227 allow-list and fail-closed rules are unchanged.

Tests: red 1c 4 tests OK; 14 modules 425 tests OK; full `python -m unittest discover -s tests` 1265 tests OK (skipped=1), 1721 s. The log has the same "SMOKE FAILED" line printed by a smoke test inside the suite; the run itself passed.

Size: 2080 bytes
