# Card 165 green record (green seat: cursor composer-2.5; context ctx-165g; adopted by D283)

Worktree: `~/projects/wt-165`, branch `card-165`. Not committed.

## `git diff --stat`

```
 docs/TRACEABILITY.md                               |   6 +
 src/Avowry.Adapters.CSharp/CSharpProvider.cs       | 114 +++++-
 src/Avowry.Adapters.CSharp/ReservedNames.cs        |   8 +
 src/Avowry.Cli/Program.cs                          |  10 +-
 .../ExistingTestsKept172.cs                        |   4 +-
 .../TasksDemoGeneratedCsharp.cs                    |   1 +
 .../TasksDemoGeneratedCsharp.golden                | 396 ++++++++++++++++++++-
 .../WebExtensionBoundary.cs                        |   3 +-
```

Untracked: `src/Avowry.Compiler/SourceMapIds.cs` (13 lines). Ephemeral local helper `tools/_card165_pins.py` was used to hash/order the golden (not part of the slice; facilitator may discard).

Pins and golden were produced with a one-off `dotnet build` / `dotnet run -- build` after the green implementation (card 165 forbids leaving a long-running process; no test suite run).

## Per file (and red tests)

| File | Change | Red / slice tests |
|------|--------|-------------------|
| `src/Avowry.Compiler/SourceMapIds.cs` | Public `SourceMapIds.StepId(kind, line, name, endpoint)` — single SHA-256 recipe (normalise name, append endpoint for failures). | `One_public_step_id_rule_lives_in_the_compiler_and_both_the_cli_and_the_adapter_call_it`; map bytes unchanged (169 / 170). |
| `src/Avowry.Cli/Program.cs` | One-line `StepId` forwarder to `SourceMapIds`; trace lookup also matches `record.StepId` exactly. | Same StepId test; `Trace_resolves_a_step_id_to_its_record_for_every_failure_and_for_the_other_kinds`; `A_step_id_is_an_exact_match_only_so_a_prefix_or_an_extended_id_is_unknown` (BornGreen). |
| `src/Avowry.Adapters.CSharp/ReservedNames.cs` | `IAvowryFailureObserver`, `AvowryFailureEvent`, `NoOpAvowryFailureObserver` in `TypeNames`; `FailureObserverFile` stem. | `A_type_named_like_a_generated_observer_type_is_a_reserved_name_diagnostic` (×3). |
| `src/Avowry.Adapters.CSharp/CSharpProvider.cs` | Emit `AvowryFailureObserver.cs` (interface, five-field record schema 1, no-op, `FailureStepIds` constants via `SourceMapIds`); `Program.cs` adds `TryAddSingleton` + `Extensions` using; endpoints take `IAvowryFailureObserver`; catch builds `new AvowryFailureEvent(...)` after `Send.ErrorsAsync(422, …)` then fire-and-forget `Task.Run` with swallowed observer exceptions; spec `Factory.Create<…>` lines pass `new NoOpAvowryFailureObserver()`. | `The_generated_runtime_declares_*`; `The_generated_host_registers_*`; `Each_endpoint_that_catches_*`; `The_event_is_built_from_five_arguments_*`; `Only_the_failure_step_ids_of_the_map_*`; live 165-1/165-2/165-3 behaviour; `The_eleven_demo_spec_files_are_byte_identical_apart_from_how_the_endpoint_is_constructed` (BornGreen). |
| `docs/TRACEABILITY.md` | Observer section: interface, `avowry-failure v1 code=… stepId=…`, `avowry trace <stepId> --map`. | `The_traceability_doc_shows_the_observer_the_production_log_line_and_tracing_it_by_step_id`. |
| `tests/.../WebExtensionBoundary.cs` | Orders output hash `e699fce…`; comment names card 165 and observer. | `The_orders_pin_is_re_recorded_with_a_comment_naming_the_observer`. |
| `tests/.../TasksDemoGeneratedCsharp.golden` | Regenerated from `avowry build` tasks demo. | `The_demo_golden_is_re_recorded_with_the_observer_and_a_comment_naming_it`; `Tasks_demo_generated_csharp_matches_the_golden_file` (176). |
| `tests/.../TasksDemoGeneratedCsharp.cs` | Comment: card 165 / observer. | Same demo golden test. |
| `tests/.../ExistingTestsKept172.cs` | Pin guard prefix `e699fce` (card 165). | Keeps 172-3 honest after re-record (per red record note). |

## Fallout checked by reading (no `dotnet test`)

- `MapCultureInvariance`: CLI keeps `static string StepId(` forwarder, no `normalizedName`.
- `GeneratedCodeUsesAvowryNames`, `PersistenceEffectCompiles`, `SourceMapTraceability`, spec projects: emit shape reviewed (single `AvowryRuntime.cs`, store `AddSingleton` unchanged, no `.avowry` in observer file).

## Fix round 1 (R165-2 product)

Mob ruling R165-2: inject `IAvowryFailureObserver` only on endpoints that declare at least one failure (same condition as `AppendFailureObserverCall` in the catch). Effect-free endpoints with no failures keep a primary-constructor-less class declaration; spec `Factory.Create` lines omit `new NoOpAvowryFailureObserver()` when the endpoint has no declared failures (fixes CS9113 unread `failureObserver` on effect-only endpoints such as RecordUpdateForm).

`Program.cs` trace lookup: split the `map.Records.Where(…).ToList()` chain so no line exceeds 120 characters (card 171).

Pins (`WebExtensionBoundary`, `TasksDemoGeneratedCsharp.golden`, `ExistingTestsKept172`) and all test sources left unchanged; facilitator re-records pins from one CLI run after this round.

No `dotnet` / `avowry` build or test in this round (shared laptop; green brief).