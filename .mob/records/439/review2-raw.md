# Card 439 review round 2, raw (QA: claude opus = claude-opus-5-5, ctx-6, reseated by D262; read-only at 4e4893d); home paths scrubbed

VERDICT: FLAG
new BLOCKING: 2; new MINOR: 4

Seat: QA round 2 (claude opus = claude-opus-5-5, reseated by D262, ctx-6). I worked in the review clone at 4e4893d, branch card-439. I edited only `tools/Avowry.Bench/PathScrub.cs` for mutants and restored it byte for byte (`cmp` equal to my backup; `git diff --stat` empty; `git status` clean). The probe is a new folder, `<home>\AppData\Local\Temp\claude\74d6169ff85fa31d\8618da5d-e236-46f1-bb75-7592da830d1c\scratchpad\probe439q`. It links the clone's PathScrub.cs, the pre-435 copy, and the 7ee5411 regex renamed `PathScrubRegex`. The account name in every probe is `alice`.

Columns in every finding: regex = 7ee5411, pre435 = PathScrubBefore435, scan1 and scan2 = the scanner's first and second pass.

## Round-1 BLOCKING findings: all fixed
- **1 (glued flags, quadratic):** fixed. At 20k / 1M characters, `/=-I` takes 1.4 / 46.9 ms, `/(-I` 1.3 / 72.3 ms and `/a=-I/a(-L` 1.4 / 69.0 ms.
- **4:** fixed. `C:/Users\<user>\x.cs` and `file:///C:/Users\<user>\x.cs` give `x.cs` in every column.
- **5:** fixed. `/mnt/wsl/c/users/<user>/x.cs` and `/opt/cygdrive/c/users/<user>/x.cs` give `x.cs`; `/a/c/users/<user>` gives `<path>`. All equal the regex.
- **7:** fixed. `{"pattern":"\\d+","path":"C:\\Users\\<user>\\x.cs"}` gives `{"pattern":"\\d+","path":"x.cs"}`. The regex lost the `\\` (`"d+"`).
- **8:** fixed. `<path> [Verification.csproj]`, `<path> "is denied"` and `<path> .{NL}next` all equal the regex.
- **2:** not reproduced. Whitespace runs after a root are linear (`C:\a ` 1.1 / 60.8 ms, `/tmp/a ` 1.6 / 65.4 ms).
- **3:** the finding-3 inputs equal the regex. Findings 1 and 2 below are new regressions of that kind, on real shapes.

## Findings

**1. BLOCKING: an account folder leaks in Unix colon-separated path lists.**
- Exact inputs and outputs:
  - `docker run -v /home/<user>/work:/home/<user> -it ubuntu`
    - regex: `docker run -v work:<path>`
    - pre435: `docker run -v work:<path>`
    - scan1: `docker run -v woralice -it ubuntu`
    - scan2: `docker run -v woralice -it ubuntu`
  - `PYTHONPATH=/home/<user>/lib:/home/<user>`
    - regex and pre435: `PYTHONPATH=lib:<path>`
    - scan1 and scan2: `PYTHONPATH=lialice`
  - `PATH=/c/Users/<user>/bin:/usr/bin:/c/Users/<user>` (Git Bash)
    - regex and pre435: `PATH=bin:/usr/bin:<path>`
    - scan1 and scan2: `PATH=bibialice`
  - The same leak, new against both the regex and the pre-435 copy:
    - `GOPATH=/home/<user>/go:/home/<user>` → `galice`
    - `NODE_PATH=…/node_modules:/home/<user>` → `node_modulealice`
    - `CDPATH=.:/home/<user>/src:/home/<user>` → `.:sralice`
    - `CLASSPATH=/home/<user>/lib/a.jar:/home/<user>/classes:/home/<user>` → `a.jaclassealice`
    - `-Djava.class.path=…/a.jar:/home/<user>` → `a.jaalice`
    - `rsync: change_dir "/home/<user>/src:/home/<user>" failed` → `"sralice"`
    - docker-compose `volumes: [/home/<user>/data:/home/<user>]` → `[datalice]`
    - `/home/<user>:/home/<user>` → `<path>alice`
