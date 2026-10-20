# Mob handoff: build the resolver

Welcome. This repo is **Phrase**, a language for LLM-maintained codebases where *the name is the spec*: every unit of behavior is named by a plain-English sentence with typed holes, and verified by its own Given/When/Then specs. Read [README.md](README.md) for the idea and [docs/GRAMMAR.md](docs/GRAMMAR.md) for the rules. Ten minutes on [orders.phrase](tests/Phrase.Acceptance.Tests/Fixtures/orders.phrase) is the fastest way in.

## Where things stand

| Stage | Status |
|---|---|
| Lexer | Done, 51 tests |
| Parser | Done, 79 tests |
| Source formatter | Done, 8 tests |
| **Resolver** | **Your job: 83 tests written and red, marked `Pending`** |
| C# emitter | Next, after the resolver |
| Acceptance test | Red (`Pending`), stops at the resolver stub |

The resolver's output, `ModuleModel` in [Semantics/Model.cs](src/Phrase.Compiler/Semantics/Model.cs), is already designed. It is the IR the C# provider will consume. You implement [Semantics/Resolver.cs](src/Phrase.Compiler/Semantics/Resolver.cs) to produce it.

## Working agreement

- **Trunk-based.** Commit to `main` in small steps; CI runs on every push and must stay green.
- **Red, green, refactor.** Pick one failing test, make it pass, refactor, commit. Tests are the spec; if a test looks wrong, discuss it as a mob, change it deliberately, and say why in the commit.
- **Pending is the to-do list.** Each resolver test class carries `[Trait("Status", "Pending")]`. When every test in a class passes, delete that trait in the same commit so CI starts guarding it.
- **Given/When/Then comments** in every new test.
- **Zero warnings.** `TreatWarningsAsErrors` is on.

```bash
dotnet test Phrase.sln --filter "Status!=Pending"                                       # what CI runs
dotnet test tests/Phrase.Compiler.Tests --filter "Status=Pending"                        # the remaining work
dotnet test tests/Phrase.Compiler.Tests --filter "FullyQualifiedName~ResolverDeclarationTests"   # one class
```

## Suggested order

Each class builds on the one before it. Helpers for building test programs are in [Resolved.cs](tests/Phrase.Compiler.Tests/Semantics/Resolved.cs).

1. **ResolverDeclarationTests** (16): type symbols, field types, phrase keys, PH010, endpoint shapes
2. **ResolverCallTests** (18): structural matching, PH001 with suggestions, PH002, scope, type checks
3. **ResolverFlowTests** (17): respond/fail rules, composites, failure and effect propagation, cycles
4. **ResolverSpecTests** (20): hole bindings, literal-only values, outcomes, endpoint specs
5. **ResolverFixtureTests** (12): the real program, plus 2,000 fuzzed mutations that must never throw

Then wire it into `PhraseCompiler.Compile` (resolve after parse, throw `CompilationFailedException` on diagnostics) so the acceptance test fails one stage further in, at the emitter.

## A design that fits the tests

This is a suggestion, not a requirement. The tests are the contract.

**Passes, in order**

1. **Types.** Register every user type name first (so forward references work), then resolve field types. Core types and arities are in `PType.Core`.
2. **Phrase library.** For each phrase, resolve hole and return types and compute its `Key` (lowercased words, holes as `_`). A second phrase with the same *word sequence* (holes ignored entirely) is PH010 and stays out of the library.
3. **Endpoints.** Check names and verb+route are unique. Register the input and output shapes as record types named `Probe.Input` / `Probe.Output`.
4. **Bind steps,** composites first, then endpoints. Scope is an endpoint's `input`, or a composite's holes, plus each `let` as it is reached.
5. **Effective failures.** A depth-first walk of the call graph: a leaf's failures are its declared ones; a composite's are declared plus its callees'. Re-entering a phrase while walking is a cycle (PH327). Endpoint failures are the union of callees plus its own `fail with` steps. Sort them ordinally.
6. **Effects.** A composite must declare every effect of every phrase it calls (PH328).
7. **Specs.** Last, because `then fails with X` needs effective failures.

