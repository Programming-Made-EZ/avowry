using System.Reflection;
using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;
using Avowry.Compiler;
using Phrase.Adapters.CSharp;
using Xunit;

namespace Phrase.Acceptance.Tests;

// Feature: Dana's orders.avowry compiles and proves its generated specs
//   As Dana, I want the orders.avowry fixture to emit only the C# it actually
//   needs, so that the generated API and specs prove the behavior I specified
//   without depending on unimplemented option, effect or nested-record mappings.
public class OrdersPhraseCompiles
{
    [Fact]
    [Trait("Slice", "087")]
    public async Task Orders_phrase_compiles_to_csharp_and_its_generated_specs_pass()
    {
        // Given Dana's orders.avowry fixture and the independent spec count
        var source = Fixture.Read("orders.avowry");
        var expectedSpecCount = Fixture.CountSpecs(source);

        // When it is compiled with the C# provider and written to a temp workspace
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the API and Specs projects are generated for the constructs this file uses
        _ = workspace.FindSingle("*.Api.csproj");
        _ = workspace.FindSingle("*.Specs.csproj");

        var generated = string.Concat(Directory.EnumerateFiles(workspace.Root, "*.cs", SearchOption.AllDirectories)
            .Select(File.ReadAllText));
        Assert.DoesNotContain("Option<", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("interface I", generated, StringComparison.Ordinal);
        var declaredTypeText = File.ReadAllText(workspace.FindSingle("Types.cs"));
        var recordDeclarations = declaredTypeText
            .Split('\n')
            .Where(line => line.Contains("record ", StringComparison.Ordinal) && !line.TrimStart().StartsWith("//", StringComparison.Ordinal));
        Assert.Single(recordDeclarations);

        // And the generated specs build, match the fixture count and produce zero failures
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(expectedSpecCount, result.Total);
        Assert.Equal(0, result.Failed);
        Assert.DoesNotContain("warning", result.Log, StringComparison.OrdinalIgnoreCase);

        // And the older Pending gate is gone from the end-to-end acceptance test
        foreach (var method in typeof(CompileEndpointToCSharp).GetMethods())
        {
            var pendingStatus = method.GetCustomAttributesData()
                .Any(attr => attr.AttributeType.Name == "TraitAttribute"
                    && attr.ConstructorArguments.Count == 2
                    && attr.ConstructorArguments[0].Value as string == "Status"
                    && attr.ConstructorArguments[1].Value as string == "Pending");
            Assert.False(pendingStatus, $"{method.Name} should not be marked Pending");
        }
    }
}
