using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
    var (prompt, existingCodeTokens) = AssemblePrompt(task, suiteDirectory);
    var outcomes = ReadOutcomes(Path.Combine(suiteDirectory, task.Outcomes));
    var promptHash = Hash(prompt);
    var scratch = Path.Combine(Path.GetTempPath(), "phrase-bench-cs", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(scratch);
    var attempts = 0;
    var tokensIn = 0;
    var tokensOut = 0;
    var diagnostics = new List<string>();
    var reply = "";
    Score? lastScore = null;
    try
    {
        var attemptPrompt = prompt;
        while (attempts < MaximumAttempts)
        {
            attempts++;
            reply = await RunModel(model, command, scratch, attemptPrompt, timeout);
            tokensIn += EstimateTokens(attemptPrompt);
            tokensOut += EstimateTokens(reply);
            var sources = ExtractCSharp(reply);
            if (sources.Count == 0)
            {
                diagnostics.Add("no_csharp_found");
            }
            else
            {
                var score = await Score(sources, task, outcomes, scratch, timeout);
                lastScore = score;
                if (score.Passed)
                {
                    var passedResult = CreateResult(task, model, true, attempts, tokensIn, tokensOut, diagnostics,
                        promptHash, reply, label, "completed", null, "specs-passed", score.ModelTestCount, score.HarnessTestCount, existingCodeTokens);
                    Persist(suiteDirectory, model, passedResult);
                    Console.WriteLine($"{model}: passed {task.Id}");
                    return true;
                }
                diagnostics.Add(score.Diagnostics);
            }
            attemptPrompt = AddDiagnostics(prompt, diagnostics[^1]);
        }

        var failed = CreateResult(task, model, false, attempts, tokensIn, tokensOut, diagnostics,
            promptHash, reply, label, "completed", null, "none", lastScore?.ModelTestCount ?? 0, lastScore?.HarnessTestCount ?? 0, existingCodeTokens);
        Persist(suiteDirectory, model, failed);
        Console.WriteLine($"{model}: failed {task.Id}");
        return false;
    }
    catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or TimeoutException)
    {
        var unavailable = CreateResult(task, model, false, attempts, tokensIn, tokensOut, diagnostics,
            promptHash, reply, label, "unavailable", exception.Message, "none", 0, 0, existingCodeTokens);
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

static async Task<Score> Score(IReadOnlyList<SourceFile> sources, BenchmarkTask task, IReadOnlyList<Outcome> outcomes, string scratch, TimeSpan timeout)
{
    var plannedModelTests = CountModelTests(string.Join(Environment.NewLine, sources.Select(source => source.Content)));
    if (plannedModelTests == 0)
        return new(false, "no_csharp_tests_found", 0, 0);

    var project = Path.Combine(scratch, $"verification-{Guid.NewGuid():N}");
    Directory.CreateDirectory(project);
    if (Directory.EnumerateFileSystemEntries(project).Any())
        throw new InvalidOperationException("verification_directory_not_empty");
    foreach (var source in sources)
        await File.WriteAllTextAsync(Path.Combine(project, source.Name), source.Content);
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
    const string trxName = "results.trx";
    using var process = StartProcess("dotnet", project, "test", "Verification.csproj", "--nologo", "--verbosity", "quiet", "--logger", $"trx;LogFileName={trxName}");
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    using var cancellation = new CancellationTokenSource(timeout);
    try { await process.WaitForExitAsync(cancellation.Token); }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
    {
        process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync();
        return new(false, "dotnet_test_timeout", 0, 0);
    }
    var output = (await stdout) + (await stderr);
    var trxPath = Path.Combine(project, "TestResults", trxName);
    if (!File.Exists(trxPath))
        return new(false, CompactFailureSummary(output), 0, 0);
    var result = ReadTestResult(trxPath);
    if (result.ModelTests == 0 || result.HarnessTests < outcomes.Count)
        return new(false, "tests_did_not_run", result.ModelTests, result.HarnessTests);
    return process.ExitCode == 0
        ? new(true, "", result.ModelTests, result.HarnessTests)
        : new(false, result.Failures, result.ModelTests, result.HarnessTests);
}

static TestResult ReadTestResult(string path)
{
    var document = XDocument.Load(path);
    var results = document.Descendants().Where(element => element.Name.LocalName == "UnitTestResult").ToArray();
    var executed = results.Where(result => !string.Equals((string?)result.Attribute("outcome"), "NotExecuted", StringComparison.OrdinalIgnoreCase)).ToArray();
    var harness = executed.Count(result => ((string?)result.Attribute("testName"))?.Contains("GeneratedOutcomeTests", StringComparison.Ordinal) == true);
    var model = executed.Length - harness;
    var failures = results.Where(result => string.Equals((string?)result.Attribute("outcome"), "Failed", StringComparison.OrdinalIgnoreCase))
        .Select(result => $"{(string?)result.Attribute("testName")}: {FirstLine(result.Descendants().FirstOrDefault(element => element.Name.LocalName == "Message")?.Value ?? "failed")}");
    return new(model, harness, Cap(string.Join(Environment.NewLine, failures), 600));
}

static int CountModelTests(string source) => string.IsNullOrWhiteSpace(source)
    ? 0
    : Regex.Matches(source, @"\[\s*(?:(?:global::)?Xunit\.)?(?:Fact|Theory)(?:Attribute)?\s*(?:\([^\]]*\))?\s*\]", RegexOptions.IgnoreCase).Count;

// Read tasks keep Phrase.Bench's check level for comments: that scorer never checks comment text.
// The executed check is the spec case in the Phrase prompt, plus a reply that still compiles.
static string GenerateTests(BenchmarkTask task, IReadOnlyList<Outcome> outcomes)
{
    if (task.Id == "008-modify-type")
        return GenerateCustomerTests(outcomes);

    var methods = outcomes.Select(outcome => $$"""
          [Fact]
          public void Given_{{GivenName(outcome.Given)}}_When_{{Identifier(outcome.When)}}_Then_{{Identifier(outcome.Name)}}()
          {
              {{CallOf(task, outcome)}}
          }
        """);
    return $$"""
        using System;
        using System.Linq;
        using System.Reflection;
        using Xunit;

        public sealed class GeneratedOutcomeTests
        {
          private static MethodInfo EntryPoint()
          {
              var matches = typeof(GeneratedOutcomeTests).Assembly.GetTypes()
                  .Where(type => type.Namespace == {{JsonSerializer.Serialize(NamespaceOf(task))}})
                  .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                  .Where(method => {{ShapeOf(task)}})
                  .ToArray();
              return matches.Length == 1 ? matches[0] : throw new InvalidOperationException(
                  {{FailureOf(task)}});
          }
        {{string.Join(Environment.NewLine, methods)}}
        }
        """;
}

static string NamespaceOf(BenchmarkTask task) => task.Id switch
{
    "001-write-greeting" => "Greeting",
    "002-write-total" => "Totals",
    "004-read-phrase" => "Identity",
    "005-read-failure" => "Names",
    "007-modify-return" => "Status",
    _ => throw new InvalidDataException($"No C# outcome adapter exists for '{task.Id}'.")
};

static string ShapeOf(BenchmarkTask task) => task.Id switch
{
    "001-write-greeting" => "method.ReturnType == typeof(string) && method.GetParameters().Length == 0",
    "002-write-total" => "method.Name == \"Double\" && method.ReturnType == typeof(int) && method.GetParameters() is [{ ParameterType: var parameterType }] && parameterType == typeof(int)",
    "004-read-phrase" => "method.ReturnType == typeof(string) && method.GetParameters() is [{ ParameterType: var parameterType }] && parameterType == typeof(string)",
    "005-read-failure" => "method.ReturnType == typeof(void) && method.GetParameters() is [{ ParameterType: var parameterType }] && parameterType == typeof(string)",
    "007-modify-return" => "method.ReturnType == typeof(string) && method.GetParameters().Length == 0",
    _ => throw new InvalidDataException($"No C# outcome adapter exists for '{task.Id}'.")
};

static string FailureOf(BenchmarkTask task) => task.Id switch
{
    "001-write-greeting" => "$\"ambiguous_entry_point: expected exactly one public static parameterless string method in namespace Greeting; found {matches.Length}\"",
    "002-write-total" => "$\"entry_point_match_count: {matches.Length}\"",
    "004-read-phrase" => "$\"ambiguous_entry_point: expected exactly one public static string method with one string parameter in namespace Identity; found {matches.Length}\"",
    "005-read-failure" => "$\"ambiguous_entry_point: expected exactly one public static void method with one string parameter in namespace Names; found {matches.Length}\"",
    "007-modify-return" => "$\"ambiguous_entry_point: expected exactly one public static parameterless string method in namespace Status; found {matches.Length}\"",
    _ => throw new InvalidDataException($"No C# outcome adapter exists for '{task.Id}'.")
};

static string CallOf(BenchmarkTask task, Outcome outcome) => task.Id switch
{
    "001-write-greeting" => $"Assert.Equal({Literal(outcome.Then.GetProperty("returns"))}, (string)EntryPoint().Invoke(null, null)!);",
    "002-write-total" => $"Assert.Equal({Literal(outcome.Then.GetProperty("returns"))}, (int)EntryPoint().Invoke(null, new object[] {{ {Literal(outcome.Given.GetProperty("value"))} }})!);",
    "004-read-phrase" => $"Assert.Equal({Literal(outcome.Then.GetProperty("returns"))}, (string)EntryPoint().Invoke(null, new object[] {{ {Literal(outcome.Given.GetProperty("value"))} }})!);",
    "005-read-failure" => $"Assert.Null(Record.Exception(() => EntryPoint().Invoke(null, new object[] {{ {Literal(outcome.Given.GetProperty("name"))} }})));",
    "007-modify-return" => $"Assert.Equal({Literal(outcome.Then.GetProperty("returns"))}, (string)EntryPoint().Invoke(null, null)!);",
    _ => throw new InvalidDataException($"No C# outcome adapter exists for '{task.Id}'.")
};

static string GenerateCustomerTests(IReadOnlyList<Outcome> outcomes)
{
    var methods = string.Join(Environment.NewLine, outcomes.Select(outcome => $$"""
          [Fact]
          public void Given_{{GivenName(outcome.Given)}}_When_{{Identifier(outcome.When)}}_Then_{{Identifier(outcome.Name)}}()
          {
              {{CustomerCall(outcome)}}
          }
        """));
    return """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Reflection;
        using Xunit;

        public sealed class GeneratedOutcomeTests
        {
          private static Type CustomerType()
          {
              var matches = typeof(GeneratedOutcomeTests).Assembly.GetTypes().Where(type => type.Name == "Customer").ToArray();
              return matches.Length == 1 ? matches[0] : throw new InvalidOperationException(
                  $"ambiguous_entry_point: expected exactly one type named Customer; found {matches.Length}");
          }

          private static MemberInfo StringMember(Type type, string name)
          {
              const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
              var property = type.GetProperty(name, flags);
              if (property != null && property.PropertyType == typeof(string)) return property;
              var field = type.GetField(name, flags);
              if (field != null && field.FieldType == typeof(string)) return field;
              return null;
          }

          private static object CreateCustomer(Type type, IReadOnlyDictionary<string, string> values)
          {
              ConstructorInfo chosen = null;
              var chosenParameters = Array.Empty<ParameterInfo>();
              foreach (var ctor in type.GetConstructors())
              {
                  var parameters = ctor.GetParameters();
                  var viable = true;
                  foreach (var parameter in parameters)
                  {
                      if (parameter.ParameterType != typeof(string) || parameter.Name is null ||
                          !values.Keys.Any(key => key.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase)))
                      {
                          viable = false;
                          break;
                      }
                  }
                  if (viable && (chosen == null || parameters.Length > chosenParameters.Length))
                  {
                      chosen = ctor;
                      chosenParameters = parameters;
                  }
              }
              if (chosen == null) throw new InvalidOperationException("customer_construction_failed");
              var args = new object[chosenParameters.Length];
              var bound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
              for (var index = 0; index < chosenParameters.Length; index++)
              {
                  var parameterName = chosenParameters[index].Name;
                  var key = values.Keys.First(candidate => candidate.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
                  args[index] = values[key];
                  bound.Add(parameterName);
              }
              var created = chosen.Invoke(args);
              foreach (var pair in values)
              {
                  if (bound.Contains(pair.Key)) continue;
                  Set(type, created, pair.Key, pair.Value);
              }
              return created;
          }

          private static void Set(Type type, object target, string name, string value)
          {
              var member = StringMember(type, name);
              if (member is PropertyInfo property) property.SetValue(target, value);
              else if (member is FieldInfo field) field.SetValue(target, value);
              else throw new InvalidOperationException($"missing_field: {name}");
          }

          private static string CallEmail(object customer)
          {
              var customerType = customer.GetType();
              var matches = customerType.Assembly.GetTypes()
                  .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                  .Where(method => method.ReturnType == typeof(string) && !method.IsSpecialName)
                  .Where(method => method.Name is not ("ToString" or "Equals" or "GetHashCode" or "Deconstruct"))
                  .Where(method => method.Name.IndexOf('<') < 0)
                  .Where(method => method.GetCustomAttributes().All(attribute => attribute.GetType().Name is not ("FactAttribute" or "TheoryAttribute")))
                  .Where(method => method.IsStatic
                      ? method.GetParameters() is [{ ParameterType: var parameterType }] && parameterType == customerType
                      : method.DeclaringType == customerType && method.GetParameters().Length == 0)
                  .ToArray();
              if (matches.Length != 1)
                  throw new InvalidOperationException($"ambiguous_entry_point: expected exactly one method that returns a customer's email; found {matches.Length}");
              var method = matches[0];
              var result = method.IsStatic ? method.Invoke(null, new[] { customer }) : method.Invoke(customer, null);
              return (string)result;
          }

        """ + methods + "\n}\n";
}

static string CustomerCall(Outcome outcome)
{
    if (outcome.Then.TryGetProperty("has_field", out var field))
        return $"Assert.NotNull(StringMember(CustomerType(), {Literal(field)}));";
    if (!outcome.Then.TryGetProperty("returns", out var returns))
        throw new InvalidDataException($"Outcome '{outcome.Name}' has no supported then clause.");
    var pairs = string.Join(", ", outcome.Given.EnumerateObject().Select(property =>
        $"[{JsonSerializer.Serialize(property.Name)}] = {Literal(property.Value)}"));
    var checks = string.Join(" ", outcome.Given.EnumerateObject().Select(property =>
        $"Assert.NotNull(StringMember(customerType, {JsonSerializer.Serialize(property.Name)}));"));
    return $"var customerType = CustomerType(); {checks} Assert.Equal({Literal(returns)}, CallEmail(CreateCustomer(customerType, new Dictionary<string, string> {{ {pairs} }})));";
}

static (string Prompt, int ExistingCodeTokens) AssemblePrompt(BenchmarkTask task, string suiteDirectory)
{
    var promptPath = Path.Combine(suiteDirectory, task.Prompt);
    var promptText = File.ReadAllText(promptPath);
    var taskDirectory = Path.GetDirectoryName(promptPath)!;
    var bodies = Directory.GetFiles(taskDirectory, "*", SearchOption.TopDirectoryOnly)
        .Where(path => !string.Equals(Path.GetFileName(path), Path.GetFileName(promptPath), StringComparison.OrdinalIgnoreCase))
        .Where(path => !string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase))
        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
        .Select(path => (Name: Path.GetFileName(path), Body: File.ReadAllText(path)))
        .ToArray();
    if (bodies.Length == 0)
        return (promptText, 0);

    var builder = new StringBuilder();
    builder.AppendLine(promptText.TrimEnd());
    builder.AppendLine();
    builder.AppendLine("Task file bodies:");
    foreach (var file in bodies)
    {
        builder.AppendLine($"--- {file.Name} ---");
        builder.AppendLine(file.Body);
    }
    return (builder.ToString().TrimEnd(), bodies.Sum(file => EstimateTokens(file.Body)));
}

