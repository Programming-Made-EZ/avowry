# Card 165 red record (red seat: claude sonnet, claude-sonnet-5-5; context ctx-165r; adopted by D283)

Worktree: `~/projects/wt-165`, branch card-165. Nothing committed. No dotnet was run (build, test or otherwise); every claim about failing today is read from the code, not observed.
Only new files were added; no product code, docs, pin or existing test was touched:

- `tests/Avowry.Acceptance.Tests/Support/FailureObserver165.cs` (fixtures, the six failing requests, the test observer source, map reader)
- `tests/Avowry.Acceptance.Tests/FailureObserverGenerated165.cs` (offline half, 17 cases)
- `tests/Avowry.Acceptance.Tests/FailureObserverLive165.cs` (live half, 9 tests, `Collection("Isolated timing")`)
- `tests/Avowry.Acceptance.Tests/FailureObserverTrace165.cs` (trace by step id, 2 tests)

All tests carry `[Trait("Slice","165")]` and `[Trait("Criterion","165-N")]`. 28 cases in all. The two guards that pass today also carry `[Trait("Kind","BornGreen")]`.

## How the live tests work (no product seam beyond the slice)

The live tests build the Tasks demo with the CLI, then, in the temporary workspace only, add `AvowryTestHooks.cs` to `Tasks.Api` (a `TestFailureObserver : IAvowryFailureObserver` and two hooks) and insert two lines into the generated `Program.cs`:
`Tasks.AvowryTestHooks.Register(builder.Services);` right after `var builder = WebApplication.CreateBuilder(args);` (it registers the test observer first, only when the env var AVOWRY_TEST_OBSERVER is set), and `Tasks.AvowryTestHooks.Report(app.Services);` right after `var app = builder.Build();` (prints which observer type the host resolved). The API is built once, then started once per mode (none, record, throw, slow, log) through `GeneratedApi` with `PrebuiltDll`. Each start sends two warm-up successes and the six failing requests (EmptyTitle and TitleTooLong from CreateTask, NotFound from GetTask, CompleteTask and DeleteTask, StoreUnavailable from ListTasks), every request carrying the marker `req-secret`.

## Tests by criterion

### 165-1 (observer, no-op default, neutral event, a throwing or slow observer cannot change the response)

