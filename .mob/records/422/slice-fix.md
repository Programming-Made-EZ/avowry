# Card 422 fix slice, review round 1 (R1-1, R1-2)

Architect (claude opus = claude-opus-5-5, D268, ctx-1b), 2026-11-15. Read-only on main and on wt-422 (card-422 at 232be69). Probe: `<scratchpad>` + `probe422fix`:
- `ref_fix.py`: the REFERENCE over the committed Python helpers, with 20 `M("...")` switches. `M("today")` replays the committed F5, F6 and `RestoreLockedOrFail` flow, as the review's `networked.py` does.
- `make_after.py` writes `after/`: the two C# files with the call changes below. It is an unbuilt draft for the wiring scan.
- `gen_eval.py` writes and measures the rows, and runs the 27 mutants (`gen_eval.out`).
- `measure_existing.py` (`measure_existing.out`).
- `test422fix_draft.py`: the Python rows.

The rows are in `<scratchpad>` + `corpusfix422.txt`: 83 rows, corpus.txt's columns and escapes, with the checks named in its header. Paths below are split strings.

## 1. Pure functions (`"tests", "Avowry.Acceptance.Tests", "RestoredLicenceChecks.cs"`)

```
internal sealed record RestoredPackage(string Id, string Version, bool FolderFound, string NuspecState, PackageEvidence? Evidence, bool RequiresAcceptance);
public static RestoredPackage RestoredPackageFrom(string id, string version, bool folderFound, string? nuspecXml, IReadOnlyList<string> relativePaths, Func<string, string> readHead)
public static IReadOnlyList<string> F6Failures(IReadOnlyList<string[]> pageRows, IReadOnlyList<RestoredPackage> restored)
public static IReadOnlyList<string> F6AcceptanceFailures(string restoredBody, IReadOnlyList<string[]> pageRows, IReadOnlyList<RestoredPackage> restored)
public static int RestoredCount(string? assetsJson)
public static string ClassifyRestore(int exitCode, bool timedOut, string text, int restoredCount) => RestoreOutcome(exitCode, timedOut, text, restoredCount);
```

**RestoredPackageFrom.** `NuspecState` is one of three values:
- `missing`, when the XML is null;
- `broken`, on an XmlException;
- `ok`, otherwise.

