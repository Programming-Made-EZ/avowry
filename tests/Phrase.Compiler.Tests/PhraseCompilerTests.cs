using Phrase.Compiler;

namespace Phrase.Compiler.Tests;

// Feature: Compiling through the public boundary
//   Resolver diagnostics stop compilation, and a valid program reaches the emitter.
public class PhraseCompilerTests
{
    private static readonly string Orders =
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "orders.phrase"));

    private static readonly ITargetProvider Provider = new TestProvider();

    [Fact]
    public void Resolver_diagnostics_stop_compilation_at_the_public_boundary()
    {
        // Given syntactically valid source containing an unknown phrase call
        const string source = """
            module Test

            phrase "known phrase"
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
        // Given the valid orders.phrase acceptance fixture

        // When it is compiled through the public API
        var exception = Assert.Throws<NotImplementedException>(() => Compile(Orders));

        // Then resolution has completed and only the emitter remains
        Assert.Contains("emitter", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("resolver", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static CompilationOutput Compile(string source) => PhraseCompiler.Compile(source, Provider);

    private sealed class TestProvider : ITargetProvider
    {
        public string Target => "test";
    }
}
