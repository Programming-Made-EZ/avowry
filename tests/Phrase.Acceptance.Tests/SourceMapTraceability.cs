using System.Text.Json;
using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler.Semantics;

namespace Phrase.Acceptance.Tests;

// Feature: generated failures point back to the Phrase source without leaking it at runtime
//   As Morgan, I want a separate deterministic source map and a trace command,
//   so that I can find the owning spec without reading or exposing generated implementation details.
public class SourceMapTraceability
{
    [Fact]
    [Trait("Slice", "161")]
    public async Task Tasks_build_writes_an_external_map_and_trace_resolves_a_declared_failure()
    {
        // Given Morgan's Tasks source and an empty build directory
        var source = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        using var workspace = TempWorkspace.Create();

        // When the source is built and Morgan traces EmptyTitle
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        var map = Path.Combine(workspace.Root, "Tasks.phrasemap.json");
        var trace = await PhraseCli.RunAsync("trace", "EmptyTitle", "--map", map);

        // Then the external map is complete, deterministic, and the trace reaches the owning spec
        Assert.True(build.ExitCode == 0, build.Text);
        Assert.True(File.Exists(map), $"Missing {map}");
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(map));
        var sourceLines = await File.ReadAllLinesAsync(source);
        var failureLine = Array.FindIndex(sourceLines, line => line.TrimStart().StartsWith("fails with EmptyTitle", StringComparison.Ordinal)) + 1;
        var specLine = Array.FindIndex(sourceLines, line => line.Contains("spec \"an empty title is rejected\"", StringComparison.Ordinal)) + 1;
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.NotEmpty(document.RootElement.GetProperty("buildId").GetString()!);
        var failure = document.RootElement.GetProperty("records").EnumerateArray()
            .Single(record => record.GetProperty("kind").GetString() == "failure" && record.GetProperty("name").GetString() == "EmptyTitle");
        Assert.Equal("Tasks.phrase", failure.GetProperty("phraseFile").GetString());
        Assert.Equal(failureLine, failure.GetProperty("line").GetInt32());
        Assert.All(document.RootElement.GetProperty("records").EnumerateArray()
            .Where(record => record.GetProperty("kind").GetString() is "endpoint" or "step" or "failure"),
            record => Assert.Matches("^[0-9a-f]{12}$", record.GetProperty("stepId").GetString()!));
        Assert.Equal("CreateTask", failure.GetProperty("endpoint").GetString());
        Assert.True(trace.ExitCode == 0, trace.Text);
        Assert.Contains("EmptyTitle  failure  422", trace.Stdout);
        Assert.Contains($"Tasks.phrase:{failureLine} endpoint CreateTask", trace.Stdout);
        Assert.Contains($"spec: Tasks.phrase:{specLine}-{specLine + 3}", trace.Stdout);
        Assert.Contains("stepId", trace.Stdout);

