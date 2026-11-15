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

## Fix round 1 red (card 441 review round 1; worktree at f547705, with the round-1 green port at 099a0c5)

Changes, uncommitted, in `PathScan441.cs` only. No earlier row was edited or reordered; the one edit to earlier code is in the completeness fact: its lookup is now `AllRows().Concat(AllFix1Rows())`, so it still matches `corpus.txt` (the 26 new corpus rows are embedded). Every expected output was checked against the architect's fixed reference (all 29 rows REF-OK, stable on a second pass); today's green was classified with the scratchpad harness and confirmed by the runs. Before each build and run the heavy lock printed `free`.

New rows (29): 
- 26 corpus rows, as written (the placeholder form and the stand-in form each): 11 `java-uri` and 7 `json-escaped` rows (criterion 441-1, 18 Pending: all fail on the green; `f1-java-uri-*`, `f1-json-escaped-*`), and 8 `volume-winslash` rows (441-2: 7 Pending; `docker run -v C:/Users/<user>/proj:/app x` passes already, characterisation).
- 3 reviewer mutant rows (441-3, all characterisation, because they pass on the green: they kill mutants of the fix, not of the green): `tilde-name-no-dot` (`~aa.bb/x.cs` gives `x.cs`), `ssh-host-no-dot` (`git clone ssh://git@example.com/home/<user>/r.git` gives `ssh://git@example.com/r.git`), `unc-slash-flag-off` (`cl.exe /I\\server\share\Users\<user>\inc a.c` gives `cl.exe /Iinc a.c`). `tilde-name-no-dot` uses a plain `aa.bb` name (review MINOR 9): the placeholder cannot test a dotted name.
- The four new mutants' killing rows are corpus rows already embedded in the sets above: `drive-after-slash-any` (`java-uri-02`), `drive-after-slash-adjacent-refused` (`volume-winslash-01`, `docker run -v D:/a:/a x`), `percent-das-off` (`java-uri-04`), `json-colon-backslash` (`json-escaped-01`); all Pending.

New Pending count: 25 (18 + 7). New characterisation: 4 (1 volume row, 3 mutant rows). No row needed a split string: every path-shaped input holds the placeholder or a non-name word, and the file passes the home-path rewrite unchanged.

Runs (build: 0 warnings, 0 errors):
- `Slice=441&Status=Pending`: `Failed!  - Failed:    25, Passed:     0, Skipped:     0, Total:    25` (25 of 25 `Assert.Equal() Failure: Strings differ`).
- `Slice=441&Status!=Pending`: `Passed!  - Failed:     0, Passed:   115, Skipped:     0, Total:   115` (the completeness fact included).
- `Slice=439|Slice=435`: `Passed!  - Failed:     0, Passed:   341, Skipped:     0, Total:   341`.

Failing Pending ids with their first assertion line (account name `<user>`; the id counts rows of its family under `# fix round 1`):
- `f1-java-uri-08`: Expected "vscode-remote://wsl+Ubuntu/x", Actual ···"ode-remote://wsl+Ubuntu/c:/users/<user>/x"
- `f1-json-escaped-01`: Expected "{"env":"PYTHONPATH=lib:<path>"}", Actual "{"env":"PYTHONPATH=li<user>"}"
- `f1-java-uri-06`: Expected ""/x"", Actual ""/c:/users/<user>/x""
- `f1-json-escaped-04`: Expected "{"files":"x.cs:y.cs"}", Actual "{"files":"x.cy.cs"}"
- `f1-json-escaped-02`: Expected "{"volumes":["data:<path>"]}", Actual "{"volumes":["dat<user>"]}"
- `f1-java-uri-11`: Expected ···"java.nio.file.NoSuchFileException: /x.csv", Actual ···"va.nio.file.NoSuchFileException: /C:x.csv"
- `f1-json-escaped-06`: Expected "{"Binds":["proj:\\/app"]}", Actual "{"Binds":["proapp"]}"
- `f1-json-escaped-07`: Expected "{"mount":"src:\\/src"}", Actual "{"mount":"srsrc"}"
- `f1-java-uri-07`: Expected "/x", Actual "/C:/USERS/<user>/x"
- `f1-json-escaped-05`: Expected ···"e":"svc:x:1000:1000::<path>:\\/bin\\/bash"}", Actual "{"line":"svc:x:1000:1000::<path>bash"}"
- `f1-json-escaped-03`: Expected "{"PATH":"bin:\\/usr\\/bin"}", Actual "{"PATH":"bibin"}"
- `f1-java-uri-02`: Expected "java.io.FileNotFoundException: /x.txt", Actual ···"NotFoundException: /c:/users/<user>/x.txt"
- `f1-java-uri-09`: Expected "ssh://host/r.git", Actual "ssh://host/C:r.git"
- `f1-java-uri-10`: Expected "file:/a.jar", Actual "file:/C:a.jar"
- `f1-java-uri-05`: Expected "///x", Actual "///c:/users/<user>/x"
- `f1-java-uri-01`: Expected "jar:file:/x.class", Actual "jar:file:/c:/users/<user>/.m2/a.jar!/x.cl"···
- `f1-java-uri-04`: Expected "file:/x.jar", Actual "file:/C:/Documents%20and%20Settings/<user"···
- `f1-java-uri-03`: Expected "file:/a.jar", Actual "file:/C:/Documents and Settings/<user>/.m"···
- `f1-volume-winslash-05`: Expected "  - <path>data", Actual "  - <path>d:/data"
- `f1-volume-winslash-02`: Expected "docker run -v <path>data x", Actual "docker run -v <path>d:/data x"
- `f1-volume-winslash-03`: Expected "docker run -v <path>data x", Actual "docker run -v <path>d:/data x"
- `f1-volume-winslash-04`: Expected "docker run -v <path>v x", Actual "docker run -v <path>v:/v x"
- `f1-volume-winslash-01`: Expected "docker run -v <path>a x", Actual "docker run -v <path>a:/a x"
- `f1-volume-winslash-06`: Expected "<path>x", Actual "<path>b:/x"
- `f1-volume-winslash-07`: Expected "-I<path>y", Actual "-I<path>I:/y"

