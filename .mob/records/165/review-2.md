SIGN-OFF (card 165 QA and security seat round 2: claude opus, claude-opus-5-5, ctx-165q; range bb5a383..a7baa97, red round 3 = 3d775ec, green round 2 = a7baa97; read only, no dotnet run)

F1, F2, F3, F4, F6 and the F7 docs are resolved as R165-3 rules. Each new or tightened red test fails on bb5a383 for the stated reason. Round 2 adds one new MINOR (R2-1): a compiler warning in a rare but valid source shape. It does not block, but it is a 4-place fix and I recommend doing it in this card.

## Round 1 findings, as ruled

- **F1 resolved.** CSharpProvider.cs `FailureStepIdName` is now `endpoint + "_" + failure` with no CSharpIdent. Names are `[A-Z][A-Za-z0-9]*`, so the result is always a plain, non-keyword identifier.
  - Test: FailureObserverGenerated165 `Failure_codes_spelled_like_csharp_keywords_give_plain_step_id_constant_names` (Default, Null).
  - On bb5a383 the constants were `Probe_@Default` and `Probe_@Null`, so `Assert.Equal({"Probe_Default","Probe_Null"})` fails, and so does the `@` check. Right reason.
- **F2 resolved.** ReservedNames.cs has `FailureStepIds` in TypeNames, and the emitter spells it through the constant (CSharpProvider.cs:315 and the event line).
  - Test: a new `[InlineData("FailureStepIds")]` row in the reserved-name theory.
  - On bb5a383 the compile did not throw AV341. Right reason.
- **F3 resolved (docs).** TRACEABILITY.md drops "opaque" and says the step id is not secret and the line is recoverable from code, endpoint and the public recipe.
- **F4 resolved.** The catch now has one `case "<Code>":` per code with a declaration. Each case builds `new AvowryFailureEvent(1, "<Code>", "<Endpoint>", FailureStepIds.<Endpoint>_<Code>, "<buildId>")` (golden lines 303-311). All five arguments are compile-time constants, nothing is read from the exception, and there is no default arm, so an undeclared code raises no event.
  - Tests: the tightened event test (code is a literal, no `\bfailure\b` in the arguments) and the new theory `The_catch_builds_one_event_per_declared_code_and_none_for_an_undeclared_code` (no `_ =>` or `default`).
  - On bb5a383 both fail: the argument was `failure.Code` and there was a `_ => ""` arm. Right reason.
  - The log-injection path from F4 is closed: the event's Code can no longer carry text built from request input.
- **F6 resolved.** MapCultureInvariance has a new test, `The_shared_step_id_recipe_in_the_compiler_holds_no_non_ordinal_call`. It runs the same `FindNonOrdinalCalls` scanner on src/Avowry.Compiler/SourceMapIds.cs, after proving that the scanner flags the planted dirty recipes (`ToLower()`, `Order()`) and accepts the clean one.
  - It is BornGreen: the recipe is clean, so it passes on bb5a383 too. That is correct for a coverage guard, which only has to catch a future regression.
- **F7 resolved (docs).** TRACEABILITY.md states the contract: fast, non-blocking, return synchronously, not `async void`, best-effort, one pool work item per failure, lost on shutdown or under load. Bounded dispatch is a named follow-up per R165-3.

## Round 2 findings

