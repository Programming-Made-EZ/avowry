# ADR: Extension map

- **Status:** Record of Decider D89 (card 141). Docs only. No compiler change.
- **Date:** 2026-10-28
- **Frozen by D72:** `docs/CHEATSHEET.md`, `docs/GRAMMAR.md` and `tests/Phrase.Acceptance.Tests/Fixtures/orders.avowry` stay as they are. This file is new. Card 143 is the later re-freeze that splits the cheat sheet.

## Context

Dana needs one map of what is core and what is an extension, plus an audit of the domain words that still sit in the core, so the language grows by clean breaks. the product owner's direction (2026-10-28, recorded in D89) moves web concepts into a web extension and gives every other application domain the same shape. The conditions on that Decider are the audit list below and the loading rule in the last section. VISION ruling 3 still stands beside this map: a base library of universal patterns, and one adapter per target language.

## Core

The core is domain-neutral. It is the language, not a catalogue of applications.

It holds:

- avows, whose names are sentences with typed holes
- specs written with Given/When/Then, including `when called`, `returns`, `fails with` and `succeeds`
- types: Text, Int, Decimal, Bool, Id, List and Option
- control and error patterns: `let`, `steps` on a composite avow, `fail with`, and linear flow
- effects as a mechanism: an avow declares `effects none` or a list of effect names, and a caller declares the union
- failure codes as a mechanism: declared names an avow may raise, carried as values the specs can name

A leaf may hold a `native <target>` block. That mechanism is core. The closed list of target names is adapter coupling and is audited below. Named effects such as Scheduler are not core keywords.

## Extension

An extension is a library with its own cheat sheet or index, loaded only when a project uses it, with its own keywords, diagnostics and adapter-side emitters.

The base library is the other library, and it is domain-neutral. It is the index of universal avows (VISION ruling 3). It ships with Avowry. A domain extension does not. `ensure` and `compute` are base-library verbs. They are vocabulary, not core keywords, and they are not a domain extension.

A target-language adapter is orthogonal to a domain extension. The adapter turns the resolved module into one language. A C# adapter serves every domain extension: the same C# adapter emits web endpoints, and later a desktop avow or a SQL avow, when those extensions are loaded. FastEndpoints, the Web SDK and xUnit are bindings inside the C# adapter. They are not the adapter seam itself. The seam is `ITargetAdapter` (`src/Avowry.Compiler/ITargetAdapter.cs` lines 9-15). How those bindings are selected, and how a project names architecture and coding standards, is [ADR-library-binding-layering.md](ADR-library-binding-layering.md) (card 152).

## Testing split

Avowry's own specs and Given/When/Then are core. They are the reliability backbone: one spec means the same thing on every target, and the resolved spec is part of the module the adapter receives.

Test-framework specifics are extension territory. xUnit and NUnit wiring, mocking and fixtures belong to a testing-frameworks extension. The adapter side of spec-to-test generation belongs there too: the C# adapter's xUnit project is that binding, written today inside `CSharpProvider` because the extension does not exist yet. The core keeps the spec. The extension decides which test framework prints it.

## Audit

Every row is a domain-specific item still living in the core tree. Line numbers are the lines read for this card. The proposed home is where the item goes when card 142 (web) or a later extension card moves it. Nothing in this table is moved by this ADR.

