using Phrase.Compiler.Lexing;

namespace Phrase.Compiler.Tests.Lexing;

/// <summary>
/// Renders tokens as one compact line so expectations read like the source:
/// names, types, numbers and symbols as written; strings quoted; layout tokens as NL, INDENT, DEDENT, RAW, EOF.
/// </summary>
internal static class Lexed
{
    /// <summary>Lexes and asserts there were no diagnostics.</summary>
    public static string Clean(string source)
    {
        var result = Lexer.Lex(source);
        Assert.True(result.Diagnostics.Count == 0, string.Join("\n", result.Diagnostics));
        return Render(result.Tokens);
    }

    public static string Render(IEnumerable<Token> tokens) =>
        string.Join(" ", tokens.Select(t => t.Kind switch
        {
            TokenKind.String => $"\"{t.Text}\"",
            TokenKind.Newline => "NL",
            TokenKind.Indent => "INDENT",
            TokenKind.Dedent => "DEDENT",
            TokenKind.Raw => "RAW",
            TokenKind.EndOfFile => "EOF",
            _ => t.Text,
        }));

    public static IReadOnlyList<string> Codes(string source) =>
        Lexer.Lex(source).Diagnostics.Select(d => d.Code).ToList();

    public static IReadOnlyList<Token> Tokens(string source) => Lexer.Lex(source).Tokens;
}
