# Card 439 review round 3, raw (QA: claude opus, ctx-6; read-only at 4b9966b); home paths scrubbed

VERDICT: FLAG
new BLOCKING: 3; new MINOR: 3

Seat: QA round 3 (claude opus = claude-opus-5-5, ctx-6, per D262), in the throwaway clone `review439c-clone` at 4b9966b. My round-2 findings are fixed, and the round-2 killing rows behave as the rulings say. But Docker volume specs whose host side is a Windows drive or a UNC share still leak, and the one-letter rule makes the commonest home-mount and `/etc/passwd` shapes change on a second pass.

The clone is back to its original state: `PathScrub.cs` is byte-equal to my backup, `git diff --stat` and `git status` are empty, HEAD is 4b9966b. The probe is a new folder: `<home>\AppData\Local\Temp\claude\74d6169ff85fa31d\8618da5d-e236-46f1-bb75-7592da830d1c\scratchpad\probe439q3`. The account name in every probe is `alice`.

## Round-2 check
- **Findings 1 and 2: fixed.** All 22 colon-list inputs I named in round 2 now equal the replaced regex, with no new leak and a stable second pass. Examples:
  - docker `-v /home/<user>/work:/home/<user>` → `work:<path>`
  - `PATH=…/bin:…/.local/bin:/usr/bin` → `bin:bin:/usr/bin`
  - `LD_LIBRARY_PATH` → `lib:lib`
  - `CLASSPATH` → `a.jar:classes:<path>`
  - compose `[data:<path>]`, rsync `"src:<path>"`
  - `/tmp/x:/home/<user>` → `x:<path>`, which removes a leak the regex has
- **Round-2 killing rows: as ruled.** `C:\home\<user>` → `x.cs`; `\\C:\u00e9x\y.cs` → `\\<path>`; `\\ server\share\x.cs` unchanged; `C:\Users\<user>` → `<path>`; `CFLAGS=-O2;-I/home/<user>/inc` → `-Iinc`; `C:\a b -I~/x.cs` → `a b -Ix.cs`; `C:\Users\<user>'b` → `<path> x2'b`. The device-path rows are pinned.
- **Round-2 real-shape files:** no new leak and no second-pass change.

## Findings

**1. BLOCKING (a leak; in the design since round 1, which I missed in round 2): colon lists whose host side is a Windows drive or a UNC share.** Adjacency still holds for any `X:/` after a drive or UNC path, so the letter before `:/` becomes a drive.

| Input | regex and pre435 | scan1 | scan2 |
|---|---|---|---|
| `docker run -v C:\Users\<user>\proj:/home/<user> ubuntu` | `docker run -v proj:<path>` | `docker run -v proalice ubuntu` | `docker run -v proalice ubuntu` |
| `docker run -v C:/Users/<user>/proj:/home/<user> ubuntu` | `docker run -v proj:<path>` | `docker run -v proalice ubuntu` | same |
| `docker run -v C:\Users\<user>:/home/<user> ubuntu` | regex `<path>e:<path>`, pre435 `<path>:<path>` | `<path>alice ubuntu` | same |
| `Error response from daemon: invalid volume specification: 'C:\Users\<user>\proj:/home/<user>:rw'` | `'proj:<path>:rw'` | `'proalice:rw'` | same |
| `podman run -v C:\Users\<user>\data:/home/<user> x` | `data:<path>` | `datalice x` | same |
| `-v \\server\share\Users\alice\proj:/home/<user>` | `proj:<path>` | `proalice` | same |
| `D:\data\proj:/home/<user>` | `proj:<path>` | `proalice` | same |

