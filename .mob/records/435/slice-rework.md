# Card 435 rework: architect re-slice (architect (claude, opus declared: regex redesign, strong tier); a fresh context, read-only; measured in a probe outside the repository)

Saved by the facilitator from the architect's reply, unedited except home paths. The facilitator's rulings on its open points follow the report.

## Card 435 rework: architect report (measured, read-only)

My redesign fixes every BLOCKING finding. It changes three constants, two lines of the pattern and one line of `Apply`. All 134 rows of the 418, 426 and 435 data sets give the same output as the green, in both forms (`<user>` and the stand-in). The 12 mutants named in the review, plus 15 mutants of my new pieces, are each killed by a named row. Most timings are now close to the pre-card copy, but two shapes are still more than 3x slower (section 5).

I measured everything on net8.0 with `RollForward Major` in my own probe. It compiles copies of the green and of `PathScrubBefore435`, and my design as a separate class. Nothing in the clone or the main checkout was edited.

### 1. Paste-ready changes to `tools/Avowry.Bench/PathScrub.cs`

**Constants** (the others stay as they are):
```csharp
private const string EscBefore = @"(?<=(?<!\\)\\[nrtbf])";
private const string Glued = @"(?<=(?:(?<![^\s""'])|" + EscBefore + @"|(?<=\\u[0-9A-Fa-f]{4}))-[A-Za-z])";
private const string FinalSegment = @"(?:<[A-Za-z]+>|" + EscLetter + @"|(?<=[A-Za-z])'(?=[A-Za-z])|(?!(?:file|[A-Za-z]):[\\/])[^\s\\/""'`|*?:;,()<>\[\]])*";
```
`Glued` must come after `EscBefore` in the file.

**Pattern line 28 (UNC, part D):**
```csharp
@"|(?:(?<![\w\\:])|(?<=(?<esc>\\u[0-9A-Fa-f]{4}))|" + EscBefore + "|" + Glued + @")(?:\\\\\\\\|(?<sgl>)\\\\)(?=[^\\/\s<])(?:(?:(?:" + Plain + @"+|[A-Za-z]:)(?(sgl)(?(esc)(?!\\u[0-9A-Fa-f]{4}))|(?!\\(?:[nrtbf""]|u[0-9A-Fa-f]{4})))" + Separator + @")+?(?<home>(?i:users|home))" + Separator + @")?" +
```

**Pattern line 35 (tail, part G):**
```csharp
@"(?(dir)(?:)|(?(home)(?<tail>(?:\(" + Plain + @"*\)(?:" + Word + @"(?![\p{L}\p{N}_]|:[\\/]))?)*(?:[ ]+" + Word + "(?!" + Stop + @"))*)))",
```

**`Apply`, the `<path>` condition:**
```csharp
if (file.Length == 0 || file == "~" || (match.Groups["home"].Success && match.Groups["dir"].Captures.Count == 0))
```

**What each change does:**
- **Part D (UNC prefix):**
  - Each prefix folder is now `Plain+` (no quotes, commas, semicolons, brackets, colons or parentheses) or a bare drive `X:`, so `\\C:\Users\…` still works.
  - After each folder it uses the same escape guard as part F, so it can only reach where the folder loop reaches. That makes it linear.
  - The UNC lookahead refuses `<`, so a first-pass `\\<path>` is not taken as a UNC root on pass 2.
- **Part F and `EscBefore`:** `EscBefore` now covers `n r t b f`. The `"` escape and `\uXXXX` already start a path through the plain and `esc` lookbehinds, so the set where a path ends and the set where a new one may start now agree.
- **`Glued`:** a flag may now follow whitespace, the start of the text, `"`, `'`, `\n r t b f`, or `\uXXXX`.
- **`FinalSegment`:** a final name never takes a letter, or `file`, when it is followed by `:` and a separator. This applies on every start.
- **Part G:** after `)`, glued text must be a `Word` that does not end before a letter, a digit or `:\` / `:/`. So it leaves `D:`, `file:` and a sentence-ending dot alone.
- **`Apply`:** a final name of `~` becomes `<path>`. This gives pass-2 stability for `…/~\nx.cs`.

**Header comment changes:**
- Add: "In a doubled-backslash or slash path a lone `\` before `n r t b f "` or `\uXXXX` ends the path, and a new path, or a glued flag, may start right after it."
- Add: "A UNC folder before `Users` or `home` never holds a quote, comma, semicolon, bracket, colon or JSON escape."
- Replace "only `-` plus one letter may be glued to a root" with: "`-` plus one letter may be glued to a root after a space, a quote or an escape (`-isystem/home/<user>` is left as is)".
- Add: "A bare `\\` before a rooted path leaves `\\name`, which a second pass changes (older)."

