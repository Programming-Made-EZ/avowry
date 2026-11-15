# Card 439 fix round: the reference scanner after review round 1

Architect (claude-opus-5-5, ctx-1), read-only; all numbers MEASURED in `<scratchpad>/probe439`. `PathScan.cs` is fixed in place; the round-1 reference is kept as `PathScanV1.cs`. Columns: pre435 = PathScrubBefore435, regex = PathScrub at 7ee5411 (byte-equal to the probe's `PathScrubToday`).

## 1. Changes to the design
1. **Walk memo (finding 1).** The walk depends only on its position, so a Unix walk starting on a `/` inside the last failed walk's span (`deadFrom..deadTo`, its start to its last visited `/`) fails the same way and is skipped; the fixed roots are still tried. Every character is walked at most once, whatever starts (glued flags after `=` or `(`) occur inside a span. Outputs are unchanged on every row; this is not a behaviour rule, so its only evidence is time (section 3).
2. **Drive account parent is unguarded (finding 4).** The first folder after a drive, when in `DriveParents`, takes any separator run after it without the escape guard, as the regex's `(?<home>users)Separator` did: `C:/Users\` + `nate\x.cs` is a mixed-slash path, not a path ended by `\n`.
3. **Opener bridge (finding 6).** A folder that ends in whitespace and `(` marks the chain bridged: `see C:/temp/x.cs (/home/<user>)` → `see <path>)`, the same output as row 2.6e.
4. **Glued word may open with `'`** before a letter or a letter escape (reviewer mutant row 7).

New mutants (each killed by its row, both forms):
- `drive-parent-guard` → `C:/Users\` + `nate\x.cs` gives `x.cs` (mutant `Users\nate\x.cs`).
- `no-opener-bridge` → `see C:/temp/x.cs (/home/<user>)` gives `see <path>)` (mutant leaks).
- `glued-apos-first` → `C:\Users\<user>(bb)'\u00f6cc` gives `<path>` (mutant `<path>'\u00f6cc`).
- The memo has no output-visible mutant; `walk-no-memo` is the V1 column of section 3.
- The killing row of `unix-no-letter-users` becomes `/tmp/a./c/users/<user>` → `<path>`, because the opener bridge now also covers the old row.

## 2. Regression checks against the fixed reference
- Data sets: 239 of 239 rows, both forms, equal today; second pass stable.
- The 100 review inputs (both forms): every output equal to V1 (only the 3 new rows above differ); leaks 2, second-pass changes 0.
- Mutants: 48 of 48 killed by their named rows (45 plus 3 new).
- Sweep A: 0 leaks, 0 second-pass changes, 0 differences from the regex on all 100,000 inputs.
- Sweep B: 4,559 leaks, 0 new against pre435, 0 second-pass changes.
- Sweep C: 2,679 leaks (was 2,904), 0 new, 0 second-pass changes.

## 3. Time (ms, minimum of runs; evidence only, D254)
| Shape | 5k | 10k | 20k | 1M | V1 at 20k |
|---|---|---|---|---|---|
| `/=-I` | 0.89 | 1.06 | 1.68 | 51.9 | 980 |
| `/(-I` | 0.37 | 0.61 | 1.07 | 48.0 | 981 |
| `/a=-I/a(-L` | 0.21 | 0.42 | 1.39 | 50.6 | 736 |
| `C:\a` + spaces | 0.20 | 0.19 | 0.29 | 16.5 | 0.5 |
| `/tmp/a` + spaces | 0.16 | 0.27 | 0.29 | 21.0 | 0.5 |
| `C:\a` + tabs | 0.09 | 0.14 | 0.32 | 22.2 | 0.6 |
| `C:\a   ` repeated | 0.27 | 0.58 | 1.46 | 32.5 | 1.2 |
| `/tmp/a -I/x` repeated | 0.33 | 0.60 | 0.83 | 58.9 | 0.9 |

The 22 card-439 slice shapes, rerun on the fixed reference: every one is at most 2.0 ms at 20k and at most 74 ms at 1M.

## 4. Every finding against the fixed reference
| # | Fixed reference | Disposition |
|---|---|---|
| 1 | linear (section 3) | FIXED (memo) |
| 2 | not reproduced with a root before the run: 0.29 ms at 20k, 16.5 ms at 1M | no defect in the reference; the green's own whitespace look-ahead |
| 3 | sweep A: equal to the regex on 100,000 of 100,000. Against pre435: 50,110 differ, 18,576 where pre435 was clean and stable; all equal the regex's outputs (card 435's accepted behaviour, e.g. pre435 `see u0022`, now `see Temp\u0022`), so none is a regression under D259 clause 4. Inputs 3a-3i equal the regex: `\ty.cs`, `x.cs`, `Candidate.cs(7,1): …`, `UsersController.cs`, `O'Brien.cs end`, `a+b.cs end` | green only |
| 4 | `x.cs` (`C:/Users\` + `riley` → `<path>`); `/home/<user>\notes\run-426` → `<path>\notes\run-426`, as the regex, no leak | FIXED |
| 5 | equal to the regex: `x.cs`, `x.cs`, `<path>` | green only |
| 6 | `see <path>)` for both inputs | FIXED (older MINOR) |
| 7 | `{"pattern":"\\d+","path":"x.cs"}` (keeps `\\d+`, which the regex lost); #3388 → `\\<path>(7,1)\u0022`; #2978 → `[<path>\nx.cs`; all stable | green only |
| 8 | equal to the regex on all five (`<path> [Verification.csproj]`, `<path> .\nnext`, …) | green only |
| 9 | 9a → `<path>`; 9b `C:\Users\<user> 1's~/Users\<user> …` leaks, as pre435 and the regex do: `~` after `s` is not a root, and `1's` is no tail word | named limit (contrived, MINOR, stable) |
| 10 | the reference is 404 lines, its probe-only mutant switches included | green only |

The reviewer's nine mutant rows, from the reference: `Candidate.cs`; `x.cs`; `gcc a.c -Iinclude`; `a\\<path>`; `a\\<path>`; `<path>`; `<path>` (new rule 4); `<path>1:\a`; `<path>file:/a`. The red can add them as rows.

## 5. Points to rule on
1. D261 asks for a mutant per rule; the memo changes no output, so its evidence is time only. The alternative is a step counter a test can read (not wall-clock).
2. Finding 4's `/home/<user>\notes\run-426` stays `<path>\notes\run-426`, as the regex gives (pre435 gave `run-426`): there is no leak.
3. 9b stays a named limit.

Size: 5341 bytes
