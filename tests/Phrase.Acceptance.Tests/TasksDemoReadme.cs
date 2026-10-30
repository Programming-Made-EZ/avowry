using System.Diagnostics;
using Phrase.Acceptance.Tests.Support;

namespace Phrase.Acceptance.Tests;

// Feature: Tasks demo README: how to run the demo yourself
//   As Steve, I want a README next to the Tasks demo that gives the exact commands
//   to compile it from Phrase to C#, build it, run its specs, start it and call an endpoint,
//   with the output to expect at each step, so that I can run the demo myself without asking the mob.
public class TasksDemoReadme
{
    [Fact]
    [Trait("Slice", "158")]
    public async Task Demo_readme_lists_the_commands_and_cannot_drift_from_the_app()
    {
        // Given a README next to the Tasks demo
        var readmePath = RepoPaths.Find("demo", "tasks", "README.md");
        Assert.True(File.Exists(readmePath), "demo/tasks/README.md is missing.");
        var readme = File.ReadAllText(readmePath);

        // When Steve reads the prerequisites and the five steps
        Assert.Contains(".NET 8", readme, StringComparison.Ordinal);
        Assert.Contains("curl", readme, StringComparison.Ordinal);
        Assert.Contains("dotnet run --project src/Avowry.Cli", readme, StringComparison.Ordinal);
        Assert.Contains("demo/tasks/Tasks.avowry", readme, StringComparison.Ordinal);
        Assert.Contains("Tasks.Api", readme, StringComparison.Ordinal);
        Assert.Contains("Tasks.Specs", readme, StringComparison.Ordinal);
        Assert.Contains("0 Warning(s)", readme, StringComparison.Ordinal);
        Assert.Contains("Passed: 11", readme, StringComparison.Ordinal);
        Assert.Contains("Registered 5 endpoints", readme, StringComparison.Ordinal);
        Assert.Contains("http://localhost:5099", readme, StringComparison.Ordinal);
        Assert.Contains("available=true", readme, StringComparison.Ordinal);
        Assert.Contains("/tasks/nope", readme, StringComparison.Ordinal);
        Assert.Contains("EmptyTitle", readme, StringComparison.Ordinal);

        // Then it says the data is in memory, failures answer 422 with a declared name,
        // native csharp remains, and the fenced commands match Tasks.avowry
        Assert.Contains("in memory", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("422", readme, StringComparison.Ordinal);
        Assert.Contains("generalErrors", readme, StringComparison.Ordinal);
        Assert.Contains("native csharp", readme, StringComparison.Ordinal);
        Assert.Contains("159", readme, StringComparison.Ordinal);
        Assert.Contains("160", readme, StringComparison.Ordinal);
        Assert.Contains("ADR-library-binding-layering.md", readme, StringComparison.Ordinal);

        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        start.ArgumentList.Add("tests.test_demo_readme");
        using var process = Process.Start(start);
        Assert.NotNull(process);
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await process.WaitForExitAsync(timeout.Token);
        var text = await stdout + await stderr;
        Assert.True(process.ExitCode == 0, text);
    }
}
