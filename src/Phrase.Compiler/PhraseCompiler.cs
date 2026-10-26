using Phrase.Compiler.Semantics;
using Phrase.Compiler.Syntax;

namespace Phrase.Compiler;

/// <summary>
/// Entry point: .phrase source in, target files out.
/// Pipeline: lexer (done) -> parser (done) -> resolver (done) -> IR -> provider
/// </summary>
public static class PhraseCompiler
{
    public static CompilationOutput Compile(string source, ITargetProvider provider)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(provider);

        var parsed = Parser.Parse(source);
        StopOnDiagnostics(parsed.Diagnostics);

        var resolved = Resolver.Resolve(parsed.File);
        StopOnDiagnostics(resolved.Diagnostics);

        throw new NotImplementedException(
            "Lexer, parser, and resolution are done. Next: the IR and the C# emitter.");
    }

    private static void StopOnDiagnostics(IReadOnlyList<Diagnostic> diagnostics)
    {
        if (diagnostics.Count > 0)
            throw new CompilationFailedException(diagnostics);
    }
}

/// <summary>Thrown when compilation stops on errors. Diagnostics are in source order of discovery.</summary>
public sealed class CompilationFailedException(IReadOnlyList<Diagnostic> diagnostics)
    : Exception("Compilation failed:\n" + string.Join("\n", diagnostics))
{
    public IReadOnlyList<Diagnostic> Diagnostics { get; } = diagnostics;
}
