using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const int MaximumAttempts = 3;
const string AgyModel = "gemini-4-argon-high";
const string AgySkipPermissionsOption = "--dangerously-skip-permissions";

if (args.Length == 0 || args[0] != "run")
{
    Console.Error.WriteLine("Usage: Phrase.BenchCs run --suite <path> --model <id> --model-command <command> [--label <label>] [--timeout-ms <milliseconds>]");
    return args.Length == 0 ? 1 : 2;
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
var timeout = options.TryGetValue("timeout-ms", out var timeoutValue)
    ? TimeSpan.FromMilliseconds(int.Parse(timeoutValue))
    : TimeSpan.FromMinutes(5);
var label = options.GetValueOrDefault("label");
var exitCode = 0;

foreach (var task in suite.Tasks.Where(task => !task.Holdout))
{
    var passed = await RunTask(task, suiteDirectory, model, modelCommand, label, timeout);
    if (!passed)
        exitCode = 3;
}
return exitCode;

static async Task<bool> RunTask(BenchmarkTask task, string suiteDirectory, string model, string command, string? label, TimeSpan timeout)
{
    var prompt = File.ReadAllText(Path.Combine(suiteDirectory, task.Prompt));
    var outcomes = ReadOutcomes(Path.Combine(suiteDirectory, task.Outcomes));
    var promptHash = Hash(prompt);
    var scratch = Path.Combine(Path.GetTempPath(), "phrase-bench-cs", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(scratch);
    var attempts = 0;
    var tokensIn = 0;
    var tokensOut = 0;
    var diagnostics = new List<string>();
    var reply = "";
    try
    {
        var attemptPrompt = prompt;
        while (attempts < MaximumAttempts)
        {
            attempts++;
            reply = await RunModel(model, command, scratch, attemptPrompt, timeout);
            tokensIn += EstimateTokens(attemptPrompt);
            tokensOut += EstimateTokens(reply);
            var source = ExtractCSharp(reply);
            if (source.Length == 0)
            {
                diagnostics.Add("no_csharp_found");
            }
            else
            {
                var score = await Score(source, task, outcomes, scratch, timeout);
                if (score.Passed)
                {
                    var passedResult = CreateResult(task, model, true, attempts, tokensIn, tokensOut, diagnostics,
                        promptHash, reply, label, "completed", null, "specs-passed", score.ModelTestCount, score.HarnessTestCount);
                    Persist(suiteDirectory, model, passedResult);
                    Console.WriteLine($"{model}: passed {task.Id}");
                    return true;
                }
                diagnostics.Add(score.Diagnostics);
            }
            attemptPrompt = AddDiagnostics(prompt, diagnostics[^1]);
        }

        var failed = CreateResult(task, model, false, attempts, tokensIn, tokensOut, diagnostics,
            promptHash, reply, label, "completed", null, "none", CountModelTests(ExtractCSharp(reply)), outcomes.Count);
        Persist(suiteDirectory, model, failed);
        Console.WriteLine($"{model}: failed {task.Id}");
        return false;
    }
    catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or TimeoutException)
    {
        var unavailable = CreateResult(task, model, false, attempts, tokensIn, tokensOut, diagnostics,
            promptHash, reply, label, "unavailable", exception.Message, "none", CountModelTests(ExtractCSharp(reply)), outcomes.Count);
        Persist(suiteDirectory, model, unavailable);
        Console.WriteLine($"{model}: unavailable {task.Id}");
        return true;
    }
    finally
    {
        try { Directory.Delete(scratch, recursive: true); } catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}

static async Task<Score> Score(string source, BenchmarkTask task, IReadOnlyList<Outcome> outcomes, string scratch, TimeSpan timeout)
{
    var modelTestCount = CountModelTests(source);
    if (modelTestCount == 0)
        return new(false, "no_csharp_tests_found", 0, outcomes.Count);

    var project = Path.Combine(scratch, "verification");
    Directory.CreateDirectory(project);
    await File.WriteAllTextAsync(Path.Combine(project, "Candidate.cs"), source);
    await File.WriteAllTextAsync(Path.Combine(project, "GeneratedOutcomeTests.cs"), GenerateTests(task, outcomes));
    await File.WriteAllTextAsync(Path.Combine(project, "Verification.csproj"), """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup><TargetFramework>net8.0</TargetFramework><IsPackable>false</IsPackable></PropertyGroup>
          <ItemGroup>
            <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
            <PackageReference Include="xunit" Version="2.9.2" />
            <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
          </ItemGroup>
        </Project>
        """);
    using var process = StartProcess("dotnet", project, "test", "Verification.csproj", "--nologo", "--verbosity", "normal");
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    using var cancellation = new CancellationTokenSource(timeout);
    try { await process.WaitForExitAsync(cancellation.Token); }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
    {
        process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync();
        return new(false, "dotnet_test_timeout", modelTestCount, outcomes.Count);
    }
    var output = (await stdout) + (await stderr);
    return process.ExitCode == 0
        ? new(true, "", modelTestCount, outcomes.Count)
        : new(false, TrimDiagnostic(output), modelTestCount, outcomes.Count);
}

static int CountModelTests(string source) => string.IsNullOrWhiteSpace(source)
    ? 0
    : Regex.Matches(source, @"\[\s*(?:(?:global::)?Xunit\.)?(?:Fact|Theory)(?:Attribute)?\s*(?:\([^\]]*\))?\s*\]", RegexOptions.IgnoreCase).Count;

static string GenerateTests(BenchmarkTask task, IReadOnlyList<Outcome> outcomes)
{
    var calls = task.Id switch
    {
        "001-write-greeting" => outcomes.Select(outcome => $"Assert.Equal({Literal(outcome.Returns)}, global::Greeting.Greeter.SayHello());"),
        "002-write-total" => outcomes.Select(outcome => $"Assert.Equal({Literal(outcome.Returns)}, global::Totals.Calculator.Double({Literal(outcome.Given.GetProperty("value"))}));"),
        _ => throw new InvalidDataException($"No C# outcome adapter exists for '{task.Id}'.")
    };
    var methods = outcomes.Zip(calls).Select(pair => $$"""
          [Fact]
          public void Given_{{GivenName(pair.First.Given)}}_When_{{Identifier(pair.First.When)}}_Then_{{Identifier(pair.First.Name)}}()
          {
              {{pair.Second}}
          }
        """);
    return $$"""
        using Xunit;

        public sealed class GeneratedOutcomeTests
        {
        {{string.Join(Environment.NewLine, methods)}}
        }
        """;
}

static IReadOnlyList<Outcome> ReadOutcomes(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    return document.RootElement.EnumerateArray().Select(item => new Outcome(
        item.GetProperty("name").GetString()!, item.GetProperty("given").Clone(), item.GetProperty("when").GetString()!,
        item.GetProperty("then").GetProperty("returns").Clone())).ToArray();
}

static string Literal(JsonElement value) => value.ValueKind switch
{
    JsonValueKind.String => JsonSerializer.Serialize(value.GetString()),
    JsonValueKind.Number => value.GetRawText(),
    JsonValueKind.True => "true",
    JsonValueKind.False => "false",
    JsonValueKind.Null => "null",
    _ => throw new InvalidDataException("Only scalar return values are supported by this slice.")
};

static string Identifier(string name)
{
    var value = Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
    return char.IsDigit(value[0]) ? "case_" + value : value;
}

static string GivenName(JsonElement given) => given.EnumerateObject().Any()
    ? string.Join("_and_", given.EnumerateObject().Select(property => Identifier(property.Name + "_is_" + property.Value.ToString())))
    : "no_inputs";

static async Task<string> RunModel(string model, string command, string workingDirectory, string prompt, TimeSpan timeout)
{
    var profile = model.Split('-', 2)[0];
    var directAgy = profile.Equals("agy", StringComparison.Ordinal) && !Path.HasExtension(command);
    using var process = directAgy
        ? StartProcess(command, workingDirectory, "-p", prompt.TrimEnd(), "--model", AgyModel, AgySkipPermissionsOption)
        : StartShell(command + ProfileArguments(model, workingDirectory, prompt), workingDirectory);
    await process.StandardInput.WriteAsync(prompt);
    process.StandardInput.Close();
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    using var cancellation = new CancellationTokenSource(timeout);
    try { await process.WaitForExitAsync(cancellation.Token); }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
    {
        process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync();
        throw new TimeoutException($"Model '{model}' timed out after {timeout.TotalMilliseconds:0} ms.");
    }
    var error = await stderr;
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"Model '{model}' exited {process.ExitCode}: {error}");
    var output = await stdout;
    if (string.IsNullOrWhiteSpace(output))
        throw new InvalidOperationException($"Model '{model}' returned an empty capture.");
    return output;
}

static Process StartShell(string command, string workingDirectory)
{
    var info = new ProcessStartInfo(OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh")
    {
        WorkingDirectory = workingDirectory, RedirectStandardInput = true, RedirectStandardOutput = true,
        RedirectStandardError = true, UseShellExecute = false,
    };
    if (OperatingSystem.IsWindows()) info.Arguments = $"/d /c {command}";
    else { info.ArgumentList.Add("-c"); info.ArgumentList.Add(command); }
    return Process.Start(info) ?? throw new InvalidOperationException("Could not start model process.");
}

static Process StartProcess(string command, string workingDirectory, params string[] arguments)
{
    var info = new ProcessStartInfo(command)
    {
        WorkingDirectory = workingDirectory, RedirectStandardInput = true, RedirectStandardOutput = true,
        RedirectStandardError = true, UseShellExecute = false,
    };
    foreach (var argument in arguments) info.ArgumentList.Add(argument);
    return Process.Start(info) ?? throw new InvalidOperationException($"Could not start '{command}'.");
}

static string ProfileArguments(string model, string workingDirectory, string prompt)
{
    var profile = model.Split('-', 2)[0];
    if (profile.Equals("codex", StringComparison.Ordinal)) return " exec --skip-git-repo-check";
    if (profile.Equals("grok", StringComparison.Ordinal))
    {
        var path = Path.Combine(workingDirectory, "prompt.txt");
        File.WriteAllText(path, prompt);
        return $" --max-turns 10 --prompt-file \"{path}\"";
    }
    if (profile.Equals("copilot", StringComparison.Ordinal)) return $" -p {Quote(prompt.TrimEnd())} -s --allow-all-tools --trust-folder";
    if (profile.Equals("agy", StringComparison.Ordinal)) return $" -p {Quote(prompt.TrimEnd())} --model {AgyModel} {AgySkipPermissionsOption}";
    return "";
}

static string Quote(string value) => OperatingSystem.IsWindows()
    ? $"\"{value.Replace("\"", "\\\"")}\"" : $"'{value.Replace("'", "'\"'\"'")}'";

