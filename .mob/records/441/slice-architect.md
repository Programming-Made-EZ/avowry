# Card 441 architect slice: a real-shape corpus and the MINOR shapes of card 439

Architect: claude-opus-5-5 (D264), ctx-1, read-only.

Every number below was MEASURED in `<scratchpad>/probe439` (net8.0, RollForward Major), which compiles:
- the card 441 reference, `PathScan.cs`;
- the product, `tools/Avowry.Bench/PathScrub.cs` at c569d86, copied as `PathScanV4.cs`;
- the pre-435 copy (`pre435`);
- the replaced regex at 7ee5411 (`regex`);
- card 439's 405 committed test rows, both forms, extracted from `PathScan439.cs` (probe mode `committed`).

The heavy lock was checked before every build (free each time). No test was run.

Path examples here are split strings (`"/home/" + "<user>"`). The corpus file uses the `<user>` placeholder.

Assumes: no invariant change.
Inputs: no list input.

## 1. The real-shape corpus (D263): `corpus441.txt`

The file has one row per line, TAB-separated: `family`, `input`, `expected`.
- Each row runs twice: as written, and with `<user>` replaced by a stand-in built at run time (`aa` + `bb`), with `expected` replaced the same way.
- Every row must also be stable on a second pass.
- Lines starting with `#` are comments. A `# pinned:` comment sits above each row that keeps an older limit, and gives the reason.
- Two rows hold a literal name, so they belong in the test code as split strings, not in the file:
  - `"-v /home/" + "u1a:/data"` → `-v <path>:/data` (the killing row of `ownletter-a-letters-only`);
  - `"see https://a/" + "Users/x ok"` (already a PathScrubForms row).

Rows are counted once per input line, and as both forms in brackets. An input without `<user>` runs once.