        // And this card adds no source file name with a line number, no trace header and no map reference
        // to the generated C# (the older [Phrase] attributes and step comments are a separate decision)
        var generated = string.Join("\n", Directory.GetFiles(workspace.Root, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
        AssertNoPhraseKnowledge(generated);
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Lf_and_crlf_sources_produce_the_same_map_lines_and_ids()
    {
        // Given Morgan has equivalent LF and CRLF copies of the Tasks source
        var text = await File.ReadAllTextAsync(RepoPaths.Find("demo", "tasks", "Tasks.phrase"));
        using var first = TempWorkspace.Create();
        using var second = TempWorkspace.Create();
        var lf = Path.Combine(first.Root, "Tasks.phrase");
        var crlf = Path.Combine(second.Root, "Tasks.phrase");
        await File.WriteAllTextAsync(lf, text.Replace("\r\n", "\n", StringComparison.Ordinal));
        await File.WriteAllTextAsync(crlf, text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\r\n", StringComparison.Ordinal));

        // When both copies are built
        var one = await PhraseCli.RunAsync("build", lf, "-o", first.Root);
        var two = await PhraseCli.RunAsync("build", crlf, "-o", second.Root);

        // Then normalization makes the complete maps byte-identical
        Assert.True(one.ExitCode == 0, one.Text);
        Assert.True(two.ExitCode == 0, two.Text);
        Assert.Equal(await File.ReadAllBytesAsync(Path.Combine(first.Root, "Tasks.phrasemap.json")),
            await File.ReadAllBytesAsync(Path.Combine(second.Root, "Tasks.phrasemap.json")));
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Repeated_builds_from_different_paths_are_byte_identical()
    {
        // Given the same LF-normalised Tasks source through absolute and backslash paths
        var source = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        using var first = TempWorkspace.Create();
        using var second = TempWorkspace.Create();

        // When Morgan builds both copies
        var one = await PhraseCli.RunAsync("build", source, "-o", first.Root);
        // The second path is a copy in another directory; on Windows it is written with backslashes
        using var copy = TempWorkspace.Create();
        var copied = Path.Combine(copy.Root, "Tasks.phrase");
        File.Copy(source, copied);
        var secondPath = OperatingSystem.IsWindows() ? copied.Replace('/', '\\') : copied;
        var two = await PhraseCli.RunAsync("build", secondPath, "-o", second.Root);

        // Then the external map and every generated C# file are byte-identical
        Assert.True(one.ExitCode == 0, one.Text);
        Assert.True(two.ExitCode == 0, two.Text);
        Assert.Equal(await File.ReadAllBytesAsync(Path.Combine(first.Root, "Tasks.phrasemap.json")), await File.ReadAllBytesAsync(Path.Combine(second.Root, "Tasks.phrasemap.json")));
        foreach (var path in Directory.GetFiles(first.Root, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(first.Root, path);
            Assert.Equal(await File.ReadAllBytesAsync(path), await File.ReadAllBytesAsync(Path.Combine(second.Root, relative)));
        }
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Unknown_trace_name_exits_nonzero_with_a_clear_message()
    {
        // Given Morgan has built the Tasks map
        using var workspace = TempWorkspace.Create();
        var source = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);

        // When a name absent from that map is traced
        var trace = await PhraseCli.RunAsync("trace", "MissingFailure", "--map", Path.Combine(workspace.Root, "Tasks.phrasemap.json"));

        // Then the command refuses the unknown name clearly
        Assert.True(build.ExitCode == 0, build.Text);
        Assert.NotEqual(0, trace.ExitCode);
        Assert.Contains("Unknown trace name or frame: MissingFailure", trace.Stderr);
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task Live_failures_and_endpoints_resolve_in_the_source_map()
    {
        // Given Morgan's Tasks source is built by the CLI into an empty temporary workspace
        var sourcePath = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        var sourceLines = await File.ReadAllLinesAsync(sourcePath);
        var resolved = Resolver.Resolve(string.Join('\n', sourceLines));
        Assert.Empty(resolved.Diagnostics);
        using var workspace = TempWorkspace.Create();
        var build = await PhraseCli.RunAsync("build", sourcePath, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);

        // When every declared failure is observed through the running API over real HTTP
        await TasksOverHttp.AssertEverySpecMatchesAsync(workspace.FindSingle("*.Api.csproj"), resolved.Module);
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(workspace.Root, "Tasks.phrasemap.json")));
        var records = document.RootElement.GetProperty("records").EnumerateArray().ToArray();

        // Then every endpoint and all four failures resolve to their exact declarations and owning endpoints
        var endpointRecords = records.Where(record => Text(record, "kind") == "endpoint").ToArray();
        Assert.Equal(resolved.Module.Endpoints.Select(endpoint => endpoint.Name).Order(), endpointRecords.Select(record => Text(record, "name")).Order());
        foreach (var endpoint in resolved.Module.Endpoints)
        {
            var record = endpointRecords.Single(candidate => Text(candidate, "name") == endpoint.Name);
            Assert.Equal(LineOf(sourceLines, $"endpoint {endpoint.Name} :"), record.GetProperty("line").GetInt32());
            Assert.Equal(endpoint.Name, Text(record, "endpoint"));
        }

        var expectedFailures = new[] { "EmptyTitle", "TitleTooLong", "NotFound", "StoreUnavailable" };
        var failureRecords = records.Where(record => Text(record, "kind") == "failure").ToArray();
        Assert.Equal(expectedFailures.Order(), failureRecords.Select(record => Text(record, "name")).Distinct().Order());

        // The set of (failure, endpoint) pairs in the map equals the pairs the endpoint specs assert, no more and no fewer
        var expectedPairs = resolved.Module.Endpoints
            .SelectMany(endpoint => endpoint.Specs.Where(spec => spec.Outcome is FailsModel).Select(spec => ((FailsModel)spec.Outcome).Code + "@" + endpoint.Name))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var actualPairs = failureRecords.Select(record => Text(record, "name") + "@" + Text(record, "endpoint")).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expectedPairs, actualPairs);

        foreach (var record in failureRecords)
        {
            var failure = Text(record, "name");
            var endpoint = Text(record, "endpoint");

            // The record points at a line that declares the failure, and its spec range contains the spec asserting it
            Assert.Contains(record.GetProperty("line").GetInt32(), LinesContainingFailure(sourceLines, failure));
            var specRange = record.GetProperty("specLines");
            var first = specRange.GetProperty("first").GetInt32();
            var last = specRange.GetProperty("last").GetInt32();
            var endpointLine = LineOf(sourceLines, $"endpoint {endpoint} :");
            var assertingLines = Enumerable.Range(first, last - first + 1)
                .Where(number => sourceLines[number - 1].Trim() == $"then fails with {failure}").ToArray();
            Assert.NotEmpty(assertingLines);
            Assert.All(assertingLines, number => Assert.True(number > endpointLine, $"{failure} spec at line {number} is not inside endpoint {endpoint}"));
            Assert.True(first > endpointLine, $"{failure} spec range does not start inside endpoint {endpoint}");
        }
    }

    [Fact]
    [Trait("Slice", "161")]
    public async Task No_phrase_knowledge_assertions_fail_on_a_reintroduced_leak()
    {
        // Given each forbidden form of Phrase source knowledge is reintroduced independently
        var leaks = new[] { "Tasks.phrase:12", "// phrase: Tasks.phrase", "X-Phrase-Source", "Tasks.phrasemap.json" };

        // When the shared generated-output assertions inspect each mutation
        foreach (var leak in leaks)
            Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => AssertNoPhraseKnowledge(leak));

        // Then those same assertions accept the real CLI-generated C# output
        var source = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
        using var workspace = TempWorkspace.Create();
        var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(build.ExitCode == 0, build.Text);
        var generated = string.Join("\n", Directory.GetFiles(workspace.Root, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
        AssertNoPhraseKnowledge(generated);
    }

    private static void AssertNoPhraseKnowledge(string generated)
    {
        Assert.DoesNotMatch(@"\.phrase:\d", generated);
        Assert.DoesNotContain("// phrase:", generated, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("X-Phrase-Source", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("phrasemap", generated, StringComparison.OrdinalIgnoreCase);
    }

    private static int LineOf(IReadOnlyList<string> lines, string declaration) =>
        Array.FindIndex(lines.ToArray(), line => line.StartsWith(declaration, StringComparison.Ordinal)) + 1;

    private static int[] LinesContainingFailure(IReadOnlyList<string> lines, string failure) =>
        lines.Select((line, index) => (line, number: index + 1))
            .Where(item => item.line.TrimStart().StartsWith("fails with ", StringComparison.Ordinal)
                && item.line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Contains(failure, StringComparer.Ordinal))
            .Select(item => item.number)
            .ToArray();

    private static string Text(JsonElement record, string property) => record.GetProperty(property).GetString()!;
}
