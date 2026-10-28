using System.Diagnostics;
using Phrase.Acceptance.Tests.Support;
using Phrase.Adapters.CSharp;
using Phrase.Compiler;

namespace Phrase.Acceptance.Tests;

// Feature: Matching Phrase and C# codebases of several sizes
//   As Riley, I want one generator to write a Phrase module and a plain C# service
//   of 1, 10 and 50 endpoints that both prove the same behavior, so that codebase size
//   can be varied without porting a service by hand.
public class MatchingCodebases
{
    [Fact]
    [Trait("Slice", "119")]
    public async Task Size_one_phrase_specs_and_plain_csharp_tests_pass()
    {
        await ProveSize(1);
    }

    [Fact]
    [Trait("Slice", "119")]
    public async Task Size_ten_phrase_specs_and_plain_csharp_tests_pass()
    {
        await ProveSize(10);
    }

    [Fact]
    [Trait("Slice", "119")]
    public async Task Size_fifty_phrase_specs_and_plain_csharp_tests_pass()
    {
        // Given the same library desk at size 50
        // When the generator writes it and both arms are tested
        // Then this stays in the gate only while it remains under about three minutes
        await ProveSize(50);
    }

    private static async Task ProveSize(int size)
    {
        // Given Riley's generator and a fixed seed for an invented library desk
        using var workspace = TempWorkspace.Create();
        var script = RepoPaths.Find("scripts", "make-codebase.py");

        // When it writes a Phrase codebase and a plain C# codebase of this size
        var generated = await RunPythonAsync(script, size.ToString(), workspace.Root, "--seed", "119");
        Assert.True(generated.Code == 0, generated.Output);

        var phrasePath = Path.Combine(workspace.Root, "phrase", "shelf.phrase");
        Assert.True(File.Exists(phrasePath), generated.Output);
        var source = File.ReadAllText(phrasePath);
        var endpoints = source.Split('\n').Count(line => line.StartsWith("endpoint ", StringComparison.Ordinal));
        Assert.Equal(size, endpoints);

        // Then the Phrase module compiles with the C# adapter and its generated specs pass
        var output = PhraseCompiler.Compile(source, new CSharpProvider());
        using var phraseOut = TempWorkspace.Create();
        output.WriteTo(phraseOut.Root);
        var phraseResult = await DotnetCli.TestAsync(phraseOut.FindSingle("*.Specs.csproj"));
        Assert.True(phraseResult.Built, phraseResult.Log);
        Assert.Equal(0, phraseResult.Failed);
        Assert.Equal(CountSpecs(source), phraseResult.Total);
        Assert.DoesNotContain("warning", phraseResult.Log, StringComparison.OrdinalIgnoreCase);

        // And the plain C# project builds without warnings and its xUnit tests pass with the same cases
        var specsProject = Path.Combine(workspace.Root, "csharp", "Shelf.Specs", "Shelf.Specs.csproj");
        var csharpResult = await DotnetCli.TestAsync(specsProject);
        Assert.True(csharpResult.Built, csharpResult.Log);
        Assert.Equal(0, csharpResult.Failed);
        Assert.Equal(phraseResult.Total, csharpResult.Total);
        Assert.DoesNotContain("warning", csharpResult.Log, StringComparison.OrdinalIgnoreCase);

        // And each modify and fix task has one outcomes file shared by both arms
        Assert.True(File.Exists(Path.Combine(workspace.Root, "phrase", "leaves.phrase")));
        Assert.True(File.Exists(Path.Combine(workspace.Root, "phrase", "endpoints", "PlaceHoldNorth.phrase")));
        Assert.True(File.Exists(Path.Combine(workspace.Root, "csharp", "Shelf.Api", "Phrases.cs")));
        Assert.True(File.Exists(Path.Combine(workspace.Root, "csharp", "Shelf.Api", "PlaceHoldNorth.cs")));
        var tasksRoot = Path.Combine(workspace.Root, "tasks");
        var tasks = Directory.GetDirectories(tasksRoot);
        Assert.Contains(tasks, dir => Path.GetFileName(dir) == "modify-loan-limit");
        Assert.Contains(tasks, dir => Path.GetFileName(dir) == "fix-place-hold");
        foreach (var task in tasks)
        {
            Assert.True(File.Exists(Path.Combine(task, "prompt.md")));
            var outcomes = Directory.GetFiles(task, "outcomes.json", SearchOption.AllDirectories);
            Assert.Single(outcomes);
            var body = File.ReadAllText(outcomes[0]);
            Assert.Contains("fails_with", body, StringComparison.Ordinal);
        }

        // The modify task changes the shared leaf and needs the endpoint that posts the desk limit
        var modifyPrompt = File.ReadAllText(Path.Combine(tasksRoot, "modify-loan-limit", "prompt.md"));
        Assert.Contains("Needs phrase file: phrase/endpoints/PlaceHoldNorth.phrase", modifyPrompt, StringComparison.Ordinal);
        Assert.Contains("Needs csharp file: csharp/Shelf.Api/PlaceHoldNorth.cs", modifyPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("posts its loan limit", modifyPrompt, StringComparison.Ordinal);
        Assert.Contains(
            "posts its loan limit",
            File.ReadAllText(Path.Combine(workspace.Root, "phrase", "endpoints", "PlaceHoldNorth.phrase")),
            StringComparison.Ordinal);

        // The fix task changes the endpoint and needs the shared leaf phrase
        var fixPrompt = File.ReadAllText(Path.Combine(tasksRoot, "fix-place-hold", "prompt.md"));
        Assert.Contains("Needs phrase file: phrase/leaves.phrase", fixPrompt, StringComparison.Ordinal);
        Assert.Contains("Needs csharp file: csharp/Shelf.Api/Phrases.cs", fixPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("is a loan of at most three weeks", fixPrompt, StringComparison.Ordinal);
    }

    private static int CountSpecs(string source) =>
        source.Split('\n').Count(line => line.TrimStart().StartsWith("spec \"", StringComparison.Ordinal));

    private static async Task<(int Code, string Output)> RunPythonAsync(string script, params string[] args)
    {
        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add(script);
        foreach (var arg in args)
            start.ArgumentList.Add(arg);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start python.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("make-codebase.py exceeded 2 minutes.");
        }

        return (process.ExitCode, (await stdout) + (await stderr));
    }
}
