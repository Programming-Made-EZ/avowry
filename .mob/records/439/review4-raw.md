# Card 439 review round 4, raw (QA: claude opus, ctx-6; read-only at e85c60b); home paths scrubbed

VERDICT: SIGN-OFF
new BLOCKING: 0; new MINOR: 4

Seat: QA round 4 (claude opus = claude-opus-5-5, ctx-6, per D262), in the throwaway clone `review439d-clone` at e85c60b. Round 3's findings are fixed. I found no leak, regression or second-pass change on a real shape and no super-linear time, so nothing is BLOCKING under D259 clause 4. Four MINOR findings remain, each a corpus row or a test gap.

The clone is back to its original state: `PathScrub.cs` is byte-equal to my backup, `git diff --stat` and `git status` are empty, HEAD is e85c60b. The probe is a new folder: `<home>\AppData\Local\Temp\claude\74d6169ff85fa31d\8618da5d-e236-46f1-bb75-7592da830d1c\scratchpad\probe439q4`. The account name in every probe is `alice`.

## Round-3 check
- **Finding 1 (leak): fixed on every input I named.** All equal the pre-435 copy, with no leak and a stable second pass:
  - `docker run -v C:\Users\<user>\proj:/home/<user> ubuntu` and the `C:/…` form → `proj:<path>`
  - `-v C:\Users\<user>:/home/<user>` → `<path>:<path>`
  - the daemon error → `'proj:<path>:rw'`
  - `podman run -v C:\Users\<user>\data:/home/<user> x` → `data:<path>`
  - `-v \\server\share\Users\alice\proj:/home/<user>` → `proj:<path>`
  - `D:\data\proj:/home/<user>` → `proj:<path>`
- **Finding 2 (regression): fixed.** All equal the regex: `-v C:\Users\<user>\proj:/app` → `proj:/app`; `C:/Users/<user>/.ssh:/root/.ssh` → `.ssh:.ssh`; `-v "C:\Users\<user>\src:/src"` → `"src:/src"`.
- **Finding 3 (second-pass change): fixed.** `docker run -v /home/<user>:/data`, `-v /Users/<user>:/workspace` and `/home/<user>:/bin/bash` give `<path>:/data`, `<path>:/workspace` and `<path>:/bin/bash`. These equal the pre-435 copy, stable, and no account letter survives.
- **Round 3's killing rows: as ruled.**
  - `/tmp/abfile:///C:/Users/<user>/x.cs` → `abx.cs`
  - `~/x:/home/<user>` → `x:<path>`
  - `\\server\share\abC:/Users/<user>/x.cs` → `abC:x.cs`, the regex's output, pinned as characterisation
- **Every earlier case file** (`real`, `more`, `more2`, `colon`, `r1`, `r3`, `r3b`, `kill`, `kill2`, `kill3`; 355 inputs) gives no new leak against both references and no second-pass change, except the named-limit and MINOR items below.

## New real shapes tried (86 inputs, `r4.txt`)
No new leak, regression or second-pass change on any of:
- **Docker and Podman:** drive, `//c/`, `/c/`, `/mnt/c/` and `\\wsl$` hosts; Windows-container `X:\` targets; `--mount src=…`; `:ro`; quoted forms; `podman machine ssh`.
- **Compose volume lists:** `C:\`, `/home`, `~` and `$HOME` entries.
- **Env dumps:** `PATH` (Linux and Git Bash), Windows `Path`, `XDG_DATA_DIRS`, `PWD`, `OLDPWD`, `HOME`, `PYTHONPATH`, `PSModulePath`.
- **passwd lines:** Linux and macOS.
- **rsync and scp `host:path` forms; git ssh remotes.**
- **Windows drive-relative and alternate-data-stream forms:** `C:x.txt`, `D:b.txt`, `x.txt:Zone.Identifier`, `:stream:$DATA`, `x.cs:line 3`.
- **Glued drives and `file:///`** after a drive path.

Every passwd line and every home or drive mount before `:/` now gives the pre-435 copy's stable `<path>:/…`. The regex gave `<path>e:/…`, which changes on its own second pass.

## Findings

**1. MINOR (contrived, no real source): a one-letter final name in a drive or UNC chain before `:/` plus a home path.** The one-letter rule (b) covers only Unix and `~` chains, so the letter starts a drive with a plain left.

