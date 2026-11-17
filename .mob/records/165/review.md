CHANGES (card 165 QA and security seat: claude opus, claude-opus-5-5, ctx-165q; range a8a74fb..bb5a383; read only, no dotnet run)

The design holds up. The event is five compile-time constants plus `failure.Code`. The no-op uses TryAddSingleton. The observer runs in a fire-and-forget Task.Run with an empty catch, after `Send.ErrorsAsync(422)`. The step-id recipe moved over byte for byte, and trace by step id is an exact ordinal match. One regression blocks: valid sources that compiled before this card now produce C# that does not compile. The other findings are minor.

## Findings

### F1 BLOCKING: a failure code spelled like a C# keyword now produces C# that does not compile
- src/Avowry.Adapters.CSharp/CSharpProvider.cs:361-362: `FailureStepIdName = CSharpIdent(endpoint) + "_" + CSharpIdent(failure)`. `CSharpKeywords` (line 1088) uses `StringComparer.OrdinalIgnoreCase`, so `CSharpIdent("Default")` returns `@Default`. For endpoint CreateTask and failure `Default`, the generator emits `internal const string CreateTask_@Default = "...";` (FailureStepIds) and `"Default" => FailureStepIds.CreateTask_@Default` (endpoint catch). `@` is legal only at the start of an identifier, so this is a C# syntax error.
- Failure codes are Capitalized ASCII letter-or-digit TypeName tokens (Lexer.cs:271-276). Nothing reserves keyword spellings (no AV diagnostic), and `fails with Default`, `Private`, `Protected`, `Readonly`, `Fixed`, `Null` or `Event` all parse. Before card 165, failure codes reached the C# only as string literals, so such a source compiled. Now any endpoint that declares one fails to build.
- No test covers it: the suite has no keyword-spelled failure code. The red's tests and the D284 regex (`\w+`) never see this case.
- Fix: names are `[A-Z][A-Za-z0-9]*` with no underscore, so `endpoint + "_" + failure` is always a valid, non-keyword identifier that cannot collide with another constant name. Drop `CSharpIdent` here, or apply it to the whole joined name. Add one row, for example an endpoint failing with `Default`, to an existing compile test.

### F2 MINOR: `FailureStepIds` is a top-level generated type, but it is not reserved
- CSharpProvider.cs:315 spells `internal static class FailureStepIds` as a literal, not through ReservedNames. ReservedNames.cs says the emitter "spells every one of them through these constants". The name is also missing from `ReservedNames.TypeNames`.
- A source with `type FailureStepIds` and any declared failure now gets CS0101 (duplicate type), not AV341. ReservedGeneratedNames (Slice 203) exists to prevent exactly that: "so that I do not meet an unexplained C# error". The test still passes because it lists its rows by hand and nothing enumerates the generated top-level types.
- A smaller case of the same problem: an effect named `AvowryFailureObserver` generates `public interface IAvowryFailureObserver { }`, which collides with the observer interface and also gives CS0101.
- Fix: add a `FailureStepIds` constant to TypeNames and spell it through the constant. Add a row to ReservedGeneratedNames. Optionally make the effect name `AvowryFailureObserver` an AV341 too.

### F3 MINOR (security; intent of the "no line in the runtime" rule): the step id makes the line number recoverable without the map
- The recipe is public (docs/TRACEABILITY.md:7 documents the exact hash input). The event already carries the code and the endpoint. So anyone holding a production log line, or the generated C# or DLL, can recover the declaration line by hashing `failure\n<n>\n<Code>\n<Endpoint>` for n = 1..N. No map is needed.
- The literal checks hold: there is no `.avowry:<digit>`, no line number and no spec text in the event or the lookup (SourceMapTraceability.AssertNoPhraseKnowledge, the 165 scans). But TRACEABILITY.md:37 calls the step id "opaque". Line 9 says the runtime carries no "source names or locations". In effect, a location is now in the runtime in an unsalted, invertible form.
- The exposure is low: a line number carries no source text, and the `[Avow("...")]` attributes already put avow templates in the runtime. D284 rejected changing the scheme (option B). So no product change is asked for here. Fix the doc: say the step id is derived from the declaration line, is not secret, and can be recomputed by anyone who knows the code, the endpoint and the recipe. Drop "opaque" in line 37.

