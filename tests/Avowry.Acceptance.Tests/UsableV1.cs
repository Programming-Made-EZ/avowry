using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Avowry.Acceptance.Tests.Support;
using Avowry.Compiler.Semantics;
using Avowry.Compiler.Syntax;

namespace Avowry.Acceptance.Tests;

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
    [Trait("Criterion", "162-2")]
    [Trait("Status", "Pending")]
    // Card 162 reruns the LLM check against the re-frozen documents; green 2 removes this Pending trait.
    public void Bar6_a_model_that_never_saw_the_app_can_add_an_endpoint_from_the_docs_alone()
    {
        // Given fresh-context model evidence for both models and the committed documentation
        var grammar = Sha256(RepoPaths.Find("docs", "GRAMMAR.md"));
        var cheatsheet = Sha256(RepoPaths.Find("docs", "CHEATSHEET.md"));
        // When each of the two model files is checked for a complete, current successful run
        var failure = Bar6Failure(RepoPaths.Find("docs", "usable-v1"), grammar, cheatsheet);
        // Then both Codex and Grok passed every spec warning-free against both current documents
        Assert.True(failure is null, "Bar 6 not met." + Environment.NewLine + failure);
    }

    // Card 162 criterion 2 rule: exactly llm-check-codex.json and llm-check-grok.json count, each must be current and
    // passing, its model must equal the file name and its proof level must be specs-passed. Other llm-check-*.json
    // files are ignored. Returns null when bar 6 is met, else the lines that say what is missing.
    internal static string? Bar6Failure(string dir, string grammar, string sheet)
    {
        var failures = new List<string>();
        foreach (var model in new[] { "codex", "grok" })
        {
            var name = $"llm-check-{model}.json";
            var path = Path.Combine(dir, name);
            if (!File.Exists(path)) { failures.Add($"{name}: evidence file is missing."); continue; }
            var failure = EvidenceFailure(path, grammar, sheet) ?? ModelFailure(path, model);
            if (failure is not null) failures.Add(failure);
        }
        return failures.Count == 0 ? null : string.Join(Environment.NewLine, failures);
    }

    private static string? ModelFailure(string path, string model)
    {
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        var root = json.RootElement;
        var name = Path.GetFileName(path);
        if (!root.TryGetProperty("model", out var value) || value.ValueKind != JsonValueKind.String || value.GetString() != model)
            return $"{name}: model must be \"{model}\".";
        if (!root.TryGetProperty("proof_level", out var proof) || proof.ValueKind != JsonValueKind.String || proof.GetString() != "specs-passed")
            return $"{name}: proof_level must be \"specs-passed\".";
        return null;
    }

    // ---- Criterion 2 evidence rule (characterisation of Bar6Failure; planted rows, they pass at red) --------
    // Named Evidence_rule_*, never Bar6_*: the usable-v1 report maps a test name starting Bar6_ to bar 6.

    private const string PlantedGrammar = "aaaa";
    private const string PlantedSheet = "bbbb";

    private static string PlantedEvidence(string model, string grammar = PlantedGrammar, int specs = 13, int specsPassed = 13,
        bool passed = true, string? modelField = null) =>
        $$"""
        {"model": "{{modelField ?? model}}", "date": "2026-11-11", "attempts": 1, "tokens_in": 6000, "tokens_out": 1000,
         "passed": {{(passed ? "true" : "false")}}, "proof_level": "specs-passed", "specs": {{specs}}, "specs_passed": {{specsPassed}},
         "build_warnings": 0, "grammar_sha256": "{{grammar}}", "cheatsheet_sha256": "{{PlantedSheet}}"}
        """;

    private static string? PlantedBar6(Action<string> plant)
    {
        using var workspace = TempWorkspace.Create();
        plant(workspace.Root);
        return Bar6Failure(workspace.Root, PlantedGrammar, PlantedSheet);
    }

    private static void Plant(string dir, string model, string json) => File.WriteAllText(Path.Combine(dir, $"llm-check-{model}.json"), json);

    [Fact]
    [Trait("Slice", "162")]
    public void Evidence_rule_V0_both_current_files_meet_bar_6()
    {
        // Given a current passing Codex file and a current passing Grok file
        // When bar 6 is evaluated
        var failure = PlantedBar6(dir => { Plant(dir, "codex", PlantedEvidence("codex")); Plant(dir, "grok", PlantedEvidence("grok")); });
        // Then it is met
        Assert.Null(failure);
    }

    [Fact]
    [Trait("Slice", "162")]
    public void Evidence_rule_E1_a_stale_grok_file_next_to_a_current_codex_file_fails_naming_grok()
    {
        var failure = PlantedBar6(dir => { Plant(dir, "codex", PlantedEvidence("codex")); Plant(dir, "grok", PlantedEvidence("grok", grammar: "stale")); });
        Assert.NotNull(failure);
        Assert.Contains("llm-check-grok.json", failure, StringComparison.Ordinal);
        Assert.Contains("grammar_sha256 is stale", failure, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "162")]
    public void Evidence_rule_E2_a_missing_codex_file_fails_naming_codex()
    {
        var failure = PlantedBar6(dir => Plant(dir, "grok", PlantedEvidence("grok")));
        Assert.NotNull(failure);
        Assert.Contains("llm-check-codex.json: evidence file is missing", failure, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "162")]
    public void Evidence_rule_E3_a_file_with_passed_false_fails_naming_it()
    {
        var failure = PlantedBar6(dir => { Plant(dir, "codex", PlantedEvidence("codex", passed: false)); Plant(dir, "grok", PlantedEvidence("grok")); });
        Assert.NotNull(failure);
        Assert.Contains("llm-check-codex.json: passed is not true", failure, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "162")]
    public void Evidence_rule_E4_a_partial_run_with_specs_passed_below_specs_fails_naming_it()
    {
        var failure = PlantedBar6(dir => { Plant(dir, "codex", PlantedEvidence("codex")); Plant(dir, "grok", PlantedEvidence("grok", specsPassed: 12)); });
        Assert.NotNull(failure);
        Assert.Contains("llm-check-grok.json: specs_passed must equal specs (13)", failure, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "162")]
    public void Evidence_rule_E5_a_file_whose_model_is_not_its_file_name_fails_naming_it()
    {
        var failure = PlantedBar6(dir => { Plant(dir, "codex", PlantedEvidence("codex")); Plant(dir, "grok", PlantedEvidence("grok", modelField: "codex")); });
        Assert.NotNull(failure);
        Assert.Contains("llm-check-grok.json: model must be \"grok\"", failure, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Slice", "162")]
    public void Evidence_rule_E6_an_extra_stale_third_file_is_ignored()
    {
        var failure = PlantedBar6(dir =>
        {
            Plant(dir, "codex", PlantedEvidence("codex"));
            Plant(dir, "grok", PlantedEvidence("grok"));
            Plant(dir, "other", PlantedEvidence("other", grammar: "stale", passed: false));
        });
        Assert.Null(failure);
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
