# Proposal for Decider D255: the task-force declaration and card text for card 426 (the bench path scrub keeps the words around a path, covers more path forms, and its label and save-step behaviour is tested)

Facilitator, 2026-11-11 UTC, main at 2872761. VOTE on this declaration: adopt the Ready card text of .mob/records/426/ready-body.md (the product manager seat's draft with the facilitator's rulings; the issue body is the same text) as card 426's Ready text, with the task force below. Card 426 is a bug found by card 418's second review; the product owner ranked it BEFORE the publish card (188 depends on it). Size M. It changes no agreement text; Decider protocol step 6 (D224) applies with the D232 paragraph (the re-run with swapped seats is trigger (d) here: no change to the agreement); the D228 preflight and the D234 evidence block apply; the seat plan cites .mob/records/426/probe-all-2026-11-11.jsonl (taken 2026-11-11 13:31 UTC; available: agy, claude, codex, cursor, grok, qwen; out: copilot, muse until 2026-11-16).

STANDING-RULES PREFLIGHT: Card format rule (the three criteria cover Z, O, M, B, I, E, S, each titled with its letters); TDD red-first (every new-state row of criteria 1 and 2 is watched failing by assertion on the current tree; criterion 3 and the 418-3b assertion may pass on arrival and are DECLARED as characterisation up front, proved by deleting the label line in each tool); D204 (one red commit with the criteria as Pending traits, then the greens); D206; D219/D226 step 0 in the gate review; D222 id lists in the red record; D234 evidence block; D236 live probe cited; D243 merge verification before every gate; D251 toolchain stamp (dotnet --version, python --version, sorted SDK folder names before and after each gate and sign-off: a changed stamp voids the run); D254 (a test that asserts a wall-clock limit runs alone in the isolated collection) applies only if the card adds such a test (it does not expect to); D249 applies only if the card adds a new kind of tracked file (it does not expect to); D245 does not apply. The product owner's ruling of 2026-11-10 (stricter): every brief and every prompt carries the account-name rule: never type, echo, search for, pass as an argument or write the local account name anywhere, only $HOME, %USERPROFILE% or <home>; leaks are counted by scripts that read the name from the environment. Consumer value: Dana can share a bench row that names no account folder and still read what failed.

## Facilitator rulings (main at 2872761)

- Facts: the draft traced the regex by reading and did not run it; the red seat proves every row by running it. The draft found 0 committed rows holding any of the missed forms (the facilitator's earlier counts: 0 of 336 and 0 of 120 pushed rows carry the account name); nothing pushed changes.
- Ruling 1 (scope): one card; the three criteria are the over-scrub rows, the missed root forms (each twice: with the placeholder and with a plain-letter folder built from parts at run time) and the label and the retry that passes (418-3b). Not in this card: the doubled-backslash prefix `\\?\`, a TMPDIR outside the listed roots, a new hook to prove the save-step scrub of diagnostics and error, rewriting pushed rows.
- Ruling 2 (the seats): read-only architect slice by a fresh Claude context; the RED by a Grok WRITING seat (grok_write.py, own linked worktree, changes left uncommitted, the facilitator committing them); the GREEN by a Cursor WRITING seat (cursor_write.py, own linked worktree); red and green on different seats; the facilitator runs every gate command in person in an isolated clone with D243 merge verification and the D251 stamp, with nothing else running during a full suite; a second read-only review (told to try to break the change), an independent gate reviewer and the sign-off are fresh Claude contexts; a facilitator takeover is logged and, when it changes production code or a check, comes with its failing test committed first.
- Ruling 3 (every seat brief repeats): keep each file's existing line endings; the account-name rule exactly as worded above (typing it counts: a seat that types it, even as a search term, reports the slip); no recursive delete; no commit trailer; a seat that finds a flaw in a test it was not asked to change stops and reports it.
- Ruling 4 (the retro): seven voices at once, the retro input committed BEFORE the window, with the idle-minutes and Dispatch-nudge lines, the Evidence (per card) section and the list of drivers sidelined for behaviour.

## The card (verbatim)

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
       | `match \d+ here` | `match \d+ here` |
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
- **Rulings (facilitator, 2026-11-11; the draft read the code and did not run it, so the red seat proves each row by running it):** (1) one card, not split: the owner ranked bug 426 before the publish as a whole; (2) the save-step 'fails if deleted' proof is for the label only (diagnostics and error are scrubbed earlier in both tools, so no test can observe their save-step scrub without a new hook; none is added); (3) a path outside the listed roots (`/usr/lib/dotnet`, a bare `/x`) stays unchanged, as the existing `/a/b` row; (4) the `\\?\` prefix stays out of this card; (5) the regular-expression row uses ONE backslash (`\d+`, left alone), as the bug says; a doubled backslash row is not claimed; (6) a drive path glued to a letter or digit counts only after a JSON `\uXXXX` escape or a quote-like boundary, so `http:` and URLs stay unchanged (the URL rows pin it); the architect lists the exact forms and the whitespace-at-the-end-of-a-folder-name idea from the draft; (7) criterion 3 and the 418-3b assertion may pass on arrival (characterisation, declared up front); the red seat proves them by deleting the label line in each tool and records both failing; criteria 1 and 2 must fail first by assertion on the rows; (8) every leak row is written twice (the placeholder `<user>` and a plain-letter folder built from parts at run time); no real account name is ever typed; (9) card number 426 (the issue number), persona Dana, size M.
- **Not in this card:** the rule that an account folder alone takes the words after it (`C:\Users\<user> is denied` → `<path>`, the finding 1 fix, kept); the `\\?\` prefix (`<path>?\x.cs`); a TMPDIR outside the listed roots; a new hook to prove the save-step scrub of diagnostics and error; the residue regex in BenchRowsHoldNoFolderPath (line 503); rewriting pushed rows (0 hold these forms); `.mob/records`.
- **Size:** M (one regex in one shared file, about 30 new rows, and two short bench runs per tool).
- **Done when:**

    dotnet test Avowry.sln --filter "Status!=Pending"

**Test note:** test inputs use only `<user>`-style placeholders and a plain-letter folder built from parts at run time, never one literal name. Checks against the real home and temp folders read `Environment.GetFolderPath(UserProfile)` and `Path.GetTempPath()`. A test runner script prints test inputs from a file, because `cmd` `echo` treats `<` and `>` as redirects.

## AMENDMENT AFTER THE PRIMARY WINDOW OF D255

Primary window result (all six voters at once): five Yes (QA agy, architect Grok, coach Cursor, developer Cursor, decider agy) and one No (audit lens Qwen). The No is a condition: the card's Done-when only runs tests, so the practice must also be auditable from the repository state alone: mandate an explicit git-based verification step (scanning the staged diff and the committed tree for the account name and reviewing the regular-expression change in the committed diff) before the test run. The proposer adopts it in this form: before every gate and before the sign-off the facilitator commits, in the card's record folder, the output of a git-based audit: (1) the committed diff of tools/Avowry.Bench/PathScrub.cs and of the two Program.cs files since the card's base, listed in full so the regular-expression change is reviewable from git; (2) the count of the account name over the staged diff and over the whole tracked tree, produced by a script that reads the name from the environment (scripts/home_paths.py and a getpass-based count), never a typed name; a count other than 0 voids the gate. This is added to the Done-when as a facilitator step; the test command itself is unchanged.

Confirmation step (D232): the objector is asked one question: is your condition now met by this form? The five Yes voters are asked one question: does the amendment change your vote?