static IReadOnlyList<Outcome> ReadOutcomes(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    return document.RootElement.EnumerateArray().Select(item => new Outcome(
        item.GetProperty("name").GetString()!, item.GetProperty("given").Clone(), item.GetProperty("when").GetString()!,
        item.GetProperty("then").Clone())).ToArray();
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

static IReadOnlyList<SourceFile> ExtractCSharp(string output)
{
    var fenced = Regex.Matches(output, @"```+[ \t]*(?:c#|csharp|cs)?[^\r\n]*\r?\n(?<body>[\s\S]*?)```+", RegexOptions.IgnoreCase)
        .Select(match => match.Groups["body"].Value.Trim())
        .Where(LooksLikeCSharp)
        .ToArray();
    var source = fenced.Length > 0 ? string.Join(Environment.NewLine, fenced) : LooksLikeCSharp(output) ? output.Trim() : "";
    if (source.Length == 0) return Array.Empty<SourceFile>();
    var markers = Regex.Matches(source, @"(?m)^\s*//\s*FILE:\s*(?<name>[^\r\n]+)\s*$", RegexOptions.IgnoreCase);
    if (markers.Count == 0) return [new("Candidate.cs", source)];
    var files = new List<SourceFile>();
    for (var index = 0; index < markers.Count; index++)
    {
        var name = Path.GetFileName(markers[index].Groups["name"].Value.Trim());
        if (!name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || files.Any(file => file.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            name = $"Candidate{index + 1}.cs";
        var start = markers[index].Index + markers[index].Length;
        var end = index + 1 < markers.Count ? markers[index + 1].Index : source.Length;
        files.Add(new(name, source[start..end].Trim()));
    }
    return files;
}
static bool LooksLikeCSharp(string text) => Regex.IsMatch(text, @"\b(namespace|class|record|struct)\b") && text.Contains('{');
static string AddDiagnostics(string prompt, string diagnostics) => $"{prompt}\n\nCompiler or test diagnostics:\n{diagnostics}";
static string CompactFailureSummary(string output)
{
    var useful = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(line => line.Contains("error", StringComparison.OrdinalIgnoreCase) || line.Contains("failed", StringComparison.OrdinalIgnoreCase))
        .Take(6);
    return Cap(string.Join(Environment.NewLine, useful), 600) is { Length: > 0 } summary ? summary : "dotnet_test_failed";
}
static string FirstLine(string value) => value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? "failed";
static string Cap(string value, int length) => value[..Math.Min(length, value.Length)];
static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));
static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
static string ReplyHead(string reply)
{
    var scrubbed = Regex.Replace(reply.Trim(), @"(?i)\b(api[_-]?key|token|secret)\s*[:=]\s*\S+", "$1=[redacted]", RegexOptions.CultureInvariant);
    scrubbed = Regex.Replace(scrubbed, @"(?<!\w)(?:(?:[A-Za-z]:\\|/|~[\\/])(?:[^\s\""'<>]+[\\/])*[^\s\""'<>]*)", "[path]", RegexOptions.CultureInvariant);
    return scrubbed[..Math.Min(200, scrubbed.Length)];
}

