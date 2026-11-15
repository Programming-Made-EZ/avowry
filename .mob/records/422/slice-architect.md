# Card 422 slice: the restored-package licence checks fail when the page is wrong

Architect (claude opus = claude-opus-5-5, D268, ctx-1), 2026-11-15, main 7f4f805 read-only. Probe in `<scratchpad>` + `probe422` (mutant run: `mutants422.txt`): `refcheck.py` (the REFERENCE: a copy of the Python checks with 37 `M("...")` switches), `orig/` (the four check files unchanged), `guard.py` (Python port of LiveTreeScanner, V0 and reference), `guardcs/GuardRef.cs` (the C# reference scanner), `gen422.py` (writes the corpus from real files), `eval422.py` (rows, V0, mutants), `real422.py` (real tree, timings), `run_units.py` (the 17 committed unit tests on both copies), `evidence422.json` (nuspec facts of the 30 packages read from the local package folder, no network). Corpus: `<scratchpad>` + `corpus422.txt`, 176 rows; columns id, family, encoding, check, expected, source, text (escapes `\\ \n \r \t`; `{OLD}` is the old two-letter prefix, so no file holds it whole). Paths below are split strings.

## 1. Design changes

Python offline module (`"tests", "test_restored_package_licences.py"`):

| Rule | Reference | Card |
|---|---|---|
| P1 rows | the section's table rows may start with 1 to 3 spaces (Markdown still renders them); 4 is a code block. Cell markup (`**MIT**`, a code span) stays a failure (fail-closed, unchanged) | [M] |
| B1 figures | new `figure_failures(page, locks)`, a separate test (not in `collect_failures`, so the planted fixture page keeps passing). 15 figure sentences, each a regex whose spaces match any whitespace run (inner, tab, wrapped line); the number is digits or a word (zero to fifty, `twenty-one`, `Thirty`), optionally in `**`, `__`, `*`, `_` or backticks. Every occurrence is checked. A figure sentence not found fails (`<name>: not found on the page`). Message: `restored-package-licences: figure -: <name>: page says <got>, rows give <want>` | [M][B] |
| B2 computed values | packages = lockfile union; Orders.Api = API lockfile count; Orders.Specs = union minus API; direct/transitive from the union; MIT and Apache-2.0 counts from the licence cells (both the none line and the Recommendation); no-expression/with-expression from the source cells; acceptance = rows whose `id version` the acceptance bullet names (`the four Microsoft test packages` too); notice = items of the parenthesised list, where `the eight Microsoft.Extensions packages` counts eight AND must equal the rows with that id prefix; an item that is no row fails | [M][B] |
| B3 URL | the xunit.abstractions bullet cites the reviewed URL (second review finding 2) | [E] |
| I1 tool files | `tool_closure(start, read)`: the Cli project, its ProjectReference closure read as XML (either quote, any attribute order, `;` lists, backslashes), every `Directory.Build.props`, `Directory.Build.targets` and `Directory.Packages.props` from each project folder up to the root (a superset of MSBuild's nearest-file rule, on purpose), and literal Imports (`$(MSBuildThisFileDirectory)` resolved; Sdk Imports skipped) | [I] |
| I2 tool items | XML, not PKG_RE: item types `PackageReference`, `GlobalPackageReference`, `PackageDownload` in any case; id from `Include`, else `Update` (`Remove` only: no gain); `;` lists split; version from the `Version`/`VersionOverride` attribute or child element (CDATA, entities, whitespace trimmed), else `-`. Reported whatever the Condition (ruling R-2). A tool file that is not XML fails, and so does an Import it cannot follow. Message unchanged: `<id> <ver>: PackageReference in <rel>` (the item type replaces PackageReference for the other two) | [I] |
| S1 props | document order over top-level children: only `PropertyGroup` counts (not inside `Target` or `Choose`); the last assignment wins; a false Condition (group or element) skips it; an undecidable Condition (anything with `$(`, `@(`, `%(`, `and`/`or`) makes it unknown; a later `Import`, `ImportGroup` or `Choose` makes it unknown. Decided: none or empty, `true/on/yes`, `false/off/no`, `'a' == 'b'`, `'a' != 'b'` (case-insensitive, quoted). Pass only when the final value is `true` (any case, trimmed). Message `<name> -: not true (<why>)` | [S] |
| O1 shallow | pure `lockfile_commit_skip(page, shallow)` returns `restored-package-licences: section -: commit-blob check skipped (shallow checkout)`; `lockfile_commit_failures(page, shallow=None)` prints it (ruling R-4 on where it shows) | [O] |

C# networked class (`RestoredPackageLicencesLockedRestore.cs`), pure static helpers the green ports from `refcheck.py`:

| Rule | Reference | Card |
|---|---|---|
| E1 licence values | `licence_value_failures(rows, evidence)`: expression rows as today; `packaged licence file`: the licence files are classified from their first 600 characters, whitespace collapsed (`MIT License` gives MIT; `Apache License` with `Version 2.0` gives Apache-2.0; `BSD n-Clause`); fail when none classifies or the row's licence is not among them; `licence URL`: the nuspec URL must be a key of a reviewed table in the test (xunit.abstractions 2.0.3: the master-branch URL gives Apache-2.0) and the row's licence must equal its value. Messages name the package and both licences, for example `xunit.abstractions 2.0.3: licence MIT but the reviewed licence URL says Apache-2.0` | [E] |
| O2 restore outcome | `restore_outcome(exit, timedOut, text, restoredCount)`: timeout fails; exit 0 passes (a NU1801 warning on a warm cache is no network miss); `NU1004` as a word (any case) is `mismatch` before any network marker; network markers count as `network-missing` only when the new `project.assets.json` holds 0 package libraries; else fail. `RestoreLockedOrFail` needs pass; F5's planted step needs mismatch. The temp copy must not carry an `obj` folder from the source tree, or the count is stale | [O] |
| O3 F6 rows | `ReadRestoredRows` takes the same 0 to 3 leading spaces as P1 | [M] |

Guard (`NoPhCodesInTheLiveTree.cs`): H6 and H7 leave `History` and `HistoryPrefixes` (`MOB.md` stays in the prefix list, ruling R-5); a file whose name is exactly `packages.lock.json` (any folder) has each `"contentHash"[ \t]*:[ \t]*"<base64>"` value blanked to spaces (same length, lines kept) before the scan. Token-level, not line-level: equal on NuGet's one-property-per-line files, and a one-line lockfile still has its other content scanned. A `.bak` sibling is no lockfile now. The dead-entry test expects H1 to H5. [B]

## 2. Corpus (D263 families, D265 encodings)

176 rows, all from real files where they exist (the real page with edits, the real tool projects and root props overlaid, `"reference", "Directory.Build.props"`, both lockfiles and their line 132/185 hash, the 369 green record's NU1004/NU1102 texts, the local nuspecs).

| Family | Rows | Checks |
|---|---|---|
| page-licence (licence cells, [E]) | 30 | offline 17, licence 6, licence-ev 7 |
| page-figure (prose figures) | 33 | offline |
| tools (PackageReference forms) | 38 | tools |
| props (PropertyGroup, Condition) | 31 | props |
| restore (locked-restore output, shallow) | 21 | restore 11, restore-planted 7, shallow 3 |
| guard (lockfile lines) | 23 | guard |

Encodings (rows): plain 57, digits 19, table-cell 15, line-break 14, leading-ws 8, condition-false 7, inner-ws 5, words 5, case 4, code-span 4, condition-unknown 4, attr-order 3, bold 3, cdata 3, condition-true 3, directory-build-props 3, entity 3, licence-file 3, single-quote 3, comment 2, directory-packages-props 2, nuspec 2, update 2, child-element 1, directory-build-targets 1. Each MSBuild encoding (attribute order, child Version, `Update=`, comments, CDATA, entities, single and double quotes, Conditions true/false/unknown, root `.props` and `.targets`), each page encoding (cells, digits, words, bold, code spans) and each whitespace form (leading, inner, line-break) has rows in the families where the check meets it.

Scenario rows: [Z] Z-page, Z-licence, Z-tools, Z-props, Z-restore, Z-guard-api, Z-guard-specs; [E] E-xa-mit, E-srm-apache, E-swap, E-swap-off; [M][B] M-add, B-total ... B-rec-apache; [I] I-order, I-child, I-update, I-root-props, I-root-targets; [S] S-false, S-plain; [O] O-warm-warning, O-cold, O-partial, O-planted-offline, O-shallow; [B] G-hash, G-resolved, G-key. Near misses already handled keep their outcome: L-gpl, L-E1 to L-E3, L-kind, L-source-label, L-bold, L-code, I-E5, S-comment, S-missing, S-broken, O-planted, O-nu1102, G-code, G-phrase, G-history.

## 3. Measured outcomes

- Reference: 176 of 176 rows as expected. Committed unit tests (17, `RestoredLicencesOffline`): 17 pass on the unchanged copy and 17 on the reference.
- V0 on the corpus: 81 rows red (V0 passes a wrong input), 8 same verdict with another message (E-expr-swap, E-url-missing, E-file-none, I-entity and I-list and I-value-ws: V0 names a wrong id, S-comment, S-missing: no reason), and 8 outcomes change on purpose:
  - I-comment, I-cdata-text: V0 fails a commented-out or CDATA-text reference, which restores nothing; now pass;
  - O-warm-warning, O-warm-nopkg: V0 says network missing on a successful restore; now pass;
  - O-partial: network lost after packages were restored, V0 network missing, now fail;
  - O-planted-offline: NU1004 with NU1301, V0 network missing, now mismatch;
  - O-planted-nu10041: V0's substring test takes NU10041 as NU1004; now fail;
  - G-other-lock: a chance hash in a lockfile in another folder, V0 hits, now no hit (any `packages.lock.json` gets hash blanking).
- Real files: today's page, both lockfiles, provider, tool closure (`"src", "Avowry.Cli", ...`, Compiler, Adapters.CSharp, root `Directory.Build.props`) and props: no failure in any check; licence values on the real nuspec evidence: none (12 packages carry a notice file, 4 ask for acceptance, matching the page). Guard over 2,796 tracked files: V0 0 hits, reference 0 hits; H1 to H5 still hold codes (1,947, 55, 18, 334, 161); both lockfiles have 1 raw hit and 0 after blanking. The C# reference scanner gives the same 23 guard rows as the Python port.

## 4. Network split

- [E]: networked: the locked restore and reading each package folder (nuspec `license`, `licenseUrl`, licence files). Network-free: `LicenceValueFailure` / classifier / reviewed-URL table with evidence fixtures (rows E-*, Z-licence; licence-ev rows edit the evidence). Offline today the figures already catch a one-way change (E-xa-mit-off); a swap that keeps 21 and 9 (E-swap-off) is caught only with the network.
- [O]: networked: the two restores and F5's planted restore. Network-free: `RestoreOutcome` on output fixtures (restore rows), the shallow note (O-shallow, O-deep, O-shallow-nohash). The SDK 8.0.x CI run is an observation, not a test.
- Not required, offered: F6 could also compare the notice-file set with the page's list (evidence gives the same 12).
- Nothing networked was run by this seat.

## 5. Mutants (37; all killed)

| Mutant | Killed by |
|---|---|
| P-indent | L-indent-dup, L-indent3-dup |
| B-off, B-off-by-one | B-total and 24 more |
| B-words | B-acc-word, W-words, Z-page |
| B-ws | W-wrapped, W-inner |
| B-first-only | W-dup-wrong |
| B-group | B-group |
| E-url-prose | W-url |
| E-file, E-url, E-url-value | E-srm-apache, E-file-*; E-url-*; E-xa-mit |
| I-v0, I-case | I-order ...; I-lower |
| I-child-version, I-update, I-list | I-child; I-update; I-list |
| I-global, I-targets, I-dirfiles | I-download, I-global; I-root-targets; I-root-props, I-src-props |
| I-imports, I-projref-regex, I-backslash | I-import*; I-projref-single/-order; I-projref-backslash |
| S-v0, S-cond, S-unknown, S-compare | S-false*; S-unknown*; S-true-compare |
| S-first-wins, S-import | S-override; S-import-after |
| O-pass-first, O-mismatch-first, O-count, O-shallow-note | O-warm-nopkg; O-planted-offline; O-partial; O-shallow |
| G-keep-H6H7, G-no-blank, G-skip-line, G-any-value, G-any-json | G-resolved; G-hash, Z-guard-api; G-minified-hit; G-hash-text; G-json |

No equivalent mutant.

## 6. Timings

All offline checks on the real files, once, from disk: 209 ms (the existing commit-blob `git show` is most of it, 245 ms when timed alone); figures 7 ms, tool closure and items 3 ms, `collect_failures` 2 ms, props 0.1 ms. The figure number pattern starts with the look-behind `(?<![\w*`])`: without it the 15 searches took 26 ms per page and the rows 2 s. Guard reference over the tracked tree: 8 ms (V0 6 ms). All 176 rows: 626 ms (the 51 offline page rows 533 ms). The 17 committed unit tests: 214 ms on the reference, 238 ms on V0.

## 7. Rows the red embeds (all 176), and where

- Python unit tests (`"tests", "test_restored_package_licences.py"`, new class for the rows, red names `test_422_*`): page-licence offline rows (Z-page, E-xa-mit-off, E-swap-off, L-*), all page-figure rows (M-add, B-*, W-*), all tools rows (Z-tools, I-*), all props rows (Z-props, S-*), and O-shallow, O-deep, O-shallow-nohash. The rows ride on the real page and real tool tree as the corpus does; the real-file [Z] tests stay as they are, plus one for the figures.
- C# acceptance (`RestoredPackageLicences.cs`): new Facts with `Slice 422`, `Criterion 422-1` [Z], `422-3` [M][B], `422-4` [I], `422-5` [S], `422-6` [O] run the new Python tests through `AssertPythonTestsPass`.
- C# only, offline [Theory] rows without a restore: Z-licence, E-xa-mit, E-srm-apache, E-swap, E-expr-swap, E-url-*, E-file-* (`422-2`, evidence as fixtures) and every restore and restore-planted row (`422-6`); the networked F5/F6 call the helpers.
- C# guard (`NoPhCodesInTheLiveTree.cs`, `422-7`): all G-* and Z-guard-* rows as `PlantedRows` (path, text, mustHit), the text built from parts with `Old`.

## 8. Rulings for the facilitator

- R-1 [E]: the reviewed-URL table (URL to licence) lives in the C# test, the record of the 2026-11-10 read; a new URL-only package fails until the table gains it. Adopt?
- R-2 [I]: a tool package under a false Condition is still reported (I-cond-false). Strict on purpose; adopt?
- R-3 [S], outside the card: a nearer `Directory.Build.props` under `"reference", "csharp"`, a `Directory.Build.targets`, or a project file can still override the two properties; the reference does not read them (second review finding 5). In this card or a new one?
- R-4 [O]: the shallow note is printed by Python; for it to show on a passing run, the C# wrapper must write Python's output to `ITestOutputHelper`, or the Python test must skip with that reason. Which?
- R-5 [B]: `MOB.md` is still in `HistoryPrefixes` with no History entry (its H6 was removed on 2026-11-10). Leave for another card?
- R-6 [M][B]: rewording a figure sentence fails until the check's pattern changes (W-gone). Adopt that coupling?
