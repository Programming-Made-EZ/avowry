SIGN-OFF

# Card 169 acceptance sign-off (sign-off seat, agy gemini-3.8-flash-medium, context ctx-169s; branch card-169 at 2041bb0; adopted by Decider D280; no repo file changed)

All three acceptance criteria pass on the repository files and are supported by tests that fail on partial or incorrect work. No repository file was modified by this seat.

## Criterion-to-test table

| Criterion | Tests proving criterion | Characterisation or true red | Verification evidence |
| --- | --- | --- | --- |
| **169-1** [culture]<br>Given a non-invariant culture with different string ordering (for example tr-TR and sv-SE), When the map is written, Then the bytes equal those written under the invariant culture | `MapCultureInvariance.Map_bytes_for_one_source_equal_the_invariant_bytes_under_tr_TR_and_sv_SE` (`tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs`) | **Characterisation** (amendment 1 of D280) | Passes on base code. Proven by facilitator's mutation runs under Avowry's lock: mutants at `Program.cs:58` (`ThenBy(Name, CurrentCulture)`) and `Program.cs:59` (`ThenBy(Endpoint, CurrentCulture)`) each failed 169-1. Restored with empty `src` diff. |
| **169-2** [ordinal]<br>Given the map writer, When it sorts, Then every sort uses an ordinal comparison (a test fails if one does not) | `MapCultureInvariance.Every_sort_in_the_map_writer_uses_an_ordinal_comparison` (`tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs`) | **Characterisation** (amendment 1 of D280) | Passes on base code (`WriteMap` and helpers `Calls`, `FailureDeclaration`, `AvowFailureDeclaration`, `MethodName`, `Hash` contain only ordinal comparisons). Proven by 29 planted non-ordinal calls and planted helper method all reported, plus facilitator's mutants at `Program.cs:58` and `59` failing 169-2. |
| **169-3** [docs]<br>Given docs/TRACEABILITY.md, When the card is merged, Then it names the test | `MapCultureInvariance.Traceability_names_the_culture_invariance_tests` (`tests/Avowry.Acceptance.Tests/MapCultureInvariance.cs`) | **True red** | Failed at red commit `c0919c9` (`docs/TRACEABILITY.md` did not name the tests). Passed after green commit `65c456d` added the required bullet line under Executable evidence. |

## Facts from the facilitator's proof runs under Avowry's lock

Round 1 (00:11Z) and fix round 1 (00:31Z): base, all three tests pass after the green (169-3 failed before it); mutants at Program.cs lines 58 and 59 to StringComparer.CurrentCulture each fail 169-1 and 169-2; restored with an empty src diff.

## Named limits

1. **Review-2 new finding 1 (scan non-triviality check location)**: The assertion verifying `.OrderBy(` is present moved from checking the map-writer region specifically to checking the whole `Program.cs` file (`MapCultureInvariance.cs:131`). If sorting moved into a new helper not listed in `MapHelpers`, 169-2 would not scan it, but 169-1's full-order check across all records would still catch non-ordinal behavior. Accepted as non-blocking.
2. **Review-2 new finding 2 (closed helper list)**: `MapHelpers` is an explicit list (`Calls`, `FailureDeclaration`, `AvowFailureDeclaration`, `MethodName`, `Hash`). New map helpers must be manually added to this list. The planted helper proof on `MethodName` confirms region extraction operates as intended.
3. **Review-2 new finding 3 (false positive potential on Min/Max)**: Regex patterns `\.Min\s*\(` and `\.Max\s*\(` could match `Math.Min(` or `Math.Max(`. This can only fail loudly (false positive), never falsely pass. No such calls currently exist in the scanned code.
4. **sv-SE ASCII sorting**: For the ASCII names in the fixture, sv-SE sorts identically to the invariant culture. The active linguistic ordering difference is provided by tr-TR (using `Aib`/`AIc`, guarded by runtime precondition). sv-SE remains in the suite as an additional culture per R169-1.
5. **Shared laptop light work (no dotnet)**: In adherence to the rules (`no dotnet`, read-only repo), tests are not re-executed by this seat; test results and mutation proofs are taken from the facilitator's verified runs under Avowry's lock, and the full test suite runs at the gate (`dotnet test Phrase.sln --filter "Status!=Pending"`).
