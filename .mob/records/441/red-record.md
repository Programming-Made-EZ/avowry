# Card 441 red record (RED seat, claude sonnet = claude-sonnet-5-5, ctx-2, D264)

Worktree wt-441, branch card-441, HEAD 9d47068. Uncommitted. No production file and none of card 439's files touched (`git diff --stat` is empty; the new file and this record are untracked).

## File
- `tests/Avowry.Acceptance.Tests/PathScan441.cs` (new). The 104 rows of `corpus.txt` are embedded row for row, plus 6 rows the corpus does not hold (the mutant killing rows); a completeness fact (`Id_441_1_the_embedded_rows_are_the_corpus_file`) reads `.mob/records/441/corpus.txt` at test time and fails if a file row is not embedded.
- Each row runs as written and with the placeholder replaced by the stand-in (`aa` + `bb`, built at run time; the expected output the same way); each is also asserted stable on a second pass and, unless it pins an older limit (expected output holds the placeholder: the `# pinned:` leaking rows `https://example.com/home/<user>/x`, `/tmp/x'y:`, `x://server`, 9b), asserted to hold neither the placeholder nor the stand-in.
- Rows were embedded by a generator script in the scratchpad; every expected output was first checked against the architect's reference (probe `PathScan.cs`): all 110 rows give the listed output with a stable second pass (REF-OK). Today's product was classified with a throwaway harness (the product file compiled beside the reference), then confirmed by the real runs below.

## Data sets (Theory per set; Pending vs characterisation)
| Set | Criterion | Pending | Characterisation |
|---|---|---|---|
| Older real shapes (compiler, env, volume, passwd, JSON, remote, trace, older MINOR) | 441-1 | 14 | 75 |
| Contrived chains and named limits (one-letter chains, `/proj:\x`, `/pj:/app`, 2.5n, 9b, c11) | 441-2 | 7 | 6 |
| Mutant killing rows not in the corpus (`ownletter-a-letters-only`, `https://a/` + `Users/x`, flag-no-slash, tilde-flag-multi, ownletter-b-any-target, `-v /home/` + `<user>:/data`) | 441-3 | 1 | 5 |
| Device paths before a drive (pinned as card 439 pinned them) | 441-5 | 0 | 2 |
| Completeness fact | 441-1 | | 1 |

441-4 (the redundant condition `j > finalStart`) has no test of its own: it is covered by every card 439 and 435 row still passing. The killing rows of mutants that are corpus rows (for example `ssh-host-off`, `json-slash-off`, `no-var-mail`) are the corpus rows themselves, in the 441-1 and 441-2 sets.
The two literal-name rows are split strings in code (ruling 4): `"-v /home/" + "u1a:/data"` (gives `-v <path>:/data`; it passes on today's product, so it is characterisation) and `"see https://a/" + "Users/x ok"` (unchanged; passes).

## Commands and summary lines (the heavy lock printed `busy` before each; only the three per-slice runs were made, after `dotnet build Avowry.sln -c Debug`: 0 warnings, 0 errors)
- `--filter "Slice=441&Status=Pending"`: `Failed!  - Failed:    22, Passed:     0, Skipped:     0, Total:    22` (22 of 22 `Assert.Equal() Failure: Strings differ`).
- `--filter "Slice=441&Status!=Pending"`: `Passed!  - Failed:     0, Passed:    89, Skipped:     0, Total:    89`.
- `--filter "Slice=439|Slice=435"`: `Passed!  - Failed:     0, Passed:   341, Skipped:     0, Total:   341`.

## Failing (Pending) ids with the first assertion line (account name `<user>`; ids are `<family>-<n>` in corpus order, `minor-n` and `mut-n` count within their family)
- `minor-07`: Expected "a/b:<path>", Actual "a/<user>"
- `remote-11`: Expected "git clone ssh://git@host:2222/r.git", Actual ···"one ssh://git@host:2222/home/<user>/r.git"
- `json-11`: Expected "{"home":"<path>"}", Actual "{"home":"\\/Users\\/<user>"}"
- `minor-03`: Expected "x.cs", Actual "\\\\?\\UNC\\server\\share\\Users\\<user>\\x.cs"
- `compiler-02`: Expected "gcc -isysteminclude main.c", Actual "gcc -isystem/home/<user>/include main.c"
- `env-13`: Expected "MAIL=<path>", Actual "MAIL=/var/spool/mail/<user>"
- `minor-01`: Expected "x.cs", Actual "~<user>/x.cs"
- `compiler-08`: Expected "cl.exe /c /Iinclude /Fomain.obj main.c", Actual "cl.exe /c /IC:\\Users\\<user>\\include /FoC:"···
- `env-08`: Expected "MAIL=<path>", Actual "MAIL=/var/mail/<user>"
- `json-03`: Expected "{"file":"x.cs"}", Actual "{"file":"\\/home\\/<user>\\/x.cs"}"
- `remote-01`: Expected "git remote add origin ssh://host/repo.git", Actual ···"dd origin ssh://host/home/<user>/repo.git"
- `compiler-16`: Expected "clang -idirafterinc -iquoteq a.c", Actual "clang -idirafter/home/<user>/inc -iquote/"···
- `remote-06`: Expected "sftp://host/x.txt", Actual "sftp://host/home/<user>/x.txt"
- `minor-02`: Expected "ls src", Actual "ls ~<user>/src"
- `minor-11`: Expected "-v <path>:/data", Actual "-v x:/data"
- `minor-13`: Expected "-v <path>:/v", Actual "-v v:/v"
- `minor-09`: Expected "x:<path>", Actual "<path><user>"
- `minor-12`: Expected "<path>:/x", Actual "x:/x"
- `minor-10`: Expected "a:<path>", Actual "<path><user>"
- `minor-08`: Expected "a:<path>", Actual "<path><user>"
- `minor-14`: Expected "<path>://server/home/<user>", Actual "x://server/home/<user>"
- `mut-01`: Expected "cl.exe /c /Iinclude main.c", Actual "cl.exe /c /IC:\\Users\\<user>\\include main."···

## Characterisation rows
All other corpus rows pass on today's product (88 rows plus the completeness fact): the container volume, passwd and stack-trace families entirely, the JSON rows except the two escaped-slash rows, the compiler, environment and remote rows other than those above, the contrived and pinned rows `/tmp/x'y:`, 2.5n, 9b, `/home/<user>:/home/<user>`, `/proj:\x`, `/pj:/app`, `\\?\` and `\\.\` device paths, and the mutant rows flag `-I/home/<user>/src~\nx.cs`, `C:\a\D:/b`, `-v /home/<user>:/data`, the `u1a` row and the `https://a/` row. (The same-named leaking pinned rows pass because today's product gives the same leak.)

## Deviations and notes
- The slice names the killing row of `ownletter-a-letters-only` as `-v /home/` + `u1a:/data`; it already passes on today's product (the product has the narrower rule in a form this row does not catch), so it is characterisation, not Pending: it kills the mutant only when the green is mutated.
- `x://server/home/<user>` (minor-14, pinned) is Pending: today's product gives `x://server/home/<user>` for the stand-in and the placeholder forms, where the reference gives `<path>://server/home/<user>` (the card says it is now stable on a second pass).
- I built and ran a small harness (scratchpad, outside the repository) with the product file and the architect's reference to classify the rows: one `dotnet build` of a tiny project while the heavy lock showed busy; no test run beyond the three filters. Nothing touched the repository.
- The two literal-name rows and every path-shaped line are checked by `scripts/home_paths.py` (the commit hook's rewrite): the test file is unchanged by it.