static string ExtractCSharp(string output)
{
    var fenced = Regex.Matches(output, @"```+[ \t]*(?:c#|csharp|cs)?[^\r\n]*\r?\n(?<body>[\s\S]*?)```+", RegexOptions.IgnoreCase)
        .Select(match => match.Groups["body"].Value.Trim())
        .Where(LooksLikeCSharp)
        .ToArray();
    if (fenced.Length > 0) return string.Join(Environment.NewLine, fenced);
    return LooksLikeCSharp(output) ? output.Trim() : "";
}
static bool LooksLikeCSharp(string text) => Regex.IsMatch(text, @"\b(namespace|class|record|struct)\b") && text.Contains('{');
static string AddDiagnostics(string prompt, string diagnostics) => $"{prompt}\n\nCompiler or test diagnostics:\n{diagnostics}";
static string TrimDiagnostic(string output) => output.Length <= 8000 ? output.Trim() : output[^8000..].Trim();
static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));
static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
static string ReplyHead(string reply) => reply.Trim()[..Math.Min(200, reply.Trim().Length)];

static BenchmarkResult CreateResult(BenchmarkTask task, string model, bool passed, int attempts, int tokensIn, int tokensOut,
    IReadOnlyList<string> diagnostics, string promptHash, string reply, string? label, string status, string? error,
    string proofLevel, int modelTestCount, int harnessTestCount) => new(model, task.Id, passed, attempts, tokensIn, tokensOut, "estimated", diagnostics,
        false, promptHash, ReplyHead(reply), status, error, label, proofLevel, "plain-csharp", "", task.Kind, modelTestCount, harnessTestCount);

