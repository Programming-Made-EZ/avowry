using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Phrase.Acceptance.Tests.Support;
using Avowry.Compiler.Semantics;
using Avowry.Compiler.Syntax;

namespace Phrase.Acceptance.Tests;

// Milestone: Usable v1
// As Steve, I want one automated check that says whether Phrase can build an application,
// so that "usable" is detected, not guessed.
public class UsableV1 : IClassFixture<UsableV1Fixture>
{
    private readonly UsableV1Fixture fixture;
    public UsableV1(UsableV1Fixture fixture) => this.fixture = fixture;
    private static string DemoApp => RepoPaths.Find("demo", "tasks", "Tasks.avowry");

    [Fact]
    [Trait("Slice", "138")]
    [Trait("Milestone", "UsableV1")]
    public void Bar1_a_real_app_written_entirely_in_phrase()
    {
        // Given the real Tasks CRUD API beyond the orders fixture
        Assert.True(File.Exists(DemoApp), "Bar 1 not met: demo/tasks/Tasks.avowry does not exist.");
        // When it goes through the real compiler front end
        var parsed = Parser.Parse(File.ReadAllText(DemoApp));
        Assert.True(parsed.Diagnostics.Count == 0, "Bar 1 not met: parse diagnostics: " + string.Join("; ", parsed.Diagnostics));
        var resolved = Resolver.Resolve(parsed.File);
        Assert.True(resolved.Diagnostics.Count == 0, "Bar 1 not met: resolver diagnostics: " + string.Join("; ", resolved.Diagnostics));
        // Then it has 3 to 5 endpoints, validation, a failure spec, and no hand-written C# beside it
        var endpoints = resolved.Module.Endpoints;
        Assert.InRange(endpoints.Count, 3, 5);
        Assert.Contains(endpoints, endpoint => endpoint.Failures.Count > 0);
        Assert.Contains(endpoints.SelectMany(endpoint => endpoint.Specs), spec => spec.Outcome is FailsModel);
        Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(DemoApp)!, "*.cs", SearchOption.AllDirectories));
    }

    [Fact]
    [Trait("Slice", "138")]
    [Trait("Milestone", "UsableV1")]
    public async Task Bar2_one_cli_command_compiles_it_to_csharp_and_fastendpoints()
    {
        // Given the real Tasks application and an empty output directory
        // When one avowry build command compiles it
        var app = await fixture.App.Value;
        // Then it emits API and specs projects, including FastEndpoints endpoints
        Assert.True(app.Cli.ExitCode == 0, "Bar 2 not met: avowry build failed." + Environment.NewLine + app.Cli.Text);
        Assert.True(File.Exists(app.ApiProject), "Bar 2 not met: the generated API project is missing.");
        Assert.True(File.Exists(app.SpecsProject), "Bar 2 not met: the generated specs project is missing.");
        var generatedCSharp = Directory.GetFiles(Path.GetDirectoryName(app.ApiProject)!, "*.cs");
        Assert.Contains(generatedCSharp, file => File.ReadAllText(file).Contains("Endpoint<", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("Slice", "138")]
    [Trait("Milestone", "UsableV1")]
    public async Task Bar3_dotnet_build_of_the_generated_projects_has_zero_warnings()
    {
        // Given both projects emitted by the cached Tasks compilation
        var app = await fixture.App.Value;
        // When dotnet builds the generated API and specs projects
        var builds = await app.Builds.Value;
        // Then both builds succeed with zero warnings
        Assert.All(builds, build => Assert.True(build.ExitCode == 0, "Bar 3 not met." + Environment.NewLine + build.Log));
        Assert.All(builds, build => Assert.Contains("0 Warning(s)", build.Log, StringComparison.Ordinal));
    }

    [Fact]
    [Trait("Slice", "138")]
    [Trait("Milestone", "UsableV1")]
    public async Task Bar4_the_generated_xunit_specs_all_pass_one_test_per_spec()
    {
        // Given the generated specs project and the eleven specs in Tasks.avowry
        var app = await fixture.App.Value;
        Assert.Equal(11, app.Module.Endpoints.Sum(endpoint => endpoint.Specs.Count));
        // When the generated xUnit project runs
        var run = await app.Specs.Value;
        // Then every Phrase spec is represented by one passing test
        Assert.True(run.ExitCode == 0, "Bar 4 not met." + Environment.NewLine + run.Log);
        Assert.Equal(11, run.Total);
        Assert.Equal(0, run.Failed);
    }

    [Fact]
    [Trait("Slice", "138")]
    [Trait("Milestone", "UsableV1")]
    public async Task Bar5_the_app_runs_and_live_http_matches_the_specs()
    {
        // Given the generated Tasks API and all eleven resolved endpoint specs
        var app = await fixture.App.Value;
        // When every spec is exercised against fresh real HTTP processes
        await TasksOverHttp.AssertEverySpecMatchesAsync(app.ApiProject, app.Module);
        // Then the shared live-HTTP checker finds no status or body mismatch
    }

    [Fact]
    [Trait("Slice", "138")]
    [Trait("Milestone", "UsableV1")]
    [Trait("Status", "Pending")]
    // Card 162 reruns the LLM check against the renamed documents and un-pends this evidence binding.
    public void Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone()
    {
        // Given fresh-context model evidence and the committed documentation
        var files = Directory.GetFiles(RepoPaths.Find("docs", "usable-v1"), "llm-check-*.json");
        Assert.True(files.Length > 0, "Bar 6 not met: no docs/usable-v1/llm-check-*.json evidence exists.");
        var grammar = Sha256(RepoPaths.Find("docs", "GRAMMAR.md"));
        var cheatsheet = Sha256(RepoPaths.Find("docs", "CHEATSHEET.md"));
        // When each file is checked for a complete, current successful run
        var failures = files.Select(file => EvidenceFailure(file, grammar, cheatsheet)).Where(value => value is not null).ToList();
        // Then at least one model passed every spec warning-free against both current documents
        Assert.True(failures.Count < files.Length,
            "Bar 6 not met: no current passing LLM evidence." + Environment.NewLine + string.Join(Environment.NewLine, failures));
    }

    private static string? EvidenceFailure(string path, string grammar, string cheatsheet)
    {
        try
        {
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            var root = json.RootElement;
            if (!root.TryGetProperty("passed", out var passed) || passed.ValueKind != JsonValueKind.True)
                return $"{Path.GetFileName(path)}: passed is not true.";
            if (!root.TryGetProperty("specs", out var specs) || !specs.TryGetInt32(out var total) || total <= 0)
                return $"{Path.GetFileName(path)}: specs must be greater than 0.";
            if (!root.TryGetProperty("specs_passed", out var specsPassed) || !specsPassed.TryGetInt32(out var passedTotal) || passedTotal != total)
                return $"{Path.GetFileName(path)}: specs_passed must equal specs ({total}).";
            if (!root.TryGetProperty("build_warnings", out var warnings) || !warnings.TryGetInt32(out var count) || count != 0)
                return $"{Path.GetFileName(path)}: build_warnings must be 0.";
            if (!HashEquals(root, "grammar_sha256", grammar))
                return $"{Path.GetFileName(path)}: grammar_sha256 is stale; re-run against docs/GRAMMAR.md.";
            if (!HashEquals(root, "cheatsheet_sha256", cheatsheet))
                return $"{Path.GetFileName(path)}: cheatsheet_sha256 is stale; re-run against docs/CHEATSHEET.md.";
            return null;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return $"{Path.GetFileName(path)}: invalid evidence ({ex.Message}).";
        }
    }

    private static bool HashEquals(JsonElement root, string name, string expected) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
        && string.Equals(value.GetString(), expected, StringComparison.OrdinalIgnoreCase);

    private static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
}

public sealed class UsableV1Fixture : IAsyncLifetime
{
    private readonly TempWorkspace workspace = TempWorkspace.Create();
    public Lazy<Task<UsableV1App>> App { get; }
    public UsableV1Fixture() => App = new(BuildAppAsync);
    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() { workspace.Dispose(); return Task.CompletedTask; }

    private async Task<UsableV1App> BuildAppAsync()
    {
        var source = RepoPaths.Find("demo", "tasks", "Tasks.avowry");
        var module = Resolver.Resolve(await File.ReadAllTextAsync(source)).Module;
        var cli = await PhraseCli.RunAsync("build", source, "-o", workspace.Root);
        Assert.True(cli.ExitCode == 0, cli.Text);
        var api = workspace.FindSingle("*.Api.csproj");
        var specs = workspace.FindSingle("*.Specs.csproj");
        return new(cli, api, specs, module,
            new(async () => new[] { await BuildAsync(api), await BuildAsync(specs) }),
            new(() => DotnetCli.TestAsync(specs)));
    }

    private static async Task<ProjectBuild> BuildAsync(string project)
    {
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = Path.GetDirectoryName(project)! };
        foreach (var argument in new[] { "build", project, "--nologo", "--disable-build-servers" }) start.ArgumentList.Add(argument);
        start.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" }) start.Environment.Remove(name);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet build.");
        var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return new(process.ExitCode, new StringBuilder().Append(await stdout).Append(await stderr).ToString());
    }
}

public sealed record UsableV1App(CliRun Cli, string ApiProject, string SpecsProject, ModuleModel Module,
    Lazy<Task<ProjectBuild[]>> Builds, Lazy<Task<TestRunResult>> Specs);
public sealed record ProjectBuild(int ExitCode, string Log);