- Each leak is new against both the regex and the pre-435 copy.
- Real sources: Docker Desktop and Podman on Windows, their documented `-v C:\…:/…` form, and the daemon's "invalid mode" and "invalid volume specification" errors.
- Candidate fix, measured: `var glue = q > p || At(q + 2) == '\\';`. This allows adjacency after every form only for `file:///` or `X:\`.
  - It gives the regex's output on every input above.
  - Run as a mutant, it passes Slice 439 (209), 435, 426 and 418.

**2. BLOCKING (a regression, the same mechanism as finding 1): the Windows-host volume specs are mangled.**

| Input | regex and pre435 | scan1 and scan2 |
|---|---|---|
| `docker run -v C:\Users\<user>\proj:/app node` | `proj:/app` | `proapp` |
| `C:/Users/<user>/.ssh:/root/.ssh` | `.ssh:.ssh` | `.ss.ssh` |
| `docker run -v "C:\Users\<user>\src:/src" x` | `"src:/src"` | `"srsrc"` |

The fix from finding 1 restores the regex's output on all three.

**3. BLOCKING (a second-pass change, new in fix round 2; the round-2 port was stable): an account folder followed by `:/`.**

| Input | regex | pre435 | scan1 | scan2 |
|---|---|---|---|---|
| `docker run -v /home/<user>:/data ubuntu` | `<path>e:/data` | `<path>:/data` | `<path>e:/data ubuntu` | `<path>data ubuntu` |
| `docker run -v /Users/<user>:/workspace node` | same pattern | same pattern | `<path>e:/workspace node` | `<path>workspace node` |
| `/home/<user>:/bin/bash` (an `/etc/passwd` or `getent passwd` line) | same pattern | stable | `<path>e:/bin/bash` | `<path>bash` |

- The regex changes on its own second pass the same way; pre435 is stable.
- The output also keeps the account's last letter (`e`): ruling 4's named limit, which shows up on these real shapes.
- The tests hide it: c11 is pinned only in the `<user>` form, and `>` is not a letter.
- Candidate fix, measured: `OwnLetter(j) => slashChain && (j == finalStart || AsciiWord(At(j - 1))) && At(j + 2) == '/'`.
  - It gives pre435's stable `<path>:/bin/bash`, `<path>:/data` and `<path>:<path>`, and removes the one-letter leak.
  - Run as a mutant, it passes all four slices.

**4. MINOR (contrived): the one-letter rule itself changes on a second pass when no root follows.**
- `-v ~/x:/data`: scan1 `-v x:/data`, scan2 `-v data`. The regex gives `-v <path>data`, stable; pre435 is the same as the scanner.
- `/tmp/x:/x`: scan1 `x:/x`, scan2 `x`.
- A one-letter mount-source folder; a corpus row.

**5. MINOR: three mutants of the new rules survive all four slices.** Killing rows:

| Mutant | Killing row | Current output | Mutant output |
|---|---|---|---|
| `glue-file-gone` (drop `q > p`) | `/tmp/abfile:///C:/Users/<user>/x.cs` | `abx.cs` | `abfile:///x.cs` (= regex) |
| `slashchain-unix-only` (OwnLetter for Unix only, not `~`) | `~/x:/home/<user>` | `x:<path>` | `<path>alice` (leaks) |
| `glue-unc-blocked` (no adjacency after UNC) | `\\server\share\abC:/Users/<user>/x.cs` | `abx.cs` | `abC:x.cs` (= regex) |

**6. MINOR (contrived, older):**
- `/tmp/x://server/home/<user>`: scan1 `x://server/home/<user>` (leaks; pre435 the same), scan2 `alice`. The regex gives `<path>alice`.
- `/tmp/x'y:/home/<user>` → `x'alice`, the same as the regex.

## Linearity
I re-timed 21 shapes at 20k / 40k / 1M characters; all are linear:
- the colon shapes `/home/<user>/bin:`, `/tmp/x:`, `x:/`, `a/b:`, `/tmp/a:C:/b`, `/tmp/a:C:\b`, `C:\a\b:/x`, `C:\Users\<user>\proj:/home/<user> `, `-v C:\a:/b `, `\\s\h\a:/b`, the passwd line, `~/x:`, `~/b:~/l:` and `/a.file:///C:/a`
- the older shapes `/=-I`, `/(-I`, `/a`, `/-` and `\\C:/a\n`

The worst is `/-` at 2.1 / 4.3 / 91.3 ms; every shape is at most 2.1 ms at 20k and at most 92 ms at 1M.

## Mutants (each run against Slice=439, then 435, 426, 418)
- **Killed (4):**
  - `glue-backslash-gone`: by 3.4h and sweeps B and C
  - `glue-after-tilde`: by c15
  - `ownletter-any-sep`: by 2.9h and sweeps B and C
  - `ownletter-any-position`: by 3.7e
- **Survived (3):** the mutants in finding 5.
- **Candidate fixes (2):** run as mutants, both pass every slice: `glue-any-path-narrow` (finding 1) and `ownletter-after-word` (finding 3).

## Commands and summary lines
- `dotnet build Avowry.sln -c Debug`: 0 Warning(s), 0 Error(s). Run at baseline and once per mutant, plus a rebuild after each restore.
- Baseline `dotnet test tests/Avowry.Acceptance.Tests/Avowry.Acceptance.Tests.csproj --no-build -c Debug --filter "Slice=N"`:

| Slice | Summary line |
|---|---|
| 439 | Passed! Failed 0, Passed 209, Total 209 |
| 435 | Passed! Failed 0, Passed 112, Total 112 |
| 426 | Passed! Failed 0, Passed 30, Total 30 |
| 418 | Passed! Failed 0, Passed 45, Total 45 |

- The mutant runs used the same filters. For example, `glue-backslash-gone` gave "Failed! Failed 3, Passed 206, Total 209"; `glue-after-tilde` and `ownletter-any-position` gave "Failed 1, Passed 208".
- The probe was built with `dotnet build -c Release` in probe439q3 and run in modes `cases`, `time` and `mut` on `r3.txt`, `r3b.txt`, `colon.txt`, `kill3.txt`, `all3.txt` and `time3.txt`, plus the round-2 files.
- **Heavy lock:** when your rule arrived, my mutant loop had finished six mutants and was in the middle of the seventh (`glue-unc-blocked`). It did not check the lock, so I stopped it by task id and restored the file from the backup (byte-equal). I confirmed none of my processes were left running. From then on, every `dotnet build` and `dotnet test` was preceded by `heavylock.sh check avowry play-tracker`. It printed `free` every time, so no run was held back; the seventh mutant was rerun in full.