# Card 195 turn 2: architect (codex/gpt-5.6-sol)

Scrubbed reply (first 40 lines), from the card-mode whiteboard.

THIN VERTICAL SLICE. It removes the obsolete product name end-to-end from the real tool artifact, CLI execution, emitted source map, and test helper. The outermost acceptance boundary is the built/packed .NET tool: invoke the actual `avowry` CLI, inspect its emitted `.avowrymap.json`, inspect the `.nupkg` contents, and exercise the freshness helper against those assemblies. No feature flag: this is an explicitly decided rename/schema refactor with no net-new capability and no existing map users. No new ADR is needed; D126/D135 already decide the contract.

Smallest ordered increments:

1. QA adds one `Slice=195` acceptance test proving the real CLI/package boundary requires `Avowry.Cli`, `Avowry.Adapters.CSharp`, `sourceFile`, and kind `avow`, then proves red.
2. Rename both directories/projects with `git mv`; update namespaces, project references, `Phrase.sln`, and assembly lookup while preserving package ID and command.
3. Update the source-map writer/reader and traceability contract from `phraseFile`/`PhraseFile` and `phrase` to `sourceFile`/`SourceFile` and `avow`; update exact corpus expectations.
4. Update CI, scripts, freshness checks/messages, tests, and current docs that locate or name the renamed projects/assemblies; leave Bench and test-project names unchanged.
5. Build Release, pack and inspect the tool, run the Slice test, confirm the `WebExtensionBoundary` hash is unchanged, then run `usable-v1-report.py` and the full gate.