When the state is `ok`, `Evidence` holds:
- the expression: the first `license` element (any namespace) whose `type` equals `expression` in any case, trimmed; otherwise null;
- the URL: the first `licenseUrl`, trimmed; otherwise null;
- the licence files: each relative path whose last segment (split on `/` and `\`), lower-cased, starts with `license`, `licence` or `copying`, mapped to `readHead(path)`.

`RequiresAcceptance` is true when the first `requireLicenseAcceptance`, trimmed, equals `true` in any case. These are today's F6 reading rules, now with the file heads.

**F6Failures**, in this order:
1. The page set against the restored set. The message is today's `locked restore vs page missing: <a>; extra: <b>`, ordered by id; move `DescribeSet` here.
2. For each page row with a restored package:
   - `<id> <v>: package folder missing after restore`;
   - else `nuspec missing after restore` or `nuspec does not parse`;
   - else `licence source is not one of the three labels`;
   - else the row and its evidence join `checkedRows` and `evidence` (keyed `id version`).
3. `failures.AddRange(LicenceValueFailures(checkedRows, evidence))`. This line is the [E] wiring.

**F6AcceptanceFailures** is today's acceptance block, moved without change. `want` is the page rows whose package requires acceptance. `named` is every restored package whose id and version both appear (Ordinal) on a body line containing `acceptance` (any case). When they differ, it returns `acceptance set missing: ...; extra: ...`.

**RestoredCount** counts the `libraries` entries whose `type` is `package`. It gives 0 for null or for no `libraries`. Malformed JSON throws, which fails closed.

**ClassifyRestore** delegates to `RestoreOutcome` and nothing else (the [O] wiring).

## 2. Call changes (`"tests", "Avowry.Acceptance.Tests", "RestoredPackageLicencesLockedRestore.cs"`)

**F5.** Lines 23 to 41 are unchanged. The planted step becomes:
```
var plantedProject = Path.Combine(drift.Root, "Orders.Api", "Orders.Api.csproj");
var planted = await RestoreLockedAsync(plantedProject);
var plantedText = PublicText(planted.Stdout + planted.Stderr);
var plantedOutcome = RestoredLicenceChecks.ClassifyRestore(planted.ExitCode, planted.TimedOut, plantedText, RestoredLicenceChecks.RestoredCount(ReadAssets(plantedProject)));
if (plantedOutcome == "network-missing") Assert.Fail("NETWORK MISSING " + FirstLine(plantedText));
Assert.True(plantedOutcome == "mismatch", "planted restore: " + plantedOutcome + ": " + plantedText);
```
These lines are removed: `Assert.False(planted.TimedOut ...)`, `if (NetworkMissing(...))`, `Assert.NotEqual(0, ...)` and `Assert.Contains("NU1004", ...)`.

**RestoreLockedOrFail**:
```
var result = await RestoreLockedAsync(projectPath);
var text = PublicText(result.Stdout + result.Stderr);
var outcome = RestoredLicenceChecks.ClassifyRestore(result.ExitCode, result.TimedOut, text, RestoredLicenceChecks.RestoredCount(ReadAssets(projectPath)));
if (outcome == "network-missing") Assert.Fail("NETWORK MISSING " + FirstLine(text));
Assert.True(outcome == "pass", "locked restore: " + outcome + ": " + text);
```

**F6.** Everything up to and including `using var assets = JsonDocument.Parse(...)` is unchanged. After that:
```
var restored = ReadRestoredPackages(assets.RootElement);
var pageText = (await File.ReadAllTextAsync(RepoPaths.Find("docs", "release", "pre-publication-checks.md"))).Replace("\r\n", "\n", StringComparison.Ordinal);
var pageRows = ReadRestoredRows(pageText).Select(r => new[] { r.Id, r.Version, r.Kind, r.Licence, r.LicenceSource, r.WhereUsed, r.Review }).ToList();
var failures = RestoredLicenceChecks.F6Failures(pageRows, restored).Concat(RestoredLicenceChecks.F6AcceptanceFailures(RestoredBody(pageText), pageRows, restored)).ToList();
Assert.True(failures.Count == 0, string.Join("\n", failures));
```
F6's loop, the set assert and the acceptance block are removed.

**New thin shell helpers:**
- `string? ReadAssets(string projectPath)` reads the project's `obj` folder `project.assets.json`, or returns null. `TempWorkspace.CopyFrom` already skips `obj`, so the file is never stale.
- `List<RestoredPackage> ReadRestoredPackages(JsonElement assets)`: for each `AssetsPackages(assets)` package, it calls `FindPackageDir`, then the top-level `*.nuspec` text, then every file's path relative to the folder, then `RestoredLicenceChecks.RestoredPackageFrom(id, version, dir is not null, nuspecText, paths, rel => ReadHead(Path.Combine(dir, rel)))`.
- `string ReadHead(string path)` returns the first 600 characters (StreamReader, BOM-aware).

**Deleted:** `NetworkMissing`, `LoadNuspec`, `LicenseExpression`, `LicenseUrl`, `RequiresAcceptance`, `HasLicenceFile`, and the `System.Xml.Linq` using if nothing else needs it. `RestoreOutcome` and the Theories `Licence_value_rows_422` and `Restore_outcome_rows_422` stay as they are.

## 3. The log change (R1-2, `"tests", "Avowry.Acceptance.Tests", "RestoredPackageLicences.cs"`)

`AssertPythonTestsPass` becomes an instance method, built from two parts:
- `RunPython(string[] fullNames)`, the existing process start;
- `private void Report(ProcessResult result) { output.WriteLine(result.Stdout + result.Stderr); Assert.True(result.ExitCode == 0 && !result.TimedOut, result.Stdout + result.Stderr); }`.

The log is written before the assert, so a failing run logs too. `AssertPython422` uses `Report` as well.

**The offline proof:**
- The Theory `Log_rows_422fix` runs FL-pass-note, FL-pass-runner, FL-pass-crlf, FL-fail and FL-timeout. Each builds `new RestoredPackageLicences(recorder)`, calls `Report(new ProcessResult(...))`, and checks whether it threw and what the recorder holds. The recorder is a `RecordingOutput : ITestOutputHelper` with a StringBuilder.
- The Fact FL-e2e-L5 runs the real `AssertPythonTestsPass("test_L5_...")` through the recorder and needs `Ran 1 test` in the log. Today nothing is written, so it fails.
- The Python half, class `Test422Fix`:
  - FL-py-L5-shallow sets `is_shallow` to true; test_L5 passes and prints the skip note to stdout.
  - FL-py-L5-deep prints nothing.
  - The C# side runs them through `AssertPython422("Test422Fix", ...)`.
- So the shallow-checkout note reaches the xUnit log on a passing run.

## 4. New rows (`corpusfix422.txt`) and where they live

| Rows | Check | Lives in |
|---|---|---|
| FW-f6-real, FW-E-swap, FW-E-xa-mit, FW-E-srm-apache, FW-E-expr-mit-apache, FW-E-expr-apache-mit, FW-ev-file-apache, FW-ev-file-apache-page-apache, FW-ev-file-none, FW-ev-file-unknown, FW-ev-url-mit, FW-ev-url-none, FW-both-url-source, FW-both-file-wins, FW-url-file-ignored, FW-file-source-no-file, FW-page-not-restored, FW-restored-not-on-page, FW-no-folder, FW-no-nuspec, FW-label, FW-acc-flag (22) | f6: F6Failures + F6AcceptanceFailures on the real page and the 30 packages (`Evidence422()` plus a new `Acceptance422` set of the 4 acceptance packages) | C# Theory `F6_rows_422fix`, Criterion 422-2 |
| FW-pkg-ns-url, -url-ws, -url-case, -url-http, -expr-single, -expr-ws, -type-case, -type-file, -broken, -acc-true-ws, -acc-false, -file-crlf, -file-nested, -file-backslash, -file-licence, -file-copying, -file-upper, -notice-apache, -dual (19) | f6 with the package rebuilt by RestoredPackageFrom from nuspec text and a path-to-text map | C# `F6_rows_422fix` |
| FW-O-planted-nu10041, -warm-warning, -partial, -planted-offline, -cold, -net-count30, -net-exit0-count0, -planted, -planted-crlf, -warm-crlf, -cold-crlf, -de-cold, -de-warm, -de-planted-offline, -de-partial, -fr-planted, -nu10045, -xnu1004, -nu1004-paren, -nu1004-lower-net, -nu1004-locked, -timeout-nu1004, -host-only (23) | classify (RestoreLockedOrFail needs pass) and classify-planted (F5 needs mismatch) through ClassifyRestore | C# Theory `Classify_rows_422fix`, 422-6 |
| FW-count-shape, -project-only, -none, -empty (4) | RestoredCount | C# Theory `Count_rows_422fix`, 422-6 |
| FW-src-f6-calls, -read-packages, -f6failures-value, -f5-classify, -locked-classify, -classify-outcome, -no-networkmissing (7) | the wiring scan over the two C# files (method bodies by signature, rules in `ref_fix.wire_failures`) | C# Theory `Wiring_rows_422fix`, 422-2/422-6 |
| FL-pass-note, FL-pass-runner, FL-pass-crlf, FL-fail, FL-timeout (5); FL-e2e-L5 (1) | log; log-e2e | C# (`RestoredPackageLicences.cs`), 422-6 |
| FL-py-L5-shallow, FL-py-L5-deep (2) | py-L5 | Python `Test422Fix`, run from C# |

The rows cover:
- each licence in both directions for each source;
- file evidence against URL evidence, a package with both, a package not restored and one not on the page;
- nuspec and file-name encodings;
- restore texts: CRLF, German and French prefixes, NU1004 inside longer codes, and the marker with 0, 12 and 30 restored and with exit 0.

## 5. Measured outcomes

- Reference on the after draft: 83 of 83 as expected (761 ms, including one real python run of test_L5).
- **TODAY on 232be69: 39 of 83 rows fail.**
  - f6: 10 rows. The networked F6 passes FW-E-swap, FW-E-xa-mit, FW-E-srm-apache, FW-ev-file-apache, FW-ev-file-unknown, FW-ev-url-mit, FW-both-url-source, FW-pkg-url-case, FW-pkg-url-http and FW-pkg-notice-apache, because it never calls `LicenceValueFailures`.
  - classify: 12 rows:
    - FW-O-planted-nu10041, FW-O-nu10045 and FW-O-xnu1004 are mismatch today (substring), where they should fail;
    - FW-O-warm-warning, FW-O-warm-crlf and FW-O-net-exit0-count0 are network-missing today, where they should pass;
    - FW-O-partial, FW-O-net-count30 and FW-O-de-partial are network-missing today, where they should fail;
    - FW-O-planted-offline, FW-O-de-planted-offline and FW-O-nu1004-lower-net are network-missing today, where they should be mismatch.
  - The 4 count rows and the 7 wiring rows: the functions do not exist and the shells are unwired.
  - 6 log rows: the static wrapper writes nothing.
  - The other 44 rows are neighbours that give the same verdict today.
- Existing corpus, 176 rows: 0 mismatches on the reference, and 0 on the committed module. The 30 existing licence, licence-ev, restore and restore-planted rows, routed through F6Failures and ClassifyRestore, give 0 outcome changes. **No existing outcome changes.**
- Python: 141 of 141 pass on wt-422 (17 of 17 alone). The 2 draft `Test422Fix` rows pass today.

## 6. Mutants (27, all killed)

| Mutant | Killed by |
|---|---|
| f6-no-value-check (F6 keeps today's existence-only decision) | FW-E-swap and 9 more |
| f6-no-set-check / -folder / -nuspec / -label / f6-no-acceptance | FW-page-not-restored, FW-restored-not-on-page / FW-no-folder / FW-no-nuspec, FW-pkg-broken / FW-label / FW-acc-flag, FW-pkg-acc-false |
| pkg-type-case, pkg-any-file, pkg-name-case, pkg-name-license-only | FW-pkg-type-case; FW-pkg-notice-apache; FW-f6-real and 19; FW-pkg-file-licence, -copying |
| pkg-files-top-only, pkg-basename-slash-only | FW-pkg-file-nested; FW-pkg-file-backslash |
| restore-network-first | FW-O-planted-offline, FW-O-net-exit0-count0, FW-O-de-planted-offline, FW-O-nu1004-lower-net |
| restore-substring-nu1004 | FW-O-planted-nu10041, FW-O-nu10045, FW-O-xnu1004 |
| restore-ignore-count, count-all-libraries | FW-O-partial, -net-count30, -de-partial; FW-count-shape, -project-only |
| log-v0, log-after-assert, log-stdout-only, log-stderr-only | FL-pass-note and 5 more; FL-fail, FL-timeout; FL-pass-runner, FL-e2e-L5; FL-pass-note, FL-pass-crlf |
| wire-f6-unwired, wire-f6failures-no-value, wire-read-no-files | FW-src-f6-calls; FW-src-f6failures-value; FW-src-read-packages |
| wire-f5-substring, wire-count-zero, wire-locked-network-first, wire-classify-v0 | FW-src-f5-classify (both); FW-src-locked-classify, FW-src-no-networkmissing; FW-src-classify-outcome |

Each wire-* mutant edits the after draft to undo one wiring decision. None is equivalent.

## 7. Rulings needed

- **FX-1, the red's starting point.**
  - Option (a), recommended:
    - The red moves today's decisions verbatim into the new pure functions:
      - F6Failures with the existence-only checks;
      - ClassifyRestore with network-first and substring NU1004;
      - RestoredPackageFrom, RestoredCount and F6AcceptanceFailures as in section 1.
    - The red also rewires the three shells as in section 2. This is a refactor with no change of verdict on the network.
    - The behaviour rows then fail for the right reason (today's decision is wrong). At the red commit 30 rows are red: 10 f6, 12 classify, 6 log, and 2 wiring (f6failures-value, classify-outcome). The green changes two function bodies and the log wrapper.
  - Option (b):
    - The red stubs the 5 functions with NotImplementedException, as in round 1, and leaves the shells alone.
    - The 7 wiring rows then carry the wiring and 39 rows are red.
  - Either way, the wiring rows make a port without wiring fail.
- **FX-2:** accept the source-scan rows (FW-src-*) as tests. They are the only offline proof that the networked Facts call the functions.
- **FX-3:** commit the rows as `".mob", "records", "422", "corpus-fix.txt"`, and extend `test_422_every_corpus_row_is_embedded` to read it too, or else append them to corpus.txt. Either way every FW-*/FL-* id must appear quoted in the four embedding files.
- **Named limit, no row:** a path segment named exactly `NU1004` in restore output reads as a mismatch (round 1's word rule). Contrived.
- **Gate:** F5 and F6 need one networked run under the heavy lock. This seat ran none.
