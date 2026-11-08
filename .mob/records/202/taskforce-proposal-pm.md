# Card 202 proposal (product manager seat, ready; read-only; a Grok seat)

The seat's reply (a fresh context, read-only on the repository), committed verbatim, with the facilitator's reruns and corrections and the declaration voted at the end. The proposal the Decider votes is the card text in section 2 (as corrected below), the scope and slice in section 4 (as corrected below), and the facilitator's section "THE DECLARATION VOTED", which replaces section 5. The standing-rules preflight is section 0 of the seat's reply and is checked again in the facilitator's section.

# Card 202 Ready (product manager, read-only)

HEAD `841362d`. CLI repro ran. Nothing in the repo was changed.

## 0. STANDING-RULES PREFLIGHT

- **D137.** One Pending criterion test: `Undeclared_native_fail_reports_a_diagnostic` (C1, both MULTI ids). Green removes only `[Trait("Status", "Pending")]`. Test body unchanged.
- **D154 / D157.** Red record stores pre-change commit, command, failing test, expected assertion. Gate replays that red on the commit before the first production change and lists Criterion order from `git log`.
- **D204.** C2 and C3 are CHARACTERISATION (declared `Fail` still builds; orders hash; corpus theory). Committed first, pass on the pre-change tree. C1 is NEW-STATE. One Pending red, then one atomic green `Criterion: 1`.
- **D206.** Code card: cold-reader seat does not apply. Story-promise check is section 3. No new history-reading guard.
- **D217.** This proposal does not change the working agreement. If the primary window has no No and no amendment, trigger (d). An amendment uses the confirmation step.
- **D222.** C1 / the Ready table names two constructs. Marked **MULTI** with ids `omitted-from-clause` and `no-clause`. Slice and red carry one id each. Red record lists this MULTI row.
- **Process-start callers.** Pending test uses `PhraseCli.RunAsync` (no `Process.Start(` in the new file). `ProcessStartCallers` unchanged (`GeneratedApiSourceScan.cs:22-29`).
- **Card format.** Connextra with Morgan; Gherkin titled with ZOMBIES letters; three criteria, each NEW-STATE or CHARACTERISATION, each with a named test and pre-change failure reason.
- **Value rule.** Morgan sees an undeclared native failure at build, with a declaration and a source-map record on the codes that remain.
- **Spike rule.** Does not apply (not a spike).
- **Diagnostics / CODING_GUIDELINES.** Compiler never throws on user input; codes stay `PHnnn`; one code per distinct problem (new resolver code, not PH326 and not PH340). Generated C# for valid sources stays byte-identical (checklist item 10).

## 1. PREMISE FACTS

**Still true.** `docs/GRAMMAR.md:247`: an avow may only fail with codes in its `fails with` clause, plus codes from avows it calls. Native bodies are not inspected: `src/` has no `Fail(` scan. Parser stores `NativeBlock(target, code, At)` (`Ast.cs:53`; `Parser.cs:344`). Resolver copies `n.Code` only (`Resolver.cs:96`) and checks PH326 only on composite `fail with` and `ensure` (`Resolver.cs:214-222`). C# emitter comment: native blocks “passed through unchanged” (`CSharpProvider.cs:407`); `IndentNative` (`:999-1011`) writes the body at `:448`. `Fail(` in generated code is the helper at `:430-434`, emitted only when some avow has `Failures.Count > 0`. Source map writes failure records only from declared endpoint failures (`Program.cs:86-96`). `docs/TRACEABILITY.md:30`: a failure raised only by native code and never declared has no record.

**Stale names.** Card and `board/202-*.md` still say Phrase/`Phrase.sln`/`phrase`. Product is Avowry (`avowry`, `avow`, `.avowry`, `Avowry.sln`). Done-when must be `dotnet test Avowry.sln --filter "Status!=Pending"`.

