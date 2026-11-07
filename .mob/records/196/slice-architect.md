# Card 196 slice (architect seat, read-only; committed with the facilitator's note at the end)

Everything was run in a `git archive 49f3069` copy, so no repository file was touched.

## 1. Scope: SHIP AS IS
One concern, three criteria. Caps: criterion 1 at most 1 cycle, criterion 2 at most 1, criterion 3 at most 1 (sum 3).

Premises rerun at 49f3069:
- `git grep -c -E "Phrase\.(Compiler|Acceptance)\.Tests" -- . ':!board' ':!.mob' ':!tests/Phrase.Compiler.Tests' ':!tests/Phrase.Acceptance.Tests'` gives 20 files and 49 lines. Four are history (`docs/decisions` x3, `docs/retros/seeds.md`), so 16 are live.
- The same pattern inside the two projects gives 96 files and 203 lines. `git ls-files` lists 127 files there (100 + 27).
- `git grep -n -E "Phrase\.Bench" -- tests/Phrase.Acceptance.Tests tests/Phrase.Compiler.Tests | wc -l` gives 40. The two-name pattern cannot match any of them. The 5 `Phrase.GetProperty` hits are all in `PrivateHoldoutResults.cs:108-112`.
- `dotnet test <proj> --list-tests`: Compiler 284, Acceptance 317 (304 distinct).

## 2. Cycles
The red commit is `Card 196 red N (D157 Criterion N)`. All tests live in `tests/test_rename_196.py` and run with `python -m unittest tests.test_rename_196`.
- Build the old-name pattern from fragments so the file never contains the literal.
- `scan(root)` walks the tracked file list, skips `board/`, `.mob/`, `docs/decisions/`, `docs/retros/`, and returns `path:line`.
- Put-back tests copy the tree into a `TemporaryDirectory`. The pre-commit hook rejects `rmtree` of a variable path, so use no `rmtree`.
- Each put-back first asserts that the pristine copy scans clean. That assertion is the right-reason red today.

**Cycle 1 (Z, E).**
- RED: `test_old_names_gone_from_tracked_live_files` fails with about 112 files listed. `test_new_dirs_tracked_old_gone` fails: 0 tracked under `tests/Avowry.*`, expected 27 and 100. `test_put_back_script` (`scripts/ci-log-outcomes-217.py:36`), `test_put_back_namespace` (`Support/BenchLauncher.cs`) and `test_put_back_gitattributes` (`.gitattributes:1`) fail at the pristine-clean precondition.
- GREEN: the one rename commit.

**Cycle 2 (O, M, B), born green.** `class ListsAndStrings`:
- Lists equal name for name, as a sorted multiset. Counts must be 284 and 317.
- The StarvationProbe prefix equals the stem of `tests/Avowry.Acceptance.Tests/*.csproj`.
- `git check-attr text` reports `unset` on all 14 corpus `*.avowry`.
- Every corpus path is `R100` from `C^` to `C`, where `C = git log --diff-filter=A -1 --reverse --format=%H -- tests/Avowry.Acceptance.Tests/Fixtures/orders.avowry`.
- The per-file `git ls-files --eol` class is unchanged.

Named put-backs, each shown red once on the green:
- **P2a:** delete one corpus pair (`Fixtures/corpus/empty-record.avowry` and its `.expected.json`): 316 listed rows with the distinct set unchanged, so only the multiset compare goes red.
- **P2b:** put one Compiler file's namespace back: the count is equal and the names differ.
- **P2c:** put back `StartsWith("Phrase.Acceptance"` at `tests/Fixtures/StarvationProbe/Program.cs:31`: the python test must go red naming that line. Also record that `dotnet test --filter StarvationProbe` stays green in that copy, which is the vacuity (unprobed).
- **P2d:** put back `.gitattributes:1`. Probed: `check-attr` then says `unspecified`.
- **P2e:** in a scratch clone, commit a CR-stripped `crlf-source.avowry`, so the rename shows below R100.

