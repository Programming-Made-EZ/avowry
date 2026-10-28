using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler.Semantics;

namespace Phrase.Acceptance.Tests;

// Feature: Real HTTP behaves as the specs say
//   As Morgan, I want the generated Tasks app started and called over real HTTP
//   so that I can trust the running service matches its specs.
//
// Each endpoint is one spec group and runs in its own process. The store starts
// empty for that group, so one endpoint's saves cannot appear in another's list or lookup.
// The concurrency check uses a further fresh process.
public class TasksOverHttp
{
    private const int ParallelCreates = 20;
    private static readonly TimeSpan SpecTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ParallelTimeout = TimeSpan.FromSeconds(15);
    private static readonly Regex RouteParameter = new(@"\{([^}]+)\}", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    [Fact]
    [Trait("Slice", "136")]
    public async Task Every_spec_matches_over_real_http()
    {
        // Given the Tasks app, built with phrase build, and the specs of its compiled model
        var sourcePath = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        var source = await File.ReadAllTextAsync(sourcePath);
        var resolved = Resolver.Resolve(source);
        Assert.Empty(resolved.Diagnostics);
        var module = resolved.Module;
        Assert.Equal(11, module.Endpoints.Sum(endpoint => endpoint.Specs.Count));
        using var workspace = TempWorkspace.Create();
        var cli = await PhraseCli.RunAsync("build", sourcePath, "-o", workspace.Root);
        Assert.True(cli.ExitCode == 0, cli.Text);
        await AssertEverySpecMatchesAsync(workspace.FindSingle("*.Api.csproj"), module);
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Every_declared_failure_names_its_Phrase_source_in_the_response_header()
    {
        // Given the Tasks demo with four distinct declared failures
        var sourcePath = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        var source = await File.ReadAllTextAsync(sourcePath);
        var module = Resolver.Resolve(source).Module;
        using var workspace = TempWorkspace.Create();
        var cli = await PhraseCli.RunAsync("build", sourcePath, "-o", workspace.Root);
        Assert.True(cli.ExitCode == 0, cli.Text);

        // When every Phrase failure spec is exercised over a live generated API
        var observed = await ObserveFailureSourcesAsync(workspace.FindSingle("*.Api.csproj"), module);

        // Then EmptyTitle, TitleTooLong, NotFound and StoreUnavailable identify file, endpoint and failure
        Assert.Equal(new[] { "EmptyTitle", "NotFound", "StoreUnavailable", "TitleTooLong" }, observed.Keys.Order(StringComparer.Ordinal));
        Assert.All(observed, item => Assert.Matches($"^Tasks\\.phrase:[0-9]+ endpoint [A-Za-z]+ failure {item.Key}$", item.Value));
    }

    private static async Task<Dictionary<string, string>> ObserveFailureSourcesAsync(string project, ModuleModel module)
    {
        var observed = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var endpoint in module.Endpoints.Where(endpoint => endpoint.Specs.Any(spec => spec.Outcome is FailsModel)))
        {
            await using var api = await GeneratedApi.StartAsync(project);
            using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl), Timeout = SpecTimeout };
            foreach (var spec in endpoint.Specs.Where(spec => spec.Outcome is FailsModel))
            {
                var call = RequestFor(endpoint, spec.Input);
                var response = await SendAsync(http, MethodOf(endpoint.Verb), call.Path, call.Json);
                var failure = (FailsModel)spec.Outcome;
                Assert.True(response.Status == HttpStatusCode.UnprocessableEntity, Detail(api, endpoint.Name, response));
                Assert.True(response.Headers.TryGetValue("X-Phrase-Source", out var source), Detail(api, endpoint.Name, response));
                observed[failure.Code] = source;
            }
        }
        return observed;
    }

    internal static async Task AssertEverySpecMatchesAsync(string project, ModuleModel module)
    {
        var mismatches = new List<string>();

        // When each spec is sent over HTTP on a fresh process for its endpoint
        foreach (var endpoint in module.Endpoints)
        {
            var api = await GeneratedApi.StartAsync(project);
            var pid = api.Pid;
            try
            {
                using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl), Timeout = SpecTimeout };
                foreach (var spec in endpoint.Specs)
                {
                    try
                    {
                        var mismatch = await ObserveAsync(http, api, module, endpoint, spec);
                        if (mismatch is not null)
                            mismatches.Add(mismatch);
                    }
                    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                    {
                        mismatches.Add($"{endpoint.Name} '{spec.Description}' was not answered.{Environment.NewLine}{ex.Message}{Environment.NewLine}{api.Log}");
                        break;
                    }
                }
            }
            finally
            {
                await api.DisposeAsync();
            }

            // Then that recorded process is gone before the next spec group starts
            AssertProcessGone(pid);
        }

        // Then all eleven live statuses and bodies equal the spec outcomes
        Assert.True(mismatches.Count == 0, string.Join(Environment.NewLine + Environment.NewLine, mismatches));
    }

    [Fact]
    [Trait("Slice", "136")]
    public async Task Twenty_parallel_creates_are_all_listed()
    {
        // Given a fresh Tasks API whose store starts empty
        var sourcePath = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        var source = await File.ReadAllTextAsync(sourcePath);
        var module = Resolver.Resolve(source).Module;
        var create = module.Endpoints.Single(endpoint => endpoint.Verb == "POST");
        var list = module.Endpoints.Single(endpoint => endpoint.Verb == "GET" && !endpoint.Route.Contains('{', StringComparison.Ordinal));
        var available = list.Input.Single(field => field.Type == PType.Bool);
        using var workspace = TempWorkspace.Create();
        var cli = await PhraseCli.RunAsync("build", sourcePath, "-o", workspace.Root);
        Assert.True(cli.ExitCode == 0, cli.Text);
        var api = await GeneratedApi.StartAsync(workspace.FindSingle("*.Api.csproj"));
        var pid = api.Pid;
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl), Timeout = ParallelTimeout };
            var ids = Enumerable.Range(1, ParallelCreates).Select(i => $"p-{i:00}").ToArray();
            var idName = create.Input.Single(field => field.Name == "id").Name;
            var titleName = create.Input.Single(field => field.Type == PType.Text).Name;

            // When twenty creates with distinct ids run together, under a short timeout
            var created = await Task.WhenAll(ids.Select(id => SendAsync(http, HttpMethod.Post, create.Route, JsonFields(new (string, string)[]
            {
                (idName, id),
                (titleName, "Parallel " + id),
            })))).WaitAsync(ParallelTimeout);
            var listed = await SendAsync(http, HttpMethod.Get, list.Route + "?" + Uri.EscapeDataString(available.Name) + "=true", null);

            // Then every create was accepted and the list returns those twenty tasks
            Assert.All(created, response => Assert.True(response.Status == HttpStatusCode.OK, Detail(api, "parallel create", response)));
            Assert.True(listed.Status == HttpStatusCode.OK, Detail(api, "parallel list", listed));
            var found = IdsOf(listed.Body, list.Output.Single(field => field.Type.IsList).Name);
            Assert.True(found.Count == ParallelCreates, Detail(api, $"list count {found.Count}", listed));
            Assert.Equal(ids.OrderBy(id => id, StringComparer.Ordinal), found.OrderBy(id => id, StringComparer.Ordinal));
        }
        finally
        {
            await api.DisposeAsync();
        }

        // And the recorded process is gone
        AssertProcessGone(pid);
    }

    private static async Task<string?> ObserveAsync(HttpClient http, GeneratedApi api, ModuleModel module, EndpointModel endpoint, EndpointSpecModel spec)
    {
        var prepared = await PrepareAsync(http, api, module, endpoint, spec);
        if (prepared is not null)
            return prepared;

        var call = RequestFor(endpoint, spec.Input);
        var response = await SendAsync(http, MethodOf(endpoint.Verb), call.Path, call.Json);
        var label = $"{endpoint.Name} '{spec.Description}'";
        switch (spec.Outcome)
        {
            case RespondsModel responds:
                if (response.Status != HttpStatusCode.OK)
                    return Detail(api, label, response);
                var differences = JsonDifferences(Json(responds.Value), response.Body);
                return differences.Count == 0 ? null : Detail(api, label, response) + Environment.NewLine + string.Join(Environment.NewLine, differences);
            case FailsModel fails:
                if (response.Status != HttpStatusCode.UnprocessableEntity || !response.Body.Contains(fails.Code, StringComparison.Ordinal))
                    return Detail(api, $"{label} fails with {fails.Code}", response);
                return null;
            case SucceedsModel:
                if (response.Status != HttpStatusCode.OK || response.Body.Length != 0)
                    return Detail(api, $"{label} succeeds", response) + Environment.NewLine + "Expected HTTP 200 and an empty body.";
                return await GoneAsync(http, api, module, spec, label);
            default:
                return $"{label} has an outcome the live check does not cover.";
        }
    }

    // A responds spec names the stored records it assumes. A succeeds spec names a known id.
    // Create rejects a blank title, so that known id is posted with the title "known".
    // A fails spec is left unprepared, so an unknown id stays absent on this fresh process.
    private static async Task<string?> PrepareAsync(HttpClient http, GeneratedApi api, ModuleModel module, EndpointModel endpoint, EndpointSpecModel spec)
    {
        var create = module.Endpoints.Single(candidate => candidate.Verb == "POST");
        if (spec.Outcome is RespondsModel responds)
        {
            foreach (var stored in RecordsOf(responds.Value, StoreEntityNames(module)))
            {
                var fields = create.Input.Select(input =>
                {
                    var supplied = stored.Fields.FirstOrDefault(field => field.Name == input.Name);
                    if (supplied is null)
                        throw new InvalidOperationException($"Stored {stored.Type.Name} has no '{input.Name}' for {create.Verb} {create.Route}.");
                    return supplied;
                }).ToList();
                var response = await SendAsync(http, HttpMethod.Post, create.Route, JsonObject(fields));
                if (response.Status != HttpStatusCode.OK)
                    return Detail(api, $"{endpoint.Name} '{spec.Description}' prepare", response);
            }
        }
        else if (spec.Outcome is SucceedsModel)
        {
            var id = spec.Input.Fields.FirstOrDefault(field => field.Name == "id");
            if (id is null)
                return $"{endpoint.Name} '{spec.Description}' succeeds without an id to store.";
            var fields = create.Input.Select(field => field.Name == "id"
                ? id
                : new BoundRecordField(field.Name, KnownValue(field))).ToList();
            var response = await SendAsync(http, HttpMethod.Post, create.Route, JsonObject(fields));
            if (response.Status != HttpStatusCode.OK)
                return Detail(api, $"{endpoint.Name} '{spec.Description}' prepare", response);
        }

        return null;
    }

    private static async Task<string?> GoneAsync(HttpClient http, GeneratedApi api, ModuleModel module, EndpointSpecModel spec, string label)
    {
        var id = spec.Input.Fields.FirstOrDefault(field => field.Name == "id");
        if (id is null)
            return $"{label} succeeds without an id to read back.";
        var lookup = module.Endpoints.Single(endpoint =>
            endpoint.Verb == "GET" && endpoint.Route.Contains("{id}", StringComparison.Ordinal));
        var path = lookup.Route.Replace("{id}", Uri.EscapeDataString(LiteralText(id.Value)), StringComparison.Ordinal);
        var response = await SendAsync(http, HttpMethod.Get, path, null);
        if (response.Status != HttpStatusCode.UnprocessableEntity || !response.Body.Contains("NotFound", StringComparison.Ordinal))
            return Detail(api, $"{label} followed by GET {path}", response);
        return null;
    }

    private static LiveCall RequestFor(EndpointModel endpoint, BoundRecord input)
    {
        var routed = RouteParameter.Matches(endpoint.Route).Select(match => match.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        var path = endpoint.Route;
        var query = new List<string>();
        var body = new List<BoundRecordField>();
        foreach (var field in input.Fields)
        {
            if (routed.Contains(field.Name))
            {
                path = path.Replace("{" + field.Name + "}", Uri.EscapeDataString(LiteralText(field.Value)), StringComparison.Ordinal);
                continue;
            }

            switch (endpoint.Verb)
            {
                case "GET":
                    query.Add(Uri.EscapeDataString(field.Name) + "=" + Uri.EscapeDataString(LiteralText(field.Value)));
                    break;
                case "POST":
                case "PUT":
                    body.Add(field);
                    break;
            }
        }

        if (query.Count > 0)
            path += "?" + string.Join("&", query);
        return new LiveCall(path, body.Count == 0 ? null : JsonObject(body));
    }

    private static IEnumerable<BoundRecord> RecordsOf(BoundExpr value, IReadOnlySet<string> entityNames)
    {
        switch (value)
        {
            case BoundRecord record:
                if (entityNames.Contains(record.Type.Name))
                    yield return record;
                foreach (var nested in record.Fields.SelectMany(field => RecordsOf(field.Value, entityNames)))
                    yield return nested;
                break;
            case BoundList list:
                foreach (var nested in list.Items.SelectMany(item => RecordsOf(item, entityNames)))
                    yield return nested;
                break;
        }
    }

    private static HashSet<string> StoreEntityNames(ModuleModel module)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var effect in module.Phrases.SelectMany(phrase => phrase.Effects).Distinct(StringComparer.Ordinal))
        {
            const string suffix = "Store";
            if (!effect.EndsWith(suffix, StringComparison.Ordinal) || effect.Length == suffix.Length)
                continue;
            var entityName = effect[..^suffix.Length];
            if (module.Types.Any(type => type.Name == entityName && type.Fields.Any(field => field.Name == "id")))
                names.Add(entityName);
        }

        return names;
    }

    private static BoundExpr KnownValue(FieldModel field) =>
        field.Type == PType.Text || field.Type == PType.Id
            ? new BoundText("known", field.Type)
            : throw new InvalidOperationException($"A known id needs a '{field.Name}' value of type {field.Type} before it can be posted.");

    private static List<string> JsonDifferences(string expectedJson, string actualJson)
    {
        var differences = new List<string>();
        try
        {
            using var expected = JsonDocument.Parse(expectedJson);
            using var actual = JsonDocument.Parse(actualJson);
            CompareJson(expected.RootElement, actual.RootElement, "$", differences);
        }
        catch (JsonException ex)
        {
            differences.Add($"Body is not the expected JSON object: {ex.Message}");
        }

        return differences;
    }

    private static void CompareJson(JsonElement expected, JsonElement actual, string path, List<string> differences)
    {
        if (expected.ValueKind == JsonValueKind.Object)
        {
            if (actual.ValueKind != JsonValueKind.Object)
            {
                differences.Add($"{path}: expected an object, got {actual.GetRawText()}");
                return;
            }

            var actualProps = actual.EnumerateObject().ToDictionary(property => property.Name, property => property.Value, StringComparer.OrdinalIgnoreCase);
            var expectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in expected.EnumerateObject())
            {
                expectedNames.Add(property.Name);
                if (!actualProps.TryGetValue(property.Name, out var actualValue))
                {
                    differences.Add($"{path}.{property.Name}: missing");
                    continue;
                }

                CompareJson(property.Value, actualValue, path + "." + property.Name, differences);
            }

            foreach (var extra in actualProps.Keys.Where(name => !expectedNames.Contains(name)))
                differences.Add($"{path}.{extra}: unexpected");
            return;
        }

        if (expected.ValueKind == JsonValueKind.Array)
        {
            if (actual.ValueKind != JsonValueKind.Array)
            {
                differences.Add($"{path}: expected an array, got {actual.GetRawText()}");
                return;
            }

            var expectedItems = expected.EnumerateArray().ToList();
            var actualItems = actual.EnumerateArray().ToList();
            if (expectedItems.Count != actualItems.Count)
                differences.Add($"{path}: expected {expectedItems.Count} items, got {actualItems.Count}");
            for (var i = 0; i < Math.Min(expectedItems.Count, actualItems.Count); i++)
                CompareJson(expectedItems[i], actualItems[i], $"{path}[{i}]", differences);
            return;
        }

        if (expected.ValueKind != actual.ValueKind || expected.GetRawText() != actual.GetRawText())
            differences.Add($"{path}: expected {expected.GetRawText()}, got {actual.GetRawText()}");
    }

    private static List<string> IdsOf(string json, string listName)
    {
        using var document = JsonDocument.Parse(json);
        var list = document.RootElement.EnumerateObject()
            .First(property => property.Name.Equals(listName, StringComparison.OrdinalIgnoreCase)).Value;
        return list.EnumerateArray().Select(item => item.EnumerateObject()
            .First(property => property.Name.Equals("id", StringComparison.OrdinalIgnoreCase)).Value.GetString()!).ToList();
    }

    private static async Task<LiveResponse> SendAsync(HttpClient http, HttpMethod method, string path, string? json)
    {
        using var request = new HttpRequestMessage(method, path);
        if (json is not null)
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request);
        var headers = response.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value), StringComparer.OrdinalIgnoreCase);
        return new LiveResponse(response.StatusCode, await response.Content.ReadAsStringAsync(), headers);
    }

    private static string Json(BoundExpr value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
            WriteJson(writer, value);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string JsonObject(IReadOnlyList<BoundRecordField> fields)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var field in fields)
            {
                writer.WritePropertyName(field.Name);
                WriteJson(writer, field.Value);
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string JsonFields(IReadOnlyList<(string Name, string Value)> fields)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var (name, value) in fields)
                writer.WriteString(name, value);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteJson(Utf8JsonWriter writer, BoundExpr value)
    {
        switch (value)
        {
            case BoundText text:
                writer.WriteStringValue(text.Value);
                break;
            case BoundNumber number:
                writer.WriteRawValue(number.Text);
                break;
            case BoundBool boolean:
                writer.WriteBooleanValue(boolean.Value);
                break;
            case BoundNone:
                writer.WriteNullValue();
                break;
            case BoundList list:
                writer.WriteStartArray();
                foreach (var item in list.Items)
                    WriteJson(writer, item);
                writer.WriteEndArray();
                break;
            case BoundRecord record:
                writer.WriteStartObject();
                foreach (var field in record.Fields)
                {
                    writer.WritePropertyName(field.Name);
                    WriteJson(writer, field.Value);
                }

                writer.WriteEndObject();
                break;
            default:
                throw new InvalidOperationException($"Spec value '{value.GetType().Name}' is not a literal.");
        }
    }

    private static string LiteralText(BoundExpr value) => value switch
    {
        BoundText text => text.Value,
        BoundBool boolean => boolean.Value ? "true" : "false",
        BoundNumber number => number.Text,
        _ => throw new InvalidOperationException($"HTTP request field is not a scalar literal ({value.GetType().Name})."),
    };

    private static HttpMethod MethodOf(string verb) => verb switch
    {
        "GET" => HttpMethod.Get,
        "POST" => HttpMethod.Post,
        "PUT" => HttpMethod.Put,
        "DELETE" => HttpMethod.Delete,
        _ => new HttpMethod(verb),
    };

    private static void AssertProcessGone(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            Assert.True(process.HasExited, $"Recorded API process {pid} is still running.");
        }
        catch (ArgumentException)
        {
            // The recorded id is not a running process.
        }
    }

    private static string Detail(GeneratedApi api, string spec, LiveResponse response) =>
        $"{spec}: PID {api.Pid} on {api.BaseUrl} returned {(int)response.Status}. Body: {response.Body}{Environment.NewLine}{api.Log}";

    private readonly record struct LiveCall(string Path, string? Json);
    private readonly record struct LiveResponse(HttpStatusCode Status, string Body, IReadOnlyDictionary<string, string> Headers);
}
