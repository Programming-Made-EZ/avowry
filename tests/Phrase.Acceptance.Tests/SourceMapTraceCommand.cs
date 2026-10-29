using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: trace checks that its evidence still belongs to the generated application
//   As Morgan, I want trace to reject stale or malformed evidence clearly,
//   so that diagnosing a failure never silently trusts the wrong build or crashes.
public class SourceMapTraceCommand
{
    [Fact]
    [Trait("Slice", "168")]
    public async Task Trace_with_a_matching_assembly_prints_a_matching_build_check()
    {
        // Given Morgan has built the Tasks source and its generated API assembly
        using var workspace = await BuiltTasks.CreateAsync();

        // When Morgan traces a declared failure against that assembly
        var trace = await PhraseCli.RunAsync("trace", "EmptyTitle", "--map", workspace.Map, "--assembly", workspace.Assembly);

        // Then trace confirms that the map belongs to the assembly
        Assert.True(trace.ExitCode == 0, trace.Text);
        Assert.Contains("map matches assembly", trace.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "168")]
    public async Task Trace_with_an_assembly_from_another_source_warns_and_strict_exits_nonzero()
    {
        // Given Morgan has one Tasks map and an assembly generated from changed Tasks source
        using var first = await BuiltTasks.CreateAsync();
        using var second = await BuiltTasks.CreateAsync(source => source.Replace("module Tasks", "# a different source build\nmodule Tasks", StringComparison.Ordinal));

        // When Morgan traces the first map against the second assembly normally and strictly
        var warning = await PhraseCli.RunAsync("trace", "EmptyTitle", "--map", first.Map, "--assembly", second.Assembly);
        var strict = await PhraseCli.RunAsync("trace", "EmptyTitle", "--map", first.Map, "--assembly", second.Assembly, "--strict");

        // Then the mismatch is loud but only strict mode makes it fatal
        Assert.True(warning.ExitCode == 0, warning.Text);
        Assert.Contains("WARNING: map does not match assembly", warning.Stdout, StringComparison.Ordinal);
        Assert.NotEqual(0, strict.ExitCode);
        Assert.Contains("WARNING: map does not match assembly", strict.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "168")]
    public async Task Trace_reports_a_generated_file_hash_mismatch()
    {
        // Given Morgan has built Tasks and one mapped generated file has changed afterward
        using var workspace = await BuiltTasks.CreateAsync();
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(workspace.Map));
        var record = document.RootElement.GetProperty("records").EnumerateArray().First();
        var generated = Path.Combine(workspace.Root, record.GetProperty("csFile").GetString()!.Replace('/', Path.DirectorySeparatorChar));
        await File.AppendAllTextAsync(generated, "// changed after map creation\n");

        // When Morgan strictly traces the record
        var trace = await PhraseCli.RunAsync("trace", record.GetProperty("name").GetString()!, "--map", workspace.Map, "--strict");

