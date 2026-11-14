# Card 435 third review, raw (qa: agy, gemini-4-argon-high pinned), 2026-11-14 21:24:59Z to 21:55:44Z, exit 0, on the rework e374ab7; home paths scrubbed

VERDICT: FLAG

### Verification of Second-Review BLOCKING Inputs

Every literal BLOCKING input listed in `.mob/records/435/second-review.md` (Findings 1, 2a–2h, 3, 4a, 4b, 7, 8, 9a, 9b, 10's four shapes, and 11's Mutant 2 + 11 new-piece mutants) was replayed in an outside-repository `net8.0` probe linking `tools/Avowry.Bench/PathScrub.cs` (`e374ab7`) and `tests/Avowry.Acceptance.Tests/Support/PathScrubBefore435.cs`, in both `<user>` and stand-in (`alice` / `aabb`) forms:
- **Findings 1, 2a–2h, 3, 4a, 4b, 7, 8, 9a, 9b, 11:** All 21 red rework rows and their listed variants are fixed on pass 1 and settle in one pass (`Apply(Apply(x)) == Apply(x)`).
- However, the rework's new pieces (`(?=[^\\/\s<])` and `Plain+|[A-Za-z]:` in Part D, `(?<=\\u[0-9A-Fa-f]{4})` in `Glued`, `(?!(?:file|[A-Za-z]):[\\/])` in `FinalSegment`, `Word(?![\p{L}\p{N}_]|:[\\/])` in Part G, and `file == "~"` in `Apply`) introduce **new BLOCKING regressions, account-folder leaks, second-pass changes, a new quadratic $O(N^2)$ backtracking shape, and 8 surviving mutants on the lines changed by the rework**.

---

### Findings

1. **BLOCKING** — **Regression and account-folder leak (`new-with-this-round`): Part D's `(?=[^\\/\s<])` lookahead (line 31) rejects UNC paths starting with an angle-bracket placeholder (`\\<host>\...`)**
   - **Why:** To prevent a pass-1 `\\<path>` from matching as a UNC root on pass 2 (Finding 9b), line 31 changed `(?=[^\\/\s])` to `(?=[^\\/\s<])` (refusing any `<` after `\\` rather than `(?!<path>)`). Because `Segment` and `FinalSegment` explicitly support `<[A-Za-z]+>` placeholders, a UNC path starting with `\\<host>\...` now fails the UNC root lookahead completely and is left untouched, leaking `<user>` (`alice`).
   - **Exact inputs and outputs:**
     - Input: `\\<host>\Users\<user>` (and `\\<host>\Users\<user>`)
       - Output before (`PathScrubBefore435.Apply`): `<path>` (pass 2: `<path>`)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `\\<host>\Users\<user>` (`\\<host>\Users\<user>`) — **new-with-this-round** (regression against `PathScrubBefore435` on a line that was correct, and leaks `<user>` / `alice`).
     - Input: `\\<host>\Users\<user>\x.cs` and `\\<host>\share\x.cs` (and with `alice`)
       - Output before (`PathScrubBefore435.Apply`): `x.cs` (pass 2: `x.cs`)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `\\<host>\Users\<user>\x.cs` and `\\<host>\share\x.cs` — **new-with-this-round** (regression and account-folder leak).

