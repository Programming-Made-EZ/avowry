# Architect slice for card 435: the bench path scrub removes the whole account folder from glued, unlisted and UNC roots, from non-ASCII, parenthesised and dotted names, and from paths followed by a JSON escape

Read-only architect (fresh context), main at 7f8680c. Nothing in the repository was edited, built or tested. Every "today" output was MEASURED in a throwaway console project outside the repository that links tools/Avowry.Bench/PathScrub.cs. It ran on the net10 SDK and the tests are net8.0, so the red re-runs them. The design ran there as a modified copy on every PathScrubForms row and every card row, the mutants, a 100,000-input generator and long lines. Saved by the facilitator from the architect's reply (angle brackets as in the card; every fact, line number and count is the architect's; the red, the green and the gate reviewer recompute them).

## 1. Verified facts (base: tools/Avowry.Bench/PathScrub.cs at 7f8680c)

Criterion 1 today (both forms alike):
- `gcc /tmp/a.c -I /usr/include -I/home/<user>/include` -> `gcc a.c -I /usr/include -I/home/<user>/include`: LEAK (line 22 `(?<![A-Za-z0-9_.])` refuses `/home/` after `I`).
- `in /tmp/x.cs at /opt/data/home/<user>/x.cs` -> `in x.cs at /opt/data/home/<user>/x.cs`: LEAK (`/opt/` is no line-23 root; line 22 refuses `/home/` after `a`).
- `Restoring /tmp/x.csproj for /cygdrive/c/Users/<user>/x.csproj` -> second path unchanged; `/cygdrive/d/Users/<user>` -> unchanged: LEAK (line 22 refuses `/c/Users/` after `e`; the draft did not list the second row).
- `\\host\c$\Users\<user>`, `\\wsl$\Ubuntu\home\<user>`, `\\\\host\\c$\\Users\\<user>` -> `<user>`: LEAK (line 20 marks `users` only as the first folder, never `home`).
- Pass today: `/Users/<user>/x.cs` -> `x.cs`, `/Users/<user>` -> `<path>`, `/mnt/d/Users/<user>/x.cs` -> `x.cs`, `/d/Users/<user>` -> `<path>`, `c:\documents and settings\<user>` -> `<path>`, `\\host\share\x.cs` -> `x.cs`, and the three unchanged rows.