| Item | Where it lives today | Proposed home |
|---|---|---|
| HTTP verbs GET, POST, PUT, PATCH and DELETE | `src/Avowry.Compiler/Syntax/Parser.cs` line 17; `docs/GRAMMAR.md` line 75; `docs/CHEATSHEET.md` line 3 | web extension |
| endpoint, with its input, output and steps | `docs/GRAMMAR.md` lines 67-73; `src/Avowry.Compiler/Syntax/Parser.cs` lines 188 and 438-486; `src/Avowry.Compiler/Syntax/Ast.cs` lines 48-56; `src/Avowry.Compiler/Semantics/Model.cs` lines 37-47; `docs/CHEATSHEET.md` lines 59-67 | web extension. `steps` on a composite phrase stays core |
| responds, including the step respond with | `docs/GRAMMAR.md` lines 84 and 110; `src/Avowry.Compiler/Syntax/Parser.cs` lines 546 and 769; `src/Avowry.Compiler/Syntax/Ast.cs` lines 65 and 116; `src/Avowry.Compiler/Semantics/Model.cs` lines 54 and 92; `src/Avowry.Compiler/Syntax/SourceFormatter.cs` lines 23 and 70; `src/Avowry.Adapters.CSharp/CSharpProvider.cs` lines 382 and 405; `docs/CHEATSHEET.md` lines 67 and 71 | web extension |
| when handled | `docs/GRAMMAR.md` line 107; `src/Avowry.Compiler/Syntax/Parser.cs` lines 754-759; `src/Avowry.Compiler/Syntax/Ast.cs` line 99; `src/Avowry.Compiler/Semantics/Resolver.cs` lines 354-356; `src/Avowry.Adapters.CSharp/CSharpProvider.cs` line 402; `docs/CHEATSHEET.md` line 70 | web extension. `when called` stays core |
| route parameters | `docs/GRAMMAR.md` lines 76-77; `src/Avowry.Compiler/Syntax/Parser.cs` lines 489-516 | web extension |
| reserved name input | `docs/GRAMMAR.md` lines 70 and 105; `src/Avowry.Compiler/Syntax/Parser.cs` lines 465 and 739; `src/Avowry.Compiler/Syntax/Ast.cs` lines 111-112; `src/Avowry.Compiler/Semantics/Resolver.cs` lines 145, 359 and 378-386; `src/Avowry.Compiler/Semantics/PType.cs` lines 26-27; `docs/CHEATSHEET.md` lines 60 and 69 | web extension. Inside a phrase template the word input stays an ordinary word (`docs/GRAMMAR.md` line 33) |
| HTTP 422 endpoint mapping | `docs/GRAMMAR.md` line 240; `src/Avowry.Adapters.CSharp/CSharpProvider.cs` lines 346 and 371; `reference/csharp/Orders.Api/CreateOrder.cs` lines 29 and 47 | web extension's C# adapter binding. Failure codes stay core; the status number is web |
| PH209 unknown HTTP verb | `docs/GRAMMAR.md` line 230; `src/Avowry.Compiler/Syntax/Parser.cs` line 449 | web extension diagnostics |
| PH304 duplicate endpoint name, or same verb and route | `docs/GRAMMAR.md` line 170; `HANDOFF.md` line 102; `src/Avowry.Compiler/Semantics/Resolver.cs` lines 135 and 137 | web extension diagnostics |
| PH322 respond with outside an endpoint, or in an endpoint with no output | `docs/GRAMMAR.md` line 178; `HANDOFF.md` line 110; `src/Avowry.Compiler/Semantics/Resolver.cs` line 208 | web extension diagnostics |
| PH323 endpoint with output never responds | `docs/GRAMMAR.md` line 179; `HANDOFF.md` line 111; `src/Avowry.Compiler/Semantics/Resolver.cs` line 148 | web extension diagnostics |
| PH330 wrong trigger | `docs/GRAMMAR.md` line 185; `HANDOFF.md` line 117; `src/Avowry.Compiler/Semantics/Resolver.cs` lines 290 and 356 | The phrase arm (`when called`, line 290) stays core. The endpoint arm (`when handled`, line 356) moves to the web extension |
| PH331 givens wrong | `docs/GRAMMAR.md` line 186; `HANDOFF.md` line 118; `src/Avowry.Compiler/Semantics/Resolver.cs` lines 301, 309, 315, 326 and 380-388 | Hole bindings (lines 309, 315 and 326) stay core. The reserved name input (lines 301 and 380-388) moves to the web extension |
| PH333 outcome the unit cannot have | `docs/GRAMMAR.md` line 188; `HANDOFF.md` line 120; `src/Avowry.Compiler/Semantics/Resolver.cs` lines 419, 432 and 455 | A phrase with no return type stays core, and so does phrase succeeds on line 455. Line 419 also rejects returns on an endpoint, and line 432 rejects responds; those two endpoint outcomes move to the web extension |
| FastEndpoints references in the C# emitter | `src/Avowry.Adapters.CSharp/CSharpProvider.cs` lines 150, 204 and 316; `reference/csharp/Orders.Api/Orders.Api.csproj` line 9; `reference/csharp/Orders.Api/Program.cs` line 4 | web extension's C# adapter binding |
| Microsoft.NET.Sdk.Web | `src/Avowry.Adapters.CSharp/CSharpProvider.cs` line 142; `reference/csharp/Orders.Api/Orders.Api.csproj` line 1 | web extension's C# project template |
| emitted Program.cs web host | `src/Avowry.Adapters.CSharp/CSharpProvider.cs` lines 31 and 200-210; `reference/csharp/Orders.Api/Program.cs` lines 6-13 | web extension's C# adapter binding |
| xUnit specs project | `src/Avowry.Adapters.CSharp/CSharpProvider.cs` lines 8 and 154-170; `reference/csharp/Orders.Specs/Orders.Specs.csproj` lines 11-16 | testing-frameworks extension, on the adapter side of spec-to-test generation |
| verb ensure | `docs/GRAMMAR.md` line 143; `docs/CHEATSHEET.md` line 42; `src/Avowry.Compiler/Semantics/Model.cs` lines 26-27 | base-library index. It is a domain-neutral guard verb, not a core keyword |
| verb validate | `docs/GRAMMAR.md` line 144 | web extension index. The grammar defines it as a structural check of input shape. It is not a core keyword |
| verb find | `docs/GRAMMAR.md` line 145 | data and databases extension. It is not a core keyword |
| verb load | `docs/GRAMMAR.md` line 146 | data and databases extension. It is not a core keyword |
| verb compute | `docs/GRAMMAR.md` line 147 | base-library index. A pure calculation is domain-neutral. It is not a core keyword |
| verb save | `docs/GRAMMAR.md` line 148 | data and databases extension, as a declared effect. It is not a core keyword |
| verb publish | `docs/GRAMMAR.md` line 148 | messaging and queues extension, as a declared effect. It is not a core keyword |
| Scheduler, Clock and Cancellation (Decider D21) | `docs/decisions/language-and-grammar.md` (record D21); `docs/retros/seeds.md` line 109 | background jobs extension, as declared effects. D21 keeps them out of the core grammar. They have no compiler symbol today |
| Events (Decider D21) | `docs/decisions/language-and-grammar.md` (record D21); `docs/retros/seeds.md` line 109 | messaging and queues extension, as a declared effect. D21 keeps it out of the core grammar. It has no compiler symbol today |
| native csharp blocks | `docs/GRAMMAR.md` lines 62-65; `src/Avowry.Compiler/Syntax/Parser.cs` lines 16 and 315; `src/Avowry.Compiler/Lexing/Lexer.cs` lines 133-136; `src/Avowry.Compiler/Syntax/Ast.cs` line 46; `src/Avowry.Adapters.CSharp/CSharpProvider.cs` line 59; `docs/CHEATSHEET.md` line 22 | The native-block mechanism stays core. The target name csharp, the closed target list, and the emitter's demand for a native csharp block are C# adapter coupling, orthogonal to every domain extension |
| AllowAnonymous on every emitted endpoint | `src/Avowry.Adapters.CSharp/CSharpProvider.cs` line 360; `reference/csharp/Orders.Api/CreateOrder.cs` line 35 | auth and security extension. Until that extension exists, a web binding that always allows anonymous callers is web-adapter behaviour, not core |
| HTTP 200 via Send.OkAsync | `src/Avowry.Adapters.CSharp/CSharpProvider.cs` line 366; `reference/csharp/Orders.Api/CreateOrder.cs` line 42 | web extension's C# adapter binding |
| Endpoint.Input and Endpoint.Output type names | `src/Avowry.Compiler/Semantics/PType.cs` lines 5 and 26-32; `docs/GRAMMAR.md` line 200 | web extension |
| PH201 and the core keyword line name endpoint beside module, type and phrase | `docs/GRAMMAR.md` lines 31 and 222; `src/Avowry.Compiler/Syntax/Parser.cs` line 193; `docs/CHEATSHEET.md` line 3 | The core declaration set is module, type and phrase. endpoint, input, output and respond with move to the web extension's keyword list |
| PH324 names respond with as a terminal step | `docs/GRAMMAR.md` lines 180 and 198; `src/Avowry.Compiler/Semantics/Resolver.cs` line 167 | Unreachable-step is a core control pattern. The respond with arm moves to the web extension with that step |
| PH334 endpoint failure list | `src/Avowry.Compiler/Semantics/Resolver.cs` lines 447-449 | The phrase arm of PH334 stays core. The endpoint arm moves to the web extension |

