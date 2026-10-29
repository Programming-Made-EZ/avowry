using Phrase.Compiler.Semantics;
using Phrase.Compiler.Syntax;

namespace Phrase.Compiler;

/// <summary>
/// Entry point: .phrase source in, target files out.
/// Pipeline: lexer (done) -> parser (done) -> resolver (done) -> target adapter.
/// </summary>
public static class PhraseCompiler
{
    public static CompilationOutput Compile(string source, ITargetAdapter adapter)
        => Compile(source, adapter, null);

    public static CompilationOutput Compile(string source, ITargetAdapter adapter, string? phraseFile)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(adapter);

        var normalized = source.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var parsed = Parser.Parse(normalized);
        StopOnDiagnostics(parsed.Diagnostics);

        var resolved = Resolver.Resolve(parsed.File);
        StopOnDiagnostics(resolved.Diagnostics);

        var module = resolved.Module;
        if (phraseFile is not null)
        {
            var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(normalized));
            module = module with
            {
                PhraseFile = Path.GetFileName(phraseFile.Replace('\\', '/')),
                BuildId = Convert.ToHexString(hash)[..16].ToLowerInvariant(),
            };
        }
        return adapter.Emit(module);
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
