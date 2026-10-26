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
        using var result = await ReadModelResult(suiteDirectory, "fake-v1");
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
        using var result = await ReadModelResult(suiteDirectory, "retry-v1");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(2, result.RootElement.GetProperty("attempts").GetInt32());
        var receivedPrompts = await File.ReadAllTextAsync(received);
        var diagnostics = result.RootElement.GetProperty("diagnostics").EnumerateArray().ToArray();
        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, diagnostic => Assert.Contains(diagnostic.GetString()!, receivedPrompts));
    }

    [Fact]
    public async Task run_records_an_unavailable_model_instead_of_losing_the_result()
    {
        // Given a model command that is unavailable
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelCommand = OperatingSystem.IsWindows()
            ? "more > nul && echo model unavailable 1>&2 && exit /b 17"
            : "cat > /dev/null; echo model unavailable >&2; exit 17";

        // When the benchmark CLI tries to run that model
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "missing-v1", modelCommand);

        // Then it records an unavailable result with the error for the suite to continue past
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("missing-v1: unavailable", stdout);
        using var result = await ReadModelResult(suiteDirectory, "missing-v1");
        Assert.Equal("unavailable", result.RootElement.GetProperty("status").GetString());
        Assert.Contains("model unavailable", result.RootElement.GetProperty("error").GetString());

        var historyLine = Assert.Single(await File.ReadAllLinesAsync(
            Path.Combine(suiteDirectory, "results", "history.jsonl")));
        using var historyResult = JsonDocument.Parse(historyLine);
        Assert.Equal("unavailable", historyResult.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task run_records_a_timed_out_model_as_unavailable()
    {
        // Given a model command that does not finish within its configured timeout
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelCommand = OperatingSystem.IsWindows()
            ? "more > nul && ping -n 6 127.0.0.1 > nul"
            : "cat > /dev/null; sleep 5";

        // When the benchmark CLI times out that model
        var (exitCode, stdout, stderr) = await RunBenchmark(
            suiteDirectory, "slow-v1", modelCommand, "--timeout-ms", "100");

        // Then it records the timeout and returns success so another model can run
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("slow-v1: unavailable", stdout);
        using var result = await ReadModelResult(suiteDirectory, "slow-v1");
        Assert.Equal("unavailable", result.RootElement.GetProperty("status").GetString());
        Assert.Contains("timed out", result.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task run_skips_holdout_tasks_in_default_development_mode()
    {
        // Given a suite with both regular and holdout tasks
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = Path.Combine(workspace.Root, "bench");
        var developmentTaskDirectory = Path.Combine(suiteDirectory, "tasks", "001-write-greeting");
        var holdoutTaskDirectory = Path.Combine(suiteDirectory, "tasks", "002-write-hello");
        Directory.CreateDirectory(developmentTaskDirectory);
        Directory.CreateDirectory(holdoutTaskDirectory);

        await File.WriteAllTextAsync(Path.Combine(suiteDirectory, "suite.json"), """
            {
              "version": 1,
              "tasks": [
                {
                  "id": "001-write-greeting",
                  "kind": "write",
                  "prompt": "tasks/001-write-greeting/prompt.md",
                  "holdout": false
                },
                {
                  "id": "002-write-hello",
                  "kind": "write",
                  "prompt": "tasks/002-write-hello/prompt.md",
                  "holdout": true
                }
              ]
            }
            """);
        await File.WriteAllTextAsync(
            Path.Combine(developmentTaskDirectory, "prompt.md"),
            "Write a valid Phrase module named `Greeting` containing a phrase that returns the text `hello`.");
        await File.WriteAllTextAsync(
            Path.Combine(holdoutTaskDirectory, "prompt.md"),
            "Write a valid Phrase module named `Hello` containing a phrase that returns the text `world`.");

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

        // When the benchmark CLI runs the default development slice
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "holdout-v1", modelCommand);

        // Then only the non-holdout task is evaluated, leaving the holdout for keep-or-revert
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("001-write-greeting", stdout);
        Assert.DoesNotContain("002-write-hello", stdout);
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

    private static async Task<JsonDocument> ReadModelResult(string suiteDirectory, string model) =>
        JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(suiteDirectory, "results", $"{model}.json")));

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunBenchmark(
        string suiteDirectory, string model, string modelCommand, params string[] additionalArguments)
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
        foreach (var argument in additionalArguments)
            startInfo.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(startInfo)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout, stderr);
    }
}