### 2. BLOCKING inputs (`u` = `<user>`; the stand-in form gives the same result)

| # | Input | Before | Green (pass 2) | Design (pass 2 = pass 1) |
|---|---|---|---|---|
| 1 | `C:\Users\<user>(1)D:\Users\<user>\x.cs` | `<path>(1)x.cs` | `<path>:\Users\<user>\x.cs` LEAK | `<path>x.cs` |
| 1 | `C:\Users\<user>(1)file://localhost/home/u/x.cs` | `<path>(1)x.cs` | `<path>://localhost/home/u/x.cs` LEAK | `<path>x.cs` |
| 1 | `C:\Users\<user>(bb)file:///C:/Users/<user>/x.cs` | `<path>(bb)x.cs` | `<path>:///x.cs` | `<path>x.cs` |
| 2 | `gcc /tmp/a.c\n-I/home/u/include` (also `\t`, `\r`) | `gcc include` | unchanged, LEAK | `gcc a.c\n-Iinclude` |
| 2 | `gcc /tmp/a.c\u0027-I/home/u/include\u0027` (also `\u0022`) | `gcc u0027` | unchanged, LEAK | `gcc a.c\u0027-Iinclude\u0027` |
| 2 | `gcc '-I/home/u/include'`, `"-I…"`, `\u0027-I…\u0027` | LEAK | LEAK | `gcc '-Iinclude'` and the same for the other two |
| 3 | `C:\\a\\x.cs\b/home/u/y.cs` (also `\f`, and `\f\\host\Users\u\y.cs`) | `y.cs` | unchanged, LEAK | `x.cs\by.cs` / `x.cs\fy.cs` |
| 4a | `\u00f6/tmp/a\u00f6C:\\Users\\<user>` | `\u00f6a\u00f6<path>` | LEAK (pass 2 `…<path>`) | `\u00f6a\u00f6<path>` |
| 4b | `\t/tmp/a.C:\\Users\\<user>`, `\n/tmp/a.C:\Users\<user>`, `-I/tmp/a.C:\Users\<user>` | `\t/tmp/a.<path>` | LEAK (pass 2 changes) | `\ta.<path>`, `\na.<path>`, `-Ia.<path>` |
| 4c | `\t\\C:\Users\<user>` (refuted) | `\t\\<path>` | `\t<path>` | `\t<path>` |
| 7 | `["\\host\share\x.cs","C:\Users\<user>\y.cs"]` | `["x.cs","y.cs"]` | `["y.cs"]` | `["x.cs","y.cs"]` |
| 7 | `\u0027\\\\host\\share\\x.cs\u0027,\u0027/home/u/y.cs\u0027` | `\u0027x.cs\u0027,\u0027y.cs\u0027` | `\u0027y.cs\u0027` | same as before |
| 7 | `\\host\share\x.cs;C:\Users\<user>\y.cs` | `x.cs;y.cs` | `y.cs` | `x.cs;y.cs` |
| 8 | `see C:\Users\<user>(bb).` | `see <path>(bb).` | `see <path>` | `see <path>.` |
| 9 | `/tmp/a/~\nx.cs`, `C:\\a\\~\nx.cs`, `/home/<user>/~\nx.cs` | `nx.cs` | `~\nx.cs` → `nx.cs` | `<path>\nx.cs` |
| 9 | `/tmp/a/~\tx.cs` | `tx.cs` | `~\tx.cs` → `tx.cs` | `<path>\tx.cs` |
| 9 | `\\/home/<user>\nx.cs` | `\\nx.cs` → `nx.cs` | `\\<path>\nx.cs` → `nx.cs` | `\\<path>\nx.cs` |
| 9 | `\\/opt/data/home/u`, `\\/cygdrive/c/Users/u` | LEAK | `\\<path>` → `<path>` | `\\<path>` |
| 10 | timing | see section 5 | | |
| 11 | `/d/users/<user>` | `<path>` | `<path>` | `<path>` |

