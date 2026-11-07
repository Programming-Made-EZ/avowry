# Card 197 slice (architect seat, read-only; committed with the facilitator's note at the end)

Everything was run at HEAD 185c384 in `git archive` copies, so the repository was not touched.

## 1. Scope: SHIP AS IS, with one amendment to the rules
The amendment: the sed must be `sed -b` and must come from a script file (section 5).

Premises, each rerun:
- **`Phrase.sln` files:** 9. `git grep -I -l 'Phrase\.sln' -- . ':!board' ':!.mob' ':!docs/decisions' ':!docs/retros' | wc -l`. 21 lines (same command with `-h`), 4 of them in `ci.yml`.
- **`Phrase.Bench` or `Phrase.BenchCs` files:** 22 (same exclusions, `-E 'Phrase\.Bench'`); that includes `Phrase.sln` itself, so 21 other files. 65 lines, plus 1 more line that only matches in escaped form.
- **Union:** 30 files, 88 changed lines (the 86 `sln|Bench` lines, the escaped line 22 of `tests/test_bench_launcher_scan_264.py`, and line 9 of `tests/test_phrase_executed_complete.py`). Command: apply the rules in a copy, then `git diff --cached -M --numstat`.
- **Project lines in `Phrase.sln`:** `grep '^Project(' Phrase.sln` gives 10 lines (format `"name", "path", "{GUID}"`).
  - Solution folders: src `{A1899B48-B9C9-4FC5-95F1-0381DD34D1D2}`, tests `{C2E33BC6-8208-4FA9-9C02-CF3891E5D178}`, tools `{7960200C-61F1-4C9C-9B54-E9B0CE0D1BD3}`.
  - Projects (type `{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}`):

| Name | Path | GUID |
|---|---|---|
| Avowry.Compiler | `src\Avowry.Compiler\Avowry.Compiler.csproj` | `{D4614084-825A-4803-B4D7-ED0A5E84D5D1}` |
| Avowry.Adapters.CSharp | `src\Avowry.Adapters.CSharp\...csproj` | `{1ADC752D-F7FE-49CC-87F0-C13706F83AF4}` |
| Avowry.Acceptance.Tests | `tests\Avowry.Acceptance.Tests\...csproj` | `{6B568404-09EE-49AA-96C2-6BB837A13631}` |
| Avowry.Compiler.Tests | `tests\Avowry.Compiler.Tests\...csproj` | `{F8C2ADE1-74CD-4458-92F5-A4F706EE67E4}` |
| Phrase.Bench | `tools\Phrase.Bench\Phrase.Bench.csproj` | `{16FFAB0C-B9DF-49E3-A15C-48D34A39DBF9}` |
| Phrase.BenchCs | `tools\Phrase.BenchCs\Phrase.BenchCs.csproj` | `{BB7FDE05-8976-4C15-A828-EE882665A980}` |
| Avowry.Cli | `src\Avowry.Cli\Avowry.Cli.csproj` | `{66AC61D2-A63C-4185-A821-44714DF8A1FC}` |

Size: the green diff is 54,537 bytes (`git diff --cached -M | wc -c`) and 32,981 bytes at `-U0`, in 33 paths: over 15,360 bytes, but no cut is proposed. The only cut is (a) solution, CI, docs and script versus (b) Bench. (a) alone is 10 paths and 7,232 bytes at `-U0`, but it leaves an `Avowry.sln` that lists `Phrase.Bench` and touches `BenchLauncherContract.cs` and `BenchLauncher.cs` twice. One mechanical rule set under a byte-parity test, so ship it whole.

## 2. Rewrite rules (forward, bytes)
Scope: live paths, not `board/`, `.mob/`, `docs/decisions/`, `docs/retros/`, nor `*history.jsonl` (0 hits there).

