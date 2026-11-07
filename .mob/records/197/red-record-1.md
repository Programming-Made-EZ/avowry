# Card 197 red 1 record (QA seat, Criterion 1: Z, E, B)

Committed paths: `tests/test_rename_197.py`, `.mob/records/197/baseline/` (new files), this file. Written at HEAD 860d552.

## Command and result today
`python -m unittest tests.test_rename_197.RenameCycle1` -> `Ran 7 tests`, `FAILED (failures=6)`.

| Test | Today | Why (the right reason) |
|---|---|---|
| test_old_names_gone_from_live_lines | FAIL | 88 lines in 30 files name an old name (list below) |
| test_new_paths_tracked_exact_case | FAIL | the six new paths are not tracked; the six old paths still are |
| test_put_back_ci_line | FAIL | precondition: pristine ci.yml scans dirty, lines 62, 65, 70, 84 |
| test_put_back_repopaths_probe | FAIL | precondition: RepoPaths.cs lines 13 and 16 |
| test_put_back_run_llm_check | FAIL | precondition: scripts/run-llm-check.py line 499 |
| test_byte_parity | FAIL | `no commit adds Avowry.sln` |
| test_scan_flags_injected_old_name | pass (born green) | seven injected forms (plain, backslash path, escaped dot with one and two backslashes, script name, upper and lower case) are all flagged; `.mob/` and `history.jsonl` copies are skipped; clean lines are not flagged |

## Design notes
- `EXCEPTIONS = ()` and `PARITY_EXCEPTIONS = ()` are hard-coded. Old names are built from fragments; the file scans clean against itself.
- Scan: bytes, per line, regex `Phrase\{0,2}\.(sln|bench)` plus `check-phrase-executed-complete`, case-insensitive, live paths only (not board/, .mob/, docs/decisions/, docs/retros/, *history.jsonl).
- Put-back tests copy only the one target file into a TemporaryDirectory. RepoPaths.cs is found under tests/ by name; ci.yml:62 and run-llm-check.py:499 are pinned by line number.
- Parity: C = earliest commit adding Avowry.sln; P = C^; every path of P (not .mob/, board/, tests/test_rename_197.py) is mapped by the git mv map; the expected path set must equal C's tree; blobs are read by id through `git cat-file --batch`; live files get the four byte rules, non-live files (docs/decisions, docs/retros, history.jsonl) must be byte-identical. Every difference is listed with path and old, expected and committed sizes. One addition beyond the brief: a file-mode comparison (an executable bit flip is listed too).
- tests/test_rename_196.py is rewritten by the rules (its four old-name lines), as the slice says.

## Self-check of the green side (not committed)
In a scratch git repo (`git archive HEAD` plus this test file, committed as P), I applied the git mv map and the four byte rules with Python and committed C. All seven tests passed there (`Ran 7 tests ... OK`), the parity function returned no difference, and the scan found 0 lines. Stripping the CRs of `BenchLauncher.cs` in C was then reported as `tests/Avowry.Acceptance.Tests/Support/BenchLauncher.cs (old 6087 bytes, expected 6087, committed 5970)`. Script: the session scratchpad `sim197_red1.py`.

## Baseline (`.mob/records/197/baseline/`)
- project-lines.txt: the 10 `^Project(` lines of Phrase.sln, exact.
- eol.txt: `git ls-files --eol` (1515 lines).
- python-counts.txt: 6, 17, 21, 36, 4 OK in the real checkout; test_rename_196 (15) passes now.
- compiler-tests.list.txt: 284 rows. acceptance-tests.list.txt: 317 rows, of which 14 are the normalised `source: <path>)` rows (2 other rows hold `"C:\x\tests\..."`, test arguments, not checkout paths). Counted with grep -c.
- nonpending-counts.txt: 284 and 316 (`--no-build --list-tests --filter "Status!=Pending"`, 4-space rows counted).
- gate-run-before.txt is the facilitator's.

## Anything unexpected
- Load: I ran `python -m unittest tests.test_rename_196` for python-counts.txt at about 08:29 to 08:30 PST. That module's ListsAndStrings test runs `dotnet test --list-tests` (56 s run), so dotnet ran once, for under a minute, while the facilitator's baseline gate (started 08:27:07) was still running. The wait rule was meant to prevent that; the gate's Load line should name it. All other dotnet commands ran after gate197-before.done existed.
- `gate197-before.out` (read, not changed) showed `PrivateHoldoutResults` failing with "not a git repository" in the archive copy, which has no .git.
- The brief's 88 lines in 30 files is confirmed.

