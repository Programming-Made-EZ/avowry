using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

public sealed class NewBenchRowsCarryNoReply
{
    private const string PhraseHistorySha = "6fcd386156464c405c34cc6e7ae68847e45213c63eac3e8647c3a8dfcc6f7202";
    private const string CSharpHistorySha = "525b4574e4f8c87fac364cd200b305de947ebb4b51efbca1449699116aafbac3";
    private const int PhrasePushedRows = 336;
    private const int CSharpPushedRows = 120;
    private const string Marker3681 = "MARKER-368-1";
    private const string Tail3681 = "TAIL-368-1-PAST-200";
    private const string MarkerPhrasePass = "MARKER-368-2A-PASS";
    private const string MarkerPhraseFail = "MARKER-368-2A-FAIL";
    private const string MarkerPhraseErr = "MARKER-368-2A-ERR";
    private const string MarkerCSharpPass = "MARKER-368-2B-PASS";
    private const string MarkerCSharpFail = "MARKER-368-2B-FAIL";
    private const string MarkerCSharpErr = "MARKER-368-2B-ERR";
    private const string PassingPhrase = """
        module Greeting

        avow to "say hello"
          native csharp
            return;
        """;
    private const string PassingCSharp = """
        // FILE: Greeter.cs
        namespace Greeting; public static class Greeter { public static string SayHello() => "hello"; }
        // FILE: GreeterTests.cs
        using Greeting;
        public sealed class GreeterTests { [Xunit.Fact] public void Greeter_returns_hello() => Xunit.Assert.Equal("hello", Greeter.SayHello()); }
        """;
    private const string OneCase = "[{\"name\":\"returns hello\",\"given\":{},\"when\":\"called\",\"then\":{\"returns\":\"hello\"}}]";

    [Fact]
    [Trait("Slice", "368")]
    [Trait("Criterion", "368-1")]
    public async Task A_completed_run_saves_its_scores_and_no_reply_text()
    {
        // Given the Phrase bench runs one task and the model replies with a passing program longer than 200 characters that starts with a known marker
        using var workspace = TempWorkspace.Create();
        var suite = WritePhraseSuite(workspace.Root);
        var reply = PassingPhraseReplyOver200(Marker3681, Tail3681);
        var replyPath = WriteReply(workspace.Root, "pass.avowry", reply);
        var model = "slice-368-1-v1";
        var label = "slice-368-1";
        var prompt = File.ReadAllText(Path.Combine(Path.GetDirectoryName(suite)!, "tasks", "001-write-greeting", "prompt.md"));

        // When the bench saves the result
        var run = await RunArm("Avowry.Bench", suite, model, PrintFileCommand(replyPath), HistoryPath(suite), label);

        // Then the new line in results/history.jsonl and the file results/<model>.json both have reply_head empty or absent, the marker appears nowhere in either, and model, task, passed, attempts, tokens_in, tokens_out, status, proof_level and label hold the values the run produced
        Assert.True(run.ExitCode == 0, run.Stdout + run.Stderr);
        var modelJson = await File.ReadAllTextAsync(ModelPath(suite, model));
        var historyLine = Assert.Single(JsonlLines(await File.ReadAllBytesAsync(HistoryPath(suite))));
        AssertNoReply(modelJson, Marker3681, Tail3681);
        AssertNoReply(historyLine, Marker3681, Tail3681);
        Assert.Equal("", HeadOf(modelJson));
        Assert.Equal("", HeadOf(historyLine));
        using var stored = JsonDocument.Parse(modelJson);
        using var history = JsonDocument.Parse(historyLine);
        AssertScoreFields(stored.RootElement, model, "001-write-greeting", passed: true, attempts: 1,
            tokensIn: EstimateTokens(prompt), tokensOut: EstimateTokens(reply), status: "completed",
            proofLevel: "parse-resolve", label: label);
        AssertScoreFields(history.RootElement, model, "001-write-greeting", passed: true, attempts: 1,
            tokensIn: EstimateTokens(prompt), tokensOut: EstimateTokens(reply), status: "completed",
            proofLevel: "parse-resolve", label: label);
    }

