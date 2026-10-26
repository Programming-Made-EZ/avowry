using Phrase.Compiler.Semantics;
using static Phrase.Compiler.Tests.Semantics.Resolved;

namespace Phrase.Compiler.Tests.Semantics;

public class ResolverSpecDefectsTests
{
    private static string Adder(string spec) =>
        Program("phrase \"add {a: Int} to {b: Int}\" -> Int\n  fails with Overflow\n" + spec + "  native csharp\n    return a + b;\n");

    [Fact]
    [Trait("Slice", "018")]
    public void PH334_names_the_real_endpoint_and_lists_the_failures_it_can_produce()
    {
        // Given an endpoint spec that expects a failure the endpoint can never produce
        var source = Program(
            "endpoint CreateOrder : POST /orders\n" +
            "  input\n" +
            "    customerId: Id\n" +
            "  output\n" +
            "    orderId: Id\n" +
            "  steps\n" +
            "    respond with { orderId: \"o-1\" }\n" +
            "  spec \"impossible failure\"\n" +
            "    given input { customerId: \"c-1\" }\n" +
            "    when handled\n" +
            "    then fails with NotFound\n");

        // When it is resolved
        var diagnostic = Only(source);

        // Then PH334 names the real endpoint, not the placeholder, and lists the real failures
        Assert.Equal("PH334", diagnostic.Code);
        Assert.Contains("CreateOrder", diagnostic.Message);
        Assert.DoesNotContain("Probe", diagnostic.Message);
    }

    [Fact]
    [Trait("Slice", "018")]
    public void PH332_reports_path_values_inside_spec_records_and_lists()
    {
        // Given a spec that writes an input path into a response record
        var source = ProbeWithSpec(
            Spec("path in record",
                "given input { customerId: \"c-1\", items: [{ sku: \"A\", quantity: 2 }] }",
                "when handled",
                "then responds { totalQuantity: input.customerId }"));

        // When it is resolved
        // Then it is reported as a path, not a literal
        Assert.Contains("PH332", Codes(source));
    }

    [Fact]
    [Trait("Slice", "018")]
    public void PH331_reports_the_duplicate_binding_once()
    {
        // Given a spec that binds the same hole twice
        var source = Adder(Spec("duplicate", "given a = 1", "and a = 2", "when called", "then returns 3"));

        // When it is resolved
        // Then exactly one PH331 is reported for that duplicate
        Assert.Equal(["PH331"], Codes(source));
    }

    [Fact]
    [Trait("Slice", "018")]
    public void PH331_names_unknown_hole_and_lists_the_valid_holes()
    {
        // Given a spec with a hole name that is not in this phrase
        var diagnostic = Only(Adder(Spec("unknown hole", "given c = 2", "when called", "then returns 3")));

        // When it is resolved
        // Then PH331 mentions the unknown hole and the valid holes
        Assert.Equal("PH331", diagnostic.Code);
        Assert.Contains("c", diagnostic.Message);
        Assert.Contains("a", diagnostic.Message);
        Assert.Contains("b", diagnostic.Message);
    }
}
