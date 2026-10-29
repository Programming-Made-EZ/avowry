# ADR: Library bindings and project profile

- **Status:** Proposed (card 152). Docs only. No compiler change. The facilitator records the Decider after review.
- **Date:** 2026-10-28
- **Frozen by D72:** `docs/CHEATSHEET.md`, `docs/GRAMMAR.md` and `tests/Phrase.Acceptance.Tests/Fixtures/orders.phrase` stay as they are. This file is new.
- **Extends:** [ADR-extension-map.md](ADR-extension-map.md) (card 141, Decider D89). VISION ruling 7 and Decider D99 are the reason this layer is written now: a spec that names a library would couple intent to one stack and make a second binding a rewrite of the spec.

## Context

Dana needs one written map of how an Avowry program chooses a concrete library, an architecture style and coding standards, without putting those choices into the spec. Card 141 already splits the language into a domain-neutral core, domain extensions, and a target-language adapter. FastEndpoints, the Web SDK and xUnit already sit inside the C# adapter as bindings, not as the adapter seam (`ITargetAdapter` in `src/Phrase.Compiler/ITargetAdapter.cs` lines 9-15). This ADR names the remaining layers: library binding under each adapter, and a project profile that selects bindings, architecture and standards.

Rationale: verifiability and preserved intent (VISION ruling 7) require the spec to stay about endpoints, routes and stores, so generated tests still mean the same thing when the host library changes. A profile file is the one place a team lead sets library and style preferences; the emitter and an LLM-as-compiler then honour the same file.

## Layers

Four layers form one selection chain: domain extension -> target-language adapter -> library binding -> project profile. Each layer has one job, so a change of library does not rewrite the spec, and a change of target language does not rewrite the domain concepts.

### Domain extension

A domain extension defines abstract concepts for one application domain. Web owns endpoint, route, verb, respond-with and HTTP status mapping. Data would own store, save, find, load and list. Testing-frameworks would own the adapter side of spec-to-test generation. The concepts are language-neutral. Card 141 records the map; card 142 partitions the compiler along it. This ADR does not move any keyword.

Rationale: abstract concepts are what a spec can name. `POST /tasks` and `then responds { task: ... }` stay true for FastEndpoints, Minimal APIs and controllers alike.

### Target-language adapter

An adapter turns a resolved module into one language. The C# adapter serves every domain extension. The seam is still `ITargetAdapter`. A second language is a second adapter, not a second binding.

Rationale: VISION ruling 3 keeps a base library of universal patterns and one adapter per target. Mixing "C#" with "FastEndpoints" in the same seam would make a second C# web library look like a second language.

### Library binding

A library binding lives under one adapter and maps one extension's abstract concepts to a concrete library.

| Extension | Adapter | Default binding | Possible later bindings |
|---|---|---|---|
| web | C# | FastEndpoints 8.3.0 | Minimal APIs; MVC controllers |
| data | C# | in-memory `{Record}Store` (provisional, card 132) | EF Core; Dapper |
| testing frameworks | C# | xUnit 2.9.2 | NUnit |

The C# emitter today is one default binding written in place, because the binding objects do not exist yet. Effect-free generated specs already call a pure handler (`CreateOrderHandler.Handle` in `reference/csharp/Orders.Api/CreateOrder.cs` lines 14-27). The FastEndpoints class around that handler is the HTTP binding. A second web binding would replace that class, the project file and the host bootstrap, and would keep the handler and the Avowry specs.

Rationale: one spec, many libraries. FastEndpoints is the compiled default because it is what Usable v1 emits (Decider D86, card 138). Minimal APIs or controllers stay possible so a team that already standardised on them is not blocked. Data bindings to EF Core or Dapper stay possible for the same reason, behind the data extension, which is NOT BUILT.

### Project profile

A project profile file selects, with smart defaults and an override:

- the bindings (web, data, test framework)
- the architecture style: vertical slices, clean or onion
- the coding standards: naming, nullable, analyzers, file layout, test framework

A project that omits the file gets the smart defaults: C# web to FastEndpoints, data to the in-memory prototype, architecture vertical slices (one endpoint file, shared `Phrases.cs` and `Types.cs`), nullable enable, implicit usings, xUnit, the current Api plus Specs project split. A present profile overrides only the keys it names. Omitted keys keep the defaults. The mapping is deterministic: the same spec and the same profile produce the same binding choice and the same file set.

