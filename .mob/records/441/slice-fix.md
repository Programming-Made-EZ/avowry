# Card 441 fix round 1: the reference scanner after review round 1

Architect (claude-opus-5-5, ctx-1), read-only. All numbers MEASURED in `<scratchpad>/probe439` after the heavy lock cleared; it was checked before every build and long run, and printed `free` each time from the first build on. `PathScan.cs` is fixed in place; the previous version is `PathScanV5.cs` (equal to the green at ede15fd on all 207 committed card 441 rows). Path examples are split strings.

## 1. Changes to the design
1. **Rule 3e narrowed (finding 1).** A bare `X:/` right after `/` is still refused (`"a/b:/home/" + "<user>"` gives `a/b:<path>`) except in two cases:
   - **(a)** its first folder is a drive account parent. `DriveParents` gains `documents%20and%20settings`, the `file:` URI spelling.
   - **(b)** it starts exactly where the previous path ended.

   With (a): `"jar:file:/c:/users/" + "<user>/.m2/a.jar!/x.class"` gives `jar:file:/x.class`, and `"java.io.FileNotFoundException: /c:/users/" + "<user>/x.txt"` gives `/x.txt`. The same holds for `file:/C:/Documents and Settings/…`, `file:/C:/Documents%20and%20Settings/…`, `///c:/users/…`, `"/c:/users/…"`, `/C:/USERS/…` and `vscode-remote://wsl+Ubuntu/c:/users/…`. All equal pre435 and the regex.
   - MINOR 8 is fixed too: `"ssh://host/C:/Users/" + "<user>/r.git"` gives `ssh://host/r.git`.
2. **Escaped colon lists (finding 2).** `X:\/` is an escaped `X:/`: the one-letter rule (`ColonSlash`) reads it as `:/`, and adjacency no longer treats it as a backslash drive.
   - `{"env":"PYTHONPATH=\/home\/` + `<user>\/lib:\/home\/` + `<user>"}` gives `{"env":"PYTHONPATH=lib:<path>"}`.
   - Volume, PATH, passwd, Binds and mount values read as lists, like their unescaped forms.
3. **The forward-slash volume spec (finding 3).** This is rule 1(b). `docker run -v D:/a:/a x` gives `docker run -v <path>a x`, stable, as the previous scanner gave. So do `-v C:/Users/` + name + `/d:/data`, compose `- C:/work/d:/data`, `file:///C:/a/b:/x` and `-IC:/x/I:/y`.

## 2. Mutants: 86 of 86 killed by their named rows, both forms
| Mutant | Killing row | Design → mutant |
|---|---|---|
| `drive-after-slash-any` | `"java.io.FileNotFoundException: /c:/users/" + "<user>/x.txt"` | `/x.txt` → leaks |
| `drive-after-slash-adjacent-refused` | `docker run -v D:/a:/a x` | `<path>a` → `<path>a:/a`, which changes on pass 2 |
| `percent-das-off` | `"file:/C:/Documents%20and%20Settings/" + "<user>/x.jar"` | `file:/x.jar` → leaks |
| `json-colon-backslash` | the escaped PYTHONPATH row above | `lib:<path>` → leaks |
| `tilde-name-no-dot` (reviewer) | `~aa.bb/x.cs` | `x.cs` → unchanged |
| `ssh-host-no-dot` (reviewer) | `"git clone ssh://git@example.com/home/" + "<user>/r.git"` | `…example.com/r.git` → leaks |
| `unc-slash-flag-off` (reviewer) | `"cl.exe /I\\server\share\Users\" + "<user>\inc a.c"` | `cl.exe /Iinc a.c` → leaks |

`drive-after-slash-ok` still has its row: `a/b:` + home gives `a/b:<path>`.

## 3. Corpus rows added (`corpus441.txt`, under `# fix round 1`)
| Family | Rows (forms) | Leaks | Pass-2 changes | Equal to pre435 and the regex |
|---|---|---|---|---|
| `java-uri` (Java `File.toURI`, `jar:`, `file:/`, `vscode-remote`, NIO errors, `ssh://host/C:/…`) | 11 (22) | 0 | 0 | yes (both scrub these) |
| `json-escaped` (env, volumes, PATH, file lists, a passwd line, `Binds`, `mount`) | 7 (14) | 0 | 0 | they leak the whole value; the design scrubs it |
| `volume-winslash` (`-v D:/a:/a`, `C:/Users/…/d:/data`, `C:/src/d:/data`, `c:/x/v:/v`, compose, `file:///…/b:/x`, `-IC:/x/I:/y`, `C:/Users/…/proj:/app`) | 8 (10) | 0 | 0 | yes, the regex's stable output |

The whole corpus file (11 families, 130 rows) gives no unexpected output. Leaks remain only in the 5 pinned MINOR rows; there is no second-pass change.

## 4. Everything that held
- **Card 439:** 405 of 405 committed rows (both forms) equal to the product and their expected outputs; the 11 blow-up units unchanged and stable.
- **Card 441:** 207 of 207 committed rows equal to V5 and their expected outputs, stable.
- **Data sets:** 239 of 239 equal today.
- **Review inputs:** the 100 review inputs are unchanged (2 leaks, 0 second-pass changes).
- **Round 1 to 3 inputs** (190 counting both forms): 6 leaks, 0 second-pass changes.
- **Sweeps:** A has 0 leaks, 0 second-pass changes and 0 differences from the regex. B has 4,559 leaks and C has 2,679, each with 0 new leaks and 0 second-pass changes.

## 5. Time (ms; evidence only, D254)
| Shape, repeated | 20k: previous / design | 1M: previous / design |
|---|---|---|
| `jar:file:/c:/users/` + name `/a.jar!/x` | 0.5 / 0.36 | 20 / 30 |
| JSON `\/home\/` + name + `\/lib:` | 0.8 / 0.39 | 38 / 17 |
| `-v D:/a:/a` | 0.4 / 0.75 | 28 / 36 |
| adversarial `/a:` | 1.1 / 1.84 | 43 / 71 |
| adversarial `/c:/users/` | 0.3 / 1.30 | 25 / 56 |

All shapes are linear: the worst is 3.7 ms at 20k (`/=-I`) and 77 ms at 1M, and the 22 slice shapes take at most 2.1 ms and 71 ms. `DriveParentAhead` reads a constant number of characters.

## 6. Points to rule on
1. **The `%20` spelling.** Only `documents%20and%20settings` is decoded; no other percent-encoding is (for example `%5C`). A general URI decode would be a new card.
2. **Rule 1(b) after a previous path.** It allows any `X:/` there, not only before an account parent. That is what makes the volume specs stable; it matches the previous scanner.
Size: 5401 bytes
