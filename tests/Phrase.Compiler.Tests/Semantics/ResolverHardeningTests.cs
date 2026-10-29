using Avowry.Compiler.Semantics;
using static Phrase.Compiler.Tests.Semantics.Resolved;

namespace Phrase.Compiler.Tests.Semantics;

// Feature: Resolver hardening
//   A program the parser already rejected must still resolve. Repeated hole names
//   (PH206) degrade to a model instead of throwing inside the resolver.
public class ResolverHardeningTests
{
    [Fact]
    public void Bare_word_variable_of_the_wrong_type_is_PH312_naming_both_types()
    {
        // Given a Bool let variable passed by bare word to an Int hole
        var source = Program(
            "avow to \"whether {items: List<LineItem>} is ready\" -> Bool\n  native csharp\n    return true;\n",
            Probe("let ready = whether input.items is ready", "ensure ready is positive"));

        // When it is resolved
        var diagnostic = Only(source);

        // Then the mismatch names both the actual and expected types
        Assert.Equal("PH312", diagnostic.Code);
        Assert.Contains("Bool", diagnostic.Message);
        Assert.Contains("Int", diagnostic.Message);
    }

    [Fact]
    public void None_for_a_Text_hole_is_PH312()
    {
        // Given none passed to a hole that requires Text
        var source = Program(
            "avow to \"retry {name: Text} up to {n: Int} times\"\n  native csharp\n    return;\n",
            Probe("retry none up to 3 times"));

        // When it is resolved
        var diagnostic = Only(source);

        // Then none is rejected because it fits only Option<T>
        Assert.Equal("PH312", diagnostic.Code);
    }

    [Fact]
    public void Record_literal_for_a_non_record_hole_is_PH312()
    {
        // Given a record literal passed to a hole that requires Int
        var source = Program(Probe("ensure { value: 3 } is positive"));

        // When it is resolved
        var diagnostic = Only(source);

        // Then the record is rejected as a type mismatch
        Assert.Equal("PH312", diagnostic.Code);
    }

    [Theory]
    [InlineData("2.5")]
    [InlineData("3")]
    [InlineData("none")]
    public void Option_Decimal_accepts_decimal_whole_number_and_none(string value)
    {
        // Given a value compatible with an Option<Decimal> hole
        var source = Program(
            "avow to \"store optional {value: Option<Decimal>}\"\n  native csharp\n    return;\n",
            Probe($"store optional {value}"));

        // When it is resolved
        var diagnostics = Resolver.Resolve(source).Diagnostics;

        // Then it is accepted without diagnostics
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Option_Id_accepts_a_string_literal_with_literal_widening()
    {
        // Given a string literal passed to an Option<Id> hole
        var source = Program(
            "avow to \"store optional {value: Option<Id>}\"\n  native csharp\n    return;\n",
            Probe("store optional \"c-1\""));

        // When it is resolved
        var diagnostics = Resolver.Resolve(source).Diagnostics;

        // Then it widens to Id and is accepted without diagnostics
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Option_Int_rejects_a_decimal_literal_with_PH312()
    {
        // Given a decimal literal passed to an Option<Int> hole
        var source = Program(
            "avow to \"store optional {value: Option<Int>}\"\n  native csharp\n    return;\n",
            Probe("store optional 2.5"));

        // When it is resolved
        var diagnostic = Only(source);

        // Then the incompatible literal is rejected
        Assert.Equal("PH312", diagnostic.Code);
    }

    [Fact]
    public void Failed_let_is_bound_as_Error_and_does_not_cascade()
    {
        // Given a let whose phrase call fails, followed by a use of that variable
        var source = Program(Probe("let total = totally unknown phrase", "ensure total is positive"));

        // When it is resolved
        var result = Resolver.Resolve(source);

        // Then only the original unknown-phrase diagnostic is produced
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("PH001", diagnostic.Code);
    }

    [Fact]
    public void A_composite_with_a_repeated_hole_resolves_instead_of_throwing()
    {
        // Given a composite whose template repeats a hole name
        var source = Program("""
            avow to "add {n: Int} to {n: Int}"
              steps
                ensure 1 is positive
            """);

        // When it is resolved
        var result = Resolver.Resolve(source);

        // Then the parser diagnostic is kept and the composite is bound
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "PH206");
        var phrase = Assert.Single(result.Module.Avows, candidate => candidate.Key == "add _ to _");
        Assert.NotNull(phrase.Steps);
        Assert.NotEmpty(phrase.Steps);
    }

    [Fact]
    public void A_call_to_a_phrase_with_a_repeated_hole_resolves_instead_of_throwing()
    {
        // Given a phrase whose template repeats a hole name, and a step that calls it
        var source = Program(
            """
            avow to "add {n: Int} to {n: Int}"
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
