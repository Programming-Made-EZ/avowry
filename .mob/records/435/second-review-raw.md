# Card 435 second review, raw (agy, gemini-4-argon-high), 2026-11-14 19:57:28Z to 20:34:40Z, exit 0, home paths scrubbed

VERDICT: FLAG

### Findings

1. **BLOCKING** — **Regression and account-folder leak (new with this card): a glued-parentheses account folder (Part G, line 35) swallows the drive letter or `file` scheme of an immediately following path**
   - **Why:** Part G adds `(?:\(" + Plain + @"*\)" + Plain + @"*)*` to `(?<tail>)` without a `(?!Stop)` guard after the trailing `Plain*`. Because `Plain` (`[^\s\\/""'`|*?:;,()<>\[\]]`, line 18) matches ASCII letters and dots, the trailing `Plain*` after `)` consumes the drive letter `D` before `:` or the `file` scheme before `://` of a second path, breaking its root so its account folder leaks where `PathScrubBefore435.Apply` scrubbed it.
   - **Exact inputs and outputs (`<user>` and `alice` / stand-in forms):**
     - Input: `C:\Users\<user>(1)D:\Users\<user>\x.cs` (also `C:\Users\<user>(bb)D:\Users\<user>\x.cs`)
       - Output before (`PathScrubBefore435.Apply`): `<path>(1)x.cs` (`<path>(bb)x.cs`)
       - Output now (`PathScrub.Apply`): `<path>:\Users\<user>\x.cs` (`<path>:\Users\<user>\x.cs`) — **new with this card** (both a regression and an account-folder leak).
     - Input: `C:\Users\<user>(1)file://localhost/home/<user>/x.cs` (also `C:\Users\<user>(bb)file:///C:/Users/<user>/x.cs`)
       - Output before: `<path>(1)x.cs` (`<path>(bb)x.cs`)
       - Output now: `<path>://localhost/home/<user>/x.cs` (`<path>:///C:/Users/<user>/x.cs`) — **new with this card** (regression and leak).

2. **BLOCKING** — **Regression and account-folder leak (new with this card, plus older in-scope gaps): stopping a path at JSON escapes (Criterion 3) exposes a following glued `-I` flag (Criterion 1) that `Glued` refuses to match**
   - **Why:** `Glued = @"(?<=(?<!\S)-[A-Za-z])"` (line 17) requires whitespace or start-of-string before `-`. When a JSON escape (`\n`, `\t`, `\r`, `\u0027`, `\u0022`) ends the first path under Part F, or wraps a `-I/home/<user>/include` argument, the character before `-I` is `n`, `t`, `r`, `7`, `2`, `"`, `'`, `(`, `[`, or `,` (all `\S`), so `(?<!\S)` fails. Before card 435, the first path over-scrubbed across `\n-I/home/<user>/include` and left no leak; now `<user>` (`alice`) leaks.
   - **Exact inputs and outputs:**
     - Input: `gcc /tmp/a.c\n-I/home/<user>/include` (also with `alice`, `\t`, or `\r`)
       - Output before: `gcc include`
       - Output now: `gcc a.c\n-I/home/<user>/include` (`gcc a.c\n-I/home/alice/include`) — **new with this card** (regression and leak).
     - Input: `gcc /tmp/a.c\u0027-I/home/<user>/include\u0027` (also with `\u0022`)
       - Output before: `gcc u0027`
       - Output now: `gcc a.c\u0027-I/home/<user>/include\u0027` — **new with this card** (regression and leak).
     - Input: `gcc "-I/home/<user>/include"`, `gcc '-I/home/<user>/include'`, `gcc \u0027-I/home/<user>/include\u0027`, `[-I/home/<user>/include]`, `(-I/home/<user>/include)`, `-Wall,-I/home/<user>/include` (and with `alice`)
       - Output before: unchanged (leaks `<user>` / `alice`)
       - Output now: unchanged (still leaks `<user>` / `alice`) — **older** (in-scope for Criterion 1 glued flags inside quoted/bracketed compiler arguments).

