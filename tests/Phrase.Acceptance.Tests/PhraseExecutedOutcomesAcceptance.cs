using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: Phrase arm scores executed outcomes for the write tasks
//   As Riley, I want the Phrase arm to compile a model's Phrase reply with the C# adapter
//   and run the write tasks' outcome cases against the generated code,
//   so that a Phrase pass means the same executed behaviour as a plain C# pass.
public class PhraseExecutedOutcomesAcceptance
{
    const string PromptHashBeforeExecutedScoring = "a7bd17c3b580099223bd99f0db9c3377ba2419a1f568448dd21fda80a7fccade";

    [Fact]
    [Trait("Slice", "098")]
    public async Task Correct_write_replies_pass_at_specs_passed_with_executed_outcomes()
    {
        // Given tasks 001 and 002 and a fake model whose recorded reply is a correct Phrase module
        var replies = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["001-write-greeting"] = """
                module Greeting

                phrase "say hello" -> Text
                  spec "proves hello"
                    when called
                    then returns "hello"
                  native csharp
                    return "hello";
                """,
            ["002-write-total"] = """
                module Totals

                phrase "double {value: Int}" -> Int
                  spec "doubles three"
                    given value = 3
                    when called
                    then returns 6
                  native csharp
                    return value * 2;
                """,
        };

