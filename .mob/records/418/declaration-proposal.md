# Proposal for Decider D250: the task-force declaration and card text for card 418 (new bench rows name files by their short name and hold no folder from the machine that ran them)

Facilitator, 2026-11-11 UTC, main at 584bb02. VOTE on this declaration: adopt the Ready card text of .mob/records/418/ready-body.md (the product manager seat's draft with the facilitator's rulings; the issue body is the same text) as card 418's Ready text, with the task force below. Card 418 is a bug filed with card 369's retro and raised by the product owner's direction of 2026-11-10 17:47: it is fixed BEFORE the publish card (188), which now depends on it, and it is ranked first. The card is size S (M if the Phrase bench needs a new seam). It changes no agreement text; Decider protocol step 6 (D224) applies with the D232 paragraph (the re-run with swapped seats is trigger (d) here: no change to the agreement); the D228 preflight and the D234 evidence block apply; the seat plan cites .mob/records/418/probe-all-2026-11-10.jsonl (taken 2026-11-11 02:26 UTC; available: agy, claude, cursor, grok, qwen; out: codex, copilot, muse until 2026-11-16).

STANDING-RULES PREFLIGHT: Card format rule (the three scenarios cover Z, O, M, B, I, E, S, each titled with its letters); TDD red-first (every new-state id is watched failing on the current tree; the tests that exist today and must keep passing are declared as characterisation up front: BenchmarkHarnessAcceptance, CSharpBaselineAcceptance, PrivateHoldoutResults and NewBenchRowsCarryNoReply, including the 336 and 120 pushed-row hashes); D204 (one red commit with the criteria as Pending traits, then the greens); D206; D219/D226 step 0 in the gate review; D222 id lists in the red record; D234 evidence block; D236 live probe cited; D243 merge verification before every gate; D245 does not apply; D247 applies if the card adds a test class that starts a tool under a time limit (it must carry the shared collection and the gate reviewer checks it by hand); D249 (the slice names the guard tests that scan any new kind of tracked file, with guard-sample.md) applies only if the card adds a new kind of tracked file (it does not expect to). The product owner's ruling of 2026-11-10 (stricter): every brief and every prompt carries the account-name rule: never type, echo, search for, pass as an argument or write the local account name anywhere, only $HOME, %USERPROFILE% or <home>, and leaks are counted by scripts that read the name from the environment. Consumer value: Dana can cite or share a bench row without exposing whose machine produced it.

## Facilitator rulings (main at 584bb02)