3. **BLOCKING** — **Regression and account-folder leak (new with this card): Part F stops paths before `\b` and `\f`, but `EscBefore` omits `b` and `f`**
   - **Why:** Line 33 ends a doubled-backslash or slash path before `(?!\\(?:[nrtbf""]|u[0-9A-Fa-f]{4}))`, which includes `\b` and `\f`, whereas `EscBefore = @"(?<=(?<!\\)\\[nrt])"` (line 16) only permits `[nrt]`. A rooted home path immediately after `\b` or `\f` is no longer swallowed by the preceding path (as in `PathScrubBefore435`), and its own lookbehind rejects `b` and `f`.
   - **Exact inputs and outputs:**
     - Input: `C:\\a\\x.cs\b/home/<user>/y.cs` and `C:\\a\\x.cs\f/home/<user>/y.cs` (also with `alice`, and `C:\\a\\x.cs\f\\host\Users\<user>\y.cs`)
       - Output before: `y.cs`
       - Output now: `x.cs\b/home/<user>/y.cs` and `x.cs\f/home/<user>/y.cs` — **new with this card** (regression and leak).

4. **BLOCKING** — **Regression, account-folder leak on pass 1, and idempotence failure on pass 2 (new with this card): `EscLetter`, `EscBefore`, and `Glued` allow a preceding path to consume the drive letter `C` before `:\Users\<user>`**
   - **Why:**
     1. With `EscLetter` added to `FinalSegment` (Part A, line 22) and `EscBefore` / `Glued` added to the Unix root lookbehind (Part B, line 30), a first path `/tmp/a` can now start after `\u00f6`, `\t`, `\n`, `\r`, or `-I`, and its `FinalSegment` consumes a trailing `\u00f6C` or `.C` right up to `:` in `C:\Users\<user>`. On pass 1, the drive letter `C` is attached to the first path's short name (`a\u00f6C` or `a.C`), leaving `:\Users\<user>` unrooted so `<user>` leaks; on pass 2, `C:\Users\<user>` is now at a valid boundary and scrubs to `<path>`.
     2. With `EscBefore` added to the UNC branch (line 28), `\t\\C:\Users\<user>` matches `\\C` as a 1-segment UNC path with `file = "C"` on pass 1 (producing `\tC:\Users\<user>`, leaking `<user>`), which then scrubs to `\t<path>` on pass 2.
   - **Exact inputs and outputs:**
     - Input: `\u00f6/tmp/a\u00f6C:\\Users\\<user>` (also with `alice`)
       - Output before: `\u00f6a\u00f6<path>` (1 pass, idempotent, no leak)
       - Output now (1st pass): `\u00f6a\u00f6C:\\Users\\<user>` (**leaks `<user>`**)
       - Output now (2nd pass): `\u00f6a\u00f6<path>` — **new with this card** (regression, leak, and second-pass change).
     - Input: `\t/tmp/a.C:\\Users\\<user>` (also `\n/tmp/a.C:\Users\<user>`, `-I/tmp/a.C:\Users\<user>`)
       - Output before: `\t/tmp/a.<path>` (1 pass, idempotent, no leak)
       - Output now (1st pass): `\ta.C:\\Users\\<user>` (**leaks `<user>`**)
       - Output now (2nd pass): `\ta.<path>` — **new with this card** (regression, leak, and second-pass change).
     - Input: `\t\\C:\Users\<user>` (also `\n\\C:\Users\<user>`)
       - Output before: `\t\\<path>` (1 pass, idempotent, no leak)
       - Output now (1st pass): `\tC:\Users\<user>` (**leaks `<user>`**)
       - Output now (2nd pass): `\t<path>` — **new with this card** (regression, leak, and second-pass change).

