using System.Net;
using System.Text;
using Phrase.Acceptance.Tests.Support;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: PUT and DELETE endpoints update and remove a stored item
//   As Dana, I want PUT and DELETE endpoints that update and remove a stored item,
//   so that I can describe a complete small CRUD service.
//
// DELETE has no output. The grammar rejects `respond with` (PH322) and `then responds`
// (PH333) on an endpoint with no output. The remaining success outcome is `then succeeds`.
// A successful endpoint is already bound with Send.OkAsync, which is HTTP 200; with no
// response record that is 200 and an empty body. One spec has one outcome, so the later
// GET that proves the item is gone is the live request sequence, not a second `then`.
public class PutAndDeleteEndpointsCompile
{
    // The in-memory store starts empty, and an unknown id is NotFound, so the live
    // sequence posts one item before PUT. That POST is the setup that makes the id known.
    private const string Source = """
        module Inventory

        type Item
          id: Id
          name: Text

        avow to "save item {id: Id} named {name: Text}" -> Item
          effects ItemStore
          native csharp
            var item = new Item(id, name);
            itemstore.Save(item);
            return item;

        avow to "update item {id: Id} to {name: Text}" -> Item
          effects ItemStore
          fails with NotFound, EmptyName
          native csharp
            if (string.IsNullOrEmpty(name))
                Fail("EmptyName");
            var existing = itemstore.Find(id);
            if (existing is null)
                Fail("NotFound");
            var updated = new Item(id, name);
            itemstore.Save(updated);
            return updated;

        avow to "find item {id: Id}" -> Item
          effects ItemStore
          fails with NotFound
          native csharp
            var item = itemstore.Find(id);
            if (item is null)
                Fail("NotFound");
            return item;

        avow to "remove item {id: Id}"
          effects ItemStore
          fails with NotFound
          native csharp
            if (!itemstore.Remove(id))
                Fail("NotFound");

        endpoint CreateItem : POST /items
          input
            id: Id
            name: Text
          output
            item: Item
          steps
            let item = save item input.id named input.name
            respond with { item: item }

        endpoint UpdateItem : PUT /items/{id}
          input
            id: Id
            name: Text
          output
            item: Item
          steps
            let item = update item input.id to input.name
            respond with { item: item }
          spec "a known id updates and responds with the new record"
            given input { id: "a", name: "Gadget" }
            when handled
            then responds { item: { id: "a", name: "Gadget" } }
          spec "an unknown id is NotFound"
            given input { id: "missing", name: "Gadget" }
            when handled
            then fails with NotFound
          spec "an empty name is EmptyName"
            given input { id: "a", name: "" }
            when handled
            then fails with EmptyName

        endpoint GetItem : GET /items/{id}
          input
            id: Id
          output
            item: Item
          steps
            let item = find item input.id
            respond with { item: item }

        endpoint DeleteItem : DELETE /items/{id}
          input
            id: Id
          steps
            remove item input.id
          spec "a known id removes the item"
            given input { id: "a" }
            when handled
            then succeeds
          spec "an unknown id is NotFound"
            given input { id: "missing" }
            when handled
            then fails with NotFound
        """;