Criterion 2 today (prefixes `C:\Users\`, `C:\\Users\\`, `\u0027C:\\Users\\...\u0027`, `/home/`):
- Escaped `aa\u00f6bb` alone -> `u00f6bb` (the lone `\` is a line-12 Separator in the line-25 loop); quoted -> `\u0027<path>\u00f6bb\u0027` (the line-25 esc guard stops at `aa`).
- NOT IN THE DRAFT: quoted escaped folder plus a file also leaks: `\u0027C:\\Users\\<user>\u00f6bb\x.cs\u0027` -> `\u0027<path>\u00f6bb\x.cs\u0027`.
- Alone: `aa(bb)` -> `<path>(bb)` (line 14 excludes parentheses); `aa (bb)` -> `<path> (bb)`; `aa bb.cc` -> `<path>.cc`; `aa` + space + `\u00F6cc` -> `<path> \u00F6cc` (line 27 words are `[A-Za-z0-9_-]`). DRAFT WRONG: it said "the rest of the word"; the whole word survives.
- Pass today: the real-letter folder (all 8 rows); every folder plus `\x.cs` except the quoted escaped one; `see C:\Users\<user> bb.` -> `see <path>.`. 21 of 49 rows fail, 28 pass.

Criterion 3 today:
- `C:\\a\\x.cs\nnext` -> `nnext`. `/tmp/a/x.cs\tnext` -> `tnext`. `see C:\\a\\x.cs\u0022` -> `see u0022` (line 25 takes `x.cs` as a folder before the lone `\`). `\"C:\\a\\x.cs\"` -> `\"<path>"`.
- `C:\\a\\x.cs\n/home/<user>/y.cs` -> `y.cs`. Latent leak confirmed: fix the escape alone and line 22 refuses `/home/` after `n`.
- `match \\d+ here` -> `match d+ here` (line 20). `""` and the empty string unchanged.

Mutants (measured on the base pattern): the three `\\u`->`u` lookbehind mutants (lines 18, 20, 22) fail NO card row and no PathScrubForms row: a real escape still ends in `u0027`. The documents mutant is caught only if spelled `(?i:users)|Documents and Settings`; the lowercase spelling already fails line 87.

## 2. The design (tools/Avowry.Bench/PathScrub.cs only)

New constants:
- `EscLetter = @"(?<!\\)\\u(?!00[0-7])[0-9A-Fa-f]{4}"`: an escaped non-ASCII letter is a name character; an ASCII escape (`\u0027`, `\u0022`) is not.
- `EscBefore = @"(?<=(?<!\\)\\[nrt])"`; `Glued = @"(?<=(?<!\S)-[A-Za-z])"`.
- `Plain = @"[^\s\\/""'`|*?:;,()<>\[\]]"`.
- `Word = @"(?:[\p{L}\p{N}_()-]|(?<=[A-Za-z])'(?=[A-Za-z])|" + EscLetter + @"|\.(?=[\p{L}\p{N}_]))+"`.
- `Stop = @"[\p{L}\p{N}_:/-]|\\(?![nrtbf""]|u[0-9A-Fa-f]{4})|'[A-Za-z]|\.[\p{L}\p{N}_]"`.

Changes:
- A. Lines 13, 14: `EscLetter` as an alternative after `<[A-Za-z]+>`.
- B. Lines 18, 20, 22: append `|EscBefore|Glued` inside each lookbehind group. The `\\u` text stays, so the mutants keep their base text.
- C. Separator style: line 18 root separator `(?:(?<sgl>\\)(?![\\/])|" + Separator + ")"`; line 20 `(?:\\\\)?\\\\` becomes `(?:\\\\\\\\|(?<sgl>)\\\\)`; line 21 `~(?:(?<sgl>\\)(?![\\/])|" + Separator + ")"`.
- D. Line 20 account folder: `(?:[^\\/\s]+" + Separator + @"(?:[^\\/\s]+" + Separator + @")*?(?<home>(?i:users|home))" + Separator + @")?`.
- E. Line 23: ``(?:/[^\s\\/""'`|*?:;,<>\[\]]*[A-Za-z0-9_])*?`` before `(?<home>`. The prefix folder must end in an ASCII letter or digit. Without that rule, a 100,000-character `/-/-` line took 165 s; with it, 79 ms (base 46).
- F. Line 25 guard after `(?<!\s)`: `(?!\\u(?!00[0-7])[0-9A-Fa-f]{4})(?(sgl)(?(esc)(?!\\u[0-9A-Fa-f]{4}))|(?!\\(?:[nrtbf""]|u[0-9A-Fa-f]{4})))`.
- G. Line 27: `(?<tail>(?:\(" + Plain + @"*\)" + Plain + @"*)*(?:[ ]+" + Word + "(?!" + Stop + @"))*)`.
- H. Header comment: drop the fixed limits; add risks 2 and 3 of section 5 and the `\\d+` over-scrub.

Why each row reads as the card says:
- Glued `-I/home/...` -> `-Iinclude`. `/opt/data/home` and `/cygdrive/c/Users` (E) start at the first folder. URLs stay, because `(?<!:/)` and the letter lookbehind still guard the start (all URL rows measured unchanged).
- UNC (D): `users` or `home` is accepted after any folders; `\\host\share\x.cs` has neither -> `x.cs`.
- Escapes (C, F): in a path written with doubled backslashes or slashes, a lone `\` before `n r t b f "` or `\uXXXX` ends the path, so `x.cs` is the file and the escape stays. In a single-backslash path they stay separators (`C:\a\new\x.cs`, `~\tmp\x.cs`, `C:\Users\<user>\tools\x.cs` all measured `x.cs`). EscBefore closes the latent leak: `x.cs\ny.cs`.
- Names (A, G): `aa\u00f6bb` is one name in every form. Glued `(bb)`, ` (bb)`, ` bb.cc` and ` \u00F6cc` join the tail. A dot before a non-letter stays (`see <path>.`). A tail word may end before an escape, so the quoted forms read `\u0027<path>\u0027`.
- All 59 PathScrubForms rows (both forms) keep their output and a stable second pass.
- `\\d+` does NOT fall out free (candidate: `match d+ here`). One more token makes it pass: line 20 `(?=[^\\/\s])` -> `(?=[^\\/\s]+[\\/])`. Every row still passes and the generator stays clean (open point).

## 3. The test plan

