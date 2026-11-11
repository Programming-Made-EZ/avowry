# Proposal for Decider D257: the task-force declaration and card text for card 435 (the bench path scrub removes the whole account folder from glued, unlisted and UNC roots, non-ASCII account names and paths followed by a JSON escape)

Facilitator, 2026-11-11 UTC, main at 4a9d1d5. VOTE on this declaration: adopt the Ready card text of .mob/records/435/ready-body.md (the product manager seat's draft with the facilitator's rulings; the issue body is the same text) as card 435's Ready text, with the task force below. Card 435 is the bug filed by the card 426 retro from the second review's older findings; it is ranked just after 426 (114.8), before the publish card (188) unless the owner ranks it after. Size L; one slice, split as 435b only if the slice goes over 15,360 bytes. It changes no agreement text; Decider protocol step 6 (D224) applies with the D232 paragraph (the re-run with swapped seats is trigger (d) here: no change to the agreement); the D228 preflight and the D234 evidence block apply; the seat plan cites .mob/records/435/probe-all-2026-11-11.jsonl (taken 2026-11-11 after card 426 closed; available: agy, claude, codex, cursor, grok, qwen; out: copilot, muse until 2026-11-16).

STANDING-RULES PREFLIGHT: Card format rule (the three criteria cover Z, O, M, B, I, E, S, each titled with its letters); TDD red-first (every new-state row of criteria 1 and 2 and the escape rows of criterion 3 are watched failing by assertion on the current tree; rows that already pass, such as plain accented account folders, are DECLARED characterisation up front; the generator test of criterion 3 is Pending and fails by assertion on the current tree; the hand-applied mutants are proved once by the red seat and restored, with an empty diff at hand-off); D204 (one red commit with the criteria as Pending traits, then the greens); D206; D219/D226 step 0 in the gate review; D222 id lists in the red record; D234 evidence block; D236 live probe cited; D243 merge verification before every gate; D251 toolchain stamp (dotnet --version, python --version, sorted SDK folder names before and after each gate and the sign-off; a changed stamp voids the run); D254 applies (no wall-clock assertion in the generator test); D249 applies if the card adds a new kind of tracked file, and the slice must list the extensions the card's own process files create (seat transcripts as .txt, not .out); D256 (new, on trial) applies: the slice names the differential check, the generator is committed with a fixed seed, at least 100,000 inputs, runs in the normal gate, and every difference is a failing test or an explicit exclusion committed in the generator test; D255-style git audit before every gate and the sign-off (the diff of PathScrub.cs and the two Program.cs files, and a whole-word account-name count read from the environment). The product owner's ruling of 2026-11-10 (stricter): every brief and every prompt carries the account-name rule: never type, echo, search for, pass as an argument or write the local account name anywhere, only $HOME, %USERPROFILE% or <home>; the commit hook scrubs only added lines that look like a home folder: a test row that looks like one must be written with a split string. Consumer value: Dana can share a row without checking each path shape by hand.

## Facilitator rulings (main at 4a9d1d5)

