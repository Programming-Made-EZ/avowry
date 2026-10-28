using Phrase.Compiler.Syntax;

namespace Phrase.Compiler.Tests.Syntax;

// Feature: Canonical source formatting
//   Formatting a parsed step reproduces canonical source, so generated code can
//   quote the exact line it came from.
public class SourceFormatterTests
{
    private static Step OnlyStep(string step)
    {
        var source = $"module T\nendpoint E : GET /e\n  input\n    n: Int\n  steps\n    {step}\n";
        var result = Parser.Parse(source);
        Assert.True(result.Diagnostics.Count == 0, string.Join("\n", result.Diagnostics));
        return Assert.Single(result.File.Endpoints[0].Steps);
    }

    [Theory]
    [InlineData("ensure input.items is not empty")]
    [InlineData("let total = total quantity of input.items")]
    [InlineData("let completed = input with { n: 1 }")]
    [InlineData("respond with { totalQuantity: total }")]
    [InlineData("respond with {}")]
    [InlineData("fail with NotBuilt")]
    [InlineData("retry \"sync\" up to 3 times with [1, 2.5] and { fast: true, none: none }")]
    public void Canonical_steps_round_trip(string step)
    {
        // Given a step written canonically
        // When it is parsed and formatted
        var formatted = SourceFormatter.Format(OnlyStep(step));

        // Then the text is unchanged
        Assert.Equal(step, formatted);
    }

    [Fact]
    public void Spacing_is_normalized()
    {
        // Given a step with irregular spacing
        // When it is parsed and formatted
        var formatted = SourceFormatter.Format(OnlyStep("respond   with {totalQuantity:total,}"));

        // Then the canonical spacing is produced
        Assert.Equal("respond with { totalQuantity: total }", formatted);
    }

    [Fact]
    public void Strings_are_re_escaped()
    {
        // Given a step with an escaped quote and backslash
        // When it is parsed and formatted
        var formatted = SourceFormatter.Format(OnlyStep("log \"say \\\"hi\\\" C:\\\\x\""));

        // Then the escapes are restored
        Assert.Equal("log \"say \\\"hi\\\" C:\\\\x\"", formatted);
    }
}
