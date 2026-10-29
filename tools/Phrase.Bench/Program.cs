using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;
using Phrase.Compiler.Semantics;

const int MaximumAttempts = 3;
const int EmptyReplyMaximumAttempts = 2;
const int MinimumNonEmptyReplyPercentage = 75;
const int FeedbackCap = 600;
const string SelfCheckModel = "self-check-v1";
const string SelfCheckModelCommand = "__phrase_bench_builtin_fake__";
const string AgyModel = "gemini-4-argon-high";
const string AgySkipPermissionsOption = "--dangerously-skip-permissions";

const string ColdPreamble = "There is no workspace and no files to read; answer directly in this reply without looking anything up.";

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: Phrase.Bench run --suite <path> --model <id> --model-command <command> [--mode zero-shot|docs-in-context] [--docs-root <path>] [--custom-docs] [--cold] [--scoring parse-resolve|executed] [--private-results <path>] [--max-attempts <n>] | --self-check [--suite <path>]");
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
var privateResults = options.TryGetValue("private-results", out var configuredPrivateResults)
    ? configuredPrivateResults
    : null;
if (privateResults is null && RequiresPrivateResults(suitePath, suite.Tasks, out var privateResultsRule))
{
    Console.Error.WriteLine($"Refusing holdout-looking suite: {privateResultsRule}. Pass --private-results <path> to run it.");
    return 1;
}
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

var scoring = BenchmarkScoring.ParseResolve;
if (options.TryGetValue("scoring", out var configuredScoring))
{
    if (configuredScoring is not (BenchmarkScoring.ParseResolve or BenchmarkScoring.Executed))
    {
        Console.Error.WriteLine("--scoring must be parse-resolve or executed.");
        return 1;
    }

    scoring = configuredScoring;
}

var maximumAttempts = MaximumAttempts;
if (options.TryGetValue("max-attempts", out var maxAttemptsValue))
{
    if (!int.TryParse(maxAttemptsValue, out maximumAttempts) || maximumAttempts < 1)
    {
        Console.Error.WriteLine("--max-attempts must be a positive integer.");
        return 1;
    }
}

