# Card 171 green record (green seat: cursor composer-2.5, context ctx-171g)

Worktree: `~/projects/wt-171` (branch card-171). Nothing committed. No dotnet was run (facilitator builds and tests under Avowry's lock).

## git diff --stat

```
 src/Avowry.Adapters.CSharp/CSharpProvider.cs |  9 +++
 src/Avowry.Cli/Program.cs                    | 94 +++++++++++++++++++---------
 src/Avowry.Compiler/CompilationOutput.cs     |  2 +
 3 files changed, 76 insertions(+), 29 deletions(-)
```

## Changes per file

### `src/Avowry.Compiler/CompilationOutput.cs`

- Added `EndpointMethods` and `AvowMethods`, each `IReadOnlyDictionary<string, string>` with empty defaults, beside `EndpointFiles` and `AvowFiles`.

**Tests:** `Map_csMethod_values_are_the_names_the_adapter_returned` (171-1) — the build test discovers the two new string maps on `CompilationOutput` by shape.

### `src/Avowry.Adapters.CSharp/CSharpProvider.cs`

- While emitting endpoints, fills `endpointMethods[endpoint.Name]` with `EndpointHandlerName(endpoint.Name) + ".Handle"` (same rule as generated handler types; no new naming logic).
- Builds `avowMethods` keyed by avow key with `ReservedNames.Avows + "." + MethodName(phrase)` using the adapter's existing `MethodName` (holes included).
- Assigns both maps on the returned `CompilationOutput`. Generated file contents unchanged.

**Tests:**

- `Map_csMethod_values_are_the_names_the_adapter_returned` (171-1) — adapter returns authoritative method names.
- `Every_map_csMethod_names_a_method_that_exists_in_the_generated_csharp` (171-3) — avow map uses hole-inclusive names that match `Avows.cs`.
- Slice 142 / Slice 176 characterisation (171-3 generated-C# pins) — adapter emit paths for `.cs` files untouched.

### `src/Avowry.Cli/Program.cs`

- Map records read `csMethod` from `output.EndpointMethods[endpoint.Name]` and `output.AvowMethods[phrase.Key]`; removed CLI `MethodName` and all interpolated literals containing adapter layout (`Handler`, `Handle`, `Avows`).
- Style (171-2): split `Usage` and version output; broke five-call `OrderBy` chain into four plus `ToList()`; braced nested `foreach` loops; wrapped long lines to ≤120 characters; removed null-forgiving `parsed!` and `GetDirectoryName(...)!`; reflowed `Hash`, `MapRecord`, and related helpers.

**Tests:**

- `Program_cs_holds_no_adapter_literal_and_no_method_name_rule` (171-1) — no banned literals or method-name rule in CLI.
- `Map_csMethod_values_are_the_names_the_adapter_returned` (171-1) — CLI writes map from adapter maps, not local rules.
- `Program_cs_keeps_one_statement_per_line_short_lines_short_chains_and_no_null_forgiving` (171-2).
- `Every_map_csMethod_names_a_method_that_exists_in_the_generated_csharp` (171-3) — corrected avow `csMethod` for phrases with holes.
- Card 169 `MapCultureInvariance` 169-2 — `MethodName` no longer in `Program.cs` (red already dropped it from `MapHelpers`).

## Not changed

- Tests, docs, generated C# emit bodies. No third `IReadOnlyDictionary<string, string>` on `CompilationOutput` (test would be ambiguous).