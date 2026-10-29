using System.Diagnostics;
using System.Text.Json;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

[CollectionDefinition("LlmCustomerCheck", DisableParallelization = true)]
public sealed class LlmCustomerCheckCollection
{
}

// Feature: LLM-customer check: a model adds an endpoint from the docs alone
//   As Ari, I want a cold model that never saw the Tasks app to add one new endpoint
//   with specs from GRAMMAR.md and the cheat sheet alone, so that I know the docs
//   are enough for a new user.
[Collection("LlmCustomerCheck")]
public sealed class LlmCustomerCheck
{
    private static readonly string OpenEndpointAddition = """

        avow to "list open tasks {available: Bool}" -> List<Task>
          effects TaskStore
          fails with StoreUnavailable
          native csharp
            if (!available)
                Fail("StoreUnavailable");
            var open = new List<Task>();
            foreach (var task in taskstore.List())
            {
                if (!task.Done)
                    open.Add(task);
            }
            return open;

        endpoint ListOpenTasks : GET /tasks/open
          input
            available: Bool
          output
            tasks: List<Task>
          steps
            let tasks = list open tasks input.available
            respond with { tasks: tasks }
          spec "available storage lists only open tasks"
            given input { available: true }
            when handled
            then responds { tasks: [{ id: "task-1", title: "Write the demo", done: false }] }
          spec "unavailable storage reports its declared failure"
            given input { available: false }
            when handled
            then fails with StoreUnavailable
        """;

    [Fact]
    [Trait("Slice", "137")]
    public async Task Correct_reply_passes_and_writes_totals_only()
    {
        // Given a cold fake model whose recorded reply is Tasks.avowry with GET /tasks/open added
        using var workspace = TempWorkspace.Create();
        var tasks = await File.ReadAllTextAsync(RepoPaths.Find("demo", "tasks", "Tasks.avowry"));
        var reply = WriteReply(workspace.Root, "correct.avowry", tasks.TrimEnd() + OpenEndpointAddition);
        var output = Path.Combine(workspace.Root, "llm-check.json");

        // When Ari runs the LLM-customer check against that reply
        var run = await RunCheck(
            "--model", "codex",
            "--model-command", PrintFileCommand(reply),
            "--out", output);

        // Then the check passes with zero warnings, every spec passing, and totals only
        Assert.True(run.ExitCode == 0, run.Stdout + Environment.NewLine + run.Stderr);
        using var json = JsonDocument.Parse(await File.ReadAllTextAsync(output));
        var root = json.RootElement;
        Assert.True(root.GetProperty("passed").GetBoolean(), await File.ReadAllTextAsync(output));
        Assert.Equal("codex", root.GetProperty("model").GetString());
        Assert.Equal("specs-passed", root.GetProperty("proof_level").GetString());
        Assert.Equal(0, root.GetProperty("build_warnings").GetInt32());
        Assert.True(root.GetProperty("specs").GetInt32() >= 13, "expected original specs plus the new endpoint");
        Assert.Equal(root.GetProperty("specs").GetInt32(), root.GetProperty("specs_passed").GetInt32());
        Assert.Equal(1, root.GetProperty("attempts").GetInt32());
        Assert.True(root.GetProperty("tokens_in").GetInt32() > 0);
        Assert.True(root.GetProperty("tokens_out").GetInt32() > 0);
        Assert.False(root.TryGetProperty("reason", out _));
        AssertTotalsOnly(root, await File.ReadAllTextAsync(output));
    }

    [Theory]
    [Trait("Slice", "137")]
    [InlineData("build")]
    [InlineData("outcome")]
    public async Task Failed_reply_names_the_reason_class_and_omits_the_reply(string reason)
    {
        // Given a fake reply that either cannot build or fails a hidden open-tasks outcome
        using var workspace = TempWorkspace.Create();
        var tasks = await File.ReadAllTextAsync(RepoPaths.Find("demo", "tasks", "Tasks.avowry"));
        var source = reason == "build"
            ? tasks.TrimEnd() + OpenEndpointAddition.Replace("return open;", "int broken = \"no\";\n            return open;", StringComparison.Ordinal)
            : tasks.TrimEnd() + OpenEndpointAddition.Replace("if (!task.Done)", "if (true)", StringComparison.Ordinal);
        var reply = WriteReply(workspace.Root, reason + ".avowry", source);
        var output = Path.Combine(workspace.Root, "llm-check.json");

        // When Ari runs the check
        var run = await RunCheck(
            "--model", "grok",
            "--model-command", PrintFileCommand(reply),
            "--out", output);

        // Then it reports passed false with that reason class and never stores the reply
        Assert.True(run.ExitCode != 0, run.Stdout + Environment.NewLine + run.Stderr);
        var text = await File.ReadAllTextAsync(output);
        using var json = JsonDocument.Parse(text);
        var root = json.RootElement;
        Assert.False(root.GetProperty("passed").GetBoolean());
        Assert.Equal(reason, root.GetProperty("reason").GetString());
        Assert.Equal("grok", root.GetProperty("model").GetString());
        Assert.Equal(1, root.GetProperty("attempts").GetInt32());
        Assert.DoesNotContain("list open tasks", text, StringComparison.Ordinal);
        Assert.DoesNotContain("int broken", text, StringComparison.Ordinal);
        Assert.DoesNotContain("GeneratedOutcomeTests", text, StringComparison.Ordinal);
        Assert.DoesNotContain("lists only open tasks", text, StringComparison.Ordinal);
        AssertTotalsOnly(root, text);
    }