static void Persist(string suiteDirectory, string model, BenchmarkResult result)
{
    var directory = Path.Combine(suiteDirectory, "results");
    Directory.CreateDirectory(directory);
    var json = JsonSerializer.Serialize(result, Serialization.Options);
    File.WriteAllText(Path.Combine(directory, $"{model}.json"), json);
    File.AppendAllText(Path.Combine(directory, "history.jsonl"), json + "\n");
}

static Dictionary<string, string> ParseOptions(string[] values)
{
    var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var index = 0; index < values.Length; index += 2)
    {
        if (!values[index].StartsWith("--", StringComparison.Ordinal) || index + 1 >= values.Length)
            throw new ArgumentException($"Invalid option '{values[index]}'.");
        parsed[values[index][2..]] = values[index + 1];
    }
    return parsed;
}

static class Serialization
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
}
sealed record Suite(int Version, IReadOnlyList<BenchmarkTask> Tasks);
sealed record BenchmarkTask(string Id, string Kind, string Prompt, string Outcomes, bool Holdout);
sealed record Outcome(string Name, JsonElement Given, string When, JsonElement Returns);
sealed record Score(bool Passed, string Diagnostics, int ModelTestCount, int HarnessTestCount);
sealed record BenchmarkResult(string Model, string Task, bool Passed, int Attempts, int TokensIn, int TokensOut,
    string TokenMeasurement, IReadOnlyList<string> Diagnostics, bool Misread, string PromptHash, string ReplyHead,
    string Status, string? Error, string? Label, string ProofLevel, string Mode, string DocsHash, string Kind,
    int ModelTestCount, int HarnessTestCount);
