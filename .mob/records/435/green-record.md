# Card 435 green record (GREEN writing seat, Cursor, linked worktree)

Changes left uncommitted for the facilitator. Worktree HEAD `2a5be77` (the red commit). Files changed: `tools/Avowry.Bench/PathScrub.cs` (design parts A to H) and the Pending traits of the four Slice 435 tests that were Pending. Not changed: `tests/Avowry.Acceptance.Tests/Support/PathScrubBefore435.cs`, `tools/Avowry.Bench/Program.cs`, `tools/Avowry.BenchCs/Program.cs` (`git diff --stat` on the three: empty). Line endings of all three changed files stayed LF (`file` reports `ASCII text`, no CRLF). The `\\d+` token of section 2 was NOT added (ruling 6). No exclusion was added: `Exclusions = []` and `PinnedExclusionCount = 0` are as the red wrote them. No new kind of tracked file (this record is `.md`).

`git diff --stat`:

```
 tests/Avowry.Acceptance.Tests/PathScrubForms.cs    |  6 ----
 .../Avowry.Acceptance.Tests/PathScrubGenerated.cs  |  2 --
 tools/Avowry.Bench/PathScrub.cs                    | 34 +++++++++++++---------
 3 files changed, 21 insertions(+), 21 deletions(-)
```

## What changed

`tools/Avowry.Bench/PathScrub.cs`, applied as section 2 of the slice reads it:

