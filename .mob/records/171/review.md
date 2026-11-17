SIGN-OFF

# Card 171 QA review (claude opus, claude-opus-5-5, context ctx-171q, fresh)

Range reviewed: `git diff 578ba98..d8bf1ad` (red d0bff79, green d8bf1ad) in a read-only clone. No dotnet run; no file changed except this one. Test results are the facilitator's proofs under Avowry's lock (red 02:48Z: the four Slice 171 tests failed, 169/142/176 passed; green 02:52Z: all 11 passed).

## 1. Criterion 1 (CLI knows nothing of the C# layout; dependency direction)
- Met. Program.cs no longer holds `Handler`, `Handle`, `Avows` or `Phrases` in any literal, nor `MethodName`/`IsHole`/`Parts`/`char.ToUpper`. csMethod is read from `output.EndpointMethods[endpoint.Name]` (Program.cs:128) and `output.AvowMethods[phrase.Key]` (Program.cs:96), beside the file maps.
- Direction is right: the maps are on `CompilationOutput` in Avowry.Compiler (CompilationOutput.cs:9-10), which both the CLI and the adapter reference; the adapter fills them (CSharpProvider.cs:42, 51, 56-59, 66-67) with its own `EndpointHandlerName` and its one `MethodName` (CSharpProvider.cs:852, 963). A second adapter can fill them the same way: they are plain `IReadOnlyDictionary<string,string>` init properties with no C# shape.
- MINOR: an adapter that leaves the maps at their empty defaults makes `WriteMap` throw an unhandled `KeyNotFoundException` (Program.cs:96, 128; only `CompilationFailedException` is caught at Program.cs:51). Same as the pre-existing `EndpointFiles`/`AvowFiles`, so not new risk, but a second-adapter author gets no clear message and `ITargetAdapter` does not say the maps are required. A guard or a doc comment on the properties would help (later card).
- MINOR: within the adapter, the handler method name `Handle` is now a literal in one more place (CSharpProvider.cs:51 `".Handle"`, beside 602, 652, 660, 784-794). Consistent today; a const would make it truly one definition.
- Note (out of scope, not a finding against this card): the CLI still selects the adapter itself (`new CSharpProvider()`, Program.cs:46; `--target csharp`, Program.cs:333). The story's "a second adapter does not need changes in the CLI" is true for naming, not for adapter selection.

## 2. Criterion 2 (style of Program.cs) and behaviour of the reformat
- Met everywhere by my own read: no line over 120 characters (longest is Program.cs:67 at exactly 120, unchanged line); no `!` null-forgiving; at most one statement per line (only `for` headers and a string literal hold two `;`); every nested loop braced (Program.cs:98-101); no chain over four calls (WriteMap is OrderBy + 3 ThenBy, then `ToList` on its own line, Program.cs:61-66).
- Behaviour: Usage text, version line, record ordering, map records (endpoint/step/failure csMethod string is identical: `endpoint.Name + "Handler.Handle"`), Print and Hash are unchanged. Two spots differ in code but not in reachable behaviour:
  - MINOR: Program.cs:178-182 is dead. `Validate(null)` already returns "the JSON value is null" (Program.cs:247-248), so the new `parsed is not { } map` branch can never run. Harmless, but it duplicates the message; `var map = parsed` after an `is not null` pattern in the Validate check, or a comment, would be cleaner.
  - MINOR: Program.cs:268-270, `CheckHash` returns `true` (reports a hash mismatch) when the map's directory is null. Before, the `!` would have thrown and trace exited 2 ("Could not trace"). Unreachable in practice (the map path passed `File.Exists` at Program.cs:166, so it has a directory), but if it ever ran it would claim a mismatch it did not check; `false` or an explicit error would be more honest.

## 3. Criterion 3 and R171-1
- Generated C#: the adapter diff only adds the two maps; no emit body changed. The pins are untouched in the range: `OrdersOutputHash = "19fe6d71..."` (WebExtensionBoundary.cs:18) and the Slice 176 golden have no diff. The facilitator's green proof has both passing.
- The avow csMethod is corrected: it now uses the adapter's hole-inclusive `MethodName` (CSharpProvider.cs:58, 963-964), the same function that names the generated method (CSharpProvider.cs:494), so e.g. `Avows.EnsureItemsIsNotEmpty` instead of the non-existent `Avows.EnsureIsNotEmpty`. No second rule was added.
- Anything else in the map: no. csFile, csHash, stepId, ordering and the endpoint/step/failure csMethod are produced as before; only avow records whose template has a hole change csMethod. R171-1 requires this to be recorded in the retro as a fix (not yet done; retro is later). docs/TRACEABILITY.md has no example csMethod for an avow, so no doc goes stale.
- MINOR (test strength): 171-3 checks the class and the method separately anywhere in the csFile (CliReadsAdapterNames.cs:201-202), not that the method is declared inside that class. Enough for today's files (one class per name); a later tightening could scope the method search to the class body.

## 4. Card 169's helper list
- Not weakened; strengthened. `MapHelpers` drops only `MethodName` (MapCultureInvariance.cs:253), which this card removes from Program.cs. The single planted program (which only proved a dirty `MethodName`) is replaced by one planted proof per remaining helper (`Calls`, `FailureDeclaration`, `AvowFailureDeclaration`, `Hash`) plus a clean program that must report nothing (MapCultureInvariance.cs:121-124, 255-261). The real-file scan and 169-1/169-3 are untouched.
- The culture-sensitive code moved, not vanished: the avow name is now built by the adapter's `Pascal` (CSharpProvider.cs:977-978), which uses `char.ToUpperInvariant`. 169-2 does not scan the adapter, but 169-1 (map bytes equal under tr-TR and sv-SE, end to end) still covers the map, and 171-1's scan forbids a `char.ToUpper`/`MethodName` returning to Program.cs.

## Could not check
- Compilation and test results: no dotnet run (forbidden); I rely on the facilitator's red and green proofs.
- The full gate (`dotnet test Phrase.sln --filter "Status!=Pending"`): the stated proofs cover 11 tests, not the whole suite. By reading, Slice 168 `Trace_resolves_a_Class_Method_frame_taken_from_the_map` stays self-consistent (it reads the csMethod from the same map), but it was not shown run.
- The red record's mutation steps 1-6 (not shown run in the facts I was given).
- That the Slice 142 and 176 pins were actually executed against the green tree beyond the facilitator's statement.
