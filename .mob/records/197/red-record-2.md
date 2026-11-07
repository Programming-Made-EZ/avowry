# Card 197 red 2 record (QA seat, Criterion 2: O, M, I)

Born green. `class ListsAndPaths` (8 tests) is appended to `tests/test_rename_197.py`; `RenameCycle1` is untouched (only the module docstring and the import block grew). Written at HEAD 258bf1a (the green, 2d58c6c, plus its record). `python -m unittest tests.test_rename_197`: 15 tests OK, about 31 s (the class alone: 8 tests, about 31 s; the five-module test and the two dotnet list calls dominate).

## The tests (all pass on HEAD)
- test_solution_projects: the 10 Project lines of Avowry.sln equal `baseline/project-lines.txt` under the byte rules (names, paths, GUIDs, order); the 7 csproj paths (10 lines less the 3 solution folders) are tracked files, exact case.
- test_lists_equal_baseline: `dotnet test tests/<project> --list-tests` for Compiler (284) and Acceptance (317), 4-space rows, the 14 `(source: "...` rows rewritten to `(source: <path>)`, sorted, duplicates kept; equal name for name to the committed baseline (which already carries the Avowry names). Failure message gives counts and the multiset difference.
- test_bench_self_check: the command is `dotnet run --project tools/Avowry.Bench -- --self-check` (bench/README.md line 16, matched by regex and run exactly, via shlex). It is cheap (about 6 s warm) but it writes under `bench/results/self-check`, so the test copies the tracked files into a TemporaryDirectory and runs there: `dotnet build tools/Avowry.Bench --nologo -v q` (exit 0), exactly one `Avowry.Bench.dll` under `tools/Avowry.Bench/bin`, then the self-check (exit 0). The self-check output ends `noise: pass_rate=0 points; attempts=0; tokens_in=0; tokens_out=0`.
- test_five_modules: `python -m unittest <module>` per module: 6, 17, 21, 36, 4 tests, exit 0, output ends `OK`.
- test_launcher_scan_not_vacuous: `TARGETS_BENCH` (loaded from the module by path) matches exactly 2 acceptance files outside the launcher and contract: `BenchmarkHarnessAcceptance.cs` and `PrivateHoldoutResults.cs` (the architect's 3 includes the contract file, which the scan excludes).
- test_triple_agree: the `tools/<x>/Program.cs` paths named in the ADR, in `scripts/check-library-binding-layering.py` and in `tests/test_library_binding_layering.py` are the same two (`tools/Avowry.Bench/Program.cs`, `tools/Avowry.BenchCs/Program.cs`), and both are tracked. (Whole-file extraction, not line numbers, so it survives edits above them.)
- test_run_llm_check_path: `run-llm-check.py` names `repo / "tools" / "Avowry.Bench" / "Avowry.Bench.csproj"` exactly once, and that path is tracked.
- test_probe_matches_solution: exactly one tracked root .sln (`Avowry.sln`); every `*.sln` token in `RepoPaths.cs` (the probe and its exception message) and in `BenchLauncherContract.cs` (line 199 and the `dotnet build Avowry.sln` remediation strings) equals it.

## Mutation evidence
Harness: scratchpad `mutate197.py` (not committed). Each mutation: `git archive HEAD` into a TemporaryDirectory, working-tree test file overlaid (it is not at HEAD yet), `git init` and one commit, mutate, `git add -A`, `git diff --cached --binary` -> `.mob/records/197/mutations/<P>.patch`. Then a second fresh copy: `git apply --check` (exit 0 for all six), `git apply` (exit 0), and the tests run there. Patches built from real bytes (`b"\n".join` on split lines), so P2b keeps its CRLF context (8 CR bytes in the patch).

| Patch | Change | Fails | Quote |
|---|---|---|---|
| P2a | RepoPaths.cs:13 `Avowry.sln` -> old name | test_probe_matches_solution; also RenameCycle1 test_old_names_gone_from_live_lines (`RepoPaths.cs:13`) and test_put_back_repopaths_probe (pristine precondition) | `['Avowry.sln'] != ['Avowry.sln', 'Phrase.sln'] : ...RepoPaths.cs names a .sln that is not the tracked root solution` |
| P2b | scan_264.py:22 `Avowry\.Bench` x3 -> old escaped-dot form | test_launcher_scan_not_vacuous; also RenameCycle1 scan (`scan_264.py:22`) | `TARGETS_BENCH matches 0 acceptance files outside the launcher and contract (2 expected on HEAD)` |
| P2c | run-llm-check.py:499 -> old Bench path | test_run_llm_check_path; also RenameCycle1 test_put_back_run_llm_check | `[('Avowry.Bench', 'Avowry.Bench.csproj')] != [('Phrase.Bench', 'Phrase.Bench.csproj')]` |
| P2d | ADR: `tools/Avowry.BenchCs/Program.cs` -> `tools/Avowry.Bench/Program.cs` | test_triple_agree; the old-name scan stays OK (1 test OK) | `'adr': ['tools/Avowry.Bench/Program.cs']` against the script's two paths |
| P2e | Avowry.Bench and Avowry.BenchCs GUIDs swapped on their Project lines | test_solution_projects | `First differing element 7: ...Avowry.Bench.csproj", "{16FFAB0C-...}" vs "{BB7FDE05-...}"` |
| P2f | delete `corpus/empty-record.avowry` and `.expected.json` | test_lists_equal_baseline (Acceptance) | `317 != 316 : expected 317 rows, listed 316; 1 missing, 0 extra` (the missing row is the normalised `source: <path>` row) |

Durations in the scratch copy: P2a-P2e tests each 0.3 to 1.8 s; P2f 12.9 s (first build). The whole harness for P2a-P2e took 98 s, P2f 30 s.

## Surprising, for the facilitator
- The slice says that under P2b the module itself stays at 6 OK (the vacuity). It does not: `tests.test_bench_launcher_scan_264` goes `FAILED (failures=2)` in the P2b copy, because its two ScannerBehaviour fixture tests (test_one_direct_start_is_flagged_with_its_line, test_many_starts_in_many_files_are_all_flagged) build fixtures that use the new names, which the mutated regex no longer matches. What stays green is `RealTree.test_simple_no_acceptance_test_starts_..._except_through_the_shared_launcher`, the check against the real acceptance directory (it passes vacuously with 0 matching files). That is the gap my test closes. Either way a developer would see the module red; the unique value is the real-tree vacuity.
- P2d: the ADR change also turns `tests.test_library_binding_layering` red (1 of 12: `audit lacks tools/Avowry.BenchCs/Program.cs`), the old-name scan stays green, as the slice said. test_triple_agree is the one that names which of the three disagrees.
- The README self-check writes into `bench/results/self-check` of the tree it runs in, so it runs in a copy; the first build in the copy is part of the 31 s.
- Nothing was run in the main checkout beyond the python modules and `dotnet test <project> --list-tests` (an incremental build into ignored bin/obj). No `Avowry.sln` test run, no push.
