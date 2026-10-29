using Phrase.Acceptance.Tests.Support;
using Xunit.Abstractions;

namespace Phrase.Acceptance.Tests;

// Feature: Generated specs catch broken C# phrase implementations
//   As Dana, I want the specs generated from orders.avowry to fail when an
//   implementation is broken, so that a green run means the behaviour I
//   specified is really there.
public class GeneratedSpecsMutationCheck
{
    private readonly ITestOutputHelper output;

    public GeneratedSpecsMutationCheck(ITestOutputHelper output) => this.output = output;

    [Fact]
    [Trait("Slice", "106")]
    public async Task Orders_specs_kill_at_least_ninety_percent_deterministically()
    {
        // Given a committed mutation fixture with boundary specs and the frozen orders fixture
        var mutationSource = Fixture.Read("orders-mutation.avowry");
        var ordersSource = Fixture.Read("orders.avowry");

        // When each fixture's generated specs are run against every mutation twice
        var first = await CSharpMutationCheck.RunAsync(mutationSource);
        var second = await CSharpMutationCheck.RunAsync(mutationSource);
        var ordersFirst = await CSharpMutationCheck.RunAsync(ordersSource);
        var ordersSecond = await CSharpMutationCheck.RunAsync(ordersSource);
        output.WriteLine($"Mutation fixture:{Environment.NewLine}{first.Report}");
        output.WriteLine($"orders.avowry (informational):{Environment.NewLine}{ordersFirst.Report}");

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

        // And the unchanged orders fixture reports its visible gap deterministically, without a threshold
        Assert.Equal(ordersFirst.DeterministicSummary, ordersSecond.DeterministicSummary);
    }
}
