using System.Diagnostics;
using System.Text.Json;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: Holdout run rows stay private
//   As Steve, I want a holdout run to store its rows outside the public repository and publish only the numbers,
//   so that sealed tasks and the replies they produced never appear in public history.
public sealed class PrivateHoldoutResults
{
    private const string ReplyMarker = "REPLYMARKER-113-ZEBRA";
    private const string PromptMarker = "PROMPTTEXT-113-QUILL";
    private const string TaskId = "H9";
    private const string HoldoutLabel = "holdout-private-label";

    [Fact]
    [Trait("Slice", "113")]
    public async Task private_run_leaves_committed_history_and_publishes_numbers_only()
    {
        // Given invented tasks, fake models whose replies carry a private marker, and the committed history files
        using var workspace = TempWorkspace.Create();
        var phraseSuite = WritePhraseSuite(workspace.Root);
        var csharpSuite = WriteCSharpSuite(workspace.Root);
        var privateRows = Path.Combine(workspace.Root, "private", "rows.jsonl");
        var phraseReply = WriteReply(workspace.Root, "phrase-reply.txt", PhraseReply);
        var csharpReply = WriteReply(workspace.Root, "csharp-reply.txt", ReplyMarker + "\nnot csharp source\n");
        var phrasePrivateCapture = Path.Combine(workspace.Root, "phrase-private-capture.txt");
        var phraseDefaultCapture = Path.Combine(workspace.Root, "phrase-default-capture.txt");
        var csharpPrivateCapture = Path.Combine(workspace.Root, "csharp-private-capture.txt");
        var csharpDefaultCapture = Path.Combine(workspace.Root, "csharp-default-capture.txt");
        var committedBefore = SnapshotCommittedHistories();

        // When both arms run with a private results path
        var phrasePrivate = await RunArm("Phrase.Bench", phraseSuite, "fixture-phrase-v1", WriteFakeModel(workspace.Root, "phrase-private.cmd", "phrase-private.sh", phraseReply, phrasePrivateCapture), privateRows);
        var csharpPrivate = await RunArm("Phrase.BenchCs", csharpSuite, "fixture-csharp-v1", WriteFakeModel(workspace.Root, "csharp-private.cmd", "csharp-private.sh", csharpReply, csharpPrivateCapture), privateRows);

        // Then the committed history files stay byte-identical and the private file holds the rows
        Assert.True(phrasePrivate.ExitCode == 0, phrasePrivate.Stdout + phrasePrivate.Stderr);
        Assert.True(csharpPrivate.ExitCode == 3, csharpPrivate.Stdout + csharpPrivate.Stderr);
        AssertCommittedHistoriesUnchanged(committedBefore);
        Assert.False(File.Exists(HistoryPath(phraseSuite)));
        Assert.False(File.Exists(HistoryPath(csharpSuite)));

        var privateText = File.ReadAllText(privateRows);
        var privateLines = privateText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, privateLines.Length);
        Assert.Contains(ReplyMarker, privateText, StringComparison.Ordinal);
        Assert.Contains(TaskId, privateText, StringComparison.Ordinal);
        Assert.Contains(HoldoutLabel, privateText, StringComparison.Ordinal);

        // When the same holdout-looking arms run again without the private path
        var phraseDefault = await RunArm("Phrase.Bench", phraseSuite, "fixture-phrase-v1", WriteFakeModel(workspace.Root, "phrase-default.cmd", "phrase-default.sh", phraseReply, phraseDefaultCapture), privateResults: null);
        var csharpDefault = await RunArm("Phrase.BenchCs", csharpSuite, "fixture-csharp-v1", WriteFakeModel(workspace.Root, "csharp-default.cmd", "csharp-default.sh", csharpReply, csharpDefaultCapture), privateResults: null);