### 3. New red rows (all use `AssertMissedRoot`: both forms, no `<user>` or `aabb`, stable second pass)

Put each criterion's failing rows first and the mutant rows after them, in the existing `GluedAndUncRoots`, `NonAsciiAccountFolders` and `EscapesAfterAPath` methods. Alternatively add three new data-set methods with a Pending and a characterisation theory each.

- **F** marks a row that fails on the green today (a red row).
- **C** marks a row that passes on the green (a mutant-killer or characterisation row).

| Crit | Input | Expected | |
|---|---|---|---|
| 435-1 | `gcc /tmp/a.c\n-I/home/<user>/include` | `gcc a.c\n-Iinclude` | F |
| 435-1 | `gcc /tmp/a.c\u0027-I/home/<user>/include\u0027` | `gcc a.c\u0027-Iinclude\u0027` | F |
| 435-1 | `gcc '-I/home/<user>/include'` | `gcc '-Iinclude'` | F |
| 435-1 | `["\\host\share\x.cs","C:\Users\<user>\y.cs"]` | `["x.cs","y.cs"]` | F |
| 435-1 | `\u0027\\\\host\\share\\x.cs\u0027,\u0027/home/<user>/y.cs\u0027` | `\u0027x.cs\u0027,\u0027y.cs\u0027` | F |
| 435-1 | `\\host\share\x.cs;C:\Users\<user>\y.cs` | `x.cs;y.cs` | F |
| 435-1 | `/d/users/<user>` | `<path>` | C (M2) |
| 435-1 | `gcc -L/home/<user>/lib` | `gcc -Llib` | C (M4) |
| 435-1 | `see a-b/tmp/x.cs` | unchanged | C (M5) |
| 435-1 | `\\host\c$\users\<user>` | `<path>` | C (M10) |
| 435-1 | `\\C:\\Users\\<user>\\x.cs` | `x.cs` | C (N13) |
| 435-2 | `C:\Users\<user>(1)D:\Users\<user>\x.cs` | `<path>x.cs` | F |
| 435-2 | `C:\Users\<user>(1)file:///C:/Users/<user>/x.cs` | `<path>x.cs` | F |
| 435-2 | `see C:\Users\<user>(bb).` | `see <path>.` | F |
| 435-2 | `C:\Users\<user> \u00f6cc` | `<path>` | C (M6) |
| 435-2 | `C:\Users\<user> bb'cc` | `<path>` | C (M7) |
| 435-2 | `see C:\Users\<user> x.cs: denied` | `see <path> x.cs: denied` | C (M8) |
| 435-2 | `C:\\Users\\<user> bb\rnext` | `<path>\rnext` | C (M9) |
| 435-2 | `C:\Users\<user>(bb)cc` | `<path>` | C (M11) |
| 435-3 | `C:\\a\\x.cs\b/home/<user>/y.cs` | `x.cs\by.cs` | F |
| 435-3 | `C:\\a\\x.cs\f/home/<user>/y.cs` | `x.cs\fy.cs` | F |
| 435-3 | `\u00f6/tmp/a\u00f6C:\\Users\\<user>` | `\u00f6a\u00f6<path>` | F |
| 435-3 | `\t/tmp/a.C:\\Users\\<user>` | `\ta.<path>` | F |
| 435-3 | `-I/tmp/a.C:\Users\<user>` | `-Ia.<path>` | F |
| 435-3 | `\u00f6/tmp/a\u00f6file://localhost/home/<user>/x.cs` | `\u00f6a\u00f6x.cs` | F |
| 435-3 | `/tmp/a/~\nx.cs` | `<path>\nx.cs` | F |
| 435-3 | `C:\\a\\~\nx.cs` | `<path>\nx.cs` | F |
| 435-3 | `\\/home/<user>\nx.cs` | `\\<path>\nx.cs` | F (pass 2) |
| 435-3 | `\\/opt/data/home/<user>` | `\\<path>` | F (pass 2) |
| 435-3 | `\\\\host\\share\\x.cs\n/home/<user>/y.cs` | `x.cs\ny.cs` | F |
| 435-3 | `\u0027\\host\share\x.cs\u0027/home/<user>/y.cs` | `\u0027x.cs\u0027y.cs` | F |
| 435-3 | `C:\\Users\\<user>\u003e` | `<path>\u003e` | C (M1) |
| 435-3 | `C:\\a\\x.cs\r/home/<user>/y.cs` | `x.cs\ry.cs` | C (M-EscBefore r) |
| 435-3 | `x\\n/tmp/a/y.cs` | unchanged | C (M-EscBefore `(?<!\\)`) |
| 435-3 | `\t\\C:\Users\<user>` | `\t<path>` | C (4c) |

