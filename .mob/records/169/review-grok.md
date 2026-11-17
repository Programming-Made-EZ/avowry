SIGN-OFF (facilitator's reading: the reply says the card meets the three criteria and lists four MINOR findings; it gave no SIGN-OFF token)

# Card 169 cross-family review (grok grok-4.6, ctx-169x2; saved by the facilitator from the seat's reply)

Card 169 (range `79ef629..HEAD`: red `c0919c9`, green `65c456d`) meets the three criteria. The green is the TRACEABILITY line only. Facilitator proof: 169-1 and 169-2 passed, 169-3 failed; `Program.cs:58` swapped to `StringComparer.CurrentCulture` made all three fail; `src` restored empty.

**169-1.** `MapCultureInvariance.Map_bytes_for_one_source_equal_the_invariant_bytes_under_tr_TR_and_sv_SE` (`tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs:26`) builds one source, invokes the built CLI entry point in-process with `CurrentCulture`/`CurrentUICulture` set (`:124`–`:143`), and compares map **bytes** under invariant, `tr-TR`, and `sv-SE` (`:41`–`:52`). The sample is `Ab, AC, Ia, IB, Zy, ZX` (`:17`). Measured with `StringComparer.Create` / `List.Sort` (no `dotnet`):

- Ordinal: `AC, Ab, IB, Ia, ZX, Zy`
- `tr-TR` and `sv-SE`: `Ab, AC, Ia, IB, ZX, Zy`
- `Ab`/`AC` and `Ia`/`IB` reverse vs ordinal; `Zy`/`ZX` keep `ZX` then `Zy` in both

The array-level precondition (`:32`–`:33`) therefore holds for both cultures. The comment at `:14`–`:16` that `Zy`/`ZX` is a differing pair is wrong; the assert is on the full array.

Culture-sensitive sort catch:

- `Program.cs:58` `ThenBy(Name)`: yes (facilitator mutant; invariant map must also be ordinal at `:50`)
- `Program.cs:59` `ThenBy(Endpoint)`: yes (step/failure records share `Name` and break ties on endpoint)
- `Program.cs:57` `OrderBy(Kind)`: no (`avow, endpoint, failure, step` is the same under ordinal, `tr-TR`, and `sv-SE`)
- `Program.cs:60` `ThenBy(Line)`: integer comparer
- `Program.cs:87` `Distinct`: equality, not order; `EmptyOrder`/`InvalidQuantity` stay distinct under either comparer

**169-2.** `Every_sort_in_the_map_writer_uses_an_ordinal_comparison` (`:58`) scans `static void WriteMap(` through `static int Trace(` (`:91`–`:97`). That window holds every current string sort/distinct: `Program.cs:57`–`:60`, `:87`. Planted non-ordinal rows (`:61`–`:78`) include missing comparers, `CurrentCulture`, `InvariantCulture`, `OrdinalIgnoreCase`, `ToLower()`, `Comparer.Default`, and comparer-less `Distinct`/`Sort`/`HashSet`/`Dictionary`. A comparer in a **variable** (`OrderBy(..., cmp)`) is reported: the call text has no `StringComparer.Ordinal`. A **lambda** without a comparer (`ThenBy(record => record.Name)`) is planted and reported.

The scanner can miss a sort **outside** that window: `MethodName` and `Hash` sit at `Program.cs:288`–`:289` (after `Trace`), as would LINQ `orderby` query syntax. The `ThenBy(record => record.Line)` allow-list (`:226`) is the exact text, so a later string `Line` with that same call would be skipped. `169-2` still covers every sort the map writer performs today.

**Other culture use in the map.** No dates. JSON is `System.Text.Json` (`Program.cs:61`–`:64`), invariant numbers and names. Hash input (`:110`) is `kind + "\n" + interpolated line + "\n" + ordinal-normalized name`; `$"{line}"` under `tr-TR` and `sv-SE` stayed `42`, `1000`, `1234567`. Casing in the writer is `ToLowerInvariant` (`:111`, `:289`) and `ToUpperInvariant` (`:288`). Hex has no `I`, so Turkish `I` casing would not move `stepId`/`csHash` on this fixture. `MethodName` is outside the scan; this fixture’s avow parts start with `e`/`t`, so a `char.ToUpper` mutant would not change bytes here.

**169-3 / green.** Green adds `docs/TRACEABILITY.md:23` with both backticked names the test builds at `:109`–`:117`. Product code, tests, and generated C# are untouched. That is the docs-only green the slice and red record asked for; nothing in 169-1 or 169-2 is weakened.

MINOR `tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs:14`–`:17`: `Zy`/`ZX` do not change order under `tr-TR` or `sv-SE`; `Ab`/`AC` and `Ia`/`IB` do.

MINOR `tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs:41`–`:52` with `src/Avowry.Cli/Program.cs:57` and `:87`: byte equality does not see a culture-sensitive **Kind** sort or a `Distinct()` comparer drop; `169-2` does.

MINOR `tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs:91`–`:97`: a new string sort in helpers after `Trace` (`Program.cs:288`–`:289` and below), or LINQ `orderby`, would not be scanned.

MINOR `tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs:226`: allow-list is syntactic (`ThenBy(record => record.Line)`).

**Could not check.** Did not run `dotnet` or the CLI. Pass/fail and the line-58 mutant are the facilitator’s proof only. Did not check invariant-globalization / no-ICU hosts (red record: the precondition fails loudly). Did not recompute the recorded orders hash `409bf3c6...`; the range diff has no generated C#.