- New constants `EscLetter`, `EscBefore`, `Glued`, `Plain`, `Word`, `Stop`, with the slice's exact text.
- A. `Segment` and `FinalSegment`: `EscLetter` added as an alternative after `<[A-Za-z]+>`.
- B. The drive, UNC and rooted-Unix lookbehind groups: `|EscBefore|Glued` appended inside each group. The `(?<=(?<esc>\\u[0-9A-Fa-f]{4}))` text is kept, so the escape-lookbehind mutants keep their base text.
- C. Separator style: the drive separator is `(?:(?<sgl>\\)(?![\\/])|Separator)`; the UNC start `(?:\\\\)?\\\\` became `(?:\\\\\\\\|(?<sgl>)\\\\)`; `~` takes `(?:(?<sgl>\\)(?![\\/])|Separator)`.
- D. UNC account folder: `(?:[^\\/\s]+Sep(?:[^\\/\s]+Sep)*?(?<home>(?i:users|home))Sep)?`.
- E. Rooted Unix: ``(?:/[^\s\\/""'`|*?:;,<>\[\]]*[A-Za-z0-9_])*?`` placed right before `(?<home>` (prefix folders only before a home root).
- F. The `dir` guard after `(?<!\s)`: `(?!\\u(?!00[0-7])[0-9A-Fa-f]{4})(?(sgl)(?(esc)(?!\\u[0-9A-Fa-f]{4}))|(?!\\(?:[nrtbf""]|u[0-9A-Fa-f]{4})))`, replacing `(?(esc)(?!\\u[0-9A-Fa-f]{4}))`.
- G. The tail: `(?<tail>(?:\(Plain*\)Plain*)*(?:[ ]+Word(?!Stop))*)`.
- H. Header comment: the escape limit is rewritten (a lone `\` before `n r t b f "` or `\uXXXX` ends a doubled-backslash or slash path); added risk 2 (a single-backslash folder named `u` plus four hex digits joins the previous name), risk 3 (only `-` plus one letter may be glued; `-isystem/home/<user>` is left as is) and the `\\d+` over-scrub (`match \\d+ here` becomes `match d+ here`); the account-folder over-scrub now names parentheses, dots and non-ASCII letters (ruling 5); the TMPDIR limit now reads "a root outside the listed ones that holds no `home` or `Users` folder". The placeholder `<user>` is the only account token in the comment.

Tests, only the traits:

- `PathScrubForms.cs`: `[Trait("Status", "Pending")]` and the comment `// Pending until the green that changes tools/Avowry.Bench/PathScrub.cs` removed from `Id_435_1_glued_unlisted_and_unc_roots`, `Id_435_2_non_ascii_account_folders`, `Id_435_3_escapes_after_a_path`.
- `PathScrubGenerated.cs`: the same two lines removed from `Id_435_3_generated_inputs_leak_nothing_and_settle_in_one_pass`.
- Nothing else in the tests changed (the data-set method names such as `GluedAndUncRootsPending` stay as the red named them).

## Commands and output

Toolchain: `dotnet --version` -> `10.0.404` (the tests target net8.0).

### `dotnet build Avowry.sln -c Debug`

After the PathScrub.cs change: `Build succeeded. 0 Warning(s) 0 Error(s) Time Elapsed 00:00:11.02`. After the trait removal: `Build succeeded. 0 Warning(s) 0 Error(s) Time Elapsed 00:00:05.18`.

### `Slice=435`

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435"
```

```
Passed!  - Failed:     0, Passed:    77, Skipped:     0, Total:    77, Duration: 6 s - Avowry.Acceptance.Tests.dll (net8.0)
```

(Before the traits were removed, the same filter with the detailed logger gave `Total tests: 77 / Passed: 77` and the counts line below.)

### `Criterion=435-1`

```
Passed!  - Failed:     0, Passed:    19, Skipped:     0, Total:    19, Duration: 74 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

### `Criterion=435-2`

```
Passed!  - Failed:     0, Passed:    49, Skipped:     0, Total:    49, Duration: 80 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

### `Criterion=435-3` (detailed logger)

```
dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-3" --logger "console;verbosity=detailed"
```

```
 inputs=100000 leaks_base=31534 leaks_new=0 regressions=0 second_pass=0 exclusions=0
Total tests: 9
     Passed: 9
```

The generator's counts line: `inputs=100000 leaks_base=31534 leaks_new=0 regressions=0 second_pass=0 exclusions=0`. `leaks_base` equals the red's 31534 (the base copy is unchanged); `leaks_new` went from 31534 at the red to 0.

### `FullyQualifiedName~PathScrubForms` (the whole class)

```
Passed!  - Failed:     0, Passed:   136, Skipped:     0, Total:   136, Duration: 87 ms - Avowry.Acceptance.Tests.dll (net8.0)
```

At the red this was Failed: 33, Passed: 103 of 136; the 418-U, 426 and 435 characterisation rows keep passing.

### `Status!=Pending` (alone)

Before the run, the only `dotnet.exe` processes alive were six idle MSBuild reuse nodes (no test host). Nothing else ran during the suite.

```
dotnet test Avowry.sln --filter "Status!=Pending"
```

```
Passed!  - Failed:     0, Passed:   344, Skipped:     0, Total:   344, Duration: 3 s - Avowry.Compiler.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:   586, Skipped:     0, Total:   586, Duration: 18 m 3 s - Avowry.Acceptance.Tests.dll (net8.0)
exit=0
```

586 = the red's 552 plus the 34 tests that were Pending (7 + 21 + 5 theory rows and the generator fact). The acceptance run took 18 m 3 s against the red's 11 m 47 s. The generator's own class took about 6 s in the Slice runs, so most of the extra time is elsewhere in the suite or on the machine. No wall-clock assertion was added or relied on.

### `python scripts/home_paths.py --root .`

```
0 files
```

## Rows not turned green

None. Every Slice 435 test passes, and `leaks_new`, `regressions` and `second_pass` are all 0 with zero exclusions.

## Findings for the facilitator (no test changed)

1. Mutant 2 survives the green. With the green code and only `/[a-z]/` narrowed to `/c/` in the rooted-Unix home root, `dotnet test ... --filter "Slice=435|Slice=426"` gave `Passed! - Failed: 0, Passed: 107, Total: 107`. The file was then restored (`cmp` against a copy saved before the mutant: identical), rebuilt, and `Slice=435` re-run: 77 of 77 passed, with the same counts line. The cause is part E: `/d/Users/<user>` and `/mnt/d/Users/<user>/x.cs` now also match as the prefix folder `/d` followed by the root `/Users/`. So the rows the red used to kill this mutant (proved on the base, ruling 9) no longer kill it. The mutant is not equivalent: by reading the pattern (not run), a lowercase `/d/users/<user>` is scrubbed by the green but would leak under the mutant, because `/Users/` is case-sensitive and the mutant takes only `/c/`. No current row or generator root has a non-`c` drive with lowercase `users` (the generator has `/c/users/`, `/d/Users/` and `/mnt/d/Users/`). A row such as `"/d/" + "users/" + user` with expected output `<path>` would kill it again. The seat was not asked to change tests, so this is left for the facilitator.
2. Mutant 7's base text `(?<home>(?i:users))` no longer exists in the file, because part D widens it to `(?<home>(?i:users|home))`. By reading (not run), the equivalent widening of the new group to any folder would still turn `\\host\share\x.cs` into `<path>`.
3. Mutants 1, 3, 4, 5 and 6 were not re-run against the green; their base text is unchanged in the green file.

## Slips

- While smoke-checking that the full suite had started, one `tail` of the raw test output printed a `Test run for ...` line holding the worktree's absolute path (and so the home folder) into this seat's tool output. It was not written to any file in the worktree. After that, only summary lines were filtered and printed. The raw output file stayed in the system temp folder, outside the repository.

## What was not run

- The other mutants against the green (see finding 3).
- The python unittest wrappers (`tests.test_home_paths_370`, `tests.test_account_name_376`, `tests.test_no_personal_names`); the brief named the `home_paths.py` scan.
