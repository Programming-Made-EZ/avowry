using Avowry.Compiler.Semantics;

namespace Avowry.Compiler.Tests.Diagnostics;

// Feature: Leading diagnostic messages name the avow (card 324 criterion 1)
public class LeadingMessagesSayAvowTests
{
    [Theory]
    [Trait("Criterion", "1")]
    [InlineData("M01-AV010", "Avow 'ping {m: Int}' has the same wording as 'ping {n: Int}'.")]
    [InlineData("M02-AV321", "Avow 'count things' returns Int; use 'let' to keep it.")]
    [InlineData("M03-AV320", "Avow 'do nothing' returns no value and cannot be used with 'let'.")]
    [InlineData("M04-AV327", "Avow 'ping {n: Int}' participates in a call cycle.")]
    [InlineData("M05-AV331-input", "Avow specs cannot bind input; use a hole binding.")]
    [InlineData("M06-AV331-hole", "Avow spec for 'check {n: Int}' does not bind hole 'q'. Valid holes: n.")]
    [InlineData("M07-AV334", "Avow 'check {n: Int}' cannot fail with 'Underflow'. Valid options: Overflow.")]
    [InlineData("M08-AV203", "Avow has no implementation; add `native <target>` or `steps`.")]
    public void The_eight_leading_messages_say_Avow(string id, string expected)
    {
        // Given a trigger source and the first diagnostic recorded for it
        var source = TriggerSources.Source(id);
        var baseline = TriggerSources.Baseline(id)[0];

        // When it is compiled
        var actual = Resolver.Resolve(source).Diagnostics.Single(diagnostic =>
            diagnostic.Code == baseline.Code &&
            diagnostic.Line == baseline.Line &&
            diagnostic.Column == baseline.Column);

        // Then its identity is unchanged and its full message names the avow
        Assert.Equal(baseline.Code, actual.Code);
        Assert.Equal(baseline.Line, actual.Line);
        Assert.Equal(baseline.Column, actual.Column);
        Assert.Equal(expected, actual.Message);
    }
}
