SIGN-OFF (facilitator's reading: the reply finds all three criteria met and one MINOR; it gave no SIGN-OFF token)

# Card 171 cross-family review (grok grok-4.6, ctx-171x, normal mode in a throwaway clone; saved by the facilitator from the seat's reply)

Criterion 1 is met. The CLI map writer no longer encodes C# layout: `Program.cs` has no `Handler` / `Handle` / `Avows` / `Phrases` string literals and no copy of the method-name rule (`MethodName`, `IsHole`, `Parts`, `char.ToUpper`). Avow records take `csMethod` from `output.AvowMethods[phrase.Key]` (`Program.cs:96`); endpoint, step, and failure records take it from `output.EndpointMethods[endpoint.Name]` (`Program.cs:128`). File paths were already adapter-returned. The two maps sit on `CompilationOutput` (`CompilationOutput.cs:9-10`) beside `EndpointFiles` / `AvowFiles`; the C# adapter fills them (`CSharpProvider.cs:42, 51, 56-59, 66-67`) using `EndpointHandlerName` and its existing `MethodName` (`CSharpProvider.cs:963-964`). Compiler does not reference the CLI. A second adapter can fill the same init properties from `ITargetAdapter.Emit` (`ITargetAdapter.cs:15`) with its own names; the CLI would not need a map-writer change. (`Program.cs` still constructs `CSharpProvider` and accepts `--target csharp`; that is pre-existing adapter selection, not generated layout.)

Criterion 2 is met. `Program.cs` has one statement per line; nested `foreach` headers are braced (`Program.cs:81-86`, `98-114`); the map sort is four chained calls then a separate `ToList()` (`Program.cs:61-66`); no null-forgiving `!`; no line over 120 (longest 120 at `Program.cs:67` and `Program.cs:128`). Reachable reformat is behaviour-preserving (same `Usage` text, same version write, same LINQ order, same `Hash` bytes). One unreachable path changed: see MINOR.

Criterion 3 and R171-1 are met. Green touch of `CSharpProvider.cs` is only the map fill in `Emit`; generated-file emit bodies are untouched. Facilitator proof: Slice 142 pin `19fe6d71...` (`WebExtensionBoundary.cs:18`) and Slice 176 demo golden stayed green. Avow `csMethod` is the intended fix: old CLI skipped holes (`Avows.EnsureIsNotEmpty`); adapter `MethodName` joins every part (`CSharpProvider.cs:963-964` → `Avows.EnsureItemsIsNotEmpty`, matching `reference/csharp/Orders.Api/Avows.cs:14`). Endpoint/step/failure `csMethod` values stay `{Name}Handler.Handle`. Other map fields (`stepId`, paths, hashes, order, schema) are unchanged; only hole-avow `csMethod` strings (and therefore those map bytes) change, as R171-1 allows.

Card 169 is not weakened. Red dropped `MethodName` from `MapHelpers` (`MapCultureInvariance.cs:253`) because this card moves that helper out of `Program.cs`. 169-1 and 169-3 are untouched. 169-2 still scans the WriteMap-to-Trace region and still requires each remaining helper (`Calls`, `FailureDeclaration`, `AvowFailureDeclaration`, `Hash`). Planted proofs now dirty each remaining helper and include a clean program (`MapCultureInvariance.cs:121-124`, `257-261`). Culture dependence in adapter names remains visible to 169-1 (full map bytes under `tr-TR` / `sv-SE`).

Findings

MINOR: `CheckHash` (`Program.cs:268-270`) treats a null `GetDirectoryName` as a hash mismatch (`return true`) instead of throwing into `Trace`’s catch (old `!` path). For a map path that already passed `File.Exists`, `GetDirectoryName(GetFullPath(...))` is not null, so the branch is not reachable in ordinary use.

Could not check

Did not run `dotnet` (forbidden). Did not re-hash generated orders or demo output; byte-identity rests on the C# emit diff plus the facilitator’s green (Slice 142, 176, and all 11 tests). Did not execute the 171-2 scanner; style was checked by reading `Program.cs` and measuring line lengths.
