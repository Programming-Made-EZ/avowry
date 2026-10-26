using Phrase.Compiler.Semantics;
using static Phrase.Compiler.Tests.Semantics.Resolved;

namespace Phrase.Compiler.Tests.Semantics;

// Feature: Resolver hardening
//   A program the parser already rejected must still resolve. Repeated hole names
//   (PH206) degrade to a model instead of throwing inside the resolver.
public class ResolverHardeningTests
{
    [Fact]
    public void A_composite_with_a_repeated_hole_resolves_instead_of_throwing()
    {
        // Given a composite whose template repeats a hole name
        var source = Program("""
            phrase "add {n: Int} to {n: Int}"
              steps
                ensure 1 is positive
            """);

        // When it is resolved
        var result = Resolver.Resolve(source);

        // Then the parser diagnostic is kept and the composite is bound
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "PH206");
        var phrase = Assert.Single(result.Module.Phrases, candidate => candidate.Key == "add _ to _");
        Assert.NotNull(phrase.Steps);
        Assert.NotEmpty(phrase.Steps);
    }

    [Fact]
    public void A_call_to_a_phrase_with_a_repeated_hole_resolves_instead_of_throwing()
    {
        // Given a phrase whose template repeats a hole name, and a step that calls it
        var source = Program(
            """
            phrase "add {n: Int} to {n: Int}"
              native csharp
                return;
            """,
            Probe("add 1 to 2"));

        // When it is resolved
        var result = Resolver.Resolve(source);

        // Then the call is bound, once per hole occurrence, and resolution does not throw
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "PH206");
        var step = Assert.IsType<BoundCallStep>(Assert.Single(ProbeModel(result.Module).Steps));
        Assert.Equal(2, step.Call.Arguments.Count);
        Assert.All(step.Call.Arguments, argument => Assert.Equal("n", argument.Hole.Name));
    }
}