**Matching a call (the heart of it)**

A template matches a call when both have the same number of parts and, position by position:

- a template word matches a call word with the same text (case-insensitive), and
- a template hole matches a call argument, *or* a call word that names a variable in scope.

No backtracking: one hole is always exactly one part. Exactly one template must match; zero is PH001, more than one is PH002. Only after a unique match do you type-check the arguments.

For PH001, rank templates by word-level edit distance from the call (holes and arguments both read as `_`) and quote the closest. If the call has a bare word where a hole belongs and some record variable has a field by that name, add a hint: *did you mean `input.items`?* LLMs make exactly this mistake.

**Type checking**

Check values against an expected type, rather than inferring and comparing:

- Paths: infer from scope, then field by field; the result must equal the expected type.
- `"text"` fits `Text` or `Id`. A whole number fits `Int` or `Decimal`; `2.5` fits only `Decimal`. `none` fits only `Option<T>`.
- A value of type `T` also fits `Option<T>`.
- Records need the exact field set of the expected record type (PH314 for missing or extra fields); lists check each item against the element type.
- Use `PType.Error` for anything that failed to resolve, and treat it as compatible with everything. One mistake should produce one diagnostic, not ten.
- In specs there is no scope: a path there is PH332 (spec data must be literals).

## Diagnostics to implement

Resolution and library: `PH001` unknown phrase, `PH002` ambiguous phrase, `PH010` duplicate wording.

| Code | Meaning |
|---|---|
| `PH300` | Unknown type |
| `PH301` | Wrong number of type arguments |
| `PH302` | Type name already taken (duplicate, or a core type) |
| `PH303` | Duplicate field (type, input or output) |
| `PH304` | Duplicate endpoint name, or same verb and route |
| `PH310` | Unknown variable (message lists what is in scope) |
| `PH311` | Unknown field, or a field read on a non-record (message lists the fields) |
| `PH312` | Type mismatch (message names both types) |
| `PH313` | Variable already defined |
| `PH314` | Record fields do not match the expected type |
| `PH320` | `let` of a phrase that returns nothing |
| `PH321` | Returned value discarded; use `let` |
| `PH322` | `respond with` outside an endpoint, or in an endpoint with no output |
| `PH323` | Endpoint with output never responds |
| `PH324` | Step after `respond with` or `fail with` is unreachable |
| `PH325` | Composite phrase with a return type (not supported in v0.1) |
| `PH326` | Composite fails with an undeclared code |
| `PH327` | Phrases call each other in a cycle |
| `PH328` | Composite calls a phrase whose effect it does not declare |
| `PH330` | Wrong trigger (`called` for phrases, `handled` for endpoints) |
| `PH331` | Givens wrong: unknown, missing or duplicate hole; `input` in a phrase spec; missing input in an endpoint spec |
| `PH332` | Spec value is a path, not a literal |
| `PH333` | Outcome the unit cannot have (`returns` with no return type, `responds` in a phrase spec, `returns` in an endpoint spec, `responds` with no output) |
| `PH334` | Expected failure the unit can never produce (message lists the ones it can) |
| `PH335` | More than one outcome in a spec |

Messages matter: an LLM reads them to fix its own code. Name the thing, say what was expected, and list the valid options where there are a few.

## Definition of done

- No `Pending` trait left in `tests/Phrase.Compiler.Tests/Semantics`
- `dotnet test Phrase.sln --filter "Status!=Pending"` green with zero warnings
- `PhraseCompiler.Compile` runs the resolver; the acceptance test now fails at the emitter
- GRAMMAR.md updated with the resolver diagnostics and any rule the mob changed

## After that

The C# emitter: turn `ModuleModel` into the files in [reference/csharp](reference/csharp), which already build and pass all six specs. When `CompileEndpointToCSharp` goes green, remove its `Pending` trait and Phrase compiles its first program.
