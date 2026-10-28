using Phrase.Acceptance.Tests.Support;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;
using System.Net;

namespace Phrase.Acceptance.Tests;

// Feature: GET endpoints read one item and a collection from the prototype store
//   As Dana, I want GET endpoints to list stored items and find one by its route id,
//   so that I can describe the read side of a small service.
public class GetEndpointsCompile
{
    [Fact]
    [Trait("Slice", "131")]
    public void Get_and_route_parameters_are_owned_by_the_web_extension()
    {
        // Given a Phrase module whose web surface is a GET route with one parameter
        const string source = """
            module Inventory

            endpoint GetItem : GET /items/{id}
              input
                id: Id
              output
                id: Id
              steps
                respond with { id: input.id }
              spec "the route id is the response id"
                given input { id: "a" }
                when handled
                then responds { id: "a" }
            """;

        // When Dana compiles it through the extension registry
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then web is loaded and the generated binding keeps the parameterized GET route
        Assert.Equal(["core", "web"], ExtensionLoad.Active(source));
        var endpoint = File.ReadAllText(workspace.FindSingle("GetItem.cs.cs"));
        Assert.Contains("Get(\"/items/{id}\")", endpoint, StringComparison.Ordinal);
        Assert.Contains("input.Id", endpoint, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "131")]
    public async Task Generated_get_specs_observe_route_binding_status_body_and_store_results()
    {
        // Given two GET endpoints over the generated ItemStore, including a route parameter,
        // a declared NotFound, and Zero/One/Many collection examples
        const string source = """
            module Inventory

            type Item
              id: Id
              name: Text

            phrase "find item {id: Id}" -> Item
              effects ItemStore
              fails with NotFound
              native csharp
                var item = itemstore.Find(id);
                if (item is null)
                    Fail("NotFound");
                return item;

            phrase "list items" -> List<Item>
              effects ItemStore
              native csharp
                return itemstore.List();

            endpoint GetItem : GET /items/{id}
              input
                id: Id
              output
                item: Item
              steps
                let item = find item input.id
                respond with { item: item }
              spec "known id returns the stored item"
                given input { id: "a" }
                when handled
                then responds { item: { id: "a", name: "Widget" } }
              spec "unknown id is NotFound"
                given input { id: "missing" }
                when handled
                then fails with NotFound

            endpoint ListItems : GET /items
              input
                query: Option<Text>
              output
                items: List<Item>
              steps
                let items = list items
                respond with { items: items }
              spec "Zero: an empty store returns an empty list"
                given input { query: none }
                when handled
                then responds { items: [] }
              spec "One: one stored item is returned"
                given input { query: none }
                when handled
                then responds { items: [{ id: "a", name: "Widget" }] }
              spec "Many: all stored items stay in order"
                given input { query: none }
                when handled
                then responds { items: [{ id: "a", name: "Widget" }, { id: "b", name: "Gadget" }] }
            """;

        // When Dana compiles the module and runs its generated specs
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then the generated specs drive the real endpoint HandleAsync path in process
        Assert.True(result.Built, result.Log);
        Assert.Equal(5, result.Total);
        Assert.Equal(0, result.Failed);
        var getSpecs = File.ReadAllText(workspace.FindSingle("GetItemSpecs.cs.cs"));
        Assert.Contains("Factory.Create<GetItemEndpoint>", getSpecs, StringComparison.Ordinal);
        Assert.Contains("HandleAsync", getSpecs, StringComparison.Ordinal);
        Assert.Contains("StatusCode", getSpecs, StringComparison.Ordinal);

        // And the generated endpoint receives ItemStore and retains the route parameter in its request
        var endpoint = File.ReadAllText(workspace.FindSingle("GetItem.cs.cs"));
        Assert.Contains("IItemStore", endpoint, StringComparison.Ordinal);
        Assert.Contains("input.Id", endpoint, StringComparison.Ordinal);
        Assert.Contains("Get(\"/items/{id}\")", endpoint, StringComparison.Ordinal);

        // And a real GET binds the path id into the request before the endpoint maps NotFound to 422
        var api = await GeneratedApi.StartAsync(workspace.FindSingle("*.Api.csproj"));
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl), Timeout = TimeSpan.FromSeconds(30) };
            var missing = await http.GetAsync("/items/missing");
            var body = await missing.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.UnprocessableEntity, missing.StatusCode);
            Assert.Contains("NotFound", body, StringComparison.Ordinal);
        }
        finally
        {
            await api.DisposeAsync();
        }
    }
}
