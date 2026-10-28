using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

public sealed class CSharpBaselineAcceptance
{
    [Fact]
    public void cold_profile_literals_cannot_drift_between_benchmark_arms()
    {
        var phrase = File.ReadAllText(RepoPaths.Find("tools", "Phrase.Bench", "Program.cs"));
        var csharp = File.ReadAllText(RepoPaths.Find("tools", "Phrase.BenchCs", "Program.cs"));

        static string Literal(string source, string name) => Regex.Match(source,
            $"const string {name} = \\\"(?<value>[^\\\"]*)\\\";").Groups["value"].Value;

        Assert.Equal(Literal(phrase, "ColdPreamble"), Literal(csharp, "ColdPreamble"));
        Assert.Equal(Literal(phrase, "grokDisallowedTools"), Literal(csharp, "grokDisallowedTools"));
        Assert.NotEmpty(Literal(csharp, "ColdPreamble"));
        Assert.NotEmpty(Literal(csharp, "grokDisallowedTools"));
    }

    [Fact]
    public async Task cold_codex_uses_the_preamble_restricted_flags_and_an_empty_attempt_workdir()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, TwoCases);
        var reply = WriteReply(workspace.Root, "correct.cs", CorrectGreetingAndTest);
        var argv = Path.Combine(workspace.Root, "argv.txt");
        var stdin = Path.Combine(workspace.Root, "stdin.txt");
        var listing = Path.Combine(workspace.Root, "listing.txt");
        string command;
        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(workspace.Root, "fake-model.cmd");
            File.WriteAllText(script, $"@echo off\r\necho %*>\"{argv}\"\r\nmore > \"{stdin}\"\r\ndir /b > \"{listing}\"\r\ntype \"{reply}\"\r\n");
            command = $"call \"{script}\"";
        }
        else
        {
            var script = Path.Combine(workspace.Root, "fake-model.sh");
            File.WriteAllText(script, $"#!/bin/sh\nprintf '%s\\n' \"$*\" > '{argv}'\ncat > '{stdin}'\nls -A > '{listing}'\ncat '{reply}'\n");
            command = $"sh \"{script}\"";
        }

        var run = await Run(suite, command, "codex-probe", cold: true);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal("exec --skip-git-repo-check --sandbox read-only", File.ReadAllText(argv).Trim());
        Assert.StartsWith("There is no workspace and no files to read; answer directly in this reply without looking anything up.", File.ReadAllText(stdin), StringComparison.Ordinal);
        Assert.Equal(string.Empty, File.ReadAllText(listing).Trim());
        using var result = ReadResult(suite, "codex-probe");
        Assert.True(result.RootElement.GetProperty("cold").GetBoolean());
        Assert.Equal("--sandbox read-only", result.RootElement.GetProperty("cold_flags").GetString());
        Assert.True(result.RootElement.GetProperty("workdir_empty_at_launch").GetBoolean());
    }

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

    // Feature: Plain C# arm scores an endpoint responds case with its status
    //   As Riley, I want a responds case judged by the response record and the HTTP status
    //   a caller observes, so that an endpoint task can pass or fail on that reply.
    [Fact]
    [Trait("Slice", "110")]
    public async Task endpoint_reply_passes_when_response_record_and_status_match()
    {
        // Given an invented endpoint task whose outcomes use responds with a status and fails_with,
        // and a fake model whose reply is a correct FastEndpoints handler
        using var workspace = TempWorkspace.Create();
        var suite = WriteEndpointSuite(workspace.Root, EndpointOutcomes);
        var reply = WriteReply(workspace.Root, "greetings-endpoint.cs", CorrectEndpointReply);

        // When the plain C# arm runs that reply
        var run = await Run(suite, PrintFileCommand(reply), "endpoint-correct-v1", timeoutMs: 180000);

        // Then the row passes at specs-passed with the executed-outcomes score bar
        Assert.True(run.ExitCode == 0, FailureDetail(suite, "endpoint-correct-v1", run));
        using var result = ReadResult(suite, "endpoint-correct-v1");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal("specs-passed", result.RootElement.GetProperty("proof_level").GetString());
        Assert.Equal("executed-outcomes", result.RootElement.GetProperty("score_bar").GetString());
        Assert.Equal("endpoint", result.RootElement.GetProperty("kind").GetString());
    }

    [Fact]
    [Trait("Slice", "110")]
    public async Task endpoint_reply_fails_when_response_record_or_status_are_wrong()
    {
        // Given the same invented endpoint task and a reply whose response record does not match
        using var workspace = TempWorkspace.Create();
        var suite = WriteEndpointSuite(workspace.Root, EndpointOutcomes);
        var reply = WriteReply(workspace.Root, "greetings-endpoint.cs", WrongEndpointReply);

        // When the plain C# arm runs that reply
        var run = await Run(suite, PrintFileCommand(reply), "endpoint-wrong-v1", timeoutMs: 180000);

        // Then the row fails and names the failing responds case
        Assert.True(run.ExitCode == 3, FailureDetail(suite, "endpoint-wrong-v1", run));
        using var result = ReadResult(suite, "endpoint-wrong-v1");
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        var diagnostics = result.RootElement.GetProperty("diagnostics").EnumerateArray().Select(item => item.GetString() ?? "").ToArray();
        Assert.Contains(diagnostics, text => text.Contains("greets_ada", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("Slice", "110")]
    public void committed_outcomes_keep_the_existing_then_kinds()
    {
        // Given the committed plain-C# baseline outcomes
        var root = RepoPaths.Find("bench", "csharp-baseline", "tasks");
        var files = Directory.GetFiles(root, "outcomes.json", SearchOption.AllDirectories);
        var allowed = new HashSet<string>(StringComparer.Ordinal) { "returns", "fails_with", "succeeds", "has_field", "responds" };

        // When each case is read
        Assert.NotEmpty(files);
        foreach (var file in files)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            foreach (var item in document.RootElement.EnumerateArray())
            {
                var kinds = item.GetProperty("then").EnumerateObject().Select(property => property.Name).Where(name => name != "status").ToArray();

                // Then the case still uses an existing then kind
                Assert.NotEmpty(kinds);
                Assert.All(kinds, kind => Assert.Contains(kind, allowed));
            }
        }
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
    public async Task empty_reply_is_retried_to_the_cap_and_records_attempt_tokens()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var empty = WriteReply(workspace.Root, "empty.txt", string.Empty);

        var run = await Run(suite, PrintFileCommand(empty), "empty-v1");

        Assert.Equal(0, run.ExitCode);
        using var result = ReadResult(suite, "empty-v1");
        Assert.Equal("unavailable", result.RootElement.GetProperty("status").GetString());
        Assert.Equal("empty reply", result.RootElement.GetProperty("error").GetString());
        Assert.Equal(3, result.RootElement.GetProperty("attempts").GetInt32());
        Assert.Equal(3 * EstimateTokens("Return only C# source for namespace Greeting, static class Greeter, static string SayHello()."),
            result.RootElement.GetProperty("tokens_in").GetInt32());
    }

    [Fact]
    public async Task empty_first_reply_can_pass_on_the_second_attempt()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var empty = WriteReply(workspace.Root, "empty.txt", string.Empty);
        var correct = WriteReply(workspace.Root, "correct.cs", CorrectGreetingAndTest);
        var command = WriteAttemptCommand(workspace.Root, empty, correct);

        var run = await Run(suite, command, "empty-then-pass-v1");

        Assert.Equal(0, run.ExitCode);
        using var result = ReadResult(suite, "empty-then-pass-v1");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(2, result.RootElement.GetProperty("attempts").GetInt32());
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
    public async Task file_scoped_namespaces_in_two_marked_files_compile_separately()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "file-scoped.cs", """
            // FILE: Greeting.cs
            namespace Greeting;
            public static class Welcome { public static string Message() => "hello"; }
            // FILE: Tests.cs
            namespace Greeting;
            public sealed class WelcomeTests { [Xunit.Fact] public void works() => Xunit.Assert.Equal("hello", Welcome.Message()); }
            """);

        var run = await Run(suite, PrintFileCommand(reply), "file-scoped-v1");

        Assert.Equal(0, run.ExitCode);
    }

    [Fact]
    public async Task multiple_greeting_entry_points_are_an_ambiguous_completed_failure()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "ambiguous.cs", """
            namespace Greeting;
            public static class Welcome
            {
                public static string Message() => "hello";
                public static string Helper() => "hello";
            }
            public sealed class Tests { [Xunit.Fact] public void works() => Xunit.Assert.Equal("hello", Welcome.Message()); }
            """);

        var run = await Run(suite, PrintFileCommand(reply), "ambiguous-v1");

        Assert.Equal(3, run.ExitCode);
        using var result = ReadResult(suite, "ambiguous-v1");
        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        Assert.Contains(result.RootElement.GetProperty("diagnostics").EnumerateArray(), value =>
            value.GetString()!.Contains("ambiguous_entry_point", StringComparison.Ordinal));
    }

    [Fact]
    public void task_001_prompt_requires_exactly_one_matching_method()
    {
        var prompt = File.ReadAllText(RepoPaths.Find("bench", "csharp-baseline", "tasks", "001-write-greeting", "prompt.md"));

        Assert.Contains("exactly one public static method", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("exactly one phrase", prompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task a_later_build_failure_cannot_reuse_an_earlier_attempt_trx()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var first = WriteReply(workspace.Root, "first.cs", WrongCodeWithPassingOwnTest);
        var later = WriteReply(workspace.Root, "later.cs", "namespace Greeting; public class Broken { [Xunit.Fact] public void test() { nope } }");
        var command = WriteAttemptCommand(workspace.Root, first, later);

        await Run(suite, command, "fresh-attempt-v1");

        using var result = ReadResult(suite, "fresh-attempt-v1");
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(0, result.RootElement.GetProperty("model_test_count").GetInt32());
        Assert.Equal(0, result.RootElement.GetProperty("harness_test_count").GetInt32());
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
    public async Task reply_head_scrubs_tilde_paths_with_either_separator_before_cutting()
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, holdout: false, OneCase);
        var reply = WriteReply(workspace.Root, "tilde-path.cs",
            @"~\AppData\Local\Temp\private-answer.cs ~/private/answer.cs " + CorrectGreetingAndTest);

        await Run(suite, PrintFileCommand(reply), "tilde-scrub-v1");

        using var result = ReadResult(suite, "tilde-scrub-v1");
        var head = result.RootElement.GetProperty("reply_head").GetString();
        Assert.DoesNotContain("AppData", head, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("~/private", head, StringComparison.OrdinalIgnoreCase);
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

    // Phrase.Bench scores read tasks at parse-resolve only: it does not check that comments were added.
    // These twins keep that level for the comment text, and execute the spec cases the Phrase prompt states.
    [Fact]
    public void read_and_modify_outcomes_match_the_phrase_prompts()
    {
        var identity = File.ReadAllText(RepoPaths.Find("bench", "tasks", "004-read-phrase", "prompt.md"));
        var identityOutcomes = File.ReadAllText(RepoPaths.Find("bench", "csharp-baseline", "tasks", "004-read-phrase", "outcomes.json"));
        Assert.Contains("returns its input", identity, StringComparison.Ordinal);
        Assert.Contains("value = \"sample\"", identity, StringComparison.Ordinal);
        Assert.Contains("returns \"sample\"", identity, StringComparison.Ordinal);
        Assert.Contains("returns its input", identityOutcomes, StringComparison.Ordinal);
        Assert.Contains("\"value\": \"sample\"", identityOutcomes, StringComparison.Ordinal);
        Assert.Contains("\"returns\": \"sample\"", identityOutcomes, StringComparison.Ordinal);

        var names = File.ReadAllText(RepoPaths.Find("bench", "tasks", "005-read-failure", "prompt.md"));
        var namesOutcomes = File.ReadAllText(RepoPaths.Find("bench", "csharp-baseline", "tasks", "005-read-failure", "outcomes.json"));
        Assert.Contains("accepts a name", names, StringComparison.Ordinal);
        Assert.Contains("name = \"Ada\"", names, StringComparison.Ordinal);
        Assert.Contains("then succeeds", names, StringComparison.Ordinal);
        Assert.Contains("MissingName", names, StringComparison.Ordinal);
        Assert.Contains("accepts a name", namesOutcomes, StringComparison.Ordinal);
        Assert.Contains("\"name\": \"Ada\"", namesOutcomes, StringComparison.Ordinal);
        Assert.Contains("\"succeeds\": true", namesOutcomes, StringComparison.Ordinal);

        var status = File.ReadAllText(RepoPaths.Find("bench", "tasks", "007-modify-return", "prompt.md"));
        var statusOutcomes = File.ReadAllText(RepoPaths.Find("bench", "csharp-baseline", "tasks", "007-modify-return", "outcomes.json"));
        Assert.Contains("returns `\"ready\"`", status, StringComparison.Ordinal);
        Assert.Contains("return;", status, StringComparison.Ordinal);
        Assert.Contains("returns ready", statusOutcomes, StringComparison.Ordinal);
        Assert.Contains("\"returns\": \"ready\"", statusOutcomes, StringComparison.Ordinal);

        var customers = File.ReadAllText(RepoPaths.Find("bench", "tasks", "008-modify-type", "prompt.md"));
        var customersOutcomes = File.ReadAllText(RepoPaths.Find("bench", "csharp-baseline", "tasks", "008-modify-type", "outcomes.json"));
        Assert.Contains("email: Text", customers, StringComparison.Ordinal);
        Assert.Contains("name: Text", customers, StringComparison.Ordinal);
        Assert.Contains("Preserve the existing field", customers, StringComparison.Ordinal);
        Assert.Contains("customer's email can be returned", customers, StringComparison.Ordinal);
        Assert.Contains("preserves the name field", customersOutcomes, StringComparison.Ordinal);
        Assert.Contains("\"has_field\": \"name\"", customersOutcomes, StringComparison.Ordinal);
        Assert.Contains("returns a customer's email", customersOutcomes, StringComparison.Ordinal);
        Assert.Contains("\"email\"", customersOutcomes, StringComparison.Ordinal);
        Assert.Contains("\"returns\": \"ada@example.com\"", customersOutcomes, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("004-read-phrase", "read")]
    [InlineData("005-read-failure", "read")]
    [InlineData("007-modify-return", "modify")]
    [InlineData("008-modify-type", "modify")]
    [InlineData("010-fix-trigger", "fix")]
    [InlineData("011-fix-type", "fix")]
    public async Task correct_change_task_reply_passes_and_records_the_starting_code(string taskId, string kind)
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteCopiedSuite(workspace.Root, taskId, kind);
        var reply = WriteReply(workspace.Root, "correct.cs", CorrectChangeReply(taskId));
        var capture = Path.Combine(workspace.Root, "prompt-capture.txt");

        var run = await Run(suite, CapturePromptCommand(workspace.Root, reply, capture), $"{taskId}-ok", timeoutMs: 120000);

        Assert.True(run.ExitCode == 0, run.Stdout + Environment.NewLine + run.Stderr);
        var prompt = File.ReadAllText(capture);
        var body = File.ReadAllText(RepoPaths.Find("bench", "csharp-baseline", "tasks", taskId, "start.cs"));
        Assert.Contains(body.Trim(), prompt, StringComparison.Ordinal);
        using var result = ReadResult(suite, $"{taskId}-ok");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(kind, result.RootElement.GetProperty("kind").GetString());
        Assert.Equal("executed-outcomes", result.RootElement.GetProperty("score_bar").GetString());
        Assert.Equal(EstimateTokens(body), result.RootElement.GetProperty("existing_code_tokens").GetInt32());
        Assert.True(result.RootElement.GetProperty("model_test_count").GetInt32() >= 1);
        Assert.True(result.RootElement.GetProperty("harness_test_count").GetInt32() >= 1);
    }

    [Theory]
    [InlineData("004-read-phrase", "read", "Given_value_is_sample_When_called_Then_returns_its_input")]
    [InlineData("005-read-failure", "read", "Given_name_is_ada_When_called_Then_accepts_a_name")]
    [InlineData("007-modify-return", "modify", "Given_no_inputs_When_called_Then_returns_ready")]
    [InlineData("008-modify-type", "modify", "Given_name_is_ada_and_email_is_ada_example_com_When_called_Then_returns_a_customer_s_email")]
    [InlineData("010-fix-trigger", "fix", "Given_no_inputs_When_called_Then_returns_hello")]
    [InlineData("011-fix-type", "fix", "Given_no_inputs_When_called_Then_returns_forty_two")]
    public async Task wrong_change_task_reply_fails_and_names_the_failing_test(string taskId, string kind, string failingTest)
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteCopiedSuite(workspace.Root, taskId, kind);
        var reply = WriteReply(workspace.Root, "wrong.cs", WrongChangeReply(taskId));

        var run = await Run(suite, PrintFileCommand(reply), $"{taskId}-bad", timeoutMs: 120000);

        Assert.True(run.ExitCode == 3, run.Stdout + Environment.NewLine + run.Stderr);
        using var result = ReadResult(suite, $"{taskId}-bad");
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains(failingTest, result.RootElement.GetProperty("diagnostics")[0].GetString());
        Assert.Equal(EstimateTokens(File.ReadAllText(RepoPaths.Find("bench", "csharp-baseline", "tasks", taskId, "start.cs"))),
            result.RootElement.GetProperty("existing_code_tokens").GetInt32());
    }

    [Theory]
    [InlineData("010-fix-trigger", "CS0117", "does not contain a definition for 'Handled'")]
    [InlineData("011-fix-type", "CS0029", "cannot implicitly convert type")]
    public async Task broken_start_code_fails_with_the_expected_compiler_diagnostic(string taskId, string diagnosticName, string expectedText)
    {
        using var workspace = TempWorkspace.Create();
        var suite = WriteCopiedSuite(workspace.Root, taskId, "fix");
        var broken = WriteReply(workspace.Root, "broken.cs", File.ReadAllText(RepoPaths.Find("bench", "csharp-baseline", "tasks", taskId, "start.cs")));

        var run = await Run(suite, PrintFileCommand(broken), $"{taskId}-broken", timeoutMs: 120000);

        Assert.True(run.ExitCode == 3, run.Stdout + Environment.NewLine + run.Stderr);
        using var result = ReadResult(suite, $"{taskId}-broken");
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains(diagnosticName, result.RootElement.GetProperty("diagnostics")[0].GetString() ?? string.Empty, StringComparison.Ordinal);
        Assert.Contains(expectedText, result.RootElement.GetProperty("diagnostics")[0].GetString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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

    private const string EndpointOutcomes = "[{\"name\":\"greets Ada\",\"given\":{\"name\":\"Ada\"},\"when\":\"called\",\"then\":{\"responds\":{\"message\":\"hello\",\"name\":\"Ada\"},\"status\":200}},{\"name\":\"rejects blank name\",\"given\":{\"name\":\"\"},\"when\":\"called\",\"then\":{\"fails_with\":\"MissingName\"}}]";
    private const string CorrectEndpointReply = """
        using FastEndpoints;
        namespace Greetings;
        public sealed record GreetingRequest(string Name);
        public sealed record GreetingResponse(string Message, string Name);
        public sealed class GreetingsEndpoint : Endpoint<GreetingRequest, GreetingResponse>
        {
            public override void Configure() => Post("/greetings");
            public override async Task HandleAsync(GreetingRequest req, CancellationToken ct)
            {
                if (string.IsNullOrWhiteSpace(req.Name))
                {
                    AddError("MissingName");
                    await Send.ErrorsAsync(422, ct);
                    return;
                }

                await Send.OkAsync(new GreetingResponse("hello", req.Name), ct);
            }
        }
        """;
    private const string WrongEndpointReply = """
        using FastEndpoints;
        namespace Greetings;
        public sealed record GreetingRequest(string Name);
        public sealed record GreetingResponse(string Message, string Name);
        public sealed class GreetingsEndpoint : Endpoint<GreetingRequest, GreetingResponse>
        {
            public override void Configure() => Post("/greetings");
            public override async Task HandleAsync(GreetingRequest req, CancellationToken ct)
            {
                if (string.IsNullOrWhiteSpace(req.Name))
                {
                    AddError("MissingName");
                    await Send.ErrorsAsync(422, ct);
                    return;
                }

                await Send.OkAsync(new GreetingResponse("goodbye", req.Name), ct);
            }
        }
        """;

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

    private static string CorrectChangeReply(string taskId) => taskId switch
    {
        "004-read-phrase" => """
            namespace Identity;
            // Returns the text it was given.
            public static class Identity {
                // Of returns value unchanged.
                public static string Of(string value) => value;
            }
            // Proves that "sample" is returned.
            public sealed class Spec {
                [Xunit.Fact]
                public void returns_its_input() => Xunit.Assert.Equal("sample", Identity.Of("sample"));
            }
            """,
        "005-read-failure" => """
            namespace Names;
            // MissingName occurs when name is null or empty.
            public static class Names {
                public static void Ensure(string name) {
                    // An empty name fails with MissingName; any other name is accepted.
                    if (string.IsNullOrEmpty(name)) throw new System.Exception("MissingName");
                }
            }
            // Proves that Ada is accepted and the call succeeds.
            public sealed class Spec {
                [Xunit.Fact]
                public void accepts_a_name() => Names.Ensure("Ada");
            }
            """,
        "007-modify-return" => """
            namespace Status;
            public static class Status {
                public static string Current() => "ready";
            }
            public sealed class Spec {
                [Xunit.Fact]
                public void returns_ready() => Xunit.Assert.Equal("ready", Status.Current());
            }
            """,
        "008-modify-type" => """
            namespace Customers;
            public sealed record Customer(string name, string email);
            public static class Customers {
                public static string EmailOf(Customer customer) => customer.email;
            }
            public sealed class Spec {
                [Xunit.Fact]
                public void returns_the_email() {
                    var customer = new Customer("Ada", "ada@example.com");
                    Xunit.Assert.Equal("ada@example.com", Customers.EmailOf(customer));
                }
            }
            """,
        "010-fix-trigger" => """
            namespace Greeting;
            public static class Greeter {
                public static string SayHello() => "hello";
            }
            public sealed class Spec {
                [Xunit.Fact]
                public void returns_hello() => Xunit.Assert.Equal("hello", Greeter.SayHello());
            }
            """,
        "011-fix-type" => """
            namespace Count;
            public static class Counter {
                public static int Answer() => 42;
            }
            public sealed class Spec {
                [Xunit.Fact]
                public void returns_forty_two() => Xunit.Assert.Equal(42, Counter.Answer());
            }
            """,
        _ => throw new ArgumentException(taskId)
    };

    private static string WrongChangeReply(string taskId) => taskId switch
    {
        "004-read-phrase" => """
            namespace Identity;
            public static class Identity {
                public static string Of(string value) => "other";
            }
            public sealed class Spec {
                [Xunit.Fact]
                public void returns_other() => Xunit.Assert.Equal("other", Identity.Of("sample"));
            }
            """,
        "005-read-failure" => """
            namespace Names;
            public static class Names {
                public static void Ensure(string name) { throw new System.Exception("MissingName"); }
            }
            public sealed class Spec {
                [Xunit.Fact]
                public void rejects_ada() => Xunit.Assert.Throws<System.Exception>(() => Names.Ensure("Ada"));
            }
            """,
        "007-modify-return" => """
            namespace Status;
            public static class Status {
                public static void Current() { return; }
            }
            public sealed class Spec {
                [Xunit.Fact]
                public void still_returns() { Status.Current(); }
            }
            """,
        "008-modify-type" => """
            namespace Customers;
            public record Customer(string name);
            public sealed class Spec {
                [Xunit.Fact]
                public void has_name() => Xunit.Assert.Equal("Ada", new Customer("Ada").name);
            }
            """,
        "010-fix-trigger" => """
            namespace Greeting;
            public static class Greeter {
                public static string SayHello() => "goodbye";
            }
            public sealed class Spec {
                [Xunit.Fact]
                public void still_returns_hello() => Xunit.Assert.Equal("hello", Greeter.SayHello());
            }
            """,
        "011-fix-type" => """
            namespace Count;
            public static class Counter {
                public static int Answer() => 41;
            }
            public sealed class Spec {
                [Xunit.Fact]
                public void returns_forty_two() => Xunit.Assert.Equal(42, Counter.Answer());
            }
            """,
        _ => throw new ArgumentException(taskId)
    };

    private static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));

    private static string FailureDetail(string suite, string model, (int ExitCode, string Stdout, string Stderr) run)
    {
        var detail = run.Stdout + Environment.NewLine + run.Stderr;
        var resultPath = Path.Combine(Path.GetDirectoryName(suite)!, "results", $"{model}.json");
        return File.Exists(resultPath) ? detail + Environment.NewLine + File.ReadAllText(resultPath) : detail;
    }

    private static string WriteEndpointSuite(string root, string outcomes)
    {
        var suiteRoot = Path.Combine(root, "csharp-baseline");
        var taskRoot = Path.Combine(suiteRoot, "tasks", "020-endpoint-greetings");
        Directory.CreateDirectory(taskRoot);
        File.WriteAllText(Path.Combine(taskRoot, "prompt.md"), "Return only a FastEndpoints endpoint for POST /greetings.");
        File.WriteAllText(Path.Combine(taskRoot, "outcomes.json"), outcomes);
        var suite = Path.Combine(suiteRoot, "suite.json");
        File.WriteAllText(suite, """
            {"version":1,"tasks":[{"id":"020-endpoint-greetings","kind":"endpoint","prompt":"tasks/020-endpoint-greetings/prompt.md","outcomes":"tasks/020-endpoint-greetings/outcomes.json","holdout":false}]}
            """);
        return suite;
    }

    private static string WriteCopiedSuite(string root, string taskId, string kind)
    {
        var source = RepoPaths.Find("bench", "csharp-baseline", "tasks", taskId);
        var taskRoot = Path.Combine(root, "csharp-baseline", "tasks", taskId);
        Directory.CreateDirectory(taskRoot);
        foreach (var file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(taskRoot, Path.GetFileName(file)), overwrite: true);
        var suite = Path.Combine(root, "csharp-baseline", "suite.json");
        File.WriteAllText(suite, $$"""
            {"version":1,"tasks":[{"id":"{{taskId}}","kind":"{{kind}}","prompt":"tasks/{{taskId}}/prompt.md","outcomes":"tasks/{{taskId}}/outcomes.json","holdout":false}]}
            """);
        return suite;
    }

    private static string CapturePromptCommand(string root, string replyPath, string capturePath)
    {
        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(root, "capture.cmd");
            File.WriteAllText(script, "@echo off\r\n" +
                $"python -c \"import pathlib,sys; pathlib.Path(r'{capturePath}').write_bytes(sys.stdin.buffer.read()); sys.stdout.buffer.write(pathlib.Path(r'{replyPath}').read_bytes())\"\r\n");
            return $"call \"{script}\"";
        }

        var shell = Path.Combine(root, "capture.sh");
        File.WriteAllText(shell, $"#!/bin/sh\npython -c \"import pathlib,sys; pathlib.Path(r'{capturePath}').write_bytes(sys.stdin.buffer.read()); sys.stdout.buffer.write(pathlib.Path(r'{replyPath}').read_bytes())\"\n");
        return $"sh '{shell}'";
    }

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

    private static string WriteAttemptCommand(string root, string first, string later)
    {
        var counter = Path.Combine(root, "attempt-counter.txt");
        if (OperatingSystem.IsWindows())
        {
            var script = Path.Combine(root, "attempts.cmd");
            File.WriteAllText(script, $"@echo off\r\nmore > nul\r\nif exist \"{counter}\" goto later\r\necho 1>\"{counter}\"\r\ntype \"{first}\"\r\nexit /b 0\r\n:later\r\ntype \"{later}\"\r\n");
            return $"call \"{script}\"";
        }
        else
        {
            var script = Path.Combine(root, "attempts.sh");
            File.WriteAllText(script, $"#!/bin/sh\ncat >/dev/null\nif [ -f '{counter}' ]; then cat '{later}'; else touch '{counter}'; cat '{first}'; fi\n");
            return $"sh '{script}'";
        }
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> Run(string suite, string command, string model, int timeoutMs = 30000, bool cold = false)
    {
        var project = RepoPaths.Find("tools", "Phrase.BenchCs", "Phrase.BenchCs.csproj");
        var info = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argument in new[] { "run", "--project", project, "--", "run", "--suite", suite, "--model", model, "--model-command", command, "--timeout-ms", timeoutMs.ToString() })
            info.ArgumentList.Add(argument);
        info.ArgumentList.Add("--private-results");
        info.ArgumentList.Add(Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl"));
        if (cold) info.ArgumentList.Add("--cold");
        using var process = Process.Start(info)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout, stderr);
    }

    private static JsonDocument ReadResult(string suite, string model) => JsonDocument.Parse(
        File.ReadAllText(Path.Combine(Path.GetDirectoryName(suite)!, "results", $"{model}.json")));
}