## Candidate domains

Each domain below is its own extension, with its own cheat sheet or index. Another domain extension is built only when a real use needs it, and only web is built now (card 142). Card 142 has not landed, so web is not in the tree yet. The status of every row, including web, is NOT BUILT.

| Domain | Status | What would make it real |
|---|---|---|
| web/HTTP | NOT BUILT | card 142, the only domain extension to build now |
| desktop UI (WinForms, WPF) | NOT BUILT | a real desktop use |
| mobile | NOT BUILT | a real mobile use |
| data and databases (queries, transactions, migrations) | NOT BUILT | a real persistence use beyond an in-memory effect |
| testing frameworks | NOT BUILT | a second test framework, or a split of the xUnit emitter |
| messaging and queues | NOT BUILT | a real publish or events use |
| file and IO | NOT BUILT | a real file use |
| auth and security | NOT BUILT | a real auth use |
| background jobs | NOT BUILT | a real scheduler, clock or cancellation use |

## How a project uses an extension

A project loads an extension in one of two ways, and the choice is deterministic (D89). The web extension loads implicitly when a file uses an endpoint, a verb or a route. The same file can name it with an explicit `uses web` line. The same source always selects the same extensions. Card 142 is the compiler slice that partitions keywords into core and web under that rule. This ADR only records the rule.

