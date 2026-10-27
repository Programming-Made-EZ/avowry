using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: Generated specs catch broken C# phrase implementations
//   As Dana, I want the specs generated from orders.phrase to fail when an
//   implementation is broken, so that a green run means the behaviour I
//   specified is really there.
public class GeneratedSpecsMutationCheck
{
    [Fact]
    [Trait("Slice", "106")]
    public async Task Orders_specs_kill_at_least_ninety_percent_deterministically()
    {
        // Given the orders fixture, its hand-written native C# bodies, and fixed mutations
        var source = Fixture.Read("orders.phrase");

        // When the generated specs are run against every mutation twice
        var first = await CSharpMutationCheck.RunAsync(source);
        var second = await CSharpMutationCheck.RunAsync(source);

        // Then totals and per-phrase results report killed over buildable mutants
        Assert.Equal(9, first.Total.Buildable);
        Assert.Equal(9, first.Total.Killed);
        Assert.Equal(3, first.Phrases.Count);
        Assert.All(first.Phrases, phrase => Assert.True(phrase.Buildable > 0, first.Report));

        // And the kill rate meets Dana's threshold and every survivor is named
        Assert.True(first.KillRate >= 0.90, first.Report);
        Assert.All(first.Survivors, survivor =>
        {
            Assert.False(string.IsNullOrWhiteSpace(survivor.Phrase));
            Assert.False(string.IsNullOrWhiteSpace(survivor.Operator));
        });
        Assert.Contains("Total: 9 / 9 killed", first.Report, StringComparison.Ordinal);
        Assert.Contains("Survivors: none", first.Report, StringComparison.Ordinal);
        Assert.Contains("Run time:", first.Report, StringComparison.Ordinal);

        // And two runs report identical mutation numbers and survivor names
        Assert.Equal(first.DeterministicSummary, second.DeterministicSummary);
    }
}
