using Phrase.Compiler.Syntax;
using static Phrase.Compiler.Tests.Syntax.Parsed;

namespace Phrase.Compiler.Tests.Syntax;

// Feature: Parsing a whole program
//   The acceptance fixture parses completely, and broken programs still yield
//   every declaration that can be recovered.
public class ParserFixtureTests
{
    private static readonly string Orders =
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "orders.phrase"));

    [Fact]
    public void The_orders_fixture_parses_into_its_declarations()
    {
        // Given the orders.phrase acceptance fixture
        // When it is parsed
        var file = Clean(Orders);

        // Then the module, type, phrases and endpoint are all there
        Assert.Equal("Orders", file.Module!.Name);
        Assert.Equal("(type LineItem (sku: Text) (quantity: Int))", Print(Assert.Single(file.Types)));
        Assert.Equal(
            [
                "ensure {items: List<LineItem>} is not empty",
                "ensure every quantity in {items: List<LineItem>} is positive",
                "total quantity of {items: List<LineItem>}",
            ],
            file.Phrases.Select(p => Print(p.Template)));

        var endpoint = Assert.Single(file.Endpoints);
        Assert.Equal(("CreateOrder", "POST", "/orders"), (endpoint.Name, endpoint.Verb, endpoint.Route));
        Assert.Equal(4, endpoint.Steps.Count);
    }

    [Fact]
    public void The_fixture_has_six_specs_split_three_and_three()
    {
        // Given the orders.phrase acceptance fixture
        // When it is parsed
        var file = Clean(Orders);

        // Then each phrase has one spec and the endpoint has three
        Assert.All(file.Phrases, p => Assert.Single(p.Specs));
        Assert.Equal(
            ["accepts a valid order", "rejects an order with no items", "rejects an order with a zero quantity"],
            file.Endpoints[0].Specs.Select(s => s.Description));
    }

    [Fact]
    public void The_fixture_contracts_are_captured()
    {
        // Given the orders.phrase acceptance fixture
        // When it is parsed
        var phrases = Clean(Orders).Phrases;

        // Then failures and return types match the source
        Assert.Equal(
            [("EmptyOrder", null), ("InvalidQuantity", null), ("", "Int")],
            phrases.Select(p => (string.Join(",", p.Failures), p.Returns?.Name)));
    }

    [Fact]
    public void A_broken_declaration_does_not_hide_the_ones_after_it()
    {
        // Given a file where the first phrase header is malformed
        var source = InModule("""
            phrase ensure items
              native csharp
                return;

            type LineItem
              sku: Text

            endpoint Stub : GET /stub
              input
                n: Int
              steps
                fail with NotBuilt
            """);

        // When it is parsed
        var result = Parser.Parse(source);

        // Then the error is reported once
        Assert.Equal(["PH200"], result.Diagnostics.Select(d => d.Code));

        // And the later declarations are recovered
        Assert.Single(result.File.Types);
        Assert.Single(result.File.Endpoints);
    }

    private const string Alphabet = " \t\n\"{}[]<>-#\\:,.=/+aZ9";

    public static TheoryData<int> Seeds => new() { 1, 2, 3, 4, 5, 6, 7, 8 };

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Mutated_programs_parse_without_throwing(int seed)
    {
        // Given the orders fixture with random single-character edits (deterministic per seed)
        var random = new Random(seed);
        for (var round = 0; round < 250; round++)
        {
            var source = Mutate(Orders, random, edits: random.Next(1, 6));

            // When it is parsed
            var result = Parser.Parse(source);

            // Then a tree comes back, and every diagnostic has a real position
            Assert.NotNull(result.File);
            Assert.All(result.Diagnostics, d => Assert.True(d.Line >= 1 && d.Column >= 1, d.ToString()));
        }
    }

    private static string Mutate(string source, Random random, int edits)
    {
        var chars = source.ToList();
        for (var e = 0; e < edits && chars.Count > 0; e++)
        {
            var at = random.Next(chars.Count);
            switch (random.Next(3))
            {
                case 0: chars.RemoveAt(at); break;
                case 1: chars.Insert(at, Alphabet[random.Next(Alphabet.Length)]); break;
                default: chars[at] = Alphabet[random.Next(Alphabet.Length)]; break;
            }
        }
        return new string([.. chars]);
    }
}
