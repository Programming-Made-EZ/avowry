# Proposal for Decider D235: the task-force declaration and card text for card 370 (tracked files carry no home path of a local account)

Facilitator, 2026-11-09 UTC, main at dfa2c83. VOTE on this declaration: adopt the card text below (the product manager seat's Ready draft with the facilitator's rulings) as card 370's Ready text, and the task force named in the rulings block: a read-only architect slice, a red (a fresh Claude QA context in its own worktree: characterisation, then the two Pending criterion tests, the scanner rows against a stub and the generator red), green 1 (the guard, the hook, the generator and the value sites) and green 2 (the generic rewrite over the tracked files, the MIGRATION.txt re-pin and the facilitator's board regeneration) by a different fresh Claude context, a second read-only review by Qwen (or a dated skip line), no cold reader, independent gate reviewer and sign-off two further fresh Claude contexts.

STANDING-RULES PREFLIGHT: TDD red-first hard rule, D137 against D204 (ruled as on cards 361, 366 and 371), D136 (slice exceptions for the rewrite and the generated files), D204 characterisation up front, D206, D222 (the id lists A, F, G, N), the Card format rule, the Value rule (the sponsor persona publishes a tree with no account name in a path), CODING_GUIDELINES (one definition of the home-path pattern; no absolute paths in output), pushed history is not rewritten. Each is answered in section 0 of the draft. Decider protocol step 6 (D224) applies, with the paragraph added by D232; this proposal changes no agreement text. The 30 percent rule and the strict reading of a No are unchanged. The D228 preflight applies to this window.

The standing evidence of the practice (D234; the committed block, quoted here):