PathScrubForms.cs: data sets go after line 110, theories after line 248. Traits are Slice 435 and Criterion 435-N, with Status Pending on the failing ones (D204).
- `GluedAndUncRoots()`: the 7 failing criterion-1 rows first, then 12 characterisation rows: the 9 passing card rows plus 3 mutant rows that stay unchanged: `id u0041C:\a\x.cs`, `id u0041\\host\share\x.cs`, `id u0041/tmp/a/x.cs`. Split the URL rows: `"https://example.com/" + "Users/x"`, `"https://example.com/" + "home/x"`. Theories `Id_435_1_glued_unlisted_and_unc_roots` (Take(7), Pending) and `Id_435_1_rows_that_pass_today` (Skip(7)), both through AssertMissedRoot (line 261).
- `NonAsciiAccountFolders()`: folders `"aa" + "\u00F6" + "bb"`, `"aa" + @"\u00f6" + "bb"`, `"aa(" + "bb)"`, `"aa (" + "bb)"`, `"aa " + "bb.cc"`, `"aa " + "\u00F6" + "cc"`. Each folder after the four prefixes. Alone -> `<path>`; plus `@"\x.cs"` -> `x.cs`, wrapped in `\u0027` for the quoted form. Plus `see C:\Users\<user> bb.` -> `see <path>.`. Each row carries a declared fails-today flag (the 21 rows of section 1). Theories `Id_435_2_non_ascii_account_folders` (Pending) and `Id_435_2_rows_that_pass_today` (28). A helper asserts the exact output, that none of `aa`, `bb`, `cc` appears (OrdinalIgnoreCase) and a stable second pass.
- `EscapesAfterAPath()`: the 5 failing rows as verbatim strings (`@"C:\\a\\x.cs\nnext"`, `@"\""C:\\a\\x.cs\"""` ...), then the `""` text and the empty string (characterisation). Theories `Id_435_3_escapes_after_a_path` (Pending) and `Id_435_3_rows_that_pass_today`. No `\\d+` row (ruling 6); the red records its output.

