namespace Phrase.Compiler.Lexing;

public enum TokenKind
{
    /// <summary>Lowercase-initial word or identifier: <c>module</c>, <c>customerId</c>, <c>first-class</c>.</summary>
    Name,
    /// <summary>Uppercase-initial identifier: types, modules, endpoints, failure codes, HTTP verbs.</summary>
    TypeName,
    /// <summary>Double-quoted string. <see cref="Token.Text"/> holds the decoded value.</summary>
    String,
    Number,
    /// <summary>Punctuation: <c>: , . = { } [ ] &lt; &gt; / -&gt;</c></summary>
    Symbol,
    Newline,
    Indent,
    Dedent,
    /// <summary>The body of a <c>native &lt;target&gt;</c> block, passed through verbatim with the block's base indentation removed.</summary>
    Raw,
    EndOfFile,
}

/// <summary>A lexical token. Line and column are 1-based.</summary>
public sealed record Token(TokenKind Kind, string Text, int Line, int Column);

public sealed record LexResult(IReadOnlyList<Token> Tokens, IReadOnlyList<Diagnostic> Diagnostics);
