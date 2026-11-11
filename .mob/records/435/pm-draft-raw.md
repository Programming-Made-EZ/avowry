I traced every shape in the bug through the current regex. Four are real leaks today, two only leak part of an account folder, the escape shapes only over-scrub, and one shape works already. Fixing the escapes would expose a new leak, so the card needs a row for it. I could not run code here (the shell was refused), so every output below comes from reading the code, and the red seat must prove each one by running it.

## 1. Key facts verified

**How the scrub works today (`tools/Avowry.Bench/PathScrub.cs`)**
- **Folder and file parts:** line 13 is a middle folder name; it allows spaces, `$`, dots and parentheses. Line 14 is the last part; it allows dots and non-ASCII letters, but not spaces or parentheses.
- **Path roots:**
  - Line 18 is a drive path. It may not follow a letter, a digit or `_`, except straight after a `\uXXXX` escape.
  - Line 19 marks `Users` or `Documents and Settings` as an account folder, in any case.
  - Line 20 is a UNC path. It only marks an account folder when `Users` is the first folder after the host.
  - Lines 22–23 are the Unix roots. They may not follow a letter, digit, `_` or `.`, and may not follow `:/`. The account roots are `/home/`, `/Users/` (exact case) and `(/mnt)?/[a-z]/(?i:users)/`.
- **Folder loop:** line 25. Line 27 is the rule that takes plain words after an account folder: `[ ]+` followed by ASCII letters, digits, `_` or `-` only. Lines 38–39 turn an account folder with nothing after it into `<path>`.

**What each shape in the bug becomes today:**

