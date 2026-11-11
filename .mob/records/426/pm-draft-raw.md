## (1) Key facts, checked against the code

I traced the regex by reading it; I did not run it, because this seat is read-only. A count search of every `bench/**/history.jsonl` found 0 committed rows holding any of the missed forms.

**How the scrub works today (`tools/Avowry.Bench/PathScrub.cs`)**

- **Roots it matches (lines 17–21):**
  - a drive path, with `Users` marked as the home folder (line 18);
  - a UNC path, which must start with exactly `\\` followed by a non-separator (line 19);
  - a `~` path (line 20);
  - `/home/`, `/Users/`, `/root/`, `/tmp/`, `/var/folders/` and `/private/var/folders/`, but only when no letter, digit, `_` or `.` comes before them (line 21).
- **Folder and file names:** a folder name (line 13) may hold spaces and brackets. The last part of the path (line 14) may not.
- **Folders, then file (line 23):** all folders are matched first, then the file name.
- **Trailing words (line 25):** a home folder that ends the path also takes the words after it.
- **Account folder alone (lines 35–37):** it becomes `<path>`, but only if the root set the home mark.
- **Empty or missing text (lines 30–31):** returned as it is.
- **Header comment (lines 4–9):** already lists the over-scrub, `\\?\` and the unmatched roots.

**Where each tool calls the scrub**

| Step | Phrase bench (`tools/Avowry.Bench/Program.cs`) | C# baseline (`tools/Avowry.BenchCs/Program.cs`) |
|---|---|---|
| Before the 600 cap | build output 1368, then cap 1423; each test failure 1414, then cap 901; `FailedExecuted` scrubs then caps at 907 (`FeedbackCap`, line 15) | output 242, then cap 830 (the `tests_did_not_run` text at 250 has no cap); failures 264, then cap 265 |
| Model exit and errors | retry 310; final error 356 (message built at 678, no cap) | `CreateResult` 839–840 scrubs diagnostics and error |
| On save | `PersistResult` 815–817 | `Persist` 848–850 |
| Label | read without scrubbing at 47; scrubbed only at 817 | read without scrubbing at 35; `CreateResult` passes it as is (840); scrubbed only at 850 |

The C# tool compiles the same file through a link (`Avowry.BenchCs.csproj` line 6). The Phrase bench's parse-resolve diagnostics are diagnostic codes only (line 734), so no path can reach them.

**What follows for the save step:** every diagnostic and error is already scrubbed before it reaches the save step in both tools. Deleting the save-step scrub of diagnostics and error (Phrase bench 815–816, C# baseline 848–849) therefore changes nothing a test can observe. Only the label (817 and 850) depends on the save step alone. The bug asks for "a row whose strings were never scrubbed earlier", and neither tool can produce one without a new test hook.

**What the tests cover today**

- **`PathScrubForms.cs`:**
  - Forms (lines 8–34) has no row for a plain `C:/` path, a trailing dot or comma, a path after a bare `/x`, or `\\?\`.
  - The untouched rows (49–61) pin the URLs (52–53) and `/a/b` (56).
  - The run-twice check (102–113) loops over Forms only.
- **`BenchRowsHoldNoFolderPath.cs`:**
  - Test 418-3b throws away its stored row (`_ = AssertStored`, line 405) and never runs the residue check.
  - The residue check (458–477) decodes only diagnostics and error. It sees the label only through the raw-text checks (529–539).
  - Its drive check (503) has the same letter-or-digit blind spot as the scrub.
- **No label with a path is tested:** the only label assertions use plain labels (`NewBenchRowsCarryNoReply.cs` lines 50 and 256; `BenchmarkHarnessAcceptance.cs` 596–597).

**What each form in the bug becomes today**

| Input | Today | Verdict |
|---|---|---|
| `Could not find file /tmp/x.cs in /usr/lib/dotnet` | `Could not find file dotnet` | over-scrub: a folder name may hold spaces, so a later `/` reads as a separator |
| `copy /tmp/a/x.cs to /tmp/b/y.cs` | `copy y.cs` | over-scrub |
| `see C:\a\b.cs (line 3) and /docs/x` | `see x` | over-scrub |
| `\\d+` | `d+`: UNC root, with `d+` as the file | over-scrub |
| `\d+` (one backslash) | unchanged | fine |
| `/mnt/c/Users/<user>/x.cs`, `/c/Users/<user>/x.cs` | unchanged: line 21 rejects `/Users/` after `c` | **leak** |
| `/var/tmp/a/x.cs`, `/private/tmp/a/x.cs` | unchanged: `/tmp/` after a letter is rejected | **folder leak** |
| `file://localhost/home/<user>/x.cs` | unchanged | **leak** |
| `C:\Documents and Settings\<user>\x.cs` | `x.cs` | fine |
| `C:\Documents and Settings\<user>` (alone) | `<user>`: no home mark, so the folder is kept as the file name | **leak** |
| `\\host\Users\<user>\x.cs` | `x.cs` | fine |
| `\\host\Users\<user>` (alone) | `<user>` | **leak** |
| `\u0027C:\\Users\\<user>\\x.cs\u0027` | the drive is rejected after `7` (line 18). With the `<user>` placeholder, the `\\x.cs\u0027` after `>` matches as UNC and becomes `u0027`. With plain letters in the account folder, nothing matches. | **leak**; the placeholder also behaves differently from a plain name |
| `\\\\host\\Users\\<user>\\x.cs` | unchanged: the check after `\\` and the check before it both fail | **leak** |
| `C:/Users/<user>/x.cs`; `C:\a\x.cs.`; `C:\a\x.cs, then`; `/x then /tmp/a/y.cs` | `x.cs`; `x.cs.`; `x.cs, then`; `/x then y.cs` | fine; only test rows are missing |
| `\\?\C:\Users\<user>\x.cs` (review finding 3; not in the bug) | `<path>?\x.cs` | no leak, but untidy |

