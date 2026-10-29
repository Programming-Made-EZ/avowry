using Phrase.Acceptance.Tests.Support;
using Phrase.Adapters.CSharp;
using Avowry.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: A native C# block is emitted verbatim and builds
//   As Ari, I want the generated method body to contain the native C# block exactly as written,
//   so that I can drop in target-specific logic without a formatting or build regression.
public class NativeCSharpBlockCompiles
{
    [Fact]
    [Trait("Slice", "044")]
    public async Task Native_csharp_block_appears_verbatim_and_the_generated_project_builds_cleanly()
    {
        // Given a phrase with a multi-line native C# block that includes nested braces and a blank line
        const string source = """
            module Numbers

            avow to "double {n: Int}" -> Int
              spec "doubles two"
                given n = 2
                when called
                then returns 4
              native csharp
                if (n == 2)
                {

                    return n * 2;
                }
                return n * 3;
            """;

        // When it is compiled and written to a temp folder
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the generated method contains the block text exactly, including its blank line and brace nesting
        var phrases = File.ReadAllText(workspace.FindSingle("Avows.cs"));
        Assert.Contains("if (n == 2)\n        {\n\n            return n * 2;\n        }\n        return n * 3;", phrases, StringComparison.Ordinal);

        // And the generated specs project builds cleanly with no warnings in the log
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.DoesNotContain("warning", result.Log, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
