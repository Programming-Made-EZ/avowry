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