That is 35 rows: 21 fail on the green, and all 35 pass on the design. Finding 10 gets no row, because D254 forbids wall-clock assertions; its timing evidence is in section 5.

### 4. Mutants (applied to the design's text; each run against its row in both forms)

| Mutant (on the design text) | Killing row | Under the mutant |
|---|---|---|
| M2 `/[a-z]/` → `/c/` | `/d/users/<user>` | `/d/users/<user>` (killed) |
| 1 EscLetter `(?!00[0-7])` → `(?!002)` | `C:\\Users\\<user>\u003e` | `<path>` (killed) |
| 2 EscBefore drop `r` | `…x.cs\r/home/<user>/y.cs` | leaks (killed) |
| 3 EscBefore drop `(?<!\\)` | `x\\n/tmp/a/y.cs` | `x\\ny.cs` (killed) |
| 4 Glued `[A-Za-z]` → `I` | `gcc -L/home/<user>/lib` | leaks (killed) |
| 5 Glued drop start guard | `see a-b/tmp/x.cs` | `see a-bx.cs` (killed) |
| 6 Word drop EscLetter | `C:\Users\<user> \u00f6cc` | `<path> \u00f6cc` (killed) |
| 7 Word drop apostrophe | `C:\Users\<user> bb'cc` | `<path> bb'cc` (killed) |
| 8 Stop drop `\.[\p{L}…]` | `see C:\Users\<user> x.cs: denied` | `see <path>.cs: denied` (killed) |
| 9 Stop `[nrtbf"]` → `[nt"]` | `C:\\Users\\<user> bb\rnext` | `<path> bb\rnext` (killed) |
| 10 UNC `(?i:users\|home)` case-sensitive | `\\host\c$\users\<user>` | `<user>` (killed) |
| 11 G drop the glued trailer | `C:\Users\<user>(bb)cc` | `<path>cc` (killed) |

Mutants of the new pieces, each killed by the row given:
- drive guard: `\t/tmp/a.C:\\…`
- `file` in the guard: the `\u00f6…file://` row
- UNC `Plain` widened: 7a
- UNC escape guard: `\\\\host…\n/home`
- its esc branch: `\u0027\\host…\u0027/home`
- UNC `<`: `\\/home\<user>`
- UNC `X:` folder: `\\C:\\Users`
- tail `:[\\/]`: `(1)D:`
- tail letter guard: `(1)file:` (it gives `<path>e:///x.cs`)
- Glued `\uXXXX`, quotes and EscBefore: the three 2-rows
- EscBefore `b` / `f`: the 3-rows
- `Apply` `~` rule: `/tmp/a/~\nx.cs`

All 27 are killed, and every killing row passes on the design.

### 5. Timings (min of 7 runs, ms)

