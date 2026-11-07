# Card 196 red 2 (Criterion 2: lists, strings, line endings), QA seat

Born green. The five tests of `class ListsAndStrings` in `tests/test_rename_196.py` pass on 1dece86 (the finished rename); cycle 1's tests are untouched (only the module docstring line for this class was updated). `python -m unittest tests.test_rename_196` on 1dece86 content: 10 tests OK, about 23 s.

## The tests
- `test_lists_equal`: runs `dotnet test tests/<project> --list-tests`, keeps the 4-space rows, rewrites the 14 rows that embed the checkout path (`(source: "C:\\...\Av"...)`) to `(source: <path>)`, sorts (duplicates kept), and compares to the committed baseline list after swapping the old project prefix for the new one. Counts: 284 (Compiler) and 317 (Acceptance), asserted on both the baseline and the live list before the content compare. Failure message gives expected/listed counts and the missing/extra multiset rows.
- `test_starvation_probe_prefix`: the one `StartsWith("...Acceptance...")` literal in `tests/Fixtures/StarvationProbe/Program.cs` plus `.Tests` must equal the stem of the single `tests/Avowry.Acceptance.Tests/*.csproj`. (The literal is `Avowry.Acceptance`, the stem is `Avowry.Acceptance.Tests`, so the compare is `prefix + ".Tests" == stem`, not literal equality as the brief worded it.) Failure names `Program.cs:31`.
- `test_corpus_text_unset`: `git check-attr text` over the 14 tracked `corpus/*.avowry` says `unset` for each.
- `test_corpus_renames_r100`: C = first commit adding `tests/Avowry.Acceptance.Tests/Fixtures/orders.avowry` (`git log --diff-filter=A --reverse`; that is 1dece86). The pathless `git diff -M --name-status C^ C` must show `R100` for each of the 14 corpus fixtures, with the old path being the old project directory.
- `test_eol_unchanged`: `git ls-files --eol` for both projects, paths mapped back to the old names, compared by path and the `i/` and `w/` columns with the committed `compiler-eol.txt` and `acceptance-eol.txt`.

## Mutation evidence
All mutations were made in scratch copies (`git archive HEAD` into a TemporaryDirectory, `git init` + one commit so git-based tests have a repo). HEAD does not hold the new test file yet, so the working-tree test file was copied over the archive. Each patch was generated with `git diff --cached --binary`, stored under `.mob/records/196/mutations/`, then re-applied with `git apply --check` plus `git apply` on a second fresh archive copy and the test run there. Harness script: scratchpad `mutate196.py` (P2a to P2d) and `mutate196e.py` (P2e and the real defect); neither is committed.

| Patch | Change | Test that fails | Failure |
|---|---|---|---|
| P2a | delete `corpus/empty-record.avowry` and `.expected.json` | `test_lists_equal` (Acceptance) | `317 != 316 : expected 317 rows, listed 316; 1 missing, 0 extra`; the missing row is the normalised `source: <path>` row, so the distinct set is unchanged |
| P2b | `ExtensionBoundaryTests.cs` namespace back to the old Compiler name | `test_lists_equal` (Compiler) | count passes (284 = 284), content fails: `4 missing, 4 extra`, the 4 `ExtensionBoundaryTests` rows now start with the old name |
| P2c | `StartsWith("Avowry.Acceptance"` back to the old name | `test_starvation_probe_prefix` | `tests/Fixtures/StarvationProbe/Program.cs:31 probes StartsWith("<old>.Acceptance") but the project is Avowry.Acceptance.Tests` |
| P2d | `.gitattributes` line 1 path back to the old project | `test_corpus_text_unset` | `14 of 14 corpus fixtures are not 'text: unset', they say ['unspecified']` |
| P2e | strip CR from `corpus/crlf-source.avowry` (442 to 417 bytes), amend the rename commit in a scratch clone | `test_corpus_renames_r100` and `test_eol_unchanged` | diff shows `R094` for crlf-source; eol: `baseline ('i/crlf','w/crlf') now ('i/lf','w/lf')` |

P2c vacuity check (from the architect's slice): in the P2c copy, `dotnet test tests/Avowry.Acceptance.Tests --filter "FullyQualifiedName~Starvation"` passed 10 of 10 (exit 0). So the dotnet tests do not notice the missed rename; only the python test does.

P2e exact commands (a patch for the CR strip does apply: `git apply --check` and `git apply` both exit 0 and keep the 25 CRs in the patch, result 417 bytes; but the test needs history, so the patch is applied in a clone):
1. `git clone --no-hardlinks <main> <scratch>`, `git checkout 1dece86`, copy the new test file in.
2. Strip: bytes `\r\n` to `\n` in `tests/Avowry.Acceptance.Tests/Fixtures/corpus/crlf-source.avowry`; `git diff -- <that path>` is `P2e.patch` (1244 bytes, 25 CR bytes).
3. In a second fresh clone at 1dece86: `git apply P2e.patch`, `git add <path>`, `git commit --amend --no-edit` (so C is the stripped version), then run the four tests. `test_corpus_text_unset` and `test_starvation_probe_prefix` stay OK there, as expected.

## Real defect run (9b941dd, the seat's original green)
`git clone --no-hardlinks`, `git checkout 9b941dd`, new test file copied in, whole class run: 5 tests, 2 fail.
- `test_corpus_renames_r100` FAIL: crlf-source is R094.
- `test_eol_unchanged` FAIL (Acceptance project): 8 files read `i/lf w/lf` where the baseline says `i/crlf w/crlf`: crlf-source.avowry and the seven CRLF `.cs` files (GeneratedApiBoundAddress, GeneratedApiBoundAddressEdges, GeneratedApiHazards, HoldoutTaskConversion, Support/BenchChild, Support/BenchLauncher, Support/HarnessProbes).
- `test_lists_equal`, `test_starvation_probe_prefix`, `test_corpus_text_unset` pass there, as predicted.

## Surprising or worth knowing
- Not caught by this class on the real defect: the swapped project GUIDs in `Phrase.sln`, the 2 bytes lost in the Compiler csproj (902 to 900, both LF files, so `--eol` cannot see it), and the stripped CRs in `tests/test_bench_launcher_scan_264.py` (outside the two project directories). The GUID swap belongs to cycle 3 (the `Phrase.sln` test); the csproj byte loss and the bench-launcher py CRs have no named test in the slice. Cycle 3 or the gate should cover them (for example a byte-equality check of the csproj against the old bytes with the one rewrite).
- C for `corpus_renames_r100` is 1dece86 because without `--follow` the log reports the rename target as the add. That is the intended commit (the rename commit); the test would also follow a future rewrite of that commit.
- The dotnet list calls dominate the runtime (about 8 s per class run on the main checkout; about 15 s in a fresh scratch copy that builds first).
- The harness overlay: scratch copies are `git archive HEAD` plus the working-tree test file, because the test file is not at HEAD until this commit.