| Form | Old | New | Applies to |
|---|---|---|---|
| plain | `Phrase.sln` | `Avowry.sln` | the 9 files, plus `test_rename_196.py` lines 389, 439, 445, 448 |
| plain | `Phrase.Bench` (also covers `Phrase.BenchCs`, `.csproj`, `.dll`, the backslash path `tools\Phrase.Bench\Phrase.Bench.csproj`) | `Avowry.Bench` | the 22 files |
| escaped dot | `Phrase\.Bench` | `Avowry\.Bench` | only `tests/test_bench_launcher_scan_264.py:22`, three times, CRLF |
| script name | `check-phrase-executed-complete.py` | `check-avowry-executed-complete.py` | `tests/test_phrase_executed_complete.py:9` |
| git mv | `Phrase.sln`, `tools/Phrase.Bench/`, `tools/Phrase.BenchCs/` (each with its csproj), `scripts/check-phrase-executed-complete.py` | `Avowry.*` | 6 paths |

**EXCEPTIONS (pinned): none.** A scan with these rules leaves 0 live hits (probed). Two exceptions the product manager expected are not exceptions: `tests/test_phrase_executed_complete.py:9` loads the renamed script (leaving it breaks the import, so it is rewritten); `tests/test_rename_196.py:439` reads `Phrase.sln` as a file (leaving it breaks the test; all four lines there are rewritten). The test hard-codes `EXCEPTIONS = ()` and `PARITY_EXCEPTIONS = ()`; any hit or any parity difference fails and lists path and line.

Kept by scope, not exceptions: `<phrase-lang>`, the `phrase-card` marker, `PhraseCli` and similar identifiers, `check_phrase_executed_complete` (the module name on line 7), kebab temp names, the benchmark arm label, and `tests/test_license_files.py:31`.

Line endings (`git ls-files --eol`): CRLF in the touched set: `Support/BenchChild.cs`, `Support/BenchLauncher.cs`, `tests/test_bench_launcher_scan_264.py`. Mixed endings: `tests/test_phrase_executed_complete.py` (line 9 is LF) and the script (git mv only, no sed). Everything else in the touched set is LF. The tree has 98 CRLF files. `.gitattributes` holds only the corpus `-text` line, `LICENSE` and `MOB.md`: no edit. `core.autocrlf` is false.

## 3. Cycles
Caps: criterion 1 at most 1 cycle, criterion 2 at most 1, criterion 3 at most 1. Red commit subject `Card 197 red N (D157 Criterion N)`. Fragments build every old name, so the test file never contains an old literal. Copies go in `TemporaryDirectory`; nothing is deleted.

**RED 1 (Z, E, B)**, `tests/test_rename_197.py`, `class RenameCycle1`, command `python -m unittest tests.test_rename_197.RenameCycle1`:
- `test_old_names_gone_from_live_lines`: scan regex `phrase\\{0,2}\.(sln|bench)` plus `check-phrase-executed-complete`, case-insensitive, over tracked live bytes; must equal `EXCEPTIONS`. Fails today with 88 lines in 30 files.
- `test_new_paths_tracked_exact_case`: compares `git ls-files` strings exactly (not `os.path`, so Windows case-folding cannot hide a typo). Expects 6 new tracked paths and 0 old. Fails today.
- `test_scan_flags_injected_old_name`: the scan is not vacuous. Born green.
- `test_put_back_ci_line` (`ci.yml:62`), `test_put_back_repopaths_probe` (`RepoPaths.cs:13`), `test_put_back_run_llm_check` (`run-llm-check.py:499`): each asserts the pristine copy scans clean, puts one old name back, and asserts the scan names that path and line. Red today at the pristine precondition.
- `test_byte_parity` (B): `rename_commit()` runs `git log --diff-filter=A --format=%H -- Avowry.sln` (works under pathspec: probed in a scratch repo); it asserts with the message "no commit adds Avowry.sln", so it is red today for the right reason. After the green: P is `C^`; every path in P's tree maps through the rules to an expected path; the set of expected paths must equal C's tree exactly (any extra or compat file fails; `.mob/` and `board/` skipped); bytes are read with `git cat-file --batch`; the rules are applied at byte level; old bytes are compared with C's blob file by file; every differing path is listed with old, expected and committed sizes. `PARITY_EXCEPTIONS = ()`.
- The green attaches its own run of `test_byte_parity` (full output, not a count) before claiming clean (D202).

