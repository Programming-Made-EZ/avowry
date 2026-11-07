# Card 324 cycle 2 characterisation record

Characterisation tests for criterion 2, written against the unchanged tree (no `src/` edits). They pass on the pre-change diagnostics on purpose.

## Files written

- `tests/Avowry.Compiler.Tests/Diagnostics/sources.txt` (2701 bytes, sha256 prefix `78745654aab01d2f`) and `baseline.txt` (1913 bytes, sha256 prefix `07e6c71ff84a543b`): byte copies of `.mob/records/324/slice-files/sources.txt` and `baseline.txt`.
- `tests/Avowry.Compiler.Tests/Diagnostics/TriggerSources.cs`: walks up from `AppContext.BaseDirectory` to `Avowry.sln`, loads those two files (CRLF normalised to LF), parses the 19 `### id` sources and baseline rows, and exposes `ToOld` / `ToNew` from the 13 fragment pairs (new fragments, longest first, `Regex.Escape`).
- `tests/Avowry.Compiler.Tests/Diagnostics/DiagnosticCharacterisationTests.cs`: Theory over the 19 ids (`Diagnostic_codes_positions_and_tails_are_unchanged`), Fact `The_baseline_covers_every_source_and_each_source_triggers`, Fact `Normalisation_does_not_hide_wording_drift`. Trait `Criterion` = `2`.
- `tests/Avowry.Acceptance.Tests/CliKeepsDiagnosticShape.cs`: Theory `The_CLI_exits_1_with_the_diagnostic_line_and_0_when_clean` (M13 PH001, M08 PH203, clean). Output folder via `TempWorkspace`.
- `.mob/records/324/baseline/`: `trigger-sources.txt`, `pre-change-diagnostics.txt` (byte copies), `SHA.txt`, `eol.txt`, `dotnet-counts.txt`, `cli-pre.txt`, this record.

## Sanity run (new tests only)

Command: `dotnet test tests/Avowry.Compiler.Tests --filter "FullyQualifiedName~DiagnosticCharacterisation" --nologo`

```
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 205 ms - Avowry.Compiler.Tests.dll (net8.0)
```

Command: `dotnet test tests/Avowry.Acceptance.Tests --filter "FullyQualifiedName~CliKeepsDiagnosticShape" --nologo`

```
Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 5 s - Avowry.Acceptance.Tests.dll (net8.0)
```

Every new test passed on the unchanged tree: 21 compiler (19 theory rows + 2 facts) and 3 acceptance.

## Listed counts

Command: `dotnet test tests/Avowry.Compiler.Tests --list-tests --nologo`

Listed 305 (architect's pre-change listing was 284; 284 + 21 = 305).

Command: `dotnet test tests/Avowry.Acceptance.Tests --list-tests --nologo`

Listed 320 (architect's pre-change listing was 317; 317 + 3 = 320).

Command: `dotnet test tests/Avowry.Acceptance.Tests --list-tests --nologo --filter Status!=Pending`

Listed 319 (architect's pre-change filtered listing was 316; 316 + 3 = 319).

## CLI pre (three characterisation rows)

See `cli-pre.txt`. M13 exit 1, stderr `avowry: Compilation failed:` then `PH001 (11:5) No phrase matches this step. Closest: "do nothing".` M08 exit 1, PH203. Clean exit 0, empty stderr.

## EOL

`git ls-files --eol` for Resolver.cs and Parser.cs: both `i/lf    w/lf`. New files written with LF.