5. **BLOCKING** — **In-scope Criterion 2 leaks (older and new partial leaks): `Plain`, `Word`, `Stop`, and `Segment` fail on combinations of JSON-escaped non-ASCII letters (`\u00f6`), glued parentheses, dots, spaces, and apostrophes in account folders**
   - **Why:**
     1. `Plain` (`[^\s\\/""'`|*?:;,()<>\[\]]`, line 18), used by Part G for glued parentheses `(?:\(" + Plain + @"*\)" + Plain + @"*)*`, excludes `\` and does **not** include `EscLetter` (unlike `Segment`, `FinalSegment`, and `Word`). Thus an account folder with a JSON-escaped non-ASCII letter inside or after glued parentheses stops at `\u00f6` and leaks the rest of the stand-in, even though the unescaped form `aa(\u00F6bb)` and the spaced form `aa (\u00f6bb)` are scrubbed completely. `Plain` also excludes spaces and `'`, so `C:\Users\<user>(bb cc)` and `C:\Users\<user>(bb'cc)` leak `(bb cc)` and `(bb'cc)` even though `C:\Users\<user> (bb cc)` becomes `<path>`.
     2. In `Word` (line 19), `\.(?=[\p{L}\p{N}_])` only allows `[\p{L}\p{N}_]` after a dot, omitting `EscLetter` and `(`, so `C:\Users\<user> bb.\u00f6cc` leaks `.\u00f6cc` (whereas unescaped `C:\Users\<user> bb.\u00F6cc` and unspaced `C:\Users\<user>.\u00f6bb` become `<path>`).
     3. In `Segment`, `FinalSegment`, and `Word` (lines 19, 21, 22), the apostrophe rule `(?<=[A-Za-z])'(?=[A-Za-z])` only allows ASCII `[A-Za-z]` on both sides of `'`, not `\p{L}` or `EscLetter`, so an account name with a non-ASCII letter adjacent to `'` leaks the part after `'`.
     4. In `Stop` (line 20), `:` is unconditional (`[\p{L}\p{N}_:/-]`), so a spaced account folder before `: ` (`C:\Users\<user> bb: denied`) fails `(?!Stop)` on `bb` and leaks `bb`; if `bb` ends with `EscLetter` (`C:\Users\<user>\u00f6: denied`), `Word` backtracks one token so `:` is no longer preceded by `Word` and leaks `\u00f6`.
   - **Exact inputs and outputs:**
     - Input: `C:\Users\<user>(\u00f6bb)` (also `C:\\Users\\<user>(\u00f6bb)`, `\u0027C:\\Users\\<user>(\u00f6bb)\u0027`, `/home/<user>(\u00f6bb)`)
       - Output before: `u00f6bb)` (quoted: `\u0027<path>(\u00f6bb)\u0027`)
       - Output now: `<path>(\u00f6bb)` (quoted: `\u0027<path>(\u00f6bb)\u0027`) — **older / partial fix new with this card** (leaks `\u00f6bb`).
     - Input: `C:\Users\<user>(bb)\u00f6cc`
       - Output before: `u00f6cc`
       - Output now: `<path>\u00f6cc` — **older / partial fix new with this card** (leaks `\u00f6cc`).
     - Input: `C:\Users\<user>(bb cc)` and `C:\Users\<user>(bb'cc)`
       - Output before: `<path>(bb cc)` and `<path>(bb'cc)`
       - Output now: `<path>(bb cc)` and `<path>(bb'cc)` — **older** (leaks `bb cc` and `bb'cc`, whereas `C:\Users\<user> (bb cc)` becomes `<path>`).
     - Input: `C:\Users\<user> bb.\u00f6cc` and `C:\Users\<user> bb.(cc)`
       - Output before: `u00f6cc` and `<path>.(cc)`
       - Output now: `<path>.\u00f6cc` and `<path>.(cc)` — **older / partial fix new with this card** (leaks `\u00f6cc` and `(cc)`).
     - Input: `C:\Users\<user>'bb\x.cs`, `C:\Users\<user>'bb`, `C:\Users\<user>\u00f6'bb` (where `ö` is `\u00F6`)
       - Output before: `<path>'bb\x.cs`, `<path>'bb`, `u00f6'bb`
       - Output now: `<path>'bb\x.cs`, `<path>'bb`, `<path>'bb` — **older** (leaks `'bb`).
     - Input: `C:\Users\<user> bb: access denied` and `C:\Users\<user>\u00f6: access denied`
       - Output before: `<path> bb: access denied` and `u00f6: access denied`
       - Output now: `<path> bb: access denied` and `<path>\u00f6: access denied` — **older / new partial leak**.

6. **BLOCKING** — **In-scope Criterion 1 and Criterion 3 leaks (older): a bare account folder in a second path on the same line leaks as `(?<file>)` when the first path bridges into it**
   - **Why:**
     1. In Criterion 1 row 1 (`gcc /tmp/a.c -I /usr/include -I/home/<user>/include`), the only reason `/tmp/a.c` does not bridge across `-I` is the space after the middle `-I ` (which trips `(?<!\s)` before `/usr`). Without that middle spaced flag, `Segment` (`[^\\/\r\n""'`|*?:;,<>\[\]]+`) matches `a.c -I` as a directory segment! When the second path is a bare account folder (`-I/home/<user>` or `(/home/<user>)`), the match starts at `/tmp/`, bridges across `a.c -I/home/` as `(?<dir>)` captures with `home.Success == false`, and emits `<user>` (`alice`) as `(?<file>)`. Even with `/include`, `gcc /tmp/a.c -I/home/<user>/include` loses `a.c -I` (`gcc include` instead of `gcc a.c -Iinclude`).
     2. In Criterion 3 (`C:\a\x.cs\n/home/<user>`), when the first path uses single backslashes, `(?<sgl>\\)` is set on line 26, so Part F (line 33) disables the `\n` escape stop (`(?(sgl)...|(?!\\...))`). The first path treats `\` in `\n` as a path separator, bridges across `n/home/` as `(?<dir>)` captures, and returns `<user>` (`alice`) as `(?<file>)`. (`PathScrubGenerated` missed this because `afters[14]` always appends `/y.cs` after `{token}`.)
   - **Exact inputs and outputs:**
     - Input: `gcc /tmp/a.c -I/home/<user>` (also `gcc /tmp/a.c -I/home/alice`, `gcc /tmp/a.c -I/Users/<user>`, `gcc /tmp/a.c -I/cygdrive/c/Users/<user>`)
       - Output before: `gcc <user>` (`gcc alice`)
       - Output now: `gcc <user>` (`gcc alice`) — **older** (leaks `<user>` / `alice` on Criterion 1's shape without the middle `-I /usr/include`).
     - Input: `gcc /tmp/a.c -I/home/<user>/include`
       - Output before: `gcc include`
       - Output now: `gcc include` — **older** (drops `a.c -I`; Criterion 1 row 1 only kept `a.c -I` because `-I /usr/include` had a space before `/`).
     - Input: `see /tmp/x.cs (/home/<user>)` (also `see /tmp/x.cs (/home/<user>)`)
       - Output before: `see <user>)` (`see alice)`)
       - Output now: `see <user>)` (`see alice)`) — **older** (leaks `<user>` / `alice`).
     - Input: `C:\a\x.cs\n/home/<user>` and `C:\Users\<user>\n/home/alice`
       - Output before: `<user>` and `alice`
       - Output now: `<user>` and `alice` — **older** (leaks `<user>` / `alice`).

7. **BLOCKING** — **Regression in surrounding words/paths (new with this card): Part D's UNC prefix loop bridges across quotes, commas, semicolons, and colons into a later home path on the same line**
   - **Why:** Part D (line 28) replaces `(?:[^\\/\s]+" + Separator + @"(?<home>(?i:users))" + Separator + @")?` with `(?:[^\\/\s]+" + Separator + @"(?:[^\\/\s]+" + Separator + @")*?(?<home>(?i:users|home))" + Separator + @")?`. Because `[^\\/\s]+` matches `"`, `'`, `,`, `;`, `:`, `[`, `]` and has no JSON-escape stop, any non-home UNC path (`\\host\share\x.cs`) followed on the same non-whitespace token by a second path containing `Users` or `home` swallows the first path's filename, the delimiter, and the second path's root as UNC prefix folders.
   - **Exact inputs and outputs:**
     - Input: `["\\host\share\x.cs","C:\Users\<user>\y.cs"]`
       - Output before: `["x.cs","y.cs"]`
       - Output now: `["y.cs"]` — **new with this card** (regression: destroys `"x.cs",`).
     - Input: `\u0027\\\\host\\share\\x.cs\u0027,\u0027/home/<user>/y.cs\u0027`
       - Output before: `\u0027x.cs\u0027,\u0027y.cs\u0027`
       - Output now: `\u0027y.cs\u0027` — **new with this card** (regression: destroys `x.cs\u0027,\u0027`).
     - Input: `\\host\share\x.cs;C:\Users\<user>\y.cs`
       - Output before: `x.cs;y.cs`
       - Output now: `y.cs` — **new with this card** (regression: destroys `x.cs;`).

