The bug is real and wider than issue 418 says. It affects both bench writers, not only the C# one. Any failed `dotnet test` build output reaches `diagnostics`, and process start errors and model stderr reach `error`. None of the pushed rows carry the account folder, but 13 pushed C# rows still hold absolute temp paths in which the account segment was already replaced by `<user>`.

(Your shell call was declined, so I read everything with workspace-scoped searches. No home path or name appears in any search I ran.)

## (1) Key facts verified

**C# baseline writer (`tools/Avowry.BenchCs/Program.cs`)**
- **Scratch folder (line 122):** it is `Path.GetTempPath()/phrase-bench-cs/<guid>`, and the verification project goes under it (line 212). On Windows this is `%USERPROFILE%\AppData\Local\Temp\...`.
- **Build output with no results file (lines 242–245):** stdout and stderr are joined and passed to `CompactFailureSummary` (lines 825–831). That keeps up to 6 lines containing "error" or "failed", capped at 600 characters. Compiler lines start with the absolute `Candidate.cs` path and end with the absolute `[...Verification.csproj]`, so the path leaks. This is the bug as filed.
- **`tests_did_not_run` (line 250):** this branch writes `tests_did_not_run: {output.Trim()}`. That is the whole dotnet output, uncapped and unscrubbed: a second leak.
- **Test failures (lines 253, 263–265):** the row gets the test name plus the first line of the failure message, capped at 600. This leaks only if a test message itself holds a path.
- **Retry feedback (lines 176–178, 824):** diagnostics are appended to the row and also pasted into the next attempt's prompt. So the path is also sent to the model vendor.
- **`error` field (line 190):** it holds `exception.Message`. That can be:
  - model stderr (line 726);
  - a start failure of dotnet or agy through `StartProcess` (lines 742–751); I believe the .NET message names the working directory, but I have not verified this;
  - a timeout (no path).
- **Other fields carry no path today:**
  - `reply_head` is empty (line 840);
  - `cold_flags` stores the template with a literal `<scratch>` (line 763), because `ExpandColdFlags` (lines 792–796) only builds arguments.
- **Saving (lines 842–856):** the same JSON goes to `results/<model>.json` and to `history.jsonl` (or the private holdout path).
- **Test seam:** the `PHRASE_BENCHCS_DOTNET` environment variable (line 230) swaps in a stand-in dotnet.

**Phrase bench (`tools/Avowry.Bench/Program.cs`)**
- **Scratch folder (line 279):** `Path.GetTempPath()/phrase-bench/<guid>`.
- **Model stderr into `diagnostics` (line 310):** on a retry, the model's exit message (built from stderr at line 678) is added to `diagnostics`. So model stderr reaches `diagnostics`, not just `error`.
- **Default parse-resolve scoring is clean:**
  - a failed compile stores diagnostic codes only (line 734);
  - a reply with no Phrase stores `no_phrase_found` (line 751).
- **Executed scoring leaks the same way as C#:**
  - compiler diagnostics are stored as `Code (line:col) Message` (line 845; `src/Avowry.Compiler/Diagnostic.cs:6`), with no path;
  - the generated project lives under scratch (line 871);
  - a dotnet build failure goes through `CompactFailureSummary` (lines 879–888, 894–896, 1412–1419), carrying the same absolute paths.
- **No test seam:** `dotnet` is hard-coded (line 1367), so there is no stand-in like the C# one.
- **`error` field (line 356):** `exception.Message`, as in the C# writer.
- **Stored-result reader (lines 1475–1498):** `BenchmarkResults.ReadStoredResult` reads `diagnostics` and `error` verbatim. It is not a leak path and needs no change.

**Existing tests that must keep passing**
- **`CSharpBaselineAcceptance.cs`:**
  - 141–165: stand-in dotnet; asserts the `tests_did_not_run:` prefix plus a marker;
  - 194–206: `returns_hello` and the 600-character cap;
  - 601–613: real compiler; asserts `CS0117`/`CS0029` plus the message text, which survives a path scrub;
  - also 222, 238, 253, 268 (`exited 7`), 283, 391, 497 and 593.
- **`BenchmarkHarnessAcceptance.cs`:**
  - 663–693 asserts each stored diagnostic appears in a later prompt. If the scrub ran only when the row is saved, a diagnostic with a path would break this, so the scrub belongs where the score is built;
  - `error` assertions at 54–56, 543, 714, 744, 768 and 1099 hold no paths;
  - 150–215 already plant `C:\` and `~\AppData` forms (reply_head precedent).
- **`PrivateHoldoutResults.cs`:** asserts nothing on `diagnostics` or `error`.
- **`NewBenchRowsCarryNoReply.cs`:**
  - 11–14 hold the hashes and the 336 / 120 row counts;
  - 77–137 cover every ending (`errorContains: "exited 7"`);
  - 142–170 check that the pushed prefix hashes stay the same.

**Pushed rows**
- **Line counts:** `bench/results/history.jsonl` has 336 lines, `bench/csharp-baseline/results/history.jsonl` has 120, and `bench/results/self-check/history.jsonl` has 240.
- **C# history:** 13 rows hold `C:\Users\<user>\AppData\Local\Temp\phrase-bench-cs\...\Candidate.cs(...)` in `diagnostics` (for example lines 18, 34 and 45). The account segment is already `<user>` from the card 370 rewrite, and 0 rows have any other segment after `Users\`.
- **No other path hits:** no pushed `error` holds a path, and the Phrase and self-check histories have none.
- **So the facilitator's count of 0 is right.** Those 13 rows would still trip the drive-path pattern in `scripts/check-bench-history-clean.py:17` if it were pointed at the C# history; its default target is the Phrase history (line 45). I did not run it.
- **Count pin:** no new test should pin the count of 0. Any byte change to the 120 rows already fails the hash pin (`NewBenchRowsCarryNoReply.cs:147–148, 169–170`), and `NoAccountNameInTrackedFiles` / `NoHomePathsInTrackedFiles` already guard the account name. A third copy adds nothing.

**Changes from the issue as filed**
- The Phrase bench and the `error` field are added.
- The "[I][B] pushed rows" scenario is dropped: it is already pinned and passes before any work, so it cannot fail on partial work.
- "[Z][S] nothing else changes" is not observable as written.

## (2) Revised card

# New bench rows name files by their short name and hold no folder from the machine that ran them

- **Shape:** Bug
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

## (3) Open points for the facilitator

- **Model stderr in `error`:** the issue keeps it out, but criterion 2 covers it at no extra cost with the same save-time scrub. Keep it in, or split it out?
- **Retry prompt (criterion 3):** scrubbing the retry prompt also stops the path going to the model vendor and keeps test 663–693 consistent. Drop it only if the size must stay S.
- **Phrase bench seam:** the Phrase bench hard-codes `dotnet` (line 1367). Add a stand-in variable like the C# one at line 230, or prove that route with a unit test of the shared scrub?
- **Linux path rule:** should it match any `/a/b` run, or only home and temp roots? A broad rule could mangle `</Project>` or URLs.
- **Start-failure message:** confirm that a failed process start names the working directory on the repo's .NET version before relying on criterion 2's start-failure check.
- **The 13 `<user>` rows:** confirm they stay. They satisfy the owner's ruling but would trip `check-bench-history-clean.py` if it were pointed at the C# history.
- **Count pin:** I recommend no test pinning 0 rows. The hash pin and the tracked-file account scans already cover it.
- **Test failure messages:** a path in a failing test's message is covered by scrubbing every `diagnostics` entry, so it needs no separate scenario.
