using Phrase.Compiler.Semantics;
using Phrase.Compiler.Syntax;
using static Phrase.Compiler.Tests.Semantics.Resolved;

namespace Phrase.Compiler.Tests.Semantics;

// Feature: Resolving the whole program
//   The acceptance fixture resolves into the model the C# provider will consume.
public class ResolverFixtureTests
{
    private static readonly string Orders =
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "orders.avowry"));

    [Fact]
    public void The_orders_fixture_resolves_cleanly()
    {
        // Given the orders.avowry acceptance fixture
        // When it is resolved
        var module = Clean(Orders);

        // Then the module, type and phrase library are there
        Assert.Equal("Orders", module.Name);
        Assert.Equal("LineItem", Assert.Single(module.Types).Name);
        Assert.Equal(3, module.Phrases.Count);
        Assert.All(module.Phrases, p => Assert.Equal("csharp", Assert.Single(p.Natives).Key));
    }

    [Fact]
    public void Every_endpoint_step_is_bound_and_keeps_its_source()
    {
        // Given the orders.avowry acceptance fixture
        // When it is resolved
        var steps = Clean(Orders).Endpoints.Single().Steps;

        // Then each step is bound to the expected phrase, or is the response
        Assert.Equal(
            ["ensure _ is not empty", "ensure every quantity in _ is positive", "total quantity of _", "respond"],
            steps.Select(s => s switch
            {
                BoundCallStep c => c.Call.PhraseKey,
                BoundLetStep l => l.Call.PhraseKey,
                BoundRespondStep => "respond",
                _ => s.GetType().Name,
            }));

        // And the source text matches the fixture lines
        Assert.Equal(
            [
                "ensure input.items is not empty",
                "ensure every quantity in input.items is positive",
                "let total = total quantity of input.items",
                "respond with { totalQuantity: total }",
            ],
            steps.Select(s => s.Source));
    }

    [Fact]
    public void Failures_propagate_to_the_endpoint()
    {
        // Given the orders.avowry acceptance fixture
        // When it is resolved
        var endpoint = Clean(Orders).Endpoints.Single();

        // Then the endpoint can fail with exactly the guards' codes
        Assert.Equal(["EmptyOrder", "InvalidQuantity"], endpoint.Failures);
    }

    [Fact]
    public void All_six_specs_survive_resolution()
    {
        // Given the orders.avowry acceptance fixture
        // When it is resolved
        var module = Clean(Orders);

        // Then three phrase specs and three endpoint specs remain
        Assert.Equal(3, module.Phrases.Sum(p => p.Specs.Count));
        var endpointSpecs = module.Endpoints.Single().Specs;
        Assert.Equal(3, endpointSpecs.Count);

        // And their canonical lines are ready for generated test comments
        Assert.Equal(
            [
                "given input { customerId: \"c-1\", items: [{ sku: \"A\", quantity: 2 }, { sku: \"B\", quantity: 3 }] }",
                "when handled",
                "then responds { totalQuantity: 5 }",
            ],
            endpointSpecs[0].Lines);
    }

    private const string Alphabet = " \t\n\"{}[]<>-#\\:,.=/+aZ9";

    public static TheoryData<int> Seeds => new() { 1, 2, 3, 4, 5, 6, 7, 8 };

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Mutated_programs_resolve_without_throwing(int seed)
    {
        // Given the orders fixture with random single-character edits (deterministic per seed)
        var random = new Random(seed);
        for (var round = 0; round < 250; round++)
        {
            var chars = Orders.ToList();
            for (var e = random.Next(1, 6); e > 0 && chars.Count > 0; e--)
            {
                var at = random.Next(chars.Count);
                switch (random.Next(3))
                {
                    case 0: chars.RemoveAt(at); break;
                    case 1: chars.Insert(at, Alphabet[random.Next(Alphabet.Length)]); break;
                    default: chars[at] = Alphabet[random.Next(Alphabet.Length)]; break;
                }
            }

            // When the parsed tree is resolved, even when it has syntax errors
            var parsed = Parser.Parse(new string([.. chars]));
            var result = Resolver.Resolve(parsed.File);

            // Then a model comes back, and every diagnostic has a real position
            Assert.NotNull(result.Module);
            Assert.All(result.Diagnostics, d => Assert.True(d.Line >= 1 && d.Column >= 1, d.ToString()));
        }
    }
}
