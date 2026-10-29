using Avowry.Compiler.Lexing;

namespace Phrase.Compiler.Tests.Lexing;

// Feature: Line and indentation layout
//   The lexer turns significant indentation into INDENT and DEDENT tokens
//   so the parser never has to count spaces.
public class LexerLayoutTests
{
    [Fact]
    public void A_single_line_ends_with_a_newline_and_end_of_file()
    {
        // Given a module declaration
        const string source = "module Orders\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then it is a name, a type name, a newline, and end of file
        Assert.Equal("module Orders NL EOF", tokens);
    }

    [Fact]
    public void A_missing_final_newline_is_supplied()
    {
        // Given source with no trailing newline
        const string source = "module Orders";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then the last line still ends with a newline token
        Assert.Equal("module Orders NL EOF", tokens);
    }

    [Fact]
    public void An_indented_block_is_wrapped_in_indent_and_dedent()
    {
        // Given a type with two fields
        const string source = """
            type LineItem
              sku: Text
              quantity: Int
            """;

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then the fields sit between INDENT and DEDENT
        Assert.Equal("type LineItem NL INDENT sku : Text NL quantity : Int NL DEDENT EOF", tokens);
    }

    [Fact]
    public void Returning_several_levels_emits_one_dedent_per_level()
    {
        // Given a block nested two levels deep followed by a top-level line
        const string source = """
            a
              b
                c
            d
            """;

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then two DEDENTs close both levels before the top-level line
        Assert.Equal("a NL INDENT b NL INDENT c NL DEDENT DEDENT d NL EOF", tokens);
    }

    [Fact]
    public void Open_blocks_are_closed_at_end_of_file()
    {
        // Given a file that ends while indented
        const string source = "a\n  b\n    c\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then every open level is closed before end of file
        Assert.Equal("a NL INDENT b NL INDENT c NL DEDENT DEDENT EOF", tokens);
    }

    [Fact]
    public void Blank_lines_and_comment_lines_do_not_affect_layout()
    {
        // Given a block interrupted by a blank line and an unindented comment
        const string source = """
            type LineItem
              sku: Text

            # a comment at column one
              quantity: Int
            """;

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then the block is unbroken and the comment produces no tokens
        Assert.Equal("type LineItem NL INDENT sku : Text NL quantity : Int NL DEDENT EOF", tokens);
    }

    [Fact]
    public void A_trailing_comment_is_ignored()
    {
        // Given a line with a comment after the code
        const string source = "module Orders # the order slice\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then the comment produces no tokens
        Assert.Equal("module Orders NL EOF", tokens);
    }

    [Fact]
    public void Newlines_and_indentation_inside_brackets_are_ignored()
    {
        // Given a record literal spread over several lines
        const string source = """
            given input {
                customerId: "c-1",
              items: []
            }
            """;

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then it lexes as one logical line
        Assert.Equal("given input { customerId : \"c-1\" , items : [ ] } NL EOF", tokens);
    }

    [Fact]
    public void Windows_line_endings_lex_the_same_as_unix()
    {
        // Given a block written with CRLF line endings
        const string source = "type LineItem\r\n  sku: Text\r\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then the carriage returns are invisible
        Assert.Equal("type LineItem NL INDENT sku : Text NL DEDENT EOF", tokens);
    }

    [Fact]
    public void Tokens_carry_one_based_line_and_column()
    {
        // Given a field on the second line, indented two spaces
        const string source = "type LineItem\n  sku: Text\n";

        // When it is lexed
        var sku = Lexed.Tokens(source).First(t => t.Text == "sku");
        var text = Lexed.Tokens(source).First(t => t.Text == "Text");

        // Then positions are 1-based and point at the first character
        Assert.Equal((2, 3), (sku.Line, sku.Column));
        Assert.Equal((2, 8), (text.Line, text.Column));
    }
}
