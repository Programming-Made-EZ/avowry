# Card 266 proposal (product manager seat, ready; read-only)

The seat's reply (a fresh context, read-only on the repository), committed with the facilitator's rerun and correction note at the end. Facts were run at HEAD 5069587. "Live" excludes `board/`, `.mob/`, `docs/decisions` and `docs/retros`. Name set P = `\bPhrase\b|Phrase[A-Za-z]|phrase[-_][A-Za-z]`.

## Premise facts
- `git grep -I -n -i phrase -- . <excl>` gives 1913 live lines (card 197's run counted 1994). `git grep -I -n -E "$P" -- . <excl>` gives 725 lines in 115 files; a word-bounded `Phrase` alone is 264 lines in 80 files, `Phrase[A-Za-z]` 128 lines in 37 files. Issue 259's "281 markdown files" is mostly history: live markdown is about 14 files and 55 lines (the facilitator's count: 15 files and 77 lines), not counting the generated verdict page or `bench/results`.

By category:
- **(a) User-facing docs: 55 lines in 14 files:** TEAM_WORKING_AGREEMENT 14 (2 are "Phrase Board", see g), PERSONAS 11 (card 204 owns them), demo/tasks/README 10, bench/README 4, holdout-template 4, HANDOFF, README, VISION, TRACEABILITY and others 1 to 2 each. README line 18 lists `src/Phrase.Providers.CSharp`, a project that no longer exists. README line 5 says the `.avowry` extension "is renamed in a later card"; it is already the extension. VISION line 76 quotes a GRAMMAR rule wording ("open phrase…") that GRAMMAR.md no longer contains.
- **(b) The product name in code:** `PhraseXxx` identifiers: 92 lines in `tests/Avowry.Acceptance.Tests` (`PhraseCli` 58 references), 12 in `Avowry.Compiler.Tests`, about 10 in python and tools; 13 tracked test paths (`PhraseCli.cs`, `PhraseCheatSheet.cs`, `PhraseBuildOneFile.cs` and others); `src/` holds only 3 comments (Lexer, SourceFormatter, CSharpProvider, 2 hits). CLI text: `Program.cs` has 0 `Phrase`; its Usage text and error prefix already say `avowry`.
- **(c) Generated and fixture outputs:** `reference/` and the corpus `*.expected.json` have 0 hits. Existing tests already assert the opposite: `DoesNotContain("Phrase", generated)`, `DoesNotContain("Phrase", version.Text)`, `DoesNotContain("[Phrase(")` (characterisations that pass today). `bench/results`, `bench/csharp-baseline/results` and `docs/results/verdict-2026-10-28.md` hold 115 name-set lines in 7 files: data, not generated output that a test compares.
- **(d) English or unit-concept "phrase":** GRAMMAR.md's only hit is "noun phrase". The language's unit is an avow (D126), so no construct named phrase remains. Lowercase `phrase(s)` survives as the old unit name in about 860 lines in about 60 files (variables, comments, test names, `phrase_root`, fixture comments, file names such as `composite-calling-failing-phrase.avowry`). Eight user-visible diagnostic messages begin "Phrase '…'" or "Phrase has no…" (Resolver.cs and Parser.cs, 9 lines); they mean the unit, not the product. `PH###` codes: 765 occurrences in 37 files, 62 of them in GRAMMAR.md.
- **(e) Frozen D72 documents:** CHEATSHEET has 0 hits; GRAMMAR has 1 (English); `orders.avowry` has 1 (comment "Leaf phrases", unit sense). None is touched.
- **(f) History that stays verbatim:** `board/` 1164 lines in 304 files, `.mob/` 4946 in 263, `docs/decisions` 215 in 7, `docs/retros` 40 in 15, the `history.jsonl` files, the `phrase-card` marker (55 live lines in `scripts/board` and `tests/board`, plus every card body), and the `<phrase-lang>` placeholder (5 lines).
- **(g) External names:** the repository is already `Programming-Made-EZ/avowry`; the CLI command is already `avowry` and no csproj has PackAsTool or PackageId; the GitHub Project is titled "Phrase Board" and `PROJECT_NAME = "Phrase Board"` is looked up by exact string in `scripts/board/project.py` and `check_real_board.py`; the label description is "Phrase board card"; benchmark data keys are `arm: "phrase"`, the `head2head-dev-r` regex keyed on `phrase`, task id `004-read-phrase` (53 references) and the diagnostic `no_phrase_found`.
- **(h) mob-crosscli:** a separate repository; 5 tracked files name `Phrase` (18 any-case): `decider.py`, `README.md`, `TEAM_WORKING_AGREEMENT.md`, a template, test fixtures. Out of scope.

Forms a scan or sed cannot reach: upper-case `PHRASE` constants (26 lines); snake and kebab names (`phrase_root`, `/tmp/phrase-tasks`, which appears in the demo README and is hard-coded in `tests/test_demo_readme.py`, so a README-only sed turns that test red; the `phrase-llm-check-` prefix; `phrase-bench-cs`, 79 lines); escaped regexes in guard tests (`\bPhrase\b` in `test_license_files.py`) and the `DoesNotContain` literals, which must stay; a name broken across a line end (TEAM_WORKING_AGREEMENT line 156, E1 line 23). CRLF: of the 115 files, 7 are CRLF and 6 mixed; the only one in card 266's scope is `docs/experiments/E1-true-mobbing.md`.

## Decisions the card needs
1. **History stays verbatim.** No per-file "formerly Phrase" notes. A live doc that points at history gets one README sentence: "Avowry was named Phrase until 2026-10-29; older records keep the old name." One edit, not about 600 edits that would break the archive's sha256 header and the `phrase-card` markers.
2. **Generated outputs and benchmarks are not renamed.** Nothing committed that a test compares holds the name, so nothing is regenerated. Arm labels, task ids, diagnostic keys and `history.jsonl` rows are schema (renaming them breaks `verdict.py` and comparability). The two prompt files that name the language (`csharp-baseline/tasks/001-write-greeting/prompt.md`, `usable-v1-llm-check/tasks/U1-add-endpoint/prompt.md`) are out of 266 (editing a prompt changes `prompt_hash`).
3. **The CLI command and package need no change** (already `avowry`, no package id): no behaviour change and no red for a rename. The `PH###` code prefix stays: renaming it touches frozen GRAMMAR.md (D72), moves the bar 6 hashes and changes 765 occurrences; it needs its own owner ruling or Decider, so it is not carded now.
4. **The "Phrase Board" project title, label and `PROJECT_NAME` constants stay**: mob-internal external names with no consumer; renaming one is a GitHub action needing owner permission.
5. **Lowercase unit residue is not carded**: internal variable names have no consumer value, so under the Value rule they are waste. Only the user-visible part, the eight diagnostic messages, has a consumer.

## Split
All three cards come in far below the 15 KB cap.
- **Card 266 (keeps; ranks first): live prose docs and a guard test.** About 38 lines in 9 files: README, HANDOFF, VISION, TEAM_WORKING_AGREEMENT, demo/tasks/README with its test, bench/README, holdout-template, docs/experiments. Consumer: Ari, who reads docs first. PERSONAS.md is a named exception pointing at card 204 (its criterion 3 adds its own test).
- **New card A (rank 2): diagnostics say what the language calls things.** Consumer: Ari, who reads compiler diagnostics while writing avows. Change the eight "Phrase …" messages to "Avow …". New-state red: a test enumerates every reachable diagnostic and fails today. ZOMBIES: Z no diagnostic, O one message, M all eight, B the PH codes unchanged, I the CLI output, E a new message containing the old name, S one message.
- **New card B (rank 3): test and code identifiers of the product name.** About 115 lines, 13 file names and the `PhraseCli` helper. Consumer: a future developer searching the repository for the product name. The value is thin, so the Decider may close it as waste. An atomic rename with the byte-parity test (D202) and its own ZOMBIES.
- **Not filed:** the benchmark prompts and verdict-generator text (`prompt_hash` baselines), the `PH` prefix, the mob-crosscli repository, and the unit residue.

## Card text (replaces the issue body; the facilitator adds the phrase-card marker and the new title)
```card
# (266) Live docs and the README use the name Avowry

- **Status:** Ready (after Decider) | **Shape:** Story | **Type:** story (docs)
- **Source:** issue #259, filed by Dispatch for the product owner
- **Depends on:** none | **Related:** 204 owns PERSONAS.md
- **Story:** As Ari, a cold-start coding agent reading the docs, I want one name, Avowry, and a README that describes the repository as it is, so that I search for and run the language I actually install.
- **Scope in:** README.md, HANDOFF.md, VISION.md, TEAM_WORKING_AGREEMENT.md, demo/tasks/README.md, bench/README.md, bench/holdout-template.md, docs/experiments/E1 and E4.
- **Scope out:** history, frozen D72 files, data and results, benchmark prompts, code and test identifiers (new card B), diagnostics (new card A), PERSONAS.md (204), "Phrase Board" and its constants, the PH prefix, mob-crosscli.
- **Risk to shipping:** low. It touches docs only, but demo/tasks/README.md is pinned by test_demo_readme and TEAM_WORKING_AGREEMENT.md by the process-wiki cite check; both must stay green.

Scenario: [Z] a live doc that never named Phrase stays as it is
  Given docs/CODING_GUIDELINES.md and docs/GRAMMAR.md
  When card 266 is merged
  Then both files are byte-identical to before
Scenario: [O] the README names a real project layout
  Given README.md
  When Ari reads its Layout block
  Then every path in it exists, with no Phrase.Providers row and no "renamed in a later card" sentence
Scenario: [M] every named prose file uses Avowry
  Given the nine files in scope
  When the scan runs
  Then none has \bPhrase\b, PhraseXxx or phrase-xxx outside the named exceptions
Scenario: [B] the named exceptions stay
  Given "Phrase Board", "noun phrase", `<phrase-lang>` and phrase-card
  When the scan runs
  Then each is allowed by name with its reason, and the README has one line saying Avowry was named Phrase until 2026-10-29
Scenario: [I] the demo commands still run as written
  Given demo/tasks/README.md
  When test_demo_readme runs
  Then its commands use /tmp/avowry-tasks and match the source, and every relative link in the scoped files resolves
Scenario: [E] a new old-name use fails the check
  Given a scoped file with "Phrase" put back
  When the scan runs
  Then it fails naming the file and line
Scenario: [S] one sentence is renamed
  Given the sentence naming Phrase in VISION.md
  When Ari reads it
  Then it names Avowry and its quoted GRAMMAR rule exists in docs/GRAMMAR.md

Done checks, one per cycle:
1. NEW-STATE. tests/test_live_docs_266.py (scan, README layout, README one-line note) is watched failing on the pre-change tree (HEAD 5069587): about 38 hits, `src/Phrase.Providers.CSharp` missing, no note.
2. NEW-STATE. test_demo_readme.py is edited first to expect /tmp/avowry-tasks and fails on HEAD (the README still says phrase-tasks). The link check goes in the same test file, and the green is the README edit.
3. CHARACTERISATION (the criterion keeps existing bytes and behaviour; it passes on the pre-change tree). A byte-parity test: every tracked file under board/, .mob/records, docs/decisions, docs/retros, docs/results, bench/results, the frozen D72 files and all C#/src paths is byte-identical to the base commit; the Status!=Pending gate equals the baseline. It is committed before the greens and passes at HEAD.
```

## Task force proposal
- **Seats:** PM; architect (slice; SHIP AS IS under 15 KB); QA (red); build seat: agy by the rotation after Grok (DRIVERS.md is stale, so the facilitator probes agy fresh, fallbacks Codex then Copilot; everyone is at Rung 1, so the facilitator re-runs every claim); a non-author reviewer; a cold-reader seat (a fresh model reads the README and the demo README as Ari: the LLM-ergonomics role that fits this card); a fresh Claude sign-off. Left out: UX, security, domain expert.
- **Red plan:** this card is not atomic. It runs as ordinary red-green cycles per criterion, with a separate green each (better under red-first: each criterion is watched failing and its green is its own small commit). Cycle 1: the red is the guard test (criterion 1), then its green. Cycle 2: the red is the demo-test edit (criterion 2), then its green. Cycle 3: the byte-parity characterisation is committed first; it passes at HEAD and is declared as characterisation, not a red. Pre-ship gate: rerun criteria 1 and 2 against the pre-change tree (both must fail) and check the commit order from `git log`.
- **Byte-parity (D202):** for the files changed by the word rule, undoing it on the new bytes must reproduce the old bytes. The rewrite is `sed -b` from a script file, with anchored rules that skip the exceptions. The hand-edited lines (the README layout row, the false sentence, the one-line note, VISION's quote) are listed by name as exceptions. The green seat runs the checker and attaches its output before it claims clean.
- **Baseline:** record the sorted test lists, the python module counts and the `Status!=Pending` summary at HEAD before the green (card 197's baseline: Compiler 284, Acceptance 313 plus 3 known failures). A failure of `BenchmarkHarnessAcceptance.timed_out_model_tree_is_stopped_and_recorded_in_both_arms` alone is rerun in isolation under card 304.
- **Gate:** isolated, in a clean `git archive`; no push or other build runs alongside it; the report carries a Load line for each full run.
- **CI path:** one push after the gate, then `python scripts/check-ci-main.py --wait` for `test`, `usable-v1` and `mob-process` green on the head SHA; record the run URL.
- **Reversal:** `git revert` of each commit (docs only, so main stays coherent).
- **Decider:** a full Decider with the independent re-run, covering the task force, the five decisions, the split and the card text.

## Risks
1. `test_demo_readme` fails when `/tmp/phrase-tasks` changes in only one of the README and the test.
2. TEAM_WORKING_AGREEMENT cite checks (check-process-wiki, test_process_wiki and the D196 cite check) quote agreement text, so editing a cited line breaks them; run them before the green.
3. A blanket sed would corrupt "Phrase Board", "noun phrase", `phrase-card` and the `DoesNotContain("Phrase")` guards; anchored rules and the named exceptions prevent this.
4. The bar 6 hashes and the benchmark prompt hashes cover the frozen docs and prompts; this card touches neither; the criterion 3 test enforces it; the seat did not trace the hash set beyond finding no README dependency in `run-llm-check.py`.
5. Links and anchors: the README and HANDOFF link into docs and `board/`; the link check in criterion 2 covers them.
6. Stale README claims: the test-status table and counts also look old; fixing them is out of scope, so a cold reader can still be misled by them; the card does not claim otherwise.
7. PERSONAS.md keeps the old name until card 204; the guard lists it as a named exception, and 204 removes the exception.
8. Load: the full dotnet gate takes about 11 minutes, so a push during it can make a load-sensitive failure.

## Facilitator's rerun and correction note
Re-run at the boundary (HEAD 5069587): live "phrase" lines 1913; name-set lines 725 in 115 files (match the seat); live markdown with the name set 15 files and 77 lines (the seat counted about 14 and 55: an unreconciled difference, not material to the split); README.md line 18 names `src/Phrase.Providers.CSharp` and line 5 says the extension "is renamed in a later card" (both confirmed); VISION.md line 76 quotes an "open phrase" rule (confirmed present, its absence in GRAMMAR.md not rerun); demo/tasks/README.md uses `/tmp/phrase-tasks` on lines 21 to 45 (confirmed); five reported diagnostics start "Phrase '" in src (Resolver.cs lines 105, 187, 200, 331, 556 seen, the seat reports eight messages in nine lines). **Corrections for the slice:** (1) the card text block lacks the `<!-- phrase-card:266 -->` marker the board tooling needs and its heading is a title line; the facilitator adds the marker, keeps the issue title format `(266) ...` with the new title, and writes Status: Ready once the Decider adopts. (2) HANDOFF.md carries lines that mention the old names as history (line 53 "phraseFile to sourceFile, kind phrase to avow", line 74 "The old keyword `phrase`, the extension `.phrase`", line 83 the "Phrase Board" project, line 99 "personas still say Phrase in eleven places"): each is either a named exception with its reason or an edit, and the guard's exceptions are pinned path and line in the slice, as the audit lens asked on card 197. (3) The new cards A and B are filed after the Decider as Draft stories; card B's consumer must be a persona from PERSONAS.md (Morgan), and its value is low, so it ranks low and may be closed as waste at its Ready step.
