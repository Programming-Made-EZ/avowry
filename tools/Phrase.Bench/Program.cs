using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Phrase.Compiler;
using Phrase.Compiler.Semantics;

const int MaximumAttempts = 3;
const int EmptyReplyMaximumAttempts = 2;
const int MinimumNonEmptyReplyPercentage = 75;
const string SelfCheckModel = "self-check-v1";
const string SelfCheckModelCommand = "__phrase_bench_builtin_fake__";
const string AgyModel = "gemini-4-argon-high";
const string AgySkipPermissionsOption = "--dangerously-skip-permissions";

const string ColdPreamble = "There is no workspace and no files to read; answer directly in this reply without looking anything up.";

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: Phrase.Bench run --suite <path> --model <id> --model-command <command> [--mode zero-shot|docs-in-context] [--docs-root <path>] [--custom-docs] [--cold] | --self-check [--suite <path>]");
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

var label = options.TryGetValue("label", out var configuredLabel) ? configuredLabel : null;
var coldRequested = options.TryGetValue("cold", out var coldOption) && bool.TryParse(coldOption, out var requestedCold) && requestedCold;
var timeout = options.TryGetValue("timeout-ms", out var timeoutValue)
    ? TimeSpan.FromMilliseconds(int.Parse(timeoutValue))
    : TimeSpan.FromMinutes(5);
var mode = BenchmarkModes.ZeroShot;
if (options.TryGetValue("mode", out var configuredMode))
{
    if (configuredMode is not (BenchmarkModes.ZeroShot or BenchmarkModes.DocsInContext))
    {
        Console.Error.WriteLine("--mode must be zero-shot or docs-in-context.");
        return 1;
    }

    mode = configuredMode;
}

suitePath = ResolveSuitePath(suitePath);
var suiteDirectory = Path.GetDirectoryName(suitePath)!;
var suite = ReadSuite(suitePath);
var docsBlock = "";
var recordedMode = mode;
if (mode == BenchmarkModes.DocsInContext)
{
    var repositoryRoot = Path.GetFullPath(Path.Combine(suiteDirectory, ".."));
    var docsRoot = repositoryRoot;
    if (options.TryGetValue("docs-root", out var configuredDocsRoot))
    {
        var explicitRoot = Path.GetFullPath(configuredDocsRoot);
        if (!IsSameFullPath(explicitRoot, repositoryRoot))
        {
            if (!options.ContainsKey("custom-docs"))
            {
                Console.Error.WriteLine(
                    $"--docs-root '{explicitRoot}' is not the repository root '{repositoryRoot}'. Pass --custom-docs to use other docs; that run is recorded as docs-in-context-custom and is not counted in a baseline.");
                return 1;
            }

            recordedMode = BenchmarkModes.DocsInContextCustom;
        }

        docsRoot = explicitRoot;
    }

    if (!TryReadDocumentationBlock(docsRoot, out docsBlock, out var docsError))
    {
        Console.Error.WriteLine(docsError);
        return 1;
    }
}

if (recordedMode == BenchmarkModes.DocsInContextCustom)
    Console.WriteLine($"{model}: custom docs; this run is not counted in a baseline");

var exitCode = 0;
var taskResults = new List<TaskRunResult>();

var coldProfile = ResolveColdProfile(model, coldRequested);
if (coldRequested && !coldProfile.Cold)
    Console.WriteLine($"{model}: warm; {coldProfile.Reason}; this run is not counted in a baseline");

foreach (var task in suite.Tasks.Where(task => !task.Holdout))
{
    var result = await RunTask(task, suiteDirectory, model, modelCommand, timeout, MaximumAttempts, label, recordedMode, docsBlock, coldProfile);
    taskResults.Add(result);
    if (!result.KeepsRunSuccessful)
        exitCode = 3;
}

var nonEmptyReplies = taskResults.Count(result => result.HasNonEmptyReply);
if (nonEmptyReplies * 100 < taskResults.Count * MinimumNonEmptyReplyPercentage)
    Console.WriteLine($"{model}: excluded from baseline; non-empty replies {nonEmptyReplies}/{taskResults.Count} (minimum {MinimumNonEmptyReplyPercentage}%)");

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
            await RunTask(task, suiteDirectory, SelfCheckModel, SelfCheckModelCommand, TimeSpan.FromSeconds(5), MaximumAttempts, coldProfile: ResolveColdProfile(SelfCheckModel, false));
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