    [Fact]
    [Trait("Slice", "137")]
    public async Task Profile_flags_reach_a_fake_cli_named_like_a_real_profile()
    {
        // Given a cold fake CLI named like a real Codex profile that records the arguments it received
        using var workspace = TempWorkspace.Create();
        var argvLog = Path.Combine(workspace.Root, "codex-probe-argv.json");
        var shim = WriteArgvRecordingCli(workspace.Root, "codex-probe", argvLog);
        var output = Path.Combine(workspace.Root, "llm-check.json");

        // When Ari runs the LLM-customer check through that fake CLI
        var run = await RunCheck(
            "--model", "codex",
            "--model-command", shim,
            "--out", output);

        // Then the Codex profile flags the bench appends (headless exec, skip-git, read-only sandbox) arrived
        Assert.True(File.Exists(argvLog), run.Stdout + Environment.NewLine + run.Stderr);
        var recorded = JsonDocument.Parse(await File.ReadAllTextAsync(argvLog)).RootElement.EnumerateArray()
            .Select(item => item.GetString() ?? "")
            .ToArray();
        foreach (var flag in new[] { "exec", "--skip-git-repo-check", "--sandbox", "read-only" })
            Assert.Contains(flag, recorded);
        var text = await File.ReadAllTextAsync(output);
        using var json = JsonDocument.Parse(text);
        Assert.False(json.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(1, json.RootElement.GetProperty("attempts").GetInt32());
        AssertTotalsOnly(json.RootElement, text);
    }

    [Fact]
    [Trait("Slice", "137")]
    public void Runner_unit_tests_pass()
    {
        // Given the Python checks for the LLM-customer runner
        var info = new ProcessStartInfo(PythonExecutable)
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.ArgumentList.Add("-m");
        info.ArgumentList.Add("unittest");
        info.ArgumentList.Add("tests.test_run_llm_check");

        // When Ari's gate runs those checks
        using var process = Process.Start(info) ?? throw new InvalidOperationException("Could not start python.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(180000);

        // Then every runner finding has a passing test
        Assert.True(process.ExitCode == 0, stdout + Environment.NewLine + stderr);
    }

    private static void AssertTotalsOnly(JsonElement root, string text)
    {
        foreach (var name in new[] { "reply", "reply_head", "prompt", "task", "diagnostics", "stdout", "stderr" })
            Assert.False(root.TryGetProperty(name, out _), name + " must not appear in the totals json");
        Assert.DoesNotContain("```", text, StringComparison.Ordinal);
        Assert.True(root.TryGetProperty("grammar_sha256", out var grammar) && grammar.GetString() is { Length: 64 });
        Assert.True(root.TryGetProperty("cheatsheet_sha256", out var sheet) && sheet.GetString() is { Length: 64 });
        Assert.True(root.TryGetProperty("date", out var date) && date.GetString() is { Length: > 0 });
    }

    private static string WriteReply(string root, string name, string source)
    {
        var path = Path.Combine(root, name);
        File.WriteAllText(path, source);
        return path;
    }

    private static string PythonExecutable => OperatingSystem.IsWindows() ? "python" : "python3";

    private static string PrintFileCommand(string path)
    {
        // Extra profile flags the wrapper forwards must not change which file is printed.
        return $"{PythonExecutable} -c \"import pathlib,sys; print(pathlib.Path(sys.argv[1]).read_text(encoding='utf-8'))\" \"{path}\"";
    }

    private static string WriteArgvRecordingCli(string root, string name, string argvLog)
    {
        var script = Path.Combine(root, name + ".py");
        var logLiteral = JsonSerializer.Serialize(argvLog);
        File.WriteAllText(script, $$"""
            import json, pathlib, sys
            pathlib.Path({{logLiteral}}).write_text(json.dumps(sys.argv[1:]), encoding="utf-8")
            print("not a phrase module")
            """);
        return $"{PythonExecutable} \"{script}\"";
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunCheck(params string[] extra)
    {
        var info = new ProcessStartInfo(PythonExecutable)
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        info.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" })
            info.Environment.Remove(name);
        info.ArgumentList.Add(RepoPaths.Find("scripts", "run-llm-check.py"));
        foreach (var argument in extra)
            info.ArgumentList.Add(argument);

        using var process = Process.Start(info) ?? throw new InvalidOperationException("Could not start python.");
        var pid = process.Id;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(8));
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited && process.Id == pid)
                process.Kill();
            throw new TimeoutException($"run-llm-check.py exceeded 8 minutes (pid {pid}).");
        }

        return (process.ExitCode, await stdout, await stderr);
    }
}
