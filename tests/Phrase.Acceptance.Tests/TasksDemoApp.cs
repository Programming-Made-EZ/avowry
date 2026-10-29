using System.Diagnostics;
using System.Text;
using Phrase.Acceptance.Tests.Support;
using Avowry.Compiler.Semantics;

namespace Phrase.Acceptance.Tests;

// Feature: The prototype Tasks API is written entirely in Phrase
//   As Steve, I want a small real Tasks API written only in Phrase,
//   so that I can see the language build something useful.
public class TasksDemoApp
{
    [Fact]
    [Trait("Slice", "135")]
    public async Task Tasks_source_is_only_phrase_and_lists_the_feature_order()
    {
        // Given the product owner's prototype source and its growth-checkpoint backlog
        var sourcePath = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        var backlogPath = RepoPaths.Find("demo", "tasks", "BACKLOG.md");
        Assert.True(File.Exists(sourcePath), $"Tasks demo not found: {sourcePath}");
        Assert.True(File.Exists(backlogPath), $"Tasks backlog not found: {backlogPath}");

        // When the demo surface and its declared native leaves are counted
        var source = await File.ReadAllTextAsync(sourcePath);
        var model = Resolver.Resolve(source);
        var backlog = await File.ReadAllTextAsync(backlogPath);

        // Then it is five Phrase endpoints, five declared native leaves, visible ensure rules, no hand-written C#,
        // and the backlog records create, get, list, complete, delete, validation in order
        Assert.Empty(model.Diagnostics);
        Assert.Equal(5, model.Module.Endpoints.Count);
        Assert.Equal(5, source.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n').Count(line => line == "  native csharp"));
        Assert.Contains("ensure title is not blank else EmptyTitle", source, StringComparison.Ordinal);
        Assert.Contains("ensure title length at most 200 else TitleTooLong", source, StringComparison.Ordinal);
        Assert.DoesNotContain("string.IsNullOrWhiteSpace", source, StringComparison.Ordinal);
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(sourcePath)!, "*.cs", SearchOption.AllDirectories));
        var positions = new[] { "Create", "Get", "List", "Complete", "Delete", "Validation" }
            .Select(feature => backlog.IndexOf(feature, StringComparison.Ordinal)).ToArray();
        Assert.All(positions, position => Assert.True(position >= 0));
        Assert.Equal(positions.Order(), positions);
    }

    [Fact]
    [Trait("Slice", "135")]
    public async Task Tasks_api_builds_and_every_endpoint_has_passing_success_and_failure_specs()
    {
        // Given the five-feature Tasks demo, written as Phrase source with declared native leaves only
        var sourcePath = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        Assert.True(File.Exists(sourcePath), $"Tasks demo not found: {sourcePath}");
        var source = await File.ReadAllTextAsync(sourcePath);
        var model = Resolver.Resolve(source);
        Assert.Empty(model.Diagnostics);
        Assert.InRange(model.Module.Endpoints.Count, 3, 5);
        Assert.Equal(5, model.Module.Endpoints.Count);
        Assert.All(model.Module.Endpoints, endpoint =>
        {
            Assert.Contains(endpoint.Specs, spec => spec.Outcome is RespondsModel or SucceedsModel);
            Assert.Contains(endpoint.Specs, spec => spec.Outcome is FailsModel);
        });
        Assert.Contains(model.Module.Endpoints.SelectMany(endpoint => endpoint.Specs), spec =>
            spec.Outcome is FailsModel { Code: "EmptyTitle" });
        Assert.Equal(5, source.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n').Count(line => line == "  native csharp"));
        var demoRoot = Path.GetDirectoryName(sourcePath)!;
        Assert.Empty(Directory.GetFiles(demoRoot, "*.cs", SearchOption.AllDirectories));
        Assert.True(File.Exists(Path.Combine(demoRoot, "BACKLOG.md")));
        using var workspace = TempWorkspace.Create();

        // When product owner builds the real file with avowry build and runs the generated specs
        var cli = await PhraseCli.RunAsync("build", sourcePath, "-o", workspace.Root);
        Assert.True(cli.ExitCode == 0, cli.Text);
        var build = await BuildAsync(workspace.FindSingle("*.Api.csproj"));
        var specs = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

        // Then the API builds with zero warnings and every Phrase spec is one passing generated test
        Assert.True(build.ExitCode == 0, build.Log);
        Assert.Contains("0 Warning(s)", build.Log, StringComparison.Ordinal);
        Assert.True(specs.ExitCode == 0, specs.Log);
        Assert.Equal(model.Module.Endpoints.Sum(endpoint => endpoint.Specs.Count), specs.Total);
        Assert.Equal(0, specs.Failed);

        // And the provisional TaskStore used by the app remains safe under concurrent requests
        var phrases = await File.ReadAllTextAsync(workspace.FindSingle("Avows.cs"));
        Assert.Equal(4, CountOf(phrases, "lock (gate)"));
    }

    private static async Task<(int ExitCode, string Log)> BuildAsync(string project)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(project)!,
        };
        foreach (var argument in new[] { "build", project, "--nologo", "--disable-build-servers" })
            start.ArgumentList.Add(argument);
        start.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" })
            start.Environment.Remove(name);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet build.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, new StringBuilder().Append(await stdout).Append(await stderr).ToString());
    }

    private static int CountOf(string text, string value)
    {
        var count = 0;
        for (var start = 0; (start = text.IndexOf(value, start, StringComparison.Ordinal)) >= 0; start += value.Length)
            count++;
        return count;
    }
}
