using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Avowry.Acceptance.Tests.Support;

// Card 165 red support: the failing requests of the Tasks demo, the source-map reader, the instrumented copy of the
// generated Tasks API (a test observer and two hooks added to the built workspace, never to product code) and the probe
// that runs that API once per observer mode.

/// <summary>One failing request of the Tasks demo, the failure it raises and the avow whose `fails with` line declares it.</summary>
public sealed record FailingCall165(string Code, string Endpoint, string AvowPrefix, HttpMethod Method, string Path, string? Json);

public sealed record MapRecord165(string StepId, string Kind, string Name, string SourceFile, int Line, string Endpoint);

public sealed record ObservedResponse165(FailingCall165 Call, HttpStatusCode Status, string Body, TimeSpan Elapsed);

public sealed record EventRecord165(string Raw, string[] Names, int SchemaVersion, string Code, string Endpoint, string StepId, string BuildId)
{
    public static EventRecord165 Parse(string raw)
    {
        using var document = JsonDocument.Parse(raw);
        var root = document.RootElement;
        var names = root.EnumerateObject().Select(property => property.Name).ToArray();
        var version = root.TryGetProperty("SchemaVersion", out var number) && number.ValueKind == JsonValueKind.Number ? number.GetInt32() : -1;
        return new EventRecord165(
            raw, names, version, TextOf(root, "Code"), TextOf(root, "Endpoint"), TextOf(root, "StepId"), TextOf(root, "BuildId"));
    }

    private static string TextOf(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";
}

public sealed record ObserverProbe165(
    string Mode, ObservedResponse165[] Responses, EventRecord165[] Events, string Log, bool Alive, HttpStatusCode AfterStatus,
    string CallsLog);

public static class FailureObserver165
{
    public const string ModeVariable = "AVOWRY_TEST_OBSERVER";
    public const string EventsVariable = "AVOWRY_TEST_EVENTS";

    // Every request below carries this marker, so a leak of request data into an event, a log line or a response is visible.
    public const string RequestMarker = "req-secret";

    public static readonly string[] EventFields = ["SchemaVersion", "Code", "Endpoint", "StepId", "BuildId"];

    // The six declared failures the Tasks demo can raise: four codes, NotFound from three endpoints.
    public static readonly FailingCall165[] FailingCalls =
    [
        new("EmptyTitle", "CreateTask", "validate task title", HttpMethod.Post, "/tasks", "{\"id\":\"req-secret-1\",\"title\":\"\"}"),
        new("TitleTooLong", "CreateTask", "validate task title", HttpMethod.Post, "/tasks",
            "{\"id\":\"req-secret-2\",\"title\":\"req-secret-title-" + new string('x', 210) + "\"}"),
        new("NotFound", "GetTask", "find task", HttpMethod.Get, "/tasks/req-secret-3", null),
        new("StoreUnavailable", "ListTasks", "list tasks", HttpMethod.Get, "/tasks?available=false", null),
        new("NotFound", "CompleteTask", "find task", HttpMethod.Put, "/tasks/req-secret-5/complete", null),
        new("NotFound", "DeleteTask", "delete task", HttpMethod.Delete, "/tasks/req-secret-6", null),
    ];

    // Added to the built Tasks.Api by the fixture. The interface, event and no-op are the generated runtime's own; nothing else is assumed.
    // The module namespace (Tasks) is where the generated runtime types are expected to live.
    public const string HookSource = """
        using System.Text.Json;
        using Microsoft.Extensions.DependencyInjection;

        namespace Tasks;

        public sealed class TestFailureObserver : IAvowryFailureObserver
        {
            private static readonly object Gate = new();

            public void OnFailure(AvowryFailureEvent failure)
            {
                var mode = Environment.GetEnvironmentVariable("AVOWRY_TEST_OBSERVER");
                if (mode == "slow")
                    Thread.Sleep(5000);
                var json = JsonSerializer.Serialize<object>(failure);
                if (mode == "log")
                    Console.WriteLine($"avowry-failure v{failure.SchemaVersion} code={failure.Code} endpoint={failure.Endpoint} stepId={failure.StepId} buildId={failure.BuildId}");
                lock (Gate)
                    File.AppendAllText(Environment.GetEnvironmentVariable("AVOWRY_TEST_EVENTS")!, json + "\n");
                if (mode == "throw")
                    throw new InvalidOperationException("observer-secret-boom");
            }
        }

        public static class AvowryTestHooks
        {
            public static void Register(IServiceCollection services)
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AVOWRY_TEST_OBSERVER")))
                    services.AddSingleton<IAvowryFailureObserver, TestFailureObserver>();
            }

            public static void Report(IServiceProvider services) =>
                Console.WriteLine("avowry-test observer-type=" + services.GetRequiredService<IAvowryFailureObserver>().GetType().Name);
        }
        """;