        // Then the changed C# hash is named and the command fails
        Assert.NotEqual(0, trace.ExitCode);
        Assert.Contains("generated C# hash does not match map", trace.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "168")]
    public async Task Trace_resolves_a_Class_Method_frame_taken_from_the_map()
    {
        // Given Morgan takes a generated Class.Method frame and its step id from the Tasks map
        using var workspace = await BuiltTasks.CreateAsync(buildAssembly: false);
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(workspace.Map));
        var record = document.RootElement.GetProperty("records").EnumerateArray().First();
        var frame = record.GetProperty("csMethod").GetString()!;
        var stepId = record.GetProperty("stepId").GetString()!;

        // When Morgan traces that generated frame
        var trace = await PhraseCli.RunAsync("trace", frame, "--map", workspace.Map);

        // Then trace prints the same source-map record
        Assert.True(trace.ExitCode == 0, trace.Text);
        Assert.Contains($"stepId {stepId}", trace.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "168")]
    public async Task Trace_of_a_shared_name_prints_every_match()
    {
        // Given the Tasks map contains several records named NotFound
        using var workspace = await BuiltTasks.CreateAsync(buildAssembly: false);
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(workspace.Map));
        var expected = document.RootElement.GetProperty("records").EnumerateArray()
            .Count(record => record.GetProperty("name").GetString() == "NotFound");
        Assert.True(expected > 1, $"Expected a shared NotFound name, found {expected} record(s).");

        // When Morgan traces the shared name
        var trace = await PhraseCli.RunAsync("trace", "NotFound", "--map", workspace.Map);

        // Then every matching record is printed instead of an arbitrary first match
        Assert.True(trace.ExitCode == 0, trace.Text);
        Assert.True(Occurrences(trace.Stdout, "NotFound  failure") >= expected, trace.Stdout);
    }

    [Theory]
    [Trait("Slice", "168")]
    [InlineData("invalid-json")]
    [InlineData("null-record")]
    [InlineData("missing-specLines")]
    [InlineData("missing-csFile")]
    [InlineData("missing-map")]
    [InlineData("missing-assembly")]
    [InlineData("not-an-assembly")]
    public async Task Trace_rejects_bad_inputs_cleanly(string badInput)
    {
        // Given Morgan supplies one malformed or missing trace input
        using var workspace = await BuiltTasks.CreateAsync(buildAssembly: false);
        var map = workspace.Map;
        string? assembly = null;
        var expected = badInput switch
        {
            "invalid-json" => "JSON",
            "missing-map" => "Source map",
            "null-record" => "record",
            "missing-specLines" => "specLines",
            "missing-csFile" => "csFile",
            _ => badInput,
        };
        if (badInput == "invalid-json")
            await File.WriteAllTextAsync(map, "{ definitely not json");
        else if (badInput == "missing-map")
            map = Path.Combine(workspace.Root, "missing.phrasemap.json");
        else if (badInput is "missing-assembly" or "not-an-assembly")
        {
            assembly = Path.Combine(workspace.Root, badInput == "missing-assembly" ? "missing.dll" : "not-an-assembly.dll");
            if (badInput == "not-an-assembly")
                await File.WriteAllTextAsync(assembly, "plain text");
            expected = "assembly";
        }
        else
        {
            var root = JsonNode.Parse(await File.ReadAllTextAsync(map))!.AsObject();
            var records = root["records"]!.AsArray();
            if (badInput == "null-record")
                records[0] = null;
            else
                records[0]!.AsObject().Remove(badInput == "missing-specLines" ? "specLines" : "csFile");
            await File.WriteAllTextAsync(map, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }

        // When Morgan asks trace to read that input
        var arguments = new List<string> { "trace", "EmptyTitle", "--map", map };
        if (assembly is not null)
            arguments.AddRange(["--assembly", assembly]);
        var trace = await PhraseCli.RunAsync(arguments.ToArray());

        // Then trace names the problem without leaking an unhandled exception or stack trace
        Assert.NotEqual(0, trace.ExitCode);
        Assert.Contains(expected, trace.Stderr, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Unhandled exception", trace.Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(@"\n\s+at \S+", trace.Text);
    }

    private static int Occurrences(string text, string value) =>
        text.Split(value, StringSplitOptions.None).Length - 1;

    private sealed class BuiltTasks : IDisposable
    {
        private readonly TempWorkspace workspace;

        private BuiltTasks(TempWorkspace workspace, string assembly)
        {
            this.workspace = workspace;
            Assembly = assembly;
        }

        public string Root => workspace.Root;
        public string Map => Path.Combine(Root, "Tasks.phrasemap.json");
        public string Assembly { get; }

        public static async Task<BuiltTasks> CreateAsync(Func<string, string>? change = null, bool buildAssembly = true)
        {
            var workspace = TempWorkspace.Create();
            var source = RepoPaths.Find("demo", "tasks", "Tasks.phrase");
            if (change is not null)
            {
                var copy = Path.Combine(workspace.Root, "Tasks.phrase");
                await File.WriteAllTextAsync(copy, change(await File.ReadAllTextAsync(source)));
                source = copy;
            }
            var build = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
            Assert.True(build.ExitCode == 0, build.Text);
            var assembly = Path.Combine(workspace.Root, "Tasks.Api", "bin", "Debug", "net8.0", "Tasks.Api.dll");
            if (buildAssembly)
            {
                var project = workspace.FindSingle("Tasks.Api.csproj");
                var result = await BuildAsync(project);
                Assert.True(result.ExitCode == 0, result.Text);
                Assert.True(File.Exists(assembly), $"Missing {assembly}\n{result.Text}");
            }
            return new BuiltTasks(workspace, assembly);
        }

        public void Dispose() => workspace.Dispose();

        private static async Task<CliRun> BuildAsync(string project)
        {
            var info = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(project)!,
            };
            foreach (var argument in new[] { "build", project, "--nologo", "--disable-build-servers" })
                info.ArgumentList.Add(argument);
            info.Environment["DOTNET_ROLL_FORWARD"] = "Major";
            info.Environment["MSBUILDDISABLENODEREUSE"] = "1";
            foreach (var name in new[]
            {
                "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly",
                "OutputPath", "OutDir", "BaseOutputPath", "BaseIntermediateOutputPath", "IntermediateOutputPath",
                "Configuration", "TargetFramework", "UseAppHost",
            })
                info.Environment.Remove(name);
            using var process = Process.Start(info) ?? throw new InvalidOperationException("Could not start dotnet build.");
            var pid = process.Id;
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited && process.Id == pid)
                    process.Kill();
                throw new TimeoutException($"dotnet build process {pid} exceeded 5 minutes.");
            }
            return new CliRun(process.ExitCode, await stdout, await stderr);
        }
    }
}
