# Card 179 sign-off (a fresh Claude context, separate from the gate reviewer; read-only; own scratch single-branch clone signoff179-clone; it read the gate review only after its own reruns)

Verdict: SIGNED OFF WITH CONDITIONS. The conditions are follow-ups for later cards; none blocks card 179.

## Findings
1. Promises against criteria. README.md, docs/GETTING_STARTED.md and docs/PREREQUISITES.md read as Dana agree with each other. Criterion 1 covers the README (own language and C# adapter preview, who it is for, the three aims, the clone URL, links to the guide and prerequisites, the limitations page named as card 187 with no link; 30 lines; no Layout, status table, resolver, trace, Wasm or other target; the only Phrase is the exact rename line). Criterion 2 covers the guide (clone, pack, install, `avowry build`, the Tasks.avowry excerpt with create, read and `fails with EmptyTitle`, specs `Passed: 11`, start, the 422 call, Afterwards) and the PREREQUISITES reword. Criterion 3 covers the PowerShell forms and `$env:TEMP` paths. Not checked by any test: the "about 15 minutes" claim (card 181); the bash forms (only PowerShell 7 was probed); Windows PowerShell 5.1 (the `.Exception.Response.StatusCode.value__` form is unrun there). No contradiction among the three pages; "Windows needs no bash" beside "other OS untested" claims a path, not a test.
2. Red replay: pass. The characterisation passes 4 of 4 at 8086854; demo/tasks/README.md is identical to b308034 at 8086854 and at the tip. At ed5819c `tests.test_getting_started` ran 7 tests, 1 assertion failure (`clone-url: missing`, `page: docs/GETTING_STARTED.md is missing`) and 6 self-tests ok; after `dotnet build src/Avowry.Cli` (0 warnings) the wrapper `Getting_started_docs_match_danas_story` failed by assertion; at the tip the wrapper passes 1 of 1. tests/test_getting_started.py is identical between ed5819c and the tip; the only wrapper change is the removed Pending trait line.
3. Full gate: Compiler 324 of 324, Acceptance 328 of 328 (10 m 11 s), 0 warnings; the benchmark 20-second flake (issue 304) did not occur.
4. Neighbours: the first seven python modules ran 50 tests with one failure, T11 (bug 338); rename_196 has 5 failures and rename_197 has 2 (7); the same 7 fail at b308034: all red before the card, nothing new red.
5. Scope and value: the diff from b308034 has 13 files, 4 of them under .mob or MOB.md, all traced to the slice. The retirements are justified by the card (the status script and its test required the stale table Criterion 1 drops; the two rename tuple lines; the README `dotnet test` count 2 to 0 and message 8 to 6; T03 deleted because Layout is dropped). Dana's value is delivered and nothing beyond it.
6. Remote state: `git ls-remote` shows GitHub main at 8a5c618, so the clone URL serves the guide as reviewed.

## Gate evidence
Tested commit ecd3e6e (main tip, clean tree). Copy: `git clone --no-hardlinks --single-branch -b main` from C:/Users/<user>/projects/phrase-lang into signoff179-clone, not an archive. `dotnet build src/Avowry.Cli`: 0 warnings, 0 errors. SDK 10.0.403 only, no 8.0.x SDK installed. Compiler 324/324 and Acceptance 328/328 under `--filter "Status!=Pending"`.

## Load
One dotnet process at the start (tasklist count 1). dotnet commands one at a time. A `git checkout` moved the clone to b308034, 8086854 and ed5819c for the replays and back to main. Nothing deleted or modified in phrase-lang.

## Conditions (non-blocking, for later cards)
- Card 180 (run the guide's commands) runs the bash forms, Windows PowerShell 5.1, an 8.0.x SDK and a clone from the pushed remote; the PowerShell 5.1 try/catch status form and the "listening" line are unverified.
- Card 181 (cold model run) owns the "about 15 minutes" claim.
- The characterisation guards the demo README only at the guide-adding commit, so a later edit passes (as the gate review also notes); tests/test_demo_readme.py guards its content only.
- Open cold-reader gaps: rerun into the same TEMP folders; "Dana stays free of any target" beside a single C# adapter. Both are acknowledged in cold-reader.md.
