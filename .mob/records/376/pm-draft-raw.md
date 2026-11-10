# Card 376 Ready draft (product manager seat, read-only, nothing run)

**Summary:** the card can go Ready at size S, at the top of that size. It needs one red commit and two greens: green 1 changes the code, green 2 rewrites the records. The draft keeps the card's three scenarios but corrects them in three ways:

- **The bench history checker leaks too.** It prints the text it matched, so a hit would publish the account in a log.
- **The hook is the only guard that can catch the name in future.** The commit hook must also refuse an added line that holds the local account name. In CI the account is `runner`, which is on the allow-list, so a tree scan in CI has no name to look for.
- **The bench checker's tests are not gated.** No C# wrapper runs them today, so CI never runs them.

Card 375 stays a separate card (the reasons are in section 2).

## 0. Standing-rules preflight

| Rule | How this card meets it |
|---|---|
| Card format (`TEAM_WORKING_AGREEMENT.md:120`) | The three scenarios below cover all seven ZOMBIES letters, each scenario titled with its letters. |
| TDD red-first (`:88`) | Every new-state id is watched failing on the current tree. The characterisation tests are declared up front (section 4). |
| D137 (`:81-83`) against D204 (`:528`) | **Ruling needed**, as on cards 361, 366, 370 and 371: one red commit holds three Pending criteria, each with its own `Criterion` line. Green 1 removes Pending from 376-2 and 376-3. Green 2 removes it from 376-1. |
| D206 (`:531`) | The record test reads committed blobs, selecting the base by a hash recorded in `.mob/records/376/scrub-commits.txt`. The tree scan reads the working tree on purpose, because it checks live state. |
| D219 and D226 (`:537`) | Not a docs card and no cold reader, so the final-tip rule does not apply. The gate review still opens with the step 0 `git merge-base --is-ancestor` line (item 1 of the evidence block). |
| D222 (`:540`) | The id lists are in section 3, and the red record lists each id. |
| D234 (`:501`) | The evidence block is in section 6. |
| D236 (`:504`) | The seat plan cites a `probe_all.py` output taken within 60 minutes (the facilitator runs it). |
| `docs/CODING_GUIDELINES.md` | `:24` no absolute paths in output. `:31` a test must be able to fail: the tree rows skip in CI with a stated reason, and a skip is not a pass. `:41` one definition: the account-name rule lives in `scripts/home_paths.py`, and the hook, `--tree` and the bench checker all use it. |
| Owner's ruling 4 | Tracked files are scrubbed; history is not rewritten. |

## 1. Premise facts

The account name is never printed in this draft. How a seat counts it without printing it:

- Read the name from the environment, by `getpass.getuser()` and `Path.home().name`. The red reports whether the two are the same (yes or no), never the values.
- Count with `git grep -I -i -w -l -e "$USERNAME"`, which prints paths only, and `-c`, which prints path and count.
- The committed record quotes the command with `$USERNAME` in it, never the expanded value.
- `-w` stops at word boundaries, which keeps out the `LICENSE` false positives (the name sitting inside ordinary words).

**Defect 1: the name standing alone, in 8 tracked files and 9 lines.** My whole-word reading of the working tree matches the facilitator's count:

| File and line | What holds the name |
|---|---|
| `scripts/check-bench-history-clean.py:9` | The `"user name"` pattern is the literal name. |
| `tests/test_bench_history_clean.py:28` | A row holds the name in upper case. |
| `.mob/records/229/nohome-check/nohome229_check.py:20` and `:48` | The script builds a planted path from parts, and later checks that the name is absent. |
| `.mob/records/226/record-in-progress.json:208` | Prose about an escaped backslash in front of the name. |
| `.mob/records/276/record-in-progress.json:38` | Prose about the scrub. |
| `.mob/records/276/signoff-ctx6.md:25` | Prose about the scrub. |
| `.mob/records/313/slice-architect.md:123` | A `git grep` alternation. |
| `.mob/records/313/green1-codex-events.jsonl:5` | An embedded copy of the 313 slice line. |

Further facts on defect 1:

- **The checker prints what it matched.** `check-bench-history-clean.py:26` prints `{match}`, so every hit prints a drive path or the name.
- **No CI run of the bench checker's tests.** No C# wrapper names `test_bench_history_clean`, and `.github/workflows/ci.yml` runs only `mob_process.py check` and `usable-v1-report.py`.
- **The shared rule cannot see the name.** `home_paths.find` (`:138-156`) knows only path segments, which is why card 370's tree scan passes with these 8 files.
- **Untracked look-alikes.** The card 237 trial session logs and a card 228 `.tmp` body hold the name and more. They are untracked: card 370's sign-off count over tracked files excludes them. They stay out because every scan uses `git ls-files`. The new hook row (G3) is what stops a careless `git add` of them.

**Defect 2: `--tree` exits 0 on a hit.**

- `scripts/check-no-secrets.py:161` returns 0 at the end of `scan_tree` unconditionally (`:139-161`), and `main` passes that on (`:167-168`). The docstring at `:12-13` promises only a printout.
- `.githooks/pre-commit:4` runs only the staged mode (`:119-136`). That mode checks added lines for home paths only, never for the account name standing alone.

**Defect 3: a name with a space is cut at the space.**

- In `scripts/home_paths.py`, `SEGX` (`:27`) excludes `\s`, so `SEG` (`:28`) stops at the first space. `path_sub` (`:84-87`) then rewrites `C:\Users\<user> Last\x` to `<user> Last\x`, so the second word is left in the file.
- The text-pattern copy used by `account_root_re` has the same gap (`_SEG_STR`, `:104`).
- `find` still reports the line, so the scan and the rewrite agree; the leak is in the rewrite.
- The `F9_space` row (`tests/test_home_paths_370.py:228`) pins that a space followed by prose ends the segment. It must keep passing.

## 2. Decision

- **The neutral form is `<user>`**, by one rule: the name, as a whole word, case-insensitive, becomes `<user>`.
  - The six record files are rewritten by that rule only, with no hand edits. A sentence such as `Users<sep><user> to <user>` reads oddly but correctly, and the gate can check every changed byte.
  - The checker and its test row are hand edits.
  - Green 2's trailer is `Slice-exception: rewrite s/ACCOUNT/<user>/`, with no name in it.
- **One definition of the name.** `home_paths` gains:
  - `local_accounts()`: `getpass.getuser()` and `Path.home().name`, minus `ALLOWED_SEGMENTS`;
  - a whole-word rule: not preceded or followed by a letter or digit, case-insensitive;
  - a new form id `F10`, "the local account name standing alone".
- **The bench checker stays meaningful once the literal is gone.**
  - Its `user name` kind is built at run time from `local_accounts()`, or from an `accounts` argument, which is how a test plants a name.
  - Its tests plant a name built at run time (for example from parts plus a random suffix), in two ways: injected in-process, and through `USERNAME`, `USER` and `LOGNAME` in a child process's environment.
  - It returns and prints `path:line:kind` only, never the matched text.
  - Its tests get a C# wrapper, so CI runs them.
