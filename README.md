# Phrase

A programming language for LLMs where **names are descriptions** and **specs are the truth**. Code at every level reads as its own summary, so the code is the context. Phrase compiles through a language-neutral IR to target providers; C# (FastEndpoints + xUnit) is first.

- Grammar and resolution rules: [docs/GRAMMAR.md](docs/GRAMMAR.md)
- First program: [tests/Phrase.Acceptance.Tests/Fixtures/orders.phrase](tests/Phrase.Acceptance.Tests/Fixtures/orders.phrase)
- What the C# provider should emit for it: [reference/csharp](reference/csharp)

## Layout

```
docs/GRAMMAR.md                    v0.1 grammar, resolution, lints, vocabulary
src/Phrase.Compiler                Lexer and Parser (done); PhraseCompiler.Compile stops at resolver
src/Phrase.Providers.CSharp        CSharpProvider (stub)
tests/Phrase.Compiler.Tests        inner-loop unit tests (lexer 51, parser 79)
tests/Phrase.Acceptance.Tests      outer-loop acceptance tests + harness
reference/csharp                   hand-written target output (FastEndpoints + xUnit)
.github/workflows/ci.yml           trunk CI: build, test, Pending excluded
```

## Running

```bash
dotnet test Phrase.sln                                # everything; the acceptance test is red by design
dotnet test Phrase.sln --filter "Status!=Pending"     # what CI gates on
```

## Test status

| Test | Status | Why |
|---|---|---|
| `ReferenceTarget.Reference_output_builds_and_every_spec_passes` | Green | Proves the target design compiles, all 6 specs pass, and the harness detects failures |
| `CompileEndpointToCSharp.Generated_csharp_builds_and_every_spec_passes` | Red (Pending) | Lexes and parses the fixture cleanly, then stops with `NotImplementedException` at the resolver |
| `Phrase.Compiler.Tests` (lexer) | Green, 51 tests | Layout, tokens, native blocks, PH100 to PH109, the full fixture, and 2,000 fuzzed mutations |
| `Phrase.Compiler.Tests` (parser) | Green, 79 tests | Declarations, templates, endpoints, steps, specs, PH200 to PH211, recovery, fixture, 2,000 fuzzed mutations |

The acceptance test counts specs in the `.phrase` source independently of the compiler, so a compiler that silently drops specs cannot pass.

## Next: the inner loop

Build inward, one unit-test suite per stage, each with Given/When/Then comments:

1. ~~**Lexer**: indentation to `INDENT`/`DEDENT`, comments, strings, raw `native` blocks~~ (done)
2. ~~**Parser**: AST for `module`, `type`, `phrase`, `endpoint`, `spec`~~ (done)
3. **Resolver**: phrase-call matching (PH001 unknown, PH002 ambiguous), hole type checks, PH010 duplicate skeletons
4. **IR**: the phrase graph the provider consumes (this is when `ITargetProvider.Emit(ir)` gets its signature)
5. **C# emitter**: types, phrases, endpoint, specs; compare against `reference/csharp`

When the acceptance test goes green, remove its `Pending` trait so CI gates on it.