8. **BLOCKING** — **Regression on sentence-ending dot after glued parentheses (new with this card)**
   - **Why:** Criterion 2 requires that a sentence-ending dot after an account folder stay (`see C:\Users\<user> bb.` -> `see <path>.`). Because `Plain` (line 18) allows `.`, Part G's trailing `Plain*` after `)` in `(?:\(" + Plain + @"*\)" + Plain + @"*)*` swallows a sentence-ending dot after `aa(bb)`.
   - **Exact input and output:**
     - Input: `see C:\Users\<user>(bb).`
       - Output before: `see <path>(bb).`
       - Output now: `see <path>` — **new with this card** (swallows the trailing `.`, whereas `see C:\Users\<user> (bb).` becomes `see <path>.`).

9. **BLOCKING** — **Idempotence failures where a second `PathScrub.Apply` changes the output (new with this card and older)**
   - **Why:** In addition to Finding 4 above (where pass 1 leaks `<user>` and pass 2 scrubs it), stopping before JSON escapes on pass 1 leaves `~\n...` or `\\<path>` behind that matches as a new single-backslash tilde or UNC root on pass 2. (Note: `tools/Avowry.Bench/Program.cs` and `tools/Avowry.BenchCs/Program.cs` call `PathScrub.Apply` multiple times on the same diagnostic string.)
   - **Exact inputs and outputs:**
     - Input: `/tmp/a/~\nx.cs` (also `C:\\a\\~\nx.cs`, `/home/<user>/~\nx.cs`, `/tmp/a/~\tx.cs`)
       - Output before: `nx.cs` (2nd pass: `nx.cs` — idempotent)
       - Output now (1st pass): `~\nx.cs`
       - Output now (2nd pass): `nx.cs` — **new with this card**.
     - Input: `\\/home/<user>\nx.cs` (also `\\/opt/data/home/<user>`, `\\/cygdrive/c/Users/<user>`)
       - Output before: `\\nx.cs` / `\\/opt/data/home/<user>` (2nd pass unchanged — idempotent)
       - Output now (1st pass): `\\<path>\nx.cs` / `\\<path>`
       - Output now (2nd pass): `nx.cs` / `<path>` — **new with this card**.
     - Input: `/tmp/a/file://localhost/home/<user>/x.cs` (also `C:\\a\\file://localhost/home/<user>/x.cs`)
       - Output before (1st pass): `file://localhost/home/<user>/x.cs` (**leaks `<user>`**); (2nd pass): `x.cs`
       - Output now (1st pass): `file://localhost/home/<user>/x.cs` (**leaks `<user>`**); (2nd pass): `x.cs` — **older** (both a 1st-pass leak and a 2nd-pass change).
     - Input: `C:\\a\\b.C:\Users\<user>`
       - Output before (1st pass): `b.C:\Users\<user>` (**leaks `<user>`**); (2nd pass): `b.<path>`
       - Output now (1st pass): `b.C:\Users\<user>` (**leaks `<user>`**); (2nd pass): `b.<path>` — **older**.