    [Fact]
    [Trait("Slice", "368")]
    [Trait("Criterion", "368-2")]
    public async Task Every_ending_of_the_Phrase_bench_saves_no_reply_text()
    {
        // Given the Phrase bench run four times: a passing reply, a reply that fails on every attempt, an empty reply, and a model that exits with an error after an earlier attempt replied with a known marker
        using var workspace = TempWorkspace.Create();
        var suite = WritePhraseSuite(workspace.Root);
        var passPath = WriteReply(workspace.Root, "pass.avowry", MarkerPhrasePass + "\n" + PassingPhrase);
        var failPath = WriteReply(workspace.Root, "fail.txt", MarkerPhraseFail + " this is prose, not a phrase program.");
        var emptyPath = WriteReply(workspace.Root, "empty.txt", string.Empty);
        var errPath = WriteReply(workspace.Root, "err.txt", MarkerPhraseErr + " this is prose, not a phrase program.");

        // When each bench saves its rows
        var passRun = await RunArm("Avowry.Bench", suite, "phrase-368-2-pass", PrintFileCommand(passPath), HistoryPath(suite));
        var failRun = await RunArm("Avowry.Bench", suite, "phrase-368-2-fail", PrintFileCommand(failPath), HistoryPath(suite));
        var emptyRun = await RunArm("Avowry.Bench", suite, "phrase-368-2-empty", PrintFileCommand(emptyPath), HistoryPath(suite));
        var errRun = await RunArm("Avowry.Bench", suite, "phrase-368-2-err", WriteAttemptThenExitCommand(workspace.Root, errPath, 7), HistoryPath(suite));

        // Then none of the history lines or model files contains any part of any reply, reply_head is empty or absent in every one, and status and error still read completed, completed, unavailable/"empty reply" and unavailable/<the error>
        Assert.True(passRun.ExitCode == 0, passRun.Stdout + passRun.Stderr);
        Assert.True(failRun.ExitCode == 3, failRun.Stdout + failRun.Stderr);
        Assert.True(emptyRun.ExitCode == 0, emptyRun.Stdout + emptyRun.Stderr);
        Assert.True(errRun.ExitCode == 0, errRun.Stdout + errRun.Stderr);
        await AssertAllEndings(
            () => AssertEnding(suite, "phrase-368-2-pass", "completed", marker: MarkerPhrasePass),
            () => AssertEnding(suite, "phrase-368-2-fail", "completed", marker: MarkerPhraseFail),
            () => AssertEnding(suite, "phrase-368-2-empty", "unavailable", error: "empty reply"),
            () => AssertEnding(suite, "phrase-368-2-err", "unavailable", errorContains: "exited 7", marker: MarkerPhraseErr));
    }

