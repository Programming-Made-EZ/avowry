# Card 203 slice (architect seat, fresh Claude context, read-only; nothing was run, C# error codes are predictions from reading CSharpProvider.cs)

Committed verbatim from the seat's reply (the `&lt;` in two places stands for a less-than sign).

## 1. Code and message
- `PH341` is free (nowhere in src or docs). Message pattern: `The C# target reserves the name '{Name}' for {role}, so {kind} '{shown}' cannot use it; rename the {kind} declared on line {L}.` {kind}: type, endpoint, module, avow, or field 'f' of type 'T'. The CLI prefix gives `PH341 (L:C)`.

## 2. Guard, constants, lines
- Guard: `RequireNoReservedNames(module)` in `CSharpProvider.cs`, called first in `Emit`, before the `RequireSupportedAvow` loop; collects every hit and throws one `CompilationFailedException` sorted by line (the CLI writes nothing on a throw; `Emit` is pure over the model).
- Constants: a new `internal static class ReservedNames` in the adapter project: consts `AvowAttribute`, `AvowryFailure`, `Avows`, `AvowSpecs`, `Program`; file stems `RuntimeFile="AvowryRuntime"`, `TypesFile="Types"`, `BuildFile="AvowryBuild"`; sets `TypeNames={AvowAttribute, AvowryFailure, Avows, AvowSpecs}`, `MethodNames={Avows, AvowryFailure}`; module reserved: `Program`. The emitter uses the same consts in place of the literals at `:29-42`, `:229`, `:235`, `:266`, `:422`, `:433`, `:484`, `:639`, `:827`.
- Endpoint rule: rejected when `{E}.cs` is one of the Api file stems (`Program`, `AvowryRuntime`, `Types`, `Avows`, `AvowryBuild`) or `{E}Specs` equals `AvowSpecs`, derived from the same consts.
- Comparisons: type name ordinal on the set; method name `MethodName(phrase)` ordinal; field `CSharpFieldName(field.Name) == CSharpIdent(type.Name)`.
- Model lines: `Line` and `Column` init properties (default 0) on `ModuleModel`, `TypeModel`, `FieldModel`, set at `Resolver.cs:46` (module, from `file.Module?.At`), `:60` (type, `declaration.At`), `:775` (field, `field.At`); endpoints reuse `EndpointModel.Line` (column 1); avows reuse `AvowModel.Line`/`.Column`.
- Bytes: valid-source bytes unchanged (constants replace literals one for one); the golden test proves it. `with` copies init properties (`AvowryCompiler.cs:31` safe). No tracked `.avowry` file, inline test source or doc example hits any reject.

## 3. Reject verification and final ids
Corrections to the Ready draft:
- R9 is unreachable: `MethodName` joins hole names too (`:910`), so `fail {x: Text}` becomes `FailX`; a bare `fail` gives `Fail()`, a legal overload of `Fail(string)`. R9 is dropped; N6 becomes `avow-fail-no-holes`.
- R5 is wrong: it does not give CS0101. `files[".../AvowSpecs.cs"]` is silently overwritten at `:49` (`EndpointSpecsFileName`, `:795`), so the avow specs vanish and the build succeeds. The same overwrite happens for endpoints named `Types`, `Avows`, `Program`, `AvowryRuntime`, `AvowryBuild` (`:46-48`); these are new rows R11-R15 (so `AvowryRuntime` and `AvowryBuild` are not file-name-only for endpoints).
- R4 and R8 reject unconditionally (native blocks can use `AvowryFailure.Raise`, specs can use any type, so "only when used" is not decidable); their fixtures still need a use so the C# error is real: R4 a spec that builds `new AvowSpecs(...)` (predicted CS1729); R8 a failing avow so `Fail` is emitted (`:430`, `:433`) beside `AvowryFailure.Raise` (predicted CS0119).
- R1-R3 (`:229`, `:235`, `:422` against `Types.cs:395`), R6 (`:266`, CS0101), R7 (`:446`, CS0542) and R10 (CS0542, `Types.cs:395-396`) are right.

