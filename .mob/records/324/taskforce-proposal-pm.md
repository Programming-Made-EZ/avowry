# Card 324 proposal (product manager seat, ready; read-only)

The seat's reply (a fresh context, read-only on the repository), committed with the facilitator's rerun and corrections at the end. Facts were run at HEAD d7c92f4, from tracked files (`git grep` or `git ls-files`), not the working tree.

## Premise facts
- **Method.** String literals in `src/` matched for `[Pp]hrase` (the regex in a scratch file), interpolation holes stripped first (`{phrase.Template}` is only an identifier).
- **Count.** 19 sites, 18 distinct messages, not eight. The card's "eight" is the set that starts with the word "Phrase": 7 in Resolver.cs and 1 in Parser.cs. Ten more messages name the unit in lowercase or after another word.
- **Trigger coverage.** Confirmed by PH code that a test triggers each message; each message text individually unprobed.

Messages starting "Phrase" (the card's eight):

| Code | File:line | Message text |
|---|---|---|
| PH010 | Resolver 105 | `Phrase '{t}' has the same wording as '{t2}'.` |
| PH321 | Resolver 187 | `Phrase '{t}' returns {r}; use 'let' to keep it.` |
| PH320 | Resolver 200 | `Phrase '{t}' returns no value and cannot be used with 'let'.` |
| PH327 | Resolver 331 | `Phrase '{t}' participates in a call cycle.` |
| PH331 | Resolver 400 | `Phrase specs cannot bind input; use a hole binding.` |
| PH331 | Resolver 408 | `Phrase spec for '{t}' does not bind hole '{n}'. Valid holes: ...` |
| PH334 | Resolver 556 | `Phrase '{t}' cannot fail with '{c}'. Valid options: ...` |
| PH203 | Parser 364 | `Phrase has no implementation; add native <target> or steps.` |

The other ten the card does not list:

| Code | File:line | Message text |
|---|---|---|
| PH325 | Resolver 119 | `Composite phrase '{t}' cannot return a value.` |
| PH328 | Resolver 351 | `Composite phrase '{t}' must declare effect '{e}'.` |
| PH330 | Resolver 389 | `Spec '{d}' on phrase '{t}' must use 'when called'.` |
| PH333 | Resolver 566 | `Spec '{d}' expects success for a phrase that returns a value.` |
| PH001 | Resolver 729 | `No phrase matches this step.` followed by ` Closest: ...` |
| PH208 | Parser 341 and 353 | `A phrase is implemented by native blocks or by steps, not both.` (one message, two sites) |
| PH200 | Parser 291 | `Expected a quoted phrase template, found X.` |
| PH200 | Parser 358 | `Expected a phrase clause (effects, fails with, spec, native or steps), found X.` |
| PH200 | Parser 699 | `Expected a phrase call, found X.` |
| PH200 | Parser 873 | `Expected called (phrase spec) or handled (endpoint spec), found X.` |

- **PH codes:** 14 distinct (PH001, PH010, PH200, PH203, PH208, PH320, PH321, PH325, PH327, PH328, PH330, PH331, PH333, PH334).
- **Not messages:** the `"phrase"` arm and the `phrase-steps` key in `ExtensionKeywords.cs` (6 literals) are registry keys pinned by `scripts/check-extension-map.py` and the extension-map ADR; the `phrase.` uses in `CSharpProvider.cs` sit inside interpolation holes or comments.
- **Trigger coverage by code:** Resolver tests cover PH010, PH325, PH327, PH328, PH320, PH321 (plus the corpus fixture for PH320), PH330, PH331, PH333, PH334, PH001 (4 files); Parser tests cover PH203, PH208 and PH200 (5 Parser test files). Unprobed: whether each of the four PH200 shapes and the PH331 input message is hit individually; the reds must include their own triggers.
- **Tests pinning message text:** `.Message` appears on 72 assertion lines in 22 files; zero assert the unit word (they assert fragments after it: "Unknown field", "customerId, items", "to, that, it"). A regex over Contains, Equal, Matches and StartsWith for `[Pp]hrase` in `tests/` returned hits only on generated code, bench and CLI text, not on compiler diagnostics. The corpus `*.expected.json` files pin only code and line (PH203, PH212, PH213, PH320: 4 of 14 fixtures). `PhraseBuildOneFile` asserts exit 1 and the prefix `PH001 (`; the `AvowryCommand` test asserts the CLI never says "phrase:". `reference/` has 0 diagnostic hits.
- **Docs:** the 19 patterns hit only decision-log history (4 lines in 3 files), board titles and `.mob/` history (stay verbatim). The frozen D72 files quote none of the old messages; GRAMMAR's table already says "avow". So no doc changes in lock step.
- **Benchmark and LLM harness:** 0 hits in `bench/`, `scripts/`, `tools/` and `docs/usable-v1`; history rows hold no message text. `tools/Avowry.Bench/Program.cs` lines 310 and 851 append diagnostic text to retry prompts through `AddDiagnostics` after `promptHash` is computed at line 277; `promptHash` and `docsHash` contain no diagnostics, so no prompt hash changes. Bar 6 evidence pins only `grammar_sha256` and `cheatsheet_sha256` (UsableV1.cs 117-135); those docs are untouched; bar 6 is suspended in `board/usable-v1-ratchet.json`. Nothing parses diagnostic text by regex.
- **Two things that would change:** future benchmark retry prompts carry the new wording (a small comparability note for attempts counts, not a hash change); a scan of `src/` must skip untracked `obj/` and `bin/` (stale `Phrase.*.AssemblyInfo.cs`).
- **Bytes:** Resolver.cs (42162 bytes) and Parser.cs (39282 bytes) are LF only and ASCII only.

## Decisions the card needs
1. **Wording:** substitute the word and fix the article ("an" before "avow"): PH010 `Avow '{t}' has the same wording as ...`; PH321 `Avow '{t}' returns ...`; PH320 `Avow '{t}' returns no value ...`; PH327 `Avow '{t}' participates in a call cycle.`; PH331 `Avow specs cannot bind input ...` and `Avow spec for '{t}' does not bind hole ...`; PH334 `Avow '{t}' cannot fail with ...`; PH203 `Avow has no implementation; ...`; PH325 `Composite avow '{t}' cannot return a value.`; PH328 `Composite avow '{t}' must declare effect ...`; PH330 `... on avow '{t}' must use 'when called'.`; PH333 `... for an avow that returns a value.`; PH001 `No avow matches this step.`; PH208 `An avow is implemented by native blocks or by steps, not both.`; PH200 `Expected a quoted avow template`, `Expected an avow clause (...)`, `Expected an avow call`, `(avow spec)`.
2. **Lowercase uses inside message text change too (all 18).** Leaving the ten would keep half the vocabulary wrong for Ari, who reads all of them.
3. **PH codes stay** (D205). Variable names and comments stay (card 325).
4. **Criterion 1 mechanism: run the compiler on trigger sources.** "Reachable" means a source exists that makes today's compiler emit the diagnostic. There is no diagnostics table to reflect over, and building one is a refactor with no consumer. The test asserts the exact new text for the eight leading messages.
5. **Criterion 3 mechanism: a static scan** of tracked `src/**/*.cs` string literals with interpolation holes removed; fails on `(?i)\bphrases?\b`; the named exception is the `ExtensionKeywords.cs` registry keys; excludes `obj/` and `bin/`; it reads the working tree on purpose (D206: a live-state guard).
6. **Why criteria 1 and 3 split this way:** one reword would satisfy both tests at once, so the second would be born green. They split by messages: criterion 1 covers the eight messages that start "Phrase"; criterion 3 covers the other ten plus the recurrence guard. Each criterion has its own production change and its own watched red.

## Split
No split: one concern (the unit word in compiler messages), three increments, 19 changed lines in 2 files, the card text about 5 KB; splitting would leave a half-renamed vocabulary. The slice should keep the new tests compact (the 400-line cap, not the 15 KB one, is the one to watch).

## Card text (replaces the issue body; the facilitator removes the title heading and writes Status: Ready once adopted)
```card
<!-- phrase-card:324 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** story
- **Repository:** avowry (src/Avowry.Compiler: Resolver.cs and Parser.cs)
- **Source:** the docs card (266) Ready, Decider D205
- **Depends on:** none
- **Story:** As Ari, a cold-start coding agent writing avows, I want every compiler diagnostic that names the unit to say "avow", so that the message matches the language and the docs I am working from and I do not search for a word that does not exist.
- **Observes:** all 18 distinct messages (19 sites) that named the unit say avow. PH codes, positions, text after the unit word and CLI exit codes are unchanged. A message that names the old unit is refused by a check.
- **Scope in:** the 19 message sites in Resolver.cs (12) and Parser.cs (7), a characterisation test, an enumeration test, a src scan.
- **Not in this card:** the PH### prefix (owner ruling later), variable names and comments (card 325), docs (no doc quotes an old message; GRAMMAR.md and CHEATSHEET.md are frozen D72 and untouched), bench prompts and hashes (no change), ExtensionKeywords registry keys.
- **Risk to shipping:** low to medium. String literals only, LF and ASCII files. It changes the retry text the benchmark feeds a model (comparability note, no hash change; bar 6 is suspended). The scan must skip untracked obj/ and bin/. The new tests may near the 400-line cap, so reuse the existing source helpers.
- **Criterion 1 (NEW-STATE):** the eight messages that began "Phrase" say "Avow" end to end. The test is the Theory `The_eight_leading_messages_say_Avow` in Avowry.Compiler.Tests (8 trigger sources, exact new text). Watched failing on the pre-change tree: the actual text starts "Phrase '...'" and the assertion fails.
- **Criterion 2 (CHARACTERISATION):** every one of the 18 messages keeps its PH code, line, column and text after the unit word (the unit word and its a/an normalised), and the CLI exits 1 on a diagnostic and 0 on a clean build. The test is `Diagnostic_codes_positions_and_tails_are_unchanged` plus a CLI row. It is committed first and passes on the pre-change tree (run and quoted). It protects criteria 1 and 3.
- **Criterion 3 (NEW-STATE):** a scan of tracked src string literals fails on the old unit word or product name outside named exceptions. The test is `No_message_in_src_names_the_old_unit` in Avowry.Acceptance.Tests, with two scanner tests on a planted temp tree (a planted old name fails naming file and line; the named exception passes), each watched failing against a stub scanner first. The live-tree test is watched failing after criterion 1's green, with exactly the 10 remaining sites listed.
- **Build order:** 2, then 1, then 3. Done checks, one per cycle: (a) characterisation green on the old tree, (b) the eight say Avow, (c) the scan is green with all ten fixed.

Scenario: [Z] a clean module prints no diagnostic
  Given a module that compiles with no errors
  When Ari runs avowry build on it
  Then it exits 0 and prints no PH### line, as before

Scenario: [O] one message says avow
  Given two avows with the same wording
  When the compiler runs
  Then it reports PH010 as "Avow '...' has the same wording as '...'." at the same line and column

Scenario: [M] every message that named the unit says avow
  Given the 8 leading messages and the 10 others (PH325, PH328, PH330, PH333, PH001, PH208 and four PH200 shapes)
  When each is triggered
  Then none contains "phrase" in any case and each reads with "avow" in its place

Scenario: [B] codes, positions and tails do not move
  Given the 18 trigger sources recorded on the pre-change tree
  When the compiler runs after the change
  Then every code, line, column and text after the unit word is identical, including the "Closest:" suffix on PH001 and the leaf versus composite wordings

Scenario: [I] the CLI shows the new wording
  Given a source with an unmatched step
  When Ari runs avowry build
  Then it exits 1 and prints "PH001 (line:col) No avow matches this step."

Scenario: [E] a new message with the old name is refused
  Given a message literal containing "Phrase" or "phrase" planted in src
  When the scan runs
  Then it fails naming the file and line, and the registry keys in ExtensionKeywords.cs pass as named exceptions

Scenario: [S] the simplest message changes by one word
  Given an avow with no implementation
  When the compiler runs
  Then it reports PH203 as "Avow has no implementation; add `native <target>` or `steps`."
```

## Task force proposal
- **Seats:** product manager (done); architect (slice and the scanner design); QA engineer (ZOMBIES, the 18 trigger sources, mutation checks); compiler engineers by rotation; an independent reviewer (a non-author and a different model per cycle); an LLM-ergonomics reviewer (a fresh model reads only the 18 new messages and GRAMMAR's table as Ari and reports any wording that misleads: read-only, runs nothing); agile coach and audit lens at the retro. Left out: security, DevOps, technical writer (no docs edited), cold-reader (not a docs card). Domain expert: no (D126 fixed the word; the ergonomics seat covers the wording).
- **Build seats:** every driver is at Rung 1, so cycles are small and bounded; the facilitator probes Codex, Copilot and Grok fresh, re-runs every claim and recomputes bytes. Cycle 1 (criterion 2, test only): Copilot (due in rotation; a new test file carries no byte risk; it misreported bytes on card 196 and is coached). Cycle 2 (criterion 1, red then green, the first edit of 8 lines): Grok (next in rotation; its card 197 green was byte-exact on 33 of 33 files). Cycle 3 (criterion 3, the scanner and the remaining 10 lines): Codex (its card 266 greens were clean and it has the guard-scanner precedent; its ledger row is not caught up for cards 299, 303 and 313, so Rung 1 oversight applies in full). Fallback is the next driver in rotation if a probe fails; agy is out of quota until about 2026-11-14.
- **Red plan:** criterion 2 passes on the old tree; criterion 1: the Theory fails by assertion with the "Phrase" text, and the facilitator reverts only the fix to prove the red; criterion 3: the planted-tree tests fail against a stub and the live scan fails listing exactly 10 sites after criterion 1's green. One criterion test is pending at a time (D137); each carries Given/When/Then comments and a Criterion trait; any born-green test or production change before its red is a retro finding. Tests that read committed blobs follow D206; the scan reads the working tree on purpose.
- **Production change:** hand edits of the 19 lines in Resolver.cs and Parser.cs; byte check: `git diff -U0` shows exactly 19 changed lines and each reverses by word substitution to the old bytes; if the wordings are used exactly as listed Resolver.cs should be 42139 bytes and Parser.cs 39270 bytes (unprobed arithmetic: the facilitator recomputes from the old blobs).
- **Gate:** `dotnet test` baseline Compiler 284 and Acceptance 313 passed with the 3 known failures; after the card the Compiler count rises by the new tests and the failures stay the same 3; gate and sign-off runs are isolated and each report carries a Load line.
- **Reversal:** `git revert` per commit, newest first, because reverting criterion 1's green turns its red test back on; no data migration.
- **CI:** push to main; `ci.yml` runs build Release, `dotnet test --filter "Status!=Pending"`, the usable-v1 report and the mob-process check.

## Risks
- Tests asserting messages: the 72 `.Message` assertions check only text after the unit word, so a changed word breaks none; the risk is a substitution that shifts "a" to "an" or adds a space, which the characterisation test catches.
- Golden and expected files: the corpus `expected.json` files pin code and line only; `reference/` holds no diagnostics.
- Frozen docs and hashes: GRAMMAR.md, CHEATSHEET.md and orders.avowry are untouched; bar 6 hashes cover those docs only; the ratchet has bar 6 suspended.
- Benchmark: prompt hashes and history rows unchanged; future retry prompts carry the new wording; the retro logs the date as a small confound for attempts comparisons.
- The scan: it can false-positive on obj/ and bin/ leftovers, interpolation holes or the registry keys, and false-negative on literals it mis-parses; the planted-tree tests and named exceptions cover it.
- Misreported edits: hand editing 19 lines risks a damaged byte; the facilitator's diff and byte recompute are required.
- Unprobed: per-message triggers for the four PH200 shapes and the PH331 input message; whether a `Phrase.*` test folder is tracked; no running benchmark was checked.

## Facilitator's rerun and corrections
Re-run at the boundary (HEAD d7c92f4): a string-literal scan of tracked `src/**/*.cs` (interpolation holes stripped) finds 12 sites in Resolver.cs (one more regex hit at line 556 is an artefact of an interpolation hole), 7 in Parser.cs (19 sites; PH208 is one message at two sites: 18 messages) and 6 registry-key literals in ExtensionKeywords.cs: matches the seat. **Corrections for the slice:** (1) the card text block started with a title heading and `Status: Ready (after Decider)`; the facilitator removes the heading, keeps `<!-- phrase-card:324 -->` first and writes Status: Ready once adopted. (2) The seat's cycle 1 "Copilot, test only" is wrong in kind: tests are the QA seat's work in every cycle; a build seat writes only greens, so the build seats are Grok (criterion 1's green) and Codex (criterion 3's green), and criterion 2 needs no green; the rotation is decided by a fresh probe at each green. (3) The D206 line applies in effect: every red here is a C# test that CI runs directly, so a red pushed alone turns CI red; each cycle's red is pushed together with its green (one push per cycle, after the green), not only "where a Python test wraps it". (4) The a/an normalisation in criterion 2 must be defined exactly in the slice (a regex over the old and new text), and the 18 trigger sources are recorded on the pre-change tree and committed as the characterisation baseline. (5) The byte arithmetic (42139 and 39270) is unprobed. (6) The LLM-ergonomics reviewer is a read-only seat at the gate like the cold-reader of card 266 and commits its report under the card's records with the messages it read (D206).