    [Fact]
    [Trait("Slice", "368")]
    [Trait("Criterion", "368-2")]
    public async Task Every_ending_of_the_C_sharp_baseline_saves_no_reply_text()
    {
        // Given the C# baseline bench run four times: a passing reply, a reply that fails on every attempt, an empty reply, and a model that exits with an error after an earlier attempt replied with a known marker
        using var workspace = TempWorkspace.Create();
        var suite = WriteCSharpSuite(workspace.Root);
        var passPath = WriteReply(workspace.Root, "pass.cs", "// " + MarkerCSharpPass + " this is prose\n" + PassingCSharp);
        var failPath = WriteReply(workspace.Root, "fail.cs",
            "// " + MarkerCSharpFail + " this is prose\n" +
            "namespace Greeting; public static class Greeter { public static string SayHello() => \"goodbye\"; }\n" +
            "public sealed class GreeterTests { [Xunit.Fact] public void Greeter_returns_hello() => Xunit.Assert.Equal(\"hello\", Greeter.SayHello()); }\n");
        var emptyPath = WriteReply(workspace.Root, "empty.txt", string.Empty);
        var errPath = WriteReply(workspace.Root, "err.txt", MarkerCSharpErr + "\nnot csharp source\n");

        // When each bench saves its rows
        var passRun = await RunArm("Avowry.BenchCs", suite, "csharp-368-2-pass", PrintFileCommand(passPath), HistoryPath(suite), timeoutMs: 180000);
        var failRun = await RunArm("Avowry.BenchCs", suite, "csharp-368-2-fail", PrintFileCommand(failPath), HistoryPath(suite), timeoutMs: 180000);
        var emptyRun = await RunArm("Avowry.BenchCs", suite, "csharp-368-2-empty", PrintFileCommand(emptyPath), HistoryPath(suite), timeoutMs: 180000);
        var errRun = await RunArm("Avowry.BenchCs", suite, "csharp-368-2-err", WriteAttemptThenExitCommand(workspace.Root, errPath, 7), HistoryPath(suite), timeoutMs: 180000);

        // Then none of the history lines or model files contains any part of any reply, reply_head is empty or absent in every one, and status and error still read completed, completed, unavailable/"empty reply" and unavailable/<the error>
        Assert.True(passRun.ExitCode == 0, passRun.Stdout + passRun.Stderr);
        Assert.True(failRun.ExitCode == 3, failRun.Stdout + failRun.Stderr);
        Assert.True(emptyRun.ExitCode == 0, emptyRun.Stdout + emptyRun.Stderr);
        Assert.True(errRun.ExitCode == 0, errRun.Stdout + errRun.Stderr);
        await AssertAllEndings(
            () => AssertEnding(suite, "csharp-368-2-pass", "completed", marker: MarkerCSharpPass),
            () => AssertEnding(suite, "csharp-368-2-fail", "completed", marker: MarkerCSharpFail),
            () => AssertEnding(suite, "csharp-368-2-empty", "unavailable", error: "empty reply"),
            () => AssertEnding(suite, "csharp-368-2-err", "unavailable", errorContains: "exited 7", marker: MarkerCSharpErr));
    }

    [Fact]
    [Trait("Slice", "368")]
    [Trait("Criterion", "368-3")]
    public async Task Pushed_rows_stay_byte_identical_and_still_read_when_a_new_row_is_appended()
    {
        // Given bench/results/history.jsonl with its 336 pushed rows and bench/csharp-baseline/results/history.jsonl with its 120, copied into a temp workspace
        var phraseOriginal = File.ReadAllBytes(RepoPaths.Find("bench", "results", "history.jsonl"));
        var csharpOriginal = File.ReadAllBytes(RepoPaths.Find("bench", "csharp-baseline", "results", "history.jsonl"));
        Assert.Equal(PhraseHistorySha, Sha256Hex(PrefixThroughNthNewline(phraseOriginal, PhrasePushedRows)));
        Assert.Equal(CSharpHistorySha, Sha256Hex(PrefixThroughNthNewline(csharpOriginal, CSharpPushedRows)));

        using var workspace = TempWorkspace.Create();
        var phraseCopy = Path.Combine(workspace.Root, "phrase-history.jsonl");
        var csharpCopy = Path.Combine(workspace.Root, "csharp-history.jsonl");
        File.WriteAllBytes(phraseCopy, phraseOriginal);
        File.WriteAllBytes(csharpCopy, csharpOriginal);
        var phraseSuite = WritePhraseSuite(workspace.Root);
        var csharpSuite = WriteCSharpSuite(workspace.Root);
        var phraseReply = WriteReply(workspace.Root, "phrase-pass.avowry", PassingPhrase);
        var csharpReply = WriteReply(workspace.Root, "csharp-pass.cs", PassingCSharp);

        // When each bench appends a new row after this change
        var phraseRun = await RunArm("Avowry.Bench", phraseSuite, "slice-368-3-phrase", PrintFileCommand(phraseReply), phraseCopy);
        var csharpRun = await RunArm("Avowry.BenchCs", csharpSuite, "slice-368-3-csharp", PrintFileCommand(csharpReply), csharpCopy, timeoutMs: 180000);

        // Then the pushed lines still hash to the recorded values, and the stored-result reader reads every old row and the new row with the model, task, passed and status that the line holds
        Assert.True(phraseRun.ExitCode == 0, phraseRun.Stdout + phraseRun.Stderr);
        Assert.True(csharpRun.ExitCode == 0, csharpRun.Stdout + csharpRun.Stderr);
        var phraseAfter = File.ReadAllBytes(phraseCopy);
        var csharpAfter = File.ReadAllBytes(csharpCopy);
        Assert.Equal(PhraseHistorySha, Sha256Hex(PrefixThroughNthNewline(phraseAfter, PhrasePushedRows)));
        Assert.Equal(CSharpHistorySha, Sha256Hex(PrefixThroughNthNewline(csharpAfter, CSharpPushedRows)));

        var phraseLines = JsonlLines(phraseAfter);
        var csharpLines = JsonlLines(csharpAfter);
        var phraseOriginalLines = JsonlLines(phraseOriginal);
        var csharpOriginalLines = JsonlLines(csharpOriginal);
        Assert.Equal(phraseOriginalLines.Length + 1, phraseLines.Length);
        Assert.Equal(csharpOriginalLines.Length + 1, csharpLines.Length);
        for (var i = 0; i < PhrasePushedRows; i++)
            AssertPhraseReaderAgrees(phraseLines[i], phraseOriginalLines[i]);
        for (var i = 0; i < CSharpPushedRows; i++)
            AssertCSharpReaderAgrees(csharpLines[i], csharpOriginalLines[i]);

        var phraseNew = phraseLines[phraseOriginalLines.Length];
        var csharpNew = csharpLines[csharpOriginalLines.Length];
        AssertPhraseReaderAgrees(phraseNew, phraseNew);
        AssertCSharpReaderAgrees(csharpNew, csharpNew);
        var phraseHead = HeadOf(phraseNew);
        var csharpHead = HeadOf(csharpNew);
        var newHeads = new List<string>();
        if (phraseHead != "")
            newHeads.Add("phrase appended reply_head: " + phraseHead);
        if (csharpHead != "")
            newHeads.Add("csharp appended reply_head: " + csharpHead);
        Assert.True(newHeads.Count == 0, string.Join("\n", newHeads));
    }

