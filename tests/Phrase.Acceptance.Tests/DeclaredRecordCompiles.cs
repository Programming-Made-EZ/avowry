using Phrase.Acceptance.Tests.Support;
using Avowry.Compiler;
using Avowry.Adapters.CSharp;

namespace Phrase.Acceptance.Tests;

// Feature: A declared record input compiles to C#
//   As Ari, I want a phrase with a declared record input to compile to C# and pass
//   a spec that constructs the record, so that I can use domain-shaped input in
//   working generated behavior.
public class DeclaredRecordCompiles
{
    [Fact]
    [Trait("Slice", "041")]
    public async Task Declared_record_input_phrase_compiles_and_its_spec_passes()
    {
        // Given a module with one declared record and a phrase that accepts it
        const string source = """
            module Orders

            type Order
              sku: Text
              quantity: Int

            avow to "total {order: Order}" -> Int
              spec "totals the order"
                given order = { sku: "A", quantity: 2 }
                when called
                then returns 2
              native csharp
                return order.Quantity;
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the API project emits the declared record and the generated spec passes
        var types = workspace.FindSingle("Types.cs");
        Assert.Contains("record Order", File.ReadAllText(types));
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "041")]
    public async Task Zero_declared_types_still_builds()
    {
        // Given a module with no declared types
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
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the generated project still builds with zero declared types
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
