using System.Diagnostics;
using Avowry.Acceptance.Tests.Support;

namespace Avowry.Acceptance.Tests;

// Feature: the sponsor reads one pre-publication findings page that matches the repo
//   As the sponsor, I want licences, vendor terms, history and name recorded with a
//   recommendation, so that the first publish is approved on evidence.
public class PrePublicationChecks
{
    [Fact]
    [Trait("Slice", "186")]
    [Trait("Criterion", "186-1")]
    [Trait("Status", "Pending")] // green 2 removes this Pending trait
    public async Task Pre_publication_findings_match_the_repo()
    {
        // Given the pre-publication findings page and the repo it describes
        var start = new ProcessStartInfo("python")
        {
            WorkingDirectory = RepoPaths.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("unittest");
        start.ArgumentList.Add("tests.test_pre_publication_checks");
        using var process = Process.Start(start);
        Assert.NotNull(process);

        // When the page's checks run
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(6));
        await process.WaitForExitAsync(timeout.Token);
        var text = await stdout + await stderr;

        // Then every section matches the repo, Package licences no longer says not reviewed, and Restored package licences is present
        Assert.True(process.ExitCode == 0, text);
    }
}