### F4 MINOR (security): the event's Code is the runtime exception string, not the declared constant, so request data can reach observer logs
- CSharpProvider.cs:342 passes `failure.Code` as the event's Code, and line 352 maps any unmatched code to `_ => ""`.
- `AvowryFailure` and `Raise(string)` are public (CSharpProvider.cs:289-294), so native csharp code can raise a code built from input, for example `AvowryFailure.Raise("Bad" + input.title)`. The 422 body already echoes that text to the caller (`AddError(failure.Code)`). The observer then sends it into production logs.
- With the documented plain logging line (`avowry-failure v1 code=... stepId=...`), a code holding a newline or ` stepId=` can forge log lines or fields (log injection).
- The `_ => ""` fallback itself is reasonable: an undeclared failure has no map record (TRACEABILITY says so) and gets no step id.
- Behaviour is also inconsistent. An endpoint with at least one declared failure reports undeclared codes with `StepId ""`. An endpoint with no declared failure (line 766: `if (endpoint.Failures.Count > 0)`) reports nothing.
- Suggested fix: in the switch, emit the declared constant for both Code and StepId. Either do not raise the event for an undeclared code, or raise it with a fixed code such as `Undeclared`. Then every event field is a compile-time constant, which is what slice.md line 9 says.

### F5 MINOR: one hash rule, but two copies of the "which declaration line" rule
- SourceMapIds.StepId is shared, and the map bytes cannot change: the recipe is identical, including the forwarder's default `endpoint = ""` for avow records.
- But the line fed into the id is chosen twice:
  - CLI: src/Avowry.Cli/Program.cs:336-345 (`FailureDeclaration`, `AvowFailureDeclaration`) plus the `FirstOrDefault` over endpoint steps at about line 100.
  - Adapter: CSharpProvider.cs:364-386 (`FindFailureDeclaration`, `FailureDeclaration`, `AvowFailureDeclaration`).
- The two copies are textually equivalent today. If they drift, the generated constant differs from the map id, and only the live Tasks test or the Tasks-only offline test would notice. The composite and ensure-ordering cases in SourceMapTraceability check the map, not the generated constants.
- Fix: move the selection into Avowry.Compiler next to `SourceMapIds.StepId`, for example `SourceMapIds.FailureDeclaration(module, endpoint, failure)`, and call it from both.

### F6 MINOR (repo-wide guard intent): MapCultureInvariance no longer scans the recipe it was written to guard
- MapCultureInvariance.cs:264-279 scans the CLI's map-writer region and the helpers named in `MapHelpers`. The real recipe now lives in src/Avowry.Compiler/SourceMapIds.cs, and the adapter's copy of the declaration rule lives in CSharpProvider.cs. Neither is scanned.
- Line 269 only proves that a `static string StepId(` forwarder exists. The recipe is clean today (`Replace(..., Ordinal)`, `ToLowerInvariant`, a positive int formatted by interpolation). But a future culture-sensitive edit in SourceMapIds.cs would not be caught statically; only the tr-TR and sv-SE byte test would catch it, and only if the edit changes Tasks or orders bytes.
- Fix (test-only, later card): add SourceMapIds.cs to the scanned regions.

### F7 MINOR (off the request path; not a test failure): an unbounded Task.Run per failure lets a slow synchronous observer starve the thread pool
- CSharpProvider.cs:354-358. Each 422 queues one pool work item. A blocking observer, such as a synchronous HTTP log shipper, holds one pool thread per failure. EmptyTitle is a trivial anonymous request, so a burst of failing requests becomes thread-pool starvation for the whole API (a DoS amplifier). The 2-second, six-request live test cannot see this.
- Exceptions: `catch { }` makes every task complete successfully, so there are no unobserved task exceptions and the process cannot be torn down by an exception the observer throws synchronously.
- An `async void OnFailure` implementation can still crash the process, because its exception escapes on the pool. The empty catch cannot help, by design.
- If `Send.ErrorsAsync` throws (client gone), the observer is not called. That is acceptable.
- The 422 status and body cannot be changed: the event is built after the send and shares no state with the response.
- Fix (doc only): TRACEABILITY.md should say that OnFailure must be fast, non-blocking and synchronous (enqueue and return), and must not be `async void`. Events are best-effort: unordered, and lost on shutdown.

