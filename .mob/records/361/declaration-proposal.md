# Proposal for Decider D229: the task-force declaration and card text for card 361 (diagnostic codes use the AV prefix)

Facilitator, 2026-11-09 UTC, main at 8348441. VOTE on this declaration: adopt the card text below (the product manager seat's Ready draft with the facilitator's rulings) as card 361's Ready text, and the task force named in the rulings block: red = a fresh Claude QA context in its own worktree, green = a different fresh Claude context (never the red's seat), architect slice first, independent gate reviewer and sign-off two further fresh Claude contexts, second read-only review by Muse or Qwen, one atomic green with `Slice-exception: rewrite s/OLD/NEW/`, two Pending criterion tests in one red commit (D204, ruled), no cold reader (not a docs card).

STANDING-RULES PREFLIGHT: TDD red-first hard rule, D137 against D204 (ruled in the notes), D136 and its slice-exception, D204 characterisation up front, D206 (guards read committed blobs), D222 (the MULTI id lists C1 to C9, L1 to L6, H1 to H6), the Card format rule, the Value rule (the sponsor persona and Dana read the codes), CODING_GUIDELINES, D127 (no alias). Each is answered in section 0 of the draft. Decider protocol step 6 (D224) applies; this proposal changes no agreement text. The 30 percent rule and the strict reading of a No are unchanged.

