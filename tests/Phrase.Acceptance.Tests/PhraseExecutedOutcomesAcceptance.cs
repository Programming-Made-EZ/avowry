using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;
using Phrase.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: Phrase arm scores executed outcomes for the write tasks
//   As Riley, I want the Phrase arm to compile a model's Phrase reply with the C# adapter
//   and run the write tasks' outcome cases against the generated code,
//   so that a Phrase pass means the same executed behaviour as a plain C# pass.
public class PhraseExecutedOutcomesAcceptance
{
    const string PromptHashBeforeExecutedScoring = "829227c49321902dcf3d18ae2b9e9e64da6bcde3c503b79019ba683e0f9adbfc";

    [Theory]
    [Trait("Slice", "099")]
    [InlineData("004-read-phrase", "read")]
    [InlineData("005-read-failure", "read")]
    [InlineData("007-modify-return", "modify")]
    [InlineData("008-modify-type", "modify")]
    public async Task Correct_read_and_modify_replies_pass_and_record_the_starting_code(string task, string kind)
    {
        // Given a read or modify task and a fake model whose recorded reply is a correct Phrase module
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, task);
        var reply = Path.Combine(workspace.Root, "correct.avowry");
        await File.WriteAllTextAsync(reply, CorrectChangeReply(task));

        // When Riley scores the reply by its executed outcomes
        var run = await Run(suite, PrintFileCommand(reply), "correct-" + task, "--scoring", "executed");