        foreach (var (task, source) in replies)
        {
            using var workspace = TempWorkspace.Create();
            var suite = WriteSuite(workspace.Root, task);
            var reply = Path.Combine(workspace.Root, "reply.phrase");
            await File.WriteAllTextAsync(reply, source);

            // When the Phrase arm runs that task with executed scoring
            var run = await Run(suite, PrintFileCommand(reply), "correct-" + task, "--scoring", "executed");

            // Then the row passes at specs-passed, patched false, on the executed-outcomes bar
            var row = Assert.Single(ReadHistory(suite).Values);
            Assert.True(run.ExitCode == 0, run.Stdout + run.Stderr + row);
            using var result = JsonDocument.Parse(row);
            Assert.Equal(task, result.RootElement.GetProperty("task").GetString());
            Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
            Assert.Equal("specs-passed", result.RootElement.GetProperty("proof_level").GetString());
            Assert.Equal("executed-outcomes", result.RootElement.GetProperty("score_bar").GetString());
            Assert.False(result.RootElement.GetProperty("patched").GetBoolean());
            Assert.Equal("write", result.RootElement.GetProperty("kind").GetString());
            Assert.Equal(1, result.RootElement.GetProperty("attempts").GetInt32());
        }
    }

    [Fact]
    [Trait("Slice", "098")]
    public async Task Wrong_reply_fails_and_names_the_failing_outcome()
    {
        // Given task 001 and a reply whose body and spec disagree with the outcome case
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, "001-write-greeting");
        var outcomeName = OutcomeName(suite, "001-write-greeting");
        var reply = Path.Combine(workspace.Root, "wrong.phrase");
        await File.WriteAllTextAsync(reply, """
            module Greeting

            phrase "say hello" -> Text
              spec "proves hello"
                when called
                then returns "hello"
              native csharp
                return "goodbye";
            """);

        // When the Phrase arm scores that reply by executing the outcome cases
        var run = await Run(suite, PrintFileCommand(reply), "wrong-v1", "--scoring", "executed");

        // Then the attempt fails and the stored feedback names the outcome and the generated spec
        var row = Assert.Single(ReadHistory(suite).Values);
        Assert.True(run.ExitCode == 3, run.Stdout + run.Stderr + row);
        using var result = JsonDocument.Parse(row);
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.NotEqual("specs-passed", result.RootElement.GetProperty("proof_level").GetString());
        var diagnostics = result.RootElement.GetProperty("diagnostics").EnumerateArray().Select(item => item.GetString()!).ToArray();
        Assert.Contains(diagnostics, text => text.Contains(outcomeName, StringComparison.Ordinal));
        Assert.Contains(diagnostics, text => text.Contains("proves hello", StringComparison.Ordinal));
        Assert.All(diagnostics, text => Assert.True(text.Length <= 600, text));
    }

    [Fact]
    [Trait("Slice", "098")]
    public void Phrase_task_outcomes_match_the_csharp_twin_byte_for_byte()
    {
        // Given the C# twin is the single source of a write task's outcome cases
        foreach (var task in new[] { "001-write-greeting", "002-write-total" })
        {
            var twinPath = RepoPaths.Find("bench", "csharp-baseline", "tasks", task, "outcomes.json");
            var copyPath = RepoPaths.Find("bench", "tasks", task, "outcomes.json");

            // When the Phrase task folder is checked against that twin
            // Then a missing or drifted copy fails naming the task
            Assert.True(File.Exists(copyPath), $"{task}: outcomes.json is missing");
            var twin = File.ReadAllBytes(twinPath);
            var copy = File.ReadAllBytes(copyPath);
            Assert.True(twin.AsSpan().SequenceEqual(copy), $"{task}: outcomes.json drifted from the C# twin");
        }
    }

    [Fact]
    [Trait("Slice", "098")]
    public async Task Default_run_keeps_prompt_hash_launch_arguments_and_parse_resolve_rows()
    {
        // Given the greeting task, its outcomes file, and the launch recorded before executed scoring
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, "001-write-greeting");
        var prompt = await File.ReadAllTextAsync(Path.Combine(Path.GetDirectoryName(suite)!, "tasks", "001-write-greeting", "prompt.md"));
        Assert.Equal(PromptHashBeforeExecutedScoring, Sha256Hex(prompt));
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["codex-default-v1"] = "exec --skip-git-repo-check",
            ["grok-default-v1"] = "--max-turns 10 --prompt-file ",
        };

        foreach (var (model, launch) in expected)
        {
            var argv = Path.Combine(workspace.Root, model + "-argv.txt");
            var stdin = Path.Combine(workspace.Root, model + "-stdin.txt");
            var command = WriteLaunchShim(workspace.Root, model, argv, stdin);

            // When the arm runs with no scoring option
            var run = await Run(suite, command, model);

            // Then the prompt hash, launch arguments and parse-resolve row are unchanged
            Assert.True(run.ExitCode == 0, run.Stdout + run.Stderr);
            Assert.Equal(prompt, await File.ReadAllTextAsync(stdin));
            var arguments = (await File.ReadAllTextAsync(argv)).Trim();
            if (model.StartsWith("codex", StringComparison.Ordinal))
                Assert.Equal(launch, arguments);
            else
                Assert.Matches(new Regex("^" + Regex.Escape(launch) + "\".*prompt\\.txt\"$", RegexOptions.CultureInvariant), arguments);
            Assert.DoesNotContain("--scoring", arguments, StringComparison.Ordinal);
            using var result = JsonDocument.Parse(ReadHistory(suite)["001-write-greeting"]);
            Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
            Assert.Equal("parse-resolve", result.RootElement.GetProperty("proof_level").GetString());
            Assert.Equal(PromptHashBeforeExecutedScoring, result.RootElement.GetProperty("prompt_hash").GetString());
            Assert.False(result.RootElement.TryGetProperty("score_bar", out _));
            Assert.False(result.RootElement.TryGetProperty("patched", out _));
            Assert.False(result.RootElement.TryGetProperty("kind", out _));
        }
    }

    private static string WriteSuite(string root, params string[] taskIds)
    {
        var suiteDirectory = Path.Combine(root, "bench");
        Directory.CreateDirectory(suiteDirectory);
        var tasks = new List<string>();
        foreach (var taskId in taskIds)
        {
            var source = RepoPaths.Find("bench", "tasks", taskId);
            var destination = Path.Combine(suiteDirectory, "tasks", taskId);
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
            var twinOutcomes = RepoPaths.Find("bench", "csharp-baseline", "tasks", taskId, "outcomes.json");
            var outcomes = Path.Combine(destination, "outcomes.json");
            if (!File.Exists(outcomes))
                File.Copy(twinOutcomes, outcomes);
            tasks.Add($$"""{"id":"{{taskId}}","kind":"write","prompt":"tasks/{{taskId}}/prompt.md","holdout":false}""");
        }

        var suite = Path.Combine(suiteDirectory, "suite.json");
        File.WriteAllText(suite, "{\"version\":1,\"tasks\":[" + string.Join(",", tasks) + "]}");
        return suite;
    }

    private static string OutcomeName(string suite, string taskId)
    {
        var path = Path.Combine(Path.GetDirectoryName(suite)!, "tasks", taskId, "outcomes.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement[0].GetProperty("name").GetString()!;
    }

    private static string WriteLaunchShim(string root, string model, string argv, string stdin)
    {
        var script = Path.Combine(root, model + (OperatingSystem.IsWindows() ? ".cmd" : ".sh"));
        if (OperatingSystem.IsWindows())
        {
            File.WriteAllText(script, $"""
                @echo off
                python -c "import sys, pathlib; pathlib.Path(r'{stdin}').write_bytes(sys.stdin.buffer.read())"
                echo(%*>"{argv}"
                echo module Greeting
                echo.
                echo phrase "say hello"
                echo   native csharp
                echo     return;
                """);
            return $"call \"{script}\"";
        }

        File.WriteAllText(script, $"""
            #!/bin/sh
            cat > '{stdin}'
            printf '%s\n' "$*" > '{argv}'
            cat <<'PHRASE'
            module Greeting

            phrase "say hello"
              native csharp
                return;
            PHRASE
            """);
        return $"sh \"{script}\"";
    }

    private static string PrintFileCommand(string path) =>
        OperatingSystem.IsWindows()
            ? $"more > nul && type \"{path}\""
            : $"cat > /dev/null && cat \"{path}\"";

    private static async Task<(int ExitCode, string Stdout, string Stderr)> Run(
        string suite, string command, string model, params string[] extra)
    {
        var project = RepoPaths.Find("tools", "Phrase.Bench", "Phrase.Bench.csproj");
        var info = new System.Diagnostics.ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        // A reused build server inherits this pipe and keeps ReadToEnd from finishing.
        info.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        info.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        foreach (var argument in new[] { "run", "--disable-build-servers", "--project", project, "--", "run", "--suite", suite, "--model", model, "--model-command", command })
            info.ArgumentList.Add(argument);
        foreach (var argument in extra)
            info.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(info)!;
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    private static Dictionary<string, string> ReadHistory(string suite)
    {
        var history = Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl");
        var rows = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in File.ReadAllLines(history))
        {
            using var document = JsonDocument.Parse(line);
            rows[document.RootElement.GetProperty("task").GetString()!] = line;
        }

        return rows;
    }

    private static string Sha256Hex(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}
