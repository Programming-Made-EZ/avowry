using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: Dana reads one README and a getting-started guide that match her story
//   As Dana, I want a README that tells me what Avowry is and a guide from clone
//   to a running spec-first API, so that I can judge Avowry and use it in about 15 minutes.
public class GettingStartedDocs
{
    [Fact]
    [Trait("Slice", "179")]
    [Trait("Criterion", "179-1")]
    [Trait("Status", "Pending")]
    public async Task Getting_started_docs_match_danas_story()
    {
        // Given README.md, docs/GETTING_STARTED.md and docs/PREREQUISITES.md on the working tree
        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        start.ArgumentList.Add("tests.test_getting_started");
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
