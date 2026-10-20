using Phrase.Compiler.Lexing;

namespace Phrase.Compiler.Tests.Lexing;

// Feature: Names, literals and symbols
//   The lexer classifies each word on a line. Keywords are not special here:
//   they are reserved only at the start of a line, which is the parser's job.
public class LexerTokenTests
{
    [Fact]
    public void Lowercase_words_are_names_and_capitalized_words_are_type_names()
    {
        // Given a line mixing words, identifiers and types
        const string source = "endpoint CreateOrder : POST /orders\n";

        // When it is lexed
        var kinds = Lexed.Tokens(source).Select(t => t.Kind).ToList();

        // Then each word is classified by its first letter
        Assert.Equal(
            [TokenKind.Name, TokenKind.TypeName, TokenKind.Symbol, TokenKind.TypeName,
             TokenKind.Symbol, TokenKind.Name, TokenKind.Newline, TokenKind.EndOfFile],
            kinds);
    }

    [Fact]
    public void Camel_case_identifiers_are_single_names()
    {
        // Given a field with a camel-case name
        const string source = "customerId: Id\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then the identifier is one token
        Assert.Equal("customerId : Id NL EOF", tokens);
    }

    [Fact]
    public void Hyphens_and_apostrophes_inside_words_stay_in_the_word()
    {
        // Given words with internal hyphens and apostrophes
        const string source = "ensure first-class don't\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then they are single names
        Assert.Equal("ensure first-class don't NL EOF", tokens);
    }

    [Fact]
    public void An_arrow_is_one_symbol_and_never_part_of_a_word()
    {
        // Given a phrase signature with a return type, with and without spacing
        const string source = "\"total of {items: List<LineItem>}\" -> Int\nx->Y\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then the arrow is its own token in both cases
        Assert.Equal("\"total of {items: List<LineItem>}\" -> Int NL x -> Y NL EOF", tokens);
    }

    [Fact]
    public void Generic_types_lex_as_angle_bracket_symbols()
    {
        // Given a nested generic type
        const string source = "items: List<Option<LineItem>>\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then each angle bracket is a separate symbol
        Assert.Equal("items : List < Option < LineItem > > NL EOF", tokens);
    }

    [Fact]
    public void Paths_lex_as_names_separated_by_dots()
    {
        // Given a step that reads a nested path
        const string source = "total quantity of input.items\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then the path is name, dot, name
        Assert.Equal("total quantity of input . items NL EOF", tokens);
    }

    [Fact]
    public void Records_and_lists_lex_as_bracket_symbols()
    {
        // Given a list holding one record
        const string source = "given items = [{ sku: \"A\", quantity: 0 }]\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then brackets, braces, colons and commas are symbols
        Assert.Equal("given items = [ { sku : \"A\" , quantity : 0 } ] NL EOF", tokens);
    }

    [Theory]
    [InlineData("5", "5")]
    [InlineData("-3", "-3")]
    [InlineData("2.50", "2.50")]
    [InlineData("-0.5", "-0.5")]
    public void Numbers_include_sign_and_decimals(string literal, string expected)
    {
        // Given a numeric literal
        var source = $"then returns {literal}\n";

        // When it is lexed
        var number = Lexed.Tokens(source).Single(t => t.Kind == TokenKind.Number);

        // Then the whole literal is one number token
        Assert.Equal(expected, number.Text);
    }

    [Fact]
    public void A_string_keeps_template_holes_verbatim()
    {
        // Given a phrase template
        const string source = "phrase \"ensure {items: List<LineItem>} is not empty\"\n";

        // When it is lexed
        var template = Lexed.Tokens(source).Single(t => t.Kind == TokenKind.String);

        // Then the braces and types inside are untouched for the parser to split later
        Assert.Equal("ensure {items: List<LineItem>} is not empty", template.Text);
    }

    [Fact]
    public void A_string_decodes_quote_and_backslash_escapes()
    {
        // Given a string with escaped characters
        const string source = "spec \"says \\\"hi\\\" via C:\\\\temp\"\n";

        // When it is lexed
        var value = Lexed.Tokens(source).Single(t => t.Kind == TokenKind.String);

        // Then the escapes are decoded
        Assert.Equal("says \"hi\" via C:\\temp", value.Text);
    }

    [Fact]
    public void A_hash_inside_a_string_is_not_a_comment()
    {
        // Given a string containing a hash
        const string source = "spec \"handles #1 priority\"\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then the hash stays in the string
        Assert.Equal("spec \"handles #1 priority\" NL EOF", tokens);
    }
}
