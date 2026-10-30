using Phrase.Acceptance.Tests.Support;
using Avowry.Adapters.CSharp;
using Avowry.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: Generated code uses the final Avowry names
//   As Morgan, I want generated C# to use the final public names everywhere,
//   so that I do not have to change generated code after the first publish.
public class GeneratedCodeUsesAvowryNames
{
    [Fact]
    [Trait("Slice", "193")]
    public async Task Generated_code_uses_only_the_final_Avowry_names()
    {
        // Given an avow whose generated API, failure path, spec and build metadata expose shared names
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

        // When Morgan compiles it with the C# adapter
        var output = AvowryCompiler.Compile(source, new CSharpProvider(), "orders.avowry");
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then every generated filename and C# symbol uses Avowry or Avow, never the old public name
        _ = workspace.FindSingle("AvowryRuntime.cs");
        _ = workspace.FindSingle("AvowryBuild.cs");
        _ = workspace.FindSingle("AvowSpecs.cs");
        _ = workspace.FindSingle("Avows.cs");
        var generated = string.Join("\n", Directory.GetFiles(workspace.Root, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
        Assert.Contains("class AvowAttribute", generated, StringComparison.Ordinal);
        Assert.Contains("[Avow(\"ensure {quantity: Int} is positive\")]", generated, StringComparison.Ordinal);
        Assert.Contains("class AvowryFailure", generated, StringComparison.Ordinal);
        Assert.Contains("AssemblyMetadata(\"AvowryBuildId\"", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("Phrase", generated, StringComparison.Ordinal);

        // And the generated spec still builds and proves the declared failure behavior
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
