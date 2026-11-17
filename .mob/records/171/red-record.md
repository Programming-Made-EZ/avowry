# Card 171 red record (red seat: claude sonnet, claude-sonnet-5-5, context ctx-171r)

Worktree: `~/projects/wt-171` (branch card-171). Nothing committed. No dotnet was run (the facilitator's run, under Avowry's heavy lock). The code below was written without a compiler, so a compile error in the new file is the first thing to rule out.

Files: new `tests/Avowry.Acceptance.Tests/CliReadsAdapterNames.cs`; edited `tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs` (helper list only). No product code, no docs.

## Tests by criterion (all `Slice=171`; the Criterion trait is `171-1`, `171-2`, `171-3`)

171-1 (two tests, both Criterion 171-1):
- `Program_cs_holds_no_adapter_literal_and_no_method_name_rule`: lexes Program.cs (strings, interpolated strings with nested strings, char literals and `//` comments are understood). Fails if any string literal (the text parts of interpolated strings too) contains `Handler`, `Handle`, `Avows` or `Phrases`, or if the masked code holds `MethodName`, `IsHole`, `Parts` or `char.ToUpper` (the method-name rule). Seven planted rows must each be reported; identifiers (`module.Avows`, `output.AvowFiles`), comments and other literals must not be.
- `Map_csMethod_values_are_the_names_the_adapter_returned`: compiles a fixture (module Names: three avows, endpoints Alpha and Beta, both calling all three avows, with failures) with `AvowryCompiler.Compile(..., new CSharpProvider(), file)`; finds the adapter's two new maps by shape (any `IReadOnlyDictionary<string,string>` property of `CompilationOutput` other than `Files`, `EndpointFiles`, `AvowFiles`; one keyed by the endpoint names, one keyed by the avow keys); runs the built `Avowry.Cli.dll` entry point in the test process (as card 169 does; `PhraseCli.RunAsync("--version")` first to build and check freshness) and reads `Names.avowrymap.json`. Each endpoint, step and failure record's `csMethod` must equal the endpoint map's value for `record.endpoint`; each avow record's must equal the avow map's value for the key of the avow with that template. Also asserts 2 endpoint records, 3 avow records, at least one step and one failure record, and that the map values are distinct.

171-2: `Program_cs_keeps_one_statement_per_line_short_lines_short_chains_and_no_null_forgiving`. Five rules on the real Program.cs, each with planted rows (and clean rows at each limit): statements (more than one `;` at paren depth 0 on a line), loop (a loop header with its body on the same line, or whose unbraced body is another loop header), length (over 120 characters), chain (more than four `.Name(` calls chained on one value at one nesting depth; a nested chain counts on its own), null-forgiving (`!` after an identifier, `)` or `]`, not `!=`, not logical not).

171-3 (revised under ruling R171-1): `Every_map_csMethod_names_a_method_that_exists_in_the_generated_csharp`. Builds orders.avowry and the names fixture (both have avows with holes) with the CLI process, and for every map record checks that `csMethod` is `Type.Method` and that the record's `csFile` declares `class Type` and a `public static ... Method(`. Requires 6 avow records checked (3 + 3). Today's hole-skipping csMethod values are no longer pinned. The byte-identical proof of the generated C# is the two existing pins below.
Existing pins that already cover the generated C# (not repeated):
- `WebExtensionBoundary.Orders_phrase_loads_web_and_keeps_the_output_recorded_before_the_slice` (Slice 142): hash of the generated orders file set, constant `19fe6d71...` (the 409bf3c6 in the card was recorded before card 193 changed generated names; the file holds 19fe6d71).
- `TasksDemoGeneratedCsharp.Tasks_demo_generated_csharp_matches_the_golden_file` (Slice 176): the demo's generated C# against `TasksDemoGeneratedCsharp.golden`.
Neither carries Slice 171, so `--filter "Slice=171|Slice=169"` does not run them. Add `|Slice=142|Slice=176` (or run both by name) to check criterion 3. I did not edit their traits.

Card 169 change: in `MapCultureInvariance.cs`, `MapHelpers` drops `MethodName` (the helper this card moves); the single planted program that carried a culture-aware call in `MethodName` is replaced by `PlantedProgram(dirtyHelper)`, built from `MapHelpers`, so each remaining helper (`Calls`, `FailureDeclaration`, `AvowFailureDeclaration`, `Hash`) has its own planted proof, plus a clean planted program that must report nothing. Every other 169 check is untouched.

## Expected today (before the green)

