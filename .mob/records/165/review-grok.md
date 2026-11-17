SIGN-OFF (grok grok-4.6, normal mode in a throwaway clone, ctx-165x; reply saved by the facilitator; three MINOR)


Card 165 at `bb5a383` matches the slice, R165-1, R165-2, and D284 option A. The generated event is a five-field neutral record; the 422 is sent before a fire-and-forget `Task.Run` whose exceptions are swallowed; CLI and adapter share `SourceMapIds.StepId`; outside-slice test edits stay inside the rulings. Facilitator proof 3 (compiler 344/344, acceptance 1578/1578) is taken as the suite result.

## MINOR

1. **Thread-pool sharing for a blocking observer.** `AppendFailureObserverCall` queues `Task.Run` on the default pool (`CSharpProvider.cs:354-358`). A synchronous `OnFailure` that blocks holds a worker for the whole call. The live slow test (`FailureObserverLive165.cs:82-91`) sends six keep-alive requests and requires each response under 2s; it does not cover a concurrent 422 storm. A flood of blocking observers can starve later requests, including later 422s. The slice’s example is this fire-and-forget task, and this response is already written by `await Send.ErrorsAsync(422, ct)` (`CSharpProvider.cs:765-767`, golden `TasksDemoGeneratedCsharp.golden:238-249`).

2. **`async void` OnFailure can still go unobserved.** The wrapper is `try { failureObserver.OnFailure(observed); } catch { }` (`CSharpProvider.cs:356-357`). A synchronous throw completes the task as `RanToCompletion`, so it will not raise `UnobservedTaskException` or tear the process down. The interface is `void` (`CSharpProvider.cs:304-305`). A host that implements `async void OnFailure` can throw after the first await, outside that catch; on ASP.NET Core that can fault the process. The throw-mode test uses a synchronous `InvalidOperationException` (`FailureObserver165.cs:85-86`).

3. **`FailureStepIds` is generated and unreserved.** The adapter emits `internal static class FailureStepIds` (`CSharpProvider.cs:315-318`). `ReservedNames.TypeNames` covers `IAvowryFailureObserver`, `AvowryFailureEvent`, and `NoOpAvowryFailureObserver` (`ReservedNames.cs:33-35`) and the AV341 tests pin those three (`FailureObserverGenerated165.cs:230-244`). A Phrase `type FailureStepIds` would collide in the module namespace at C# compile.

## Checks

### 1. Security (no source or request leak)

The event is `new AvowryFailureEvent(1, failure.Code, "<endpoint>", <switch>, "<buildId>")` (`CSharpProvider.cs:342-353`). Arguments are schema `1`, `failure.Code` (the declared code; `AvowryFailure` is `Exception(code)` at `CSharpProvider.cs:289-294`), the compile-time endpoint name, a switch onto `FailureStepIds.*` constants, and `module.BuildId`. The catch region reads no `req`, `HttpContext`, `Message`, or `ToString`.

The lookup file (`golden:5-29`) holds interface, record, no-op, and `internal const string Endpoint_Code = "<12 hex>"`. Those lines have no `.avowry`, no line number, and no spec text. The 12-hex value is SHA-256 truncated (`SourceMapIds.cs:9-14`); it does not reverse to file name or spec text. Constant names repeat endpoint and code, which the 422 already returns as `errors.generalErrors`. `BuildId` is SHA-256 of the source truncated to 16 hex (`AvowryCompiler.cs:30-35`), the same opaque assembly metadata as card 161.

The production-shaped log line is the test sink (`FailureObserver165.cs:82`): `avowry-failure v1 code=… endpoint=… stepId=… buildId=…`. Live tests assert no `req-secret` in events or log lines (`FailureObserverLive165.cs:118-121, 189-190`) and no step id or build id in 422 bodies (`FailureObserverLive165.cs:220-227`). `docs/TRACEABILITY.md:35-39` matches that shape.

A host observer can still log `IHttpContextAccessor` or `AsyncLocal` state it captured itself. The generated call passes only `observed`.

Card 161’s recipe includes the source line in the hash input, so a line number is brute-forceable if code, endpoint, and the public recipe are known. D284 rejected dropping the line. That property is the reused scheme.