    /// <summary>The source line (1-based) of the `fails with` declaration the call's endpoint reaches, read from the avow source itself.</summary>
    public static int ExpectedLine(string[] sourceLines, FailingCall165 call)
    {
        var header = Array.FindIndex(sourceLines, line => line.StartsWith("avow to \"" + call.AvowPrefix, StringComparison.Ordinal));
        if (header < 0)
            throw new InvalidOperationException($"The Tasks source has no avow starting '{call.AvowPrefix}'.");
        for (var index = header + 1; index < sourceLines.Length; index++)
        {
            var text = sourceLines[index].Trim();
            if (text.StartsWith("avow to ", StringComparison.Ordinal) || text.StartsWith("endpoint ", StringComparison.Ordinal))
                break;
            if (text.StartsWith("fails with ", StringComparison.Ordinal)
                && text["fails with ".Length..].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Contains(call.Code, StringComparer.Ordinal))
                return index + 1;
        }

        throw new InvalidOperationException($"Avow '{call.AvowPrefix}' declares no failure {call.Code}.");
    }

    public static (string BuildId, MapRecord165[] Records) ReadMap(string mapPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(mapPath));
        var root = document.RootElement;
        var records = root.GetProperty("records").EnumerateArray()
            .Select(record => new MapRecord165(
                record.GetProperty("stepId").GetString()!,
                record.GetProperty("kind").GetString()!,
                record.GetProperty("name").GetString()!,
                record.GetProperty("sourceFile").GetString()!,
                record.GetProperty("line").GetInt32(),
                record.GetProperty("endpoint").GetString()!))
            .ToArray();
        return (root.GetProperty("buildId").GetString()!, records);
    }

    /// <summary>Every generated .cs file under the root (bin and obj left out), keyed by LF-normalised relative path.</summary>
    public static Dictionary<string, string> GeneratedCs(string root) =>
        Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Select(path => (path, rel: Path.GetRelativePath(root, path).Replace('\\', '/')))
            .Where(file => !file.rel.Split('/').Any(part => part is "bin" or "obj"))
            .OrderBy(file => file.rel, StringComparer.Ordinal)
            .ToDictionary(file => file.rel, file => Normalize(File.ReadAllText(file.path)), StringComparer.Ordinal);

    public static string Normalize(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);

    /// <summary>The body's errors.generalErrors, where FastEndpoints puts the declared failure name.</summary>
    public static string[] GeneralErrors(string body)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("errors").GetProperty("generalErrors").EnumerateArray()
            .Select(item => item.GetString() ?? "").ToArray();
    }

    public static int Occurrences(string text, string value) =>
        text.Split(value, StringSplitOptions.None).Length - 1;
}

/// <summary>The Tasks demo built once by the CLI into a temporary folder: generated C#, source map and source lines. Nothing is injected.</summary>
public sealed class PristineTasks165 : IAsyncLifetime
{
    private TempWorkspace? workspace;

    public string Root => workspace!.Root;
    public string SourcePath { get; } = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
    public string[] SourceLines { get; private set; } = Array.Empty<string>();
    public string Map => Path.Combine(Root, "Tasks.avowrymap.json");
    public string BuildId { get; private set; } = "";
    public MapRecord165[] Records { get; private set; } = Array.Empty<MapRecord165>();
    public Dictionary<string, string> Generated { get; private set; } = new();

    public async Task InitializeAsync()
    {
        workspace = TempWorkspace.Create();
        SourceLines = await File.ReadAllLinesAsync(SourcePath);
        var build = await PhraseCli.RunAsync("build", SourcePath, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);
        (BuildId, Records) = FailureObserver165.ReadMap(Map);
        Generated = FailureObserver165.GeneratedCs(Root);
    }

    public Task DisposeAsync()
    {
        workspace?.Dispose();
        return Task.CompletedTask;
    }
}

/// <summary>
/// The Tasks demo built by the CLI, then given a test observer: a file added to Tasks.Api and two calls added to its generated Program.cs
/// (register the test observer first when AVOWRY_TEST_OBSERVER is set; print which observer the host resolved). The API is built once;
/// ProbeAsync then starts it once per mode and sends the six failing requests.
/// </summary>
public sealed class InstrumentedTasks165 : IAsyncLifetime
{
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan EventDeadline = TimeSpan.FromSeconds(40);
    private readonly ConcurrentDictionary<string, Lazy<Task<ObserverProbe165>>> probes = new();
    private TempWorkspace? workspace;
    private string project = "";
    private string dll = "";

    public string Root => workspace!.Root;
    public string SourcePath { get; } = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
    public string[] SourceLines { get; private set; } = Array.Empty<string>();
    public string Map => Path.Combine(Root, "Tasks.avowrymap.json");
    public string Assembly => dll;
    public string BuildId { get; private set; } = "";
    public MapRecord165[] Records { get; private set; } = Array.Empty<MapRecord165>();

    public async Task InitializeAsync()
    {
        workspace = TempWorkspace.Create();
        SourceLines = await File.ReadAllLinesAsync(SourcePath);
        var build = await PhraseCli.RunAsync("build", SourcePath, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);
        (BuildId, Records) = FailureObserver165.ReadMap(Map);
        project = workspace.FindSingle("*.Api.csproj");
        var directory = Path.GetDirectoryName(project)!;
        Inject(directory);
        dll = Path.Combine(directory, "bin", "Debug", "net8.0", Path.GetFileNameWithoutExtension(project) + ".dll");

        // The first start builds the project (and fails with the build log when the generated runtime does not compile). Later starts reuse the build.
        var first = await GeneratedApi.StartAsync(project);
        await first.DisposeAsync();
    }