static async Task<TaskRunResult> RunTask(
    BenchmarkTask task,
    string suiteDirectory,
    string model,
    string modelCommand,
    TimeSpan timeout,
    int maximumAttempts,
    string? label = null,
    string mode = BenchmarkModes.ZeroShot,
    string docsBlock = "",
    ColdProfile? coldProfile = null)
{
    var taskPrompt = ReadPrompt(task, suiteDirectory);
    var resolvedColdProfile = coldProfile ?? ResolveColdProfile(model, false);
    var coldPreamble = resolvedColdProfile.Cold ? ColdPreamble : string.Empty;
    var prompt = ComposeModelPrompt(docsBlock, taskPrompt, coldPreamble);
    var promptHash = Hash(prompt);
    var docsHash = docsBlock.Length == 0 ? "" : Hash(docsBlock);
    var scratchDirectory = Path.Combine(Path.GetTempPath(), "phrase-bench", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(scratchDirectory);
    var workdirEmptyAtLaunch = false;

    try
    {
        var attemptTaskPrompt = taskPrompt;
        var attempts = 0;
        var tokensIn = 0;
        var tokensOut = 0;
        var diagnostics = new List<string>();
        var reply = string.Empty;
        var score = new BenchmarkScore(false, []);
        do
        {
            attempts++;
            var attemptPrompt = ComposeModelPrompt(docsBlock, attemptTaskPrompt, coldPreamble);
            var attemptWorkingDirectory = Path.Combine(scratchDirectory, $"attempt-{attempts}", "workdir");
            Directory.CreateDirectory(attemptWorkingDirectory);
            var promptDirectory = Path.Combine(scratchDirectory, $"attempt-{attempts}", "prompt");
            Directory.CreateDirectory(promptDirectory);
            var promptFilePath = Path.Combine(promptDirectory, "prompt.txt");
            File.WriteAllText(promptFilePath, attemptPrompt);
            workdirEmptyAtLaunch = !Directory.EnumerateFileSystemEntries(attemptWorkingDirectory).Any();
            try
            {
                reply = await RunModel(model, modelCommand, attemptWorkingDirectory, attemptPrompt, timeout, resolvedColdProfile, promptFilePath);
            }
            catch (InvalidOperationException exception) when (attempts < maximumAttempts)
            {
                diagnostics.Add(exception.Message);
                continue;
            }

            tokensIn += EstimateAttemptTokens(attemptTaskPrompt, docsBlock, coldPreamble);
            tokensOut += EstimateTokens(reply);
            if (IsEmptyReply(reply))
            {
                if (attempts < EmptyReplyMaximumAttempts)
                    continue;

                var unavailable = BenchmarkResult.Unavailable(
                    model, task.Id, promptHash, "empty reply", attempts, tokensIn, tokensOut, CreateReplyHead(reply),
                    label: label, mode: mode, docsHash: docsHash,
                    cold: resolvedColdProfile.Cold, coldFlags: resolvedColdProfile.Flags, coldPreamble: coldPreamble,
                    coldReason: resolvedColdProfile.Reason, workdirEmptyAtLaunch: workdirEmptyAtLaunch);
                PersistResult(suiteDirectory, model, unavailable);
                Console.WriteLine($"{model}: unavailable {task.Id}");
                return new(true, false);
            }

            score = ScoreReply(reply);
            diagnostics.AddRange(score.Diagnostics);
            if (!score.Passed)
                attemptTaskPrompt = AddDiagnostics(taskPrompt, score.Diagnostics);
        }
        while (!score.Passed && attempts < maximumAttempts);

        var result = new BenchmarkResult(
            model, task.Id, score.Passed, attempts, tokensIn, tokensOut,
            "estimated", diagnostics, false, promptHash, CreateReplyHead(reply),
            Status: "completed", Error: null, Label: label, ProofLevel: DetermineProofLevel(score, diagnostics),
            Mode: mode, DocsHash: docsHash, Cold: resolvedColdProfile.Cold, ColdFlags: resolvedColdProfile.Flags,
            ColdPreamble: coldPreamble, ColdReason: resolvedColdProfile.Reason, WorkdirEmptyAtLaunch: workdirEmptyAtLaunch);
        PersistResult(suiteDirectory, model, result);
        Console.WriteLine($"{model}: {(score.Passed ? "passed" : "failed")} {task.Id}");
        return new(score.Passed, true);
    }
    catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or TimeoutException)
    {
        var result = BenchmarkResult.Unavailable(
            model, task.Id, promptHash, exception.Message, label: label, mode: mode, docsHash: docsHash,
            cold: resolvedColdProfile.Cold, coldFlags: resolvedColdProfile.Flags, coldPreamble: coldPreamble,
            coldReason: resolvedColdProfile.Reason, workdirEmptyAtLaunch: workdirEmptyAtLaunch);
        PersistResult(suiteDirectory, model, result);
        Console.WriteLine($"{model}: unavailable {task.Id}");
        return new(true, false);
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
    for (var index = 0; index < values.Length; index++)
    {
        if (!values[index].StartsWith("--", StringComparison.Ordinal))
            throw new ArgumentException($"Invalid option '{values[index]}'.");
        var name = values[index][2..];
        if (name == "custom-docs" || name == "cold")
        {
            result[name] = "true";
            continue;
        }

        if (index + 1 == values.Length)
            throw new ArgumentException($"Invalid option '{values[index]}'.");
        result[name] = values[++index];
    }
    return result;
}

static bool IsSameFullPath(string left, string right) =>
    string.Equals(
        NormalizeFullPath(left),
        NormalizeFullPath(right),
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

static string NormalizeFullPath(string path)
{
    var full = Path.GetFullPath(path);
    var root = Path.GetPathRoot(full);
    if (root is not null && full.Length > root.Length)
        full = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    return full;
}

static string ResolveSuitePath(string suitePath)
{
    if (Path.IsPathRooted(suitePath))
        return Path.GetFullPath(suitePath);

    var candidates = new[]
    {
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), suitePath)),
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "bench", suitePath)),
    };
    foreach (var candidate in candidates)
    {
        if (File.Exists(candidate))
            return candidate;
    }
    return Path.GetFullPath(suitePath);
}

