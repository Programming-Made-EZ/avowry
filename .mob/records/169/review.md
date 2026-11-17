CHANGES

# Card 169 QA review (QA seat: claude opus, claude-opus-5-5, ctx-169q)

Range: `git diff 79ef629..HEAD` on card-169 (red c0919c9: the test file only; green 65c456d: one line in docs/TRACEABILITY.md). Read-only; nothing built or run. Line numbers are from the card-169 clone. The facilitator's proof run (base: 169-1 and 169-2 pass, 169-3 fails; Program.cs:58 set to `StringComparer.CurrentCulture`: all three fail; restored with an empty src diff) is taken as given.

## Findings

1. BLOCKING (169-2): the scan stops short of the map helpers that the slice names. The slice, binding on the red, says to scan "from `WriteMap` to the end of the map helpers". The test scans from `static void WriteMap(` to `static int Trace(` (MapCultureInvariance.cs:91-94), so Program.cs:107-112 (`StepId`) is covered but Program.cs:278-289 is not. Those lines hold the helpers that feed the map: `Calls`, `FailureDeclaration`, `AvowFailureDeclaration`, `MethodName` (it builds `csMethod`, Program.cs:84, and uses `char.ToUpperInvariant`) and `Hash` (it builds `csHash`, Program.cs:84 and :106). A non-ordinal sort, `Distinct` or `ToUpper` added there is not reported. A `char.ToUpper` in `MethodName` is the classic tr-TR bug. Fix: scan Program.cs:278-289 too, for example a second region from `static bool Calls(` to `static void Error(`, and assert that `MethodName` and `Hash` are in it.
2. BLOCKING (169-2): the method list misses culture-sensitive calls that this codebase already uses. `CheckedMethods` (MapCultureInvariance.cs:219-223) has no `Order`, `OrderDescending`, `Min`, `Max`, `MinBy`, `MaxBy`, `ToLookup` or `BinarySearch`. With no comparer, `names.Order()` and `names.Max()` use `Comparer<string>.Default`, which is culture-sensitive, and the scan does not report them. `.Order(StringComparer.Ordinal)` is this repo's own idiom (Resolver.cs:365, :390, :419), so `.Order()` is the likeliest call to slip in. Fix: add these names and plant one text each, for example `var o = names.Order();`.
3. MINOR (169-1): the culture comparison is weaker than the red record claims. The precondition (MapCultureInvariance.cs:29-34) only proves that each culture orders the sample differently from ordinal. It does not prove that tr-TR or sv-SE order it differently from the invariant culture. They do not, by my reading. Linguistically, Ab<AC, Ia<IB and ZX<Zy under all three cultures. The Turkish dotted/dotless I never matters, because both I-names start with a capital I and their second letters are a and B. So a culture-aware mutant gives equal bytes under all three cultures, and the byte check at :51-52 does not catch it. Only the ordinal-order check at :50 catches it. The facilitator's run fits this: :50 throws before :52 runs, so the red record's step-4 claim that "tr-TR and sv-SE bytes differ" was never observed. Suggested: add a pair that tr-TR orders differently from invariant. Endpoint names must be ASCII and start upper-case (Lexer.cs:201-202, 262), so for example `Aib`/`AIc`. That pair is unverified: I did not check that the parser accepts it.
4. MINOR (169-1): it detects a culture-sensitive sort at only one site. Line 50 checks the order of the `endpoint` records only, and those are decided by Program.cs:58 (Name). A `CurrentCulture` mutant at :59 (Endpoint) reorders the step and failure records, which tie on name across the six endpoints, but the map is the same under all three cultures (finding 3) and :50 does not look at those records. So 169-1 passes and only 169-2 catches it. The other sites cannot be seen in the output: :57 (kinds `avow`/`endpoint`/`failure`/`step` order the same under every culture), :60 (int), and :87 (`Distinct` equality is ordinal by default). Cheap fix: at :50, assert that the whole invariant record list is in ordinal (kind, name, endpoint) order, not just the endpoint names.
5. MINOR (169-2, robustness): a comparer passed in a variable or lambda is safe. `OrderBy(x, cmp)` has no `Ordinal` text, so it is reported. `Comparer<string>.Create((a, b) => a.CompareTo(b))` or `string.Compare` inside a lambda is reported. Any `StringComparer.X` other than `Ordinal` is reported (MapCultureInvariance.cs:244). The scan can still be fooled, but only by contrived text: `Ordinal` mentioned anywhere inside the argument text (a comment, or a helper `Pick(StringComparer.Ordinal)` that returns something else); a char literal `'('` or `')'` inside a checked call, because `ArgumentsEnd` tracks only `"` strings (:249-266); or the allow-listed `ThenBy(record => record.Line)` (:227) if `Line` ever became a string (it is an `int` at Program.cs:314). Not worth blocking.
6. MINOR (169-1, harness): `Assembly.LoadFrom` (MapCultureInvariance.cs:151) loads the CLI into the default context, where the test project has already loaded `Avowry.Compiler` and `Avowry.Adapters.CSharp` (Avowry.Acceptance.Tests.csproj project references). The CLI therefore runs against the test's copies, not its own bin copies. Both come from the same commit in a solution build, so the result holds, but the comment's "the real build path" is slightly overstated.

## Check 3: other culture-sensitive inputs in the map
None found in the map writer:
- JSON: System.Text.Json is culture-invariant.
- Strings: `Replace(..., StringComparison.Ordinal)` (Program.cs:63, :109, :289).
- Hex: `Convert.ToHexString` plus `ToLowerInvariant` (:111, :289).
- `BuildId`: `ToLowerInvariant` (AvowryCompiler.cs:34).
- `MethodName`: `char.ToUpperInvariant` (:288).
- No dates or decimals.

The only culture-dependent formatting is the `int` interpolation in `StepId` (:110: `{line}`). .NET always writes ASCII digits, and line numbers are positive, so `NegativeSign` never applies. It is safe today and out of scope. The compiler side looks invariant as well:
- Parsing uses `InvariantCulture` (Resolver.cs:657, Parser.cs:638).
- Name casing is `*Invariant` throughout CSharpProvider.cs.
- String sorts are ordinal (ExtensionKeywords.cs:106, Resolver.cs:365, :390).

## Check 4: green
It satisfies 169-3 and weakens nothing. 65c456d adds one bullet to docs/TRACEABILITY.md under "Executable evidence". The bullet contains both strings that 169-3 builds (MapCultureInvariance.cs:109-117, ordinal `Assert.Contains`). The green touches no test and no src file (the diff stat is 2 files: the test and the doc), so the generated C# and the orders hash 409bf3c6... cannot have changed. 169-3 was a true red at c0919c9.

## What I could not check
- I ran nothing (no dotnet, per the rules). The claims about linguistic order in findings 3 and 4 come from reading CLDR/ICU behaviour, not from a run. The cheapest proof is a second mutant at Program.cs:59: by my reading, 169-1 would pass and 169-2 would fail.
- Whether `Aib`/`AIc`-style endpoint names compile, and whether sv-SE orders any ASCII-only pair differently from invariant (v/w is version-dependent).
- Whether ICU or NLS is active on the CI and laptop runners. The precondition at :33 does fail loudly under invariant globalization.
- The cross-family review (review-grok.md): not read, as instructed.
