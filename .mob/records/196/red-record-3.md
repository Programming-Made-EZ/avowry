# Card 196 red 3 (Criterion 3: path-bound scripts, the solution, a byte-exact anchored rewrite), QA seat

Not fully born green: 2 of the 5 tests of `class PathBound` in `tests/test_rename_196.py` are RED on HEAD 1dece86, and that is a real defect in the green, not a test fault. Cycle 1 and 2 classes are untouched (only the module docstring line for PathBound changed).

## The real defect found (facilitator must act)
`scripts/ci-log-outcomes-217.py:65`, `project_of`, still reads `re.match(r"^(Phrase\.[A-Za-z0-9_]+\.Tests)\.", name)`. The anchored sed (literal `Phrase.(Compiler|Acceptance).Tests`) cannot match this regex form (escaped dots, a character class), and the Z scan cannot see it. So `project_of("Avowry.Acceptance.Tests.X")` returns `Avowry`, every Acceptance line is mis-grouped, and the extractor rejects any real log with `empty no Avowry.Acceptance.Tests result lines`. `python -m unittest tests.test_outcome_extractors_217` on HEAD: 17 tests, failures=5, errors=14 (baseline said 17 OK). Real CI would break at the extractor step. Fix, one line: `Phrase\.` to `Avowry\.` in that regex (stored as `mutations/P3fix.patch`, applied only in scratch copies, NOT applied to main). Red on HEAD: `test_five_modules` (names the 217 module with its tail) and `test_extractor_not_empty`.

## The tests (HEAD 1dece86; durations from the run on HEAD, whole class about 13 to 17 s)
- `test_five_modules`: subprocess `python -m unittest` for the five modules, expects 6, 17, 21, 36, 4 and OK; failure lists per-module counts and the stderr tail. RED on HEAD (217 module only). About 12 s.
- `test_launcher_scan_not_vacuous`: `ACCEPTANCE` in the 264 module is a directory with at least 60 `.cs` files (bin/obj excluded). The brief said 90: that was a miscount, the project has 69 tracked `.cs` files (100 is all tracked files), so I used 60. Green.
- `test_extractor_not_empty`: synthetic log of three `Avowry.Acceptance.Tests.Fake.*` lines with a summary block through `extract_text`; expects the three outcomes. RED on HEAD (Rejected: empty).
- `test_solution_guids`: `Phrase.sln` Project lines for both new projects have the new csproj paths and the baseline GUID pairing from `baseline/project-guids.txt`; both csproj exist; the Compiler csproj `Fixtures` link is exactly `../Avowry.Acceptance.Tests/Fixtures` and that directory exists. Green.
- `test_rewrite_is_byte_exact_and_anchored`: R = first commit adding the Compiler csproj (1dece86). For all 144 files in `git diff -M --name-status -z R^ R` (R and M; `tests/test_rename_196.py` and `.mob/` exempt), old bytes from R^ via `git cat-file --batch`, byte-level anchored rewrite plus the one StarvationProbe edit, compared with the bytes at R; every differing path is listed. Also, a file the rewrite leaves alone must be R100. Green. About 1 s.

## Mutation evidence
Harness: scratchpad `mutate196c3.py` (not committed). Mode W: fresh `git clone --no-hardlinks` at HEAD, `P3fix.patch` then the mutation applied (each `git apply --check` clean), class run. Mode H: clone at 1dece86, patch applied, `git commit --amend --no-edit`, class run; "changed" = versus the unmutated 1dece86 clone (which already has the two real-defect failures). Base HEAD + P3fix: all 5 pass.

| Patch | Change | Mode W failures | Mode H extra failures |
|---|---|---|---|
| P3a | `PROJECT` back to the old name in ci-log-outcomes-217.py | `extractor_not_empty` (Rejected empty), `five_modules` | `rewrite_is_byte_exact` |
| P3b | swap the two GUIDs in Phrase.sln | `solution_guids` | `rewrite_is_byte_exact` and `solution_guids` |
| P3c | old fixture link on the Compiler csproj line 18 | `solution_guids` only (the dotnet fixture tests were not run here) | `rewrite_is_byte_exact` |
| P3d | old `ACCEPTANCE` in test_bench_launcher_scan_264.py | `launcher_scan_not_vacuous` only; `five_modules` stays green, so the 264 module is vacuous (6 OK) | `rewrite_is_byte_exact` |
| P3e | old `FLOOR` path in check-run-pairs-217.py | `five_modules`: `test_run_pairs_217` fails 3 tests (floor tests, real record check) | none (the amend is not seen by tests that read the working tree and not R's non-matching bytes, see note) |
| P3f | CR bytes stripped from test_bench_launcher_scan_264.py in R | none in mode W (working tree is the intact file) | `rewrite_is_byte_exact` |

Notes: P3e in mode H shows no extra failure because the base already fails `five_modules`, hiding it; mode W is the evidence. P3f is a history mutation (only the amended R is wrong); patch file is the CR strip, applied in a scratch clone and amended. The rewrite message for P3b/P3f was not printed by the harness; the defect run below shows the naming.

## Real defect run (9b941dd, branch card-196-green-original, scratch clone, new tests copied in), 17.5 s
4 of 5 fail:
- `rewrite_is_byte_exact`: 11 of 144 files named: Phrase.sln (same length, GUIDs swapped), `crlf-source.avowry` (442 to 417), GeneratedApiBoundAddress (8227 to 8079), GeneratedApiBoundAddressEdges (7575 to 7410), GeneratedApiHazards (8389 to 8211), HoldoutTaskConversion (1426 to 1391), Support/BenchChild (3409 to 3334), Support/BenchLauncher (6087 to 5970), Support/HarnessProbes (5804 to 5643), the Compiler csproj (902 to 900), `tests/test_bench_launcher_scan_264.py` (3794 to 3713). That is the 9 CR files, the sln and the csproj, as predicted.
- `solution_guids`: both entries carry the other project's GUID.
- `five_modules`, `extractor_not_empty`: the project_of defect (also present there).
- `launcher_scan_not_vacuous`: passes.

## Note for the facilitator
The sed-miss in `project_of` is the same class as the StarvationProbe miss: a regex form of the name. A scan for `Phrase\.` (escaped dot) style fragments over scripts/ would find any other.