    [Fact]
    [Trait("Slice", "133")]
    public async Task Put_and_delete_specs_update_and_remove_a_stored_item()
    {
        // Given PUT /items/{id} and DELETE /items/{id} over the generated ItemStore,
        // including a known id, an unknown id, and an empty name
        var output = PhraseCompiler.Compile(Source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // When Dana runs the generated specs
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then a known id updates with status 200 and the new record, an unknown id is NotFound
        // as HTTP 422, and an empty name is EmptyName as HTTP 422
        Assert.True(result.Built, result.Log);
        Assert.Equal(5, result.Total);
        Assert.Equal(0, result.Failed);
        var updateSpecs = File.ReadAllText(workspace.FindSingle("UpdateItemSpecs.cs.cs"));
        Assert.Contains("Assert.Equal(200, endpoint.HttpContext.Response.StatusCode)", updateSpecs, StringComparison.Ordinal);
        Assert.Contains("NotFound", updateSpecs, StringComparison.Ordinal);
        Assert.Contains("EmptyName", updateSpecs, StringComparison.Ordinal);
        Assert.Contains("Assert.Equal(422, endpoint.HttpContext.Response.StatusCode)", updateSpecs, StringComparison.Ordinal);

        // And DELETE of a known id succeeds (then succeeds, HTTP 200, no response record)
        // and an unknown id fails with NotFound as HTTP 422
        var deleteSpecs = File.ReadAllText(workspace.FindSingle("DeleteItemSpecs.cs.cs"));
        Assert.Contains("// Then succeeds", deleteSpecs, StringComparison.Ordinal);
        Assert.Contains("Assert.Equal(200, endpoint.HttpContext.Response.StatusCode)", deleteSpecs, StringComparison.Ordinal);
        Assert.Contains("NotFound", deleteSpecs, StringComparison.Ordinal);
        Assert.Contains("Assert.Equal(422, endpoint.HttpContext.Response.StatusCode)", deleteSpecs, StringComparison.Ordinal);
        var deleteEndpoint = File.ReadAllText(workspace.FindSingle("DeleteItem.cs.cs"));
        Assert.Contains("Delete(\"/items/{id}\")", deleteEndpoint, StringComparison.Ordinal);
        Assert.Contains("Send.OkAsync(ct)", deleteEndpoint, StringComparison.Ordinal);
        var updateEndpoint = File.ReadAllText(workspace.FindSingle("UpdateItem.cs.cs"));
        Assert.Contains("Put(\"/items/{id}\")", updateEndpoint, StringComparison.Ordinal);

        // And the prototype store can replace by id and remove, and its methods are locked
        var phrases = File.ReadAllText(workspace.FindSingle("Phrases.cs"));
        Assert.Contains("bool Remove(string id)", phrases, StringComparison.Ordinal);
        Assert.Contains("items[index] = item;", phrases, StringComparison.Ordinal);
        Assert.Contains("lock (gate)", phrases, StringComparison.Ordinal);
        Assert.Equal(4, CountOf(phrases, "lock (gate)"));
    }

    [Fact]
    [Trait("Slice", "133")]
    public async Task Live_http_put_then_get_then_delete_then_get()
    {
        // Given the generated app and one stored item (the store starts empty, so POST makes the id known)
        var output = PhraseCompiler.Compile(Source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var api = await GeneratedApi.StartAsync(workspace.FindSingle("*.Api.csproj"));
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl), Timeout = TimeSpan.FromSeconds(30) };
            var created = await SendAsync(http, HttpMethod.Post, "/items", """{"id":"a","name":"Widget"}""");
            var createdBody = await created.Content.ReadAsStringAsync();
            Assert.True(created.StatusCode == HttpStatusCode.OK, Detail(api, created.StatusCode, createdBody));

            // When Dana updates that item, reads it back, removes it, and reads it again
            var updated = await SendAsync(http, HttpMethod.Put, "/items/a", """{"id":"a","name":"Gadget"}""");
            var updatedBody = await updated.Content.ReadAsStringAsync();
            var afterPut = await http.GetAsync("/items/a");
            var afterPutBody = await afterPut.Content.ReadAsStringAsync();
            var deleted = await http.DeleteAsync("/items/a");
            var deletedBody = await deleted.Content.ReadAsStringAsync();
            var afterDelete = await http.GetAsync("/items/a");
            var afterDeleteBody = await afterDelete.Content.ReadAsStringAsync();

            // Then PUT responds with the new record, GET shows that record, DELETE succeeds with no body,
            // and the later GET fails with NotFound as HTTP 422
            Assert.True(updated.StatusCode == HttpStatusCode.OK, Detail(api, updated.StatusCode, updatedBody));
            Assert.Contains("Gadget", updatedBody, StringComparison.Ordinal);
            Assert.True(afterPut.StatusCode == HttpStatusCode.OK, Detail(api, afterPut.StatusCode, afterPutBody));
            Assert.Contains("Gadget", afterPutBody, StringComparison.Ordinal);
            Assert.DoesNotContain("Widget", afterPutBody, StringComparison.Ordinal);
            Assert.True(deleted.StatusCode == HttpStatusCode.OK, Detail(api, deleted.StatusCode, deletedBody));
            Assert.True(afterDelete.StatusCode == HttpStatusCode.UnprocessableEntity, Detail(api, afterDelete.StatusCode, afterDeleteBody));
            Assert.Contains("NotFound", afterDeleteBody, StringComparison.Ordinal);
        }
        finally
        {
            await api.DisposeAsync();
        }
    }

    [Fact]
    [Trait("Slice", "133")]
    public async Task Record_without_an_id_or_an_unmatched_store_name_gets_no_store()
    {
        // Given a record with no id field, and an effect named WidgetStore where no Widget record exists
        const string source = """
            module Notes

            type Note
              text: Text

            avow to "keep note {note: Note}"
              effects NoteStore
              spec "a note with no id is kept without a store"
                given note = { text: "hello" }
                when called
                then succeeds
              native csharp
                _ = notestore;
                if (note.Text != "hello")
                    throw new InvalidOperationException(note.Text);

            avow to "keep name {name: Text}"
              effects WidgetStore
              spec "a store name with no matching record is not a store"
                given name = "widget"
                when called
                then succeeds
              native csharp
                _ = widgetstore;
                if (name.Length == 0)
                    throw new InvalidOperationException("empty");
            """;

        // When Dana compiles it
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);

        // Then neither effect becomes a prototype store: each stays an empty effect interface
        foreach (var file in Directory.GetFiles(workspace.Root, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("InMemory", text, StringComparison.Ordinal);
            Assert.DoesNotContain("AddSingleton", text, StringComparison.Ordinal);
            Assert.DoesNotContain("void Save", text, StringComparison.Ordinal);
            Assert.DoesNotContain("bool Remove", text, StringComparison.Ordinal);
        }

        var phrases = File.ReadAllText(workspace.FindSingle("Phrases.cs"));
        Assert.Contains("public interface INoteStore { }", phrases, StringComparison.Ordinal);
        Assert.Contains("public interface IWidgetStore { }", phrases, StringComparison.Ordinal);

        // And both specs still pass
        var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));
        Assert.True(result.Built, result.Log);
        Assert.Equal(2, result.Total);
        Assert.Equal(0, result.Failed);
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpClient http, HttpMethod method, string path, string json)
    {
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(method, path) { Content = body };
        return await http.SendAsync(request);
    }

    private static string Detail(GeneratedApi api, HttpStatusCode status, string body) =>
        $"PID {api.Pid} on {api.BaseUrl} returned {(int)status}. Body: {body}{Environment.NewLine}{api.Log}";

    private static int CountOf(string text, string value)
    {
        var count = 0;
        var start = 0;
        while ((start = text.IndexOf(value, start, StringComparison.Ordinal)) >= 0)
        {
            count++;
            start += value.Length;
        }

        return count;
    }
}
