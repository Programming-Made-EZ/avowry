# Card 439 fix round 2: the reference scanner after review round 2

Architect (claude-opus-5-5, ctx-1), read-only; all numbers MEASURED in `<scratchpad>/probe439`. `PathScan.cs` is fixed in place; round 2's reference is kept as `PathScanV2.cs`. Columns: pre435 = PathScrubBefore435, regex = PathScrub at 7ee5411. Path examples are split strings (`"/home/" + "<user>"`) for the home-path hook.

## 1. Changes to the design
1. **Adjacency narrowed (findings 1 and 2).** Start rule 5 (a drive starts where the previous path ended) holds after a drive or UNC path as before. After a Unix or `~` path it holds only for `file:///` or a backslash drive (`X:\`). A letter before `:/` after a Unix name is a colon list, not a drive.
   - Rows 3.4f, 3.4h and 3.4i still pass: `<path>x.cs`, `\n<path>`, `\ta.x.cs`.
2. **One-letter final name (finding 3, a trivial measured fix).** In a Unix or `~` chain, a final name never stops before its own first letter when that letter is followed by `:/`.
   - `"/tmp/x:/home/" + "<user>"` gives `x:<path>` (regex: leaks; pre435: `x:<path>`).
   - `"/home/" + "<user>/a:/mnt/c/Users/" + "<user>"` gives `a:<path>` (regex: leaks; pre435: the same as the design).
3. **The `/mnt/` candidate is removed from `AccountMarkerAt` (finding 6).**
   - It was dead: the walk passes `/mnt` and finds `/x/users/` with the same end.
   - Every row, mutant and sweep is unchanged without it, and the per-step array allocation goes too.

New mutants (each killed by its row in both forms):
- `adjacency-any` → `"PATH=/home/" + "<user>/bin:/home/" + "<user>/.local/bin:/usr/bin"` gives `PATH=bin:bin:/usr/bin` (mutant `PATH=bibibin`).
- `final-start-root-stop` → `"/tmp/x:/home/" + "<user>"` gives `x:<path>` (mutant `<path><user>`).

## 2. Finding 5: the seven surviving mutants, with killing rows (all killed, both forms)
| Mutant | Killing row | Reference (equal to the regex unless noted) |
|---|---|---|
| `wordends-no-dash` | `"C:\Users\" + "<user> smith-jones"` | `<path>` |
| `drive-parents-take-home` | `C:\home\` + `x.cs` | `x.cs` (*) |
| `guard-letter-esc-gone` | `\\C:\u00e9x\y.cs` | `\\<path>` (regex `<path>y.cs`) |
| `unc-host-space-ok` | `\\ server\share\x.cs` | unchanged |
| `flagleft-no-semicolon` | `"CFLAGS=-O2;-I/home/" + "<user>/inc"` | `CFLAGS=-O2;-Iinc` (regex leaks) |
| `root-ahead-tilde-gone` | `C:\a b -I~/x.cs` | `a b -Ix.cs` (regex `x.cs`) |
| `stopat-apos-gone` | `"C:\Users\" + "<user> x2'b"` | `<path> x2'b` |

(*) I do not take the reviewer's row `D:\home\` + name, which expects the account name to survive: a test would pin a leak. `C:\home\` + `x.cs` kills the same mutant without pinning one.

## 3. Regression checks
- Data sets: 239 of 239 rows (both forms) equal today; second pass stable.
- The 100 review inputs: every output equal to V2; 2 leaks; 0 second-pass changes.
- The round-1 inputs and the reviewer's mutant rows are unchanged.
- Mutants: 57 of 57 killed by their named rows (48, plus 7 of finding 5, plus 2 new).
- Sweep A: 0 leaks, 0 second-pass changes, 0 differences from the regex.
- Sweep B: 4,559 leaks, 0 new against pre435, 0 second-pass changes.
- Sweep C: 2,679 leaks, 0 new, 0 second-pass changes.

## 4. Every colon-list input (findings 1 and 2), against the fixed reference
All equal the regex and pre435, with no leak and a stable second pass:
- docker `-v …/work:` + home → `docker run -v work:<path>` (the tail takes ` -it ubuntu`, as the regex does)
- `PYTHONPATH` → `lib:<path>`
- Git Bash `PATH` → `bin:/usr/bin:<path>`
- `GOPATH` → `go:<path>`
- `NODE_PATH` → `node_modules:<path>`
- `CDPATH` → `.:src:<path>`
- `CLASSPATH` → `a.jar:classes:<path>`
- `-Djava.class.path=` → `a.jar:<path>`
- rsync `change_dir` → `"src:<path>"`
- compose `volumes: […]` → `[data:<path>]`
- home`:`home → `<path>:<path>`
- `PATH=…/bin:…/.local/bin:/usr/bin` → `bin:bin:/usr/bin`
- `LD_LIBRARY_PATH` → `lib:lib`
- `- …/.config:/root/.config` → `.config:.config`
- `~/bin:~/lib:` + home → `bin:lib:<path>`
- `/tmp/a:C:/Users/` + name + `/x.cs` → `a:x.cs`

## 5. Findings 3 and 4: older named limits (no change)
These leak exactly as the regex and pre435 do:
- `a/b:` + home
- MSVC glued `/I` and `/Fo`
- `-isystem` + home
- JSON-escaped slashes `\/home\/…`
- 9b from round 1

Fixed by rule 2: `/tmp/x:` + home, `…/a:/mnt/c/Users/` + name.

Finding 4: `'\\?\C:\Users\` + name + `\x.cs'` gives `'\\?\x.cs'`, and `\\.\C:\Users\` + name + `\x.cs` gives `<path>x.cs`. Neither leaks and both are stable; they differ from the regex. I propose them as corpus rows that pin these outputs.

## 6. Time (ms, minimum of runs; evidence only, D254)
| Shape | 20k | 1M |
|---|---|---|
| `PATH=` + `…/bin:` repeated | 0.37 | 15.9 |
| `/tmp/x:` repeated | 0.48 | 23.8 |
| `a/b:` repeated | 0.28 | 13.2 |
| `/tmp/a:C:/b` repeated | 0.43 | 20.6 |
| `/=-I`, `/(-I` | 2.21, 0.60 | 32.0, 31.2 |

Re-timed: the 22 slice shapes (at most 1.0 ms at 20k and 43 ms at 1M) and every round-1 shape. All are linear.

## 7. Points to rule on
1. Finding 5's `drive-parents-take-home`: should its killing row be `C:\home\` + `x.cs` (my choice, no pinned leak) or the reviewer's row?
2. Finding 4: pin the device-path outputs as corpus rows, or leave them unpinned?
3. Rule 2 goes beyond "older limits stay": it is a one-line fix with a mutant, it removes a regex leak, and it matches pre435.
Size: 5377 bytes
