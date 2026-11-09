# Card 361 red (D157 Criteria 361-1 and 361-2), QA (red) seat

Two Pending new-state tests, committed together (one atomic green can only turn them green together; the facilitator ruled that D204 governs over D137's one-pending rule for this card, see the Ready draft's notes). No production code changed (nothing under `src/`, no docs change, no rename). Pre-change tree: `57f94f200eb0e7e8256edcc4289e3b4245847128`; characterisation commit (a): `c52f9817139f4769bcfd9ad915a2dee370433e4f` (see `characterisation-record.md`).

- `AvPrefixedDiagnostics.The_CLI_prints_AV_codes_with_unchanged_numbers` (`Slice=361`, `Criterion=361-1`, `Status=Pending`), rows C1 to C9 (ten CLI builds: C5 has two arms).
- `NoPhCodesInTheLiveTree.Tracked_live_files_contain_no_PH_diagnostic_code` (`Slice=361`, `Criterion=361-2`, `Status=Pending`), rows L1 to L6 plus a catch-all L7. This test and the scanner were committed in (a) with their Pending trait (the scanner, its planted rows and the history test share one file); commit (b) adds the CLI test and this record.

Reading note: this record is under `.mob/` (history allow-list), so it quotes old-prefix codes as evidence. Nothing under `tests/` written by this seat contains the old prefix as a token (the scanner passes over them).

## D222 list: the Ready draft's rows first

| id | construct | kind |
|---|---|---|
| C1 | `lexer-AV100` (tab in indentation) | NEW-STATE (criterion 1) |
| C2 | `parser-AV203` (reuse M08) | NEW-STATE |
| C3 | `resolver-AV001` (reuse M13) | NEW-STATE |
| C4 | `resolver-AV010` | NEW-STATE |
| C5 | `resolver-ext-AV330` (through `CoreArm` and `WebArm`) | NEW-STATE |
| C6 | `adapter-AV020` | NEW-STATE |
| C7 | `resolver-AV337` | NEW-STATE |
| C8 | `adapter-AV340` | NEW-STATE |
| C9 | `adapter-AV341` | NEW-STATE |
| L1 | `src` | NEW-STATE (criterion 2) |
| L2 | `tests` | NEW-STATE |
| L3 | `live-docs` (GRAMMAR, ADR-extension-map, PATTERNS, CODING_GUIDELINES) | NEW-STATE |
| L4 | `scripts` | NEW-STATE |
| L5 | `working-agreement` | NEW-STATE |
| L6 | `matcher-literals` (`PH\d`, `"PH"`, `PH10`, `PHnnn`, `PH###`, `PH1xx`) | NEW-STATE |
| H1 to H6 | `.mob/`, `docs/decisions/`, `docs/retros/`, `bench/results/`, `board/`, `MOB.md` | CHARACTERISATION (in (a)) |

## The slice's changes, with the reason

