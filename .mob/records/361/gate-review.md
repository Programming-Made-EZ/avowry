# Card 361 independent gate review (fresh Claude context, read-only in the main checkout; own clone)

**Step 0:** `git merge-base --is-ancestor 8ed59b157f6f86d52d0e12dc174c442ec42b04ad HEAD`, tested commit 249610e6b3fc (main), exit status 0.

**Verdict: SHIP.** No defects found; the surviving mutants are test limits, not blockers.

## 1. Order and red-first
Commits 57f94f2 (slice), c52f981 (characterisation), 44c7381 (red), a25dad6 (MOB.md regeneration), 8ed59b1 (green). `git diff 44c7381 8ed59b1` on the two red test files shows exactly one removed line in each: the `Status=Pending` trait. Red replay in a detached worktree at 44c7381 (CLI built, 0 errors, Pending traits removed uncommitted): `dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Slice=361"` gave `Failed! - Failed: 2, Passed: 24, Total: 26`, all by assertion: C1 to C9 "expected AVnnn, actual PHnnn; stderr still names PHnnn" for 100, 203, 001, 010, 330 (both arms), 020, 337, 340, 341; L1 124 hits in 5 files; L2 354 in 50; L3 90 in 3; L4 7 in 1; L5 1; L6 23 in 13: exactly the red record's counts.

## 2. The rename is exact
- Byte-for-byte reproduction at a25dad6 (the architect's two-pass sed on the 66 matching tracked files outside the history paths, the `git mv` of the `StaysPH325` test file, the hand edit of `tests/test_getting_started.py:48` to `\b(?:P[H]|AV)\d*\b`): the `git diff --cached` of the reproduction against 8ed59b1 differs only by the two Pending traits the green removes by design; MOB.md identical. The hand edit is genuinely needed (the sed does not reach it: `PH` there has `b` in front, so the `\b` pass misses it). Git reports one rename (R085) plus modifications; every changed file has equal insertions and deletions apart from the two trait lines.
- History untouched: `git diff --stat a25dad6 8ed59b1 -- .mob docs/decisions docs/retros bench/results board MOB.md` is empty.
- Numbers and messages: the prefix-only normalisation of the src diff is clean (124 lines change twice and 6 once, all with a prefix token); `CodesKeepTheirNumbers` passes 20 of 20.
- Live-tree probes (digit, `nnn`, `###`, `"`, `\d`, bare word) return zero hits outside the allow-list; before the rename there was no live `AV` plus digit, so no collision. `PHRASE`/`ORPHANS`/`Phrase` line count is 603 before and after. All 68 changed files are LF; no whole-file rewrites.

## 3. Coverage (D222)
C1 to C9 (C5 split into C5a and C5b, one per arm), L1 to L6 plus the added L7, and H1 to H6 are all in the red record. The architect's added forms (bare `PH` prose lines, `PH\d` regexes, the `"PH"` literal, `PHnnn`, `PH###`, `PH1xx`) are in L6 and L1; `ExtensionKeywords.cs` is covered (mutation c was caught at `ExtensionKeywords.cs:88`).

## 4. Mutation table (in a worktree of the reviewer's clone at 8ed59b1; restored, `git status` clean)
| # | Mutation | Result |
|---|---|---|
| a | `CSharpProvider.cs:74` AV020 changed to the old prefix | Killed: C6 ("expected AV020, actual PH020") and "L1 src: 1 hits" at `CSharpProvider.cs:74` |
| b1 | `CSharpProvider.cs:65` AV340 changed to AV341 | Killed by the C8 CLI row only; `CodesKeepTheirNumbers` does not cover adapter codes (as the red record said) |
| b2 | `Resolver.cs:116` AV010 changed to AV011 | Killed by `CodesKeepTheirNumbers` (`M01-AV010`, expected 010, actual 011) |
| c | `ExtensionKeywords.cs:88` key changed to the old prefix, Resolver left at AV330 | Killed three ways: Compiler.Tests failures (`M11-AV330`, `M18-AV200-trigger`), the CLI test C5a with `ArgumentException: 'AV330' is not a registered diagnostic`, and L1 at `ExtensionKeywords.cs:88` (so `Require` does throw on a missing key) |
| d1 | an old-prefix code appended to `docs/GRAMMAR.md` | Killed: "L3 live-docs: 1 hits (docs/GRAMMAR.md:275)" |
| d2 | an old-prefix code appended to `scripts/check-extension-map.py` | Killed: "L4 scripts: 1 hits (...:250)" |
| e | an old-prefix code appended under `docs/retros` | Survived by design (25 of 25 pass; the guard is quiet) |
| f | the `test_getting_started` regex narrowed to the old prefix with `AV325` added to the README | Survived: the unit test reports OK; with the green regex the same README fails with "ph-code: still present" |
Test limits: (f) nothing committed fails if the regex is narrowed again, because the README is clean today; (b1) the numbers of the adapter codes AV020, AV337, AV340, AV341 rest on the CLI rows alone; the scanner reads files from disk, not committed blobs.

## 5. Gate in the reviewer's clone
`dotnet build src/Avowry.Cli`: 0 errors. `dotnet test Avowry.sln --nologo --filter "Status!=Pending"`: `Passed! - Failed: 0, Passed: 344, Total: 344 - Avowry.Compiler.Tests.dll`; `Passed! - Failed: 0, Passed: 376, Total: 376 - Avowry.Acceptance.Tests.dll (9 m 34 s)`; no flake. `python -m unittest tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_extension_map tests.test_getting_started tests.board.test_story_persona`: Ran 78 tests, FAILED (failures=8): the expected eight (`test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`, `test_lists_equal_baseline` x2, T11). What Dana sees: a duplicate avow prints `avowry: Compilation failed:` then `AV010 (7:1) Avow 'ping {m: Int}' has the same wording as 'ping {m: Int}'.`, exit 1; a tab-indented source prints `AV100 (4:1) Indentation must use spaces, not tabs.` then `AV106 (4:3) Native block has no body.`

**Gate evidence:** tested commit 249610e6b3fc, which contains the green 8ed59b1; copy: a fresh `git clone --no-hardlinks --single-branch -b main` plus detached worktrees at 44c7381, a25dad6 and 8ed59b1 inside it; dotnet processes at start: 1.
**Load:** the gate suite ran alone; the mutation runs came after it, one at a time. A dotnet process was present at the end (count 1, as at the start).

## Facilitator note
My own isolated baseline gate on the green (clone, CLI first): Compiler 344 and Acceptance 376, no failure listed. Stray files of the reviewer's: `C:\Users\<user>\projects\gate361-review` with untracked `_x/`, `files.txt`, `mut.sh` and three worktrees (no recursive delete is allowed; removal by `git worktree remove` for the worktrees).
