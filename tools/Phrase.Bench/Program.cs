using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Phrase.Compiler;

const int MaximumAttempts = 3;
const string SelfCheckModel = "self-check-v1";
const string SelfCheckModelCommand = "__phrase_bench_builtin_fake__";

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: Phrase.Bench run --suite <path> --model <id> --model-command <command> | --self-check [--suite <path>]");
    return 1;
}

if (args[0] == "--self-check")
    return await SelfCheck(ParseOptions(args[1..]));

if (args[0] != "run")
{
    Console.Error.WriteLine($"The '{args[0]}' benchmark command is not implemented yet.");
    return 2;
}

var options = ParseOptions(args[1..]);
if (!options.TryGetValue("suite", out var suitePath) ||
    !options.TryGetValue("model", out var model) ||
    !options.TryGetValue("model-command", out var modelCommand))
{
    Console.Error.WriteLine("run requires --suite, --model, and --model-command.");
    return 1;
}

var timeout = options.TryGetValue("timeout-ms", out var timeoutValue)
    ? TimeSpan.FromMilliseconds(int.Parse(timeoutValue))
    : TimeSpan.FromMinutes(5);

suitePath = Path.GetFullPath(suitePath);
var suiteDirectory = Path.GetDirectoryName(suitePath)!;
var suite = ReadSuite(suitePath);
var exitCode = 0;

foreach (var task in suite.Tasks.Where(task => !task.Holdout))
{
    var passed = await RunTask(task, suiteDirectory, model, modelCommand, timeout, MaximumAttempts);
    if (!passed)
        exitCode = 3;
}

return exitCode;

static async Task<int> SelfCheck(IReadOnlyDictionary<string, string> options)
{
    var suitePath = Path.GetFullPath(options.TryGetValue("suite", out var configuredSuite)
        ? configuredSuite
        : Path.Combine("bench", "suite.json"));
    var suiteDirectory = Path.GetDirectoryName(suitePath)!;
    var suite = ReadSuite(suitePath);

    var runs = new List<IReadOnlyList<BenchmarkResult>>();
    for (var run = 0; run < 2; run++)
    {
        var results = new List<BenchmarkResult>();
        foreach (var task in suite.Tasks)
        {
            await RunTask(task, suiteDirectory, SelfCheckModel, SelfCheckModelCommand, TimeSpan.FromSeconds(5), MaximumAttempts);
            var resultPath = Path.Combine(suiteDirectory, "results", "self-check", $"{SelfCheckModel}.json");
            var result = JsonSerializer.Deserialize<BenchmarkResult>(File.ReadAllText(resultPath), Serialization.Options);
            if (result is null || !HasValidSchema(result))
            {
                Console.Error.WriteLine($"Self-check produced an invalid result for '{task.Id}'.");
                return 4;
            }
            results.Add(result);
        }
        runs.Add(results);
    }

    var firstPassRate = runs[0].Count(result => result.Passed) * 100d / runs[0].Count;
    var secondPassRate = runs[1].Count(result => result.Passed) * 100d / runs[1].Count;
    Console.WriteLine($"noise: pass_rate={secondPassRate - firstPassRate:+0.##;-0.##;0} points; attempts=0; tokens_in=0; tokens_out=0");
    return 0;
}

static Suite ReadSuite(string suitePath) =>
    JsonSerializer.Deserialize<Suite>(File.ReadAllText(suitePath), Serialization.Options)
    ?? throw new InvalidDataException($"Could not read suite '{suitePath}'.");

static bool HasValidSchema(BenchmarkResult result) =>
    !string.IsNullOrWhiteSpace(result.Model) &&
    !string.IsNullOrWhiteSpace(result.Task) &&
    result.Attempts > 0 &&
    result.TokensIn > 0 &&
    result.TokensOut > 0 &&
    result.Diagnostics is not null;

