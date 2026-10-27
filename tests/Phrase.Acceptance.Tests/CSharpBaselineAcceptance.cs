using System.Diagnostics;
using System.Text.Json;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

public sealed class CSharpBaselineAcceptance
{
    // The Phrase prompt mentions a C# native body for task 002 because that is a Phrase mechanic; the C# twin intentionally does not.
    [Fact]
    public async Task correct_csharp_reply_passes_with_specs_passed_proof()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, TwoCases);
        var reply = WriteReply(workspace.Root, "correct.cs", CorrectGreetingAndTest);

        var run = await Run(suite, PrintFileCommand(reply), "correct-v1");

        Assert.Equal(0, run.ExitCode);
        using var result = ReadResult(suite, "correct-v1");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal("specs-passed", result.RootElement.GetProperty("proof_level").GetString());
        Assert.Equal("plain-csharp", result.RootElement.GetProperty("mode").GetString());
        Assert.Equal("executed-outcomes", result.RootElement.GetProperty("score_bar").GetString());
        Assert.Equal(1, result.RootElement.GetProperty("model_test_count").GetInt32());
        Assert.Equal(2, result.RootElement.GetProperty("harness_test_count").GetInt32());
    }

    [Fact]
    public async Task wrong_reply_fails_with_the_failing_test_name_in_diagnostics()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "wrong.cs", WrongCodeWithPassingOwnTest);

        var run = await Run(suite, PrintFileCommand(reply), "wrong-v1");

        Assert.Equal(3, run.ExitCode);
        using var result = ReadResult(suite, "wrong-v1");
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("returns_hello", result.RootElement.GetProperty("diagnostics")[0].GetString());
        Assert.True(result.RootElement.GetProperty("diagnostics")[0].GetString()!.Length <= 600);
        Assert.Equal(1, result.RootElement.GetProperty("model_test_count").GetInt32());
        Assert.Equal(1, result.RootElement.GetProperty("harness_test_count").GetInt32());
    }

    [Fact]
    public async Task correct_code_without_own_test_fails_with_no_tests_diagnostic()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "no-test.cs", CorrectGreeting);

        var run = await Run(suite, PrintFileCommand(reply), "no-test-v1");

        Assert.Equal(3, run.ExitCode);
        using var result = ReadResult(suite, "no-test-v1");
        Assert.Contains(result.RootElement.GetProperty("diagnostics").EnumerateArray(), value => value.GetString() == "no_csharp_tests_found");
        Assert.Equal(0, result.RootElement.GetProperty("model_test_count").GetInt32());
        Assert.Equal(0, result.RootElement.GetProperty("harness_test_count").GetInt32());
    }

    [Fact]
    public async Task correct_code_with_wrong_own_test_fails_and_names_that_test()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "wrong-test.cs", CorrectCodeWithWrongOwnTest);

        var run = await Run(suite, PrintFileCommand(reply), "wrong-test-v1");

        Assert.Equal(3, run.ExitCode);
        using var result = ReadResult(suite, "wrong-test-v1");
        Assert.Contains("Greeter_returns_goodbye", result.RootElement.GetProperty("diagnostics")[0].GetString());
    }

    [Fact]
    public async Task narration_only_reply_is_a_completed_no_csharp_found_failure()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "narration.txt", "I would create a greeting class.");

        var run = await Run(suite, PrintFileCommand(reply), "narration-v1");

        Assert.Equal(3, run.ExitCode);
        using var result = ReadResult(suite, "narration-v1");
        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        Assert.Contains(result.RootElement.GetProperty("diagnostics").EnumerateArray(), value => value.GetString() == "no_csharp_found");
    }

    [Fact]
    public async Task non_zero_model_exit_is_unavailable()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var command = OperatingSystem.IsWindows() ? "more > nul & exit /b 7" : "cat >/dev/null; exit 7";

        var run = await Run(suite, command, "unavailable-v1");

        Assert.Equal(0, run.ExitCode);
        using var result = ReadResult(suite, "unavailable-v1");
        Assert.Equal("unavailable", result.RootElement.GetProperty("status").GetString());
        Assert.Contains("exited 7", result.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task generated_test_count_equals_the_number_of_outcome_cases()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, TwoCases);
        var reply = WriteReply(workspace.Root, "correct.cs", CorrectGreetingAndTest);

        await Run(suite, PrintFileCommand(reply), "count-v1");

        using var result = ReadResult(suite, "count-v1");
        Assert.Equal(1, result.RootElement.GetProperty("model_test_count").GetInt32());
        Assert.Equal(2, result.RootElement.GetProperty("harness_test_count").GetInt32());
    }

    [Fact]
    public async Task entry_point_is_discovered_without_a_hard_coded_class_or_method_name()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "discovered.cs", """
            namespace Greeting; public static class Welcome { public static string Message() => "hello"; }
            public sealed class WelcomeTests { [Xunit.Fact] public void works() => Xunit.Assert.Equal("hello", Welcome.Message()); }
            """);

        var run = await Run(suite, PrintFileCommand(reply), "discovered-v1");

        Assert.Equal(0, run.ExitCode);
    }

    [Fact]
    public async Task marked_two_file_reply_is_written_as_two_compilation_units()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "two-files.cs", """
            // FILE: Greeting.cs
            namespace Greeting { public static class Welcome { public static string Message() => "hello"; } }
            // FILE: Tests.cs
            namespace Greeting { public sealed class WelcomeTests { [Xunit.Fact] public void works() => Xunit.Assert.Equal("hello", Welcome.Message()); } }
            """);

        var run = await Run(suite, PrintFileCommand(reply), "two-files-v1");

        Assert.Equal(0, run.ExitCode);
    }

    [Fact]
    public async Task failed_build_claims_no_executed_test_counts()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "broken.cs", "namespace Greeting; public class Broken { [Xunit.Fact] public void test() { nope } }");

        await Run(suite, PrintFileCommand(reply), "broken-v1");

        using var result = ReadResult(suite, "broken-v1");
        Assert.Equal(0, result.RootElement.GetProperty("model_test_count").GetInt32());
        Assert.Equal(0, result.RootElement.GetProperty("harness_test_count").GetInt32());
        Assert.Equal("none", result.RootElement.GetProperty("proof_level").GetString());
    }

    [Fact]
    public async Task reply_head_is_scrubbed_before_it_is_cut()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var secretPath = @"C:\Users\alice\private\answer.cs";
        var reply = WriteReply(workspace.Root, "path.cs", new string('x', 190) + " " + secretPath + "\n" + CorrectGreetingAndTest);

        await Run(suite, PrintFileCommand(reply), "scrub-v1");

        using var result = ReadResult(suite, "scrub-v1");
        Assert.DoesNotContain("alice", result.RootElement.GetProperty("reply_head").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task zero_executed_model_tests_is_a_completed_failure()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "skipped.cs", """
            namespace Greeting; public static class Welcome { public static string Message() => "hello"; }
            public sealed class Tests { [Xunit.Fact(Skip = "not run")] public void skipped() { } }
            """);

        var run = await Run(suite, PrintFileCommand(reply), "skipped-v1");

        Assert.Equal(3, run.ExitCode);
        using var result = ReadResult(suite, "skipped-v1");
        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        Assert.Equal("tests_did_not_run", result.RootElement.GetProperty("diagnostics")[0].GetString());
        Assert.Equal(0, result.RootElement.GetProperty("model_test_count").GetInt32());
        Assert.Equal("none", result.RootElement.GetProperty("proof_level").GetString());
    }

    [Fact]
    public async Task holdout_task_is_skipped()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: true, OneCase);

        var run = await Run(suite, "a-command-that-must-not-run", "holdout-v1");

        Assert.Equal(0, run.ExitCode);
        Assert.False(File.Exists(Path.Combine(Path.GetDirectoryName(suite)!, "results", "holdout-v1.json")));
    }

    private const string CorrectGreeting = "namespace Greeting; public static class Greeter { public static string SayHello() => \"hello\"; }";
    private const string CorrectGreetingAndTest = """
        // FILE: Greeter.cs
        namespace Greeting; public static class Greeter { public static string SayHello() => "hello"; }
        // FILE: GreeterTests.cs
        using Greeting;
        public sealed class GreeterTests { [Xunit.Fact] public void Greeter_returns_hello() => Xunit.Assert.Equal("hello", Greeter.SayHello()); }
        """;
    private const string CorrectCodeWithWrongOwnTest = """
        namespace Greeting; public static class Greeter { public static string SayHello() => "hello"; }
        public sealed class GreeterTests { [Xunit.Fact] public void Greeter_returns_goodbye() => Xunit.Assert.Equal("goodbye", Greeter.SayHello()); }
        """;
    private const string WrongCodeWithPassingOwnTest = """
        namespace Greeting; public static class Greeter { public static string SayHello() => "goodbye"; }
        public sealed class GreeterTests { [Xunit.Fact] public void Greeter_returns_goodbye() => Xunit.Assert.Equal("goodbye", Greeter.SayHello()); }
        """;
    private const string OneCase = "[{\"name\":\"returns hello\",\"given\":{},\"when\":\"called\",\"then\":{\"returns\":\"hello\"}}]";
    private const string TwoCases = "[{\"name\":\"returns hello\",\"given\":{},\"when\":\"called\",\"then\":{\"returns\":\"hello\"}},{\"name\":\"returns hello again\",\"given\":{},\"when\":\"called\",\"then\":{\"returns\":\"hello\"}}]";

    private static string WriteSuite(string root, bool holdout, string outcomes)
    {
        var suiteRoot = Path.Combine(root, "csharp-baseline");
        var taskRoot = Path.Combine(suiteRoot, "tasks", "001-write-greeting");
        Directory.CreateDirectory(taskRoot);
        File.WriteAllText(Path.Combine(taskRoot, "prompt.md"), "Return only C# source for namespace Greeting, static class Greeter, static string SayHello().");
        File.WriteAllText(Path.Combine(taskRoot, "outcomes.json"), outcomes);
        var suite = Path.Combine(suiteRoot, "suite.json");
        File.WriteAllText(suite, $$"""
            {"version":1,"tasks":[{"id":"001-write-greeting","kind":"write","prompt":"tasks/001-write-greeting/prompt.md","outcomes":"tasks/001-write-greeting/outcomes.json","holdout":{{holdout.ToString().ToLowerInvariant()}}}]}
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
        ? $"more > nul & type \"{path}\""
        : $"cat >/dev/null; cat '{path}'";

    private static async Task<(int ExitCode, string Stdout, string Stderr)> Run(string suite, string command, string model)
    {
        var project = RepoPaths.Find("tools", "Phrase.BenchCs", "Phrase.BenchCs.csproj");
        var info = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argument in new[] { "run", "--project", project, "--", "run", "--suite", suite, "--model", model, "--model-command", command, "--timeout-ms", "30000" })
            info.ArgumentList.Add(argument);
        using var process = Process.Start(info)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout, stderr);
    }

    private static JsonDocument ReadResult(string suite, string model) => JsonDocument.Parse(
        File.ReadAllText(Path.Combine(Path.GetDirectoryName(suite)!, "results", $"{model}.json")));
}
