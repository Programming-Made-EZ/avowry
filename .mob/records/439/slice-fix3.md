# Card 439 fix round 3: the reference scanner after review round 3

Architect (claude-opus-5-5, ctx-1), read-only; all numbers MEASURED in `<scratchpad>/probe439`. `PathScan.cs` is fixed in place; round 3's reference is kept as `PathScanV3.cs`. Columns: pre435 = PathScrubBefore435, regex = PathScrub at 7ee5411. Path examples are split strings for the home-path hook.

## 1. Changes to the design (the reviewer's two candidates, one widened)
1. **Adjacency, the reviewer's candidate adopted (findings 1 and 2).** A drive starts exactly where the previous path ended, after a path of any form, only for `file:///` or a backslash drive (`X:\`). `glue = q > p || At(q + 2) == '\\'`.
   - Rows 3.4f, 3.4h and 3.4i still pass.
   - The `lastForm` field is no longer needed by the rule.
2. **A letter before `:/` stays in the final name** when either of these holds:
   - **(a)** it follows an ASCII letter, digit or `_`, in a chain of any form. This widens the reviewer's candidate, which covered Unix and `~` chains only.
   - **(b)** it is the first letter of a Unix or `~` final name (fix 2's rule).

   Why widen (a): with the candidate alone, `"docker run -v C:\Users\" + "aabb:/home/" + "aabb"` gave `<path>b:<path>`, keeping the account's last letter. With (a) it gives pre435's `<path>:<path>`. The account-folder `:/` shapes (finding 3) are stable on pass 1.

## 2. Mutants: 62 of 62 killed by their named rows, both forms
New or re-rowed:

| Mutant | Killing row | Design | Mutant |
|---|---|---|---|
| `adjacency-any` (re-rowed: the old PATH row no longer reaches it) | `"C:\Users\" + "<user>(bb)ccD:/Users/" + "<user>/x.cs"` | `<path>D:x.cs` | `<path>x.cs` |
| `glue-after-drive-slash` (round 3's rule) | the same row | `<path>D:x.cs` | `<path>x.cs` |
| `ownletter-start-only` (fix 2's rule) | `"docker run -v /home/" + "aabb:/data ubuntu"` | `<path>:/data ubuntu` | `<path>b:/data`, pass 2 changes |
| `ownletter-word-slash-only` (the reviewer's candidate) | `"docker run -v C:\Users\" + "aabb:/home/" + "aabb ubuntu"` | `<path>:<path>` | `<path>b:<path>` |
| `glue-file-gone` (reviewer) | `"/tmp/abfile:///C:/Users/" + "<user>/x.cs"` | `abx.cs` | `abfile:///x.cs` |
| `slashchain-unix-only` (reviewer) | `"~/x:/home/" + "<user>"` | `x:<path>` | leaks |

- The reviewer's third survivor, `glue-unc-blocked`, is equivalent under rule 1, because no rule depends on the previous form any more. Its row `"\\server\share\abC:/Users/" + "<user>/x.cs"` now gives the regex's `abC:x.cs`. I propose it as a characterisation row.
- The `ownletter-start-only` and `ownletter-word-slash-only` rows use the `aabb` stand-in: the `<user>` form ends in `>`, which is not a word letter.

## 3. Round-3 inputs against the fixed reference
- Findings 1 and 2, all equal to the regex and pre435, no leak, stable second pass:
  - `-v C:\…\proj:` + home → `proj:<path>` (both slash styles)
  - `-v C:\Users\` + account + `:` + home → `<path>:<path>`
  - daemon error `'proj:<path>:rw'`
  - podman `data:<path>`
  - UNC `-v \\server\share\Users\…\proj:` + home → `proj:<path>`
  - `D:\data\proj:` + home → `proj:<path>`
  - `proj:/app`
  - `.ssh:.ssh`
  - `"src:/src"`
- Finding 3: `-v /home/` + account + `:/data`, `/Users/…:/workspace` and the passwd line `/home/` + account + `:/bin/bash` give `<path>:/data`, `<path>:/workspace` and `<path>:/bin/bash`. These equal pre435 and are stable (V3: `<path>b:` on pass 1).
- Finding 4 (MINOR) stays a named limit, the second-pass change in `-v ~/x:/data` (pass 1 `-v x:/data`, pass 2 `-v data`) and `/tmp/x:/x`. Pre435 gives the same first pass. There is no leak.
  - Measured option (switch `opt-one-letter-pathmark`, off): a one-letter final name before `:/` or `:\` becomes `<path>`.
  - It makes all 190 round-1 to round-3 inputs stable on a second pass, with the same leaks and sweeps.
  - Its cost: fix 2's rows change: `"/tmp/x:/home/" + "<user>"` gives `<path>:<path>` instead of `x:<path>`, and so do C3b and `~/x:` + home.
- Finding 6 (MINOR, older) stays: `/tmp/x://server/home/…` and `/tmp/x'y:/home/…` leak, as the regex does.

## 4. Regression checks
- Data sets: 239 of 239 rows (both forms) equal today; second pass stable.
- The 100 review inputs: unchanged (2 leaks, 0 second-pass changes).
- The inputs of rounds 1 to 3 (190 counting both forms):
  - 14 leaks, all older named limits: 9b, `a/b:`, MSVC `/I`, `-isystem`, JSON `\/home\/`, and finding 6 (`x://`, `x'y:`);
  - 4 second-pass changes: finding 4 (×2), and finding 6's `x://` in both forms (it leaks before and after).
- Sweep A: 0 leaks, 0 second-pass changes, 0 differences from the regex.
- Sweep B: 4,559 leaks, 0 new, 0 second-pass changes.
- Sweep C: 2,679 leaks, 0 new, 0 second-pass changes.

## 5. Time (ms, minimum of runs; evidence only, D254)
| Shape | 20k | 1M |
|---|---|---|
| `-v C:\Users\` + account + `\proj:/home/` + account, repeated | 0.40 | 19.0 |
| `-v /home/` + account + `:/data`, repeated | 0.25 | 13.2 |
| `PATH=` + `…/bin:` list | 0.24 | 12.2 |
| `/tmp/x:` list | 0.62 | 26.9 |
| `/=-I` | 2.26 | 35.1 |

The 22 slice shapes (at most 0.91 ms at 20k and 40 ms at 1M) and every fix-round shape were re-timed: all are linear.

## 6. Points to rule on
1. Rule 2(a) widens the reviewer's candidate to every chain, so the drive form of finding 3 keeps no account letter. Adopt it?
2. The finding-4 option (`<path>` for a one-letter name before `:/`): adopt it, removing the last second-pass change at the cost of fix 2's `x:<path>` rows, or keep finding 4 as a named limit (my default)?
3. `glue-unc-blocked` is equivalent now: add its row as characterisation only?
Size: 5617 bytes