**Proving it with placeholder paths only**

- Each scenario is proved through the scrub function or through `--label`, using `<user>` paths.
- Every leak form also gets a second row with a plain-letter account folder built from parts at run time, the way `PathScrubForms.cs` lines 38–41 already do. This matters because the folder-name rule (line 13) gives the `<user>` placeholder its own branch, and the JSON-escaped row above shows a placeholder can match differently from plain letters.
- Checks against the real profile and temp folders keep reading them from the environment (`BenchRowsHoldNoFolderPath.cs` 543–544), so no name is ever typed.

## (2) Revised card

```markdown
<!-- phrase-card:426 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** bug
- **Repository:** avowry (tools/Avowry.Bench/PathScrub.cs, linked into tools/Avowry.BenchCs; tests/Avowry.Acceptance.Tests)
- **Source:** issue 426 (card 418 second review, findings 2, 3 and 5)
- **Depends on:** none (418 done)
- **Story:** As Dana, I want the bench to remove every folder path from a stored row, in the shell and escape forms it misses today, without losing the words around a path, so that I can share a row that names no account and still read what failed.
- **Consumer value:** A row from a run under WSL, Git Bash or a tool that prints JSON-escaped text names no account folder; a diagnostic reads `Could not find file x.cs in /usr/lib/dotnet`, not `Could not find file dotnet`; a stored label holds no folder path.
- **Acceptance criteria (at most 3, each failing on partial work):**
  1. **Scenario: [Z][E][B] the words around a path stay**
     - **Given** a stored diagnostic holding each text (proved through `PathScrub.Apply`, which both tools call on save):
       | text | stored as |
       | `Could not find file /tmp/x.cs in /usr/lib/dotnet` | `Could not find file x.cs in /usr/lib/dotnet` |
       | `copy /tmp/a/x.cs to /tmp/b/y.cs` | `copy x.cs to y.cs` |
       | `see C:\a\b.cs (line 3) and /docs/x` | `see b.cs (line 3) and /docs/x` |
       | `match \\d+ here` | `match \\d+ here` |
       | `see C:\a\x.cs.` | `see x.cs.` |
       | `C:\a\x.cs, then` | `x.cs, then` |
       | `/x then /tmp/a/y.cs` | `/x then y.cs` |
     - **When** the scrub runs
     - **Then** each row reads exactly as listed, and every existing 418-U1, 418-U2 and 418-U5 row (including `D:\My Projects\Sub Folder\x.cs` → `x.cs` and the spaced account folders) still reads as it does today.
  2. **Scenario: [M][I] no account folder survives in the forms the scrub misses**
     - **Given** each text, written once with `<user>` and once with a plain-letter account folder built from parts at run time:
       | text | stored as |
       | `/mnt/c/Users/<user>/x.cs` | `x.cs` |
       | `/mnt/c/Users/<user>` | `<path>` |
       | `/c/Users/<user>/x.cs` | `x.cs` |
       | `/c/Users/<user>` | `<path>` |
       | `/var/tmp/a/x.cs` | `x.cs` |
       | `/private/tmp/a/x.cs` | `x.cs` |
       | `file://localhost/home/<user>/x.cs` | `x.cs` |
       | `C:\Documents and Settings\<user>` | `<path>` |
       | `\\host\Users\<user>` | `<path>` |
       | `\u0027C:\\Users\\<user>\\x.cs\u0027` | `\u0027x.cs\u0027` |
       | `\\\\host\\Users\\<user>\\x.cs` | `x.cs` |
       | `C:/Users/<user>/x.cs` | `x.cs` |
     - **When** the scrub runs
     - **Then** each row reads exactly as listed, neither the placeholder nor the plain-letter folder appears in any output, a second pass changes nothing, and `https://host/path` and `https://example.com/home/x` stay unchanged.
  3. **Scenario: [O][S][I] a label with a folder path, and a retry that then passes**
     - **Given** the Phrase bench and the C# baseline each run a passing reply with `--label C:\Users\<user>\notes\run-426`, and a second run of each with `--label run-426`
     - **When** each row is saved
     - **Then** `label` reads `run-426` in `results/<model>.json` and in the history line, in both tools, for both runs; and the stored row of 418-3b (a real broken Phrase build, then a correct reply) is passed, after 2 attempts, and passes the residue check.
