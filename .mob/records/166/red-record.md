# Card 166 red (D285): claude sonnet (claude-sonnet-5-5), context ctx-166r, fresh

Worktree ~/projects/wt-166, branch card-166. Written without running dotnet, a build or a test (the facilitator runs them under the heavy lock), so every "fails today" below is read from the code. No product file changed. No commit.

The card says [Phrase] and PhraseAttribute; the slice says it means today's `[Avow("...")]` and `AvowAttribute`. The tests use the Avow names.

## Files

New, in tests/Avowry.Acceptance.Tests (Trait Slice 166; Criterion 166-1 or 166-3, as the Slice 165 tests do):
- LinkInTheMap166.cs (class `LinkInTheMap166`, `IClassFixture<PristineTasks165>`, so the Tasks demo is built once by the CLI): 15 test methods, 18 cases. 166-1.
- LinkInTheMap166Comments.cs (class `LinkInTheMap166Comments`): 5 test methods, 8 cases. 166-3. In-process compile only, no CLI, no dotnet.
- No Support/ file was needed (the fixture PristineTasks165 and PhraseCli are reused).

Total: 20 test methods, 26 cases (15 facts and 5 theory methods with 2, 3, 2, 2 and 2 cases).

Changed existing tests (each with a `//` comment naming card 166 and D285; no method removed or renamed, so ExistingTestsKept172 is untouched):
- GeneratedCodeUsesAvowryNames: `Contains("class AvowAttribute")` and `Contains("[Avow(...)]")` become `DoesNotContain("class AvowAttribute")` and `DoesNotContain("[Avow(")`.
- AvPrefixedDiagnostics: row "C9 adapter-341" uses `type AvowryFailure` (was AvowAttribute); same line, same code 341.
- ReservedGeneratedNames: the row `type-avowattribute` is removed (a comment says why). Switching it to AvowryFailure would have duplicated the next row, `type-avowryfailure`, so I removed it instead of switching. Same coverage; the slice said "switch", this is the one deviation.
- SourceMapTraceability.AssertNoPhraseKnowledge also forbids `[Avow(` and `AvowAttribute`; the mutation test `No_phrase_knowledge_assertions_fail_on_a_reintroduced_leak` gains the leaks `[Avow("an avow")]` and `public sealed class AvowAttribute`.
- PutAndDeleteEndpointsCompile: `Assert.Contains("// Then succeeds", deleteSpecs)` becomes `DoesNotContain` (verdict 3 removes the spec Then lines). The slice did not list this test; it asserts a comment that verdict 3 deletes, so the green would have turned it red.
- FailureObserverGenerated165.The_eleven_demo_spec_files_...: the "kept" lines also exclude every comment line (trimmed text starting `//`), and the expected hash is the new constant `DemoSpecsCodeHashCard166` = 37864aa6e3136f39896fe48103dbe1c8ff7b30feb0c2928238c7f99fb3c9ae6a. I computed it with a small script (no dotnet) from the committed golden's Tasks.Specs sections with the same recipe; the same script run without the comment exclusion reproduces the old 8ed00cd9... exactly, which checks the recipe. Today this test still passes (today's output equals the golden). After the green it passes only if the 11 spec bodies and the Factory.Create lines are unchanged and the green deletes comment lines whole (leaving no blank line in their place). The old constant stays, unused.
- CliReadsAdapterNames (card 171) `NameMaps`: skips properties whose name holds "Template". Reason: that test does `SingleOrDefault` over every extra string map on CompilationOutput and matches by key set. A new template map keyed by endpoint name would make two maps match and the test throw. This is why I fixed the template-map name form (see Ambiguities, item 1).

## The tests and why each fails today

