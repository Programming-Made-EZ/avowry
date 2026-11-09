# Card 361 slice (architect seat, fresh Claude context, read-only; every figure from commands run on main at 9d51b76)

Committed from the seat's reply (formatting tightened; the content is the seat's).

## 1. Reach, verified
- Quoting trap: `git grep -E 'PH\d'` silently matches nothing; the `\d` form needs `git grep -F -e 'PH\d'`. `PH\d`, `PHnnn`, `PH###` need their own probes.
- Real reach outside the history paths: 66 files, 575 lines. src: 6 files, 127 lines (the draft says 5: `ExtensionKeywords.cs` is the sixth, 9 lines, dictionary keys at :84-91 and a comment at :25). tests, non-corpus: 33 files (including `test_getting_started.py` and `test_story_persona.py`). corpus: 21 files (19 of 31 `.expected.json`, 2 `.avowry`). docs and the rest: 6 files (GRAMMAR 67 lines, ADR 10, PATTERNS 1, CODING_GUIDELINES 1, `check-extension-map.py` 7, the working agreement 1).
- Every match has, in front of it, start of line, space, quote, backtick, parenthesis, `-` (`M13-PH001`), `_` (test names) or a single `s` (`StaysPH325`). No hex or base64 hit (SHA-256 is lowercase); no lowercase or mixed-case form; no dynamically built code (`PH{`, `"PH" +`). False-positive probe: `ORPHANS` (working agreement :115) contains `PH` but is not matched by the rule; `Phrase`, `PHRASE` (heredoc markers, env var `PHRASE_BENCHCS_DOTNET`, python regex names) are not matched either.
- Missed by the draft: four prose lines with a bare `PH` and no digit: `BenchmarkHarnessAcceptance.cs:394`, `:430`, `CliKeepsDiagnosticShape.cs:6`, `UnsupportedCsharpConstructs.cs:29`. `PH1xx`/`PH2xx` comments (`Lexer.cs:13`, `Parser.cs:12`, `LexerDiagnosticTests.cs:6`) and `PH10"` at `BenchmarkHarnessAcceptance.cs:366,402,438,467` are caught by the `PH[0-9]` branch.
- There is no live `AV` + digit, `AVnnn`, `AV###` or `"AV"` anywhere in the tree, so the rewrite cannot collide.

## 2. Rewrite rule, file list, hand edit
No left anchor (`StaysPH325` must be hit); the right side is anchored with `\b` for the bare word. Two passes, in this order:
```
xargs -0 sed -i -E 's/PH([0-9]|\\d|nnn|###|")/AV\1/g; s/\bPH\b/AV/g'
```
fed the list of files that match (`git grep -z -l -a -E -e 'PH([0-9]|nnn|###|")'` plus `git grep -z -l -a -F -e 'PH\d'`), not all of `git ls-files`: it touches only the 66 files and keeps line endings (corpus `.avowry` is `-text`). Samples: `"PH10"` becomes `"AV10"`; `(PH\d{3})` becomes `(AV\d{3})`; `StaysPH325` becomes `StaysAV325`; `PH###` becomes `AV###`; `without PH diagnostics` becomes `without AV diagnostics`. `PHRASE` and `ORPHANS` stay.
Allow-list (history; every entry holds a PH code today): `.mob/` (75 files, records), `docs/decisions/` (3), `docs/retros/` (6), `bench/results/` (3 files, 34 lines: pushed rows are schema), `board/` (38, generated from the issues), `MOB.md` (2 lines, generated from records), and the red's own files by path (guard-clean anyway: `P[H]` never matches the rule).
Hand edit after the sed: `tests/test_getting_started.py:48` becomes `r"\b(?:P[H]|AV)\d*\b"` (the character-class form keeps the line from tripping the scanner). `TEAM_WORKING_AGREEMENT.md:434` is renamed by the rule (no exception). The rewrite is line-preserving, so the line numbers cited inside `ADR-extension-map.md` stay valid.

## 3. Moves
`git mv tests/Avowry.Acceptance.Tests/CompositeReturnTypeStaysPH325.cs` to `...StaysAV325.cs` (the class name inside is renamed by the sed). It is the only tracked path containing PH + digit.