## Lines the scan finds today (88)
.github/workflows/ci.yml:62
.github/workflows/ci.yml:65
.github/workflows/ci.yml:70
.github/workflows/ci.yml:84
HANDOFF.md:117
Phrase.sln:20
Phrase.sln:22
README.md:28
README.md:29
TEAM_WORKING_AGREEMENT.md:526
bench/README.md:16
bench/csharp-baseline/README.md:5
docs/adr/ADR-library-binding-layering.md:97
scripts/check-library-binding-layering.py:71
scripts/check-library-binding-layering.py:72
scripts/ci-log-outcomes-217.py:6
scripts/repeat-run-264.py:5
scripts/run-llm-check.py:499
tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj:16
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:5
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:7
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:13
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:19
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:20
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:21
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:22
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:23
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:59
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:111
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:112
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:124
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:141
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:142
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:195
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:199
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:200
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:207
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:215
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:218
tests/Avowry.Acceptance.Tests/BenchLauncherContract.cs:222
tests/Avowry.Acceptance.Tests/BenchRunHarness.cs:6
tests/Avowry.Acceptance.Tests/BenchmarkHarnessAcceptance.cs:29
tests/Avowry.Acceptance.Tests/BenchmarkHarnessAcceptance.cs:30
tests/Avowry.Acceptance.Tests/BenchmarkHarnessAcceptance.cs:77
tests/Avowry.Acceptance.Tests/CSharpBaselineAcceptance.cs:13
tests/Avowry.Acceptance.Tests/CSharpBaselineAcceptance.cs:14
tests/Avowry.Acceptance.Tests/CSharpBaselineAcceptance.cs:486
tests/Avowry.Acceptance.Tests/CSharpBaselineAcceptance.cs:911
tests/Avowry.Acceptance.Tests/CSharpBaselineAcceptance.cs:927
tests/Avowry.Acceptance.Tests/PhraseCheatSheet.cs:190
tests/Avowry.Acceptance.Tests/PhraseExecutedOutcomesAcceptance.cs:96
tests/Avowry.Acceptance.Tests/PhraseExecutedOutcomesAcceptance.cs:97
tests/Avowry.Acceptance.Tests/PrivateHoldoutResults.cs:35
tests/Avowry.Acceptance.Tests/PrivateHoldoutResults.cs:36
tests/Avowry.Acceptance.Tests/PrivateHoldoutResults.cs:53
tests/Avowry.Acceptance.Tests/PrivateHoldoutResults.cs:54
tests/Avowry.Acceptance.Tests/PrivateHoldoutResults.cs:131
tests/Avowry.Acceptance.Tests/Support/BenchChild.cs:6
tests/Avowry.Acceptance.Tests/Support/BenchLauncher.cs:6
tests/Avowry.Acceptance.Tests/Support/BenchLauncher.cs:8
tests/Avowry.Acceptance.Tests/Support/BenchLauncher.cs:10
tests/Avowry.Acceptance.Tests/Support/BenchLauncher.cs:14
tests/Avowry.Acceptance.Tests/Support/BenchLauncher.cs:42
tests/Avowry.Acceptance.Tests/Support/BenchLauncher.cs:87
tests/Avowry.Acceptance.Tests/Support/RepoPaths.cs:13
tests/Avowry.Acceptance.Tests/Support/RepoPaths.cs:16
tests/test_bench_launcher_scan_264.py:1
tests/test_bench_launcher_scan_264.py:4
tests/test_bench_launcher_scan_264.py:5
tests/test_bench_launcher_scan_264.py:6
tests/test_bench_launcher_scan_264.py:22
tests/test_bench_launcher_scan_264.py:26
tests/test_bench_launcher_scan_264.py:55
tests/test_bench_launcher_scan_264.py:59
tests/test_bench_launcher_scan_264.py:64
tests/test_bench_launcher_scan_264.py:68
tests/test_bench_launcher_scan_264.py:77
tests/test_findings_264.py:72
tests/test_library_binding_layering.py:18
tests/test_library_binding_layering.py:19
tests/test_phrase_executed_complete.py:9
tests/test_rename_196.py:389
tests/test_rename_196.py:439
tests/test_rename_196.py:445
tests/test_rename_196.py:448
tools/Phrase.Bench/Program.cs:25
tools/Phrase.BenchCs/Program.cs:15
tools/Phrase.BenchCs/Program.cs:272