        // Then the row proves the requested behaviour and charges the inlined starting Phrase
        var row = Assert.Single(ReadHistory(suite).Values);
        Assert.True(run.ExitCode == 0, run.Stdout + run.Stderr + row);
        using var result = JsonDocument.Parse(row);
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal("specs-passed", result.RootElement.GetProperty("proof_level").GetString());
        Assert.Equal(kind, result.RootElement.GetProperty("kind").GetString());
        Assert.Equal(EstimateTokens(StartingPhrase(task)), result.RootElement.GetProperty("existing_code_tokens").GetInt32());
    }

    [Theory]
    [Trait("Slice", "099")]
    [InlineData("004-read-phrase", "read")]
    [InlineData("005-read-failure", "read")]
    [InlineData("007-modify-return", "modify")]
    [InlineData("008-modify-type", "modify")]
    public async Task Wrong_read_and_modify_replies_fail_naming_the_outcome(string task, string kind)
    {
        // Given a read or modify task and a fake model whose reply violates its first outcome
        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, task);
        var reply = Path.Combine(workspace.Root, "wrong.avowry");
        await File.WriteAllTextAsync(reply, WrongChangeReply(task));

        // When Riley scores the reply by its executed outcomes
        var run = await Run(suite, PrintFileCommand(reply), "wrong-" + task, "--scoring", "executed");

        // Then it fails and the compact feedback names the outcome that was not preserved
        var row = Assert.Single(ReadHistory(suite).Values);
        Assert.True(run.ExitCode == 3, run.Stdout + run.Stderr + row);
        using var result = JsonDocument.Parse(row);
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        var diagnostics = result.RootElement.GetProperty("diagnostics").EnumerateArray().Select(item => item.GetString()!).ToArray();
        Assert.Contains(diagnostics, text => text.Contains(OutcomeName(suite, task), StringComparison.Ordinal));
        Assert.Equal(kind, result.RootElement.GetProperty("kind").GetString());
    }

    [Fact]
    [Trait("Slice", "099")]
    public async Task Read_and_modify_starting_modules_compile_and_their_specs_pass()
    {
        // Given every inlined starting module for the read and modify tasks
        foreach (var task in ChangeTasks)
        {
            // When it is compiled with the C# adapter and its generated specs run
            var output = Phrase.Compiler.PhraseCompiler.Compile(StartingPhrase(task), new Phrase.Adapters.CSharp.CSharpProvider());
            using var workspace = TempWorkspace.Create();
            output.WriteTo(workspace.Root);
            var result = await DotnetCli.TestAsync(workspace.FindSingle("*.Specs.csproj"));

            // Then the fixture itself is valid, with any rejection naming the task
            Assert.True(result.ExitCode == 0, task + Environment.NewLine + result.Log);
            Assert.Equal(0, result.Failed ?? 0);
        }
    }

    [Fact]
    [Trait("Slice", "099")]
    public void Phrase_and_csharp_rows_name_change_metadata_the_same_way()
    {
        // Given the stored result contracts for the Phrase and plain C# arms
        var phraseFields = ResultFields(RepoPaths.Find("tools", "Phrase.Bench", "Program.cs"));
        var csharpFields = ResultFields(RepoPaths.Find("tools", "Phrase.BenchCs", "Program.cs"));

        // When their change-task metadata is compared
        var expected = new HashSet<string>(["Kind", "ExistingCodeTokens"], StringComparer.Ordinal);

        // Then both arms expose the same field names
        Assert.True(expected.SetEquals(phraseFields.Intersect(expected)), string.Join(", ", phraseFields));
        Assert.True(expected.SetEquals(csharpFields.Intersect(expected)), string.Join(", ", csharpFields));
    }

    [Fact]
    [Trait("Slice", "098")]
    public async Task Correct_write_replies_pass_at_specs_passed_with_executed_outcomes()
    {
        // Given tasks 001 and 002 and a fake model whose recorded reply is a correct Phrase module
        var replies = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["001-write-greeting"] = """
                module Greeting

                avow to "say hello" -> Text
                  spec "proves hello"
                    when called
                    then returns "hello"
                  native csharp
                    return "hello";
                """,
            ["002-write-total"] = """
                module Totals

                avow to "double {value: Int}" -> Int
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
            var reply = Path.Combine(workspace.Root, "reply.avowry");
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
        var reply = Path.Combine(workspace.Root, "wrong.avowry");
        await File.WriteAllTextAsync(reply, """
            module Greeting

            avow to "say hello" -> Text
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
        foreach (var task in new[] { "001-write-greeting", "002-write-total" }.Concat(ChangeTasks))
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
                Assert.Matches(new Regex("^" + Regex.Escape(launch) + "\"?.*prompt\\.txt\"?$", RegexOptions.CultureInvariant), arguments);
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
            var kind = taskId.Contains("-read-", StringComparison.Ordinal) ? "read"
                : taskId.Contains("-modify-", StringComparison.Ordinal) ? "modify" : "write";
            tasks.Add($$"""{"id":"{{taskId}}","kind":"{{kind}}","prompt":"tasks/{{taskId}}/prompt.md","holdout":false}""");
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
                echo avow to "say hello"
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

            avow to "say hello"
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
        info.ArgumentList.Add("--private-results");
        info.ArgumentList.Add(Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl"));
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

    private static string ReadResultByModel(string suite, string model)
    {
        var history = Path.Combine(Path.GetDirectoryName(suite)!, "results", "history.jsonl");
        foreach (var line in File.ReadAllLines(history))
        {
            using var document = JsonDocument.Parse(line);
            if (document.RootElement.GetProperty("model").GetString() == model)
                return line;
        }

        throw new InvalidOperationException($"Model '{model}' was not found in history");
    }

    private static string Sha256Hex(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static readonly string[] ChangeTasks = ["004-read-phrase", "005-read-failure", "007-modify-return", "008-modify-type"];

    private static string StartingPhrase(string task)
    {
        var prompt = File.ReadAllText(RepoPaths.Find("bench", "tasks", task, "prompt.md"));
        var match = Regex.Match(prompt, "```avowry\\r?\\n(?<source>.*?)\\r?\\n```", RegexOptions.Singleline | RegexOptions.CultureInvariant);
        Assert.True(match.Success, $"{task}: starting Phrase fixture is missing");
        return match.Groups["source"].Value;
    }

    private static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));

    private static HashSet<string> ResultFields(string path)
    {
        var source = File.ReadAllText(path);
        var start = source.IndexOf("record BenchmarkResult(", StringComparison.Ordinal);
        Assert.True(start >= 0, path + ": BenchmarkResult is missing");
        var end = source.IndexOf(");", start, StringComparison.Ordinal);
        var declaration = source[start..end];
        return Regex.Matches(declaration, @"\b(?:string|int|bool)\??\s+(?<name>[A-Z]\w*)")
            .Select(match => match.Groups["name"].Value)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string CorrectChangeReply(string task) => task switch
    {
        "004-read-phrase" => StartingPhrase(task) + "\n\n# The phrase returns its Text input unchanged.\n# The spec proves sample is preserved.",
        "005-read-failure" => StartingPhrase(task) + "\n\n# MissingName occurs for an empty name.\n# The spec proves Ada succeeds.",
        "007-modify-return" => """
            module Status

            avow to "current status" -> Text
              spec "returns ready"
                when called
                then returns "ready"
              native csharp
                return "ready";
            """,
        "008-modify-type" => """
            module Customers

            type Customer
              name: Text
              email: Text

            avow to "email of {customer: Customer}" -> Text
              spec "returns a customer's email"
                given customer = { name: "Ada", email: "ada@example.com" }
                when called
                then returns "ada@example.com"
              native csharp
                return customer.Email;
            """,
        _ => throw new ArgumentException(task),
    };

    private static string WrongChangeReply(string task) => task switch
    {
        "004-read-phrase" => CorrectChangeReply(task).Replace("return value;", "return \"other\";", StringComparison.Ordinal),
        "005-read-failure" => CorrectChangeReply(task).Replace("if (string.IsNullOrEmpty(name))", "if (!string.IsNullOrEmpty(name))", StringComparison.Ordinal),
        "007-modify-return" => CorrectChangeReply(task).Replace("return \"ready\";", "return \"waiting\";", StringComparison.Ordinal),
        "008-modify-type" => """
            module Customers

            type Customer
              email: Text

            avow to "email of {customer: Customer}" -> Text
              spec "returns a customer's email"
                given customer = { email: "ada@example.com" }
                when called
                then returns "ada@example.com"
              native csharp
                return customer.Email;
            """,
        _ => throw new ArgumentException(task),
    };

    [Fact]
    [Trait("Slice", "100")]
    public void Fix_task_prompts_stay_broken_and_report_the_documented_diagnostics()
    {
        // Given each broken start prompt for the fix tasks
        var prompts = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["010-fix-trigger"] = "Fix the failing spec in this Avowry module. Do not change the intended behavior or remove the spec. Return only valid Avowry source.\n\n```avowry\nmodule Greeting\n\navow to \"greeting\" -> Text\n  spec \"greets\"\n    when handled\n    then returns \"hello\"\n  native csharp\n    return \"hello\";\n```\n",
            ["011-fix-type"] = "Fix the type error in this Avowry module while preserving the avow name and the spec's expected value. Return only valid Avowry source.\n\n```avowry\nmodule Count\n\navow to \"answer\" -> Text\n  spec \"is forty two\"\n    when called\n    then returns 42\n  native csharp\n    return 42;\n```\n",
        };

        foreach (var (task, expected) in prompts)
        {
            var actual = File.ReadAllText(RepoPaths.Find("bench", "tasks", task, "prompt.md"));

            // When the prompt is checked against the committed broken start
            // Then it must stay byte-identical and fail with its documented code
            Assert.Equal(expected, actual);
            var match = Regex.Match(actual, "```avowry\\r?\\n(?<source>.*?)\\r?\\n```", RegexOptions.Singleline | RegexOptions.CultureInvariant);
            Assert.True(match.Success, task + ": broken Phrase fixture is missing");
            var source = match.Groups["source"].Value;
            var exception = Assert.Throws<CompilationFailedException>(() => PhraseCompiler.Compile(source, new Phrase.Adapters.CSharp.CSharpProvider()));
            if (task == "010-fix-trigger")
            {
                Assert.Contains(exception.Diagnostics, diagnostic => diagnostic.Code == "PH330" && diagnostic.Message.Contains("when called", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                Assert.Contains(exception.Diagnostics, diagnostic => diagnostic.Code == "PH312" && diagnostic.Message.Contains("Text", StringComparison.OrdinalIgnoreCase) && diagnostic.Message.Contains("Int", StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    [Theory]
    [Trait("Slice", "100")]
    [InlineData("010-fix-trigger")]
    [InlineData("011-fix-type")]
    public async Task Fixed_reply_passes_and_faulty_reply_fails_with_the_named_outcome(string task)
    {
        // Given a correct repair for each fix task and a wrong one that leaves the original bug
        var correct = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["010-fix-trigger"] = """
                module Greeting

                avow to "greeting" -> Text
                  spec "greets"
                    when called
                    then returns "hello"
                  native csharp
                    return "hello";
                """,
            ["011-fix-type"] = """
                module Count

                avow to "answer" -> Int
                  spec "is forty two"
                    when called
                    then returns 42
                  native csharp
                    return 42;
                """,
        };
        var wrong = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["010-fix-trigger"] = """
                module Greeting

                avow to "greeting" -> Text
                  spec "greets"
                    when handled
                    then returns "hello"
                  native csharp
                    return "hello";
                """,
            ["011-fix-type"] = """
                module Count

                avow to "answer" -> Text
                  spec "is forty two"
                    when called
                    then returns 42
                  native csharp
                    return 42;
                """,
        };

        using var workspace = TempWorkspace.Create();
        var suite = WriteSuite(workspace.Root, task);
        var correctReply = Path.Combine(workspace.Root, "fixed.avowry");
        var wrongReply = Path.Combine(workspace.Root, "broken.avowry");
        await File.WriteAllTextAsync(correctReply, correct[task]);
        await File.WriteAllTextAsync(wrongReply, wrong[task]);

        // When Riley scores the fixed reply and then the unchanged-bug reply by executed outcomes
        var fixedRun = await Run(suite, PrintFileCommand(correctReply), task + "-fixed", "--scoring", "executed");
        var brokenRun = await Run(suite, PrintFileCommand(wrongReply), task + "-broken", "--scoring", "executed");

        // Then the fixed one passes and the broken one fails naming the outcome that still fails
        Assert.True(fixedRun.ExitCode == 0, fixedRun.Stdout + fixedRun.Stderr);
        var fixedModel = task + "-fixed";
        var brokenModel = task + "-broken";
        using var fixedRow = JsonDocument.Parse(ReadResultByModel(suite, fixedModel));
        Assert.Equal("specs-passed", fixedRow.RootElement.GetProperty("proof_level").GetString());
        Assert.True(fixedRow.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(brokenRun.ExitCode == 3, brokenRun.Stdout + brokenRun.Stderr);
        using var brokenRow = JsonDocument.Parse(ReadResultByModel(suite, brokenModel));
        var diagnostics = brokenRow.RootElement.GetProperty("diagnostics").EnumerateArray().Select(item => item.GetString()!).ToArray();
        if (task == "010-fix-trigger")
            Assert.Contains(diagnostics, text => text.Contains("PH330", StringComparison.Ordinal) && text.Contains("when called", StringComparison.OrdinalIgnoreCase));
        else
            Assert.Contains(diagnostics, text => text.Contains("PH312", StringComparison.Ordinal) && text.Contains("Text", StringComparison.OrdinalIgnoreCase) && text.Contains("Int", StringComparison.OrdinalIgnoreCase));
    }
}