## 4. The two Pending tests
- Traits: `Slice=361`, `Criterion=361-1` or `361-2`, `Status=Pending`, like `UsableV1.cs:101`.
- `AvPrefixedDiagnostics.The_CLI_prints_AV_codes_with_unchanged_numbers` (Acceptance): builds a source with `PhraseCli.RunAsync`, asserts stderr line 2 matches `^AV\d{3} \(l:c\) `. Rows C1-C9 reuse existing sources: C1 AV100 the tab case in `LexerDiagnosticTests`; C2 AV203 `CliKeepsDiagnosticShape`'s M08; C3 AV001 its M13; C4 AV010 M01 of the compiler baseline; C5 AV330 M11 (core arm) plus an endpoint source for the web arm; C6 AV020 `UnsupportedCsharpConstructs` row :174; C7 AV337 the first failing row of `UndeclaredNativeFail`; C8 AV340 `UnsupportedCsharpConstructs` row :189; C9 AV341 the first row of `ReservedGeneratedNames`.
- `NoPhCodesInTheLiveTree.Tracked_live_files_contain_no_PH_diagnostic_code` (Acceptance): the file list comes from `git ls-files -z` run through `ProcessRunner.RunAsync`; the class `GeneratedApiSourceScan` (`ProcessStartCallers`) only counts the literal process-start text, so no new entry is needed (do not write that token; card 186 needed an entry only because it called it directly). Content is read from disk (in a clean CI checkout equal to the committed blobs). Scanner regex, case-sensitive, no left anchor: `P[H](?:[0-9]|n{3}|\b)`. Rows L1-L6: src, tests (including corpus), live docs, scripts, the working agreement, matcher literals and prose forms (the four prose lines, the `\d` regexes, the `"PH"` literal) as a separate hit list.
- Scanner rows: the scanner is a pure function over (path, text) pairs; planted rows build their samples from parts (`"P"+"H"+"325"`); the stub is written first. Must hit: `PH325`, `M01-PH010`, `StaysPH325`, `PH10"`, `PH` + `\d`, `"PH"`, `PHnnn`, `PH###`, a bare `PH `. Must not hit: `Phrase`, `PHRASE_X`, `ORPHANS`, anything under an allow-listed path.
- H1-H6 (characterisation): each allow-list prefix still contains a match; the test carries its reason.
- `CodesKeepTheirNumbers` (Compiler.Tests, characterisation): do NOT use a base-commit blob: CI job `test` uses `checkout@v4` at depth 1 (only `mob-process` has `fetch-depth: 0`, and it runs only the process checker); the card 266 precedent (`comparison_commit()`) is a Python test needing a full clone, not run in the dotnet job. Instead the red commits a PH-free snapshot `Diagnostics/numbers-361.txt` generated on the current tree: `ordinal|digits|line|col|message` per trigger source, in the order of `sources.txt`; the test builds each source and compares with the two-letter prefix stripped, so it passes before and after.

## 5. Order
1. Red commit: the two Pending tests, the scanner with its planted-tree rows, the H test, the snapshot and `CodesKeepTheirNumbers`. Main stays green because Pending is filtered out (`Status!=Pending`) and everything else passes.
2. The red record quotes the failing run `dotnet test --filter "Slice=361"`.
3. Green commit: the sed, the `git mv`, the hand edit, then the Pending trait removed from both tests; `Criterion: 1-2` and `Slice-exception: rewrite`.

## 6. Risks and corrections to the Ready draft
- The counts are wrong (66 files and 575 lines; src is 6 files); the `\d` forms are invisible to `git grep -E`; the four bare-`PH` prose lines are missed by the draft.
- Verified true: `check-extension-map.py` requires the 7 tokens; the ADR has 10 code lines; `ExtensionKeywords` is keyed by code and called through `CoreArm` and `WebArm` at `Resolver.cs:400-493`; `test_getting_started.py:48` needs the edit.
- Unverified: that `Require` throws on a missing key; the claim about `DiagnosticCharacterisationTests.cs:48`.
- The README-with-`AV325` check works through the existing `DROPS` loop (`test_getting_started.py:160`) once the regex covers `AV`. Python checks run inside the dotnet suite (`ExtensionMap.cs`, `GettingStartedDocs.cs`), so a miss turns the CI job red.
- Not covered by the baseline snapshot: C6-C9 (AV020, AV337, AV340, AV341): covered only by the CLI rows.