Other domain extensions use the same two doors when they exist: an explicit `uses <domain>` line, or a keyword that belongs only to that domain. No such keyword is defined here.

## Consequences

Card 142 can partition the compiler along the web rows above without a behaviour change and without editing the frozen docs. Cards 131, 133 and 135 then extend the web part. Card 143 splits the scored cheat sheet into a core sheet and a web sheet, under a new Decider and a fresh baseline. The testing-frameworks, data, messaging, auth and background-jobs rows wait until a real use needs them.

## Provisional conventions

These behaviours exist only because Avowry has no syntax for them yet. They are PROVISIONAL: later work must not depend on them silently, and each has a stated replacement.

| Convention | Where | Why provisional | Replacement |
|---|---|---|---|
| An effect named `{Record}Store`, used by an avow with that record type, where the record has an `id` field of type Text or Id, is a prototype store: the C# adapter emits `I{Record}Store` with `Save` (replace by id, else append), `Find`, `Remove(id)` returning whether an item was removed (card 133) and `List` (a snapshot), an `InMemory{Record}Store` whose four methods take a lock, a singleton registration in `Program.cs` and a spec double that derives from it (card 132, Decider D95) | `src/Avowry.Adapters.CSharp/CSharpProvider.cs` | Avowry has no syntax for effect operations; the `save`, `find` and `load` verbs are vocabulary, not declarations. A name-keyed convention is a prototype step, not a language design. | A language decision on effect operations, most likely carried by the data and databases extension (NOT BUILT), which declares `save`, `find`, `load` and `list` for a record and lets an adapter emit real stores. Until then cards 135 to 138 may use the convention for the Tasks prototype only, and each use is listed in the card (also: spec seeding through `Save` for `then succeeds` specs, which handles Text, Id and Bool fields (Bool seeds as false, card 135) and throws NotImplementedException for other types, and constructor injection of the store into endpoint classes). |
