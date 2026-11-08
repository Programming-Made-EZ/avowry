# Card 202 slice (architect seat, a fresh Claude context, read-only)

The seat changed nothing in the repo. Its repros (scratchpad arch202: omitted, noclause, notfirst, oneof, twoundecl, crlf, multiline, declared twice, computed, comments, strings, qualified, pyblock, endnative, dup) were run as `dotnet exec src/Avowry.Cli/bin/Debug/net8.0/Avowry.Cli.dll build FILE -o OUT`; the sources are kept in .mob/records/202/repros/ (the QA seat starts from them). The built CLI predates the card 189 green; that does not affect resolver-level results, but the CLI is rebuilt (`dotnet build src/Avowry.Cli`) before the red replay.

## 1. THE IDS
"Today" is the observed result; "After" is the intended result.

| id | Source | Today | After |
|---|---|---|---|
| `omitted-from-clause` | `fails with Declared`, `Fail("X")` | exit 0 | exit 1, PH337, X |
| `no-clause` | the corpus fixture (no `fails with`, `Fail("NativeOnly")` on line 5) | exit 0 | exit 1, PH337 at line 5 |
| `fail-not-first-line` | blank line after the header, `Fail("Declared")` on line 7, `Fail("X")` on line 9 | exit 0 | exit 1, line 9 |
| `one-of-two-undeclared` | `Fail("Declared")`, `Fail("X")` | exit 0 | exit 1, X |
| `two-undeclared` | A, B, no clause | exit 0 | exit 1, A first (one diagnostic per distinct undeclared code) |
| `crlf` | `omitted` converted to CRLF | exit 0 | exit 1, same line |
| `multi-line-call` | `Fail(` then `"X");` | exit 0 | exit 1, line of `Fail(` |
| `declared-twice` (keep) | `Fail("X")` twice, X declared | exit 0 | exit 0, no PH |
| `computed` (keep) | `Fail(code)` | exit 0 | exit 0 |
| `line-comment` (keep) | `// Fail("X");` | exit 0 | exit 0 |
| `block-comment` (keep) | `/* Fail("X"); */` | exit 0 | exit 0 |
| `trailing-comment` (keep) | `Fail("Y"); // not Fail("X")`, Y declared | exit 0 | exit 0 |
| `raw-string` (keep) | `"""Fail("X")"""` | exit 0 | exit 0 |
| `qualified-call` (keep) | `Other.Fail("X")` and `MyFail("Z")` | exit 0 | exit 0 |
| `other-target` (keep) | csharp declares Y, `native python` has `Fail("X")` | exit 0 | exit 0 |