**Cycle 3 (I, S), born green.** `class PathBound`:
- The five modules run by subprocess give 6, 17, 21, 36, 4 with OK.
- Non-vacuity: the `ACCEPTANCE` constant in `test_bench_launcher_scan_264` is a directory with at least 90 `.cs` files.
- A synthetic `Avowry.Acceptance.Tests.*` log through the 217 extractor returns a non-empty outcome list.
- `Phrase.sln` names both new projects, with GUIDs `{6B568404-09EE-49AA-96C2-6BB837A13631}` (Acceptance) and `{F8C2ADE1-74CD-4458-92F5-A4F706EE67E4}` (Compiler), and both csproj paths exist.
- The Compiler csproj link target directory exists.

Named put-backs:
- **P3a:** old `PROJECT` in `ci-log-outcomes-217.py`: the extractor returns an empty list.
- **P3b:** old Compiler path in `Phrase.sln`.
- **P3c:** old link on `Phrase.Compiler.Tests.csproj:18`. Probed by moving `Fixtures` away: `ParserFixtureTests`, `LexerFixtureTests` and others fail.
- **P3d:** old `ACCEPTANCE` in `test_bench_launcher_scan_264.py`: the module stays green at 6 OK, which is vacuous, so only the non-vacuity assertion goes red.
- **P3e:** old `FLOOR` in `check-run-pairs-217.py`.

## 3. Green recipe
Work in a fresh worktree, since a directory `git mv` carries ignored `bin/obj` with it. If it must run in a checkout that has `bin/obj`, move per file instead: `git ls-files -z tests/Phrase.X.Tests | xargs -0 ...git mv`.
1. `git mv tests/Phrase.Compiler.Tests tests/Avowry.Compiler.Tests`
2. `git mv tests/Phrase.Acceptance.Tests tests/Avowry.Acceptance.Tests`
3. `git mv` each csproj file to its new name (`Avowry.Compiler.Tests.csproj`, `Avowry.Acceptance.Tests.csproj`).
4. Run `sed -i -E 's/Phrase\.(Compiler|Acceptance)\.Tests/Avowry.\1.Tests/g'` over `git grep -l -E 'Phrase\.(Compiler|Acceptance)\.Tests' -- . ':!board' ':!.mob' ':!docs/decisions' ':!docs/retros'` after the move (112 files, including `Phrase.sln` and `HANDOFF.md:33`).
   - It never touches corpus fixtures (no old names), and never uses a bare `Phrase.` pattern, so `Phrase.GetProperty`, `Phrase.Bench` and `Phrase.BenchCs` cannot match.
   - Use GNU sed, not python text mode, so CR bytes survive.
5. Make one explicit edit on `StarvationProbe/Program.cs:31` to `StartsWith("Avowry.Acceptance"`. The sed misses it.
6. Stage explicit paths only (`git add` each file from the lists). Delete nothing. Leave the old `bin/obj` alone.

Line endings:
- CRLF `.cs` files that get the sed: `GeneratedApiBoundAddress`, `GeneratedApiBoundAddressEdges`, `GeneratedApiHazards`, `HoldoutTaskConversion`, `Support/BenchChild`, `Support/BenchLauncher`, `Support/HarnessProbes`.
- CRLF file outside the projects: `tests/test_bench_launcher_scan_264.py`.
- `-text` corpus fixtures (14; `crlf-source.avowry` is CRLF): pure `git mv`, no sed.
- Everything else is LF. `core.autocrlf` is false.

Commit message: subject `Card 196 green 1 (D157 Criterion 1): ...`, then the trailer `Slice-exception: rewrite s/Phrase.Compiler.Tests/Avowry.Compiler.Tests/ (and Acceptance)`. Add no `Co-Authored-By` line: `.githooks/commit-msg` rejects it.

