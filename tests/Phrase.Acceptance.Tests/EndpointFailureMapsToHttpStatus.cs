using System.Net;
using System.Text;
using Phrase.Acceptance.Tests.Support;
using Avowry.Adapters.CSharp;
using Avowry.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: A declared failure maps to its HTTP status, not a 500
//   As Morgan, I want invalid endpoint input to return its declared failure
//   with the mapped HTTP status instead of a 500, so that I can verify the
//   intended failure behavior from the response.
public class EndpointFailureMapsToHttpStatus
{
    [Fact]
    [Trait("Slice", "049")]
    public async Task Invalid_input_returns_the_declared_failure_with_its_mapped_status()
    {
        // Given a generated API for orders.avowry, whose empty-items failure is EmptyOrder
        // and which GRAMMAR.md maps to HTTP 422 with the code as the error
        var source = Fixture.Read("orders.avowry");
        var output = AvowryCompiler.Compile(source, new CSharpProvider());
        using var workspace = TempWorkspace.Create();
        output.WriteTo(workspace.Root);
        var api = await GeneratedApi.StartAsync(workspace.FindSingle("*.Api.csproj"));
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl), Timeout = TimeSpan.FromSeconds(30) };

            // When an invalid request (an empty items list) is posted to the endpoint
            var invalid = await PostAsync(http, """{"customerId":"c-1","items":[]}""");
            var invalidBody = await invalid.Content.ReadAsStringAsync();

            // Then the response is the declared failure: HTTP 422, carrying the code EmptyOrder
            Assert.True(
                invalid.StatusCode == HttpStatusCode.UnprocessableEntity,
                Detail(api, invalid.StatusCode, invalidBody));
            Assert.Contains("EmptyOrder", invalidBody, StringComparison.Ordinal);

            // And a valid request returns 200, not the failure status
            var valid = await PostAsync(http, """{"customerId":"c-1","items":[{"sku":"A","quantity":2},{"sku":"B","quantity":3}]}""");
            var validBody = await valid.Content.ReadAsStringAsync();
            Assert.True(valid.StatusCode == HttpStatusCode.OK, Detail(api, valid.StatusCode, validBody));
            Assert.NotEqual(HttpStatusCode.UnprocessableEntity, valid.StatusCode);
        }
        finally
        {
            await api.DisposeAsync();
        }
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient http, string json)
    {
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        return await http.PostAsync("/orders", body);
    }

    private static string Detail(GeneratedApi api, HttpStatusCode status, string body) =>
        $"PID {api.Pid} on {api.BaseUrl} returned {(int)status}. Body: {body}{Environment.NewLine}{api.Log}";
}