static bool TryReadDocumentationBlock(string docsRoot, out string docsBlock, out string error)
{
    var grammarPath = Path.Combine(docsRoot, "docs", "GRAMMAR.md");
    var examplePath = Path.Combine(docsRoot, "tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.phrase");
    if (!File.Exists(grammarPath) || !File.Exists(examplePath))
    {
        docsBlock = "";
        error = $"docs-in-context mode could not find docs/GRAMMAR.md and tests/Phrase.Acceptance.Tests/Fixtures/orders.phrase under '{docsRoot}'.";
        return false;
    }

    docsBlock = DocumentationBlock(File.ReadAllText(grammarPath), File.ReadAllText(examplePath));
    error = "";
    return true;
}

static string DocumentationBlock(string grammar, string example)
{
    var builder = new StringBuilder();
    builder.Append(grammar);
    if (builder.Length == 0 || builder[^1] != '\n')
        builder.Append('\n');
    builder.Append('\n');
    builder.Append(example);
    if (builder.Length == 0 || builder[^1] != '\n')
        builder.Append('\n');
    builder.Append('\n');
    return builder.ToString();
}

static string ComposeModelPrompt(string docsBlock, string taskPrompt, string coldPreamble = "")
{
    var promptBody = docsBlock.Length == 0 ? taskPrompt : docsBlock + taskPrompt;
    return string.IsNullOrEmpty(coldPreamble) ? promptBody : coldPreamble + "\n\n" + promptBody;
}

static string ReadPrompt(BenchmarkTask task, string suiteDirectory)
{
    var promptPath = Path.Combine(suiteDirectory, task.Prompt);
    var promptText = File.ReadAllText(promptPath);
    var taskDirectory = Path.GetDirectoryName(promptPath)!;
    var taskFiles = Directory.GetFiles(taskDirectory, "*", SearchOption.TopDirectoryOnly)
        .Where(path => !string.Equals(Path.GetFileName(path), Path.GetFileName(promptPath), StringComparison.OrdinalIgnoreCase))
        .Where(path => !string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase))
        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
        .ToArray();
    if (taskFiles.Length == 0)
        return promptText;

    var builder = new StringBuilder();
    builder.AppendLine(promptText.TrimEnd());
    builder.AppendLine();
    builder.AppendLine("Task file bodies:");
    foreach (var taskFile in taskFiles)
    {
        builder.AppendLine($"--- {Path.GetFileName(taskFile)} ---");
        builder.AppendLine(File.ReadAllText(taskFile));
    }
    return builder.ToString().TrimEnd();
}