    public Task DisposeAsync()
    {
        workspace?.Dispose();
        return Task.CompletedTask;
    }

    public Task<ObserverProbe165> ProbeAsync(string mode) =>
        probes.GetOrAdd(mode, key => new Lazy<Task<ObserverProbe165>>(() => RunAsync(key))).Value;

    private static void Inject(string directory)
    {
        File.WriteAllText(Path.Combine(directory, "AvowryTestHooks.cs"), FailureObserver165.HookSource);
        var programPath = Path.Combine(directory, "Program.cs");
        var program = File.ReadAllText(programPath);
        var registered = Regex.Replace(
            program,
            @"var builder = WebApplication\.CreateBuilder\(args\);",
            "var builder = WebApplication.CreateBuilder(args);\nTasks.AvowryTestHooks.Register(builder.Services);");
        var reported = Regex.Replace(
            registered,
            @"var app = builder\.Build\(\);",
            "var app = builder.Build();\nTasks.AvowryTestHooks.Report(app.Services);");
        Assert.True(
            registered != program && reported != registered,
            "The generated Program.cs no longer has `var builder = WebApplication.CreateBuilder(args);` and `var app = builder.Build();` for the test hooks.");
        File.WriteAllText(programPath, reported);
    }

    private async Task<ObserverProbe165> RunAsync(string mode)
    {
        var eventsPath = Path.Combine(Root, "events-" + mode + ".jsonl");
        var options = new GeneratedApiStartOptions
        {
            PrebuiltDll = dll,
            BeforeChildStart = psi =>
            {
                if (mode != "none")
                {
                    psi.Environment[FailureObserver165.ModeVariable] = mode;
                    psi.Environment[FailureObserver165.EventsVariable] = eventsPath;
                }
            },
        };
        var api = await GeneratedApi.StartAsync(project, options);
        var responses = new List<ObservedResponse165>();
        EventRecord165[] events;
        bool alive;
        HttpStatusCode after;
        string callsLog;
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(api.BaseUrl), Timeout = HttpTimeout };

            // Two successful requests warm the process up. A success is not a failure, so neither may produce an event.
            var warm = await SendAsync(http, HttpMethod.Post, "/tasks", "{\"id\":\"warm-1\",\"title\":\"warm\"}");
            Assert.True(warm.Status == HttpStatusCode.OK, "warm-up create: " + warm.Status + " " + warm.Body + Environment.NewLine + api.Log);
            var listed = await SendAsync(http, HttpMethod.Get, "/tasks?available=true", null);
            Assert.True(listed.Status == HttpStatusCode.OK, "warm-up list: " + listed.Status + " " + listed.Body + Environment.NewLine + api.Log);

            foreach (var call in FailureObserver165.FailingCalls)
            {
                var clock = Stopwatch.StartNew();
                var response = await SendAsync(http, call.Method, call.Path, call.Json);
                clock.Stop();
                responses.Add(new ObservedResponse165(call, response.Status, response.Body, clock.Elapsed));
            }

            // The observer runs after the response, so the events are awaited; a slow observer needs the longest wait.
            var wanted = mode == "none" ? 0 : FailureObserver165.FailingCalls.Length;
            var wait = Stopwatch.StartNew();
            var lines = await ReadEventLinesAsync(eventsPath);
            while (lines.Length < wanted && wait.Elapsed < EventDeadline)
            {
                await Task.Delay(200);
                lines = await ReadEventLinesAsync(eventsPath);
            }

            events = lines.Select(EventRecord165.Parse).ToArray();

            // The log as it stands before the after-request below, so it holds output of the six failing calls only (card 165 fix round 1).
            // A short pause lets the stdout pump drain the lines the observer has already written.
            await Task.Delay(300);
            callsLog = api.Log;

            // The API must still answer after its observer threw or sat for five seconds.
            var again = await SendAsync(http, HttpMethod.Get, "/tasks/req-secret-3", null);
            after = again.Status;
            try
            {
                using var process = Process.GetProcessById(api.Pid);
                alive = !process.HasExited;
            }
            catch (ArgumentException)
            {
                alive = false;
            }
        }
        finally
        {
            await api.DisposeAsync();
        }

        return new ObserverProbe165(mode, responses.ToArray(), events, api.Log, alive, after, callsLog);
    }

    private static async Task<string[]> ReadEventLinesAsync(string path)
    {
        if (!File.Exists(path))
            return Array.Empty<string>();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = await reader.ReadToEndAsync();
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static async Task<(HttpStatusCode Status, string Body)> SendAsync(HttpClient http, HttpMethod method, string path, string? json)
    {
        using var request = new HttpRequestMessage(method, path);
        if (json is not null)
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request);
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