static BenchmarkResult CreateResult(BenchmarkTask task, string model, bool passed, int attempts, int tokensIn, int tokensOut,
    IReadOnlyList<string> diagnostics, string promptHash, string reply, string? label, string status, string? error,
    string proofLevel, int modelTestCount, int harnessTestCount, int existingCodeTokens) => new(model, task.Id, passed, attempts, tokensIn, tokensOut, "estimated", diagnostics,
        false, promptHash, ReplyHead(reply), status, error, label, proofLevel, "plain-csharp", "", task.Kind, modelTestCount, harnessTestCount, "executed-outcomes", existingCodeTokens);

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
sealed record Outcome(string Name, JsonElement Given, string When, JsonElement Then);
sealed record Score(bool Passed, string Diagnostics, int ModelTestCount, int HarnessTestCount);
sealed record SourceFile(string Name, string Content);
sealed record TestResult(int ModelTests, int HarnessTests, string Failures);
sealed record BenchmarkResult(string Model, string Task, bool Passed, int Attempts, int TokensIn, int TokensOut,
    string TokenMeasurement, IReadOnlyList<string> Diagnostics, bool Misread, string PromptHash, string ReplyHead,
    string Status, string? Error, string? Label, string ProofLevel, string Mode, string DocsHash, string Kind,
    int ModelTestCount, int HarnessTestCount, string ScoreBar, int ExistingCodeTokens);
