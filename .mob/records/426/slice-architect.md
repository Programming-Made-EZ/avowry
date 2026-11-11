# Architect slice for card 426: the bench path scrub keeps the words around a path, covers more path forms, and its label and save-step behaviour is tested

Read-only architect (a fresh Claude context), main at d7a610e. Nothing was built or run. Saved by the facilitator from the architect's reply (angle brackets are written as the placeholder user form `<user>`; every fact and line number is the architect's; the red, the green and the gate reviewer recompute them).

## 1. Verified facts

How the regex works today (tools/Avowry.Bench/PathScrub.cs): a middle folder name (line 13) may hold spaces and brackets; the last part (line 14) may hold neither and may be empty. Four roots: a drive path (line 18; may not follow a letter, digit or `_`); a UNC path (line 19: exactly two backslashes then a non-separator, non-space); a `~` path (line 20); the Unix roots (line 21; may not follow a letter, digit, `_` or `.`). After the root: all folders first, then the file name (line 23), then the trailing-words rule for an account folder (line 25). Lines 36 and 37 turn an empty file name, or an account folder with nothing after it, into `<path>`.

Criterion 1 today: rows 1 to 3 FAIL (`Could not find file /tmp/x.cs in /usr/lib/dotnet` becomes `Could not find file dotnet`; `copy /tmp/a/x.cs to /tmp/b/y.cs` becomes `copy y.cs`; `see C:\a\b.cs (line 3) and /docs/x` becomes `see x`); rows 4 to 7 pass today (`match \d+ here` unchanged, the trailing dot and comma rows, `/x then /tmp/a/y.cs`).

Criterion 2 today (with `<user>` and with plain letters): `/mnt/c/Users/...` and `/c/Users/...` unchanged (line 21 refuses `/Users/` after `c`): FAIL; `/var/tmp/a/x.cs` and `/private/tmp/a/x.cs` unchanged: FAIL; `file://localhost/home/<user>/x.cs` unchanged: FAIL; `C:\Documents and Settings\<user>` becomes the name (no home mark): FAIL; `\\host\Users\<user>` becomes the name: FAIL; the JSON-escaped drive row: FAIL either way (with the placeholder the `\\` after `>` matches as a UNC root: `\u0027C:\\Users\\<user>u0027`); the doubled-UNC row `\\\\host\\Users\\<user>\\x.cs`: the draft said unchanged, WRONG for the placeholder form (it becomes `...<user>x.cs`; the plain-letter form is unchanged); still a leak. `C:/Users/<user>/x.cs` becomes `x.cs`: passes today. Label: Phrase bench reads it at Program.cs line 47 and scrubs only at line 817; C# baseline reads it at 35, passes it unscrubbed at 840 and scrubs at 850 (re-read and confirmed).

Summary on the current tree: 3 of the 7 criterion 1 rows fail and 22 of the 24 criterion 2 rows fail.

## 2. The design (PathScrub.cs only)