    private static async Task AssertAllEndings(params Func<Task>[] endings)
    {
        var failures = new List<string>();
        foreach (var ending in endings)
        {
            try
            {
                await ending();
            }
            catch (Exception exception)
            {
                failures.Add(exception.Message);
            }
        }
        Assert.True(failures.Count == 0, string.Join("\n---\n", failures));
    }

    private static async Task AssertEnding(string suite, string model, string status, string? error = null, string? errorContains = null, string? marker = null)
    {
        var modelJson = await File.ReadAllTextAsync(ModelPath(suite, model));
        var historyLine = HistoryLineFor(suite, model);
        Assert.Equal("", HeadOf(modelJson));
        Assert.Equal("", HeadOf(historyLine));
        if (marker is not null)
        {
            AssertNoReply(modelJson, marker);
            AssertNoReply(historyLine, marker);
        }
        using var stored = JsonDocument.Parse(modelJson);
        using var history = JsonDocument.Parse(historyLine);
        Assert.Equal(status, stored.RootElement.GetProperty("status").GetString());
        Assert.Equal(status, history.RootElement.GetProperty("status").GetString());
        AssertError(stored.RootElement, error, errorContains);
        AssertError(history.RootElement, error, errorContains);
    }

    private static void AssertError(JsonElement row, string? error, string? errorContains)
    {
        var actual = row.TryGetProperty("error", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        if (errorContains is not null)
            Assert.Contains(errorContains, actual ?? "", StringComparison.Ordinal);
        else
            Assert.Equal(error, actual);
    }

    private static void AssertScoreFields(
        JsonElement row, string model, string task, bool passed, int attempts, int tokensIn, int tokensOut,
        string status, string proofLevel, string label)
    {
        Assert.Equal(model, row.GetProperty("model").GetString());
        Assert.Equal(task, row.GetProperty("task").GetString());
        Assert.Equal(passed, row.GetProperty("passed").GetBoolean());
        Assert.Equal(attempts, row.GetProperty("attempts").GetInt32());
        Assert.Equal(tokensIn, row.GetProperty("tokens_in").GetInt32());
        Assert.Equal(tokensOut, row.GetProperty("tokens_out").GetInt32());
        Assert.Equal(status, row.GetProperty("status").GetString());
        Assert.Equal(proofLevel, row.GetProperty("proof_level").GetString());
        Assert.Equal(label, row.GetProperty("label").GetString());
    }

    private static void AssertPhraseReaderAgrees(string copyLine, string originalLine)
    {
        var stored = BenchmarkResults.ReadStoredResult(copyLine);
        using var independent = JsonDocument.Parse(copyLine);
        using var original = JsonDocument.Parse(originalLine);
        Assert.Equal(independent.RootElement.GetProperty("model").GetString(), stored.Model);
        Assert.Equal(independent.RootElement.GetProperty("task").GetString(), stored.Task);
        Assert.Equal(independent.RootElement.GetProperty("passed").GetBoolean(), stored.Passed);
        Assert.Equal(independent.RootElement.GetProperty("status").GetString(), stored.Status);
        Assert.Equal(original.RootElement.GetProperty("model").GetString(), stored.Model);
        Assert.Equal(original.RootElement.GetProperty("task").GetString(), stored.Task);
        Assert.Equal(original.RootElement.GetProperty("passed").GetBoolean(), stored.Passed);
        Assert.Equal(original.RootElement.GetProperty("status").GetString(), stored.Status);
    }

    private static void AssertCSharpReaderAgrees(string copyLine, string originalLine)
    {
        using var copy = JsonDocument.Parse(copyLine);
        using var original = JsonDocument.Parse(originalLine);
        using var independent = JsonDocument.Parse(copyLine);
        Assert.Equal(independent.RootElement.GetProperty("model").GetString(), copy.RootElement.GetProperty("model").GetString());
        Assert.Equal(independent.RootElement.GetProperty("task").GetString(), copy.RootElement.GetProperty("task").GetString());
        Assert.Equal(independent.RootElement.GetProperty("passed").GetBoolean(), copy.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(independent.RootElement.GetProperty("status").GetString(), copy.RootElement.GetProperty("status").GetString());
        Assert.Equal(original.RootElement.GetProperty("model").GetString(), copy.RootElement.GetProperty("model").GetString());
        Assert.Equal(original.RootElement.GetProperty("task").GetString(), copy.RootElement.GetProperty("task").GetString());
        Assert.Equal(original.RootElement.GetProperty("passed").GetBoolean(), copy.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(original.RootElement.GetProperty("status").GetString(), copy.RootElement.GetProperty("status").GetString());
    }

    private static void AssertNoReply(string stored, params string[] fragments)
    {
        foreach (var fragment in fragments)
            Assert.DoesNotContain(fragment, stored, StringComparison.Ordinal);
    }

    private static string HeadOf(string json)
    {
        using var document = JsonDocument.Parse(json);
        return HeadOf(document.RootElement);
    }

    private static string HeadOf(JsonElement row) =>
        row.TryGetProperty("reply_head", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";

    private static string PassingPhraseReplyOver200(string marker, string tail)
    {
        var prefix = marker + "\nmodule Greeting\n\navow to \"say hello\"\n  native csharp\n    // ";
        var pad = Math.Max(0, 200 - prefix.Length);
        return prefix + new string('x', pad) + tail + "\n    return;\n";
    }

    private static string WritePhraseSuite(string root)
    {
        var suiteDirectory = Path.Combine(root, "bench");
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
        return Path.Combine(suiteDirectory, "suite.json");
    }

    private static string WriteCSharpSuite(string root)
    {
        var suiteRoot = Path.Combine(root, "csharp-baseline");
        var taskRoot = Path.Combine(suiteRoot, "tasks", "001-write-greeting");
        Directory.CreateDirectory(taskRoot);
        File.WriteAllText(Path.Combine(taskRoot, "prompt.md"), "Return only C# source for namespace Greeting, static class Greeter, static string SayHello().");
        File.WriteAllText(Path.Combine(taskRoot, "outcomes.json"), OneCase);
        var suite = Path.Combine(suiteRoot, "suite.json");
        File.WriteAllText(suite, """
            {"version":1,"tasks":[{"id":"001-write-greeting","kind":"write","prompt":"tasks/001-write-greeting/prompt.md","outcomes":"tasks/001-write-greeting/outcomes.json","holdout":false}]}
            """);
        return suite;
    }

    private static string WriteReply(string root, string name, string contents)
    {
        var path = Path.Combine(root, name);
        File.WriteAllText(path, contents);
        return path;
    }

    private static string PrintFileCommand(string path) => OperatingSystem.IsWindows()
        ? $"more > nul && type \"{path}\""
        : $"cat > /dev/null && cat \"{path}\"";

    private static string WriteAttemptThenExitCommand(string root, string firstReplyPath, int laterExit)
    {
        var counter = Path.Combine(root, Path.GetFileName(firstReplyPath) + "-attempt-counter.txt");
        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(root, Path.GetFileName(firstReplyPath) + "-attempts.cmd");
            File.WriteAllText(script, "@echo off\r\n" +
                "more > nul\r\n" +
                $"if exist \"{counter}\" goto later\r\n" +
                $"echo 1>\"{counter}\"\r\n" +
                $"type \"{firstReplyPath}\"\r\n" +
                "exit /b 0\r\n" +
                ":later\r\n" +
                $"exit /b {laterExit}\r\n");
            return $"call \"{script}\"";
        }

        var shell = Path.Combine(root, Path.GetFileName(firstReplyPath) + "-attempts.sh");
        File.WriteAllText(shell, "#!/bin/sh\n" +
            "cat >/dev/null\n" +
            $"if [ -f '{counter}' ]; then exit {laterExit}; fi\n" +
            $"touch '{counter}'\n" +
            $"cat '{firstReplyPath}'\n");
        return $"sh '{shell}'";
    }

    private static string HistoryPath(string suite) =>
        Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl");

    private static string ModelPath(string suite, string model) =>
        Path.Combine(Path.GetDirectoryName(suite)!, "results", model + ".json");

    private static string HistoryLineFor(string suite, string model)
    {
        var lines = JsonlLines(File.ReadAllBytes(HistoryPath(suite)));
        return Assert.Single(lines, line => line.Contains("\"model\":\"" + model + "\"", StringComparison.Ordinal));
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunArm(
        string project, string suite, string model, string command, string privateResults, string? label = null, int timeoutMs = 60000)
    {
        var arguments = new List<string>
        {
            "run", "--suite", suite, "--model", model, "--model-command", command,
            "--timeout-ms", timeoutMs.ToString(),
            "--private-results", privateResults,
        };
        if (label is not null)
        {
            arguments.Add("--label");
            arguments.Add(label);
        }

        var extra = project == "Avowry.BenchCs" ? new[] { "--disable-build-servers" } : Array.Empty<string>();
        var info = BenchLauncher.CreateStartInfoFor(project, arguments, extra);
        info.WorkingDirectory = RepoPaths.Root;
        info.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        info.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        foreach (var name in new[] { "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly" })
            info.Environment.Remove(name);

        var wait = project == "Avowry.BenchCs" ? TimeSpan.FromMinutes(6) : TimeSpan.FromMinutes(1);
        var result = await ProcessRunner.RunAsync(info, wait);
        Assert.False(result.TimedOut, project + " " + model + " timed out.");
        return (result.ExitCode, result.Stdout, result.Stderr);
    }

    private static string[] JsonlLines(byte[] bytes)
    {
        var lines = new List<string>();
        var start = 0;
        for (var i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] != (byte)'\n')
                continue;
            var end = i;
            if (end > start && bytes[end - 1] == (byte)'\r')
                end--;
            lines.Add(Encoding.UTF8.GetString(bytes, start, end - start));
            start = i + 1;
        }
        if (start < bytes.Length)
            lines.Add(Encoding.UTF8.GetString(bytes, start, bytes.Length - start));
        return lines.ToArray();
    }

    private static byte[] PrefixThroughNthNewline(byte[] bytes, int n)
    {
        var seen = 0;
        for (var i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] != (byte)'\n')
                continue;
            seen++;
            if (seen == n)
                return bytes[..(i + 1)];
        }
        throw new InvalidOperationException($"Expected {n} newlines, found {seen}.");
    }

    private static string Sha256Hex(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));
}