| Input | Output today | Why | Kind |
|---|---|---|---|
| `gcc /tmp/a.c -I /usr/include -I/home/<user>/include` | `gcc a.c -I /usr/include -I/home/<user>/include` | Line 22 refuses `/home/` after `I` | **Leak** |
| `in /tmp/x.cs at /opt/data/home/<user>/x.cs` | `in x.cs at /opt/data/home/<user>/x.cs` | `/opt/` is not a root; line 22 refuses `/home/` after `a` | **Leak** |
| `Restoring /tmp/x.csproj for /cygdrive/c/Users/<user>/x.csproj` | the second path is unchanged | `/c/Users/` follows `e` | **Leak** |
| `\\host\c$\Users\<user>` (also followed by ` is denied`) | `<user>` | Line 20: `c$` is not `Users`, so no account mark; folders `host`, `c$`, `Users` and the last part `<user>` | **Leak** (the account folder is kept as the short name) |
| `\\wsl$\Ubuntu\home\<user>`, and the doubled `\\\\host\\c$\\Users\\<user>` | `<user>` | Same as above | **Leak** |
| The same three UNC shapes followed by `\x.cs` | `x.cs` | | No leak |
| `C:\\Users\\<user>\u00f6bb` (a JSON-escaped non-ASCII letter) | `u00f6bb` | The `\` of the escape is read as a separator (line 25) | **Partial leak** |
| `\u0027C:\\Users\\<user>\u00f6bb\u0027` | `\u0027<path>\u00f6bb\u0027` | The escape guard on line 25 refuses the folder, so the last part is only `aa` | **Partial leak** |
| `C:\Users\<user>(bb)` | `<path>(bb)` | Line 14 excludes parentheses | **Partial leak** |
| `C:\Users\<user> (bb)` | `<path> (bb)` | Line 27 cannot start on `(` | **Partial leak** |
| `C:\Users\<user>`, and a second word starting with a non-ASCII letter | `<path>.cc`, `<path>` + the rest of the word | The look-ahead on line 27 stops at the dot or the non-ASCII letter | **Partial leak** |
| `C:\Users\<user>`, `/home/<user>/x.cs` (plain non-ASCII, no escape, no space) | `<path>`, `x.cs` | | Already correct (characterisation rows) |
| `C:\\a\\x.cs\nnext` (also `\t`, `\u0022`) | `nnext`, `u0022` | `x.cs` is taken as a folder before the `\` | Over-scrub |
| `\"C:\\a\\x.cs\"` | `\"<path>"` | The last part is empty | Over-scrub |
| `match \\d+ here` | `match d+ here` | Line 20 reads `\\d+` as a UNC path with no separator after the host | Over-scrub |

**The latent leak:** `C:\\a\\x.cs\n/home/<user>/y.cs` becomes `y.cs` today, so nothing leaks. If the escape over-scrub is fixed alone, `/home/` then follows `n`, line 22 refuses it, and the path leaks. This is exactly the kind of regression D256 was written for, so the card must have a row for it.

**Tests (`tests/Avowry.Acceptance.Tests/PathScrubForms.cs`)**
- The one-letter URL host row already exists (line 109), so that part of the bug needs no new work.
- No test row covers the following, which confirms the second review's list of mutations the tests do not catch:
  - a macOS path under `/Users/`;
  - a drive other than `c` under `/mnt/` or Git Bash;
  - `Documents and Settings` in another case (line 87 is the only row);
  - a UNC share path one folder deep. Line 17's `\\host\share\folder\Candidate.cs` gives `Candidate.cs` even if line 20 accepts any folder as the account folder.
- The idempotence check (lines 151–171) loops only over the listed rows.
- The plain stand-in folder is built from parts (`"aa" + "bb"`, line 252), and every row is checked in both forms (lines 254–271).

**Records**
- `.mob/records/426/second-review.md` line 18 lists the uncaught mutations, and line 16 gives the idempotence result: 177 of 300,000 changed on a second pass.
- `docs/retros/card-426-2026-11-11.md` line 82 shows `dcd96f4` is the card 426 architect-slice commit, so its `PathScrub.cs` is the scrub from before card 426. The current file is the base for card 435.
- `TEAM_WORKING_AGREEMENT.md` line 525 is D256. The search output cut that line off after "the generator is committed as an offline t…", so the part saying it runs in the normal gate comes from your summary.

## 2. Revised card

- **Status:** Ready · **Shape:** Story · **Type:** bug · **Source:** issue 435 (card 426 second review, findings 1 rows 2–4, 4, 5, 6, 8, 10 and the list of uncaught mutations) · **Depends on:** 426 done
- **Repository:** avowry (`tools/Avowry.Bench/PathScrub.cs`, linked into `tools/Avowry.BenchCs`; `tests/Avowry.Acceptance.Tests`)
- **Story:** As Dana, I want the bench path scrub to remove the whole account folder from glued, unlisted and UNC roots, from account names with non-ASCII letters, parentheses or dots, and from paths followed by a JSON escape, without losing the words around a path, so that I can share a stored row that never names whose machine produced it, whatever tool printed the path.
- **Consumer value:** A row can be shared without checking each path shape by hand.
- **Acceptance criteria (each fails on partial work; every row is written once with `<user>` and once with a stand-in folder built from parts at run time):**
  1. **Scenario: [M][I][O] glued, unlisted and UNC roots**
     - **Given** each text:
       | text | stored as |
       | `gcc /tmp/a.c -I /usr/include -I/home/<user>/include` | `gcc a.c -I /usr/include -Iinclude` |
       | `in /tmp/x.cs at /opt/data/home/<user>/x.cs` | `in x.cs at x.cs` |
       | `Restoring /tmp/x.csproj for /cygdrive/c/Users/<user>/x.csproj` | `Restoring x.csproj for x.csproj` |
       | `/cygdrive/d/Users/<user>` | `<path>` |
       | `\\host\c$\Users\<user>` | `<path>` |
       | `\\wsl$\Ubuntu\home\<user>` | `<path>` |
       | `\\\\host\\c$\\Users\\<user>` | `<path>` |
       | `/Users/<user>/x.cs` · `/Users/<user>` | `x.cs` · `<path>` |
       | `/mnt/d/Users/<user>/x.cs` · `/d/Users/<user>` | `x.cs` · `<path>` |
       | `c:\documents and settings\<user>` | `<path>` |
       | `\\host\share\x.cs` | `x.cs` |
       | `https://example.com/Users/x`, `https://example.com/home/x`, `/opt/data/x.cs` | unchanged |
     - **When** the scrub runs
     - **Then** each row reads exactly as listed, and neither `<user>` nor any part of the stand-in appears in the output.
  2. **Scenario: [E][B] account names that are not plain ASCII**
     - **Given** the account folders `aa`+ö+`bb`, its JSON-escaped form `aa\u00f6bb`, `aa(bb)`, `aa (bb)`, `aa bb.cc` and `aa` + a space + a word starting with ö, each built from parts at run time (the non-ASCII letter written as a C# `\u00F6` escape, so the source stays ASCII). Each folder is used after `C:\Users\`, `C:\\Users\\`, `\u0027C:\\Users\\…\u0027` and `/home/`, alone and followed by `\x.cs`.
     - **When** the scrub runs
     - **Then** the folder alone becomes `<path>` (and `\u0027<path>\u0027` in the quoted form), the folder followed by a file becomes `x.cs`, no half of any stand-in survives, and `see C:\Users\<user> bb.` still ends in `<path>.`.
  3. **Scenario: [Z][S] escapes after a path, and 100,000 generated inputs**
     - **Given** these text rows, and the D256 generator test:
       | text | stored as |
       | `C:\\a\\x.cs\nnext` | `x.cs\nnext` |
       | `/tmp/a/x.cs\tnext` | `x.cs\tnext` |
       | `see C:\\a\\x.cs\u0022` | `see x.cs\u0022` |
       | `\"C:\\a\\x.cs\"` | `\"x.cs\"` |
       | `C:\\a\\x.cs\n/home/<user>/y.cs` | `x.cs\ny.cs` |
       | `match \\d+ here`, `""` | unchanged |
     - The generator test is a committed generator with a fixed seed and its own small pseudo-random generator, not `System.Random`. It builds at least 100,000 strings from the shapes in criteria 1 to 3: the text before a path, the root, the account token, zero to three folders, an optional file and the text after it. It plants either `<user>` or a two-part token built from parts.
     - **When** the scrub runs once and then again
     - **Then** each row reads exactly as listed. For every generated input:
       - the new output holds no planted token or half-token;
       - no input leaks a token where the old scrub's output did not;
       - a second pass changes nothing.
       - Each exception is an exclusion committed in the generator test (the shape, the reason and `card 435`); the test prints how many inputs each exclusion matched, and fails when an exclusion matches none.
- **Criterion checks:**
  - Criteria 1 and 2 fail if any row differs.
  - The red record shows each of these hand-applied mutants of `PathScrub.cs` failing a named row and then restored, with an empty diff at hand-off:
    - `/Users/` changed to `/users/` (line 23);
    - `[a-z]` narrowed to `c` (line 23);
    - `documents and settings` made case-sensitive (line 19);
    - `\\u` changed to `u` in the escape lookbehind, once each on lines 18, 20 and 22;
    - the UNC `users` on line 20 widened to any folder.
  - Criterion 3 fails while any in-scope shape leaks. The old scrub is a verbatim copy of the base `PathScrub.cs`, with only the class renamed (for example to `PathScrubBefore435`, under `tests/…/Support/`). A small fact asserts that the copy still reproduces three known base leaks (the `-I`, `/opt/data/home` and `\\host\c$` rows), so a patched copy cannot quietly empty the comparison. The gate reviewer checks the copy against `git show <base>:tools/Avowry.Bench/PathScrub.cs`.
- **ZOMBIES:**
  - Z: an empty string and text with no root (`\\d+`, `/opt/data/x.cs`) stay unchanged (criteria 3 and 1).
  - O: one account folder alone becomes `<path>` in UNC, WSL and non-ASCII forms (criteria 1 and 2).
  - M: two paths on one line, the generator's 100,000 inputs (criteria 1 and 3).
  - B: a dot ending a sentence after an account folder, `\u0022` and `\"` at both ends of a path (criteria 2 and 3).
  - I: both tools call the shared file (418-U4 kept); WSL, Cygwin and JSON-escaped forms (criterion 1).
  - E: JSON-escaped letters, parentheses and dots in account names (criterion 2).
  - S: a second pass changes nothing (criterion 3).
- **D256 differential check (named for the slice):** the check is `Id_435_3` generated inputs against the base copy, with a fixed seed and at least 100,000 inputs. It runs in the normal gate, with no Pending trait after the green and no wall-clock assertion (D254).
- **Must keep passing:** all of `PathScrubForms` (418-U1, U2, U4, U5 and the 426 rows), `BenchRowsHoldNoFolderPath`, `NewBenchRowsCarryNoReply`, `BenchmarkHarnessAcceptance`, `PrivateHoldoutResults`.
- **Not in this card:**
  - the `\\?\` prefix;
  - a TMPDIR or other unlisted root that holds no `home` or `Users` folder;
  - prose followed by a relative path (a known limit);
  - a drive glued after `\x27` or `%27`;
  - rewriting pushed rows (0 hold an account name);
  - the residue regex in `BenchRowsHoldNoFolderPath`;
  - the pre-commit scrubber tooling item;
  - a mutation-testing tool (the mutants are applied by hand).
- **Size:** L as one slice (one regex in one shared file, about 35 rows, 8 hand-applied mutants, one generator test and the base copy). If the slice goes over 15,360 bytes, split criterion 2 into 435b. The generator stays with whichever slice changes the regex first, and the next slice extends it. Never split the generator off on its own.
- **Done when:**

    dotnet test Avowry.sln --filter "Status!=Pending"

**Test note:** test inputs use only `<user>` and stand-ins built from parts at run time (`"aa" + "bb"`, with the non-ASCII letter as a `\u00F6` escape in the source). No real name or home folder appears. Outputs the red seat must prove by running are every "today" output in the table in section 1, and the generator's first counts.

## 3. Open points for the facilitator

- **D256 old version:** by D256, the old version for this card is the base file, not `dcd96f4` (which is the scrub before card 426). Keeping a `dcd96f4` copy as a second comparison is optional.
- **Pinned failing count:** the bug's "a failing count is pinned" conflicts with D256. The card replaces it with exclusions committed by shape.
- **Personal name in the bug:** the bug's example account name is a personal name. Edit issue 435 to use a stand-in built from parts.
- **Lost backslash:** the UNC examples in the bug and in the 426 records have lost a backslash. The card writes `\\host\c$\…` and `\\wsl$\…`.
- **Glued-flag output:** confirm the expected output for a root glued to a flag: `-Iinclude` (the existing short-name rule) or `-I<path>`.
- **Wider word rule:** taking parentheses, dots and non-ASCII words after a bare account folder makes the known over-scrub of prose after it larger. Confirm.
- **`\\d+` row:** the card now claims `\\d+` unchanged, which reverses the scope of card 426 ruling 5. Confirm.
- **Plain accented folder:** by my reading, a plain accented account folder is already removed whole, so those rows are characterisation rows and the bug's scenario 2 wording changes.
- **D256 wording:** confirm that D256's cut-off sentence ("committed as an offline t…") still lets the generator run in the normal gate.