| change | reason (architect's slice) |
|---|---|
| C4 is card 324's M01 source, C5 is M11 (phrase arm) plus an endpoint source (endpoint arm), C6 is the `UnsupportedCsharpConstructs` row at line 160, C7 the first failing `UndeclaredNativeFail` row, C8 the `two-effects` row, C9 the first `ReservedGeneratedNames` row | every row reuses an existing trigger source; no new construct is invented |
| scanner regex is `P[H](?:[0-9]|n{3}|\b)`, case-sensitive, no left anchor | `StaysPH325` has no word boundary before the prefix; `ORPHANS`, `Phrase`, `PHRASE_X` must stay out; the character class keeps the pattern's own line clean |
| L6 also covers four bare-prose lines the draft missed | the architect found a bare prefix with no digit in `BenchmarkHarnessAcceptance.cs` (2 lines), `CliKeepsDiagnosticShape.cs` and `UnsupportedCsharpConstructs.cs` |
| the file list comes from `git ls-files -z` through `ProcessRunner.RunAsync`; content is read from disk | the dotnet CI job checks out at depth 1, so no base-commit blob can be read; in a clean checkout disk equals the committed blobs |
| `CodesKeepTheirNumbers` compares with a committed prefix-free snapshot, not a base-commit blob | same depth-1 reason; the snapshot is generated from the pre-change tree and holds before and after |
| counts: 66 files and 575 lines outside history, src is 6 files | the draft said 5 src files (`ExtensionKeywords.cs` is the sixth) |

## This seat's own choices (deviations from the slice, with the reason)

| change | reason |
|---|---|
| C5 is two builds, C5a `resolver-ext-330-phrase-arm` and C5b `resolver-ext-330-endpoint-arm` | the slice names both arms; one id per construct (D222) |
| a seventh row L7 `other-live-paths` | rows L1 to L6 do not cover a code in `tools/`, `bench/` outside results, `.github/` or the root; without L7 such a hit would be invisible. Measured today: 0 hits |
| L2 and L6 are split by form, not by path: a hit is a code if the text at the match is the prefix plus exactly three digits; anything else (`PH10"`, `PH\d`, `"PH"`, `PHnnn`, `PH###`, `PH1xx`, a bare word) is a matcher literal and counts in L6 whatever the file | the draft defines L6 by form, and the two lists must not double count |
| each C row also asserts exit 1, line 1 `avowry: Compilation failed:`, the diagnostic line number, and that stderr names no old-prefix code anywhere | the slice asks for line 2 and the number; the line number and the whole-stderr scan close a partial rename of a message that mentions another code |
| the Pending live-tree test was committed in the characterisation commit (a), not in (b) | the scanner, its planted rows and the history test live in one class; splitting the class across two commits would have left (a) without its scanner. Pending is filtered by `Status!=Pending`, so main stays green |

## Pre-change failure line per id

Run with the Pending traits removed (uncommitted, restored afterwards, verified with grep):

`dotnet test tests/Avowry.Acceptance.Tests --nologo --filter "Slice=361"`

```
Failed Avowry.Acceptance.Tests.AvPrefixedDiagnostics.The_CLI_prints_AV_codes_with_unchanged_numbers [1 s]
  C1 lexer-100: expected AV100, actual PH100; stderr still names PH100
  C2 parser-203: expected AV203, actual PH203; stderr still names PH203
  C3 resolver-001: expected AV001, actual PH001; stderr still names PH001
  C4 resolver-010: expected AV010, actual PH010; stderr still names PH010
  C5a resolver-ext-330-phrase-arm: expected AV330, actual PH330; stderr still names PH330
  C5b resolver-ext-330-endpoint-arm: expected AV330, actual PH330; stderr still names PH330
  C6 adapter-020: expected AV020, actual PH020; stderr still names PH020
  C7 resolver-337: expected AV337, actual PH337; stderr still names PH337
  C8 adapter-340: expected AV340, actual PH340; stderr still names PH340
  C9 adapter-341: expected AV341, actual PH341; stderr still names PH341
Failed Avowry.Acceptance.Tests.NoPhCodesInTheLiveTree.Tracked_live_files_contain_no_PH_diagnostic_code [402 ms]
  L1 src: 124 hits in 5 files (first: src/Avowry.Adapters.CSharp/CSharpProvider.cs:65, ...:74, ...:81)
  L2 tests: 354 hits in 50 files (first: tests/Avowry.Acceptance.Tests/CliKeepsDiagnosticShape.cs:7, ...:12, ...:13)
  L3 live-docs: 90 hits in 3 files (first: docs/GRAMMAR.md:126, docs/GRAMMAR.md:126, docs/GRAMMAR.md:139)
  L4 scripts: 7 hits in 1 files (first: scripts/check-extension-map.py:66, ...:67, ...:68)
  L5 working-agreement: 1 hits in 1 files (first: TEAM_WORKING_AGREEMENT.md:434)
  L6 matcher-literals: 23 hits in 13 files (first: docs/CODING_GUIDELINES.md:19, src/Avowry.Compiler/Diagnostic.cs:3, src/Avowry.Compiler/Lexing/Lexer.cs:13)
  (L7 other-live-paths: 0 hits, so no line)
Failed!  - Failed:     2, Passed:    24, Skipped:     0, Total:    26, Duration: 1 s - Avowry.Acceptance.Tests.dll (net8.0)
```

(24 passes are the characterisation and scanner tests of the same slice.) Counts are occurrences, not lines (a line with two codes counts twice). Each C row fails by assertion on the prefix (never a missing file or a compile error); each L row names a real count and the first three places.

## Test limits noticed

- The digit-equality part of every C row (the "number changed" branch) cannot fail on the pre-change tree: the prefix check fires first. It is only reachable after the green; the number itself is protected before and after by `CodesKeepTheirNumbers` for the 19 resolver sources (the snapshot does not cover C6 to C9 or the lexer and parser rows; those numbers rest on the CLI rows alone).
- The scanner reads files from disk, not committed blobs: in a dirty working tree an uncommitted edit counts. In a clean CI checkout they are equal. Binary files (a NUL byte) are skipped.
- L7 (and a hit on a file added later outside `src`, `tests`, `docs`, `scripts` and the agreement) is reported but has no named owner row.
- The planted must-not-hit rows pass against a stub that finds nothing; they are only shown able to fail by the second mutation in the characterisation record (a loose pattern turns three of them red).
- The Pending C test depends on `src/Avowry.Cli` being built (`PhraseCli` builds it if the dll is absent and throws if it is stale).

## Checks that stay green with the Pending tests committed

`dotnet test Avowry.sln --nologo --filter "Status!=Pending"` (both Pending tests present, the CLI test file untracked at the time, committed next):

```
Passed!  - Failed:     0, Passed:   344, Skipped:     0, Total:   344, Duration: 1 s - Avowry.Compiler.Tests.dll (net8.0)
Passed!  - Failed:     0, Passed:   374, Skipped:     0, Total:   374, Duration: 9 m 41 s - Avowry.Acceptance.Tests.dll (net8.0)
```

`python -m unittest tests.test_rename_196 tests.test_rename_197 tests.test_live_docs_266 tests.test_extension_map tests.test_getting_started tests.board.test_story_persona`: before my files (characterisation record) `Ran 78 tests`, `FAILED (failures=8)`; after both commits `Ran 78 tests`, `FAILED (failures=8)`, the same eight (five in rename_196: `test_eol_unchanged` x2, `test_lists_equal` x2, `test_new_dirs_tracked_old_gone`; two in rename_197: `test_lists_equal_baseline` x2; T11 in live_docs_266). No count check changed with the new tracked files (none is a corpus fixture), so nothing was adjusted.