D256 differential: `tests/Avowry.Acceptance.Tests/PathScrubGenerated.cs`, class `PathScrubGenerated`. It takes ITestOutputHelper, as GeneratedSpecsMutationCheck.cs line 14 does.
- Generator: SplitMix64 written out, seed 435, pick = next % length, `const int Inputs = 100_000`. Steps: `state += 0x9E3779B97F4A7C15`; xor-shift 30, multiply `0xBF58476D1CE4E5B9`; xor-shift 27, multiply `0x94D049BB133111EB`; xor-shift 31.
- Shapes: Before (12): `""`, `see `, `-I`, `\u0027`, `\n`, `\t`, `\"`, `(`, `'`, `in /tmp/x.cs at `, `moved `, `[`. Roots (19); `|` becomes the style's separator at run time, which keeps home-path literals out of the source: `C:|Users|` single and doubled, `C:/Users/`, `file:///C:/Users/`, `c:|documents and settings|`, `C:|Documents and Settings|` doubled, `||host|Users|`, `||host|c$|Users|` single and doubled, `||wsl$|Ubuntu|home|` single and doubled, `/home/`, `/Users/`, `/mnt/d/Users/`, `/d/Users/`, `/c/users/`, `/cygdrive/c/Users/`, `/opt/data/home/`, `file://localhost/home/`. Tokens (8): `<user>` and the seven stand-ins of criteria 1-2, built from parts. Zero to three folders from `Temp`, `My Projects`, `src`, `Sub Folder`, `x.y`. File: none, `x.cs`, `Candidate.cs(7,1)`, `y.csproj`. After (15): `""`, ` end`, `.`, `, then`, `\u0027`, `\u0022`, `\nnext`, `\tnext`, `\"`, ` zz.`, ` to D:\x.cs`, `]`, `' is denied.`, ` (line 3)`, `\n/home/{token}/y.cs`. No shape text holds `aa`, `bb` or `cc`.
- Checks: the new output holds the token or a half (OrdinalIgnoreCase); a regression is such a leak where the base copy's output has none; a second pass changes the output. Any of these on a non-excluded input fails.
- Exclusions: `(string Shape, Func<string, bool> Matches, string Reason)[] Exclusions = []`, each reason naming `card 435`. `private const int PinnedExclusionCount = 0;` is asserted equal to `Exclusions.Length`. Each exclusion must match at least one input.
- Printed: `inputs=100000 leaks_base=N leaks_new=N regressions=N second_pass=N exclusions=0`, plus one line per exclusion.
- Measured with these shapes: on the base, 31,704 of 100,000 inputs leak, so the red fails by assertion; on the candidate: 0 leaks, 0 regressions, 0 second-pass changes; 3.0-3.6 s for both scrubs and the second pass (400,000 inputs: 11.3 s). Gate estimate: 4-8 s.
- Call `PathScrub.Apply` directly (the test project references Avowry.Bench, csproj line 16), not through MethodInfo.Invoke.
- Base copy: `tests/Avowry.Acceptance.Tests/Support/PathScrubBefore435.cs` is `git show 7f8680c:tools/Avowry.Bench/PathScrub.cs` with only `class PathScrub` renamed `PathScrubBefore435` (global namespace).
- Fact `Id_435_3_base_copy_still_reproduces_three_known_leaks` (passes at the red) asserts exactly `gcc a.c -I /usr/include -I/home/<user>/include`, `in x.cs at /opt/data/home/<user>/x.cs`, and `<user>` for `\\host\c$\Users\<user>`. Generator fact `Id_435_3_generated_inputs_leak_nothing_and_settle_in_one_pass` (Pending at the red).

The seven mutants. Each is named by its base text, applied to the base file, Slice=435 run, the failing row recorded, then restored:
1. line 23 `/Users/` -> `/users/`: `/Users/<user>/x.cs` and `/Users/<user>` stay unchanged.
2. line 23 `/[a-z]/` -> `/c/`: `/mnt/d/Users/<user>/x.cs`, `/d/Users/<user>` stay unchanged.
3. line 19 `(?i:users|documents and settings)` -> `(?i:users)|Documents and Settings`: `c:\documents and settings\<user>` -> `<user>`.
4. line 18 `(?<=(?<esc>\\u[0-9A-Fa-f]{4}))` -> `(?<=(?<esc>u[0-9A-Fa-f]{4}))`: `id u0041C:\a\x.cs` -> `id u0041x.cs`.
5. the same on line 20: `id u0041\\host\share\x.cs` -> `id u0041x.cs`.
6. the same on line 22: `id u0041/tmp/a/x.cs` -> `id u0041x.cs`.
7. line 20 `(?<home>(?i:users))` -> `(?<home>[^\\/\s]+)`: `\\host\share\x.cs` -> `<path>`.
Measured: each row passes on the base and fails under its mutant.

## 4. Guard tests

- D249: no new kind of tracked file. Code: `.cs` in the tests folder and its Support folder. Process files: `.md` (slice, records), `.txt` (seat transcripts, gate and sign-off output), and the `.done`, `.err` and `.jsonl` kinds already in .mob/records/435. No guard-sample.md.
- NoHomePathsInTrackedFiles (scripts/home_paths.py lines 46-49): `<user>` blocks a match, and the stand-ins are built at run time. The URL rows are split, because the commit hook also rewrites them (card 426 slip). Never type a stand-in after `Users` or `home` in a literal or comment. NoAccountNameInTrackedFiles: placeholder and parts only.
- WallClockTestsRunIsolated (D254): no Stopwatch and no time-limited CancellationTokenSource in the generator. Run time comes from the gate's class-durations.txt. NoPhCodesInTheLiveTree, GeneratedApiSourceScan (lines 66, 74) and NoBlockingProcessWaits: no matching code.
- Line pins: none on PathScrub.cs or PathScrubForms.cs (418-U4 reads only the csproj and Program.cs). tests/test_live_docs_266.py pins the agreement, which is untouched.
- Gate output: card 426's filter-*.txt hold only summary lines, so test output is lost by default. Every gate and the sign-off run `dotnet test Avowry.sln --filter "Criterion=435-3" --logger "console;verbosity=detailed"` and commit that file.
- No 435b split (size below).

## 5. Risks and order of work

Risks:
1. Contrived fuzz (300,000 random-piece strings): in 9 inputs, the new scrub keeps a token in a non-account position that the base over-scrub removed (a file after `~/` or `C:/`, before a JSON escape). Second-pass changes: 84 (base 82). The second review may report them.
2. A single-backslash folder named like `u00e9...` joins the previous name (over-scrub).
3. Only `-` plus one letter may be glued: `-isystem/home/<user>` still leaks (header comment).
4. Timing: long lines measured at most 164 ms (base 88). Dropping E's ending rule brings back the 165 s case.
5. A shell command here halved doubled backslashes: keep rows in C# files only.

Order:
1. Red: may touch only tests/Avowry.Acceptance.Tests/PathScrubForms.cs, the new PathScrubGenerated.cs and Support/PathScrubBefore435.cs, plus temporary mutant edits of tools/Avowry.Bench/PathScrub.cs (empty diff at hand-off). Records the failing ids (435-1: 7 rows in both forms; 435-2: 21; 435-3: 5 rows and the generator) and the characterisation rows apart; records the generator's first counts (detailed logger), the `\\d+` output and the seven mutants.
2. Facilitator commits the red and diffs the copy against the base blob (one line differs).
3. Green: may touch only tools/Avowry.Bench/PathScrub.cs (A-H) and the Pending traits of the 435 tests; never the base copy.
4. Facilitator: gates with D243, the D251 stamp, the D255 audit and the detailed 435-3 output.

## 6. Open points for the facilitator

- `\\d+`: not free; the one-token line-20 change passes everything measured. Default: not claimed.
- "followed by `\x.cs`" is read literally: one backslash after all four prefixes.
- `""` is read as both the two-quote text and the empty string.
- Three mutant rows go beyond the card table, because no card row catches the escape mutants: confirm.
- Exclusion pin 0 was measured with my shapes. The red's generator may differ; each leak is then a failing row or a visible exclusion with the pin raised.

Size: 15319 bytes
