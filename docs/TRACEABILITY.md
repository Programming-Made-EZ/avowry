# Traceability

Phrase traceability is an external source map. Generated implementation code must not know about the authoring layer that produced it, so CLI builds contain no Phrase file names, line numbers, endpoint/step source comments, or `#line` directives.

`phrase build Tasks.phrase -o generated` writes `Tasks.phrasemap.json` beside the generated projects. The deterministic, LF-terminated JSON contains `schemaVersion`, a shortened SHA-256 `buildId`, and sorted endpoint, phrase, and failure records. Each record has `kind`, `name`, `phraseFile` (base name only), `line`, `endpoint`, `step`, `specLines`, `csFile`, `csMethod`, and a SHA-256 `csHash`. No timestamp or absolute path is recorded.

The same opaque build ID is compiled as the assembly metadata key `PhraseBuildId`. This lets tools distinguish a matching map from a stale one without putting source names or locations in the runtime.

Use `phrase trace EmptyTitle --map generated/Tasks.phrasemap.json`. A generated frame such as `CreateTaskHandler.Handle` can be used instead of a Phrase name. Add `--assembly generated/Tasks.Api/bin/Debug/net8.0/Tasks.Api.dll` to check the assembly build ID. A mismatch is printed loudly; `--strict` also makes it an error. The generated C# hash is checked whenever the mapped file is next to the map.

The map is a build artifact outside both generated projects. It is therefore neither served by the API nor copied by `dotnet publish` of the API project. Keep deployment packaging rooted at the published project output, not the build workspace.

`#line` directives are deliberately deferred to card 164 because they would place Phrase paths in debug information and stack traces.