Rationale: Dana and a team lead set library and style preferences once, in a file the compiler reads, instead of scattering them through native blocks or prompt text. Smart defaults keep the prototype path short. An override is how a later real project differs without forking the emitter.

### Honouring the profile

Both production paths honour the same profile.

- The deterministic C# emitter (`src/Phrase.Adapters.CSharp/CSharpProvider.cs`) will read the profile when a later card wires it. Until that card, the emitter is the default profile compiled in place.
- An LLM-as-compiler (hash-pinned output, card 029) receives the profile next to the spec and must emit the selected binding. It must not invent a library the profile did not name.

Rationale: two compilers that disagree on the host library would make generated tests lie. Honouring one profile keeps the name-is-the-spec rule intact for both paths.

### Same spec, every binding

The same spec and the generated tests that come from it verify every binding. A binding is accepted when those tests pass against its emitted host. Card 136 already observes the Tasks app over real HTTP; that observation is binding-neutral. Factory.Create checks pin today's FastEndpoints default and are not the only verifier.

Rationale: if each binding invented its own tests, a second binding could drift from the spec. Shared specs are the reliability backbone (card 141 testing split).

## Logic sources, in order

Implementation logic comes from layered sources, in this order:

1. **Library pattern.** Universal patterns such as validation and guard steps or record update are expressed in Avowry syntax and the base library. This moves logic out of native blocks and preserves it as portable intent.
2. **Adapter idiom.** The target-language adapter supplies an idiomatic, high-quality implementation of the pattern. The library binding and project profile select the libraries and style, so generated code follows the target's established practices instead of being a raw minimal translation.
3. **LLM-generated body.** Where no library pattern exists, an LLM generates the body from the avow's specs, follows idioms supplied by the adapter, and relies on the generated tests for verification.
4. **Native escape hatch.** A native block remains available when the preceding sources cannot express the logic, but it is an escape hatch rather than the default.

For card 159, the core definition is an `ensure` step with declared failure semantics. The C# adapter implements that pattern as an idiomatic guard clause. The project profile can select a binding or coding style that changes the guard's host-library representation without changing the `.phrase` source. For card 160, the core definition is an immutable record-update pattern. The C# adapter implements it as a record `with`-expression; the profile can change surrounding layout or selected library conventions while the update intent stays the same.

These sources make `.phrase` hold intent and specs, not target-language code. Language-specific quality belongs in adapters: an improvement to generated C# belongs in the C# adapter, not in the core. The share of logic still in native blocks is tracked as a trade-off signal, not a target; it shows where portability is being exchanged for an immediate escape hatch without turning a metric into a quota.

Rationale: putting universal intent in the core makes it portable, while putting target-specific quality in adapters lets each output language be genuinely idiomatic. Specs provide the contract for generated bodies when the reusable library has a gap. Native code remains practical without becoming the normal source of application logic.

## Audit of FastEndpoints in the C# emitter

Every row is a place FastEndpoints (or its in-process test host) is wired today. Line numbers are the lines read for this card. The seam is what a second C# web binding would replace. Nothing in this table is moved by this ADR.

