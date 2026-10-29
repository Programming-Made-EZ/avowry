using Avowry.Compiler.Lexing;

namespace Phrase.Compiler.Tests.Lexing;

// Feature: Lexing a whole program
//   The acceptance fixture is the first real program; the lexer must handle all of it.
public class LexerFixtureTests
{
    private static readonly string Orders =
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "orders.avowry"));

    [Fact]
    public void The_orders_fixture_lexes_without_diagnostics()
    {
        // Given the orders.avowry acceptance fixture
        // When it is lexed
        var result = Lexer.Lex(Orders);

        // Then there are no diagnostics
        Assert.True(result.Diagnostics.Count == 0, string.Join("\n", result.Diagnostics));
    }

    [Fact]
    public void Every_indent_in_the_fixture_is_matched_by_a_dedent()
    {
        // Given the orders.avowry acceptance fixture
        // When it is lexed
        var tokens = Lexer.Lex(Orders).Tokens;

        // Then layout is balanced
        Assert.Equal(
            tokens.Count(t => t.Kind == TokenKind.Indent),
            tokens.Count(t => t.Kind == TokenKind.Dedent));

        // And it ends with end of file
        Assert.Equal(TokenKind.EndOfFile, tokens[^1].Kind);
    }

    [Fact]
    public void Each_native_block_in_the_fixture_is_one_raw_token()
    {
        // Given the orders.avowry acceptance fixture, which has three native csharp blocks
        // When it is lexed
        var raws = Lexer.Lex(Orders).Tokens.Where(t => t.Kind == TokenKind.Raw).Select(t => t.Text).ToList();

        // Then each body comes through verbatim
        Assert.Equal(
            [
                "if (items.Count == 0) Fail(\"EmptyOrder\");",
                "if (items.Any(i => i.Quantity <= 0)) Fail(\"InvalidQuantity\");",
                "return items.Sum(i => i.Quantity);",
            ],
            raws);
    }

    [Fact]
    public void The_fixture_declares_six_specs()
    {
        // Given the orders.avowry acceptance fixture
        // When it is lexed
        var tokens = Lexer.Lex(Orders).Tokens;

        // Then six lines start with the name `spec` followed by a string
        var specs = tokens.Zip(tokens.Skip(1))
            .Count(pair => pair.First is { Kind: TokenKind.Name, Text: "spec" } && pair.Second.Kind == TokenKind.String);
        Assert.Equal(6, specs);
    }
}