**Corpus today.** `tests/Avowry.Acceptance.Tests/Fixtures/corpus/undeclared-native-failure.avowry`: leaf `reject natively`, no `fails with`, `Fail("NativeOnly")` on line 5. Header names card 202. `.expected.json`: `{"outcome":"builds",...}` one avow record line 3, `runSpecs: false`. Theory: `EdgeCaseFixtureCorpus.Every_corpus_fixture_matches_its_expected_outcome`. `test_rename_196.py:240` already expects 23 fixtures.

**Repro (allowed CLI only).** Scratch `ready202/repro.avowry`: leaf with `fails with Declared` and `Fail("X")`. Command: `dotnet exec src/Avowry.Cli/bin/Debug/net8.0/Avowry.Cli.dll build FILE -o OUT`. **Observed: exit 0.** Generated `Avows.cs` contains `Fail("X");` plus the helper. Map has only an avow record; no failure record for `X` or `Declared`.

**Scan design (Context).** Recommend a resolver scan of each leaf’s native **csharp** body for `Fail("literal")` (optional whitespace). Computed codes (`Fail(ident)`, interpolation, concatenation), other-target natives, and a full C# parse stay unchecked and are documented as such. Full-line `//` may be skipped; `/* */`, trailing comments, and multi-line `Fail(` may false-positive or miss. Diagnostic names the code and the source line of the match (RAW token line `Lexer.cs:182` plus body offset; `NativeBlock.At` is the `native` header). Raise in the resolver so `StopOnDiagnostics` (`AvowryCompiler.cs:40-43`) runs before emit. CLI already prints `CompilationFailedException` (`Program.cs:47-51`).