- **`--tree` exits non-zero on a hit.**
  - Exit 1 when any `F1` to `F10` hit is found; exit 0 on a clean tree.
  - Output: the per-area summary, then every hit as `path:line:form` (capped at 50, like the hook), then `N hits in M files`. Never the matched text.
  - The hook's added-lines mode also checks `F10`.
  - In CI, `F10` has no name to look for, because `runner` is on the allow-list. The guard therefore works locally (the hook, the facilitator's gate, the gate reviewer, the sign-off), and the planted rows run everywhere.
- **The rule for a name with a space.** The account segment may contain single inner spaces (or `%20` in the URL form, F8) only when a separator, a closing quote or a backtick follows it directly. Otherwise the segment still stops at the first space, which keeps `F9_space`. The rule applies to `PATH`, `_SEG_STR` and therefore `find`, `scrub_bytes` and `account_root_re`.
  - A must-not-overreach row guards prose that follows a path.
  - The slug form (F5) cannot tell a hyphenated or spaced account from the rest of the slug. That limit stays and is stated.
- **Card 375 stays separate.** The reasons:
  - It is about a person's first name, not the machine account, and it is enforced by `check-no-personal-names.py`.
  - It is fixed in the issue bodies or the generator, plus a board regeneration that needs `gh`. Card 376 touches no `board/` file and needs no `gh`.
  - It is a Contract bug with its own three done checks, so folding it in would break the three-scenario cap and the S size.
  - The two cards share no file, so either can land first.

## 3. Scenarios (ids per D222)

```gherkin
Scenario: [Z][O][M][S] the name is gone and the bench checker still refuses it   (376-1)
  Given the tracked tree and the local account name read from the environment
  When the whole-word scan reads every tracked file, and the bench checker reads planted rows
  Then the scan finds 0 hits in every area (W-A1 to W-A8; skipped with a reason when no local name is set, as in CI)
  And the checker refuses a row with a planted name at its line (B1), in upper case (B2), and on each of many rows (B3)
  And accepts the same row with <user> (B4), never prints the matched text (B5), and ignores the name inside a longer word (B6)

Scenario: [B][I][E] the tree scan and the hook gate by exit code   (376-2)
  Given a temporary repository whose tracked file holds a planted home path or, with the name planted in the environment, the name standing alone
  When check-no-secrets.py --tree runs, or the hook checks a staged file that adds that line
  Then --tree exits 1 naming file:line:form (T1 home path, T2 F10), exits 0 on the neutral file (T3), and prints no planted text (T4)
  And the hook refuses the added name (G3) and commits the <user> line (G4)

Scenario: [B][I] an account name with a space   (376-3)
  Given planted paths whose account is two words
  When the rewrite and the scan run
  Then the whole segment becomes <user> for backslash (S1), slash (S2), doubled backslash (S3), a closing quote (S4) and %20 (S5)
  And a path followed by prose keeps the prose (S6, as F9_space)
```

How each scenario fails on partial work:

- **Scenario 1** fails if any of the 8 files keeps the name, if the checker loses its rule (B1), or if it still prints matched text (B5).
- **Scenario 2** fails if only the exit code is fixed (T2 and G3 still fail).
- **Scenario 3** fails if only the backslash form is fixed (S2 to S5 still fail), or if the rule eats prose (S6).

## 4. Named tests

| Test | Kind | On the current tree |
|---|---|---|
| `NoAccountNameInTrackedFiles` (C# wrapper, `Criterion=376-1`, Pending): W-A1 to W-A8, plus `test_bench_history_clean` B1 to B6 | New-state | On a local machine: A1 fails with 7 lines in 6 files, A3 with 1 in 1, A4 with 1 in 1 (the red measures the exact counts), printed as counts and `file:line:form` only. B1 to B3: "expected a user-name hit, got none". B5: the output holds the planted drive path. B4 and B6 are controls. |
| `TreeScanGatesByExitCode` (`Criterion=376-2`, Pending): T1 to T4, G3, G4 | New-state | T1 and T2: "expected exit 1, got 0". G3: "expected exit 1, got 0". T3, T4 and G4 pass; they are controls. |
| `ScannerRows` S1 to S6 in `tests/test_home_paths_370.py` (`Criterion=376-3`, Pending through its wrapper) | New-state | S1 to S5 fail on the rewrite assertion (the second word is left). S6 passes; it is the control. |
| C1: `scrub_tracked` over the tree changes 0 files before and after the space rule | **Characterisation** | Passes: the wider rule changes nothing in today's tree. |
| R1: each record changed by green 2 equals its base blob with the rule applied (base by recorded hash; skipped in CI and in a depth-1 clone) | **Characterisation** | Passes, because the target equals the base. |
| Existing `ScannerRows` (including `F9_space`), G1, G2, TreeScan A1 to A8, N1 to N4 | **Characterisation**, existing | They pass; the red quotes the counts each reported. |

## 5. Size and what is not in the card

**Size: S, at the top.** About 14 files:

- `home_paths.py`, `check-no-secrets.py`, the bench checker and its test, the 370 scanner test, two C# wrappers;
- the six records;
- the 376 records.

**Not in this card:**

- Rewriting git history.
- Commit authorship.
- GitHub issue bodies.
- Card 375 (the first name in `board/`).
- A new `ci.yml` step: the in-gate TreeScan already fails CI on a home path, and `F10` has no name to look for in CI.
- Untracked files.
- `LICENSE`: the name only sits inside ordinary words there.
- The slug form with a hyphenated account.
- The secret patterns in `--tree` (the long-hex noise).

## 6. Task-force declaration

| Seat | Who |
|---|---|
| Product manager | Cursor, read-only (this draft). |
| Architect | A fresh Claude context, read-only. It fixes the space rule, the `F10` boundary and the injection interface. |
| Red (QA) | A Muse writing seat in its own worktree. This is the retro's Probation widening for Muse: a red on a code card. |
| Green 1 (code) and green 2 (records, by the rule) | A Cursor writing seat in its own worktree, never the red's. |
| Second read-only review | Grok (back in rotation now that its card 162 run is done), with the records inlined. Its focus is overreach and missed forms. |
| Independent gate reviewer | A fresh Claude context. It replays the red on the pre-change tree and runs R1 in a full clone. |
| Sign-off | A further fresh Claude context that runs its own full gate. |
| Cold reader | None (not a docs card). |
| Decider voters | Rotation among Qwen, agy, Grok, Muse, Cursor and Claude. Codex is out until 2026-11-11 (its card 162 run comes first). |

**Why Cursor takes the green:**

- The card 370 retro widens Cursor to a green.
- Its card 370 red was byte-exact, and green 2 must keep each record's line endings.
- Qwen is not used for green 2, because its line-ending normalisation after every run would change the CRLF records.
- Cursor leaves its changes uncommitted (its commit trailer is refused by the hook). The facilitator commits them with a trailer-free message and records that as one amendment line.

Every seat brief repeats two rules: never type or echo the name (use `$USERNAME`), and committed seat event logs must pass G3.

**Standing evidence** (`.mob/records/templates/declaration-evidence.md`, D234):

> STANDING EVIDENCE OF THE PRACTICE (committed template; every line can be checked with git; included in every task-force declaration proposal):
> 1. Review records. For every card the cold-reader report (a docs card), the second read-only review (when run), the independent gate review (opening with the step 0 line: the `git merge-base --is-ancestor` command, the tested commit and its exit status, and listing the files and criteria it checked) and the sign-off are committed under `.mob/records/<card>/`, and the closure record `.mob/records/<card>.json` names the full commit hashes of the ship and signoff seats (the closure check refuses a card without them).
> 2. Separate sessions. The closure record lists each seat with its own context label (Claude ctx-N, all distinct; the green never the red's), each seat's work is its own commit, and the red and the green work in their own worktree branches. The decider seats' launches are lines of `.mob/records/seat-use.jsonl` (seat, role, context, time). No claim of model isolation is made: separate sessions, worktrees and distinct commits are the evidence.
> 3. Red-first order. The characterisation and red commits precede the green in git; the gate review replays the red on the pre-change tree and the green changes the red files by the Pending traits only.
> 4. Idle minutes and Dispatch nudges are two committed lines in each retro (D230).
> 5. Per card, not only in general. Each card's retro document carries a section `Evidence (per card)` that lists the full commit hashes of that card's review records (the cold-reader report, the second review when run, the gate review, the sign-off) and of its red and green commits, copied from the closure record `.mob/records/<card>.json`, so that the block's lines 1 to 4 are instantiated for the card in git and checked by `git show` on those hashes; the declaration of the next card quotes the previous card's section as the example of the evidence it will produce.

**The previous card's example** (card 370 retro, section 9):

```80:89:docs/retros/card-370-2026-11-09.md
## 9. Evidence (per card) (copied from the closure record `.mob/records/370.json`)

- Ready draft and rulings: `8ca1ba4e54c0727a45436437dcde4f64243c8198`
- Architect slice: `88ce4470a9308cbd1f7c191cdb86c042cf2ba3e4`
- Characterisation and red (Cursor writing seat): `0a3ec382725f5e753bfae8d306cd6d083e834937`
- Green 1, the guard (Muse writing seat): `626172d5606b88a22536edba4a38453ba86c1385`
- Green 2a, the rewrite and the re-pin (Muse writing seat): `a886f63f9bb5b6eedcf458b6ddee22272875b466`
- Green 2b, the board regeneration (facilitator): `e19198dd2d2901b808f14bca60db1aa367595c22`
- Independent gate review (ship): `79fdf9de9380eb4611c14a65d38e3a54f766b8b7`
- Sign-off: `6e37d5d7b9c87152e3fcb685339ced769fbe5fcf`
```

Card 376's retro will carry the same section with the hashes of its Ready draft, slice, characterisation and red, green 1, green 2, second review, gate review and sign-off.

**Done when:** `dotnet test Avowry.sln --filter "Status!=Pending"` passes, and on the maintainer's machine both of these hold:

- `python scripts/check-no-secrets.py --tree` exits 0;
- `git grep -I -i -w -l -e "$USERNAME"` lists no file.