- A. Folder names may not end in whitespace: line 23 becomes `(?:(?<dir>Segment)(?<!\s)(?(esc)(?!\\u[0-9A-Fa-f]{4}))Separator)*`. `x.cs in `, `x.cs to ` and `b.cs (line 3) and ` can no longer be folders; spaced folders (`My Projects`, `Sub Folder`) end in a letter and still match.
- B. The JSON-escape rule: the check before a drive (line 18) becomes `(?:(?<![A-Za-z0-9_])|(?<=(?<esc>\\u[0-9A-Fa-f]{4})))`: a drive glued to a letter or digit counts only directly after a literal backslash-u and four hex digits; `http:` and `https:` never follow that; a single `\uXXXX` is not treated as a separator when `esc` was captured, so `\\x.cs\u0027` ends at `x.cs` and the result is `\u0027x.cs\u0027`.
- C. Line 18 home mark becomes `(?<home>(?i:users|documents and settings))`.
- D. Line 19, the UNC root: accept four leading backslashes (`(?:\\\\)?`) and an optional host followed by `Users` that sets the home mark, so the doubled UNC row becomes `x.cs` and `\\host\Users\<user>` becomes `<path>`; the 418-U1 row `\\host\share\...` is unchanged.
- E. Line 21, the Unix roots: `(?:file://(?:localhost)?)?(?:(?<home>/home/|/Users/|(?:/mnt)?/[a-z]/(?i:users)/)|/root/|/tmp/|/var/tmp/|/var/folders/|/private/tmp/|/private/var/folders/)`; `/a/b`, `src/a/b.cs`, `</Project>` and `https://example.com/home/x` still do not match.
- F. Header comment: drop the limits this card fixes; keep `\\?\`, the account folder alone taking the words after it and a TMPDIR outside the listed roots; add the new known limits (prose followed by a relative path; a path that starts after a `\uXXXX` ends before a folder named `u` plus four hex digits).

Why the existing rows still pass: 418-U1 (lines 11 to 33) has no folder ending in a space and no `\u`; U4 unaffected; `C:\` still becomes `<path>`; the run-twice check holds (`x.cs`, `<path>` and `\u0027x.cs\u0027` contain no root).

## 3. The test plan

- PathScrubForms.cs: new data sets WordsAroundAPath (the 7 criterion 1 rows; the regex row verbatim as `@"match \d+ here"`) and MissedRoots (the 12 criterion 2 rows as verbatim strings with `<user>`); the plain-letter form of each row is made at run time with `text.Replace("<user>", plain)` where `plain` is built from parts (never a literal stand-in after `Users\`; home_paths.py would flag one).
- New theories with `[Trait("Slice","426")]`, a Criterion trait (426-1, 426-2) and `[Trait("Status","Pending")]` (D204): Id_426_1 asserts each row's exact output; Id_426_2 asserts the exact output, that neither the placeholder nor the plain folder appears (ignoring case) and that a second pass changes nothing. Extend `Id_418_U2_apply_is_idempotent_on_every_form` (line 107) to loop over Forms, WordsAroundAPath and both forms of MissedRoots.
- Must fail first by assertion: criterion 1 rows 1 to 3 and criterion 2 rows 1 to 11 in both forms. Characterisation (pass on arrival, declared up front): criterion 1 rows 4 to 7, the `C:/Users` row in both forms, the extended loop, criterion 3 and the strengthened 418-3b.
- BenchRowsHoldNoFolderPath.cs, criterion 3 (traits Slice 426, Criterion 426-3, Pending): use `RunArm(..., extraBenchArguments: ["--label", label])` (line 854), which passes arguments through ArgumentList. Id_426_3a, Phrase bench: `PrintFileCommand(PassingPhrase)` with the default scoring (as NewBenchRowsCarryNoReply.cs line 54 does). Id_426_3b, C# baseline: `PrintFileCommand(PassingCSharp)` with `DotnetStandIn(WriteStandInDotnet(root, emptyFile, trxToCopy: PassingTrx, exitCode: 0))` (lines 626, 629; stand-in seam PHRASE_BENCHCS_DOTNET at BenchCs line 230). Each runs twice under two model names: `--label C:\Users\<user>\notes\run-426` and `--label run-426`, and asserts `label` reads `run-426` in results/<model>.json and in the history line.
- 418-3b: line 406 changes from `_ = AssertStored` to `var row = ...`, then asserts passed, `attempts == 2` and `AssertNoResidue(suite, "418-3b")`.
- The label proof: (1) delete tools/Avowry.Bench/Program.cs line 817 (the trailing comma on line 816 still compiles): 426-3a must fail on the path-label run; restore. (2) Delete tools/Avowry.BenchCs/Program.cs line 850: 426-3b must fail; restore. Record both failures; the diff of both Program.cs files is empty at hand-off.
- Unchanged: the residue check at BenchRowsHoldNoFolderPath.cs line 503.

## 4. Guard tests

D249: no new kind of tracked file, no guard sample. D254: no wall-clock assertion (do not add a Stopwatch or a CancellationTokenSource with a TimeSpan limit). Guards that could trip: NoHomePathsInTrackedFiles (a users folder followed by an account-like segment; `<user>`, a quote or a brace blocks it; comments must use `<user>` too), NoAccountNameInTrackedFiles (`plain` built from parts), NoPhCodesInTheLiveTree (no risk). Line-number pins: the agreement line 156 pin (tests/test_live_docs_266.py line 581) is not touched; no test pins PathScrub.cs text or line numbers; U4 checks only the csproj link.

## 5. Risks and order of work

Risks: (1) the `esc` capture inside a lookbehind used by `(?(esc)...)` must be proved by running the rows (fallback: an unconditional `(?!\\u[0-9A-Fa-f]{4}(?![\\/]))` guard on single backslashes); (2) a single-letter URL host such as `https://a/Users/x` would now be scrubbed (no row covers it); (3) on Windows a runner must print inputs from a file (`cmd` echo treats angle brackets as redirects).

Order: 1 Red (Grok): may touch only tests/Avowry.Acceptance.Tests/PathScrubForms.cs and BenchRowsHoldNoFolderPath.cs plus the temporary deletions of Program.cs lines 817 and 850 (restored); runs the new tests, reports which ids fail by assertion and which pass as characterisation, runs both deletion proofs. 2 Facilitator commits the red. 3 Green (Cursor): may touch only tools/Avowry.Bench/PathScrub.cs (parts A to F) and remove the Pending traits of the 426 tests. 4 Facilitator: the gates with the D243 merge check, the D251 stamp and the D255 git audit.

## 6. Open points for the facilitator

- Six criterion 1 and 2 rows pass today, so the red record lists them as characterisation (confirm).
- The draft's "unchanged" for the doubled-UNC placeholder row was wrong (still a leak; the card is unchanged).
- Should the strengthened 418-3b carry Criterion 426-3 and Pending during the red?
- A drive glued after `\x27` or `%27` stays out of scope (confirm).
- Four leading backslashes before a non-path (`\\\\d+`) now lose them, as `\\d+` does today; ruling 5 does not claim this row.
- The new known limit (prose followed by a relative path) goes in the header comment (confirm out of scope).
