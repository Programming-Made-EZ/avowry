using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler;
using Phrase.Adapters.CSharp;

namespace Phrase.Acceptance.Tests;

// Feature: Compile a .phrase endpoint to C#
//   As an engineer (or an LLM) writing behavior as phrases
//   I want the C# provider to emit a FastEndpoints API and xUnit specs
//   So that every spec in the .phrase source runs as a real test in the target
//
// This is the outer loop of outside-in TDD. It stays red until the compiler
// pipeline is driven out by inner unit tests. It is tagged Pending so trunk CI
// stays green; remove the trait once it passes.
public class CompileEndpointToCSharp
{
    [Fact]
    [Trait("Status", "Pending")]
    public async Task Generated_csharp_builds_and_every_spec_passes()
    {
        // Given a .phrase file describing one endpoint, its leaf phrases, and their specs
        var source = Fixture.Read("orders.phrase");
        var expectedSpecCount = Fixture.CountSpecs(source);

        // When it is compiled with the C# provider and written to disk
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the generated specs project builds
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);

        // And one test runs per spec in the source
        Assert.Equal(expectedSpecCount, result.Total);

        // And every test passes
        Assert.Equal(0, result.Failed);
    }
}