static Process StartModelViaShell(string command, string workingDirectory)
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

static string AppendProfileFlagsToShellCommand(string command, string extraFlags)
{
    if (string.IsNullOrEmpty(extraFlags))
        return command;

    var operators = OperatingSystem.IsWindows() ? new[] { "&&", "||", "&", "|" } : new[] { "&&", "||", ";", "|" };
    var index = FindTopLevelOperatorIndex(command, operators);
    if (index < 0)
        return command + " " + extraFlags;

    return command.Insert(index, $" {extraFlags}");
}

static int FindTopLevelOperatorIndex(string text, IReadOnlyList<string> operators)
{
    var quote = '\0';
    for (var index = 0; index < text.Length; index++)
    {
        var current = text[index];
        if (current == '\'' || current == '"')
        {
            if (quote == '\0')
                quote = current;
            else if (quote == current)
                quote = '\0';
            continue;
        }

        if (quote != '\0')
            continue;

        foreach (var op in operators.OrderByDescending(candidate => candidate.Length))
        {
            if (index + op.Length <= text.Length && string.CompareOrdinal(text, index, op, 0, op.Length) == 0)
                return index;
        }
    }

    return -1;
}

static Process StartModelWithArguments(string command, string workingDirectory, params string[] arguments)
{
    var startInfo = new ProcessStartInfo(command)
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);
    return Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start model process.");
}

static ColdProfile ResolveColdProfile(string model, bool coldRequested)
{
    if (!coldRequested)
        return new(false, string.Empty, "cold mode not requested");

    var profile = model.Split('-', 2)[0];
    const string grokDisallowedTools = "read_file,list_dir,grep,run_terminal_cmd,search_replace,web_search,web_fetch,todo_write,task,Agent";
    return profile switch
    {
        "codex" => new(true, "--sandbox read-only", "Codex exposes a read-only sandbox and a one-shot exec."),
        "grok" => new(true, $"--cwd \"<scratch>\" --disallowed-tools {grokDisallowedTools} --disable-web-search", "Grok supports a fresh empty cwd and explicit tool removal."),
        "copilot" => new(true, "--deny-tool shell --deny-tool write --deny-tool read --deny-tool url", "Copilot denies the shell, write, read and url tool categories (verified against the real CLI: a deny-tool of * is rejected as an invalid rule)."),
        "agy" => new(false, string.Empty, "AGY profiles cannot be made cold; running warm."),
        _ => new(false, string.Empty, "No cold-session flag is available for this CLI profile."),
    };
}

static async Task<string> RunModel(
    string model, string command, string workingDirectory, string prompt, TimeSpan timeout, ColdProfile? coldProfile = null, string? promptFilePath = null)
{
    if (command == SelfCheckModelCommand)
        return "module SelfCheck\n\nphrase \"self check\"\n  native csharp\n    return;\n";

    var profile = model.Split('-', 2)[0];
    var effectiveColdProfile = coldProfile ?? ResolveColdProfile(model, false);
    var extraFlags = ProfileArguments(model, workingDirectory, prompt, promptFilePath, effectiveColdProfile);
    var useDirectArguments = profile.Equals("agy", StringComparison.Ordinal)
        && !Path.HasExtension(command);
    using var process = useDirectArguments
        ? StartModelWithArguments(
            command,
            workingDirectory,
            "-p",
            prompt.TrimEnd(),
            "--model",
            AgyModel,
            AgySkipPermissionsOption)
        : StartModelViaShell(AppendProfileFlagsToShellCommand(command, extraFlags), workingDirectory);
    try
    {
        await process.StandardInput.WriteAsync(prompt);
        process.StandardInput.Close();
    }
    catch (IOException)
    {
        // The model exited without reading its stdin (profiles that take the prompt from a file or argument, such as Grok):
        // a broken pipe is not a failure of the run; its reply is read and scored as usual.
    }
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

    var output = await outputTask;
    var error = await errorTask;
    if (!string.IsNullOrEmpty(output))
        Console.WriteLine(output);
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"Model '{model}' exited {process.ExitCode}: {error}");
    return output;
}

