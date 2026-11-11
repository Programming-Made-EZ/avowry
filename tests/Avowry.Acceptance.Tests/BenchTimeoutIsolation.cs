using Avowry.Acceptance.Tests.Support;
using System.Diagnostics;
using System.Text.Json;

namespace Avowry.Acceptance.Tests;

[Collection("Isolated timing")] // D247 and card 417: a test that asserts a wall-clock limit runs alone, after every parallel collection has finished
public class BenchTimeoutIsolation
{
    // Feature: A model that outlives its timeout is stopped with its whole process tree
    //   As Riley, I want timed-out model attempts to end promptly and be recorded,
    //   so that one stuck CLI cannot stall the benchmark run or escape as a child process.
    [Fact]
    [Trait("Slice", "114")]
    public async Task timed_out_model_tree_is_stopped_and_recorded_in_both_arms()
    {
        // Given each benchmark arm a roughly 10 KB prompt and the same shell-launched fake model
        // whose grandchild ignores stdin, inherits the output pipes, and records its exact process id
        using var workspace = TempWorkspace.Create();
        var phraseSuite = CopySuiteTo(Path.Combine(workspace.Root, "phrase"));
        File.WriteAllText(
            Path.Combine(phraseSuite, "tasks", "001-write-greeting", "prompt.md"),
            new string('p', 10 * 1024));
        var csharpSuite = WriteCSharpTimeoutSuite(Path.Combine(workspace.Root, "csharp"));

        foreach (var arm in new[]
        {
            (Project: "Avowry.Bench", Suite: Path.Combine(phraseSuite, "suite.json"), Model: "tree-phrase-v1"),
            (Project: "Avowry.BenchCs", Suite: csharpSuite, Model: "tree-csharp-v1"),
        })
        {
            var grandchildPidPath = Path.Combine(workspace.Root, arm.Model + "-grandchild.pid");
            var modelCommand = WriteIgnoringModelTree(workspace.Root, arm.Model, grandchildPidPath);
            int? grandchildPid = null;
            try
            {
                // When Riley runs the arm with a three-second model timeout
                var stopwatch = Stopwatch.StartNew();
                var run = await RunBenchmarkArm(arm.Project, arm.Suite, arm.Model, modelCommand, 3000);
                stopwatch.Stop();
                grandchildPid = int.Parse(await File.ReadAllTextAsync(grandchildPidPath));

                // Then the attempt is a prompt timeout, finishes within twenty seconds, consumes its
                // prompt tokens, and leaves no recorded grandchild running
                Assert.True(run.ExitCode == 0, run.Stdout + run.Stderr);
                Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20), $"{arm.Project} took {stopwatch.Elapsed}.");
                using var result = JsonDocument.Parse(await File.ReadAllTextAsync(
                    Path.Combine(Path.GetDirectoryName(arm.Suite)!, "results", arm.Model + ".json")));
                Assert.Equal("unavailable", result.RootElement.GetProperty("status").GetString());
                Assert.Equal(1, result.RootElement.GetProperty("attempts").GetInt32());
                Assert.True(result.RootElement.GetProperty("tokens_in").GetInt32() >= 2500);
                Assert.Contains("timed out", result.RootElement.GetProperty("error").GetString());
                Assert.Contains("3000 ms", result.RootElement.GetProperty("error").GetString());
                Assert.Contains("after", result.RootElement.GetProperty("error").GetString());
                Assert.False(IsRunning(grandchildPid.Value), $"Grandchild {grandchildPid} from {arm.Project} was still running.");
            }
            finally
            {
                if (grandchildPid is null && File.Exists(grandchildPidPath) &&
                    int.TryParse(await File.ReadAllTextAsync(grandchildPidPath), out var recordedPid))
                    grandchildPid = recordedPid;
                if (grandchildPid is int pid && IsRunning(pid))
                {
                    using var process = Process.GetProcessById(pid);
                    process.Kill();
                    await process.WaitForExitAsync();
                }
            }
        }
    }

    private static string WriteCSharpTimeoutSuite(string root)
    {
        var suiteDirectory = Path.Combine(root, "csharp-baseline");
        var taskDirectory = Path.Combine(suiteDirectory, "tasks", "001-write-greeting");
        Directory.CreateDirectory(taskDirectory);
        File.WriteAllText(Path.Combine(taskDirectory, "prompt.md"), new string('c', 10 * 1024));
        File.WriteAllText(Path.Combine(taskDirectory, "outcomes.json"),
            "[{\"name\":\"returns greeting\",\"given\":{},\"when\":\"Greeter.SayHello\",\"then\":{\"returns\":\"hello\"}}]");
        var suite = Path.Combine(suiteDirectory, "suite.json");
        File.WriteAllText(suite,
            "{\"version\":1,\"tasks\":[{\"id\":\"001-write-greeting\",\"kind\":\"write\",\"prompt\":\"tasks/001-write-greeting/prompt.md\",\"outcomes\":\"tasks/001-write-greeting/outcomes.json\",\"holdout\":false}]}");
        return suite;
    }

    private static string WriteIgnoringModelTree(string root, string name, string grandchildPidPath)
    {
        var script = Path.Combine(root, name + "-model.py");
        File.WriteAllText(script, $$"""
            import pathlib, subprocess, sys
            child = subprocess.Popen(
                [sys.executable, "-c", "import time; time.sleep(120)"],
                stdin=subprocess.DEVNULL)
            pathlib.Path(r"{{grandchildPidPath}}").write_text(str(child.pid))
            child.wait()
            """);
        return $"python \"{script}\"";
    }

    private static bool IsRunning(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunBenchmarkArm(
        string projectName, string suite, string model, string modelCommand, int timeoutMs)
    {
        var info = BenchLauncher.CreateStartInfoFor(projectName, new[]
        {
            "run", "--suite", suite,
            "--model", model, "--model-command", modelCommand, "--timeout-ms", timeoutMs.ToString(),
            "--private-results", Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl"),
        });
        using var process = Process.Start(info)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        try
        {
            await process.WaitForExitAsync(limit.Token);
        }
        catch (OperationCanceledException) when (limit.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            throw new Xunit.Sdk.XunitException($"{projectName} did not finish within 20 seconds.");
        }
        return (process.ExitCode, await stdout, await stderr);
    }

    private static string CopySuiteTo(string workspaceRoot)
    {
        var suiteDirectory = Path.Combine(workspaceRoot, "bench");
        var taskDirectory = Path.Combine(suiteDirectory, "tasks", "001-write-greeting");
        Directory.CreateDirectory(taskDirectory);
        File.WriteAllText(Path.Combine(suiteDirectory, "suite.json"), """
            {
              "version": 1,
              "tasks": [
                {
                  "id": "001-write-greeting",
                  "kind": "write",
                  "prompt": "tasks/001-write-greeting/prompt.md",
                  "holdout": false
                }
              ]
            }
            """);
        File.Copy(
            RepoPaths.Find("bench", "tasks", "001-write-greeting", "prompt.md"),
            Path.Combine(taskDirectory, "prompt.md"));
        return suiteDirectory;
    }
}

[CollectionDefinition("Isolated timing", DisableParallelization = true)]
public sealed class IsolatedTimingCollection { }