- **Criterion checks:** (1) and (2) fail if any listed row differs; (3) fails if the label line of `PersistResult` (tools/Avowry.Bench/Program.cs line 817) or of `Persist` (tools/Avowry.BenchCs/Program.cs line 850) is deleted, and the red record shows both deletions failing.
- **ZOMBIES:**
  - Z: text with no listed root (`/usr/lib/dotnet`, `\\d+`, a bare `/x`) stays byte-identical (1).
  - O: one label (3).
  - M: twelve root forms (2).
  - B: a path ending a sentence with a dot or comma; two paths on one line (1).
  - I: the WSL, Git Bash, `file://localhost` and JSON-escape forms (2); the save step in both tools (3).
  - E: an error that names both a file and a folder (1).
  - S: a retry that then passes, 418-3b (3).
- **Must keep passing:** PathScrubForms (418-U1, U2, U4, U5 and the run-twice check, which loops over the new rows too), BenchRowsHoldNoFolderPath, NewBenchRowsCarryNoReply (the 336 and 120 pushed-row hashes), BenchmarkHarnessAcceptance (plain labels), PrivateHoldoutResults.
- **Not in this card:** the rule that an account folder alone takes the words after it (`C:\Users\<user> is denied` → `<path>`, the finding 1 fix, kept); the `\\?\` prefix (`<path>?\x.cs`); a TMPDIR outside the listed roots; a new hook to prove the save-step scrub of diagnostics and error; the residue regex in BenchRowsHoldNoFolderPath (line 503); rewriting pushed rows (0 hold these forms); `.mob/records`.
- **Size:** M (one regex in one shared file, about 30 new rows, and two short bench runs per tool).
- **Done when:**

    dotnet test Avowry.sln --filter "Status!=Pending"

**Test note:** test inputs use only `<user>`-style placeholders and a plain-letter folder built from parts at run time, never one literal name. Checks against the real home and temp folders read `Environment.GetFolderPath(UserProfile)` and `Path.GetTempPath()`. A test runner script prints test inputs from a file, because `cmd` `echo` treats `<` and `>` as redirects.
```

**Split:** one slice is enough. Only the regex in `PathScrub.cs` changes, and all three criteria test it. It must be split only if the owner wants the leak fixes (scenarios 2 and 3) before the publish and the over-scrub fix (scenario 1) after it.

## (3) Open points for the facilitator

- The bug's "rank after the publish is fine" holds only for the over-scrub; the forms in scenario 2 can store an account folder (0 committed rows hold one today).
- If the owner ranks the leak fixes before the publish and the over-scrub fix after, split the card: scenarios 2 and 3 now, scenario 1 later.
- No test can prove the save-step scrub of diagnostics and error today, so the card limits "fails if deleted" to the label: confirm, or ask the architect for a test hook.
- Scenario 3 and the 418-3b assertions will probably pass on arrival; the red seat proves them by deleting line 817 or 850 and records it.
- The card keeps a path outside the listed roots (`/usr/lib/dotnet`) unchanged, in line with the existing `/a/b` row, rather than cutting it to `dotnet`: confirm.
- Treating a drive path glued to a letter as a path can catch `http:` in a URL, so the fix should accept only a JSON `\uXXXX` escape before the drive; the URL rows pin this.
- Review finding 3 asks for a `\\?\` row and the bug leaves it out; the card leaves it out too: confirm.
- Architect hint: banning whitespace at the end of a folder name fixes all three over-scrub rows and keeps every spaced-folder row (checked by reading only).
- My read-only tool calls used the workspace's absolute path, which sits under `<home>`, as a file argument; nothing was written, and this reply does not contain that path.
