using System.Text.Json;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: Phrase arm scores endpoint responds outcomes with status
//   As Riley, I want a Phrase endpoint judged by the response record and HTTP status a caller observes,
//   so that its executed score means the same thing as the plain C# endpoint score.
public sealed class PhraseEndpointExecutedOutcomes
{
    [Fact]
    [Trait("Slice", "111")]
    public async Task Correct_endpoint_reply_passes_at_specs_passed()
    {
        // Given an invented endpoint task whose outcomes use responds with a status and fails_with,
        // and a fake model whose recorded reply is a correct Phrase endpoint module
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root);
        var reply = WriteReply(workspace.Root, "correct.phrase", CorrectReply);

        // When the Phrase arm runs it with executed scoring
        var run = await Run(suite, PrintFileCommand(reply), "endpoint-correct-v1");

        // Then the generated endpoint is called in process and the row passes at the executed-outcomes bar
        Assert.True(run.ExitCode == 0, FailureDetail(suite, "endpoint-correct-v1", run));
        using var result = ReadResult(suite, "endpoint-correct-v1");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal("specs-passed", result.RootElement.GetProperty("proof_level").GetString());
        Assert.Equal("executed-outcomes", result.RootElement.GetProperty("score_bar").GetString());
        Assert.Equal("endpoint", result.RootElement.GetProperty("kind").GetString());
    }

    [Fact]
    [Trait("Slice", "111")]
    public async Task Wrong_record_or_status_fails_and_names_the_case()
    {
        // Given the same endpoint task, first with a wrong response record and then with a drifted expected status
        using var workspace = TempWorkspace.Create();
        var reply = WriteReply(workspace.Root, "wrong.phrase", WrongReply);
        var wrongRecordSuite = WriteSuite(Path.Combine(workspace.Root, "wrong-record"));
        var wrongStatusSuite = WriteSuite(Path.Combine(workspace.Root, "wrong-status"), EndpointOutcomes.Replace("\"status\":200", "\"status\":201", StringComparison.Ordinal));

        // When Riley scores each reply by executing the endpoint outcome cases
        var wrongRecord = await Run(wrongRecordSuite, PrintFileCommand(reply), "endpoint-wrong-record-v1");
        var wrongStatus = await Run(wrongStatusSuite, PrintFileCommand(WriteReply(workspace.Root, "correct-for-status.phrase", CorrectReply)), "endpoint-wrong-status-v1");

        // Then each row fails and names the greets-Ada case that did not match
        AssertNamedFailure(wrongRecordSuite, "endpoint-wrong-record-v1", wrongRecord, "greets_ada");
        AssertNamedFailure(wrongStatusSuite, "endpoint-wrong-status-v1", wrongStatus, "greets_ada");
    }

    private static void AssertNamedFailure(string suite, string model, (int ExitCode, string Stdout, string Stderr) run, string caseName)
    {
        Assert.True(run.ExitCode == 3, FailureDetail(suite, model, run));
        using var result = ReadResult(suite, model);
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        var diagnostics = result.RootElement.GetProperty("diagnostics").EnumerateArray().Select(item => item.GetString() ?? "").ToArray();
        Assert.Contains(diagnostics, text => text.Contains(caseName, StringComparison.Ordinal));
    }

    private static string WriteSuite(string root, string outcomes = EndpointOutcomes)
    {
        var suiteRoot = Path.Combine(root, "bench");
        var taskRoot = Path.Combine(suiteRoot, "tasks", "020-endpoint-greetings");
        Directory.CreateDirectory(taskRoot);
        File.WriteAllText(Path.Combine(taskRoot, "prompt.md"), "Return only a Phrase endpoint module for POST /greetings.");
        File.WriteAllText(Path.Combine(taskRoot, "outcomes.json"), outcomes);
        var suite = Path.Combine(suiteRoot, "suite.json");
        File.WriteAllText(suite, """
            {"version":1,"tasks":[{"id":"020-endpoint-greetings","kind":"endpoint","prompt":"tasks/020-endpoint-greetings/prompt.md","holdout":false}]}
            """);
        return suite;
    }

    private static string WriteReply(string root, string name, string source)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, name);
        File.WriteAllText(path, source);
        return path;
    }

    private static string PrintFileCommand(string path) =>
        OperatingSystem.IsWindows() ? $"more > nul && type \"{path}\"" : $"cat > /dev/null && cat \"{path}\"";

    private static async Task<(int ExitCode, string Stdout, string Stderr)> Run(string suite, string command, string model)
    {
        var project = RepoPaths.Find("tools", "Phrase.Bench", "Phrase.Bench.csproj");
        var info = new System.Diagnostics.ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        info.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        info.Environment["DOTNET_ROLL_FORWARD"] = "Major";
        foreach (var argument in new[] { "run", "--disable-build-servers", "--project", project, "--", "run", "--suite", suite, "--model", model, "--model-command", command, "--scoring", "executed" })
            info.ArgumentList.Add(argument);
        info.ArgumentList.Add("--private-results");
        info.ArgumentList.Add(Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl"));
        using var process = System.Diagnostics.Process.Start(info)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await stdout, await stderr);
    }

    private static JsonDocument ReadResult(string suite, string model) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(suite)!, "results", model + ".json")));

    private static string FailureDetail(string suite, string model, (int ExitCode, string Stdout, string Stderr) run)
    {
        var path = Path.Combine(Path.GetDirectoryName(suite)!, "results", model + ".json");
        return run.Stdout + Environment.NewLine + run.Stderr + (File.Exists(path) ? Environment.NewLine + File.ReadAllText(path) : "");
    }

    private const string EndpointOutcomes = "[{\"name\":\"greets Ada\",\"given\":{\"name\":\"Ada\"},\"when\":\"called\",\"then\":{\"responds\":{\"message\":\"hello\",\"name\":\"Ada\"},\"status\":200}},{\"name\":\"rejects blank name\",\"given\":{\"name\":\"\"},\"when\":\"called\",\"then\":{\"fails_with\":\"MissingName\"}}]";

    private const string CorrectReply = """
        module Greetings

        phrase "ensure {name: Text} is present"
          fails with MissingName
          spec "rejects blank name"
            given name = ""
            when called
            then fails with MissingName
          native csharp
            if (string.IsNullOrWhiteSpace(name)) Fail("MissingName");

        endpoint Greetings : POST /greetings
          input
            name: Text
          output
            message: Text
            name: Text
          steps
            ensure input.name is present
            respond with { message: "hello", name: input.name }

          spec "greets Ada"
            given input { name: "Ada" }
            when handled
            then responds { message: "hello", name: "Ada" }
        """;

    private const string WrongReply = """
        module Greetings

        phrase "ensure {name: Text} is present"
          fails with MissingName
          spec "rejects blank name"
            given name = ""
            when called
            then fails with MissingName
          native csharp
            if (string.IsNullOrWhiteSpace(name)) Fail("MissingName");

        endpoint Greetings : POST /greetings
          input
            name: Text
          output
            message: Text
            name: Text
          steps
            ensure input.name is present
            respond with { message: "goodbye", name: input.name }

          spec "greets Ada"
            given input { name: "Ada" }
            when handled
            then responds { message: "goodbye", name: "Ada" }
        """;
}
