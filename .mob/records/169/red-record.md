# Card 169 red record (red seat: claude sonnet, claude-sonnet-5-5, ctx-169r)

Nothing was built or run (the heavy lock is held elsewhere); every expectation below is from reading the code. The facilitator runs `dotnet test --filter "Slice=169"` and the mutation proof.

File: `tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs` (class `MapCultureInvariance`, `Slice=169`, one `Criterion` trait per test).

## Tests by criterion
- 169-1 `Map_bytes_for_one_source_equal_the_invariant_bytes_under_tr_TR_and_sv_SE`
  Builds one source (orders-style fixture text, six endpoints Ab, AC, Ia, IB, Zy, ZX, so endpoint, step, avow and failure names tie and break on endpoint) under the invariant culture, tr-TR and sv-SE, and compares the map bytes. It first asserts the sample really sorts differently under each culture than by ordinal (a vacuous pass is impossible) and that the invariant map lists the endpoints in ordinal order (so a mutant that is culture-aware but equal across runs is still caught).
  How it runs the writer: the built `Avowry.Cli.dll` is loaded with `Assembly.LoadFrom` and its synthesised entry point (`Assembly.EntryPoint`) is invoked on the test thread with `build FILE -o OUT`, with `CurrentCulture` and `CurrentUICulture` set around the call and restored in `finally`. Why: .NET has no environment variable for the culture on Windows, so a child process cannot be forced into tr-TR; `WriteMap` is a local function of top-level statements (not callable); this route runs the real build path with no product seam. The culture covers the whole compile plus the map (stricter than the writer alone).
  It calls `PhraseCli.RunAsync("--version")` first so the CLI is built and the existing freshness check runs.
- 169-2 `Every_sort_in_the_map_writer_uses_an_ordinal_comparison`
  Source scan of Program.cs from `static void WriteMap(` to `static int Trace(` (the map helpers). Flags: OrderBy, OrderByDescending, ThenBy, ThenByDescending, Sort, Distinct, GroupBy, ToDictionary, ToHashSet, Compare, CompareTo, StartsWith, EndsWith, IndexOf, LastIndexOf, `new HashSet/Dictionary/Sorted*<...>(`, string.Compare, whose argument text lacks `StringComparer.Ordinal` or `StringComparison.Ordinal`; any `ToLower()`/`ToUpper()`, any other `StringComparer.*` or `StringComparison.*` (OrdinalIgnoreCase counts as non-ordinal), `Comparer.Default`. The one non-string key (`ThenBy(record => record.Line)`) is allow-listed by its exact text. Sixteen planted texts must each be reported (the planted row is its proof); an ordinal text must not be; the real region must be clean and non-trivial (finds `StepId` and `.OrderBy(`).
- 169-3 `Traceability_names_the_culture_invariance_tests`
  `docs/TRACEABILITY.md` must contain the two backticked names `` `MapCultureInvariance.Map_bytes_for_one_source_equal_the_invariant_bytes_under_tr_TR_and_sv_SE` `` and `` `MapCultureInvariance.Every_sort_in_the_map_writer_uses_an_ordinal_comparison` ``.

## Expected today
- 169-3: FAILS (true red): TRACEABILITY.md names neither test. The green adds a line to its "Executable evidence" list naming both.
- 169-2: PASSES (characterisation): every call in the region is ordinal today (lines 57 to 60, 63, 87, 109).
- 169-1: PASSES (characterisation) if the compile and map are culture-free today. No product seam is missing. If it fails, that is a real finding (the compile pipeline is culture-sensitive under tr-TR or sv-SE) and a true red for the green to fix with the failing culture named in the message.
- Green: no seam needed; docs line only (possibly nothing in code). The generated C# and orders hash 409bf3c6... are untouched.

## Mutation steps for the facilitator (under Avowry's own lock; restore afterwards, empty diff at hand-off)
1. Baseline: `dotnet test Phrase.sln --filter "Slice=169"` (expect 169-3 red only).
2. Edit `src/Avowry.Cli/Program.cs` line 58: change `.ThenBy(record => record.Name, StringComparer.Ordinal)` to `.ThenBy(record => record.Name, StringComparer.CurrentCulture)`.
   (Line 57, the Kind sort, is a weaker target: kinds avow, endpoint, failure, step order the same under every culture. Line 59, Endpoint, is a good second mutant.)
