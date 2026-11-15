# Card 439 review round 1, raw (QA: agy, gemini-4-argon-high, pinned; 02:35:47Z to 03:23:54Z, exit 0; read-only at 1ae9d03); home paths scrubbed

VERDICT: FLAG
new BLOCKING: 8; new MINOR: 2

### Findings

1. **BLOCKING — Super-linear $O(N^2)$ time on failed Unix walks containing glued flags (`/=-I` and `/(-I`)**
   - **Real source of shape**: Compiler/linker command lines, Makefile variables, and configuration fragments with `=` or `(` before `-I`/`-L` path flags (`-I/dir`, `FLAGS=-I/...`). Under Decider D259 clause 4 and card 439 criterion 3, any super-linear time is `BLOCKING`.
   - **Root cause**: [`slice-architect.md`](file:///<home>/projects/review439-clone/.mob/records/439/slice-architect.md#L7) relied on prefix folders containing only `[A-Za-z0-9_]` so no Unix root could start inside a failed Unix walk. In [`PathScrub.TryUnix`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L235-L286), [`IsPrefixFolder`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L764-L776) instead allows any non-whitespace, non-`NameStop` characters ending in a word character — which permits `=`, `(`, and `-` (`=-I`, `(-I`). At every `/` (`p = 0, 4, 8, ...`), [`AllowsStart`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L687-L707) returns `true` via [`IsGluedFlagStart`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L723-L742), and `TryUnix` walks all remaining `/=-I` or `/(-I` segments to the end of the string before returning `false`. [`PathScrub.Apply`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L31-L37) then runs `Scan` a second time, doubling the quadratic work.
   - **Exact input**: `string.Concat(Enumerable.Repeat("/=-I", 5000))` (20,000 chars) and `string.Concat(Enumerable.Repeat("/(-I", 5000))` (20,000 chars)
   - **Replaced regex (`7ee5411`)**: unchanged (`20.22 ms` on `/=-I` at 20,000 chars; `16.32 ms` on `/(-I` at 20,000 chars)
   - **Pre-435 copy (`PathScrubBefore435`)**: unchanged (`9.86 ms` on `/=-I` at 20,000 chars; `14.33 ms` on `/(-I` at 20,000 chars)
   - **Scanner (`PathScrub.Apply`)**:
     - Pass 1: unchanged
     - Pass 2: unchanged
     - Measured time on `/=-I`: **`305.8 ms` at 5,000 chars, `1,079.0 ms` at 10,000 chars (3.5x), `4,691.7 ms`–`6,396.5 ms` at 20,000 chars (~16x the 5k time; >93x over the 50 ms budget), and `23,308.9 ms` at 40,000 chars** (extrapolates to ~4 hours at 1,000,000 chars).
     - Measured time on `/(-I`: **`309.7 ms` at 5,000 chars, `1,275.2 ms` at 10,000 chars (4.1x), `4,805.3 ms` at 20,000 chars (15.5x the 5k time), and `19,046.5 ms` at 40,000 chars**.

2. **BLOCKING — Super-linear $O(N^2)$ time on whitespace runs after any path root (`ReadFolderCandidate` calling `IsFlagBoundaryAhead`)**
   - **Real source of shape**: Column-aligned or space-padded compiler/tool tables and logs after a rooted path. Under Decider D259 clause 4, any super-linear time is `BLOCKING`.
   - **Root cause**: In [`PathScrub.ReadFolderCandidate`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L597-L598), at **every** whitespace character `p`, the scanner calls [`IsFlagBoundaryAhead(text, p)`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L778-L791), which scans `while (i < text.Length && char.IsWhiteSpace(text[i])) i++;` to the end of the whitespace run. A run of $N$ spaces after any root (`C:\a` or `/tmp/a`) performs $N(N+1)/2$ character reads per `Scan` pass (twice per `Apply`).
   - **Exact input**: `@"C:\a" + new string(' ', 19996)` (20,000 chars)
   - **Replaced regex (`7ee5411`)**: `"a" + new string(' ', 19996)` (`9.30 ms` at 20,000 chars)
   - **Pre-435 copy (`PathScrubBefore435`)**: `"a" + new string(' ', 19996)` (`6.09 ms` at 20,000 chars)
   - **Scanner (`PathScrub.Apply`)**:
     - Pass 1: `"a" + new string(' ', 19996)`
     - Pass 2: `"a" + new string(' ', 19996)`
     - Measured time: **`121.18 ms` at 5,000 chars, `481.67 ms` at 10,000 chars (4.0x), and `1,809.56 ms` at 20,000 chars (14.9x the 5k time)** (extrapolates to ~75 minutes at 1,000,000 chars).

3. **BLOCKING — Regressions on 543 inputs of the Card 435 Generator Corpus (Sweep A), doubled-backslash Windows drive paths, mixed-slash compiler diagnostics,Nested `Users` folders, and non-plain UNC filenames**
   - **Real source of shape**:
     1. **543 inputs of card 435's committed generator corpus (`PathScrubGenerated`, seed 435, Sweep A)** (violates card 439 criterion 2: *"Given ... card 435's generator (100,000 inputs) ... the outputs equal today's on every row the rework handles correctly"*). Sweep A's assertion missed these 543 regressions because [`PathScrubGenerated`](file:///<home>/projects/review439-clone/tests/Avowry.Acceptance.Tests/PathScrubGenerated.cs#L147-L163) only checks `!HoldsTokenOrHalf` and not whether the trailing filename `y.cs` was destroyed.
     2. Windows paths with a doubled backslash from joining `$(UserProfile)\` (`C:\Users\\<user>\x.cs`, `C:\\Users\<user>\x.cs`).
     3. MSBuild / CMake / Roslyn diagnostics mixing `/` and `\` on Windows (`C:/Users/<user>\Candidate.cs(7,1): error CS1529: planted`, `C:\Users\<user>/Candidate.cs(7,1): error CS1529: planted`).
     4. A project under `C:\Users\<user>` containing a `Users` subfolder (`C:\Users\<user>\src\Users\UsersController.cs` — named as a real shape in [`slice-architect.md` line 153](file:///<home>/projects/review439-clone/.mob/records/439/slice-architect.md#L153)).
     5. A UNC path under `Users`/`home` whose filename has an apostrophe, `+`, `~`, or non-ASCII escape (`\\host\Users\<user>\O'Brien.cs end`).
   - **Root cause**: Four separate over-broad conditions in [`PathScrub.IsAccountFolder`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L341-L394):
     - Lines 379–393 return `true` whenever *any* earlier folder `dirs[m]` is in `AccountParents` after or at `seps[n] != root.ChainSep`, **without checking whether `last` (`dirs[^1]`) is the account parent**, turning the final filename `y.cs` / `x.cs` into `<path>`.
     - Lines 343–350 return `true` whenever `InSet(dirs[0], DriveParents)` and *any* separator in `seps` differs in slash direction from `root.ChainSep` (`!SameSepShape`), turning the filename of any mixed-slash drive path into `<path>`.
     - Line 351 (`if (InSet(dirs[0], DriveParents) && InSet(last, DriveParents)) return true;`) fires at any depth when both `dirs[0]` and `dirs[^1]` are `"Users"`, instead of checking `dirs.Count == 1`.
     - Lines 358–368 (`if (anyParent && !IsPlainShortName(file)) return true;`) treat any non-plain filename under a UNC `Users`/`home` share as an account folder, replacing the filename with `<path>` and invoking `ReadTail` to swallow following prose words.
   - **Exact inputs & outputs**:
     - Sweep A corpus input (`#60`): `\tC:\Users\<user>\n/home/<user>/y.cs` (and `#132`: `in /tmp/x.cs at C:\Users\<user>\src\x.cs\n/home/aaöbb/y.cs`)
       - Replaced regex (`7ee5411`): `\ty.cs` (`in x.cs at y.cs`)
       - Pre-435 copy (`PathScrubBefore435`): `\ty.cs` (`in x.cs at y.cs`)
       - Scanner (`PathScrub.Apply`): pass 1 = `\t<path>` (`in x.cs at <path>`), pass 2 = `\t<path>` (`in x.cs at <path>`)
     - Doubled backslash in Windows drive path: `C:\Users\\<user>\x.cs` (and `C:\\Users\<user>\x.cs`)
       - Replaced regex (`7ee5411`): `x.cs`
       - Pre-435 copy (`PathScrubBefore435`): `x.cs`
       - Scanner (`PathScrub.Apply`): pass 1 = `<path>`, pass 2 = `<path>`
     - Mixed-slash compiler diagnostic: `C:/Users/<user>\Candidate.cs(7,1): error CS1529: planted` (and `C:\Users\<user>/Candidate.cs(7,1): error CS1529: planted`)
       - Replaced regex (`7ee5411`): `Candidate.cs(7,1): error CS1529: planted`
       - Pre-435 copy (`PathScrubBefore435`): `Candidate.cs(7,1): error CS1529: planted`
       - Scanner (`PathScrub.Apply`): pass 1 = `<path>(7,1): error CS1529: planted`, pass 2 = `<path>(7,1): error CS1529: planted`
     - Nested `Users` folder under user profile: `C:\Users\<user>\src\Users\UsersController.cs`
       - Replaced regex (`7ee5411`): `UsersController.cs`
       - Pre-435 copy (`PathScrubBefore435`): `UsersController.cs`
       - Scanner (`PathScrub.Apply`): pass 1 = `<path>`, pass 2 = `<path>`
     - UNC path with apostrophe in filename: `\\host\Users\<user>\O'Brien.cs end` (and `\\host\Users\<user>\a+b.cs end`)
       - Replaced regex (`7ee5411`): `O'Brien.cs end` (`a+b.cs end`)
       - Pre-435 copy (`PathScrubBefore435`): `O'Brien.cs end` (`a+b.cs end`)
       - Scanner (`PathScrub.Apply`): pass 1 = `<path>`, pass 2 = `<path>`

4. **BLOCKING — Account name LEAK and regression on mixed-slash paths when the account folder or subfolder starts with `n`, `r`, `t`, `b`, `f`, or `uXXXX` (`C:/Users\<user>\x.cs`)**
   - **Real source of shape**: CMake, Git Bash, or MSBuild tools on Windows joining a forward-slash root (`C:/Users` or `file:///C:/Users` or `/home/<user>`) with a backslash relative path (`\riley\x.cs`, `\notes\run-426`) where the account name or folder starts with `r`, `n`, `t`, `b`, or `f` (including the card's own persona `riley`, `runner`, `root`, `nate`, `bob`, `frank`).
   - **Root cause**: [`ReadDriveOrTildeSeparator`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L654-L674) and [`TryUnix`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L290) set `sgl = false` whenever the first separator after the root is `/`. In [`FolderPassesGuard`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L642-L646), when `!root.Sgl`, any single `\` before `n`, `r`, `t`, `b`, `f` (`IsE1`) or `uXXXX` (`IsUnicodeEscape`) rejects the folder segment. On `C:/Users\<user>\x.cs`, `\r` causes `"Users"` to fail `FolderPassesGuard`, so the scanner treats `"Users"` as the final file name and **leaves `\riley\x.cs` completely unscrubbed, leaking the account name `riley`**.
   - **Exact inputs & outputs**:
     - Input 1: `C:/Users\<user>\x.cs` (and `file:///C:/Users\<user>\x.cs`, `C:/Users\<user>\x.cs`)
       - Replaced regex (`7ee5411`): `x.cs`
       - Pre-435 copy (`PathScrubBefore435`): `x.cs`
       - Scanner (`PathScrub.Apply`): pass 1 = `Users\riley\x.cs` (`Users\nate\x.cs`), pass 2 = `Users\riley\x.cs` (`Users\nate\x.cs`) (**LEAKS account name!**)
     - Input 2: `/home/<user>\notes\run-426` (and `/home/<user>\foo.cs`)
       - Replaced regex (`7ee5411`): `<path>\notes\run-426` (`<path>\foo.cs`)
       - Pre-435 copy (`PathScrubBefore435`): `run-426` (`foo.cs`)
       - Scanner (`PathScrub.Apply`): pass 1 = `<path>\notes\run-426` (`<path>\foo.cs`), pass 2 = `<path>\notes\run-426` (`<path>\foo.cs`)

5. **BLOCKING — Account name LEAK and regression vs `7ee5411` on multi-segment or 1-character prefix folders before `/c/users/<user>` and `/mnt/c/users/<user>`**
   - **Real source of shape**: WSL2 shared mounts (`/mnt/wsl/c/users/<user>/x.cs`), nested mount points (`/opt/cygdrive/c/users/<user>/x.cs`), and single-letter mount prefixes (`/a/c/users/<user>`).
   - **Root cause**: In [`PathScrub.TryUnix`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L258-L271), the green implementation added `&& (w == rootAt || IsPrefixFolder(text, rootAt + 1, w - 1))` to the `/x/users/` and `/mnt/x/users/` checks. When there are two or more prefix folders (`/mnt/wsl/c/users/<user>`), `text[rootAt + 1 .. w - 1]` spans across `'/'`, which is in `IsNameStop`, so `IsPrefixFolder` returns `false`. When there is a 1-character prefix folder (`/a/c/users/<user>`), `rootAt + 1 == w - 1`, so `IsPrefixFolder` returns `false` (`start >= end`). In both cases `TryUnix` fails and leaks the entire path and account name.
   - **Exact inputs & outputs**:
     - Input 1: `/mnt/wsl/c/users/<user>/x.cs` (and `/mnt/wsl/c/users/aabb/x.cs`)
       - Replaced regex (`7ee5411`): `x.cs`
       - Pre-435 copy (`PathScrubBefore435`): `/mnt/wsl/c/users/<user>/x.cs`
       - Scanner (`PathScrub.Apply`): pass 1 = `/mnt/wsl/c/users/<user>/x.cs`, pass 2 = `/mnt/wsl/c/users/<user>/x.cs` (**LEAKS `<user>` / `aabb`!**)
     - Input 2: `/opt/cygdrive/c/users/<user>/x.cs`
       - Replaced regex (`7ee5411`): `x.cs`
       - Pre-435 copy (`PathScrubBefore435`): `/opt/cygdrive/c/users/<user>/x.cs`
       - Scanner (`PathScrub.Apply`): pass 1 = `/opt/cygdrive/c/users/<user>/x.cs`, pass 2 = `/opt/cygdrive/c/users/<user>/x.cs` (**LEAKS `<user>` / `aabb`!**)
     - Input 3: `/a/c/users/<user>`
       - Replaced regex (`7ee5411`): `<path>`
       - Pre-435 copy (`PathScrubBefore435`): `/a/c/users/<user>`
       - Scanner (`PathScrub.Apply`): pass 1 = `/a/c/users/<user>`, pass 2 = `/a/c/users/<user>` (**LEAKS `<user>` / `aabb`!**)

6. **BLOCKING — Account name LEAK on bridged `(/home/<user>)` after forward-slash Drive or Tilde paths (`see C:/temp/x.cs (/home/<user>)`)**
   - **Real source of shape**: Diagnostic prose naming a file path followed by a parenthesized home directory, identical to card 439 row `2.6e` (`see /tmp/x.cs (/home/<user>)`) but with a forward-slash Windows drive path (`C:/temp/x.cs`) or tilde path (`~/a.cs`) as the first path.
   - **Root cause**: When the first path is `RootKind.Drive` or `RootKind.Tilde` with `/` separators (`root.ChainSep == "/"`), `ReadFolderCandidate` bridges across `"x.cs ("` into `/home/<user>` with separator `"/"` (`seps[n] == root.ChainSep`). Because `seps[n] == root.ChainSep`, the bridged-chain check in [`IsAccountFolder`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L379-L393) does not trigger, and line 371 only checks `last == "home" || last == "Users"` when `kind == RootKind.Unix`. `IsAccountFolder` returns `false`, outputting the raw account name `<user>` (`aabb`) as if it were a filename.
   - **Exact input**: `see C:/temp/x.cs (/home/<user>)` (and `see ~/a.cs (/home/<user>)`, plus `aabb` forms)
   - **Replaced regex (`7ee5411`)**: `see <user>)` (`see aabb)`)
   - **Pre-435 copy (`PathScrubBefore435`)**: `see <user>)` (`see aabb)`)
   - **Scanner (`PathScrub.Apply`)**:
     - Pass 1: `see <user>)` (`see aabb)`) (**LEAKS `<user>` / `aabb`!**)
     - Pass 2: `see <user>)` (`see aabb)`)

7. **BLOCKING — Regressions in JSON logs and 157 Sweep B corpus rows from `TryUnc` unrestricted host characters, `AllowsTildeStart` allowing `>`, and `Apply` running `Scan` twice**
   - **Real source of shape**: JSON logs with an escaped regex pattern (`"\\d+"`) before a path field, and **157 inputs in card 439's own Sweep B generator corpus** (`PathScan439Generated`, seed 439).
   - **Root cause**:
     1. [`TryUnc`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L164-L182) reads `host` with `while (!IsSeparator(text[i]) && !char.IsWhiteSpace(text[i]))`, allowing `"`, `,`, `:`, `;`, `(`, `)`, `[`, and `]` inside `host` (violating line 14's header comment), checks only `host == "<path>"` instead of `StartsWith(host, "<path>")`, and does not require at least one folder (`dirs.Count > 0`, violating [`slice-architect.md` line 39](file:///<home>/projects/review439-clone/.mob/records/439/slice-architect.md#L39)).
     2. [`AllowsTildeStart`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L714) includes `'>'`, violating [`slice-architect.md` line 18](file:///<home>/projects/review439-clone/.mob/records/439/slice-architect.md#L18) (`not '>', so '<path>~' stays stable`).
     3. To mask the resulting second-pass changes in Sweep B, [`PathScrub.Apply`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L35-L36) runs `Scan` **twice**. Consequently, on 157 Sweep B inputs where `Scan` pass 1 outputs `\\<path>(7,1)\n<path>`, `\\<path>(7,1)\u0022`, or `[<path>~\nx.cs`, the hidden second `Scan` inside `Apply` matches `\\<path>(7,1)\...` as a UNC root or `~\nx.cs` after `<path>` as a tilde root and corrupts the output.
   - **Exact inputs & outputs**:
     - JSON log with regex and path: `{"pattern":"\\d+","path":"/home/<user>"}` (and `{"pattern":"\\d+","path":"/tmp/x.cs"}`)
       - Replaced regex (`7ee5411`): `{"pattern":"d+","path":"<path>"}` (`{"pattern":"d+","path":"x.cs"}`)
       - Pre-435 copy (`PathScrubBefore435`): `{"pattern":"d+","path":"<path>"}` (`{"pattern":"d+","path":"x.cs"}`)
       - Scanner (`PathScrub.Apply`): pass 1 = `{"pattern":"<path>"}` (`{"pattern":"x.cs"}`), pass 2 = `{"pattern":"<path>"}` (`{"pattern":"x.cs"}`)
     - Sweep B corpus input (`#3388`): `\\/mnt/e/users/<user>/src/Candidate.cs(7,1)\u0022` (and `\\/home/<user>/Candidate.cs(7,1)\n/home/<user>`)
       - Replaced regex (`7ee5411`): `\\Candidate.cs(7,1)\u0022` (`\\Candidate.cs(7,1)\n<path>`)
       - Pre-435 copy (`PathScrubBefore435`): `\\Candidate.cs(7,1)\u0022` (`\\Candidate.cs(7,1)\n/home/<user>`)
       - Scanner (`PathScrub.Apply`): pass 1 = `u0022` (`n<path>`) [`Scan` 1 produces `\\<path>(7,1)\u0022` and `\\<path>(7,1)\n<path>`; `Scan` 2 corrupts them to `u0022` and `n<path>`], pass 2 = `u0022` (`n<path>`)
     - Sweep B corpus input (`#2978`): `[\\\\host\\c$\\Users\\<user>\nx.cs`
       - Replaced regex (`7ee5411`): `[<path>~\nx.cs`
       - Pre-435 copy (`PathScrubBefore435`): `[nx.cs`
       - Scanner (`PathScrub.Apply`): pass 1 = `[<path>nx.cs` [`Scan` 1 produces `[<path>~\nx.cs`; `Scan` 2 corrupts it to `[<path>nx.cs`], pass 2 = `[<path>nx.cs`

8. **BLOCKING — Regressions in `ReadSpacedWord` and `ReadEscapeBoundToken` swallowing spaces and punctuation after account folders**
   - **Real source of shape**: MSBuild diagnostics with `[Verification.csproj]` or quoted text after an account folder, and JSON-escaped multi-line prose with a period or operator before `\n`.
   - **Root cause**:
     1. [`ReadSpacedWord`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L541) includes `|| c == ' '` inside the word-character loop, and [`SpacedWordStopFollow`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L878-L890) returns `false` at `[`, `"`, `'`, `(`, `)`, `,`, `;`, and `' '`. Thus `ReadSpacedWord` swallows spaces after an account folder before `[Verification.csproj]` or `"is denied"`.
     2. [`ReadEscapeBoundToken`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L417-L461) (not in `slice-architect.md`) swallows any non-whitespace, non-`FinalStop` punctuation token (`.`, `!=`, `=`) after a space when followed by `\n`, `\r`, `\t`, `\b`, `\f`, `\"`, or `\uXXXX`.
   - **Exact inputs & outputs**:
     - Input 1: `C:\Users\<user> bb [Verification.csproj]` (and `C:\Users\<user>  [Verification.csproj]`, `C:\Users\<user> bb "is denied"`)
       - Replaced regex (`7ee5411`): `<path> [Verification.csproj]` (`<path>  [Verification.csproj]`, `<path> "is denied"`)
       - Pre-435 copy (`PathScrubBefore435`): `<path> [Verification.csproj]` (`<path>  [Verification.csproj]`, `<path> "is denied"`)
       - Scanner (`PathScrub.Apply`): pass 1 = `<path>[Verification.csproj]` (`<path>[Verification.csproj]`, `<path>"is denied"`), pass 2 = `<path>[Verification.csproj]`
     - Input 2: `C:\\Users\\<user> .\nnext` (and `C:\\Users\\<user> !=\nnext`)
       - Replaced regex (`7ee5411`): `<path> .\nnext` (`<path> !=\nnext`)
       - Pre-435 copy (`PathScrubBefore435`): `nnext`
       - Scanner (`PathScrub.Apply`): pass 1 = `<path>\nnext`, pass 2 = `<path>\nnext`

9. **MINOR — Second-pass change and pass-1 account leak on `PathScrub.Apply` when 3 paths chain via `RootKind.Unc` or `RootKind.Tilde` after an account tail**
   - **Classification**: Contrived 3-path composition (`MINOR` under D259 clause 4; becomes a corpus row).
   - **Root cause**: [`AllowsStart`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L690) allows adjacency (`p == lastPathEnd`) only for `RootKind.Drive` and `RootKind.Unix`, omitting `RootKind.Unc` and `RootKind.Tilde`. When an account folder's tail ends in a word character (`C:\Users\<user>(bb)cc`) glued to `\\host\Users\<user>`, `Scan` pass 1 cannot start `TryUnc` at `p == lastPathEnd` because `PlainLeft` checks `text[p - 1]` (`'c'`). Once `<path>` is written, its trailing `'>'` satisfies `PlainLeft` on the next `Scan` pass, so each `Scan` pass peels off only one glued UNC (or tilde) path. Because `Apply` runs `Scan` twice, a chain of two glued UNC or tilde paths after an account tail leaks the third `<user>` on `Apply` pass 1 and scrubs it on `Apply` pass 2.
   - **Exact input**: `C:\Users\<user>(bb)cc\\host\Users\<user>(bb)cc\\host\Users\<user>` (and `C:\Users\<user> 1's~/Users\<user> 1's~/Users\<user>`)
   - **Replaced regex (`7ee5411`)**: `<path><path>\\host\Users\<user>` (`<path> 1's~/Users\<user> 1's~/Users\<user>`)
   - **Pre-435 copy (`PathScrubBefore435`)**: `<path>(bb)cc\\host\Users\<user>(bb)cc\\host\Users\<user>` (`<path> 1's~/Users\<user> 1's~/Users\<user>`)
   - **Scanner (`PathScrub.Apply`)**:
     - Pass 1: `<path><path>\\host\Users\<user>` (`<path><path>~/Users\<user>`) (**leaks `<user>` / `aabb`**)
     - Pass 2: `<path><path><path>` (`<path><path><path>`) (**second-pass change**)

10. **MINOR — Code bloat (939 lines vs architect's ~330-line reference), dead code, duplicated rules, and unreachable branches in [`PathScrub.cs`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs)**
    - **Double scan in [`Apply`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L35-L36)**: `var once = Scan(text); return Scan(once);` runs the scanner twice on every call instead of settling in a single pass.
    - **Duplicated `/mnt/x/users/` branch in [`TryUnix`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L266-L277)**: Whenever `w == rootAt`, `/mnt/x/users/` is completely subsumed by the `/x/users/` check at lines 253–264 on the next iteration of the `while` loop after advancing past prefix segment `"mnt"`.
    - **Duplicated bridged-chain check in [`IsAccountFolder`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L383-L384)**: `if (seps[n] != root.ChainSep && n + 1 < dirs.Count && InSet(dirs[n + 1], AccountParents)) return true;` is duplicated by the first iteration (`m = n + 1`) of `for (var m = n + 1; m < dirs.Count; m++)` at lines 385–392.
    - **Duplicate struct fields [`RootStart.SawMarker`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L112) and [`RootStart.MarkerAtRoot`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L113)**: Set to identical values in every constructor call (lines 133, 179, 196, 228–276) and only read together as `root.SawMarker && root.MarkerAtRoot` (line 336).
    - **Unreachable branches**:
      - [`TryUnc` line 167](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L167) (`if (i == hostStart) return false;`): unreachable because line 162 already returned `false` if `i >= text.Length || IsSeparator(text[i]) || char.IsWhiteSpace(text[i])`.
      - [`TryUnc` line 175](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L175) (`if (sepStart == sepEnd) return false;`): unreachable because [`ReadSeparatorRun`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L684) returns `true` only when `p > i`.
      - [`ReadPathTail` line 329](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L329) (`return end > root.RootIndex;`): always `true` because `end >= root.ChainIndex > root.RootIndex`, making the `if (!ReadPathTail(...))` false branches at lines 134, 180, 197, and 291 dead.
      - [`IsFlagBoundaryAhead` line 790](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L790) (`return j + 1 < text.Length && IsAsciiLetter(text[j]) && text[j + 1] == ':';`): no-op in `ReadPathTail` because `':'` is in `NameStops` (line 931), so `ReadFolderCandidate` stops at `':'` anyway and `ReadSeparatorRun` fails on `':'`.
      - [`SpacedWordStopFollow` line 889](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L889) (`|| IsEscLetterAt(text, p + 1)`): no-op because [`ReadSpacedWord`](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L555-L558) immediately advances `p` past `.` to `\uXXXX`, where `SpacedWordStopFollow` (line 886) always returns `false`, overwriting `end`.
    - **Tautological arithmetic (lines 694, 699, 716, 718, 728, 733, 735)**: `p - 6 + 6 == p`, `p - 2 + 2 == p`, `p - 8 + 6 == p - 2`, and `p - 4 + 2 == p - 2` are always `true`.

---

### Linearity Measurements (20,000 and 1,000,000 characters)

| Shape unit | 20,000 chars (ms) | 1,000,000 chars (ms) | 1M / 20k ratio | Pass 2 == Pass 1 | Status |
|---|---:|---:|---:|---|---|
| `\b` | 21.51 | 834.41 | 38.8x | True | Linear |
| `\;` | 12.22 | 610.41 | 50.0x | True | Linear |
| `\,` | 12.69 | 617.54 | 48.7x | True | Linear |
| `\a;` | 14.97 | 661.84 | 44.2x | True | Linear |
| `\\host\share\x.cs\n` | 3.72 | 221.73 | 59.6x | True | Linear |
| `\C:/a\n` | 14.20 | 532.08 | 37.5x | True | Linear |
| `\C:\a\n` | 8.69 | 446.01 | 51.3x | True | Linear |
| `\\a$\n` | 3.20 | 310.18 | 96.8x | True | Linear |
| `/a` | 16.89 | 810.75 | 48.0x | True | Linear |
| `/-` | 18.79 | 920.80 | 49.0x | True | Linear |
| `/=-I` (Finding 1) | **4,691.73–6,396.53** (40k: **23,308.90**) | >10,000,000 (est. ~4 h) | **Quadratic $O(N^2)$** | True | **BLOCKING** |
| `/(-I` (Finding 1) | **4,805.28** (40k: **19,046.45**) | >10,000,000 (est. ~3.3 h) | **Quadratic $O(N^2)$** | True | **BLOCKING** |
| `C:\a` + spaces (Finding 2) | **1,809.56** (5k: 121.18, 10k: 481.67) | >4,000,000 (est. ~75 min) | **Quadratic $O(N^2)$** | True | **BLOCKING** |

---

### Mutants Surviving `Slice=439`, `Slice=435`, `Slice=426`, and `Slice=418`

All 9 rule mutants below were applied to `tools/Avowry.Bench/PathScrub.cs`, built, verified to pass `Slice=439` (163/163) and `Slice=435|Slice=426|Slice=418` (186/186, plus all 262,197 distinct inputs across the data sets and Sweeps A, B, and C), and then restored byte-for-byte:

1. **`fixed-no-private-var-folders`** — [`UnixFixedRoots` (line 28)](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L28): remove `"/private/var/folders/"`.
   - Killing row: `/private/var/folders/xx/yy/T/Candidate.cs`
   - Replaced regex (`7ee5411`): `Candidate.cs` | Pre-435: `Candidate.cs` | Scanner: `Candidate.cs` | Mutant: `/private/var/folders/xx/yy/T/Candidate.cs`
2. **`mnt-no-prefix`** — [`TryUnix` (line 271)](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L271): change `&& (w == rootAt || IsPrefixFolder(text, rootAt + 1, w - 1))` to `&& w == rootAt` (or delete the `/mnt/` block at lines 266–277).
   - Killing row: `/host/mnt/c/users/<user>/x.cs`
   - Replaced regex (`7ee5411`): `x.cs` | Pre-435: `/host/mnt/c/users/<user>/x.cs` | Scanner: `x.cs` | Mutant: `/host/mnt/c/users/<user>/x.cs` (leaks `<user>`)
3. **`flag-split-no-unc`** — [`IsFlagBoundaryAhead` (line 788)](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L788): remove `|| text[j] == '\\'`.
   - Killing row: `gcc /tmp/a.c -I\\host\Users\<user>\include`
   - Replaced regex (`7ee5411`): `gcc include` | Pre-435: `gcc include` | Scanner: `gcc a.c -Iinclude` | Mutant: `gcc <path>`
4. **`drive-no-bare-unc`** — [`TryDrive` (line 133)](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L133): change `bareUnc: IsAfterBareUnc(text, p, lastPathEnd)` to `bareUnc: false`.
   - Killing row: `a\\C:\a\x.cs`
   - Replaced regex (`7ee5411`): `a\\x.cs` | Pre-435: `a\\x.cs` | Scanner: `a\\<path>` | Mutant: `a\\x.cs`
5. **`tilde-no-bare-unc`** — [`TryTilde` (line 196)](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L196): change `bareUnc: IsAfterBareUnc(text, p, lastPathEnd)` to `bareUnc: false`.
   - Killing row: `a\\~/a/x.cs`
   - Replaced regex (`7ee5411`): `a\\x.cs` | Pre-435: `a\\x.cs` | Scanner: `a\\<path>` | Mutant: `a\\x.cs`
6. **`glued-word-dot-no-esc`** — [`ReadGluedWord` (line 515)](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L515): remove `|| IsEscLetterAt(text, p + 1)`.
   - Killing row: `C:\Users\<user>(bb).\u00f6cc`
   - Replaced regex (`7ee5411`): `<path>.\u00f6cc` | Pre-435: `u00f6cc` | Scanner: `<path>` | Mutant: `<path>.\u00f6cc` (leaks `cc`)
7. **`glued-word-apos-no-esc`** — [`ReadGluedWord` (line 508)](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L508): remove `|| IsEscLetterAt(text, p + 1)`.
   - Killing row: `C:\Users\<user>(bb)'\u00f6cc`
   - Replaced regex (`7ee5411`): `<path>'\u00f6cc` | Pre-435: `<path>(bb)'\u00f6cc` | Scanner: `<path>` | Mutant: `<path>'\u00f6cc` (leaks `cc`)
8. **`glued-end-no-colon-sep`** — [`IsValidGluedEnd` (line 903)](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L903): change `return !(c == ':' && p + 1 < text.Length && IsSeparator(text[p + 1]));` to `return true;`.
   - Killing row: `C:\Users\<user>(bb)1:\a`
   - Replaced regex (`7ee5411`): `<path>1:\a` | Pre-435: `<path>(bb)1:\a` | Scanner: `<path>1:\a` | Mutant: `<path>:\a`
9. **`root-prefix-file-case`** — [`LooksLikeRootPrefix` (line 797)](file:///<home>/projects/review439-clone/tools/Avowry.Bench/PathScrub.cs#L797): change `StartsWithIgnoreCase(text, i, "file:/")` to `StartsWith(text, i, "file://")`.
   - Killing row: `C:\Users\<user>file:/a`
   - Replaced regex (`7ee5411`): `<path>file:/a` | Pre-435: `<path>:/a` | Scanner: `<path>file:/a` | Mutant: `<path>a`

---

### Commands Run and Summary Lines

- `dotnet build Avowry.sln -c Debug` -> `Build succeeded. 0 Warning(s) 0 Error(s) (Time Elapsed 00:00:08.05)`
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439"` -> `Passed! - Failed: 0, Passed: 163, Skipped: 0, Total: 163, Duration: 12 s - Avowry.Acceptance.Tests.dll (net8.0)`
- `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435|Slice=426|Slice=418"` -> `Passed! - Failed: 0, Passed: 186, Skipped: 0, Total: 186, Duration: 2 m 8 s - Avowry.Acceptance.Tests.dll (net8.0)`
- `dotnet run --project "$env:TEMP\probe439-qa\Probe439.csproj" -c Debug` -> differential, linearity (20k and 1M chars), and mutant probes against `PathScrub`, `PathScrub7ee5411` (`7ee5411:tools/Avowry.Bench/PathScrub.cs`), and `PathScrubBefore435` (`exit 0`)
- (With 9 temporary mutants in `tools/Avowry.Bench/PathScrub.cs`):
  - `dotnet build Avowry.sln -c Debug` -> `Build succeeded. 0 Warning(s) 0 Error(s) (Time Elapsed 00:00:07.28)`
  - `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=439"` -> `Passed! - Failed: 0, Passed: 163, Skipped: 0, Total: 163, Duration: 12 s - Avowry.Acceptance.Tests.dll (net8.0)`
  - `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=435|Slice=426|Criterion=418-U"` -> `Passed! - Failed: 0, Passed: 176, Skipped: 0, Total: 176, Duration: 49 s - Avowry.Acceptance.Tests.dll (net8.0)`
- (After `git checkout -- tools/Avowry.Bench/PathScrub.cs`):
  - `git diff --stat` -> empty
  - `git status --short` -> empty
  - `dotnet build Avowry.sln -c Debug` -> `Build succeeded. 0 Warning(s) 0 Error(s) (Time Elapsed 00:00:04.92)`