10. **BLOCKING** — **Catastrophic / quadratic $O(N^2)$ backtracking on 20,000-character and 1 MB lines (new with this card)**
    - **Why:** Part D (line 28) added `(?:[^\\/\s]+" + Separator + @"(?:[^\\/\s]+" + Separator + @")*?(?<home>(?i:users|home))" + Separator + @")?` to the UNC branch, where `[^\\/\s]+` is much wider than `Segment` and has no JSON-escape stop. When a single line contains repeated UNC starts followed by characters excluded from `Segment`/`FinalSegment` (such as `;` or `,` in path lists) or doubled-backslash UNC paths separated by `\n`, each UNC match attempt scans all remaining $O(N)$ tokens to the end of the line looking for `users|home`, fails, falls back to the outer `?`, consumes only the short UNC prefix, and repeats at the next `\\` — turning linear $O(N)$ scanning into $O(N^2)$ backtracking (~350–490x slower at 20,000 characters):
    - **Measured timings (outside-repo net8.0 probe, warmed up):**
      | Input shape | Length | Before (`PathScrubBefore435`) | Now (`PathScrub`) | Status |
      |---|---:|---:|---:|---|
      | `string.Concat(Enumerable.Repeat(@"\\;", 6_666))` | 19,998 | **16 ms** | **6,587 ms** | **BLOCKING (new, 411x blow-up)** |
      | `string.Concat(Enumerable.Repeat(@"\\,", 6_666))` | 19,998 | **14 ms** | **6,868 ms** | **BLOCKING (new, 490x blow-up)** |
      | `string.Concat(Enumerable.Repeat(@"\\a;", 5_000))` | 20,000 | **14 ms** | **4,310 ms** | **BLOCKING (new, 307x blow-up)** |
      | `string.Concat(Enumerable.Repeat(@"\\\\host\\share\\x.cs\n", 870))` | 20,010 | **3 ms** | **923 ms** | **BLOCKING (new, 307x blow-up)** |
      | `string.Concat(Enumerable.Repeat("/-", 10_000))` | 20,000 | 16 ms | 29 ms | OK |
      | `string.Concat(Enumerable.Repeat("/a", 10_000))` | 20,000 | 27 ms | 47 ms | OK |
      | `string.Concat(Enumerable.Repeat(@"C:\Users\<user> ", 1_251))` | 20,016 | 8 ms | 11 ms | OK |
      | `string.Concat(Enumerable.Repeat(@"C:\Users\<user>(a)", 1_111))` | 19,998 | 15 ms | 11 ms | OK |
      | ~1 MB mixed paths + prose (`40,000` paths) | 1,072,000 | 576 ms | 891 ms | OK |
      | ~1 MB `"/a"` repeated `524,288` times | 1,048,576 | 1,098 ms | 1,775 ms | OK |
      | ~1 MB `"/-"` repeated `524,288` times | 1,048,576 | 778 ms | 1,171 ms | OK |