| Shape | Length | Before | Green | Design |
|---|---|---|---|---|
| `\\;` ×6666 | 19,998 | 7.7 | 3713 | 8.3 |
| `\\,` ×6666 | 19,998 | 6.4 | 3517 | 6.6 |
| `\\a;` ×5000 | 20,000 | 6.6 | 2197 | 8.2 |
| `\\\\host\\share\\x.cs\n` ×870 | 20,010 | 1.3 | 353 | 6.2 |
| ×1740 | 40,020 | 2.7 | 1378 | 16.7 |
| `\\\\a$\n` ×4000 / ×8000 | 32k / 64k | 2.0 / 3.6 | 3386 / 12823 | 16.4 / 33.3 |
| `\u0027\\a` ×2000 | 18,000 | 4.4 | 628 | 6.5 |
| `\\h\s\x.cs;` ×1800 | 19,800 | 4.0 | 699 | 7.3 |
| `/-`, `/a` ×10000 | 20,000 | 6.7, 10.0 | 10.2, 14.9 | 11.2, 15.6 |
| `C:\Users\<user>(a)` ×1111 | 19,998 | 2.8 | 5.6 | 3.0 |
| 1 MB mixed / `/a` / `/-` | ~1M | 232 / 570 / 440 | 390 / 1031 / 671 | 362 / 948 / 573 |

Every shape is under 100 ms at 20,000 characters, and doubling the length doubles the time, so all are linear. About 15 other adversarial shapes run within 0.5x to 3x of the pre-card copy.

**Target not met for two shapes.** `\\\\host\\share\\x.cs\n` is about 4.8x and `\\\\a$\n` about 8x the pre-card copy. In absolute terms that is a few ms (6 to 16 ms).
- On the first shape, deleting the UNC prefix entirely only lowers the time from 8.1 to 6.8 ms. So part D is not the cause; the extra cost comes from the green's other lookbehinds (`EscBefore`/`Glued` on each branch) and part F.
- Fixing that would mean redesigning beyond this rework. The facilitator should decide whether 3x applies to these two linear, low-millisecond shapes or whether to ask for that work.

### 6. Regressions and generator sweep

**Data sets:** all 134 rows pass, and no output differs from the green in either form.

**Replica of `PathScrubGenerated`** (seed 435, 100,000 inputs):
- before: 31,534 leaks;
- green: 0 leaks, 0 regressions, 0 second-pass changes;
- design: 0 leaks, 0 regressions, 0 second-pass changes.

The design's output differs from the green on 51 inputs. All 51 are the finding-8 fix: `aa(bb).` now keeps its `.`.

**Extended sweep** (200,000 inputs; adds 18 befores, 4 roots and 17 afters drawn from the review's shapes):

| | Before | Green | Design |
|---|---|---|---|
| Leaks | 86,209 | 61,514 | 7,807 |
| Leaks where before had none | — | 21,109 | 160 |
| Second-pass changes | 5,415 | 17,322 | 2,463 |

- **The 160 regressions** are older-class bridging (MINOR finding 6), where a first path swallows a second root as folders, for example `-I/tmp/a.\\\\host\\c$\\Users\\…` and `\u00f6/tmp/a\u00f6/mnt/e/users/…`. None is a BLOCKING shape.
- **Second-pass changes where before was stable: 586.** All of them come from a bare `\\` placed before a rooted path, which leaves `\\name` (the older class in finding 9).
- **Over-scrub:** `\\C:\a\x.cs` now gives `<path>x.cs`, where both before and the green gave `C:\a\x.cs`, which a second pass changes. It is stable and leaks nothing.

The probe, including `Design.cs` with the paste-ready text, is in `<scratchpad>\probe435-arch`. Its modes are `data`, `blocking`, `newrows`, `mutants`, `time` and `gen`.
## Facilitator's rulings on the open points (2026-11-14)

1. Timing: the 3x target was the facilitator's own bound in the brief, set to catch blow-ups. The two shapes over it (`\\host\share\x.cs\n` about 4.8x, `\\a$\n` about 8x) are linear and stay at 6 to 17 ms per 20,000 to 64,000 characters. Accepted for this card; the finding-10 blow-up (seconds, quadratic) is fixed. Recorded for the retro.
2. The extended sweep's 160 leaks where the pre-card copy had none (first-path bridging into a second root, the older class of MINOR finding 6) and its 586 second-pass changes after a bare `\`: not fixed in this round (none is a BLOCKING shape; the green had 21,109 and 17,322). They go to the third review to judge, and to the follow-up card if it agrees.
3. Seats for the round: red by a fresh Claude context (sonnet declared: the 35 rows are given, mechanical test writing, mid tier); green by Cursor (default model declared: paste-ready text, mechanical); the third review by agy (gemini-4-argon-high: adversarial, strong tier; it wrote neither the tests nor the code).
