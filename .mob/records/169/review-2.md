SIGN-OFF

# Card 169 QA review, round 2 (QA seat: claude opus, claude-opus-5-5, ctx-169q)

Range: `git diff 65c456d..HEAD` (2041bb0, fix round 1 red under ruling R169-1). It changes tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs only; `git diff 79ef629..HEAD -- src` is empty. I worked read-only and ran nothing. Line numbers are from the card-169 clone at 2041bb0. I take the facilitator's proof (00:31 to 00:32Z) as given:
- At base, all three tests pass, and the Aib/AIc precondition holds and the source parses.
- A mutant at Program.cs:58, and separately one at :59, each fails 169-1 and 169-2.
- Both mutants were restored with an empty src diff.

## Round-1 findings
- **B1 (scan range): fixed.** `MapWriterFindings` still scans `WriteMap` up to `Trace`, which keeps the `StepId` check. It also finds each of `Calls`, `FailureDeclaration`, `AvowFailureDeclaration`, `MethodName` and `Hash` by its declaration. Each helper's text runs to the next line that starts with `static` or `sealed`.
  - I ran the same declaration pattern with grep. It finds the real declarations: Program.cs:278, :279, :285, :288 and :289.
  - Two earlier lines mention a helper but cannot match. The `Hash(` call at :106 is on an indented line. The `FailureDeclaration(` inside `AvowFailureDeclaration(` has no word boundary before it.
  - The test fails if a helper is missing.
  - A planted `char.ToUpper` in `MethodName` is reported.
- **B2 (missing calls): fixed.** The list of checked methods now includes `Order`, `OrderDescending`, `Min`, `Max`, `MinBy`, `MaxBy`, `ToLookup` and `BinarySearch`.
  - LINQ `orderby` is flagged.
  - `ToUpper`/`ToLower`, on strings and chars, is flagged unless the call passes `CultureInfo.InvariantCulture`. The `...Invariant` methods cannot match.
  - There is one planted row each (o to aa).
  - A negative control (the `Order(Ordinal)`, `ToLower(InvariantCulture)`, `ToUpperInvariant` text) is not reported.
- **M3 (cultures do not differ from invariant): fixed.** The Aib/AIc pair has its own precondition: tr-TR must order it differently from the invariant culture. The facilitator's run confirms that it holds and parses. So with any culture-aware sort, the tr-TR bytes can now differ from the invariant bytes. Before, only the ordinal-order check could catch a mutant.
- **M4 (only one sort site detected): fixed.** 169-1 now checks the whole record list against an ordinal (Kind, Name, Endpoint, Line) re-sort. It also asserts that all four kinds are present, so the shared step and failure records are checked. The facilitator's mutant at :59 fails 169-1, as expected.
- **M6 (harness comment): addressed.** The comment now says the CLI runs against the test project's copies of the compiler assemblies.

Nothing that round 1 relied on was weakened. Every earlier planted row, negative control and precondition is still there.

## New findings
1. **MINOR: the check that the scan is not trivial moved from the writer to the whole file.** The old test asserted that `.OrderBy(` appears inside the writer region. The new one checks the whole of Program.cs (MapCultureInvariance.cs:131). If the sort moved into a new helper that is not on the `MapHelpers` list (MapCultureInvariance.cs:255), 169-2 would still pass with nothing scanned. 169-1's full-order check would still catch a culture-aware sort there, so I am not blocking on it. Suggested: assert `.OrderBy(` inside the `WriteMap..Trace` region again.
2. **MINOR: the helper list is closed.** A new map helper must be added to `MapHelpers` by hand. The planted-helper proof (MapCultureInvariance.cs:121-126) only asserts that something is reported, and only `MethodName` carries a plant. That is enough today, because the region extraction is the same for every helper. A one-line comment at `WriteMap` pointing to the list would help future authors.
3. **MINOR, false positives only:** `\.Min\s*\(` and `\.Max\s*\(` also match `Math.Min(` and `Math.Max(`. This can only fail loudly, never pass wrongly. No such call is in the scanned text today.

## What I could not check
- I ran nothing. The mutant results and the base pass are the facilitator's.
- I did not verify the ICU order of Aib/AIc myself. The precondition at MapCultureInvariance.cs:39-41 guards it at runtime.
- sv-SE still adds nothing over the invariant culture for these ASCII names. R169-1 accepts that ("sv-SE stays as a further culture").
- Whether a CRLF checkout changes the helper extraction. I believe it does not: the pattern excludes `\r\n`, and `(?m)^` matches after `\n`. The clone is LF.