11. **BLOCKING** — **Surviving mutants on lines changed by the card (Mutant 2 plus 11 mutants of the new constants and parts)**
    - **Why:**
      1. **Mutant 2 (`/[a-z]/` -> `/c/` on line 31) confirmed surviving:** All 77 `Slice=435` tests and all 30 `Slice=426` tests pass under it. Because Part E added `(?:/[^\s\\/""'`|*?:;,<>\[\]]*[A-Za-z0-9_])*?` before `(?<home>/home/|/Users/|(?:/mnt)?/[a-z]/(?i:users)/)`, `/d/Users/<user>` and `/mnt/d/Users/<user>/x.cs` now match via prefix folder `/d` (or `/mnt/d`) + `/Users/`. However, `/Users/` is case-sensitive, so a non-`c` drive with lowercase `users` still relies on `(?:/mnt)?/[a-z]/(?i:users)/`.
         - **Smallest test row that kills Mutant 2:** `"/d/" + "users/" + user` with expected output `"<path>"` (green returns `"<path>"`; Mutant 2 returns `"/d/users/<user>"`).
      2. **11 mutants of the new constants and parts on lines 15–20, 28, 35 also survive every `Slice=435` and `Slice=426` test** (full list and killing inputs in the **Mutants** section below).

12. **MINOR** — **Tilde (`~`) branch (line 29) omits `EscBefore`, `Glued`, and the `\uXXXX` lookbehind**
    - **Exact inputs and outputs:**
      - Input: `C:\\a\\x.cs\n~/y.cs` -> Output before: `y.cs`, Output now: `x.cs\n~/y.cs` (**new with this card**; `~` does not Carry `<user>`, so no account-name leak, but `~/y.cs` is no longer scrubbed after `\n`).
      - Input: `\u0027~/x.cs\u0027` and `-I~/include` -> Output before: `\u0027~/x.cs\u0027` and `-I~/include`, Output now: unchanged (**older**).

13. **MINOR** — **UNC paths with `Documents and Settings` are not recognized as `<home>`**
    - **Exact input and output:**
      - Input: `\\host\c$\Documents and Settings\<user>` -> Output before: `<user>`, Output now: `<user>` (**older**; Part D only checks `(?i:users|home)` on UNC paths, whereas line 27 checks `(?i:users|documents and settings)` on drive paths).

---

### Mutants

#### A. The seven slice mutants (`tools/Avowry.Bench/PathScrub.cs`)

1. **Mutant 1** (line 31: `/Users/` -> `/users/`): **KILLED**
   - `Slice=435`: `Failed! - Failed: 3, Passed: 74, Skipped: 0, Total: 77` (fails `/Users/<user>/x.cs`, `/Users/<user>`, and `Id_435_3_generated_inputs_leak_nothing_and_settle_in_one_pass`).
2. **Mutant 2** (line 31: `/[a-z]/` -> `/c/`): **SURVIVES (BLOCKING)**
   - `Slice=435`: `Passed! - Failed: 0, Passed: 77, Skipped: 0, Total: 77`
   - `Slice=426`: `Passed! - Failed: 0, Passed: 30, Skipped: 0, Total: 30`
   - **Smallest test row that kills it:** `R("/d/" + "users/" + user, "<path>")` (or `/mnt/d/users/<user>`).
3. **Mutant 3** (line 27: `(?i:users|documents and settings)` -> `(?i:users)|Documents and Settings`): **KILLED**
   - `Slice=435`: `Failed! - Failed: 2, Passed: 75, Skipped: 0, Total: 77` (fails `c:\documents and settings\<user>` and the generator).
4. **Mutant 4** (line 26: drive `(?<=(?<esc>\\u[0-9A-Fa-f]{4}))` -> `(?<=(?<esc>u[0-9A-Fa-f]{4}))`): **KILLED**
   - `Slice=435`: `Failed! - Failed: 1, Passed: 76, Skipped: 0, Total: 77` (fails `id u0041C:\a\x.cs`).
5. **Mutant 5** (line 28: UNC `(?<=(?<esc>\\u[0-9A-Fa-f]{4}))` -> `(?<=(?<esc>u[0-9A-Fa-f]{4}))`): **KILLED**
   - `Slice=435`: `Failed! - Failed: 1, Passed: 76, Skipped: 0, Total: 77` (fails `id u0041\\host\share\x.cs`).
6. **Mutant 6** (line 30: Unix `(?<=(?<esc>\\u[0-9A-Fa-f]{4}))` -> `(?<=(?<esc>u[0-9A-Fa-f]{4}))`): **KILLED**
   - `Slice=435`: `Failed! - Failed: 1, Passed: 76, Skipped: 0, Total: 77` (fails `id u0041/tmp/a/x.cs`).
7. **Mutant 7** (line 28: UNC `(?<home>(?i:users|home))` widened to `(?<home>[^\\/\s]+)`): **KILLED**
   - Confirmed still caught after Part D changed its base text.
   - `Slice=435`: `Failed! - Failed: 5, Passed: 72, Skipped: 0, Total: 77` (fails `\\host\share\x.cs`, `\\host\c$\Users\<user>`, `\\wsl$\Ubuntu\home\<user>`, `\\\\host\\c$\\Users\\<user>`, and the generator).

#### B. Mutants of the new constants and parts (`EscLetter`, `EscBefore`, `Glued`, `Word`, `Stop`, Part D, Part G) that no test catches

Every mutant below was compiled and run against `Slice=435` (77 tests) and `Criterion=426-1|Criterion=426-2|Criterion=426-4` (27 tests); **all 11 SURVIVE (0 failures)**:

1. **`EscLetter` (line 15):** `(?<!\\)\\u(?!00[0-7])[0-9A-Fa-f]{4}` -> `(?<!\\)\\u(?!002)[0-9A-Fa-f]{4}` — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). No test uses `\u003b`, `\u003a`, `\u005c`, or any `\u00[013-7]X` escape adjacent to a path/account name.
2. **`EscBefore` (line 16):** `(?<=(?<!\\)\\[nrt])` -> `(?<=(?<!\\)\\[nt])` (dropping `r`) — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). No test or generator shape places `\r` before a path (would fail on `C:\\a\\x.cs\r/home/<user>/y.cs`).
3. **`EscBefore` (line 16):** `(?<=(?<!\\)\\[nrt])` -> `(?<=\\[nrt])` (dropping `(?<!\\)`) — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). No test places an escaped backslash `\\n` before a path.
4. **`Glued` (line 17):** `(?<=(?<!\S)-[A-Za-z])` -> `(?<=(?<!\S)-I)` (narrowing `[A-Za-z]` to `I`) — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). No test or generator shape uses `-L`, `-F`, or any flag letter other than `-I`.
5. **`Glued` (line 17):** `(?<=(?<!\S)-[A-Za-z])` -> `(?<=-[A-Za-z])` (dropping `(?<!\S)`) — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). No test checks that a word-internal hyphenated segment such as `foo-bar/home/x` is not treated as a glued flag.
6. **`Word` (line 19):** dropping `|" + EscLetter` from `Word` — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). Neither `PathScrubForms` nor `PathScrubGenerated` tests an account folder with a space followed by a JSON-escaped non-ASCII letter (such as `C:\Users\<user> \u00f6cc`, which leaks `\u00f6cc` under this mutant).
7. **`Word` (line 19):** dropping `|(?<=[A-Za-z])'(?=[A-Za-z])` from `Word` — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). `OddAccountFolders` (`Slice=418`) only tests `O'Brien` without a space (`C:\Users\<user>'Brien`, matched by `FinalSegment`, not `Word`), and no test has a space before an apostrophe word (`C:\Users\<user>'bb`).
8. **`Stop` (line 20):** dropping `|\.[\p{L}\p{N}_]` from `Stop` — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). Under this mutant, `C:\Users\<user>` backtracks to `C:\Users\<user>` and leaks `.cc!`, but no test follows `aa bb.cc` with punctuation after `.cc`.
9. **`Stop` (line 20):** changing `[nrtbf""]` to `[nt""]` in `Stop` — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). No test follows a spaced account folder with `\r`, `\b`, or `\f` (`C:\Users\<user>\r`).
10. **Part D (line 28):** changing `(?<home>(?i:users|home))` to case-sensitive `(?<home>Users|home)` — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). No test or generator root uses lowercase `users` or uppercase `HOME` in a UNC path (`\\host\c$\users\<user>`).
11. **Part G (line 35):** dropping the trailing `Plain*` in `(?:\(" + Plain + @"*\)" + Plain + @"*)*` -> `(?:\(" + Plain + @"*\))*` — **SURVIVES** (`Slice=435`: 77/77 pass; `426`: 27/27 pass). No test has characters glued after `)` in an account folder (such as `C:\Users\<user>(bb)cc`).

