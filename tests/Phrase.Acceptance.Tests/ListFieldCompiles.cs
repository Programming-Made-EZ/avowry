using Phrase.Acceptance.Tests.Support;
using Avowry.Compiler;
using Phrase.Adapters.CSharp;

namespace Phrase.Acceptance.Tests;

// Feature: A declared model with a list field compiles to C#
//   As Ari, I want a declared model with a list field to compile into usable C# types,
//   so that I can represent realistic request data without native type definitions.
public class ListFieldCompiles
{
    [Fact]
    [Trait("Slice", "042")]
    public async Task Declared_model_with_a_list_field_compiles_and_its_spec_passes()
    {
        // Given a declared model whose field is a list of text
        const string source = """
            module Orders

            type Order
              skus: List<Text>

            avow to "count of {order: Order}" -> Int
              spec "counts the skus"
                given order = { skus: ["A", "B"] }
                when called
                then returns 2
              native csharp
                return order.Skus.Count;
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then List maps to IReadOnlyList<T> and the generated spec passes
        var types = File.ReadAllText(workspace.FindSingle("Types.cs"));
        Assert.Contains("IReadOnlyList<string> Skus", types, StringComparison.Ordinal);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "042")]
    public async Task Type_named_like_a_csharp_keyword_is_escaped_and_builds()
    {
        // Given a record named like the C# keyword event, with a field named like class
        const string source = """
            module Orders

            type Event
              class: Int

            avow to "value of {item: Event}" -> Int
              spec "reads the field"
                given item = { class: 2 }
                when called
                then returns 2
              native csharp
                return item.Class;
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the keyword-like names are escaped with @ and the generated spec passes
        var types = File.ReadAllText(workspace.FindSingle("Types.cs"));
        Assert.Contains("record @Event", types, StringComparison.Ordinal);
        Assert.Contains("@Class", types, StringComparison.Ordinal);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