        // Then both arms refuse before launching a model or publishing any task-derived row
        Assert.NotEqual(0, phraseDefault.ExitCode);
        Assert.NotEqual(0, csharpDefault.ExitCode);
        Assert.Contains("holdout", phraseDefault.Stderr, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--private-results", phraseDefault.Stderr, StringComparison.Ordinal);
        Assert.Contains("holdout", csharpDefault.Stderr, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--private-results", csharpDefault.Stderr, StringComparison.Ordinal);
        Assert.DoesNotContain(PromptMarker, phraseDefault.Stdout + phraseDefault.Stderr, StringComparison.Ordinal);
        Assert.DoesNotContain(PromptMarker, csharpDefault.Stdout + csharpDefault.Stderr, StringComparison.Ordinal);
        Assert.False(File.Exists(phraseDefaultCapture));
        Assert.False(File.Exists(csharpDefaultCapture));
        AssertCommittedHistoriesUnchanged(committedBefore);
        Assert.Equal(2, File.ReadAllText(privateRows).Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.False(File.Exists(HistoryPath(phraseSuite)));
        Assert.False(File.Exists(HistoryPath(csharpSuite)));

        var phrasePrivateRow = JsonDocument.Parse(privateLines.Single(line => line.Contains("fixture-phrase-v1", StringComparison.Ordinal)));
        var csharpPrivateRow = JsonDocument.Parse(privateLines.Single(line => line.Contains("fixture-csharp-v1", StringComparison.Ordinal)));

        var summary = Path.Combine(workspace.Root, "summary.json");
        var summarized = RunPython(RepoPaths.Find("scripts", "summarize-private-results.py"), privateRows, "--out", summary);
        Assert.True(summarized.ExitCode == 0, summarized.Stdout + summarized.Stderr);
        AssertNumbersOnly(summary, phrasePrivateRow.RootElement.GetProperty("prompt_hash").GetString()!);
        using var live = JsonDocument.Parse(File.ReadAllText(summary));
        var phraseGroup = Group(live, "phrase", "fixture-phrase-v1");
        var csharpGroup = Group(live, "csharp", "fixture-csharp-v1");
        Assert.Equal("", phraseGroup.GetProperty("kind").GetString());
        Assert.Equal(1, phraseGroup.GetProperty("count").GetInt32());
        Assert.Equal(1, phraseGroup.GetProperty("completed_count").GetInt32());
        Assert.Equal(0, phraseGroup.GetProperty("unavailable_count").GetInt32());
        Assert.Equal(1d, phraseGroup.GetProperty("pass_rate").GetDouble(), 6);
        Assert.Equal(1d, phraseGroup.GetProperty("mean_attempts").GetDouble(), 6);
        Assert.True(phraseGroup.GetProperty("mean_total_tokens").GetDouble() > 0);
        Assert.False(phraseGroup.TryGetProperty("documentation_tokens_share", out _));
        Assert.Equal("write", csharpGroup.GetProperty("kind").GetString());
        Assert.Equal(1, csharpGroup.GetProperty("count").GetInt32());
        Assert.Equal(1, csharpGroup.GetProperty("completed_count").GetInt32());
        Assert.Equal(0, csharpGroup.GetProperty("unavailable_count").GetInt32());
        Assert.Equal(0d, csharpGroup.GetProperty("pass_rate").GetDouble(), 6);
        Assert.Equal(3d, csharpGroup.GetProperty("mean_attempts").GetDouble(), 6);
        Assert.True(csharpGroup.GetProperty("mean_total_tokens").GetDouble() > 0);
        Assert.False(csharpGroup.TryGetProperty("documentation_tokens_share", out _));

        var docsRows = Path.Combine(workspace.Root, "private", "docs-rows.jsonl");
        File.WriteAllText(docsRows, DocsRows);
        var docsSummary = Path.Combine(workspace.Root, "docs-summary.json");
        var docsRun = RunPython(RepoPaths.Find("scripts", "summarize-private-results.py"), docsRows, "--out", docsSummary);
        Assert.True(docsRun.ExitCode == 0, docsRun.Stdout + docsRun.Stderr);
        AssertNumbersOnly(docsSummary, "unused-prompt-hash");
        using var docs = JsonDocument.Parse(File.ReadAllText(docsSummary));
        var docsPhrase = Group(docs, "phrase", "fixture-phrase-v1");
        var docsCSharp = Group(docs, "csharp", "fixture-csharp-v1");
        Assert.Equal(2, docsPhrase.GetProperty("count").GetInt32());
        Assert.Equal(0.5, docsPhrase.GetProperty("pass_rate").GetDouble(), 6);
        Assert.Equal(2d, docsPhrase.GetProperty("mean_attempts").GetDouble(), 6);
        Assert.Equal(60d, docsPhrase.GetProperty("mean_total_tokens").GetDouble(), 6);
        Assert.Equal(0.25, docsPhrase.GetProperty("documentation_tokens_share").GetDouble(), 6);
        Assert.False(docsCSharp.TryGetProperty("documentation_tokens_share", out _));
    }

    [Fact]
    [Trait("Slice", "113")]
    public async Task git_ignored_suite_refuses_before_the_model_is_launched()
    {
        // Given an invented non-H task in a git-ignored temp folder inside the repository
        var root = Path.Combine(RepoPaths.Root, "tests", "Phrase.Acceptance.Tests", "bin", "slice-113-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var suite = WritePhraseSuite(root, "development-task");
            var reply = WriteReply(root, "reply.txt", PhraseReply);
            var capture = Path.Combine(root, "model-started.txt");
            var committedBefore = SnapshotCommittedHistories();

            // When the Phrase arm is run without --private-results
            var result = await RunArm("Phrase.Bench", suite, "fixture-phrase-v1", WriteFakeModel(root, "ignored.cmd", "ignored.sh", reply, capture), privateResults: null);

            // Then the ignored-path rule refuses without exposing task text or launching the model
            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("git-ignored", result.Stderr, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("--private-results", result.Stderr, StringComparison.Ordinal);
            Assert.DoesNotContain(PromptMarker, result.Stdout + result.Stderr, StringComparison.Ordinal);
            Assert.False(File.Exists(capture));
            AssertCommittedHistoriesUnchanged(committedBefore);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    [Trait("Slice", "113")]
    public void repository_check_passes_here_and_fails_on_a_tracked_holdout_path()
    {
        // Given the repository check wired with the other public-repo checks
        var hook = File.ReadAllText(RepoPaths.Find(".githooks", "pre-commit"));
        Assert.Contains("scripts/check-no-holdout-in-tracked-files.py", hook, StringComparison.Ordinal);

        // When it runs on this repo and on a temp git repo that tracks a holdout path
        var suite = RunPython("-m", "unittest", "tests.test_no_holdout_in_tracked_files");

        // Then this repo passes and the temp repo fails
        Assert.True(suite.ExitCode == 0, suite.Stdout + suite.Stderr);
    }

    private static void AssertNumbersOnly(string summaryPath, string promptHash)
    {
        var text = File.ReadAllText(summaryPath);
        Assert.DoesNotContain(ReplyMarker, text, StringComparison.Ordinal);
        Assert.DoesNotContain(PromptMarker, text, StringComparison.Ordinal);
        Assert.DoesNotContain(TaskId, text, StringComparison.Ordinal);
        Assert.DoesNotContain(HoldoutLabel, text, StringComparison.Ordinal);
        Assert.DoesNotContain("reply_head", text, StringComparison.Ordinal);
        Assert.DoesNotContain("prompt_hash", text, StringComparison.Ordinal);
        Assert.DoesNotContain(promptHash, text, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(text);
        Assert.Equal(new[] { "groups" }, document.RootElement.EnumerateObject().Select(property => property.Name).ToArray());
        var allowed = new HashSet<string>
        {
            "arm", "model", "kind", "count", "completed_count", "unavailable_count", "pass_rate", "mean_attempts", "mean_total_tokens", "documentation_tokens_share",
        };
        foreach (var group in document.RootElement.GetProperty("groups").EnumerateArray())
        {
            foreach (var property in group.EnumerateObject())
                Assert.Contains(property.Name, allowed);
        }
    }

    private static JsonElement Group(JsonDocument document, string arm, string model)
    {
        var matches = document.RootElement.GetProperty("groups").EnumerateArray()
            .Where(group => group.GetProperty("arm").GetString() == arm && group.GetProperty("model").GetString() == model)
            .ToArray();
        return Assert.Single(matches);
    }

    private static Dictionary<string, byte[]> SnapshotCommittedHistories() =>
        CommittedHistories.ToDictionary(path => path, File.ReadAllBytes);

    private static void AssertCommittedHistoriesUnchanged(Dictionary<string, byte[]> before)
    {
        foreach (var (path, bytes) in before)
            Assert.Equal(bytes, File.ReadAllBytes(path));
    }

    private static string HistoryPath(string suite) =>
        Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl");

    private static string WritePhraseSuite(string root, string taskId = TaskId)
    {
        var suiteRoot = Path.Combine(root, "phrase-suite");
        var taskRoot = Path.Combine(suiteRoot, "tasks", taskId);
        Directory.CreateDirectory(taskRoot);
        File.WriteAllText(Path.Combine(taskRoot, "prompt.md"), PromptMarker + "\nReturn a Phrase module that says hello.\n");
        File.WriteAllText(Path.Combine(suiteRoot, "suite.json"), SuiteJson(includeOutcomes: false, taskId));
        return Path.Combine(suiteRoot, "suite.json");
    }

    private static string WriteCSharpSuite(string root)
    {
        var suiteRoot = Path.Combine(root, "csharp-suite");
        var taskRoot = Path.Combine(suiteRoot, "tasks", TaskId);
        Directory.CreateDirectory(taskRoot);
        File.WriteAllText(Path.Combine(taskRoot, "prompt.md"), PromptMarker + "\nReturn C# that says hello.\n");
        File.WriteAllText(Path.Combine(taskRoot, "outcomes.json"), """
            [{"name":"says hello","given":{},"when":"called","then":{"returns":"hello"}}]
            """);
        File.WriteAllText(Path.Combine(suiteRoot, "suite.json"), SuiteJson(includeOutcomes: true, TaskId));
        return Path.Combine(suiteRoot, "suite.json");
    }

    private static string SuiteJson(bool includeOutcomes, string taskId)
    {
        var outcomes = includeOutcomes ? $",\"outcomes\":\"tasks/{taskId}/outcomes.json\"" : "";
        return "{\"version\":1,\"tasks\":[{\"id\":\"" + taskId + "\",\"kind\":\"write\",\"prompt\":\"tasks/" + taskId + "/prompt.md\"" + outcomes + ",\"holdout\":false}]}";
    }

    private static string WriteReply(string root, string name, string contents)
    {
        var path = Path.Combine(root, name);
        File.WriteAllText(path, contents);
        return path;
    }

    private static string WriteFakeModel(string root, string windowsName, string unixName, string replyPath, string capturePath)
    {
        var helper = Path.Combine(root, "fake_model.py");
        if (!File.Exists(helper))
        {
            File.WriteAllText(helper, """
                import hashlib, pathlib, sys
                capture, reply, *rest = sys.argv[1:]
                digest = hashlib.sha256(sys.stdin.buffer.read()).hexdigest()
                with pathlib.Path(capture).open("a", encoding="utf-8") as handle:
                    handle.write(" ".join(rest) + "\t" + digest + "\n")
                sys.stdout.buffer.write(pathlib.Path(reply).read_bytes())
                """);
        }

        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(root, windowsName);
            File.WriteAllText(script, "@echo off\r\n" +
                $"python \"{helper}\" \"{capturePath}\" \"{replyPath}\" %*\r\n");
            return $"call \"{script}\"";
        }

        var shell = Path.Combine(root, unixName);
        File.WriteAllText(shell, "#!/bin/sh\n" +
            $"python \"{helper}\" \"{capturePath}\" \"{replyPath}\" \"$@\"\n");
        return $"sh \"{shell}\"";
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunArm(
        string project, string suite, string model, string command, string? privateResults)
    {
        var projectPath = RepoPaths.Find("tools", project, project + ".csproj");
        var info = new ProcessStartInfo("dotnet")
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
        foreach (var argument in new[]
        {
            "run", "--disable-build-servers", "--project", projectPath, "--",
            "run", "--suite", suite, "--model", model, "--model-command", command,
            "--label", HoldoutLabel, "--timeout-ms", "60000",
        })
            info.ArgumentList.Add(argument);
        if (privateResults is not null)
        {
            info.ArgumentList.Add("--private-results");
            info.ArgumentList.Add(privateResults);
        }

        using var process = Process.Start(info)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await stdout, await stderr);
    }

    private static (int ExitCode, string Stdout, string Stderr) RunPython(params string[] arguments)
    {
        var info = new ProcessStartInfo("python")
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        foreach (var argument in arguments)
            info.ArgumentList.Add(argument);
        using var process = Process.Start(info)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }

    private static readonly string[] CommittedHistories =
    {
        RepoPaths.Find("bench", "results", "history.jsonl"),
        RepoPaths.Find("bench", "csharp-baseline", "results", "history.jsonl"),
    };

    private const string PhraseReply = ReplyMarker + """

        module Greeting

        phrase "say hello"
          native csharp
            return;

        """;

    private const string DocsRows =
        "{\"model\":\"fixture-phrase-v1\",\"task\":\"" + TaskId + "\",\"status\":\"completed\",\"passed\":true,\"attempts\":1,\"tokens_in\":60,\"tokens_out\":40,\"reply_head\":\"" + ReplyMarker + "\",\"prompt\":\"" + PromptMarker + "\",\"label\":\"" + HoldoutLabel + "\",\"mode\":\"docs-in-context\",\"kind\":\"write\",\"documentation_tokens\":25}\n"
        + "{\"model\":\"fixture-phrase-v1\",\"task\":\"H2\",\"status\":\"completed\",\"passed\":false,\"attempts\":3,\"tokens_in\":20,\"tokens_out\":0,\"reply_head\":\"" + ReplyMarker + " extra reply text\",\"prompt\":\"" + PromptMarker + "\",\"label\":\"" + HoldoutLabel + "\",\"mode\":\"docs-in-context\",\"kind\":\"write\",\"documentation_tokens\":5}\n"
        + "{\"model\":\"fixture-csharp-v1\",\"task\":\"" + TaskId + "\",\"status\":\"completed\",\"passed\":false,\"attempts\":3,\"tokens_in\":10,\"tokens_out\":10,\"reply_head\":\"" + ReplyMarker + "\",\"label\":\"" + HoldoutLabel + "\",\"mode\":\"plain-csharp\",\"kind\":\"write\",\"documentation_tokens\":10}\n";
}