### 2. Off the request path / cannot change the response

Order in every failing endpoint: `AddError` → `await Send.ErrorsAsync(422, ct)` → build event → discarded `Task.Run` (`CSharpProvider.cs:764-767`, golden `:238-249`). The observer is a constructor argument only when `endpoint.Failures.Count > 0` (`CSharpProvider.cs:715-716, 766-767, 835-836`), which is R165-2. Host default is `TryAddSingleton` (`CSharpProvider.cs:406-407`). Specs pass `new NoOpAvowryFailureObserver()` so spec bodies stay independent of a host sink.

`_ => ""` (`CSharpProvider.cs:352`) is the right fallback for an undeclared `AvowryFailure`: StepId stays empty rather than copying `failure.Code` or exception text into a field that `avowry trace` treats as an id. Empty is not a map record (ids are 12 hex). Declared Tasks codes are pinned by 165-2 against the map.

### 3. One step-id rule

`SourceMapIds.StepId` is the old CLI recipe moved verbatim (`SourceMapIds.cs:9-14`; pre-165 CLI used the same `kind\n{line}\n{normalizedName}` plus `\n{endpoint}` for failures, `ToLowerInvariant` hex). CLI keeps a one-line forwarder in the map-writer region (`Program.cs:129-130`), which is what `MapCultureInvariance` requires (`MapCultureInvariance.cs:269`). The adapter calls `SourceMapIds.StepId("failure", declaration.Line, failure, endpoint.Name)` (`CSharpProvider.cs:335`). The 165 test requires one public `StepId(string,int,string,string)`, CLI without `normalizedName`, and both call sites (`FailureObserverGenerated165.cs:200-225`).

Declaration walking is still copied (CLI `Program.cs:336-345`, adapter `CSharpProvider.cs:364-385`) with the same first-step / own-declaration-first rule. Hash input is the shared rule; Tasks 165-2 would fail if the two walks picked different lines there.

### 4. Test edits outside Slice 165

- **R165-2 guards:** `PersistenceEffectCompiles.cs:114-121` skips `AvowryFailureObserver.cs` and strips the single `TryAddSingleton<IAvowryFailureObserver, NoOpAvowryFailureObserver>();` line; `InMemory` / `AddSingleton` / `interface I` still apply to every other file. `OrdersPhraseCompiles.cs:33-38` excludes only that file for `interface I` / `Option<`. `PutAndDeleteEndpointsCompile.cs:252-258` strips only that line; `void Save` and `bool Remove` stay. Comments name card 165.
- **D284 A:** as-is `OrdersOutputHash` is still exact (`WebExtensionBoundary.cs:20, 37`). Explicit-file equality is after masking only `[0-9a-f]{12}` on `internal const string` lines in `AvowryFailureObserver.cs` (`:67-84`), plus `SourceMapIds.StepId` against independently read `fails with` lines (`:47-64`). Comment names 165 and D284.
- **172 pin:** prefix updated to `ec3a552` with a 165 observer comment (`ExistingTestsKept172.cs:44-45`), matching the new hash.
- **Pins:** `WebExtensionBoundary.cs:19-20` and `TasksDemoGeneratedCsharp.cs:7` name card 165 and the observer. Golden contains the observer runtime and `TryAddSingleton` (`golden:12-29, 488`).

### 5. Trace by exact step id

`TraceCore` matches `record.Name == name || record.CsMethod == name || record.StepId == name` (`Program.cs:179-181`). StepId is exact equality. Prefix and id-plus-one-character stay unknown (`FailureObserverTrace165.cs:50-64`). Tasks/orders names and `Class.Method` frames are not 12 lowercase hex, so a production step id does not collide with those names. A Phrase name that is exactly some other record’s 12-hex id would print both records, which is the existing shared-name behaviour.

## Could not check

- `dotnet` was forbidden; suite green is the facilitator’s proof 3 at `bb5a383`.
- Live FastEndpoints flush timing, thread-pool behaviour under concurrent 422s, and `async void` teardown were read from source and tests, not executed here.
- Orders hash and golden bytes were not recomputed.
- `MapCultureInvariance` was not run; the forwarder and recipe were read only.
