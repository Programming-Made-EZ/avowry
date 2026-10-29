# Traceability

Avowry traceability is an external source map. Generated implementation code must not know about the authoring layer that produced it, so this mechanism adds no Avowry file names with line numbers, no trace header, no `// phrase:` comments and no `#line` directives to the generated C#.

Older in-code links remain for now: the C# adapter already emits `[Avow("...")]` attributes with the avow template (and a runtime class for them), one comment per step with the step text, and a header comment naming the module. The attribute is the same inverted dependency as the rest of this document rules out, so the direction is settled: the link moves into this source map, the benchmark scorer reads the map, and the grammar re-freeze drops the attribute (the tracking card sequences the three parts with the re-freeze). Per-step comments are judged by one rule: plain readability text with no Avowry coupling may stay; text that copies the Avowry source of a step, or names the `.avowry` file, moves to the map. Until that card lands, the generated output of this mechanism is otherwise unchanged.

`avowry build Tasks.avowry -o generated` writes `Tasks.avowrymap.json` beside the generated projects. The deterministic, LF-terminated JSON contains `schemaVersion`, a shortened SHA-256 `buildId`, and ordinally sorted endpoint, step, phrase, and failure records. Each record has `stepId`, `kind`, `name`, `phraseFile` (base name only), `line`, `endpoint`, `step`, `specLines`, `csFile`, `csMethod`, and a SHA-256 `csHash`. No timestamp or absolute path is recorded. `stepId` is the lowercase first 12 hexadecimal characters of SHA-256 over this exact UTF-8 text, with no trailing newline: `kind + "\n" + invariant-decimal-line + "\n" + normalized-name`. A failure record appends `"\n" + endpoint`, giving each (failure, endpoint) pair its own id. Normalization replaces CRLF and bare CR in the name with LF. The input contains no path, so the id is stable across working directories, path separators, drive letters, cultures, and LF/CRLF source copies.

The same opaque build ID is compiled as the assembly metadata key `AvowryBuildId`. This lets tools distinguish a matching map from a stale one without putting source names or locations in the runtime.

Use `avowry trace EmptyTitle --map generated/Tasks.avowrymap.json`. A generated frame such as `CreateTaskHandler.Handle` can be used instead of an avow name. Add `--assembly generated/Tasks.Api/bin/Debug/net8.0/Tasks.Api.dll` to check the assembly build ID. A mismatch is printed loudly; `--strict` also makes it an error. The generated C# hash is checked whenever the mapped file is next to the map.

The map is a build artifact outside both generated projects. It is therefore neither served by the API nor copied by `dotnet publish` of the API project. Keep deployment packaging rooted at the published project output, not the build workspace.

`#line` directives are deliberately deferred to card 164 because they would place Avowry paths in debug information and stack traces.

## Executable evidence

- `SourceMapTraceability` covers deterministic maps, opaque IDs, path independence, LF/CRLF normalization, trace lookup, and absence of Avowry source knowledge in generated C#.
- `SourceMapDeployment.Publish_output_never_contains_the_source_map` proves a published Tasks API contains its assembly and no `.avowrymap.json` file.
- `SourceMapDeployment.Running_api_answers_404_for_the_source_map_paths` proves the live Tasks API serves a real endpoint but answers 404 for obvious source-map paths.
- `PhraseBuildOneFile.Build_output_matches_the_compiler_bytes` proves the CLI preserves every compiler-generated C# byte and adds only the map.
- `TasksOverHttp` covers all demo endpoint behavior and the unchanged declared-failure response bodies; its live process checks the generated API boundary.
- `WebExtensionBoundary` pins the complete Orders generated-output hash, while the demo acceptance suite executes all 11 Tasks specs.

## Trace command verification

`Trace_with_a_matching_assembly_prints_a_matching_build_check`, `Trace_with_an_assembly_from_another_source_warns_and_strict_exits_nonzero`, `Trace_reports_a_generated_file_hash_mismatch`, `Trace_resolves_a_Class_Method_frame_taken_from_the_map`, `Trace_of_a_shared_name_prints_every_match`, and `Trace_rejects_bad_inputs_cleanly` verify the trace command's build checks, strict mismatch handling, generated-file hashes, frame and shared-name lookup, and clean bad-input errors. Step ids are checked for shape and cross-build equality today; the independent recompute and one id per failure per endpoint are card 170.

Failure records: one record per (endpoint, failure) pair, at the line where the failure is declared by `fails with`. When an endpoint reaches the declaration through several steps, or two different avows declare the same name, the first step in the endpoint's order that reaches a declaration wins, and a composite avow's own declaration is preferred to those of its callees. A failure raised only by native code and never declared has no record.

Record order and the endpoint of an avow record: records are sorted ordinally by kind, then name, then endpoint. An avow record's `endpoint` is the first endpoint, in source order, that calls that avow directly in one of its own steps; an avow reached only through other avows has an empty endpoint. Whether an avow record should list every endpoint that reaches it is an open question (card 172).
