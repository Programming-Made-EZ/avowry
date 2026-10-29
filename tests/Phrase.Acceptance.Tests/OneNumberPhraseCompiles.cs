using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler;
using Phrase.Adapters.CSharp;

namespace Phrase.Acceptance.Tests;

// Feature: One phrase with one number input compiles to C#
//   As Ari, I want one phrase with an Int input and a Given/When/Then spec
//   to compile into C# API and Specs projects with a passing generated test,
//   so that I can see my first behavior work end to end in one attempt.
public class OneNumberPhraseCompiles
{
    [Fact]
    [Trait("Slice", "039")]
    public async Task One_int_phrase_compiles_and_its_spec_passes()
    {
        // Given a phrase with a single Int input and one Given/When/Then spec
        const string source = """
            module Numbers

            avow to "double {n: Int}" -> Int
              spec "doubles two"
                given n = 2
                when called
                then returns 4
              native csharp
                return n * 2;
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then both the API project and the Specs project are present and build
        _ = workspace.FindSingle("*.Api.csproj");
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);

        // And exactly one generated spec runs and passes
        Assert.True(result.Total == 1, result.Log);
        Assert.True(result.Failed == 0, result.Log);
    }
}
