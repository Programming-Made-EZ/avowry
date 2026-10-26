using Phrase.Compiler.Semantics;
using static Phrase.Compiler.Tests.Semantics.Resolved;

namespace Phrase.Compiler.Tests.Semantics;

// Feature: Declarations and the phrase library
//   Types resolve to core or user types, and every phrase in the library has a
//   unique wording, so a step can only ever mean one thing.
public class ResolverDeclarationTests
{
    [Fact]
    public void Types_resolve_to_core_and_user_types()
    {
        // Given the prelude and a trivial endpoint
        var module = Clean(Program(Probe("fail with NotBuilt")));

        // When the type model is read
        var lineItem = Assert.Single(module.Types);

        // Then fields carry resolved types
        Assert.Equal(["sku: Text", "quantity: Int"], lineItem.Fields.Select(f => $"{f.Name}: {f.Type}"));
    }

    [Fact]
    public void Types_may_refer_to_types_declared_later()
    {
        // Given a type that uses one declared below it
        const string source = "module Test\n\ntype Order\n  lines: List<LineItem>\n\ntype LineItem\n  sku: Text\n";

        // When it is resolved
        var module = Clean(source);

        // Then the reference resolves
        Assert.Equal("List<LineItem>", module.Types[0].Fields[0].Type.ToString());
    }

    [Fact]
    public void An_unknown_type_is_PH300_at_the_reference()
    {
        // Given a field whose type does not exist
        const string source = "module Test\n\ntype Basket\n  prices: Map<Text, Decimal>\n";

        // When it is resolved
        var diagnostic = Only(source);

        // Then the type name is reported where it is written
        Assert.Equal(("PH300", 4, 11), (diagnostic.Code, diagnostic.Line, diagnostic.Column));
    }

    [Theory]
    [InlineData("List<Int, Int>")]
    [InlineData("Int<Text>")]
    [InlineData("Option")]
    public void The_wrong_number_of_type_arguments_is_PH301(string type)
    {
        // Given a field with a malformed generic
        var source = $"module Test\n\ntype Box\n  value: {type}\n";

        // When it is resolved
        // Then the arity is reported
        Assert.Equal(["PH301"], Codes(source));
    }

    [Theory]
    [InlineData("type LineItem\n  sku: Text\n\ntype LineItem\n  id: Id\n")]
    [InlineData("type Text\n  value: Int\n")]
    public void A_duplicate_or_core_type_name_is_PH302(string types)
    {
        // Given a type name that is already taken
        var source = "module Test\n\n" + types;

        // When it is resolved
        // Then the second definition is reported
        Assert.Equal(["PH302"], Codes(source));
    }

    [Fact]
    public void A_duplicate_field_is_PH303()
    {
        // Given a type with the same field twice
        const string source = "module Test\n\ntype LineItem\n  sku: Text\n  sku: Id\n";

        // When it is resolved
        // Then the second field is reported
        Assert.Equal(["PH303"], Codes(source));
    }

    [Fact]
    public void An_unknown_type_in_a_hole_is_PH300()
    {
        // Given a phrase whose hole type does not exist
        var source = Program("phrase \"weigh {items: List<Thing>}\"\n  native csharp\n    return;\n");

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH300"], Codes(source));
    }

    [Fact]
    public void Phrase_keys_are_lowercased_words_with_holes_as_underscores()
    {
        // Given the prelude
        var module = Clean(Program(Probe("fail with NotBuilt")));

        // When the phrase library is read
        // Then each phrase is keyed by its wording
        Assert.Equal(
            ["ensure _ is not empty", "ensure every quantity in _ is positive", "total quantity of _", "ensure _ is positive"],
            module.Phrases.Select(p => p.Key));
        Assert.Equal("total quantity of {items: List<LineItem>}", module.Phrase("total quantity of _").Template);
    }

    [Fact]
    public void Two_phrases_with_the_same_wording_is_PH010()
    {
        // Given a phrase worded like a prelude phrase, differing only in its hole
        var source = Program("phrase \"ensure {count: Int} is not empty\"\n  native csharp\n    return;\n");

        // When it is resolved
        var diagnostic = Only(source);

        // Then the duplicate is reported and names the phrase it collides with
        Assert.Equal("PH010", diagnostic.Code);
        Assert.Contains("ensure {items: List<LineItem>} is not empty", diagnostic.Message);
    }

    [Fact]
    public void Two_endpoints_with_the_same_name_is_PH304()
    {
        // Given the probe endpoint declared twice on different routes
        var second = Probe("fail with NotBuilt").Replace("/probe", "/other", StringComparison.Ordinal);
        var source = Program(Probe("fail with NotBuilt"), second);

        // When it is resolved
        // Then the second is reported
        Assert.Equal(["PH304"], Codes(source));
    }

    [Fact]
    public void Two_endpoints_on_the_same_verb_and_route_is_PH304()
    {
        // Given a second endpoint with a new name on the same route
        var second = Probe("fail with NotBuilt").Replace("endpoint Probe", "endpoint Probe2", StringComparison.Ordinal);
        var source = Program(Probe("fail with NotBuilt"), second);

        // When it is resolved
        // Then the clash is reported
        Assert.Equal(["PH304"], Codes(source));
    }

    [Fact]
    public void A_duplicate_input_field_is_PH303()
    {
        // Given an endpoint input with a repeated field
        var source = Program(Probe("fail with NotBuilt").Replace("    items: List<LineItem>\n", "    items: List<LineItem>\n    items: Int\n", StringComparison.Ordinal));

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH303"], Codes(source));
    }

    [Fact]
    public void Endpoint_shapes_become_named_record_types()
    {
        // Given an endpoint with input and output
        var endpoint = ProbeModel(Clean(Program(ProbeWithOutput("fail with NotBuilt"))));

        // When its shapes are read
        // Then they are typed records named after the endpoint
        Assert.Equal(("Probe.Input", "Probe.Output"), (endpoint.InputType.ToString(), endpoint.OutputType!.ToString()));
        Assert.Equal(["customerId: Id", "items: List<LineItem>"], endpoint.Input.Select(f => $"{f.Name}: {f.Type}"));
    }
}
