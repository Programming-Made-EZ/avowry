SIGN-OFF (grok grok-4.6, normal mode in a throwaway clone, ctx-166x; reply saved by the facilitator; five MINOR)

## Findings

### MINOR: CLI dummy-reads the template maps
`src/Avowry.Cli/Program.cs:46-47` discards `compiled.EndpointTemplates` and `compiled.AvowTemplates` so `LinkInTheMap166.Program_cs_builds_no_template_...` still sees `.<name>` after R166-1 moved the writer. The real read is `SourceMapWriter.Write` (`src/Avowry.Compiler/SourceMapWriter.cs:44, 60`). Card 171 still holds: Program.cs builds no `endpoint Name : VERB /route` string and has no `Signature(`. The scanner was not retargeted onto the writer.

### MINOR: FindByRoute first-match order can differ when two templates contain the route
Old `FindByRoute` (`tools/Avowry.Bench/Program.cs` before the range) walked `AllTypes()` and took the first `AvowAttribute` whose `Template` contained the route. New code (`tools/Avowry.Bench/Program.cs:1096-1107`) walks map endpoint records in ordinal name order (`SourceMapWriter.cs:25-28`). The match rule is the same `Contains(..., OrdinalIgnoreCase)`. Existing fixtures have one `/tasks/open` hit (`LinkInTheMap166.cs:463-464`). A candidate with two templates that both contain `/tasks/open` can pick a different `IEndpoint` and change that attempt’s score. A candidate cannot omit the map: `ScoreExecuted` always writes it (`tools/Avowry.Bench/Program.cs:878-880`) before `ReadEndpointTemplatesFromMap`. A missing or malformed map throws out of `ScoreExecuted` instead of becoming `FailedExecuted`.

### MINOR: `// When handled` remains only on effectful specs
D285 verdict 3 and the red’s ambiguity 6 leave this free. Effect-free specs dropped it (`CSharpProvider.cs` EffectFreeEndpointSpecs); effectful specs still emit it (`CSharpProvider.cs:837`). Tasks golden keeps those lines. Not a step-source copy; Given/Then copies are gone.

### MINOR: ADR C# provider line citations were not retargeted
Reference citations are right: `reference/csharp/Orders.Api/CreateOrder.cs.cs` line 4 `using FastEndpoints`, 13-22 handler, 24 HTTP-binding 422 summary, 30 `AllowAnonymous()`, 37 `Send.OkAsync`, 42 `Send.ErrorsAsync(422)`, 25-62 the endpoint class (class closes at 63). `docs/adr/ADR-extension-map.md:52,74,75` and `docs/adr/ADR-library-binding-layering.md:92-95` still cite old `CSharpProvider.cs` lines (AllowAnonymous is now 744, `Send.OkAsync` 757/761, `ApiProject` 248). The ADR audits only check that cited files exist and line numbers fall inside the file.

### MINOR: leftover emitter comment
`src/Avowry.Adapters.CSharp/CSharpProvider.cs:608` still says “One comment and one statement per step”. `StepsBody` no longer emits the comment.

## Checks

1. **Criterion 1.** No `[Avow(` and no `AvowAttribute` under `src/`, `reference/`, or the golden. `AvowAttribute` is gone from `ReservedNames.cs`. Adapter fills `EndpointTemplates` / `AvowTemplates` (`CSharpProvider.cs:61-78`). Every map record ends with `template` (`SourceMapWriter.cs:12, 44, 60, 75`); avow/endpoint get Signature / `endpoint Name : VERB /route`, step/failure `""`. `schemaVersion` stays 1. Trace prints `template <text>` when non-empty (`Program.cs:235-236`); missing field defaults to empty (`MapRecord` default). Writer in Compiler is sound: CLI and bench share it, adapter does not reference it, card 169 now scans the whole of `SourceMapWriter.cs` (ordinal `OrderBy`/`ThenBy`/`Distinct` kept), card 165’s single recipe still lives in `SourceMapIds.cs` and both writer and adapter call it.

2. **Bench scorer.** Route table is built from endpoint `template` fields. Containment rule matches the old attribute rule. Red test 15 pins tasks-with-open, greetings, and orders. Measurement is unchanged for those cases.

3. **Criterion 3 / D285.** Auto-generated markers and runtime headers stay. File/construct names left the headers. Per-step and Given/Then copies are gone, including avow-spec `spec.Lines` comments. XML summaries that copied templates are gone. `Pure handler`, `HTTP binding`, and `Fails with:` remarks stay; `AvowryFailure` summary is “A declared failure code raised by an avow.” Comment lines were deleted whole; the golden shows no extra blanks. R166-2 restored the newline after the handler `{`.

4. **Tests outside slice 166.** Guards were retargeted, not dropped: `GeneratedCodeUsesAvowryNames` and `SourceMapTraceability` now forbid the attribute; `AvPrefixedDiagnostics` C9 uses `AvowryFailure`; `ReservedGeneratedNames` removes the duplicate `type-avowattribute` row (AvowryFailure already covered); `CliReadsAdapterNames` skips `*Template*` maps so `SingleOrDefault` still holds; `FailureObserverGenerated165` excludes comment lines and pins `DemoSpecsCodeHashCard166` (slice allowed); `PutAndDeleteEndpointsCompile` still asserts 200/422; `MapCultureInvariance` and the step-id rule moved to the writer with planted proofs kept and `normalizedName` forbidden outside `SourceMapIds.cs`; `CompositePhraseOrderCompiles` asserts adjacent call lines; `ReferenceTarget` mutates `AssertSpecEqual`.

5. **Reference.** `AvowryBuild.cs`, `AvowryFailureObserver.cs`, `CreateOrder.cs.cs`, and `CreateOrderSpecs.cs.cs` match `CSharpProvider` emission (build id `4d7a24b9877a79b4` is the same in the assembly metadata and the observer events; native bodies match `orders.avowry`; effect-free specs call the handler). `packages.lock.json` is untouched. Renames match card 193.

## Could not check

Did not run `dotnet` (forbidden). Did not recompute `WebExtensionBoundary.OrdersOutputHash`, regenerate the golden, or rehash `FailureStepIds` / `AvowryBuildId`. Did not run the bench suite; existing-case equality is from test 15 and the facilitator’s LlmCustomerCheck path. Facilitator proof 4 (compiler 344/344, acceptance 1611/1611 at `9d7e9e2`) is taken as given. Criterion 2 (grammar and cheat-sheet re-freeze) is out of this range.