- Facts: the facilitator counted by script (the name read from the environment, never typed) 0 of the 336 pushed rows of bench/results/history.jsonl, 0 of the 120 of bench/csharp-baseline/results/history.jsonl and 0 tracked files under bench, tools, tests, docs and scripts that carry the local account name; the product manager's draft found 13 pushed C# rows that hold a temp path whose account segment already reads the placeholder `<user>` (from card 370's rewrite); nothing pushed changes. The slice, the red and the gate reviewer recompute every figure.
- Ruling 1, scope: both benches (the C# baseline and the Phrase bench), the `diagnostics` field and the `error` field (the model's stderr stays in scope), the model's retry prompt (the path would otherwise go to the model vendor), the scrub where the score is built and again on save; the Linux and macOS rule matches only paths rooted at home or temp folders plus drive, UNC and tilde forms; the Phrase bench seam is the architect's choice (the smaller change wins).
- Ruling 2, the seats: read-only architect slice by a fresh Claude context; the RED by a Grok WRITING seat (grok_write.py, own linked worktree); the GREEN by a Cursor WRITING seat (cursor_write.py, own linked worktree, changes left uncommitted and the facilitator committing them); red and green on different seats; no Claude stopgap (Grok and Cursor passed their live probes); the facilitator runs every gate command in person in an isolated clone with D243 merge verification, with nothing else running during a full suite; a second read-only review, an independent gate reviewer and the sign-off are fresh Claude contexts; a facilitator takeover is logged and, when it changes production code or a check, comes with its failing test committed first.
- Ruling 3, every seat brief repeats: keep each file's existing line endings; the account-name rule exactly as worded above (typing it counts: a seat that types it, even as a search term, reports the slip); no recursive delete; no commit trailer; a seat that finds a flaw in a test it was not asked to change stops and reports it.
- Ruling 4, the retro (seven voices at once; the retro input committed BEFORE the window) is due after the sign-off with the idle-minutes and Dispatch-nudge lines, the Evidence (per card) section, the list of drivers sidelined for behaviour, and the line the owner asked for: whether any pushed row carried the account folder (the answer so far: none) and what was done.

## The card (verbatim)

<!-- phrase-card:418 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** bug
- **Repository:** avowry (tools/Avowry.Bench, tools/Avowry.BenchCs; bench/results and bench/csharp-baseline/results)
- **Source:** issue 418 (card 368 green seat and second review); the facilitator's check of 2026-11-10; the owner's ruling of 2026-11-10 that pushed rows carrying the account folder are scrubbed (0 found, so none change).
- **Depends on:** none (368 done)
- **Story:** As Dana, I want every new bench row to name a failing file by its short name and hold no folder path from the machine that ran the bench, so that I can cite or share a row without exposing whose machine produced it.
- **Consumer value:** From this card on, a failed build, a failed start or a model's error in any new row reads as `Candidate.cs(7,1): error CS1529: ... [Verification.csproj]`, with no drive, home or temp folder, in both benches, in both the row and the model file.
- **Scrub rule:**
  - Every absolute path in a `diagnostics` entry or in `error` becomes its last segment (the file's short name). A path with no file segment becomes `<path>`. The account name is never written.
  - Absolute paths include drive-letter `X:\` or `X:/`, UNC `\\host\`, `~\` or `~/`, and rooted home or temp paths.
  - The scrub runs before the 600-character cap.
  - It runs where the score is built, so the retry prompt and the row hold the same text, and it runs again on every string when the row is saved.
- **Acceptance criteria (Given/When/Then; at most 3, each failing on partial work):**
  1. **Scenario: [Z][O][B][E] the C# baseline build fails with home-folder paths in its output**
     - **Given:**
       - The C# baseline bench runs with a stand-in dotnet (`PHRASE_BENCHCS_DOTNET`) that prints a planted file and exits 1 without a results file. The file holds `C:\Users\<user>\AppData\Local\Temp\phrase-bench-cs\a\verification-b\Candidate.cs(7,1): error CS1529: planted [C:\Users\<user>\AppData\Local\Temp\phrase-bench-cs\a\verification-b\Verification.csproj]` and the same line under `/home/<user>/tmp/phrase-bench-cs/a/verification-b/`, repeated past 600 characters.
       - A second run's stand-in writes a results file with no harness tests and prints the same file (the `tests_did_not_run` branch).
       - A third run's stand-in prints only `Build FAILED.`
     - **When:** each row is saved.
     - **Then:**
       - In `history.jsonl` and `results/<model>.json`, the first two rows' diagnostics read `Candidate.cs(7,1): error CS1529: planted [Verification.csproj]`.
       - The third row reads `Build FAILED.` unchanged.
       - No file contains `<user>`, `Users\`, `/home/`, `AppData`, `phrase-bench-cs`, a drive-letter path, `Path.GetTempPath()` or the test machine's `%USERPROFILE%`.
  2. **Scenario: [M][E] every other route into diagnostics or error, on both benches**
     - **Given:**
       - The Phrase bench runs a model command that writes the planted Windows and Linux paths to stderr and exits 1 on every attempt. Its first exit reaches `diagnostics`; its last reaches `error`.
       - The Phrase bench, in executed scoring, runs a stand-in dotnet that prints the planted file.
       - The C# baseline runs with `PHRASE_BENCHCS_DOTNET` pointing to a missing file inside a planted `<user>` folder in the test's workspace.
     - **When:** each row is saved.
     - **Then:**
       - Each row still shows its cause: `exited 1`, `CS1529`, and status `unavailable` for the start failure.
       - No `diagnostics` entry or `error` holds any of the forms listed in criterion 1.
  3. **Scenario: [I][S] the model's retry prompt carries the same scrubbed text**
     - **Given:** a model that records each prompt it receives, whose first C# reply fails with the planted output of criterion 1 and whose second reply passes.
     - **When:** the C# baseline bench retries.
     - **Then:** the second prompt's diagnostics block holds `Candidate.cs(7,1): error CS1529: planted [Verification.csproj]` and none of the forms listed in criterion 1, and the passing row has empty `diagnostics` and a null `error`.
- **ZOMBIES:**
  - Z: output with no path passes through unchanged (1).
  - O: one failed build (1).
  - M: every route on both benches (2).
  - B: a path cut by the 600-character cap (1).
  - I: the prompt-to-row interface (3); the stored-result reader and the pushed-row shape stay pinned by `NewBenchRowsCarryNoReply` 368-3.
  - E: build failure, start failure and model error (1, 2).
  - S: a plain retry that then passes (3).
- **Must keep passing:** `BenchmarkHarnessAcceptance.cs`, `CSharpBaselineAcceptance.cs`, `PrivateHoldoutResults.cs` and `NewBenchRowsCarryNoReply.cs` (including the 336 and 120 pushed-row hashes).
- **Rulings (facilitator, 2026-11-10; facts re-run by the draft and by the facilitator's count by script: 0 of the 336 and 0 of the 120 pushed rows carry the account name; the 13 pushed C# rows that hold a temp path already show the account segment as the placeholder `<user>`):** (1) the model's stderr in `error` stays in scope (criterion 2: the same scrub, no extra cost); (2) the retry prompt stays in scope (criterion 3: the path would otherwise go to the model vendor); (3) the Phrase bench has no stand-in dotnet seam today: the architect chooses between adding one like `PHRASE_BENCHCS_DOTNET` and proving that route with a test of the scrub function (the smaller change wins; the card stays S unless a seam is needed, then M); (4) the Linux and macOS rule matches only paths rooted at home or temp folders (`/home/`, `/Users/`, `/root/`, `/tmp/`, `/var/folders/`) and drive, UNC and tilde forms, never a bare `/a/b` run (so `</Project>` and URLs stay); the architect lists the exact forms; (5) the 13 pushed C# rows in `<user>` form stay (the owner's ruling is met: no account name is pushed); no test pins the count 0 (the hash pins of card 368 and the tracked-file account scans already guard it); (6) the scrub is applied where the score is built and again on save; (7) the start-failure message check (whether it names the working directory on the repository's .NET version) is proved by the red seat with a real run before criterion 2 relies on it; (8) card number 418 (the issue number is the card number, as for card 365); persona Dana; size S, or M if a seat is needed.
- **Not in this card:**
  - rewriting any pushed row, including the 13 C# rows already in `<user>` form;
  - the self-check history;
  - changes to `scripts/check-bench-history-clean.py` or to the stored-result reader;
  - capping the `tests_did_not_run` output;
  - `.mob/records`.
- **Size:** S (M if the Phrase bench needs a new dotnet stand-in seam)
- **Done when:**

    dotnet test Avowry.sln --filter "Status!=Pending"

**Test note:** the stand-in prints planted text from a file (`type` or `cat`), because `cmd` `echo` treats `<` and `>` as redirects. Plants use only the `<user>` placeholder. Checks against the real home use `Path.GetTempPath()` and `Environment.GetFolderPath(UserProfile)` and never a typed name.