- Second pass: stable, so these are not second-pass changes.
- Mechanism:
  - The final name stops before `X:/` (RootStartAt), so `…/lib` ends as `li`.
  - Start rule 5 (adjacency, `rootGlue && p == prevEnd` in TryDrive) then reads `b:/home/<user>` as a drive path.
  - On a drive, only `users` and `documents and settings` are account parents, so the final `alice` survives.
  - This is the DESIGN (slice-architect "Starts" rule 5), not a departure by the green.
- Real sources: docker `-v src:dst` volume specs and the daemon's "invalid mount config" errors, docker-compose volume lists, env dumps of PATH, PYTHONPATH, GOPATH, NODE_PATH, CDPATH and CLASSPATH (compiler and runtime arguments), and rsync errors.
- Removing adjacency outright is not the fix: that mutant is killed by rows 3.4f and 3.4h.
- The mutant `drive-parents-take-home` (DriveParents → AccountParents) survives all four slices and turns these leaks into `li<path>`. It does not fix finding 2.

**2. BLOCKING: every Unix colon list is mangled (a regression on a real shape, no leak).**
- `PATH=/home/<user>/bin:/home/<user>/.local/bin:/usr/bin`
  - regex and pre435: `PATH=bin:bin:/usr/bin`
  - scan1 and scan2: `PATH=bibibin`
- `LD_LIBRARY_PATH=/home/<user>/lib:/home/<user>/.local/lib`
  - regex: `lib:lib`
  - scan: `lilib`
- `- /home/<user>/.config:/root/.config`
  - regex: `.config:.config`
  - scan: `.confi.config`
- The cause is the same as finding 1: the last letter of each entry becomes a "drive" letter, and the `:` and the next entry's folders are swallowed.
- Real sources: PATH and LD_LIBRARY_PATH in shell and env output, and docker volume specs.

**3. MINOR (older, the same in all three columns): leaks on real shapes that the replaced regex and the pre-435 copy also leak.**
- MSVC glued `/I`: `cl.exe /c /IC:\Users\<user>\include /FoC:\Users\<user>\obj\main.obj …` keeps both glued paths.
- `gcc -isystem/home/<user>/include` (a named limit in the regex header).
- `~alice/x.cs`.
- JSON with escaped slashes: `{"file":"\/home\/<user>\/x.cs"}`.
- `\\?\UNC\server\share\Users\alice\x.cs`.
- URLs such as `https://example.com/home/<user>/x`.
- `a/b:/home/<user>` → `a/alice`.
- `/home/<user>/a:/mnt/c/Users/<user>` and `/tmp/x:/home/<user>` → `<path>alice`. The regex leaks these too; pre435 did not, so they are card 435's.
- Each should become a corpus row.

**4. MINOR: device-path prefixes differ from the regex, and no row pins them.**
- No row in PathScan439 or PathScrubForms covers `\\?\` or `\\.\`.
- `'\\?\C:\Users\<user>\x.cs'`
  - regex and pre435: `'<path>?\x.cs'`
  - scan1 and scan2: `'\\?\x.cs'` (arguably better)
- `\\.\C:\Users\<user>\x.cs`
  - regex: `x.cs`
  - pre435: unchanged
  - scan1 and scan2: `<path>x.cs`
- No leak, and both are stable on a second pass.

**5. MINOR: seven non-equivalent mutants survive Slice 439, 435, 426 and 418.** One of them leaks on a real shape. Killing rows, found in the probe, with the current output and the mutant's output:

| Mutant | Killing row | Current output | Mutant output |
|---|---|---|---|
| `wordends-no-dash` (drop `-` in WordEnds) | `C:\Users\<user>` | `<path>` (= regex, = pre435) | `<path> smith-jones` (leaks the second half of a spaced account name; see AccountNameWithASpace) |
| `drive-parents-take-home` | `D:\home\<user>` | `alice` (= regex) | `<path>` |
| `guard-letter-esc-gone` (Guard's first line) | `\\C:\u00e9x\y.cs` | `\\<path>` (regex `<path>y.cs`) | `y.cs` |
| `unc-host-space-ok` | `\\ server\share\x.cs` | unchanged | `x.cs` |
| `flagleft-no-semicolon` | `CFLAGS=-O2;-I/home/<user>/inc` | `CFLAGS=-O2;-Iinc` (the regex leaks) | leaks |
| `root-ahead-tilde-gone` | `C:\a b -I~/x.cs` | `a b -Ix.cs` | `x.cs` |
| `stopat-apos-gone` | `C:\Users\<user>'b` | `<path> x2'b` (= regex) | `<path>'b` |