---

### Commands Run and Summary Lines

All temporary mutant edits to `tools/Avowry.Bench/PathScrub.cs` were restored byte-for-byte (`git diff --stat` and `git status -s` empty at completion).

1. `dotnet build Avowry.sln -c Debug`
   - `Build succeeded. 0 Warning(s) 0 Error(s) Time Elapsed 00:00:03.17`
2. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435"`
   - `Passed!  - Failed:     0, Passed:    77, Skipped:     0, Total:    77, Duration: 6 s - Avowry.Acceptance.Tests.dll (net8.0)`
3. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=426"`
   - `Passed!  - Failed:     0, Passed:    30, Skipped:     0, Total:    30, Duration: 29 s - Avowry.Acceptance.Tests.dll (net8.0)`
4. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-1"`
   - `Passed!  - Failed:     0, Passed:    19, Skipped:     0, Total:    19, Duration: 52 ms - Avowry.Acceptance.Tests.dll (net8.0)`
5. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-2"`
   - `Passed!  - Failed:     0, Passed:    49, Skipped:     0, Total:    49, Duration: 47 ms - Avowry.Acceptance.Tests.dll (net8.0)`
6. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-3" --logger "console;verbosity=detailed"`
   - `inputs=100000 leaks_base=31534 leaks_new=0 regressions=0 second_pass=0 exclusions=0`
   - `Total tests: 9, Passed: 9`
