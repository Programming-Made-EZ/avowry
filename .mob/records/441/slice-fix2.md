# Card 441 fix round 2: the reference scanner after review round 2

Architect (claude opus = claude-opus-5-5, ctx-1, D264), read-only for the repository. All numbers are MEASURED in `<scratchpad>/probe439`. The heavy lock was checked before every build and long run. While play-tracker's #1017 E2E held it, I waited and checked again. One breach: a chained command ran a short `f2cmp` pass (a few seconds, the old binary) right after the check printed `busy`; its build step did not run. I re-ran that pass after the lock cleared. Path examples are split strings.

## 0. The saved version
`PathScanV6.cs` is `PathScan.cs` as it was before this round, with only the class renamed. MEASURED: it equals the green port at 389d790 (`tools/Avowry.Bench/PathScrub.cs`, copied into the probe as `PathScrubGreen389`) on 1,918 rows (both forms):
- the 139 rows of card 441's committed test, extracted from `PathScan441.cs`;
- the 130 corpus rows;
- card 439's 405 rows and 435's 239 data rows;
- the review, review-2 and mutant rows.

The 11 blow-up units are also equal. Count of differences: 0.

## 1. Changes to the design
1. **Finding 1: a separator run after a drive after `/`.** `DriveParentAhead` skips the whole separator run (`/` and `\`) before it looks for a drive account parent. With this, `"/C://Users//" + "<user>//x"` gives `/x`; `"jar:file:/c:///users/" + "<user>/x"` gives `jar:file:/x`; and `"x/c:/\users\" + "<user>\x"` gives `x/x`. Both earlier scanners give the same output.
   - This is the reviewer's checked fix as given. I added a second mutant to it (`fix2-run-slash-only`) so that the mixed `/\` run is tested too.
   - The scan stays linear: each drive candidate skips only the run that follows its own `:/`.
2. **Finding 2: a lone letter before an escaped `:\/`.** The chain rule that turns a lone one-letter final name before `:/` into `<path>` is now `LoneLetterColon`, and it also accepts the escaped `:\/`. `{"Binds":["\/home\/` + `<user>\/d:\/data"]}` gives `{"Binds":["<path>:\/data"]}`, which is stable on pass 2 (it was `d:\/data`, then `data`).
   - This is better than the reviewer's fix. The rule keeps the letter when an escaped JSON root (`\/home\/` or `\/Users\/`) follows, just as the unescaped rule keeps it before a Unix root.
   - So `{"volumes":["\/home\/` + `<user>\/d:\/home\/` + `<user>"]}` keeps `d:<path>`, as V6 and the unescaped `/home/…/d:/home/…` do. Under the reviewer's fix it would become `<path>:<path>`. Mutant `fix2-escaped-root-ignored` pins this.
3. **MINOR 3: fixed with a narrow change.** An escaped `X:\/` that starts right where a word glued after an account tail ended now glues. The tail's `GluedWord` records that position.
   - `"C:\Users\" + "<user>(1)xC:\/a\/y"` gives `<path>y`, which is stable and equals ede15fd. V6 gave `<path>C:\/a\/y`, then `<path>y`.
   - No committed row changes. A pinned row was not possible here: the row changes on a second pass, and every corpus row asserts that it is stable.
4. **MINOR 4 and MINOR 5 stay pinned with their current output.**
   - MINOR 4 is ruling 1: only the full `%20` spelling is decoded.
   - MINOR 5: candidate `cand-minor5` (rule 3e for the escaped `\/X:\/`) was measured. It fixes the row (`{"PATH":"\/opt\/b:<path>"}`) but also changes m10's killing row `{"PATH":"\/opt\/x:\/opt\/y"}` from `\/opt\/y` to unchanged. That fails "nothing else changed", so it is not in the reference (see ruling 2).

## 2. Rows whose output changed (step 3)
Measured against V6: every committed card 441 row (139 test rows, 130 corpus rows), card 439's 405 and 435's 239 rows, review and review-2 inputs, every older mutant row, and the 11 blow-up units' second pass. All are unchanged: 0 expected-output misses, 0 second-pass changes and 0 blow-up changes.

The only changed outputs are the rows of the findings and of MINOR 3 (38 forms, all listed in §3). In sweeps A, B and C (100,000 inputs each), the output differs from V6 in 0 inputs. The leak counts are unchanged: A 0, B 4,559, C 2,679, each with 0 new leaks and 0 second-pass changes.

## 3. Corpus rows added (`<scratchpad>/corpus441-fix2.txt`, 21 rows under `# fix round 2`)
| Family | Input | Expected |
|---|---|---|
| java-uri | `/C://Users//` + `<user>//x` | `/x` |
| java-uri | `file:/C://Users/` + `<user>/x` | `file:/x` |
| java-uri | `jar:file:/c://users/` + `<user>/x.class` | `jar:file:/x.class` |
| java-uri | `jar:file:/c:///users/` + `<user>/x` | `jar:file:/x` |
| java-uri | `jar:file:/C://Documents and Settings/` + `<user>/x` | `jar:file:/x` |
| java-uri | `x/c:/\users\` + `<user>\x` | `x/x` |
| java-uri | `file:/C://Documents%20and%20Settings/` + `<user>/x.jar` | `file:/x.jar` |
| json-escaped | `{"Binds":["\/home\/` + `<user>\/d:\/data"]}` | `{"Binds":["<path>:\/data"]}` |
| json-escaped | `{"Binds":["\/home\/` + `<user>\/d:\/data:ro"]}` | `{"Binds":["<path>:\/data:ro"]}` |
| json-escaped | `{"PATH":"\/home\/` + `<user>\/x:\/usr\/bin"}` | `{"PATH":"<path>:\/usr\/bin"}` |
| json-escaped | `{"env":"PYTHONPATH=\/home\/` + `<user>\/l:\/opt\/x"}` | `{"env":"PYTHONPATH=<path>:\/opt\/x"}` |
| json-escaped | `{"env":"PYTHONPATH=\/home\/` + `<user>\/l:\/home\/` + `<user>"}` | `{"env":"PYTHONPATH=l:<path>"}` |
| json-escaped | `{"mount":"\/Users\/` + `<user>\/s:\/src"}` | `{"mount":"<path>:\/src"}` |
| json-escaped | `{"volumes":["\/home\/` + `<user>\/d:\/home\/` + `<user>"]}` | `{"volumes":["d:<path>"]}` |
| json-escaped | `{"files":"\/home\/` + `<user>\/x:\/tmp\/y"}` | `{"files":"<path>:\/tmp\/y"}` |
| minor (MINOR 3, fixed; m4's row) | `C:\Users\` + `<user>(1)xC:\/a\/y` | `<path>y` |
| minor (pinned MINOR 4) | `file:/C:/Documents%20and Settings/` + `<user>/x` | unchanged (leaks; pre435 and the regex give `file:/x`) |
| minor (pinned MINOR 5) | `{"PATH":"\/opt\/b:\/home\/` + `<user>"}` | `{"PATH":"\/opt\/` + `<user>"}` (all three scanners leak) |
| env (m7) | `PATH=/opt/b:/userspace/x` | unchanged |
| java-uri (m10) | `x/c:\tools\` + `<user>\a.cs` | `x/a.cs` |
| json-escaped (m10) | `{"PATH":"\/opt\/x:\/opt\/y"}` | `{"PATH":"\/opt\/y"}` |

- **Corpus totals.** The corpus is now 151 rows in 11 families (no new family). Leaks remain only in the pinned `minor` rows (14 forms), and there are 0 second-pass changes.
- **Finding 1 rows.** The 7 rows equal pre435 and the regex.
- **Finding 2 rows.** For the 7 rows the earlier scanners leak, the design scrubs the value.
- **MINOR 3 row.** It equals ede15fd, since the regex leaves `xC:\/a\/y` as plain text.

## 4. Mutants: 93 of 95 killed by their named rows; 2 named equivalent
New mutants (this round):

| Mutant | Killing row | Design → mutant |
|---|---|---|
| `fix2-run-off` | `"jar:file:/c:///users/" + "<user>/x"` | `jar:file:/x` → leaks |
| `fix2-run-slash-only` | `"x/c:/\users\" + "<user>\x"` | `x/x` → leaks |
| `fix2-escaped-letter-off` | the Binds row | `<path>:\/data` → `d:\/data` (changes on pass 2) |
| `fix2-escaped-root-ignored` | the volumes `d:\/home\/` row | `d:<path>` → `<path>:<path>` |
| `fix2-minor3-off` | the MINOR 3 row | `<path>y` → `<path>C:\/a\/y` (changes on pass 2) |

The reviewer's survivors:

| Mutant | Killing row | Result |
|---|---|---|
| m7 `rev2-m7-parent-no-sep` | `PATH=/opt/b:/userspace/x` | unchanged → `PATH=/opt/x`: killed |
| m10 `rev2-m10-3e-any-sep` | `"x/c:\tools\" + "<user>\a.cs"` | `x/a.cs` → leaks: killed |
| m4 `rev2-m4-glue-reverted` | equivalent after the MINOR 3 fix | see below |
| m11 `rev2-m11-p-gt-prevend` | equivalent | see below |

- **m4 is equivalent after the MINOR 3 fix.** A drive starts at the previous path's end, right after an ASCII word character, only when a word glued after an account tail ended there. In every other case the final name keeps the letter as its own, because `OwnLetter` reads `X:\/` as `X:/`. That position now glues `X:\/` anyway.
- **m11 is equivalent, as the reviewer found.** In `Run`, `p` never falls below `prevEnd`.

All 86 older mutants are still killed by their named rows. Rows used: every committed row, the corpus with the new rows, the review rows and the mutant rows.

## 5. Time (ms, minimum of runs; evidence only, D254)
| Shape | 20k: V6 / fixed | 1M: V6 / fixed |
|---|---|---|
| `/C://Users//` + name + `//x` repeated | 3.43 / 1.67 | 29.8 / 15.6 |
| `jar:file:/c:///users/` + name + `/x` repeated | 0.49 / 0.37 | 24.9 / 18.2 |
| `x/c:/\users\` + name + `\x` repeated | 0.33 / 0.41 | 17.1 / 21.1 |
| adversarial `/c:` + one `/` run | 1.63 / 1.60 | 85.7 / 81.1 |
| adversarial `/c:` + one `/\` run | 1.02 / 1.02 | 51.5 / 54.9 |
| adversarial `/a://` repeated | 1.06 / 1.25 | 52.6 / 53.0 |
| adversarial `/a:/\` repeated | 1.00 / 0.96 | 47.3 / 50.4 |
| adversarial `x/c://user` repeated | 0.46 / 0.53 | 25.7 / 29.4 |
| Binds `\/home\/` + name + `\/d:\/data` repeated | 0.36 / 0.24 | 19.5 / 12.4 |
| escaped list `\/home\/` + name + `\/d:` | 0.44 / 0.21 | 23.0 / 11.5 |
| escaped list `d:\/home\/` + name | 0.38 / 0.17 | 20.5 / 8.9 |
| adversarial `\/a\/d:\/` repeated | 1.09 / 0.43 | 54.1 / 20.5 |
| adversarial `\/home\/x\/d:\/` repeated | 0.64 / 0.36 | 34.8 / 24.1 |
| adversarial `/tmp/d:\/` repeated | 0.69 / 0.72 | 43.2 / 39.5 |
| MINOR 3 `C:\Users\` + name + `(1)xC:\/a\/y` repeated | 0.51 / 0.63 | 27.0 / 26.3 |
| adversarial MINOR 3 chained `(1)xC:\/` + `Users\/a` | 0.42 / 0.51 | 23.9 / 27.6 |

All shapes are linear. The worst is 81 ms at 1M characters (one `/c:` before a single 1M-character `/` run) and 1.67 ms at 20k.

## 6. Points to rule on
1. **Finding 2: the escaped root after the letter.** The reference keeps the letter before `\/home\/` or `\/Users\/` (`d:<path>`), as the unescaped rule does before a Unix root. The reviewer's fix would give `<path>:<path>`. Both outputs are stable and neither leaks.
2. **MINOR 5 stays pinned.** The measured candidate (rule 3e for the escaped `\/X:\/` away from a previous path's end, before a non-account folder) fixes the leak and gives `\/opt\/b:<path>`. It changes no committed row, no sweep output and no blow-up row. It does change the uncommitted m10 row `{"PATH":"\/opt\/x:\/opt\/y"}` (`\/opt\/y` → unchanged, the same as its unescaped form). Accept it as a narrow fix, or keep it pinned (the current state)?
3. **An older sibling of MINOR 3 (forward slash), not added as a row.** `"C:\Users\" + "<user>(1)xC:/a/y"` gives `<path>C:/a/y`, then `<path>y`, in both V6 and the fixed reference. It does not leak, but it is unstable, so it cannot be a pinned corpus row. Gluing `X:/` after a glued word (`cand-minor3-slash`) fixes it, but it changes card 439's committed rows `adjacency-any` and `glue-after-drive-slash`: `"C:\Users\" + "<user>(bb)ccD:/Users/" + "<user>/x.cs"` would give `<path>x.cs` instead of `<path>D:x.cs`. I propose recording it as a named limit, or opening a new card.