**Cycle 2 (O, M, I), born green**, `class ListsAndPaths`:
- `test_solution_projects`: the 10 Project lines equal `baseline/project-lines.txt` after the rules (names, paths, GUIDs, pairing); every csproj path is tracked.
- `test_lists_equal_baseline`: `dotnet test <proj> --list-tests`, normalised and sorted, equals the baseline name for name.
- `test_bench_self_check`: the `bench/README.md` self-check command exits 0 and `Avowry.Bench.dll` exists.
- `test_five_modules`: counts 6, 17, 21, 36, 4, all OK (probed at HEAD in a `git archive` copy).
- `test_launcher_scan_not_vacuous`: `TARGETS_BENCH` matches at least 2 files in `ACCEPTANCE` outside the launcher and contract (probed today: 3 files including the contract).
- `test_triple_agree`: ADR line 97, `check-library-binding-layering.py:71-72` and `test_library_binding_layering.py:18-19` name the same paths, and those paths are tracked.
- `test_run_llm_check_path`, `test_probe_matches_solution`: the `RepoPaths` probe string equals the single tracked root `.sln`, and so does the fake layout in `BenchLauncherContract.cs:199`.
- Put-backs: P2a `RepoPaths.cs:13`; P2b the escaped regex at `scan_264:22` (only the non-vacuity test goes red; the module stays at 6 OK, which is the vacuity); P2c `run-llm-check.py:499`; P2d the ADR names `tools/Avowry.Bench/Program.cs` where the script says `BenchCs` (the scan stays green, only the triple test is red).

**Cycle 3 (S, E), born green**, `class OldCommandRefused`:
- `test_old_command_fails`: in a copy, `dotnet test Phrase.sln` exits non-zero and says "does not exist" (wording unprobed: QA probes and pins it).
- `test_no_compat_files`: no tracked `Phrase.*` path, exactly one root `.sln`.
- `test_guid_pairing_all_seven`.
- `test_commands_name_tracked_solution`: all four `ci.yml` lines (62, 65, 70, 84), the README, HANDOFF and agreement gate commands.
- Put-backs: P3a a CR strip in `test_bench_launcher_scan_264.py` (a history mutation in a scratch clone with `commit --amend`; parity names it); P3b swap the `Avowry.Bench` and `Avowry.BenchCs` GUIDs (parity and the GUID test both go red); P3c `ci.yml:70`; P3d a compat `Phrase.sln` added (`test_old_command_fails` and `test_no_compat_files` go red).
- Real-defect evidence (to be committed as the defective-green diff against the green and applied clean): E1 plain `sed -i` without `-b`: probed here, GNU sed 4.9 under git-bash strips CR on 4 files (`BenchLauncher.cs` 117 lines, `test_phrase_executed_complete.py` 90, `test_bench_launcher_scan_264.py` 81, `BenchChild.cs` 75); parity must list exactly those 4. E2 the escaped rule typed inline: this tool layer halves backslashes; a one-line `sed` produced `Avowry.Bench\.csproj` at `scan_264:22`, dropping the backslash; parity names that line.
- All patches live under `.mob/records/197/mutations/` and each must pass `git apply --check` on a fresh `git archive` of the green commit.

## 4. Baseline, committed by red 1 under `.mob/records/197/baseline/`
- `compiler-tests.list.txt` and `acceptance-tests.list.txt`: from `dotnet test tests/Avowry.<X>.Tests --list-tests` (note: the test projects are already renamed by card 196), 4-space lines kept, sorted, duplicates kept, the 14 `source: "C:\\..."` rows normalised. Expected counts 284 and 317.
- `nonpending-counts.txt`: the `--no-build --list-tests --filter "Status!=Pending"` counts (284 and 316 recorded on card 196).
- `gate-run-before.txt`: summary lines of `dotnet test Phrase.sln --filter "Status!=Pending"`: 284 and 313 passed with 3 known failures by name. The facilitator runs it in a clean archive copy with nothing else running (about 11 minutes).
- `python-counts.txt`: 6, 17, 21, 36, 4 (probed, all OK), plus `test_rename_197` after its own red.
- `project-lines.txt`: the 10 Project lines (section 1). `eol.txt`: `git ls-files --eol`.

