using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler;
using Phrase.Adapters.CSharp;

namespace Phrase.Acceptance.Tests;

// Feature: A composite phrase calls its leaf phrases in order
//   As Dana, I want a composite phrase to call its leaves in source order,
//   bind each result, and expose the failures those leaves can raise,
//   so that the service behavior I specify is preserved in the generated program.
public class CompositePhraseOrderCompiles
{
    [Fact]
    [Trait("Slice", "045")]
    public async Task Composite_phrase_calls_three_leaves_in_order_and_surfaces_a_leaf_failure()
    {
        // Given three Int and Text leaves and a composite that binds each result, in source order
        const string source = """
            module Quotes

            avow to "price of {sku: Text}" -> Int
              fails with UnknownSku
              native csharp
                if (sku.Length == 0) Fail("UnknownSku");
                return 2;

            avow to "units of {name: Text}" -> Int
              fails with MissingName
              native csharp
                if (name.Length == 0) Fail("MissingName");
                return 3;

            avow to "label of {name: Text}" -> Text
              native csharp
                return name;

            avow to "quote {sku: Text} for {name: Text}"
              steps
                let price = price of sku
                let units = units of name
                let label = label of name
              spec "rejects an unknown sku"
                given sku = ""
                and name = "Ada"
                when called
                then fails with UnknownSku
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the generated composite keeps call order, the three locals, and the union of leaf failures
        var phrases = File.ReadAllText(workspace.FindSingle("Phrases.cs"));
        Assert.Contains("/// <remarks>Fails with: MissingName, UnknownSku</remarks>", phrases, StringComparison.Ordinal);
        Assert.Contains(
            "        // let price = price of sku\n" +
            "        var price = Phrases.PriceOfSku(sku);\n" +
            "        // let units = units of name\n" +
            "        var units = Phrases.UnitsOfName(name);\n" +
            "        // let label = label of name\n" +
            "        var label = Phrases.LabelOfName(name);",
            phrases,
            StringComparison.Ordinal);

        // And the generated composite spec passes, including the leaf that raises UnknownSku
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.DoesNotContain("warning", result.Log, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