2. **BLOCKING** — **Regression and account-folder leak (`new-with-this-round`): Part D (line 31) restricts UNC prefix folders to `Plain+`, which excludes `'`, `()`, `<...>`, and `EscLetter` that `Segment` allows**
   - **Why:** Line 31 narrowed the UNC prefix folder before `(?<home>(?i:users|home))` from `[^\\/\s]+` to `(?:" + Plain + @"+|[A-Za-z]:)` with `(?!\\(?:[nrtbf""]|u[0-9A-Fa-f]{4}))`. However, `Plain` (`[^\s\\/""'`|*?:;,()<>\[\]]`, line 21) excludes `'`, `(`, `)`, `<`, `>`, and `\`, whereas `Segment` (line 24) permits `(?<=[A-Za-z])'(?=[A-Za-z])`, `(`, `)`, `<[A-Za-z]+>`, and `EscLetter` (`\u00f6`), and Part D omits Part F's `(?!\\u(?!00[0-7])[0-9A-Fa-f]{4})` guard. When a UNC host or share before `Users` contains any of those characters, Part D fails to match `(?<home>Users)`, `(?<dir>)` consumes `host...\Users\` via `Segment` with `home.Success == false`, and `<user>` (`alice`) is returned as `(?<file>)`.
   - **Exact inputs and outputs:**
     - Input: `\\host(1)\Users\<user>` (and `\\host(1)\Users\<user>`)
       - Output before (`PathScrubBefore435.Apply`): `<path>` (pass 2: `<path>`)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `<user>` (`alice`) — **new-with-this-round** (regression against `PathScrubBefore435` on a line that was correct, and leaks `<user>` / `alice`).
     - Input: `\\host's\Users\<user>` (and `\\host's\Users\<user>`)
       - Output before (`PathScrubBefore435.Apply`): `<path>` (pass 2: `<path>`)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `<user>` (`alice`) — **new-with-this-round** (regression against `PathScrubBefore435` and leak).
     - Input: `\\a<b>\Users\<user>` (and `\\a<b>\Users\<user>`)
       - Output before (`PathScrubBefore435.Apply`): `<path>` (pass 2: `<path>`)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `<user>` (`alice`) — **new-with-this-round** (regression against `PathScrubBefore435` and leak).
     - Input: `\\\\h\u00f6st\\Users\\<user>`, `\\\\host\\sh\u00f6re\\Users\\<user>`, and `\u0027\\h\u00f6st\Users\<user>\u0027`
       - Output before (`PathScrubBefore435.Apply`): `<user>`, `<user>`, and `\u0027h\u00f6st\Users\<user>\u0027`
       - Output in first green (`547538f`): `<path>`, `<path>`, and `\u0027<path>\u0027`
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `<user>`, `<user>`, and `\u0027<user>\u0027` — **new-with-this-round** (regression against `547538f`, leaks `<user>` / `alice`).

3. **BLOCKING** — **Regression and account-folder leak (`new-with-this-round`): `Glued` (line 20) uses `(?<=\\u[0-9A-Fa-f]{4})` without capturing `(?<esc>)` and without excluding `EscLetter`**
   - **Why:**
     1. On lines 29, 31, and 33, the hex-escape lookbehind is `(?<=(?<esc>\\u[0-9A-Fa-f]{4}))`, which sets the `(?<esc>)` group so that single-backslash paths (`(?<sgl>)`) still stop at a closing `\u0027` or `\u0022` via `(?(sgl)(?(esc)(?!\\u[0-9A-Fa-f]{4})))` in Part D (line 31) and Part F (line 36). In `Glued` (line 20), the rework wrote `(?<=\\u[0-9A-Fa-f]{4})` **without** `(?<esc>)`. When a single-backslash drive or UNC path starts after `\u0027-I` or `\u0022-I`, `sgl` is true and `esc` is false, disabling the `\uXXXX` stop in both Part D and Part F.
     2. Because `(?<=\\u[0-9A-Fa-f]{4})` in `Glued` does not exclude non-ASCII `EscLetter` (`\u00f6`), a word-internal hyphen after `\u00f6` (`a\u00f6-b/tmp/x.cs`, compare rework characterisation row `see a-b/tmp/x.cs`) is treated as a `-b` flag.
   - **Exact inputs and outputs:**
     - Input: `\u0027-IC:\a\x.cs\u0027/home/<user>` (and with `alice`)
       - Output before (`PathScrubBefore435.Apply`): `\u0027-IC:\a\x.cs\u0027<path>` (pass 2 unchanged — no leak)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `\u0027-I<user>` (`\u0027-Ialice`) — **new-with-this-round** (regression against `PathScrubBefore435` and leaks `<user>` / `alice`).
     - Input: `\u0027-I\\host\share\x.cs\u0027/home/<user>/y.cs` (compare rework row `\u0027\\host\share\x.cs\u0027/home/<user>/y.cs` -> `\u0027x.cs\u0027y.cs`)
       - Output before (`PathScrubBefore435.Apply`): `\u0027-I\\host\share\x.cs\u0027y.cs` (pass 2 unchanged)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `\u0027-Iy.cs` — **new-with-this-round** (regression: Part D bridges across `\u0027` and destroys `\\host\share\x.cs\u0027`).
     - Input: `gcc \u0027-IC:\Users\<user>\include\u0027` and `gcc \u0027-IC:\Users\<user>\u0027`
       - Output before (`PathScrubBefore435.Apply`): unchanged
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `gcc \u0027-Iu0027` — **new-with-this-round** (swallows `\` of the closing `\u0027` as a path separator).
     - Input: `see a\u00f6-b/tmp/x.cs`
       - Output before (`PathScrubBefore435.Apply`): `see a\u00f6-b/tmp/x.cs` (pass 2 unchanged)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `see a\u00f6-bx.cs` — **new-with-this-round** (regression against `PathScrubBefore435`).

4. **BLOCKING** — **Second-pass changes and pass-1 account-folder leaks (`new-with-this-round` and `new-with-this-card`): `FinalSegment`'s `(?!(?:file|[A-Za-z]):[\\/])` guard (line 25) and `Apply`'s `file == "~"` check (line 49) fail idempotence on lines that were stable before**
   - **Why:**
     1. In `Apply` (line 49), `file == "~"` only checks exact equality with `"~"`. Because the tilde root lookbehind (line 32) is `(?<![\w.])~`, any `FinalSegment` ending in `~` after a character allowed in `FinalSegment` but outside `[\w.]` (such as `-`, `~`, `$`, `+`, `=`, `@`, `#`, `>`) is emitted as-is before `\n` or `\t` on pass 1 (`foo-~\nx.cs`, `~~\nx.cs`) and then matches the tilde branch on pass 2 (`foo-nx.cs`, `~nx.cs`).
     2. In `FinalSegment` (line 25), `(?!(?:file|[A-Za-z]):[\\/])` stops the first path before `D:\` or `file://`. When the first path is a bare account folder ending in `[A-Za-z0-9_]` before `D:\` (`C:\Users\<user>:\Users\<user>\x.cs`, `C:\Users\<user>_D:\Users\<user>\x.cs`) or ending in `.` before `file://` (`C:\Users\<user>.file://localhost/home/<user>/x.cs`), the first path is replaced by `<path>` on pass 1, while the second path fails its start lookbehind (`(?<![A-Za-z0-9_])` on line 29 or `(?<![A-Za-z0-9_.])` on line 33) against the original input string on pass 1 (leaking `alice` / `<user>`) and then matches after `<path>` on pass 2. Similarly, in `\/tmp/a/nC:\Users\<user>`, `/tmp/a/n` stops before `C:\` and becomes `n` on pass 1, forming `\nC:\Users\<user>` (leaking `<user>` on pass 1) which then matches `EscBefore` on pass 2 (`\n<path>`).
   - **Exact inputs and outputs:**
     - Input: `/tmp/a/foo-~\nx.cs`, `/tmp/a/~~\nx.cs`, and `C:\\a\\foo-~\nx.cs`
       - Output before (`PathScrubBefore435.Apply`): `nx.cs` (pass 2: `nx.cs` — stable)
       - Output now (pass 1): `foo-~\nx.cs`, `~~\nx.cs`, `foo-~\nx.cs`
       - Output now (pass 2): `foo-nx.cs`, `~nx.cs`, `foo-nx.cs` — **new-with-this-card** (second pass changes output on a line that was stable before).
     - Input: `C:\Users\<user>:\Users\<user>\x.cs` and `C:\Users\<user>_D:\Users\<user>\x.cs`
       - Output before (`PathScrubBefore435.Apply`): `<path>:\Users\<user>\x.cs` and `<path>:\Users\<user>\x.cs` (pass 2 unchanged — stable)
       - Output now (pass 1): `<path>D:\Users\<user>\x.cs` and `<path>D:\Users\<user>\x.cs` (**leaks `alice` / `<user>`**)
       - Output now (pass 2): `<path>x.cs` — **new-with-this-round** (pass-1 leak and pass-2 change on a line that was stable before).
     - Input: `C:\Users\<user>.file://localhost/home/<user>/x.cs` (and with `alice`)
       - Output before (`PathScrubBefore435.Apply`): `<path>://localhost/home/<user>/x.cs` (pass 2 unchanged — stable)
       - Output now (pass 1): `<path>file://localhost/home/<user>/x.cs` (**leaks `<user>`**)
       - Output now (pass 2): `<path>x.cs` — **new-with-this-round** (pass-1 leak and pass-2 change on a line that was stable before).
     - Input: `\/tmp/a/nC:\Users\<user>` (and with `alice`)
       - Output before (`PathScrubBefore435.Apply`): `\nC:\Users\<user>` (pass 2 unchanged — stable)
       - Output now (pass 1): `\nC:\Users\<user>` (**leaks `<user>`**)
       - Output now (pass 2): `\n<path>` — **new-with-this-round** (pass-1 leak and pass-2 change on a line that was stable before).
     - Input: `\t/tmp/a.file://localhost/home/<user>/x.cs` (compare rework row `\u00f6/tmp/a\u00f6file://localhost/home/<user>/x.cs` -> `\u00f6a\u00f6x.cs`)
       - Output before (`PathScrubBefore435.Apply`): `\t/tmp/a.file://localhost/home/<user>/x.cs`
       - Output now (pass 1 and pass 2): `\ta.file://localhost/home/<user>/x.cs` — **new-with-this-round** (leaks `<user>` because `FinalSegment` stops before `file://` after `.`, but line 33's `(?<![A-Za-z0-9_.])` rejects `.` before `file://`).

5. **BLOCKING** — **Regression in surrounding parentheses and partial account-folder leak (`new-with-this-round`): Part G (line 38) replaces `Plain*` after `)` with `Word(?![\p{L}\p{N}_]|:[\\/])`**
   - **Why:**
     1. In `547538f`, Part G matched `(?:\(" + Plain + @"*\)" + Plain + @"*)*`, where `Plain` excluded `(` and `)`. In `e374ab7`, `Plain*` after `)` was replaced with `(?:" + Word + @"(?![\p{L}\p{N}_]|:[\\/]))?`, and `Word` (`(?:[\p{L}\p{N}_()-]|...)+`, line 22) includes `(` and `)`. When an account folder with glued parentheses is enclosed in parentheses (`see (C:\Users\<user>(bb))`), `Word` consumes the outer closing parenthesis `)` into `<tail>`.
     2. When an account folder has glued word characters after `)` (`C:\Users\<user>(bb)cc`, rework row M11) followed immediately by `D:\...` or `file:///...`, `Word` greedily matches `ccD` or `ccfile`, fails `(?![\p{L}\p{N}_]|:[\\/])` at `:\` or `:/`, and cannot backtrack to `cc` because `D` and `f` are in `[\p{L}\p{N}_]`. Thus `(?:Word...)?` matches nothing after `(bb)`, leaking `cc` from the first account folder (and for `D:\`, also leaving `D` preceded by `c` so the second path leaks `<user>`).
   - **Exact inputs and outputs:**
     - Input: `see (C:\Users\<user>(bb))` and `see (C:\Users\<user>(bb)) here`
       - Output before (`PathScrubBefore435.Apply`): `see (<path>(bb))` and `see (<path>(bb)) here`
       - Output in first green (`547538f`): `see (<path>)`
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `see (<path>` — **new-with-this-round** (regression: swallows the outer closing parenthesis `)`).
     - Input: `C:\Users\<user>(bb)ccfile:///C:/Users/<user>/x.cs` and `C:\Users\<user>(bb)ccD:\Users\<user>\x.cs`
       - Output before (`PathScrubBefore435.Apply`): `<path>(bb)ccfile:///x.cs` and `<path>(bb)ccD:\Users\<user>\x.cs`
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `<path>ccfile:///x.cs` (**leaks `cc`**) and `<path>ccD:\Users\<user>\x.cs` (**leaks `cc` and `<user>`**) — **new-with-this-round**.

6. **BLOCKING** — **Super-linear / quadratic $O(N^2)$ backtracking on 20,000-character and 40,000-character lines (`new-with-this-round`): Part D's `|[A-Za-z]:` inside the `+` loop with `(?<sgl>)` scans to end-of-line on repeated `\\C:/a\n` or `\\C:\\a\n`**
   - **Why:** While the four quadratic shapes from the second review (`\\;`, `\\,`, `\\a;`, `\\\\host\\share\\x.cs\n`) are now linear (6–15 ms at 20,000 chars), the rework placed `|[A-Za-z]:` inside Part D's `+` loop: `(?:\\\\\\\\|(?<sgl>)\\\\)(?=[^\\/\s<])(?:(?:(?:" + Plain + @"+|[A-Za-z]:)(?(sgl)(?(esc)(?!\\u[0-9A-Fa-f]{4}))|(?!\\(?:[nrtbf""]|u[0-9A-Fa-f]{4})))" + Separator + @")+?(?<home>(?i:users|home))" + Separator + @")?` (line 31). When a UNC root starts with two backslashes `\\`, `(?<sgl>)` is true, which disables the `\n` escape stop in Part D! On repeated `\\C:/a\n` or `\\C:\\a\n`:
     1. At each `\\`, the UNC branch sets `sgl = true`, and Part D scans all remaining $O(N)$ copies of `C:/`, `a\`, `n\\` to the end of the line looking for `users|home`, fails, and falls back to matching only `\\` (because `Segment` and `FinalSegment` stop before `C:`).
     2. Next, `C:/a` (or `C:\\a`) matches the Drive branch with `sgl = false`, which stops at `\n`, leaving the next `\\C:/a\n` unconsumed to repeat the full $O(N)$ scan.
   - **Measured timings (outside-repo `net8.0` probe, warmed up):**
     | Input shape | Length | Before (`PathScrubBefore435`) | Now (`PathScrub`) | Status |
     |---|---:|---:|---:|---|
     | `string.Concat(Enumerable.Repeat(@"\\C:/a\n", 1_250))` | 10,000 | 6.2 ms | 695.1 ms | **BLOCKING ($O(N^2)$)** |
     | `string.Concat(Enumerable.Repeat(@"\\C:/a\n", 2_500))` | 20,000 | **7.3 ms** | **1,866.0 ms** | **BLOCKING (new-with-this-round, 256x blow-up)** |
     | `string.Concat(Enumerable.Repeat(@"\\C:/a\n", 5_000))` | 40,000 | **14.3 ms** | **10,888.6 ms** | **BLOCKING (new-with-this-round, 762x blow-up; 5.8x for 2x length)** |
     | `string.Concat(Enumerable.Repeat(@"\\C:\\a\n", 1_111))` | 9,999 | 5.1 ms | 514.7 ms | **BLOCKING ($O(N^2)$)** |
     | `string.Concat(Enumerable.Repeat(@"\\C:\\a\n", 2_222))` | 19,998 | **9.5 ms** | **2,585.4 ms** | **BLOCKING (new-with-this-round, 273x blow-up; 5.0x for 2x length)** |
     | `string.Concat(Enumerable.Repeat(@"\\;", 6_666))` | 19,998 | 7.5 ms | 9.1 ms | OK (fixed) |
     | `string.Concat(Enumerable.Repeat(@"\\,", 6_666))` | 19,998 | 9.2 ms | 8.7 ms | OK (fixed) |
     | `string.Concat(Enumerable.Repeat(@"\\a;", 5_000))` | 20,000 | 6.4 ms | 11.5 ms | OK (fixed) |
     | `string.Concat(Enumerable.Repeat(@"\\\\host\\share\\x.cs\n", 870))` | 20,010 | 3.0 ms | 9.2 ms | OK (fixed, linear) |
     | `string.Concat(Enumerable.Repeat(@"\\\\host\\share\\x.cs\n", 1_740))` | 40,020 | 3.6 ms | 15.1 ms | OK (linear) |
     | `string.Concat(Enumerable.Repeat(@"\\\\a$\n", 4_000))` | 32,000 | 2.3 ms | 19.2 ms | OK (linear, accepted by ruling 1) |
     | `string.Concat(Enumerable.Repeat(@"\\\\a$\n", 8_000))` | 64,000 | 4.7 ms | 48.5 ms | OK (linear, accepted by ruling 1) |
     | `string.Concat(Enumerable.Repeat(@"\u0027\\a", 2_000))` | 18,000 | 7.3 ms | 9.6 ms | OK |
     | `string.Concat(Enumerable.Repeat(@"\\h\s\x.cs;", 1_800))` | 19,800 | 8.2 ms | 11.6 ms | OK |
     | `string.Concat(Enumerable.Repeat(@"/-", 10_000))` | 20,000 | 9.9 ms | 15.9 ms | OK |
     | `string.Concat(Enumerable.Repeat(@"/a", 10_000))` | 20,000 | 14.2 ms | 23.6 ms | OK |
     | `string.Concat(Enumerable.Repeat(@"C:\Users\<user>(a)", 1_111))` | 19,998 | 5.3 ms | 5.3 ms | OK |
     | ~1 MB mixed (`40,000` paths) | 1,080,000 | 253.2 ms | 395.5 ms | OK |
     | ~1 MB `"/a"` (`524,288` repeats) | 1,048,576 | 614.9 ms | 1,014.5 ms | OK |
     | ~1 MB `"/-"` (`524,288` repeats) | 1,048,576 | 547.4 ms | 740.4 ms | OK |

7. **BLOCKING** — **8 surviving mutants on lines changed by the rework (`new-with-this-round`)**
   - **Why:** All 12 mutants from the second review (Mutant 2 plus the 11 new-constant/part mutants) and the 15 mutants in `slice-rework.md` are killed by the rework test rows. However, **8 mutants of the lines changed by the rework (`lines 20, 25, 31, 38` of `tools/Avowry.Bench/PathScrub.cs`) survive every test in `Slice=435` (112/112), `Slice=426` (30/30), and `Slice=418` (45/45)** — including dropping `"` from `Glued` on line 20 (`gcc "-I/home/<user>/include"` was listed in `slice-rework.md` section 2 but omitted from section 3's test rows). Full details and killing rows are in the **Mutants** section below.

8. **MINOR** — **Evaluation of the extended sweep's ~160 leaks where `PathScrubBefore435` had none and ~586 second-pass changes (`new-with-this-card` exposure of `older` bridging / bare-`\\` classes)**
   - **Why & judgement:** Reproducing the 200,000-input extended sweep in the outside-repository probe yields two groups left for this review by facilitator ruling 2:
     1. **The ~160–170 leaks where `PathScrubBefore435` had none:** Every one of these has a first path prefix that `PathScrubBefore435` either did not start (`-I/tmp/a.`, `\t/tmp/a.`) or stopped early (`\u00f6/tmp/a\u00f6`), followed immediately by a bare Unix or UNC account-folder root (`/home/<user>`, `/mnt/e/users/<user>`, `\\host\Users\<user>`, `\\host\c$\Users\<user>`, `\\wsl$\Ubuntu\home\<user>`). Because `Separator` (`[\\/]+`) and `Segment` have no root stop for `/` or `\\` (unlike `:` in `(?!(?:file|[A-Za-z]):[\\/])`), the first path bridges across the second path's root as `(?<dir>)` folders (`home.Success == false`, `dir.Captures.Count > 0`) and emits the bare account folder as `(?<file>)`. When the first path starts after a space (`see /tmp/a.\\host\Users\<user>` or `see /tmp/a/home/<user>`), `PathScrubBefore435` already leaked `<user>` by the exact same bridging mechanism (Finding 6, ruled **MINOR** for the follow-up card).
     2. **The ~513–586 second-pass changes where `PathScrubBefore435` was stable:** Every one of these places a bare `\\` before a newly supported root with a file segment (`\\/opt/data/home/<user>/x.cs`, `\\/cygdrive/c/Users/<user>/x.cs`). `PathScrubBefore435` left those roots untouched on both passes (leaking `<user>`), whereas `PathScrub` scrubs the path to `\\x.cs` on pass 1 (no leak) and `x.cs` on pass 2 — the exact bare-`\\` class of `\\/home/<user>/x.cs` in `PathScrubBefore435` (Finding 9, ruled **MINOR**).
   - **Exact sample inputs and outputs from the extended sweep:**
     - Input: `-I/tmp/a.\\host\c$\Users\<user>\nnext` and `\t/tmp/a.\\host\Users\<user>`
       - Output before (`PathScrubBefore435.Apply`): `-I/tmp/a.nnext` and `\t/tmp/a.<path>` (pass 2 unchanged)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `-Iaabb\nnext` and `\t<user>` — **older bridging class exposed by new starts (MINOR per facilitator ruling 2)**.
     - Input: `\u00f6/tmp/a\u00f6\\wsl$\Ubuntu\home\aabb\tnext` and `\u00f6/tmp/a\u00f6/home/<user>`
       - Output before (`PathScrubBefore435.Apply`): `\u00f6a\u00f6tnext` and `\u00f6a\u00f6<path>` (pass 2 unchanged)
       - Output now (`PathScrub.Apply`, pass 1 and pass 2): `\u00f6aabb\tnext` and `\u00f6<user>` — **older bridging class exposed by `EscLetter` (MINOR per facilitator ruling 2)**.
     - Input: `\\/opt/data/home/<user>/src/x.cs` and `\\/cygdrive/c/Users/aabb/x.cs\tnext`
       - Output before (`PathScrubBefore435.Apply`): unchanged on pass 1 and pass 2 (leaks `<user>` / `aabb`)
       - Output now (pass 1): `\\x.cs` and `\\x.cs\tnext` (no leak)
       - Output now (pass 2): `<path>` and `tnext` — **older bare-`\\` class on newly scrubbed roots (MINOR per facilitator ruling 2)**.

---

### Mutants

#### A. Previously reported mutants (all 12 from the second review + 15 from `slice-rework.md`)
All 27 mutants from the second review and `slice-rework.md` (including Mutant 2 `/[a-z]/` -> `/c/`, `EscLetter` `(?!00[0-7])` -> `(?!002)`, `EscBefore` dropping `r`/`b`/`f`/`(?<!\\)`, `Glued` `[A-Za-z]` -> `I` and dropping start guard, `Word` dropping `EscLetter` or apostrophe, `Stop` dropping `\.[\p{L}\p{N}_]` or `r`/`b`/`f`, Part D case-sensitive `Users|home`, Part G dropping the glued trailer, and `Apply` dropping `file == "~"`) were verified **KILLED** by `Slice=435`.

#### B. 8 surviving mutants on lines changed by the rework (`tools/Avowry.Bench/PathScrub.cs`)
Each mutant below was applied to `tools/Avowry.Bench/PathScrub.cs`, built with `dotnet build Avowry.sln -c Debug --nologo`, and tested against `Slice=435` (112 tests) and `Criterion=426-1|Criterion=426-2|Criterion=426-4|Criterion=418-U` (61 tests, plus verified against all `Slice=426` and `Slice=418` rows); **all 8 SURVIVE (0 failures)**:

1. **M-R1 — Line 20 (`Glued`):** `(?<![^\s""'])` -> `(?<![^\s'])` (dropping `""` from the quote start guard) — **SURVIVES** (`Slice=435`: 112/112 pass; `426`/`418`: 61/61 pass).
   - **Killing row:** `R(@"gcc ""-I/home/<user>/include""", @"gcc ""-Iinclude""")` (green returns `gcc "-Iinclude"`; mutant returns `gcc "-I/home/<user>/include"`, leaking `<user>`).
2. **M-R2 — Line 20 (`Glued`):** `"|" + EscBefore + "` -> `"|(?<=\\\\n)"` (narrowing `EscBefore` inside `Glued` to `\n` only) — **SURVIVES** (`Slice=435`: 112/112 pass; `426`/`418`: 61/61 pass).
   - **Killing row:** `R(@"gcc /tmp/a.c\t-I/home/<user>/include", @"gcc a.c\t-Iinclude")` (green returns `gcc a.c\t-Iinclude`; mutant returns `gcc a.c\t-I/home/<user>/include`, leaking `<user>`).
3. **M-R3 — Line 20 (`Glued`):** `(?<=\\u[0-9A-Fa-f]{4})` -> `(?<=u[0-9A-Fa-f]{4})` (dropping `\\` before `u` in `Glued`'s hex-escape lookbehind) — **SURVIVES** (`Slice=435`: 112/112 pass; `426`/`418`: 61/61 pass).
   - **Killing row:** `R(@"id u0041-I/tmp/a/x.cs", @"id u0041-I/tmp/a/x.cs")` (green leaves unchanged; mutant returns `id u0041-Ix.cs`).
4. **M-R4 — Line 25 (`FinalSegment`):** `(?!(?:file|[A-Za-z]):[\\/])` -> `(?!(?:file|C):[\\/])` (narrowing `[A-Za-z]` to `C`, or to `[A-Z]`) — **SURVIVES** (`Slice=435`: 112/112 pass; `426`/`418`: 61/61 pass).
   - **Killing row:** `R(@"\t/tmp/a.d:\Users\<user>", @"\ta.<path>")` (green returns `\ta.<path>`; mutant returns `\ta.d:\Users\<user>`, leaking `<user>` on pass 1 and changing on pass 2).
5. **M-R5 — Line 25 (`FinalSegment`):** `(?!(?:file|[A-Za-z]):[\\/])` -> `(?!(?:file:/|[A-Za-z]:\\))` (dropping `/` after `[A-Za-z]:`) — **SURVIVES** (`Slice=435`: 112/112 pass; `426`/`418`: 61/61 pass).
   - **Killing row:** `R(@"\t/tmp/a.C:/Users/<user>", @"\ta.<path>")` (green returns `\ta.<path>`; mutant returns `\ta.C:<path>`).
6. **M-R6 — Line 31 (Part D, UNC branch):** `(?:" + Plain + @"+|[A-Za-z]:)` -> `(?:" + Plain + @"+|C:)` (narrowing `[A-Za-z]:` to `C:`, or to `[A-Z]:`) — **SURVIVES** (`Slice=435`: 112/112 pass; `426`/`418`: 61/61 pass).
   - **Killing row:** `R(@"\\d:\\Users\\<user>\\x.cs", "x.cs")` (green returns `x.cs`; mutant returns `<path>x.cs`).
7. **M-R7 — Line 31 (Part D, UNC branch):** `(?!\\(?:[nrtbf""]|u[0-9A-Fa-f]{4}))` -> `(?!\\n)` (narrowing the non-`sgl` escape stop in Part D to `\n` only) — **SURVIVES** (`Slice=435`: 112/112 pass; `426`/`418`: 61/61 pass).
   - **Killing row:** `R(@"\\\\host\\share\\x.cs\r/home/<user>/y.cs", @"x.cs\ry.cs")` (green returns `x.cs\ry.cs`; mutant returns `y.cs`, swallowing `x.cs\r`).
8. **M-R8 — Line 38 (Part G, tail):** `(?![\p{L}\p{N}_]|:[\\/])` -> `(?![a-z]|:[\\/])` (narrowing `[\p{L}\p{N}_]` to `[a-z]`) — **SURVIVES** (`Slice=435`: 112/112 pass; `426`/`418`: 61/61 pass).
   - **Killing row:** `R(@"C:\Users\<user>(1)-D:\Users\<user>\x.cs", "<path>-x.cs")` (green returns `<path>-x.cs`; mutant swallows `-` into `<tail>` and returns `<path>x.cs`).

---

### Commands Run and Summary Lines

All temporary mutant edits to `tools/Avowry.Bench/PathScrub.cs` were restored byte-for-byte (`git diff --stat` and `git status -s` empty at completion).

1. `dotnet build Avowry.sln -c Debug`
   - `Build succeeded. 0 Warning(s) 0 Error(s) Time Elapsed 00:00:06.77`
2. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435"`
   - `Passed!  - Failed:     0, Passed:   112, Skipped:     0, Total:   112, Duration: 4 s - Avowry.Acceptance.Tests.dll (net8.0)`
3. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=426"`
   - `Passed!  - Failed:     0, Passed:    30, Skipped:     0, Total:    30, Duration: 26 s - Avowry.Acceptance.Tests.dll (net8.0)`
4. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=418"`
   - `Passed!  - Failed:     0, Passed:    45, Skipped:     0, Total:    45, Duration: 1 m - Avowry.Acceptance.Tests.dll (net8.0)`
5. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-1"`
   - `Passed!  - Failed:     0, Passed:    30, Skipped:     0, Total:    30, Duration: 225 ms - Avowry.Acceptance.Tests.dll (net8.0)`
6. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-2"`
   - `Passed!  - Failed:     0, Passed:    57, Skipped:     0, Total:    57, Duration: 57 ms - Avowry.Acceptance.Tests.dll (net8.0)`
7. `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Criterion=435-3" --logger "console;verbosity=detailed"`
   - `inputs=100000 leaks_base=31534 leaks_new=0 regressions=0 second_pass=0 exclusions=0`
   - `Test Run Successful. Total tests: 25 Passed: 25 Total time: 4.0604 Seconds`
8. Mutant runs (`M-R1` through `M-R8`, each built with `dotnet build Avowry.sln -c Debug --nologo` -> `0 Error(s)` and tested with `dotnet test --no-build -c Debug`):
   - `M-R1` through `M-R8` on `--filter "Slice=435"`: `Passed!  - Failed:     0, Passed:   112, Skipped:     0, Total:   112, Duration: 3-4 s - Avowry.Acceptance.Tests.dll (net8.0)`
   - `M-R1` through `M-R8` on `--filter "Criterion=426-1|Criterion=426-2|Criterion=426-4|Criterion=418-U"`: `Passed!  - Failed:     0, Passed:    61, Skipped:     0, Total:    61, Duration: 40-50 ms - Avowry.Acceptance.Tests.dll (net8.0)`
9. Post-restore verification (`git checkout -- tools/Avowry.Bench/PathScrub.cs`, `git diff --stat`, `git status -s`, `dotnet build Avowry.sln -c Debug`, `Slice=435`):
   - `git diff --stat`: empty
   - `git status -s`: empty
   - `Build succeeded. 0 Warning(s) 0 Error(s)`
   - `Passed!  - Failed:     0, Passed:   112, Skipped:     0, Total:   112, Duration: 3 s - Avowry.Acceptance.Tests.dll (net8.0)`
