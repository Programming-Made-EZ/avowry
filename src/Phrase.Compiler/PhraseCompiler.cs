using Phrase.Compiler.Syntax;

namespace Phrase.Compiler;

/// <summary>
/// Entry point: .phrase source in, target files out.
/// Pipeline: lexer (done) -> parser (done) -> resolver -> IR -> provider
/// </summary>
public static class PhraseCompiler
{
    public static CompilationOutput Compile(string source, ITargetProvider provider)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(provider);

        var parsed = Parser.Parse(source);
        if (parsed.Diagnostics.Count > 0)
            throw new CompilationFailedException(parsed.Diagnostics);

        throw new NotImplementedException(
            "Lexer and parser are done. Next: the resolver (bind phrase calls to templates, " +
            "check hole types, PH001/PH002/PH010), then the IR and the C# emitter.");
    }
}

/// <summary>Thrown when compilation stops on errors. Diagnostics are in source order of discovery.</summary>
public sealed class CompilationFailedException(IReadOnlyList<Diagnostic> diagnostics)
    : Exception("Compilation failed:\n" + string.Join("\n", diagnostics))
{
    public IReadOnlyList<Diagnostic> Diagnostics { get; } = diagnostics;
}
