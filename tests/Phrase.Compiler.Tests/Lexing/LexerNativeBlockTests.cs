using Phrase.Compiler.Lexing;

namespace Phrase.Compiler.Tests.Lexing;

// Feature: Native blocks
//   The body of `native <target>` is target code, not Phrase. The lexer passes it
//   through as one RAW token: no comments stripped, no tokens, relative indentation kept.
public class LexerNativeBlockTests
{
    [Fact]
    public void A_native_block_becomes_one_raw_token()
    {
        // Given a leaf phrase with a C# body
        const string source = """
            avow to "total quantity of {items: List<LineItem>}" -> Int
              native csharp
                return items.Sum(i => i.Quantity);
            """;

        // When it is lexed
        var result = Lexer.Lex(source);

        // Then the body is a single RAW token inside the phrase block, with its base indentation removed
        Assert.Empty(result.Diagnostics);
        Assert.Equal(
            "avow to \"total quantity of {items: List<LineItem>}\" -> Int NL INDENT native csharp NL RAW NL DEDENT EOF",
            Lexed.Render(result.Tokens));
        Assert.Equal("return items.Sum(i => i.Quantity);", result.Tokens.Single(t => t.Kind == TokenKind.Raw).Text);
    }

    [Fact]
    public void Relative_indentation_and_blank_lines_inside_the_block_are_kept()
    {
        // Given a multi-line body with nesting and a blank line
        const string source = "native csharp\n  if (items.Count == 0)\n  {\n\n      Fail(\"EmptyOrder\");\n  }\n";

        // When it is lexed
        var raw = Lexed.Tokens(source).Single(t => t.Kind == TokenKind.Raw);

        // Then the body keeps its shape
        Assert.Equal("if (items.Count == 0)\n{\n\n    Fail(\"EmptyOrder\");\n}", raw.Text);
    }

    [Fact]
    public void Characters_that_mean_something_in_phrase_are_left_alone()
    {
        // Given a body containing a hash, an unbalanced bracket and a tab
        const string source = "native python\n  x = data[\"#tag\"  # comment stays\n  \ty = 1\n";

        // When it is lexed
        var result = Lexer.Lex(source);

        // Then nothing inside the block is interpreted
        Assert.Empty(result.Diagnostics);
        Assert.Equal("x = data[\"#tag\"  # comment stays\n\ty = 1", result.Tokens.Single(t => t.Kind == TokenKind.Raw).Text);
    }

    [Fact]
    public void Trailing_blank_lines_are_not_part_of_the_block()
    {
        // Given a body followed by blank lines and the next declaration
        const string source = "native csharp\n  return 1;\n\n\navow to \"next\"\n";

        // When it is lexed
        var result = Lexer.Lex(source);

        // Then the block ends at its last code line and lexing resumes normally
        Assert.Equal("return 1;", result.Tokens.Single(t => t.Kind == TokenKind.Raw).Text);
        Assert.Equal("native csharp NL RAW NL avow to \"next\" NL EOF", Lexed.Render(result.Tokens));
    }

    [Fact]
    public void The_raw_token_points_at_the_first_line_of_the_body()
    {
        // Given a body starting on line 2, column 3
        const string source = "native csharp\n  return 1;\n";

        // When it is lexed
        var raw = Lexed.Tokens(source).Single(t => t.Kind == TokenKind.Raw);

        // Then its position is the start of the body
        Assert.Equal((2, 3), (raw.Line, raw.Column));
    }

    [Fact]
    public void Native_is_only_special_at_the_start_of_a_line()
    {
        // Given a step that happens to contain the word native, followed by an indented line
        const string source = "ensure native csharp\n  x\n";

        // When it is lexed
        var tokens = Lexed.Clean(source);

        // Then no raw block is opened and the indented line is ordinary layout
        Assert.Equal("ensure native csharp NL INDENT x NL DEDENT EOF", tokens);
    }
}
