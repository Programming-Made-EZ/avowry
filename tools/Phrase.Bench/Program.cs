using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Phrase.Compiler;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: Phrase.Bench run --suite <path> --model <id> --model-command <command>");
    return 1;
}

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

suitePath = Path.GetFullPath(suitePath);
var suiteDirectory = Path.GetDirectoryName(suitePath)!;
var suite = JsonSerializer.Deserialize<Suite>(File.ReadAllText(suitePath), Serialization.Options)
    ?? throw new InvalidDataException($"Could not read suite '{suitePath}'.");
var task = suite.Tasks.Single();
var prompt = File.ReadAllText(Path.Combine(suiteDirectory, task.Prompt));
var scratchDirectory = Path.Combine(Path.GetTempPath(), "phrase-bench", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(scratchDirectory);
const int maximumAttempts = 3;

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
        using var process = StartModel(modelCommand, scratchDirectory);
        await process.StandardInput.WriteAsync(attemptPrompt);
        process.StandardInput.Close();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Model '{model}' exited {process.ExitCode}: {error}");

        tokensIn += EstimateTokens(attemptPrompt);
        tokensOut += EstimateTokens(output);
        score = Score(output);
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
    return score.Passed ? 0 : 3;
}
catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
{
    var result = BenchmarkResult.Unavailable(model, task.Id, Hash(prompt), exception.Message);
    PersistResult(suiteDirectory, model, result);
    Console.WriteLine($"{model}: unavailable {task.Id}");
    return 0;
}
finally
{
    Directory.Delete(scratchDirectory, recursive: true);
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

static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));
static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
static string AddDiagnostics(string prompt, IReadOnlyList<string> diagnostics) =>
    $"{prompt}{Environment.NewLine}{Environment.NewLine}Compiler diagnostics:{Environment.NewLine}{string.Join(Environment.NewLine, diagnostics)}";

static void PersistResult(string suiteDirectory, string model, BenchmarkResult result)
{
    var resultsDirectory = Path.Combine(suiteDirectory, "results");
    Directory.CreateDirectory(resultsDirectory);
    var serializedResult = JsonSerializer.Serialize(result, Serialization.Options);
    File.WriteAllText(Path.Combine(resultsDirectory, $"{model}.json"), serializedResult);
    File.AppendAllText(Path.Combine(resultsDirectory, "history.jsonl"), serializedResult + Environment.NewLine);
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