| Test | Today | Why |
|---|---|---|
| 171-1 scan | FAIL | `$"Avows.{MethodName(phrase)}"`, `$"{endpoint.Name}Handler.Handle"`, `static string MethodName(` with `IsHole`, `Parts`, `char.ToUpperInvariant` are in Program.cs. The planted rows pass. |
| 171-1 build | FAIL | `CompilationOutput` has no name maps, so `Assert.True(endpointMethods is not null, ...)` fails with the message naming the missing map. (True red: the build and the map read succeed first.) |
| 171-2 style | FAIL | Lines over 120 characters; lines 86 to 87 stacked `foreach`; the five-call chain (`.OrderBy .ThenBy .ThenBy .ThenBy .ToList`) in WriteMap; `parsed!`, `GetDirectoryName(...)!` null-forgiving. The planted and clean rows pass (they run first). |
| 171-3 | FAIL | True red: the CLI writes `Avows.EnsureIsNotEmpty` (holes skipped) but Avows.cs declares `EnsureItemsIsNotEmpty`, so the avow records for a phrase with a hole name a method that does not exist. Endpoint, step and failure records (`<Endpoint>Handler.Handle`) pass. |
| 169-1, 169-2, 169-3 | PASS | 169-2 still finds all four remaining helpers in Program.cs today. |
| 142 and 176 pins | PASS | Unchanged. |

After the green (the adapter's one rule, holes included, no second rule): 171-1 x2, 171-2, 171-3, 169-1..3 all pass.

## Seams the green must add (and a trap)

- `CompilationOutput` gains two `IReadOnlyDictionary<string, string>` properties (names are the green's choice; the test finds them by shape and expects exactly those two beyond `Files`, `EndpointFiles`, `AvowFiles`; a third string map would make the lookup ambiguous, a failure not a pass): method per endpoint name, method per avow key. The adapter fills them (CSharpProvider.cs, next to `EndpointFiles` and `AvowFiles`, lines 55 to 58).
- No second rule: per R171-1 the adapter's existing `MethodName` (holes included, the generated method's real name) is the one definition; the map's avow `csMethod` changes from `Avows.EnsureIsNotEmpty` to `Avows.EnsureItemsIsNotEmpty` (a fix, for the retro). The generated C# must not change.
- Program.cs must then keep no literal containing Handler, Handle, Avows or Phrases (the `Avows` property of the module is an identifier and is fine), and the five style rules.
- Card 169's 169-2 only needs `MethodName` removed from Program.cs; it fails with "The map helper MethodName was not found" only if the helper list still names it (the red already removed it).

## Mutation steps (for the facilitator; the green's tree, run under the lock, restore after each)

1. 171-1 scan: put `$"{endpoint.Name}Handler.Handle"` back in `Record` (or any literal with `Avows`). Expect the scan to fail and the build test to still pass (the adapter value equals it). Proves the scan is the guard for hardcoding.
2. 171-1 build: change the adapter's endpoint map value (for example append `X`) without changing the CLI. Expect the build test to fail on an endpoint, step or failure record. Repeat for the avow map. Proves the CLI reads the maps and does not compute names.
3. 171-1 build, other direction: have the CLI compute the name itself but not via a banned literal (for example a helper that concatenates `endpoint.Name + "Hand" + "ler"`) while the adapter returns something else: the build test fails.
4. 171-2: re-join two statements on one line, or stack two `foreach` headers, or pad a line to 121 characters, or chain five calls, or add a `!`. Each must fail the test under its own rule (the planted rows already prove the scanner; this proves the real file is read).
5. 171-3: make the adapter's avow map skip holes again (or return any name that Avows.cs does not declare). Expect 171-3 to fail on the avow records; the Slice 142 and 176 pins stay green (they pin generated C#, not the map). Then change a generated method name in the adapter without the map (generated C# changes): the Slice 142 or 176 pin fails.
6. 169-2: put a `.ToLower()` in `Calls`, `FailureDeclaration`, `AvowFailureDeclaration` or `Hash` in Program.cs. Expect 169-2 to fail for each (the planted proofs cover each helper).

## Ambiguities

- The slice says the recorded orders hash is `409bf3c6...`; the Slice 142 pin holds `19fe6d71...` (changed by card 193). Criterion 3 means the current pin (ruling R171-1 agrees).
- The slice names the maps "per endpoint (and phrase)" but fixes no property names; the build test finds them by shape. If the green wants fixed names, the facilitator should say so (a rename would be a one-line edit in the test).
- "No statement chaining more than four `.Name(` calls" is read as four calls in a row on one value at one depth; arguments with their own chains are counted on their own. A stricter reading (all calls in a statement) would fail on `Hash`, which nests six calls through argument positions; I did not choose it.
- "One statement per line" is checked as at most one `;` at paren depth 0 per line; a braced one-liner with one statement (`{ x; }`) passes the statements rule.
- The 171-1 build test runs the CLI in the test process, which needs the CLI built and fresh; it calls `PhraseCli.RunAsync("--version")` first, as card 169 does.
