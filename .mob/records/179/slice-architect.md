# Card 179 slice (architect seat, a fresh Claude context, read-only; nothing created or run except python on the live tree: test_live_docs_266 reads 21 links and ends in the known failure)

## 1. Decision on correction 2: (a)
Remove the status table, the script and its test.
- Criterion 1 drops the table. Option (b) would keep a table plus its `DOTNET_ROLL_FORWARD` claim (README.md:39), which Criterion 1 also drops.
- Nothing calls the script except its own test: not ci.yml, HANDOFF or the agreement. Without the row, `main()` returns 1 (scripts/check-readme-reference-status.py:41).
- Both rename tests list the module only as one tuple line: test_rename_196.py:296 and test_rename_197.py:326.

The green commit touches:
- README.md (rewrite) and docs/GETTING_STARTED.md (new).
- docs/PREREQUISITES.md lines 7 to 9.
- Deleted: the script and tests/test_readme_reference_status.py.
- Deleted: the tuple line at test_rename_196.py:296 and at test_rename_197.py:326.
- Two edits the draft missed:
  - test_rename_197.py:591 pins `"README.md": 2`, the two `dotnet test Avowry.sln` lines in Running. It becomes 0, and the "8" in the message at :592 becomes 6.
  - test_live_docs_266.py T03 (:432) does `lines.index("## Layout")`, which raises once Layout is dropped. Delete T03 only.
- The names `test_five_modules` and `FIVE_MODULES` stay. Name this in the green record.

README constraints so these tests stay green without edits:
- Line 3 holds `Avowry` and is not the rename line (T10 :595 expects exactly one problem).
- The rename line is exact and appears once (:264).
- `# Avowry` is the first line. The command sentence, the D125 sentence and the generated-code sentence stay exact (test_license_files:32 to 47).
- No other `Phrase` appears.
- At least 5 relative links remain, all to tracked files (ScopedLinks floor 15 in total, :187).

The red adds files only and must not break any of: test_license_files, test_live_docs_266 (T03 needs the Layout until the green), test_prerequisites, test_demo_readme, test_rename_196/197. T11 (bug 338) is already red.