static string ProfileArguments(string model, string workingDirectory, string prompt, string? promptFilePath = null, ColdProfile? coldProfile = null)
{
    var profile = model.Split('-', 2)[0];
    var extraFlags = coldProfile is { Cold: true } ? ExpandColdFlags(coldProfile.Flags, workingDirectory) : string.Empty;

    if (profile.Equals("codex", StringComparison.Ordinal))
    {
        var defaultFlags = " exec --skip-git-repo-check";
        return defaultFlags + (string.IsNullOrEmpty(extraFlags) ? string.Empty : $" {extraFlags}");
    }

    if (profile.Equals("grok", StringComparison.Ordinal))
    {
        var resolvedPromptPath = promptFilePath ?? Path.Combine(workingDirectory, "prompt.txt");
        var defaultFlags = $" --max-turns 10 --prompt-file {QuoteForShell(resolvedPromptPath)}";
        return defaultFlags + (string.IsNullOrEmpty(extraFlags) ? string.Empty : $" {extraFlags}");
    }

    if (profile.Equals("copilot", StringComparison.Ordinal))
    {
        // the prompt travels on stdin: a cmd.exe command line ends at the first newline, so -p cannot carry a multi-line prompt
        var defaultFlags = " -s --allow-all-tools --trust-folder";
        if (coldProfile is { Cold: true })
            defaultFlags = defaultFlags.Replace(" --allow-all-tools", string.Empty, StringComparison.Ordinal);
        return defaultFlags + (string.IsNullOrEmpty(extraFlags) ? string.Empty : $" {extraFlags}");
    }

    if (profile.Equals("agy", StringComparison.Ordinal))
        return $" -p {QuoteForShell(prompt.TrimEnd())} --model {AgyModel} {AgySkipPermissionsOption}";
    return "";
}

