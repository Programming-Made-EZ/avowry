using Avowry.Compiler.Lexing;

namespace Avowry.Compiler.Tests.Lexing;

// Feature: Lexer diagnostics
//   Malformed input produces stable AV1xx codes with positions, and lexing
//   continues so one mistake does not hide the next.
public class LexerDiagnosticTests
{
    [Fact]
    public void A_tab_in_indentation_is_AV100()
    {
        // Given a field indented with a tab
        const string source = "type LineItem\n\tsku: Text\n";

        // When it is lexed
        var diagnostic = Assert.Single(Lexer.Lex(source).Diagnostics);

        // Then it is reported at the tab
        Assert.Equal(("AV100", 2, 1), (diagnostic.Code, diagnostic.Line, diagnostic.Column));
    }

    [Fact]
    public void Dedenting_to_a_level_that_was_never_opened_is_AV101()
    {
        // Given open levels at 0, 2 and 4 spaces, and a line that dedents to 3
        const string source = "a\n  b\n    c\n   d\n";

        // When it is lexed
        var codes = Lexed.Codes(source);

        // Then the inconsistent dedent is reported
        Assert.Contains("AV101", codes);
    }

    [Fact]
    public void Indenting_by_more_than_one_level_is_AV102()
    {
        // Given a block indented four spaces at once
        const string source = "type LineItem\n    sku: Text\n";

        // When it is lexed
        var diagnostic = Assert.Single(Lexer.Lex(source).Diagnostics);

        // Then it is reported, because canonical layout is two spaces per level
        Assert.Equal(("AV102", 2, 1), (diagnostic.Code, diagnostic.Line, diagnostic.Column));
    }

    [Fact]
    public void An_unterminated_string_is_AV103()
    {
        // Given a string missing its closing quote
        const string source = "spec \"accepts a valid order\n";

        // When it is lexed
        var diagnostic = Assert.Single(Lexer.Lex(source).Diagnostics);

        // Then it is reported at the opening quote
        Assert.Equal(("AV103", 1, 6), (diagnostic.Code, diagnostic.Line, diagnostic.Column));
    }

    [Fact]
    public void An_unexpected_character_is_AV104_and_lexing_continues()
    {
        // Given a line with a character Phrase does not use
        const string source = "let total = a + b\n";

        // When it is lexed
        var result = Lexer.Lex(source);

        // Then the character is reported and the rest of the line still lexes
        Assert.Equal(("AV104", 1, 15), (result.Diagnostics[0].Code, result.Diagnostics[0].Line, result.Diagnostics[0].Column));
        Assert.Equal("let total = a b NL EOF", Lexed.Render(result.Tokens));
    }

    [Fact]
    public void A_native_block_with_no_body_is_AV106()
    {
        // Given a native header with nothing indented under it
        const string source = "native csharp\navow to \"next\"\n";

        // When it is lexed
        var diagnostic = Assert.Single(Lexer.Lex(source).Diagnostics);

        // Then it is reported at the header
        Assert.Equal(("AV106", 1, 1), (diagnostic.Code, diagnostic.Line, diagnostic.Column));
    }

    [Fact]
    public void An_unknown_escape_in_a_string_is_AV107()
    {
        // Given a string with an escape Phrase does not define
        const string source = "spec \"line\\nbreak\"\n";

        // When it is lexed
        var diagnostic = Assert.Single(Lexer.Lex(source).Diagnostics);

        // Then it is reported at the backslash
        Assert.Equal(("AV107", 1, 11), (diagnostic.Code, diagnostic.Line, diagnostic.Column));
    }

    [Fact]
    public void An_unclosed_bracket_at_end_of_file_is_AV108()
    {
        // Given a record that is never closed
        const string source = "given input { customerId: \"c-1\"\n";

        // When it is lexed
        var diagnostic = Assert.Single(Lexer.Lex(source).Diagnostics);

        // Then it is reported at the opening brace
        Assert.Equal(("AV108", 1, 13), (diagnostic.Code, diagnostic.Line, diagnostic.Column));
    }

    [Fact]
    public void A_closing_bracket_with_no_opener_is_AV109()
    {
        // Given a stray closing bracket
        const string source = "given items = ]\n";

        // When it is lexed
        var diagnostic = Assert.Single(Lexer.Lex(source).Diagnostics);

        // Then it is reported where it appears
        Assert.Equal(("AV109", 1, 15), (diagnostic.Code, diagnostic.Line, diagnostic.Column));
    }
}