foreach (var task in suite.Tasks.Where(task => !task.Holdout))
{
    var result = await RunTask(task, suiteDirectory, model, modelCommand, timeout, maximumAttempts, label, recordedMode, docsBlock, coldProfile, scoring, privateResults);
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

static bool RequiresPrivateResults(string suitePath, IReadOnlyList<BenchmarkTask> tasks, out string rule)
{
    if (tasks.Any(task => Regex.IsMatch(task.Id, "^H[0-9]+$", RegexOptions.CultureInvariant)))
    {
        rule = "task ids matching ^H[0-9]+$ require private results";
        return true;
    }

    if (suitePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        .Any(part => part.Equals("holdouts-unsealed", StringComparison.OrdinalIgnoreCase)))
    {
        rule = "suites under a holdouts-unsealed folder require private results";
        return true;
    }

    var ignored = RunGit("check-ignore", "--quiet", "--no-index", "--", suitePath);
    if (ignored == 0)
    {
        rule = "git-ignored suite paths require private results";
        return true;
    }

    if (ignored != 1 && RunGit("rev-parse", "--is-inside-work-tree") != 0)
    {
        rule = "Git must be available to verify that the suite is tracked";
        return true;
    }

    if (RunGit("ls-files", "--error-unmatch", "--", suitePath) != 0)
    {
        rule = "suite paths outside Git tracking require private results";
        return true;
    }

    rule = "";
    return false;
}

static int RunGit(params string[] arguments)
{
    try
    {
        var info = new ProcessStartInfo("git")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
            info.ArgumentList.Add(argument);
        using var process = Process.Start(info);
        if (process is null)
            return -1;
        process.WaitForExit();
        return process.ExitCode;
    }
    catch
    {
        return -1;
    }
}

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
    ColdProfile? coldProfile = null,
    string scoring = BenchmarkScoring.ParseResolve,
    string? privateResultsPath = null)
{
    var (taskPrompt, existingCodeTokens) = ReadPrompt(task, suiteDirectory);
    var resolvedColdProfile = coldProfile ?? ResolveColdProfile(model, false);
    var coldPreamble = resolvedColdProfile.Cold ? ColdPreamble : string.Empty;
    var prompt = ComposeModelPrompt(docsBlock, taskPrompt, coldPreamble);
    var promptHash = Hash(prompt);
    var docsHash = docsBlock.Length == 0 ? "" : Hash(docsBlock);
    var scratchDirectory = Path.Combine(Path.GetTempPath(), "phrase-bench", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(scratchDirectory);
    var workdirEmptyAtLaunch = false;
    var attempts = 0;
    var tokensIn = 0;
    var tokensOut = 0;

    try
    {
        var attemptTaskPrompt = taskPrompt;
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
            tokensIn += EstimateAttemptTokens(attemptTaskPrompt, docsBlock, coldPreamble);
            try
            {
                reply = await RunModel(model, modelCommand, attemptWorkingDirectory, attemptPrompt, timeout, resolvedColdProfile, promptFilePath);
            }
            catch (InvalidOperationException exception) when (attempts < maximumAttempts)
            {
                diagnostics.Add(exception.Message);
                continue;
            }

            tokensOut += EstimateTokens(reply);
            if (IsEmptyReply(reply))
            {
                if (attempts < EmptyReplyMaximumAttempts && attempts < maximumAttempts)
                    continue;

                var unavailable = BenchmarkResult.Unavailable(
                    model, task.Id, promptHash, "empty reply", attempts, tokensIn, tokensOut, CreateReplyHead(reply),
                    label: label, mode: mode, docsHash: docsHash,
                    cold: resolvedColdProfile.Cold, coldFlags: resolvedColdProfile.Flags, coldPreamble: coldPreamble,
                    coldReason: resolvedColdProfile.Reason, workdirEmptyAtLaunch: workdirEmptyAtLaunch);
                PersistResult(suiteDirectory, model, unavailable, privateResultsPath);
                Console.WriteLine($"{model}: unavailable {task.Id}");
                return new(true, false);
            }

            score = ScoreReply(reply);
            if (scoring == BenchmarkScoring.Executed && TryOutcomesPath(task, suiteDirectory, out var outcomesPath))
            {
                score = await ScoreExecuted(ExtractPhrase(reply), task, outcomesPath, scratchDirectory, attempts);
            }
            diagnostics.AddRange(score.Diagnostics);
            if (!score.Passed)
                attemptTaskPrompt = AddDiagnostics(taskPrompt, score.Diagnostics);
        }
        while (!score.Passed && attempts < maximumAttempts);

        var result = new BenchmarkResult(
            model, task.Id, score.Passed, attempts, tokensIn, tokensOut,
            "estimated", diagnostics, false, promptHash, CreateReplyHead(reply),
            Status: "completed", Error: null, Label: label, ProofLevel: DetermineProofLevel(score),
            Mode: mode, DocsHash: docsHash, Cold: resolvedColdProfile.Cold, ColdFlags: resolvedColdProfile.Flags,
            ColdPreamble: coldPreamble, ColdReason: resolvedColdProfile.Reason, WorkdirEmptyAtLaunch: workdirEmptyAtLaunch,
            ScoreBar: score.ScoreBar, Patched: score.ScoreBar is null ? null : false, Kind: score.ScoreBar is null ? null : task.Kind,
            ExistingCodeTokens: score.ScoreBar is null ? null : existingCodeTokens);
        PersistResult(suiteDirectory, model, result, privateResultsPath);
        Console.WriteLine($"{model}: {(score.Passed ? "passed" : "failed")} {task.Id}");
        return new(score.Passed, true);
    }
    catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or TimeoutException)
    {
        var result = BenchmarkResult.Unavailable(
            model, task.Id, promptHash, exception.Message, attempts, tokensIn, tokensOut, CreateReplyHead(string.Empty),
            label: label, mode: mode, docsHash: docsHash,
            cold: resolvedColdProfile.Cold, coldFlags: resolvedColdProfile.Flags, coldPreamble: coldPreamble,
            coldReason: resolvedColdProfile.Reason, workdirEmptyAtLaunch: workdirEmptyAtLaunch);
        PersistResult(suiteDirectory, model, result, privateResultsPath);
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
    var cheatSheetPath = Path.Combine(docsRoot, "docs", "CHEATSHEET.md");
    var examplePath = Path.Combine(docsRoot, "tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.avowry");
    if (!File.Exists(grammarPath))
    {
        docsBlock = "";
        error = $"docs-in-context mode could not find docs/GRAMMAR.md under '{docsRoot}'.";
        return false;
    }

    // Prefer the orders fixture when it is present so scored custom-docs roots stay unchanged.
    // A docs root that holds only GRAMMAR.md and CHEATSHEET.md (the LLM-customer check) uses the sheet.
    if (File.Exists(examplePath))
    {
        docsBlock = DocumentationBlock(File.ReadAllText(grammarPath), File.ReadAllText(examplePath));
        error = "";
        return true;
    }

    if (File.Exists(cheatSheetPath))
    {
        docsBlock = DocumentationBlock(File.ReadAllText(grammarPath), File.ReadAllText(cheatSheetPath));
        error = "";
        return true;
    }

    docsBlock = "";
    error = $"docs-in-context mode could not find docs/CHEATSHEET.md or tests/Phrase.Acceptance.Tests/Fixtures/orders.avowry under '{docsRoot}'.";
    return false;
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

static (string Prompt, int ExistingCodeTokens) ReadPrompt(BenchmarkTask task, string suiteDirectory)
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
        return (promptText, StartingPhraseTokens(promptText));

    var builder = new StringBuilder();
    builder.AppendLine(promptText.TrimEnd());
    builder.AppendLine();
    builder.AppendLine("Task file bodies:");
    foreach (var taskFile in taskFiles)
    {
        builder.AppendLine($"--- {Path.GetFileName(taskFile)} ---");
        builder.AppendLine(File.ReadAllText(taskFile));
    }
    return (builder.ToString().TrimEnd(), taskFiles.Sum(path => EstimateTokens(File.ReadAllText(path))));
}