Findings the PM draft missed:
- Native blocks only occur on a leaf `avow` declaration. An endpoint step `native csharp` is not a native block: it parses as a call and gives `PH001 (10:5)`. "native-only-step" is just a leaf called by an endpoint. The declared set is always the leaf's own `fails with`.
- A composite cannot have a native block: it is `steps` or `native`, and both is PH208 (Parser.cs:341,353). A composite calling a leaf is PH340 in the C# target, so it cannot be a keep-building id.
- The `no-clause` output cannot compile: `Avows.cs` calls `Fail("NativeOnly")` but the helper is emitted only when some avow has declared failures (CSharpProvider.cs:430-434); the build exits 0 with C# that does not compile.
- Duplicate wording (PH010) skips an avow, so the scan must not depend on the skeleton table.
- Inline sources in `.cs` tests (not covered by the facilitator's 41-file scan): 21 `Fail("...")` hits in the acceptance tests, every code declared; ParserPhraseTests.cs:171 is parser-only; Diagnostics/sources.txt has no `Fail(`.

## 2. THE DESIGN
- Where: in `Resolver.ResolveAvows`, in the declaration loop (Resolver.cs:60), on `declaration.Natives` (the AST), before the PH010 `continue`, so `StopOnDiagnostics` (AvowryCompiler.cs:40-43) stops before `Emit`.
- Line: `NativeBlock.At` is the `native` header (Parser.cs:344); the lexer drops leading blank lines from the body but starts the RAW token at the first non-blank line (Lexer.cs:176-182); the AST does not keep that line. Add a fourth parameter `Position BodyAt` at Ast.cs:53, pass `Pos(raw)` at Parser.cs:344 (nothing else constructs `NativeBlock`). Model.cs is unchanged. Reported line = `BodyAt.Line` + the number of `\n` before the match; column = `BodyAt.Column` + the offset in that line.
- Scan rule (csharp native only, one regex pass left to right): `//[^\n]*|/\*[\s\S]*?\*/|(?<![\w.])Fail\(\s*"([^"\\\n]*)"\s*\)|"(?:\\.|[^"\\\n])*"|'(?:\\.|[^'\\\n])*'`; only the Fail alternative captures a code; the comment and string alternatives consume text so it cannot match. Tested in Python (rx.py); the green verifies on .NET with `RegexOptions.CultureInvariant`. Comparison Ordinal as PH326. One diagnostic per distinct undeclared code per block, at its first occurrence, in source order.
- Limits to document next to the code: computed, interpolated and concatenated codes are not checked; verbatim `@"…""…"` and nested-quote interpolated strings are not tokenised; `#if` branches are scanned; other targets are not scanned; `AvowryFailure.Raise(…)` and a differently named helper are not seen; a `Fail(` split by a comment is missed.
- Code: PH337 is unused (only in the decision text and the card; PH336 reserved, language-and-grammar.md:24); not in ExtensionKeywords.cs, so a plain code like PH326.
- Message (must start "Avow", as `LeadingMessagesSayAvowTests` requires): `Avow 'reject {n: Int}' raises Fail("X") in its native csharp block on line 5, but X is not in its 'fails with' (declared: none). Add X to 'fails with' or remove the call.` The template comes from `SourceFormatter.Format(declaration.Template)`, the declared list from `string.Join(", ", declaration.Failures)` or "none".
- Docs, in the green: a `PH337` row in the GRAMMAR.md table after `PH335`; "`PH300` to `PH335`, plus `PH340`" becomes "plus `PH337` and `PH340`"; one clause at :247 carrying the limits; the D72 reason in the green commit body (card 189's precedent). TRACEABILITY.md:30 stays true (computed codes still have no record); do not edit it.

## 3. THE FILES
- Characterisation (increment 1): nothing new is needed. The existing tests pass today: `LeafPhraseFailureCompiles.Failing_leaf...`, `SourceMapTraceability.Failures_inherited...` and `...earlier_step_raises_the_same_name` (CLI, exit 0 plus a failure record at the `fails with` line, :25, :70), the corpus theory, `Orders_phrase_loads_web...`. The keep-building ids inside the red pin the rest. The facilitator records their pre-change pass lines in the red record.
- Red (increment 2): new `tests/Avowry.Acceptance.Tests/UndeclaredNativeFail.cs`; attributes in the order Fact, Slice 202, Criterion 202-1, Status Pending; `PhraseCli.RunAsync` and inline raw strings, no `Process.Start(`; one `failures` list and one aggregated assertion in the style of UnsupportedCsharpConstructs.cs:14-30; plus .mob/records/202/red-record.md.
- Green (increment 3): seven files, six countable (`expected.json` is exempt under D136): Ast.cs, Parser.cs, Resolver.cs, docs/GRAMMAR.md, `Fixtures/corpus/undeclared-native-failure.expected.json` becomes `{"outcome":"diagnostic","code":"PH337","line":5}`, `.../corpus/undeclared-native-failure.avowry` (reword the header on line 1 only: it still says "builds / flips this fixture"; the line count stays the same so `Fail` stays on line 5; it must still start `# undeclared-native-failure:` (EdgeCaseFixtureCorpus.cs:77)), and UndeclaredNativeFail.cs with only the Pending trait line removed.
- Python test: `test_rename_196.py:240` asserts 23 tracked corpus fixtures; a changed file does not change the count and the green adds no fixtures; nothing is deleted; the r100 check only looks at the historical rename commit; the suite is already red from card 189 (5 names).

## 4. THE TEST DESIGN
Failing ids use the card 189 `Check` shape (CLI via `PhraseCli.RunAsync`, output normalised to LF): the exit code must be 1; `(?m)^(PH\d{3}) \((\d+):(\d+)\) ` must match with code `PH337`; the line must equal the line of the `Fail(` call in the source (computed with `LineOf`, so it also proves the blank-line offset and CRLF handling); the message must contain the code X, the template, `fails with` and "line N"; no "Unhandled exception"; for `two-undeclared` the first diagnostic is A. Keep-building ids: exit 0 and no `(?m)^PH\d{3} \(` anywhere in the output. On the pre-change tree the 7 failing ids fail by assertion ("exit 0; no PHnnn") and the 8 keep ids pass, so the aggregate assertion fails; it does not fail on a missing file.
Red record (D222): it lists the MULTI Ready row (criterion 1) with its two ids `omitted-from-clause` and `no-clause`, lists the hostile ids each tagged "slice hostile (not from the Ready row)", and names the green's one data change (the expected.json and the header edit). After the green the CLI is stale until rebuilt (`PhraseCli.EnsureFresh`).

## 5. SIZE
The green is about 45 countable lines: Resolver about 30, Ast 1, Parser 1, GRAMMAR 3, fixture header 1, Pending removal -1. Cap: 90 lines, 8 files. Nothing is deferred. If the green wants more than the regex pass for strings, drop to "skip full-line `//` only" and file a bug the same day.

## 6. ORDER AND GATE
1. Characterisation: the pre-change pass of the named existing tests, recorded. 2. Red: the Pending test plus the red record, `Criterion: 1`. 3. Green: one atomic commit, `Criterion: 1`. Gate replay: build the CLI, run the red on the commit before the green (exactly the 7 failing ids); at the green it passes; `git diff red green` on the test file is the one removed line; `git log` Criterion order; the D222 list against the Ready table; `dotnet test Avowry.sln --filter "Status!=Pending"`. Neighbours that must stay green: EdgeCaseFixtureCorpus (theory and header test), CliKeepsDiagnosticShape, WebExtensionBoundary (orders hash), LeafPhraseFailureCompiles, SourceMapTraceability, UnsupportedCsharpConstructs, GeneratedApiSourceScan.Guard_checked_set_of_process_start_callers, the Compiler tests (the Diagnostics baseline, LeadingMessagesSayAvowTests, ExtensionBoundaryTests).

## 7. RISKS NOT VERIFIED
- .NET regex semantics: tested in Python only; the green must confirm each id on .NET.
- Sources built at runtime (the benchmark's LLM-generated avows) are not scanned: a model-written leaf with a stray undeclared `Fail` would now get a PH337 (a correct result; benchmark scores may shift).
- The PH337 range: ExtensionBoundaryTests.cs:104-106 was read, not run.
- Column: unpinned, like card 189 (bug 352); not verified for a body that PH105 re-indented.
- Generated C#: `no-clause` was observed only as source text, not compiled.
- The hostile `Fail("A" + b)` case is covered only in the Python check.

## Facilitator's decisions on the slice (2026-11-08)
- Accepted as written. The red is built against a freshly built CLI. The 15 ids of section 1 are the red's rows (7 failing, 8 keep-building); the red record lists the MULTI row with its two ids first (D222).
- The `no-clause` observation (the emitted C# cannot compile) goes to the retro as a product finding and to the card's Not-in-this-card list: this card makes it a diagnostic, it does not change the helper emission.