The draft (verbatim, with the facilitator's notes first):

# Card 361 proposal (product manager seat, ready; reply-only; a Cursor seat on Probation rung 1, its third real seat)

The seat's reply (Cursor in ask mode, through cursor_reply.py, a fresh context; read-only, it ran nothing), committed verbatim below the facilitator's notes. Two earlier attempts on a Muse seat (called directly with its full path) returned no output within 840 seconds; the brief needs repository reading, which Muse does not finish (the same on card 203's draft). The facilitator supplied the measured reach (git grep counts) in the brief because a Cursor ask-mode seat cannot run commands.

## Facilitator notes and rulings (2026-11-09 UTC, main at 8348441)

- Rerun by the facilitator: src 5 files and 124 occurrences, tests 50 files and 346, live docs 3 files and 78, history docs 9 files and 29, `.mob/records` 70 files and 655, `bench/results` 3 files and 34 (git grep -E "PH[0-9]{3}"); the draft adds matcher literals the pattern misses (PH\d{3}, PH\d+, StartsWith("PH10"), StartsWith("PH"), PHnnn, PH###, PH1xx, PH2xx), `docs/adr/ADR-extension-map.md` and a class and file name ending in `StaysPH325`: the red seat and the gate reviewer check these by reading, since the draft is the only source for them.
- Ruling on D137 against D204 (the draft's flagged conflict): this card has two criteria whose tests fail together and can only turn green together (one atomic green, `Criterion: 1-2`); D204 allows one red commit holding several criteria when the slice says so. Two Pending tests (`Criterion=361-1` and `361-2`) are committed in the red; `UsableV1.Bar6_*` is another card's pending and not a criterion of this card.
- Ruling on `TEAM_WORKING_AGREEMENT.md:434` (PH312 in a living document): rename it (no exception inside a living file).
- Ruling on `tests/test_getting_started.py:48`: the check covers both `PH` and `AV` after the rename, and a README containing `AV325` must fail it (a hand edit, not part of the rewrite rule).
- Ruling on the red's own files: written guard-clean (the character-class form `P[H][0-9]` or built from parts) and excluded from the green's rewrite by path.
- Correction to the draft's section 5 (the declaration): Grok, Codex, Copilot and agy are out of quota. Declared instead: architect Claude (read-only); red a fresh Claude QA context in its own worktree; green a different fresh Claude context in its own worktree (never the red's seat); independent gate reviewer and sign-off two further fresh Claude contexts; second read-only review by Muse or Qwen (records inlined) and it is either run or removed at the red (a declared seat that is skipped gets a dated skip line); Decider voters by rotation among Claude, Cursor, Qwen and Muse (short votes only for Muse).
- The follow-ups the draft names (one shared definition of the codes, the CODING_GUIDELINES item) are filed after the card closes.

---

# Card 361 Ready draft (product manager seat, read-only, nothing run)

**Summary:** the card can go Ready as one atomic green commit. That commit uses one rewrite rule over the tracked live tree and leaves history untouched. Reading the code corrects the card's premise in four ways:

- **Matcher literals the facilitator's count misses.** The pattern `PH[0-9]{3}` cannot see these, and a scan built on it would wrongly pass:
  - `PH\d{3}` in four test files.
  - `PH\d+` in `EdgeCaseFixtureCorpus.cs`.
  - `StartsWith("PH10")` four times in `BenchmarkHarnessAcceptance.cs`.
  - `StartsWith("PH")` in `TriggerSources.cs`.
  - `\bPH\d*\b` in `test_getting_started.py`.
  - `PH1xx` and `PH2xx` in comments.
  - `PHnnn` and `PH###`.
- **A live document missing from the card's list.** `docs/adr/ADR-extension-map.md` has 10 code lines, and `scripts/check-extension-map.py` requires 7 of those codes as audit tokens.
- **A generated file with codes.** `MOB.md` has 2 lines that contain codes.
- **Stale counts in the card.** It is now 19 of 31 corpus `.expected.json` files, not 14 of 23.

## 0. Standing-rules preflight

| Rule | How this card honours it |
|---|---|
| Card format (`TEAM_WORKING_AGREEMENT.md:120`) | A Connextra story for the sponsor persona and 3 Gherkin scenarios. Together they cover Z, O, M, B, I, E and S, and each title carries its letters. |
| TDD red-first, hard rule (`:88`) | Two new-state Pending tests are watched failing on the current tree before the green. Every characterisation test is named up front with its reason (section 3). |
| D137, outside-in TDD (`:81-83`) | Traits `Criterion=361-1` and `Criterion=361-2`. The green removes `Pending` in the commit that makes them pass. **Conflict to rule on:** D137 allows "at most one criterion test pending at a time". `UsableV1.Bar6_*` is already Pending, and this card adds two. D204 (`:519`) allows one red commit holding several criteria when the slice says so. The facilitator rules which rule governs before the red. |
| D136, thin slices (`:126`) | About 60 files, well over the 15-file cap. The green uses `Slice-exception: rewrite s/OLD/NEW/` with the exact rule in section 4, which the audit can verify line by line. Corpus `.expected.json` files are not countable. |
| D204, red before the atomic green (`:519-520`) | Criteria 1 and 2 are new-state, with reds committed and watched failing before the green. Criterion 3 is characterisation: committed before the green and passing on the current tree. |
| D206, guards read committed blobs (`:523`) | The guard selects files with `git ls-files` (tracked files only). So the untracked `.mob/records/234/scratch/` copies, which hold old `PH` fixtures, can never make it fail. |
| D222, two constructs become two ids (`:531-532`) | Criterion 1 is MULTI (ids C1 to C9), criterion 2 is MULTI (ids L1 to L6), and the history half is H1 to H6. The red record lists every id. |
| `CODING_GUIDELINES.md` | Line `:19` says "keep the existing `PHnnn` code style" and becomes `AVnnn`. Line `:41` (one definition for shared names) is not met today: the codes are inline literals at about 120 emit sites. Fixing that is a follow-up, not this card. Line `:31`: every test can fail. Line `:65`, review item 10: emitted bytes unchanged. |
| D127, no backward compatibility; card 162 re-freeze | No `PH` alias is kept. This card lands before card 162, so that 162's re-freeze ratifies AV in `docs/GRAMMAR.md`. |
| `docs/decisions/process-and-staffing.md:1115` | That record says "the `PH###` prefix stays (a later owner ruling or Decider)". The product-owner decision of 2026-11-08 is that later ruling. The facilitator appends it to `docs/decisions`, which is history and is allowed to grow. |

## 1. Premise facts

**F1. Where a code is defined and printed.**
- `Diagnostic(string Code, string Message, int Line, int Column)` is at `src/Avowry.Compiler/Diagnostic.cs:4`.
- `ToString` at `:6` gives `{Code} ({Line}:{Column}) {Message}`. That is the line the CLI prints after `avowry: Compilation failed:`, pinned by `CliKeepsDiagnosticShape.cs:41-44`.
- The doc comment at `:3` says `PH###`, which `PH\d{3}` does not match.
- There is no central table of codes. Every code is a string literal at its emit site.

**F2. Where codes are emitted.** No `PH` grep should stop at the first match; every file below has several.

| File | Lines with a code | Examples |
|---|---|---|
| `Lexer.cs` | 11 | `:87` PH100, `:341` PH108; comment `:13` "PH1xx" |
| `Parser.cs` | 37 | comment `:12` "PH2xx" |
| `Resolver.cs` | 57 | `:116` PH010, `:609` PH001, `:614` PH002, `:836` PH337, `:400` and `:466` `ExtensionDiagnostics.CoreArm("PH330")` and `WebArm("PH330")` |
| `CSharpProvider.cs` | 10 | `:74` PH020; `:65`, `:91`, `:96`, `:119`, `:182`, `:194`, `:198` PH340; `:129` PH341; comment `:81` PH325 |

`ExtensionKeywords.cs:84-91` is a dictionary keyed by code (PH209, PH304, PH322, PH323, PH330, PH331, PH333, PH334), and `CoreArm` and `WebArm` look codes up in it. If the resolver literals are renamed but the keys are not, the lookup misses, so a partial rename fails loudly.

**F3. Where codes are matched in tests and scripts.** These are the traps.

*Caught by a `PH` + digit rule:*
- Plain pins: 346 occurrences in 50 test files.
- Corpus pins: 19 `.expected.json` files and 2 `.avowry` header comments (`undeclared-native-failure`, `native-python-only`).
- `Diagnostics/baseline.txt` (40 rows) and `sources.txt` (19).
- Fixture ids such as `M13-PH001` and `M08-PH203` (`CliKeepsDiagnosticShape.cs:12-13`).
- The class and file name `CompositeReturnTypeStaysPH325.cs`. There is no word boundary before `PH` in that name, so `\bPH\d{3}` misses it.
- `StartsWith("PH10")` at `BenchmarkHarnessAcceptance.cs:366`, `:402`, `:438` and `:467`. That literal has only two digits, so `PH[0-9]{3}` misses it.

*Need a separate rule:*
- The regex text `PH\d{3}` at `ReservedGeneratedNames.cs:13`, `UndeclaredNativeFail.cs:10` and `:14`, and `UnsupportedCsharpConstructs.cs:10`; `PH\d+` at `EdgeCaseFixtureCorpus.cs:88`.
- `PHnnn` at `UndeclaredNativeFail.cs:55` and `:67`, `UnsupportedCsharpConstructs.cs:58`, and `CODING_GUIDELINES.md:19`.
- `StartsWith("PH")` at `TriggerSources.cs:111`. If `baseline.txt` is renamed and this line is not, every baseline row is skipped. `DiagnosticCharacterisationTests.cs:48` (`Assert.NotEmpty`) then catches it, so this partial rename fails rather than passing vacuously.
- `\bPH\d*\b` at `test_getting_started.py:48`, a check that the README and guide show no code. Left alone after the rename, it can no longer fail on a pasted AV code, so it must cover `AV` too.

*Other places:*
- `scripts/check-extension-map.py:66-72` requires PH209 to PH333 as tokens in `ADR-extension-map.md:53-79`, so the script and the ADR must change together.
- `tests/board/test_story_persona.py:59` has a dummy `PH333`.

**F4. Live documents.**
- `docs/GRAMMAR.md` has 67 lines with codes. That includes `:162`, which lists the range "PH300 to PH335" plus PH337, PH340 and PH341, and the rows at `:169` and `:195-197`. A careless range-based replace misses PH020, PH337, PH340 and PH341.
- `docs/adr/ADR-extension-map.md` has 10 lines and `docs/PATTERNS.md:38` has 1 (PH200). `CODING_GUIDELINES.md:19` uses the `PHnnn` form.
- `TEAM_WORKING_AGREEMENT.md:434` has PH312, in a ledger row. **Recommendation:** rename it, because the card names this file as living and the guard should not need an exception inside a living file. The alternative is an exact (path, line) exception, which the architect decides.
- `CHEATSHEET.md`, `TRACEABILITY.md`, `README`, `HANDOFF`, `VISION`, `demo/`, `tools/` and `bench/README.md` have no codes.

**F5. History, which must not change.**
- `.mob/` (records).
- `docs/decisions/` (3 files) and `docs/retros/` (6 files).
- `bench/results/`: 3 files, 34 occurrences of pushed rows. Per `process-and-staffing.md:1115`, `history.jsonl` rows are schema.
- `board/`: generated from the issues, never hand-edited.
- `MOB.md:18-19`: generated from the records.

The card 266 guard in `test_live_docs_266.py:59` keeps `GRAMMAR.md` and `CODING_GUIDELINES.md` byte-identical, but it compares the base commit with the card 266 green commit (`:66-73`), not with HEAD. It does not block this card.

**F6. Generated output, source map and trace.**
- No code appears in emitted C# or in `*.avowrymap.json`.
- `avowry trace` reads only the map and assembly (`Program.cs:23`, `:122-160`), so it never prints a code.
- The orders hash `19fe6d71…` is pinned by `WebExtensionBoundary.cs`.
- Changing `GRAMMAR.md` changes its SHA-256. That only touches `UsableV1.Bar6_*`, which is already Pending and is re-run by card 162 (`UsableV1.cs:101-108`).
- SHA-256 hex is lowercase, so it can never contain `PH`.

**F7. The trap of replacing the bare word.** `PH` without a digit after it appears in `Phrase`, `PHRASE_BENCHCS_DOTNET` (`tools/Avowry.BenchCs/Program.cs:230`) and the `PHRASE` regex in `test_live_docs_266.py:210`. Only `PH` followed by a digit, `\d`, `nnn`, `###` or `"` is a code.

## 2. Scenarios

**Story:** As the sponsor persona, I want every diagnostic code to use the AV prefix, so that the codes I read, paste and search are Avowry's before the preview publishes them.

```gherkin
Scenario: [O][S][I][E] the CLI prints AVnnn with the same number for every emitter
  Given one failing source per emitting site (ids C1 to C9)
  When `avowry build` runs on it
  Then stderr line 2 is exactly `AVnnn (line:col) <unchanged message>`, with the same number as today, and stderr contains no "PH" followed by a digit

Scenario: [Z][M][E] no PH code remains in the live tree, and history keeps its codes
  Given every tracked file (`git ls-files`) outside the history allow-list
  When the guard scans for PH followed by a digit, `\d`, `nnn`, `###` or `"`
  Then it finds zero hits in src, tests, live docs, scripts and the working agreement (ids L1 to L6)
  And every allow-listed history prefix still holds at least one PH code (ids H1 to H6)
  And a planted PH code in any live path makes the guard fail

Scenario: [B] numbers, positions, messages and emitted bytes do not change
  Given the trigger sources, the corpus, orders.avowry and the Tasks demo
  When they are built before and after the rename
  Then each diagnostic's number, line, column and message (prefix stripped) is identical
  And the orders hash and the Tasks golden file are byte-identical, and trace and map output contain no code
```

All seven ZOMBIES letters are covered:

| Letter | Covered by |
|---|---|
| Z | An empty live hit list (scenario 2). |
| O and S | One simple build error (scenario 1). |
| M | Every emitter and every live file (scenarios 1 and 2). |
| B | The low end PH001 and PH020, the high end PH341, the two-digit `PH10`, `StaysPH325` with no word boundary, and unchanged numbers (scenario 3). |
| I | The CLI stderr line and the `Diagnostic.Code` string (scenario 1). |
| E | A reintroduced code fails the guard (scenario 2). |

**How each scenario fails on partial work:**
- Scenario 1 fails if any one of the 5 source files is missed: its C id still prints PH.
- Scenario 2 fails if any live area is missed, including the matcher literals (L6). It also fails if history is scrubbed or an allow-list entry is dead (H ids).
- Scenario 3 fails if the rewrite touches a number, a message or emitted bytes.

**Ids (D222):**

| Kind | Ids |
|---|---|
| Criterion 1, NEW-STATE | C1 `lexer-AV100` (tab), C2 `parser-AV203` (reuse M08), C3 `resolver-AV001` (reuse M13), C4 `resolver-AV010`, C5 `resolver-ext-AV330` (through `CoreArm` and `WebArm`), C6 `adapter-AV020`, C7 `resolver-AV337`, C8 `adapter-AV340`, C9 `adapter-AV341` |
| Criterion 2, NEW-STATE | L1 `src`, L2 `tests`, L3 `live-docs` (GRAMMAR, ADR-extension-map, PATTERNS, CODING_GUIDELINES), L4 `scripts`, L5 `working-agreement`, L6 `matcher-literals` (`PH\d`, `"PH"`, `PH10`, `PHnnn`, `PH###`, `PH1xx`) |
| Criterion 2, CHARACTERISATION | H1 `.mob/`, H2 `docs/decisions/`, H3 `docs/retros/`, H4 `bench/results/`, H5 `board/`, H6 `MOB.md`. Each allow-list entry carries its reason in the test. |

## 3. Named tests and what each shows on the current tree

| Test | Kind | Current tree |
|---|---|---|
| `AvPrefixedDiagnostics.The_CLI_prints_AV_codes_with_unchanged_numbers` (new; `Slice=361`, `Criterion=361-1`, `Pending`; rows C1 to C9) | NEW-STATE, MULTI | Each row fails by assertion: "Expected `AV100 (…`, actual `PH100 (…`". |
| `NoPhCodesInTheLiveTree.Tracked_live_files_contain_no_PH_diagnostic_code` (new; `Criterion=361-2`, `Pending`; rows L1 to L6) | NEW-STATE, MULTI | Each row fails, listing its hits. The red record quotes the per-area counts, which should be about src 5 files, tests 50, docs 4, scripts 1, the working agreement 1, and the matcher literals. |
| `NoPhCodesInTheLiveTree.Scanner_*` planted-tree rows (new) | NEW-STATE unit | These are watched failing against a stub scanner first, as on card 324. Must hit: `PH325`, `M01-PH010`, `StaysPH325`, `PH10"`, `PH\d`, `"PH"`, `PHnnn`, `PH###`. Must not hit: `Phrase`, `PHRASE_X`, a code under `.mob/`. |
| `NoPhCodesInTheLiveTree.Every_history_allow_list_entry_still_holds_a_PH_code` (rows H1 to H6) | CHARACTERISATION | Passes now and after. It fails if history is rewritten or an entry goes dead. |
| `CodesKeepTheirNumbers.Every_trigger_source_reports_the_same_number_position_and_message` (new; compares with the prefix stripped against `baseline.txt` read from the base commit's blob, D206) | CHARACTERISATION | Passes now. This is the only evidence that the rewrite kept the numbers: the existing pins are rewritten by the same green, so they cannot show it. |
| `WebExtensionBoundary` (orders hash), `TasksDemoGeneratedCsharp…golden_file`, `SourceMapTraceability`, the corpus rows with outcome `builds` (map records), a trace row (no code in output) | CHARACTERISATION, existing | Pass now and after. These cover review item 10. |
| The 346 existing PH pins, `CliKeepsDiagnosticShape`, `DiagnosticCharacterisationTests`, the corpus rows with outcome `diagnostic` | Impacted, existing | These pass now and are rewritten in the green. They are not reds, and the red record says so. |

**Trap for the red seat:** its own files must stay guard-clean. Write "no PH code" assertions as a character class such as `P[H][0-9]`, or build them from parts. Run the guard over the red's own files. Exclude the red's files from the green's rewrite by path.

## 4. Size, the one atomic green, and what is not in the card

**One atomic green is right.** Every partial split leaves main red:
- Renaming the emitters alone breaks the 346 pins.
- Renaming the pins alone breaks those same pins against the old emitters.
- Renaming the resolver alone throws at the `ExtensionKeywords` lookup.
- Renaming the ADR alone fails `check-extension-map.py`.

The green commit carries `Criterion: 1-2` and `Slice-exception: rewrite s/OLD/NEW/`. Its single rule is applied to `git ls-files`, minus the allow-list and the red's test files:

```
s/PH\([0-9]\|\\d\|nnn\|###\|"\)/AV\1/g
```

That rule plus one `git mv` of `CompositeReturnTypeStaysPH325.cs` (with its class name) is the whole change, except one hand edit: `test_getting_started.py:48`. The rule would turn it into `\bAV\d*\b`. The architect decides whether it should match both `PH` and `AV`, and its failure check is that a README containing `AV325` must fail.

The audit replays the rule and compares bytes. The gate reviewer confirms that history is untouched with:

```
git diff --stat <base>..<green> -- .mob docs/decisions docs/retros bench/results board MOB.md
```

The output must be empty, apart from facilitator record commits.

**Size:** M by reach. The work is mechanical: about 60 files, about 470 code lines, plus about 80 doc lines.

**Not in this card:**
- Changing numbers or message wording.
- Moving the codes into one shared definition (the `CODING_GUIDELINES.md:41` gap). That should be a follow-up card.
- Rewriting the board archive, open issues that cite PH codes, retros, decision records, records or bench rows.
- Card 162's re-freeze and its LLM re-run for Bar 6.
- Publishing (card 188).
- Removing xUnit.
- Any alias or compatibility for `PH`, per D127.

## 5. Task-force declaration

| Seat | Who and what |
|---|---|
| Product manager (Ready) | Cursor, read-only (this draft). |
| Architect (slice) | A fresh Claude context, read-only. It fixes the rewrite rule and its file list, the allow-list and its reasons, the `TEAM_WORKING_AGREEMENT.md:434` call, the `test_getting_started` regex, the mechanism for reading the base blob, and the D137 versus D204 pending ruling with the facilitator. |
| Red (QA) | A fresh Claude context in its own worktree. It writes the two Pending tests, the scanner rows (watched failing against a stub), and the characterisation tests. The red record quotes each id failing, or passing for characterisation, on the current tree. |
| Green (developer) | A different fresh Claude context in its own worktree, never the red's seat. It applies the rule, does the `git mv`, removes `Pending`, and runs the Python checks (`test_getting_started`, `test_story_persona`, `scripts/check-extension-map.py`). |
| Independent gate reviewer | A fresh Claude context. It replays the reds on the current tree (every C and L id fails, every characterisation test passes). It checks the D222 ids against section 2, replays the rewrite rule byte for byte, runs the history `git diff --stat`, and runs the gate in a clone. |
| Second read-only review | Muse, or Qwen with the records inlined. Its focus is matcher literals missed by the rule, and false positives of `PH` followed by a digit in live files. |
| Sign-off | A separate fresh Claude context that runs its own full gate. |
| Cold reader | None. The prose is unchanged apart from a token, so D206's docs-card seat does not apply. |
| Decider voters | Claude, Cursor, Qwen and Muse, by rotation. |
| Domain expert | None needed. |

**Done when:**

```
dotnet test Avowry.sln --filter "Status!=Pending"
```

The guard must report zero live hits and every history entry still holding a PH code. CI on main must be green, and the card must land before card 162.