3. `dotnet build src/Avowry.Cli` (needed: the acceptance project does not reference the CLI, the freshness check fails stale otherwise, and 169-1 loads that dll).
4. `dotnet test Phrase.sln --filter "Slice=169"`: expect 169-2 FAILS (reports the `.ThenBy(... StringComparer.CurrentCulture)` call) and 169-1 FAILS (invariant map not in ordinal order; tr-TR and sv-SE bytes differ). 169-3 still red.
5. Restore: `git checkout -- src/Avowry.Cli/Program.cs`, `dotnet build src/Avowry.Cli`, `git diff --stat -- src` must be empty.
Optional: repeat with line 59 and with `Distinct(StringComparer.Ordinal)` on line 87 changed to `Distinct()` (169-2 only: Distinct on strings with the default comparer is ordinal in behaviour, so 169-1 cannot see it; the scan does).

## Ambiguities
- Names in the source are ASCII only (the lexer takes ASCII letters for names), so the slice's `ı`, `İ`, `å`, `ä`, `ö` cannot appear in endpoint names. The sample uses case and letter pairs that cultures order differently from ordinal (Ab/AC, Ia/IB, Zy/ZX); the precondition asserts this per culture. Non-ASCII inside avow templates or strings was not tried (unrun, not known to compile).
- On a Linux box without ICU (invariant globalization) tr-TR is unavailable or sorts ordinally; the precondition then fails loudly rather than passing.
- Interpolated ints in `StepId` and the Avow record (`{line}`) use the current culture's number format; line numbers are positive so no effect today, out of scope for sorts.
- The test depends on the built CLI dll; configuration is picked from the test output path (Debug or Release), as `PhraseCli` does.

## Fix round 1 red (R169-1)

Only `tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs` changed (nothing run, nothing committed). Nothing was weakened: every earlier planted row, precondition and assertion stays.

What changed
- B1 (169-2 scope): the scan is now `MapWriterFindings(program)`: WriteMap up to Trace (as before) plus the declarations of `Calls`, `FailureDeclaration`, `AvowFailureDeclaration`, `MethodName` and `Hash`, each found by a `^static ... Name(` match up to the next `static`/`sealed` line, never by line number. A missing declaration fails the test. A planted program text with `char.ToUpper` inside a `MethodName` placed after `Trace` must be reported.
- B2: added `Order`, `OrderDescending`, `Min`, `Max`, `MinBy`, `MaxBy`, `ToLookup`, `BinarySearch` to the checked calls (a call passes only if its arguments name `StringComparer.Ordinal` or `StringComparison.Ordinal`), LINQ `orderby` (always reported), and `ToUpper`/`ToLower` (string and char) unless the arguments name `CultureInfo.InvariantCulture` (`ToUpperInvariant` and `ToLowerInvariant` never match). One planted row each (13 new rows); two new ordinal/invariant texts must stay unreported. Today's region and helpers contain none of these calls (grepped).
- M3/M4 (169-1): the invariant map's whole record list (all four kinds, asserted present) must equal itself sorted by ordinal Kind, Name, Endpoint, then Line; the endpoint-name check stays. Endpoints are now `Ab, AC, Ia, IB, Zy, ZX, Aib, AIc`.
- The Ib/ia pair: an endpoint name must start with an upper-case ASCII letter, so `ia` cannot be one. `Aib`/`AIc` has the same property (the Turkish dotless I is a letter before the dotted i): `AIc` before `Aib` under tr-TR, `Aib` before `AIc` under the invariant culture (as the QA review suggested). A precondition asserts that its order differs between the invariant culture and tr-TR. Unverified: that the parser accepts these names (names allow any ASCII letters and digits per the lexer). If it does not, the facilitator should pick another upper-case pair with the same dotted/dotless shape.
- Also fixed the wrong comment about `Zy`/`ZX` and the overstated "real build path" comment (grok MINOR, QA 6).

Expected results
- Today (no mutant): 169-1, 169-2 and 169-3 pass.
- Mutant at line 58 (`ThenBy(Name, CurrentCulture)`) after `dotnet build src/Avowry.Cli`: 169-1 fails (the invariant run's records are not in ordinal order: endpoint, avow, failure and step names, and the endpoint-name check) and 169-2 fails (the `ThenBy` call). 169-3 passes.
- Mutant at line 59 (`ThenBy(Endpoint, CurrentCulture)`): 169-1 now fails at the whole-record-order assertion (step and failure records tie on name across endpoints and break the tie by endpoint: under the invariant culture `Ab` precedes `AC`, ordinal gives `AC` first); 169-2 fails. In round 0 this mutant passed 169-1.
- Mutant at line 57 (Kind) or `Distinct()` at line 87: only 169-2 can see them, as before.
- Mutant `char.ToUpper` in `MethodName` (line 288): 169-2 fails (new helper scan); 169-1 cannot see it with this fixture.