## 2. Files
- Characterisation: tests/test_demo_readme_untouched_179.py (no C# wrapper, as in 178).
- Red: tests/test_getting_started.py, tests/Avowry.Acceptance.Tests/GettingStartedDocs.cs, and `"GettingStartedDocs.cs"` in `ProcessStartCallers` (GeneratedApiSourceScan.cs:22 to 29; the list is compared sorted). Also `.mob/records/179/red-record.md`.
- Green: the files in section 1, plus removing the one `[Trait("Status", "Pending")]` line.

## 3. Test design (tests/test_getting_started.py, 350 lines or fewer, regex and needle tables, no parser)
Every check is an id in one `failures` list, ended by `assertEqual([], failures, HEADER + ...)`. A missing page gives `page: ... is missing` plus `blocked: page missing` per id, as test_prerequisites.py:437.

README ids, each its own check (the keeps and drops of Criterion 1 under decision (a)):
- Keeps: the H1; the three exact sentences; the rename line once; the clone URL; the two links; `its own language`; `C# adapter preview`; `FastEndpoints`; a paragraph with `Dana` and `agent`; `verifiability`, `preserved intent` and `reviewability` as three ids; `known limitations` with `card 187`.
- Drops, one id each: `## Layout`, `Test status`, any line starting `|`, `Pending`, `resolver` (case-insensitive), `NotImplementedException`, `\btrace\b`, `\bPH\d*\b`, `DOTNET_ROLL_FORWARD`, `dotnet test`, Wasm, WebAssembly, `ReferenceTarget`.
- Other ids: line count at most 50; every relative link resolves to an existing file; the script and test are absent (`status-script-retired`).

PREREQUISITES ids:
- `newer-sdk`: one sentence holds `newer SDK`, `untested`, `does not replace` and `.NET 8 SDK`. Today's line 9 fails it.
- Keep the 178 `untested` paragraph, and the `architecture` plus `unpinned` sentence. Run `collect_failures(read_page(), parse_all_workflows())` from tests.test_prerequisites.

Guide ids:
- clone, with the URL.
- `dotnet pack src/Avowry.Cli/Avowry.Cli.csproj`, and `dotnet tool install Avowry --tool-path` with `--add-source`.
- No angle-bracket placeholder inside any fence.
- Build by `avowry(.exe)"? build ...Tasks.avowry -o`, and no `dotnet run --project src/Avowry.Cli` in any fence.
- An `avowry` fence whose non-blank lines appear, in order, as whole lines of demo/tasks/Tasks.avowry. It holds `POST /tasks`, `GET /tasks/{id}` and `fails with`.
- `dotnet test` of Tasks.Specs and `Passed: N` with N counted from `spec "` lines.
- `dotnet run --project` of Tasks.Api with `--urls http://localhost:5099`.
- A create call, a read call and a `422` naming a declared failure.
- Afterwards, three ids: `.avowry` is source; generated is output; rebuild overwrites the same relative paths and does not delete leftover files.
- The PREREQUISITES link and `.NET 8 SDK`.
- Reuse `command_failures` from tests.test_prerequisites on the guide text, which covers Run from, Needs, Same in PowerShell and curl needing a real block.
- Extra PowerShell rules (no bare `curl` in a pwsh block; at least one pwsh block with `Invoke-RestMethod` or `curl.exe`):
  - A pwsh block holds `$env:TEMP\` or `X:\`.
  - No pwsh block holds `/tmp/`.
- No `Phrase`, `Wasm` or `resolver`.

Self-tests in a separate class, with embedded fixtures only and a synthetic Tasks source, so they pass on both trees:
- Compliant fixtures give `[]`.
- A missing guide gives the missing id and the blocked ids, with no exception.
- A compliant README plus a restyled table (the NotImplementedException row removed, the Pending resolver row or a `|` row kept) fails.
- A curl bash block with no pwsh block fails, and so does a pwsh block with a bare `curl`.
- The old PREREQUISITES lines 7 to 9 fail `newer-sdk`, and the new wording passes.

Live state reads the working tree. No history is read, so D202/D206/D217 do not apply.

C# wrapper: copy PrerequisitesPage.cs. Class `GettingStartedDocs`; method `Getting_started_docs_match_danas_story`, which runs `-m unittest tests.test_getting_started`. Attributes in order: Fact, Slice 179, Criterion 179-1, Status Pending.
- Red body line: `Criterion: 1-3`.
- Green body line: `Criterion: 1-3`. The green removes only the Pending line and leaves the python file byte-identical.

## 4. Characterisation
Module tests.test_demo_readme_untouched_179, built like test_readmes_untouched_178.py:
- BASE is the 40-character HEAD at its commit. It is checked as an ancestor of HEAD.
- Each side is read with `git show REV:demo/tasks/README.md` (exact key). An empty blob fails.
- The comparison commit is `git log --diff-filter=A -1 -- docs/GETTING_STARTED.md`, or HEAD if none. No subject prefix is used.
- Self-tests run in a `git init` synthetic repo, not a clone (178 revision 1): a look-alike commit and an uncommitted edit stay equal, and a guide-adding commit that edits the demo README reports a difference.
- Before the red is filed, build a scratch tree with the guide and README already in the new state and show it passes. Then show an edit of the demo README failing.

## 5. Proven compile target
- demo/tasks/Tasks.avowry is the only one proven for the installed tool. PackedToolIsVersionedInstallsAndBuildsTheDemo.cs:46 to 74 packs, installs and builds it, and checks the two csproj files.
- It is also proven for build, 11 specs, start and calls through the dev CLI (TasksDemoApp.cs:73 to 80; UsableV1.cs).
- orders.avowry (CompileEndpointToCSharp.cs:17 to 40) is proven only through the dev CLI and `dotnet test` of Specs. It has no read endpoint.
- No smaller listing is proven for create, read and failure. The guide shows an excerpt of Tasks.avowry (guarded by the in-order-lines check) and says which file it compiles.

Commands, bash then PowerShell (PowerShell uses `$env:TEMP\avowry-feed` for FEED, `$env:TEMP\avowry-tools` for TOOLS and `$env:TEMP\avowry-tasks` for OUT; bash uses `/tmp/...`):
1. `git clone https://github.com/Programming-Made-EZ/avowry.git`, then `cd avowry`.
2. `dotnet pack src/Avowry.Cli/Avowry.Cli.csproj -c Release -o FEED`.
3. `dotnet tool install Avowry --tool-path TOOLS --add-source FEED`.
4. `TOOLS/avowry --version`, which prints `Avowry 0.1.0`. The Windows file is `avowry.exe`.
5. `TOOLS/avowry build demo/tasks/Tasks.avowry -o OUT`. This writes Tasks.Api, Tasks.Specs and Tasks.avowrymap.json.
6. `dotnet test OUT/Tasks.Specs`, which gives `Passed: 11`.
7. `dotnet run --project OUT/Tasks.Api --urls http://localhost:5099` in a second window.
8. `Invoke-RestMethod -Method Post -Uri http://localhost:5099/tasks -ContentType application/json -Body '{"id":"task-1","title":"Write the demo"}'`.
9. `Invoke-RestMethod http://localhost:5099/tasks/task-1`.
10. A failure call with an empty title.

The facilitator runs once in a scratch directory before the red, in Windows PowerShell 5.1 and in pwsh: steps 1 to 10, then a second build after adding a leftover file in OUT (WriteTo, CompilationOutput.cs:14 to 21). The probe records:
- The unpushed-HEAD limit on the clone (it fetches pushed commits only).
- Whether restore needs nuget.org for the generated projects.
- The JSON shape of each call.
- How a 422 surfaces (`ErrorDetails.Message`).
- The SDK list and any roll-forward need.

## 6. Order and size
Characterisation, then red, then green. Caps: the wrapper at most 45 lines, the python test 350 lines or fewer, and no more green cycles for the characterisation. The gate reviewer replays:
- the red on the commit before the green: the named test fails by assertion among N, with the self-tests passing;
- the characterisation on both trees;
- `python -m unittest` for tests.test_getting_started, test_prerequisites, test_demo_readme, test_license_files, test_live_docs_266, test_rename_196 and test_rename_197 (these two need a clone);
- `dotnet test Avowry.sln --filter "Status!=Pending"`, and `git diff red green --name-status`.

## 7. Risks not verified
- test_rename_197 `test_lists_equal_baseline` pins 317 acceptance tests (:384). The new C# test may break it, if it is not already red. Check it on HEAD.
- Quoting of JSON bodies, and the 422 behaviour of `Invoke-RestMethod`, in Windows PowerShell 5.1 versus pwsh.
- The clone URL is not fetched.
- Whether `dotnet test` of the generated projects works offline or on a newer SDK.
