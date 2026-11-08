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
    public async Task Pre_publication_findings_match_the_repo()
    {
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
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(6));
        await process.WaitForExitAsync(timeout.Token);
        var text = await stdout + await stderr;
        Assert.True(process.ExitCode == 0, text);
    }
}