### R2-1 MINOR: an endpoint whose only failures have no declaration now gets an empty switch block (CS1522 warning)
- `endpoint.Failures` is "declared plus everything its steps can fail with" (Model.cs:51; Resolver.cs:377-390). An endpoint-level `ensure ... else Code` or `fail with Code` adds Code without any `fails with` declaration. Endpoints resolve their steps with `declaredFailures: null`, so no AV326 is reported (Resolver.cs:225-232). SourceMapTraceability:63-77 shows endpoint-level ensure is legal.
- For such a code, `FindFailureDeclaration` returns null and AppendFailureObserverCall (CSharpProvider.cs:342-356) skips it. If all of an endpoint's failures are like that, the emitter writes `switch (failure.Code) { }`, which is CS1522 ("Empty switch block", level 1).
- The endpoint still takes the observer parameter and gets an uncalled private `ObserveFailure`. Neither of those warns: the parameter is read inside ObserveFailure, so there is no CS9113, and unused private methods are only an analyzer hint.
- The output still builds, but with one warning. Before round 2 the same source emitted `failure.Code switch { _ => "" }` and built warning-free. The project's generated-C# norm is 0 warnings (UsableV1 bar 3, EnsureStepCompiles:143, MatchingCodebases).
- No current test has this shape: Tasks and orders declare every code.
- Fix: one predicate, "the endpoint has at least one failure with a declaration", in place of `endpoint.Failures.Count > 0` at all four sites: the constructor parameter (about line 726), the catch call (about line 776), the helper (about line 780) and the spec `Factory.Create` argument (about line 844). Add one compile-warning-free row to an existing test.
- If the facilitator prefers, this can be a follow-up alongside F5, which is the same "which failures have a declaration" rule.

### R2-2 MINOR (test wording): the new theory equates "declared" with `endpoint.Failures`
- `The_catch_builds_one_event_per_declared_code...` takes `declared = endpoint.Failures`. The generator emits an arm only for codes that have a declaration. These agree for Tasks and orders, but the theory would fail on a module with an endpoint-level undeclared code, even though the generator behaves as ruled there.
- It only runs on tasks and orders, so there is no false result today. Note it for when R2-1 or F5 lands.

### R2-3 MINOR (docs nit): "Only declared failure codes are observed" (TRACEABILITY.md) is slightly loose
- An endpoint-level `ensure ... else Code` is a legitimate failure that the endpoint's specs can assert, yet it is not observed and has no map record.
- Suggested wording: "Only failures that a `fails with` declaration reaches are observed (the same failures that have a map record)."

### Checked, no finding
- **Name collision with the new private `ObserveFailure`.** The endpoint class body is only `Configure`, `HandleAsync` and `ObserveFailure`. Avow handlers live in separate `<Endpoint>Handler` classes, so user names never become members of the endpoint class.
  - Effect constructor parameters are lowercased effect names. The observer parameter is `failureObserver`; an effect `FailureObserver` becomes `failureobserver`, which is distinct.
  - FastEndpoints' `Endpoint<,>` has no `ObserveFailure` member.
  - A user type named `ObserveFailure` would be shadowed by the method in invocation position. The call still binds to the method, and the type is not used in that class.
  - `failureEvent` and the catch variable `failure` are in different scopes.
- **A code the declaration walk cannot find.** It is silently unobserved, consistent with the map having no record for it. Doc wording is covered in R2-3; the warning edge case is R2-1.
- **Endpoints with no failures at all.** No parameter, no helper, no switch, and no extra spec argument, as R165-2 requires (unchanged).
- **Duplicate case labels.** None: `endpoint.Failures.Distinct(StringComparer.Ordinal)`, and the resolver rejects a repeated failure in `fails with` (AV329).
- **Pins.** The orders hash moved to df716c5c..., and ExistingTestsKept172 follows with a 7-hex prefix (as in round 1). The golden matches the new emission: no `@`, no `failure.Code` in any event, `FailureStepIds` unqualified and now reserved.
- **Reserving `FailureStepIds`.** It is reserved for every module, so a module with no failures that declared `type FailureStepIds` now gets AV341. This is consistent with the other reserved names, and acceptable.

## What I could not check
- No dotnet run. That CS1522 fires on `switch (failure.Code) { }` is from my knowledge of the C# compiler, not from a build.
- I relied on the facilitator's green proof 4 (1583 of 1583) and round 3 red proof (6 failed as predicted) for pass and fail status. I did not run them.
- I did not read review-grok.md, per the brief's rule on review files.
- Follow-ups F5, bounded dispatch, and the effect named AvowryFailureObserver are out of scope per R165-3.
