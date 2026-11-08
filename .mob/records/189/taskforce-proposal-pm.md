# Card 189 proposal (product manager seat, ready; read-only; a Grok seat)

The seat's reply (a fresh context, read-only on the repository), committed verbatim, with the facilitator's reruns and corrections and the declaration voted at the end. The proposal the Decider votes is the card text in section 2 (as corrected below), the scope and slice in section 4 (as corrected below), and the facilitator's section "THE DECLARATION VOTED", which replaces section 5. The standing-rules preflight is section 0 of the seat's reply and is checked again in the facilitator's section.

# Card 189 Ready (product manager, read-only)

## 0. STANDING-RULES PREFLIGHT

- **D137.** One Pending criterion test: `Unsupported_csharp_constructs_report_a_diagnostic` (C1+C2). Green removes only `[Trait("Status", "Pending")]`. Test body unchanged.
- **D154 / D157.** Red record stores pre-change commit, command, failing test, expected failure. Gate replays that red on the commit before the first production change and lists Criterion order from `git log`.
- **D204.** C3 is CHARACTERISATION (PH325 already a diagnostic; orders output hash; existing corpus). Committed first, passes on the pre-change tree. C1+C2 are NEW-STATE. One Pending red, then one atomic green `Criterion: 1-2`.
- **D206.** Code card: cold-reader seat does not apply. Story-promise check is section 3. No new history-reading guard. Live orders-hash test already reads compiled output, not `git show`.
- **D217.** This proposal does not change the working agreement. If the primary window has no No and no amendment, trigger (d): no separate re-run. An amendment uses the confirmation step.
- **D219.** Docs-card gate-on-final-tip: does not apply.
- **Process-start callers.** Pending test uses `PhraseCli.RunAsync` (no `Process.Start(` in the new file). `ProcessStartCallers` unchanged. If the slice instead copies `ProcessStartInfo("python")` / `Process.Start(`, name the file and add it to `GeneratedApiSourceScan.cs:22-29` in the **red**.
- **Card format.** Connextra with Dana; Gherkin titled with ZOMBIES letters; three criteria, each NEW-STATE or CHARACTERISATION, each with a named test and pre-change failure reason.
- **Value rule.** Dana sees a coded diagnostic for a construct the C# target cannot do, instead of a crash.
- **350-line / no parser.** Does not apply (C# tests, no new python).
- **Diagnostics / CODING_GUIDELINES.** Compiler never throws on user input; codes stay `PHnnn` (not AV); one code per distinct problem; message names the construct, says the C# target does not support it yet, and carries a source location. New row in `docs/GRAMMAR.md`. Generated C# for supported sources stays byte-identical (checklist item 10).

## 1. PREMISE FACTS

HEAD `29680ec` (`Avowry.sln`; command `avowry`; keyword `avow`; files `.avowry`). Card text still says phrase/`phrase build`/`.phrase`/`Phrase.sln`/`RequireSupportedPhrase`. **Stale names:** `RequireSupportedPhrase` is `RequireSupportedAvow` (`CSharpProvider.cs:59`). Composite unit is an avow. Plain call step is `BoundCallStep` (`Model.cs:71`; resolver `Resolver.cs:181-188`). PH320 still exists (`Resolver.cs:200`; GRAMMAR.md:179). Corpus fixture `composite-let-of-valueless-phrase` already expects PH320 line 16.

**CLI catch path (`Program.cs:40-51`):** only `CompilationFailedException` → stderr `avowry: Compilation failed:` plus `Diagnostic.ToString()` (`PH### (line:col) message`, `Diagnostic.cs:6`), exit 1. `NotImplementedException` from the provider is uncaught. `NotSupportedException`: none in `src/`.

**Shape 1 (plain call of a composite) still reaches the emitter.** Resolver allows a statement-call of a valueless avow (`Resolver.cs:181-188`; PH321 only if it returns). `RequireSupportedComposite` (`CSharpProvider.cs:83-88`) accepts only `BoundEnsureStep`, `BoundLetUpdateStep`, `BoundLetStep`; a `BoundCallStep` throws `The C# emitter does not support step '{step.Source}' yet.` Did not run the CLI; inferred from this catch path. Facilitator must run the command in the table.

**Shape 2 (composite with a return type) is already a diagnostic.** Resolver reports PH325 (`Resolver.cs:118-119`; test `ResolverFlowTests.A_composite_with_a_return_type_is_PH325`). `StopOnDiagnostics` (`AvowryCompiler.cs:40-43`) runs before `Emit`. The throw at `CSharpProvider.cs:80` is unreachable via `avowry build`. Original card claim is **stale**.

**Orders hash `409bf3c6` is stale.** Live pin is `WebExtensionBoundary.cs:19` `OrdersOutputHash = "19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765"`.

**PH020** (missing native for target) is documented (`GRAMMAR.md:248`) and **not implemented**. A leaf with only `native python`/`typescript` (legal targets, `Parser.cs:16`) throws at `CSharpProvider.cs:71`. PH203 is “no native and no steps”, a different case.

**How a code is added:** emit a `Diagnostic(code, message, line, column)`; document it in `docs/GRAMMAR.md` (the one table, card 053); cover it with a test. `ExtensionKeywords` registers only core/web-split codes; a whole-code core diagnostic is not entered there. Codes in force: lexer PH100–109, parser PH200–213, resolver PH001/002/010 and PH300–335 (PH329 is reported in `Resolver.cs:70` and is **absent** from the GRAMMAR table). No AV codes. `CODING_GUIDELINES.md:19` keeps `PHnnn`.

**Corpus of 174:** `tests/Avowry.Acceptance.Tests/Fixtures/corpus/` (14 `.avowry` + `.expected.json`); theory `EdgeCaseFixtureCorpus.Every_corpus_fixture_matches_its_expected_outcome` (`PH\d+ (line:col)`). No multi-level plain-call fixture (174 decision text).

**Compiler-core NIE:** none on user source. `PhraseCompilerTests` NIE is a test adapter. `tools/Avowry.Bench` NIE is not `avowry build`.

### Reachable unsupported shapes (static read of HEAD; CLI not run)

| Shape | file:line | How user source reaches it | Today | Diagnostic | In this card? |
|---|---|---|---|---|---|
| Composite step that is a plain call (`BoundCallStep`, e.g. `inner n`) | `CSharpProvider.cs:88` | Valueless callee, statement-call; MultiLevel source below | Uncaught NIE | New PHnnn, line of the step, C# target does not support it yet | **Yes (C1)** |
| Composite `fail with` (`BoundFailStep`) | `:88` | Declared code; parser allows it (`Parser.cs:613-618`) | Uncaught NIE | Same family | **Yes (C2)** |
| Leaf with `native python`/`typescript` only | `:71` | PH207 does not fire | Uncaught NIE | Documented **PH020**, or same family | **Yes (C2)** |
| `effects A, B` | `:62` | Grammar allows a list (`GRAMMAR.md:62`) | Uncaught NIE | Same family | **Yes (C2)** |
| Composite `let` argument that is not a variable (literal, `none`, field) | `:112` | `ResolveCall` binds literals (`Resolver.cs:614-624`) | Uncaught NIE | Same family | **Yes (C2)** |
| Avow spec `given`/`then returns` of `none` (`BoundNone`; `IsSupportedValue` misses it `:144-152`) | `:122/:126` | Endpoint specs skip this check (`GetEndpointsCompile.cs:99` compiles `query: none`) | Uncaught NIE | Same family | **Yes (C2)** |
| Composite `let` of an effectful returning leaf | `:472` | `StepsBody` calls `CallExpression` with `availableEffects: null`; PH328 is satisfied if the effect is declared | Uncaught NIE | Same family | **Yes (C2)** |
| `{Record}Store` seed of a non-Text/Id/Bool field on `then succeeds` | `:741` | `KnownIdSeed` `:644-647` | Uncaught NIE | Same family | **Yes (C2)** |
| Composite with a return type | `:80` | **Does not reach Emit** | **PH325**, exit 1 | Already PH325 | **C3 characterisation** |
| Empty module name `:21`; unsupported hole/return type `:100/:104` after a clean resolve; `StepStatement` default `:442`; unknown condition `:463`; `CSharpArgument` default `:486` | those lines | Parser/resolver diagnostics first, or only after `:88/:112` | Dead or not shown from user source | — | **No** |
| Supporting multi-level composites / return values | — | Language | PH320/PH325 | — | **No** (187 + later) |

**Unverified without dotnet (facilitator):**

```
dotnet build src/Avowry.Cli
dotnet test Avowry.sln --filter "FullyQualifiedName~WebExtensionBoundary.Orders_phrase_loads_web"
# write MultiLevel.avowry as in C1, then:
dotnet exec src/Avowry.Cli/bin/Debug/net8.0/Avowry.Cli.dll build MultiLevel.avowry -o OUT
```

Quote exit code, whether stderr contains `Unhandled exception` / `NotImplementedException`, and whether PH325 via CLI matches `CliKeepsDiagnosticShape`.

## 2. THE CARD TEXT

```markdown
<!-- phrase-card:189 -->

- **Status:** Ready
- **Shape:** Story
- **Type:** bug (code; found by the card 174 corpus; the compiler must never throw on user input)
- **Repository:** avowry (`src/Avowry.Adapters.CSharp/CSharpProvider.cs`, `src/Avowry.Cli/Program.cs`, `docs/GRAMMAR.md`, `tests/Avowry.Acceptance.Tests/UnsupportedCsharpConstructs.cs`, corpus fixtures under `tests/Avowry.Acceptance.Tests/Fixtures/corpus/`)
- **Source:** Card 174 (2026-10-29); `docs/CODING_GUIDELINES.md` (never throw on user input); D220 (head of Ready)
- **Depends on:** none
- **Story:** As Dana, I want a clear message when I use something the C# target cannot do yet, so that I know it is a limitation and not a crash.
- **Observes:** `avowry build` on each in-card unsupported shape exits 1, prints a `PHnnn (line:col)` diagnostic naming the construct and that the C# target does not support it yet, and prints no `Unhandled exception` and no stack trace. Supported sources keep the recorded orders output hash. Composite return type stays PH325.

- **Criterion 1 (NEW-STATE):** Given module MultiLevel (leaf `reject {n: Int}` -> Int with a failure and `native csharp`; composite `inner {n: Int}` whose step is `let checked = reject n`; composite `outer {n: Int}` whose step is the plain call `inner n`; endpoint that calls `outer`), When `avowry build FILE.avowry -o OUT` runs, Then it exits 1 with a diagnostic that has a PH code, the source line of the plain-call step, and a plain statement that the C# target does not support it yet, and prints no stack trace and no `Unhandled exception` text. Named test (shared with C2): `Unsupported_csharp_constructs_report_a_diagnostic`. Pre-change failure: CLI does not catch `NotImplementedException` from `RequireSupportedComposite` (`CSharpProvider.cs:88`; `Program.cs:40-51`).

- **Criterion 2 (NEW-STATE):** Each other in-card row in the Ready table (composite `fail with`; leaf with only `native python` or `typescript`; two effects; composite `let` with a non-variable argument; avow spec `none`; composite `let` of an effectful returning leaf; store seed of Int/Decimal) produces the same kind of diagnostic (PH code, location, “C# target does not support it yet” or the documented PH020 for missing csharp native) instead of an exception. Same named test; one independent check per row, one aggregated assertion. Pre-change failure: each row still throws `NotImplementedException` at the cited line.

- **Criterion 3 (CHARACTERISATION):** PH325 via `avowry build` on a composite with a return type already exits 1 with `PH325` and no `Unhandled exception` (passes on the pre-change tree). Existing 174 corpus fixtures keep their pinned outcomes. Generated C# for `orders.avowry` keeps hash `19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765` (`WebExtensionBoundary.Orders_phrase_loads_web_and_keeps_the_output_recorded_before_the_slice`). No existing test is removed. Named tests: that hash test; `EdgeCaseFixtureCorpus.Every_corpus_fixture_matches_its_expected_outcome`; `A_composite_with_a_return_type_already_reports_PH325`. Pre-change: these pass.

Named test (C1–C2): `Avowry.Acceptance.Tests.UnsupportedCsharpConstructs.Unsupported_csharp_constructs_report_a_diagnostic` with `[Trait("Slice", "189")]`, `[Trait("Criterion", "189-1")]`, `[Trait("Status", "Pending")]`. Style: `PhraseBuildOneFile.Compile_error_exits_one_and_prints_the_diagnostic` (`PhraseCli.RunAsync`). New corpus fixtures for C1/C2 land in the **green** (the 174 theory is not Pending).

Scenario: [Z] a supported program
  Given orders.avowry
  When avowry build runs
  Then exit 0 and the generated file hash is 19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765

Scenario: [O,S] one unsupported plain call
  Given the MultiLevel source of criterion 1
  When avowry build runs
  Then exit 1, a PH code at the line of `inner n`, the C# target does not support it yet, and no Unhandled exception text

Scenario: [M] every in-card unsupported shape
  Given each other in-card row
  When avowry build runs
  Then that row’s check in the named test fails today and passes after the green

Scenario: [B] composite return type and missing csharp native
  Given a composite with `-> Int`, or a leaf with only `native python`
  When avowry build runs
  Then the first is PH325 (already); the second is PH020 or the new unsupported-target code, not a crash

Scenario: [I] CLI diagnostic shape
  Given an in-card unsupported file
  When Dana runs avowry build
  Then stderr is the Compilation failed shape (`CliKeepsDiagnosticShape`): exit 1, `PHnnn (line:col)`, no stack trace

Scenario: [E] the exception path
  Given a construct the C# emitter currently throws on
  When avowry build runs
  Then Dana sees a diagnostic, not an unhandled exception

ZOMBIES: all letters apply.

- **Not in this card:** implementing multi-level composites or composite return values (language; known-limitations page card 187); dead throw sites (`:21`, `:80` via build, `:100/:104` after a clean resolve, `:442`, `:463`, `:486`); PH329 table drift; `test_rename_197` 317-count drift (already red on 178/179); supporting two effects or effectful composite lets as working features; AV codes.

## Done when

```
dotnet test Avowry.sln --filter "Status!=Pending"
```
```

## 3. THE STORY-PROMISE CHECK

Promise: Dana sees a limitation message, not a crash, when the C# target cannot do the construct.

| Dana must find | Cover |
|---|---|
| Plain-call composite (`inner n`) is a diagnostic, not a crash | C1 [O][E] |
| Other reachable emitter NIEs are the same kind of diagnostic | C2 [M][B] |
| Exit 1, PH code, location, no stack trace | C1+C2 [I] |
| Composite `->` type is already PH325 | C3 [B] |
| Supported programs still build; orders hash unchanged | C3 [Z] |
| Multi-level composition actually works | Not-in-this-card (187 / later) |

## 4. SCOPE AND SLICE

SHIP AS IS: one concern (uncaught NIE → diagnostic), three criteria, three increments. Architect may split C2’s last two rows (`:472`, `:741`) if the green would exceed 400 countable lines; that split is a follow-up bug, not this Ready.

1. Characterisation (C3) first, on the pre-change tree: PH325 CLI; existing corpus; orders hash.
2. One Pending red for C1+C2, watched failing because MultiLevel (and the other rows) still throw; red record complete.
3. One atomic green `Criterion: 1-2`: convert the in-card throws to diagnostics (CLI still prints them via `CompilationFailedException`); GRAMMAR row(s); corpus fixtures; remove only Pending. Do not change emission for supported sources.

Caps: C3 zero green cycles; C1+C2 one atomic green. Hostile inputs: MultiLevel; `native python` only; two effects; composite `fail with`; `let x = inner 5`; avow `given v = none`; composite let of an effectful leaf; store entity with an Int field and `then succeeds`.

## 5. TASK-FORCE DECLARATION

- **Ready (PM):** Grok (this proposal). Domain: Dana’s no-crash promise and the reachable-shape table.
- **Slice (architect):** fresh Claude, read-only. Reason: PM is Grok; Codex out until about 2026-11-11; Copilot no quota; agy rate-limited. Domain: which throw becomes which PH code (PH020 vs one new adapter code), location on the step, and whether `:472`/`:741` stay in the green.
- **Red (QA):** Grok, fresh. Characterisation first, then the one Pending red; neighbouring tests `CliKeepsDiagnosticShape`, `EdgeCaseFixtureCorpus`, `WebExtensionBoundary`, `PhraseBuildOneFile.Compile_error_exits_one_and_prints_the_diagnostic` before commit.
- **Green:** fresh Claude; never the red’s seat.
- **Independent gate reviewer:** fresh Claude. Replay the Pending red on the commit before the first production change; Criterion order from `git log`.
- **Fresh sign-off:** a separate fresh Claude context that does not read the gate report before its own reruns.

Left out: cold-reader (not a docs card); tech-writer; security (no new trust boundary); extra backend; delivery lead. Domain-expert: none beyond the architect (emitter vs resolver). Muse/Cursor: trial drivers for low-stakes seats only, not gate or green.

**Gate, in order:** `dotnet build src/Avowry.Cli`; full suite in a `git clone --no-hardlinks --single-branch -b main` of main (not an archive); replay the Pending red; `git log` Criterion order; `dotnet test Avowry.sln --filter "Status!=Pending"`. Report: Gate evidence line (hash, CLI build, how the copy was made, totals) and a Load line. Known unrelated red: `tests.test_rename_197.ListsAndPaths.test_lists_equal_baseline` (317 vs a larger live list; 179 already recorded this) — do not HOLD on it.

## 6. RISKS AND WHAT YOU COULD NOT VERIFY

- Did not run dotnet. Facilitator: the three commands in section 1. Exact host banner and Windows exit code for uncaught NIE are unrun.
- One vs several PH codes: guidelines say one code per distinct problem; the card’s wording is one *kind* of diagnostic. Architect assigns PH020 vs a new code after PH335.
- `:472` and `:741` sit outside `RequireSupported*`. Leaving them out would leave Dana a crash. If the green is too large, split them; do not silently drop them.
- New corpus fixtures in the red would fail the non-Pending 174 theory and turn the red commit’s `Status!=Pending` suite red. Add those fixtures in the green.
- Card should **not** be split at Ready: one user-visible concern. Optional later split is only the two emit-time rows if size demands it.

---

# Facilitator's reruns, corrections and the declaration

## Reruns (facts checked by the facilitator on HEAD 29680ec)

- The repro of criterion 1 was run: `MultiLevel.avowry` (the module of the card text in the Avowry syntax: the leaf `reject`, the composite `inner` with `let checked = reject n`, the composite `outer` whose only step is the plain call `inner n`, and the endpoint) built with `dotnet exec src/Avowry.Cli/bin/Debug/net8.0/Avowry.Cli.dll build MultiLevel.avowry -o out` after `dotnet build src/Avowry.Cli` (0 warnings, 0 errors). Observed: the process prints `Unhandled exception. System.NotImplementedException: The C# emitter does not support step 'inner n' yet.` with a stack trace through `CSharpProvider.RequireSupportedComposite` (CSharpProvider.cs:88), `RequireSupportedAvow` (:66), `Emit` (:24), `AvowryCompiler.Compile` (AvowryCompiler.cs:37) and `Program.cs:42`; the exit code is not 1 (the host reported 127 under bash on Windows). So the card's first claim is true on HEAD today. The repro file is kept at .mob/records/189/MultiLevel.avowry.
- `OrdersOutputHash` is at `tests/Avowry.Acceptance.Tests/WebExtensionBoundary.cs:18` (not 19) and equals `19fe6d711cc6d8289489da037c78bc2b308e1b4a9118c8812574771ef0e61765`, as the draft says.
- CSharpProvider.cs holds 15 `throw new NotImplementedException` sites; the composite return-type throw at :80 is unreachable from `avowry build` because the resolver reports PH325 first (the draft's shape 2 is already a diagnostic, criterion 3's characterisation).
- Drivers re-probed 2026-11-08 12:5x: Grok, Muse, Cursor OK; Codex usage limit (back about 2026-11-11 12:10), Copilot no quota, agy rate-limited (about 130 hours).

## Corrections to the seat's draft (binding on the card text)

1. **Every in-card row must be shown to crash before it is kept.** The draft's rows 2 to 8 are a static read of HEAD; only the plain-call row was run. The QA seat runs each row through the CLI on the pre-change tree and the red records the observed crash per row; a row that does not crash today is dropped from criterion 2 (not invented, and noted in the red record). A reachable row the slice defers because of size is filed the same day as a bug issue (with its repro) and named in the card's Not-in-this-card list; it is never silently dropped.
2. **Exit code.** The card says the build must exit 1 with a diagnostic and no `Unhandled exception`; the pre-change exit code is not 1. The named test asserts exit code 1 exactly, plus the `PHnnn (line:col)` shape (CliKeepsDiagnosticShape), plus no `Unhandled exception` / `NotImplementedException` / stack-trace text on stderr or stdout.
3. **Code assignment.** The architect's slice decides the PH codes: one new adapter code in the range after PH335 for the "C# target does not support it yet" family (message names the construct and the line), or several only if two shapes are distinct problems; PH020 for a leaf with no csharp native is used only if the slice shows PH020 is what GRAMMAR.md:248 documents for that case; any new code is documented in the one table in docs/GRAMMAR.md (this card does not touch the frozen documents D72: GRAMMAR.md is frozen under D72, the slice must check whether adding a row to the GRAMMAR table is a re-freeze, and if so the code is documented in docs/PATTERNS.md or a new diagnostics section outside the frozen documents and the freeze test stays green).
4. **No emitter behaviour change for supported sources.** The characterisation (criterion 3) includes the orders hash, the 174 corpus theory and the PH325 CLI case, all passing on the pre-change tree; the new corpus fixtures for criteria 1 and 2 land in the green (they would turn the non-Pending corpus theory red in the red commit otherwise), declared in the red record.
5. **Size.** The slice caps the green (countable lines); if the eight rows exceed it, the architect cuts the last rows (composite let of an effectful leaf, store seed of Int/Decimal) into a follow-up bug as in correction 1.
6. **Hash in the card text.** The card text says `409bf3c6`; the live pin is the full 64-character hash above; the card text uses the live one.

## THE DECLARATION VOTED

The task force of section 5 of the draft with these changes: slice (architect) Claude, read-only, decides corrections 3 and 5; red (QA) Grok, a worktree (characterisation first, then the one Pending red whose per-row checks come from the observed CLI runs); green a fresh Claude context that is never the red's seat; no cold-reader seat (not a docs card); gate reviewer and sign-off two separate fresh Claude contexts (no non-author non-trial driver is in quota); the baseline gate by the facilitator, isolated, in a clone, the CLI built first. Decider seats: six fresh seats at once through decider.py, Muse in two of them. Per D217 this proposal does not change the agreement, so if the primary window passes with no No and no amendment no separate re-run is run (trigger line (d)); an amendment goes through the confirmation step.