STANDING EVIDENCE OF THE PRACTICE (committed template; every line can be checked with git; included in every task-force declaration proposal):
1. Review records. For every card the cold-reader report (a docs card), the second read-only review (when run), the independent gate review (opening with the step 0 line: the `git merge-base --is-ancestor` command, the tested commit and its exit status, and listing the files and criteria it checked) and the sign-off are committed under `.mob/records/<card>/`, and the closure record `.mob/records/<card>.json` names the full commit hashes of the ship and signoff seats (the closure check refuses a card without them).
2. Separate sessions. The closure record lists each seat with its own context label (Claude ctx-N, all distinct; the green never the red's), each seat's work is its own commit, and the red and the green work in their own worktree branches. The decider seats' launches are lines of `.mob/records/seat-use.jsonl` (seat, role, context, time). No claim of model isolation is made: separate sessions, worktrees and distinct commits are the evidence.
3. Red-first order. The characterisation and red commits precede the green in git; the gate review replays the red on the pre-change tree and the green changes the red files by the Pending traits only.
4. Idle minutes and Dispatch nudges are two committed lines in each retro (D230).
5. Per card, not only in general. Each card's retro document carries a section `Evidence (per card)` that lists the full commit hashes of that card's review records (the cold-reader report, the second review when run, the gate review, the sign-off) and of its red and green commits, copied from the closure record `.mob/records/<card>.json`, so that the block's lines 1 to 4 are instantiated for the card in git and checked by `git show` on those hashes; the declaration of the next card quotes the previous card's section as the example of the evidence it will produce.

The previous card's per-card evidence (the example the next declaration quotes): section 9 of docs/retros/card-371-2026-11-09.md, which lists the full hashes of card 371's Ready draft, slice, card 186 revision, characterisation and red, green, cold reader and second review records, page revision, gate review and sign-off.

The draft (verbatim, with the facilitator's notes first):

# Card 370 proposal (product manager seat, ready; reply-only; a Cursor seat on Probation rung 1, its sixth real seat)

The seat's reply (Cursor in ask mode, through cursor_reply.py, a fresh context; read-only, it ran nothing), committed verbatim below the facilitator's notes.

## Facilitator notes and rulings (2026-11-09 UTC, main at dfa2c83)

- Rerun by the facilitator: 329 tracked files and 926 lines carry a Windows home path with an account name by `git grep` on main (253 under `.mob/records`); the draft's further findings (the generator `scripts/board/generate_archive.py` expanding placeholders to the local checkout path; `check-board-archive.py` refusing a hand-edited board file; `docs/decisions/MIGRATION.txt` pinning a hash over the first 106 decision records; `HANDOFF.md`, `TEAM_WORKING_AGREEMENT.md:137`, `bench/csharp-baseline/results/history.jsonl` and the 24 decision files under `.mob/records/decisions/` missing from the facilitator's list; the board count about 61 files) are the red seat's and the architect's to measure by command: the draft is the only source for them. The facilitator's "about 4" for `board/` was a truncated listing.
- Ruling 1, D137 against D204: as on cards 361, 366 and 371, one red commit holds the criteria that turn green together; here two Pending criterion tests (`370-1`, `370-3`) and the order red, green 1 (the guard), green 2 (the rewrite).
- Ruling 2, `docs/decisions/MIGRATION.txt`: green 2 re-pins it; the gate checks that the new hash equals the proof computed over the rule-applied records; the red measures whether D191 is inside the 106 records.
- Ruling 3, bench rows: `bench/csharp-baseline/results/history.jsonl` and other tracked bench files are tracked files and are scrubbed (the owner's words: scrub them from tracked files; history stays public). Card 361's "pushed bench rows stay" was about the diagnostic prefix, not about paths.
- Ruling 4: the account name standing alone (in the bench checker and one test row) goes to a follow-up card; GitHub issue bodies are not tracked files and are out; the owner is told in the retro.
- Ruling 5: the neutral form is `<user>` replacing only the account segment (existing convention; `<` cannot occur in a Windows file name); a short reasoned allow-list (`runner`, the two fictional fixtures pinned by file, the bundle path).
- Ruling 6, the rule's own text: the facilitator's commit messages and the records never print the account name; the rule is written generically (any account segment not on the allow-list becomes `<user>`), the guard and the red record print file:line:form, never the matched text.
- Correction to the draft's section 6 (the declaration): Grok, Codex, Copilot and agy are out of quota. Declared: a read-only architect slice (fresh Claude); red a fresh Claude QA context in its own worktree; green 1 and green 2 a different fresh Claude context in its own worktree (never the red's seat); the facilitator runs `generate_archive.py --apply` after green 2 and records the command and output; a second read-only review by Qwen (records inlined) or none with a dated skip line; no cold reader (not a docs card: the story promises an absence the scan checks); independent gate reviewer and sign-off two further fresh Claude contexts (the gate reviewer runs the characterisation test in a full clone, as on card 361); Decider voters by rotation among Claude, Cursor, Qwen and Muse (short votes only for Muse).
- The review records are committed under `.mob/records/370/` and named in the closure evidence; the retro carries the `Evidence (per card)` section (D234).

---

# Card 370 Ready draft (product manager seat, read-only, nothing run)

**Summary:** the card can go Ready at size M as a red followed by two greens: a code green that adds the guard, then a rewrite green. Reading the code changes the card's premise in four ways:

- **The board generator puts the paths back.** `scripts/board/generate_archive.py:85-86` replaces the issue placeholders with the local checkout path and a hard-coded home path (`:32`) every time it regenerates. So scrubbing `board/` holds only if the generator changes in this card.
- **`board/` cannot be hand-edited.** `check-board-archive.py:52-54` compares each file against the hash in its header, so a rewritten board file is refused by the commit hook. These files must be regenerated.
- **A pinned hash covers a line that holds a path.** `docs/decisions/MIGRATION.txt` pins a sha256 over the first 106 decision records. D51 (`compiler-and-adapters.md:29`) holds a path and is inside that range, so rewriting it fails `check-decisions-index.py:46-55`, which is a commit-hook step (`.githooks/pre-commit:9`).
- **The reach is larger than the count.** By my reading, `board/` is about 61 files, not about 4. The facilitator's other per-directory counts add up to 269 of 329, so "about 4" is probably a typo. The facilitator's list also leaves out:
  - `HANDOFF.md` (8 lines);
  - `TEAM_WORKING_AGREEMENT.md:137`;
  - `bench/csharp-baseline/results/history.jsonl` (13 rows, JSON-escaped);
  - 24 decision files under `.mob/records/decisions/`.

## 0. Standing-rules preflight

| Rule | How this card meets it |
|---|---|
| Card format (`TEAM_WORKING_AGREEMENT.md:120`) | The filed card covers only O, S, Z, E and I; **M and B are missing**. The scenarios below cover all seven letters. The story is rewritten for the sponsor persona. |
| TDD red-first, a hard rule (`:88`) | Criteria 1 and 3 are new-state tests, watched failing on the current tree. Criterion 2 is declared characterisation up front. |
| D137 (`:81-83`) against D204 (`:525`) | This card has two Pending criterion tests (370-1 and 370-3). **Ruling needed**, as on card 361: one red commit holding both criteria, each with its own "Criterion" line. |
| D136 slices (`:126`) | Green 2 carries `Slice-exception: rewrite` for the rule-only files and `Slice-exception: generated` for `board/`. Green 1 has about 12 countable files. If it goes over 15, the architect moves the board-script edits into a card that lands first. |
| D206 (`:528`) | The live-tree scan reads `git ls-files` plus the working tree on purpose (live state). The "nothing else changes" test reads committed blobs, selected by a hash recorded in `.mob/records/370/`. |
| D222 (`:537`) | Area ids A1 to A8, form ids F1 to F9, guard ids G1 and G2, and characterisation ids N1 to N4. The red record lists every one. |
| D234 (`:501`) | Section 6 includes the evidence block. |
| `CODING_GUIDELINES.md` | `:24` says no absolute paths in output, which the generator breaks today. `:41` asks for one definition of shared names: one home-path pattern module, used by the guard, the generator and `check-no-secrets.py`. `:31` says every test must be able to fail: planted rows and a control row. |
| Pushed history is not rewritten (`:108`) | Only the current text of tracked files changes. |

## 1. Premise facts

**F1. What guards home paths today.**
- `scripts/check-no-secrets.py:2` and `:32` check only a staged file named `MOB.md`.
- Its home-path patterns at `:23-24` already exempt the placeholder `<user>`.
- Those patterns miss three forms: doubled backslashes (`C:\\Users\\…` in Python and JSON source), the Claude project-folder slug form (`C--Users-…-`), and URL-encoded forms. The `/Users/` pattern does catch `C:/Users/x` and `/c/Users/x` as substrings.
- The hook is opt-in for each clone (`.githooks/pre-commit:2`), so CI is the only guard every change passes through.
- `MOB.md` has no hits today.

**F2. Why the history scan drops the pattern.**
- `scripts/scan-history-for-secrets.py:5` and `:11` list `{"long hex string", "home path"}` as `NOISY`. Hundreds of history commits hold a home path, which would bury real secret hits, and pushed history cannot be changed anyway.
- `docs/release/pre-publication-checks.md:60` counted them separately: 146 files at `dde36dc`, 137 of them under `.mob/records`. `:62` and `:117` record the owner item (scrub tracked files; history stays public).

**F3. Generated files: regenerate them, never hand-edit them.**
- `board/`: about 61 files. Most of them come from the placeholder expansion at `generate_archive.py:85-86` and `:109`. A few issue bodies hold a literal path (for example cards 036, 227 and 297).
- `generate_archive.py:32` and `check_real_board.py:291` hold a literal home path as a default value.
- `tests/board/test_board_archive.py:173-203` asserts that placeholders are expanded, and `:235` asserts the literal default. Both change with the generator.
- Regenerating needs `gh` authentication, so the facilitator runs it.
- `MOB.md` is not affected.

**F4. Tests that quote a path on purpose.**

| File:line | What it is | Treatment |
|---|---|---|
| `tests/test_live_docs_266.py:219`, `:234` | Pins `HANDOFF.md:3` and `:115` by their exact full line, read from the working tree (t01 and t02, `:395-430`). The paths are written with escaped backslashes in Python source. | The rewrite must change the file and the test identically. The rule must handle `\\`. The BASE comparisons read the card 266 green commit, not HEAD (`:66-73`, `:322-334`), so they do not block this card. |
| `tests/test_bench_history_clean.py:26` | A fictional account (`someone`). | Allow-list by file. |
| `tests/Avowry.Acceptance.Tests/CSharpBaselineAcceptance.cs:441` | A fictional account (`alice`) in a scrubber fixture, with `DoesNotContain` at `:447`. | Keep it and allow-list it. Changing it to `<user>` makes `:447` unable to fail, and the bench scrubber stops at `<`. I found no account name in any acceptance test; the red confirms which file the facilitator's count meant. |
| `tests/board/test_board_migrate.py:19-22`, `:402-403`, `:425`; `test_board_sync.py:450`, `:472-474` | Fixtures for the scrubber at `scripts/board/migrate_cards.py:33-36`, which turns the real path into a placeholder. | Hand edit. A literal rewrite would leave the scrubber unable to scrub any real path while its tests still pass. |
| `tests/test_outcome_extractors_217.py:206` | `/home/runner/`, the CI runner account. | Allow-list. |
| `scripts/check-bench-history-clean.py:9`, `tests/test_bench_history_clean.py:28` | The account name on its own, not inside a path. | Out of this card (ruling 4). |

**F5. Card-361-style traps.**
1. **A rule that is too broad.**
   - A bare-name replace also hits ordinary words that contain the name.
   - A case-insensitive `/users/` also hits URLs such as `api.github.com/users/x`.
   - The rule's own text must never contain the name: write it generically (any account segment that is not on the allow-list becomes `<user>`). Otherwise the commit message `Slice-exception: rewrite s/OLD/NEW/` publishes the name.
   - The guard and the red record print file:line:form, never the matched text.
2. **Content that is pinned.**
   - `MIGRATION.txt`, as described in the summary.
   - `check-wiki-trial.py:236-238` refuses any later change to `freeze.json` or `miss-key.json`. Neither holds a path; only `237/trial/runs/*.json`, `*.tools.jsonl` and `reports/*.md` do. Run the checker before and after.
   - Decision vote files: the CITE checks read the window commit (`TEAM_WORKING_AGREEMENT.md:139`), so a rewrite does not affect them. Run `mob_process.py check` before and after.
   - The mutation patches under `.mob/records/*/mutations` hold no path, so `git apply` stays clean.
3. **Byte level.** Keep CRLF, a missing final newline, and every escape level (`\`, `\\`, `\\\\`). `.gitattributes` only marks the corpus `-text`.
4. **Untracked look-alikes.** `.mob/records/234/scratch/`, the `237` `*.session.jsonl` files, `obj/` and `bench/holdouts-unsealed/` all hold paths but are untracked. The scan and the rewrite use `git ls-files` only, and they must leave untracked files byte-identical.
5. **A binary file.** The bundle under `.mob/records/196` cannot be scrubbed and is history content. Allow-list it by exact path.
6. **The guard's own files.** Write their patterns with character classes such as `U[s]ers`, build test samples from parts, and exclude these files from the rewrite by path.
7. **What the hook reads.** The hook checks staged files only. A whole-tree hook would refuse every record commit between green 1 and green 2.

**Forms the red must measure** (count with `git grep -I`, and also check text files that `-I` skips):
- `C:\Users\x\` (F1);
- `C:\\Users\\x\\` and `\\\\` (F2);
- `C:/Users/x/` and lowercase `c:\users\` (F3);
- `/c/Users/x/` and `/mnt/c/Users/x/` (F4);
- `C--Users-x-`, which is in `HANDOFF.md:115` (F5);
- `/Users/x/`, case-sensitive (F6);
- `/home/x/` (F7);
- `file:///C:/Users/x/` and `C%3A%5CUsers%5Cx` (F8);
- a segment that ends a line, or is followed by a quote, backtick or space (F9);
- UTF-16 text files.

`%USERPROFILE%`, `~` and `SpecialFolder.UserProfile` are neutral and are not hits.

## 2. Decision

- **The neutral form** replaces only the account segment with `<user>`. Everything else stays byte for byte:
  - `C:\Users\<user>\projects\phrase-lang`
  - `C:\\Users\\<user>\\` (escaping kept)
  - `/c/Users/<user>/`
  - `C--Users-<user>-projects-phrase-lang`
- **Why `<user>`:**
  - It is the existing convention: `check-no-secrets.py:23-24` already exempts it, and decision files and board card 283 already use it.
  - `<` and `>` are not allowed in Windows file names, so no real account can be named `<user>`.
  - The scanner exempts exactly that literal segment, plus a short allow-list kept with its reasons: `runner`, the two fictional fixtures (pinned by file), and the bundle path.
- **Scope:**
  - Every tracked file is rewritten, including `.mob/records`, `docs/decisions`, `docs/retros`, `HANDOFF.md`, the working agreement and `bench/csharp-baseline` rows (ruling 3).
  - Git history is unchanged and no file is deleted.
  - The generator keeps the placeholders as they are and neutralises any literal account segment, using the shared pattern.
  - Value sites become derived values with no literal: the defaults become `Path.home() / "mob-crosscli"`, and `migrate_cards` uses a generic pattern.
- **Rulings for the facilitator:**
  1. D204 over D137, so two criteria can be Pending together.
  2. Green 2 re-pins `MIGRATION.txt`. The gate checks that the new hash equals the proof computed over the rule-applied records. The red measures whether D191 (`process-and-staffing.md:1028`) is also inside the 106.
  3. The `bench/csharp-baseline` rows are tracked files, so they are scrubbed (the owner's words). Card 361 had treated bench rows as untouchable.
  4. The account name on its own (in the bench checker and one test row) goes to a follow-up card.
  5. GitHub issue bodies are not tracked files, so they are out. Ask the owner as a follow-up.

## 3. Scenarios

**Story:** As the sponsor persona, I want no tracked file to carry a home path of a local account, so that publishing the repository does not publish where its maintainer works.

```gherkin
Scenario: [O][M][B][S] a scan of the tracked tree finds no home path
  Given every file `git ls-files` lists, in every area (A1 .mob/records, A2 board, A3 tests, A4 scripts, A5 docs, A6 HANDOFF and the working agreement, A7 bench, A8 other)
  When the shared scanner looks for every form F1 to F9
  Then it reports zero hits outside the reasoned allow-list
  And a regenerated board archive also scans clean

Scenario: [Z] nothing else changes and history stays
  Given the base commit recorded in .mob/records/370
  When each tracked file is compared with the rule applied to its base blob
  Then every difference is an account segment that became <user>, except the listed hand edits (N1)
  And no file is deleted (N2), untracked files are byte-identical (N3), and the base blob still holds its home path (N4)

Scenario: [I][E] a guard refuses a new home path
  Given a clone with the hook enabled
  When a staged file that is not MOB.md adds a home path
  Then the commit hook exits non-zero and names the file and line, but not the account (G1)
  And the same file with <user> commits cleanly (G2)
  And the in-gate scan from scenario 1 fails in CI on the same file
```

Each scenario fails on partial work:
- Scenario 1 fails if any area or form is missed, or if the generator still expands paths.
- Scenario 2 fails if the rule is too broad, if a file is deleted, if an untracked file is touched, or if history is rewritten.
- Scenario 3 fails if the guard checks only `MOB.md` or misses the escaped forms.

## 4. Named tests

| Test | Kind | On the current tree |
|---|---|---|
| `NoHomePathsInTrackedFiles.Tracked_files_carry_no_home_path_of_a_local_account` (Acceptance; `Slice=370`, `Criterion=370-1`, Pending; rows A1 to A8) | New-state, multi-row | Each area fails by assertion with a count and the first three file:line places, never the text. |
| `test_home_paths_370` scanner rows (planted samples built from parts) | New-state unit | Watched failing against a stub first. Must hit: F1 to F9. Must not hit: `<user>` in each form, `%USERPROFILE%`, `~/`, `/home/runner/`, `api.github.com/users/x`, the pinned fixtures. |
| `PreCommitRefusesAHomePath.The_hook_names_the_file_that_adds_a_home_path` (Acceptance; `Criterion=370-3`, Pending; rows G1 and G2) | New-state | G1 fails with "expected exit 1, got 0", because `check-no-secrets.py` ignores files other than `MOB.md`. G2 passes; it is the control. |
| `test_board_archive`: the generated file keeps its placeholders and writes no home path; the default comes from the home directory | New-state unit | Fails: the body holds the expanded checkout path. |
| `ScrubChangesOnlyAccountSegments_370` (N1 to N4, base selected by recorded hash) | Characterisation | Passes, because HEAD equals the base. It fails after the green on any edit that is not an account segment. CI checks out at depth 1, so the gate reviewer runs it in a full clone (the card 361 precedent). |
| `check-decisions-index.py`, `check-wiki-trial.py`, `mob_process.py check`, `check-board-archive.py`, `generate_archive.py --check`, `test_live_docs_266` t01 and t02, `test_restart_record`, `test_board_*` | Characterisation, existing | Pass before. They pass after once green 2 rewrites the impacted ones. The red quotes how many each run reported before the green. |

## 5. Size and what is not in the card

**Size: M.** The order of commits:
1. Characterisation.
2. Red (370-1, 370-3, the scanner rows against a stub, the generator red).
3. Green 1: the guard module and script, the hook, the generator, the value-site hand edits and their tests. This removes Pending from 370-3.
4. Green 2: the generic rewrite over `git ls-files` minus the allow-list, the `MIGRATION.txt` re-pin, and the facilitator's board regeneration. This removes Pending from 370-1.

**Not in this card:**
- Rewriting git history, or a force push.
- Deleting or moving files.
- GitHub issue bodies.
- The account name on its own in the bench checker.
- The card 368 `reply_head` trim.
- Board generator bug 375. Whichever of 375 and 370 lands second regenerates the archive again.
- Updating the pre-publication page.

## 6. Task-force declaration

| Seat | Who |
|---|---|
| Product manager | Cursor, read-only (this draft). |
| Architect | A fresh Claude context, read-only. It fixes the rule, the forms, the allow-list and its reasons, the value sites, the mechanism for the `MIGRATION.txt` re-pin, and where the shared pattern lives. |
| Red (QA) | A fresh Claude context in its own worktree. |
| Green (developer) | A different fresh Claude context in its own worktree, never the red's. The facilitator runs `generate_archive.py --apply` and records the command and output. |
| Independent gate reviewer | A fresh Claude context. It replays the red on the pre-change tree, replays the rule byte for byte, runs N1 to N4 in a full clone, checks the D222 ids, and runs `git merge-base --is-ancestor <base> <green>`. |
| Second read-only review | Qwen, or Muse with the records inlined. Its focus is missed forms and false positives. It is either run or removed with a dated skip line. |
| Sign-off | A further fresh Claude context that runs its own full gate. |
| Cold reader | None. This is not a docs card: the story promises an absence, which the scan checks, not what a reader finds. |
| Decider voters | Claude, Cursor, Qwen and Muse, by rotation. Grok, Codex, Copilot and agy are out of quota. |

**Standing evidence (`.mob/records/templates/declaration-evidence.md`, D234):**

> 1. Review records. For every card the cold-reader report (a docs card), the second read-only review (when run), the independent gate review (opening with the step 0 line: the `git merge-base --is-ancestor` command, the tested commit and its exit status, and listing the files and criteria it checked) and the sign-off are committed under `.mob/records/<card>/`, and the closure record `.mob/records/<card>.json` names the full commit hashes of the ship and signoff seats (the closure check refuses a card without them).
> 2. Separate sessions. The closure record lists each seat with its own context label (Claude ctx-N, all distinct; the green never the red's), each seat's work is its own commit, and the red and the green work in their own worktree branches. The decider seats' launches are lines of `.mob/records/seat-use.jsonl` (seat, role, context, time). No claim of model isolation is made: separate sessions, worktrees and distinct commits are the evidence.
> 3. Red-first order. The characterisation and red commits precede the green in git; the gate review replays the red on the pre-change tree and the green changes the red files by the Pending traits only.
> 4. Idle minutes and Dispatch nudges are two committed lines in each retro (D230).
> 5. Per card, not only in general. Each card's retro document carries a section `Evidence (per card)` …

**The previous card's example**, from `docs/retros/card-371-2026-11-09.md:76-86`:

```76:86:docs/retros/card-371-2026-11-09.md
## 9. Evidence (per card) (the first section the D234 entry requires; copied from the closure record `.mob/records/371.json`)

- Ready draft and rulings: `022b3a8921053252ce174f3b63abe36b299d3203`
- Architect slice: `1a82c2f1c0d69f4a9a53b70acc19b09f47bbc080`
- Card 186 revision (loosening): `935573555c547e056e8a48b1acb38248f65a60f1`
- Characterisation and red: `54a687570f2081f6c33fd3ef38207fdaf0af8383`
- Green: `479f2dbd974a7d5f0060b4d8407615931a425928`
- Cold reader and second review records: `9132dbe73ef4fa2501c1890dd3fe66c602e9a3db`
- Page revision 1: `f61e99c24acd5821a1e40dee9341eb34fb3fae3e`
- Independent gate review (ship): `720ce3c53cf5b0c609b66d91122063f3ddf5838a`
- Sign-off: `8eb0ebb7c9e368f9336391aa512c65ac2bd37663`
```

Card 370's retro will carry the same section with its characterisation, red, green 1, green 2, board regeneration, gate and sign-off hashes.

**Done when:**

```
dotnet test Avowry.sln --filter "Status!=Pending"
```

The scan must report zero hits, `check-decisions-index.py` and `generate_archive.py --check` must pass, and CI on main must be green.
