using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Phrase.Acceptance.Tests.Support;
using Avowry.Adapters.CSharp;
using Avowry.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: A compact Phrase cheat sheet for model prompts
//   As Ari, I want a one-page cheat sheet of Phrase that a model can read in a few hundred tokens,
//   so that I can write a working phrase without reading the whole grammar.
public class PhraseCheatSheet
{
    private static readonly string[] HeadToHeadTasks =
    [
        "001-write-greeting",
        "002-write-total",
        "004-read-phrase",
        "005-read-failure",
        "007-modify-return",
        "008-modify-type",
        "010-fix-trigger",
        "011-fix-type",
    ];

    // Core keywords and outcomes from docs/GRAMMAR.md, plus the native target and HTTP verbs.
    private static readonly string[] Keywords =
    [
        "respond with",
        "fail with",
        "fails with",
        "module",
        "type",
        "avow",
        "endpoint",
        "input",
        "output",
        "steps",
        "let",
        "effects",
        "spec",
        "given",
        "when",
        "then",
        "and",
        "native",
        "returns",
        "responds",
        "succeeds",
        "called",
        "handled",
        "csharp",
        "true",
        "false",
        "none",
        "GET",
        "POST",
        "PUT",
        "PATCH",
        "DELETE",
    ];

    private static readonly string[] CoreTypes = ["Text", "Int", "Decimal", "Bool", "Id", "List", "Option"];

    [Fact]
    [Trait("Slice", "104")]
    public void Sheet_is_at_most_700_estimated_tokens()
    {
        // Given the cheat sheet a model would read instead of the full grammar
        var path = RepoPaths.Find("docs", "CHEATSHEET.md");
        Assert.True(File.Exists(path), "docs/CHEATSHEET.md is missing.");
        var sheet = File.ReadAllText(path);

        // When its size is estimated with the benchmark's own token estimator
        var tokens = EstimateTokens(sheet);

        // Then it is at most 700 estimated tokens
        Assert.True(tokens <= 700, $"Cheat sheet is {tokens} estimated tokens, above 700.");
    }

    [Fact]
    [Trait("Slice", "104")]
    public void Sheet_names_every_keyword_and_core_type_and_its_examples_compile()
    {
        // Given orders.avowry and the prompts and starting code of the eight head-to-head tasks
        var path = RepoPaths.Find("docs", "CHEATSHEET.md");
        Assert.True(File.Exists(path), "docs/CHEATSHEET.md is missing.");
        var sheet = File.ReadAllText(path);
        var corpus = Corpus();
        var required = Keywords.Concat(CoreTypes).Where(term => ContainsTerm(corpus, term)).ToArray();

        // When each keyword and core type they use is looked up in the cheat sheet
        var missing = required.Where(term => !ContainsTerm(sheet, term)).ToArray();

        // Then each one appears, and every fenced example compiles with the Phrase compiler
        Assert.True(
            missing.Length == 0,
            "missing from the cheat sheet: " + string.Join(", ", missing));
        var examples = PhraseExamples(sheet).ToArray();
        Assert.NotEmpty(examples);
        foreach (var example in examples)
        {
            try
            {
                _ = AvowryCompiler.Compile(example, new CSharpProvider());
            }
            catch (Exception exception)
            {
                Assert.Fail($"Example failed to compile: {exception.Message}{Environment.NewLine}{example}");
            }
        }
    }