Offline (`FailureObserverGenerated165`), each a Theory over tasks and orders unless noted:
- `The_generated_runtime_declares_the_observer_interface_the_five_field_event_and_a_no_op`: one API file declares `public interface IAvowryFailureObserver { void OnFailure(AvowryFailureEvent x); }`, `public sealed record AvowryFailureEvent(int SchemaVersion, string Code, string Endpoint, string StepId, string BuildId);` and `public sealed class NoOpAvowryFailureObserver : IAvowryFailureObserver` with an empty `OnFailure`, in the module namespace; the file holds no `.avowry` and no `avowrymap`.
- `The_generated_host_registers_the_no_op_with_TryAddSingleton_so_a_host_can_register_first`: Program.cs has `TryAddSingleton<IAvowryFailureObserver, NoOpAvowryFailureObserver>()`, no plain AddSingleton of the interface, no Replace.
- `Each_endpoint_that_catches_a_declared_failure_takes_the_observer_and_builds_the_event_after_the_422` (tasks: at least 5 endpoint files, orders: at least 1): `IAvowryFailureObserver` appears before `HandleAsync(`; `new AvowryFailureEvent(` appears in the catch after `AddError(` and `Send.ErrorsAsync(422`.
- `The_event_is_built_from_five_arguments_with_no_request_exception_or_source_data`: the event constructor has five top-level arguments; the arguments do not mention req, input, request, HttpContext, Query, Headers, Body, Route, ex, exception, `.Message`, `.StackTrace`, `.Data`, `.InnerException`, `ToString(`, or any `failure.` member except `.Code`; the catch region has no `.avowry` and reads nothing but `failure.Code` from the exception.
- `Only_the_failure_step_ids_of_the_map_are_in_the_generated_csharp_on_lines_that_hold_no_source_knowledge` (uses the CLI map): the six failure step ids each appear in some `Tasks.Api/` file, on lines with no `.avowry`, no `line N`, no spec text; the ids of endpoint, step and avow records appear nowhere in the generated C#.
- `One_public_step_id_rule_lives_in_the_compiler_and_both_the_cli_and_the_adapter_call_it`: exactly one public static `StepId(string, int, string, string)` in the Avowry.Compiler assembly (found by reflection, so the class name is the green's choice) returns, for every record of the CLI-written Tasks map, the id in the map (so the map bytes cannot change); the CLI source no longer contains `normalizedName` and calls `X.StepId(`; CSharpProvider.cs calls `X.StepId(`.
- `A_type_named_like_a_generated_observer_type_is_a_reserved_name_diagnostic` (3 rows: IAvowryFailureObserver, AvowryFailureEvent, NoOpAvowryFailureObserver): compile throws the single AV341 naming the type. The green adds the three names to `ReservedNames.TypeNames`.

Live (`FailureObserverLive165`):
- `With_no_observer_registered_the_host_uses_the_no_op_and_answers_the_declared_422s` (none): log says `observer-type=NoOpAvowryFailureObserver`; six 422s with `errors.generalErrors == [code]`; no events.
- `An_observer_the_host_registered_first_is_kept_and_called_once_per_failure` (record): resolved type is the host's `TestFailureObserver` (TryAdd did not replace it); six events.
- `A_throwing_observer_leaves_every_status_and_body_unchanged_and_the_api_running` (throw vs none): six events were recorded (so the observer really ran), every status and body byte-equal to the none run, the exception text absent from every body, process alive, a further request still 422.
- `A_five_second_observer_neither_delays_nor_changes_the_response` (slow vs none): every response under 2 seconds, status and body equal to the none run, six events arrive eventually (within 40 s), process alive, a further request still 422.
- `The_event_has_exactly_the_five_neutral_fields_and_carries_no_request_data_or_source_name` (record): the JSON of each event has exactly SchemaVersion, Code, Endpoint, StepId, BuildId; SchemaVersion 1; StepId is 12 lowercase hex; no `req-secret`, no `avowry`, no `boom`, no title text in the event.

### 165-2 (tracing plugs in outside the runtime; the map resolves every declared failure; the logging line resolves offline)

- `A_recording_observer_gets_one_event_per_declared_failure_that_resolves_through_the_map_to_the_exact_line_and_endpoint` (live, record): one event per (code, endpoint) of the six requests, four distinct codes; each event's stepId resolves to exactly one map record of kind failure with the same name and endpoint, sourceFile Tasks.avowry, and the line of the `fails with` declaration in the avow that endpoint calls (the expected line is read independently from the avow source: find task, delete task, list tasks, validate task title).
- `The_event_build_id_is_the_map_build_id_and_the_assembly_metadata` (live, record): every event's BuildId equals the map's buildId, and `avowry trace EmptyTitle --map ... --assembly Tasks.Api.dll` prints `map matches assembly`.
- `The_plain_logging_observer_writes_a_production_line_that_trace_resolves_offline` (live, log): six lines of the form `avowry-failure v1 code=C endpoint=E stepId=<12 hex> buildId=B` on stdout (the logging observer is the test project's own `TestFailureObserver` in log mode, its source is `FailureObserver165.HookSource`); with the API already stopped, `avowry trace <stepId> --map` exits 0 and prints `C  failure  422  stepId <id>` and `Tasks.avowry:<expected line> endpoint E`.
- `Trace_resolves_a_step_id_to_its_record_for_every_failure_and_for_the_other_kinds` (`FailureObserverTrace165`, CLI only): trace by step id for the six failure records and the first endpoint, step and avow record: exit 0, `stepId <id>`, `<file>:<line> endpoint <endpoint>`, exactly as many records printed as carry that id, failure lines equal the independently read declaration line.
- `A_step_id_is_an_exact_match_only_so_a_prefix_or_an_extended_id_is_unknown` (BornGreen): a prefix or an id plus one character exits 1 with `Unknown trace name or frame`.
- `The_traceability_doc_shows_the_observer_the_production_log_line_and_tracing_it_by_step_id` (offline): docs/TRACEABILITY.md contains `IAvowryFailureObserver`, `avowry-failure v1 code=`, `stepId=`, and `avowry trace` followed by a step id placeholder (`<stepId>`, `<step id>`) or a 12-hex id.

### 165-3 (bodies and specs byte-identical; pins re-recorded honestly)

- `The_422_bodies_are_byte_identical_whatever_observer_is_registered_and_hold_none_of_the_event` (live): none, record, throw, slow and log bodies are byte-equal for all six failures; the body is `errors.generalErrors == [code]`; no event stepId or buildId in any body.
- `The_eleven_demo_spec_files_are_byte_identical_apart_from_how_the_endpoint_is_constructed` (offline, BornGreen): the generated `Tasks.Specs/*.cs` files, minus the 11 lines containing `Factory.Create<` and any line containing `AvowryFailureObserver`, hash to `8ed00cd9...bba4f`, taken from the Slice 176 golden before this card; the 11 `[Fact(DisplayName` lines are still 11.
- `The_orders_pin_is_re_recorded_with_a_comment_naming_the_observer` (offline): `OrdersOutputHash` in WebExtensionBoundary.cs is a new 64-hex value (not `19fe6d71...`), and the `//` block directly above it mentions `165` and `observer`.
- `The_demo_golden_is_re_recorded_with_the_observer_and_a_comment_naming_it` (offline): the golden contains `public interface IAvowryFailureObserver` and `TryAddSingleton`, and TasksDemoGeneratedCsharp.cs has a `//` line naming `165` and `observer`.
- The README drift test, the Usable v1 bars and every other existing check are the facilitator's full `dotnet test --filter "Status!=Pending"` run.

## Expected result today (nothing implemented)

Fail today (26 of 28), each for the stated reason:
- Offline runtime, host, endpoint and event theories (8 cases): no `IAvowryFailureObserver` anywhere in the generated C#.
- `Only_the_failure_step_ids...`: no step id is in the generated C#.
- `One_public_step_id_rule...`: no public `StepId` in Avowry.Compiler (it is a private function in the CLI).
- The three reserved-name rows: the names are not reserved, compile succeeds, no AV341.
- `The_orders_pin...`, `The_demo_golden...`, the docs test: pins and docs are unchanged.
- `Trace_resolves_a_step_id...`: `Unknown trace name or frame`, exit 1.
- All nine live tests: the class fixture fails first, because `Tasks.Api` with the test observer does not build (CS0246, `IAvowryFailureObserver` does not exist); the fixture throws the build log. They are red for the right reason but cannot show their own assertions fail until the green gives the runtime types; the facilitator should expect nine identical "class fixture failed" results.

Pass today (2 of 28, both BornGreen guards): `The_eleven_demo_spec_files_are_byte_identical...` and `A_step_id_is_an_exact_match_only...`. Both are there so the green cannot break them.

## What the green must re-record, and why (I did not touch these)

1. `WebExtensionBoundary.OrdersOutputHash` (Slice 142, `19fe6d71...`): the orders endpoints gain the observer dependency and the event, and Program.cs the no-op registration, so the orders generated bytes change. Re-record the new hash and add a `//` line above it naming card 165 and the failure observer (my test reads that comment).
2. `TasksDemoGeneratedCsharp.golden` (Slice 176): new runtime file, changed endpoints, changed Program.cs, changed spec construction lines. Re-record it (the test's output), with a `//` comment in TasksDemoGeneratedCsharp.cs naming card 165 and the observer.
3. `ExistingTestsKept172.The_generated_csharp_pins_are_unchanged` asserts the text `OrdersOutputHash = "19fe6d71` still occurs in WebExtensionBoundary.cs. That will fail the moment the pin is re-recorded. It is a card 172 guard and cannot stay as written: the green must change it to the new prefix (with the card 165 reason). Not in my brief, flagged so it is not discovered as a surprise.
4. `MapCultureInvariance` (Slice 169) asserts `static string StepId(` in the CLI's map-writer region and scans that region for non-ordinal calls. Moving the rule into Avowry.Compiler must leave a one-line `static string StepId(...) => X.StepId(...)` forwarder in Program.cs (my test accepts exactly that: no `normalizedName`, a call `X.StepId(`), or that test must be changed deliberately.
5. Likely other fallout to check in the full run: `GeneratedCodeUsesAvowryNames` (`DoesNotContain("Phrase", generated)`, and `FindSingle("AvowryRuntime.cs")` wants exactly one such file), `PersistenceEffectCompiles` (`FindSingle("Program.cs")` and `AddSingleton<IItemStore, InMemoryItemStore>()` text), `SourceMapTraceability.AssertNoPhraseKnowledge` (no `.avowry:<digit>`, `avowrymap`, `Phrases.` and so on in any generated C#), `LeafPhraseFailureCompiles`, `CompileEndpointToCSharp` and the other tests that build the generated specs project, since the specs call `Factory.Create<XEndpoint>(..., taskstore)` and the endpoint gains a constructor dependency.

## Mutation steps (each must turn a named test red; for the green and the QA seat)

1. Register the no-op with plain `AddSingleton` (or Replace): `The_generated_host_registers...` and `An_observer_the_host_registered_first...` fail (resolved type is the no-op).
2. Call the observer before `Send.ErrorsAsync(422`: `Each_endpoint_that_catches...` fails (order). Call it inline and awaited after the send: `A_five_second_observer...` fails (responses or the following requests wait five seconds).
3. Call the observer with no try/catch (or on a path where its exception reaches the response or the process): `A_throwing_observer...` fails (status or body changes, process gone, or a later request fails).
4. Add `failure.Message`, `req.Id` or `ex.ToString()` to the event arguments, or a sixth argument: `The_event_is_built_from_five_arguments...` fails; adding a sixth record field also fails `The_generated_runtime_declares...` and `The_event_has_exactly_the_five_neutral_fields...`.
5. Put `Tasks.avowry`, a line number or a spec description next to a step id in the lookup: `Only_the_failure_step_ids...` fails.
6. Key the step-id lookup by code only, not code and endpoint: the NotFound events of GetTask, CompleteTask and DeleteTask share one id and `A_recording_observer_gets_one_event...` fails (endpoint or line mismatch).
7. Change the id recipe in the compiler (one character, or drop the endpoint for failures) while the CLI still has its own copy: `One_public_step_id_rule...` fails; if both use the shared one, the map bytes change and the 170 recompute test fails too.
8. Emit the ids of endpoint, step or avow records too: `Only_the_failure_step_ids...` fails.
9. Make `avowry trace` match a prefix, or remove the step-id match: `A_step_id_is_an_exact_match_only...` (BornGreen) or `Trace_resolves_a_step_id...` fails.
10. Put a different build id in the event than the AvowryBuildId metadata: `The_event_build_id_is_the_map_build_id...` fails.
11. Add a header or a body field to the 422 when an observer is present: `The_422_bodies_are_byte_identical...` fails.
12. Edit any generated spec line other than the endpoint construction: `The_eleven_demo_spec_files...` fails.
13. Leave `OrdersOutputHash` or the golden at the old value: the Slice 142 and 176 tests fail, and so do my pin tests (the old hash is also asserted to be gone).

## Ambiguities and the forms I fixed (the green needs these, or must tell me where I over-reached)

- Namespace and file: the interface, event and no-op are expected in the module namespace (`Tasks`), all three in one generated file under `<Module>.Api/`. The live hooks refer to them by simple name inside `namespace Tasks;`.
- The generated `Program.cs` must keep the two lines `var builder = WebApplication.CreateBuilder(args);` and `var app = builder.Build();` (the hooks are inserted next to them; the fixture says so loudly if they vanish). `TryAddSingleton` needs `using Microsoft.Extensions.DependencyInjection.Extensions;` in that file.
- Constructor injection is read as: the endpoint's class declaration (or constructor) names `IAvowryFailureObserver` before `HandleAsync(`. Resolving it some other way fails `Each_endpoint_that_catches...`.
- The event is built with `new AvowryFailureEvent(` (not target-typed `new(`) and five arguments, inside the catch region of the endpoint file, after `Send.ErrorsAsync(422`. The observer call itself may go through a helper; only the construction site is scanned. The arguments may read `failure.Code` (the declared code) but nothing else from the exception.
- "Off the request path" is tested strictly: with a five-second observer, all six responses on one keep-alive client must each finish under 2 seconds. The slice's "small bound" is my 2 seconds; a green that runs the observer inline after the flush would stall the next request on the same connection and fail. If the QA seat reads the slice as allowing that, raise the question; I held to "off the request path". On a machine with few cores the test observer's `Thread.Sleep(5000)` ties up pool threads, so a very small core count could add some hundreds of milliseconds; 2 seconds leaves room for that.
- The slice says "the 11 specs byte-identical". Constructor injection changes the line `Factory.Create<CreateTaskEndpoint>(context => ..., taskstore)` in the generated spec files (eleven lines), so byte-identity of the generated spec C# cannot hold literally. I pinned everything except those lines (and any line mentioning AvowryFailureObserver) to the pre-card golden. If the green keeps the endpoint constructible without the extra argument (an optional parameter), the spec files do not change at all and the test still passes.
- Step ids in the generated C#: I required only failure ids (one per code and endpoint, six for Tasks) and none of the other kinds. The slice says "the step id for that code and endpoint".
- The shared rule is found by reflection as a public static `StepId(string, int, string, string)` in any public type of Avowry.Compiler; the class name (`SourceMapIds` in the slice) is not pinned. Endpoint is passed for every kind and only used for failures, exactly as the CLI does.
- The logging line format is the test project's (`avowry-failure v1 code=... endpoint=... stepId=... buildId=...`, the slice's example). The docs test requires the same prefix `avowry-failure v1 code=` and `stepId=` in docs/TRACEABILITY.md; if the green words the line differently in the docs, change both consistently.
- No test asserts the exact FastEndpoints 422 JSON text (I could not run the API to read it). The 165-3 byte-identity is relative: with no observer against with each of four observers, plus the structural `errors.generalErrors == [code]`. The existing README and spec checks hold the rest.
- I did not add tests for modules with no endpoints or for orders live; orders is covered offline (runtime, registration, endpoint, event). A module with no endpoint should still get the interface file (the slice says the generated files contain it); I did not pin that case.

## Red fix round 1 (ruling R165-2; no dotnet run; tests only)

1. `Support/FailureObserver165.cs`: `ObserverProbe165` gains `CallsLog`, the API log captured (after a 300 ms pause for the stdout pump) once the events are read and before the after-request `/tasks/req-secret-3`. `FailureObserverLive165.The_plain_logging_observer_writes_a_production_line_that_trace_resolves_offline` now matches log lines in `CallsLog`, so it still asserts exactly one line per failing call (6), none from the after-request.
2. Existing guards, intent kept (no store interface, no store registration), each change has a `//` comment naming card 165 and the failure observer:
   - `PersistenceEffectCompiles.cs`: the per-file loop skips the file named `AvowryFailureObserver.cs` and removes the single line `builder.Services.TryAddSingleton<IAvowryFailureObserver, NoOpAvowryFailureObserver>();` before the `InMemory`, `AddSingleton` and `interface I` checks.
   - `PutAndDeleteEndpointsCompile.cs`: the same single-line removal before its `InMemory`, `AddSingleton`, `void Save` checks (its `bool Remove` check is unchanged).
   - `OrdersPhraseCompiles.cs`: the concatenation of generated C# excludes the file `AvowryFailureObserver.cs` only, so `interface I` and `Option<` stay forbidden everywhere else.
   No test was renamed or moved. Not touched: src, the golden, the pins, any other test.
