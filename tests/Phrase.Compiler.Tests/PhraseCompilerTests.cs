using Phrase.Compiler;
using Phrase.Compiler.Semantics;

namespace Phrase.Compiler.Tests;

// Feature: Compiling through the public boundary
//   Resolver diagnostics stop compilation, and a valid program reaches the emitter.
public class PhraseCompilerTests
{
    private static readonly string Orders =
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "orders.avowry"));

    private static readonly ITargetAdapter Adapter = new TestAdapter();

    [Fact]
    public void Resolver_diagnostics_stop_compilation_at_the_public_boundary()
    {
        // Given syntactically valid source containing an unknown phrase call
        const string source = """
            module Test

            avow to "known phrase"
              native csharp
                return;

            endpoint Probe : POST /probe
              input
                request: Text
              steps
                mistyped phrase
            """;

        // When it is compiled through the public API
        var exception = Assert.Throws<CompilationFailedException>(() => Compile(source));

        // Then the resolver diagnostic is exposed to the caller
        Assert.Contains(exception.Diagnostics, diagnostic => diagnostic.Code == "PH001");
    }

    [Fact]
    public void Valid_orders_source_reaches_the_emitter_boundary()
    {
        // Given the valid orders.avowry acceptance fixture

        // When it is compiled through the public API
        var exception = Assert.Throws<NotImplementedException>(() => Compile(Orders));

        // Then resolution has completed and only the emitter remains
        Assert.Contains("emitter", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("resolver", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static CompilationOutput Compile(string source) => PhraseCompiler.Compile(source, Adapter);

    private sealed class TestAdapter : ITargetAdapter
    {
        public string Target => "test";

        public CompilationOutput Emit(ModuleModel module) =>
            throw new NotImplementedException(
                "Lexer, parser, and resolution are done. Next: the IR and the C# emitter.");
    }
}