static int StartingPhraseTokens(string prompt)
{
    var match = Regex.Match(prompt, "```avowry\\r?\\n(?<source>.*?)\\r?\\n```", RegexOptions.Singleline | RegexOptions.CultureInvariant);
    return match.Success ? EstimateTokens(match.Groups["source"].Value) : 0;
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
        return "module SelfCheck\n\navow to \"self check\"\n  native csharp\n    return;\n";

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
    var rootPid = process.Id;
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    using var timeoutCancellation = new CancellationTokenSource(timeout);
    var elapsed = Stopwatch.StartNew();
    try
    {
        try
        {
            await process.StandardInput.WriteAsync(prompt.AsMemory(), timeoutCancellation.Token);
            process.StandardInput.Close();
        }
        catch (IOException)
        {
            // The model exited without reading its stdin (profiles that take the prompt from a file or argument, such as Grok):
            // a broken pipe is not a failure of the run; its reply is read and scored as usual.
        }
        await process.WaitForExitAsync(timeoutCancellation.Token);
    }
    catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
    {
        if (!process.HasExited && process.Id == rootPid)
            process.Kill(entireProcessTree: true);
        await Task.WhenAny(
            Task.WhenAll(process.WaitForExitAsync(), outputTask, errorTask),
            Task.Delay(TimeSpan.FromSeconds(2)));
        throw new TimeoutException(
            $"Model '{model}' timed out with a limit of {timeout.TotalMilliseconds:0} ms after {elapsed.Elapsed.TotalMilliseconds:0} ms.");
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

static string DetermineProofLevel(BenchmarkScore score) =>
    score.ProofLevel ?? (score.Passed ? "parse-resolve" : "none");

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
        @"```+[ \t]*(?:avowry[^\r\n]*)?\r?\n(?<body>[\s\S]*?)```+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
    {
        var body = match.Groups["body"].Value.Trim();
        if (ContainsPhrase(body))
            return body;
    }

    var plainMatch = Regex.Match(
        output,
        @"(?ms)(?:^|\n)(?<body>(?:module|avow|type|endpoint)\b[^\n]*(?:\n\s+.*)*)",
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
        @"(?m)^(?:module|avow|type|endpoint)\b",
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

static void PersistResult(string suiteDirectory, string model, BenchmarkResult result, string? privateResultsPath = null)
{
    var resultsDirectory = IsSelfCheckModel(model)
        ? Path.Combine(suiteDirectory, "results", "self-check")
        : Path.Combine(suiteDirectory, "results");
    Directory.CreateDirectory(resultsDirectory);
    var serializedResult = JsonSerializer.Serialize(result, Serialization.Options);
    File.WriteAllText(Path.Combine(resultsDirectory, $"{model}.json"), serializedResult);
    // Holdout rows carry reply text. A private path keeps that append out of the committed history file.
    var historyPath = string.IsNullOrWhiteSpace(privateResultsPath)
        ? Path.Combine(resultsDirectory, "history.jsonl")
        : privateResultsPath;
    var historyDirectory = Path.GetDirectoryName(historyPath);
    if (!string.IsNullOrEmpty(historyDirectory))
        Directory.CreateDirectory(historyDirectory);
    File.AppendAllText(historyPath, serializedResult + "\n");
}

static bool IsSelfCheckModel(string model) => model.StartsWith("self-check", StringComparison.OrdinalIgnoreCase);

static bool TryOutcomesPath(BenchmarkTask task, string suiteDirectory, out string outcomesPath)
{
    var promptPath = Path.Combine(suiteDirectory, task.Prompt);
    outcomesPath = Path.Combine(Path.GetDirectoryName(promptPath)!, "outcomes.json");
    return File.Exists(outcomesPath);
}

// One build per attempt: the generated specs project already references the API, and the
// outcome harness is a file in that same project, so model specs and outcome cases run together.
static async Task<BenchmarkScore> ScoreExecuted(string source, BenchmarkTask task, string outcomesPath, string scratchDirectory, int attempt)
{
    CompilationOutput output;
    try
    {
        output = PhraseCompiler.Compile(source, new CSharpProvider());
    }
    catch (CompilationFailedException exception)
    {
        return FailedExecuted(string.Join(Environment.NewLine, exception.Diagnostics.Select(diagnostic => diagnostic.ToString())));
    }
    catch (NotImplementedException exception)
    {
        return FailedExecuted(exception.Message);
    }

    IReadOnlyList<Outcome> outcomes;
    try
    {
        outcomes = ReadOutcomes(outcomesPath);
    }
    catch (InvalidDataException exception)
    {
        return FailedExecuted(exception.Message);
    }

    var unsupported = outcomes.FirstOrDefault(outcome =>
        !outcome.Then.TryGetProperty("returns", out _) &&
        !outcome.Then.TryGetProperty("succeeds", out _) &&
        !outcome.Then.TryGetProperty("has_field", out _) &&
        !outcome.Then.TryGetProperty("responds", out _) &&
        !outcome.Then.TryGetProperty("fails_with", out _));
    if (unsupported is not null)
        return FailedExecuted($"outcome \"{unsupported.Name}\" has no supported then clause");

    var generated = Path.Combine(scratchDirectory, $"attempt-{attempt}", "generated");
    output.WriteTo(generated);
    var relativeSpecs = output.Files.Keys.FirstOrDefault(path => path.EndsWith(".Specs.csproj", StringComparison.Ordinal));
    if (relativeSpecs is null)
        return FailedExecuted("generated specs project is missing");

    var specsProject = Path.GetFullPath(Path.Combine(generated, relativeSpecs.Replace('/', Path.DirectorySeparatorChar)));
    await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(specsProject)!, "GeneratedOutcomeTests.cs"), GenerateOutcomeTests(task, outcomes));
    var (exitCode, log) = await RunDotnet(
        Path.GetDirectoryName(specsProject)!,
        TimeSpan.FromMinutes(5),
        "test", specsProject, "--nologo", "--disable-build-servers", "--verbosity", "quiet", "--logger", "trx;LogFileName=results.trx");
    if (exitCode < 0)
        return FailedExecuted("dotnet_test_timeout");

    var trxPath = Path.Combine(Path.GetDirectoryName(specsProject)!, "TestResults", "results.trx");
    if (!File.Exists(trxPath))
        return FailedExecuted(CompactFailureSummary(log));

    var failures = ReadFailures(trxPath);
    if (exitCode == 0 && failures.Length == 0)
        return new BenchmarkScore(true, [], BenchmarkScoring.ExecutedOutcomes, "specs-passed");

    var summary = failures.Length > 0
        ? Cap(string.Join(Environment.NewLine, failures), FeedbackCap)
        : CompactFailureSummary(log);
    return FailedExecuted(summary);
}

static BenchmarkScore FailedExecuted(string feedback) =>
    new(false, [Cap(feedback, FeedbackCap)], BenchmarkScoring.ExecutedOutcomes, "none");

static IReadOnlyList<Outcome> ReadOutcomes(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    return document.RootElement.EnumerateArray().Select(item => new Outcome(
        item.GetProperty("name").GetString() ?? "",
        item.GetProperty("given").Clone(),
        item.GetProperty("when").GetString() ?? "",
        item.GetProperty("then").Clone())).ToArray();
}

static string GenerateOutcomeTests(BenchmarkTask task, IReadOnlyList<Outcome> outcomes)
{
    if (outcomes.Any(outcome => outcome.Then.TryGetProperty("responds", out _)))
        return GenerateEndpointOutcomeTests(outcomes);
    if (task.Id == "008-modify-type")
        return GenerateCustomerOutcomeTests(outcomes);

    var code = new StringBuilder();
    code.AppendLine("using System.Reflection;");
    code.AppendLine();
    code.AppendLine("public sealed class GeneratedOutcomeTests");
    code.AppendLine("{");
    code.AppendLine("    private static MethodInfo Phrase()");
    code.AppendLine("    {");
    code.AppendLine("        var specs = typeof(GeneratedOutcomeTests).Assembly;");
    code.AppendLine("        foreach (var reference in specs.GetReferencedAssemblies())");
    code.AppendLine("            System.Reflection.Assembly.Load(reference);");
    code.AppendLine("        var methods = System.AppDomain.CurrentDomain.GetAssemblies()");
    code.AppendLine("            .SelectMany(TypesOf)");
    code.AppendLine("            .Where(type => type.Name == \"Phrases\" && type.IsClass && type.IsAbstract && type.IsSealed)");
    code.AppendLine("            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))");
    code.AppendLine("            .Where(method => !method.IsSpecialName)");
    code.AppendLine("            .ToArray();");
    code.AppendLine("        if (methods.Length != 1)");
    code.AppendLine("            throw new InvalidOperationException(\"ambiguous_phrase: expected exactly one public static method on Phrases; found \" + methods.Length);");
    code.AppendLine("        return methods[0];");
    code.AppendLine("    }");
    code.AppendLine();
    code.AppendLine("    private static IEnumerable<Type> TypesOf(System.Reflection.Assembly assembly)");
    code.AppendLine("    {");
    code.AppendLine("        try { return assembly.GetTypes(); }");
    code.AppendLine("        catch (ReflectionTypeLoadException exception) { return exception.Types.OfType<Type>(); }");
    code.AppendLine("    }");
    code.AppendLine();
    code.AppendLine("    private static object Call(IReadOnlyDictionary<string, object> given)");
    code.AppendLine("    {");
    code.AppendLine("        var method = Phrase();");
    code.AppendLine("        var parameters = method.GetParameters();");
    code.AppendLine("        var args = new object[parameters.Length];");
    code.AppendLine("        for (var index = 0; index < parameters.Length; index++)");
    code.AppendLine("        {");
    code.AppendLine("            var name = parameters[index].Name ?? \"\";");
    code.AppendLine("            if (!given.TryGetValue(name, out var value))");
    code.AppendLine("                throw new InvalidOperationException(\"missing_given: \" + name);");
    code.AppendLine("            args[index] = value;");
    code.AppendLine("        }");
    code.AppendLine();
    code.AppendLine("        return method.Invoke(null, parameters.Length == 0 ? null : args)!;");
    code.AppendLine("    }");
    foreach (var outcome in outcomes)
    {
        var pairs = string.Join(", ", outcome.Given.EnumerateObject().Select(property =>
            $"[{CsString(property.Name)}] = {Literal(property.Value)}"));
        var initializer = pairs.Length == 0 ? "" : " { " + pairs + " }";
        code.AppendLine();
        code.AppendLine("    [Fact]");
        code.AppendLine($"    public void Outcome_{Identifier(outcome.Name)}()");
        code.AppendLine("    {");
        code.AppendLine("        try");
        code.AppendLine("        {");
        if (outcome.Then.TryGetProperty("returns", out var returns))
        {
            code.AppendLine($"            var actual = Call(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase){initializer});");
            code.AppendLine($"            if (!object.Equals({Literal(returns)}, actual))");
            code.AppendLine($"                throw new Xunit.Sdk.XunitException(\"outcome \" + {CsString(outcome.Name)} + \" expected \" + {CsString(Display(returns))} + \" but was \" + actual);");
        }
        else
        {
            code.AppendLine($"            Call(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase){initializer});");
        }
        code.AppendLine("        }");
        code.AppendLine("        catch (Xunit.Sdk.XunitException)");
        code.AppendLine("        {");
        code.AppendLine("            throw;");
        code.AppendLine("        }");
        code.AppendLine("        catch (System.Exception exception)");
        code.AppendLine("        {");
        code.AppendLine($"            throw new Xunit.Sdk.XunitException(\"outcome \" + {CsString(outcome.Name)} + \" failed: \" + exception.GetBaseException().Message);");
        code.AppendLine("        }");
        code.AppendLine("    }");
    }

    code.AppendLine("}");
    return code.ToString();
}

static string GenerateEndpointOutcomeTests(IReadOnlyList<Outcome> outcomes)
{
    var methods = string.Join(Environment.NewLine, outcomes.Select(outcome => $$"""
          [Fact]
          public async global::System.Threading.Tasks.Task Outcome_{{Identifier(outcome.Name)}}()
          {
              var result = await InvokeEndpointAsync({{JsonSerializer.Serialize(outcome.Given.GetRawText())}});
              {{EndpointAssertion(outcome)}}
          }
        """));
    return """
        using System.IO;
        using System.Reflection;
        using System.Text;
        using System.Text.Json;
        using System.Threading;
        using FastEndpoints;
        using Microsoft.AspNetCore.Http;

        public sealed class GeneratedOutcomeTests
        {
          private static readonly string[] StoreSeedKeys = { "tasks", "stored", "items" };
          private static readonly string[] OriginalTaskEndpoints =
          {
              "CreateTaskEndpoint", "GetTaskEndpoint", "ListTasksEndpoint",
              "CompleteTaskEndpoint", "DeleteTaskEndpoint"
          };

          private static IEnumerable<Type> TypesOf(Assembly assembly)
          {
              try { return assembly.GetTypes(); }
              catch (ReflectionTypeLoadException exception) { return exception.Types.OfType<Type>(); }
          }

          private static Type[] AllTypes()
          {
              foreach (var reference in typeof(GeneratedOutcomeTests).Assembly.GetReferencedAssemblies())
                  Assembly.Load(reference);
              return AppDomain.CurrentDomain.GetAssemblies().SelectMany(TypesOf).ToArray();
          }

          private static Type[] EndpointTypes()
          {
              return AllTypes()
                  .Where(type => type is { IsAbstract: false, IsInterface: false } && typeof(IEndpoint).IsAssignableFrom(type))
                  .ToArray();
          }

          private static Type FindEndpointType(JsonElement given)
          {
              var matches = EndpointTypes();
              if (matches.Length == 1)
                  return matches[0];

              var open = FindByRoute(matches, "/tasks/open");
              if (open is not null)
                  return open;

              var requestKeys = RequestKeys(given);
              var byRequest = matches.Where(type => RequestPropertyNames(type).SetEquals(requestKeys)).ToArray();
              if (byRequest.Length == 1)
                  return byRequest[0];

              var added = matches.Where(type => Array.IndexOf(OriginalTaskEndpoints, type.Name) < 0).ToArray();
              if (added.Length == 1)
                  return added[0];

              throw new InvalidOperationException($"endpoint_type_match_count: {matches.Length}");
          }

          private static Type? FindByRoute(Type[] endpoints, string route)
          {
              foreach (var type in AllTypes())
              {
                  var attribute = type.GetCustomAttributes().FirstOrDefault(item => item.GetType().Name == "PhraseAttribute");
                  if (attribute is null)
                      continue;
                  var template = attribute.GetType().GetProperty("Template")?.GetValue(attribute) as string ?? "";
                  if (!template.Contains(route, StringComparison.OrdinalIgnoreCase))
                      continue;
                  var endpointName = type.Name.EndsWith("Handler", StringComparison.Ordinal)
                      ? type.Name[..^7] + "Endpoint"
                      : type.Name + "Endpoint";
                  var match = endpoints.FirstOrDefault(endpoint => endpoint.Name == endpointName);
                  if (match is not null)
                      return match;
              }
              return null;
          }

          private static HashSet<string> RequestKeys(JsonElement given)
          {
              var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
              if (given.ValueKind != JsonValueKind.Object)
                  return keys;
              foreach (var property in given.EnumerateObject())
              {
                  if (StoreSeedKeys.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                      continue;
                  keys.Add(property.Name);
              }
              return keys;
          }

          private static HashSet<string> RequestPropertyNames(Type endpointType)
          {
              var request = RequestType(endpointType);
              return request.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Select(property => property.Name)
                  .ToHashSet(StringComparer.OrdinalIgnoreCase);
          }

          private static Type RequestType(Type endpointType)
          {
              for (var type = endpointType; type is not null; type = type.BaseType)
              {
                  if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Endpoint<,>))
                      return type.GetGenericArguments()[0];
                  if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Endpoint<>))
                      return type.GetGenericArguments()[0];
              }
              throw new InvalidOperationException("endpoint_request_type_not_found");
          }

          private static string RequestJson(JsonElement given)
          {
              if (given.ValueKind != JsonValueKind.Object)
                  return given.GetRawText();
              using var stream = new MemoryStream();
              using (var writer = new Utf8JsonWriter(stream))
              {
                  writer.WriteStartObject();
                  foreach (var property in given.EnumerateObject())
                  {
                      if (StoreSeedKeys.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                          continue;
                      property.WriteTo(writer);
                  }
                  writer.WriteEndObject();
              }
              return Encoding.UTF8.GetString(stream.ToArray());
          }

          private static object[] CreateDependencies(Type endpointType, JsonElement given)
          {
              var ctor = endpointType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
              if (ctor is null)
                  return Array.Empty<object>();
              var args = new List<object>();
              foreach (var parameter in ctor.GetParameters())
              {
                  if (!parameter.ParameterType.IsInterface)
                      continue;
                  var impl = AllTypes().FirstOrDefault(type =>
                      type is { IsClass: true, IsAbstract: false } &&
                      parameter.ParameterType.IsAssignableFrom(type) &&
                      type != endpointType);
                  if (impl is null)
                      throw new InvalidOperationException("store_implementation_not_found: " + parameter.ParameterType.Name);
                  var instance = Activator.CreateInstance(impl)
                      ?? throw new InvalidOperationException("store_not_created: " + impl.Name);
                  SeedStore(instance, given);
                  args.Add(instance);
              }
              return args.ToArray();
          }

          private static void SeedStore(object store, JsonElement given)
          {
              var save = store.GetType().GetMethod("Save");
              if (save is null || given.ValueKind != JsonValueKind.Object)
                  return;
              JsonElement items = default;
              var found = false;
              foreach (var key in StoreSeedKeys)
              {
                  if (given.TryGetProperty(key, out items) && items.ValueKind == JsonValueKind.Array)
                  {
                      found = true;
                      break;
                  }
              }
              if (!found)
                  return;
              var itemType = save.GetParameters()[0].ParameterType;
              var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
              foreach (var item in items.EnumerateArray())
              {
                  var value = JsonSerializer.Deserialize(item.GetRawText(), itemType, options)
                      ?? throw new InvalidOperationException("store_item_deserialization_failed");
                  save.Invoke(store, new object[] { value });
              }
          }

          private static async global::System.Threading.Tasks.Task<(int StatusCode, string Body)> InvokeEndpointAsync(string givenJson)
          {
              using var given = JsonDocument.Parse(givenJson);
              var requestJson = RequestJson(given.RootElement);
              var endpointType = FindEndpointType(given.RootElement);
              var context = new DefaultHttpContext();
              context.Response.Body = new MemoryStream();
              var dependencies = CreateDependencies(endpointType, given.RootElement);
              var create = typeof(Factory).GetMethods(BindingFlags.Public | BindingFlags.Static)
                  .Single(method => method.Name == "Create" && method.IsGenericMethodDefinition &&
                      method.GetParameters().Length == 2 &&
                      method.GetParameters()[0].ParameterType == typeof(DefaultHttpContext));
              var endpoint = create.MakeGenericMethod(endpointType).Invoke(null, new object[] { context, dependencies })
                  ?? throw new InvalidOperationException("endpoint_instance_not_created");
              var requestType = RequestType(endpointType);
              var payload = JsonSerializer.Deserialize(requestJson, requestType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? throw new InvalidOperationException("request_deserialization_failed");
              var handle = endpointType.GetMethod("HandleAsync", new[] { requestType, typeof(CancellationToken) })
                  ?? throw new InvalidOperationException("endpoint_handle_not_found");
              var returned = handle.Invoke(endpoint, new object[] { payload, CancellationToken.None });
              if (returned is global::System.Threading.Tasks.Task task)
                  await task;
              context.Response.Body.Position = 0;
              using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
              return (context.Response.StatusCode, await reader.ReadToEndAsync());
          }

          private static void AssertRecordEqual(string expectedJson, string actualBody)
          {
              using var expected = JsonDocument.Parse(expectedJson);
              using var actual = JsonDocument.Parse(string.IsNullOrWhiteSpace(actualBody) ? "null" : actualBody);
              Assert.True(SameJson(expected.RootElement, actual.RootElement), $"expected: {expectedJson}; actual: {actualBody}");
          }

          private static bool SameJson(JsonElement expected, JsonElement actual)
          {
              if (expected.ValueKind == JsonValueKind.Object && actual.ValueKind == JsonValueKind.Object)
              {
                  var expectedProps = expected.EnumerateObject().ToArray();
                  var actualProps = actual.EnumerateObject().ToDictionary(property => property.Name, property => property.Value, StringComparer.OrdinalIgnoreCase);
                  return expectedProps.Length == actualProps.Count && expectedProps.All(property =>
                      actualProps.TryGetValue(property.Name, out var actualValue) && SameJson(property.Value, actualValue));
              }
              if (expected.ValueKind == JsonValueKind.Array && actual.ValueKind == JsonValueKind.Array)
              {
                  var expectedItems = expected.EnumerateArray().ToArray();
                  var actualItems = actual.EnumerateArray().ToArray();
                  return expectedItems.Length == actualItems.Length && expectedItems.Zip(actualItems).All(pair => SameJson(pair.First, pair.Second));
              }
              if (expected.ValueKind == JsonValueKind.Number && actual.ValueKind == JsonValueKind.Number)
                  return expected.GetDecimal() == actual.GetDecimal();
              if (expected.ValueKind == JsonValueKind.String && actual.ValueKind == JsonValueKind.String)
                  return expected.GetString() == actual.GetString();
              return expected.ValueKind == actual.ValueKind && expected.GetRawText() == actual.GetRawText();
          }
        """ + methods + "\n}\n";
}

static string EndpointAssertion(Outcome outcome)
{
    if (outcome.Then.TryGetProperty("responds", out var responds))
    {
        var status = outcome.Then.TryGetProperty("status", out var statusValue) ? statusValue.GetInt32() : 200;
        return $"Assert.Equal({status}, result.StatusCode);\n              AssertRecordEqual({JsonSerializer.Serialize(responds.GetRawText())}, result.Body);";
    }
    if (outcome.Then.TryGetProperty("fails_with", out var code))
    {
        var failureCode = code.GetString() ?? throw new InvalidDataException($"Outcome '{outcome.Name}' fails_with is not a string.");
        return $"Assert.Equal(422, result.StatusCode);\n              Assert.Contains({JsonSerializer.Serialize(failureCode)}, result.Body, StringComparison.Ordinal);";
    }
    throw new InvalidDataException($"Outcome '{outcome.Name}' has no supported endpoint then clause.");
}

static string GenerateCustomerOutcomeTests(IReadOnlyList<Outcome> outcomes)
{
    var code = new StringBuilder();
    code.AppendLine("using System.Reflection;");
    code.AppendLine();
    code.AppendLine("public sealed class GeneratedOutcomeTests");
    code.AppendLine("{");
    code.AppendLine("    private static Type Customer() => typeof(GeneratedOutcomeTests).Assembly.GetReferencedAssemblies().Select(System.Reflection.Assembly.Load).SelectMany(a => a.GetTypes()).Single(t => t.Name == \"Customer\");");
    code.AppendLine("    private static MemberInfo Field(string name)");
    code.AppendLine("    {");
    code.AppendLine("        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;");
    code.AppendLine("        return (MemberInfo?)Customer().GetProperty(name, flags) ?? Customer().GetField(name, flags) ?? throw new InvalidOperationException(\"missing_field: \" + name);");
    code.AppendLine("    }");
    code.AppendLine("    private static object Create(IReadOnlyDictionary<string, string> values)");
    code.AppendLine("    {");
    code.AppendLine("        var ctor = Customer().GetConstructors().Single();");
    code.AppendLine("        return ctor.Invoke(ctor.GetParameters().Select(p => (object)values[p.Name!]).ToArray());");
    code.AppendLine("    }");
    code.AppendLine("    private static object Call(object customer)");
    code.AppendLine("    {");
    code.AppendLine("        var method = Customer().Assembly.GetTypes().Where(t => t.Name == \"Phrases\").SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)).Single(m => m.ReturnType == typeof(string) && m.GetParameters() is [{ ParameterType: var p }] && p == Customer());");
    code.AppendLine("        return method.Invoke(null, new[] { customer })!;");
    code.AppendLine("    }");
    foreach (var outcome in outcomes)
    {
        code.AppendLine();
        code.AppendLine("    [Fact]");
        code.AppendLine($"    public void Outcome_{Identifier(outcome.Name)}()");
        code.AppendLine("    {");
        code.AppendLine("        try");
        code.AppendLine("        {");
        if (outcome.Then.TryGetProperty("has_field", out var field))
        {
            code.AppendLine($"            _ = Field({Literal(field)});");
        }
        else
        {
            var pairs = string.Join(", ", outcome.Given.EnumerateObject().Select(property => $"[{CsString(property.Name)}] = {Literal(property.Value)}"));
            var returns = outcome.Then.GetProperty("returns");
            code.AppendLine($"            var actual = Call(Create(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {{ {pairs} }}));");
            code.AppendLine($"            if (!object.Equals({Literal(returns)}, actual)) throw new Xunit.Sdk.XunitException(\"outcome \" + {CsString(outcome.Name)} + \" expected \" + {CsString(Display(returns))} + \" but was \" + actual);");
        }
        code.AppendLine("        }");
        code.AppendLine("        catch (Xunit.Sdk.XunitException) { throw; }");
        code.AppendLine($"        catch (System.Exception exception) {{ throw new Xunit.Sdk.XunitException(\"outcome \" + {CsString(outcome.Name)} + \" failed: \" + exception.GetBaseException().Message); }}");
        code.AppendLine("    }");
    }
    code.AppendLine("}");
    return code.ToString();
}

static string Literal(JsonElement value) => value.ValueKind switch
{
    JsonValueKind.String => CsString(value.GetString() ?? ""),
    JsonValueKind.Number => value.GetRawText(),
    JsonValueKind.True => "true",
    JsonValueKind.False => "false",
    JsonValueKind.Null => "null",
    _ => throw new InvalidDataException("Only scalar return values are supported by this slice."),
};

static string Display(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : value.GetRawText();

static string CsString(string value) =>
    "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

static string Identifier(string name)
{
    var value = Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
    if (value.Length == 0)
        return "outcome";
    return char.IsDigit(value[0]) ? "case_" + value : value;
}

static async Task<(int ExitCode, string Output)> RunDotnet(string workingDirectory, TimeSpan timeout, params string[] arguments)
{
    var startInfo = DotnetStart(workingDirectory);
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start dotnet.");
    var pid = process.Id;
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    using var cancellation = new CancellationTokenSource(timeout);
    try
    {
        await process.WaitForExitAsync(cancellation.Token);
    }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
    {
        if (!process.HasExited && process.Id == pid)
            process.Kill();
        await process.WaitForExitAsync();
        return (-1, "dotnet_test_timeout");
    }

    return (process.ExitCode, (await stdout) + (await stderr));
}

static ProcessStartInfo DotnetStart(string workingDirectory)
{
    var startInfo = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    startInfo.Environment["DOTNET_ROLL_FORWARD"] = "Major";
    startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
    var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
    if (string.IsNullOrEmpty(dotnetRoot))
    {
        var userDotnet = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet");
        if (Directory.Exists(userDotnet))
            dotnetRoot = userDotnet;
    }

    if (!string.IsNullOrEmpty(dotnetRoot))
    {
        startInfo.Environment["DOTNET_ROOT"] = dotnetRoot;
        var path = startInfo.Environment["PATH"] ?? "";
        if (!path.StartsWith(dotnetRoot, StringComparison.OrdinalIgnoreCase))
            startInfo.Environment["PATH"] = dotnetRoot + Path.PathSeparator + path;
    }

    foreach (var name in new[]
    {
        "MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath", "MSBuildLoadMicrosoftTargetsReadOnly",
        "OutputPath", "OutDir", "BaseOutputPath", "BaseIntermediateOutputPath", "IntermediateOutputPath",
        "Configuration", "TargetFramework", "UseAppHost",
    })
        startInfo.Environment.Remove(name);
    return startInfo;
}

static string[] ReadFailures(string path)
{
    var document = XDocument.Load(path);
    return document.Descendants()
        .Where(element => element.Name.LocalName == "UnitTestResult")
        .Where(result => string.Equals((string?)result.Attribute("outcome"), "Failed", StringComparison.OrdinalIgnoreCase))
        .Select(result => $"{(string?)result.Attribute("testName")}: {FirstLine(result.Descendants().FirstOrDefault(element => element.Name.LocalName == "Message")?.Value ?? "failed")}")
        .ToArray();
}

static string CompactFailureSummary(string output)
{
    var useful = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(line => line.Contains("error", StringComparison.OrdinalIgnoreCase) || line.Contains("failed", StringComparison.OrdinalIgnoreCase))
        .Take(6);
    var summary = Cap(string.Join(Environment.NewLine, useful), FeedbackCap);
    return summary.Length > 0 ? summary : "dotnet_test_failed";
}

static string FirstLine(string value) =>
    value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? "failed";

static string Cap(string value, int length) => value.Length <= length ? value : value[..length];

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
    bool WorkdirEmptyAtLaunch = false,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ScoreBar = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] bool? Patched = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Kind = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? ExistingCodeTokens = null)
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
sealed record BenchmarkScore(bool Passed, IReadOnlyList<string> Diagnostics, string? ScoreBar = null, string? ProofLevel = null);
sealed record TaskRunResult(bool KeepsRunSuccessful, bool HasNonEmptyReply);
sealed record Outcome(string Name, JsonElement Given, string When, JsonElement Then);

static class BenchmarkScoring
{
    public const string ParseResolve = "parse-resolve";
    public const string Executed = "executed";
    public const string ExecutedOutcomes = "executed-outcomes";
}

sealed class ScoringProvider : ITargetAdapter
{
    public string Target => "benchmark";

    public CompilationOutput Emit(ModuleModel module) =>
        throw new NotImplementedException(
            "Lexer, parser, and resolution are done. Next: the IR and the C# emitter.");
}