| Place | Where it lives today | Seam it needs |
|---|---|---|
| Project template | `src/Phrase.Adapters.CSharp/CSharpProvider.cs` lines 143-154 (`Microsoft.NET.Sdk.Web` at 144, `PackageReference` FastEndpoints 8.3.0 at 152); `reference/csharp/Orders.Api/Orders.Api.csproj` lines 1 and 9; `scripts/make-codebase.py` lines 98-108; generated Tasks.Api.csproj from demo/tasks/Tasks.phrase | Web project-file seam: the binding writes the SDK, package and nullable/implicit-usings property group. The module name and TargetFramework stay adapter-level. |
| Program.cs | `src/Phrase.Adapters.CSharp/CSharpProvider.cs` lines 202-226 (`using FastEndpoints` at 209, `AddFastEndpoints` at 214, `UseFastEndpoints` at 223, `{Record}Store` singleton at 215-219); `reference/csharp/Orders.Api/Program.cs` lines 4, 7 and 10; `scripts/make-codebase.py` lines 134-145 | Host-bootstrap seam: the binding writes service registration and middleware. Store singletons belong to the data-binding seam, composed here. `public partial class Program` stays so in-process tests can share the host type. |
| Endpoint classes | `src/Phrase.Adapters.CSharp/CSharpProvider.cs` lines 485-580 (`using FastEndpoints` at 492, `Endpoint<TRequest, TResponse>` at 533-536, verb methods `Get`/`Post`/`Put`/`Patch`/`Delete` at 541-548, `AllowAnonymous` at 549, `HandleAsync` at 551, `Send.OkAsync` at 562 and 566, `Send.ErrorsAsync(422)` at 575); `reference/csharp/Orders.Api/CreateOrder.cs` lines 4 and 30-48; `scripts/make-codebase.py` lines 366-419; generated CreateTask.cs from demo/tasks/Tasks.phrase | HTTP-binding class seam: the binding writes the type that exposes the route. The pure handler (`CreateTaskHandler.Handle`) and the request/response records stay. Status mapping (200 empty or body, 422 with the failure code) is a web-extension contract the binding must honour, not a FastEndpoints type. |
| Effectful generated specs | `src/Phrase.Adapters.CSharp/CSharpProvider.cs` lines 603-669 (`using FastEndpoints` at 612, `Factory.Create<...Endpoint>` at 640, `HandleAsync` and status 200/422 at 644-661); generated CreateTaskSpecs.cs from demo/tasks/Tasks.phrase (Factory.Create plus InMemoryTaskStore) | In-process HTTP invoker seam: the binding writes how an effectful spec constructs the host, injects the store and reads status plus body. Effect-free specs already call the handler and need no FastEndpoints types (same file, lines 671-709). |
| Test helpers | `tests/Phrase.Acceptance.Tests/GetEndpointsCompile.cs` line 123 (`Factory.Create<GetItemEndpoint>`) and line 131 (`Get("/items/{id}")`); `tests/Phrase.Acceptance.Tests/UsableV1.cs` lines 43-53 (Bar 2 asserts generated files contain `Endpoint<`); `tests/Phrase.Acceptance.Tests/CSharpBaselineAcceptance.cs` lines 570-610 and 786 (invented FastEndpoints replies and prompt); `tests/Phrase.Acceptance.Tests/CompileEndpointToCSharp.cs` line 9; `tests/Phrase.Acceptance.Tests/Support/GeneratedApi.cs` (starts the Web SDK host on a loopback port, no FastEndpoints type) | Default-pin versus HTTP-observation seam: helpers that assert FastEndpoints identifiers pin today's default and stay on that default. `GeneratedApi` and card 136 already observe real HTTP; a second binding that still hosts as `Microsoft.NET.Sdk.Web` reuses them. New cards add HTTP or handler assertions, not new `Factory.Create` pins, unless the card is explicitly about the default binding. |
| Benchmark scorers of cards 110 and 111 | Plain C# arm, card 110: `tools/Phrase.BenchCs/Program.cs` lines 311, 323 (FastEndpoints package), 505 (`using FastEndpoints`), 514 (`typeof(IEndpoint)`), 525 (`typeof(Endpoint<,>)`), 536-540 (reflective `Factory.Create` lookup through `typeof(Factory).GetMethods`); Phrase arm, card 111: `tools/Phrase.Bench/Program.cs` lines 1021 (`using FastEndpoints`), 1049 (`IEndpoint`), 1121-1123 (`Endpoint<,>` and `Endpoint<>`), 1148-1169 (constructor-injected store), 1207-1211 (reflective `Factory.Create` lookup through `typeof(Factory).GetMethods`) | Reflective scorer-invoker seam: both arms discover both the HTTP type and the default binding's factory method at runtime. A second binding needs an equivalent reflective invoker, keyed by route and request shape rather than by `IEndpoint` or `Factory`, and shared with the generated-spec invocation strategy. The C# arm prompt may still name FastEndpoints while FastEndpoints remains that arm's default library. |
| {Record}Store provisional convention | `src/Phrase.Adapters.CSharp/CSharpProvider.cs` lines 229-267 (name-keyed {Record}Store with id of Text or Id), 278-344 (I{Record}Store with Save, Find, Remove, List and a locked InMemory{Record}Store), 215-219 (singleton in Program.cs), 373-380 (emitted in Phrases.cs); generated Tasks.Api/Phrases.cs ITaskStore / InMemoryTaskStore; listed as PROVISIONAL in `docs/adr/ADR-extension-map.md` lines 107-113 | Data-binding seam: Save, Find, Remove and List are the abstract store operations. EF Core or Dapper would implement the same four members against a real database. Until the data extension exists, the name-keyed convention stays provisional. Cards on the prototype path may use it for Tasks only, each use listed on the card. It is not a FastEndpoints type, but Program.cs registers it next to AddFastEndpoints, so a second web binding must compose with it rather than swallow it. |

