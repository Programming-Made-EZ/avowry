using System.Text.Json;
using Phrase.Acceptance.Tests.Support;

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
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.NotEmpty(document.RootElement.GetProperty("buildId").GetString()!);
        var failure = document.RootElement.GetProperty("records").EnumerateArray()
            .Single(record => record.GetProperty("kind").GetString() == "failure" && record.GetProperty("name").GetString() == "EmptyTitle");
        Assert.Equal("Tasks.phrase", failure.GetProperty("phraseFile").GetString());
        Assert.Equal(18, failure.GetProperty("line").GetInt32());
        Assert.Equal("CreateTask", failure.GetProperty("endpoint").GetString());
        Assert.True(trace.ExitCode == 0, trace.Text);
        Assert.Contains("EmptyTitle  failure  422", trace.Stdout);
        Assert.Contains("Tasks.phrase:18 endpoint CreateTask", trace.Stdout);
        Assert.Contains("spec: Tasks.phrase:66-69", trace.Stdout);

        // And no generated C# contains Phrase file, line, endpoint-comment, or step-comment knowledge
        var generated = string.Join("\n", Directory.GetFiles(workspace.Root, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
        Assert.DoesNotContain("Tasks.phrase", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("// phrase:", generated, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("// validate task title input.title", generated, StringComparison.Ordinal);
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
        var two = await PhraseCli.RunAsync("build", source.Replace('/', '\\'), "-o", second.Root);

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
}