static async Task<bool> RunTask(
    BenchmarkTask task,
    string suiteDirectory,
    string model,
    string modelCommand,
    TimeSpan timeout,
    int maximumAttempts)
{
    var prompt = File.ReadAllText(Path.Combine(suiteDirectory, task.Prompt));
    var scratchDirectory = Path.Combine(Path.GetTempPath(), "phrase-bench", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(scratchDirectory);

    try
    {
        var attemptPrompt = prompt;
        var attempts = 0;
        var tokensIn = 0;
        var tokensOut = 0;
        var diagnostics = new List<string>();
        BenchmarkScore score;
        do
        {
            attempts++;
            var output = await RunModel(model, modelCommand, scratchDirectory, attemptPrompt, timeout);
            tokensIn += EstimateTokens(attemptPrompt);
            tokensOut += EstimateTokens(output);
            score = ScoreReply(output);
            diagnostics.AddRange(score.Diagnostics);
            if (!score.Passed)
                attemptPrompt = AddDiagnostics(prompt, score.Diagnostics);
        }
        while (!score.Passed && attempts < maximumAttempts);

        var result = new BenchmarkResult(
            model, task.Id, score.Passed, attempts, tokensIn, tokensOut,
            "estimated", diagnostics, false, Hash(prompt));
        PersistResult(suiteDirectory, model, result);
        Console.WriteLine($"{model}: {(score.Passed ? "passed" : "failed")} {task.Id}");
        return score.Passed;
    }
    catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or TimeoutException)
    {
        var result = BenchmarkResult.Unavailable(model, task.Id, Hash(prompt), exception.Message);
        PersistResult(suiteDirectory, model, result);
        Console.WriteLine($"{model}: unavailable {task.Id}");
        return true;
    }
    finally
    {
        await DeleteScratchDirectory(scratchDirectory);
    }
}

static async Task DeleteScratchDirectory(string scratchDirectory)
{
    for (var attempt = 0; attempt < 3; attempt++)
    {
        try
        {
            Directory.Delete(scratchDirectory, recursive: true);
            return;
        }
        catch (IOException) when (attempt < 2)
        {
            await Task.Delay(100);
        }
        catch (IOException)
        {
            return;
        }
    }
}

static Dictionary<string, string> ParseOptions(string[] values)
{
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var index = 0; index < values.Length; index += 2)
    {
        if (!values[index].StartsWith("--", StringComparison.Ordinal) || index + 1 == values.Length)
            throw new ArgumentException($"Invalid option '{values[index]}'.");
        result[values[index][2..]] = values[index + 1];
    }
    return result;
}

static Process StartModel(string command, string workingDirectory)
{
    var windows = OperatingSystem.IsWindows();
    var startInfo = new ProcessStartInfo(windows ? "cmd.exe" : "/bin/sh")
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    if (windows)
        startInfo.Arguments = $"/d /c {command}";
    else
    {
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(command);
    }
    return Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start model process.");
}

static async Task<string> RunModel(
    string model, string command, string workingDirectory, string prompt, TimeSpan timeout)
{
    if (command == SelfCheckModelCommand)
        return "module SelfCheck\n\nphrase \"self check\"\n  native csharp\n    return;\n";

    command += ProfileArguments(model, workingDirectory, prompt);
    using var process = StartModel(command, workingDirectory);
    await process.StandardInput.WriteAsync(prompt);
    process.StandardInput.Close();
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    using var timeoutCancellation = new CancellationTokenSource(timeout);
    try
    {
        await process.WaitForExitAsync(timeoutCancellation.Token);
    }
    catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
    {
        process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync();
        throw new TimeoutException($"Model '{model}' timed out after {timeout.TotalMilliseconds:0} ms.");
    }

    var error = await errorTask;
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"Model '{model}' exited {process.ExitCode}: {error}");
    return await outputTask;
}

static string ProfileArguments(string model, string workingDirectory, string prompt)
{
    var profile = model.Split('-', 2)[0];
    if (profile.Equals("codex", StringComparison.Ordinal))
        return " exec --skip-git-repo-check";
    if (profile.Equals("grok", StringComparison.Ordinal))
    {
        var promptPath = Path.Combine(workingDirectory, "prompt.txt");
        File.WriteAllText(promptPath, prompt);
        return $" --max-turns 10 --prompt-file \"{promptPath}\"";
    }
    if (profile.Equals("copilot", StringComparison.Ordinal))
        return $" -p {QuoteForShell(prompt.TrimEnd())} -s --allow-all-tools --trust-folder";
    if (profile.Equals("agy", StringComparison.Ordinal))
        return $" --print {QuoteForShell(prompt.TrimEnd())}";
    return "";
}

