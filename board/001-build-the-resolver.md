# Build the resolver

- **Story:** As an engineer (or an LLM) writing Phrase, I want every step bound to exactly one phrase and every value and spec checked, so that mistakes are caught at compile time with messages precise enough to fix without reading the compiler.
- **Acceptance criteria:**
  - Given the 83 resolver tests in `tests/Phrase.Compiler.Tests/Semantics`, When the resolver is implemented, Then they all pass and none is marked `Pending`
  - Given `orders.phrase`, When it is resolved, Then there are no diagnostics and the model matches `ResolverFixtureTests`
  - Given a step with a typo, When it is resolved, Then `PH001` quotes the closest template
  - Given any source, even one with syntax errors, When it is resolved, Then the resolver never throws
  - Given `PhraseCompiler.Compile` on `orders.phrase`, When it runs, Then it gets past the resolver and stops at the C# emitter
  - Given the CI gate, When it runs, Then it is green with zero warnings

## Review findings to fix before Done (Grok review of 59d8a9c and 84b5737, 2026-10-25, verdict CHANGES)

Fix each red-first in a new `tests/Phrase.Compiler.Tests/Semantics/ResolverHardeningTests.cs` (Given/When/Then comments):

- Still open after 45813e1 (repeated holes fixed): bare-word PH312, `none`, record literal, Option coercion, failed `let`, and moving the diagnostic table from HANDOFF.md into GRAMMAR.md. Name each test with a marker (`Bare_word`, `None_`, `Record_literal`, `Option`, `Failed_let`) so the Done-when check can see it.
- Never throw: a composite whose holes repeat (`add {n: Int} to {n: Int}`, parser reports PH206) throws at `Holes.ToDictionary`; a call throws at `Holes.Single`. Both must degrade to diagnostics or `PType.Error`.
- A bare word naming a variable skips the type check (`Bool` variable accepted by an `Int` hole). It must report PH312 like the path form.
- `none` fits only `Option<T>`; it is currently accepted for a `Text` hole.
- A record literal where the expected type is not a record must report PH312 (lists already do).
- `3` must fit `Option<Decimal>`, `"c-1"` must fit `Option<Id>` (a value of type T fits Option<T>, with literal widening).
- A failed `let` must still bind its variable as `PType.Error`, so one mistake gives one diagnostic.
- `PhraseCompiler.Compile` must run the resolver and throw `CompilationFailedException` on diagnostics, then stop at the emitter.
- `docs/GRAMMAR.md` lists every resolver diagnostic (PH001, PH002, PH010, PH300-PH335) and any rule the mob changed.

## Context

- Read first: `HANDOFF.md` (working agreement, suggested order and design, every diagnostic), then `README.md` and `docs/GRAMMAR.md`.
- Production code goes in `src/Phrase.Compiler/Semantics/Resolver.cs` (currently a stub). The output model in `Semantics/Model.cs` is already designed; change it only for a reason recorded in the reply.
- The tests are the spec. If one looks wrong, say so in the reply rather than quietly editing it.
- Work one test class at a time in the order `HANDOFF.md` gives, and remove a class's `Pending` trait in the same increment that turns it green.
- No feature flag: this is compiler internals with no user-facing surface yet. The architect may overrule.
- Stage explicit paths only, and never stage `.mob/`.
- Toolchain: .NET 8 SDK.

## Done when

The facilitator runs each command from the repo root; the card is done when all of them exit 0.

```
python -c "import pathlib,sys; sys.exit(sum('Trait(\"Status\", \"Pending\")' in p.read_text(encoding='utf-8') for p in pathlib.Path('tests/Phrase.Compiler.Tests/Semantics').glob('*.cs')))"
python -c "import pathlib,sys; sys.exit(0 if pathlib.Path('tests/Phrase.Compiler.Tests/Semantics/ResolverHardeningTests.cs').exists() else 1)"
python -c "import pathlib,sys; t=pathlib.Path('tests/Phrase.Compiler.Tests/Semantics/ResolverHardeningTests.cs').read_text(encoding='utf-8').lower(); sys.exit(sum(k not in t for k in ['bare_word','none_','record_literal','option','failed_let']))"
python -c "import pathlib,sys; g=pathlib.Path('docs/GRAMMAR.md').read_text(encoding='utf-8'); sys.exit(1 if 'until the resolver lands' in g or 'PH335' not in g else 0)"
python -c "import pathlib,sys; sys.exit(1 if 'NotImplementedException' in pathlib.Path('src/Phrase.Compiler/PhraseCompiler.cs').read_text(encoding='utf-8') and 'Resolver' not in pathlib.Path('src/Phrase.Compiler/PhraseCompiler.cs').read_text(encoding='utf-8') else 0)"
dotnet test Phrase.sln --filter "Status!=Pending"
```