    [Fact]
    [Trait("Slice", "104")]
    public async Task Custom_docs_run_records_the_sheet_hash_and_charges_its_tokens_on_every_attempt()
    {
        // Given a docs root whose grammar is the cheat sheet, the orders fixture, and a fake model that fails once
        var sheetPath = RepoPaths.Find("docs", "CHEATSHEET.md");
        Assert.True(File.Exists(sheetPath), "docs/CHEATSHEET.md is missing.");
        var sheet = await File.ReadAllTextAsync(sheetPath);
        var ordersPath = RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.avowry");
        var orders = await File.ReadAllTextAsync(ordersPath);
        using var workspace = TempWorkspace.Create();
        var suiteDirectory = CopySuiteTo(workspace.Root);
        var docsRoot = Path.Combine(workspace.Root, "docs-root");
        CopyFile(sheetPath, Path.Combine(docsRoot, "docs", "GRAMMAR.md"));
        CopyFile(ordersPath, Path.Combine(docsRoot, "tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.avowry"));
        var validOutput = await WritePassingPhrase(workspace.Root);
        var modelCommand = RetryThenPassCommand(workspace.Root, validOutput, "sheet");
        var taskPrompt = await File.ReadAllTextAsync(Path.Combine(suiteDirectory, "tasks", "001-write-greeting", "prompt.md"));

        // When the Phrase arm runs with --custom-docs against that root
        var (exitCode, stdout, stderr) = await RunBenchmark(
            suiteDirectory,
            "sheet-custom-v1",
            modelCommand,
            "--mode", "docs-in-context",
            "--docs-root", docsRoot,
            "--custom-docs");

        // Then the row is docs-in-context-custom, docs_hash is the sha256 of the documentation block
        // (the sheet as its grammar, then orders.avowry), and tokens_in includes the sheet on every attempt
        Assert.True(exitCode == 0, stdout + stderr);
        Assert.Contains("custom docs; this run is not counted in a baseline", stdout, StringComparison.Ordinal);
        using var result = await ReadModelResult(suiteDirectory, "sheet-custom-v1");
        Assert.Equal("docs-in-context-custom", result.RootElement.GetProperty("mode").GetString());
        var docsBlock = DocumentationBlock(sheet, orders);
        Assert.StartsWith(sheet.TrimEnd(), docsBlock.TrimEnd(), StringComparison.Ordinal);
        Assert.Equal(Sha256Hex(docsBlock), result.RootElement.GetProperty("docs_hash").GetString());
        var attempts = result.RootElement.GetProperty("attempts").GetInt32();
        var tokensIn = result.RootElement.GetProperty("tokens_in").GetInt32();
        var sheetTokens = EstimateTokens(sheet);
        Assert.True(attempts >= 2, $"Expected the sheet to be charged on a retry, got {attempts} attempt(s).");
        Assert.True(
            tokensIn >= attempts * sheetTokens,
            $"tokens_in {tokensIn} does not charge {sheetTokens} sheet tokens on each of {attempts} attempts (task prompt {EstimateTokens(taskPrompt)} tokens).");
    }

    private static string Corpus()
    {
        var builder = new StringBuilder();
        builder.AppendLine(File.ReadAllText(RepoPaths.Find("tests", "Phrase.Acceptance.Tests", "Fixtures", "orders.avowry")));
        foreach (var task in HeadToHeadTasks)
            builder.AppendLine(File.ReadAllText(RepoPaths.Find("bench", "tasks", task, "prompt.md")));
        return builder.ToString();
    }

    private static bool ContainsTerm(string text, string term)
    {
        var pattern = $@"(?<![A-Za-z0-9]){Regex.Escape(term)}(?![A-Za-z0-9])";
        var options = term.Any(char.IsUpper)
            ? RegexOptions.CultureInvariant
            : RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
        return Regex.IsMatch(text, pattern, options);
    }

    private static IEnumerable<string> PhraseExamples(string sheet)
    {
        foreach (Match match in Regex.Matches(
            sheet,
            "```avowry\\r?\\n(?<source>.*?)\\r?\\n```",
            RegexOptions.Singleline | RegexOptions.CultureInvariant))
            yield return match.Groups["source"].Value.Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    // Same estimator as tools/Phrase.Bench/Program.cs. Ceiling of length over four, and at least one.
    private static int EstimateTokens(string text) => Math.Max(1, (int)Math.Ceiling(text.Length / 4d));

    private static string DocumentationBlock(string grammar, string example)
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

    private static string Sha256Hex(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

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

    private static void CopyFile(string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination);
    }

    private static async Task<JsonDocument> ReadModelResult(string suiteDirectory, string model) =>
        JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(suiteDirectory, "results", $"{model}.json")));

    private static async Task<string> WritePassingPhrase(string workspaceRoot)
    {
        var modelOutput = Path.Combine(workspaceRoot, "model-output.avowry");
        await File.WriteAllTextAsync(modelOutput, """
            module Greeting

            avow to "say hello"
              native csharp
                return;
            """);
        return modelOutput;
    }

    private static string RetryThenPassCommand(string workspaceRoot, string validOutput, string name)
    {
        var marker = Path.Combine(workspaceRoot, name + "-attempted");
        return OperatingSystem.IsWindows()
            ? $"more > nul && if exist \"{marker}\" (type \"{validOutput}\") else (echo invalid & type nul > \"{marker}\")"
            : $"cat > /dev/null && if test -f '{marker}'; then cat '{validOutput}'; else echo invalid; touch '{marker}'; fi";
    }

    private static async Task<(int ExitCode, string Stdout, string Stderr)> RunBenchmark(
        string suiteDirectory, string model, string modelCommand, params string[] additionalArguments)
    {
        var arguments = new List<string>
        {
            "run", "--suite", Path.Combine(suiteDirectory, "suite.json"),
            "--model", model, "--model-command", modelCommand,
            "--private-results", Path.Combine(suiteDirectory, "results", "history.jsonl"),
        };
        arguments.AddRange(additionalArguments);
        var runner = RepoPaths.Find("tools", "Phrase.Bench", "Phrase.Bench.csproj");
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = RepoPaths.Root,
        };
        foreach (var argument in new[] { "run", "--project", runner, "--" }.Concat(arguments))
            startInfo.ArgumentList.Add(argument);
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
        })
            startInfo.Environment.Remove(name);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start dotnet.");
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout, stderr);
    }
}
