using Avowry.Compiler.Semantics;
using static Phrase.Compiler.Tests.Semantics.Resolved;

namespace Phrase.Compiler.Tests.Semantics;

// Feature: Checking specs
//   A spec must be runnable as written: it binds every hole (or gives the endpoint
//   input), uses literal values of the right types, and expects an outcome the unit
//   can actually produce.
public class ResolverSpecTests
{
    private static string Adder(string spec) =>
        Program("avow to \"add {a: Int} to {b: Int}\" -> Int\n  fails with Overflow\n" + spec + "  native csharp\n    return a + b;\n");

    private static string Checker(string spec) =>
        Program("avow to \"check {n: Int}\"\n  fails with Overflow\n" + spec + "  native csharp\n    return;\n");

    private static string ProbeWithSpec(string spec) =>
        Program(Endpoint(
            output: true,
            ["let total = total quantity of input.items", "respond with { totalQuantity: total }"],
            spec));

    [Fact]
    public void A_phrase_spec_binds_holes_in_hole_order()
    {
        // Given a spec that binds the holes out of order
        var module = Clean(Adder(Spec("adds", "given b = 2", "and a = 1", "when called", "then returns 3")));

        // When it is resolved
        var spec = Assert.Single(module.Avow("add _ to _").Specs);

        // Then givens are in hole order and the outcome is typed
        Assert.Equal(["a", "b"], spec.Givens.Select(g => g.Hole.Name));
        Assert.Equal(new BoundNumber("3", PType.Int), Assert.IsType<ReturnsModel>(spec.Outcome).Value);

        // And the canonical lines are kept for generated comments
        Assert.Equal(["given b = 2", "and a = 1", "when called", "then returns 3"], spec.Lines);
    }

    [Fact]
    public void A_phrase_spec_that_says_handled_is_PH330()
    {
        // Given a phrase spec using the endpoint trigger
        var source = Adder(Spec("wrong trigger", "given a = 1", "and b = 2", "when handled", "then returns 3"));

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH330"], Codes(source));
    }

    [Theory]
    [InlineData("given a = 1", "and c = 2")]          // no hole named c, and b is missing
    [InlineData("given a = 1", "and a = 2")]          // a twice, b missing
    [InlineData("given input { a: 1, b: 2 }", null)]  // input is for endpoint specs
    public void Givens_must_bind_each_hole_exactly_once(string first, string? second)
    {
        // Given a spec with wrong givens
        var lines = new List<string> { first };
        if (second is not null) lines.Add(second);
        lines.AddRange(["when called", "then returns 3"]);
        var source = Adder(Spec("bad givens", [.. lines]));

        // When it is resolved
        // Then every problem is PH331
        var codes = Codes(source);
        Assert.NotEmpty(codes);
        Assert.All(codes, c => Assert.Equal("PH331", c));
    }

    [Fact]
    public void A_missing_hole_binding_is_PH331_naming_the_hole()
    {
        // Given a spec that never binds b
        var diagnostic = Only(Adder(Spec("half", "given a = 1", "when called", "then returns 1")));

        // When it is resolved
        // Then the hole is named
        Assert.Equal("PH331", diagnostic.Code);
        Assert.Contains("b", diagnostic.Message);
    }

    [Fact]
    public void A_spec_value_that_is_a_path_is_PH332()
    {
        // Given a spec that refers to a variable
        var source = Adder(Spec("path", "given a = input.count", "and b = 2", "when called", "then returns 3"));

        // When it is resolved
        // Then it is reported, because specs run with literal data only
        Assert.Equal(["PH332"], Codes(source));
    }

    [Fact]
    public void A_returned_value_of_the_wrong_type_is_PH312()
    {
        // Given a spec expecting text from an Int phrase
        var source = Adder(Spec("text", "given a = 1", "and b = 2", "when called", "then returns \"three\""));

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH312"], Codes(source));
    }

    [Theory]
    [InlineData("then returns 1")]                  // check returns nothing
    [InlineData("then responds { n: 1 }")]          // responds is for endpoints
    public void An_outcome_the_phrase_cannot_have_is_PH333(string then)
    {
        // Given a spec on a phrase with no return type
        var source = Checker(Spec("bad outcome", "given n = 1", "when called", then));

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH333"], Codes(source));
    }

    [Fact]
    public void Expecting_a_failure_the_phrase_cannot_produce_is_PH334()
    {
        // Given a spec expecting a code the phrase never declares
        var diagnostic = Only(Checker(Spec("impossible", "given n = 1", "when called", "then fails with Underflow")));

        // When it is resolved
        // Then the possible codes are listed
        Assert.Equal("PH334", diagnostic.Code);
        Assert.Contains("Overflow", diagnostic.Message);
    }

    [Fact]
    public void More_than_one_outcome_is_PH335()
    {
        // Given a spec that expects a value and a failure
        var source = Adder(Spec("both", "given a = 1", "and b = 2", "when called", "then returns 3", "and fails with Overflow"));

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH335"], Codes(source));
    }

    [Fact]
    public void A_composite_spec_may_expect_a_failure_from_a_phrase_it_calls()
    {
        // Given a composite whose spec expects a guard's failure
        var source = Program(
            "avow to \"validate {items: List<LineItem>}\"\n" +
            Spec("rejects empty", "given items = []", "when called", "then fails with EmptyOrder") +
            "  steps\n    ensure items is not empty\n");

        // When it is resolved
        // Then it is valid
        Assert.Single(Clean(source).Avow("validate _").Specs);
    }

    [Fact]
    public void An_endpoint_spec_gives_typed_input_and_expects_a_typed_response()
    {
        // Given a valid endpoint spec
        var module = Clean(ProbeWithSpec(Spec("totals",
            "given input { customerId: \"c-1\", items: [{ sku: \"A\", quantity: 2 }] }",
            "when handled",
            "then responds { totalQuantity: 2 }")));

        // When it is resolved
        var spec = Assert.Single(ProbeModel(module).Specs);

        // Then the input is the endpoint's input type, with the string typed as an Id
        Assert.Equal("Probe.Input", spec.Input.Type.ToString());
        Assert.Equal(new BoundText("c-1", PType.Id), spec.Input.Fields[0].Value);
        Assert.Equal("Probe.Output", Assert.IsType<RespondsModel>(spec.Outcome).Value.Type.ToString());
    }

    [Theory]
    [InlineData("given input { customerId: \"c\", items: [] }", "when called", "then responds { totalQuantity: 0 }", "PH330")]
    [InlineData("given items = []", "when handled", "then responds { totalQuantity: 0 }", "PH331")]
    [InlineData("given input { items: [] }", "when handled", "then responds { totalQuantity: 0 }", "PH314")]
    [InlineData("given input { customerId: \"c\", items: [] }", "when handled", "then returns 0", "PH333")]
    [InlineData("given input { customerId: \"c\", items: [] }", "when handled", "then fails with EmptyOrder", "PH334")]
    public void Invalid_endpoint_specs_are_reported(string given, string when, string then, string code)
    {
        // Given an endpoint spec with one mistake
        var source = ProbeWithSpec(Spec("bad", given, when, then));

        // When it is resolved
        // Then the mistake has its own code
        Assert.Equal([code], Codes(source));
    }

    [Fact]
    public void An_endpoint_spec_with_no_input_is_PH331()
    {
        // Given an endpoint spec with no given at all
        var source = ProbeWithSpec(Spec("no input", "when handled", "then responds { totalQuantity: 0 }"));

        // When it is resolved
        // Then it is reported
        Assert.Equal(["PH331"], Codes(source));
    }
}