Other mentions, no seam needed: `scripts/usable-v1-report.py` (the `C#/FastEndpoints` label); `scripts/check-extension-map.py` (a needle); `tests/Phrase.Acceptance.Tests/CSharpBaselineAcceptance.cs` and `tests/Phrase.Acceptance.Tests/UsableV1.cs` (comments).

## Hard-wire rule

The prototype path may keep today's FastEndpoints default compiled in place: the package, `Program.cs`, the `Endpoint<T>` class, `Factory.Create` in effectful specs, and tests that pin those identifiers. Usable v1 (card 138) and the Tasks demo stay on that default. The in-memory `{Record}Store` stays the provisional data default.

Later cards may not hard-wire a library in any of these ways:

- A `.phrase` spec, a Given/When/Then, or `ModuleModel` names FastEndpoints, Minimal APIs, controllers, EF Core, Dapper, xUnit or NUnit.
- A new FastEndpoints call site is added outside the rows above without updating this audit in the same change.
- The core grammar, the cheat sheet, or `orders.phrase` grows a library name (also frozen by D72).
- An LLM-as-compiler prompt tells the model to emit a library the profile did not select, or to ignore the profile.
- The `{Record}Store` name convention is treated as language design rather than a provisional prototype (card 141).
- A second binding is built because it would be tidy, rather than because a real project needs it.

Rationale: D99 already said the prototype path must not hard-wire FastEndpoints beyond today's default. The list above is that rule in a form a later card can fail. Pinning the default is allowed so the current app keeps compiling. Spreading new FastEndpoints types into the IR or the spec is what would block a second binding.

## Second binding

A second binding is built only on a real need. The default stays FastEndpoints. The specs never name a library.

A real need is a project that cannot ship on FastEndpoints (a host standard, a platform constraint, or a measured failure of the default on that codebase). A binding is not added for taste, symmetry, or an unvalidated possibility that another hosting style could be useful later. When a real need arrives, the work is a thin card that implements one binding behind the seams in the audit, proves the same specs and generated tests pass, and leaves FastEndpoints as the default for every project that does not override the profile.

Rationale: VISION ruling 6 and Decider D89 already apply this shape to domain extensions (only web is built now). Bindings follow the same increment: one default that works, the seams written down, the next library when evidence demands it. Building Minimal APIs or EF Core now would spend the prototype path on a second stack before the first stack has more than an in-memory Tasks app.

## Profile sketch

The following `phrase.profile.toml` is a sketch, NOT implemented. No compiler reads it. No file of this name is added to the tree by this card. It exists so Dana can see the shape of an override.

```toml
# sketch: phrase.profile.toml — NOT implemented
# A project that omits this file gets the smart defaults commented below.

[bindings]
web = "fastendpoints"   # C# default. possible: "minimal-apis", "controllers"
data = "in-memory"      # prototype default. possible: "ef-core", "dapper"

[architecture]
style = "vertical-slices"  # possible: "clean", "onion"

[standards]
naming = "pascal-records"
nullable = true
analyzers = ["nullable"]
file-layout = "feature-folders"
test-framework = "xunit"   # possible: "nunit"
```

Rationale: a small TOML file at the project root is enough to name every choice the layers require, including the test framework as a coding standard. YAML would carry the same keys; TOML is the sketch because it matches other .NET adjacent manifests. Implementing the reader, the override merge and the second binding is later work, each behind a red test.

## Consequences

- Card 152 is docs only. `CSharpProvider` stays the FastEndpoints default compiled in place. The frozen scored docs are untouched.
- Later emitter cards that touch a row in the audit update this table in the same change, or they are adding a hard-wire this ADR forbids.
- A profile reader, a second web binding and a real data binding each wait for a real need and a thin card. The sketch is not a backlog of those cards.
- The LLM-as-compiler path (card 029) and the cheat-sheet-plus-index path (card 140) treat the profile as part of the project, next to the spec, when they exist.
- Card 141's extension map remains the map of core versus domain. This file is the map of library versus spec.
