# Card 176: pre-ship gate review (independent fresh Claude context, read-only; scratch single-branch clones)

Verdict: SHIP. No blockers.

Gate evidence: tested ed44d13 (the pending red) and dd3d511 (the green), each a `git clone --no-hardlinks --single-branch -b main` then `git checkout`; the CLI builds at dd3d511 with 0 warnings and 0 errors; red replay at ed44d13 with the Pending trait included: Failed 1, Passed 0, Total 1; the gate filter `Status!=Pending&FullyQualifiedName~Packed_tool_is_versioned` at ed44d13: no test matches (0 ran); at dd3d511 the target test, the golden-file characterisation, CliKeepsDiagnosticShape, AvowryCommand and PhraseCliFreshness together: Passed 10, Failed 0 (the target test was not run on its own there). Load: the facilitator's push hook ran until the done marker appeared (about 11 minutes of polling); every dotnet command ran after it; the four mutation builds ran one at a time. Not run: the full solution suite (the facilitator's isolated baseline gate: Compiler 324 of 324, Acceptance 325 of 325, 0 warnings, at dd3d511).

1. NOTE: the diff between ed44d13 and dd3d511 is exactly three files: Avowry.Cli.csproj (+6 lines), Program.cs (the `--version` line) and the test (the Pending line removed). src/ is unchanged between c2cb76f and ed44d13, so every `Criterion:` line precedes the first production change (characterisation Criterion 3; red and revision Criterion 1-2; green Criterion 1-2).
2. NOTE: the test body at dd3d511 is byte-identical to ed44d13 once the Pending line is removed (cmp); 264 lines, under the 400 cap.
3. NOTE: the red at ed44d13 fails by assertion from Checks.AssertAll (line 263): pack, metadata, explicit-version-source and install failed; version and demo-build reported blocked; the text matches red-record.md Revision 1; the install failure reads "avowry is not found in NuGet feeds <local feed>" (it did not reach nuget.org).
4. NOTE: by hand at dd3d511: `dotnet pack` produced only Avowry.0.1.0.nupkg; nuspec id Avowry, version 0.1.0, packageType DotnetTool; DotnetToolSettings.xml Command Name="avowry"; install into a scratch --tool-path with a cleared-source nuget.config succeeded; `avowry --version` printed `Avowry 0.1.0`; `avowry build demo/tasks/Tasks.avowry` exited 0 and produced the Tasks.Api and Tasks.Specs project files; an install against an empty feed failed with "avowry is not found" (no public fallback); `dotnet tool list -g` identical before and after.
5. NOTE: PackageId and ToolCommandName appear only in the CLI csproj; no old `Phrase.Cli` name remains in src or tests.
6. NOTE: the test cannot publish or install globally (install uses --tool-path and a nuget.config clearing the other sources; it only reads the global list before and after).
7. NOTE: Version 0.1.0 is stable (needed because the test installs without --prerelease) and set in one place.
8. NOTE: TempWorkspace deletes its directory on dispose; 120 directories under Temp/phrase-acceptance are not attributable to this test alone (not counted before the runs).
9. NOTE (not a defect): `dotnet pack` prints a missing-readme advisory; the test's pack step restores from the machine's default NuGet sources, only the install step is isolated, which matches the card's wording.

Mutations at dd3d511, one at a time; each made the test fail (Failed 1):
| Mutation | Checks that failed |
|---|---|
| (a) remove `<Version>` | metadata ("Version is 1.0.0"), explicit-version-source |
| (b) ToolCommandName `avowryx` | metadata (tool command avowryx), install (command missing); version and demo-build blocked |
| (c) PackageId `AvowryX` | pack (AvowryX.0.1.0.nupkg), metadata (id), install; version and demo-build blocked |
| (d) `--version` back to GetName().Version | version (strings differ) |
No mutation survived.

Could not verify: leftover processes (process lists not inspected); the count of leftover temp directories; behaviour on other operating systems or SDK versions; the full solution suite (the facilitator's run).

Independence: a fresh Claude context that wrote none of the card; the tests were written by a Grok seat and the green by a Claude seat; the Claude seat is because Codex, Copilot and agy are out of quota and the trial drivers are for low-stakes seats. Every number is from the reviewer's own runs.