Final reject ids: R1 `type-avowattribute`, R2 `type-avowryfailure`, R3 `type-avows`, R4 `type-avowspecs`, R5 `endpoint-avow`, R6 `module-program`, R7 `avow-method-avows`, R8 `avow-method-avowryfailure`, R10 `field-named-like-its-type`, R11-R15 `endpoint-types`, `endpoint-avows`, `endpoint-program`, `endpoint-avowryruntime`, `endpoint-avowrybuild`. Common tail where an endpoint is needed: `avow to "ok {n: Int}"` / `native csharp` / `return;`, then `endpoint Ping : POST /ping`, `input n: Int`, `steps ok input.n`. Snippets: R1-R3 `type <Name>` with `n: Int`; R4 `type AvowSpecs` with `n: Int` plus an avow with hole `{s: AvowSpecs}` returning Int with a spec `given s = { n: 1 }` (check the literal syntax against GRAMMAR); R5 and R11-R15 the tail endpoint renamed; R6 `module Program`; R7 `avow to "avows"` with `native csharp`; R8 `avow to "avowry failure"` plus `avow to "check {n: Int}"` with `fails with Bad` and `if (n<0) Fail("Bad");`; R10 `type Email` with `email: Text`.
Near misses (all `runSpecs: true`, at least one spec): merged fixture `type-lookalikes` for N1-N5 (types `Avow`, `AvowryRuntime`, `AvowryBuild`, `Program`, `AVOWS`, plus an avow with a spec; one rule, an exact ordinal match, one `dotnet test` instead of five); N6 `avow-fail-no-holes` (`avow to "fail"` plus a failing avow). Whether they really build needs a `dotnet build`: N1 relies on Roslyn ignoring a non-attribute `Avow` for `[Avow]`; N4 on `M.Program` against the global `Program` (no `using M` in `Program.cs` unless stores exist); N6 on the `Fail()` and `Fail(string)` overloads.

## 4. Test plan
- Pending test: class `ReservedGeneratedNames`, method `Names_reserved_by_generated_csharp_report_a_diagnostic`, traits `Slice=203`, `Criterion=203-1`, `Status=Pending`, modelled on `UnsupportedCsharpConstructs.cs`. Row shape `Row(Id, Source, Marker, Name, KindWord)`, 14 rows. Per row: exit code exactly 1; no unhandled-exception or stack text; the diagnostic line matches `^PH341 \((\d+):(\d+)\) `; the line equals the marker line; the message contains `'Name'`, the kind word and `line N`; the `out` directory has no `*.cs` and no `*.avowrymap.json`. On the pre-change tree every row fails by assertion (exit 0, no PH341, `.cs` files present); the test writes its own temp sources.
- Allow-list: not needed (the test goes through `PhraseCli`, already in `ProcessStartCallers`); the test must not run `dotnet`.
- Red commit also carries the two near-miss fixtures (pass today) and raises the count in `tests/test_rename_196.py:240` from 23 to 25. `.gitattributes` already covers `corpus/*.avowry`.
- Green adds five reject corpus fixtures (R3, R5, R6, R7, R10), raises the count to 30, removes `Pending`, adds the guard, constants and model lines, and the `PH341` row in `GRAMMAR.md` (table after `:196`, sentence at `:162`). Run `tests/test_extension_map.py` and `tests/test_live_docs_266.py` for GRAMMAR coupling.

## 5. Risks
- R5 and R11-R15 are silent file overwrites, not C# errors: the red record's `dotnet build` evidence cannot cover them; show that `AvowSpecs.cs` lacks the avow facts instead.
- Names differing only in case would overwrite each other on a case-insensitive file system (endpoint `AVOWS` against `Avows.cs`): out of scope; a follow-up card.
- Unconditional rejection of R4 and R8 turns some builds that work today into rejections: the cost of a simple guard.
- Unverified: a type named `Specs` against the `M.Specs` namespace; not covered.

## Facilitator note (D222)
The final id list changes the Ready draft's list (R9 dropped as unreachable; R11-R15 added; N1-N5 merged into one fixture); the red record lists the Ready draft's rows first, then these changes with the reason.