**New code.** Recommend **PH337** (PH336 reserved, `language-and-grammar.md:24`; PH337–PH339 free per 189 slice; PH340 is the C# unsupported-construct family). Architect assigns. One table row plus a clause on `GRAMMAR.md:247`.

**How 189 documented.** Green `164b928` added PH020/PH340 rows and the clause at `GRAMMAR.md:162` because D72 hashes were already stale, Usable v1 Bar 6 is Pending (`UsableV1.cs:101-103`), and no gating test pins GRAMMAR.md bytes (`test_run_llm_check.py:129` recomputes live). **Recommend 189’s precedent.** The card’s “list the new code at the card 162 re-freeze” line is stale. **Flag for the Decider.**

**Other natives.** Parser targets are csharp, typescript, python (`Parser.cs:16`). Only the C# adapter exists. `native python` alone is PH020. No python/ts `Fail` idiom in-repo.

**Valid `.avowry` `Fail("…")` hits:** all declared except the corpus fixture.

## 2. THE CARD TEXT

```markdown
<!-- phrase-card:202 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** compiler (code; native Fail vs declared failures)
- **Repository:** avowry (`src/Avowry.Compiler/Semantics/Resolver.cs`, `docs/GRAMMAR.md`, `tests/Avowry.Acceptance.Tests/UndeclaredNativeFail.cs`, `tests/Avowry.Acceptance.Tests/Fixtures/corpus/undeclared-native-failure.expected.json`)
- **Source:** Review of card 175 (2026-10-29); GRAMMAR.md:247; TRACEABILITY.md:30; card 189 documentation precedent
- **Depends on:** none
- **Story:** As Morgan, I want a native block that raises a failure code its avow never declared to be reported at build time, so that a failure cannot reach an endpoint without a declaration and a source-map record.
- **Observes:** `avowry build` on a leaf whose native csharp body calls `Fail("X")` when X is not in that avow’s declared set exits 1 with a `PHnnn (line:col)` diagnostic naming X and the Fail line. A declared `Fail("Declared")` still builds. The corpus theory test is unchanged. Generated C# of valid sources keeps hash `19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765`.

**MULTI** Ready table (D222): C1 names two constructs; slice and red give each its own id; red record lists this row.

| id | construct |
|---|---|
| omitted-from-clause | native csharp `Fail("X")` while `fails with` omits X |
| no-clause | native csharp `Fail("NativeOnly")` with no `fails with` (corpus fixture) |

- **Criterion 1 (NEW-STATE, MULTI):** Each MULTI id, when built with `avowry build FILE.avowry -o OUT`, exits 1 with a PH code (architect: PH337 unless a recorded reason says otherwise), the source line of the `Fail("…")` call, and the undeclared code in the message. Named test: `Undeclared_native_fail_reports_a_diagnostic`. Pre-change failure: both ids exit 0 with no `PHnnn` (repro exit 0; corpus expected `builds`).

- **Criterion 2 (CHARACTERISATION):** A leaf that declares the code it raises still builds; a declared native failure still has a source-map failure record at the `fails with` line. Named tests: `LeafPhraseFailureCompiles.Failing_leaf_phrase_surfaces_its_declared_code_and_its_spec_passes`; `SourceMapTraceability.Failures_inherited_through_a_composite_phrase_still_get_a_failure_record`. Pre-change: these pass.

- **Criterion 3 (CHARACTERISATION):** Existing corpus fixtures keep their pinned outcomes except `undeclared-native-failure.expected.json`, which the **green** of C1 changes from `builds` to that diagnostic (`code` + `line`: 5) in the same commit; `EdgeCaseFixtureCorpus` is not edited. Generated C# for `orders.avowry` keeps hash `19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765`. Named tests: `EdgeCaseFixtureCorpus.Every_corpus_fixture_matches_its_expected_outcome`; `WebExtensionBoundary.Orders_phrase_loads_web_and_keeps_the_output_recorded_before_the_slice`. Pre-change: these pass (fixture still `builds`).

Named test (C1): `Avowry.Acceptance.Tests.UndeclaredNativeFail.Undeclared_native_fail_reports_a_diagnostic` with `[Fact]`, `[Trait("Slice", "202")]`, `[Trait("Criterion", "202-1")]`, `[Trait("Status", "Pending")]`. Style: `UnsupportedCsharpConstructs` (`PhraseCli.RunAsync`; one aggregated assertion; one check per id). No new corpus fixture file (the 174 fixture already exists).

Scenario: [Z] no undeclared Fail
  Given a leaf whose native csharp `Fail` uses only a declared code
  When avowry build runs
  Then exit 0

Scenario: [O,S] one omitted literal
  Given id omitted-from-clause
  When avowry build runs
  Then exit 1, PH code, line of `Fail("X")`, code X in the message

Scenario: [M] two undeclared literals
  Given one native block with `Fail("A")` and `Fail("B")`, neither declared
  When avowry build runs
  Then exit 1 and a diagnostic names at least one undeclared code

Scenario: [B] empty declared set
  Given id no-clause (the corpus fixture)
  When avowry build runs
  Then exit 1 with that diagnostic at line 5; after the green the expected file says so

Scenario: [I] CLI diagnostic shape
  Given an in-card undeclared file
  When Morgan runs avowry build
  Then stderr is the Compilation failed shape (`CliKeepsDiagnosticShape`): exit 1, `PHnnn (line:col)`

Scenario: [E] computed code stays unchecked
  Given native csharp `Fail(code)` where `code` is a variable
  When avowry build runs
  Then it is not this diagnostic (documented)

ZOMBIES: all letters apply.

- **Not in this card:** computed / interpolated / concatenated Fail; python or typescript native idioms; a C# parse of native bodies; source-map records for the Fail call site (declared codes already record at `fails with`); PH329 table drift; CHEATSHEET.md; card 162 itself; `test_rename_197` 317-count drift.

## Done when

```
dotnet test Avowry.sln --filter "Status!=Pending"
```
```

## 3. THE STORY-PROMISE CHECK

Promise: Morgan sees undeclared native Fail at build, so a failure cannot reach an endpoint without a declaration and a source-map record.

| Morgan must find | Cover |
|---|---|
| `Fail("X")` omitted from `fails with` is a diagnostic | C1 `omitted-from-clause` [O][I] |
| `Fail("X")` with no `fails with` is the same diagnostic | C1 `no-clause` [B] |
| Declared `Fail` still builds; map record stays at `fails with` | C2 [Z] |
| Valid programs and the corpus theory stay green | C3 |
| Computed Fail is diagnosed | Not-in-this-card [E] |

## 4. SCOPE AND SLICE

SHIP AS IS: one concern, three criteria, three increments.

1. Characterisation (C2+C3) first, on the pre-change tree: declared Fail; orders hash; corpus theory (fixture still `builds`).
2. One Pending red for C1, both MULTI ids watched failing by assertion (exit 0, no `PHnnn`). Red record lists the MULTI row.
3. One atomic green `Criterion: 1`: resolver scan; GRAMMAR.md row + clause (if the Decider follows 189); `undeclared-native-failure.expected.json` only (data, same green; theory file unchanged); remove only Pending.

**Byte-identical generated C#:** no emit path change (`IndentNative`, helper at `:433`, `AppendAvow`). Check runs in the resolver before `Emit`. Orders hash holds because `orders.avowry` only `Fail`s declared codes.

Caps: C2+C3 zero green cycles; C1 one atomic green. Hostile: CRLF; `Fail` not on the first native line; two literals; mixed declared+undeclared.

## 5. TASK-FORCE DECLARATION

- **Ready (PM):** Grok (this). Domain: Morgan’s reviewability promise and the MULTI ids.
- **Slice (architect):** fresh Claude, read-only. Domain: regex, line, PH337 vs another code, comments, whether Model.cs must carry the native start line.
- **Red (QA):** Grok, fresh. Characterisation first, then the one Pending red.
- **Green:** fresh Claude; never the red’s seat.
- **Independent gate reviewer:** fresh Claude. Replay the Pending red on the commit before the first production change; Criterion order from `git log`; D222 list vs Ready table.
- **Fresh sign-off:** a separate fresh Claude context that does not read the gate report before its own reruns.

Left out: cold-reader; tech-writer; security. Muse/Cursor: trial drivers for low-stakes seats only, not gate or green. Codex out until about 2026-11-11; Copilot no quota; agy rate-limited.

**Gate, in order:** `dotnet build src/Avowry.Cli`; full suite in a `git clone --no-hardlinks --single-branch -b main` of main (not an archive); replay the Pending red; `git log` Criterion order; `dotnet test Avowry.sln --filter "Status!=Pending"`. Report: Gate evidence line (hash, CLI build, how the copy was made, totals) and a Load line. Neighbours: `CliKeepsDiagnosticShape`, `EdgeCaseFixtureCorpus`, `WebExtensionBoundary`, `LeafPhraseFailureCompiles`, `SourceMapTraceability`, `GeneratedApiSourceScan.Guard_checked_set_of_process_start_callers`.

## 6. RISKS AND WHAT YOU COULD NOT VERIFY

- Did not run the full suite or compile the generated C# of the repro. Facilitator: characterisation neighbours; confirm both MULTI ids exit 0 today.
- **GRAMMAR.md / card 162:** recommend 189’s precedent (table row in this green, reason in the green record; card 162 ratifies hashes). The card’s “list it at 162” line does not hold on HEAD. **Decider chooses.** CHEATSHEET.md stays untouched.
- Simple-scan holes (comments, multi-line `Fail(`, other targets) are Not-in-this-card; document them next to the new code.
- Diagnostics `baseline.txt` / trigger sources: unverified whether a new resolver row is required; architect checks.
- `test_rename_197.test_lists_equal_baseline` still asserts 317; whether it is red at HEAD was not run.
- Do **not** split the card: one user-visible concern. Empty vs non-empty declared set is two ids of one slice, not two cards.

---

# Facilitator's reruns, corrections and the declaration

## Reruns (facts checked by the facilitator on HEAD 841362d)

- The corpus fixture `tests/Avowry.Acceptance.Tests/Fixtures/corpus/undeclared-native-failure.avowry` (a leaf `reject natively` with no `fails with` and `Fail("NativeOnly");` on line 5) builds with exit 0 through the CLI at HEAD, and its `.expected.json` says `{"outcome":"builds", ...}` with one avow record (line 3): the card's claim and the draft's repro hold.
- The scan the draft recommends cannot change any other outcome: a script over every tracked `.avowry` file (41 files, 18 `Fail("...")` literals; the script is kept at .mob/records/202/scan202.py) found exactly one literal that is not in its leaf's `fails with` clause, the target fixture. So the demo (Tasks.avowry), the orders fixtures and the card 189 fixtures keep their outcomes.
- The two characterisation tests the draft names exist: `LeafPhraseFailureCompiles.Failing_leaf_phrase_surfaces_its_declared_code_and_its_spec_passes` and `SourceMapTraceability.Failures_inherited_through_a_composite_phrase_still_get_a_failure_record`.
- PH337 to PH339 are unused anywhere under src, docs and tests; PH336 is reserved (docs/decisions/language-and-grammar.md:24); card 189 used PH340 and PH020.
- Drivers re-probed 2026-11-08: Grok, Muse, Cursor OK; Codex usage limit (back about 2026-11-11 12:10), Copilot no quota, agy rate-limited.

## Corrections to the seat's draft (binding on the card text)

1. **The documentation question is settled for this card by the card 189 precedent, and the Decider confirms it by voting this declaration.** The card text's line "frozen docs change only at the card 162 re-freeze; list the new code there" is replaced by: the green adds the new code as a row of the GRAMMAR.md diagnostics table and one clause at GRAMMAR.md:247, with the reason recorded in the green commit body (GRAMMAR.md is frozen under D72, its hash already differs from the recorded one, the Usable v1 bar 6 evidence hashes are already stale, no gating test pins the file's bytes, and card 162 ratifies the hashes that exist when it runs). Card 162's Ready draft is updated at its pull to include this row with the PH020 and PH340 rows.
2. **Where native blocks may appear.** The draft scans a leaf avow's `native csharp` body. The slice checks where else a native block can occur (an endpoint step: see the corpus fixture `native-only-step`) and which declared set applies there, and states it; a native block whose declared set is not defined stays unchecked and is listed in Not-in-this-card, never silently.
3. **Ids (D222).** The MULTI row of criterion 1 carries two ids (`omitted-from-clause`, `no-clause`); the red also carries the hostile inputs (CRLF; `Fail` not on the first native line; two literals, one declared and one not; a leaf that declares X and calls `Fail("X")` twice) as extra ids, each watched failing or, for a case that must keep building (declared; computed code), asserted to build with exit 0 and no new diagnostic; the red record lists the MULTI row and every id with the Ready row it came from.
4. **Corpus fixture.** `undeclared-native-failure.expected.json` is a data file; it changes in the green only (in the red commit the corpus theory must stay green, so the fixture still says `builds` there); the theory file is not edited. The green's fixture change is declared in the red record.
5. **Diagnostic message.** It names the undeclared code, the leaf's template and the line of the `Fail` call, and what to do ("declare it in `fails with` or remove the call"), per the diagnostics rule that an LLM reads the message to fix its code; the architect fixes the wording and the code (PH337 unless a recorded reason says otherwise).
6. **Scan limits are documented, not hidden.** The simple scan of `Fail("literal")` misses computed codes, other-target natives, comments (a commented-out call may be flagged: the slice decides to skip full-line `//` comments and says so) and multi-line calls; the GRAMMAR.md clause says so next to the new code.

## THE DECLARATION VOTED

The task force of section 5 of the draft with these changes: slice (architect) Claude, read-only, who runs the variants of the repro through the CLI before the red (as on card 189) and decides corrections 2, 5 and 6 and the code; red (QA) Grok, a worktree (characterisation first, then the one Pending red whose ids come from the architect's observed CLI runs); green a fresh Claude context that is never the red's seat; no cold-reader seat (not a docs card); gate reviewer and sign-off two separate fresh Claude contexts (no non-author non-trial driver is in quota); the gate reviewer checks the red record's MULTI list against the Ready table (D222); the baseline gate by the facilitator, isolated, in a clone, the CLI built first. Decider seats: six fresh seats at once through decider.py, Muse in two of them. Per D217 this proposal does not change the agreement, so if the primary window passes with no No and no amendment no separate re-run is run (trigger line (d)); an amendment goes through the confirmation step.