**Frozen-sample rule.** Update any string that a script or test resolves or filters against the live tree or live CI log, any sample that must look like live output, and any sentence about live paths. Freeze only data that a test compares byte for byte with a committed `.mob/records` file.
- No case found that must stay frozen. Neither `check-run-pairs-217.py` nor `check-repeat-evidence.py` filters on a prefix. The real 217 and 264 records keep their old names as history.
- Update all of these: `ACC` and the `C` list in `test_outcome_extractors_217.py:22,76`; `T162`, `T303A`, `T303B`, `PASSING` and the `Fake` names in `test_run_pairs_217.py:48-56,161-191`; `ROW` and the commands in `test_repeat_evidence.py:42-60`; `repeat-run-264.py:25,110`.
- If an update turns a test red because it reads an old-name record, the green stops and asks. It does not add an allowlist to the Z scan.

## 4. Baseline (red commit 1, under `.mob/records/196/baseline/`)
- **Sorted lists:** `compiler-tests.list.txt` and `acceptance-tests.list.txt`, from `dotnet test tests/Phrase.<X>.Tests --list-tests`, keeping 4-space lines, sorted, duplicates kept.
  - Normalise the 14 rows `source: "C:\\...\1f"...)` to `source: <path>)`: they embed the checkout path, so a raw compare fails in a clean copy. The 13 duplicate names are exactly these rows (no other `uniq -d` hit).
  - Verified counts: 284 and 317 (`grep -c`).
- **Non-pending listed:** `--no-build --list-tests --filter "Status!=Pending"` gives 284 and 316 (one Pending test, `UsableV1.cs:101`). Expected run total 600, unprobed.
- **Run total:** `gate-run-before.txt`, the summary lines of `dotnet test Phrase.sln --filter "Status!=Pending"` in the real checkout, with HEAD, date and the failing-test names. The full run is unprobed; the 217 notes say about 11 minutes and 3 known failures.
- **Python counts:** the five modules (6, 17, 21, 36, 4, all OK in the copy), plus `test_demo_readme` from the real checkout (the copy printed a bar-5 REGRESSION, so it is not baselined).
- **Also commit:** the two GUIDs and `git ls-files --eol` for both projects.

## 5. Gate brief and Windows evidence
The non-author runs, in an empty directory: `git archive <green sha>`, then the full gate. They check:
- Both `Avowry.Compiler.Tests.dll` and `Avowry.Acceptance.Tests.dll` appear in the run, each with its own totals.
- `git log --reverse --format='%h %s'` from the red-1 parent shows the order red 1, green, red 2, red 3, one criterion per commit.
- Anchoring: undoing `Avowry.(Compiler|Acceptance).Tests` to `Phrase...` on every changed line of the green reproduces the pre-image, except the one StarvationProbe line.
- `git diff --diff-filter=D C^ C` is empty, and there are 127 renames.

The facilitator records `.mob/records/196/local-windows-run.md`: the green sha and a clean `git status`; OS and `dotnet --version`; the gate command, its per-project summary lines, and the diff of totals and failure names against the baseline; the empty list diff and the five python results; the Linux CI run URL with its extractor outcome count.

## 6. Card gaps
- "20 outside files" is 16 live and 4 history.
- `usable-v1-report.py:147` is run by CI at `ci.yml:87`, so a missed path would fail CI after the push. This corrects the product manager seat's claim that no workflow depends on the paths.
- `test_bench_launcher_scan_264` and the 217 `FLOOR` check pass vacuously on a missed path. The scenarios do not say so; the cycle 3 non-vacuity assertion covers it.
- The StarvationProbe prefix is not matched by the card's two-name rewrite, and the Z search cannot see it. Only [B] catches it.
- The `Status!=Pending` total is 600 listed. `README.md:41` is stale ("83 pending") but out of scope.
- The unowned `Phrase.Bench` names were raised on card 197 (a D201 condition; done by the facilitator).

## Facilitator's note
The slice amends nothing in the card text (SHIP AS IS); its gaps are carried into the red and gate briefs. The 600 expected run total and the 11-minute run are unprobed and recorded by the red.
