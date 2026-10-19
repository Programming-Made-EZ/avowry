namespace Phrase.Compiler;

/// <summary>
/// Entry point: .phrase source in, target files out.
/// Pipeline (to be built inward from the acceptance test):
///   lexer (INDENT/DEDENT) -> parser (AST) -> resolver (phrase calls, types, lints) -> IR -> provider
/// </summary>
public static class PhraseCompiler
{
    public static CompilationOutput Compile(string source, ITargetProvider provider)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(provider);

        throw new NotImplementedException(
            "Walking skeleton: lexer, parser, resolver and IR are not built yet. " +
            "Drive them out with unit tests, starting from the lexer.");
    }
}
