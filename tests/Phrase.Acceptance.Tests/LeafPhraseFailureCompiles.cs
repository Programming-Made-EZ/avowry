using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler;
using Phrase.Adapters.CSharp;

namespace Phrase.Acceptance.Tests;

// Feature: A leaf phrase can fail with its declared code
//   As Ari, I want a failing leaf phrase to surface its declared typed failure code
//   and pass its failure spec, so that I can verify an expected failure path end to end.
public class LeafPhraseFailureCompiles
{
    [Fact]
    [Trait("Slice", "043")]
    public async Task Failing_leaf_phrase_surfaces_its_declared_code_and_its_spec_passes()
    {
        // Given a leaf phrase that declares and raises a failure code with a matching spec
        const string source = """
            module Orders

            avow to "ensure {quantity: Int} is positive"
              fails with InvalidQuantity
              spec "rejects zero"
                given quantity = 0
                when called
                then fails with InvalidQuantity
              native csharp
                Fail("InvalidQuantity");
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the runtime exposes the typed failure code and the generated failure spec passes
        var runtime = File.ReadAllText(workspace.FindSingle("AvowryRuntime.cs"));
        Assert.Contains("sealed class AvowryFailure", runtime, StringComparison.Ordinal);
        Assert.Contains("public string Code", runtime, StringComparison.Ordinal);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