| Family | Rows (forms) | Shapes | Design leaks | pre435 leaks | Regex leaks | Pass-2 changes | Differs from V4 |
|---|---|---|---|---|---|---|---|
| compiler and linker | 17 (34) | gcc and clang `-I` `-L` `-isystem` `-idirafter` `-iquote` `-Wl,-rpath,`; a gcc diagnostic and `In file included from`; ld; MSVC `/I` `/Fo` `/Fe:` `/I `; cl and LINK errors; csc `/out:`; `CFLAGS=` | 0 | 14 | 6 | 0 | 6 (fixes) |
| environment lists | 13 (26) | `PATH`, `PYTHONPATH`, `CLASSPATH`, Windows `Path` and `PSModulePath`, `HOME`, `USERPROFILE`, `MAIL` (`/var/mail/`, `/var/spool/mail/`), `OLDPWD`, `XDG_DATA_DIRS`, `GOPATH`, `export JAVA_HOME=` | 0 | 4 | 4 | 0 | 4 (fixes) |
| container volumes | 13 (26) | docker and podman `-v` (Linux, `C:\`, `C:/`, `/mnt/c/`, `/c/`, `//c/`, `\\wsl$`), `--mount src=`, compose list items, quoted specs | 0 | 0 | 0 | 0 | 0 |
| passwd lines | 9 (15) | Linux and macOS lines, `/root`, `/nonexistent`, a trailing slash, an empty shell | 0 | 0 | 0 | 0 | 0 |
| JSON logs | 11 (22) | path fields, Windows `\\`, escaped slashes `\/home\/`, `\/Users\/`, argv arrays, a `\n` stack string, `file:///`, an escaped regex | 0 | 4 | 4 | 0 | 4 (fixes) |
| git, ssh, scp, rsync | 11 (21) | `ssh://host/…`, `ssh://git@host:2222/…`, `sftp://`, `host:/…`, `git@host:/…`, `file:///…`, rsync and git errors | 0 | 6 | 6 | 0 | 6 (fixes) |
| stack traces | 10 (18) | .NET `in …:line`, Python `File "…"` (Linux and Windows), Java, Node `(…:1:7)`, Rust, `file:///…mjs` | 0 | 0 | 0 | 0 | 0 |
| MINOR shapes (card 439) | 20 (36) | section 3 | 10 (5 pinned rows) | 18 | 22 | 0 | 20 |

- **No new leak in any family.** Every design leak is a pinned row that also leaks in the regex. Two of them (`/tmp/x'y:` in both forms) were clean in pre-435 and leak in the regex since card 435: an older limit.
- **Outputs that differ from both a clean regex and pre-435** (12 inputs counting both forms), all deliberate:
  - gcc `-I… -L…`: the design keeps `-Iinclude -Llib` where the regex over-scrubs.
  - The escaped-regex JSON row keeps `\\d+`.
  - The device paths stay as card 439 pinned them.
  - Rule 4c below (`-v <path>:/data`).

## 2. Changes to the reference (each narrow, with a mutant and its killing row)

All 17 new mutants are killed by their named rows, in both forms where the row holds `<user>` (79 of 79 in total).

1. **Glued flags.**
   - (a) A dash flag is `-` plus one or more ASCII letters (was: one letter), so `-isystem`, `-idirafter` and `-iquote` glue to a root.
     - `"gcc -isystem/home/" + "<user>/include main.c"` → `gcc -isysteminclude main.c`. Mutant `flag-one-letter`.
   - (b) A slash flag (MSVC) is `/` plus one or two ASCII letters, and glues only to a backslash drive or a UNC root.
     - `"cl.exe /c /IC:\Users\" + "<user>\include main.c"` → `cl.exe /c /Iinclude main.c`. Mutant `flag-no-slash`.
     - `/IC:\…\include /FoC:\…\obj\main.obj` → `/Iinclude /Fomain.obj`.
     - Mutant `slash-flag-any-sep`: `docker run -v /pj:/app x` stays unchanged (mutant: `/papp`).
     - Mutant `slash-flag-any-length`: `see /proj:\x` stays unchanged (mutant: `/prox`).
   - (c) `~` keeps one-letter flags only. Mutant `tilde-flag-multi`, row `"-I/home/" + "<user>/src~\nx.cs"` → `-Isrc~\nx.cs`; the mutant changes it on pass 2.
   - (d) The folder-name split also takes a multi-letter flag.
     - `"clang -idirafter/home/" + "<user>/inc -iquote/home/" + "<user>/q a.c"` → `clang -idirafterinc -iquoteq a.c`. Mutant `flag-split-one-letter`.
   - The flag letters are scanned back once per root candidate, and the runs never overlap, so the scan stays linear.
2. **New Unix markers: `/var/mail/` and `/var/spool/mail/`.** The file under them is named after the account.
   - `"MAIL=/var/mail/" + "<user>"` → `MAIL=<path>`. Mutant `no-var-mail`.
3. **New roots.**
   - (a) **JSON escaped slashes.** `\/home\/` and `\/Users\/` are account markers, read at a `\` that does not follow a word character, `.` or `\`.
     - `{"file":"\/home\/` + `<user>\/x.cs"}` → `{"file":"x.cs"}`. Mutant `json-slash-off`.
   - (b) **Long-form UNC.** `\\?\UNC\server\share\…` is a UNC path.
     - `"\\?\UNC\server\share\Users\" + "<user>\x.cs"` → `x.cs`. Mutant `unc-long-off`.
     - The device prefixes `\\?\C:\` and `\\.\C:\` keep their pinned outputs.
   - (c) **Named home `~name/`.** The name (letters, digits, `_ . -`, or the placeholder) is never output.
     - `~<user>/x.cs` → `x.cs`. Mutant `tilde-name-off`.
     - A bare `~name` with no separator after it stays text.
   - (d) **SSH URLs.** A path right after `ssh://[user@]host[:port]` or `sftp://…` starts a root and keeps its leading `/`.
     - `"git remote add origin ssh://host/home/" + "<user>/repo.git"` → `… ssh://host/repo.git`.
     - Mutant `ssh-host-off` (the account leaks). Mutant `ssh-no-slash` (`ssh://hostrepo.git`).
     - `https://` URLs stay untouched (a PathScrubForms row).
   - (e) **No drive right after `/`.** A bare `X:/` directly after `/` is not a drive (`file:///C:/` is read at its `f`).
     - `"a/b:/home/" + "<user>"` → `a/b:<path>` (V4 leaked `a/` + name). Mutant `drive-after-slash-ok`.
4. **The one-letter rule.**
   - (a) **Unchanged, minus the redundant `j > finalStart`.** That condition is equivalent: a final name starts after a separator, so the character before it is never a word character.
     - Killing row of `ownletter-a-letters-only`: `"-v /home/" + "u1a:/data"` → `-v <path>:/data` (mutant: `<path>a:/data`, then pass 2 changes).
   - (b) **Widened (round 4 finding 1).** In a drive or UNC chain, a first-letter final name after at least one folder keeps its letter before `:/`, but only when a Unix root follows (`UnixRootAt`: the walk to a marker, else a fixed root).
     - `"C:\tmp\x:/home/" + "<user>"` → `x:<path>`.
     - `"\\server\share\a:/home/" + "<user>"` and `"C:\Users\" + "<user>\a:/home/" + "<user>"` → `a:<path>`.
     - Mutant `ownletter-b-slash-only`: the row `C:\tmp\x:/home/` + name leaks under it.
     - Mutant `ownletter-b-any-target`: `C:\a\D:/b` gives `<path>b`, stable (mutant: `<path>:/b`).
     - The blow-up row `\\C:/a\n` is untouched: its host `C:` is no folder.
   - (c) **New (round 3 finding 4, round 4's `v:/v`).** A one-letter final name kept before `:/` with no Unix root after the `:/` becomes `<path>`, because `x:/data` would be read as a drive on pass 2.
     - `-v ~/x:/data` → `-v <path>:/data`; `/tmp/x:/x` → `<path>:/x`; `"-v /home/" + "<user>/v:/v"` → `-v <path>:/v`. All are stable (V4 changed on pass 2).
     - Mutant `one-letter-pathmark-off`.
   - Linearity: `UnixRootAt` walks forward only until the next `:` (a NameStop), so each character is read at most once more.

## 3. Every MINOR shape of card 439's rounds 2 to 4

Outputs are from the placeholder form; the stand-in form is the same with the name replaced.

| Round, finding | Shape | Disposition | Card 441 output |
|---|---|---|---|
| R2 3 | MSVC glued `/I`, `/Fo` | FIXED, rule 1b | `/Iinclude /Fomain.obj` |
| R2 3 | `-isystem` + home | FIXED, rule 1a | `-isysteminclude` |
| R2 3 | `~name/` | FIXED, rule 3c | `x.cs` |
| R2 3 | JSON `\/home\/…` | FIXED, rule 3a | `{"file":"x.cs"}` |
| R2 3 | `\\?\UNC\server\share\Users\` + name | FIXED, rule 3b | `x.cs` |
| R2 3 | `https://example.com/home/` + name `/x` | PINNED | unchanged (a URL is a web path; a PathScrubForms row keeps `"https://example.com/" + "home/x"`; the regex and pre435 leak alike) |
| R2 3 | `a/b:/home/` + name | FIXED, rule 3e | `a/b:<path>` |
| R2 3 | `/tmp/x:` + home, `…/a:/mnt/c/Users/` + name | already fixed in card 439 | `x:<path>`, `a:<path>` |
| R2 4 | `\\?\C:\…`, `\\.\C:\…` | PINNED, as ruled | `'\\?\x.cs'`, `<path>x.cs` |
| R3 4 | `-v ~/x:/data`, `/tmp/x:/x` | FIXED, rule 4c | `-v <path>:/data`, `<path>:/x`, stable |
| R3 6 | `/tmp/x://server/home/` + name | PINNED (contrived) | `<path>://server/home/<user>`, which leaks as pre435 and the regex do; now stable (V4 changed on pass 2) |
| R3 6 | `/tmp/x'y:/home/` + name | PINNED (contrived) | `x'<user>`, the regex's output; pre435 gave `x'y:<path>` |
| R4 1 | one-letter final in a drive or UNC chain before `:/` + home | FIXED, rule 4b | `x:<path>`, `a:<path>` |
| R4 2 | `ownletter-a-letters-only` | killing row added | `-v <path>:/data` |
| R4 3 | redundant `j > finalStart` | REMOVED | every committed row unchanged |
| R4 4 | `MAIL=/var/mail/` + name | FIXED, rule 2 | `MAIL=<path>` |
| R4 4 | `ssh://host/home/` + name `/repo.git` | FIXED, rule 3d | `ssh://host/repo.git` |
| R4 | `-v /home/` + name `/v:/v` | FIXED, rule 4c | `-v <path>:/v` |
| named | 2.5n `C:\Users\` + name + ` bb: access denied` | PINNED | ` bb:` stays (prose; data row) |
| named | 9b `C:\Users\` + name + ` 1's~/Users\` + name … | PINNED | the regex's leak; `~` after `s` is no root |
| named | c11 plain form, home + name + `:` + home + name | now `<path>:<path>` in both forms | the one-letter limit of ruling 4 (fix round 2) is gone |
| named | 3.4d, 3.5e, 3.8a-d | unchanged | as ruled |

## 4. Regression checks (card 441 reference)

- **Committed card 439 rows:** all 405 (both forms) equal V4 and their expected outputs, and are stable on a second pass. The 11 blow-up units at 20k are unchanged and stable.
- **Data sets:** 239 of 239 rows (both forms) equal today.
- **Review inputs:** the 100 review inputs are unchanged (2 leaks, 0 second-pass changes).
- **Round 1 to 3 inputs** (190 counting both forms):
  - leaks: 6, down from V4's 14 (9b, `x://`, `x'y:`, each in both forms);
  - second-pass changes: 0, down from 4.
- **Mutants:** 79 of 79 killed by their named rows (62 from card 439, 17 new).
- **Sweeps** (100,000 inputs each):

| Sweep | Leaks | New leaks | Second-pass changes |
|---|---|---|---|
| A | 0 | 0 | 0 (0 differences from the regex) |
| B | 4,559 | 0 | 0 |
| C | 2,679 | 0 | 0 |

## 5. Time (ms, minimum of runs; evidence only, D254)

The machine was loaded: the V4 column of `/=-I` went from 2.3 ms in fix round 3 to 5.7 ms here.

| Family shape, repeated | 20k: V4 / design | 1M: V4 / design |
|---|---|---|
| `-isystem` + home | 0.7 / 0.67 | 33 / 36 |
| MSVC `/IC:\…\inc /FoC:\…` | 0.7 / 1.07 | 38 / 51 |
| `MAIL=/var/mail/` + name | 0.9 / 0.43 | 44 / 24 |
| `-v C:\Users\` + name + `\p:/home/` + name | 0.6 / 0.64 | 31 / 35 |
| a passwd line | 0.7 / 0.63 | 27 / 36 |
| JSON `\/home\/` + name | 0.8 / 0.47 | 42 / 24 |
| `ssh://git@h:22/home/` + name | 0.7 / 0.81 | 40 / 40 |
| a .NET trace line | 0.6 / 0.46 | 27 / 26 |
| `~` + name `/x` | 0.9 / 0.31 | 42 / 18 |
| `\\?\UNC\…` | 0.5 / 0.50 | 25 / 28 |
| `/tmp/x:` list | 1.3 / 2.65 | 58 / 124 |
| adversarial `/t/x:/a/a/…` | 1.3 / 1.99 | 58 / 112 |
| adversarial one long `/t/x:/a/a/…` | 0.7 / 2.22 | 44 / 115 |
| `/=-I` | 5.7 / 7.46 | 75 / 98 |

- Further shapes, all linear:
  - adversarial: `-aaaa…/`, ` -abcdefgh/` repeated, one long ssh host, `ssh://h/` repeated, a `~aaaa…` run, a `\/home\/` run;
  - the 22 card 439 slice shapes: at most 4.3 ms at 20k and 106 ms at 1M under this load.
- Every 5k → 10k → 20k series about doubles.
- **The cost of rule 4:** a one-letter colon chain reads its next entry twice, so it costs about twice V4.

## 6. Points to rule on

1. **SSH URLs (rule 3d).** The design keeps the leading `/` (`ssh://host/repo.git`); without that it would give `ssh://hostrepo.git`, the plain short-name form. Adopt?
2. **Rule 4c's outputs.** It turns V4's unstable `x:/data` into `<path>:/data`, which keeps the `:/data` the regex drops (`<path>data`). Nothing was committed for these shapes, so no pinned row changes. Adopt?
3. **The slash-flag limits (rule 1b).** One or two letters, before `X:\` or `\\` only, so `/proj:/app` and `/pj:/app` stay text. `/LIBPATH:C:\…` needs no rule: it is already a root after `:`.
4. **Rows that cannot live in the corpus file.** The `u1a` killing row and the `https://a/` + `Users/x` row hold a literal name, so the red writes them as split strings in test code.
Size: 13582 bytes