LinkInTheMap166 (166-1):
1. `No_generated_file_holds_an_avow_attribute_or_its_class` (tasks, orders; 2 cases). Fails today: CSharpProvider.cs emits `AvowAttribute` in the runtime file (about line 283), `[Avow(` on each avow (about 601) and each endpoint handler (about 702). Searches `output.Files`, so Api and Specs.
2. `A_type_named_AvowAttribute_compiles_because_nothing_generated_uses_that_name`. Fails today: the compile throws CompilationFailedException with AV341 (ReservedNames.TypeNames holds AvowAttribute).
3. `The_adapter_returns_the_template_text_of_every_avow_and_endpoint_through_the_compilation_output`. Fails today: CompilationOutput has no property whose name holds "Template". Expects the non-empty values of those maps to be exactly the 6 + 5 attribute texts of the golden (literals copied from TasksDemoGeneratedCsharp.golden lines 134 to 183 and 206 to 456, a comment says so).
4. `Every_Tasks_map_record_ends_with_a_template_field_holding_the_text_the_attribute_carried`. Fails today: records stop at csHash. Checks the exact property order of every record (stepId, kind, name, sourceFile, line, endpoint, step, specLines, csFile, csMethod, csHash, template), schemaVersion 1, 6 avow and 5 endpoint records with the literal text, step and failure records "".
5. `The_orders_map_ends_every_record_with_template_and_an_endpoint_record_holds_its_attribute_text`. Fails today: same. The endpoint text is rebuilt from the module as `endpoint Name : VERB /route`; avow records non-empty; step and failure "".
6. `Trace_of_an_avow_prints_its_template_line` (`avowry trace "find task {id: Id}"`, expects the trimmed line `template find task {id: Id} -> Task`, exit 0). Fails today: no such line. The avow record's name is the typed Template and its template field is the Signature (adds ` -> Task`), so the two differ and the test cannot pass by printing the name.
7. `Trace_of_an_endpoint_prints_its_template_line` (`trace CreateTask`, line `template endpoint CreateTask : POST /tasks`). Fails today.
8. `Trace_of_a_step_or_a_failure_prints_no_template_line`. PASSES today (guard against printing an empty `template ` line, or one for other kinds).
9. `Trace_prints_the_template_text_the_map_holds_and_does_not_recompute_it`. A planted one-record map with template "planted template text {x: Int}". Fails today (nothing printed). Stops the trace from recomputing the text instead of reading the field.
10. `A_map_without_a_template_field_still_traces_and_prints_no_template_line`. PASSES today (guard: the green must read a missing field as empty, with no exception, exit 0 and no line; same shape as card 172's planted maps).
11. `A_record_with_an_empty_template_prints_no_template_line`. PASSES today (guard).
12. `Program_cs_builds_no_template_and_calls_no_signature_and_reads_the_adapters_template_maps`. A scanner with planted forms first (an `endpoint ` literal, a literal holding ` : `, `Signature(`), then the real src/Avowry.Cli/Program.cs must have none, then it must contain `.<name>` for every CompilationOutput property whose name holds "Template". Fails today only at the last step (no such property yet); the "no knowledge" part passes today and guards the green.
13. `The_bench_scorer_no_longer_looks_for_the_attribute_or_reflects_on_custom_attributes`. Fails today: tools/Avowry.Bench/Program.cs holds `AvowAttribute` and `GetCustomAttributes` in the raw string of GenerateEndpointOutcomeTests (FindByRoute, about line 1079). Offline: the generated outcome source is a raw string in that file, so the file text is the seam.
14. `The_bench_scorer_reads_the_template_of_the_endpoint_records_from_the_candidates_map`. Fails today: the bench source holds neither `.avowrymap.json` nor `"template"`. Pins the form the slice binds.
15. `The_route_table_taken_from_the_map_finds_the_endpoints_the_attribute_rule_found` (tasks-with-open, greetings, orders; 3 cases). For each route of the fixture (and `/tasks/open`) the endpoint names whose text holds the route (OrdinalIgnoreCase, the old rule) are compared between the text the attribute carried (rebuilt from the module, so independent of the green) and the `template` of the CLI map's endpoint records. For tasks-with-open, `/tasks/open` finds exactly ListOpenTasks. Fixtures are the committed ones: the Tasks demo plus card 137's `ListOpenTasks` addition (copied from LlmCustomerCheck.OpenEndpointAddition; the only committed bench case where FindByRoute runs, with six endpoints), the 020 Greetings reply (copied from PhraseEndpointExecutedOutcomes; one endpoint, so FindByRoute is not reached, kept as the single-endpoint case), and orders. Fails today: the map's endpoint records have no template, so the map side finds nothing.

The executed proof that the scorer still picks the same endpoint is an existing test, not a new one: LlmCustomerCheck.Correct_reply_passes_and_writes_totals_only (and the other LlmCustomerCheck facts) build Tasks plus GET /tasks/open and run the real bench with executed scoring, which reaches FindByRoute. Test 15 pins the map half offline; that test pins the scorer half end to end.

LinkInTheMap166Comments (166-3; the text comes from the adapter in process):
16. `No_generated_comment_names_the_avowry_file_copies_a_step_or_copies_an_avow_template` (tasks, orders; 2 cases). A comment line (`//` or `///`) must not contain `.avowry`, `native csharp`, `fails with` (case-sensitive, so `Fails with:` in the remarks is fine), a hole with its type (`{name: Type`), any non-empty endpoint step text (an endpoint step's Source is the map's `step`), or any avow template (typed, and untyped as the old summary wrote it). Also, as the slice words it, no generated line at all (code or comment) contains `.avowry` or `native csharp`. Fails today: the headers name `.avowry` and `native csharp`, every step is copied as a comment, the summaries copy templates, AvowryFailure's summary says `fails with Code`, the spec comments say `Then fails with`.
17. `The_spec_Given_and_Then_comments_that_copy_the_source_are_gone` (2 cases): no comment starts `// Given` or `// Then`. Fails today (spec files, about lines 825 to 907 of CSharpProvider.cs).
18. `The_markers_the_plain_behaviour_docs_and_the_reworded_headers_are_in_the_generated_files` (2 cases): the auto-generated marker stays in every endpoint file, the Avows file, the runtime file and every endpoint spec file; each endpoint file has the exact line `/// <summary>Pure handler: the endpoint's steps, one line per step.</summary>`, a `/// <summary>HTTP binding.` line and `// Emitted by the Avowry C# provider.`; the Avows file has `<remarks>Fails with: ` and a header line `// Emitted by the Avowry C# adapter...` that holds `Generated code; do not edit.`; the runtime file has `/// <summary>A declared failure code raised by an avow.</summary>`; each endpoint spec header still starts `// Emitted by the Avowry C# provider` and names no endpoint. Fails today on the endpoint header, the Avows header and the failure summary (the markers and plain docs half passes today and guards the green).
19. `The_Tasks_remarks_still_list_each_declared_failure_of_each_avow`. PASSES today (guard: `Fails with: EmptyTitle, TitleTooLong`, `StoreUnavailable`, and `NotFound` twice).
20. `The_comment_scanner_reports_each_planted_coupling_and_accepts_plain_comments`. PASSES today (it tests the scanner: seven planted couplings are reported, six plain comments are not; the `Pure handler`, `HTTP binding`, `Fails with:` and marker lines are in the plain set).

Expected split today: 21 cases fail, 5 pass (26 cases in all). By method: 15 fail, 5 pass (tests 8, 10, 11, 19 and 20 pass; test 1 has 2 cases, test 15 has 3, tests 16 to 18 have 2 each).

Among the existing tests I changed: GeneratedCodeUsesAvowryNames, PutAndDeleteEndpointsCompile and the real-output half of the SourceMapTraceability tests that call AssertNoPhraseKnowledge fail today, by design (they now assert the new state). AvPrefixedDiagnostics, ReservedGeneratedNames, the card 171 CliReadsAdapterNames, the BornGreen eleven-spec-files test and `No_phrase_knowledge_assertions_fail_on_a_reintroduced_leak` pass today and stay green after the green.

## What the green must change

CSharpProvider.cs: drop the `AvowAttribute` class (runtime file), every `[Avow(` line (avow methods and endpoint handlers); drop the `Summary` XML summary of avows; drop the per-step `// ...` comments in endpoint handlers and avow bodies, the spec `// Given` and `// Then` comments, and the avow spec lines (about 1028 to 1038); rewrite the headers (endpoint file `// Emitted by the Avowry C# provider.`; Avows file `// Emitted by the Avowry C# adapter. Generated code; do not edit.` without the `native csharp` line; spec headers without the endpoint name and `.avowry`); AvowryFailure summary `A declared failure code raised by an avow.`; keep `Pure handler`, `HTTP binding`, `<remarks>Fails with:`. Return the template text (the adapter's Signature per avow key; `endpoint Name : VERB /route` per endpoint name) on CompilationOutput as `IReadOnlyDictionary<string, string>` properties whose names hold "Template" (for example EndpointTemplates and AvowTemplates). ReservedNames.cs: remove AvowAttribute (the constant and the TypeNames entry).

CLI (Program.cs): add `template` to MapRecord as the last property (after CsHash; avow and endpoint records from the CompilationOutput maps, step and failure ""); schemaVersion stays 1; make the trace reader accept a missing field (null reads as empty); `Print` writes `  template <text>` for a non-empty template. No adapter knowledge in the CLI (no `endpoint ` or ` : ` template building, no Signature).

Bench (tools/Avowry.Bench/Program.cs): remove `AvowAttribute` and `GetCustomAttributes`; read the candidate build's `*.avowrymap.json` at generation time and embed the route-to-endpoint table from the endpoint records' template; FindByRoute uses it. Note that the harness never writes a map today (it calls `output.WriteTo(generated)`, and WriteMap lives in the CLI), so the green needs a map writer the bench can call or a CLI build of the candidate (Ambiguities, item 3).

docs/TRACEABILITY.md names `[Avow("...")]` and the header comment in its second paragraph; no test pins that text, so update it with the card (the new `template` field and the trace line). GRAMMAR.md line 297 and the cheat sheet are part 2, not now.

The facilitator, not the green: the Slice 176 golden and its re-record comment (the card 165 test `The_demo_golden_is_re_recorded...` needs a comment line in TasksDemoGeneratedCsharp.cs that names 165 and the observer; keep it), WebExtensionBoundary.OrdersOutputHash (the comment block directly above it must keep "165" and "observer" for that card 165 test; add 166 and D285), reference/csharp regeneration, the card 172 guard text.

## Existing tests the green or the pin run touch

Fail until pinned or regenerated by the facilitator: TasksDemoGeneratedCsharp (golden), WebExtensionBoundary (orders hash), the card 172 guard text. The tests I changed deliberately are listed under Files. Tests that run the generated specs (DotnetCli.TestAsync in many files) stay green only if the spec bodies stay the same; the green must not drop a Specs `using` or change a body. LlmCustomerCheck (executed bench with GET /tasks/open) is the end-to-end check of the scorer change. ReferenceTarget builds reference/csharp (it is not compared with the generator), so regenerating the reference matters only if it fails to build.

## Mutation steps (what turns each test red)

- 1, 13: put `[Avow(` back on one endpoint, or leave `GetCustomAttributes` in the bench harness.
- 2: leave `AvowAttribute` in ReservedNames.TypeNames.
- 3: return the endpoint template without the VERB, drop one avow's ` -> Type`, or name the map without "Template".
- 4, 5: write `template` before csHash, write text for step records, or bump schemaVersion.
- 6, 7: print the record's name instead of its template; print it for the wrong record; indent it so the trimmed text no longer starts `template `.
- 8: print `template ` with an empty text for step records.
- 9: recompute the template in the CLI from the record's name instead of reading the field.
- 10, 11: throw on a missing field; print `template ` for an empty one.
- 12: add `$"endpoint {name} : {verb}"` or `Signature(` to Program.cs; or read the template through a name that is not a CompilationOutput property.
- 14: drop the map read from the bench.
- 15: emit the endpoint text with a different route form or without the route.
- 16: leave one `// let ...` step comment, `.avowry` in a header, `native csharp`, `fails with Code` in AvowryFailure's summary, or keep the avow summary.
- 17: keep one `// Then fails with` line in a spec.
- 18: remove `Pure handler`, drop the `<remarks>`, keep the old header, or leave the endpoint name in a spec header.
- 19: change the `Fails with:` remark spelling.
- 20: make the scanner skip `///` lines (the planted XML-summary lines would no longer be reported).

## Ambiguities in the slice, and the form I fixed

1. Name of the template maps on CompilationOutput: the slice says "as EndpointMethods and AvowMethods do" and nothing more. Fixed: `IReadOnlyDictionary<string, string>` properties whose names hold "Template". Why a form at all: card 171's `Map_csMethod_values_are_the_names_the_adapter_returned` finds method maps by shape and uses `SingleOrDefault`, so an unnamed extra map keyed by endpoint name breaks it. I made that test skip names holding "Template".
2. Template text: the slice says "the adapter's Signature" for avows and "the text its attribute carried" for endpoints. Fixed: avow = Template plus ` -> Returns` when it returns a value (the record's `name` stays the typed Template without the return); endpoint = `endpoint Name : VERB /route`.
3. How the bench gets a map: the slice says the harness "reads the candidate build's `*.avowrymap.json`", but ScoreExecuted only calls `output.WriteTo(generated)` and the map is written by the CLI. I fixed only the observable form: the bench source mentions `.avowrymap.json` and `"template"` and no longer mentions `AvowAttribute` or `GetCustomAttributes`. If the green prefers to read the CompilationOutput template maps directly (no map file), the facilitator should relax test 14; the slice binds the map, so I followed it.
4. "any step source text from the map's step records": I read the step texts from the compiler module (an endpoint step's Source is what the map writes as `step`), so test 16 runs in process. Avow template text is checked typed and untyped.
5. Verdict 2's "the spec header likewise": taken as an exact header only for endpoint files; for endpoint spec files I pin the start `// Emitted by the Avowry C# provider` and the absence of any endpoint name and of `.avowry`. A spec header that keeps "from the `spec` blocks" passes. The avow spec file (AvowSpecs.cs) header is checked only by test 16 (no `.avowry`, no step or template text).
6. Verdict 3 and `// When handled`: the slice names "the spec Given/Then lines"; I pin `// Given` and `// Then` gone and leave `// When handled` free either way.
7. Comment lines a user writes inside a `native csharp` block pass through unchanged; the Tasks demo and orders have none, so test 16 does not have to tell them apart.
8. The BornGreen test: the slice says the red "may extend its exclusion to removed comment lines". I excluded every comment line (including the changed header) and recorded a new hash; there is no allowance for blank lines, so the green must remove comment lines whole.
9. ReservedGeneratedNames: the row was removed rather than switched (it would duplicate the next row), see Files.
10. Test 12 reads the CLI source by regex (string literals that start `endpoint ` or hold ` : `, and `Signature(`); it does not handle escaped quotes in a literal, and Program.cs holds none today.

## Red fix round 1 (R166-1, green proof 2's four existing-test failures)

Same seat, ctx-166r. Four existing tests changed, each with a `//` comment naming card 166 and R166-1; no method renamed or removed, nothing relaxed beyond the ruling. Not compiled or run (no dotnet).

1. MapCultureInvariance.Every_sort_in_the_map_writer_uses_an_ordinal_comparison: reads src/Avowry.Compiler/SourceMapWriter.cs (the whole file) instead of Program.cs's WriteMap-to-Trace region. Kept: the non-ordinal scanner and all its planted proofs; the helper list (Calls, FailureDeclaration, AvowFailureDeclaration, Hash), each found by its declaration and scanned to the next declaration (the declaration regex now allows indentation and private/public/internal, and the boundary also stops at a `record`); the planted program per dirty helper (now shaped like the writer: a class, indented helpers); the `.OrderBy(` presence check; `static string StepId(` must exist in the writer. The StepId requirement is now stated as the writer's call to the shared rule: the writer must match `SourceMapIds.StepId(` (the planted clean program has that call). The failure message now says SourceMapWriter.cs. No other MapCultureInvariance test read the Program.cs region (the other uses of Program.cs there are comments).
2. FailureObserverGenerated165.One_public_step_id_rule_lives_in_the_compiler_and_both_the_cli_and_the_adapter_call_it: the call requirement moves from Program.cs to SourceMapWriter.cs; the adapter check stays; new: `normalizedName` must appear in no `.cs` file under src/ (bin and obj skipped) except SourceMapIds.cs. This replaces the old check that only Program.cs lacked it, so it is stricter, not looser. The method name still says "the cli"; kept for ExistingTestsKept172.
3. CompositePhraseOrderCompiles.Composite_phrase_calls_three_leaves_in_order_and_surfaces_a_leaf_failure: the three `// let ...` comment lines are dropped from the expected block; the order is asserted through the three consecutive call lines `var price = Avows.PriceOfSku(sku);`, `var units = Avows.UnitsOfName(name);`, `var label = Avows.LabelOfName(name);` (the green's StepsBody joins steps with one newline and no comments, so they are adjacent; they are read from the generated Avows.cs as before).
4. ReferenceTarget.Reference_runner_reports_a_mutated_expectation_as_failed: mutates `AssertSpecEqual("sums quantities across line items", 5, result);` to 6 (the line exists in the regenerated reference/csharp/Orders.Specs/AvowSpecs.cs, line 31).

Mutation steps: 1: add `.ToLower()` to a helper in SourceMapWriter.cs, or make StepId stop calling SourceMapIds; 2: paste the recipe (`normalizedName`) into any other src file, or make the writer hash on its own; 3: swap two call lines or reintroduce a comment between them; 4: not applicable (the mutation is the test's own).
Risk for the facilitator: item 2 and test 12 of the first red both read file text; the added src scan reads every src .cs file once (cheap).

## Red guard (R166-2)

One Theory added to LinkInTheMap166Comments.cs (the only file touched): `No_generated_line_opens_a_brace_and_carries_more_text_on_the_same_line` (tasks, orders; Slice 166, Criterion 166-3; in-process compile, no dotnet). Rule: no line of any generated .cs file starts, after whitespace, with `{` followed by more non-whitespace text (regex `^\s*\{\s*\S`, per line, CRLF normalised). Planted proofs in the test: `{    public static int Handle(int input)` is reported; `{`, `    {`, `    }` and `    public static int Handle(int input) { return 1; }` are not.
Fails today: the green's handler classes open with `{    public static ... Handle(` (5 lines in the Tasks golden, lines 185, 245, 305, 363, 421; 1 in the regenerated reference CreateOrder.cs.cs line 14, which is the orders output). I grepped the golden and the reference for the same pattern: those six lines are the only matches, so the rule holds no false positive on today's output.
Mutation: put a handler's `public static` back on the brace line, or drop the newline after any other brace. The planted line proves the check itself.

## Red round 2 (R166-3: review findings M1, M2, M4)

Test files only (LinkInTheMap166.cs, LinkInTheMap166Comments.cs). Not compiled or run (no dotnet). The green is 9d7e9e2.

- M1: new Theory `No_generated_comment_restates_a_step_of_a_spec_including_When_handled` (tasks, orders; Slice 166, Criterion 166-3) in LinkInTheMap166Comments.cs: no comment line of any generated .cs file starts `// When`, `// Given` or `// Then`. Fails today for tasks (effectful specs emit `// When handled`, CSharpProvider.cs about line 837; 11 times in the golden); the orders case passes today (effect-free specs lost the line in the green), and guards it. The older Given/Then theory stays.
- M2: `Program_cs_builds_no_template_and_calls_no_signature_and_reads_the_adapters_template_maps` (name kept, comment names R166-3): the CompilationOutput template maps must include EndpointTemplates and AvowTemplates (found by shape as before); Program.cs must pass the template-knowledge scan and must not contain any template map's name (the discards `_ = compiled.EndpointTemplates;` and `_ = compiled.AvowTemplates;` at Program.cs:46-47 must go); src/Avowry.Compiler/SourceMapWriter.cs must pass the same scan (no `endpoint ` or ` : ` template text, no Signature) and must contain `.EndpointTemplates` and `.AvowTemplates`. Fails today on the Program.cs discards; the writer half passes today (guards the writer against building templates itself).
- M4: new Fact `A_record_whose_template_is_JSON_null_traces_as_an_empty_template` (166-1): a planted map whose record has `"template": null` (the test asserts the planted file holds that text; the planting helper `Plant` gained an optional `jsonNull` flag): `trace` exits 0, prints the whole record (a `  build: ` line), prints no `template` line, and its output holds neither `Object reference` nor `Could not trace`. Fails today: Program.cs:235 reads `record.Template.Length` on null, throws after five lines, and the catch-all gives exit 2.

Fail today: M1 tasks case, M2, M4. Pass today: M1 orders case, and the SourceMapWriter half inside M2 (but that test fails as a whole).
Mutation: M1: re-emit `// When handled` in any spec; M2: read a template map in Program.cs again, build `endpoint X : ...` text or call Signature in the writer, or stop the writer reading one map; M4: read `record.Template.Length` without a null check.
