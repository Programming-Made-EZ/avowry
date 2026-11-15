# Card 439 architect slice: a linear scanner for the bench path scrub

Architect (claude, claude-opus-5-5, D261), ctx-1, read-only. All numbers MEASURED in `<scratchpad>/probe439` (net8.0, RollForward Major: copies of today's `PathScrub.cs` as `PathScrubToday`, `PathScrubBefore435.cs`, the design as `PathScan`). Reference implementation: `probe439/PathScan.cs` (about 330 lines; drop the probe-only `M("…")` mutant switches).

## 1. Design

`Scan` walks the text once. At each p it tries drive, UNC, `~`, Unix; on a match it appends the replacement and continues at the match end, else p+1. **Linear by construction:** a match consumes all it reads except O(1) look-around. The only longer failed reads: a failed Unix walk (it stops at its first break, and no Unix root can start inside it, since each `/` there follows an ASCII letter, digit or `_`), and one failed tail word per match.

### Constants (each once)
- `PathMark` = `<path>`: the output when there is no short name. It is never a UNC host.
- `AccountParents` = `users`, `home`, `documents and settings` (any case): the parent folders of an account for UNC paths and bridged chains.
- `DriveParents` = `users`, `documents and settings` (any case): the first folder after a drive.
- `UnixFixedRoots` = `/root/ /tmp/ /var/tmp/ /var/folders/ /private/tmp/ /private/var/folders/`: Unix roots that carry no account marker.
- `EscLetters` = `nrtbf`.
- `NameStops` = `` \ / " ' ` | * ? : ; , < > [ ] `` (plus CR and LF): never inside a folder name.
- `FinalStops` = NameStops plus `( )` and whitespace: never inside a final name or a glued group.
- `FlagLeft` = `" ' ( [ , = ;`: a glued flag may follow these.
- `TildeLeft` = `` \ / " ' ` | * ? : ; , ( ) < [ ] ``: a `~` root may follow these. It leaves out `>`, so `<path>~` stays stable.

### The shared escape set
- **E1**: `\` plus a letter of `EscLetters`, where that `\` does not itself follow a `\`.
- **U**: `\uXXXX`. U comes in two kinds:
  - **Letter escape**: a code outside `0000`-`007F`, whose `\` does not follow a `\`. It is a name character everywhere: folders, final names, tail words and glued groups.
  - **ASCII escape**: a code in `0000`-`007F`.
- **What ends a path:** in a doubled-backslash or slash chain, E1, `\"` or U after a name ends the path (no folder). In a single-backslash (sgl) chain that is **esc**, U does.
- **esc**: the root follows a U directly, or follows a glued flag that follows an ASCII escape (new).

### Starts
**Drive, UNC, Unix**, first rule that holds:
1. Plain left. Drive: not after an ASCII letter, digit or `_`. UNC: not after `\w`, `\` or `:`. Unix: not after an ASCII letter, digit, `_` or `.`, nor after `:/`.
2. Right after U (sets esc). 3. Right after E1.
4. After a glued flag `-X` (X an ASCII letter) whose `-` is at the start, or after whitespace, `FlagLeft` or E1; or after an ASCII escape (sets esc). Never after a letter escape (`a\u00f6-b` is a word).
5. Adjacency (new; drive and `file:` only): exactly where the previous path ended.

**`~`**: at the start, or after whitespace, `TildeLeft`, E1, an ASCII escape or a glued flag.

### Roots
- **Drive**: optional `file:///`, ASCII letter, `:`, then **sgl** (one `\` with no separator after it) or a run of `\` and `/`.
- **UNC**: `\\\\` (doubled) or `\\` (sgl); next character not a separator or whitespace, and not `<path>`; the host may be `X:`; at least one folder must follow (new: `match \\d+ here` stays).
- **`~`**: `~` plus a separator.
- **Unix**: optional `file://`, optional `localhost`, `/`, then **the walk**. At each `/` look for a marker: `/home/`, `/Users/` (exact case), `/x/users/` or `/mnt/x/users/` (x in a-z, `users` any case). A prefix folder has no whitespace or `NameStops`, ends in an ASCII letter, digit or `_`, and is followed by `/`. No marker: a `UnixFixedRoots` root, or no path.

### The chain
**Folder:** the longest run of tokens. A token is one of: `<letters>`, a letter escape, an apostrophe, or a character that is not in `NameStops`, CR or LF.
- Whitespace followed by `-X` and then `/`, `\\`, `~` or `Y:` ends the run (new: `a.c -I/home/…`).
- The folder must not end in whitespace, and must be followed by a separator.
- The **guard**:
  - no letter escape follows;
  - if the chain is not sgl, no E1, `\"` or U follows;
  - if the chain is sgl and esc, no U follows.

**Final name:** the longest run of tokens. A token is one of: `<letters>`, a letter escape, an apostrophe, or a character that is not whitespace and not in `FinalStops`, and does not start `X:\`, `X:/`, `file:\` or `file:/`.

**Apostrophe:** after a letter, a placeholder's `>` or a letter escape, and before a letter or a letter escape.

### The account rule
The final name is an account folder in two cases:
- The walk found a marker and no folder follows it.
- The last folder is a parent:
  - **Drive**: the first folder is in `DriveParents`.
  - **UNC**: a folder after the host is in `AccountParents`.
  - **Unix**: `home` or `Users` (exact case), or `users` (any case) after a one-letter folder.
  - **Bridged chain** (new): a separator run between two folders differs from the chain's own run (`\`, `\\` or `/`), and the folder is in `AccountParents`.

### The tail (after an account folder only)
**Glued groups:** `(`, then any mix of non-`FinalStops` characters, spaces, letter escapes and apostrophes, then `)`. Each group may be followed by a glued word.
- Glued word: letters, digits, `_`, `-`, apostrophes, letter escapes, `.` before a letter, digit, `_` or letter escape; no parentheses; stops at a root start. It takes its longest end that is either not followed by a letter, digit, `_`, `:\` or `:/`, or is a letter, digit or `_` followed by a root.

**Spaced words** follow the groups: spaces, then a word made of the same tokens plus `( )`, and `.` before `(`.
- Each takes its longest end that is not followed by **Stop**.
- Stop is a letter, digit, `_`, `:`, `/` or `-`; a `\` that starts no escape; `'` before an ASCII letter; or `.` before a letter, digit or `_`.

### Apply's output
The output is `<path>` when:
- the final name is empty, is `~`, or is an account folder (the account case also takes the tail); or
- the path follows a bare `\\` that lies outside the previous match (new: `\\name\…` would become a UNC root on pass 2).

Otherwise the output is the final name.

## 2. Behaviour contract (both forms: `<user>` and `aabb`)

**Data sets:** all 239 rows of the 418, 426 and 435 data sets, in both forms, give today's output exactly. A second pass changes nothing on any of them.

**Review inputs:** 100 inputs, 200 counting both forms.
- Leaks (before / today / design): 102 / 83 / 2.
- Second-pass changes: 8 / 14 / 0.

Each review input is a red row.

**Same as today** (the rework's fixes kept): 2.1a-d, 2.2a-h, 2.3a-c, 2.4a-f, 2.5e, 2.5o, 2.7a-c, 2.8, 2.9a-j, 2.11, 3.5e (`<path>-x.cs`), 3.7a-g. The ids and every input are in `probe439/Review.cs` (`review` mode prints before, today and design).

**Fixed, third-review BLOCKING** (`u` = `<user>`):

| Row | Input | Design output |
|---|---|---|
| 3.1a-c | `\\<host>\Users\<user>` / `…\x.cs` / `\\<host>\share\x.cs` | `<path>` / `x.cs` / `x.cs` |
| 3.2a-c | `\\host(1)\Users\<user>`, `\\host's\…`, `\\a<b>\…` | `<path>` |
| 3.3a | `\u0027-IC:\a\x.cs\u0027/home/u` | `\u0027-Ix.cs\u0027<path>` |
| 3.3b | `\u0027-I\\host\share\x.cs\u0027/home/u/y.cs` | `\u0027-Ix.cs\u0027y.cs` |
| 3.3c / 3.3d | `gcc \u0027-IC:\Users\<user>\include\u0027` / `…u\u0027` | `gcc \u0027-Iinclude\u0027` / `gcc \u0027-I<path>\u0027` |
| 3.3e | `see a\u00f6-b/tmp/x.cs` | unchanged |
| 3.4a-c | `/tmp/a/foo-~\nx.cs`, `/tmp/a/~~\nx.cs`, `C:\\a\\foo-~\nx.cs` | `foo-~\nx.cs`, `~~\nx.cs`, `foo-~\nx.cs`, all stable |
| 3.4e / 3.4f / 3.4g | `C:\Users\<user>:\Users\<user>\x.cs`, `…u_D:…`, `…u.file://localhost/home/u/x.cs` | `<path>x.cs` |
| 3.4h / 3.4i | `\/tmp/a/nC:\Users\<user>`, `\t/tmp/a.file://localhost/home/u/x.cs` | `\n<path>`, `\ta.x.cs` |
| 3.5a / 3.5b | `see (C:\Users\<user>(bb))`, then the same plus ` here` | `see (<path>)`, then the same plus ` here` |
| 3.5c / 3.5d | `C:\Users\<user>(bb)ccfile:///C:/Users/<user>/x.cs`, `…ccD:\Users\<user>\x.cs` | `<path>x.cs` |
| 3.6 | `\\C:/a\n` blow-up | linear (section 4) |

**Fixed, MINOR (second and third reviews):**

| Row | Design output |
|---|---|
| 2.2i, 2.2j, 2.2k | `[-Iinclude]`, `(-Iinclude)`, `-Wall,-Iinclude` |
| 2.5a-d, 2.5f-m | `<path>` (quoted: `\u0027<path>\u0027`), except 2.5j `C:\Users\<user>'bb\x.cs`, which gives `x.cs` |
| 2.6a-c | `gcc a.c -I<path>` |
| 2.6d | `gcc a.c -Iinclude` |
| 2.6e | `see <path>)` |
| 2.6f, 2.6g | `<path>` |
| 2.12a, 2.12b, 2.12c | `x.cs\ny.cs`, `\u0027x.cs\u0027`, `-Iinclude` |
| 2.13 | `<path>` |
| 3.2d-f | `<path>` (quoted: `\u0027<path>\u0027`) |
| 3.8a-d | `-I<path>\nnext`, `\t<path>`, `\u00f6<path>\tnext`, `\u00f6<path>` |
| 3.8e, 3.8f | `\\<path>`, `\\<path>\tnext` |
| header limit | `match \\d+ here` is now left unchanged |

**Named limits** (D259 clause 4: no leak on a real shape, or a contrived composition):
- 2.5n: `C:\Users\<user> bb: access denied` keeps ` bb:`. A data row requires this: `see C:\Users\<user> x.cs: denied` must keep `x.cs:`.
- 3.4d: `foo-~\nx.cs` is now left as is (before and today give `foo-nx.cs`).
- 3.8a-d: the bridged first path's name, such as `a.`, becomes `<path>`. This over-scrubs but leaks nothing.
- A root glued to the end of a word, not where a path ended, is not a root: `x.cs(pp)qqD:\Users\…`, `Candidate.cs(7,1).file://…`. These leaked before too.

## 3. Mutants (45 of 45 killed by their named rows, both forms; `u` = `<user>`; "p2" = killed by the second pass)

Each line: rule, then mutant → killing row.
- **E1:** no `\`-before check → `x\\n/tmp/a/y.cs`; drop `b` → `C:\\a\\x.cs\b/home/u/y.cs`.
- **Letter escape:** ASCII codes count → `C:\\Users\\<user>\u003e`.
- **Flag:**
  - `I` only → `gcc -L/home/u/lib`;
  - `FlagLeft` cut to quotes → `[-I/home/u/include]`;
  - after any U → `see a\u00f6-b/tmp/x.cs`;
  - no esc → `gcc \u0027-IC:\Users\<user>\include\u0027`.
- **Adjacency:** off → `C:\Users\<user>:\Users\<user>\x.cs`.
- **UNC:**
  - `<path>` host allowed → `\\/home/<user>\nx.cs` (p2);
  - always sgl → `\\\\host\\share\\x.cs\n/home/u/y.cs`;
  - host alone allowed → `match \\d+ here`;
  - no `X:` host → `\\C:\\Users\\<user>\\x.cs`.
- **`~`:** today's left rule → `/tmp/a/foo-~\nx.cs` (p2); no `(` → `(~/x.cs)` → `(x.cs)`.
- **`~` name:** the rule off → `/tmp/a/~\nx.cs`.
- **Walk:** a prefix ends in anything → `/x-/home/u` → `/x-<path>`.
- **Fixed roots:** drop `/var/tmp/` → `/var/tmp/a/x.cs`.
- **Markers:** drop `/Users/` → `/Users/<user>/x.cs`; `/[a-z]/` → `/c/` → `/d/users/<user>`.
- **Final name:** `(` allowed → `C:\a\(b)C:\Users\<user>` → `<path>(b)<path>`.
- **Folder:**
  - `;` allowed → `PATH=/tmp/a;/home/<user>/bin` → `PATH=a;bin`;
  - no flag split → `gcc /tmp/a.c -I/home/u/include`;
  - trailing space allowed → `gcc /tmp/a.c -I /usr/include -I/home/u/include`.
- **Apostrophe:** ASCII only, or not after `>` → `C:\Users\<user>'bb`; not after a letter escape → `C:\Users\<user>\u00f6'bb`.
- **Guard:** `\n` only → `C:\\a\\x.cs\r/home/u/y.cs`; esc off → `\u0027\\host\share\x.cs\u0027/home/u/y.cs`.
- **Account:**
  - off → `C:\Users\<user>`;
  - drop `home` → `\\wsl$\Ubuntu\home\u`;
  - drop Documents and Settings → `C:\Documents and Settings\u`;
  - case-sensitive → `\\host\c$\Users\<user>`;
  - drive parent at any depth → `D:\src\Users\UsersController.cs` (a real shape);
  - no bridge → `C:\a\x.cs\n/home/u`;
  - Unix any case → `/tmp/a/Home/x.cs`;
  - no `/x/users` → `see /tmp/x.cs (/c/users/<user>)`.
- **Tail:**
  - off → `C:\Users\<user>(bb)cc`;
  - group without letter escapes → `C:\Users\<user>(\u00f6bb)`;
  - group without spaces → `C:\Users\<user>(bb cc)`;
  - word without letter escapes → `C:\Users\<user> \u00f6cc`;
  - `.` without a letter escape after → `C:\Users\<user> bb.\u00f6cc`;
  - `.` without `(` after → `C:\Users\<user> bb.(cc)`;
  - glued word with parentheses → `see (C:\Users\<user>(bb))`;
  - glued word across a root → `C:\Users\<user>(bb)ccD:\Users\<user>\x.cs`.
- **Bare `\\`:** off → `\\/cygdrive/c/Users/u/x.cs\tnext` (p2).

## 4. Linear time (ms, minimum of runs; evidence only, no test asserts time, per D254)

| Shape | 20k: before / today / design | 40k design | 1M: before / today / design |
|---|---|---|---|
| `\\;` | 6.6 / 8.3 / 0.88 | 1.54 | 499 / 325 / 15.7 |
| `\\,` | 4.5 / 5.7 / 0.24 | 0.48 | 242 / 388 / 12.7 |
| `\\a;` | 6.7 / 11.0 / 0.64 | 0.79 | 558 / 637 / 19.9 |
| `\\\\host\\share\\x.cs\n` | 1.3 / 6.9 / 0.51 | 1.22 | 78 / 348 / 27.2 |
| `\\C:/a\n` | 5.8 / **2005** / 0.80 | 0.84 | 343 / skipped / 26.7 |
| `\\C:\\a\n` | 4.9 / **1670** / 0.35 | 0.77 | 337 / skipped / 17.9 |
| `\\\\a$\n` | 1.1 / 9.9 / 0.41 | 0.82 | 80 / 719 / 29.5 |
| `/a` | 14.4 / 24.5 / 0.74 | 1.50 | 716 / 1002 / 39.3 |
| `/-` | 8.8 / 11.5 / 1.21 | 2.03 | 375 / 512 / 68.9 |

- **Today's quadratic shape:** at 40k, `\\C:/a\n` takes 8,229 ms, 4.1 times its 20k time.
- **The design:** every shape is under 1.3 ms at 20k and under 70 ms at 1M. Fifty times the length costs 18 to 72 times the time.
- **Other shapes measured:** `\u0027\\a`, `\\h\s\x.cs;`, the tail shapes, `/a/a/a.-` and an 850k mixed text (before / today / design: 210 / 320 / 16.9 ms). All are linear.

## 5. The generator (D256)

Three sweeps of 100,000 inputs each. A leak is the token or one of its halves.

| Sweep | Before: leaks / pass 2 | Today: leaks / new / pass 2 | Design: leaks / new / pass 2 |
|---|---|---|---|
| A: today's test, verbatim, seed 435 | 31,534 / 0 | 0 / 0 / 0 | 0 / 0 / 0 |
| B: extended lists, seed 439 | 45,965 / 6,518 | 19,720 / 2,424 / 1,523 | 4,559 / 0 / 0 |
| C: review shapes, seed 4391 | 50,216 / 2,877 | 28,735 / 6,592 / 6,867 | 2,904 / 0 / 0 |

- **A:** the design's output differs from today's on 0 inputs.
- **B** adds pieces taken from the review inputs: 25 befores, 9 roots and 28 afters (for example `-I/tmp/a.`, `\\<host>`, `\\host(1)`, `\\C:`, `(pp)qqD:\Users\{token}\x.cs`, `\n~/y.cs`). Its literal tail text uses `pp` and `qq`, so the text itself never counts as a leak.
- **C** uses every review input as a template: `<user>` becomes the token, and the original befores and afters wrap it. It also replaces `gcc` with `ld` and `access denied` with `denied`.
- **The design's remaining leaks** all leaked before as well: a root glued to a word, and the ` bb:` limit.

**For the red:** keep today's fact as it is. Add B and C as facts with the same counters and an empty `Exclusions` list.

## 6. Open points for the facilitator

1. **How far the account rule reaches.** For UNC paths, any folder named in `AccountParents` counts; for Unix, `home` and `Users` at any depth, as today. So `\\srv\share\Users\UsersController.cs` gives `<path>`, as today. Drive paths stay first-folder only.
2. **What B and C assert.** Their absolute leaks are not 0, and every one of those inputs leaked before too. Choose one:
   - assert `regressions == 0 && second_pass == 0` and pin the printed leak counts; or
   - commit two exclusions: "root glued to a word" and the ` bb:` limit.
3. **3.5e.** The third-review summary calls `<path>-x.cs` a regression. But the raw report's mutant M-R8 expects exactly that output, and criterion 2 treats a glued `(1)` as account text. I keep `<path>-x.cs`.
4. **3.4d.** `foo-~\nx.cs` now stays as is: stable, no leak.

Size: 15342 bytes