- Facts: the draft read the code and did not run it; the red seat proves every 'today' output by running it. The facilitator's earlier counts: 0 of the pushed rows carry the account name; nothing pushed changes.
- Ruling 1 (scope): one card; criterion 1 the glued, unlisted and UNC roots, criterion 2 account names that are not plain ASCII, criterion 3 escapes after a path and the D256 generator test against a verbatim copy of the base PathScrub.cs. Not in this card: the `\\?\` prefix, a TMPDIR outside the listed roots, prose followed by a relative path, a drive glued after `\x27` or `%27`, rewriting pushed rows, the residue regex, the pre-commit scrubber tooling item.
- Ruling 2 (the seats): read-only architect slice by a fresh Claude context (it names the D256 check and lists the extensions of the card's own process files); the RED by a Grok WRITING seat (grok_write.py, own linked worktree, changes left uncommitted, the facilitator committing them); the GREEN by a Cursor WRITING seat (cursor_write.py, own linked worktree); red and green on different seats; the facilitator runs every gate command in person in an isolated clone with D243 merge verification and the D251 stamp, with nothing else running during a full suite; a second read-only review (told to try to break the change, with a differential run of its own), an independent gate reviewer and the sign-off are fresh Claude contexts; a facilitator takeover is logged and, when it changes production code or a check, comes with its failing test committed first.
- Ruling 3 (every seat brief repeats): keep each file's existing line endings; the account-name rule exactly as worded above (typing it counts: a seat that types it, even as a search term, reports the slip); no recursive delete; no commit trailer; a seat that finds a flaw in a test it was not asked to change stops and reports it; a test row that looks like a home folder is written with a split string.
- Ruling 4 (the retro): seven voices at once, the retro input committed BEFORE the window, with the idle-minutes and Dispatch-nudge lines, the Evidence (per card) section, the list of drivers sidelined for behaviour, and the headings Individual, Team, Organisation and Product as heading lines.

## The card (verbatim)

<!-- phrase-card:435 -->

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
- **Rulings (facilitator, 2026-11-11; the draft read the code and did not run it, so the red seat proves every 'today' output by running it):** (1) the old version for the D256 differential is a verbatim copy of the CURRENT base `tools/Avowry.Bench/PathScrub.cs` (the file at the base commit of this card), renamed for the test; a copy from `dcd96f4` is not needed; (2) the bug's 'a failing count is pinned' is replaced by D256's rule: every difference is a failing test or an explicit exclusion committed in the generator test (the shape, the reason, `card 435`), and the exclusions are counted and reported; (3) the bug's example account name is replaced by the stand-in built from parts (the issue body is replaced by this text); (4) a root glued to a flag reads `-Iinclude` (the existing short-name rule), as the table says; (5) taking parentheses, dots and non-ASCII words after a bare account folder widens the known over-scrub of prose after it: accepted (an over-scrub never leaks); (6) the single-backslash regular-expression row `\d+` stays unchanged as in card 426; the double-backslash `\\d+` row is NOT claimed unless it falls out of the design for free (the architect says; the red records it); (7) plain accented account folders are already removed whole: those rows are characterisation rows, declared up front, and the red record lists them apart from the failing rows; (8) one card, one slice; if the slice goes over 15,360 bytes the architect splits criterion 2 as 435b, never the generator on its own; (9) the hand-applied mutants are proved by the red seat once against the final tests and restored (an empty diff at hand-off), as in card 426's label proof; (10) card number 435 (the issue number), persona Dana, size L; D251 stamp, D254 (no wall-clock assertion) and D256 apply.
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

## AMENDMENT AFTER THE PRIMARY WINDOW OF D257

Primary window result (all six voters at once, 5 minutes 51 seconds, 16:59:12 to 17:05:03 UTC): five Yes (QA agy, architect Grok, coach Cursor, developer Cursor, decider agy) and one No (audit lens Qwen). The No is a condition: the proposal cannot be verified from git alone (it relies on running a 100,000-input generator and hand-applied mutants), and explicit exclusions let the team pass by documenting failures; reduce the verification to deterministic, git-verifiable diffs and assertions. The proposer adopts the part that can be adopted and says plainly what cannot: (1) the NUMBER of exclusions is pinned in the generator test itself as a constant (a change to the pin, or a new exclusion, is a visible diff that the gate reviewer must name), and the test fails when the actual count differs from the pin; (2) the committed raw output of every gate and of the sign-off holds the generator's printed counts (inputs, exclusions per shape), so the execution is checkable from the repository's committed files; (3) the mutants are pinned to the base commit (the red record names each by the text of the line at the base commit, not by a line number, and the red seat records the failing row for each), and the count is seven (the draft said eight but listed seven); (4) the doubled-backslash row is raw text (a C# verbatim string); (5) the gate reviewer re-derives the exclusion count from the committed generator file. A test result that needs execution cannot be reduced to a diff; no text rule can promise otherwise, and the amendment says so: the repository's evidence is the committed code of the checks, the pinned counts and the committed raw gate output.

Confirmation step (D232): the objector is asked one question: is your condition now met by this form? The five Yes voters are asked one question: does the amendment change your vote?
