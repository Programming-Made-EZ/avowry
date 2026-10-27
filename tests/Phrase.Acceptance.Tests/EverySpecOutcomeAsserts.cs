using Phrase.Acceptance.Tests.Support;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: Every generated spec outcome is useful when behavior drifts
//   As Morgan, I want every generated spec outcome to assert its promised behavior and name a broken spec,
//   so that I can identify the exact consumer behavior that broke.
public class EverySpecOutcomeAsserts
{
    [Fact]
    [Trait("Slice", "050")]
    public async Task Failure_response_and_composite_specs_each_pass()
    {
        // Given a failure-code spec, an endpoint-response spec, and a composite phrase spec
        const string source = """
            module Outcomes

            phrase "reject {value: Int}"
              fails with Rejected
              spec "rejects the value"
                given value = 1
                when called
                then fails with Rejected
              native csharp
                PhraseFailure.Raise("Rejected");

            phrase "identity {value: Int}" -> Int
              native csharp
                return value;

            phrase "process {value: Int}"
              spec "processes through a composite"
                given value = 2
                when called
                then succeeds
              steps
                let result = identity value

            endpoint Echo : POST /echo
              input
                value: Int
              output
                value: Int
              steps
                let result = identity input.value
                respond with { value: result }
              spec "responds with the value"
                given input { value: 3 }
                when handled
                then responds { value: 3 }
            """;

        // When the generated Specs project is run
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then one generated test per spec builds and passes with its outcome assertion
        Assert.True(result.Built, result.Log);
        Assert.Equal(3, result.Total);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "050")]
    public async Task Wrong_expected_value_fails_once_and_names_the_spec_in_its_message()
    {
        // Given one spec whose named expectation is deliberately wrong
        const string source = """
            module Outcomes

            phrase "identity {value: Int}" -> Int
              spec "returns the promised value"
                given value = 1
                when called
                then returns 99
              native csharp
                return value;
            """;

        // When the generated Specs project is run
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then exactly that test fails and its assertion message names the spec and both values
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(1, result.Failed);
        Assert.Contains("Spec \"returns the promised value\" expected 99 but got 1.", result.Log, StringComparison.Ordinal);
    }
}
