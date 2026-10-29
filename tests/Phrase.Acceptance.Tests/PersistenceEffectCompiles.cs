using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler;
using Phrase.Adapters.CSharp;

namespace Phrase.Acceptance.Tests;

// Feature: A persistence effect with a generated in-memory implementation
//   As Dana, I want a declared persistence effect to come with a generated in-memory
//   implementation that specs can use, so that a service that saves and loads items
//   is testable without a database.
public class PersistenceEffectCompiles
{
    [Fact]
    [Trait("Slice", "132")]
    public async Task Saving_and_loading_items_uses_the_generated_in_memory_store()
    {
        // Given a phrase that declares an ItemStore effect for saving and loading one Item type.
        // The grammar already has effects, records, fails with, and native csharp. It has no
        // effect members: save, find and load are vocabulary, so the phrase calls the effect by name.
        const string source = """
            module Inventory

            type Item
              id: Text
              name: Text

            avow to "save {item: Item} and load it back" -> Item
              effects ItemStore
              spec "the saved item loads back"
                given item = { id: "a", name: "Widget" }
                when called
                then returns { id: "a", name: "Widget" }
              native csharp
                itemstore.Save(item);
                return itemstore.Find(item.Id)!;

            avow to "load item {id: Text}" -> Item
              effects ItemStore
              fails with NotFound
              spec "an unknown id is NotFound"
                given id = "missing"
                when called
                then fails with NotFound
              native csharp
                var loaded = itemstore.Find(id);
                if (loaded is null)
                    Fail("NotFound");
                return loaded;

            avow to "save {items: List<Item>} and list them" -> List<Item>
              effects ItemStore
              spec "several saved items stay in order"
                given items = [{ id: "a", name: "Widget" }, { id: "b", name: "Gadget" }]
                when called
                then returns [{ id: "a", name: "Widget" }, { id: "b", name: "Gadget" }]
              native csharp
                foreach (var item in items)
                    itemstore.Save(item);
                return itemstore.List();
            """;

        // When it is compiled with the C# provider and written to a temp folder
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then the generated API has the store interface and an in-memory implementation registered for the app
        var phrases = File.ReadAllText(workspace.FindSingle("Phrases.cs"));
        Assert.Contains("public interface IItemStore", phrases, StringComparison.Ordinal);
        Assert.Contains("void Save(Item item)", phrases, StringComparison.Ordinal);
        Assert.Contains("Item? Find(string id)", phrases, StringComparison.Ordinal);
        Assert.Contains("IReadOnlyList<Item> List()", phrases, StringComparison.Ordinal);
        Assert.Contains("class InMemoryItemStore : IItemStore", phrases, StringComparison.Ordinal);
        var program = File.ReadAllText(workspace.FindSingle("Program.cs"));
        Assert.Contains("AddSingleton<IItemStore, InMemoryItemStore>()", program, StringComparison.Ordinal);

        // And the spec double from card 047 is still the object the specs construct
        var specs = File.ReadAllText(workspace.FindSingle("PhraseSpecs.cs"));
        Assert.Contains("sealed class ItemStoreDouble : InMemoryItemStore", specs, StringComparison.Ordinal);
        Assert.Contains("new ItemStoreDouble()", specs, StringComparison.Ordinal);

        // And one saved item loads back, an unknown id is NotFound, and several stay in order
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(3, result.Total);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    [Trait("Slice", "132")]
    public async Task Phrase_with_no_effect_emits_no_store()
    {
        // Given a phrase that declares no effect
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

        // Then the generated projects contain no store and the spec still passes
        foreach (var file in Directory.GetFiles(workspace.Root, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("InMemory", text, StringComparison.Ordinal);
            Assert.DoesNotContain("AddSingleton", text, StringComparison.Ordinal);
            Assert.DoesNotContain("interface I", text, StringComparison.Ordinal);
        }

        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(1, result.Total);
        Assert.Equal(0, result.Failed);
    }
}