**6. MINOR: dead code.**
- The `/mnt/` candidate in `AccountMarkerAt` (`Has(w, "/mnt/") ? w + 4 : -1`) is unreachable in effect: the walk always reaches the `/x/users/` marker at the next `/`, with the same end.
- The mutant `mnt-branch-gone` survives all four slices and differed from the scanner on none of the inputs I tried.
- The same line allocates a new array on every walk step.

## Linearity (minimum of 3 runs, Release build of the probe)
- I measured 58 repeated-unit shapes at 20k / 40k / 1M characters: the 9 slice shapes, the 2 fix shapes, `/a=-I/a(-L` and 46 of my own.
- I also measured 13 single-structure shapes, for example:
  - an unclosed `(` tail
  - `<` plus letters
  - one long failed walk
  - a long glued colon chain
  - a UNC host full of spaces
  - many spaced tail words
  - glued groups
  - letter escapes
- Everything is linear. The worst is `/-` at 1.9 / 3.8 / 100.7 ms; every shape is at most 1.9 ms at 20k and at most 101 ms at 1M.
- No super-linear shape found.

## Mutants (each run against Slice=439, then 435, 426, 418; it stops at the first failing slice)
- **Killed (7):**
  - `x-one-letter-users-gone` (row "unix-no-letter-users", sweep B)
  - `drive-adjacency-gone` (3.4f, 3.4h)
  - `unc-pathmark-guard-gone` (3.8f, 2.9e)
  - `tilde-after-flag-gone` (2.12c)
  - `apostrophe-after-placeholder-gone` (2.5j, 2.5k)
  - `tail-paren-no-space` (2.5f, group-no-space)
  - `file-localhost-gone` (3.4g, 3.4i)
  - `unc-drive-host-gone` (3.7f, 2.4e, 2.4f), after the first `if (false)` form failed to build under TreatWarningsAsErrors
- **Survived (8):** `mnt-branch-gone` (equivalent, finding 6) and the seven in finding 5.

## Commands and summary lines
- `dotnet build Avowry.sln -c Debug`: 0 Error(s). Run at baseline, once per mutant, and once after the restore.
- Baseline `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=N"`:

| Slice | Summary line |
|---|---|
| 439 | Passed! Failed 0, Passed 180, Total 180 |
| 435 | Passed! Failed 0, Passed 112, Total 112 |
| 426 | Passed! Failed 0, Passed 30, Total 30 |
| 418 | Passed! Failed 0, Passed 45, Total 45 |

- Mutant runs used the same filtered commands; the killed lines are quoted above, for example "Failed! Failed 9, Passed 171, Total 180" for `drive-adjacency-gone`.
- Probe: `dotnet build -c Release` in probe439q. Modes `cases`, `time`, `big` and `mut`, with input files `r1.txt`, `real.txt`, `colon.txt`, `more.txt`, `more2.txt`, `time.txt`, `kill.txt` and `kill2.txt`.
- Restore check: `cmp PathScrub.cs <backup>` gives byte-equal; `git diff --stat` is empty; HEAD is 4e4893d.