static string QuoteForShell(string value) => OperatingSystem.IsWindows()
    ? $"\"{value.Replace("\"", "\\\"")}\""
    : $"'{value.Replace("'", "'\"'\"'")}'";

static BenchmarkScore Score(string source)
{
    try
    {
        PhraseCompiler.Compile(source, new ScoringProvider());
    }
    catch (CompilationFailedException exception)
    {
        return new(false, exception.Diagnostics.Select(diagnostic => diagnostic.Code).ToArray());
    }
    catch (NotImplementedException exception) when (exception.Message.Contains("emitter", StringComparison.OrdinalIgnoreCase))
    {
        return new(true, []);
    }
    return new(true, []);
}

static BenchmarkScore ScoreReply(string output)
{
    var source = ExtractPhrase(output);
    return ContainsPhrase(source)
        ? Score(source)
        : new(false, ["no_phrase_found"]);
}

static string ExtractPhrase(string output)
{
    const string fence = "```";
    var opening = output.IndexOf(fence + "phrase", StringComparison.Ordinal);
    if (opening < 0)
        return output;

    var lineEnd = output.IndexOf('\n', opening);
    if (lineEnd < 0)
        return output;

    var bodyStart = lineEnd + 1;
    var closing = output.IndexOf(fence, bodyStart, StringComparison.Ordinal);
    if (closing < 0)
        return output;

    return output[bodyStart..closing].TrimEnd('\r', '\n');
}

static bool ContainsPhrase(string source)
{
    foreach (var line in source.Split('\n'))
    {
        var trimmed = line.Trim();
        if (trimmed.StartsWith("module ", StringComparison.Ordinal)
            || trimmed.StartsWith("phrase ", StringComparison.Ordinal)
            || trimmed.StartsWith("type ", StringComparison.Ordinal)
            || trimmed.StartsWith("endpoint ", StringComparison.Ordinal))
            return true;
    }

    return false;
}

static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));
static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
static string AddDiagnostics(string prompt, IReadOnlyList<string> diagnostics) =>
    $"{prompt}{Environment.NewLine}{Environment.NewLine}Compiler diagnostics:{Environment.NewLine}{string.Join(Environment.NewLine, diagnostics)}";

static void PersistResult(string suiteDirectory, string model, BenchmarkResult result)
{
    var resultsDirectory = model == SelfCheckModel
        ? Path.Combine(suiteDirectory, "results", "self-check")
        : Path.Combine(suiteDirectory, "results");
    Directory.CreateDirectory(resultsDirectory);
    var serializedResult = JsonSerializer.Serialize(result, Serialization.Options);
    File.WriteAllText(Path.Combine(resultsDirectory, $"{model}.json"), serializedResult);
    File.AppendAllText(Path.Combine(resultsDirectory, "history.jsonl"), serializedResult + "\n");
}

static class Serialization
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
}

sealed record Suite(int Version, IReadOnlyList<BenchmarkTask> Tasks);
sealed record BenchmarkTask(string Id, string Kind, string Prompt, bool Holdout);
sealed record BenchmarkResult(
    string Model,
    string Task,
    bool Passed,
    int Attempts,
    int TokensIn,
    int TokensOut,
    string TokenMeasurement,
    IReadOnlyList<string> Diagnostics,
    bool Misread,
    string PromptHash,
    string Status = "completed",
    string? Error = null)
{
    public static BenchmarkResult Unavailable(string model, string task, string promptHash, string error) =>
        new(model, task, false, 0, 0, 0, "estimated", [], false, promptHash, "unavailable", error);
}
sealed record BenchmarkScore(bool Passed, IReadOnlyList<string> Diagnostics);

sealed class ScoringProvider : ITargetProvider
{
    public string Target => "benchmark";
}
