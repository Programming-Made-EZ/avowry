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
        var suiteDirectory = Path.Combine(workspace.Root, "bench");
        Directory.CreateDirectory(Path.Combine(suiteDirectory, "tasks", "001-write-greeting"));
        File.Copy(RepoPaths.Find("bench", "suite.json"), Path.Combine(suiteDirectory, "suite.json"));
        File.Copy(
            RepoPaths.Find("bench", "tasks", "001-write-greeting", "prompt.md"),
            Path.Combine(suiteDirectory, "tasks", "001-write-greeting", "prompt.md"));
        var modelOutput = Path.Combine(workspace.Root, "model-output.phrase");
        await File.WriteAllTextAsync(modelOutput, """
            module Greeting

            phrase "say hello"
              native csharp
                return;
            """);
        var modelCommand = OperatingSystem.IsWindows()
            ? $"type \"{modelOutput}\""
            : $"cat \"{modelOutput}\"";

        // When the benchmark CLI runs the fake model through its real process boundary
        var runner = RepoPaths.Find("tools", "Phrase.Bench", "Phrase.Bench.csproj");
        var startInfo = new System.Diagnostics.ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in new[]
        {
            "run", "--project", runner, "--", "run", "--suite", Path.Combine(suiteDirectory, "suite.json"),
            "--model", "fake-v1", "--model-command", modelCommand,
        })
            startInfo.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(startInfo)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        // Then real compiler scoring passes and both result stores contain the task metrics
        Assert.True(process.ExitCode == 0, stdout + stderr);
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
}