### F8 MINOR: the test edits outside Slice 165 stay within R165-2 and D284, with two small widenings
- PersistenceEffectCompiles.cs:114-118 and PutAndDeleteEndpointsCompile.cs:252-254 remove only the exact single TryAddSingleton line. PersistenceEffectCompiles also skips the whole AvowryFailureObserver.cs file. Both are as ruled.
- OrdersPhraseCompiles.cs:33-36 excludes the file from the `Option<` check as well as `interface I`. The ruling allows excluding the file. A store or `Option<` emitted into that file later would go unseen; stripping only the one interface line would be tighter.
- ExistingTestsKept172.cs:44-45 now pins a 7-hex prefix `ec3a552`; the old pin was 8 hex, `19fe6d71`. Harmless, but the pin is weaker than before.
- WebExtensionBoundary (D284 option A) is sound:
  - The mask touches only `internal const string X = "<12 hex>";` lines in AvowryFailureObserver.cs, and every other byte is compared.
  - It asserts the constant sets are equal and non-empty, and that each id equals `SourceMapIds.StepId` at the independently read line (as-is line plus one).
  - The as-is hash pin stays exact.
- The golden and the orders pin carry comments naming card 165 and the observer. The 11-spec test pins everything except the `Factory.Create<` lines.
- Doc nit: the TRACEABILITY "Executable evidence" list does not mention the Slice 165 tests.

### F9 no finding: trace by exact step id is unambiguous in practice
- Program.cs:179-181 matches `Name == q || CsMethod == q || StepId == q`, ordinal, with no prefix match (the BornGreen test guards this).
- Endpoint and failure names and frames start uppercase, so none can equal 12 lowercase hex.
- Only an avow template or step text made entirely of characters 0-9 and a-f, 12 long, could collide, which is extremely unlikely. Trace prints every match, so a collision would be visible, not silent. The same holds for a 48-bit id collision.

## Repo-wide scans of the generated C# and the generated file set (checked for intent, not only for passing)
- SourceMapTraceability.AssertNoPhraseKnowledge (:337): the intent holds literally (no `.avowry:N`, no map reference, no Phrase names in the new file). The spirit is weakened by F3, which is a doc fix.
- GeneratedCodeUsesAvowryNames: holds. `FindSingle("AvowryRuntime.cs")` is not matched by the new file name, and "Phrase" does not appear in it.
- SourceMapDeployment: holds. The map is still not in the publish output and is still not served, and the new file carries no map.
- UsableV1 bars 1-6: hold. The zero-warning bar is why R165-2 dropped the observer parameter where it is unread. Bar 6 currency is keyed to the grammar and cheat-sheet hashes, so it is unaffected.
- ReservedGeneratedNames (203): its intent is broken for `FailureStepIds` (F2). It passes because its rows are hand-listed.
- MapCultureInvariance (169): its coverage no longer includes the moved recipe (F6).
- PersistenceEffectCompiles, PutAndDeleteEndpointsCompile and OrdersPhraseCompiles: intent kept (F8).
- WebExtensionBoundary (142, D284) and TasksDemoGeneratedCsharp (176): sound (F8).
- DeclaredEffectDependencyCompiles, RecordUpdateFormCompiles, the MatchingCodebases and NativeCSharpBlockCompiles zero-warning scans, and EdgeCaseFixtureCorpus: no assertion is weakened by the new file; their compile-and-warning bars are what R165-2 had to satisfy.

## What I could not check
- No dotnet run, so F1 (and F2's CS0101) are by reading the emitter, not by a compile.
- I did not measure thread-pool behaviour under load (F7).
- I did not read the d283 and d284 ballots.
- I did not reread the full golden byte by byte; I relied on the facilitator's green proof 3 for its equality.