static string ExpandColdFlags(string flags, string workingDirectory)
{
    var actualPath = QuoteForShell(workingDirectory);
    return flags
        .Replace("\"<scratch>\"", actualPath, StringComparison.Ordinal)
        .Replace("<scratch>", actualPath, StringComparison.Ordinal);
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

static string DetermineProofLevel(BenchmarkScore score, IReadOnlyList<string> diagnostics)
{
    if (score.Passed)
        return "parse-resolve";
    return "none";
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
    foreach (Match match in Regex.Matches(
        output,
        @"```+[ \t]*(?:phrase[^\r\n]*)?\r?\n(?<body>[\s\S]*?)```+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
    {
        var body = match.Groups["body"].Value.Trim();
        if (ContainsPhrase(body))
            return body;
    }

    var plainMatch = Regex.Match(
        output,
        @"(?ms)(?:^|\n)(?<body>(?:module|phrase|type|endpoint)\b[^\n]*(?:\n\s+.*)*)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    if (plainMatch.Success)
    {
        var body = plainMatch.Groups["body"].Value.Trim();
        if (ContainsPhrase(body))
            return body;
    }

    return string.Empty;
}

static bool ContainsPhrase(string source)
{
    return Regex.IsMatch(
        source,
        @"(?m)^(?:module|phrase|type|endpoint)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}

static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));

// Documentation is charged on its own estimate on every attempt. Joining it with the
// task and dividing by four can drop a token, and the frozen bar counts those tokens
// against Phrase in full.
static int EstimateAttemptTokens(string taskPrompt, string docsBlock, string coldPreamble = "")
{
    var tokens = EstimateTokens(taskPrompt);
    if (docsBlock.Length > 0)
        tokens += EstimateTokens(docsBlock);
    if (coldPreamble.Length > 0)
        tokens += EstimateTokens(coldPreamble);
    return tokens;
}
static bool IsEmptyReply(string reply) => string.IsNullOrWhiteSpace(reply);
static string CreateReplyHead(string reply)
{
    var scrubbed = Regex.Replace(reply.Trim(), @"(?i)\b(api[_-]?key|token|secret)\s*[:=]\s*\S+", "$1=[redacted]", RegexOptions.CultureInvariant);
    scrubbed = Regex.Replace(scrubbed, @"(?<!\w)(?:(?:[A-Za-z]:\\|/|~[\\/])(?:[^\s\""'<>]+[\\/])*[^\s\""'<>]*)", "[path]", RegexOptions.CultureInvariant);
    return scrubbed[..Math.Min(200, scrubbed.Length)];
}
static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
static string AddDiagnostics(string prompt, IReadOnlyList<string> diagnostics) =>
    $"{prompt}{Environment.NewLine}{Environment.NewLine}Compiler diagnostics:{Environment.NewLine}{string.Join(Environment.NewLine, diagnostics)}";

static void PersistResult(string suiteDirectory, string model, BenchmarkResult result)
{
    var resultsDirectory = IsSelfCheckModel(model)
        ? Path.Combine(suiteDirectory, "results", "self-check")
        : Path.Combine(suiteDirectory, "results");
    Directory.CreateDirectory(resultsDirectory);
    var serializedResult = JsonSerializer.Serialize(result, Serialization.Options);
    File.WriteAllText(Path.Combine(resultsDirectory, $"{model}.json"), serializedResult);
    var historyPath = Path.Combine(resultsDirectory, "history.jsonl");
    File.AppendAllText(historyPath, serializedResult + "\n");
}

static bool IsSelfCheckModel(string model) => model.StartsWith("self-check", StringComparison.OrdinalIgnoreCase);

static class Serialization
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
}

sealed record Suite(int Version, IReadOnlyList<BenchmarkTask> Tasks);
sealed record BenchmarkTask(string Id, string Kind, string Prompt, bool Holdout);
public sealed record BenchmarkResult(
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
    string ReplyHead = "",
    string Status = "completed",
    string? Error = null,
    string? Label = null,
    string ProofLevel = "none",
    string Mode = BenchmarkModes.ZeroShot,
    string DocsHash = "",
    bool Cold = false,
    string ColdFlags = "",
    string ColdPreamble = "",
    string ColdReason = "",
    bool WorkdirEmptyAtLaunch = false)
{
    public static BenchmarkResult Unavailable(
        string model, string task, string promptHash, string error,
        int attempts = 0, int tokensIn = 0, int tokensOut = 0, string replyHead = "",
        string? label = null, string mode = BenchmarkModes.ZeroShot, string docsHash = "",
        bool cold = false, string coldFlags = "", string coldPreamble = "", string coldReason = "", bool workdirEmptyAtLaunch = false) =>
        new(model, task, false, attempts, tokensIn, tokensOut, "estimated", [], false, promptHash, replyHead,
            "unavailable", error, label, "none", mode, docsHash, cold, coldFlags, coldPreamble, coldReason, workdirEmptyAtLaunch);
}

public static class BenchmarkResults
{
    public static BenchmarkResult ReadStoredResult(string json)
    {
        using var document = JsonDocument.Parse(json);
        var result = JsonSerializer.Deserialize<BenchmarkResult>(json, Serialization.Options)
            ?? throw new InvalidDataException("Could not read a benchmark result.");
        var root = document.RootElement;
        var mode = ReadOptionalString(root, "mode");
        var docsHash = ReadOptionalString(root, "docs_hash");
        var cold = ReadOptionalBool(root, "cold");
        var coldFlags = ReadOptionalString(root, "cold_flags");
        var coldPreamble = ReadOptionalString(root, "cold_preamble");
        var coldReason = ReadOptionalString(root, "cold_reason");
        var workdirEmptyAtLaunch = ReadOptionalBool(root, "workdir_empty_at_launch");
        return result with
        {
            Mode = string.IsNullOrEmpty(mode) ? BenchmarkModes.ZeroShot : mode,
            DocsHash = docsHash ?? "",
            Cold = cold ?? false,
            ColdFlags = coldFlags ?? string.Empty,
            ColdPreamble = coldPreamble ?? string.Empty,
            ColdReason = coldReason ?? string.Empty,
            WorkdirEmptyAtLaunch = workdirEmptyAtLaunch ?? false,
        };
    }

    private static string? ReadOptionalString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool? ReadOptionalBool(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True
            ? true
            : root.TryGetProperty(name, out value) && value.ValueKind == JsonValueKind.False
                ? false
                : null;
}

static class BenchmarkModes
{
    public const string ZeroShot = "zero-shot";
    public const string DocsInContext = "docs-in-context";
    public const string DocsInContextCustom = "docs-in-context-custom";
}
sealed record ColdProfile(bool Cold, string Flags, string Reason);
sealed record BenchmarkScore(bool Passed, IReadOnlyList<string> Diagnostics);
sealed record TaskRunResult(bool KeepsRunSuccessful, bool HasNonEmptyReply);

sealed class ScoringProvider : ITargetProvider
{
    public string Target => "benchmark";

    public CompilationOutput Emit(ModuleModel module) =>
        throw new NotImplementedException(
            "Lexer, parser, and resolution are done. Next: the IR and the C# emitter.");
}