## 5. Green recipe
Work in a fresh worktree (a directory `git mv` carries ignored `bin/obj`).
1. `git mv Phrase.sln Avowry.sln`
2. `git mv tools/Phrase.Bench tools/Avowry.Bench`, then `git mv tools/Avowry.Bench/Phrase.Bench.csproj tools/Avowry.Bench/Avowry.Bench.csproj`.
3. The same for `Phrase.BenchCs`.
4. `git mv scripts/check-phrase-executed-complete.py scripts/check-avowry-executed-complete.py`
5. The sed script file `r197.sed` has exactly these four lines. Write it with a file tool, never inline:
```
s/Phrase\.sln/Avowry.sln/g
s/Phrase\.Bench/Avowry.Bench/g
s/Phrase\\\.Bench/Avowry\\\.Bench/g
s/check-phrase-executed-complete\.py/check-avowry-executed-complete.py/g
```
6. Run it over the list from `git grep -I -l -E 'Phrase\.(sln|Bench)|check-phrase-executed-complete\.py' -- . ':!board' ':!.mob' ':!docs/decisions' ':!docs/retros' ':!*history.jsonl'` (30 paths, after the moves): `xargs -a list sed -b -i -f r197.sed`.
7. Stage explicit paths only. Delete nothing, recursive or not. Leave old `bin/obj`. Keep line endings.
8. Probed result of steps 1 to 7 in a copy: 0 residual, 98 CRLF files unchanged, 88 lines changed with equal insertions and deletions, `Phrase.sln` as R094.

Commit message: subject `Card 197 green 1 (D157 Criterion 1): ...`, a `Criterion: 1` line, a `Slice-exception:` trailer naming the rewrite, NO Co-Authored-By line (`.githooks/commit-msg` rejects it).

## 6. Gate brief and sign-off (D202)
- Isolated: a clean `git archive <green sha>`; no push, no second gate, no seat build at the same time; each full run gets a Load line.
- Reports show both Avowry test dlls AND both Bench dlls (`Avowry.Bench.dll`, `Avowry.BenchCs.dll`), each with totals.
- Exact-case tracked-path check for Linux: `git ls-files` equals the expected 6 paths.
- Commit order: red 1, green, red 2, red 3. The green's own parity output is attached.
- One push after the gate; then `python scripts/check-ci-main.py --wait` for `test`, `usable-v1` and `mob-process` on the head SHA.
- The facilitator records a `local-windows-run.md` as in card 196 (the green sha, `dotnet --version`, a lists diff, the five module results, totals against the baseline), the CI run URL and the 217 extractor outcome count, the card 304 caveat (a lone timeout-test failure is rerun alone and filed under 304), and the board follow-up (70 open issues).

## 7. Card text, after the rerun
- [M] "Compiler 284, Acceptance 317 listed; 600 non-pending" is unprobed by the architect (it needs a build).
- The task force names 3 CRLF files; there are 3 CRLF plus a mixed-ending test file (`test_phrase_executed_complete.py`) that gets a sed edit.
- The card says about 107 lines; measured: 88 changed lines in 30 files.
- The expected exceptions are not exceptions (section 2).
- The card does not say that plain `sed -i` on this machine strips CRs, nor that a shell command strips backslashes: both are covered above.
- The "does not exist" wording for the old command is unprobed.

## Facilitator's note
The slice amends nothing in the card text (SHIP AS IS). Its gaps become the red and gate briefs: the pinned exceptions list is empty and hard-coded in the test (the D203 audit-lens condition); the put-back proofs, Load lines and the CI run URL are committed under `.mob/records/197/`; the baseline gate run is the facilitator's, alone on the machine.