7. Mutant runs (each built with `dotnet build Avowry.sln -c Debug --nologo` -> `Build succeeded. 0 Error(s)` and tested with `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug`):
   - Mutant 1 (`Slice=435`): `Failed!  - Failed:     3, Passed:    74, Skipped:     0, Total:    77, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)`
   - Mutant 2 (`Slice=435`): `Passed!  - Failed:     0, Passed:    77, Skipped:     0, Total:    77, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)`
   - Mutant 2 (`Slice=426`): `Passed!  - Failed:     0, Passed:    30, Skipped:     0, Total:    30, Duration: 27 s - Avowry.Acceptance.Tests.dll (net8.0)`
   - Mutant 3 (`Slice=435`): `Failed!  - Failed:     2, Passed:    75, Skipped:     0, Total:    77, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)`
   - Mutant 4 (`Slice=435`): `Failed!  - Failed:     1, Passed:    76, Skipped:     0, Total:    77, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)`
   - Mutant 5 (`Slice=435`): `Failed!  - Failed:     1, Passed:    76, Skipped:     0, Total:    77, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)`
   - Mutant 6 (`Slice=435`): `Failed!  - Failed:     1, Passed:    76, Skipped:     0, Total:    77, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)`
   - Mutant 7 (`Slice=435`): `Failed!  - Failed:     5, Passed:    72, Skipped:     0, Total:    77, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)`
   - 11 new-constant/part mutants (`EscLetter`, `EscBefore` x2, `Glued` x2, `Word` x2, `Stop` x2, `Part D`, `Part G`):
     - `Slice=435` (each of 11): `Passed!  - Failed:     0, Passed:    77, Skipped:     0, Total:    77, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)`
     - `Criterion=426-1|Criterion=426-2|Criterion=426-4` (each of 11): `Passed!  - Failed:     0, Passed:    27, Skipped:     0, Total:    27, Duration: 70-88 ms - Avowry.Acceptance.Tests.dll (net8.0)`
8. Post-restore verification (`git diff --stat`, `git status -s`, `dotnet build Avowry.sln -c Debug`, `Slice=435`):
   - `git diff --stat`: empty
   - `git status -s`: empty
   - `Build succeeded. 0 Error(s)`
   - `Passed!  - Failed:     0, Passed:    77, Skipped:     0, Total:    77, Duration: 7 s - Avowry.Acceptance.Tests.dll (net8.0)`
