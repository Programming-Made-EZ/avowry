using Phrase.Acceptance.Tests.Support;
using System.Security.Cryptography;
using System.Text;
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
    public void v1_suite_has_fixed_task_and_holdout_coverage()
    {
        // Given the versioned benchmark suite
        var suitePath = RepoPaths.Find("bench", "suite.json");
        using var suite = JsonDocument.Parse(File.ReadAllText(suitePath));
        var root = suite.RootElement;
        var tasks = root.GetProperty("tasks").EnumerateArray().ToArray();

        // When its task mix is inspected
        var kinds = tasks.Select(task => task.GetProperty("kind").GetString()).ToHashSet();
        var holdoutCount = tasks.Count(task => task.GetProperty("holdout").GetBoolean());
        var suiteDirectory = Path.GetDirectoryName(suitePath)!;

        // Then it covers all four task kinds with a fixed quarter held out and prompt-backed specs
        Assert.Equal(1, root.GetProperty("version").GetInt32());
        Assert.True(tasks.Length >= 12, $"Expected at least 12 tasks, found {tasks.Length}.");
        Assert.True(new[] { "write", "read", "modify", "fix" }.All(kinds.Contains));
        Assert.True(holdoutCount * 4 >= tasks.Length, $"Expected at least a quarter holdout, found {holdoutCount}/{tasks.Length}.");
        foreach (var task in tasks)
        {
            var promptPath = Path.Combine(suiteDirectory, task.GetProperty("prompt").GetString()!);
            Assert.True(File.Exists(promptPath), $"Missing task prompt: {promptPath}");
            Assert.Contains("spec", File.ReadAllText(promptPath), StringComparison.OrdinalIgnoreCase);
        }
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
        var modelCommand = PrintFileCommand(modelOutput);

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
    public async Task reply_head_scrubs_a_user_path_before_cutting_to_200_characters()
    {
        var prefix = new string('x', 190);
        var reply = prefix + @" C:\ tail!";

        var replyHead = await RunAndReadReplyHead(reply, "reply-head-user-path-v1");

        Assert.True(replyHead.Length <= 200, $"Reply head was {replyHead.Length} characters.");
        Assert.Contains(" [path]", replyHead, StringComparison.Ordinal);
        Assert.DoesNotContain(@"C:\", replyHead, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task reply_head_masks_a_tilde_relative_path_inside_the_first_200_characters()
    {
        var replyHead = await RunAndReadReplyHead(
            @"Inspect ~\AppData\Local\Temp\private\reply.txt before answering.",
            "reply-head-tilde-path-v1");

        Assert.Equal("Inspect [path] before answering.", replyHead);
    }

    [Fact]
    public async Task reply_head_masks_a_secret_looking_token()
    {
        var replyHead = await RunAndReadReplyHead(
            "Use api_key=top-secret-value before answering.",
            "reply-head-secret-v1");

        Assert.Equal("Use api_key=[redacted] before answering.", replyHead);
    }

    [Fact]
    public async Task reply_head_without_sensitive_content_is_only_cut_to_200_characters()
    {
        var reply = new string('a', 210);

        var replyHead = await RunAndReadReplyHead(reply, "reply-head-plain-v1");

        Assert.Equal(reply[..200], replyHead);
    }

    [Fact]
    public async Task run_invokes_each_profile_once_headless_with_its_working_flags()
    {
        // Given each supported model profile and a shim that records its arguments
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var expectedTokens = new Dictionary<string, string[]>
        {
            ["codex-probe"] = ["exec", "--skip-git-repo-check"],
            ["grok-probe"] = ["--max-turns", "10", "--prompt-file"],
            ["copilot-probe"] = ["-s", "--allow-all-tools", "--trust-folder"],
            ["agy-probe"] = ["-p", "--model", "gemini-4-argon-high", "--dangerously-skip-permissions"],
        };
        var missingTokens = new List<string>();
        var recordedLines = new List<string>();

        foreach (var (model, requiredTokens) in expectedTokens)
        {
            var argvLog = Path.Combine(workspace.Root, $"{model}-argv.log");
            await File.WriteAllTextAsync(argvLog, string.Empty);
            var modelCommand = await WriteArgumentRecordingShim(workspace.Root, model, argvLog);

            // When the benchmark CLI runs one task through the profile
            var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, model, modelCommand);

            // Then it invokes the shim once, headlessly, and completes the result
            Assert.True(exitCode == 0, stdout + stderr);
            Assert.Contains($"{model}: passed", stdout);
            var argvLine = Assert.Single(await File.ReadAllLinesAsync(argvLog));
            if (model.StartsWith("codex", StringComparison.Ordinal) ||
                model.StartsWith("grok", StringComparison.Ordinal) ||
                model.StartsWith("copilot", StringComparison.Ordinal))
                Assert.DoesNotContain("Greeting", argvLine);
            else
                Assert.Contains("Greeting", argvLine);
            recordedLines.Add($"{model}: {argvLine}");
            var actualTokens = argvLine.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            missingTokens.AddRange(requiredTokens
                .Where(required => !actualTokens.Contains(required, StringComparer.Ordinal))
                .Select(required => $"{model}: {required}"));
            using var result = await ReadModelResult(suiteDirectory, model);
            Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        }

        Assert.True(
            missingTokens.Count == 0,
            $"Missing profile tokens: {string.Join(", ", missingTokens)}{Environment.NewLine}Recorded: {string.Join(Environment.NewLine, recordedLines)}");
    }

    [Fact]
    public async Task cold_runs_append_exactly_the_cold_extras_and_never_break_the_command_line()
    {
        // Given each profile launched cold, with a shim that records the real arguments it received
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var required = new Dictionary<string, string[]>
        {
            ["codex-probe"] = ["exec", "--skip-git-repo-check", "--sandbox", "read-only"],
            ["grok-probe"] = ["--max-turns", "10", "--prompt-file", "--cwd", "--disallowed-tools", "read_file,list_dir,grep,run_terminal_cmd,search_replace,web_search,web_fetch,todo_write,task,Agent", "--disable-web-search"],
            ["copilot-probe"] = ["-s", "--trust-folder", "--deny-tool", "shell", "write", "read", "url"],
            ["agy-probe"] = ["-p", "--model", "gemini-4-argon-high", "--dangerously-skip-permissions"],
        };
        var forbidden = new Dictionary<string, string[]>
        {
            ["copilot-probe"] = ["--allow-all-tools", "*", "-p"],
        };
        var problems = new List<string>();

        foreach (var (model, tokens) in required)
        {
            var argvLog = Path.Combine(workspace.Root, $"{model}-cold-argv.log");
            await File.WriteAllTextAsync(argvLog, string.Empty);
            var modelCommand = await WriteArgumentRecordingShim(workspace.Root, model, argvLog);

            // When the benchmark runs one task cold
            var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, model, modelCommand, "--cold");

            // Then the command line has each required token, no forbidden one, no duplicated flag, and no flag left without its value
            Assert.True(exitCode == 0, stdout + stderr);
            var lines = await File.ReadAllLinesAsync(argvLog);
            Assert.True(lines.Length == 1, $"{model}: expected one recorded command line, got {lines.Length}. stdout: {stdout} stderr: {stderr}");
            var line = lines[0];
            var actual = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            problems.AddRange(tokens.Where(token => !actual.Contains(token, StringComparer.Ordinal)).Select(token => $"{model}: missing {token}"));
            if (forbidden.TryGetValue(model, out var banned))
                problems.AddRange(banned.Where(token => actual.Contains(token, StringComparer.Ordinal)).Select(token => $"{model}: forbidden {token}"));
            problems.AddRange(actual.Where(token => token.StartsWith("--", StringComparison.Ordinal))
                .GroupBy(token => token).Where(group => group.Key != "--deny-tool" && group.Count() > 1).Select(group => $"{model}: duplicated {group.Key}"));
            if (model == "grok-probe")
            {
                var index = Array.IndexOf(actual, "--prompt-file");
                if (index < 0 || index + 1 >= actual.Length || actual[index + 1].StartsWith("--", StringComparison.Ordinal))
                    problems.Add("grok-probe: --prompt-file has no value");
                var cwd = Array.IndexOf(actual, "--cwd");
                if (cwd < 0 || cwd + 1 >= actual.Length || actual[cwd + 1].StartsWith("--", StringComparison.Ordinal))
                    problems.Add("grok-probe: --cwd has no value");
            }
        }

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public async Task run_survives_a_model_that_exits_without_reading_its_stdin_given_a_large_prompt()
    {
        // Given a model that takes its prompt from a file or argument and exits at once without reading stdin
        // (the real Grok profile does), and a large prompt (a big task file is inlined into it)
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        await File.WriteAllTextAsync(Path.Combine(suiteDirectory, "tasks", "001-write-greeting", "big-context.txt"), new string('x', 400_000));
        var modelCommand = OperatingSystem.IsWindows()
            ? "python -c \"print('module Greeting\\n\\nphrase \\\"say hello\\\"\\n  native csharp\\n    return;')\""
            : "python -c \"print('module Greeting\\n\\nphrase \\\"say hello\\\"\\n  native csharp\\n    return;')\"";

        // When the benchmark runs the task
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "stdin-ignorer", modelCommand);

        // Then the broken pipe does not crash the run: the reply is still read and scored
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("stdin-ignorer: passed", stdout);
    }

    [Fact]
    public async Task run_delivers_the_copilot_prompt_on_stdin_so_a_multi_line_prompt_survives()
    {
        // Given a copilot profile whose task has a multi-line prompt (task file bodies and the documentation block are multi-line)
        // and a shim that captures stdin and the argument line (a cmd.exe command line ends at the first newline, so -p cannot carry a multi-line prompt)
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var promptFile = Path.Combine(suiteDirectory, "tasks", "001-write-greeting", "prompt.md");
        await File.WriteAllTextAsync(promptFile, "First line of the task.\n\nSecond paragraph of the task.\nWrite a valid Phrase module named Greeting.");
        var capturedPrompt = Path.Combine(workspace.Root, "copilot-stdin.txt");

        // When the task is run through the copilot profile
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "copilot-probe", CapturePromptCommand(capturedPrompt));

        // Then the whole multi-line prompt arrived on stdin, and no -p argument carried it
        Assert.True(exitCode == 0, stdout + stderr);
        var stdinText = await File.ReadAllTextAsync(capturedPrompt);
        Assert.Contains("First line of the task.", stdinText.Replace("\r\n", "\n"));
        Assert.Contains("Second paragraph of the task.\nWrite a valid Phrase module named Greeting.", stdinText.Replace("\r\n", "\n"));
    }

    [Fact]
    public async Task run_scores_phrase_wrapped_in_narration_and_a_markdown_fence()
    {
        // Given a one-task suite and model output containing narrated, fenced Phrase
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelOutput = Path.Combine(workspace.Root, "model-output.txt");
        await File.WriteAllTextAsync(modelOutput, """
            Here is the program.

            ```phrase
            module Greeting

            phrase "say hello"
              native csharp
                return;
            ```

            That should compile.
            """);
        var modelCommand = PrintFileCommand(modelOutput);

        // When the benchmark CLI runs the fake model through its real process boundary
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "fenced-v1", modelCommand);

        // Then the extracted Phrase is scored as a completed first-attempt pass
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("fenced-v1: passed", stdout);
        using var result = await ReadModelResult(suiteDirectory, "fenced-v1");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(1, result.RootElement.GetProperty("attempts").GetInt32());
        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        Assert.DoesNotContain(
            result.RootElement.GetProperty("diagnostics").EnumerateArray(),
            diagnostic => diagnostic.GetString()!.StartsWith("PH10", StringComparison.Ordinal));
    }

    [Fact]
    public async Task run_extracts_phrase_from_an_unlabeled_markdown_fence()
    {
        // Given a model reply that wraps the Phrase in a plain markdown fence with no language tag
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelOutput = Path.Combine(workspace.Root, "model-output.txt");
        await File.WriteAllTextAsync(modelOutput, """
            Here is the final program.

            ```
            module Greeting

            phrase "say hello"
              native csharp
                return;
            ```

            This compiles cleanly.
            """);
        var modelCommand = PrintFileCommand(modelOutput);

        // When the benchmark CLI runs the fake model through its real process boundary
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "plain-fence-v1", modelCommand);

        // Then the Phrase is stripped from the fence and scored without PH diagnostics
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("plain-fence-v1: passed", stdout);
        using var result = await ReadModelResult(suiteDirectory, "plain-fence-v1");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        Assert.DoesNotContain(
            result.RootElement.GetProperty("diagnostics").EnumerateArray(),
            diagnostic => diagnostic.GetString()!.StartsWith("PH10", StringComparison.Ordinal));
    }

    [Fact]
    public async Task run_extracts_phrase_from_case_insensitive_markdown_fence_and_narration()
    {
        // Given a quoted model reply with mixed narration and a case-insensitive fenced Phrase block
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelOutput = Path.Combine(workspace.Root, "model-output.txt");
        await File.WriteAllTextAsync(modelOutput, """
            Here is the program I meant.

            ```PHRASE
            module Greeting

            phrase "say hello"
              native csharp
                return;
            ```

            That should compile cleanly.
            """);
        var modelCommand = PrintFileCommand(modelOutput);

        // When the benchmark CLI runs the fake model through its real process boundary
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "fenced-case-v1", modelCommand);

        // Then the phrase is extracted from the fenced block and scored without PH diagnostics
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("fenced-case-v1: passed", stdout);
        using var result = await ReadModelResult(suiteDirectory, "fenced-case-v1");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        Assert.DoesNotContain(
            result.RootElement.GetProperty("diagnostics").EnumerateArray(),
            diagnostic => diagnostic.GetString()!.StartsWith("PH10", StringComparison.Ordinal));
    }

    [Fact]
    public async Task run_records_no_phrase_found_when_the_reply_contains_no_phrase()
    {
        // Given a one-task suite and model output containing narration without Phrase
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelOutput = Path.Combine(workspace.Root, "model-output.txt");
        await File.WriteAllTextAsync(modelOutput, """
            I cannot write that program.
            The request is unclear, so there is no source here.
            """);
        var modelCommand = PrintFileCommand(modelOutput);

        // When the benchmark CLI runs the fake model through its real process boundary
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "narration-v1", modelCommand);

        // Then the completed attempt fails clearly without compiler syntax diagnostics
        Assert.True(exitCode == 3, stdout + stderr);
        Assert.Contains("narration-v1: failed", stdout);
        using var result = await ReadModelResult(suiteDirectory, "narration-v1");
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        var diagnostics = result.RootElement.GetProperty("diagnostics").EnumerateArray().ToArray();
        Assert.Contains(diagnostics, diagnostic => diagnostic.GetString() == "no_phrase_found");
        Assert.DoesNotContain(
            diagnostics,
            diagnostic => diagnostic.GetString()!.StartsWith("PH10", StringComparison.Ordinal));
    }

    [Fact]
    public async Task run_records_narration_only_failure_as_completed_model_error_without_unavailable_status()
    {
        // Given a model reply that narrates but never emits Phrase source
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelOutput = Path.Combine(workspace.Root, "model-output.txt");
        await File.WriteAllTextAsync(modelOutput, "I will open the working tree and look at the files first.");
        var modelCommand = PrintFileCommand(modelOutput);

        // When the benchmark runner evaluates that reply
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "narration-only-v1", modelCommand);

        // Then it is treated as a real model failure, not as an unavailable harness error
        Assert.True(exitCode == 3, stdout + stderr);
        Assert.Contains("narration-only-v1: failed", stdout);
        using var result = await ReadModelResult(suiteDirectory, "narration-only-v1");
        Assert.False(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal("completed", result.RootElement.GetProperty("status").GetString());
        Assert.Equal("none", result.RootElement.GetProperty("proof_level").GetString());
        var diagnostics = result.RootElement.GetProperty("diagnostics").EnumerateArray().ToArray();
        Assert.Contains(diagnostics, diagnostic => diagnostic.GetString() == "no_phrase_found");
        Assert.DoesNotContain("unavailable", result.RootElement.GetProperty("status").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task run_records_exiting_model_as_unavailable_with_none_proof_level()
    {
        // Given a model command that exits non-zero without output
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelCommand = OperatingSystem.IsWindows()
            ? "more > nul && exit /b 17"
            : "cat > /dev/null; exit 17";

        // When the benchmark CLI runs that model
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "nonzero-v1", modelCommand);

        // Then it records an unavailable row without claiming parse or specs proof
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("nonzero-v1: unavailable", stdout);
        using var result = await ReadModelResult(suiteDirectory, "nonzero-v1");
        Assert.Equal("unavailable", result.RootElement.GetProperty("status").GetString());
        Assert.Equal("none", result.RootElement.GetProperty("proof_level").GetString());
        Assert.Contains("exited 17", result.RootElement.GetProperty("error").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task run_keeps_legacy_history_rows_byte_for_byte_when_appending_new_results()
    {
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var resultsDirectory = Path.Combine(suiteDirectory, "results");
        Directory.CreateDirectory(resultsDirectory);
        var legacyRow = "{\"model\":\"legacy-v1\",\"task\":\"001-write-greeting\",\"status\":\"completed\"}\n";
        await File.WriteAllTextAsync(Path.Combine(resultsDirectory, "history.jsonl"), legacyRow);

        var modelOutput = Path.Combine(workspace.Root, "model-output.phrase");
        await File.WriteAllTextAsync(modelOutput, """
            module Greeting

            phrase "say hello"
              native csharp
                return;
            """);
        var modelCommand = PrintFileCommand(modelOutput);

        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "legacy-appended-v1", modelCommand);

        Assert.True(exitCode == 0, stdout + stderr);
        var rows = await File.ReadAllLinesAsync(Path.Combine(resultsDirectory, "history.jsonl"));
        Assert.Equal(2, rows.Length);
        Assert.Equal(legacyRow.TrimEnd(), rows[0]);
    }

    [Fact]
    public async Task run_inlines_task_file_bodies_in_the_assembled_prompt()
    {
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = Path.Combine(workspace.Root, "bench");
        var taskDirectory = Path.Combine(suiteDirectory, "tasks", "001-inline-source");
        Directory.CreateDirectory(taskDirectory);
        await File.WriteAllTextAsync(Path.Combine(suiteDirectory, "suite.json"), """
            {
              "version": 1,
              "tasks": [
                {
                  "id": "001-inline-source",
                  "kind": "write",
                  "prompt": "tasks/001-inline-source/prompt.md",
                  "holdout": false
                }
              ]
            }
            """);
        await File.WriteAllTextAsync(Path.Combine(taskDirectory, "prompt.md"), "Read source.phrase and return it unchanged.");
        await File.WriteAllTextAsync(Path.Combine(taskDirectory, "source.phrase"), """
            module Greeting

            phrase "say hello"
              native csharp
                return;
            """);
        var capturedPrompt = Path.Combine(workspace.Root, "captured-prompt.txt");
        var modelCommand = OperatingSystem.IsWindows()
            ? $"python -c \"import sys, pathlib; pathlib.Path(r'{capturedPrompt}').write_text(sys.stdin.read()); print('module Greeting\\n\\nphrase \\\"say hello\\\"\\n  native csharp\\n    return;')\""
            : $"python -c \"import sys, pathlib; pathlib.Path(r'{capturedPrompt}').write_text(sys.stdin.read()); print('module Greeting\\n\\nphrase \\\"say hello\\\"\\n  native csharp\\n    return;')\"";

        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "inline-v1", modelCommand);

        Assert.True(exitCode == 0, stdout + stderr);
        var promptText = await File.ReadAllTextAsync(capturedPrompt);
        Assert.Contains("module Greeting", promptText, StringComparison.Ordinal);
        Assert.Contains("return;", promptText, StringComparison.Ordinal);
        Assert.Contains("source.phrase", promptText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task run_labels_parse_resolve_when_the_emitter_is_still_unimplemented()
    {
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var capturePath = Path.Combine(workspace.Root, "captured.txt");
        var modelCommand = OperatingSystem.IsWindows()
            ? $"python -c \"import sys; from pathlib import Path; Path(r'{capturePath}').write_text(sys.stdin.read()); print('module Greeting\\n\\nphrase \\\"say hello\\\"\\n  native csharp\\n    return;')\""
            : $"python -c \"import sys; from pathlib import Path; Path(r'{capturePath}').write_text(sys.stdin.read()); print('module Greeting\\n\\nphrase \\\"say hello\\\"\\n  native csharp\\n    return;')\"";

        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "parse-v1", modelCommand);

        Assert.True(exitCode == 0, stdout + stderr);
        using var result = await ReadModelResult(suiteDirectory, "parse-v1");
        Assert.True(result.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal("parse-resolve", result.RootElement.GetProperty("proof_level").GetString());
    }

    [Fact]
    public async Task run_rerun_appends_rows_without_rewriting_old_history_entries()
    {
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelOutput = Path.Combine(workspace.Root, "model-output.phrase");
        await File.WriteAllTextAsync(modelOutput, """
            module Greeting

            phrase "say hello"
              native csharp
                return;
            """);
        var modelCommand = PrintFileCommand(modelOutput);

        var first = await RunBenchmark(suiteDirectory, "grok-rerun", modelCommand, "--label", "rerun-085");
        var second = await RunBenchmark(suiteDirectory, "grok-rerun", modelCommand, "--label", "rerun-085-2");

        Assert.True(first.ExitCode == 0, first.Stdout + first.Stderr);
        Assert.True(second.ExitCode == 0, second.Stdout + second.Stderr);
        var rows = await File.ReadAllLinesAsync(Path.Combine(suiteDirectory, "results", "history.jsonl"));
        Assert.Equal(2, rows.Length);
        using var firstRow = JsonDocument.Parse(rows[0]);
        using var secondRow = JsonDocument.Parse(rows[1]);
        Assert.Equal("rerun-085", firstRow.RootElement.GetProperty("label").GetString());
        Assert.Equal("rerun-085-2", secondRow.RootElement.GetProperty("label").GetString());
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
    public async Task run_retries_an_empty_reply_once_and_records_it_as_unavailable()
    {
        // Given a model that repeatedly produces empty output
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var attempts = Path.Combine(workspace.Root, "attempts.txt");
        var modelCommand = OperatingSystem.IsWindows()
            ? $"more > nul && echo attempt>>\"{attempts}\""
            : $"cat > /dev/null; echo attempt >> '{attempts}'";

        // When the benchmark CLI tries to capture the reply
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "empty-v1", modelCommand);

        // Then it retries once, records an auditable unavailable result, and excludes the model from the baseline
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("empty-v1: unavailable", stdout);
        Assert.Contains("empty-v1: excluded from baseline; non-empty replies 0/1 (minimum 75%)", stdout);
        Assert.Equal(2, (await File.ReadAllLinesAsync(attempts)).Length);
        using var result = await ReadModelResult(suiteDirectory, "empty-v1");
        Assert.Equal("unavailable", result.RootElement.GetProperty("status").GetString());
        Assert.Equal(2, result.RootElement.GetProperty("attempts").GetInt32());
        Assert.Equal("empty reply", result.RootElement.GetProperty("error").GetString());
        Assert.Equal(string.Empty, result.RootElement.GetProperty("reply_head").GetString());
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

        // Then it records the timeout, cleans up without crashing, and returns success so another model can run
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("slow-v1: unavailable", stdout);
        Assert.DoesNotContain("IOException", stderr);
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
        var modelCommand = PrintFileCommand(modelOutput);

        // When the benchmark CLI runs the default development slice
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "holdout-v1", modelCommand);

        // Then only the non-holdout task is evaluated, leaving the holdout for keep-or-revert
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("001-write-greeting", stdout);
        Assert.DoesNotContain("002-write-hello", stdout);
    }

    [Fact]
    public async Task self_check_runs_every_task_twice_and_reports_zero_noise()
    {
        // Given an isolated copy of the full benchmark suite
        using var workspace = TempWorkspace.Create();
        var sourceSuiteDirectory = Path.GetDirectoryName(RepoPaths.Find("bench", "suite.json"))!;
        var suiteDirectory = Path.Combine(workspace.Root, "bench");
        Directory.CreateDirectory(suiteDirectory);
        File.Copy(Path.Combine(sourceSuiteDirectory, "suite.json"), Path.Combine(suiteDirectory, "suite.json"));
        CopyDirectory(Path.Combine(sourceSuiteDirectory, "tasks"), Path.Combine(suiteDirectory, "tasks"));

        // When the deterministic self-check runs
        var (exitCode, stdout, stderr) = await RunSelfCheck(suiteDirectory);

        // Then every task has two schema-valid history rows and the reported noise is zero
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("noise: pass_rate=0 points", stdout);
        using var suite = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(suiteDirectory, "suite.json")));
        var taskCount = suite.RootElement.GetProperty("tasks").GetArrayLength();
        var rows = await File.ReadAllLinesAsync(Path.Combine(suiteDirectory, "results", "self-check", "history.jsonl"));
        Assert.Equal(taskCount * 2, rows.Length);
        foreach (var row in rows)
        {
            using var result = JsonDocument.Parse(row);
            foreach (var property in new[] { "model", "task", "passed", "attempts", "tokens_in", "tokens_out", "diagnostics", "misread" })
                Assert.True(result.RootElement.TryGetProperty(property, out _), $"Missing result property '{property}'.");
        }
    }

    [Fact]
    public async Task self_check_writes_only_under_self_check_and_leaves_baseline_history_untouched()
    {
        // Given an isolated copy of the full benchmark suite with existing baseline history
        using var workspace = TempWorkspace.Create();
        var sourceSuiteDirectory = Path.GetDirectoryName(RepoPaths.Find("bench", "suite.json"))!;
        var suiteDirectory = Path.Combine(workspace.Root, "bench");
        Directory.CreateDirectory(suiteDirectory);
        File.Copy(Path.Combine(sourceSuiteDirectory, "suite.json"), Path.Combine(suiteDirectory, "suite.json"));
        CopyDirectory(Path.Combine(sourceSuiteDirectory, "tasks"), Path.Combine(suiteDirectory, "tasks"));
        var resultsDirectory = Path.Combine(suiteDirectory, "results");
        Directory.CreateDirectory(resultsDirectory);
        var seededHistory = "{\"model\":\"codex-baseline\",\"task\":\"001-write-greeting\",\"status\":\"completed\"}" + Environment.NewLine;
        await File.WriteAllTextAsync(Path.Combine(resultsDirectory, "history.jsonl"), seededHistory);

        // When the deterministic self-check runs
        var (exitCode, stdout, stderr) = await RunSelfCheck(suiteDirectory);

        // Then baseline history is untouched and self-check results stay in their own directory
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Equal(seededHistory, await File.ReadAllTextAsync(Path.Combine(resultsDirectory, "history.jsonl")));
        var selfCheckDirectory = Path.Combine(resultsDirectory, "self-check");
        var selfCheckHistory = Path.Combine(selfCheckDirectory, "history.jsonl");
        Assert.True(File.Exists(selfCheckHistory));
        var rows = await File.ReadAllLinesAsync(selfCheckHistory);
        Assert.NotEmpty(rows);
        foreach (var row in rows)
        {
            using var result = JsonDocument.Parse(row);
            Assert.StartsWith("self-check", result.RootElement.GetProperty("model").GetString());
        }
        Assert.True(File.Exists(Path.Combine(selfCheckDirectory, "self-check-v1.json")));
    }

    [Fact]
    public async Task run_docs_in_context_puts_grammar_and_orders_verbatim_ahead_of_the_task()
    {
        // Given a development task and the repository grammar and orders example
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        CopyRepositoryDocs(workspace.Root);
        var capturedPrompt = Path.Combine(workspace.Root, "captured-prompt.txt");
        var taskPrompt = await File.ReadAllTextAsync(
            Path.Combine(suiteDirectory, "tasks", "001-write-greeting", "prompt.md"));
        var grammar = await File.ReadAllTextAsync(Path.Combine(workspace.Root, "docs", "GRAMMAR.md"));
        var orders = await File.ReadAllTextAsync(Path.Combine(
            workspace.Root, "tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.phrase"));

        // When the task runs in docs-in-context mode, resolving docs from the suite parent
        var (exitCode, stdout, stderr) = await RunBenchmark(
            suiteDirectory,
            "docs-verbatim-v1",
            CapturePromptCommand(capturedPrompt),
            "--mode",
            "docs-in-context");

        // Then the docs block is verbatim ahead of the task text and the row records both hashes
        Assert.True(exitCode == 0, stdout + stderr);
        var captured = await File.ReadAllTextAsync(capturedPrompt);
        var grammarAt = captured.IndexOf(grammar, StringComparison.Ordinal);
        var ordersAt = captured.IndexOf(orders, StringComparison.Ordinal);
        Assert.True(grammarAt >= 0, "Grammar was not copied into the prompt verbatim.");
        Assert.True(ordersAt >= 0, "orders.phrase was not copied into the prompt verbatim.");
        Assert.True(grammarAt < ordersAt, captured);
        Assert.EndsWith(taskPrompt, captured, StringComparison.Ordinal);
        Assert.True(ordersAt < captured.Length - taskPrompt.Length, captured);
        var docsBlock = captured[..^taskPrompt.Length];
        using var result = await ReadModelResult(suiteDirectory, "docs-verbatim-v1");
        Assert.Equal("docs-in-context", result.RootElement.GetProperty("mode").GetString());
        Assert.Equal(Sha256Hex(docsBlock), result.RootElement.GetProperty("docs_hash").GetString());
        Assert.Equal(Sha256Hex(captured), result.RootElement.GetProperty("prompt_hash").GetString());
    }

    [Fact]
    public async Task run_docs_hash_changes_when_a_docs_file_changes_and_matches_when_it_does_not()
    {
        // Given two identical docs roots and a third whose orders example differs
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var sameRoot = Path.Combine(workspace.Root, "docs-same");
        var sameCopy = Path.Combine(workspace.Root, "docs-same-copy");
        var changedRoot = Path.Combine(workspace.Root, "docs-changed");
        await WriteDocs(sameRoot, "grammar one\n", "example one\n");
        await WriteDocs(sameCopy, "grammar one\n", "example one\n");
        await WriteDocs(changedRoot, "grammar one\n", "example two\n");
        var modelCommand = PrintFileCommand(await WritePassingPhrase(workspace.Root));

        // When the same task is run against each root
        var first = await RunBenchmark(
            suiteDirectory, "hash-same", modelCommand, "--mode", "docs-in-context", "--docs-root", sameRoot, "--custom-docs");
        var second = await RunBenchmark(
            suiteDirectory, "hash-same-copy", modelCommand, "--mode", "docs-in-context", "--docs-root", sameCopy, "--custom-docs");
        var third = await RunBenchmark(
            suiteDirectory, "hash-changed", modelCommand, "--mode", "docs-in-context", "--docs-root", changedRoot, "--custom-docs");

        // Then the docs hash follows the file bytes and ignores an identical copy, and the row is custom
        Assert.True(first.ExitCode == 0, first.Stdout + first.Stderr);
        Assert.True(second.ExitCode == 0, second.Stdout + second.Stderr);
        Assert.True(third.ExitCode == 0, third.Stdout + third.Stderr);
        using var same = await ReadModelResult(suiteDirectory, "hash-same");
        using var copy = await ReadModelResult(suiteDirectory, "hash-same-copy");
        using var changed = await ReadModelResult(suiteDirectory, "hash-changed");
        var sameHash = same.RootElement.GetProperty("docs_hash").GetString();
        Assert.False(string.IsNullOrEmpty(sameHash));
        Assert.Equal(sameHash, copy.RootElement.GetProperty("docs_hash").GetString());
        Assert.NotEqual(sameHash, changed.RootElement.GetProperty("docs_hash").GetString());
        Assert.Equal("docs-in-context-custom", same.RootElement.GetProperty("mode").GetString());
        Assert.Equal("docs-in-context-custom", copy.RootElement.GetProperty("mode").GetString());
        Assert.Equal("docs-in-context-custom", changed.RootElement.GetProperty("mode").GetString());
    }

    [Fact]
    public async Task run_default_mode_keeps_the_prompt_hash_from_before_docs_in_context()
    {
        // Given the greeting fixture whose prompt hash was fixed before this mode existed
        const string promptHashBeforeDocsInContext = "a7bd17c3b580099223bd99f0db9c3377ba2419a1f568448dd21fda80a7fccade";
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var capturedPrompt = Path.Combine(workspace.Root, "captured-prompt.txt");
        var taskPrompt = await File.ReadAllTextAsync(
            Path.Combine(suiteDirectory, "tasks", "001-write-greeting", "prompt.md"));
        Assert.Equal(promptHashBeforeDocsInContext, Sha256Hex(taskPrompt));

        // When the benchmark runs with no --mode flag
        var (exitCode, stdout, stderr) = await RunBenchmark(
            suiteDirectory, "zero-shot-default-v1", CapturePromptCommand(capturedPrompt));

        // Then the sent prompt and its stored hash are unchanged, and docs are not charged
        Assert.True(exitCode == 0, stdout + stderr);
        var captured = await File.ReadAllTextAsync(capturedPrompt);
        Assert.Equal(taskPrompt, captured);
        using var result = await ReadModelResult(suiteDirectory, "zero-shot-default-v1");
        Assert.Equal(promptHashBeforeDocsInContext, result.RootElement.GetProperty("prompt_hash").GetString());
        Assert.Equal("zero-shot", result.RootElement.GetProperty("mode").GetString());
        Assert.Equal(string.Empty, result.RootElement.GetProperty("docs_hash").GetString());

        var explicitRun = await RunBenchmark(
            suiteDirectory, "zero-shot-explicit-v1", CapturePromptCommand(capturedPrompt), "--mode", "zero-shot");
        Assert.True(explicitRun.ExitCode == 0, explicitRun.Stdout + explicitRun.Stderr);
        Assert.Equal(taskPrompt, await File.ReadAllTextAsync(capturedPrompt));
        using var explicitResult = await ReadModelResult(suiteDirectory, "zero-shot-explicit-v1");
        Assert.Equal(promptHashBeforeDocsInContext, explicitResult.RootElement.GetProperty("prompt_hash").GetString());
        Assert.Equal("zero-shot", explicitResult.RootElement.GetProperty("mode").GetString());
        Assert.Equal(string.Empty, explicitResult.RootElement.GetProperty("docs_hash").GetString());
    }

    [Fact]
    public async Task run_records_cold_profile_metadata_and_uses_an_empty_workdir_at_launch()
    {
        // Given a fresh scratch directory and a fake Grok command that lists its working directory
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelOutput = Path.Combine(workspace.Root, "model-output.txt");
        await File.WriteAllTextAsync(modelOutput, """
            module Greeting

            phrase "say hello"
              native csharp
                return;
            """);
        var modelCommand = OperatingSystem.IsWindows()
            ? $"python -c \"import pathlib, os; listing = os.listdir('.'); print(listing); path = pathlib.Path(r'{modelOutput}'); print(path.read_text())\""
            : $"python -c \"import pathlib, os; listing = os.listdir('.'); print(listing); path = pathlib.Path(r'{modelOutput}'); print(path.read_text())\"";

        // When the benchmark runs the profile in its own scratch workspace
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "grok-probe", modelCommand, "--cold");

        // Then the row records the profile as cold, with the flags used, and the workdir started empty
        Assert.True(exitCode == 0, stdout + stderr);
        using var result = await ReadModelResult(suiteDirectory, "grok-probe");
        Assert.True(result.RootElement.GetProperty("cold").GetBoolean());
        Assert.False(string.IsNullOrEmpty(result.RootElement.GetProperty("cold_flags").GetString()));
        Assert.Equal("There is no workspace and no files to read; answer directly in this reply without looking anything up.", result.RootElement.GetProperty("cold_preamble").GetString());
        Assert.True(result.RootElement.GetProperty("workdir_empty_at_launch").GetBoolean());
        var replyHead = result.RootElement.GetProperty("reply_head").GetString();
        Assert.StartsWith("[]", replyHead ?? string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain("bench", replyHead ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Phrase.Acceptance.Tests", replyHead ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task run_uses_a_fresh_empty_workdir_on_retry_attempts()
    {
        // Given a Grok model that prints its launch directory and fails once before succeeding
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var marker = Path.Combine(workspace.Root, "retry-marker.txt");
        var output = Path.Combine(workspace.Root, "valid-output.txt");
        await File.WriteAllTextAsync(output, """
            module Greeting

            phrase "say hello"
              native csharp
                return;
            """);
        var modelCommand = OperatingSystem.IsWindows()
            ? $"python -c \"import os, pathlib; print(os.listdir('.')); marker = pathlib.Path(r'{marker}'); import sys; sys.exit(1) if not marker.exists() else None\" & if exist \"{marker}\" (type \"{output}\") else (echo invalid & type nul>\"{marker}\" & exit /b 1)"
            : $"python -c \"import os, pathlib; print(os.listdir('.')); marker = pathlib.Path(r'{marker}'); import sys; sys.exit(1) if not marker.exists() else None\"; if [ -f '{marker}' ]; then cat '{output}'; else echo invalid; touch '{marker}'; exit 1; fi";

        // When the benchmark retries that model in its cold Grok profile
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "grok-probe", modelCommand, "--cold");

        // Then every launch begins from an empty directory, including the retry after the failed first attempt
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Equal(2, CountOccurrences(stdout, "[]"));
        using var result = await ReadModelResult(suiteDirectory, "grok-probe");
        Assert.True(result.RootElement.GetProperty("workdir_empty_at_launch").GetBoolean());
    }

    [Fact]
    public void read_stored_result_treats_missing_cold_fields_as_warm_legacy_rows()
    {
        // Given a row from before cold-run metadata existed
        var legacy = "{\"model\":\"legacy-v1\",\"task\":\"001-write-greeting\",\"passed\":true,\"attempts\":1,\"tokens_in\":42,\"tokens_out\":1,\"token_measurement\":\"estimated\",\"diagnostics\":[],\"misread\":false,\"prompt_hash\":\"abc\",\"status\":\"completed\"}";

        // When it is read back
        var result = BenchmarkResults.ReadStoredResult(legacy);

        // Then the cold metadata falls back to a warm legacy default rather than a lie
        Assert.False(result.Cold);
        Assert.Equal(string.Empty, result.ColdFlags);
        Assert.Equal(string.Empty, result.ColdPreamble);
        Assert.Equal(string.Empty, result.ColdReason);
        Assert.False(result.WorkdirEmptyAtLaunch);
    }

    [Fact]
    public async Task run_records_a_max_turns_error_as_unavailable_with_the_reason()
    {
        // Given a model that refuses to answer within its turn cap
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelCommand = OperatingSystem.IsWindows()
            ? "echo Error: max turns reached 1>&2 & exit /b 1"
            : "echo 'Error: max turns reached' >&2; exit 1";

        // When the benchmark runs it
        var (exitCode, stdout, stderr) = await RunBenchmark(suiteDirectory, "turn-limit-v1", modelCommand);

        // Then the result is unavailable with the turn-limit reason instead of a language failure
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("turn-limit-v1: unavailable", stdout, StringComparison.Ordinal);
        using var result = await ReadModelResult(suiteDirectory, "turn-limit-v1");
        Assert.Equal("unavailable", result.RootElement.GetProperty("status").GetString());
        Assert.Contains("max turns reached", result.RootElement.GetProperty("error").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task run_docs_in_context_counts_documentation_tokens_above_the_same_zero_shot_task()
    {
        // Given one task, one fake model, and a docs root
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var docsRoot = Path.Combine(workspace.Root, "docs-root");
        await WriteDocs(docsRoot, "grammar for the token charge\n", "orders example for the token charge\n");
        var capturedPrompt = Path.Combine(workspace.Root, "captured-prompt.txt");
        var modelCommand = CapturePromptCommand(capturedPrompt);

        // When that task is scored in both modes
        var zeroShot = await RunBenchmark(suiteDirectory, "tokens-zero-v1", modelCommand);
        var zeroPrompt = await File.ReadAllTextAsync(capturedPrompt);
        var docsInContext = await RunBenchmark(
            suiteDirectory, "tokens-docs-v1", modelCommand, "--mode", "docs-in-context", "--docs-root", docsRoot, "--custom-docs");
        var docsPrompt = await File.ReadAllTextAsync(capturedPrompt);

        // Then the docs-in-context input estimate grows by at least the docs block
        Assert.True(zeroShot.ExitCode == 0, zeroShot.Stdout + zeroShot.Stderr);
        Assert.True(docsInContext.ExitCode == 0, docsInContext.Stdout + docsInContext.Stderr);
        Assert.EndsWith(zeroPrompt, docsPrompt, StringComparison.Ordinal);
        var docsBlock = docsPrompt[..^zeroPrompt.Length];
        var docsTokens = EstimateTokens(docsBlock);
        using var zeroResult = await ReadModelResult(suiteDirectory, "tokens-zero-v1");
        using var docsResult = await ReadModelResult(suiteDirectory, "tokens-docs-v1");
        var zeroTokens = zeroResult.RootElement.GetProperty("tokens_in").GetInt32();
        var docsTokensIn = docsResult.RootElement.GetProperty("tokens_in").GetInt32();
        Assert.True(
            docsTokensIn - zeroTokens >= docsTokens,
            $"tokens_in {docsTokensIn} is not at least {docsTokens} above zero-shot {zeroTokens}.");
        Assert.Equal("docs-in-context-custom", docsResult.RootElement.GetProperty("mode").GetString());
    }

    [Fact]
    public async Task run_docs_in_context_charges_the_docs_block_on_every_attempt()
    {
        // Given a model that fails once and then emits Phrase, and a docs root
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var docsRoot = Path.Combine(workspace.Root, "docs-root");
        await WriteDocs(docsRoot, "grammar charged on each attempt\n", "orders charged on each attempt\n");
        var capturedPrompt = Path.Combine(workspace.Root, "captured-once.txt");
        var measured = await RunBenchmark(
            suiteDirectory, "tokens-measure-v1", CapturePromptCommand(capturedPrompt),
            "--mode", "docs-in-context", "--docs-root", docsRoot, "--custom-docs");
        Assert.True(measured.ExitCode == 0, measured.Stdout + measured.Stderr);
        var docsPrompt = await File.ReadAllTextAsync(capturedPrompt);
        var taskPrompt = await File.ReadAllTextAsync(
            Path.Combine(suiteDirectory, "tasks", "001-write-greeting", "prompt.md"));
        Assert.EndsWith(taskPrompt, docsPrompt, StringComparison.Ordinal);
        var docsTokens = EstimateTokens(docsPrompt[..^taskPrompt.Length]);
        var validOutput = await WritePassingPhrase(workspace.Root);

        // When the same retrying model runs in both modes
        var zeroShot = await RunBenchmark(
            suiteDirectory, "retry-tokens-zero-v1", RetryThenPassCommand(workspace.Root, validOutput, "zero"));
        var docsInContext = await RunBenchmark(
            suiteDirectory, "retry-tokens-docs-v1", RetryThenPassCommand(workspace.Root, validOutput, "docs"),
            "--mode", "docs-in-context", "--docs-root", docsRoot, "--custom-docs");

        // Then each attempt, not only the first, includes the docs block estimate
        Assert.True(zeroShot.ExitCode == 0, zeroShot.Stdout + zeroShot.Stderr);
        Assert.True(docsInContext.ExitCode == 0, docsInContext.Stdout + docsInContext.Stderr);
        using var zeroResult = await ReadModelResult(suiteDirectory, "retry-tokens-zero-v1");
        using var docsResult = await ReadModelResult(suiteDirectory, "retry-tokens-docs-v1");
        Assert.Equal(2, zeroResult.RootElement.GetProperty("attempts").GetInt32());
        Assert.Equal(2, docsResult.RootElement.GetProperty("attempts").GetInt32());
        var increase = docsResult.RootElement.GetProperty("tokens_in").GetInt32()
            - zeroResult.RootElement.GetProperty("tokens_in").GetInt32();
        Assert.True(
            increase >= docsTokens * 2,
            $"Two attempts increased tokens_in by {increase}, which is below twice the docs estimate {docsTokens}.");
        Assert.Equal("docs-in-context-custom", docsResult.RootElement.GetProperty("mode").GetString());
    }

    [Fact]
    public async Task run_appends_a_new_row_after_a_legacy_row_and_keeps_the_legacy_line_byte_identical()
    {
        // Given a history file whose first row has no mode field
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var docsRoot = Path.Combine(workspace.Root, "docs-root");
        await WriteDocs(docsRoot, "grammar\n", "orders\n");
        var resultsDirectory = Path.Combine(suiteDirectory, "results");
        Directory.CreateDirectory(resultsDirectory);
        var legacyRow = "{\"model\":\"legacy-v1\",\"task\":\"001-write-greeting\",\"passed\":true,\"attempts\":1,\"tokens_in\":42,\"tokens_out\":1,\"token_measurement\":\"estimated\",\"diagnostics\":[],\"misread\":false,\"prompt_hash\":\"abc\",\"status\":\"completed\"}\n";
        var historyPath = Path.Combine(resultsDirectory, "history.jsonl");
        await File.WriteAllTextAsync(historyPath, legacyRow);
        var modelCommand = PrintFileCommand(await WritePassingPhrase(workspace.Root));

        // When a new custom-docs row is appended
        var (exitCode, stdout, stderr) = await RunBenchmark(
            suiteDirectory, "append-mode-v1", modelCommand, "--mode", "docs-in-context", "--docs-root", docsRoot, "--custom-docs");

        // Then the legacy line is byte-identical and the new row carries the custom mode
        Assert.True(exitCode == 0, stdout + stderr);
        var historyBytes = await File.ReadAllBytesAsync(historyPath);
        var legacyBytes = Encoding.UTF8.GetBytes(legacyRow);
        Assert.Equal(legacyBytes, historyBytes[..legacyBytes.Length]);
        var rows = await File.ReadAllLinesAsync(historyPath);
        Assert.Equal(2, rows.Length);
        Assert.DoesNotContain("\"mode\"", rows[0], StringComparison.Ordinal);
        using var appended = JsonDocument.Parse(rows[1]);
        Assert.Equal("docs-in-context-custom", appended.RootElement.GetProperty("mode").GetString());
        Assert.False(string.IsNullOrEmpty(appended.RootElement.GetProperty("docs_hash").GetString()));
    }

    [Fact]
    public async Task usage_mentions_the_custom_docs_flag()
    {
        // Given the benchmark CLI with no arguments
        var (exitCode, _, stderr) = await RunBenchmarkProcess();

        // Then usage names the flag that keeps invented docs out of the baseline
        Assert.Equal(1, exitCode);
        Assert.Contains("--custom-docs", stderr, StringComparison.Ordinal);
        Assert.Contains("--cold", stderr, StringComparison.Ordinal);
    }

    [Fact]
    public async Task run_refuses_an_explicit_docs_root_that_is_not_the_repository_without_custom_docs()
    {
        // Given fixture docs outside the repository root derived from the suite
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var docsRoot = Path.Combine(workspace.Root, "invented-docs");
        await WriteDocs(docsRoot, "invented grammar\n", "invented orders\n");
        var modelCommand = PrintFileCommand(await WritePassingPhrase(workspace.Root));
        var fullDocsRoot = Path.GetFullPath(docsRoot);

        // When docs-in-context is pointed at that folder without --custom-docs
        var (exitCode, stdout, stderr) = await RunBenchmark(
            suiteDirectory, "refused-docs-v1", modelCommand,
            "--mode", "docs-in-context", "--docs-root", docsRoot);

        // Then the run is refused and no row is stored as docs-in-context
        Assert.Equal(1, exitCode);
        Assert.Contains(fullDocsRoot, stderr, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--custom-docs", stderr, StringComparison.Ordinal);
        Assert.Contains("not the repository root", stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("refused-docs-v1: passed", stdout, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(suiteDirectory, "results", "refused-docs-v1.json")));
    }

    [Fact]
    public async Task run_custom_docs_is_labeled_docs_in_context_custom_and_not_counted_in_a_baseline()
    {
        // Given fixture docs outside the repository root derived from the suite
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var docsRoot = Path.Combine(workspace.Root, "invented-docs");
        await WriteDocs(docsRoot, "invented grammar\n", "invented orders\n");
        var modelCommand = PrintFileCommand(await WritePassingPhrase(workspace.Root));

        // When that folder is passed with --docs-root and --custom-docs
        var (exitCode, stdout, stderr) = await RunBenchmark(
            suiteDirectory, "custom-docs-v1", modelCommand,
            "--mode", "docs-in-context", "--docs-root", docsRoot, "--custom-docs");

        // Then the row is labeled custom, keeps the docs hash, and is not a baseline run
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains(
            "custom-docs-v1: custom docs; this run is not counted in a baseline",
            stdout,
            StringComparison.Ordinal);
        using var result = await ReadModelResult(suiteDirectory, "custom-docs-v1");
        Assert.Equal("docs-in-context-custom", result.RootElement.GetProperty("mode").GetString());
        Assert.False(string.IsNullOrEmpty(result.RootElement.GetProperty("docs_hash").GetString()));
    }

    [Fact]
    public async Task run_accepts_the_canonical_repository_root_as_docs_root_without_custom_docs()
    {
        // Given the repository root derived from the suite, passed explicitly
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var canonicalRoot = Path.GetFullPath(Path.Combine(suiteDirectory, ".."));
        await WriteDocs(canonicalRoot, "repository grammar\n", "repository orders\n");
        var modelCommand = PrintFileCommand(await WritePassingPhrase(workspace.Root));

        // When docs-in-context uses that path as --docs-root and omits --custom-docs
        var (exitCode, stdout, stderr) = await RunBenchmark(
            suiteDirectory, "canonical-docs-v1", modelCommand,
            "--mode", "docs-in-context", "--docs-root", canonicalRoot);

        // Then the run is accepted and the row stays docs-in-context
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.DoesNotContain("not counted in a baseline", stdout, StringComparison.Ordinal);
        using var result = await ReadModelResult(suiteDirectory, "canonical-docs-v1");
        Assert.Equal("docs-in-context", result.RootElement.GetProperty("mode").GetString());
        Assert.False(string.IsNullOrEmpty(result.RootElement.GetProperty("docs_hash").GetString()));
    }

    [Fact]
    public void read_stored_result_treats_a_missing_mode_as_zero_shot()
    {
        // Given a stored row from before mode existed, and a row that names its mode
        var legacy = "{\"model\":\"legacy-v1\",\"task\":\"001-write-greeting\",\"passed\":true,\"attempts\":1,\"tokens_in\":42,\"tokens_out\":1,\"token_measurement\":\"estimated\",\"diagnostics\":[],\"misread\":false,\"prompt_hash\":\"abc\",\"status\":\"completed\"}";
        var labeled = "{\"model\":\"docs-v1\",\"task\":\"001-write-greeting\",\"passed\":false,\"attempts\":2,\"tokens_in\":10,\"tokens_out\":4,\"token_measurement\":\"estimated\",\"diagnostics\":[\"no_phrase_found\"],\"misread\":false,\"prompt_hash\":\"def\",\"status\":\"completed\",\"mode\":\"docs-in-context\",\"docs_hash\":\"abc123\"}";

        // When the history reader loads each row
        var legacyResult = BenchmarkResults.ReadStoredResult(legacy);
        var labeledResult = BenchmarkResults.ReadStoredResult(labeled);

        // Then a missing mode reads as zero-shot and an explicit mode is kept
        Assert.Equal("zero-shot", legacyResult.Mode);
        Assert.Equal(string.Empty, legacyResult.DocsHash);
        Assert.Equal("abc", legacyResult.PromptHash);
        Assert.Equal("legacy-v1", legacyResult.Model);
        Assert.Equal("docs-in-context", labeledResult.Mode);
        Assert.Equal("abc123", labeledResult.DocsHash);
        Assert.Equal("def", labeledResult.PromptHash);
    }

    private static int CountOccurrences(string text, string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    private static async Task<string> WriteArgumentRecordingShim(string workspaceRoot, string model, string argvLog)
    {
        var shimPath = Path.Combine(workspaceRoot, OperatingSystem.IsWindows() ? $"{model}.cmd" : $"{model}.sh");
        var shim = OperatingSystem.IsWindows()
            ? $"""
                @echo off
                more > nul
                echo(%*>>"{argvLog}"
                echo module Greeting
                echo.
                echo phrase "say hello"
                echo   native csharp
                echo     return;
                """
            : $"""
                #!/bin/sh
                cat > /dev/null
                printf '%s\n' "$*" >> '{argvLog}'
                cat <<'PHRASE'
                module Greeting

                phrase "say hello"
                  native csharp
                    return;
                PHRASE
                """;
        await File.WriteAllTextAsync(shimPath, shim);
        return OperatingSystem.IsWindows() ? shimPath : $"sh {shimPath}";
    }

    private static string PrintFileCommand(string path) =>
        OperatingSystem.IsWindows()
            ? $"more > nul && type \"{path}\""
            : $"cat > /dev/null && cat \"{path}\"";

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

    private static async Task<JsonDocument> ReadModelResult(string suiteDirectory, string model) =>
        JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(suiteDirectory, "results", $"{model}.json")));

    private static async Task<string> RunAndReadReplyHead(string reply, string model)
    {
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var modelOutput = Path.Combine(workspace.Root, "model-output.txt");
        await File.WriteAllTextAsync(modelOutput, reply);
        await RunBenchmark(suiteDirectory, model, PrintFileCommand(modelOutput));
        using var result = await ReadModelResult(suiteDirectory, model);
        return result.RootElement.GetProperty("reply_head").GetString()!;
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunBenchmark(
        string suiteDirectory, string model, string modelCommand, params string[] additionalArguments)
    {
        var arguments = new List<string>
        {
            "run", "--suite", Path.Combine(suiteDirectory, "suite.json"),
            "--model", model, "--model-command", modelCommand,
        };
        arguments.AddRange(additionalArguments);
        return await RunBenchmarkProcess(arguments.ToArray());
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunSelfCheck(string suiteDirectory)
        => await RunBenchmarkProcess(
            "--self-check", "--suite", Path.Combine(suiteDirectory, "suite.json"));

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunBenchmarkProcess(
        params string[] benchmarkArguments)
    {
        var runner = RepoPaths.Find("tools", "Phrase.Bench", "Phrase.Bench.csproj");
        var startInfo = new System.Diagnostics.ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in new[] { "run", "--project", runner, "--" }.Concat(benchmarkArguments))
            startInfo.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(startInfo)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout, stderr);
    }

    private static void CopyDirectory(string source, string destination)
    {
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destinationFile = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
            File.Copy(file, destinationFile);
        }
    }

    private static void CopyRepositoryDocs(string docsRoot)
    {
        CopyFile(RepoPaths.Find("docs", "GRAMMAR.md"), Path.Combine(docsRoot, "docs", "GRAMMAR.md"));
        CopyFile(
            RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.phrase"),
            Path.Combine(docsRoot, "tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.phrase"));
    }

    private static void CopyFile(string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination);
    }

    private static async Task WriteDocs(string docsRoot, string grammar, string orders)
    {
        var grammarPath = Path.Combine(docsRoot, "docs", "GRAMMAR.md");
        var ordersPath = Path.Combine(docsRoot, "tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.phrase");
        Directory.CreateDirectory(Path.GetDirectoryName(grammarPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(ordersPath)!);
        await File.WriteAllTextAsync(grammarPath, grammar);
        await File.WriteAllTextAsync(ordersPath, orders);
    }

    private static async Task<string> WritePassingPhrase(string workspaceRoot)
    {
        var modelOutput = Path.Combine(workspaceRoot, "model-output.phrase");
        await File.WriteAllTextAsync(modelOutput, """
            module Greeting

            phrase "say hello"
              native csharp
                return;
            """);
        return modelOutput;
    }

    private static string CapturePromptCommand(string capturedPrompt)
    {
        var writeAndPrint = "import sys, pathlib; pathlib.Path(r'" + capturedPrompt + "').write_bytes(sys.stdin.buffer.read()); print('module Greeting\\n\\nphrase \\\"say hello\\\"\\n  native csharp\\n    return;')";
        return $"python -c \"{writeAndPrint}\"";
    }

    private static string RetryThenPassCommand(string workspaceRoot, string validOutput, string name)
    {
        var marker = Path.Combine(workspaceRoot, name + "-attempted");
        var received = Path.Combine(workspaceRoot, name + "-received.txt");
        return OperatingSystem.IsWindows()
            ? $"more >> \"{received}\" && if exist \"{marker}\" (type \"{validOutput}\") else (echo invalid & type nul > \"{marker}\")"
            : $"cat >> '{received}' && if test -f '{marker}'; then cat '{validOutput}'; else echo invalid; touch '{marker}'; fi";
    }

    private static string Sha256Hex(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));
}