## Fix round 2 red (card 441 review round 2; worktree at bfbe1d8, fix slice 2 in slice-fix2.md)

Changes, uncommitted, in `PathScan441.cs` only. No earlier row was edited or reordered; the one edit to earlier code is the completeness fact's lookup, now `AllRows().Concat(AllFix1Rows()).Concat(AllFix2Rows())`, so it still matches `corpus.txt`. The 21 rows under `# fix round 2` are embedded as `AllFix2Rows()` with ids `f2-<family>-NN` (counted within the family under that heading). Every expected output was checked against the architect's fixed reference (probe `corpus` mode on `corpus.txt`: expected_miss=0 in every family, no second-pass change); nothing disagreed with slice-fix2.md. Today's green (389d790 `PathScrub.cs`) was classified with a scratchpad harness and confirmed by the runs. The heavy lock was `busy` for about 1.5 hours (play-tracker #1017) and `free` before each build and run below. No row needed a split string.

Rows (21):
- 441-1, Pending (14): `f2-java-uri-01` to `-07` (finding 1), `f2-json-escaped-01`, `-02`, `-03`, `-04`, `-06`, `-08` (finding 2), `f2-minor-01` (MINOR 3). Theory `Id_441_1_fix2_separator_runs_escaped_lone_letters_and_the_glued_drive`, `Status=Pending`.
- 441-1, characterisation (4): `f2-json-escaped-05` and `-07` (the letter kept before an escaped root), `f2-minor-02` (pinned MINOR 4), `f2-minor-03` (pinned MINOR 5).
- 441-3, characterisation (3): `f2-env-01` (m7), `f2-java-uri-08` (m10), `f2-json-escaped-09` (m10).

Pending count: 14. Characterisation count: 7.

Runs (build: 0 warnings, 0 errors):
- `Slice=441&Status=Pending`: `Failed!  - Failed:    14, Passed:     0, Skipped:     0, Total:    14` (14 of 14 `Assert.Equal() Failure`, none by exception).
- `Slice=441&Status!=Pending`: `Passed!  - Failed:     0, Passed:   147, Skipped:     0, Total:   147` (the earlier 115, the 25 round 1 rows that no longer carry the Pending trait, and the 7 new characterisation rows).
- `Slice=439|Slice=435`: `Passed!  - Failed:     0, Passed:   341, Skipped:     0, Total:   341`.

Failing Pending ids with their first assertion line (account name `<user>`):
- `f2-java-uri-01`: Expected "/x", Actual "/C://Users//<user>//x"
- `f2-java-uri-02`: Expected "file:/x", Actual "file:/C://Users/<user>/x"
- `f2-java-uri-03`: Expected "jar:file:/x.class", Actual "jar:file:/c://users/<user>/x.class"
- `f2-java-uri-04`: Expected "jar:file:/x", Actual "jar:file:/c:///users/<user>/x"
- `f2-java-uri-05`: Expected "jar:file:/x", Actual "jar:file:/C://Documents and Settings/<use"···
- `f2-java-uri-06`: Expected "x/x", Actual "x/c:/\users\<user>\x"
- `f2-java-uri-07`: Expected "file:/x.jar", Actual "file:/C://Documents%20and%20Settings/<use"···
- `f2-json-escaped-01`: Expected "{"Binds":["<path>:\/data"]}", Actual "{"Binds":["d:\/data"]}"
- `f2-json-escaped-02`: Expected "{"Binds":["<path>:\/data:ro"]}", Actual "{"Binds":["d:\/data:ro"]}"
- `f2-json-escaped-03`: Expected "{"PATH":"<path>:\/usr\/bin"}", Actual "{"PATH":"x:\/usr\/bin"}"
- `f2-json-escaped-04`: Expected "{"env":"PYTHONPATH=<path>:\/opt\/x"}", Actual "{"env":"PYTHONPATH=l:\/opt\/x"}"
- `f2-json-escaped-06`: Expected "{"mount":"<path>:\/src"}", Actual "{"mount":"s:\/src"}"
- `f2-json-escaped-08`: Expected "{"files":"<path>:\/tmp\/y"}", Actual "{"files":"x:\/tmp\/y"}"
- `f2-minor-01`: Expected "<path>y", Actual "<path>C:\/a\/y"