| Input | regex | pre435 | scan1 | scan2 |
|---|---|---|---|---|
| `\\server\share\a:/home/<user>` | `<path>` | `a:<path>` | `<path>alice` | `<path>alice` |
| `C:\tmp\x:/home/<user>` | `<path>alice` | `x:<path>` | `<path>alice` | `<path>alice` |
| `C:\Users\<user>\a:/home/<user>` | `<path>alice` | `a:<path>` | `<path>alice` | `<path>alice` |

- The UNC row is new against both references; the two drive rows are new only against pre435.
- Widening rule (b) to every chain (mutant `ownletter-b-any-chain`) fixes all three, but the `\\C:/a\n` blow-up row's second pass kills it. A fix would have to be narrower. This becomes a corpus row.

**2. MINOR: a mutant of rule (a) survives all four slices.** `ownletter-a-letters-only` (AsciiWord → letters only):
- Killing row: `-v /home/<user>:/data` (an account name with a digit or `_` before its last letter).
- Current output: `<path>:/data`, stable. The mutant gives `<path>a:/data`, which keeps a letter and becomes `<path>data` on pass 2, as the regex does.

**3. MINOR (dead code): `j > finalStart` in rule (a) is redundant.** A final name always starts after a separator, so `AsciiWord(At(j - 1))` is false at `finalStart`. The mutant `ownletter-a-no-pos` is equivalent: it survived all four slices and differed from the scanner on none of the 355 inputs.

**4. MINOR (older, the same in all three): leaks on real shapes not yet in the corpus.**
- `MAIL=/var/mail/alice`: `/var/mail/` is not a root.
- `git remote add origin ssh://host/home/<user>/repo.git`
- Each should become a corpus row.

Named limits, unchanged and not counted: round 3's finding 4 (`-v ~/x:/data` → `x:/data` → `data`, `/tmp/x:/x`, and the new real-looking `-v /home/<user>/v:/v` → `v:/v` → `v`), and finding 6 (`x://`, `x'y:`).

## Linearity
I timed 21 shapes at 20k / 40k / 1M characters; all are linear:
- the new shapes: `-v C:\Users\<user>\proj:/home/<user> `, `-v C:\Users\<user>:/data `, `-v /home/<user>:/data `, the passwd line, `\\s\h\proj:/home/<user> `, `\\s\h\a:/b `, `C:\a\b:/x`, `C:\a\bc:/`, `C:\ab:`, `x:/`, `a:/`, `/tmp/x:`, `/home/<user>/v:/v `, `/home/<user>:/data `, `C:\a\x.csD:/a`, `C:\a\x.csD:\a`, `C:\a\x.csfile:///C:/a`, `host:/home/<user> `
- the older shapes `/=-I`, `/-` and `\\C:/a\n`

The worst is `-v C:\Users\<user>\proj:/home/<user> ` at 1.4 / 2.4 / 53.7 ms.

## Mutants (each run against Slice=439, then 435, 426, 418)
- **Killed (3):**
  - `ownletter-a-any-sep` (`:\` as well as `:/`): by 3.4f, 3.4h and others
  - `ownletter-b-any-chain`: by the blow-up row `\\C:/a\n`, which fails on its second pass
  - `glue-drive-slash-too` (adjacency for `X:/` too): by the rows "adjacency-any (re-rowed)" and "glue-after-drive-slash"
- **Survived (2):**
  - `ownletter-a-letters-only` (finding 2, with its killing row)
  - `ownletter-a-no-pos` (equivalent, finding 3)

## Commands and summary lines
- Before every `dotnet build` and `dotnet test`, I ran `bash heavylock.sh check avowry play-tracker`. It printed `free` every time, so no run was held back.
- `dotnet build Avowry.sln -c Debug`: 0 Warning(s), 0 Error(s). Run at baseline and once per mutant, plus a rebuild after the restore.
- Baseline `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=N"`:

| Slice | Summary line |
|---|---|
| 439 | Passed! Failed 0, Passed 229, Total 229 |
| 435 | Passed! Failed 0, Passed 112, Total 112 |
| 426 | Passed! Failed 0, Passed 30, Total 30 |
| 418 | Passed! Failed 0, Passed 45, Total 45 |

- Mutant runs used the same filters:
  - `ownletter-a-any-sep`: "Failed! Failed 7, Passed 222, Total 229"
  - `ownletter-b-any-chain`: "Failed 1, Passed 228"
  - `glue-drive-slash-too`: "Failed 2, Passed 227"
- The probe was built with `dotnet build -c Release` in probe439q4, after a lock check, and run in modes `cases`, `mut` and `time` on `r4.txt`, `all4.txt`, `time4.txt` and the earlier case files.