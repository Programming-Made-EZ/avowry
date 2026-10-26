using Phrase.Acceptance.Tests.Support;
using System.Text.Json;

namespace Phrase.Acceptance.Tests;

public class BenchmarkHarnessAcceptance
{
    [Fact]
    public void v1_benchmark_harness_files_exist_for_the_cli_contract()
    {
        var suite = RepoPaths.Find("bench", "suite.json");
        var runner = RepoPaths.Find("tools", "Phrase.Bench", "Phrase.Bench.csproj");
        var resultsDir = RepoPaths.Find("bench", "results");
        var history = RepoPaths.Find("bench", "results", "history.jsonl");

        Assert.True(File.Exists(suite), $"Missing benchmark suite: {suite}");
        Assert.True(File.Exists(runner), $"Missing benchmark runner: {runner}");
        Assert.True(Directory.Exists(resultsDir), $"Missing benchmark results directory: {resultsDir}");
        Assert.True(File.Exists(history), $"Missing benchmark history file: {history}");
    }

    [Fact]
    public async Task run_scores_one_fake_model_and_persists_its_result()
    {
        // Given a one-task suite and a deterministic model command in an isolated workspace
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelOutput = Path.Combine(workspace.Root, "model-output.phrase");
        await File.WriteAllTextAsync(modelOutput, """
            module Greeting

            phrase "say hello"
              native csharp
                return;
            """);
        var modelCommand = OperatingSystem.IsWindows()
            ? $"more > nul && type \"{modelOutput}\""
            : $"cat > /dev/null && cat \"{modelOutput}\"";

        // When the benchmark CLI runs the fake model through its real process boundary
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "fake-v1", modelCommand);

        // Then real compiler scoring passes and both result stores contain the task metrics
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("fake-v1: passed", stdout);
        using var result = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(suiteDirectory, "results", "fake-v1.json")));
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(1, result.RootElement.GetProperty("attempts").GetInt32());

        var historyLine = Assert.Single(await File.ReadAllLinesAsync(
            Path.Combine(suiteDirectory, "results", "history.jsonl")));
        using var historyResult = JsonDocument.Parse(historyLine);
        Assert.Equal("fake-v1", historyResult.RootElement.GetProperty("model").GetString());
    }

    [Fact]
    public async Task run_retries_with_compiler_diagnostics_until_the_model_output_passes()
    {
        // Given a model that emits invalid Phrase once and valid Phrase after receiving diagnostics
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var validOutput = Path.Combine(workspace.Root, "valid.phrase");
        await File.WriteAllTextAsync(validOutput, """
            module Greeting

            phrase "say hello"
              native csharp
                return;
            """);
        var marker = Path.Combine(workspace.Root, "attempted");
        var received = Path.Combine(workspace.Root, "received.txt");
        var modelCommand = OperatingSystem.IsWindows()
            ? $"more >> \"{received}\" && if exist \"{marker}\" (type \"{validOutput}\") else (echo invalid & type nul > \"{marker}\")"
            : $"cat >> '{received}' && if test -f '{marker}'; then cat '{validOutput}'; else echo invalid; touch '{marker}'; fi";

        // When the benchmark CLI runs the attempt loop
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "retry-v1", modelCommand);

        // Then the second attempt passes and was prompted with the first attempt's diagnostics
        Assert.True(exitCode == 0, stdout + stderr);
        using var result = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(suiteDirectory, "results", "retry-v1.json")));
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(2, result.RootElement.GetProperty("attempts").GetInt32());
        var receivedPrompts = await File.ReadAllTextAsync(received);
        var diagnostics = result.RootElement.GetProperty("diagnostics").EnumerateArray().ToArray();
        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, diagnostic => Assert.Contains(diagnostic.GetString()!, receivedPrompts));
    }

    private static string CopySuiteTo(string workspaceRoot)
    {
        var suiteDirectory = Path.Combine(workspaceRoot, "bench");
        var taskDirectory = Path.Combine(suiteDirectory, "tasks", "001-write-greeting");
        Directory.CreateDirectory(taskDirectory);
        File.Copy(RepoPaths.Find("bench", "suite.json"), Path.Combine(suiteDirectory, "suite.json"));
        File.Copy(
            RepoPaths.Find("bench", "tasks", "001-write-greeting", "prompt.md"),
            Path.Combine(taskDirectory, "prompt.md"));
        return suiteDirectory;
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunBenchmark(
        string suiteDirectory, string model, string modelCommand)
    {
        var runner = RepoPaths.Find("tools", "Phrase.Bench", "Phrase.Bench.csproj");
        var startInfo = new System.Diagnostics.ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in new[]
        {
            "run", "--project", runner, "--", "run", "--suite", Path.Combine(suiteDirectory, "suite.json"),
            "--model", model, "--model-command", modelCommand,
        })
            startInfo.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(startInfo)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout, stderr);
    }
}
